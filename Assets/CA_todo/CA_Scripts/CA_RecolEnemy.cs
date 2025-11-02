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
    [SerializeField] ParticleSystem hitParticles; // 🌟 prefab del sistema de partículas
    [SerializeField] Transform particleSpawnPoint; // opcional: punto exacto de aparición

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

    void Update()
    {
        if (health <= 0)
        {
            return; // Ya est� muerto
        }

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
        if (health <= 0) return; // Ya est� muerto

        health -= _damageDone;

        // ⚡ Efecto de flash (si existe)
        if (_damageFlash != null)
            _damageFlash.CallDamageFlash();
        if (health <= 0)
        {
            ActivarMuerte();
            return;
        }

        // 💥 Recoil físico
        if (!isRecoiling && rb != null)
        {
            isRecoiling = true;
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection, ForceMode2D.Impulse);
        }

        // 💫 Partículas de impacto
        if (hitParticles != null)
        {
            // determina posición (si hay spawn point, úsalo)
            Vector3 spawnPos = particleSpawnPoint != null ? particleSpawnPoint.position : transform.position;

            // instanciar el sistema
            ParticleSystem ps = Instantiate(hitParticles, spawnPos, Quaternion.identity);
            ps.Play();

            // destruir el sistema cuando termina (para evitar basura en escena)
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }
    }
    void ActivarMuerte()
    {
        // Desactivar componentes físicos y de comportamiento
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // Desactivar colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // Sistema de animación por tag
        string deathParam = ObtenerParametroMuertePorTag();
        if (animator != null && !string.IsNullOrEmpty(deathParam))
        {
            animator.SetBool(deathParam, true);
        }

        // Desactivar scripts de comportamiento específicos
        DesactivarComportamientosEnemigo();

        // Destruir después de tiempo (o manejar por evento de animación)
        StartCoroutine(DestruirDespuesDeAnimacion());
    }

    string ObtenerParametroMuertePorTag()
    {
        // Buscar en los tags del GameObject
        foreach (string tag in deathAnimations.Keys)
        {
            if (gameObject.CompareTag(tag))
            {
                return deathAnimations[tag];
            }
        }

        // Tag por defecto si no se encuentra coincidencia
        return "IsDead";
    }

    void DesactivarComportamientosEnemigo()
    {
        // Desactivar scripts comunes de enemigos
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
        // Esperar tiempo suficiente para la animación de muerte
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    // Método público para forzar muerte (útil para efectos instantáneos)
    public void ForzarMuerte()
    {
        health = 0;
        ActivarMuerte();
    }

    // En la clase CA_RecolEnemy, agregar este método:
    public float GetHealth()
    {
        return health;
    }

    public bool EstaMuerto()
    {
        return health <= 0;
    }
}
