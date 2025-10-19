using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RodanteFungico : MonoBehaviour
{
    [Header("Puntos de movimiento")]
    public Transform puntoA; 
    public Transform puntoB; 

    [Header("Parámetros")]
    public float velocidad = 2f;
    public float knockbackForce = 15f;
    public int dano = 1;

    [Header("Detección del jugador")]
    public float rangoDeteccion = 5f;

    private Rigidbody2D rb;
    private bool jugadorDetectado = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.freezeRotation = true;

        // Inicia escondido
        transform.position = puntoB.position;
    }

    void Update()
    {
        DetectarJugadorPorTag();
        Mover();
    }

    void DetectarJugadorPorTag()
    {
        jugadorDetectado = false; // Reiniciamos detección cada frame

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
        Vector3 destino = jugadorDetectado ? puntoA.position : puntoB.position;

        transform.position = Vector2.MoveTowards(transform.position, destino, velocidad * Time.deltaTime);

        // Volteo del sprite según la dirección del movimiento
        Vector3 escala = transform.localScale;
        escala.x = destino.x < transform.position.x ? -Mathf.Abs(escala.x) : Mathf.Abs(escala.x);
        transform.localScale = escala;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rbPlayer = collision.gameObject.GetComponent<Rigidbody2D>();

            if (rbPlayer != null)
            {
                // Empuje en dirección contraria y un poco hacia arriba
                Vector2 direccion = (collision.transform.position - transform.position).normalized;
                Vector2 empuje = new Vector2(direccion.x, Mathf.Abs(direccion.y) + 0.5f);
                rbPlayer.velocity = Vector2.zero;
                rbPlayer.AddForce(empuje * knockbackForce, ForceMode2D.Impulse);
            }

            PlayerHealth salud = collision.gameObject.GetComponent<PlayerHealth>();
            if (salud != null)
            {
                salud.RecibirDanio(dano);
            }
        }
    }
}
