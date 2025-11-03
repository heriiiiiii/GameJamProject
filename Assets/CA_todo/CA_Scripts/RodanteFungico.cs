using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RodanteFungico : MonoBehaviour
{
    [Header("Puntos de movimiento")]
    public Transform puntoA; // Posición cuando sale (ataque)
    public Transform puntoB; // Posición escondido

    [Header("Parámetros")]
    public float velocidad = 2f;
    public float knockbackForce = 15f;
    public int dano = 1;

    [Header("Detección del jugador")]
    public float rangoDeteccion = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private bool jugadorDetectado = false;
    private bool estaAtacando = false;
    private bool movimientoCompletado = true;

    // Parámetros del Animator
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int IsIdleAttack = Animator.StringToHash("IsIdleAttack");
    private static readonly int IsReturning = Animator.StringToHash("IsReturning");

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.isKinematic = true;
        rb.freezeRotation = true;

        // Inicia escondido
        transform.position = puntoB.position;

        // Configurar estado inicial
        animator.SetBool(IsReturning, false);
        animator.SetBool(IsIdleAttack, false);
        animator.SetBool(IsAttacking, false);
    }

    void Update()
    {
        bool deteccionAnterior = jugadorDetectado;
        DetectarJugadorPorTag();

        Mover();
        VerificarPosicion();
        ActualizarAnimaciones();
    }

    void DetectarJugadorPorTag()
    {
        jugadorDetectado = false;

        Collider2D[] colisiones = Physics2D.OverlapCircleAll(transform.position, rangoDeteccion);

        foreach (Collider2D col in colisiones)
        {
            if (col.CompareTag("Player"))
            {
                jugadorDetectado = true;
                break;
            }
        }
    }

    void Mover()
    {
        if (estaAtacando) return;

        Vector3 destino = jugadorDetectado ? puntoA.position : puntoB.position;

        // Mover hacia el destino
        transform.position = Vector2.MoveTowards(transform.position, destino, velocidad * Time.deltaTime);

        // Verificar si llegó al destino
        movimientoCompletado = Vector2.Distance(transform.position, destino) < 0.1f;
    }

    void VerificarPosicion()
    {
        // Si está en posición de ataque (puntoA) y jugador detectado
        if (movimientoCompletado && jugadorDetectado && Vector2.Distance(transform.position, puntoA.position) < 0.1f)
        {
            animator.SetBool(IsIdleAttack, true);
        }
    }

    void ActualizarAnimaciones()
    {
        // Lógica principal de animaciones
        if (jugadorDetectado && !movimientoCompletado)
        {
            // Saliendo hacia el punto A (ataque)
            animator.SetBool(IsReturning, false);
            animator.SetBool(IsIdleAttack, false);
        }
        else if (!jugadorDetectado && !movimientoCompletado)
        {
            // Volviendo al punto B (esconderse)
            animator.SetBool(IsReturning, true);
            animator.SetBool(IsIdleAttack, false);
        }
        else if (!jugadorDetectado && movimientoCompletado)
        {
            // Completamente escondido
            animator.SetBool(IsReturning, false);
            animator.SetBool(IsIdleAttack, false);
            animator.SetBool(IsAttacking, false);
        }
    }

    // Método para activar el ataque de mordida
    public void IniciarAtaque()
    {
        estaAtacando = true;
        animator.SetBool(IsAttacking, true);
        animator.SetBool(IsIdleAttack, false);
    }

    // Método para finalizar el ataque de mordida
    public void FinalizarAtaque()
    {
        estaAtacando = false;
        animator.SetBool(IsAttacking, false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        if (puntoA != null && puntoB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(puntoA.position, 0.3f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(puntoB.position, 0.3f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(puntoA.position, puntoB.position);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !estaAtacando)
        {
            IniciarAtaque();

            Rigidbody2D rbPlayer = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rbPlayer != null)
            {
                Vector2 direccion = (collision.transform.position - transform.position).normalized;
                Vector2 empuje = new Vector2(direccion.x, Mathf.Abs(direccion.y) + 0.5f);
                rbPlayer.velocity = Vector2.zero;
                rbPlayer.AddForce(empuje * knockbackForce, ForceMode2D.Impulse);
            }

            NF_PlayerHealth salud = collision.gameObject.GetComponent<NF_PlayerHealth>();
            if (salud != null)
            {
                salud.TakeDamageWithoutKnockback(dano);
            }

            Invoke("FinalizarAtaque", 1f);
        }
    }
}