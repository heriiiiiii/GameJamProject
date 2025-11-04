using UnityEngine;
using TMPro;

public class TextReveal : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public float duration = 2f;

    private Material mat;
    private float elapsed = 0f;
    private float startValue = -1f;
    private float endValue = 0f;

    void Start()
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshProUGUI>();

        mat = textMesh.fontMaterial;
        mat.SetFloat(ShaderUtilities.ID_FaceDilate, startValue);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        float current = Mathf.Lerp(startValue, endValue, t);
        mat.SetFloat(ShaderUtilities.ID_FaceDilate, current);
    }
}
