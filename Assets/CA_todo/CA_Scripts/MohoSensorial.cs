using System.Collections;
using UnityEngine;

public class MohoSensorial : MonoBehaviour
{
    [Header("Daño y duración")]
    public int danoPorSegundo = 1;
    public float duracionInmovilizacion = 2f;
    public float fuerzaEmpuje = 10f;

    [Header("Animaciones")]
    private Animator animator;

    [Header("Referencias")]
    public Transform puntoAtraccion;

    // ===============================
    // 🔊 AUDIO (POR INSTANCIA)
    // ===============================
    [Header("🔊 Clips")]
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip grabClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip releaseClip;

    [SerializeField] private float audioDistance = 6f;

    private AudioSource idleSource; // SOLO idle
    private AudioSource sfxSource;  // SOLO efectos

    private Transform player;
    private bool idlePlaying = false;
    private bool grabSoundPlayed = false;

    // Animator params
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int IsInmovilizing = Animator.StringToHash("IsInmovilizing");
    private static readonly int IsDead = Animator.StringToHash("IsDead");

    private bool jugadorEnContacto = false;
    private GameObject jugador;
    private Coroutine danioCoroutine;
    private Coroutine inmovilizacionCoroutine;
    private bool estaMuerto = false;

    void Awake()
    {
        animator = GetComponent<Animator>();

        // 🔥 TOMAR LOS AUDIOSOURCE DE ESTA INSTANCIA
        AudioSource[] sources = GetComponents<AudioSource>();

        if (sources.Length < 2)
        {
            Debug.LogError($"[{name}] Necesita 2 AudioSource (Idle + SFX)");
            return;
        }

        idleSource = sources[0];
        sfxSource = sources[1];

        // Config segura
        idleSource.loop = true;
        idleSource.playOnAwake = false;
        idleSource.spatialBlend = 0f;

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
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
        if (!player || !idleClip) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= audioDistance && !idlePlaying)
        {
            idleSource.clip = idleClip;
            idleSource.enabled = true;
            idleSource.Play();
            idlePlaying = true;
        }
        else if (distance > audioDistance && idlePlaying)
        {
            idleSource.Stop();
            idlePlaying = false;
        }
    }

    // 🔥 FORZADO TOTAL SOLO DE ESTA INSTANCIA
    void ForzarParadaIdle()
    {
        if (idleSource)
        {
            idleSource.Stop();
            idleSource.clip = null;
            idleSource.enabled = false;
        }

        idlePlaying = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (estaMuerto) return;

        if (collision.CompareTag("Player") && !jugadorEnContacto)
        {
            jugador = collision.gameObject;
            jugadorEnContacto = true;

            if (grabClip && !grabSoundPlayed)
            {
                sfxSource.PlayOneShot(grabClip, 1f);
                grabSoundPlayed = true;
            }

            AtraparJugadorInmediatamente();
        }
    }

    void AtraparJugadorInmediatamente()
    {
        if (!jugador) return;

        jugador.transform.position = puntoAtraccion.position;

        animator.SetBool(IsAttacking, false);
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
        inmovilizacionCoroutine = StartCoroutine(FaseInmovilizacion());
    }

    IEnumerator FaseInmovilizacion()
    {
        float tiempoIdlePrevio = 0.5f;
        float tiempoAtrapado = Mathf.Max(0f, duracionInmovilizacion - tiempoIdlePrevio);

        yield return new WaitForSeconds(tiempoAtrapado);
        if (estaMuerto) yield break;

        animator.SetBool(IsInmovilizing, false);
        yield return new WaitForSeconds(tiempoIdlePrevio);

        SoltarJugador();
    }

    void SoltarJugador()
    {
        if (releaseClip)
            sfxSource.PlayOneShot(releaseClip, 1f);

        if (jugador)
        {
            Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
            if (rb)
            {
                rb.gravityScale = 1f;
                rb.velocity = Vector2.zero;
                rb.AddForce(Vector2.up * fuerzaEmpuje, ForceMode2D.Impulse);
            }

            CA_PlayerController movimiento = jugador.GetComponent<CA_PlayerController>();
            if (movimiento) movimiento.enabled = true;
        }

        jugadorEnContacto = false;
        grabSoundPlayed = false;

        if (danioCoroutine != null) StopCoroutine(danioCoroutine);
        if (inmovilizacionCoroutine != null) StopCoroutine(inmovilizacionCoroutine);

        animator.SetBool(IsAttacking, false);
        animator.SetBool(IsInmovilizing, false);
    }

    IEnumerator DanioConstante()
    {
        NF_PlayerHealth salud = jugador.GetComponent<NF_PlayerHealth>();

        while (jugadorEnContacto && animator.GetBool(IsInmovilizing) && !estaMuerto)
        {
            if (attackClip)
                sfxSource.PlayOneShot(attackClip, 0.8f);

            salud?.TakeDamageWithoutKnockback(danoPorSegundo);
            yield return new WaitForSeconds(1f);
        }
    }

    public void Morir()
    {
        if (estaMuerto) return;

        estaMuerto = true;

        // 🔥 SOLO ESTE ENEMIGO SE CALLA
        ForzarParadaIdle();

        if (jugadorEnContacto)
            SoltarJugador();

        animator.SetBool(IsDead, true);

        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
    }

    void OnDisable()
    {
        ForzarParadaIdle();
    }

    void OnDestroy()
    {
        ForzarParadaIdle();
    }
}
