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

    [Header("Referencias")]
    public Transform puntoAtraccion;

    // Parámetros del Animator
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int IsInmovilizing = Animator.StringToHash("IsInmovilizing");
    private static readonly int IsDead = Animator.StringToHash("IsDead");

    private bool jugadorEnContacto = false;
    private GameObject jugador;
    private Coroutine danioCoroutine;
    private Coroutine inmovilizacionCoroutine;
    private Vector3 posicionCentro;
    private bool estaMuerto = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool(IsAttacking, false);
        animator.SetBool(IsInmovilizing, false);
        animator.SetBool(IsDead, false);
        posicionCentro = transform.position;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Si está muerto, no hacer nada
        if (estaMuerto) return;

        if (collision.CompareTag("Player") && !jugadorEnContacto)
        {
            jugador = collision.gameObject;
            jugadorEnContacto = true;

            // Atrapar inmediatamente sin esperar
            AtraparJugadorInmediatamente();
        }
    }

    void AtraparJugadorInmediatamente()
    {
        if (estaMuerto || jugador == null) return;

        // Centrar al jugador inmediatamente
        //jugador.transform.position = posicionCentro;
        jugador.transform.position = puntoAtraccion.position;

        // Cambiar directamente a la animación de inmovilización
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

        // Iniciar fase de inmovilización
        inmovilizacionCoroutine = StartCoroutine(FaseInmovilizacion());
    }

    IEnumerator FaseInmovilizacion()
    {
        // Si está muerto, cancelar
        if (estaMuerto) yield break;

        // Tiempo que estará atrapado (duración total menos un pequeño tiempo para la transición)
        float tiempoIdlePrevio = 0.5f;
        float tiempoAtrapado = Mathf.Max(0f, duracionInmovilizacion - tiempoIdlePrevio);

        // Mantener animación de atrapado durante la mayor parte del tiempo
        yield return new WaitForSeconds(tiempoAtrapado);

        // Si está muerto durante la espera, cancelar
        if (estaMuerto) yield break;

        // Cambiar a Idle un poco antes de liberar
        animator.SetBool(IsInmovilizing, false);

        // Esperar un poco más (Idle visible mientras aún está atrapado)
        yield return new WaitForSeconds(tiempoIdlePrevio);

        // Si está muerto durante la espera, cancelar
        if (estaMuerto) yield break;

        // Liberar al jugador
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

        // Volver a estado normal
        animator.SetBool(IsAttacking, false);
        animator.SetBool(IsInmovilizing, false);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // Si está muerto, no hacer nada
        if (estaMuerto) return;

        // Solo permitir salir si no ha comenzado la inmovilización
        if (collision.CompareTag("Player") && jugadorEnContacto)
        {
            if (!animator.GetBool(IsInmovilizing))
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

        while (jugadorEnContacto && animator.GetBool(IsInmovilizing) && !estaMuerto)
        {
            if (salud != null)
                salud.TakeDamageWithoutKnockback(danoPorSegundo);

            yield return new WaitForSeconds(1f);
        }
    }

    // Método para activar la muerte del enemigo
    public void Morir()
    {
        if (estaMuerto) return;

        estaMuerto = true;

        // Liberar al jugador si estaba atrapado
        if (jugadorEnContacto)
        {
            SoltarJugador();
        }

        // Detener todas las corrutinas
        if (danioCoroutine != null)
            StopCoroutine(danioCoroutine);
        if (inmovilizacionCoroutine != null)
            StopCoroutine(inmovilizacionCoroutine);

        // Activar animación de muerte
        animator.SetBool(IsDead, true);
        animator.SetBool(IsAttacking, false);
        animator.SetBool(IsInmovilizing, false);

        // Opcional: Deshabilitar colisiones
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;
    }

    void Update()
    {
        if (jugadorEnContacto && !estaMuerto)
        {
            Debug.Log("Jugador atrapado: " + animator.GetBool(IsInmovilizing));
        }
    }
}