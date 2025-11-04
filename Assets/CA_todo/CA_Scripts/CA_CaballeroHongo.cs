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
    public GameObject enemigoParaGenerar; // Prefab del enemigo a spawnear
    public float tiempoEntreSpawn = 3f;
    public int vidaParaEmpezarSpawn = 50; // Vida a la mitad si salud máxima es 100
    public float radioSpawn = 2f;

    // Variables privadas para spawn
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
    private string estadoAnterior = "";
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

        // Inicializar detección de daño
        InicializarDeteccionDano();
    }

    void InicializarDeteccionDano()
    {
        if (recolEnemy != null)
        {
            // Guardar salud inicial para detectar cambios
            saludAnterior = recolEnemy.GetHealth();
            inicializado = true;
            Debug.Log("🔍 Sistema de detección de daño inicializado");
        }
        else
        {
            Debug.LogWarning("⚠️ CA_RecolEnemy no encontrado, teletransporte por daño no funcionará");
        }
    }

    void Update()
    {
        if (estaMuerto) return;

        // Detectar cambios en la salud
        DetectarDanoRecibido();

        // VERIFICAR SI DEBE GENERAR ENEMIGOS (NUEVO)
        VerificarGeneracionEnemigos();

        ActualizarDeteccionJugador();
        ActualizarDireccion();
        ActualizarParametrosAnimator();
        DetectarCambioDeEstado();
        ActualizarPatrulla();
        ActualizarPersecucionYAtaque();
    }

    void VerificarGeneracionEnemigos()
    {
        if (!inicializado || recolEnemy == null || spawnActivado) return;

        float saludActual = recolEnemy.GetHealth();

        // Activar generación cuando la vida esté a la mitad o menos
        if (saludActual <= vidaParaEmpezarSpawn && !puedeGenerarEnemigos)
        {
            puedeGenerarEnemigos = true;
            spawnActivado = true;
            Debug.Log("👥 Activando generación de enemigos - Vida a la mitad");
        }

        // Generar enemigos cada 3 segundos si está activado
        if (puedeGenerarEnemigos && Time.time - tiempoUltimoSpawn > tiempoEntreSpawn)
        {
            GenerarEnemigo();
            tiempoUltimoSpawn = Time.time;
        }
    }

    void GenerarEnemigo()
    {
        if (enemigoParaGenerar == null) return;

        // Calcular posición aleatoria alrededor del enemigo
        Vector2 posicionSpawn = (Vector2)transform.position + Random.insideUnitCircle * radioSpawn;

        // Verificar que la posición sea válida (no en obstáculos)
        if (!HayObstaculoEnPosicion(posicionSpawn))
        {
            Instantiate(enemigoParaGenerar, posicionSpawn, Quaternion.identity);
            Debug.Log("👥 Enemigo generado en posición: " + posicionSpawn);

            // Efecto visual opcional
            CrearEfectoEstocada(); // Reutilizar efecto existente
        }
        else
        {
            Debug.Log("🚫 No se pudo generar enemigo - posición bloqueada");
        }
    }

    bool HayObstaculoEnPosicion(Vector2 posicion)
    {
        Collider2D colision = Physics2D.OverlapCircle(posicion, 0.5f, capasObstaculos);
        return colision != null;
    }

    void DetectarDanoRecibido()
    {
        if (!inicializado || recolEnemy == null || estaTeletransportandose) return;

        float saludActual = recolEnemy.GetHealth();

        // Si la salud disminuyó, se recibió daño
        if (saludActual < saludAnterior && saludActual > 0)
        {
            float danoRecibido = saludAnterior - saludActual;
            Debug.Log($"💥 Detectado daño recibido: {danoRecibido}");

            // Iniciar teletransporte
            IniciarTeletransportePorDaño();
        }

        // Actualizar salud anterior para el próximo frame
        saludAnterior = saludActual;
    }

    // MÉTODO PARA TELETRANSPORTE
    public void IniciarTeletransportePorDaño()
    {
        if (estaMuerto || estaTeletransportandose || !puedeTeletransportarse) return;

        Debug.Log("🔮 Iniciando teletransporte por daño");
        StartCoroutine(TeletransportarYAtacar());
    }

    private IEnumerator TeletransportarYAtacar()
    {
        estaTeletransportandose = true;
        puedeAtacar = false;

        // FASE 1: INICIAR EFECTO DASH TRAIL
        if (dashTrailEffect != null)
        {
            dashTrailEffect.StartTrail();
            Debug.Log("🌈 Dash Trail activado para teletransporte");
        }

        // Pequeña pausa dramática antes de teletransportarse
        yield return new WaitForSeconds(0.1f);

        // FASE 2: CALCULAR Y APLICAR TELETRANSPORTE
        Vector3 nuevaPosicion = CalcularPosicionTeletransporteSegura();
        transform.position = nuevaPosicion;
        Debug.Log($"🔮 Teletransportado a posición segura: {nuevaPosicion}");

        // FASE 3: ESPERAR UN FRAME PARA QUE EL PUNTO DE ATAQUE ESTÉ EN POSICIÓN
        yield return null;

        // FASE 4: FORZAR MIRADA HACIA EL JUGADOR
        if (jugador != null)
        {
            bool deberiaMirarDerecha = jugador.position.x > transform.position.x;
            if (deberiaMirarDerecha != mirandoDerecha)
            {
                Voltear();
            }
        }

        // FASE 5: DISPARAR CORTE A DISTANCIA
        if (jugador != null && proyectilCorte != null && puntoAtaque != null)
        {
            StartCoroutine(DisparoConRetraso());
        }
        else
        {
            Debug.LogWarning("❌ No se puede disparar - Jugador, proyectil o puntoAtaque nulo");
        }

        // FASE 6: MANTENER EFECTO DASH POR UN MOMENTO
        yield return new WaitForSeconds(0.2f);

        // FASE 7: DETENER EFECTO DASH TRAIL
        if (dashTrailEffect != null)
        {
            dashTrailEffect.StopTrail();
            Debug.Log("🌈 Dash Trail detenido");
        }

        // FASE 8: REACTIVAR ATAQUES NORMALES
        puedeAtacar = true;
        estaTeletransportandose = false;
    }


    private Vector3 CalcularPosicionTeletransporteSegura()
    {
        Vector3 mejorPosicion = transform.position;
        float mejorDistancia = 0f;

        // Probar varias direcciones para encontrar una posición segura
        for (int i = 0; i < 8; i++)
        {
            float angulo = i * 45f * Mathf.Deg2Rad;
            Vector3 direccion = new Vector3(Mathf.Cos(angulo), Mathf.Sin(angulo), 0);

            Vector3 posicionCandidata = transform.position + direccion * distanciaTeletransporte;

            // Asegurar que esté dentro del área de patrulla
            posicionCandidata.x = Mathf.Clamp(posicionCandidata.x, limiteIzquierdo, limiteDerecho);
            posicionCandidata.y = Mathf.Clamp(posicionCandidata.y, centroArea.y - areaPatrulla.y / 2f, centroArea.y + areaPatrulla.y / 2f);

            // Verificar si hay obstáculos en esa dirección
            if (!HayObstaculoEnDireccion(transform.position, posicionCandidata))
            {
                float distanciaAlJugador = jugador != null ? Vector3.Distance(posicionCandidata, jugador.position) : 0f;

                // Preferir posiciones más lejanas al jugador
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

        if (hit.collider != null)
        {
            Debug.Log($"🚫 Obstáculo detectado: {hit.collider.gameObject.name}");
            return true;
        }

        return false;
    }

    private void DispararCorteADistancia()
    {
        if (proyectilCorte == null || jugador == null || puntoAtaque == null) return;

        // Forzar a mirar al jugador
        bool deberiaMirarDerecha = jugador.position.x > transform.position.x;
        if (deberiaMirarDerecha != mirandoDerecha)
        {
            Voltear();
        }

        // Instanciar el prefab en el punto de ataque
        GameObject proyectil = Instantiate(proyectilCorte, puntoAtaque.position, Quaternion.identity);

        // Calcular dirección hacia el jugador
        Vector3 direccion = (jugador.position - puntoAtaque.position).normalized;

        // Aplicar movimiento inicial si el prefab tiene Rigidbody2D
        Rigidbody2D rb = proyectil.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direccion * velocidadProyectil;
        }

        // Escalar el efecto visual del proyectil si quieres
        proyectil.transform.localScale = Vector3.one;

        // Opcional: rotar hacia el jugador
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        proyectil.transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);

        Debug.Log("⚔️ Proyectil instanciado y disparado hacia el jugador");
    }


    private IEnumerator DisparoConRetraso()
    {
        // Esperar un frame
        yield return null;

        // Calcular dirección hacia el jugador
        Vector3 direccion = (jugador.position - puntoAtaque.position).normalized;

        // Instanciar proyectil
        GameObject proyectil = Instantiate(proyectilCorte, puntoAtaque.position, Quaternion.identity);

        // Configurar proyectil
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

        // Efecto visual en el punto de ataque
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

        Debug.Log("⚔️ Corte a distancia disparado desde punto de ataque");
    }


    // CLASE INTERNA PARA EL PROYECTIL
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

            // Orientar el proyectil hacia la dirección
            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);

            // Auto-destrucción después de tiempo
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
            if (other.gameObject == dueño) return; // Ignorar al dueño

            // Verificar si es un obstáculo
            if ((capasObstaculos.value & (1 << other.gameObject.layer)) != 0)
            {
                haGolpeado = true;
                Destroy(gameObject);
                return;
            }

            if (other.CompareTag("Player"))
            {
                // Aplicar daño al jugador
                NF_PlayerHealth salud = other.GetComponent<NF_PlayerHealth>();
                if (salud != null)
                {
                    salud.TakeDamage((int)dano);
                    Debug.Log($"💥 Proyectil hizo {dano} de daño al jugador");
                }

                haGolpeado = true;
                Destroy(gameObject);
            }
        }
    }

    // EL RESTO DE LOS MÉTODOS EXISTENTES SE MANTIENEN IGUAL...
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

    void DetectarCambioDeEstado()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        string estadoActual = "";

        if (stateInfo.IsName("Patrulla"))
            estadoActual = "Patrulla";
        else if (stateInfo.IsName("Persecucion"))
            estadoActual = "Persecucion";
        else if (stateInfo.IsName("Corte"))
            estadoActual = "Corte";
        else if (stateInfo.IsName("Estocada"))
            estadoActual = "Estocada";

        if (estadoActual != estadoAnterior)
        {
            switch (estadoAnterior)
            {
                case "Estocada":
                    DetenerEfectoDash();
                    break;
                case "Corte":
                    StartCoroutine(ReactivarAtaqueDespuesDeCorte());
                    break;
            }

            switch (estadoActual)
            {
                case "Corte":
                    IniciarCorte();
                    break;
                case "Estocada":
                    IniciarEstocada();
                    break;
            }

            estadoAnterior = estadoActual;
        }
    }

    IEnumerator ReactivarAtaqueDespuesDeCorte()
    {
        yield return new WaitForSeconds(tiempoEntreCortes);
        enSecuenciaAtaque = false;
    }

    void ActualizarParametrosAnimator()
    {
        if (jugador == null) return;

        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);

        anim.SetBool("JugadorEnRango", jugadorDetectado);
        anim.SetBool("JugadorEnRangoCorte", distanciaAlJugador <= rangoCorte);
        anim.SetBool("JugadorEnRangoEstocada", distanciaAlJugador <= rangoEstocada);
        anim.SetBool("PuedeAtacar", puedeAtacar);
        anim.SetInteger("ContadorCortes", contadorCortes);
        anim.SetInteger("CortesParaEstocada", cortesParaEstocada);
    }

    void ActualizarDireccion()
    {
        if (jugador == null) return;

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

    void IniciarCorte()
    {
        CrearEfectoSlash();
        AplicarDañoCorte();
        IncrementarContadorCortes();
    }

    void IniciarEstocada()
    {
        // La estocada se maneja en el StateBehaviour
    }

    public void IniciarEfectoDash()
    {
        if (dashTrailEffect != null)
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
                    salud.TakeDamage(danoCorte);

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
        Collider2D[] objetivos = Physics2D.OverlapCircleAll(transform.position, 3f);

        foreach (Collider2D objetivo in objetivos)
        {
            if (objetivo.CompareTag("Player"))
            {
                NF_PlayerHealth salud = objetivo.GetComponent<NF_PlayerHealth>();
                if (salud != null)
                {
                    salud.TakeDamage(danoEstocada);

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
        if (efectoSlash != null && puntoAtaque != null)
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
        if (efectoEstocada != null)
        {
            GameObject efecto = Instantiate(efectoEstocada, transform.position, Quaternion.identity);
            efecto.transform.localScale = Vector3.one * 1.5f;
            Destroy(efecto, 1f);
        }
    }

    public void IncrementarContadorCortes()
    {
        contadorCortes++;
    }

    public void ReiniciarContadorCortes()
    {
        contadorCortes = 0;
    }

    public void SetPuedeAtacar(bool valor)
    {
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