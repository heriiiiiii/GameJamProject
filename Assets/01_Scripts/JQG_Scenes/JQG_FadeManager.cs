using System.Collections;
using UnityEngine;

public class JQG_FadeManager : MonoBehaviour
{
    public static JQG_FadeManager Instance;
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.5f;

    private bool isFading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0;
            fadeCanvas.blocksRaycasts = false;
            fadeCanvas.gameObject.SetActive(false);
        }
    }

    public IEnumerator FadeOut()
    {
        if (isFading || fadeCanvas == null) yield break;
        isFading = true;

        fadeCanvas.gameObject.SetActive(true);
        fadeCanvas.blocksRaycasts = true;

        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            fadeCanvas.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = 1;
        isFading = false;
    }

    public IEnumerator FadeIn()
    {
        if (isFading || fadeCanvas == null) yield break;
        isFading = true;

        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            fadeCanvas.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = 0;
        fadeCanvas.blocksRaycasts = false;

        Destroy(fadeCanvas.gameObject);
        Destroy(gameObject);

        isFading = false;
    }
}
