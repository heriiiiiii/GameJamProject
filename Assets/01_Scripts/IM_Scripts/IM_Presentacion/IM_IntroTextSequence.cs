using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IM_IntroTextSequence : MonoBehaviour
{
    [Header("Referencias de los textos en orden")]
    public CanvasGroup[] textos;

    [Header("Fade general al final")]
    public Image fadeImage;              // Imagen negra transparente que cubre toda la pantalla
    public float finalFadeDuration = 2f; // Duración del fundido a negro
    public string nextSceneName;         // Nombre de la escena a cargar después

    [Header("Duraciones de texto")]
    public float fadeDuration = 1f;      // Tiempo que tarda en aparecer cada texto
    public float delayBetweenTexts = 2f; // Tiempo visible antes del siguiente

    private void Start()
    {
        StartCoroutine(SecuenciaIntro());
    }

    IEnumerator SecuenciaIntro()
    {
        // Asegurarse de que todos comiencen invisibles
        foreach (CanvasGroup t in textos)
            t.alpha = 0;

        // Mostrar uno por uno
        foreach (CanvasGroup t in textos)
        {
            yield return StartCoroutine(FadeIn(t));
            yield return new WaitForSeconds(delayBetweenTexts);
        }

        // Esperar 5 segundos al final antes de fundir
        yield return new WaitForSeconds(5f);

        // Fundido a negro
        yield return StartCoroutine(FadeToBlack());

        // Cargar la siguiente escena
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator FadeIn(CanvasGroup group)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }
        group.alpha = 1;
    }

    IEnumerator FadeToBlack()
    {
        if (fadeImage == null)
            yield break;

        Color c = fadeImage.color;
        c.a = 0;
        fadeImage.color = c;

        float t = 0f;
        while (t < finalFadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / finalFadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 1;
        fadeImage.color = c;
    }
}
