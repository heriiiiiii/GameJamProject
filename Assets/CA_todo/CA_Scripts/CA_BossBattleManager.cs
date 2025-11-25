using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_BossBattleManager : MonoBehaviour
{
    [Header("Configuración del Boss")]
    public CA_ActivadorEnemigo activadorBoss;
    public GameObject bossPrefab; // Prefab del boss para instanciar
    public Transform bossSpawnPoint; // Punto donde spawnear el boss

    [Header("Estado del Encuentro")]
    public bool enfrentamientoActivo = false;
    public bool bossDerrotado = false;

    [Header("Referencias")]
    public NF_PlayerHealth playerHealth;

    private GameObject bossInstance; // Instancia actual del boss
    private Vector3 spawnPosition;

    // 🔹 NUEVO: Para mini boss con múltiples enemigos
    [Header("Configuración Mini Boss")]
    public bool esMiniBoss = false;
    public CA_MiniBossVigiasEsporales miniBossController;

    void Start()
    {
        // Buscar el player health si no está asignado
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<NF_PlayerHealth>();
        }

        // Suscribirse al evento de muerte del jugador
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath += ResetearBossBattle;
        }

        // Determinar posición de spawn
        if (bossSpawnPoint != null)
        {
            spawnPosition = bossSpawnPoint.position;
        }
        else
        {
            spawnPosition = transform.position;
        }

        // 🔹 BUSCAR MINI BOSS CONTROLLER SI ES MINI BOSS
        if (esMiniBoss && miniBossController == null)
        {
            miniBossController = GetComponent<CA_MiniBossVigiasEsporales>();
        }

        Debug.Log("Boss Battle Manager inicializado. Tipo: " + (esMiniBoss ? "Mini Boss" : "Boss Normal"));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // SOLO activar si el boss NO ha sido derrotado y no hay enfrentamiento activo
        if (other.CompareTag("Player") && !enfrentamientoActivo && !bossDerrotado)
        {
            IniciarEnfrentamiento();
        }
    }

    public void IniciarEnfrentamiento()
    {
        if (enfrentamientoActivo || bossDerrotado) return;

        enfrentamientoActivo = true;
        Debug.Log("¡Enfrentamiento con el boss iniciado!");

        if (esMiniBoss)
        {
            // 🔹 ACTIVAR MINI BOSS EXISTENTE EN ESCENA
            ActivarMiniBoss();
        }
        else
        {
            // Spawnear el boss normal
            SpawnearBoss();
        }

        // Activar el enfrentamiento a través del activador
        if (activadorBoss != null)
        {
            activadorBoss.IniciarEnfrentamientoDesdeManager();
        }
    }

    private void SpawnearBoss()
    {
        // Destruir cualquier boss existente (por si acaso)
        if (bossInstance != null)
        {
            Destroy(bossInstance);
        }

        // Instanciar nuevo boss
        if (bossPrefab != null)
        {
            bossInstance = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
            bossInstance.name = bossPrefab.name + "_Instance";

            // Configurar referencias en el boss
            CA_RecolEnemy bossScript = bossInstance.GetComponent<CA_RecolEnemy>();
            if (bossScript != null)
            {
                bossScript.activador = activadorBoss;
                bossScript.battleManager = this;
            }

            Debug.Log("Boss spawneado: " + bossInstance.name);
        }
        else
        {
            Debug.LogError("No hay bossPrefab asignado para spawnear");
        }
    }

    // 🔹 NUEVO: Método para activar mini boss que ya está en escena
    private void ActivarMiniBoss()
    {
        if (miniBossController != null)
        {
            // Configurar referencias en el mini boss
            miniBossController.activador = activadorBoss;
            miniBossController.battleManager = this;

            // Activar el mini boss
            miniBossController.ActivarBoss();

            Debug.Log("Mini Boss activado: " + miniBossController.gameObject.name);
        }
        else
        {
            Debug.LogError("No hay miniBossController asignado");
        }
    }

    public void BossDerrotado()
    {
        enfrentamientoActivo = false;
        bossDerrotado = true;

        // Limpiar referencia del boss
        bossInstance = null;

        Debug.Log("¡Boss derrotado! Encuentro completado.");
    }

    public void ResetearBossBattle()
    {
        // SOLO resetear si el enfrentamiento estaba activo
        if (enfrentamientoActivo)
        {
            StartCoroutine(ResetearCoroutine());
        }
    }

    private IEnumerator ResetearCoroutine()
    {
        Debug.Log("Iniciando reseteo de boss battle...");

        // Esperar un frame para asegurar que la muerte del player se procesó
        yield return null;

        // 1. Resetear estado interno
        enfrentamientoActivo = false;

        // 2. Desactivar paredes a través del activador
        if (activadorBoss != null)
        {
            activadorBoss.DesactivarParedesBloqueo();
        }

        // 3. Resetear el boss según el tipo
        if (esMiniBoss)
        {
            ResetearMiniBoss();
        }
        else
        {
            ResetearBossNormal();
        }

        Debug.Log("Boss battle reseteado - Listo para nuevo encuentro");
    }

    private void ResetearBossNormal()
    {
        // Destruir el boss actual
        if (bossInstance != null)
        {
            Destroy(bossInstance);
            bossInstance = null;
            Debug.Log("Boss normal destruido");
        }

        // Buscar y destruir cualquier otro boss en escena (por si acaso)
        CA_RecolEnemy[] bossesEnEscena = FindObjectsOfType<CA_RecolEnemy>();
        foreach (CA_RecolEnemy boss in bossesEnEscena)
        {
            if (boss != null && boss.gameObject != bossPrefab) // No destruir el prefab
            {
                Destroy(boss.gameObject);
                Debug.Log("Boss adicional destruido: " + boss.gameObject.name);
            }
        }
    }

    // 🔹 NUEVO: Método para resetear mini boss
    private void ResetearMiniBoss()
    {
        if (miniBossController != null)
        {
            miniBossController.ResetearMiniBoss();
            Debug.Log("Mini Boss reseteado");
        }
        else
        {
            Debug.LogWarning("No hay miniBossController para resetear");
        }
    }

    // Método para obtener la instancia actual del boss (útil para otros scripts)
    public GameObject GetBossInstance()
    {
        return bossInstance;
    }

    void OnDestroy()
    {
        // Desuscribirse de eventos
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath -= ResetearBossBattle;
        }
    }
}