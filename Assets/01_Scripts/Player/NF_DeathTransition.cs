using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NF_DeathTransition : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;          // ← arrastra aquí tu Image (aunque esté inactiva)
    [SerializeField] private float fadeInDuration = 0.6f;  // ⏩ ahora más rápido por defecto
    [SerializeField] private float holdBlackTime = 0.8f;
    [SerializeField] private float fadeOutDuration = 1.2f;

    private bool isTransitioning;

    private void Reset()
    {
        if (!fadeImage) fadeImage = GetComponent<Image>();
    }

    private void Awake()
    {
        if (!fadeImage)
        {
            Debug.LogError("[NF_DeathTransition] Falta asignar 'fadeImage' en el Inspector.");
            return;
        }

        var c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        if (fadeImage.gameObject.activeInHierarchy)
            fadeImage.enabled = false;
    }

    public IEnumerator PlayDeathTransition(System.Action onMidpointAction)
    {
        if (!fadeImage || isTransitioning) yield break;
        isTransitioning = true;

        if (!fadeImage.gameObject.activeSelf) fadeImage.gameObject.SetActive(true);
        fadeImage.enabled = true;

        Color c = fadeImage.color;

        // 1️⃣ Fade In (0 → 1) — ahora más rápido y con curva más suave
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            // curva acelerada (ease-in rápido)
            float eased = Mathf.Pow(t / fadeInDuration, 0.7f);
            c.a = Mathf.Lerp(0f, 1f, eased);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 1f;
        fadeImage.color = c;

        // 2️⃣ Mantener negro
        yield return new WaitForSeconds(holdBlackTime);

        // ⏺ Respawn (en negro)
        onMidpointAction?.Invoke();

        // 3️⃣ Fade Out (1 → 0)
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            // curva suave al final
            float eased = Mathf.SmoothStep(0f, 1f, t / fadeOutDuration);
            c.a = Mathf.Lerp(1f, 0f, eased);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0f;
        fadeImage.color = c;
        fadeImage.enabled = false;
        fadeImage.gameObject.SetActive(false);

        isTransitioning = false;
    }
}
