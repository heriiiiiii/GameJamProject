using UnityEngine;

public class CA_ProyectilHilo : MonoBehaviour
{
    public int danio = 1;
    public LayerMask layerSuelo;

    [Header("Configuración Tentáculo")]
    public int segmentos = 14;
    public float longitudTentaculo = 3f;
    public float radioOnda = 0.6f;
    public float frecuenciaOnda = 6f;
    public float suavizado = 0.8f;

    [Header("Visual")]
    public float anchoBase = 0.2f;
    public float anchoFinal = 0.05f;
    public Color colorInicio = new Color(1f, 0.5f, 1f, 1f);
    public Color colorFinal = new Color(0.2f, 0f, 0.6f, 0.1f);
    public Material materialTentaculo;

    private Rigidbody2D rb;
    private LineRenderer lineRenderer;
    private float tiempo;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }

        // Configurar collider
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;
        col.radius = 0.4f;

        // Configurar LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        ConfigurarLineRenderer();
    }

    void ConfigurarLineRenderer()
    {
        lineRenderer.positionCount = segmentos;
        lineRenderer.startWidth = anchoBase;
        lineRenderer.endWidth = anchoFinal;
        lineRenderer.startColor = colorInicio;
        lineRenderer.endColor = colorFinal;

        if (materialTentaculo != null)
            lineRenderer.material = materialTentaculo;
        else
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.numCapVertices = 8; // redondear extremos
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        tiempo += Time.deltaTime;
        ActualizarTentaculo();

        // Autodestruir por seguridad
        if (tiempo > 6f)
            Destroy(gameObject);
    }

    void ActualizarTentaculo()
    {
        if (rb == null || lineRenderer == null) return;

        Vector3 origen = transform.position;
        Vector3 direccion = rb.velocity.normalized;
        Vector3 normal = Vector3.Cross(direccion, Vector3.forward);

        for (int i = 0; i < segmentos; i++)
        {
            float t = i / (float)(segmentos - 1);
            float distancia = t * longitudTentaculo;

            // posición base a lo largo de la dirección opuesta
            Vector3 basePos = origen - direccion * distancia;

            // crear movimiento ondulante circular
            float fase = (t * frecuenciaOnda + tiempo * 2f) * Mathf.PI * 2f;
            float radio = radioOnda * (1f - t * suavizado);

            // forma circular alrededor de la línea base
            Vector3 offset = normal * Mathf.Sin(fase) * radio + direccion * Mathf.Cos(fase) * (radio * 0.3f);

            lineRenderer.SetPosition(i, basePos + offset);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth hp = other.GetComponent<PlayerHealth>();
            if (hp != null)
                hp.RecibirDanio(danio);

            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & layerSuelo) != 0)
        {
            Destroy(gameObject);
        }
    }
}