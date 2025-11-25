using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_RecolEnemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float health = 10f;

    [Header("Recoil")]
    [SerializeField] float recoilLength = 0.2f;
    [SerializeField] float recoilFactor = 1f;
    [SerializeField] bool isRecoiling = false;

    [Header("VFX")]
    [SerializeField] ParticleSystem hitParticles;
    [SerializeField] Transform particleSpawnPoint;

    [Header("Referencias")]
    public CA_ActivadorEnemigo activador;
    public CA_BossBattleManager battleManager;
    public CA_MiniBossVigiasEsporales grupoController;

    [Header("Grupo de Enemigos")]
    public List<GameObject> grupoEnemigos;
    private List<GameObject> enemigosMuertos = new List<GameObject>();

    private float recoilTimer;
    private Rigidbody2D rb;
    private NF_DamageFlash _damageFlash;
    Animator animator;

    private Dictionary<string, string> deathAnimations = new Dictionary<string, string>()
    {
        { "Enemy", "IsDead" },
    };

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _damageFlash = GetComponent<NF_DamageFlash>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // Inicializar lista de grupo si es necesario
        if (grupoEnemigos == null)
        {
            grupoEnemigos = new List<GameObject>();
        }

        if (!grupoEnemigos.Contains(gameObject))
        {
            grupoEnemigos.Add(gameObject);
        }

        Debug.Log("Boss inicializado - Salud: " + health);
    }

    void Update()
    {
        if (health <= 0) return;

        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
                recoilTimer += Time.deltaTime;
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
    }

    public void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        if (health <= 0) return;

        health -= _damageDone;

        if (_damageFlash != null)
            _damageFlash.CallDamageFlash();

        if (health <= 0)
        {
            ActivarMuerte();
            return;
        }

        if (!isRecoiling && rb != null)
        {
            isRecoiling = true;
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection, ForceMode2D.Impulse);
        }

        if (hitParticles != null)
        {
            Vector3 spawnPos = particleSpawnPoint != null ? particleSpawnPoint.position : transform.position;
            ParticleSystem ps = Instantiate(hitParticles, spawnPos, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }
    }

    void ActivarMuerte()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
            col.enabled = false;

        string deathParam = ObtenerParametroMuertePorTag();
        if (animator != null && !string.IsNullOrEmpty(deathParam))
            animator.SetBool(deathParam, true);

        DesactivarComportamientosEnemigo();
        VerificarMuerteGrupo();

        StartCoroutine(DestruirDespuesDeAnimacion());
    }

    void VerificarMuerteGrupo()
    {
        if (grupoEnemigos == null || grupoEnemigos.Count == 0) return;

        if (!enemigosMuertos.Contains(gameObject))
        {
            enemigosMuertos.Add(gameObject);
        }

        bool todosMuertos = true;
        foreach (GameObject enemigo in grupoEnemigos)
        {
            if (enemigo != null)
            {
                CA_RecolEnemy enemyScript = enemigo.GetComponent<CA_RecolEnemy>();
                if (enemyScript != null && !enemyScript.EstaMuerto())
                {
                    todosMuertos = false;
                    break;
                }
            }
        }

        if (todosMuertos)
        {
            if (activador != null)
            {
                activador.DesactivarParedesBloqueo();
                activador.gameObject.SetActive(false);
            }

            if (battleManager != null)
            {
                battleManager.BossDerrotado();
            }

            Debug.Log("¡Todos los enemigos del grupo han sido derrotados!");
        }
    }

    string ObtenerParametroMuertePorTag()
    {
        foreach (string tag in deathAnimations.Keys)
        {
            if (gameObject.CompareTag(tag))
                return deathAnimations[tag];
        }
        return "IsDead";
    }

    void DesactivarComportamientosEnemigo()
    {
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this && script != animator &&
                script.GetType() != typeof(SpriteRenderer) &&
                script.GetType() != typeof(Transform))
            {
                script.enabled = false;
            }
        }
    }

    IEnumerator DestruirDespuesDeAnimacion()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    public void ForzarMuerte()
    {
        health = 0;
        ActivarMuerte();
    }

    public float GetHealth() => health;
    public bool EstaMuerto() => health <= 0;
}