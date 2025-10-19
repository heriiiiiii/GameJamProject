using System.Collections;
using System.Collections.Generic;
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

    private bool jugadorDetectado = false;
    private float temporizador;
    private Transform jugador;

    void Update()
    {
        DetectarJugador();

        if (jugadorDetectado)
        {
            temporizador -= Time.deltaTime;
            if (temporizador <= 0f)
            {
                Disparar();
                temporizador = tiempoEntreDisparos;
            }
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

    void Disparar()
    {
        if (prefabEspina == null || jugador == null) return;

        Vector2 direccionBase = (jugador.position - transform.position).normalized;

        // Ángulos de dispersión
        float[] angulos = { -25f, 0f, 25f };

        foreach (float ang in angulos)
        {
            Quaternion rot = Quaternion.Euler(0, 0, ang);
            Vector2 dir = rot * direccionBase;

            GameObject bala = Instantiate(prefabEspina, puntoDisparo.position, Quaternion.identity);
            bala.GetComponent<EspinaBrote>().Inicializar(dir);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(tagJugador))
        {
            PlayerHealth salud = collision.gameObject.GetComponent<PlayerHealth>();
            if (salud != null) salud.RecibirDanio(danoContacto);

            Rigidbody2D rbPlayer = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rbPlayer != null)
            {
                Vector2 direccion = (collision.transform.position - transform.position).normalized;
                rbPlayer.AddForce(direccion * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
}
