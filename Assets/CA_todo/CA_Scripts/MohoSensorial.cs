using System.Collections;
using UnityEngine;

public class MohoSensorial : MonoBehaviour
{
    [Header("Daño y duración")]
    public int danoPorSegundo = 1;          
    public float duracionInmovilizacion = 1.5f; 

    private bool jugadorEnContacto = false;
    private GameObject jugador;
    private Coroutine danioCoroutine;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugador = collision.gameObject;
            jugadorEnContacto = true;

            // Inmovilizar físicamente al jugador
            Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;   
                rb.gravityScale = 0f;         
            }

            // Desactivar controles del jugador
            CA_PlayerController movimiento = jugador.GetComponent<CA_PlayerController>();
            if (movimiento != null)
                movimiento.enabled = false;

            // Inicia daño periódico
            danioCoroutine = StartCoroutine(DanioConstante());

            // Reactivar movimiento después de la duración
            StartCoroutine(DesbloquearMovimiento(rb, movimiento));
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorEnContacto = false;

            // Detener daño
            if (danioCoroutine != null)
                StopCoroutine(danioCoroutine);

            // Reactivar físicas y controles
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.gravityScale = 1f;

            CA_PlayerController movimiento = collision.GetComponent<CA_PlayerController>();
            if (movimiento != null)
                movimiento.enabled = true;
        }
    }

    IEnumerator DanioConstante()
    {
        PlayerHealth salud = jugador.GetComponent<PlayerHealth>();

        while (jugadorEnContacto)
        {
            if (salud != null)
                salud.RecibirDanio(danoPorSegundo);

            yield return new WaitForSeconds(1f); 
        }
    }

    IEnumerator DesbloquearMovimiento(Rigidbody2D rb, CA_PlayerController movimiento)
    {
        yield return new WaitForSeconds(duracionInmovilizacion);

        // Reactivar físicas y controles si sigue en contacto
        if (jugadorEnContacto && rb != null && movimiento != null)
        {
            rb.gravityScale = 1f;
            movimiento.enabled = true;
        }
    }
}
