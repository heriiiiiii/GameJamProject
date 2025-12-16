using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(100)] // asegura que la cámara se mueva después del player
public class NF_CameraShake : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 6f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f);

    [Header("Shake Settings")]
    [Tooltip("Factor de intensidad general del shake")]
    [SerializeField] private float baseShakeMultiplier = 1.2f;

    public static NF_CameraShake Instance;
    private Vector3 shakeOffset = Vector3.zero;
    private Coroutine shakeCoroutine;
    private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        Instance = this;
    }

    private void LateUpdate()
    {
        if (CA_PlayerController.Instance == null) return;

        // 🧭 Posición base del seguimiento
        Vector3 targetPosition = CA_PlayerController.Instance.transform.position + offset;

        // 💨 Movimiento suave hacia el objetivo + offset de vibración
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition + shakeOffset,
            ref velocity,
            1f / followSpeed
        );
    }

    // 🎥 Llamada pública para sacudir cámara (se puede llamar desde cualquier lugar)
    public void CallShake(float duration, float magnitude, Vector2? direction = null)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(Shake(duration, magnitude, direction));
    }

    private IEnumerator Shake(float duration, float magnitude, Vector2? direction = null)
    {
        float elapsed = 0f;
        magnitude *= baseShakeMultiplier;

        // 💥 direccionalidad (si hay golpe, cámara retrocede un poco en esa dirección)
        Vector2 dir = direction.HasValue ? direction.Value.normalized : Random.insideUnitCircle;
        Vector3 impactVector = new Vector3(-dir.x, -dir.y, 0f) * 0.5f;

        while (elapsed < duration)
        {
            // mezcla entre dirección principal y ruido aleatorio
            float noiseX = Random.Range(-1f, 1f) * magnitude * 0.6f;
            float noiseY = Random.Range(-1f, 1f) * magnitude * 0.6f;

            shakeOffset = new Vector3(
                impactVector.x * magnitude + noiseX,
                impactVector.y * magnitude + noiseY,
                0f
            );

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
    }
}
