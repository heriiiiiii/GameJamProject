using System.Collections;
using UnityEngine;

public class Esporantula : MonoBehaviour
{
    [Header("Puntos de salto")]
    public Transform[] puntos;
    public float velocidadSalto = 8f;
    public float tiempoEntreSaltos = 1f;

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

    private bool saltando = false;
    private Transform jugador;
    private bool jugadorDetectado = false;
    private int contadorSaltos = 0;
    private Vector3 destinoActual;
    private Vector3 escalaOriginal;  //  GUARDAR ESCALA ORIGINAL

    void Start()
    {
        escalaOriginal = transform.localScale;
        ConfigurarLineRenderer();
        StartCoroutine(MovimientoAleatorio());
    }

    void ConfigurarLineRenderer()
    {
        // Si no hay LineRenderer asignado, crear uno en un objeto separado
        if (lineRendererTela == null)
        {
            GameObject telaObj = new GameObject("TelaArana");
            telaObj.transform.SetParent(transform);
            telaObj.transform.localPosition = Vector3.zero;
            lineRendererTela = telaObj.AddComponent<LineRenderer>();
        }

        // Configurar todas las propiedades del LineRenderer
        lineRendererTela.startWidth = anchoTela;
        lineRendererTela.endWidth = anchoTela;
        lineRendererTela.material = new Material(Shader.Find("Sprites/Default"));
        lineRendererTela.startColor = colorTela;
        lineRendererTela.endColor = colorTela;
        lineRendererTela.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        lineRendererTela.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRendererTela.receiveShadows = false;
        lineRendererTela.useWorldSpace = true;

        // Posiciones iniciales
        lineRendererTela.positionCount = 2;
        lineRendererTela.SetPosition(0, transform.position);
        lineRendererTela.SetPosition(1, transform.position);
        lineRendererTela.enabled = false;
    }

    void Update()
    {
        DetectarJugador();
        MirarJugador();
    }

    void DetectarJugador()
    {
        jugadorDetectado = false;
        Collider2D[] colisiones = Physics2D.OverlapCircleAll(transform.position, rangoDeteccion);
        foreach (Collider2D col in colisiones)
        {
            if (col.CompareTag(tagJugador))
            {
                jugadorDetectado = true;
                jugador = col.transform;
                break;
            }
        }
    }

    void MirarJugador()
    {
        if (jugador != null)
        {
            if (jugador.position.x < transform.position.x)
                transform.localScale = new Vector3(-escalaOriginal.x, escalaOriginal.y, escalaOriginal.z);
            else
                transform.localScale = new Vector3(escalaOriginal.x, escalaOriginal.y, escalaOriginal.z);
        }
        else
        {
            transform.localScale = escalaOriginal;
        }
    }

    IEnumerator MovimientoAleatorio()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (!saltando && puntos.Length > 0)
            {
                Transform destino;

                if (jugadorDetectado && contadorSaltos >= saltosParaAtacar && jugador != null)
                {
                    destino = jugador;
                    contadorSaltos = 0;
                    Debug.Log("¡Atacando al jugador!");
                }
                else
                {
                    destino = ObtenerPuntoAleatorio();
                    contadorSaltos++;
                    Debug.Log("Saltando a punto: " + destino.name);
                }

                saltando = true;
                destinoActual = destino.position;

                // Mostrar tela de araña antes de saltar
                yield return StartCoroutine(MostrarTelaArana(destinoActual));

                // Realizar el salto
                yield return StartCoroutine(SaltarHaciaDestino(destinoActual));

                saltando = false;
                yield return new WaitForSeconds(tiempoEntreSaltos);
            }
            else
            {
                yield return null;
            }
        }
    }

    IEnumerator MostrarTelaArana(Vector3 destino)
    {
        // Mostrar la tela
        lineRendererTela.enabled = true;

        // Crear puntos para la curva de la tela
        Vector3 puntoInicio = transform.position;
        Vector3 puntoFinal = destino;
        Vector3 puntoControl = (puntoInicio + puntoFinal) / 2 + Vector3.up * 1.5f;

        // Configurar más puntos para una curva suave
        lineRendererTela.positionCount = 15;

        for (int i = 0; i < 15; i++)
        {
            float t = i / 14f;
            Vector3 puntoCurva = CalcularCurvaBezier(puntoInicio, puntoControl, puntoFinal, t);
            lineRendererTela.SetPosition(i, puntoCurva);
        }

        // Efecto de aparición suave
        float tiempo = 0f;
        while (tiempo < tiempoMostrarTela)
        {
            tiempo += Time.deltaTime;
            yield return null;
        }

        // Ocultar tela
        lineRendererTela.enabled = false;
    }

    Vector3 CalcularCurvaBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        float uu = u * u;
        float tt = t * t;

        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;

        return p;
    }

    IEnumerator SaltarHaciaDestino(Vector3 destino)
    {
        Vector3 posicionInicial = transform.position;
        float distanciaTotal = Vector3.Distance(posicionInicial, destino);
        float duracionSalto = distanciaTotal / velocidadSalto;
        float tiempoTranscurrido = 0f;

        float alturaMaxima = Mathf.Min(distanciaTotal * 0.3f, 2f);

        while (tiempoTranscurrido < duracionSalto)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / duracionSalto;

            Vector3 posicionHorizontal = Vector3.Lerp(posicionInicial, destino, progreso);
            float altura = Mathf.Sin(progreso * Mathf.PI) * alturaMaxima;

            transform.position = new Vector3(posicionHorizontal.x, posicionHorizontal.y + altura, posicionHorizontal.z);

            yield return null;
        }

        transform.position = destino;
    }

    Transform ObtenerPuntoAleatorio()
    {
        if (puntos.Length == 0) return null;

        Transform puntoSeleccionado;
        int intentos = 0;

        do
        {
            puntoSeleccionado = puntos[Random.Range(0, puntos.Length)];
            intentos++;

            if (intentos >= 5) break;

        } while (Vector3.Distance(transform.position, puntoSeleccionado.position) < 0.5f);

        return puntoSeleccionado;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(tagJugador))
        {
            PlayerHealth salud = collision.gameObject.GetComponent<PlayerHealth>();
            if (salud != null) salud.RecibirDanio(danoMordisco);

            Rigidbody2D rbPlayer = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rbPlayer != null)
            {
                Vector2 direccion = (collision.transform.position - transform.position).normalized;
                rbPlayer.AddForce(direccion * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }
}