using System.Collections;
using UnityEngine;

public class CA_Tiburon : MonoBehaviour
{
    [Header("Puntos de movimiento")]
    public Transform puntoA;
    public Transform puntoB;

    [Header("Velocidades")]
    public float velocidadNado = 5f;          // velocidad al nadar normalmente
    public float velocidadSaltitos = 3f;      // velocidad del movimiento durante los saltitos

    [Header("Saltitos")]
    public float fuerzaSalto = 2f;            // altura del salto
    public float paso = 1f;                   // distancia horizontal por salto
    public float tiempoEntreSaltos = 0.1f;    // pausa entre saltitos

    private Vector3 destinoActual;
    private bool mirandoDerecha = true;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        destinoActual = puntoB.position;
        StartCoroutine(CicloDeComportamiento());
    }

    IEnumerator CicloDeComportamiento()
    {
        while (true)
        {
            // Nada 2 rondas (ida y vuelta)
            yield return MoverDeAtoB();
            yield return MoverDeAtoB();

            // Luego 1 ronda con saltitos pequeños
            yield return SaltitosHastaDestino(puntoB.position);
            yield return SaltitosHastaDestino(puntoA.position);
        }
    }

    IEnumerator MoverDeAtoB()
    {
        while (Vector2.Distance(transform.position, destinoActual) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, destinoActual, velocidadNado * Time.deltaTime);

            // Mirar hacia el destino
            if ((destinoActual.x > transform.position.x && !mirandoDerecha) ||
                (destinoActual.x < transform.position.x && mirandoDerecha))
            {
                Girar();
            }

            yield return null;
        }

        // Cambiar destino
        destinoActual = destinoActual == puntoA.position ? puntoB.position : puntoA.position;
    }

    IEnumerator SaltitosHastaDestino(Vector3 destino)
    {
        Vector3 direccion = (destino - transform.position).normalized;

        while (Vector2.Distance(transform.position, destino) > paso)
        {
            Vector2 nuevoDestino = transform.position + direccion * paso;
            Vector2 puntoMedio = (Vector2)transform.position + (nuevoDestino - (Vector2)transform.position) / 2 + Vector2.up * fuerzaSalto;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * velocidadSaltitos; // 👈 ahora controlas esta velocidad
                transform.position = Vector2.Lerp(
                    Vector2.Lerp(transform.position, puntoMedio, t),
                    Vector2.Lerp(puntoMedio, nuevoDestino, t),
                    t
                );
                yield return null;
            }

            yield return new WaitForSeconds(tiempoEntreSaltos);
        }

        transform.position = destino;
    }

    void Girar()
    {
        mirandoDerecha = !mirandoDerecha;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }
}
