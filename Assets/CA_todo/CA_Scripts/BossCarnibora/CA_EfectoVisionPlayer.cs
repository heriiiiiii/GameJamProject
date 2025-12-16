using UnityEngine;
using UnityEngine.UI;

public class CA_EfectoVisionPlayer : MonoBehaviour
{
    [Header("Efecto de Visión Reducida - Alternativo")]
    public Image overlayVision;
    public float velocidadTransicion = 2f;

    private Color colorObjetivo = new Color(0.1f, 0.3f, 0.1f, 0f);
    private bool efectoActivo = false;

    void Start()
    {
        // Crear overlay si no existe
        if (overlayVision == null)
        {
            CreateVisionOverlay();
        }
    }

    void CreateVisionOverlay()
    {
        // Crear Canvas
        GameObject canvasObj = new GameObject("VisionOverlayCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Muy alto para estar sobre todo

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();

        // Crear Image overlay
        GameObject imageObj = new GameObject("VisionOverlay");
        imageObj.transform.SetParent(canvasObj.transform);

        overlayVision = imageObj.AddComponent<Image>();
        overlayVision.color = new Color(0.1f, 0.3f, 0.1f, 0f);

        // Hacer full screen
        RectTransform rect = overlayVision.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    void Update()
    {
        if (overlayVision != null)
        {
            overlayVision.color = Color.Lerp(overlayVision.color, colorObjetivo, velocidadTransicion * Time.deltaTime);
        }
    }

    public void AplicarReduccionVision(float intensidad)
    {
        efectoActivo = true;
        colorObjetivo.a = intensidad * 0.7f; // Convertir a alpha
        Debug.Log("Aplicando reducción de visión: " + intensidad);
    }

    public void RemoverReduccionVision()
    {
        efectoActivo = false;
        colorObjetivo.a = 0f;
        Debug.Log("Removiendo reducción de visión");
    }

    public void AplicarAturdimiento(float duracion)
    {
        StartCoroutine(EfectoAturdimiento(duracion));
    }

    private System.Collections.IEnumerator EfectoAturdimiento(float duracion)
    {
        Debug.Log("Jugador aturdido!");

        // Efecto de parpadeo para aturdimiento
        Color colorOriginal = colorObjetivo;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            float parpadeo = Mathf.PingPong(tiempo * 10f, 1f);
            colorObjetivo.a = colorOriginal.a + parpadeo * 0.3f;

            tiempo += Time.deltaTime;
            yield return null;
        }

        colorObjetivo = colorOriginal;
        Debug.Log("Jugador recuperado del aturdimiento");
    }
}