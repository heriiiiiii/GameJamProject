using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_BossPlantaCarnivora : MonoBehaviour
{
    [Header("Estados del Boss")]
    public bool estaActivo = false;
    public float vidaMaxima = 500f;
    public float vidaActual;
    public int faseActual = 1;

    [Header("Referencias")]
    public Transform jugador;
    public GameObject florParasitaPrefab;
    public GameObject nubeEsporasPrefab;
    public Material materialRaices;
    public ParticleSystem particulasGolpe;
    public ParticleSystem particulasSpawn;
    public CA_EfectoVisionPlayer efectoVision;

    [Header("Látigos de Raíz")]
    public int numeroRaices = 8;
    public float radioRaices = 4f;
    public float velocidadRaices = 2f;
    public float danioRaiz = 15f;
    public Color colorRaices = new Color(0.4f, 0.2f, 0.1f, 1f);
    public Gradient gradienteRaices;

    [Header("Flor Parásita")]
    public float tiempoEntreFlores = 3f;
    public int danioFlor = 2;
    public float duracionAturdimiento = 1.5f;

    [Header("Exhalación de Esporas")]
    public float duracionNube = 8f;
    public float reduccionVision = 0.4f;
    public float tiempoDisipacionNormal = 5f;
    public float tiempoDisipacionQuieto = 2f;

    [Header("Sistema de Partículas")]
    public ParticleSystem particulasToxicas;
    public ParticleSystem particulasAtaque;
    public ParticleSystem particulasHumo;

    [Header("Efectos Visuales")]
    public LineRenderer prefabLineaRaiz;
    public Shader shaderRaices;
    public Texture2D texturaRaiz;

    // Variables internas
    private List<GameObject> raicesActivas = new List<GameObject>();
    private GameObject nubeEsporas;
    private float ultimoAtaqueRaices;
    private float ultimaFlor;
    private bool ataqueEnCurso = false;
    private Camera camaraPrincipal;

    void Start()
    {
        vidaActual = vidaMaxima;
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        camaraPrincipal = Camera.main;

        if (efectoVision == null)
            efectoVision = FindObjectOfType<CA_EfectoVisionPlayer>();

        ConfigurarParticulas();
        CrearMaterialRaices();

        if (particulasSpawn != null)
            particulasSpawn.Play();
    }

    void Update()
    {
        if (!estaActivo) return;

        VerificarCambioFase();

        if (!ataqueEnCurso)
        {
            if (Time.time - ultimoAtaqueRaices > 4f)
            {
                StartCoroutine(AtaqueLatigosRaiz());
                ultimoAtaqueRaices = Time.time;
            }
            else if (Time.time - ultimaFlor > tiempoEntreFlores)
            {
                StartCoroutine(AtaqueFlorParasita());
                ultimaFlor = Time.time;
            }
            else if (Random.Range(0f, 1f) > 0.7f)
            {
                StartCoroutine(AtaqueExhalacionEsporas());
            }
        }
    }

    void ConfigurarParticulas()
    {
        // Configurar partículas tóxicas
        if (particulasToxicas != null)
        {
            var main = particulasToxicas.main;
            main.startColor = new Color(0.3f, 0.8f, 0.2f, 0.6f);
            main.startSpeed = 2f;
            main.startLifetime = 3f;

            var emission = particulasToxicas.emission;
            emission.rateOverTime = 50f;

            var shape = particulasToxicas.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 1f;
        }

        // Configurar partículas de humo
        if (particulasHumo != null)
        {
            var main = particulasHumo.main;
            main.startColor = new Color(0.2f, 0.3f, 0.1f, 0.4f);
            main.startSpeed = 1f;
            main.startLifetime = 4f;
            main.maxParticles = 1000;

            var emission = particulasHumo.emission;
            emission.rateOverTime = 100f;
        }
    }

    void CrearMaterialRaices()
    {
        if (materialRaices == null)
        {
            materialRaices = new Material(Shader.Find("Sprites/Default"));
            materialRaices.color = colorRaices;

            if (texturaRaiz != null)
            {
                materialRaices.mainTexture = texturaRaiz;
            }
        }
    }

    // ATAQUE 1: LÁTIGOS DE RAÍZ MEJORADO
    IEnumerator AtaqueLatigosRaiz()
    {
        ataqueEnCurso = true;
        Debug.Log("¡Látigos de Raíz!");

        if (particulasAtaque != null)
            particulasAtaque.Play();

        yield return new WaitForSeconds(0.5f);

        List<Coroutine> coroutines = new List<Coroutine>();
        for (int i = 0; i < numeroRaices; i++)
        {
            Coroutine coroutine = StartCoroutine(CrearLatigoRaizEspiral(i));
            coroutines.Add(coroutine);
            yield return new WaitForSeconds(0.1f);
        }

        // Esperar a que todas las raíces terminen
        foreach (Coroutine coroutine in coroutines)
        {
            yield return coroutine;
        }

        yield return new WaitForSeconds(1f);

        // Destruir raíces con efecto
        foreach (GameObject raiz in raicesActivas)
        {
            if (raiz != null)
            {
                StartCoroutine(DesvanecerRaiz(raiz));
            }
        }
        raicesActivas.Clear();

        ataqueEnCurso = false;
    }

    IEnumerator CrearLatigoRaizEspiral(int indice)
    {
        GameObject raizObj = new GameObject($"CA_LatigoRaiz_{indice}");
        raizObj.transform.position = transform.position;
        raicesActivas.Add(raizObj);

        LineRenderer lineRenderer = raizObj.AddComponent<LineRenderer>();
        ConfigurarLineRendererRaiz(lineRenderer);

        float anguloInicial = indice * (360f / numeroRaices);
        int puntosEnEspiral = 20;
        float duracionAtaque = 1.5f;

        lineRenderer.positionCount = puntosEnEspiral;

        float tiempo = 0f;
        while (tiempo < duracionAtaque)
        {
            float progreso = tiempo / duracionAtaque;

            for (int i = 0; i < puntosEnEspiral; i++)
            {
                float t = i / (float)(puntosEnEspiral - 1);
                float angulo = anguloInicial + (progreso * 360f * 2f) + (i * 10f);
                float radio = Mathf.Lerp(0.5f, radioRaices, t) * progreso;

                // Patrón espiral con noise para irregularidad
                float noise = Mathf.PerlinNoise(i * 0.3f, Time.time * 3f + indice) * 0.5f;
                radio += noise * 0.3f;

                Vector3 punto = CalcularPuntoEnEspiral(transform.position, angulo, radio, progreso);
                lineRenderer.SetPosition(i, punto);
            }

            // Verificar colisión con jugador
            if (VerificarColisionRaizJugador(lineRenderer))
            {
                AplicarDanioJugador(danioRaiz, "Latigo de Raíz");
                break;
            }

            tiempo += Time.deltaTime;
            yield return null;
        }

        // Efecto de golpe al final
        if (particulasGolpe != null)
        {
            Vector3 ultimoPunto = lineRenderer.GetPosition(puntosEnEspiral - 1);
            particulasGolpe.transform.position = ultimoPunto;
            particulasGolpe.Play();
        }
    }

    bool VerificarColisionRaizJugador(LineRenderer raiz)
    {
        if (jugador == null) return false;

        for (int i = 0; i < raiz.positionCount - 1; i++)
        {
            Vector3 puntoA = raiz.GetPosition(i);
            Vector3 puntoB = raiz.GetPosition(i + 1);

            float distancia = DistanciaPuntoALinea(jugador.position, puntoA, puntoB);
            if (distancia < 0.3f)
            {
                return true;
            }
        }
        return false;
    }

    float DistanciaPuntoALinea(Vector3 punto, Vector3 lineaInicio, Vector3 lineaFin)
    {
        Vector3 direccionLinea = lineaFin - lineaInicio;
        float longitudLinea = direccionLinea.magnitude;
        direccionLinea.Normalize();

        Vector3 puntoAlInicio = punto - lineaInicio;
        float proyeccion = Vector3.Dot(puntoAlInicio, direccionLinea);
        proyeccion = Mathf.Clamp(proyeccion, 0f, longitudLinea);

        Vector3 puntoMasCercano = lineaInicio + direccionLinea * proyeccion;
        return Vector3.Distance(punto, puntoMasCercano);
    }

    IEnumerator DesvanecerRaiz(GameObject raiz)
    {
        LineRenderer lr = raiz.GetComponent<LineRenderer>();
        Color colorInicial = lr.startColor;
        float duracion = 0.5f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            float alpha = Mathf.Lerp(1f, 0f, tiempo / duracion);
            lr.startColor = new Color(colorInicial.r, colorInicial.g, colorInicial.b, alpha);
            lr.endColor = new Color(colorInicial.r, colorInicial.g, colorInicial.b, alpha);
            tiempo += Time.deltaTime;
            yield return null;
        }

        Destroy(raiz);
    }

    Vector3 CalcularPuntoEnEspiral(Vector3 centro, float angulo, float radio, float progreso)
    {
        float anguloRad = angulo * Mathf.Deg2Rad;
        float irregularidad = Mathf.PerlinNoise(angulo * 0.1f, Time.time * 2f) * 0.5f;

        Vector3 punto = centro + new Vector3(
            Mathf.Cos(anguloRad) * (radio + irregularidad),
            Mathf.Sin(anguloRad) * (radio + irregularidad * 0.5f),
            0
        );

        // Agregar altura variable para efecto 3D
        punto.z = Mathf.Sin(progreso * Mathf.PI) * 0.5f;

        return punto;
    }

    void ConfigurarLineRendererRaiz(LineRenderer lr)
    {
        lr.startWidth = 0.4f;
        lr.endWidth = 0.1f;
        lr.material = materialRaices;
        lr.startColor = colorRaices;
        lr.endColor = colorRaices;
        lr.numCapVertices = 5;
        lr.numCornerVertices = 5;
        lr.textureMode = LineTextureMode.Tile;
        lr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.useWorldSpace = true;

        if (gradienteRaices != null)
        {
            lr.colorGradient = gradienteRaices;
        }
    }

    // Resto de los métodos se mantienen igual...
    IEnumerator AtaqueFlorParasita()
    {
        ataqueEnCurso = true;
        Debug.Log("¡Flores Parásitas!");

        int numeroFlores = faseActual == 1 ? 3 : 5;

        for (int i = 0; i < numeroFlores; i++)
        {
            LanzarFlorParasita();
            yield return new WaitForSeconds(0.5f);
        }

        ataqueEnCurso = false;
    }

    void LanzarFlorParasita()
    {
        if (florParasitaPrefab == null || jugador == null) return;

        Vector3 direccion = (jugador.position - transform.position).normalized;
        Vector3 posicionLanzamiento = transform.position + direccion * 2f;

        GameObject flor = Instantiate(florParasitaPrefab, posicionLanzamiento, Quaternion.identity);
        CA_FlorParasita florScript = flor.GetComponent<CA_FlorParasita>();

        if (florScript != null)
        {
            florScript.Inicializar(jugador, danioFlor, duracionAturdimiento);
        }
    }

    IEnumerator AtaqueExhalacionEsporas()
    {
        ataqueEnCurso = true;
        Debug.Log("¡Exhalación de Esporas!");

        if (particulasToxicas != null)
        {
            particulasToxicas.transform.position = transform.position;
            particulasToxicas.Play();
        }

        if (particulasHumo != null)
        {
            particulasHumo.transform.position = transform.position;
            particulasHumo.Play();
        }

        if (nubeEsporasPrefab != null && jugador != null)
        {
            nubeEsporas = Instantiate(nubeEsporasPrefab, jugador.position, Quaternion.identity);
            CA_NubeEsporas nubeScript = nubeEsporas.GetComponent<CA_NubeEsporas>();

            if (nubeScript != null)
            {
                nubeScript.Inicializar(duracionNube, reduccionVision, tiempoDisipacionNormal, tiempoDisipacionQuieto, jugador, efectoVision);
            }
        }

        yield return new WaitForSeconds(2f);
        ataqueEnCurso = false;
    }

    void AplicarDanioJugador(float danio, string fuente)
    {
        PlayerHealth playerHealth = jugador.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.RecibirDanio((int)danio);
            Debug.Log($"{fuente} hizo {danio} de daño");
        }
    }

    void VerificarCambioFase()
    {
        float porcentajeVida = vidaActual / vidaMaxima;

        if (porcentajeVida <= 0.5f && faseActual == 1)
        {
            CambiarFase(2);
        }
        else if (porcentajeVida <= 0.2f && faseActual == 2)
        {
            CambiarFase(3);
        }
    }

    void CambiarFase(int nuevaFase)
    {
        faseActual = nuevaFase;
        Debug.Log($"Boss entra en Fase {faseActual}!");

        switch (faseActual)
        {
            case 2:
                numeroRaices = 12;
                tiempoEntreFlores = 2f;
                velocidadRaices *= 1.3f;
                break;
            case 3:
                numeroRaices = 16;
                tiempoEntreFlores = 1f;
                duracionNube = 12f;
                velocidadRaices *= 1.5f;
                break;
        }

        if (particulasSpawn != null)
            particulasSpawn.Play();
    }

    public void RecibirDanio(float danio)
    {
        if (!estaActivo) return;

        vidaActual -= danio;
        Debug.Log($"Boss recibe {danio} de daño - Vida: {vidaActual}/{vidaMaxima}");

        // Efecto visual al recibir daño
        StartCoroutine(EfectoDanio());

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    IEnumerator EfectoDanio()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Color original = sprite.color;
            sprite.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sprite.color = original;
        }
    }

    void Morir()
    {
        Debug.Log("Boss derrotado!");
        estaActivo = false;
        StartCoroutine(EfectoMuerte());
    }

    IEnumerator EfectoMuerte()
    {
        // Efectos de muerte
        if (particulasToxicas != null)
            particulasToxicas.Stop();

        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    public void ActivarBoss()
    {
        estaActivo = true;
        Debug.Log("¡BOSS ACTIVADO! - Planta Carnívora");

        if (particulasSpawn != null)
            particulasSpawn.Play();
    }
}