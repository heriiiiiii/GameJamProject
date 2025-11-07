using System.Collections;
using UnityEngine;

public class CA_HongoVolador : MonoBehaviour
{
    [Header("Disparo")]
    public GameObject balaPrefab;
    public Transform[] puntosDisparo;
    public float tiempoEntreRafagas = 3f;
    public float tiempoEntreBalas = 0.2f;

    [Header("Detección del Jugador")]
    public float rangoDeteccion = 10f;
    private Transform jugador;

    [Header("Movimiento")]
    public float velocidadMovimiento = 2f;
    private Vector3 posicionInicial;
    private bool siguiendo;

    [Header("Animación")]
    private Animator animator;
    private CA_RecolEnemy recolEnemy;
    private bool estaMuerto = false;

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        posicionInicial = transform.position;
        animator = GetComponent<Animator>();
        recolEnemy = GetComponent<CA_RecolEnemy>();

        // INICIALIZAR PARÁMETROS DEL ANIMATOR
        if (animator != null)
        {
            animator.SetBool("IsFlying", true);
            animator.SetBool("IsAttacking", false);
            animator.SetBool("IsDead", false);
        }

        StartCoroutine(Rafagas());
    }

    void Update()
    {
        if (estaMuerto) return;

        // Verificar si murió
        if (recolEnemy != null && recolEnemy.EstaMuerto() && !estaMuerto)
        {
            Morir();
            return;
        }

        Mover();
        ActualizarAnimaciones();
    }

    void Mover()
    {
        if (jugador == null || estaMuerto) return;

        float distancia = Vector2.Distance(transform.position, jugador.position);
        if (distancia < rangoDeteccion)
        {
            siguiendo = true;
            transform.position = Vector2.MoveTowards(transform.position, jugador.position, velocidadMovimiento * Time.deltaTime);
        }
        else
        {
            siguiendo = false;
            transform.position = Vector2.MoveTowards(transform.position, posicionInicial, velocidadMovimiento * Time.deltaTime);
        }
    }

    void ActualizarAnimaciones()
    {
        if (animator == null || estaMuerto) return;

        // Siempre volando mientras está vivo
        animator.SetBool("IsFlying", true);
    }

    IEnumerator Rafagas()
    {
        while (true)
        {
            if (estaMuerto) yield break;

            if (jugador != null && !estaMuerto && Vector2.Distance(transform.position, jugador.position) < rangoDeteccion)
            {
                // Activar animación de ataque
                if (animator != null)
                {
                    animator.SetBool("IsAttacking", true);
                }

                // 3 disparos seguidos por ráfaga
                for (int i = 0; i < 3; i++)
                {
                    if (estaMuerto) yield break;
                    DispararDesdePuntos();
                    yield return new WaitForSeconds(tiempoEntreBalas);
                }

                // Desactivar animación de ataque
                if (animator != null)
                {
                    animator.SetBool("IsAttacking", false);
                }
            }
            yield return new WaitForSeconds(tiempoEntreRafagas);
        }
    }

    void DispararDesdePuntos()
    {
        if (estaMuerto) return;

        foreach (Transform punto in puntosDisparo)
        {
            GameObject nuevaBala = Instantiate(balaPrefab, punto.position, Quaternion.identity);
            Vector2 direccion = (jugador.position - punto.position).normalized;
            // Asegúrate de que CA_BalaVolador tenga el método SetDireccion
            CA_BalaVolador balaScript = nuevaBala.GetComponent<CA_BalaVolador>();
            if (balaScript != null)
            {
                balaScript.SetDireccion(direccion);
            }
        }
    }

    void Morir()
    {
        estaMuerto = true;

        if (animator != null)
        {
            animator.SetBool("IsDead", true);
            animator.SetBool("IsAttacking", false);
            animator.SetBool("IsFlying", false);
        }

        // Detener todas las corrutinas
        StopAllCoroutines();

        // Desactivar movimiento
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }
}