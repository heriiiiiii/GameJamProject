using UnityEngine;

public class NF_CheckpointZone : MonoBehaviour
{
    private NF_GameController gameController;
    private NF_PlayerHealth playerHealth;
    private bool playerInRange = false;

    [Header("🔊 Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip checkpointClip;

    private void Awake()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController")
            .GetComponent<NF_GameController>();

        playerHealth = GameObject.FindGameObjectWithTag("Player")
            .GetComponent<NF_PlayerHealth>();

        // 🔒 Seguridad por si se olvidan asignarlo
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Si el jugador está dentro del área y presiona F
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            // 🔊 SONIDO
            if (checkpointClip && audioSource)
                audioSource.PlayOneShot(checkpointClip);

            // 📍 Guardar checkpoint
            gameController.UpdateCheckpoint(transform.position, "Zone");

            // ❤️ Curar al guardar
            playerHealth.HealToFull();

            Debug.Log("💖 Checkpoint Zone guardado, vida restaurada y sonido reproducido.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("✅ Jugador entró al área del checkpoint.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("🚪 Jugador salió del área del checkpoint.");
        }
    }
}
