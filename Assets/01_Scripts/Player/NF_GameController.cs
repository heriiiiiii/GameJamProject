using System.Collections;
using UnityEngine;

public class NF_GameController : MonoBehaviour
{
    public GameObject player;
    private NF_PlayerHealth playerHealth;

    private Vector2 checkpointZonePos;
    private Vector2 checkpointParkourPos;
    public int lifeRespawn; 
    private void Awake()
    {
        playerHealth = player.GetComponent<NF_PlayerHealth>();

        checkpointZonePos = player.transform.position;
        checkpointParkourPos = player.transform.position;
    }

    public void UpdateCheckpoint(Vector2 pos, string checkpointType)
    {
        if (checkpointType == "Zone")
        {
            checkpointZonePos = pos;
            Debug.Log($"💾 Checkpoint Zone guardado en: {pos}");
        }
        else if (checkpointType == "Parkour")
        {
            checkpointParkourPos = pos;
            Debug.Log($"🏁 Checkpoint Parkour guardado en: {pos}");
        }
    }

    public IEnumerator Respawn(float duration, string checkpointType)
    {
        // 🔹 Referencias necesarias
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        CA_PlayerController controller = player.GetComponent<CA_PlayerController>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        NF_PlayerHealth health = player.GetComponent<NF_PlayerHealth>();

        // 🔸 Desactiva movimiento y oculta al jugador temporalmente
        if (controller != null)
            controller.enabled = false;
        if (sr != null)
            sr.enabled = false;
        if (rb != null)
            rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(duration);

        // 🔹 Determina a qué checkpoint ir
        if (checkpointType == "Parkour")
        {
            player.transform.position = checkpointParkourPos;

            // Si murió y revivió aquí, le damos 1 de vida mínima
            if (health != null && health.currentHealth <= 0)
                health.currentHealth = 1;
        }
        else // 🔹 Respawn Zone
        {
            player.transform.position = checkpointZonePos;

            // 🩸 Curar vida completa al reaparecer en Zone
            if (health != null)
                health.currentHealth = lifeRespawn;
        }

        // 🔸 Reactiva todo
        if (sr != null)
            sr.enabled = true;
        if (controller != null)
            controller.enabled = true;
    }


}
