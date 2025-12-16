using UnityEngine;

public class CA_ProyectilNoxarSimple : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 4f;
    public float dano = 1f;
    public float tiempoVida = 5f;
    public float tamanoMeteorito = 1f;

    [Header("Layers para Colisiones")]
    public LayerMask layerParedes; // Asigna el layer de paredes en el Inspector
    public LayerMask layerSuelo;   // Asigna el layer de suelo en el Inspector

    [Header("Efectos")]
    public GameObject efectoImpacto;

    private Vector3 direccion;
    private GameObject duenio;
    private bool esTorbellino = false;
    private bool esAnillo = false;
    private bool esMeteorito = false;
    private Vector3 objetivoMeteorito;
    private float anguloInicial;
    private bool haImpactado = false;
    private float tiempoCreacion;

    void Start()
    {
        tiempoCreacion = Time.time;

        // Asegurar componentes básicos
        if (GetComponent<SpriteRenderer>() == null)
        {
            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.color = Color.red;
            renderer.sortingOrder = 10;
        }

        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.2f;
        }

        // Destruir automáticamente
        Destroy(gameObject, tiempoVida);
    }

    public void Configurar(Vector3 dir, float vel, float dmg, GameObject owner)
    {
        direccion = dir.normalized;
        velocidad = vel;
        dano = dmg;
        duenio = owner;
        esTorbellino = false;
        esAnillo = false;
        esMeteorito = false;

        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.color = Color.red;
    }

    public void ConfigurarTorbellino(GameObject owner, float dmg, float anguloInicial)
    {
        duenio = owner;
        dano = dmg;
        esTorbellino = true;
        this.anguloInicial = anguloInicial;
        tiempoVida = 10f;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.color = Color.yellow;
    }

    public void ConfigurarAnillo(GameObject owner, float dmg, float anguloInicial)
    {
        duenio = owner;
        dano = dmg;
        esAnillo = true;
        this.anguloInicial = anguloInicial;
        tiempoVida = 8f;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.color = Color.cyan;
    }

    public void ConfigurarMeteorito(Vector3 objetivo, float dmg, GameObject owner)
    {
        objetivoMeteorito = objetivo;
        dano = dmg;
        duenio = owner;
        esMeteorito = true;
        velocidad = 8f;
        tiempoVida = 4f;

        // Calcular dirección hacia abajo
        direccion = (objetivoMeteorito - transform.position).normalized;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = new Color(1f, 0.4f, 0.1f);
            if (tamanoMeteorito != 1f)
            {
                transform.localScale = Vector3.one * tamanoMeteorito;
            }
        }
    }

    public void SetTamanoMeteorito(float tamano)
    {
        tamanoMeteorito = tamano;
        if (esMeteorito)
        {
            transform.localScale = Vector3.one * tamanoMeteorito;
        }
    }

    void Update()
    {
        if (haImpactado) return;

        if (esMeteorito)
        {
            float distancia = Vector3.Distance(transform.position, objetivoMeteorito);
            float velocidadActual = velocidad * (1f + (1f / Mathf.Max(distancia, 1f)));

            transform.position = Vector3.MoveTowards(transform.position,
                objetivoMeteorito, velocidadActual * Time.deltaTime);

            // Rotación
            transform.Rotate(0, 0, 180f * Time.deltaTime);

            // Efecto de parpadeo cerca del impacto
            if (distancia < 2f)
            {
                SpriteRenderer renderer = GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    float alpha = 0.7f + Mathf.Sin(Time.time * 15f) * 0.3f;
                    renderer.color = new Color(1f, 0.4f, 0.1f, alpha);
                }
            }

            // Verificar impacto
            if (distancia < 0.2f)
            {
                Impactar();
            }
        }
        else if (!esTorbellino && !esAnillo)
        {
            transform.position += direccion * velocidad * Time.deltaTime;
        }
    }

    void Impactar()
    {
        if (haImpactado) return;
        haImpactado = true;

        CrearEfectoImpacto();

        if (esMeteorito)
        {
            Collider2D[] objetivos = Physics2D.OverlapCircleAll(transform.position,
                Mathf.Max(0.5f, tamanoMeteorito * 0.5f));
            foreach (Collider2D objetivo in objetivos)
            {
                if (objetivo.CompareTag("Player"))
                {
                    NF_PlayerHealth salud = objetivo.GetComponent<NF_PlayerHealth>();
                    if (salud != null)
                    {
                        salud.TakeDamageWithoutKnockback((int)dano);

                        Rigidbody2D rb = objetivo.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            Vector2 direccionEmpuje = (objetivo.transform.position - transform.position).normalized;
                            rb.AddForce(direccionEmpuje * 3f, ForceMode2D.Impulse);
                        }
                    }
                }
            }
        }

        Destroy(gameObject, 0.1f);
    }

    void CrearEfectoImpacto()
    {
        if (efectoImpacto != null)
        {
            GameObject efecto = Instantiate(efectoImpacto, transform.position, Quaternion.identity);
            float escalaEfecto = transform.localScale.x * 1.2f;
            efecto.transform.localScale = Vector3.one * escalaEfecto;
            Destroy(efecto, 1f);
        }
        else
        {
            GameObject efectoSimple = new GameObject("EfectoImpactoSimple");
            efectoSimple.transform.position = transform.position;

            SpriteRenderer efectoRenderer = efectoSimple.AddComponent<SpriteRenderer>();
            efectoRenderer.color = new Color(1f, 0.6f, 0.2f, 0.8f);
            efectoRenderer.sortingOrder = 9;

            float escalaEfecto = transform.localScale.x;
            efectoRenderer.transform.localScale = Vector3.one * escalaEfecto * 1.5f;

            Destroy(efectoSimple, 0.3f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (haImpactado) return;
        if (duenio != null && other.gameObject == duenio) return;
        if (esAnillo) return;

        if (other.CompareTag("Player"))
        {
            NF_PlayerHealth salud = other.GetComponent<NF_PlayerHealth>();
            if (salud != null)
            {
                salud.TakeDamageWithoutKnockback((int)dano);

                if (esMeteorito)
                {
                    Impactar();
                }
                else
                {
                    CrearEfectoImpacto();
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            // Obtener el layer del objeto colisionado
            int layerObjeto = other.gameObject.layer;

            // Verificar si el objeto está en el layer de paredes o suelo
            // Compara usando máscaras de bits
            if (((1 << layerObjeto) & layerParedes) != 0 ||
                ((1 << layerObjeto) & layerSuelo) != 0)
            {
                if (esMeteorito)
                    Impactar();
                else
                {
                    CrearEfectoImpacto();
                    Destroy(gameObject);
                }
            }
        }
    }

    void OnBecameInvisible()
    {
        if (!esTorbellino && !esAnillo && !esMeteorito)
            Destroy(gameObject);
    }
}