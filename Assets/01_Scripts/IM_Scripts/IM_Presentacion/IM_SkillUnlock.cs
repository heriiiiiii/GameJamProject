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

    public float extraWaitTime = 1.5f;

    public GameObject interactHint;

    private VideoPlayer vp;
    private Rigidbody2D rb;

    // 🎵 Música de fondo con FADE
    private AudioSource bgMusic;
    private float originalMusicVolume;

    [Header("Fade de Música")]
    public float fadeDuration = 1.2f;  // ⬅ Fade suave

    void Start()
    {
        if (videoScreen != null)
        {
            vp = videoScreen.GetComponentInChildren<VideoPlayer>();
            videoScreen.SetActive(false);
        }

        if (panelAfterVideo != null)
            panelAfterVideo.SetActive(false);

        // 🎵 Buscar música de fondo automáticamente
        GameObject m = GameObject.Find("BG_Music");
        if (m != null)
        {
            bgMusic = m.GetComponent<AudioSource>();
            if (bgMusic != null)
                originalMusicVolume = bgMusic.volume;
        }
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
        // 1) Desbloquear habilidad
        switch (skillToUnlock)
        {
            case SkillType.Dash: player.canUseDash = true; break;
            case SkillType.WallJump: player.canUseWallJump = true; break;
            case SkillType.DoubleJump: player.canUseDoubleJump = true; break;
        }

        if (interactHint != null) interactHint.SetActive(false);

        // 2) Congelar jugador
        player.enabled = false;
        rb.velocity = Vector2.zero;

        // 🎵 FADE OUT DE LA MÚSICA
        if (bgMusic != null)
            yield return StartCoroutine(FadeMusic(0f));

        // 3) Preparar video
        videoScreen.SetActive(true);
        vp.Prepare();
        while (!vp.isPrepared)
            yield return null;

        vp.Play();
        while (vp.isPlaying)
            yield return null;

        videoScreen.SetActive(false);

        // 🎵 FADE IN DE LA MÚSICA
        if (bgMusic != null)
            yield return StartCoroutine(FadeMusic(originalMusicVolume));

        // 5) Mostrar panel
        panelAfterVideo.SetActive(true);

        yield return new WaitForSeconds(extraWaitTime);

        // 6) Tecla para cerrar
        switch (closeKeyOption)
        {
            case CloseKeyOption.C: while (!Input.GetKeyDown(KeyCode.C)) yield return null; break;
            case CloseKeyOption.Z: while (!Input.GetKeyDown(KeyCode.Z)) yield return null; break;
            case CloseKeyOption.X: while (!Input.GetKeyDown(KeyCode.X)) yield return null; break;
            case CloseKeyOption.F: while (!Input.GetKeyDown(KeyCode.F)) yield return null; break;
            case CloseKeyOption.AnyKey: while (!Input.anyKeyDown) yield return null; break;
        }

        panelAfterVideo.SetActive(false);

        // 7) Liberar jugador
        player.enabled = true;

        Destroy(gameObject);
    }

    // ⭐ FADE SUAVE
    IEnumerator FadeMusic(float targetVolume)
    {
        float startVolume = bgMusic.volume;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            bgMusic.volume = Mathf.Lerp(startVolume, targetVolume, t / fadeDuration);
            yield return null;
        }

        bgMusic.volume = targetVolume;
    }
}
