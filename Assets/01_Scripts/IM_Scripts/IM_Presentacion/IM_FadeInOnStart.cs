using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IM_FadeInOnStart : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 2f;

    private void Start()
    {
        if (fadeImage == null)
            fadeImage = GetComponent<Image>();

        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        Color c = fadeImage.color;
        float t = 0f;
        float startAlpha = 1f;
        float endAlpha = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = endAlpha;
        fadeImage.color = c;
    }
}
