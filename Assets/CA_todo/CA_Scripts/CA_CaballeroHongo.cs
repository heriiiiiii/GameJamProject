using System.Collections;
using UnityEngine;

public class CA_HongoCaballero : MonoBehaviour
{
    [Header("Movimiento")]
    public Transform puntoA;
    public Transform puntoB;
    public float velocidad = 2f;
    private Vector3 destinoActual;
    private Rigidbody2D rb;
    private Transform jugador;
    private bool mirandoDerecha = true;

    [Header("Ataques")]
    public float rangoDeteccion = 6f;
    public float rangoCorte = 2.5f;
    public float rangoEstocada = 5f;

    public float tiempoEntreAtaques = 2f;
    public float duracionCorte = 0.3f;
    public float fuerzaEstocada = 12f;
    public float tiempoRetirada = 1f;

    [Header("Da�o")]
    public int danoCorte = 1;
    public int danoEstocada = 2;

    [Header("Efectos")]
    public GameObject slashEfecto; // efecto visual del corte
    public Transform puntoAtaque;  // donde aparecer� el efecto slash

    [Header("F�sica")]
    public float masa = 5f; // Para evitar que salga volando

    private bool atacando;
    private bool retrocediendo;
    private Vector3 direccionEstocada;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        jugador = GameObject.FindGameObjectWithTag("Player").transform;

        // Configurar f�sica para evitar empujones excesivos
        if (rb != null)
        {
            rb.mass = masa;
            rb.drag = 3f; // Mayor resistencia al movimiento
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        destinoActual = puntoB.position;
        StartCoroutine(ControlarAtaques());
    }

    void Update()
    {
        if (!atacando && !retrocediendo)
            Patrullar();

        ActualizarDireccion();
    }

    void Patrullar()
    {
        transform.position = Vector2.MoveTowards(transform.position, destinoActual, velocidad * Time.deltaTime);

        if (Vector2.Distance(transform.position, destinoActual) < 0.2f)
            destinoActual = destinoActual == puntoA.position ? puntoB.position : puntoA.position;
    }

    void ActualizarDireccion()
    {
        if (atacando || retrocediendo) return;

        // Determinar direcci�n seg�n movimiento o posici�n del jugador
        if (jugador != null && Vector2.Distance(transform.position, jugador.position) <= rangoDeteccion)
        {
            // Mirar hacia el jugador cuando est� cerca
            bool jugadorALaDerecha = jugador.position.x > transform.position.x;
            if (jugadorALaDerecha != mirandoDerecha)
                Voltear();
        }
        else
        {
            // Mirar seg�n direcci�n de patrulla
            bool moviendoseDerecha = (destinoActual == puntoB.position);
            if (moviendoseDerecha != mirandoDerecha)
                Voltear();
        }
    }

    void Voltear()
    {
        mirandoDerecha = !mirandoDerecha;
        spriteRenderer.flipX = !mirandoDerecha;

        // Ajustar punto de ataque si es necesario
        if (puntoAtaque != null)
        {
            Vector3 escalaAtaque = puntoAtaque.localScale;
            escalaAtaque.x = Mathf.Abs(escalaAtaque.x) * (mirandoDerecha ? 1 : -1);
            puntoAtaque.localScale = escalaAtaque;
        }
    }

    IEnumerator ControlarAtaques()
    {
        while (true)
        {
            yield return new WaitForSeconds(tiempoEntreAtaques);

            if (jugador != null && !atacando && !retrocediendo)
            {
                float distancia = Vector2.Distance(transform.position, jugador.position);

                if (distancia <= rangoDeteccion)
                {
                    // 80% probabilidad de corte, 20% de estocada
                    bool hacerEstocada = Random.value < 0.2f;

                    if (distancia <= rangoCorte && !hacerEstocada)
                    {
                        yield return StartCoroutine(IniciarCorte());
                    }
                    else if (distancia <= rangoEstocada && hacerEstocada)
                    {
                        yield return StartCoroutine(IniciarEstocada());
                    }
                }
            }
        }
    }

