using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_HongoCaballero : MonoBehaviour
{
    [Header("Movimiento")]
    public Transform puntoA;
    public Transform puntoB;
    public float velocidad = 2f;
    public float velocidadEstocada = 8f;
    private Vector3 destinoActual;
    private Transform jugador;
    private bool mirandoDerecha = true;

    [Header("Ataques")]
    public float rangoDeteccion = 6f;
    public float rangoCorte = 2.5f;
    public float rangoEstocada = 5f;

    public float tiempoEntreAtaques = 2f;
    public float duracionCorte = 0.3f;
    public float tiempoRetirada = 1f;
    public int cortesParaEstocada = 2;

    [Header("Estocada Específico")]
    public float alturaPreparacion = 3f;
    public float tiempoPreparacion = 0.5f;
    public float fuerzaEmpujePlayer = 10f;

    [Header("Daño")]
    public int danoCorte = 1;
    public int danoEstocada = 2;

    [Header("Efectos")]
    public GameObject slashEfecto;
    public GameObject estocadaEfecto;
    public Transform puntoAtaque;

    private bool atacando;
    private bool enEstocada = false;
    private bool retrocediendo = false;
    private Vector3 direccionEstocada;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private int contadorCortes = 0;
    private bool puedeAtacar = true;
    private Collider2D colisionador;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisionador = GetComponent<Collider2D>();
        jugador = GameObject.FindGameObjectWithTag("Player").transform;

        

        destinoActual = puntoB.position;
        StartCoroutine(ControlarAtaques());
    }

    void Update()
    {
        if (!atacando && !enEstocada && !retrocediendo && puedeAtacar)
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
        if (atacando || enEstocada || retrocediendo) return;

        if (jugador != null && Vector2.Distance(transform.position, jugador.position) <= rangoDeteccion)
        {
            bool jugadorALaDerecha = jugador.position.x > transform.position.x;
            if (jugadorALaDerecha != mirandoDerecha)
                Voltear();
        }
        else
        {
            bool moviendoseDerecha = (destinoActual == puntoB.position);
            if (moviendoseDerecha != mirandoDerecha)
                Voltear();
        }
    }

    void Voltear()
    {
        mirandoDerecha = !mirandoDerecha;
        spriteRenderer.flipX = !mirandoDerecha;

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
            yield return new WaitForSeconds(0.5f);

            if (jugador != null && !atacando && !enEstocada && !retrocediendo && puedeAtacar)
            {
                float distancia = Vector2.Distance(transform.position, jugador.position);

                if (distancia <= rangoDeteccion)
                {
                    bool hacerEstocada = contadorCortes >= cortesParaEstocada;

                    if (distancia <= rangoCorte && !hacerEstocada)
                    {
                        yield return StartCoroutine(IniciarCorte());
                    }
                    else if (distancia <= rangoEstocada && hacerEstocada)
                    {
                        yield return StartCoroutine(IniciarEstocada());
                    }
                    else if (distancia <= rangoCorte && hacerEstocada)
                    {
                        yield return StartCoroutine(IniciarCorte());
                    }
                }
            }
        }
    }

    IEnumerator IniciarCorte()
    {
        atacando = true;
        puedeAtacar = false;

        if (anim != null)
            anim.SetTrigger("Atacar");

        if (slashEfecto && puntoAtaque)
        {
            GameObject efecto = Instantiate(slashEfecto, puntoAtaque.position, puntoAtaque.rotation);
            SpriteRenderer efectoRenderer = efecto.GetComponent<SpriteRenderer>();
            if (efectoRenderer != null)
                efectoRenderer.flipX = !mirandoDerecha;
            Destroy(efecto, 1f);
        }

        yield return new WaitForSeconds(duracionCorte * 0.3f);

        Vector2 puntoGolpe = (Vector2)transform.position + (mirandoDerecha ? Vector2.right : Vector2.left) * 1.2f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(puntoGolpe, 1f);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                NF_PlayerHealth vida = hit.GetComponent<NF_PlayerHealth>();
                if (vida != null)
                {
                    // Dirección desde el enemigo hacia el player
                    Vector2 hitDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                    vida.TakeDamage(danoCorte, hitDir);

                    // Empuje pequeño en el corte
                    AplicarEmpujeAlPlayer(hit.transform, 5f);
                }
            }
        }

        yield return new WaitForSeconds(duracionCorte * 0.7f);

        contadorCortes++;
        atacando = false;

        yield return new WaitForSeconds(0.5f);
        puedeAtacar = true;
    }

    IEnumerator IniciarEstocada()
    {
        atacando = true;
        enEstocada = true;
        puedeAtacar = false;

        // FASE 1: PREPARACIÓN - Saltar hacia arriba
        if (anim != null)
            anim.SetTrigger("PrepararEstocada");

        Vector3 posicionInicial = transform.position;
        Vector3 posicionPreparacion = posicionInicial + Vector3.up * alturaPreparacion;

        // Efecto visual de preparación
        if (estocadaEfecto)
        {
            GameObject efectoPrep = Instantiate(estocadaEfecto, transform.position, Quaternion.identity);
            efectoPrep.transform.localScale = Vector3.one * 1.5f;
            Destroy(efectoPrep, tiempoPreparacion);
        }

        // Movimiento hacia arriba
        float tiempoPrep = 0f;
        while (tiempoPrep < tiempoPreparacion)
        {
            transform.position = Vector3.MoveTowards(transform.position, posicionPreparacion,
                (alturaPreparacion / tiempoPreparacion) * Time.deltaTime);
            tiempoPrep += Time.deltaTime;
            yield return null;
        }

        // FASE 2: ESTOCADA - Movimiento rápido hacia el jugador
        if (anim != null)
            anim.SetTrigger("Estocada");

        // Calcular dirección hacia el jugador
        direccionEstocada = (jugador.position - transform.position).normalized;

        // Efecto visual durante la estocada
        if (slashEfecto && puntoAtaque)
        {
            GameObject efecto = Instantiate(slashEfecto, puntoAtaque.position, puntoAtaque.rotation);
            SpriteRenderer efectoRenderer = efecto.GetComponent<SpriteRenderer>();
            if (efectoRenderer != null)
                efectoRenderer.flipX = !mirandoDerecha;
            efecto.transform.localScale *= 2f;
            Destroy(efecto, 0.8f);
        }

        // Movimiento de estocada (sin física)
        float distanciaEstocada = 4f;
        Vector3 posicionObjetivo = transform.position + (direccionEstocada * distanciaEstocada);
        float tiempoEstocada = 0.5f;
        float tiempoTranscurrido = 0f;

        // Lista para evitar empujar múltiples veces al mismo frame
        HashSet<Collider2D> playersEmpujados = new HashSet<Collider2D>();

        while (tiempoTranscurrido < tiempoEstocada)
        {
            transform.position = Vector3.MoveTowards(transform.position, posicionObjetivo,
                velocidadEstocada * Time.deltaTime);

            // Empujar players durante la estocada (solo una vez por colisión)
            Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, 2f);
            foreach (Collider2D player in players)
            {
                if (player.CompareTag("Player") && !playersEmpujados.Contains(player))
                {
                    AplicarEmpujeAlPlayer(player.transform, fuerzaEmpujePlayer);
                    playersEmpujados.Add(player);

                    // Aplicar daño con dirección desde el enemigo hacia el player
                    NF_PlayerHealth vida = player.GetComponent<NF_PlayerHealth>();
                    if (vida != null)
                    {
                        Vector2 hitDir = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
                        vida.TakeDamage(danoEstocada, hitDir);
                    }
                }
            }

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        // FASE 3: CAER AL SUELO
        Vector3 posicionSuelo = new Vector3(transform.position.x, posicionInicial.y, transform.position.z);
        float tiempoCaida = 0.3f;
        float tiempoCaidaTranscurrido = 0f;

        while (tiempoCaidaTranscurrido < tiempoCaida)
        {
            transform.position = Vector3.MoveTowards(transform.position, posicionSuelo,
                (alturaPreparacion / tiempoCaida) * Time.deltaTime);
            tiempoCaidaTranscurrido += Time.deltaTime;
            yield return null;
        }

        // Asegurar posición exacta en el suelo
        transform.position = posicionSuelo;

        // FASE 4: RETROCESO
        retrocediendo = true;
        Vector3 puntoRetirada = transform.position - (direccionEstocada * 2f);
        float tiempoRetiradaTranscurrido = 0f;

        while (tiempoRetiradaTranscurrido < tiempoRetirada)
        {
            transform.position = Vector3.MoveTowards(transform.position, puntoRetirada,
                (velocidad + 2f) * Time.deltaTime);
            tiempoRetiradaTranscurrido += Time.deltaTime;
            yield return null;
        }

        // LIMPIAR ESTADOS
        retrocediendo = false;
        enEstocada = false;
        atacando = false;
        contadorCortes = 0;

        // Permitir nuevos ataques
        yield return new WaitForSeconds(1f);
        puedeAtacar = true;
    }

    void AplicarEmpujeAlPlayer(Transform playerTransform, float fuerza)
    {
        Rigidbody2D rbPlayer = playerTransform.GetComponent<Rigidbody2D>();
        if (rbPlayer != null)
        {
            // Calcular dirección del empuje (desde el enemigo hacia el player)
            Vector2 direccionEmpuje = (playerTransform.position - transform.position).normalized;

            // Asegurar que el empuje sea principalmente horizontal
            direccionEmpuje.y = Mathf.Clamp(direccionEmpuje.y, -0.2f, 0.3f); // Un poco hacia arriba
            direccionEmpuje.Normalize();

            // Aplicar el empuje
            rbPlayer.velocity = Vector2.zero; // Resetear velocidad primero
            rbPlayer.AddForce(direccionEmpuje * fuerza, ForceMode2D.Impulse);

            Debug.Log($"Empujando player con fuerza: {fuerza}, dirección: {direccionEmpuje}");
        }
    }

    void GolpearJugador(int dano)
    {
        Vector2 puntoGolpe = (Vector2)transform.position + (mirandoDerecha ? Vector2.right : Vector2.left) * 1.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(puntoGolpe, 1.5f);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                NF_PlayerHealth vida = hit.GetComponent<NF_PlayerHealth>();
                if (vida != null)
                {
                    Vector2 hitDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                    vida.TakeDamage(dano, hitDir);
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Si colisiona con paredes durante la estocada, detenerse
        if (enEstocada && other.CompareTag("Ground"))
        {
            StopAllCoroutines();
            StartCoroutine(RecuperarDeColision());
        }

        // Empujar al player si colisiona durante la estocada
        if (enEstocada && other.CompareTag("Player"))
        {
            AplicarEmpujeAlPlayer(other.transform, fuerzaEmpujePlayer * 1.2f); // Empuje extra por colisión directa
        }
    }

    IEnumerator RecuperarDeColision()
    {
        // Caer al suelo rápidamente
        Vector3 posicionSuelo = new Vector3(transform.position.x, FindSueloPosition(), transform.position.z);

        float tiempoCaida = 0.2f;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoCaida)
        {
            transform.position = Vector3.MoveTowards(transform.position, posicionSuelo,
                10f * Time.deltaTime);
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        transform.position = posicionSuelo;

        // Limpiar estados
        retrocediendo = false;
        enEstocada = false;
        atacando = false;
        contadorCortes = 0;

        yield return new WaitForSeconds(1f);
        puedeAtacar = true;
    }

    float FindSueloPosition()
    {
        // Buscar la posición del suelo debajo del enemigo
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10f, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
        {
            return hit.point.y + colisionador.bounds.extents.y;
        }
        return transform.position.y;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoCorte);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoEstocada);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        // Mostrar área de empuje durante la estocada
        if (Application.isPlaying && enEstocada)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
    }
}