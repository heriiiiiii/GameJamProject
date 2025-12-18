using UnityEngine;

/// <summary>
/// Maneja ÚNICAMENTE el audio de:
/// - golpe recibido
/// - golpe final (muerte)
/// No controla vida, animaciones ni lógica de enemigo.
/// Seguro para usar en cualquier tipo de enemigo.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class EnemyHitAudio : MonoBehaviour
{
    [Header("?? Clips")]
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip deathClip;

    [Header("?? Volumen")]
    [Range(0f, 1f)]
    [SerializeField] private float hitVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float deathVolume = 1f;

    private AudioSource audioSource;
    private bool deathPlayed = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Configuración segura
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // 2D (cambia a 1 si quieres 3D)
    }

    /// <summary>
    /// Reproduce sonido de golpe recibido.
    /// No se reproduce si el enemigo ya murió.
    /// </summary>
    public void PlayHit()
    {
        if (deathPlayed) return;
        if (!hitClip) return;

        audioSource.PlayOneShot(hitClip, hitVolume);
    }

    /// <summary>
    /// Reproduce sonido de muerte.
    /// Se reproduce UNA SOLA VEZ y no depende del AudioSource del enemigo.
    /// </summary>
    public void PlayDeath()
    {
        if (deathPlayed) return;
        deathPlayed = true;

        if (!deathClip) return;

        // ?? Audio independiente, NO rompe nada
        AudioSource.PlayClipAtPoint(deathClip, transform.position, deathVolume);
    }

    /// <summary>
    /// Reinicia el estado (útil para pooling).
    /// </summary>
    public void ResetAudioState()
    {
        deathPlayed = false;
    }
}
