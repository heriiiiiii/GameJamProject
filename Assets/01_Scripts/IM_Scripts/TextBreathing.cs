using UnityEngine;
using TMPro;

public class TextBreathing : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Color baseColor = new Color(0.9f, 1f, 0.9f); // tono verde claro
    public Color glowColor = new Color(0.5f, 1f, 0.5f); // tono más brillante
    public float speed = 2f; // velocidad del pulso

    void Start()
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
        text.color = Color.Lerp(baseColor, glowColor, t);
    }
}

