using System.Collections;
using UnityEngine;

public class NF_TestEnemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldown = 0.7f; // Tiempo entre golpes
    private bool canAttack = true;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && canAttack)
        {
            NF_PlayerHealth playerHealth = collision.GetComponent<NF_PlayerHealth>();
            if (playerHealth != null)
            {
                // 🧭 Calcular dirección del golpe desde el enemigo hacia el jugador
                Vector2 hitDirection = (collision.transform.position - transform.position).normalized;

                // 💥 Aplicar daño al jugador (esto activa el knockback + hitstop)
                playerHealth.TakeDamage(damage, hitDirection);

                // ⏳ Evitar múltiples daños por frame
                StartCoroutine(AttackCooldown());
            }
        }
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
