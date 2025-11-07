using UnityEngine;
using UnityEngine.UI;

public class DialogPanelFollow : MonoBehaviour
{
    [Header("Referencias")]
    public Transform npcTarget;          // NPC a seguir
    public RectTransform uiPanel;        // Panel del diálogo (RectTransform del Image)
    public Vector3 offset = new Vector3(0, 2f, 0); // Altura del panel sobre el NPC

    [Header("Canvas")]
    public Canvas parentCanvas;

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();
    }

    void LateUpdate()
    {
        if (npcTarget == null || uiPanel == null)
            return;

        // Convierte posición del mundo (NPC) a pantalla
        Vector3 screenPos = mainCam.WorldToScreenPoint(npcTarget.position + offset);

        // Convierte a coordenadas locales del Canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            screenPos,
            parentCanvas.worldCamera,
            out Vector2 localPoint
        );

        uiPanel.localPosition = localPoint;
    }
}
