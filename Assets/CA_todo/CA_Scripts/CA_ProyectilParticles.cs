using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CA_ProyectilParticles : MonoBehaviour
{
    [Header("Shader y color")]
    public Material shaderMaterial;       // Material con shader personalizado
    public Color colorBase = Color.green; // Color del proyectil
    public float intensidadBrillo = 1.5f; // Intensidad del brillo en el shader

    [Header("Part�culas")]
    public ParticleSystem trailParticles;     // Humo o rastro
    public ParticleSystem impactParticles;    // Explosi�n o chispas al destruirse

    [Header("Ajustes visuales")]
    public float escalaInicial = 1f;
    public float escalaFinal = 0.6f;
    public float tiempoCambioEscala = 0.3f;
    public float velocidadParticulas = 1f;
    public float cantidadParticulas = 20f;

    private SpriteRenderer sprite;
    private Coroutine cambioEscalaCoroutine;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();

        //  Aplicar material (shader) si se configur�
        if (shaderMaterial != null)
        {
            sprite.material = new Material(shaderMaterial);
            sprite.material.SetColor("_Color", colorBase);
            sprite.material.SetFloat("_Intensity", intensidadBrillo);
        }

        //  Escala de aparici�n suave
        transform.localScale = Vector3.zero;
        cambioEscalaCoroutine = StartCoroutine(EscalarSuavemente(escalaInicial, tiempoCambioEscala));

        //  Configurar part�culas si existen
        if (trailParticles != null)
        {
            var main = trailParticles.main;
            main.startColor = colorBase;
            main.startSpeed = velocidadParticulas;
            main.startSize = escalaFinal;

            var emission = trailParticles.emission;
            emission.rateOverTime = cantidadParticulas;
        }
    }

    //  Escala de aparici�n progresiva (animaci�n simple)
    IEnumerator EscalarSuavemente(float escalaObjetivo, float tiempo)
    {
        Vector3 escalaInicialObj = transform.localScale;
        Vector3 escalaFinalObj = Vector3.one * escalaObjetivo;
        float t = 0f;

        while (t < tiempo)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(escalaInicialObj, escalaFinalObj, t / tiempo);
            yield return null;
        }

        transform.localScale = escalaFinalObj;
    }

    //  Efecto visual cuando el proyectil se destruye
    public void DestruirConEfecto()
    {
        if (impactParticles != null)
        {
            // Crear una copia para que las part�culas sigan tras destruir el objeto
            ParticleSystem efecto = Instantiate(impactParticles, transform.position, Quaternion.identity);
            var main = efecto.main;
            main.startColor = colorBase;
            efecto.Play();
            Destroy(efecto.gameObject, main.startLifetime.constant);
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (cambioEscalaCoroutine != null)
            StopCoroutine(cambioEscalaCoroutine);
    }
}
