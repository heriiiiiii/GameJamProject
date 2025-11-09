using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class IM_SkillUnlock : MonoBehaviour
{
    public enum SkillType { Dash, WallJump, DoubleJump }
    public SkillType skillToUnlock;

    private bool playerInRange = false;
    private CA_PlayerController player;

    [Header("UI")]
    public GameObject videoScreen;
    public GameObject panelAfterVideo;

    public enum CloseKeyOption { C, Z, X, F, AnyKey }
    public CloseKeyOption closeKeyOption = CloseKeyOption.C;

    [Header("Tiempo antes de poder cerrar el panel")]
    public float extraWaitTime = 1.5f;

    [Header("Opcional")]
    public GameObject interactHint;

    private VideoPlayer vp;
    private Rigidbody2D rb;

    void Start()
    {
        if (videoScreen != null)
        {
            vp = videoScreen.GetComponentInChildren<VideoPlayer>();
            videoScreen.SetActive(false);
        }

        if (panelAfterVideo != null)
            panelAfterVideo.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.G))
        {
            StartCoroutine(UnlockSequence());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            player = collision.GetComponent<CA_PlayerController>();
            rb = collision.GetComponent<Rigidbody2D>();

            if (interactHint != null)
                interactHint.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;

            if (interactHint != null)
                interactHint.SetActive(false);
        }
    }

    IEnumerator UnlockSequence()
    {
        // ✅ 1) Desbloquear habilidad
        switch (skillToUnlock)
        {
            case SkillType.Dash: player.canUseDash = true; break;
            case SkillType.WallJump: player.canUseWallJump = true; break;
            case SkillType.DoubleJump: player.canUseDoubleJump = true; break;
        }

        if (interactHint != null) interactHint.SetActive(false);

        // ✅ 2) Congelar jugador
        player.enabled = false;
        rb.velocity = Vector2.zero;

        // ✅ 3) Preparar y reproducir video
        videoScreen.SetActive(true);
        vp.Prepare();
        while (!vp.isPrepared)
            yield return null;

        vp.Play();

        while (vp.isPlaying)
            yield return null;

        videoScreen.SetActive(false);

        // ✅ 5) Mostrar panel
        panelAfterVideo.SetActive(true);

        // Esperar tiempo antes de permitir cerrar
        yield return new WaitForSeconds(extraWaitTime);

        // ✅ 6) Leer la tecla configurada
        switch (closeKeyOption)
        {
            case CloseKeyOption.C:
                while (!Input.GetKeyDown(KeyCode.C)) yield return null;
                break;
            case CloseKeyOption.Z:
                while (!Input.GetKeyDown(KeyCode.Z)) yield return null;
                break;
            case CloseKeyOption.X:
                while (!Input.GetKeyDown(KeyCode.X)) yield return null;
                break;
            case CloseKeyOption.F:
                while (!Input.GetKeyDown(KeyCode.F)) yield return null;
                break;
            case CloseKeyOption.AnyKey:
                while (!Input.anyKeyDown) yield return null;
                break;
        }

        panelAfterVideo.SetActive(false);

        // ✅ 7) Liberar jugador + destruir item
        player.enabled = true;
        Destroy(gameObject);
    }
}
