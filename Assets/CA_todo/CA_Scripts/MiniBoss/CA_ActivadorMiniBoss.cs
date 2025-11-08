using UnityEngine;

public class CA_ActivadorMiniBoss : MonoBehaviour
{
    public CA_MiniBossVigiasEsporales miniBoss; // ? NOMBRE CORRECTO
    public GameObject[] paredesBloqueo; // Array para las paredes de bloqueo

    void Start()
    {
        // Si no se asignó manualmente, buscar en el padre
        if (miniBoss == null)
        {
            miniBoss = GetComponentInParent<CA_MiniBossVigiasEsporales>();
        }

        // Buscar automáticamente paredes si no se asignaron manualmente
        if (paredesBloqueo == null || paredesBloqueo.Length == 0)
        {
            BuscarParedesBloqueo();
        }

        Debug.Log("Activador listo. MiniBoss asignado: " + (miniBoss != null));
        Debug.Log("Paredes de bloqueo asignadas: " + (paredesBloqueo != null ? paredesBloqueo.Length : 0));
    }

    void BuscarParedesBloqueo()
    {
        // Puedes buscar por tag, nombre, o de otra manera
        GameObject[] paredes = GameObject.FindGameObjectsWithTag("ParedBoss");
        if (paredes.Length > 0)
        {
            paredesBloqueo = paredes;
        }
        else
        {
            // Alternativa: buscar por nombre
            GameObject pared = GameObject.Find("ParedesBloqueoBoss");
            if (pared != null)
            {
                paredesBloqueo = new GameObject[] { pared };
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger activado por: " + other.gameObject.name + " - Tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Jugador entró en el círculo activador!");
            ActivarEnfrentamientoBoss();
        }
    }

    void ActivarEnfrentamientoBoss()
    {
        // Activar el mini boss
        if (miniBoss != null)
        {
            miniBoss.ActivarBoss();
        }
        else
        {
            Debug.LogError("MiniBoss no asignado en el activador");
        }

        // Activar las paredes de bloqueo
        ActivarParedesBloqueo();
    }

    void ActivarParedesBloqueo()
    {
        if (paredesBloqueo != null && paredesBloqueo.Length > 0)
        {
            foreach (GameObject pared in paredesBloqueo)
            {
                if (pared != null)
                {
                    pared.SetActive(true);
                    Debug.Log("Pared activada: " + pared.name);
                }
            }
            Debug.Log("Todas las paredes de bloqueo han sido activadas");
        }
        else
        {
            Debug.LogWarning("No se encontraron paredes de bloqueo para activar");
        }
    }

    // Método público para desactivar las paredes (útil cuando el boss es derrotado)
    public void DesactivarParedesBloqueo()
    {
        if (paredesBloqueo != null && paredesBloqueo.Length > 0)
        {
            foreach (GameObject pared in paredesBloqueo)
            {
                if (pared != null)
                {
                    pared.SetActive(false);
                }
            }
            Debug.Log("Paredes de bloqueo desactivadas");
        }
    }
}