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

        // Rotación inicial basada en la dirección
        if (direccionInicial != Vector2.zero)
        {
            RotarHaciaDireccion(direccionInicial);
        }

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

        // Rotar el proyectil para que apunte hacia la dirección de movimiento
        RotarHaciaDireccion(direccionInicial);
    }

    void RotarHaciaDireccion(Vector2 direccion)
    {
        if (direccion != Vector2.zero)
        {
            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angulo);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            NF_PlayerHealth salud = other.GetComponent<NF_PlayerHealth>();
            if (salud != null)
            {
                // 🧭 Calculamos la dirección del golpe (desde proyectil hacia el jugador)
                Vector2 hitDirection = (other.transform.position - transform.position).normalized;

                // 💥 Llamamos al daño con knockback
                salud.TakeDamageWithoutKnockback(dano);
            }

            // 💫 Destrucción visual del proyectil
            var visual = GetComponent<CA_ProyectilParticles>();
            if (visual != null)
                visual.DestruirConEfecto();
            else
                Destroy(gameObject);
        }

        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            var visual = GetComponent<CA_ProyectilParticles>();
            if (visual != null)
                visual.DestruirConEfecto();
            else
                Destroy(gameObject);
        }
    }

}