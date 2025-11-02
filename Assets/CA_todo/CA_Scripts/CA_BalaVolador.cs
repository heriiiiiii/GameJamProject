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
    }

    void Update()
    {
        transform.Translate(direccion * velocidad * Time.deltaTime);
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
