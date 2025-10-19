using UnityEngine;

public class EnemigoCuadradoKnockback : MonoBehaviour
{
    [Header("Ruta del cuadrado")]
    public Transform[] waypoints;
    public float velocidad = 2f;

    [Header("Daño y knockback")]
    public int dano = 1;
    public float knockbackForce = 5f;

    private int indiceActual = 0;
    private Vector3 escalaOriginal;
    private bool piesEnPlataforma = true;

    void Start()
    {
        // Guardar la escala original para referencia
        escalaOriginal = transform.localScale;
    }

    void Update()
    {
        if (waypoints.Length == 0) return;

        Transform objetivo = waypoints[indiceActual];

        // Mover hacia el waypoint actual
        transform.position = Vector2.MoveTowards(transform.position, objetivo.position, velocidad * Time.deltaTime);

        RotarParaPlataforma(objetivo.position);

        // Cambio de waypoint
        if (Vector2.Distance(transform.position, objetivo.position) < 0.05f)
        {
            indiceActual = (indiceActual + 1) % waypoints.Length;
        }
    }

    void RotarParaPlataforma(Vector3 posicionObjetivo)
    {
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

            // Si estamos en la parte inferior del cubo
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

    void OnCollisionEnter2D(Collision2D collision)
    {
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
        Gizmos.color = Color.red;
        Vector3 direccionPies = piesEnPlataforma ? Vector3.down : Vector3.up;
        Gizmos.DrawLine(transform.position, transform.position + direccionPies * 0.5f);
    }
}