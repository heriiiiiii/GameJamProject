using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_ReboteEntreEnemigos : MonoBehaviour
{
    [Header("Rebote entre enemigos")]
    public float fuerzaRebote = 5f;

    [Header("Giro hacia el jugador")]
    public string tagJugador = "Player";
    public float velocidadRotacion = 10f;

    private Rigidbody2D rb;
    private Transform jugador;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject objJugador = GameObject.FindGameObjectWithTag(tagJugador);
        if (objJugador != null)
        {
            jugador = objJugador.transform;
        }
    }

    void Update()
    {
        // Si existe el jugador, el enemigo lo mira
        if (jugador != null)
        {
            // Calcular dirección al jugador
            Vector3 direccion = jugador.position - transform.position;

            // Si el jugador está a la derecha
            if (direccion.x > 0 && transform.localScale.x < 0)
            {
                Voltear();
            }
            // Si el jugador está a la izquierda
            else if (direccion.x < 0 && transform.localScale.x > 0)
            {
                Voltear();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Calcular dirección opuesta al otro enemigo
            Vector2 direccionRebote = (transform.position - collision.transform.position).normalized;

            // Aplicar impulso
            rb.AddForce(direccionRebote * fuerzaRebote, ForceMode2D.Impulse);
        }
    }

    void Voltear()
    {
        Vector3 escala = transform.localScale;
        escala.x *= -1; // Invertir la escala en X
        transform.localScale = escala;
    }
}
