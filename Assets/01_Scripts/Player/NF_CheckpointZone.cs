using UnityEngine;

public class NF_CheckpointZone : MonoBehaviour
{
    private NF_GameController gameController;
    private NF_PlayerHealth playerHealth;
    private bool playerInRange = false;

    private void Awake()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<NF_GameController>();
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<NF_PlayerHealth>();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            gameController.UpdateCheckpoint(transform.position, "Zone");
            playerHealth.HealToFull(); // Cura al guardar
            Debug.Log("💖 Checkpoint Zone guardado y vida restaurada.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = false;
    }
}
