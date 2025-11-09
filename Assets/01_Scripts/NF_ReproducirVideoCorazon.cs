using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class NF_ReproducirVideoCorazon : MonoBehaviour
{
    [Header("UI / Video")]
    public GameObject videoScreen;
    public GameObject mensajeInteractuar;

    [Header("Parámetro recibido del Activador")]
    public string recibidoDesdeActivador; // 🔹 Dato pasado desde el activador enemigo

    private bool playerInRange = false;
    private VideoPlayer vp;
    private CA_PlayerController player;
    private Rigidbody2D rb;

    void Start()
    {
        vp = videoScreen.GetComponentInChildren<VideoPlayer>();
        vp.playOnAwake = false;

        if (vp.audioOutputMode == VideoAudioOutputMode.AudioSource)
        {
            AudioSource a = vp.GetTargetAudioSource(0);
            if (a != null)
                a.playOnAwake = false;
        }

        videoScreen.SetActive(false);
        mensajeInteractuar.SetActive(false);

        // 🔹 El prefab puede estar invisible al instanciarse
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = false;

        Debug.Log("Prefab Corazón creado desde activador con ID: " + recibidoDesdeActivador);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.G))
            StartCoroutine(ReproducirVideo());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            player = collision.GetComponent<CA_PlayerController>();
            rb = collision.GetComponent<Rigidbody2D>();
            mensajeInteractuar.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            mensajeInteractuar.SetActive(false);
        }
    }

    IEnumerator ReproducirVideo()
    {
        mensajeInteractuar.SetActive(false);
        player.enabled = false;
        rb.velocity = Vector2.zero;

        videoScreen.SetActive(true);
        vp.Prepare();
        while (!vp.isPrepared) yield return null;

        vp.Play();
        while (vp.isPlaying) yield return null;

        videoScreen.SetActive(false);
        player.enabled = true;

        Destroy(gameObject);
    }
}
