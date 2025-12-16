using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_Noxar : MonoBehaviour
{
    [Header("Referencias")]
    public Transform jugador;
    public Transform centroDisparo;
    public GameObject prefabBala;

    [Header("Configuración de Puntos de Spawn para Espadas")]
    public int cantidadPuntosSpawn = 5;
    public float alturaDesdeLimiteInferior = 0.5f;
    private List<Transform> puntosSpawnCreados = new List<Transform>();

    [Header("Prefabs de Balas Específicos")]
    public GameObject prefabBalaBasica;
    public GameObject prefabBalaAvanzada;
    public GameObject prefabMeteoritoEspecial;
    public GameObject prefabEspadaSuelo;

    [Header("Área de Patrulla")]
    public Vector2 areaPatrulla = new Vector2(8f, 8f);
    public Vector2 centroArea;
    public bool usarPosicionInicialComoCentro = true;

    [Header("Movimiento")]
    public float velocidadPatrulla = 1.5f;
    public float velocidadPersecucion = 2.5f;
    public float velocidadFaseFinal = 3.5f;
    public float velocidadMovimientoConstante = 1.5f;

    [Header("Rangos de Detección")]
    public float rangoDeteccion = 7f;
    public float rangoMinimoPersecucion = 2f;
    public float distanciaParaSaltoAtras = 1.5f;

    [Header("Ataque - Disparo Radial")]
    public int cantidadBalasRadial = 8;
    public float velocidadBalaRadial = 4f;
    public float danoBalaRadial = 1;
    public float tiempoEntreRafagas = 0.8f;
    public float tiempoEntreBalasEnRafaga = 0.1f;

    [Header("Ataque - Torbellino de Balas")]
    public int cantidadBalasTorbellino = 12;
    public float velocidadBalaTorbellino = 3f;
    public float danoBalaTorbellino = 1;
    public float tiempoPreparacionTorbellino = 1f;
    public float duracionTorbellino = 3f;
    public float radioInicialTorbellino = 1.5f;
    public float velocidadExpansionTorbellino = 1f;
    public float velocidadRotacionTorbellino = 120f;

    [Header("Ataque - Lluvia de Meteoritos")]
    public int cantidadMeteoritos = 8;
    public float anchoAreaLluvia = 6f;
    public float alturaInicialMeteoritos = 8f;
    public float danoMeteorito = 2;
    public float tiempoPreparacionLluvia = 1f;
    public float tiempoEntreMeteoritos = 0.3f;
    public GameObject efectoZonaImpacto;
    public float tamanoMeteorito = 0.4f;

    [Header("Ataque - Anillo de Protección")]
    public int cantidadBalasAnillo = 16;
    public float radioAnillo = 2f;
    public float velocidadRotacionAnillo = 90f;
    public float danoBalaAnillo = 1;
    public float duracionAnillo = 3f;

    [Header("Ataque - Espadas del Suelo")]
    public int cantidadEspadas = 5;
    public float alturaMaximaEspadas = 3f;
    public float velocidadSubidaEspadas = 2f;
    public float velocidadOscilacionEspadas = 2f;
    public float amplitudOscilacion = 1.5f;
    public float danoEspada = 2f;
    public float duracionEspadas = 4f;
    public GameObject efectoSalidaEspada;

    [Header("Fases del Combate")]
    [Range(0, 100)] public int saludFase2 = 70;
    [Range(0, 100)] public int saludFase3 = 40;
    public float aumentoVelocidadFase2 = 1.3f;
    public float aumentoVelocidadFase3 = 1.6f;

    [Header("Movimientos Especiales")]
    public float distanciaTeletransporte = 4f;
    public float tiempoEntreTeletransportes = 2f;
    public GameObject efectoTeletransporte;

    [Header("Efectos Visuales")]
    public GameObject efectoCarga;
    public GameObject efectoImpacto;
    public GameObject auraFase2;
    public GameObject auraFase3;
    public Material materialInvencible;
    private Material materialOriginal;

    // Variables privadas
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Collider2D colisionador;
    private CA_RecolEnemy recolEnemy;

    private bool estaMuerto = false;
    private bool puedeAtacar = true;
    private bool enSecuenciaAtaque = false;
    private float tiempoUltimoAtaque = 0f;
    private int faseActual = 1;

    private bool jugadorDetectado = false;
    private Vector3 posicionInicial;
    private Vector3 destinoPatrulla;

    private List<GameObject> balasAnilloActual = new List<GameObject>();
    private List<GameObject> balasActivas = new List<GameObject>();
    private List<GameObject> espadasActivas = new List<GameObject>();
    private bool anilloActivo = false;

    // Estados visuales
    private bool estaAtacando = false;
    private bool estaEnPreparacion = false;
    private Color colorOriginal;

    // Variables para movimiento constante
    private Vector3 direccionMovimiento = Vector3.right;
    private float tiempoUltimoCambioDireccion = 0f;
    private float intervaloCambioDireccion = 2f;
    private bool movimientoConstanteActivo = true;

    // Variables para secuencia de ataques
    private List<System.Func<IEnumerator>> ataquesFase1 = new List<System.Func<IEnumerator>>();
    private List<System.Func<IEnumerator>> ataquesFase2 = new List<System.Func<IEnumerator>>();
    private List<System.Func<IEnumerator>> ataquesFase3 = new List<System.Func<IEnumerator>>();

    // Variables para el sistema de ataques aleatorios
    private float tiempoEntreAtaques = 1.5f;
    private float tiempoMinimoEntreAtaques = 0.8f;
    private float tiempoMaximoEntreAtaques = 2.5f;
    private float tiempoUltimoAtaqueAleatorio = 0f;

    void Start()
    {
        // Inicializar componentes
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        colisionador = GetComponent<Collider2D>();
        recolEnemy = GetComponent<CA_RecolEnemy>();

        // Configurar Rigidbody para no girar
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Asegurar que tenemos un sprite renderer
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            // Crear sprite básico circular
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Texture2D tex = new Texture2D(64, 64);
            Color colorSphere = new Color(0.2f, 0.2f, 0.8f, 1f);

            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(32, 32));
                    if (dist < 32)
                        tex.SetPixel(x, y, colorSphere);
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();

            Sprite sphereSprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
            Destroy(sphere);

            spriteRenderer.sprite = sphereSprite;
            spriteRenderer.color = new Color(0.2f, 0.2f, 0.8f);
        }

        // Guardar color y material original
        colorOriginal = spriteRenderer.color;
        materialOriginal = spriteRenderer.material;
        spriteRenderer.enabled = true;

        // Buscar jugador si no está asignado
        if (jugador == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                jugador = playerObj.transform;
            else
                Debug.LogWarning("Noxar: No se encontró jugador con tag 'Player'");
        }

        // Configurar posición inicial
        posicionInicial = transform.position;
        if (usarPosicionInicialComoCentro)
            centroArea = posicionInicial;

        // Crear puntos de spawn para espadas EN EL LÍMITE INFERIOR
        CrearPuntosSpawnEnLimiteInferior();

        // Iniciar con movimiento constante
        CambiarDireccionMovimiento();

        // Inicializar listas de ataques por fase
        InicializarListasAtaques();

        Debug.Log($"Noxar inicializado con {puntosSpawnCreados.Count} puntos de spawn para espadas");
    }

    void CrearPuntosSpawnEnLimiteInferior()
    {
        // Limpiar puntos anteriores si existen
        LimpiarPuntosSpawnAnteriores();

        // Calcular el límite inferior del área
        float limiteInferiorY = centroArea.y - (areaPatrulla.y / 2f);

        // Calcular el ancho disponible (un poco menos que el área completa)
        float anchoDisponible = areaPatrulla.x * 0.8f;
        float inicioX = centroArea.x - (anchoDisponible / 2f);
        float separacionX = anchoDisponible / (cantidadPuntosSpawn - 1);

        Debug.Log($"Creando {cantidadPuntosSpawn} puntos de spawn en Y={limiteInferiorY + alturaDesdeLimiteInferior}");

        // Crear puntos distribuidos horizontalmente en el límite inferior
        for (int i = 0; i < cantidadPuntosSpawn; i++)
        {
            GameObject punto = new GameObject($"PuntoEspada_{i}");

            // Posicionar en el límite inferior del área
            float posX = inicioX + (i * separacionX);
            float posY = limiteInferiorY + alturaDesdeLimiteInferior;

            punto.transform.position = new Vector3(posX, posY, 0);
            punto.transform.parent = null; // Sin parent

            puntosSpawnCreados.Add(punto.transform);

            // Agregar un componente para debugging visual
            punto.AddComponent<SpawnPointVisualizer>();
        }

        Debug.Log($"Creados {cantidadPuntosSpawn} puntos de spawn en el límite inferior");
    }

    void LimpiarPuntosSpawnAnteriores()
    {
        foreach (Transform punto in puntosSpawnCreados)
        {
            if (punto != null && punto.gameObject != null)
                Destroy(punto.gameObject);
        }
        puntosSpawnCreados.Clear();
    }

    void InicializarListasAtaques()
    {
        // Fase 1
        ataquesFase1.Add(() => AtaqueRadialBasicoCoroutine());
        ataquesFase1.Add(() => TeletransporteYDispareCoroutine());

        // Fase 2
        ataquesFase2.Add(() => AtaqueTorbellinoCoroutine());
        ataquesFase2.Add(() => AtaqueRadialMejoradoCoroutine());
        ataquesFase2.Add(() => LluviaDeMeteoritosCoroutine());
        ataquesFase2.Add(() => AnilloDeProteccionCoroutine());
        ataquesFase2.Add(() => EspadasDelSueloCoroutine());

        // Fase 3
        ataquesFase3.Add(() => LluviaDeMeteoritosCoroutine());
        ataquesFase3.Add(() => AtaqueTorbellinoMejoradoCoroutine());
        ataquesFase3.Add(() => AnilloDeProteccionMejoradoCoroutine());
        ataquesFase3.Add(() => AtaqueMasivo360Coroutine());
        ataquesFase3.Add(() => EspadasDelSueloMejoradoCoroutine());
    }

    void Update()
    {
        if (estaMuerto) return;

        // Verificar muerte
        if (recolEnemy != null && recolEnemy.EstaMuerto())
        {
            Morir();
            return;
        }

        // Actualizar fase del combate
        ActualizarFaseCombate();

        // Detección del jugador
        ActualizarDeteccionJugador();

        // Movimiento constante SIEMPRE ACTIVO (excepto durante preparaciones)
        if (movimientoConstanteActivo && !estaEnPreparacion)
        {
            MovimientoConstante();
        }

        // Sistema de ataques aleatorios
        if (puedeAtacar && !enSecuenciaAtaque && Time.time > tiempoUltimoAtaqueAleatorio + tiempoEntreAtaques)
        {
            StartCoroutine(EjecutarAtaqueAleatorio());
        }

        // Asegurar visibilidad constante
        AsegurarVisibilidad();
    }

    IEnumerator EjecutarAtaqueAleatorio()
    {
        if (estaMuerto || !puedeAtacar || enSecuenciaAtaque) yield break;

        enSecuenciaAtaque = true;

        // Obtener lista de ataques disponibles para la fase actual
        List<System.Func<IEnumerator>> ataquesDisponibles = GetAtaquesFaseActual();

        if (ataquesDisponibles.Count > 0)
        {
            // Seleccionar un ataque aleatorio
            int indiceAtaque = Random.Range(0, ataquesDisponibles.Count);

            // Posibilidad de ataque doble en fases avanzadas
            bool ataqueDoble = false;
            if (faseActual >= 2 && Random.value > 0.6f)
            {
                ataqueDoble = true;
            }

            // Posibilidad de ataque triple en fase 3
            bool ataqueTriple = false;
            if (faseActual == 3 && Random.value > 0.7f)
            {
                ataqueTriple = true;
            }

            // Ejecutar el ataque principal
            yield return StartCoroutine(ataquesDisponibles[indiceAtaque]());

            // Posibilidad de ataque doble
            if (ataqueDoble && !estaMuerto)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));

                // Seleccionar otro ataque (puede ser el mismo o diferente)
                int segundoIndice = Random.value > 0.5f ? indiceAtaque : Random.Range(0, ataquesDisponibles.Count);
                yield return StartCoroutine(ataquesDisponibles[segundoIndice]());
            }

            // Posibilidad de ataque triple (solo fase 3)
            if (ataqueTriple && !estaMuerto && ataqueDoble)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.2f));

                // Tercer ataque (siempre diferente)
                int tercerIndice;
                do
                {
                    tercerIndice = Random.Range(0, ataquesDisponibles.Count);
                } while (tercerIndice == indiceAtaque);

                yield return StartCoroutine(ataquesDisponibles[tercerIndice]());
            }
        }

        // Calcular tiempo para el próximo ataque
        CalcularTiempoProximoAtaque();
        tiempoUltimoAtaqueAleatorio = Time.time;
        enSecuenciaAtaque = false;
    }

    void CalcularTiempoProximoAtaque()
    {
        // El tiempo entre ataques varía según la fase
        switch (faseActual)
        {
            case 1:
                tiempoEntreAtaques = Random.Range(1.8f, 2.5f);
                break;
            case 2:
                tiempoEntreAtaques = Random.Range(1.2f, 2.0f);
                break;
            case 3:
                tiempoEntreAtaques = Random.Range(0.8f, 1.5f);
                break;
        }

        // Reducir tiempo si el jugador está cerca
        if (jugadorDetectado)
        {
            float distancia = Vector2.Distance(transform.position, jugador.position);
            if (distancia < rangoDeteccion * 0.5f)
            {
                tiempoEntreAtaques *= 0.7f;
            }
        }

        // Asegurar límites
        tiempoEntreAtaques = Mathf.Clamp(tiempoEntreAtaques, tiempoMinimoEntreAtaques, tiempoMaximoEntreAtaques);
    }

    List<System.Func<IEnumerator>> GetAtaquesFaseActual()
    {
        switch (faseActual)
        {
            case 3: return ataquesFase3;
            case 2: return ataquesFase2;
            default: return ataquesFase1;
        }
    }

    void MovimientoConstante()
    {
        // Cambiar dirección periódicamente
        if (Time.time > tiempoUltimoCambioDireccion + intervaloCambioDireccion)
        {
            CambiarDireccionMovimiento();
            tiempoUltimoCambioDireccion = Time.time;

            // Ajustar intervalo según fase
            if (faseActual == 3)
                intervaloCambioDireccion = Random.Range(1f, 1.5f);
            else if (faseActual == 2)
                intervaloCambioDireccion = Random.Range(1.5f, 2f);
            else
                intervaloCambioDireccion = Random.Range(2f, 3f);
        }

        // Calcular nueva posición
        Vector3 nuevaPosicion = transform.position + direccionMovimiento * GetVelocidadMovimiento() * Time.deltaTime;

        // Verificar límites del área
        if (Mathf.Abs(nuevaPosicion.x - centroArea.x) > areaPatrulla.x / 2f)
        {
            direccionMovimiento.x *= -1;
            nuevaPosicion.x = Mathf.Clamp(nuevaPosicion.x,
                centroArea.x - areaPatrulla.x / 2f,
                centroArea.x + areaPatrulla.x / 2f);
        }
        if (Mathf.Abs(nuevaPosicion.y - centroArea.y) > areaPatrulla.y / 2f)
        {
            direccionMovimiento.y *= -1;
            nuevaPosicion.y = Mathf.Clamp(nuevaPosicion.y,
                centroArea.y - areaPatrulla.y / 2f,
                centroArea.y + areaPatrulla.y / 2f);
        }

        // Mover al jefe (SOLO MOVIMIENTO, SIN ROTACIÓN)
        transform.position = nuevaPosicion;
    }

    void CambiarDireccionMovimiento()
    {
        // Elegir nueva dirección aleatoria
        float angulo = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        direccionMovimiento = new Vector3(Mathf.Cos(angulo), Mathf.Sin(angulo), 0);

        // Si el jugador está detectado, dirigirse más hacia él
        if (jugadorDetectado && Random.value > 0.5f && faseActual >= 2)
        {
            Vector3 direccionAlJugador = (jugador.position - transform.position).normalized;
            direccionMovimiento = Vector3.Lerp(direccionMovimiento, direccionAlJugador, 0.3f).normalized;
        }
    }

    float GetVelocidadMovimiento()
    {
        float velocidadBase = velocidadMovimientoConstante;

        switch (faseActual)
        {
            case 3: return velocidadBase * aumentoVelocidadFase3;
            case 2: return velocidadBase * aumentoVelocidadFase2;
            default: return velocidadBase;
        }
    }

    void AsegurarVisibilidad()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = colorOriginal;
        }
    }

    void ActualizarFaseCombate()
    {
        if (recolEnemy == null) return;

        int saludActual = (int)recolEnemy.GetHealth();
        int nuevaFase = 1;

        if (saludActual <= saludFase3)
            nuevaFase = 3;
        else if (saludActual <= saludFase2)
            nuevaFase = 2;

        if (nuevaFase != faseActual)
        {
            faseActual = nuevaFase;
            CambiarAuraFase(faseActual);
            Debug.Log($"Noxar: Cambiando a Fase {faseActual}");
        }
    }

    void CambiarAuraFase(int fase)
    {
        // Desactivar todas las auras
        if (auraFase2 != null) auraFase2.SetActive(false);
        if (auraFase3 != null) auraFase3.SetActive(false);

        // Activar aura según fase
        switch (fase)
        {
            case 2:
                if (auraFase2 != null)
                {
                    auraFase2.SetActive(true);
                    auraFase2.transform.localScale = Vector3.one * 1.2f;
                }
                break;
            case 3:
                if (auraFase3 != null)
                {
                    auraFase3.SetActive(true);
                    auraFase3.transform.localScale = Vector3.one * 1.5f;
                }
                break;
        }
    }

    void ActualizarDeteccionJugador()
    {
        if (jugador == null) return;

        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);
        jugadorDetectado = distanciaAlJugador <= rangoDeteccion;
    }

    // ============================================
    // ATAQUES PRINCIPALES - MODIFICADOS PARA ATAQUES ALEATORIOS
    // ============================================

    IEnumerator AtaqueRadialBasicoCoroutine()
    {
        if (!puedeAtacar || estaMuerto) yield break;

        estaAtacando = true;
        puedeAtacar = false;
        enSecuenciaAtaque = true;

        CrearEfectoCarga();
        yield return new WaitForSeconds(0.2f);

        DisparoRadial(cantidadBalasRadial, velocidadBalaRadial, danoBalaRadial, true);

        yield return new WaitForSeconds(0.5f);

        estaAtacando = false;
        puedeAtacar = true;
        enSecuenciaAtaque = false;
    }

    IEnumerator AtaqueRadialMejoradoCoroutine()
    {
        if (!puedeAtacar || estaMuerto) yield break;

        estaAtacando = true;
        puedeAtacar = false;
        enSecuenciaAtaque = true;

        CrearEfectoCarga();
        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < 2; i++)
        {
            int balas = cantidadBalasRadial + (i * 4);
            float velocidad = velocidadBalaRadial + (i * 1f);
            DisparoRadial(balas, velocidad, danoBalaRadial + i, false);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.3f);

        estaAtacando = false;
        puedeAtacar = true;
        enSecuenciaAtaque = false;
    }

    IEnumerator AtaqueTorbellinoCoroutine()
    {
        if (!puedeAtacar || estaMuerto) yield break;

        estaAtacando = true;
        estaEnPreparacion = true;
        movimientoConstanteActivo = false;
        puedeAtacar = false;
        enSecuenciaAtaque = true;

        CrearEfectoCarga();
        yield return new WaitForSeconds(0.5f);

        estaEnPreparacion = false;
        movimientoConstanteActivo = true;

        List<GameObject> balasTorbellino = new List<GameObject>();
        float anguloPorBala = 360f / cantidadBalasTorbellino;

        for (int i = 0; i < cantidadBalasTorbellino; i++)
        {
            float angulo = i * anguloPorBala * Mathf.Deg2Rad;
            Vector3 posicion = transform.position + new Vector3(
                Mathf.Cos(angulo) * radioInicialTorbellino,
                Mathf.Sin(angulo) * radioInicialTorbellino,
                0
            );

            GameObject bala = InstantiarBala(posicion, Quaternion.identity, false);
            if (bala != null)
            {
                ConfigurarBalaTorbellino(bala, angulo);
                balasTorbellino.Add(bala);
                balasActivas.Add(bala);

                SpriteRenderer balaRenderer = bala.GetComponent<SpriteRenderer>();
                if (balaRenderer != null)
                {
                    balaRenderer.color = Color.yellow;
                }
            }
        }

        float tiempoInicio = Time.time;
        float duracionTotal = duracionTorbellino * 1.0f;

        while (Time.time - tiempoInicio < duracionTotal && !estaMuerto)
        {
            float tiempoTranscurrido = Time.time - tiempoInicio;
            float radioActual = radioInicialTorbellino + (tiempoTranscurrido * velocidadExpansionTorbellino * 0.8f);

            for (int i = 0; i < balasTorbellino.Count; i++)
            {
                if (balasTorbellino[i] == null) continue;

                float angulo = (i * anguloPorBala + (tiempoTranscurrido * velocidadRotacionTorbellino * 0.7f)) * Mathf.Deg2Rad;
                Vector3 nuevaPosicion = transform.position + new Vector3(
                    Mathf.Cos(angulo) * radioActual,
                    Mathf.Sin(angulo) * radioActual,
                    0
                );

                balasTorbellino[i].transform.position = nuevaPosicion;
            }
            yield return null;
        }

        foreach (GameObject bala in balasTorbellino)
        {
            if (bala != null)
                Destroy(bala);
            balasActivas.Remove(bala);
        }

        yield return new WaitForSeconds(0.2f);

        estaAtacando = false;
        puedeAtacar = true;
        enSecuenciaAtaque = false;
    }

    IEnumerator AtaqueTorbellinoMejoradoCoroutine()
    {
        if (!puedeAtacar || estaMuerto) yield break;

        estaAtacando = true;
        estaEnPreparacion = true;
        movimientoConstanteActivo = false;
        puedeAtacar = false;
        enSecuenciaAtaque = true;

        CrearEfectoCarga();
        yield return new WaitForSeconds(0.3f);

        estaEnPreparacion = false;
        movimientoConstanteActivo = true;

        for (int t = 0; t < 2; t++)
        {
            StartCoroutine(CrearTorbellinoIndividual(
                radioInicialTorbellino + (t * 1f),
                velocidadRotacionTorbellino * (t % 2 == 0 ? 1 : -1.2f),
                t * 0.2f
            ));
        }

        yield return new WaitForSeconds(duracionTorbellino * 1.2f + 0.2f);

        estaAtacando = false;
        puedeAtacar = true;
        enSecuenciaAtaque = false;
    }

    IEnumerator LluviaDeMeteoritosCoroutine()
    {
        if (!puedeAtacar || estaMuerto) yield break;

        puedeAtacar = false;
        estaAtacando = true;
        estaEnPreparacion = true;
        movimientoConstanteActivo = false;
        enSecuenciaAtaque = true;

        CrearEfectoCarga();
        yield return new WaitForSeconds(0.5f);

        estaEnPreparacion = false;
        movimientoConstanteActivo = true;

        Vector2 centroAreaLluvia;
        if (jugador != null)
        {
            centroAreaLluvia = jugador.position;
        }
        else
        {
            centroAreaLluvia = transform.position;
        }

        float limiteIzquierdo = centroAreaLluvia.x - (anchoAreaLluvia / 2f);
        float limiteDerecho = centroAreaLluvia.x + (anchoAreaLluvia / 2f);

        for (int i = 0; i < cantidadMeteoritos; i++)
        {
            float posX = Random.Range(limiteIzquierdo, limiteDerecho);
            float posY = centroAreaLluvia.y + alturaInicialMeteoritos;

            Vector2 posicionObjetivo = new Vector2(posX, centroAreaLluvia.y);
            Vector2 posicionInicio = new Vector2(posX, posY);

            if (efectoZonaImpacto != null && i % 2 == 0)
            {
                GameObject zona = Instantiate(efectoZonaImpacto, posicionObjetivo, Quaternion.identity);
                zona.transform.localScale = Vector3.one * 0.25f;
                Destroy(zona, 0.3f);
            }

            CrearMeteorito(posicionInicio, posicionObjetivo);
            yield return new WaitForSeconds(tiempoEntreMeteoritos * 0.8f);
        }

        yield return new WaitForSeconds(0.5f);

        estaAtacando = false;
        puedeAtacar = true;
        enSecuenciaAtaque = false;
    }

    IEnumerator AnilloDeProteccionCoroutine()
    {
        if (!puedeAtacar || estaMuerto) yield break;

        puedeAtacar = false;
        anilloActivo = true;
        estaAtacando = true;
        enSecuenciaAtaque = true;

        float anguloPorBala = 360f / cantidadBalasAnillo;

        for (int i = 0; i < cantidadBalasAnillo; i++)
        {
            float angulo = i * anguloPorBala * Mathf.Deg2Rad;
            Vector3 posicion = transform.position + new Vector3(
                Mathf.Cos(angulo) * radioAnillo,
                Mathf.Sin(angulo) * radioAnillo,
                0
            );

            GameObject bala = InstantiarBala(posicion, Quaternion.identity, false);
            if (bala != null)
            {
                ConfigurarBalaAnillo(bala, i * anguloPorBala);
                balasAnilloActual.Add(bala);

                SpriteRenderer balaRenderer = bala.GetComponent<SpriteRenderer>();
                if (balaRenderer != null)
                {
                    balaRenderer.color = Color.cyan;
                }
            }
        }

        float tiempoInicio = Time.time;
        float duracionTotal = duracionAnillo * 1.0f;

        while (Time.time - tiempoInicio < duracionTotal && !estaMuerto)
        {
            float anguloRotacion = (Time.time - tiempoInicio) * velocidadRotacionAnillo * 0.8f;

            for (int i = 0; i < balasAnilloActual.Count; i++)
            {
                if (balasAnilloActual[i] == null) continue;

                float angulo = ((i * anguloPorBala) + anguloRotacion) * Mathf.Deg2Rad;
                Vector3 nuevaPosicion = transform.position + new Vector3(
                    Mathf.Cos(angulo) * radioAnillo,
                    Mathf.Sin(angulo) * radioAnillo,
                    0
                );

                balasAnilloActual[i].transform.position = nuevaPosicion;
            }
            yield return null;
        }

        foreach (GameObject bala in balasAnilloActual)
        {
            if (bala != null)
                Destroy(bala);
        }
        balasAnilloActual.Clear();

        anilloActivo = false;
        estaAtacando = false;
        puedeAtacar = true;
        enSecuenciaAtaque = false;
    }

    IEnumerator AnilloDeProteccionMejoradoCoroutine()
    {
        if (!puedeAtacar || estaMuerto) yield break;

        puedeAtacar = false;
        anilloActivo = true;
        estaAtacando = true;
        enSecuenciaAtaque = true;

        for (int anillo = 0; anillo < 2; anillo++)
        {
            float radioActual = radioAnillo + (anillo * 0.8f);
            float velocidadActual = velocidadRotacionAnillo * (anillo % 2 == 0 ? 1 : -1.5f);

            StartCoroutine(CrearAnilloIndividual(
                cantidadBalasAnillo / 2,
                radioActual,
                velocidadActual,
                anillo * 0.1f
            ));
        }

        float tiempoInicio = Time.time;
        float duracionTotal = duracionAnillo * 1.2f;

        while (Time.time - tiempoInicio < duracionTotal && !estaMuerto)
        {
            if (Time.time - tiempoInicio > 0.3f && Mathf.FloorToInt(Time.time - tiempoInicio) % 1 == 0)
            {
                DisparoRadialDesdeAnillo();
            }
            yield return null;
        }

        foreach (GameObject bala in balasAnilloActual)
        {
            if (bala != null)
                Destroy(bala);
        }
        balasAnilloActual.Clear();

        anilloActivo = false;
        estaAtacando = false;
        puedeAtacar = true;
        enSecuenciaAtaque = false;
    }

    IEnumerator AtaqueMasivo360Coroutine()
    {
        if (!puedeAtacar || estaMuerto) yield break;

        puedeAtacar = false;
        estaAtacando = true;
        enSecuenciaAtaque = true;

        SetInvencible(true);

        for (int i = 0; i < 2; i++)
        {
            CrearEfectoCarga();
            yield return new WaitForSeconds(0.1f);
        }

        for (int capa = 0; capa < 3; capa++)
        {
            int balas = 16 + (capa * 6);
            float velocidad = 4f + (capa * 0.8f);
            DisparoRadial(balas, velocidad, danoBalaRadial + 1, false);
            yield return new WaitForSeconds(0.05f);
        }

        SetInvencible(false);
        yield return new WaitForSeconds(0.5f);

        estaAtacando = false;
        puedeAtacar = true;
        enSecuenciaAtaque = false;
    }

    IEnumerator TeletransporteYDispareCoroutine()
    {
        if (!puedeAtacar || estaMuerto) yield break;

        puedeAtacar = false;
        estaAtacando = true;
        enSecuenciaAtaque = true;

        if (efectoTeletransporte != null)
            Instantiate(efectoTeletransporte, transform.position, Quaternion.identity);

        Vector3 nuevaPosicion = CalcularPosicionTeletransporte();

        float tiempoMovimiento = 0.2f;
        float tiempoTranscurrido = 0f;
        Vector3 posicionInicialMov = transform.position;

        while (tiempoTranscurrido < tiempoMovimiento)
        {
            tiempoTranscurrido += Time.deltaTime;
            float t = tiempoTranscurrido / tiempoMovimiento;
            transform.position = Vector3.Lerp(posicionInicialMov, nuevaPosicion, t);
            yield return null;
        }

        if (efectoTeletransporte != null)
            Instantiate(efectoTeletransporte, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.05f);
        DisparoRadial(cantidadBalasRadial * 2, velocidadBalaRadial * 1.2f, danoBalaRadial, false);

        yield return new WaitForSeconds(0.3f);
        estaAtacando = false;
        puedeAtacar = true;
        enSecuenciaAtaque = false;
    }

    // ============================================
    // ATAQUE DE ESPADAS DEL SUELO - MODIFICADO PARA USAR PUNTOS DINÁMICOS
    // ============================================

    IEnumerator EspadasDelSueloCoroutine()
    {
        if (!puedeAtacar || estaMuerto) yield break;

        estaAtacando = true;
        puedeAtacar = false;
        enSecuenciaAtaque = true;

        Debug.Log("Noxar: Espadas del Suelo (usando puntos dinámicos)");

        CrearEfectoCarga();
        yield return new WaitForSeconds(0.3f);

        // Verificar que tenemos puntos de spawn
        if (puntosSpawnCreados.Count == 0)
        {
            Debug.LogWarning("No hay puntos de spawn creados. Creando puntos ahora...");
            CrearPuntosSpawnEnLimiteInferior();
        }

        // Usar los puntos de spawn creados dinámicamente
        int espadasASpawnear = Mathf.Min(cantidadEspadas, puntosSpawnCreados.Count);

        Debug.Log($"Spawneando {espadasASpawnear} espadas en {puntosSpawnCreados.Count} puntos disponibles");

        for (int i = 0; i < espadasASpawnear; i++)
        {
            Transform puntoSpawn = puntosSpawnCreados[i];
            if (puntoSpawn == null)
            {
                Debug.LogWarning($"Punto de spawn {i} es null");
                continue;
            }

            Vector3 posicionSpawn = puntoSpawn.position;
            Debug.Log($"Espada {i} en posición: {posicionSpawn}");

            if (efectoSalidaEspada != null)
            {
                GameObject efecto = Instantiate(efectoSalidaEspada, posicionSpawn, Quaternion.identity);
                Destroy(efecto, 0.5f);
            }

            CrearEspada(posicionSpawn, i);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(duracionEspadas);

        foreach (GameObject espada in espadasActivas)
        {
            if (espada != null)
                Destroy(espada);
        }
        espadasActivas.Clear();

        estaAtacando = false;
        puedeAtacar = true;
        enSecuenciaAtaque = false;
    }

    IEnumerator EspadasDelSueloMejoradoCoroutine()
    {
        if (!puedeAtacar || estaMuerto) yield break;

        estaAtacando = true;
        puedeAtacar = false;
        enSecuenciaAtaque = true;

        Debug.Log("Noxar: Espadas del Suelo Mejorado");

        CrearEfectoCarga();
        yield return new WaitForSeconds(0.2f);

        // Verificar puntos de spawn
        if (puntosSpawnCreados.Count == 0)
        {
            CrearPuntosSpawnEnLimiteInferior();
        }

        // Para el ataque mejorado, usar todos los puntos disponibles
        int puntosTotales = puntosSpawnCreados.Count;

        for (int i = 0; i < puntosTotales; i++)
        {
            Transform puntoSpawn = puntosSpawnCreados[i];
            if (puntoSpawn == null) continue;

            Vector3 posicionSpawn = puntoSpawn.position;

            if (efectoSalidaEspada != null)
            {
                GameObject efecto = Instantiate(efectoSalidaEspada, posicionSpawn, Quaternion.identity);
                efecto.transform.localScale = Vector3.one * 0.8f;
                Destroy(efecto, 0.5f);
            }

            CrearEspadaMejorada(posicionSpawn, i, i % 2);
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(duracionEspadas * 1.2f);

        foreach (GameObject espada in espadasActivas)
        {
            if (espada != null)
                Destroy(espada);
        }
        espadasActivas.Clear();

        estaAtacando = false;
        puedeAtacar = true;
        enSecuenciaAtaque = false;
    }

    void CrearEspada(Vector3 posicion, int indice)
    {
        if (prefabEspadaSuelo == null)
        {
            Debug.LogError("Noxar: No hay prefab de espada asignado!");
            return;
        }

        GameObject espada = Instantiate(prefabEspadaSuelo, posicion, Quaternion.identity);
        espadasActivas.Add(espada);

        CA_EspadaSuelo espadaScript = espada.GetComponent<CA_EspadaSuelo>();
        if (espadaScript == null)
            espadaScript = espada.AddComponent<CA_EspadaSuelo>();

        espadaScript.Configurar(
            alturaMaximaEspadas,
            velocidadSubidaEspadas,
            velocidadOscilacionEspadas,
            amplitudOscilacion,
            indice * 0.2f,
            danoEspada,
            duracionEspadas,
            gameObject
        );

        SpriteRenderer renderer = espada.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            if (faseActual == 3)
                renderer.color = new Color(1f, 0.3f, 0.3f);
            else if (faseActual == 2)
                renderer.color = new Color(1f, 0.5f, 0f);
            else
                renderer.color = new Color(1f, 0.8f, 0f);
        }
    }

    void CrearEspadaMejorada(Vector3 posicion, int indice, int fila)
    {
        if (prefabEspadaSuelo == null) return;

        GameObject espada = Instantiate(prefabEspadaSuelo, posicion, Quaternion.identity);
        espadasActivas.Add(espada);

        CA_EspadaSuelo espadaScript = espada.GetComponent<CA_EspadaSuelo>();
        if (espadaScript == null)
            espadaScript = espada.AddComponent<CA_EspadaSuelo>();

        float velocidadOscilacion = velocidadOscilacionEspadas * (fila == 0 ? 1f : -1.5f);
        float desfase = (indice * 0.15f) + (fila * 0.5f);

        espadaScript.Configurar(
            alturaMaximaEspadas * 1.2f,
            velocidadSubidaEspadas * 1.3f,
            velocidadOscilacion,
            amplitudOscilacion * 1.5f,
            desfase,
            danoEspada * 1.5f,
            duracionEspadas * 1.2f,
            gameObject
        );

        SpriteRenderer renderer = espada.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = fila == 0 ?
                new Color(1f, 0.2f, 0.2f) :
                new Color(0.8f, 0.8f, 0f);
        }
    }

    // ============================================
    // MÉTODOS AUXILIARES DE ATAQUE
    // ============================================

    void DisparoRadial(int cantidad, float velocidad, float dano, bool esPrimerAtaque = false)
    {
        GameObject prefabAUsar;

        if (esPrimerAtaque && prefabBalaBasica != null)
        {
            prefabAUsar = prefabBalaBasica;
        }
        else if (prefabBalaAvanzada != null)
        {
            prefabAUsar = prefabBalaAvanzada;
        }
        else if (prefabBala != null)
        {
            prefabAUsar = prefabBala;
        }
        else
        {
            Debug.LogError("Noxar: No hay prefabs de bala asignados!");
            return;
        }

        float anguloPorBala = 360f / cantidad;

        for (int i = 0; i < cantidad; i++)
        {
            float angulo = i * anguloPorBala * Mathf.Deg2Rad;
            Vector3 direccion = new Vector3(Mathf.Cos(angulo), Mathf.Sin(angulo), 0);

            GameObject bala = Instantiate(prefabAUsar, centroDisparo.position, Quaternion.identity);
            if (bala != null)
            {
                CA_ProyectilNoxarSimple proyectil = bala.GetComponent<CA_ProyectilNoxarSimple>();
                if (proyectil == null)
                    proyectil = bala.AddComponent<CA_ProyectilNoxarSimple>();

                proyectil.velocidad = velocidad;
                proyectil.dano = dano;

                proyectil.Configurar(direccion, velocidad, dano, gameObject);

                balasActivas.Add(bala);
            }
        }
    }

    void DisparoRadialDesdeAnillo()
    {
        if (balasAnilloActual.Count == 0) return;

        foreach (GameObject bala in balasAnilloActual)
        {
            if (bala == null) continue;

            Vector3 direccion = (bala.transform.position - transform.position).normalized;
            GameObject nuevaBala = InstantiarBala(bala.transform.position, Quaternion.identity, false);
            if (nuevaBala != null)
            {
                CA_ProyectilNoxarSimple proyectil = nuevaBala.GetComponent<CA_ProyectilNoxarSimple>();
                if (proyectil == null)
                    proyectil = nuevaBala.AddComponent<CA_ProyectilNoxarSimple>();

                proyectil.velocidad = velocidadBalaRadial * 1.5f;
                proyectil.dano = danoBalaRadial;

                proyectil.Configurar(direccion, velocidadBalaRadial * 1.5f, danoBalaRadial, gameObject);

                SpriteRenderer balaRenderer = nuevaBala.GetComponent<SpriteRenderer>();
                if (balaRenderer != null)
                {
                    balaRenderer.color = Color.magenta;
                }
            }
        }
    }

    GameObject InstantiarBala(Vector3 posicion, Quaternion rotacion, bool esPrimerAtaque = false)
    {
        GameObject prefabAUsar;

        if (esPrimerAtaque && prefabBalaBasica != null)
        {
            prefabAUsar = prefabBalaBasica;
        }
        else if (prefabBalaAvanzada != null)
        {
            prefabAUsar = prefabBalaAvanzada;
        }
        else if (prefabBala != null)
        {
            prefabAUsar = prefabBala;
        }
        else
        {
            Debug.LogError("Noxar: Prefab de bala no asignado!");
            return null;
        }

        GameObject bala = Instantiate(prefabAUsar, posicion, rotacion);
        balasActivas.Add(bala);
        return bala;
    }

    void ConfigurarBalaTorbellino(GameObject bala, float anguloInicial)
    {
        if (bala == null) return;

        CA_ProyectilNoxarSimple proyectil = bala.GetComponent<CA_ProyectilNoxarSimple>();
        if (proyectil == null)
            proyectil = bala.AddComponent<CA_ProyectilNoxarSimple>();

        proyectil.dano = danoBalaTorbellino;
        proyectil.ConfigurarTorbellino(gameObject, danoBalaTorbellino, anguloInicial);
    }

    void ConfigurarBalaAnillo(GameObject bala, float anguloInicial)
    {
        if (bala == null) return;

        CA_ProyectilNoxarSimple proyectil = bala.GetComponent<CA_ProyectilNoxarSimple>();
        if (proyectil == null)
            proyectil = bala.AddComponent<CA_ProyectilNoxarSimple>();

        proyectil.dano = danoBalaAnillo;
        proyectil.ConfigurarAnillo(gameObject, danoBalaAnillo, anguloInicial);
    }

    IEnumerator CrearTorbellinoIndividual(float radio, float velocidadRotacion, float delay)
    {
        yield return new WaitForSeconds(delay);

        List<GameObject> balas = new List<GameObject>();
        float anguloPorBala = 360f / (cantidadBalasTorbellino / 2);

        for (int i = 0; i < cantidadBalasTorbellino / 2; i++)
        {
            float angulo = i * anguloPorBala * Mathf.Deg2Rad;
            Vector3 posicion = transform.position + new Vector3(
                Mathf.Cos(angulo) * radio,
                Mathf.Sin(angulo) * radio,
                0
            );

            GameObject bala = InstantiarBala(posicion, Quaternion.identity, false);
            if (bala != null)
            {
                ConfigurarBalaTorbellino(bala, angulo);
                balas.Add(bala);

                SpriteRenderer balaRenderer = bala.GetComponent<SpriteRenderer>();
                if (balaRenderer != null)
                {
                    balaRenderer.color = new Color(1f, 0.8f, 0.2f);
                }
            }
        }

        float tiempoInicio = Time.time;
        float duracion = duracionTorbellino * 1.0f - delay;

        while (Time.time - tiempoInicio < duracion && !estaMuerto)
        {
            float tiempoTranscurrido = Time.time - tiempoInicio;

            for (int i = 0; i < balas.Count; i++)
            {
                if (balas[i] == null) continue;

                float angulo = (i * anguloPorBala + (tiempoTranscurrido * velocidadRotacion * 0.7f)) * Mathf.Deg2Rad;
                Vector3 nuevaPosicion = transform.position + new Vector3(
                    Mathf.Cos(angulo) * radio,
                    Mathf.Sin(angulo) * radio,
                    0
                );

                balas[i].transform.position = nuevaPosicion;
            }
            yield return null;
        }

        foreach (GameObject bala in balas)
        {
            if (bala != null)
                Destroy(bala);
            balasActivas.Remove(bala);
        }
    }

    IEnumerator CrearAnilloIndividual(int cantidad, float radio, float velocidad, float delay)
    {
        yield return new WaitForSeconds(delay);

        List<GameObject> balasAnillo = new List<GameObject>();
        float anguloPorBala = 360f / cantidad;

        for (int i = 0; i < cantidad; i++)
        {
            float angulo = i * anguloPorBala * Mathf.Deg2Rad;
            Vector3 posicion = transform.position + new Vector3(
                Mathf.Cos(angulo) * radio,
                Mathf.Sin(angulo) * radio,
                0
            );

            GameObject bala = InstantiarBala(posicion, Quaternion.identity, false);
            if (bala != null)
            {
                ConfigurarBalaAnillo(bala, i * anguloPorBala);
                balasAnillo.Add(bala);
                balasAnilloActual.Add(bala);

                SpriteRenderer balaRenderer = bala.GetComponent<SpriteRenderer>();
                if (balaRenderer != null)
                {
                    balaRenderer.color = new Color(0f, 0.8f, 1f);
                }
            }
        }

        float tiempoInicio = Time.time;
        float duracion = duracionAnillo * 1.0f - delay;

        while (Time.time - tiempoInicio < duracion && !estaMuerto)
        {
            float tiempoTranscurrido = Time.time - tiempoInicio;
            float anguloRotacion = tiempoTranscurrido * velocidad * 0.8f;

            for (int i = 0; i < balasAnillo.Count; i++)
            {
                if (balasAnillo[i] == null) continue;

                float angulo = ((i * anguloPorBala) + anguloRotacion) * Mathf.Deg2Rad;
                Vector3 nuevaPosicion = transform.position + new Vector3(
                    Mathf.Cos(angulo) * radio,
                    Mathf.Sin(angulo) * radio,
                    0
                );

                balasAnillo[i].transform.position = nuevaPosicion;
            }
            yield return null;
        }
    }

    void CrearMeteorito(Vector2 posicionInicio, Vector2 posicionObjetivo)
    {
        GameObject prefabAUsar;

        if (prefabMeteoritoEspecial != null)
        {
            prefabAUsar = prefabMeteoritoEspecial;
        }
        else if (prefabBalaAvanzada != null)
        {
            prefabAUsar = prefabBalaAvanzada;
        }
        else if (prefabBala != null)
        {
            prefabAUsar = prefabBala;
        }
        else
        {
            Debug.LogError("Noxar: No hay prefabs de bala asignados!");
            return;
        }

        GameObject meteorito = Instantiate(prefabAUsar, posicionInicio, Quaternion.identity);

        CA_ProyectilNoxarSimple proyectil = meteorito.GetComponent<CA_ProyectilNoxarSimple>();
        if (proyectil == null)
            proyectil = meteorito.AddComponent<CA_ProyectilNoxarSimple>();

        proyectil.tamanoMeteorito = tamanoMeteorito;
        proyectil.ConfigurarMeteorito(posicionObjetivo, danoMeteorito, gameObject);

        SpriteRenderer meteoritoRenderer = meteorito.GetComponent<SpriteRenderer>();
        if (meteoritoRenderer != null)
        {
            meteoritoRenderer.color = new Color(1f, 0.4f, 0.1f);
        }

        balasActivas.Add(meteorito);
    }

    // ============================================
    // MÉTODOS DE UTILIDAD
    // ============================================

    Vector3 CalcularPosicionTeletransporte()
    {
        if (jugador == null) return transform.position;

        float angulo = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distancia = Random.Range(2f, 4f);

        Vector3 posicionCandidata = jugador.position + new Vector3(
            Mathf.Cos(angulo) * distancia,
            Mathf.Sin(angulo) * distancia,
            0
        );

        return posicionCandidata;
    }

    void CrearEfectoCarga()
    {
        if (efectoCarga != null && !estaMuerto)
        {
            GameObject efecto = Instantiate(efectoCarga, transform.position, Quaternion.identity);
            efecto.transform.localScale = Vector3.one * (1f + faseActual * 0.3f);
            Destroy(efecto, 0.5f);
        }
    }

    void SetInvencible(bool invencible)
    {
        if (spriteRenderer == null) return;

        if (invencible && materialInvencible != null)
        {
            spriteRenderer.material = materialInvencible;
            if (colisionador != null)
                colisionador.enabled = false;
        }
        else
        {
            spriteRenderer.material = materialOriginal;
            if (colisionador != null)
                colisionador.enabled = true;
        }
    }

    void LimpiarBalas()
    {
        foreach (GameObject bala in balasActivas)
        {
            if (bala != null)
                Destroy(bala);
        }
        balasActivas.Clear();

        foreach (GameObject bala in balasAnilloActual)
        {
            if (bala != null)
                Destroy(bala);
        }
        balasAnilloActual.Clear();
    }

    void LimpiarEspadas()
    {
        foreach (GameObject espada in espadasActivas)
        {
            if (espada != null)
                Destroy(espada);
        }
        espadasActivas.Clear();
    }

    void Morir()
    {
        if (estaMuerto) return;

        estaMuerto = true;
        Debug.Log("💀 Noxar MURIENDO");

        StopAllCoroutines();
        movimientoConstanteActivo = false;

        LimpiarBalas();
        LimpiarEspadas();

        // Limpiar puntos de spawn creados
        LimpiarPuntosSpawnAnteriores();

        if (colisionador != null)
            colisionador.enabled = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        }

        CambiarAuraFase(0);

        if (efectoImpacto != null)
        {
            GameObject efecto = Instantiate(efectoImpacto, transform.position, Quaternion.identity);
            efecto.transform.localScale = Vector3.one * 1.5f;
            Destroy(efecto, 1.5f);
        }

        StartCoroutine(DesvanecerYDestruir());
    }

    IEnumerator DesvanecerYDestruir()
    {
        if (spriteRenderer != null)
        {
            float duracion = 1.5f;
            float tiempo = 0f;
            Color colorInicial = spriteRenderer.color;

            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                float t = tiempo / duracion;
                float alpha = Mathf.Lerp(1f, 0f, t);
                spriteRenderer.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, alpha);
                yield return null;
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // Área de patrulla
        Gizmos.color = Color.cyan;
        Vector3 centroDibujo = usarPosicionInicialComoCentro && Application.isPlaying ?
            (Vector3)centroArea : transform.position;
        Gizmos.DrawWireCube(centroDibujo, new Vector3(areaPatrulla.x, areaPatrulla.y, 0));

        // Rango de detección
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        // Radio de anillo
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, radioAnillo);

        // Límite inferior donde se crearán los puntos
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            float limiteInferiorY = centroArea.y - (areaPatrulla.y / 2f) + alturaDesdeLimiteInferior;
            Gizmos.DrawLine(
                new Vector3(centroArea.x - areaPatrulla.x / 2f, limiteInferiorY, 0),
                new Vector3(centroArea.x + areaPatrulla.x / 2f, limiteInferiorY, 0)
            );
        }
    }
}

// Script auxiliar para visualizar los puntos de spawn
public class SpawnPointVisualizer : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}