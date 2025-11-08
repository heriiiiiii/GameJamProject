using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_AnimacionCaminar : MonoBehaviour
{
    [Header("FRAMES DE CAMINAR")]
    public Sprite[] framesCaminar;
    public float velocidadAnimacion = 8f;

    [Header("CONFIGURACIÓN SHADER")]
    public Material materialPersonalizado;
    public Color colorTinte = Color.white;

    private SpriteRenderer spriteRenderer;
    private int frameActual = 0;
    private bool estaAnimando = false;

    void Start()
    {
        // Obtener o añadir SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Aplicar material personalizado si está asignado
        if (materialPersonalizado != null)
        {
            spriteRenderer.material = materialPersonalizado;
        }

        // Aplicar color de tinte
        spriteRenderer.color = colorTinte;

        // Iniciar animación automáticamente
        IniciarAnimacion();
    }

    void Update()
    {
        // La animación se maneja en la corrutina, pero puedes añadir lógica aquí si necesitas
    }

    public void IniciarAnimacion()
    {
        if (!estaAnimando && framesCaminar != null && framesCaminar.Length > 0)
        {
            estaAnimando = true;
            StartCoroutine(AnimacionLoop());
        }
        else if (framesCaminar == null || framesCaminar.Length == 0)
        {
            Debug.LogError("❌ No hay frames asignados en " + gameObject.name);
        }
    }

    public void DetenerAnimacion()
    {
        estaAnimando = false;
        StopAllCoroutines();
    }

    public void PausarAnimacion()
    {
        estaAnimando = false;
        StopAllCoroutines();
    }

    public void ReanudarAnimacion()
    {
        if (!estaAnimando)
        {
            IniciarAnimacion();
        }
    }

    private IEnumerator AnimacionLoop()
    {
        while (estaAnimando && framesCaminar != null && framesCaminar.Length > 0)
        {
            // Cambiar al frame actual
            spriteRenderer.sprite = framesCaminar[frameActual];

            // Avanzar al siguiente frame
            frameActual++;

            // Volver al primer frame si llegamos al final
            if (frameActual >= framesCaminar.Length)
            {
                frameActual = 0;
            }

            // Esperar antes del siguiente frame
            yield return new WaitForSeconds(1f / velocidadAnimacion);
        }
    }

    // Método para cambiar frames en tiempo de ejecución
    public void CambiarFrames(Sprite[] nuevosFrames, float nuevaVelocidad = 8f)
    {
        DetenerAnimacion();
        framesCaminar = nuevosFrames;
        velocidadAnimacion = nuevaVelocidad;
        frameActual = 0;
        IniciarAnimacion();
    }

    // Método para aplicar efectos de shader
    public void AplicarEfectoShader(string propiedad, float valor)
    {
        if (spriteRenderer.material != null)
        {
            spriteRenderer.material.SetFloat(propiedad, valor);
        }
    }

    public void CambiarColorTinte(Color nuevoColor)
    {
        colorTinte = nuevoColor;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = colorTinte;
        }
    }

    void OnDestroy()
    {
        DetenerAnimacion();
    }
}
