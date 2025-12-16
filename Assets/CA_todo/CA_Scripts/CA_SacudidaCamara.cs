using UnityEngine;
using System.Collections;

public class CA_SacudidaCamara : MonoBehaviour
{
    [Header("Configuración Sacudida")]
    public float sacudidaDecaimiento = 0.9f;

    private Vector3 posicionOriginal;
    private float intensidadActual = 0f;
    private float duracionActual = 0f;
    private bool sacudiendo = false;

    void Start()
    {
        posicionOriginal = transform.localPosition;
    }

    void Update()
    {
        if (sacudiendo)
        {
            if (duracionActual > 0f)
            {
                // Generar offset aleatorio para la sacudida
                Vector3 offset = Random.insideUnitSphere * intensidadActual;
                offset.z = 0f; // Mantener en 2D

                transform.localPosition = posicionOriginal + offset;

                // Reducir intensidad con el tiempo
                intensidadActual *= sacudidaDecaimiento;
                duracionActual -= Time.deltaTime;
            }
            else
            {
                // Terminar sacudida
                transform.localPosition = posicionOriginal;
                sacudiendo = false;
            }
        }
    }

    public void IniciarSacudida(float intensidad, float duracion)
    {
        if (!sacudiendo)
        {
            posicionOriginal = transform.localPosition;
        }

        intensidadActual = intensidad;
        duracionActual = duracion;
        sacudiendo = true;
    }

    // Método para sacudida más intensa (opcional)
    public void SacudidaFuerte(float intensidad = 1f, float duracion = 0.5f)
    {
        IniciarSacudida(intensidad, duracion);
    }

    // Método para detener la sacudida manualmente
    public void DetenerSacudida()
    {
        sacudiendo = false;
        transform.localPosition = posicionOriginal;
    }
}