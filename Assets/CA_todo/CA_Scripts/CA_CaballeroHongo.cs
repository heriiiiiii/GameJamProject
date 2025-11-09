using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_HongoCaballero : MonoBehaviour
{
    [Header("Referencias")]
    public Transform jugador;
    public Transform puntoAtaque;

    [Header("Área de Patrulla")]
    public Vector2 areaPatrulla = new Vector2(10f, 5f);
    public Vector2 centroArea;
    public bool usarPosicionInicialComoCentro = true;

    [Header("Movimiento")]
    public float velocidadPatrulla = 2f;
    public float velocidadPersecucion = 3f;
    public float velocidadEstocada = 20f;

    [Header("Rangos de Ataque")]
    public float rangoDeteccion = 6f;
    public float rangoEstocada = 4f;
    public float rangoMinimoPersecucion = 1f;

    [Header("Ataque - Corte")]
    public int danoCorte = 1;
    public float duracionCorte = 0.2f;
    public float tiempoEntreCortes = 0.3f;
    public float escalaEfectoCorte = 3f;
    public float rangoCorte = 3f;
    public float areaDañoCorte = 3f;
    public int cortesParaEstocada = 2;

    [Header("Ataque - Corte a Distancia")]
    public GameObject proyectilCorte;
    public float velocidadProyectil = 10f;
    public float danoProyectil = 2f;

    [Header("Ataque - Estocada")]
    public int danoEstocada = 2;
    public float alturaSalto = 3f;
    public float tiempoPreparacion = 0.2f;
    public float tiempoEstocada = 0.4f;
    public float tiempoRecuperacion = 0.3f;
    public float fuerzaEmpuje = 25f;

    [Header("Sistema de Teletransporte")]
    public float distanciaTeletransporte = 5f;
    public float tiempoTeletransporte = 0.5f;
    public bool puedeTeletransportarse = true;
    public LayerMask capasObstaculos = 1 << 6; // Layer 6: Ground por defecto

    [Header("Efectos")]
    public GameObject efectoSlash;
    public GameObject efectoEstocada;
    public CA_DashTrailEffect dashTrailEffect;

    [Header("Generación de Enemigos")]
    public GameObject enemigoParaGenerar;
    public float tiempoEntreSpawn = 3f;
    public int vidaParaEmpezarSpawn = 50;
    public float radioSpawn = 2f;

    // Variables privadas
    private bool puedeGenerarEnemigos = false;
    private float tiempoUltimoSpawn = 0f;
    private bool spawnActivado = false;

    [HideInInspector] public bool mirandoDerecha = true;
    [HideInInspector] public int contadorCortes = 0;
    [HideInInspector] public bool puedeAtacar = true;
    [HideInInspector] public Vector3 destinoPatrulla;

    private Animator anim;
    public SpriteRenderer spriteRenderer;
    private Collider2D colisionador;
    private Rigidbody2D rb;
    private CA_RecolEnemy recolEnemy;
    private Vector3 posicionInicial;
    private bool moviendoDerecha = true;
    private float limiteIzquierdo;
    private float limiteDerecho;

    // Variables para control de ataques
    private bool jugadorDetectado = false;
    private float tiempoUltimoAtaque = 0f;
    private bool enSecuenciaAtaque = false;
    private float tiempoEntreAtaques = 0.5f;

    // Variables para sistema de teletransporte
    private bool estaTeletransportandose = false;
    private bool estaMuerto = false;
    private float saludAnterior;
    private bool inicializado = false;

    // Parámetros Animator
    private static readonly int IsDead = Animator.StringToHash("IsDead");
    private static readonly int JugadorEnRango = Animator.StringToHash("JugadorEnRango");
    private static readonly int JugadorEnRangoCorte = Animator.StringToHash("JugadorEnRangoCorte");
    private static readonly int JugadorEnRangoEstocada = Animator.StringToHash("JugadorEnRangoEstocada");
    private static readonly int PuedeAtacar = Animator.StringToHash("PuedeAtacar");
    private static readonly int ContadorCortes = Animator.StringToHash("ContadorCortes");
    private static readonly int CortesParaEstocada = Animator.StringToHash("CortesParaEstocada");

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisionador = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        recolEnemy = GetComponent<CA_RecolEnemy>();

        if (jugador == null)
            jugador = GameObject.FindGameObjectWithTag("Player").transform;

        posicionInicial = transform.position;
        if (usarPosicionInicialComoCentro)
        {
            centroArea = posicionInicial;
        }

        limiteIzquierdo = centroArea.x - areaPatrulla.x / 2f;
        limiteDerecho = centroArea.x + areaPatrulla.x / 2f;

        if (dashTrailEffect == null)
            dashTrailEffect = GetComponent<CA_DashTrailEffect>();

        moviendoDerecha = true;
        destinoPatrulla = new Vector3(limiteDerecho, centroArea.y, transform.position.z);

        InicializarDeteccionDano();
        ResetearAnimator();
    }

    void ResetearAnimator()
    {
        // Resetear todos los parámetros al inicio
        anim.SetBool(IsDead, false);
        anim.SetBool(JugadorEnRango, false);
        anim.SetBool(JugadorEnRangoCorte, false);
        anim.SetBool(JugadorEnRangoEstocada, false);
        anim.SetBool(PuedeAtacar, true);
        anim.SetInteger(ContadorCortes, 0);
        anim.SetInteger(CortesParaEstocada, cortesParaEstocada);

        // Resetear triggers
        anim.ResetTrigger("Cortar");
        anim.ResetTrigger("Estocar");
    }

    void InicializarDeteccionDano()
    {
        if (recolEnemy != null)
        {
            saludAnterior = recolEnemy.GetHealth();
            inicializado = true;
        }
    }

    void Update()
    {
        // VERIFICACIÓN DE MUERTE - LO PRIMERO Y MÁS IMPORTANTE
        if (!estaMuerto && recolEnemy != null && recolEnemy.EstaMuerto())
        {
            Morir();
            return;
        }

        // SI ESTÁ MUERTO, NO HACER NADA MÁS
        if (estaMuerto) return;

        // El resto del código solo se ejecuta si NO está muerto
        DetectarDanoRecibido();
        VerificarGeneracionEnemigos();
        ActualizarDeteccionJugador();
        ActualizarDireccion();
        ActualizarParametrosAnimator();
        ActualizarPatrulla();
        ActualizarPersecucionYAtaque();
    }

    void Morir()
    {
        if (estaMuerto) return;

        estaMuerto = true;
        Debug.Log("💀 Hongo Caballero MURIENDO");

        // SOLUCIÓN TEMPORAL: REVERTIR KINEMATIC INMEDIATAMENTE
        if (rb != null)
        {
            // Esperar un frame y luego revertir kinematic
            StartCoroutine(RevertirKinematic());
        }

        StopAllCoroutines();

        if (colisionador != null)
            colisionador.enabled = false;

        if (dashTrailEffect != null)
            dashTrailEffect.StopTrail();

        anim.SetBool(JugadorEnRango, false);
        anim.SetBool(JugadorEnRangoCorte, false);
        anim.SetBool(JugadorEnRangoEstocada, false);
        anim.SetBool(PuedeAtacar, false);
        anim.ResetTrigger("Cortar");
        anim.ResetTrigger("Estocar");

        anim.SetBool(IsDead, true);
        anim.Update(0f);

        StartCoroutine(DesactivarCompletamente());
    }

    IEnumerator RevertirKinematic()
    {
        yield return null; // Esperar un frame para que CA_RecolEnemy aplique kinematic

        if (rb != null)
        {
            rb.isKinematic = false; // Revertir a no kinematic
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    IEnumerator DesactivarCompletamente()
    {
        // Esperar un frame para asegurar que el Animator procesó el cambio
        yield return null;

        // Deshabilitar este script pero mantener el GameObject para la animación
        enabled = false;

        Debug.Log("✅ Script deshabilitado - Enemigo en estado de muerte permanente");
    }

    void VerificarGeneracionEnemigos()
    {
        if (!inicializado || recolEnemy == null || estaMuerto) return;

        float saludActual = recolEnemy.GetHealth();

        // Activar la generación de enemigos cuando la salud llega al umbral
        if (saludActual <= vidaParaEmpezarSpawn && !puedeGenerarEnemigos)
        {
            puedeGenerarEnemigos = true;
            spawnActivado = true;
            Debug.Log($"🔄 Generación de enemigos ACTIVADA. Salud: {saludActual}, Umbral: {vidaParaEmpezarSpawn}");
        }

        // Generar enemigos continuamente mientras puedaGenerarEnemigos sea true
        if (puedeGenerarEnemigos && Time.time - tiempoUltimoSpawn > tiempoEntreSpawn)
        {
            GenerarEnemigo();
            tiempoUltimoSpawn = Time.time;
        }
    }

    void GenerarEnemigo()
    {
        if (enemigoParaGenerar == null || estaMuerto) return;

        Vector2 posicionSpawn = (Vector2)transform.position + Random.insideUnitCircle * radioSpawn;

        if (!HayObstaculoEnPosicion(posicionSpawn))
        {
            Instantiate(enemigoParaGenerar, posicionSpawn, Quaternion.identity);
            CrearEfectoEstocada();
            Debug.Log($"🎯 Enemigo generado en posición: {posicionSpawn}");
        }
        else
        {
            // Intentar con una posición alternativa si la primera está obstruida
            for (int i = 0; i < 5; i++)
            {
                posicionSpawn = (Vector2)transform.position + Random.insideUnitCircle * radioSpawn;
                if (!HayObstaculoEnPosicion(posicionSpawn))
                {
                    Instantiate(enemigoParaGenerar, posicionSpawn, Quaternion.identity);
                    CrearEfectoEstocada();
                    Debug.Log($"🎯 Enemigo generado en posición alternativa: {posicionSpawn}");
                    break;
                }
            }
        }
    }

    bool HayObstaculoEnPosicion(Vector2 posicion)
    {
        Collider2D colision = Physics2D.OverlapCircle(posicion, 0.5f, capasObstaculos);
        return colision != null;
    }

    void DetectarDanoRecibido()
    {
        if (!inicializado || recolEnemy == null || estaTeletransportandose || estaMuerto) return;

        float saludActual = recolEnemy.GetHealth();

        if (saludActual < saludAnterior && saludActual > 0)
        {
            IniciarTeletransportePorDaño();
        }

        saludAnterior = saludActual;
    }

    public void IniciarTeletransportePorDaño()
    {
        if (estaMuerto || estaTeletransportandose || !puedeTeletransportarse) return;

        StartCoroutine(TeletransportarYAtacar());
    }

    private IEnumerator TeletransportarYAtacar()
    {
        estaTeletransportandose = true;
        puedeAtacar = false;

        if (dashTrailEffect != null)
            dashTrailEffect.StartTrail();

        yield return new WaitForSeconds(0.1f);

        Vector3 nuevaPosicion = CalcularPosicionTeletransporteSegura();
        transform.position = nuevaPosicion;

        yield return null;

        if (jugador != null)
        {
            bool deberiaMirarDerecha = jugador.position.x > transform.position.x;
            if (deberiaMirarDerecha != mirandoDerecha)
            {
                Voltear();
            }
        }

        if (jugador != null && proyectilCorte != null && puntoAtaque != null)
        {
            StartCoroutine(DisparoConRetraso());
        }

        yield return new WaitForSeconds(0.2f);

        if (dashTrailEffect != null)
            dashTrailEffect.StopTrail();

        puedeAtacar = true;
        estaTeletransportandose = false;
    }

    private Vector3 CalcularPosicionTeletransporteSegura()
    {
        Vector3 mejorPosicion = transform.position;
        float mejorDistancia = 0f;

        for (int i = 0; i < 8; i++)
        {
            float angulo = i * 45f * Mathf.Deg2Rad;
            Vector3 direccion = new Vector3(Mathf.Cos(angulo), Mathf.Sin(angulo), 0);

            Vector3 posicionCandidata = transform.position + direccion * distanciaTeletransporte;

            posicionCandidata.x = Mathf.Clamp(posicionCandidata.x, limiteIzquierdo, limiteDerecho);
            posicionCandidata.y = Mathf.Clamp(posicionCandidata.y, centroArea.y - areaPatrulla.y / 2f, centroArea.y + areaPatrulla.y / 2f);

            if (!HayObstaculoEnDireccion(transform.position, posicionCandidata))
            {
                float distanciaAlJugador = jugador != null ? Vector3.Distance(posicionCandidata, jugador.position) : 0f;

                if (distanciaAlJugador > mejorDistancia)
                {
                    mejorDistancia = distanciaAlJugador;
                    mejorPosicion = posicionCandidata;
                }
            }
        }

        return mejorPosicion;
    }

    private bool HayObstaculoEnDireccion(Vector3 desde, Vector3 hasta)
    {
        Vector3 direccion = (hasta - desde).normalized;
        float distancia = Vector3.Distance(desde, hasta);

        RaycastHit2D hit = Physics2D.Raycast(desde, direccion, distancia, capasObstaculos);
        return hit.collider != null;
    }

    private IEnumerator DisparoConRetraso()
    {
        yield return null;

        Vector3 direccion = (jugador.position - puntoAtaque.position).normalized;
        GameObject proyectil = Instantiate(proyectilCorte, puntoAtaque.position, Quaternion.identity);

        ProyectilCorte scriptProyectil = proyectil.GetComponent<ProyectilCorte>();
        if (scriptProyectil != null)
        {
            scriptProyectil.Configurar(direccion, velocidadProyectil, danoProyectil, gameObject, capasObstaculos);
        }
        else
        {
            scriptProyectil = proyectil.AddComponent<ProyectilCorte>();
            scriptProyectil.Configurar(direccion, velocidadProyectil, danoProyectil, gameObject, capasObstaculos);
        }

        if (efectoSlash != null)
        {
            GameObject slash = Instantiate(efectoSlash, puntoAtaque.position, Quaternion.identity);
            slash.transform.localScale = Vector3.one * escalaEfectoCorte;

            SpriteRenderer slashRenderer = slash.GetComponent<SpriteRenderer>();
            if (slashRenderer != null)
            {
                slashRenderer.flipX = !mirandoDerecha;
            }

            Destroy(slash, 1f);
        }
    }

    [System.Serializable]
    public class ProyectilCorte : MonoBehaviour
    {
        private Vector3 direccion;
        private float velocidad;
        private float dano;
        private GameObject dueño;
        private LayerMask capasObstaculos;
        private bool haGolpeado = false;

        public void Configurar(Vector3 dir, float vel, float dmg, GameObject owner, LayerMask obstaculos)
        {
            direccion = dir;
            velocidad = vel;
            dano = dmg;
            dueño = owner;
            capasObstaculos = obstaculos;

            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);

            Destroy(gameObject, 3f);
        }

        void Update()
        {
            if (!haGolpeado)
            {
                transform.position += direccion * velocidad * Time.deltaTime;
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (haGolpeado) return;
            if (other.gameObject == dueño) return;

            if ((capasObstaculos.value & (1 << other.gameObject.layer)) != 0)
            {
                haGolpeado = true;
                Destroy(gameObject);
                return;
            }

            if (other.CompareTag("Player"))
            {
                NF_PlayerHealth salud = other.GetComponent<NF_PlayerHealth>();
                if (salud != null)
                {
                    salud.TakeDamageWithoutKnockback((int)dano);
                }

                haGolpeado = true;
                Destroy(gameObject);
            }
        }
    }

    void ActualizarDeteccionJugador()
    {
        if (jugador == null || estaMuerto) return;

        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);

        if (distanciaAlJugador <= rangoDeteccion)
        {
            jugadorDetectado = true;
        }

        if (jugadorDetectado && distanciaAlJugador > rangoDeteccion * 2f)
        {
            jugadorDetectado = false;
        }
    }

    void ActualizarPatrulla()
    {
        if (estaMuerto || estaTeletransportandose) return;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Patrulla") && !jugadorDetectado)
        {
            MoverHaciaDestino();

            if (HaLlegadoAlDestino())
            {
                moviendoDerecha = !moviendoDerecha;

                if (moviendoDerecha)
                {
                    destinoPatrulla = new Vector3(limiteDerecho, centroArea.y, transform.position.z);
                }
                else
                {
                    destinoPatrulla = new Vector3(limiteIzquierdo, centroArea.y, transform.position.z);
                }
            }
        }
    }

    void ActualizarPersecucionYAtaque()
    {
        if (estaMuerto || estaTeletransportandose || !jugadorDetectado || !puedeAtacar) return;

        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);

        if (distanciaAlJugador > rangoMinimoPersecucion)
        {
            Vector3 direccion = (jugador.position - transform.position).normalized;
            transform.position += direccion * velocidadPersecucion * Time.deltaTime;
        }

        if (!enSecuenciaAtaque && Time.time - tiempoUltimoAtaque > tiempoEntreAtaques)
        {
            DecidirAtaque();
        }
    }

    void DecidirAtaque()
    {
        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);
        enSecuenciaAtaque = true;

        if (distanciaAlJugador <= rangoCorte && contadorCortes < cortesParaEstocada)
        {
            anim.SetTrigger("Cortar");
            tiempoUltimoAtaque = Time.time;
        }
        else if (distanciaAlJugador <= rangoEstocada && contadorCortes >= cortesParaEstocada)
        {
            anim.SetTrigger("Estocar");
            tiempoUltimoAtaque = Time.time;
        }
        else
        {
            enSecuenciaAtaque = false;
        }
    }

    void MoverHaciaDestino()
    {
        Vector3 direccion = (destinoPatrulla - transform.position).normalized;
        transform.position += direccion * velocidadPatrulla * Time.deltaTime;
    }

    bool HaLlegadoAlDestino()
    {
        return Vector2.Distance(transform.position, destinoPatrulla) < 0.2f;
    }

    void ActualizarParametrosAnimator()
    {
        if (jugador == null || estaMuerto) return;

        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);

        anim.SetBool(JugadorEnRango, jugadorDetectado);
        anim.SetBool(JugadorEnRangoCorte, distanciaAlJugador <= rangoCorte);
        anim.SetBool(JugadorEnRangoEstocada, distanciaAlJugador <= rangoEstocada);
        anim.SetBool(PuedeAtacar, puedeAtacar);
        anim.SetInteger(ContadorCortes, contadorCortes);
        anim.SetInteger(CortesParaEstocada, cortesParaEstocada);
    }

    void ActualizarDireccion()
    {
        if (jugador == null || estaMuerto) return;

        bool deberiaMirarDerecha = mirandoDerecha;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Persecucion") || stateInfo.IsName("Corte") || stateInfo.IsName("Estocada"))
        {
            deberiaMirarDerecha = jugador.position.x > transform.position.x;
        }
        else if (stateInfo.IsName("Patrulla"))
        {
            deberiaMirarDerecha = moviendoDerecha;
        }

        if (deberiaMirarDerecha != mirandoDerecha)
        {
            Voltear();
        }
    }

    public void Voltear()
    {
        if (estaMuerto) return;

        mirandoDerecha = !mirandoDerecha;

        Vector3 escala = transform.localScale;
        escala.x = Mathf.Abs(escala.x) * (mirandoDerecha ? 1 : -1);
        transform.localScale = escala;

        if (puntoAtaque != null)
        {
            Vector3 posicionPuntoAtaque = puntoAtaque.localPosition;
            posicionPuntoAtaque.x = Mathf.Abs(posicionPuntoAtaque.x) * (mirandoDerecha ? 1 : -1);
            puntoAtaque.localPosition = posicionPuntoAtaque;
        }
    }

    public void IniciarCorte()
    {
        if (estaMuerto) return;
        CrearEfectoSlash();
        AplicarDañoCorte();
        IncrementarContadorCortes();
    }

    public void IniciarEstocada()
    {
        if (estaMuerto) return;
        // La estocada se maneja en el StateBehaviour
    }

    public void IniciarEfectoDash()
    {
        if (dashTrailEffect != null && !estaMuerto)
        {
            dashTrailEffect.StartTrail();
        }
    }

    public void DetenerEfectoDash()
    {
        if (dashTrailEffect != null)
        {
            dashTrailEffect.StopTrail();
        }
    }

    public void AplicarDañoCorte()
    {
        if (estaMuerto) return;

        Vector2 puntoGolpe = (Vector2)transform.position +
                            (mirandoDerecha ? Vector2.right : Vector2.left) * 3f;

        Collider2D[] objetivos = Physics2D.OverlapCircleAll(puntoGolpe, areaDañoCorte);

        foreach (Collider2D objetivo in objetivos)
        {
            if (objetivo.CompareTag("Player"))
            {
                NF_PlayerHealth salud = objetivo.GetComponent<NF_PlayerHealth>();
                if (salud != null)
                {
                    salud.TakeDamageWithoutKnockback(danoCorte);

                    Rigidbody2D rb = objetivo.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 direccionEmpuje = (objetivo.transform.position - transform.position).normalized;
                        direccionEmpuje.y = 0.2f;
                        rb.AddForce(direccionEmpuje * (fuerzaEmpuje * 0.3f), ForceMode2D.Impulse);
                    }
                }
            }
        }
    }

    public void AplicarDañoEstocada()
    {
        if (estaMuerto) return;

        Collider2D[] objetivos = Physics2D.OverlapCircleAll(transform.position, 3f);

        foreach (Collider2D objetivo in objetivos)
        {
            if (objetivo.CompareTag("Player"))
            {
                NF_PlayerHealth salud = objetivo.GetComponent<NF_PlayerHealth>();
                if (salud != null)
                {
                    salud.TakeDamageWithoutKnockback(danoEstocada);

                    Rigidbody2D rb = objetivo.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 direccionEmpuje = (objetivo.transform.position - transform.position).normalized;
                        direccionEmpuje.y = 0.3f;
                        rb.velocity = Vector2.zero;
                        rb.AddForce(direccionEmpuje * fuerzaEmpuje, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }

    public void CrearEfectoSlash()
    {
        if (efectoSlash != null && puntoAtaque != null && !estaMuerto)
        {
            Vector3 posicionEfecto = puntoAtaque.position + (mirandoDerecha ? Vector3.right : Vector3.left) * 1.5f;
            GameObject slash = Instantiate(efectoSlash, posicionEfecto, Quaternion.identity);
            slash.transform.localScale = Vector3.one * escalaEfectoCorte;

            SpriteRenderer slashRenderer = slash.GetComponent<SpriteRenderer>();
            if (slashRenderer != null)
                slashRenderer.flipX = !mirandoDerecha;

            Destroy(slash, 1.5f);
        }
    }

    public void CrearEfectoEstocada()
    {
        if (efectoEstocada != null && !estaMuerto)
        {
            GameObject efecto = Instantiate(efectoEstocada, transform.position, Quaternion.identity);
            efecto.transform.localScale = Vector3.one * 1.5f;
            Destroy(efecto, 1f);
        }
    }

    public void IncrementarContadorCortes()
    {
        if (!estaMuerto)
            contadorCortes++;
    }

    public void ReiniciarContadorCortes()
    {
        contadorCortes = 0;
    }

    public void SetPuedeAtacar(bool valor)
    {
        if (!estaMuerto)
            puedeAtacar = valor;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 centroDibujo = usarPosicionInicialComoCentro && Application.isPlaying ? centroArea : (Vector3)centroArea;
        Gizmos.DrawWireCube(centroDibujo, new Vector3(areaPatrulla.x, areaPatrulla.y, 0));

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoCorte);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoEstocada);

        Gizmos.color = Color.green;
        Vector3 direccion = mirandoDerecha ? Vector3.right : Vector3.left;
        Gizmos.DrawRay(transform.position, direccion * 2f);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(destinoPatrulla, 0.3f);
            Gizmos.DrawLine(transform.position, destinoPatrulla);
        }
    }
}