using UnityEngine;
using System.Collections;

public class EnemigoRebote : MonoBehaviour
{
    [Header("Configuración de Rebote")]
    public float fuerzaRebote = 10f;
    public int danoPorContacto = 1;
    public float tiempoDesactivado = 5f;

    [Header("Efectos Visuales")]
    public Color colorNormal = Color.yellow;
    public Color colorGolpeado = Color.red;
    public Color colorDesactivado = Color.gray;
    public float tiempoColorGolpeado = 0.3f;

    private SpriteRenderer spriteRenderer;
    private Collider2D colisionador;
    private bool estaActivo = true;
    private bool puedeDarRebote = true;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisionador = GetComponent<Collider2D>();

        if (spriteRenderer != null)
        {
            spriteRenderer.color = colorNormal;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!estaActivo) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // Solo aplicar rebote si el jugador lo golpea (desde arriba)
            bool golpeDesdeArriba = EsGolpeDesdeArriba(collision);

            if (golpeDesdeArriba && puedeDarRebote)
            {
                AplicarRebote(collision.gameObject);
                StartCoroutine(DesactivarTemporalmente());
            }
            else
            {
                AplicarDano(collision.gameObject);
            }
        }
    }

    // Este método será llamado por el sistema de ataque del jugador
    public void RecibirAtaque()
    {
        if (!estaActivo) return;

        if (puedeDarRebote)
        {
            // Aplicar rebote cuando el jugador ataca al enemigo
            AplicarRebotePorAtaque();
            StartCoroutine(DesactivarTemporalmente());
        }
    }

    bool EsGolpeDesdeArriba(Collision2D collision)
    {
        // Calcular desde qué dirección viene el golpe
        ContactPoint2D contacto = collision.contacts[0];
        Vector2 direccionGolpe = (contacto.point - (Vector2)transform.position).normalized;

        // Si el golpe viene principalmente desde arriba (dirección Y negativa)
        return direccionGolpe.y < -0.5f && Mathf.Abs(direccionGolpe.y) > Mathf.Abs(direccionGolpe.x);
    }

    void AplicarRebote(GameObject jugador)
    {
        Rigidbody2D rbPlayer = jugador.GetComponent<Rigidbody2D>();

        if (rbPlayer != null)
        {
            // Aplicar fuerza de rebote hacia arriba
            rbPlayer.velocity = new Vector2(rbPlayer.velocity.x, 0f); // Resetear velocidad Y
            rbPlayer.AddForce(Vector2.up * fuerzaRebote, ForceMode2D.Impulse);

            // Efectos visuales
            StartCoroutine(EfectoGolpe());
        }
    }

    void AplicarRebotePorAtaque()
    {
        // Buscar al jugador en la escena
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            Rigidbody2D rbPlayer = jugador.GetComponent<Rigidbody2D>();
            if (rbPlayer != null)
            {
                // Rebote más suave cuando es golpeado por ataque
                rbPlayer.velocity = new Vector2(rbPlayer.velocity.x, 0f);
                rbPlayer.AddForce(Vector2.up * (fuerzaRebote * 0.7f), ForceMode2D.Impulse);

                // Efectos visuales
                StartCoroutine(EfectoGolpe());
            }
        }
    }

    void AplicarDano(GameObject jugador)
    {
        PlayerHealth salud = jugador.GetComponent<PlayerHealth>();
        if (salud != null)
        {
            salud.RecibirDanio(danoPorContacto);

            // Aplicar pequeño empuje
            Rigidbody2D rbPlayer = jugador.GetComponent<Rigidbody2D>();
            if (rbPlayer != null)
            {
                Vector2 direccionEmpuje = (jugador.transform.position - transform.position).normalized;
                rbPlayer.AddForce(direccionEmpuje * fuerzaRebote * 0.3f, ForceMode2D.Impulse);
            }
        }
    }

    IEnumerator EfectoGolpe()
    {
        // Cambiar color temporalmente a rojo
        if (spriteRenderer != null)
        {
            spriteRenderer.color = colorGolpeado;
        }

        yield return new WaitForSeconds(tiempoColorGolpeado);

        // Si sigue activo, volver al color normal
        if (estaActivo && spriteRenderer != null)
        {
            spriteRenderer.color = colorNormal;
        }
    }

    IEnumerator DesactivarTemporalmente()
    {
        // Desactivar el enemigo
        estaActivo = false;
        puedeDarRebote = false;

        // Cambiar a color desactivado
        if (spriteRenderer != null)
        {
            spriteRenderer.color = colorDesactivado;
        }

        // Opcional: desactivar el collider para evitar cualquier interacción
        if (colisionador != null)
        {
            colisionador.enabled = false;
        }

        // Esperar el tiempo de desactivación
        yield return new WaitForSeconds(tiempoDesactivado);

        // Reactivar el enemigo
        if (colisionador != null)
        {
            colisionador.enabled = true;
        }

        estaActivo = true;
        puedeDarRebote = true;

        // Volver al color normal
        if (spriteRenderer != null)
        {
            spriteRenderer.color = colorNormal;
        }
    }

    // Método para debug visual
    void OnDrawGizmos()
    {
        if (!estaActivo) return;

        // Dibujar área de rebote
        Gizmos.color = puedeDarRebote ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>().bounds.size * 1.1f);

        // Dibujar dirección de rebote
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.5f);
    }
}