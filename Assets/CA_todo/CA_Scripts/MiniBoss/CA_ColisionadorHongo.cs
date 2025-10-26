using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_ColisionadorHongo : MonoBehaviour
{
    public int indiceHongo;
    public GameObject miniBossObject; //  CAMBIADO A GameObject
    private CA_MiniBossVigiasEsporales miniBoss; //  Referencia al script
    private CA_RecolEnemy recolEnemy;

    void Start()
    {
        // Obtener el script del mini boss desde el GameObject
        if (miniBossObject != null)
        {
            miniBoss = miniBossObject.GetComponent<CA_MiniBossVigiasEsporales>();
        }

        // Si no se asignó, buscar en el padre
        if (miniBoss == null)
        {
            miniBoss = GetComponentInParent<CA_MiniBossVigiasEsporales>();
        }

        // Obtener componente de recoil
        recolEnemy = GetComponent<CA_RecolEnemy>();
        if (recolEnemy == null)
        {
            recolEnemy = gameObject.AddComponent<CA_RecolEnemy>();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Detectar cuando el ataque del jugador golpea este hongo
        if (other.CompareTag("AttackHitbox"))
        {
            Debug.Log("¡Hongo " + indiceHongo + " golpeado por ataque de espada!");

            float danoAtaque = 15f; // Ajusta según el balance de tu juego

            if (miniBoss != null)
            {
                miniBoss.RecibirDano(indiceHongo, danoAtaque);

                // Aplicar efecto de recoil al hongo individual
                if (recolEnemy != null)
                {
                    Vector2 hitDirection = (transform.position - other.transform.position).normalized;
                    recolEnemy.EnemyHit(danoAtaque, hitDirection, 5f);
                }
            }
        }
    }
}
