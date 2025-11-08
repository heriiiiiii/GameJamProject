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

    [Header("REFERENCIAS PARA PERSONAJE CON HUESOS")]
    [Tooltip("Si el personaje usa bones/rigging, asignar aquí el GameObject raíz")]
    public GameObject characterRoot;

    [Header("AJUSTES AVANZADOS")]
    [Tooltip("Tiempo que tarda cada sprite en desaparecer")]
    public float fadeDuration = 0.5f;

    // Variables privadas
    private SpriteRenderer[] allRenderers; // Para personajes con múltiples partes
    private SpriteRenderer mainRenderer;   // Para personajes simples
    private bool isTrailActive = false;
    private Coroutine trailCoroutine;
    private List<GameObject> activeTrails = new List<GameObject>();

    void Start()
    {
        // Intentar encontrar todos los SpriteRenderers para personajes con huesos
        if (characterRoot != null)
        {
            allRenderers = characterRoot.GetComponentsInChildren<SpriteRenderer>(true);
        }
        else
        {
            // Buscar en este GameObject y sus hijos
            allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        // Para compatibilidad con personajes simples
        mainRenderer = GetComponent<SpriteRenderer>();

        if (allRenderers == null || allRenderers.Length == 0)
        {
            Debug.LogError("No se encontraron SpriteRenderers en " + gameObject.name);
        }
        else
        {
            Debug.Log($"Encontrados {allRenderers.Length} SpriteRenderers para efectos de trail");
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
            // Crear trails para todos los sprites
            CreateTrailForAllSprites();

            // Esperar antes de crear el siguiente
            yield return new WaitForSeconds(spawnInterval);
        }

        isTrailActive = false;
    }

    /// <summary>
    /// Crea trails para todos los SpriteRenderers del personaje
    /// </summary>
    private void CreateTrailForAllSprites()
    {
        if (allRenderers == null || allRenderers.Length == 0) return;

        // Crear un contenedor padre para este frame de trail
        GameObject trailFrame = new GameObject("TrailFrame");
        trailFrame.transform.position = transform.position;
        trailFrame.transform.rotation = transform.rotation;
        trailFrame.transform.localScale = transform.localScale;

        // Para cada SpriteRenderer en el personaje, crear un trail
        foreach (SpriteRenderer originalRenderer in allRenderers)
        {
            if (originalRenderer == null || !originalRenderer.enabled || originalRenderer.sprite == null)
                continue;

            CreateIndividualTrail(originalRenderer, trailFrame.transform);
        }

        // Añadir a la lista de trails activos
        activeTrails.Add(trailFrame);

        // Iniciar fade out y destrucción
        StartCoroutine(FadeAndDestroyTrailFrame(trailFrame));
    }

    /// <summary>
    /// Crea un trail individual para un SpriteRenderer específico
    /// </summary>
    private void CreateIndividualTrail(SpriteRenderer originalRenderer, Transform parent)
    {
        // Crear nuevo GameObject para el trail individual
        GameObject trailObject = new GameObject($"Trail_{originalRenderer.gameObject.name}");
        trailObject.transform.SetParent(parent);

        // Mantener la posición relativa al personaje
        trailObject.transform.position = originalRenderer.transform.position;
        trailObject.transform.rotation = originalRenderer.transform.rotation;
        trailObject.transform.localScale = originalRenderer.transform.localScale;

        // Añadir SpriteRenderer
        SpriteRenderer trailRenderer = trailObject.AddComponent<SpriteRenderer>();

        // Copiar propiedades del sprite original
        trailRenderer.sprite = originalRenderer.sprite;
        trailRenderer.color = trailColor;
        trailRenderer.sortingLayerID = originalRenderer.sortingLayerID;
        trailRenderer.sortingOrder = originalRenderer.sortingOrder - 1; // Detrás del original

        // Copiar flip
        trailRenderer.flipX = originalRenderer.flipX;
        trailRenderer.flipY = originalRenderer.flipY;

        // Aplicar el material especial
        if (trailMaterial != null)
        {
            trailRenderer.material = trailMaterial;
        }
        else
        {
            trailRenderer.material = originalRenderer.material;
        }
    }

    /// <summary>
    /// Hace que todo el frame de trail se desvanezca y luego se destruya
    /// </summary>
    private IEnumerator FadeAndDestroyTrailFrame(GameObject trailFrame)
    {
        float elapsedTime = 0f;
        SpriteRenderer[] trailRenderers = trailFrame.GetComponentsInChildren<SpriteRenderer>();

        // Fade out gradual para todos los sprites del frame
        while (elapsedTime < fadeDuration && trailFrame != null)
        {
            foreach (SpriteRenderer trailRenderer in trailRenderers)
            {
                if (trailRenderer != null)
                {
                    Color currentColor = trailRenderer.color;
                    float alpha = Mathf.Lerp(trailColor.a, 0f, elapsedTime / fadeDuration);
                    trailRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                }
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Remover de la lista y destruir
        activeTrails.Remove(trailFrame);
        if (trailFrame != null)
            Destroy(trailFrame);
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