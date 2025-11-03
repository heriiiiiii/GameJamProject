using System.Collections;
using UnityEngine;

public class MohoSensorial : MonoBehaviour
{
    [Header("Daño y duración")]
    public int danoPorSegundo = 1;
    public float duracionInmovilizacion = 2f; // tiempo total atrapado
    public float fuerzaEmpuje = 10f;          // fuerza al soltar

    [Header("Animaciones")]
    private Animator animator;

    // Parámetros del Animator
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int IsInmovilizing = Animator.StringToHash("IsInmovilizing");

    private bool jugadorEnContacto = false;
    private GameObject jugador;
    private Coroutine danioCoroutine;
    private Coroutine inmovilizacionCoroutine;
    private Vector3 posicionCentro;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool(IsAttacking, false);
        animator.SetBool(IsInmovilizing, false);
        posicionCentro = transform.position;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !jugadorEnContacto)
        {
            jugador = collision.gameObject;
            jugadorEnContacto = true;

            // Comienza la animación de ataque inicial
            animator.SetBool(IsAttacking, true);
            animator.SetBool(IsInmovilizing, false);

            // Esperar antes de atrapar
            StartCoroutine(PrepararAtrapamiento());
        }
    }

    IEnumerator PrepararAtrapamiento()
    {
        // Espera 1 segundo antes de atrapar (para centrar al player)
        yield return new WaitForSeconds(1f);

        if (jugadorEnContacto && jugador != null)
        {
            jugador.transform.position = posicionCentro;

            animator.SetBool(IsAttacking, false);
            animator.SetBool(IsInmovilizing, true);

            // Desactivar movimiento del jugador
            Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.gravityScale = 0f;
            }

            CA_PlayerController movimiento = jugador.GetComponent<CA_PlayerController>();
            if (movimiento != null)
                movimiento.enabled = false;

            // Daño periódico
            danioCoroutine = StartCoroutine(DanioConstante());

            // Inicia la fase de atrapamiento
            inmovilizacionCoroutine = StartCoroutine(FaseInmovilizacion());
        }
    }

    IEnumerator FaseInmovilizacion()
    {
        //  Esperar (duracionInmovilizacion - 1 segundo)
        // porque queremos que el último segundo sea de animación Idle
        float tiempoIdlePrevio = 0.5f;
        float tiempoAtrapado = Mathf.Max(0f, duracionInmovilizacion - tiempoIdlePrevio);

        // Mantener animación de atrapado durante la primera parte
        yield return new WaitForSeconds(tiempoAtrapado);

        //  Cambiar a Idle 1 segundo antes de liberar
        animator.SetBool(IsInmovilizing, false);

        // Esperar 1 segundo más (Idle visible mientras aún está atrapado)
        yield return new WaitForSeconds(tiempoIdlePrevio);

        //  Ahora liberar al jugador
        SoltarJugador();
    }

    void SoltarJugador()
    {
        if (jugador != null)
        {
            Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 1f;
                Vector2 direccionEmpuje = new Vector2(Random.Range(-1f, 1f), 1f).normalized;
                rb.velocity = Vector2.zero;
                rb.AddForce(direccionEmpuje * fuerzaEmpuje, ForceMode2D.Impulse);
            }

            CA_PlayerController movimiento = jugador.GetComponent<CA_PlayerController>();
            if (movimiento != null)
                movimiento.enabled = true;
        }

        jugadorEnContacto = false;

        if (danioCoroutine != null)
            StopCoroutine(danioCoroutine);
        if (inmovilizacionCoroutine != null)
            StopCoroutine(inmovilizacionCoroutine);

        //  Mantener Idle después de liberar
        animator.SetBool(IsAttacking, false);
        animator.SetBool(IsInmovilizing, false);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && jugadorEnContacto)
        {
            if (animator.GetBool(IsAttacking) && !animator.GetBool(IsInmovilizing))
            {
                jugadorEnContacto = false;

                if (danioCoroutine != null)
                    StopCoroutine(danioCoroutine);

                animator.SetBool(IsAttacking, false);
                animator.SetBool(IsInmovilizing, false);
            }
        }
    }

    IEnumerator DanioConstante()
    {
        NF_PlayerHealth salud = jugador.GetComponent<NF_PlayerHealth>();

        while (jugadorEnContacto && animator.GetBool(IsInmovilizing))
        {
            if (salud != null)
                salud.TakeDamageWithoutKnockback(danoPorSegundo); // ✅ sin knockback

            yield return new WaitForSeconds(1f);
        }
    }


    void Update()
    {
        if (jugadorEnContacto)
        {
            Debug.Log("Jugador atrapado: " + animator.GetBool(IsInmovilizing));
        }
    }
}
