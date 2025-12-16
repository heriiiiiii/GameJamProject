using UnityEngine;
using System.Collections;

public class CA_FlorParasita : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 3f;
    public float radioExplosion = 1.5f;
    public float fuerzaEmpuje = 15f;
    public ParticleSystem particulasExplosion;
    public Sprite spriteFlor;

    [Header("Sacudida de Cámara")]
    public float intensidadSacudida = 0.5f;
    public float duracionSacudida = 0.3f;

    private Transform objetivo;
    private int danio;
    private float duracionAturdimiento;
    private bool adherente = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        CrearEfectoVisual();
    }

    public void Inicializar(Transform target, int damage, float stunDuration)
    {
        objetivo = target;
        danio = damage;
        duracionAturdimiento = stunDuration;
    }

    void Update()
    {
        if (objetivo == null)
        {
            Destroy(gameObject);
            return;
        }

        if (!adherente)
        {
            Vector3 direccion = (objetivo.position - transform.position).normalized;
            transform.position += direccion * velocidad * Time.deltaTime;

            if (direccion != Vector3.zero)
            {
                float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angulo - 90f);
            }

            if (Vector3.Distance(transform.position, objetivo.position) < 0.5f)
            {
                Adherirse();
            }
        }
        else
        {
            transform.position = objetivo.position;
            transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
        }
    }

    void CrearEfectoVisual()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        if (spriteFlor != null)
        {
            spriteRenderer.sprite = spriteFlor;
        }
        else
        {
            spriteRenderer.color = Color.red;
        }

        spriteRenderer.sortingOrder = 10;

        CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.3f;
    }

    void Adherirse()
    {
        adherente = true;
        Debug.Log("🌺 Flor se adhiere al jugador!");

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow;
        }

        transform.position = new Vector3(objetivo.position.x, objetivo.position.y, -1f);

        Invoke("Explotar", 2f);
    }

    void Explotar()
    {
        Debug.Log("💥 Flor explota!");

        // Aplicar sacudida de cámara
        AplicarSacudidaCamara();

        AplicarExplosion();

        if (particulasExplosion != null)
        {
            ParticleSystem explosion = Instantiate(particulasExplosion, transform.position, Quaternion.identity);
            explosion.Play();
            Destroy(explosion.gameObject, 2f);
        }

        Destroy(gameObject);
    }

    void AplicarSacudidaCamara()
    {
        Camera camaraPrincipal = Camera.main;
        if (camaraPrincipal != null)
        {
            // Buscar o agregar el script de sacudida a la cámara
            CA_SacudidaCamara sacudidaScript = camaraPrincipal.GetComponent<CA_SacudidaCamara>();
            if (sacudidaScript == null)
            {
                sacudidaScript = camaraPrincipal.gameObject.AddComponent<CA_SacudidaCamara>();
            }

            // Iniciar la sacudida
            sacudidaScript.IniciarSacudida(intensidadSacudida, duracionSacudida);
            Debug.Log("📷 Aplicando sacudida de cámara");
        }
    }

    void AplicarExplosion()
    {
        PlayerHealth playerHealth = objetivo.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.RecibirDanio(danio);
            Debug.Log($"💔 Jugador recibe {danio} de daño");
        }

        AplicarEmpujeJugador();

        CA_EfectoVisionPlayer efectoVision = objetivo.GetComponent<CA_EfectoVisionPlayer>();
        if (efectoVision != null)
        {
            efectoVision.AplicarAturdimiento(duracionAturdimiento);
            Debug.Log($"🌀 Aplicando aturdimiento: {duracionAturdimiento}s");
        }

        EmpujarObjetosCercanos();
    }

    void AplicarEmpujeJugador()
    {
        Rigidbody2D rb = objetivo.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direccionEmpuje = (objetivo.position - transform.position).normalized;

            if (direccionEmpuje.magnitude < 0.1f)
            {
                direccionEmpuje = Vector2.up;
            }

            Debug.Log($"💨 Aplicando empuje: dirección {direccionEmpuje}, fuerza {fuerzaEmpuje}");

            rb.velocity = Vector2.zero;
            rb.AddForce(direccionEmpuje * fuerzaEmpuje, ForceMode2D.Impulse);
            rb.AddForce(Vector2.up * fuerzaEmpuje * 0.3f, ForceMode2D.Impulse);
        }
    }

    void EmpujarObjetosCercanos()
    {
        Collider2D[] objetosCercanos = Physics2D.OverlapCircleAll(transform.position, radioExplosion);
        foreach (Collider2D col in objetosCercanos)
        {
            if (col.CompareTag("Enemy") || col.CompareTag("Destructible"))
            {
                Rigidbody2D rbObjeto = col.GetComponent<Rigidbody2D>();
                if (rbObjeto != null)
                {
                    Vector3 dirObjeto = (col.transform.position - transform.position).normalized;
                    rbObjeto.AddForce(dirObjeto * fuerzaEmpuje * 0.7f, ForceMode2D.Impulse);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioExplosion);
    }
}