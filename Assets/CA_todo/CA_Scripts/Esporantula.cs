using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_Esporantula : MonoBehaviour
{
    [Header("Puntos de salto")]
    public Transform[] puntos;
    public float velocidadSalto = 8f;
    public float tiempoEntreSaltos = 1f;
    public float tiempoPreparacionSalto = 1.5f;
    public float tiempoPreparacionAtaque = 1f;

    [Header("Jugador y detección")]
    public float rangoDeteccion = 5f;
    public string tagJugador = "Player";

    [Header("Mordisco")]
    public int danoMordisco = 1;
    public float knockbackForce = 5f;
    public int saltosParaAtacar = 2;

    [Header("Efecto Tela de Araña")]
    public LineRenderer lineRendererTela;
    public float anchoTela = 0.05f;
    public Color colorTela = new Color(1f, 1f, 1f, 0.7f);
    public float tiempoMostrarTela = 0.3f;

    [Header("Ajustes del tejido extremo (abanico)")]
    public int ringCount = 4;
    public int ringSegments = 12;
    public float ringSpacing = 0.2f;
    public float fanAngle = 160f;
    public int puntosCurvaCentral = 15;

    [Header("Nudos de telaraña")]
    public bool activarNudosVisuales = true;
    public float tamañoNudo = 0.05f;
    public Color colorNudo = new Color(1f, 1f, 1f, 0.9f);

    private List<Vector3> posicionesNudos = new List<Vector3>();

    private Animator animator;
    private bool saltando = false;
    private Transform jugador;
    private bool jugadorDetectado = false;
    private int contadorSaltos = 0;
    private Vector3 destinoActual;
    private Vector3 escalaOriginal;

    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int IsFalling = Animator.StringToHash("IsFalling");
    private static readonly int IsLanding = Animator.StringToHash("IsLanding");
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int IsJumpIdle = Animator.StringToHash("IsJumpIdle");
    private static readonly int IsDetectingPlayer = Animator.StringToHash("IsDetectingPlayer");
    private static readonly int IsIdle = Animator.StringToHash("IsIdle");
    private static readonly int IsDead = Animator.StringToHash("IsDead");
    private static readonly int IsAttackingPlayer = Animator.StringToHash("IsAttackingPlayer");

    private bool estaMuerto = false;
    private bool movimientoActivo = false;
    private bool preparandoAtaque = false;
    private bool esAtaqueAlJugador = false;
    private Vector3 posicionInicialSalto;
    private float duracionSaltoActual;
    private float tiempoTranscurridoSalto;
    private float alturaMaximaSalto;

    // ===============================
    // 🔊 AUDIO TELARAÑA (CON PROXIMIDAD)
    // ===============================
    [Header("🔊 Audio Ataque (Telaraña)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip telaranaLanzarClip;
    [SerializeField] private AudioClip telaranaSaltoClip;
    [Range(0f, 1f)][SerializeField] private float volLanzar = 1f;
    [Range(0f, 1f)][SerializeField] private float volSalto = 1f;
    [SerializeField] private float rangoAudio = 6f; // 🔥 NUEVO

    private void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (audioSource)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        escalaOriginal = transform.localScale;
        ConfigurarLineRenderer();
        if (TryGetComponent<Rigidbody2D>(out Rigidbody2D rb)) rb.freezeRotation = true;
        ResetearAnimaciones();
        StartCoroutine(MovimientoAleatorio());
    }

    // 🔹 PROXIMIDAD AUDIO
    bool PlayerEnRangoAudio()
    {
        if (!jugador) return false;
        return Vector2.Distance(transform.position, jugador.position) <= rangoAudio;
    }

    void ResetearAnimaciones()
    {
        animator.SetBool(IsIdle, true);
        animator.SetBool(IsJumping, false);
        animator.SetBool(IsAttacking, false);
        animator.SetBool(IsDead, false);
        animator.SetBool(IsAttackingPlayer, false);
    }

    void ConfigurarLineRenderer()
    {
        if (lineRendererTela == null)
        {
            GameObject telaObj = new GameObject("TelaArana");
            telaObj.transform.SetParent(transform);
            telaObj.transform.localPosition = Vector3.zero;
            lineRendererTela = telaObj.AddComponent<LineRenderer>();
        }

        lineRendererTela.startWidth = anchoTela;
        lineRendererTela.endWidth = anchoTela;
        lineRendererTela.material = new Material(Shader.Find("Sprites/Default"));
        lineRendererTela.startColor = colorTela;
        lineRendererTela.endColor = colorTela;
        lineRendererTela.enabled = false;
        lineRendererTela.numCapVertices = 4;
    }

    void Update()
    {
        DetectarJugador();
        MirarJugador();
        if (movimientoActivo) EjecutarMovimientoSalto();
    }

    void DetectarJugador()
    {
        if (estaMuerto) return;

        GameObject jugadorObj = GameObject.FindGameObjectWithTag(tagJugador);
        if (jugadorObj == null) return;

        jugador = jugadorObj.transform;
        float distancia = Vector2.Distance(transform.position, jugador.position);
        jugadorDetectado = distancia <= rangoDeteccion;

        animator.SetBool(IsDetectingPlayer, jugadorDetectado);
    }

    void MirarJugador()
    {
        if (jugador == null) return;

        Vector3 direccion = jugador.position - transform.position;
        transform.localScale = new Vector3(
            direccion.x > 0 ? Mathf.Abs(escalaOriginal.x) : -Mathf.Abs(escalaOriginal.x),
            escalaOriginal.y,
            escalaOriginal.z
        );
    }

    IEnumerator MovimientoAleatorio()
    {
        yield return new WaitForSeconds(1f);

        while (!estaMuerto)
        {
            if (!jugadorDetectado)
            {
                Transform puntoDestino = puntos[Random.Range(0, puntos.Length)];
                destinoActual = puntoDestino.position;
                yield return StartCoroutine(PrepararSalto());
                yield return StartCoroutine(Saltar(destinoActual));
            }
            else
            {
                contadorSaltos++;
                if (contadorSaltos >= saltosParaAtacar)
                {
                    contadorSaltos = 0;
                    yield return StartCoroutine(PrepararAtaque());
                    yield return StartCoroutine(Saltar(jugador.position, true));
                }
                else
                {
                    Transform puntoDestino = puntos[Random.Range(0, puntos.Length)];
                    destinoActual = puntoDestino.position;
                    yield return StartCoroutine(PrepararSalto());
                    yield return StartCoroutine(Saltar(destinoActual));
                }
            }

            yield return new WaitForSeconds(tiempoEntreSaltos);
        }
    }

    IEnumerator PrepararSalto()
    {
        animator.SetBool(IsJumpIdle, true);
        yield return new WaitForSeconds(tiempoPreparacionSalto);
        animator.SetBool(IsJumpIdle, false);
    }

    IEnumerator PrepararAtaque()
    {
        preparandoAtaque = true;
        animator.SetBool(IsAttacking, true);
        yield return new WaitForSeconds(tiempoPreparacionAtaque);
        animator.SetBool(IsAttacking, false);
        preparandoAtaque = false;
    }

    IEnumerator Saltar(Vector3 destino, bool atacarJugador = false)
    {
        movimientoActivo = true;
        esAtaqueAlJugador = atacarJugador;
        posicionInicialSalto = transform.position;
        duracionSaltoActual = Vector2.Distance(posicionInicialSalto, destino) / velocidadSalto;
        tiempoTranscurridoSalto = 0;
        alturaMaximaSalto = 1f + Vector2.Distance(posicionInicialSalto, destino) * 0.2f;

        animator.SetBool(IsJumping, true);

        // 🔊 AUDIO SALTO ATAQUE (CON PROXIMIDAD)
        if (esAtaqueAlJugador && telaranaSaltoClip && audioSource && PlayerEnRangoAudio())
            audioSource.PlayOneShot(telaranaSaltoClip, volSalto);

        StartCoroutine(MostrarTelaArana(destino));

        while (tiempoTranscurridoSalto < duracionSaltoActual)
        {
            tiempoTranscurridoSalto += Time.deltaTime;
            float t = tiempoTranscurridoSalto / duracionSaltoActual;
            Vector3 pos = Vector3.Lerp(posicionInicialSalto, destino, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * alturaMaximaSalto;
            transform.position = pos;
            yield return null;
        }

        animator.SetBool(IsJumping, false);
        animator.SetBool(IsLanding, true);
        yield return new WaitForSeconds(0.2f);
        animator.SetBool(IsLanding, false);

        if (esAtaqueAlJugador)
            StartCoroutine(AtacarJugador());

        movimientoActivo = false;
    }

    void EjecutarMovimientoSalto() { }

    IEnumerator AtacarJugador()
    {
        if (jugador == null) yield break;
        animator.SetBool(IsAttackingPlayer, true);
        yield return new WaitForSeconds(0.6f);
        animator.SetBool(IsAttackingPlayer, false);
    }

    IEnumerator MostrarTelaArana(Vector3 destino)
    {
        // 🔊 AUDIO TELARAÑA (CON PROXIMIDAD)
        if (telaranaLanzarClip && audioSource && PlayerEnRangoAudio())
            audioSource.PlayOneShot(telaranaLanzarClip, volLanzar);

        lineRendererTela.enabled = true;
        posicionesNudos.Clear();

        Vector3 inicio = transform.position;
        Vector3 fin = destino;

        List<Vector3> puntosTela = new List<Vector3>();
        for (int i = 0; i < puntosCurvaCentral; i++)
        {
            float t = i / (float)(puntosCurvaCentral - 1);
            Vector3 control = (inicio + fin) / 2 + Vector3.up * 1.2f;
            Vector3 punto = CalcularCurvaBezier(inicio, control, fin, t);
            puntosTela.Add(punto);

            if (i % 4 == 0) posicionesNudos.Add(punto);
        }

        List<List<Vector3>> abanicoInicio = GenerarTejidoFanHilos(inicio);
        List<List<Vector3>> abanicoFin = GenerarTejidoFanHilos(fin);

        List<Vector3> puntosFinales = new List<Vector3>();

        foreach (var hilo in abanicoInicio)
        {
            puntosFinales.AddRange(hilo);
            posicionesNudos.AddRange(hilo);
        }

        puntosFinales.AddRange(puntosTela);

        foreach (var hilo in abanicoFin)
        {
            puntosFinales.AddRange(hilo);
            posicionesNudos.AddRange(hilo);
        }

        lineRendererTela.positionCount = puntosFinales.Count;
        lineRendererTela.SetPositions(puntosFinales.ToArray());

        yield return new WaitForSeconds(tiempoMostrarTela);
        lineRendererTela.enabled = false;
    }

    List<List<Vector3>> GenerarTejidoFanHilos(Vector3 centro)
    {
        List<List<Vector3>> hilos = new List<List<Vector3>>();
        float halfAngle = fanAngle * 0.5f;

        for (int s = 0; s < ringSegments; s++)
        {
            float a = Mathf.Lerp(-halfAngle, halfAngle, s / (float)(ringSegments - 1));

            Vector3 dirD = Quaternion.AngleAxis(a, Vector3.forward) * Vector3.up;
            List<Vector3> hiloD = new List<Vector3>();
            for (int r = 1; r <= ringCount; r++)
                hiloD.Add(centro + dirD.normalized * ringSpacing * r * 1.5f);

            hilos.Add(hiloD);

            Vector3 dirI = Quaternion.AngleAxis(-a, Vector3.forward) * Vector3.up;
            List<Vector3> hiloI = new List<Vector3>();
            for (int r = 1; r <= ringCount; r++)
                hiloI.Add(centro + dirI.normalized * ringSpacing * r * 1.5f);

            hilos.Add(hiloI);
        }

        return hilos;
    }

    Vector3 CalcularCurvaBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        return (u * u * p0) + (2 * u * t * p1) + (t * t * p2);
    }

    void OnDrawGizmos()
    {
        if (!activarNudosVisuales || posicionesNudos.Count == 0) return;
        Gizmos.color = colorNudo;
        foreach (Vector3 nudo in posicionesNudos)
            Gizmos.DrawSphere(nudo, tamañoNudo);
    }
}
