using UnityEngine;

public class CA_BalaVolador : MonoBehaviour
{
    public float velocidad = 8f;
    public int dano = 1;
    public float duracion = 4f;
    private Vector2 direccion;

    void Start()
    {
        Destroy(gameObject, duracion);
    }

    public void SetDireccion(Vector2 dir)
    {
        direccion = dir.normalized;

        // Rotar el prefab completo hacia la dirección del movimiento
        if (direccion != Vector2.zero)
        {
            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angulo);
        }
    }

    void Update()
    {
        transform.Translate(Vector2.right * velocidad * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            NF_PlayerHealth vida = col.GetComponent<NF_PlayerHealth>();
            if (vida != null)
            {
                Vector2 hitDir = ((Vector2)col.transform.position - (Vector2)transform.position).normalized;

                vida.TakeDamage(dano, hitDir);
            }
            Destroy(gameObject);
        }
        else if (!col.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}