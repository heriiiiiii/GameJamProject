using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_DashTrailEffect : MonoBehaviour
{
    [Header("CONFIGURACIÓN DEL RASTRO")]
    [Tooltip("Material con el shader especial para el rastro")]
    public Material trailMaterial;

    [Tooltip("Cuánto tiempo dura el efecto de rastro")]
    public float trailDuration = 0.4f;

    [Tooltip("Cada cuánto tiempo se crea un nuevo sprite de rastro")]
    public float spawnInterval = 0.02f;

    [Tooltip("Color del rastro")]
    public Color trailColor = new Color(0.2f, 0.8f, 1f, 0.7f);

    [Header("AJUSTES AVANZADOS")]
    [Tooltip("Tiempo que tarda cada sprite en desaparecer")]
    public float fadeDuration = 0.5f;

    [Tooltip("Si debe copiar el flip del sprite original")]
    public bool copySpriteFlip = true;

    // Variables privadas
    private SpriteRenderer mainRenderer;
    private bool isTrailActive = false;
    private Coroutine trailCoroutine;
    private List<GameObject> activeTrails = new List<GameObject>();

    void Start()
    {
        // Obtener el SpriteRenderer del enemigo
        mainRenderer = GetComponent<SpriteRenderer>();
        if (mainRenderer == null)
        {
            Debug.LogError("No se encontró SpriteRenderer en " + gameObject.name);
        }
    }

    /// <summary>
    /// Inicia el efecto de rastro
    /// </summary>
    public void StartTrail()
    {
        if (isTrailActive) return;

        isTrailActive = true;
        trailCoroutine = StartCoroutine(TrailRoutine());

        Debug.Log("Efecto de rastro INICIADO");
    }

    /// <summary>
    /// Detiene el efecto de rastro
    /// </summary>
    public void StopTrail()
    {
        if (!isTrailActive) return;

        isTrailActive = false;
        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
            trailCoroutine = null;
        }

        Debug.Log("Efecto de rastro DETENIDO");
    }

    /// <summary>
    /// Rutina principal que crea los sprites de rastro
    /// </summary>
    private IEnumerator TrailRoutine()
    {
        float startTime = Time.time;

        while (Time.time - startTime < trailDuration && isTrailActive)
        {
            // Crear un nuevo sprite de rastro
            CreateTrailSprite();

            // Esperar antes de crear el siguiente
            yield return new WaitForSeconds(spawnInterval);
        }

        isTrailActive = false;
    }

    /// <summary>
    /// Crea un sprite individual de rastro
    /// </summary>
    private void CreateTrailSprite()
    {
        if (mainRenderer == null || !mainRenderer.enabled || mainRenderer.sprite == null)
            return;

        // Crear nuevo GameObject para el rastro
        GameObject trailObject = new GameObject("TrailSprite");
        trailObject.transform.position = transform.position;
        trailObject.transform.rotation = transform.rotation;
        trailObject.transform.localScale = transform.localScale;

        // Añadir SpriteRenderer
        SpriteRenderer trailRenderer = trailObject.AddComponent<SpriteRenderer>();

        // Copiar propiedades del sprite original
        trailRenderer.sprite = mainRenderer.sprite;
        trailRenderer.color = trailColor;
        trailRenderer.sortingLayerID = mainRenderer.sortingLayerID;
        trailRenderer.sortingOrder = mainRenderer.sortingOrder - 1; // Detrás del original

        // Aplicar el material especial
        if (trailMaterial != null)
        {
            trailRenderer.material = trailMaterial;
        }
        else
        {
            // Usar material por defecto si no hay uno asignado
            trailRenderer.material = mainRenderer.material;
        }

        // Copiar flip si está habilitado
        if (copySpriteFlip)
        {
            trailRenderer.flipX = mainRenderer.flipX;
            trailRenderer.flipY = mainRenderer.flipY;
        }

        // Añadir a la lista de trails activos
        activeTrails.Add(trailObject);

        // Iniciar fade out y destrucción
        StartCoroutine(FadeAndDestroyTrail(trailObject, trailRenderer));
    }

    /// <summary>
    /// Hace que el sprite se desvanezca y luego se destruya
    /// </summary>
    private IEnumerator FadeAndDestroyTrail(GameObject trailObject, SpriteRenderer trailRenderer)
    {
        float elapsedTime = 0f;
        Color startColor = trailRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        // Fade out gradual
        while (elapsedTime < fadeDuration)
        {
            if (trailRenderer != null)
            {
                float progress = elapsedTime / fadeDuration;
                trailRenderer.color = Color.Lerp(startColor, endColor, progress);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Remover de la lista y destruir
        activeTrails.Remove(trailObject);
        Destroy(trailObject);
    }

    /// <summary>
    /// Limpia todos los trails activos inmediatamente
    /// </summary>
    public void ClearAllTrails()
    {
        foreach (GameObject trail in activeTrails)
        {
            if (trail != null)
                Destroy(trail);
        }
        activeTrails.Clear();
    }

    /// <summary>
    /// Para limpiar cuando se destruye el objeto
    /// </summary>
    void OnDestroy()
    {
        ClearAllTrails();
    }
}
