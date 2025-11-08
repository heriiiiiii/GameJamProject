using UnityEngine;

public class CA_ProyectilMiniBoss : MonoBehaviour
{
    [Header("Parámetros del proyectil")]
    public float velocidad = 6f;
    public int dano = 1;
    public float duracion = 4f;

    private Rigidbody2D rb;
    private Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null)
        {
            Vector2 direccion = (player.position - transform.position).normalized;
            rb.velocity = direccion * velocidad;
        }

        Destroy(gameObject, duracion);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth salud = other.GetComponent<PlayerHealth>();
            if (salud != null) salud.RecibirDanio(dano);
            Destroy(gameObject);
        }

        if (other.CompareTag("Obstacle") || other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
