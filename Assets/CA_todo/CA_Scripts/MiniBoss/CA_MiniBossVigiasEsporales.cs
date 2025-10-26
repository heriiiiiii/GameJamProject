using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_MiniBossVigiasEsporales : MonoBehaviour
{
    [System.Serializable]
    public class HongoData
    {
        public GameObject hongo;
        public float vida;
        public float vidaMaxima = 100f;
        public bool estaVivo = true;
        public int tipoAtaque;
    }

    [Header("Rotación Visual")]
    public bool usarRotacionVisual = true;
    public float velocidadRotacion = 5f;
    public float anguloMaximoInclinacion = 15f;


    private Vector3[] direccionesMovimiento = new Vector3[3];
    private Quaternion[] rotacionesObjetivo = new Quaternion[3];

    [Header("Configuración Hongos")]
    public HongoData[] hongos = new HongoData[3];
    public float velocidadAtaqueNormal = 2f;
    public float velocidadAtaqueFusion = 1f;

    [Header("Estados")]
    public bool estaDespierto = false;
    public bool enMovimientoEvasivo = false;
    public int hongosVivos = 3;

    [Header("Movimiento Evasivo Zig-Zag")]
    public float rangoMovimientoX = 3f;
    public float rangoMovimientoY = 2f;
    public float velocidadMovimiento = 3f;
    public float frecuenciaCambioDireccion = 2f;
    public float duracionMovimientoEvasivo = 6f;

    [Header("Ataques")]
    public GameObject proyectilEspora;
    public Material materialHilo;
    public float tiempoEntreAtaques = 2f;
    private int ataqueActual = 0;
    private bool ataqueEnCurso = false;

    [Header("Ataque de Balas Giratorias")]
    public float radioHilos = 3f;
    public int numeroHilos = 6;
    public float velocidadRotacionHilos = 90f;
    public float velocidadHilosRectos = 8f;
    public float anchoHilo = 0.1f;
    public Color colorHilo = new Color(1f, 1f, 1f, 0.8f);
    public LayerMask layerSuelo;

    [Header("Estocada")]
    public float velocidadEstocada = 15f;
    public float duracionEstocada = 1f;
    public int danioEstocada = 1;
    public Transform[] puntosRetirada;

    [Header("Disparos Lentos")]
    public float velocidadProyectilLento = 3f;
    public float tiempoEntreDisparosLentos = 1.5f;

    [Header("Daño por Contacto")]
    public int danioContacto = 1;
    public float tiempoEntreDanioContacto = 1f;

    // VARIABLES PARA DAÑO Y VELOCIDAD ESCALABLES
    private float multiplicadorDanio = 2f;
    private float multiplicadorVelocidad = 2f;
    private int danioBaseEstocada = 1;
    private int danioBaseProyectil = 1;
    private int danioBaseHilos = 1;
    private int danioBaseContacto = 1;
    private float velocidadBaseMovimiento = 3f;
    private float velocidadBaseEstocada = 15f;
    private float velocidadBaseHilos = 8f;

    private float tiempoUltimoAtaque = 0f;
    private Vector3[] posicionesOriginales = new Vector3[3];
    private List<GameObject> hilosActivos = new List<GameObject>();
    private Transform jugador;
    private bool movimientoEvasivoActivado = false;
    private Vector3[] direccionesActuales = new Vector3[3];
    private float[] tiemposCambioDireccion = new float[3];
    private Coroutine movimientoEvasivoCoroutine;
    private Coroutine disparosLentosCoroutine;
    private float[] ultimoDanioContacto = new float[3];

    void Start()
    {
        danioBaseEstocada = danioEstocada;
        danioBaseContacto = danioContacto;
        velocidadBaseMovimiento = velocidadMovimiento;
        velocidadBaseEstocada = velocidadEstocada;
        velocidadBaseHilos = velocidadHilosRectos;

        InicializarHongos();
        GuardarPosicionesOriginales();
        InicializarSistemaRotacion(); // ← NUEVO
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) jugador = playerObj.transform;

        for (int i = 0; i < direccionesActuales.Length; i++)
        {
            direccionesActuales[i] = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
            tiemposCambioDireccion[i] = 0f;
            ultimoDanioContacto[i] = 0f;
        }
    }

    // NUEVO MÉTODO: Inicializar sistema de rotación
    void InicializarSistemaRotacion()
    {
        for (int i = 0; i < hongos.Length; i++)
        {
            direccionesMovimiento[i] = Vector3.right; // Dirección inicial
            rotacionesObjetivo[i] = hongos[i].hongo.transform.rotation;
        }
    }

    void InicializarHongos()
    {
        hongos[0].tipoAtaque = 0;
        hongos[1].tipoAtaque = 0;
        hongos[2].tipoAtaque = 1;

        for (int i = 0; i < hongos.Length; i++)
        {
            hongos[i].vida = hongos[i].vidaMaxima;
            hongos[i].estaVivo = true;

            CA_RecolEnemy recolEnemy = hongos[i].hongo.GetComponent<CA_RecolEnemy>();
            if (recolEnemy == null)
            {
                recolEnemy = hongos[i].hongo.AddComponent<CA_RecolEnemy>();
            }

            CircleCollider2D collider = hongos[i].hongo.GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = hongos[i].hongo.AddComponent<CircleCollider2D>();
                collider.isTrigger = true;
                collider.radius = 0.8f;
            }

            SpriteRenderer sr = hongos[i].hongo.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = i == 2 ? Color.red : Color.green;
            }
        }
    }

    void GuardarPosicionesOriginales()
    {
        for (int i = 0; i < hongos.Length; i++)
        {
            posicionesOriginales[i] = hongos[i].hongo.transform.position;
        }
    }

    void Update()
    {
        if (!estaDespierto) return;

        if (!movimientoEvasivoActivado && hongosVivos == 3)
        {
            VerificarMovimientoEvasivoPorVida();
        }

        if (enMovimientoEvasivo)
        {
            ActualizarMovimientoEvasivo();
        }

        ActualizarDanioContacto();

        // NUEVO: Actualizar rotaciones visuales
        if (usarRotacionVisual)
        {
            ActualizarRotacionesVisuales();
        }

        if (ataqueEnCurso || enMovimientoEvasivo) return;

        if (Time.time - tiempoUltimoAtaque > tiempoEntreAtaques)
        {
            SeleccionarYRealizarAtaque();
            tiempoUltimoAtaque = Time.time;
        }
    }

    // NUEVO: Actualizar rotaciones de todos los hongos
    void ActualizarRotacionesVisuales()
    {
        for (int i = 0; i < hongos.Length; i++)
        {
            if (hongos[i].estaVivo && hongos[i].hongo != null)
            {
                ActualizarRotacionHongo(i);
            }
        }
    }

    // NUEVO: Actualizar rotación individual de cada hongo
    void ActualizarRotacionHongo(int indiceHongo)
    {
        Transform hongoTransform = hongos[indiceHongo].hongo.transform;

        // Calcular dirección del movimiento
        Vector3 direccionActual = CalcularDireccionMovimiento(indiceHongo);

        if (direccionActual != Vector3.zero)
        {
            // Calcular rotación objetivo basada en la dirección
            Quaternion rotacionObjetivo = CalcularRotacionDesdeDireccion(direccionActual);

            // Suavizar la rotación
            hongoTransform.rotation = Quaternion.Lerp(
                hongoTransform.rotation,
                rotacionObjetivo,
                velocidadRotacion * Time.deltaTime
            );
        }
    }

    // NUEVO: Calcular dirección actual del movimiento
    Vector3 CalcularDireccionMovimiento(int indiceHongo)
    {
        Vector3 direccion = Vector3.zero;

        if (enMovimientoEvasivo)
        {
            // Durante movimiento evasivo, usar la dirección actual
            direccion = direccionesActuales[indiceHongo];
        }
        else if (ataqueEnCurso)
        {
            // Durante ataques, calcular dirección basada en el movimiento
            direccion = CalcularDireccionDuranteAtaque(indiceHongo);
        }
        else
        {
            // Durante idle, dirección hacia el jugador o movimiento suave
            direccion = CalcularDireccionIdle(indiceHongo);
        }

        direccionesMovimiento[indiceHongo] = direccion;
        return direccion;
    }

    // NUEVO: Calcular dirección durante ataques específicos
    Vector3 CalcularDireccionDuranteAtaque(int indiceHongo)
    {
        Transform hongoTransform = hongos[indiceHongo].hongo.transform;

        switch (ataqueActual)
        {
            case 2: // Estocada
                if (jugador != null)
                {
                    return (jugador.position - hongoTransform.position).normalized;
                }
                break;

            case 1: // Balas Giratorias
                    // Movimiento aleatorio durante este ataque
                return direccionesActuales[indiceHongo];

            default:
                return direccionesMovimiento[indiceHongo];
        }

        return direccionesMovimiento[indiceHongo];
    }

    // NUEVO: Calcular dirección durante estado idle
    Vector3 CalcularDireccionIdle(int indiceHongo)
    {
        if (jugador != null)
        {
            Transform hongoTransform = hongos[indiceHongo].hongo.transform;
            Vector3 direccionJugador = (jugador.position - hongoTransform.position).normalized;

            // Suavizar el cambio de dirección
            return Vector3.Lerp(direccionesMovimiento[indiceHongo], direccionJugador, 0.1f);
        }

        return direccionesMovimiento[indiceHongo];
    }

    // NUEVO: Calcular rotación desde dirección (2D)
    Quaternion CalcularRotacionDesdeDireccion(Vector3 direccion)
    {
        if (direccion == Vector3.zero)
            return Quaternion.identity;

        // Calcular ángulo en grados (para 2D)
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        // Aplicar inclinación basada en la velocidad vertical
        float inclinacion = -direccion.y * anguloMaximoInclinacion;

        return Quaternion.Euler(0f, 0f, angulo + inclinacion);
    }

    void ActualizarDanioContacto()
    {
        for (int i = 0; i < hongos.Length; i++)
        {
            if (hongos[i].estaVivo && hongos[i].hongo != null && jugador != null)
            {
                float distancia = Vector3.Distance(hongos[i].hongo.transform.position, jugador.position);
                if (distancia < 1.2f && Time.time - ultimoDanioContacto[i] > tiempoEntreDanioContacto)
                {
                    AplicarDanioContacto(i);
                }
            }
        }
    }

    void AplicarDanioContacto(int indiceHongo)
    {
        if (jugador == null) return;

        PlayerHealth playerHealth = jugador.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            int danioActual = Mathf.RoundToInt(danioBaseContacto * multiplicadorDanio);
            playerHealth.RecibirDanio(danioActual);
            ultimoDanioContacto[indiceHongo] = Time.time;
            Debug.Log($"Daño por contacto: {danioActual} (Hongo {indiceHongo})");
        }
    }

    void VerificarMovimientoEvasivoPorVida()
    {
        foreach (HongoData hongo in hongos)
        {
            if (hongo.estaVivo && hongo.vida <= hongo.vidaMaxima / 2f)
            {
                movimientoEvasivoActivado = true;
                StartCoroutine(ActivarMovimientoEvasivoPorVida());
                break;
            }
        }
    }

    IEnumerator ActivarMovimientoEvasivoPorVida()
    {
        Debug.Log("¡Hongo a mitad de vida! Activando movimiento evasivo con disparos lentos");
        yield return StartCoroutine(IniciarMovimientoEvasivo());
        movimientoEvasivoActivado = false;
    }

    // --- SISTEMA DE MOVIMIENTO EVASIVO ZIG-ZAG ---
    IEnumerator IniciarMovimientoEvasivo()
    {
        enMovimientoEvasivo = true;
        Debug.Log("Iniciando movimiento evasivo zig-zag");

        movimientoEvasivoCoroutine = StartCoroutine(MovimientoEvasivoCoroutine());
        disparosLentosCoroutine = StartCoroutine(DisparosLentosDuranteMovimiento());

        yield return new WaitForSeconds(duracionMovimientoEvasivo);

        if (movimientoEvasivoCoroutine != null)
            StopCoroutine(movimientoEvasivoCoroutine);
        if (disparosLentosCoroutine != null)
            StopCoroutine(disparosLentosCoroutine);

        yield return StartCoroutine(RegresarAPosicionesOriginales());

        enMovimientoEvasivo = false;
        Debug.Log("Fin del movimiento evasivo");
    }

    IEnumerator MovimientoEvasivoCoroutine()
    {
        while (enMovimientoEvasivo)
        {
            yield return null;
        }
    }

    void ActualizarMovimientoEvasivo()
    {
        for (int i = 0; i < hongos.Length; i++)
        {
            if (hongos[i].estaVivo && hongos[i].hongo != null)
            {
                tiemposCambioDireccion[i] += Time.deltaTime;

                if (tiemposCambioDireccion[i] >= frecuenciaCambioDireccion)
                {
                    direccionesActuales[i] = GenerarNuevaDireccion(i);
                    tiemposCambioDireccion[i] = 0f;
                }

                MoverHongoEvasivo(i);
            }
        }
    }

    Vector3 GenerarNuevaDireccion(int indiceHongo)
    {
        Vector3 nuevaDireccion = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;

        Vector3 posicionActual = hongos[indiceHongo].hongo.transform.position;
        Vector3 posicionOriginal = posicionesOriginales[indiceHongo];

        if (Mathf.Abs(posicionActual.x - posicionOriginal.x) > rangoMovimientoX * 0.8f)
        {
            nuevaDireccion.x = Mathf.Sign(posicionOriginal.x - posicionActual.x);
        }

        if (Mathf.Abs(posicionActual.y - posicionOriginal.y) > rangoMovimientoY * 0.8f)
        {
            nuevaDireccion.y = Mathf.Sign(posicionOriginal.y - posicionActual.y);
        }

        return nuevaDireccion.normalized;
    }

    void MoverHongoEvasivo(int indiceHongo)
    {
        if (hongos[indiceHongo].hongo == null) return;

        Transform hongoTransform = hongos[indiceHongo].hongo.transform;
        Vector3 posicionOriginal = posicionesOriginales[indiceHongo];

        float velocidadActual = velocidadBaseMovimiento * multiplicadorVelocidad;
        Vector3 movimiento = direccionesActuales[indiceHongo] * velocidadActual * Time.deltaTime;
        Vector3 nuevaPosicion = hongoTransform.position + movimiento;

        nuevaPosicion.x = Mathf.Clamp(nuevaPosicion.x,
            posicionOriginal.x - rangoMovimientoX,
            posicionOriginal.x + rangoMovimientoX);
        nuevaPosicion.y = Mathf.Clamp(nuevaPosicion.y,
            posicionOriginal.y - rangoMovimientoY,
            posicionOriginal.y + rangoMovimientoY);

        // NUEVO: Actualizar dirección de movimiento
        direccionesMovimiento[indiceHongo] = (nuevaPosicion - hongoTransform.position).normalized;

        hongoTransform.position = nuevaPosicion;
    }

    IEnumerator DisparosLentosDuranteMovimiento()
    {
        while (enMovimientoEvasivo)
        {
            foreach (HongoData hongo in hongos)
            {
                if (hongo.estaVivo && hongo.hongo != null && jugador != null)
                {
                    DisparoProyectilLento(hongo.hongo.transform);
                }
            }

            yield return new WaitForSeconds(tiempoEntreDisparosLentos);
        }
    }

    void DisparoProyectilLento(Transform origen)
    {
        if (origen == null || jugador == null) return;

        if (proyectilEspora != null)
        {
            GameObject proyectil = Instantiate(proyectilEspora, origen.position, Quaternion.identity);
            Vector2 direccion = (jugador.position - origen.position).normalized;

            CA_ProyectilHilo proyectilScript = proyectil.GetComponent<CA_ProyectilHilo>();
            if (proyectilScript != null)
            {
                int danioActual = Mathf.RoundToInt(danioBaseProyectil * multiplicadorDanio);
                proyectilScript.danio = danioActual;
                proyectilScript.layerSuelo = layerSuelo;
            }

            Rigidbody2D rb = proyectil.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direccion * velocidadProyectilLento;
            }

            Destroy(proyectil, 5f);
        }
    }

    IEnumerator RegresarAPosicionesOriginales()
    {
        List<Coroutine> movimientos = new List<Coroutine>();

        for (int i = 0; i < hongos.Length; i++)
        {
            if (hongos[i].estaVivo && hongos[i].hongo != null)
            {
                movimientos.Add(StartCoroutine(MoverHongoSuave(hongos[i].hongo.transform, posicionesOriginales[i], 1f)));
            }
        }

        foreach (Coroutine mov in movimientos)
        {
            yield return mov;
        }
    }

    // --- SISTEMA DE SELECCIÓN DE ATAQUES ---
    void SeleccionarYRealizarAtaque()
    {
        List<int> ataquesDisponibles = new List<int>();

        // Siempre disponibles
        ataquesDisponibles.Add(1); // Balas Giratorias con Rayo

        // Añadir movimiento evasivo solo si hay 3 hongos
        if (hongosVivos == 3)
        {
            ataquesDisponibles.Add(0); // Movimiento Evasivo
        }

        // Añadir estocada con mayor probabilidad cuando hay menos hongos
        if (hongosVivos < 3)
        {
            for (int i = 0; i < 3; i++)
            {
                ataquesDisponibles.Add(2); // Estocada
            }
        }
        else
        {
            ataquesDisponibles.Add(2); // Estocada
        }

        ataqueActual = ataquesDisponibles[Random.Range(0, ataquesDisponibles.Count)];

        Debug.Log($"Ataque seleccionado: {ataqueActual} - Disponibles: {ataquesDisponibles.Count}");

        switch (ataqueActual)
        {
            case 0:
                StartCoroutine(AtaqueMovimientoEvasivo());
                break;
            case 1:
                StartCoroutine(AtaqueBalasGiratorias());
                break;
            case 2:
                StartCoroutine(AtaqueEstocada());
                break;
        }
    }

    IEnumerator AtaqueMovimientoEvasivo()
    {
        ataqueEnCurso = true;
        Debug.Log("Iniciando ataque de movimiento evasivo");

        yield return StartCoroutine(IniciarMovimientoEvasivo());

        ataqueEnCurso = false;
    }

    // --- ATAQUE BALAS GIRATORIAS CON EFECTO RAYO ---
    IEnumerator AtaqueBalasGiratorias()
    {
        ataqueEnCurso = true;
        Debug.Log("Iniciando ataque de balas giratorias con rayo");

        Coroutine movimientoCoroutine = StartCoroutine(MovimientoDuranteBalasGiratorias());

        List<Coroutine> coroutinesBalas = new List<Coroutine>();

        foreach (HongoData hongo in hongos)
        {
            if (hongo.estaVivo && hongo.hongo != null)
            {
                coroutinesBalas.Add(StartCoroutine(CrearBalasGiratorias(hongo.hongo.transform)));
            }
        }

        yield return new WaitForSeconds(3f);

        if (movimientoCoroutine != null)
            StopCoroutine(movimientoCoroutine);

        foreach (GameObject bala in hilosActivos)
        {
            if (bala != null) Destroy(bala);
        }
        hilosActivos.Clear();

        ataqueEnCurso = false;
    }

    IEnumerator MovimientoDuranteBalasGiratorias()
    {
        float tiempoInicio = Time.time;
        while (Time.time - tiempoInicio < 3f)
        {
            for (int i = 0; i < hongos.Length; i++)
            {
                if (hongos[i].estaVivo && hongos[i].hongo != null)
                {
                    Vector3 direccionAleatoria = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
                    float velocidadRapida = velocidadBaseMovimiento * multiplicadorVelocidad * 2f;

                    Vector3 nuevaPosicion = hongos[i].hongo.transform.position + direccionAleatoria * velocidadRapida * Time.deltaTime;
                    Vector3 posicionOriginal = posicionesOriginales[i];

                    nuevaPosicion.x = Mathf.Clamp(nuevaPosicion.x,
                        posicionOriginal.x - rangoMovimientoX,
                        posicionOriginal.x + rangoMovimientoX);
                    nuevaPosicion.y = Mathf.Clamp(nuevaPosicion.y,
                        posicionOriginal.y - rangoMovimientoY,
                        posicionOriginal.y + rangoMovimientoY);

                    hongos[i].hongo.transform.position = nuevaPosicion;
                }
            }
            yield return null;
        }
    }

    IEnumerator CrearBalasGiratorias(Transform origen)
    {
        if (origen == null) yield break;

        GameObject contenedorBalas = new GameObject("BalasGiratoriasConRayo");
        contenedorBalas.transform.position = origen.position;
        hilosActivos.Add(contenedorBalas);

        List<GameObject> balasConEfecto = new List<GameObject>();

        for (int i = 0; i < numeroHilos; i++)
        {
            GameObject contenedorBala = new GameObject("BalaConRayo_" + i);
            contenedorBala.transform.SetParent(contenedorBalas.transform);

            GameObject balaObj = Instantiate(proyectilEspora, origen.position, Quaternion.identity);
            balaObj.name = "BalaVisual_" + i;
            balaObj.transform.SetParent(contenedorBala.transform);

            CA_ProyectilHilo proyectilHilo = balaObj.GetComponent<CA_ProyectilHilo>();
            if (proyectilHilo != null)
            {
                int danioActual = Mathf.RoundToInt(danioBaseHilos * 0.7f * multiplicadorDanio);
                proyectilHilo.danio = danioActual;
                proyectilHilo.layerSuelo = layerSuelo;

                Rigidbody2D rb = balaObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    rb.gravityScale = 0f;
                }

                balaObj.transform.localScale = Vector3.one * 0.5f;
            }

            GameObject efectoRayo = new GameObject("EfectoRayo_" + i);
            efectoRayo.transform.SetParent(contenedorBala.transform);
            efectoRayo.transform.position = origen.position;

            LineRenderer lineRenderer = efectoRayo.AddComponent<LineRenderer>();
            ConfigurarLineRenderer(lineRenderer);

            lineRenderer.startWidth = anchoHilo * 0.5f;
            lineRenderer.endWidth = anchoHilo * 0.3f;
            lineRenderer.positionCount = 8;

            balasConEfecto.Add(contenedorBala);

            float angulo = i * (360f / numeroHilos);
            PosicionarBalaConRayoEnCirculo(contenedorBala.transform, balaObj.transform, efectoRayo, origen.position, angulo, radioHilos);
        }

        float tiempoInicio = Time.time;
        while (Time.time - tiempoInicio < 3f && contenedorBalas != null && origen != null)
        {
            contenedorBalas.transform.RotateAround(origen.position, Vector3.forward, velocidadRotacionHilos * Time.deltaTime);

            for (int i = 0; i < balasConEfecto.Count; i++)
            {
                if (balasConEfecto[i] != null)
                {
                    float angulo = i * (360f / numeroHilos) + contenedorBalas.transform.eulerAngles.z;
                    ActualizarBalaConRayo(balasConEfecto[i].transform, origen.position, angulo, radioHilos, Time.time - tiempoInicio);
                }
            }
            yield return null;
        }

        if (contenedorBalas != null)
        {
            foreach (GameObject bala in balasConEfecto)
            {
                if (bala != null) Destroy(bala);
            }
            Destroy(contenedorBalas);
        }
    }

    void PosicionarBalaConRayoEnCirculo(Transform contenedorBala, Transform balaVisual, GameObject efectoRayo, Vector3 centro, float angulo, float radio)
    {
        float anguloRad = angulo * Mathf.Deg2Rad;
        Vector3 posicionBala = centro + new Vector3(Mathf.Cos(anguloRad), Mathf.Sin(anguloRad), 0) * radio;

        contenedorBala.position = posicionBala;
        balaVisual.position = posicionBala;

        ActualizarRayo(efectoRayo, centro, posicionBala, 0f);
    }

    void ActualizarBalaConRayo(Transform contenedorBala, Vector3 centro, float angulo, float radio, float tiempo)
    {
        if (contenedorBala == null) return;

        float anguloRad = angulo * Mathf.Deg2Rad;
        Vector3 posicionBala = centro + new Vector3(Mathf.Cos(anguloRad), Mathf.Sin(anguloRad), 0) * radio;

        contenedorBala.position = posicionBala;

        Transform efectoRayo = null;
        Transform balaVisual = null;

        foreach (Transform child in contenedorBala)
        {
            if (child.name.StartsWith("EfectoRayo_"))
                efectoRayo = child;
            else if (child.name.StartsWith("BalaVisual_"))
                balaVisual = child;
        }

        if (efectoRayo != null)
        {
            ActualizarRayo(efectoRayo.gameObject, centro, posicionBala, tiempo);
        }

        if (balaVisual != null)
        {
            balaVisual.position = posicionBala;
        }
    }

    void ActualizarRayo(GameObject efectoRayo, Vector3 inicio, Vector3 fin, float tiempo)
    {
        LineRenderer lineRenderer = efectoRayo.GetComponent<LineRenderer>();
        if (lineRenderer == null) return;

        Vector3 direccion = (fin - inicio).normalized;
        float distancia = Vector3.Distance(inicio, fin);
        Vector3 normal = new Vector3(-direccion.y, direccion.x, 0);

        lineRenderer.positionCount = 8;

        for (int i = 0; i < 8; i++)
        {
            float t = i / 7f;
            Vector3 puntoBase = Vector3.Lerp(inicio, fin, t);

            float frecuencia = 12f;
            float amplitud = 0.15f * (1f - t * 0.5f);
            float onda = Mathf.Sin(t * frecuencia * Mathf.PI + tiempo * 20f) * amplitud;

            Vector3 puntoFinal = puntoBase + normal * onda;
            lineRenderer.SetPosition(i, puntoFinal);
        }
    }

    void ConfigurarLineRenderer(LineRenderer lr)
    {
        lr.startWidth = anchoHilo;
        lr.endWidth = anchoHilo;
        lr.material = materialHilo != null ? materialHilo : new Material(Shader.Find("Sprites/Default"));
        lr.startColor = colorHilo;
        lr.endColor = colorHilo;
        lr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.useWorldSpace = true;
    }

    // --- ATAQUE ESTOCADA ---
    IEnumerator AtaqueEstocada()
    {
        ataqueEnCurso = true;
        Debug.Log("¡INICIANDO ATAQUE ESTOCADA!");

        List<Coroutine> estocadas = new List<Coroutine>();

        foreach (HongoData hongo in hongos)
        {
            if (hongo.estaVivo && hongo.hongo != null && jugador != null)
            {
                float velocidadBase = hongosVivos < 3 ? velocidadBaseEstocada * 1.5f : velocidadBaseEstocada;
                float velocidadActual = velocidadBase * multiplicadorVelocidad;
                estocadas.Add(StartCoroutine(EstocadaIndividualMejorada(hongo.hongo.transform, velocidadActual)));
            }
        }

        foreach (Coroutine estocada in estocadas)
        {
            yield return estocada;
        }

        ataqueEnCurso = false;
        Debug.Log("Estocada completada");
    }

    IEnumerator EstocadaIndividualMejorada(Transform hongo, float velocidad)
    {
        if (hongo == null || jugador == null) yield break;

        Vector3 posicionInicial = hongo.position;

        Debug.Log($"Hongo {hongo.name} iniciando estocada hacia player");

        SpriteRenderer sr = hongo.GetComponent<SpriteRenderer>();
        Color originalColor = sr != null ? sr.color : Color.white;
        if (sr != null)
        {
            sr.color = Color.yellow;
            Vector3 escalaOriginal = hongo.localScale;
            hongo.localScale = escalaOriginal * 1.2f;
        }

        yield return new WaitForSeconds(0.3f);

        if (sr != null)
        {
            sr.color = Color.red;
        }

        Vector3 direccion = (jugador.position - hongo.position).normalized;
        float distanciaInicial = Vector3.Distance(hongo.position, jugador.position);
        float distanciaRecorrida = 0f;
        bool golpeado = false;

        while (distanciaRecorrida < distanciaInicial && !golpeado && hongo != null)
        {
            float distanciaFrame = velocidad * Time.deltaTime;
            hongo.position += direccion * distanciaFrame;
            distanciaRecorrida += distanciaFrame;

            Collider2D colision = Physics2D.OverlapCircle(hongo.position, 1f);
            if (colision != null && colision.CompareTag("Player"))
            {
                PlayerHealth playerHealth = colision.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    int danioActual = Mathf.RoundToInt(danioBaseEstocada * multiplicadorDanio);
                    playerHealth.RecibirDanio(danioActual);
                    golpeado = true;
                    Debug.Log($"¡ESTOCO! {danioActual} de daño (Multiplicador: {multiplicadorDanio}x)");
                }
                break;
            }

            yield return null;
        }

        int ObtenerIndiceDesdeTransform(Transform hongoTransform)
        {
            for (int i = 0; i < hongos.Length; i++)
            {
                if (hongos[i].hongo != null && hongos[i].hongo.transform == hongoTransform)
                {
                    return i;
                }
            }
            return -1;
        }

        yield return new WaitForSeconds(0.2f);

        Vector3 puntoRetirada = ObtenerPuntoRetiradaAleatorio();
        Debug.Log($"Hongo {hongo.name} retirándose a punto: {puntoRetirada}");

        if (sr != null)
        {
            sr.color = originalColor;
            hongo.localScale = hongo.localScale / 1.2f;
        }

        Vector3 direccionRetirada = (puntoRetirada - hongo.position).normalized;
        float distanciaRetirada = Vector3.Distance(hongo.position, puntoRetirada);
        float distanciaRetiradaRecorrida = 0f;

        while (distanciaRetiradaRecorrida < distanciaRetirada && hongo != null)
        {
            float distanciaFrame = velocidad * 0.7f * Time.deltaTime;
            hongo.position += direccionRetirada * distanciaFrame;
            distanciaRetiradaRecorrida += distanciaFrame;
            yield return null;
        }

        Debug.Log($"Hongo {hongo.name} completó estocada");
    }

    Vector3 ObtenerPuntoRetiradaAleatorio()
    {
        if (puntosRetirada != null && puntosRetirada.Length > 0)
        {
            return puntosRetirada[Random.Range(0, puntosRetirada.Length)].position;
        }
        else
        {
            Vector3 centro = transform.position;
            float radio = 5f;
            float angulo = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            return centro + new Vector3(Mathf.Cos(angulo) * radio, Mathf.Sin(angulo) * radio, 0);
        }
    }

    IEnumerator MoverHongoSuave(Transform hongo, Vector3 objetivo, float duracion)
    {
        if (hongo == null) yield break;

        Vector3 inicio = hongo.position;
        float tiempo = 0f;

        while (tiempo < duracion && hongo != null)
        {
            hongo.position = Vector3.Lerp(inicio, objetivo, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }

        if (hongo != null) hongo.position = objetivo;
    }

    // --- SISTEMA DE DAÑO Y FUSIÓN CON MULTIPLICADORES ---
    public void RecibirDano(int indiceHongo, float dano)
    {
        if (!estaDespierto || ataqueEnCurso) return;
        if (indiceHongo < 0 || indiceHongo >= hongos.Length) return;
        if (!hongos[indiceHongo].estaVivo || hongos[indiceHongo].hongo == null) return;

        hongos[indiceHongo].vida -= dano;
        ActualizarColorHongo(indiceHongo);

        if (hongos[indiceHongo].vida <= 0)
        {
            HongoDerrotado(indiceHongo);
        }
    }

    void ActualizarColorHongo(int indice)
    {
        if (hongos[indice].hongo == null) return;
        SpriteRenderer sr = hongos[indice].hongo.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            float porcentajeVida = hongos[indice].vida / hongos[indice].vidaMaxima;
            sr.color = Color.Lerp(Color.black, indice == 2 ? Color.red : Color.green, porcentajeVida);
        }
    }

    void HongoDerrotado(int indice)
    {
        if (hongos[indice].hongo != null)
        {
            SpriteRenderer sr = hongos[indice].hongo.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.black;
            StartCoroutine(EfectoMuerte(hongos[indice].hongo));
        }

        hongos[indice].estaVivo = false;
        hongosVivos--;

        Debug.Log($"¡Hongo {indice} derrotado! Hongos restantes: {hongosVivos}");

        ActualizarMultiplicadores();

        if (hongosVivos == 2) StartCoroutine(FusionarHongos());
        else if (hongosVivos == 1) StartCoroutine(FusionFinal());
        else if (hongosVivos == 0) Debug.Log("¡Mini Boss Derrotado!");
    }

    void ActualizarMultiplicadores()
    {
        int hongosMuertos = 3 - hongosVivos;

        multiplicadorDanio = Mathf.Pow(2f, hongosMuertos);
        multiplicadorVelocidad = Mathf.Pow(2f, hongosMuertos);

        danioEstocada = Mathf.RoundToInt(danioBaseEstocada * multiplicadorDanio);
        velocidadMovimiento = velocidadBaseMovimiento * multiplicadorVelocidad;
        velocidadHilosRectos = velocidadBaseHilos * multiplicadorVelocidad;
        danioContacto = Mathf.RoundToInt(danioBaseContacto * multiplicadorDanio);

        Debug.Log($"Multiplicadores actualizados - Daño: {multiplicadorDanio}x, Velocidad: {multiplicadorVelocidad}x");
    }

    IEnumerator EfectoMuerte(GameObject hongo)
    {
        if (hongo == null) yield break;
        for (int i = 0; i < 3; i++)
        {
            if (hongo != null)
            {
                hongo.transform.localScale *= 0.7f;
                yield return new WaitForSeconds(0.1f);
            }
        }
        if (hongo != null) hongo.SetActive(false);
    }

    IEnumerator FusionarHongos()
    {
        Debug.Log("¡Fusión! 2 hongos restantes - Ataques más rápidos y agresivos");
        tiempoEntreAtaques = velocidadAtaqueFusion;
        foreach (HongoData hongo in hongos)
        {
            if (hongo.estaVivo && hongo.hongo != null)
            {
                SpriteRenderer sr = hongo.hongo.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.yellow;
                    StartCoroutine(EfectoFusion(hongo.hongo.transform));
                }
            }
        }
        yield return new WaitForSeconds(1f);
    }

    IEnumerator FusionFinal()
    {
        Debug.Log("¡Fusión Final! 1 hongo restante - Ataques ultra rápidos");
        tiempoEntreAtaques = velocidadAtaqueFusion * 0.5f;
        foreach (HongoData hongo in hongos)
        {
            if (hongo.estaVivo && hongo.hongo != null)
            {
                SpriteRenderer sr = hongo.hongo.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.red;
                    StartCoroutine(EfectoFusionFinal(hongo.hongo.transform));
                }
            }
        }
        yield return new WaitForSeconds(1f);
    }

    IEnumerator EfectoFusion(Transform hongo)
    {
        if (hongo == null) yield break;
        Vector3 escalaOriginal = hongo.localScale;
        float tiempo = 0f;
        while (tiempo < 0.5f && hongo != null)
        {
            hongo.localScale = escalaOriginal * (1f + Mathf.Sin(tiempo * 10f) * 0.2f);
            tiempo += Time.deltaTime;
            yield return null;
        }
        if (hongo != null) hongo.localScale = escalaOriginal;
    }

    IEnumerator EfectoFusionFinal(Transform hongo)
    {
        if (hongo == null) yield break;
        Vector3 escalaOriginal = hongo.localScale;
        hongo.localScale = escalaOriginal * 1.3f;
        float tiempo = 0f;
        while (tiempo < 1f && hongo != null)
        {
            hongo.localScale = escalaOriginal * 1.3f * (1f + Mathf.Sin(tiempo * 15f) * 0.1f);
            tiempo += Time.deltaTime;
            yield return null;
        }
    }

    public void ActivarBoss()
    {
        if (!estaDespierto)
        {
            estaDespierto = true;
            Debug.Log("¡MINI BOSS ACTIVADO!");
            foreach (HongoData hongo in hongos)
            {
                if (hongo.hongo != null)
                {
                    StartCoroutine(EfectoActivacion(hongo.hongo.transform));
                }
            }
            tiempoUltimoAtaque = Time.time;
        }
    }

    IEnumerator EfectoActivacion(Transform hongo)
    {
        if (hongo == null) yield break;
        Vector3 escalaOriginal = hongo.localScale;
        float duracion = 0.5f;
        float tiempo = 0f;
        while (tiempo < duracion && hongo != null)
        {
            hongo.localScale = escalaOriginal * (1f + Mathf.PingPong(tiempo * 2f, 0.3f));
            tiempo += Time.deltaTime;
            yield return null;
        }
        if (hongo != null) hongo.localScale = escalaOriginal;
    }
}