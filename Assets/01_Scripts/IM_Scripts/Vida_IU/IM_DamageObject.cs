using UnityEngine;

public class IM_DamageObject : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damageAmount = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si el objeto que entra tiene el componente IM_PlayerHealth, le hacemos daño
        IM_PlayerHealth player = other.GetComponent<IM_PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damageAmount);
        }
    }
}
