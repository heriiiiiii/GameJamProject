using UnityEngine;

public class NF_CheckpointParkour : MonoBehaviour
{
    private NF_GameController gameController;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip checkpointClip;

    private bool isActive = false;

    private void Awake()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController")
            .GetComponent<NF_GameController>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void Activate()
    {
        // Si ya es el checkpoint activo, no hacer nada
        if (isActive) return;

        isActive = true;

        if (checkpointClip && audioSource)
            audioSource.PlayOneShot(checkpointClip);

        gameController.UpdateCheckpoint(transform.position, "Parkour");
    }

    public void Deactivate()
    {
        isActive = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        gameController.SetActiveCheckpoint(this);
    }
}
