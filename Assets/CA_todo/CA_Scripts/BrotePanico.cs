using System.Collections;
using UnityEngine;

public class BrotePanico : MonoBehaviour
{
    [Header("Detección del jugador")]
    public float rangoDeteccion = 5f;
    public string tagJugador = "Player";

    [Header("Disparo")]
    public GameObject prefabEspina;
    public float tiempoEntreDisparos = 3f;
    public Transform puntoDisparo;

    [Header("Daño por contacto")]
    public int danoContacto = 1;
    public float knockbackForce = 6f;

    // Componentes
    private Animator animator;
    public bool jugadorDetectado = false;
    private float temporizador;
    private Transform jugador;

    // Parámetros Animator
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int PlayerDetected = Animator.StringToHash("PlayerDetected");
    private static readonly int IsDead = Animator.StringToHash("IsDead");

    void Start()
    {
        animator = GetComponent<Animator>();
        temporizador = tiempoEntreDisparos;

        animator.SetBool(PlayerDetected, false);
        animator.SetBool(IsAttacking, false);
        //animator.SetBool(IsMoving, false);
        animator.SetBool(IsDead, false);
    }

    void Update()
    {
        if (animator.GetBool(IsDead)) return;

        bool deteccionAnterior = jugadorDetectado;
        DetectarJugador();

        if (deteccionAnterior != jugadorDetectado)
            animator.SetBool(PlayerDetected, jugadorDetectado);

        if (jugadorDetectado)
        {
            temporizador -= Time.deltaTime;
            if (temporizador <= 0f)
            {
                animator.SetBool(IsAttacking, true);
                temporizador = tiempoEntreDisparos;
            }
        }
        else
        {
            animator.SetBool(IsAttacking, false);
        }
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

    // ✅ AHORA este método será llamado desde el Behaviour (CA_ataque)
    public void Disparar()
    {
        if (prefabEspina == null || jugador == null) return;

        Vector2 direccionBase = (jugador.position - transform.position).normalized;
        float[] angulos = { -25f, 0f, 25f };

        foreach (float ang in angulos)
        {
            Quaternion rot = Quaternion.Euler(0, 0, ang);
            Vector2 dir = rot * direccionBase;
            GameObject bala = Instantiate(prefabEspina, puntoDisparo.position, Quaternion.identity);
            bala.GetComponent<EspinaBrote>().Inicializar(dir);
        }
    }

    public void Morir()
    {
        animator.SetBool(IsDead, true);
        GetComponent<Collider2D>().enabled = false;
        enabled = false;
        Destroy(gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(tagJugador) && !animator.GetBool(IsDead))
        {
            PlayerHealth salud = collision.gameObject.GetComponent<PlayerHealth>();
            if (salud != null) salud.RecibirDanio(danoContacto);

            Rigidbody2D rbPlayer = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rbPlayer != null)
            {
                Vector2 direccion = (collision.transform.position - transform.position).normalized;
                rbPlayer.AddForce(direccion * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
}
