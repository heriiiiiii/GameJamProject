using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MenuButtonVisual : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler
{
    [Header("Referencias")]
    public TextMeshProUGUI buttonText;
    public GameObject leftIcon;
    public GameObject rightIcon;

    [Header("Colores")]
    public Color normalColor = Color.white;
    public Color glowColor = new Color(0.6f, 1f, 0.8f);

    [Header("Animación")]
    public float pulseSpeed = 3f;
    public float iconMoveAmount = 8f;
    public float iconMoveSpeed = 2f;

    [Header("Sonidos")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private bool isActive = false;
    private Vector3 leftStartPos;
    private Vector3 rightStartPos;

    void Start()
    {
        if (leftIcon) { leftStartPos = leftIcon.transform.localPosition; leftIcon.SetActive(false); }
        if (rightIcon) { rightStartPos = rightIcon.transform.localPosition; rightIcon.SetActive(false); }
        if (buttonText) buttonText.color = normalColor;
    }

    void Update()
    {
        if (!isActive) return;

        float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
        if (buttonText) buttonText.color = Color.Lerp(normalColor, glowColor, t);

        float offset = Mathf.Sin(Time.unscaledTime * iconMoveSpeed) * iconMoveAmount;
        if (leftIcon) leftIcon.transform.localPosition = leftStartPos + new Vector3(-offset, 0, 0);
        if (rightIcon) rightIcon.transform.localPosition = rightStartPos + new Vector3(offset, 0, 0);
    }

    public void OnSelect(BaseEventData eventData)
    {
        isActive = true;
        buttonText.color = glowColor;

        if (leftIcon) leftIcon.SetActive(true);
        if (rightIcon) rightIcon.SetActive(true);

        if (hoverSound) AudioManager.Instance?.PlaySFX(hoverSound);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isActive = false;
        buttonText.color = normalColor;

        if (leftIcon) { leftIcon.SetActive(false); leftIcon.transform.localPosition = leftStartPos; }
        if (rightIcon) { rightIcon.SetActive(false); rightIcon.transform.localPosition = rightStartPos; }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (clickSound) AudioManager.Instance?.PlaySFX(clickSound);
        GetComponent<Button>().onClick?.Invoke();
    }
}
