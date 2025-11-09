using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_ActivadorEnemigo : MonoBehaviour
{
    [Header("Configuración General")]
    public GameObject enemigoPrefab;            // Enemigo a activar (puede ser cualquier prefab)
    public GameObject[] paredesBloqueo;         // Paredes de bloqueo opcionales

    private MonoBehaviour scriptEnemigo;        // Referencia genérica al script del enemigo
    private bool enfrentamientoActivo = false;  // Control para evitar múltiples activaciones
    public GameObject prefabCorazon; // Arrastra aquí el corazón DESACTIVADO en la escena


    void Start()
    {
        // Buscar automáticamente las paredes si no se asignaron manualmente
        if (paredesBloqueo == null || paredesBloqueo.Length == 0)
        {
            BuscarParedesBloqueo();
        }

        if (enemigoPrefab != null)
        {
            scriptEnemigo = enemigoPrefab.GetComponent<MonoBehaviour>();
        }

        Debug.Log("Activador listo. Enemigo asignado: " + (enemigoPrefab != null));
        Debug.Log("Paredes de bloqueo asignadas: " + (paredesBloqueo != null ? paredesBloqueo.Length : 0));
    }

    void BuscarParedesBloqueo()
    {
        GameObject[] paredes = GameObject.FindGameObjectsWithTag("ParedBoss");
        if (paredes.Length > 0)
        {
            paredesBloqueo = paredes;
        }
        else
        {
            GameObject pared = GameObject.Find("ParedesBloqueo");
            if (pared != null)
            {
                paredesBloqueo = new GameObject[] { pared };
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !enfrentamientoActivo)
        {
            enfrentamientoActivo = true;
            Debug.Log("Jugador activó el enfrentamiento con " + enemigoPrefab.name);
            ActivarEnfrentamiento();
        }
    }

    void ActivarEnfrentamiento()
    {
        if (enemigoPrefab != null)
        {
            enemigoPrefab.SetActive(true);

            if (scriptEnemigo != null)
            {
                var metodo = scriptEnemigo.GetType().GetMethod("ActivarBoss");
                if (metodo == null)
                    metodo = scriptEnemigo.GetType().GetMethod("ActivarEnemigo");

                if (metodo != null)
                    metodo.Invoke(scriptEnemigo, null);
                else
                    Debug.LogWarning("El enemigo no tiene un método ActivarBoss() ni ActivarEnemigo()");
            }
        }
        else
        {
            Debug.LogError("Enemigo no asignado en el activador");
        }

        ActivarParedesBloqueo();
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
        else
        {
            Debug.LogWarning("No se encontraron paredes de bloqueo para activar");
        }
    }

    // Método público para desactivar las paredes asignadas
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
    }


}
