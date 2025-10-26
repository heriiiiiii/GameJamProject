using UnityEngine;

public class CA_ActivadorMiniBoss : MonoBehaviour
{
    public CA_MiniBossVigiasEsporales miniBoss; // ? NOMBRE CORRECTO

    void Start()
    {
        // Si no se asignó manualmente, buscar en el padre
        if (miniBoss == null)
        {
            miniBoss = GetComponentInParent<CA_MiniBossVigiasEsporales>();
        }

        Debug.Log("Activador listo. MiniBoss asignado: " + (miniBoss != null));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger activado por: " + other.gameObject.name + " - Tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Jugador entró en el círculo activador!");
            if (miniBoss != null)
            {
                miniBoss.ActivarBoss();
            }
            else
            {
                Debug.LogError("MiniBoss no asignado en el activador");
            }
        }
    }
}