using System.Collections;
using UnityEngine;

public class NF_PlayerHealth : MonoBehaviour
{
    public int currentHealth = 3;
    public int maxHealth = 3;
    private NF_GameController gameController;

    private void Awake()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<NF_GameController>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void HealToFull()
    {
        currentHealth = maxHealth;
    }

    private void Die()
    {
        Debug.Log("☠️ El jugador ha muerto. Respawn en Zone.");
        StartCoroutine(gameController.Respawn(1f, "Zone"));
    }
}
