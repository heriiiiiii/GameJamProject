using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ButtonHoverEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("Íconos laterales")]
    public GameObject leftIcon;
    public GameObject rightIcon;

    [Header("Texto del botón")]
    public TextMeshProUGUI buttonText;
    public Color normalColor = Color.white;
    public Color glowColor = new Color(0.7f, 1f, 0.7f);
    public float glowSpeed = 3f;

    [Header("Movimiento de iconos")]
    public float moveDistance = 10f;
    public float moveSpeed = 2f;

    [Header("Sonidos")]
    public AudioClip hoverSound;   // 🔹 Nuevo: sonido al navegar con flechas
    public AudioClip clickSound;   // 🔹 Sonido al confirmar

    private Vector3 leftStartPos;
    private Vector3 rightStartPos;
    private bool isHovered;
    private bool wasSelected;

    void Start()
    {
        if (leftIcon) leftStartPos = leftIcon.transform.localPosition;
        if (rightIcon) rightStartPos = rightIcon.transform.localPosition;

        if (leftIcon) leftIcon.SetActive(false);
        if (rightIcon) rightIcon.SetActive(false);
        if (buttonText) buttonText.color = normalColor;
    }

    void Update()
    {
        // 🔹 Detectar navegación con teclado (cuando el botón se selecciona)
        GameObject current = EventSystem.current.currentSelectedGameObject;
        bool selected = current == gameObject;

        if (selected && !wasSelected)
        {
            wasSelected = true;
            PlayHoverSound();
            ActivateHover(true);
        }
        else if (!selected && wasSelected)
        {
            wasSelected = false;
            ActivateHover(false);
        }

        // 🔹 Animación visual cuando está activo
        if (isHovered)
        {
            float offset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;

            if (leftIcon)
                leftIcon.transform.localPosition = leftStartPos + new Vector3(offset, 0, 0);
            if (rightIcon)
                rightIcon.transform.localPosition = rightStartPos - new Vector3(offset, 0, 0);

            if (buttonText)
            {
                float t = (Mathf.Sin(Time.time * glowSpeed) + 1f) / 2f;
                buttonText.color = Color.Lerp(normalColor, glowColor, t);
            }

            // 🔹 Confirmar con Enter o Espacio
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                PlayClickSound();

                // Ejecuta el evento OnClick real del botón
                Button btn = GetComponent<Button>();
                if (btn != null)
                    btn.onClick.Invoke();
            }
        }
    }

    // === EVENTOS DEL MOUSE ===
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHoverSound();
        ActivateHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ActivateHover(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClickSound();
    }

    // === EVENTOS DEL TECLADO ===
    public void OnSelect(BaseEventData eventData)
    {
        PlayHoverSound();
        ActivateHover(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        ActivateHover(false);
    }

    // === FUNCIONES INTERNAS ===
    private void ActivateHover(bool active)
    {
        isHovered = active;

        if (leftIcon) leftIcon.SetActive(active);
        if (rightIcon) rightIcon.SetActive(active);

        if (!active)
        {
            if (buttonText) buttonText.color = normalColor;
            if (leftIcon) leftIcon.transform.localPosition = leftStartPos;
            if (rightIcon) rightIcon.transform.localPosition = rightStartPos;
        }
    }

    private void PlayHoverSound()
    {
        if (hoverSound)
            AudioManager.Instance?.PlaySFX(hoverSound);
    }

    private void PlayClickSound()
    {
        if (clickSound)
            AudioManager.Instance?.PlaySFX(clickSound);
    }
}
