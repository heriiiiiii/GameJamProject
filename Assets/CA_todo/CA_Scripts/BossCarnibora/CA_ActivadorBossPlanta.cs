using UnityEngine;

public class CA_ActivadorBossPlanta : MonoBehaviour
{
    public CA_BossPlantaCarnivora bossPlanta;

    void Start()
    {
        if (bossPlanta == null)
        {
            bossPlanta = GetComponentInParent<CA_BossPlantaCarnivora>();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Activador de Boss Planta detectado!");
            if (bossPlanta != null)
            {
                bossPlanta.ActivarBoss();
            }
            else
            {
                Debug.LogError("Boss Planta no asignado en el activador");
            }
        }
    }
}