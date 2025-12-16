using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_ActivadorEnemigo : MonoBehaviour
{
    [Header("Configuración General")]
    public GameObject[] paredesBloqueo;

    [System.NonSerialized] public bool enfrentamientoActivo = false;
    public GameObject prefabCorazon;

    [Header("Gestor de Boss Battle")]
    public CA_BossBattleManager battleManager;

    void Start()
    {
        // Buscar paredes si no están asignadas
        if (paredesBloqueo == null || paredesBloqueo.Length == 0)
        {
            BuscarParedesBloqueo();
        }

        // Buscar battle manager si no está asignado
        if (battleManager == null)
        {
            battleManager = GetComponentInParent<CA_BossBattleManager>();
        }

        Debug.Log("Activador listo. Battle Manager: " + (battleManager != null));
    }

    void BuscarParedesBloqueo()
    {
        GameObject[] paredes = GameObject.FindGameObjectsWithTag("ParedBoss");
        if (paredes.Length > 0)
        {
            paredesBloqueo = paredes;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // SOLO activar si no hay enfrentamiento activo
        if (other.CompareTag("Player") && !enfrentamientoActivo)
        {
            if (battleManager != null && !battleManager.bossDerrotado)
            {
                // Usar el battle manager
                battleManager.IniciarEnfrentamiento();
            }
        }
    }

    // Método para que el manager active el enfrentamiento
    public void IniciarEnfrentamientoDesdeManager()
    {
        if (!enfrentamientoActivo)
        {
            enfrentamientoActivo = true;
            Debug.Log("Activando enfrentamiento desde manager");
            ActivarParedesBloqueo();
        }
    }

    void ActivarParedesBloqueo()
    {
        if (paredesBloqueo != null && paredesBloqueo.Length > 0)
        {
            foreach (GameObject pared in paredesBloqueo)
            {
                if (pared != null)
                    pared.SetActive(true);
            }
            Debug.Log("Paredes activadas.");
        }
    }

    public void DesactivarParedesBloqueo()
    {
        if (paredesBloqueo != null && paredesBloqueo.Length > 0)
        {
            foreach (GameObject pared in paredesBloqueo)
            {
                if (pared != null)
                    pared.SetActive(false);
            }
            Debug.Log("Paredes desactivadas.");
        }

        enfrentamientoActivo = false;

        // 🔹 CORREGIDO: Solo notificar al battle manager si el boss fue realmente derrotado
        if (battleManager != null && !battleManager.bossDerrotado)
        {
            // Verificar si el boss está realmente muerto
            if (!battleManager.IsBossAlive())
            {
                // Boss fue derrotado
                battleManager.BossDerrotado();
                // Desactivar este activador permanentemente
                this.gameObject.SetActive(false);
            }
        }
    }
}