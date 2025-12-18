using System.Collections;
using UnityEngine;

public class BrotePanico : MonoBehaviour
{
    [Header("Detección del jugador")]
    public float rangoDeteccion = 5f;
    public string tagJugador = "Player";

    [Header("Disparo")]
    public GameObject prefabEspina;
    public float tiempoEntreDisparos = 3f;
    public Transform puntoDisparo;

    [Header("Daño por contacto")]
    public int danoContacto = 1;
    public float knockbackForce = 6f;

    // ===============================
    // 🔊 AUDIO ATAQUE
    // ===============================
    [Header("🔊 Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip attackClip;

    // Componentes
    private Animator animator;
    public bool jugadorDetectado = false;
    private float temporizador;
    private Transform jugador;

    // Parámetros Animator
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int PlayerDetected = Animator.StringToHash("PlayerDetected");
    private static readonly int IsDead = Animator.StringToHash("IsDead");

    void Start()
    {
        animator = GetComponent<Animator>();
        temporizador = tiempoEntreDisparos;

        animator.SetBool(PlayerDetected, false);
        animator.SetBool(IsAttacking, false);
        animator.SetBool(IsDead, false);

        // Config audio segura
        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (sfxSource)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
        }
    }

    void Update()
    {
        if (animator.GetBool(IsDead)) return;

        bool deteccionAnterior = jugadorDetectado;
        DetectarJugador();

        if (deteccionAnterior != jugadorDetectado)
            animator.SetBool(PlayerDetected, jugadorDetectado);

        if (jugadorDetectado)
        {
            temporizador -= Time.deltaTime;
            if (temporizador <= 0f)
            {
                animator.SetBool(IsAttacking, true);
                temporizador = tiempoEntreDisparos;
            }
        }
        else
        {
            animator.SetBool(IsAttacking, false);
        }
    }

    void DetectarJugador()
    {
        jugadorDetectado = false;
        Collider2D[] colisiones = Physics2D.OverlapCircleAll(transform.position, rangoDeteccion);
        foreach (Collider2D col in colisiones)
        {
            if (col.CompareTag(tagJugador))
            {
                jugadorDetectado = true;
                jugador = col.transform;
                break;
            }
        }
    }

    // ===============================
    // 🔥 ATAQUE + SONIDO
    // ===============================
    // Este método se llama desde el Animation Event
    public void Disparar()
    {
        if (prefabEspina == null || jugador == null) return;

        // 🔊 SONIDO DEL ATAQUE (UNA SOLA VEZ)
        if (sfxSource && attackClip)
            sfxSource.PlayOneShot(attackClip, 1f);

        Vector2 direccionBase = (jugador.position - transform.position).normalized;
        float[] angulos = { -25f, 0f, 25f };

        foreach (float ang in angulos)
        {
            Quaternion rot = Quaternion.Euler(0, 0, ang);
            Vector2 dir = rot * direccionBase;

            GameObject bala = Instantiate(
                prefabEspina,
                puntoDisparo.position,
                Quaternion.identity
            );

            bala.GetComponent<EspinaBrote>().Inicializar(dir);
        }
    }

    public void Morir()
    {
        animator.SetBool(IsDead, true);
        GetComponent<Collider2D>().enabled = false;
        enabled = false;
        Destroy(gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(tagJugador) && !animator.GetBool(IsDead))
        {
            PlayerHealth salud = collision.gameObject.GetComponent<PlayerHealth>();
            if (salud != null)
                salud.RecibirDanio(danoContacto);

            Rigidbody2D rbPlayer = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rbPlayer != null)
            {
                Vector2 direccion = (collision.transform.position - transform.position).normalized;
                rbPlayer.AddForce(direccion * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
}
