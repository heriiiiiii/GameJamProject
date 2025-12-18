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

    // ===============================
    // 🔊 AUDIO
    // ===============================
    [Header("🔊 Audio Enemigo")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip finalHitClip;
    [SerializeField] private float audioDistance = 6f;

    private Transform player;
    private bool isAudioPlaying = false;
    private bool deathSoundPlayed = false;

    private float recoilTimer;
    private Rigidbody2D rb;
    private NF_DamageFlash _damageFlash;
    private Animator animator;

    private Dictionary<string, string> deathAnimations = new Dictionary<string, string>()
    {
        { "Enemy", "IsDead" },
    };

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _damageFlash = GetComponent<NF_DamageFlash>();
        animator = GetComponent<Animator>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (grupoEnemigos == null)
            grupoEnemigos = new List<GameObject>();

        if (!grupoEnemigos.Contains(gameObject))
            grupoEnemigos.Add(gameObject);

        if (audioSource)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 🔥 2D
        }
    }

    void Update()
    {
        if (health <= 0) return;

        HandleAudioProximity();

        if (isRecoiling)
        {
            recoilTimer += Time.deltaTime;
            if (recoilTimer >= recoilLength)
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
    }

    // ===============================
    // 🔊 AUDIO POR PROXIMIDAD (IDLE)
    // ===============================
    void HandleAudioProximity()
    {
        if (!player || !audioSource || !idleClip || deathSoundPlayed)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= audioDistance && !isAudioPlaying)
        {
            audioSource.clip = idleClip;
            audioSource.Play();
            isAudioPlaying = true;
        }
        else if (distance > audioDistance && isAudioPlaying)
        {
            audioSource.Stop();
            isAudioPlaying = false;
        }
    }

    // ===============================
    // 💥 RECIBIR DAÑO
    // ===============================
    public void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        if (health <= 0) return;

        health -= _damageDone;

        if (_damageFlash != null)
            _damageFlash.CallDamageFlash();

        // 🔊 Golpe normal
        if (hitClip && audioSource && health > 0)
            audioSource.PlayOneShot(hitClip, 0.8f);

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
            Vector3 pos = particleSpawnPoint ? particleSpawnPoint.position : transform.position;
            var ps = Instantiate(hitParticles, pos, Quaternion.identity);
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }
    }

    // ===============================
    // ☠️ MUERTE
    // ===============================
    void ActivarMuerte()
    {
        if (deathSoundPlayed) return;
        deathSoundPlayed = true;

        // Cortar cualquier loop
        if (audioSource && audioSource.isPlaying)
            audioSource.Stop();

        // 🔥 AUDIO DE MUERTE 2D REAL (FUERTE)
        if (finalHitClip)
        {
            GameObject audioGO = new GameObject("EnemyDeathSound");
            AudioSource src = audioGO.AddComponent<AudioSource>();

            src.clip = finalHitClip;
            src.volume = 1f;
            src.spatialBlend = 0f; // 🔊 2D
            src.playOnAwake = false;
            src.loop = false;

            src.Play();
            Destroy(audioGO, finalHitClip.length + 0.1f);
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;

        string deathParam = ObtenerParametroMuertePorTag();
        if (animator && !string.IsNullOrEmpty(deathParam))
            animator.SetBool(deathParam, true);

        DesactivarComportamientosEnemigo();
        VerificarMuerteGrupo();
        StartCoroutine(DestruirDespuesDeAnimacion());
    }

    void VerificarMuerteGrupo()
    {
        if (!enemigosMuertos.Contains(gameObject))
            enemigosMuertos.Add(gameObject);

        foreach (GameObject e in grupoEnemigos)
        {
            if (e && e.GetComponent<CA_RecolEnemy>()?.EstaMuerto() == false)
                return;
        }

        activador?.DesactivarParedesBloqueo();
        battleManager?.BossDerrotado();
    }

    string ObtenerParametroMuertePorTag()
    {
        foreach (var kv in deathAnimations)
            if (CompareTag(kv.Key)) return kv.Value;
        return "IsDead";
    }

    void DesactivarComportamientosEnemigo()
    {
        foreach (var m in GetComponents<MonoBehaviour>())
            if (m != this && m != animator)
                m.enabled = false;
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

    // 🔴 MÉTODOS USADOS POR OTROS SISTEMAS
    public float GetHealth() => health;
    public bool EstaMuerto() => health <= 0;
}