    IEnumerator IniciarCorte()
    {
        atacando = true;
        rb.velocity = Vector2.zero;

        // Animaci�n de ataque
        if (anim != null)
            anim.SetTrigger("Atacar");

        // Efecto slash visual con rotaci�n correcta
        if (slashEfecto && puntoAtaque)
        {
            GameObject efecto = Instantiate(slashEfecto, puntoAtaque.position, puntoAtaque.rotation);

            // Asegurar que el efecto mira en la direcci�n correcta
            SpriteRenderer efectoRenderer = efecto.GetComponent<SpriteRenderer>();
            if (efectoRenderer != null)
                efectoRenderer.flipX = !mirandoDerecha;
        }

        // Esperar un momento antes del da�o para sincronizar con animaci�n
        yield return new WaitForSeconds(duracionCorte * 0.3f);

        // �rea de golpe en forma de arco frente al personaje
        Vector2 puntoGolpe = (Vector2)transform.position + (mirandoDerecha ? Vector2.right : Vector2.left) * 1.2f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(puntoGolpe, 1f);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                NF_PlayerHealth vida = hit.GetComponent<NF_PlayerHealth>();
                if (vida != null)
                {
                    vida.TakeDamage(danoCorte);
                    // Aplicar peque�o empuj�n al jugador
                    AplicarEmpujonJugador(hit.transform, 3f);
                }
            }
        }

        yield return new WaitForSeconds(duracionCorte * 0.7f);
        atacando = false;
    }

    IEnumerator IniciarEstocada()
    {
        atacando = true;
        rb.velocity = Vector2.zero;

        // Animaci�n de estocada
        if (anim != null)
            anim.SetTrigger("Estocada");

        // Direcci�n hacia el jugador
        direccionEstocada = (jugador.position - transform.position).normalized;

        // Efecto visual
        if (slashEfecto && puntoAtaque)
        {
            GameObject efecto = Instantiate(slashEfecto, puntoAtaque.position, puntoAtaque.rotation);
            SpriteRenderer efectoRenderer = efecto.GetComponent<SpriteRenderer>();
            if (efectoRenderer != null)
                efectoRenderer.flipX = !mirandoDerecha;
        }

        // Movimiento r�pido hacia el jugador con f�sica controlada
        float tiempoEstocada = 0.3f;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoEstocada)
        {
            // Usar MovePosition para mejor control f�sico
            rb.MovePosition(rb.position + (Vector2)direccionEstocada * (fuerzaEstocada * 0.5f) * Time.fixedDeltaTime);

            // Da�o continuo durante la estocada
            GolpearJugador(danoEstocada);

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        // Se detiene
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.1f);

        // Retroceso
        retrocediendo = true;
        Vector3 puntoRetirada = transform.position - direccionEstocada * 2f;
        float tiempo = 0f;

        while (tiempo < tiempoRetirada)
        {
            if (rb != null)
                rb.MovePosition(Vector2.MoveTowards(transform.position, puntoRetirada, (velocidad + 1) * Time.fixedDeltaTime));

            tiempo += Time.deltaTime;
            yield return null;
        }

        retrocediendo = false;
        atacando = false;
    }

    void GolpearJugador(int dano)
    {
        Vector2 puntoGolpe = (Vector2)transform.position + (mirandoDerecha ? Vector2.right : Vector2.left) * 1.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(puntoGolpe, 1.2f);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                NF_PlayerHealth vida = hit.GetComponent<NF_PlayerHealth>();
                if (vida != null)
                {
                    vida.TakeDamage(dano);
                    AplicarEmpujonJugador(hit.transform, 5f);
                }
            }
        }
    }

    void AplicarEmpujonJugador(Transform jugadorTransform, float fuerza)
    {
        Rigidbody2D rbJugador = jugadorTransform.GetComponent<Rigidbody2D>();
        if (rbJugador != null)
        {
            Vector2 direccionEmpujon = (jugadorTransform.position - transform.position).normalized;
            rbJugador.AddForce(direccionEmpujon * fuerza, ForceMode2D.Impulse);
        }
    }

    // Para evitar empujones excesivos del player
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && (atacando || retrocediendo))
        {
            // Reducir la fuerza de colisi�n durante ataques
            if (rb != null)
                rb.velocity *= 0.3f;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoCorte);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoEstocada);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        // Mostrar �rea de ataque
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Vector2 puntoGolpe = (Vector2)transform.position + (mirandoDerecha ? Vector2.right : Vector2.left) * 1.2f;
            Gizmos.DrawWireSphere(puntoGolpe, 1f);
        }
    }
}