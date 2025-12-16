using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IM_FadeInOnStart : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float holdSolidTime = 1.0f;     // tiempo que se mantiene completamente negro
    [SerializeField] private float darkToVisibleDuration = 2.5f; // duración total del desvanecimiento
    [SerializeField, Range(0.9f, 1f)] private float solidAlpha = 0.98f; // qué tan negro se mantiene al inicio (0.98 ≈ 250)

    private void Start()
    {
        if (fadeImage == null)
            fadeImage = GetComponent<Image>();

        // Asegurar que arranque completamente opaco (negro total)
        Color c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;

        StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        Color c = fadeImage.color;

        // 1️⃣ Mantener un negro sólido (Alpha = 1) un momento
        yield return new WaitForSeconds(holdSolidTime);

        // 2️⃣ Transición leve entre 255 -> 245 (o lo que definas)
        float t1 = 0f;
        float transitionTime = 0.5f; // este pequeño tramo hace un "bajón" de 1 a solidAlpha (250)
        while (t1 < transitionTime)
        {
            t1 += Time.deltaTime;
            c.a = Mathf.Lerp(1f, solidAlpha, t1 / transitionTime);
            fadeImage.color = c;
            yield return null;
        }

        // 3️⃣ Mantener el negro “semi-opaco” (250) un pequeño tiempo extra antes del fade real
        yield return new WaitForSeconds(holdSolidTime * 0.3f);

        // 4️⃣ Fade real: ir desde solidAlpha (≈245) hasta 0
        float t2 = 0f;
        while (t2 < darkToVisibleDuration)
        {
            t2 += Time.deltaTime;
            // Curva suave (ease-out)
            float eased = Mathf.SmoothStep(0, 1, t2 / darkToVisibleDuration);
            c.a = Mathf.Lerp(solidAlpha, 0f, eased);
            fadeImage.color = c;
            yield return null;
        }

        // 5️⃣ Asegurar transparencia total
        c.a = 0f;
        fadeImage.color = c;
    }
}
