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
    public float velocidadPatrulla = 1.5f;
    public Vector2 areaVuelo = new Vector2(5f, 3f);
    private Vector3 posicionInicial;
    private Vector3 objetivoPatrulla;
    private bool siguiendo;

    [Header("Animación")]
    private Animator animator;
    private CA_RecolEnemy recolEnemy;
    private bool estaMuerto = false;

    [Header("Flip")]
    private bool mirandoDerecha = true;
    private Vector3 escalaOriginal;

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        posicionInicial = transform.position;
        animator = GetComponent<Animator>();
        recolEnemy = GetComponent<CA_RecolEnemy>();

        // Guardar la escala original para mantener las proporciones
        escalaOriginal = transform.localScale;

        // Generar primer objetivo de patrulla
        GenerarNuevoObjetivoPatrulla();

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
        ActualizarFlip();
        ActualizarAnimaciones();
    }

    void Mover()
    {
        if (jugador == null || estaMuerto) return;

        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);

        if (distanciaAlJugador < rangoDeteccion)
        {
            // Seguir al jugador
            siguiendo = true;
            transform.position = Vector2.MoveTowards(transform.position, jugador.position, velocidadMovimiento * Time.deltaTime);
        }
        else
        {
            // Patrullar por el área definida
            siguiendo = false;
            Patrullar();
        }
    }

    void Patrullar()
    {
        // Moverse hacia el objetivo de patrulla
        transform.position = Vector2.MoveTowards(transform.position, objetivoPatrulla, velocidadPatrulla * Time.deltaTime);

        // Si llegó al objetivo, generar uno nuevo
        if (Vector2.Distance(transform.position, objetivoPatrulla) < 0.1f)
        {
            GenerarNuevoObjetivoPatrulla();
        }
    }

    void GenerarNuevoObjetivoPatrulla()
    {
        // Generar una posición aleatoria dentro del área de vuelo
        float randomX = Random.Range(-areaVuelo.x / 2f, areaVuelo.x / 2f);
        float randomY = Random.Range(-areaVuelo.y / 2f, areaVuelo.y / 2f);

        objetivoPatrulla = posicionInicial + new Vector3(randomX, randomY, 0f);
    }

    void ActualizarFlip()
    {
        if (estaMuerto) return;

        Vector3 objetivo = siguiendo ? jugador.position : objetivoPatrulla;

        // Determinar dirección basada en la posición X del objetivo
        bool deberiaMirarDerecha = objetivo.x > transform.position.x;

        // Aplicar flip solo si la dirección cambió
        if (deberiaMirarDerecha != mirandoDerecha)
        {
            Flip(deberiaMirarDerecha);
        }
    }

    void Flip(bool mirarDerecha)
    {
        mirandoDerecha = mirarDerecha;

        Vector3 nuevaEscala = escalaOriginal;

        // INVERTIR LA LÓGICA SI ESTÁ AL REVÉS
        if (!mirarDerecha) // Cambié mirarDerecha por !mirarDerecha
        {
            nuevaEscala.x = Mathf.Abs(escalaOriginal.x);
        }
        else
        {
            nuevaEscala.x = -Mathf.Abs(escalaOriginal.x);
        }

        transform.localScale = nuevaEscala;
    }

    void ActualizarAnimaciones()
    {
        if (animator == null || estaMuerto) return;

        animator.SetBool("IsFlying", true);
        // La animación de ataque se controla en la corrutina de ráfagas
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
        // Dibujar rango de detección
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        // Dibujar área de vuelo
        Gizmos.color = Color.blue;
        Vector3 drawPosition = Application.isPlaying ? posicionInicial : transform.position;
        Gizmos.DrawWireCube(drawPosition, new Vector3(areaVuelo.x, areaVuelo.y, 0f));

        // Dibujar objetivo de patrulla actual (solo en play mode)
        if (Application.isPlaying && !siguiendo)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(objetivoPatrulla, 0.2f);
            Gizmos.DrawLine(transform.position, objetivoPatrulla);
        }
    }
}