using System.Collections;
using UnityEngine;

public class MohoSensorial : MonoBehaviour
{
    [Header("Daño y duración")]
    public int danoPorSegundo = 1;
    public float duracionInmovilizacion = 2f;
    public float fuerzaEmpuje = 10f;

    [Header("Referencias")]
    public Transform puntoAtraccion;

    [Header("🔊 Audio")]
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip grabClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip releaseClip;
    [SerializeField] private float audioDistance = 6f;

    private AudioSource idleSource;
    private AudioSource sfxSource;

    private Animator animator;
    private Transform player;
    private GameObject jugador;

    private bool jugadorEnContacto = false;
    private bool estaMuerto = false;
    private bool idlePlaying = false;
    private bool grabSoundPlayed = false;

    private Coroutine danioCoroutine;
    private Coroutine inmovilizacionCoroutine;

    // Animator params
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int IsInmovilizing = Animator.StringToHash("IsInmovilizing");
    private static readonly int IsDead = Animator.StringToHash("IsDead");

    void Awake()
    {
        animator = GetComponent<Animator>();

        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            idleSource = sources[0];
            sfxSource = sources[1];

            idleSource.loop = true;
            idleSource.playOnAwake = false;
            idleSource.spatialBlend = 0f;

            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
        }
        else
        {
            Debug.LogError($"[{name}] Necesita 2 AudioSource (Idle + SFX)");
        }
    }

    void Start()
    {
        animator.SetBool(IsAttacking, false);
        animator.SetBool(IsInmovilizing, false);
        animator.SetBool(IsDead, false);

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (!estaMuerto)
            HandleIdleAudio();
    }

    // ===============================
    // 🔊 IDLE POR PROXIMIDAD
    // ===============================
    void HandleIdleAudio()
    {
        if (!player || !idleClip || !idleSource) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= audioDistance && !idlePlaying)
        {
            idleSource.clip = idleClip;
            idleSource.Play();
            idlePlaying = true;
        }
        else if (distance > audioDistance && idlePlaying)
        {
            idleSource.Stop();
            idlePlaying = false;
        }
    }

    void ForzarParadaIdle()
    {
        if (idleSource)
            idleSource.Stop();

        idlePlaying = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (estaMuerto || jugadorEnContacto) return;

        if (collision.CompareTag("Player"))
        {
            jugador = collision.gameObject;
            jugadorEnContacto = true;

            if (grabClip && !grabSoundPlayed && sfxSource)
            {
                sfxSource.PlayOneShot(grabClip, 1f);
                grabSoundPlayed = true;
            }

            AtraparJugador();
        }
    }

    void AtraparJugador()
    {
        if (!jugador) return;

        jugador.transform.position = puntoAtraccion.position;

        animator.SetBool(IsInmovilizing, true);

        Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        CA_PlayerController movimiento = jugador.GetComponent<CA_PlayerController>();
        if (movimiento) movimiento.enabled = false;

        danioCoroutine = StartCoroutine(DanioConstante());
        inmovilizacionCoroutine = StartCoroutine(TemporizadorLiberacion());
    }

    IEnumerator TemporizadorLiberacion()
    {
        yield return new WaitForSeconds(duracionInmovilizacion);
        SoltarJugador();
    }

    IEnumerator DanioConstante()
    {
        NF_PlayerHealth salud = jugador.GetComponent<NF_PlayerHealth>();

        while (jugadorEnContacto && !estaMuerto)
        {
            if (attackClip && sfxSource)
                sfxSource.PlayOneShot(attackClip, 0.8f);

            salud?.TakeDamageWithoutKnockback(danoPorSegundo);
            yield return new WaitForSeconds(1f);
        }
    }

    void SoltarJugador()
    {
        if (!jugador) return;

        if (releaseClip && sfxSource)
            sfxSource.PlayOneShot(releaseClip, 1f);

        Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.gravityScale = 1f;
            rb.velocity = Vector2.zero;
            rb.AddForce(Vector2.up * fuerzaEmpuje, ForceMode2D.Impulse);
        }

        CA_PlayerController movimiento = jugador.GetComponent<CA_PlayerController>();
        if (movimiento) movimiento.enabled = true;

        jugadorEnContacto = false;
        grabSoundPlayed = false;

        if (danioCoroutine != null) StopCoroutine(danioCoroutine);
        if (inmovilizacionCoroutine != null) StopCoroutine(inmovilizacionCoroutine);

        animator.SetBool(IsInmovilizing, false);
    }

    public void Morir()
    {
        if (estaMuerto) return;

        estaMuerto = true;
        ForzarParadaIdle();

        if (jugadorEnContacto)
            SoltarJugador();

        animator.SetBool(IsDead, true);

        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
    }

    void OnDisable() => ForzarParadaIdle();
    void OnDestroy() => ForzarParadaIdle();
}
