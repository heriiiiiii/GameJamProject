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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _damageFlash = GetComponent<NF_DamageFlash>();
    }

    void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
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
        health -= _damageDone;

        // ⚡ Efecto de flash (si existe)
        if (_damageFlash != null)
            _damageFlash.CallDamageFlash();

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
}
