using UnityEngine;

public class CA_HiloMicelial : MonoBehaviour
{
    [Header("Tipo de hilo")]
    public bool esGiratorio = true;
    public float velocidadGiro = 150f;
    public int dano = 1;

    void Update()
    {
        if (esGiratorio)
        {
            transform.Rotate(Vector3.forward * velocidadGiro * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth salud = other.GetComponent<PlayerHealth>();
            if (salud != null) salud.RecibirDanio(dano);
        }
    }
}
