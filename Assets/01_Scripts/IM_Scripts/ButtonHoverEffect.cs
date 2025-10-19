using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Íconos laterales")]
    public RectTransform leftIcon;
    public RectTransform rightIcon;

    [Header("Texto del botón")]
    public TextMeshProUGUI buttonText;
    public Color normalColor = Color.white;
    public Color glowColor = Color.cyan;
    public float glowSpeed = 3f;

    [Header("Movimiento de íconos")]
    public float moveDistance = 5f;
    public float moveSpeed = 1f;

    [Header("Sonidos")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private Vector3 leftStartPos, rightStartPos;
    private bool isHovered = false;

    void Start()
    {
        if (leftIcon) leftStartPos = leftIcon.anchoredPosition;
        if (rightIcon) rightStartPos = rightIcon.anchoredPosition;
    }

    void Update()
    {
        // Efecto de color
        if (buttonText)
        {
            Color targetColor = isHovered ? glowColor : normalColor;
            buttonText.color = Color.Lerp(buttonText.color, targetColor, Time.deltaTime * glowSpeed);
        }

        // Movimiento de iconos
        if (leftIcon && rightIcon)
        {
            Vector3 leftTarget = leftStartPos + (isHovered ? Vector3.left * moveDistance : Vector3.zero);
            Vector3 rightTarget = rightStartPos + (isHovered ? Vector3.right * moveDistance : Vector3.zero);

            leftIcon.anchoredPosition = Vector3.Lerp(leftIcon.anchoredPosition, leftTarget, Time.deltaTime * moveSpeed);
            rightIcon.anchoredPosition = Vector3.Lerp(rightIcon.anchoredPosition, rightTarget, Time.deltaTime * moveSpeed);
        }
    }

    // 👉 Sonido y animación al pasar el mouse
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        if (hoverSound)
            AudioManager.Instance.PlaySFX(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    // 👉 Sonido al hacer clic (por si querés mantenerlo)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound)
            AudioManager.Instance.PlaySFX(clickSound);
    }
}
