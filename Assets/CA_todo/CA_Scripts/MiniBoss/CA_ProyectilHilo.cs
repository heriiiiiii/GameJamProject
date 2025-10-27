using UnityEngine;

public class CA_ProyectilHilo : MonoBehaviour
{
    public int danio = 1;
    public LayerMask layerSuelo;

    [Header("Configuración Sierra")]
    public float amplitudSierra = 0.8f;
    public float frecuenciaSierra = 4f;
    public float longitudHilo = 3f;
    public float anchoHilo = 0.1f;
    public Material materialHilo;
    public Color colorHilo = new Color(1f, 1f, 1f, 0.8f);

    private LineRenderer lineRenderer;
    private Rigidbody2D rb;
    private float tiempoVida = 0f;

    void Start()
    {
        // Configurar collider
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            collider.radius = 0.5f;
        }
        else
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f;
        }

        // Configurar Rigidbody
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }

        // Configurar LineRenderer para el efecto sierra
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        ConfigurarLineRenderer();

        // Configurar puntos iniciales del hilo sierra
        lineRenderer.positionCount = 12;
        ActualizarFormaSierra();
    }

    void ConfigurarLineRenderer()
    {
        lineRenderer.startWidth = anchoHilo;
        lineRenderer.endWidth = anchoHilo;

        if (materialHilo != null)
        {
            lineRenderer.material = materialHilo;
        }
        else
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        lineRenderer.startColor = colorHilo;
        lineRenderer.endColor = colorHilo;
        lineRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        if (lineRenderer != null)
        {
            tiempoVida += Time.deltaTime;
            ActualizarFormaSierra();
        }

        // Destruir después de 5 segundos por seguridad
        if (tiempoVida > 5f)
        {
            Destroy(gameObject);
        }
    }

    void ActualizarFormaSierra()
    {
        if (lineRenderer == null || rb == null) return;

        Vector3 posicionActual = transform.position;
        Vector3 direccionMovimiento = rb.velocity.normalized;
        Vector3 normal = Vector3.Cross(direccionMovimiento, Vector3.forward).normalized;

        // Crear forma de sierra con múltiples puntos
        for (int i = 0; i < 12; i++)
        {
            float t = i / 11f; // 0 a 1
            float distancia = t * longitudHilo;

            // Calcular punto base en la dirección del movimiento
            Vector3 puntoBase = posicionActual - direccionMovimiento * distancia;

            // Añadir onda de sierra - más pronunciada cerca del final
            float amplitudActual = amplitudSierra * (0.5f + t * 0.5f); // Creciente
            float fase = t * frecuenciaSierra * Mathf.PI * 2f + tiempoVida * 8f;
            float onda = Mathf.Sin(fase) * amplitudActual;

            // Aplicar la onda perpendicular a la dirección
            Vector3 puntoFinal = puntoBase + normal * onda;

            lineRenderer.SetPosition(i, puntoFinal);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // SIEMPRE destruir al impactar con Player
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.RecibirDanio(danio);
                Debug.Log($"¡Hilo Sierra impactó! {danio} de daño al jugador");
            }
            Destroy(gameObject);
            return;
        }

        // También destruir al impactar con suelo
        if (((1 << other.gameObject.layer) & layerSuelo) != 0)
        {
            Debug.Log("Hilo Sierra impactó con el suelo");
            Destroy(gameObject);
        }
    }

    // Método para configurar desde el boss si es necesario
    public void ConfigurarSierra(float nuevaAmplitud, float nuevaFrecuencia, float nuevaLongitud)
    {
        amplitudSierra = nuevaAmplitud;
        frecuenciaSierra = nuevaFrecuencia;
        longitudHilo = nuevaLongitud;
    }
}