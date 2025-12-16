using UnityEngine;

public class NF_CheckpointParkour : MonoBehaviour
{
    private NF_GameController gameController;

    private void Awake()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<NF_GameController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Solo actualiza el punto de respawn Parkour
            gameController.UpdateCheckpoint(transform.position, "Parkour");
            Debug.Log("🏁 Checkpoint Parkour actualizado en: " + transform.position);
        }
    }
}
