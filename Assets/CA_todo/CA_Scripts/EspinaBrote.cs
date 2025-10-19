using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EspinaBrote : MonoBehaviour
{
    [Header("Parámetros")]
    public float velocidad = 3f;
    public int dano = 1;
    public float duracion = 3f;
    public float fuerzaGiro = 2f;
    public float tiempoAntesDeSeguir = 0.5f; 

    private Transform jugador;
    private Rigidbody2D rb;
    private Vector2 direccionInicial;
    private bool persiguiendo = false;

    public void Inicializar(Vector2 direccion)
    {
        direccionInicial = direccion.normalized;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jugador = GameObject.FindGameObjectWithTag("Player")?.transform;
        Destroy(gameObject, duracion);
        StartCoroutine(ActivarPersecucion());
    }

    IEnumerator ActivarPersecucion()
    {
        // Se mueve recto un corto tiempo antes de perseguir
        rb.velocity = direccionInicial * velocidad;
        yield return new WaitForSeconds(tiempoAntesDeSeguir);
        persiguiendo = true;
    }

    void Update()
    {
        if (!persiguiendo || jugador == null) return;

        // Ajusta suavemente la dirección hacia el jugador
        Vector2 direccionObjetivo = (jugador.position - transform.position).normalized;
        direccionInicial = Vector2.Lerp(direccionInicial, direccionObjetivo, fuerzaGiro * Time.deltaTime);

        rb.velocity = direccionInicial * velocidad;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth salud = other.GetComponent<PlayerHealth>();
            if (salud != null) salud.RecibirDanio(dano);
            Destroy(gameObject);
        }

        if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
