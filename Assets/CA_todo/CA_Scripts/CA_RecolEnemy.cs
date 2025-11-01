using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_RecolEnemy : MonoBehaviour
{
    [SerializeField] float health;
    [SerializeField] float recoilLength;
    [SerializeField] float recoilFactor;
    [SerializeField] bool isRecoiling = false;

    float recoilTimer;
    Rigidbody2D rb;
    Animator animator;

    private Dictionary<string, string> deathAnimations = new Dictionary<string, string>()
    {
        { "Enemy", "IsDead" },
    };

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
            {
                recoilTimer += Time.deltaTime;
            }
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

        // Verificar si muri� despu�s del da�o
        if (health <= 0)
        {
            ActivarMuerte();
            return;
        }

        // Aplicar retroceso solo si sigue vivo
        if (!isRecoiling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
            isRecoiling = true;
            recoilTimer = 0f;
        }
    }

    void ActivarMuerte()
    {
        // Desactivar componentes f�sicos y de comportamiento
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

        // Sistema de animaci�n por tag
        string deathParam = ObtenerParametroMuertePorTag();
        if (animator != null && !string.IsNullOrEmpty(deathParam))
        {
            animator.SetBool(deathParam, true);
        }

        // Desactivar scripts de comportamiento espec�ficos
        DesactivarComportamientosEnemigo();

        // Destruir despu�s de tiempo (o manejar por evento de animaci�n)
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
        // Esperar tiempo suficiente para la animaci�n de muerte
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    // M�todo p�blico para forzar muerte (�til para efectos instant�neos)
    public void ForzarMuerte()
    {
        health = 0;
        ActivarMuerte();
    }

    // En la clase CA_RecolEnemy, agregar este m�todo:
    public float GetHealth()
    {
        return health;
    }

    public bool EstaMuerto()
    {
        return health <= 0;
    }
}