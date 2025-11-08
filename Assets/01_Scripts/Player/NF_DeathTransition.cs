using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NF_DeathTransition : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeInDuration = 1.2f;   // tiempo para oscurecer (0 → 1)
    [SerializeField] private float holdBlackTime = 0.8f;    // tiempo en negro
    [SerializeField] private float fadeOutDuration = 1.2f;  // tiempo para aclarar (1 → 0)

    private bool isTransitioning = false;

    private void Awake()
    {
        if (fadeImage == null)
            fadeImage = GetComponent<Image>();

        // 🔹 La imagen inicia transparente y desactivada
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
        fadeImage.enabled = false;
    }

    // 🔸 Llamar este método cuando el jugador toque un obstáculo
    public IEnumerator PlayDeathTransition(System.Action onMidpointAction)
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        fadeImage.enabled = true;
        Color c = fadeImage.color;

        // 1️⃣ Fade In (pantalla se oscurece)
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeImage.color = c;

        // 2️⃣ Mantener negro
        yield return new WaitForSeconds(holdBlackTime);

        // 🧩 Punto medio: ejecutar el respawn aquí
        onMidpointAction?.Invoke();

        // 3️⃣ Fade Out (pantalla se aclara)
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0f;
        fadeImage.color = c;
        fadeImage.enabled = false;
        isTransitioning = false;
    }
}
