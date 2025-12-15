using UnityEngine;

public class CA_EspadaSuelo : MonoBehaviour
{
    [Header("Configuración")]
    public float alturaMaxima = 3f;
    public float velocidadSubida = 2f;
    public float velocidadOscilacion = 2f;
    public float amplitudOscilacion = 1.5f;
    public float dano = 2f;
    public float duracion = 4f;

    [Header("Efectos")]
    public GameObject efectoImpacto;

    // Variables privadas
    private Vector3 posicionInicial;
    private Vector3 posicionObjetivo;
    private float tiempoInicio;
    private float desfaseOscilacion;
    private GameObject duenio;
    private bool haSubido = false;
    private bool activa = false;
    private bool haImpactado = false;

    private SpriteRenderer spriteRenderer;
    private Collider2D colisionador;

    void Start()
    {
        tiempoInicio = Time.time;
        posicionInicial = transform.position;
        posicionObjetivo = new Vector3(
            posicionInicial.x,
            posicionInicial.y + alturaMaxima,
            posicionInicial.z
        );

        spriteRenderer = GetComponent<SpriteRenderer>();
        colisionador = GetComponent<Collider2D>();

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            // Crear sprite de espada simple
            Texture2D tex = new Texture2D(32, 64);
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    if (x >= 12 && x <= 20 && y < 50) // Hoja de la espada
                        tex.SetPixel(x, y, Color.gray);
                    else if (x >= 14 && x <= 18 && y >= 50) // Empuñadura
                        tex.SetPixel(x, y, Color.red);
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();

            Sprite espadaSprite = Sprite.Create(tex, new Rect(0, 0, 32, 64), new Vector2(0.5f, 0f));
            spriteRenderer.sprite = espadaSprite;
            spriteRenderer.sortingOrder = 5;
        }

        if (colisionador == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.3f, 0.8f);
            collider.offset = new Vector2(0, 0.4f);
        }

        // Destruir automáticamente después de la duración
        Destroy(gameObject, duracion + 0.5f);
    }

    public void Configurar(float altura, float velocidadSub, float velocidadOsc,
                          float amplitud, float desfase, float dmg, float dur, GameObject owner)
    {
        alturaMaxima = altura;
        velocidadSubida = velocidadSub;
        velocidadOscilacion = velocidadOsc;
        amplitudOscilacion = amplitud;
        desfaseOscilacion = desfase;
        dano = dmg;
        duracion = dur;
        duenio = owner;

        posicionObjetivo = new Vector3(
            transform.position.x,
            transform.position.y + alturaMaxima,
            transform.position.z
        );
    }

    void Update()
    {
        if (haImpactado) return;

        float tiempoTranscurrido = Time.time - tiempoInicio;

        // Fase 1: Subir desde el suelo
        if (!haSubido)
        {
            float progresoSubida = Mathf.Clamp01(tiempoTranscurrido * velocidadSubida);
            transform.position = Vector3.Lerp(posicionInicial, posicionObjetivo, progresoSubida);

            // Cuando llega arriba, activar
            if (progresoSubida >= 1f)
            {
                haSubido = true;
                activa = true;

                // Hacer que el collider sea más grande cuando está activa
                if (colisionador is BoxCollider2D boxCollider)
                {
                    boxCollider.size = new Vector2(0.5f, 1f);
                }
            }
        }
        // Fase 2: Oscilar como una aguja de medidor
        else if (activa && tiempoTranscurrido < duracion)
        {
            // Calcular oscilación (movimiento de izquierda a derecha como aguja)
            float oscilacion = Mathf.Sin((tiempoTranscurrido * velocidadOscilacion) + desfaseOscilacion) * amplitudOscilacion;

            // Posición base en Y (ya está arriba)
            float posY = posicionObjetivo.y;

            // Oscilación en X
            float posX = posicionInicial.x + oscilacion;

            transform.position = new Vector3(posX, posY, transform.position.z);

            // Rotar ligeramente según la posición para efecto de "aguja"
            float anguloRotacion = Mathf.Clamp(oscilacion * 5f, -30f, 30f);
            transform.rotation = Quaternion.Euler(0, 0, anguloRotacion);

            // Efecto visual: brillo cuando está en los extremos
            if (Mathf.Abs(oscilacion) > amplitudOscilacion * 0.8f && spriteRenderer != null)
            {
                float intensidad = 0.7f + Mathf.Abs(Mathf.Sin(Time.time * 10f)) * 0.3f;
                spriteRenderer.color = new Color(1f, 1f, 0.5f, intensidad);
            }
        }
        // Fase 3: Bajar y destruirse
        else if (tiempoTranscurrido >= duracion)
        {
            float tiempoRestante = tiempoTranscurrido - duracion;
            float progresoBajada = Mathf.Clamp01(tiempoRestante * velocidadSubida * 0.5f);
            transform.position = Vector3.Lerp(posicionObjetivo, posicionInicial, progresoBajada);

            if (progresoBajada >= 1f)
            {
                CrearEfectoImpacto();
                Destroy(gameObject);
            }
        }
    }

    void CrearEfectoImpacto()
    {
        if (efectoImpacto != null)
        {
            GameObject efecto = Instantiate(efectoImpacto, transform.position, Quaternion.identity);
            Destroy(efecto, 1f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (haImpactado) return;
        if (duenio != null && other.gameObject == duenio) return;
        if (!activa) return;

        if (other.CompareTag("Player"))
        {
            NF_PlayerHealth salud = other.GetComponent<NF_PlayerHealth>();
            if (salud != null)
            {
                salud.TakeDamageWithoutKnockback((int)dano);

                // Empujar al jugador en dirección de la oscilación
                Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 direccionEmpuje = new Vector2(
                        Mathf.Sign(transform.position.x - posicionInicial.x),
                        0.5f
                    ).normalized;
                    rb.AddForce(direccionEmpuje * 5f, ForceMode2D.Impulse);
                }

                haImpactado = true;
                CrearEfectoImpacto();

                // Desactivar collider
                if (colisionador != null)
                    colisionador.enabled = false;

                // Hacer invisible
                if (spriteRenderer != null)
                    spriteRenderer.enabled = false;

                Destroy(gameObject, 0.1f);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Mostrar área de oscilación
        Gizmos.color = Color.yellow;
        Vector3 centro = transform.position;
        Gizmos.DrawLine(
            new Vector3(centro.x - amplitudOscilacion, centro.y + alturaMaxima, centro.z),
            new Vector3(centro.x + amplitudOscilacion, centro.y + alturaMaxima, centro.z)
        );

        // Mostrar altura máxima
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            new Vector3(centro.x, centro.y + alturaMaxima, centro.z),
            new Vector3(0.5f, 0.1f, 0)
        );
    }
}