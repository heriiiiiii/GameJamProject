using UnityEngine;

public class EnemigoCuadradoKnockback : MonoBehaviour
{
    [Header("🔊 Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deathClip;


    [Header("Ruta del cuadrado")]
    public Transform[] waypoints;
    public float velocidad = 2f;

    [Header("Daño y knockback")]
    public int dano = 1;
    public float knockbackForce = 5f;

    // Componentes
    private Animator animator;
    private CA_RecolEnemy sistemaVida;
    private int indiceActual = 0;
    private Vector3 escalaOriginal;
    private bool piesEnPlataforma = true;
    private bool estaMuerto = false;

    // Parámetros Animator
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsDead = Animator.StringToHash("IsDead");

    void Start()
    {
        animator = GetComponent<Animator>();
        sistemaVida = GetComponent<CA_RecolEnemy>();
        escalaOriginal = transform.localScale;

        // Estado inicial - vivo y caminando
        animator.SetBool(IsWalking, true);
        animator.SetBool(IsDead, false);
        estaMuerto = false;
    }

    void Update()
    {
        // Verificar muerte
        if (!estaMuerto && sistemaVida != null && sistemaVida.GetHealth() <= 0)
        {
            Morir();
            return;
        }

        if (estaMuerto) return;

        if (waypoints.Length == 0) return;

        Transform objetivo = waypoints[indiceActual];

        // Mover hacia el waypoint actual
        transform.position = Vector2.MoveTowards(transform.position, objetivo.position, velocidad * Time.deltaTime);

        // VOLTEAR SEGÚN LA DIRECCIÓN DEL MOVIMIENTO
        VoltearHaciaDireccion(objetivo.position);

        RotarParaPlataforma(objetivo.position);

        // Cambio de waypoint
        if (Vector2.Distance(transform.position, objetivo.position) < 0.05f)
        {
            indiceActual = (indiceActual + 1) % waypoints.Length;
        }
    }

    void VoltearHaciaDireccion(Vector3 posicionObjetivo)
    {
        if (estaMuerto) return;

        // Calcular dirección horizontal
        float direccionX = posicionObjetivo.x - transform.position.x;

        // Voltear el sprite según la dirección
        if (direccionX > 0)
        {
            // Mirando hacia la derecha
            transform.localScale = new Vector3(Mathf.Abs(escalaOriginal.x), escalaOriginal.y, escalaOriginal.z);
        }
        else if (direccionX < 0)
        {
            // Mirando hacia la izquierda
            transform.localScale = new Vector3(-Mathf.Abs(escalaOriginal.x), escalaOriginal.y, escalaOriginal.z);
        }
        // Si direccionX == 0, mantener la escala actual
    }

    void RotarParaPlataforma(Vector3 posicionObjetivo)
    {
        if (estaMuerto) return;

        Vector2 direccion = (posicionObjetivo - transform.position).normalized;

        bool moviendoseVerticalmente = Mathf.Abs(direccion.y) > Mathf.Abs(direccion.x);

        if (moviendoseVerticalmente)
        {
            if (direccion.y > 0)
            {
                transform.rotation = Quaternion.identity;
                piesEnPlataforma = true;
            }
            else if (direccion.y < 0)
            {
                transform.rotation = Quaternion.Euler(0, 0, 180f);
                piesEnPlataforma = false;
            }
        }
        else
        {
            MantenerRotacionHorizontal();
        }
    }

    void MantenerRotacionHorizontal()
    {
        if (indiceActual > 0)
        {
            Transform waypointAnterior = waypoints[indiceActual - 1];
            Transform waypointActual = waypoints[indiceActual];

            if (waypointAnterior.position.y < transform.position.y && waypointActual.position.y < transform.position.y)
            {
                transform.rotation = Quaternion.Euler(0, 0, 180f);
                piesEnPlataforma = false;
            }
            else
            {
                transform.rotation = Quaternion.identity;
                piesEnPlataforma = true;
            }
        }
    }

    void Morir()
    {
        // ✅ Reproducir sonido de muerte
        if (audioSource != null && deathClip != null)
            audioSource.PlayOneShot(deathClip, 0.9f);
        estaMuerto = true;
        animator.SetBool(IsWalking, false);
        animator.SetBool(IsDead, true);



        // Desactivar movimiento y colisiones
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = false;

        // (Opcional) Si quieres que desaparezca después
        Destroy(gameObject, 1.2f); // ajusta el tiempo según la animación
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (estaMuerto) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rbPlayer = collision.gameObject.GetComponent<Rigidbody2D>();

            if (rbPlayer != null)
            {
                Vector2 direccionKnockback = (collision.transform.position - transform.position).normalized;

                if (!piesEnPlataforma)
                {
                    direccionKnockback.y *= -1;
                }

                rbPlayer.velocity = Vector2.zero;
                rbPlayer.AddForce(direccionKnockback * knockbackForce, ForceMode2D.Impulse);
            }

            PlayerHealth salud = collision.gameObject.GetComponent<PlayerHealth>();
            if (salud != null)
            {
                salud.RecibirDanio(dano);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (estaMuerto) return;

        Gizmos.color = Color.red;
        Vector3 direccionPies = piesEnPlataforma ? Vector3.down : Vector3.up;
        Gizmos.DrawLine(transform.position, transform.position + direccionPies * 0.5f);

        // DEBUG: Mostrar dirección del próximo waypoint
        if (waypoints.Length > 0 && indiceActual < waypoints.Length)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, waypoints[indiceActual].position);
        }
    }
}