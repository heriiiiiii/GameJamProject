using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UITutorialSequence : MonoBehaviour
{
    [Header("Referencias (Opcionales)")]
    public CanvasGroup panel;
    public CanvasGroup background;
    public CanvasGroup hongo1;
    public CanvasGroup hongo2;
    public CanvasGroup keys;
    public TMP_Text text;

    [Header("Texto final parpadeante (Opcional)")]
    public CanvasGroup pressKeyGroup;
    public TMP_Text pressKeyText;
    public string continueMessage = "(TOCA LA TECLA DE LA ACCIÓN PARA CONTINUAR)";

    [Header("Contenido del Texto Principal")]
    [TextArea] public string fullText;
    public float typingSpeed = 0.04f;

    [Header("Tiempos de Transición")]
    public float panelFadeTime = 1.2f;
    public float backgroundFadeTime = 1f;
    public float hongosFadeTime = 0.8f;
    public float keysFadeTime = 1f;

    private void OnEnable()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        if (panel) yield return StartCoroutine(FadeCanvas(panel, 0f, 0.9f, panelFadeTime));

        yield return StartCoroutine(TypeText());

        if (background) yield return StartCoroutine(FadeCanvas(background, 0f, 0.4f, backgroundFadeTime));

        if (hongo1) yield return StartCoroutine(FadeCanvas(hongo1, 0f, 1f, hongosFadeTime));
        if (hongo2) yield return StartCoroutine(FadeCanvas(hongo2, 0f, 1f, hongosFadeTime));

        if (keys) yield return StartCoroutine(FadeCanvas(keys, 0f, 1f, keysFadeTime));

        if (pressKeyText) pressKeyText.text = continueMessage;
        if (pressKeyGroup) StartCoroutine(BlinkPressText());
    }

    IEnumerator FadeCanvas(CanvasGroup cg, float start, float end, float time)
    {
        if (!cg) yield break; // Seguridad

        float t = 0;
        cg.alpha = start;

        while (t < time)
        {
            t += Time.deltaTime;
            if (cg) cg.alpha = Mathf.Lerp(start, end, t / time);
            yield return null;
        }
    }

    IEnumerator TypeText()
    {
        if (!text)
            yield break;

        text.text = "";
        foreach (char c in fullText)
        {
            text.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    IEnumerator BlinkPressText()
    {
        if (!pressKeyGroup)
            yield break;

        while (true)
        {
            yield return StartCoroutine(FadeCanvas(pressKeyGroup, 0f, 1f, 0.8f));
            yield return StartCoroutine(FadeCanvas(pressKeyGroup, 1f, 0f, 0.8f));
        }
    }
}
