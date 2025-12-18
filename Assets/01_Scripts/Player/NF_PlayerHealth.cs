using System;
using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class NF_PlayerHealth : MonoBehaviour
{
    [Header("Player Health Settings")]
    public int maxHealth = 3;
    public int currentHealth = 3;

    [Header("Healing Animation")]
    [SerializeField] private float healDuration = 0.8f;

    private float displayedHealth;
    private Coroutine healRoutine;

    private NF_GameController gameController;
    private NF_Knockback knockback;
    private CinemachineImpulseSource impulseSource;

    [Header("UI References")]
    [SerializeField] private Image healthFill;
    [SerializeField] private Animator uiAnimator;
    [SerializeField] private bool useUI = true;

    // 🔹 Evento de muerte
    public event Action OnPlayerDeath;

    private bool isHitStopping = false;

    // =========================
    // 🔊 AUDIO DAÑO PLAYER
    // =========================
    [Header("🔊 Audio Daño")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitPlayerClip;
    [Range(0f, 1f)]
    [SerializeField] private float hitVolume = 1f;

    // =========================
    // UNITY
    // =========================
    private void Awake()
    {
        GameObject gc = GameObject.FindGameObjectWithTag("GameController");
        if (gc != null)
            gameController = gc.GetComponent<NF_GameController>();

        // Audio seguro
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();

        if (audioSource)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f; // 2D
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        displayedHealth = currentHealth;

        knockback = GetComponent<NF_Knockback>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        UpdateHealthUIImmediate();
        UpdateWeakState();
    }

    // =========================
    // DAÑO
    // =========================
    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        // 🔊 SONIDO DE GOLPE
        PlayHitSound();

        if (NF_CameraShakeManager.instance != null)
            NF_CameraShakeManager.instance.CameraShake(impulseSource);

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        displayedHealth = currentHealth;
        UpdateHealthUIImmediate();
        UpdateWeakState();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(HitStop(0.05f));

        if (knockback != null)
            knockback.CallKnockback(hitDirection);
    }

    public void TakeDamageWithoutKnockback(int damage)
    {
        // 🔊 SONIDO DE GOLPE
        PlayHitSound();

        if (NF_CameraShakeManager.instance != null)
            NF_CameraShakeManager.instance.CameraShake(impulseSource);

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        displayedHealth = currentHealth;
        UpdateHealthUIImmediate();
        UpdateWeakState();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(HitStop(0.05f));
    }

    // =========================
    // 🔊 AUDIO MÉTODO
    // =========================
    private void PlayHitSound()
    {
        if (hitPlayerClip && audioSource)
            audioSource.PlayOneShot(hitPlayerClip, hitVolume);
    }

    // =========================
    // CURACIÓN
    // =========================
    public void HealToFullAnimated()
    {
        if (healRoutine != null)
            StopCoroutine(healRoutine);

        healRoutine = StartCoroutine(HealRoutine(maxHealth));
    }

    private IEnumerator HealRoutine(int targetHealth)
    {
        float startHealth = displayedHealth;
        float elapsed = 0f;

        while (elapsed < healDuration)
        {
            elapsed += Time.deltaTime;
            displayedHealth = Mathf.Lerp(startHealth, targetHealth, elapsed / healDuration);
            UpdateHealthUIImmediate();
            yield return null;
        }

        displayedHealth = targetHealth;
        currentHealth = targetHealth;
        UpdateHealthUIImmediate();
        UpdateWeakState();
    }

    // =========================
    // MÉTODOS LEGACY
    // =========================
    public void HealToFull()
    {
        HealToFullAnimated();
    }

    public void UpdateHealthUI()
    {
        UpdateHealthUIImmediate();
    }

    // =========================
    // UI
    // =========================
    private void UpdateHealthUIImmediate()
    {
        if (!useUI || healthFill == null || maxHealth <= 0) return;
        healthFill.fillAmount = displayedHealth / maxHealth;
    }

    public void UpdateWeakState()
    {
        if (!useUI || uiAnimator == null) return;
        bool isWeak = currentHealth <= Mathf.CeilToInt(maxHealth * 0.25f);
        uiAnimator.SetBool("isWeak", isWeak);
    }

    // =========================
    // HIT STOP
    // =========================
    private IEnumerator HitStop(float duration)
    {
        if (isHitStopping)
            yield break;

        isHitStopping = true;

        float originalScale = Time.timeScale;
        Time.timeScale = 0.05f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalScale;
        isHitStopping = false;
    }

    // =========================
    // MUERTE
    // =========================
    private void Die()
    {
        Debug.Log("☠️ El jugador ha muerto.");

        OnPlayerDeath?.Invoke();

        currentHealth = maxHealth;
        displayedHealth = maxHealth;

        UpdateHealthUIImmediate();
        UpdateWeakState();

        if (NF_CameraManager.instance != null)
            NF_CameraManager.instance.ForceResetToDefaultCamera();

        if (CA_PlayerController.Instance != null)
            CA_PlayerController.Instance.Die();
    }

    private void OnDestroy()
    {
        OnPlayerDeath = null;
    }
}
