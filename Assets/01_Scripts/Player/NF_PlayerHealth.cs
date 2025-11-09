using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class NF_PlayerHealth : MonoBehaviour
{
    [Header("Player Health Settings")]
    public int currentHealth = 3;
    public int maxHealth = 3;

    private NF_GameController gameController;
    private NF_Knockback knockback;
    private CinemachineImpulseSource impulseSource;

    [Header("References")]
    [SerializeField] private Image healthFill;   
    [SerializeField] private Animator uiAnimator;  
    [SerializeField] private bool useUI = true;

    private void Awake()
    {
        // Busca el GameController en la escena
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<NF_GameController>();
      
    }

    private void Start()
    {
        // Inicializa la vida al máximo y obtiene el componente Knockback
        currentHealth = maxHealth;
        knockback = GetComponent<NF_Knockback>();
          impulseSource=GetComponent<CinemachineImpulseSource>();

        UpdateHealthUI();
        UpdateWeakState();
    }
    private void Update()
    {
        UpdateHealthUI();
        UpdateWeakState();
    }
    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        NF_CameraShakeManager.instance.CameraShake(impulseSource);
        // Resta vida
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
        UpdateWeakState();
        // Direccion del input (no usada directamente, pero mantenida por compatibilidad)
        float inputDirection = Mathf.Sign(CA_PlayerController.Instance.transform.localScale.x);

        // Si la vida llega a cero -> morir
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        // 🧠 HITSTOP (ralentiza el tiempo brevemente)
        StartCoroutine(HitStop(0.05f));

       

        // 💫 KNOCKBACK (retroceso físico del jugador)
        knockback.CallKnockback(hitDirection);
    }
    // 💀 Versión sin knockback (para daño pasivo o atrapamientos tipo moho)
    public void TakeDamageWithoutKnockback(int damage)
    {
        if (NF_CameraShakeManager.instance != null && impulseSource != null)
            NF_CameraShakeManager.instance.CameraShake(impulseSource);

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
        UpdateWeakState();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        // 🔹 Solo efecto de impacto (no knockback)
        StartCoroutine(HitStop(0.05f));
    }

    public void HealToFull()
    {
        currentHealth = maxHealth;
    }
    public void UpdateHealthUI()
    {
        if (!useUI || healthFill == null) return;
        healthFill.fillAmount = (float)currentHealth / (float)maxHealth;
    }
    //public void Heal(int amount)
    //{
    //    currentHealth += amount;
    //    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

    //    UpdateHealthUI();
    //    UpdateWeakState();
    //}
    public void UpdateWeakState()
    {
        if (!useUI || uiAnimator == null) return;
        bool isWeak = currentHealth <= Mathf.CeilToInt(maxHealth * 0.25f); // ≤25% de vida
        uiAnimator.SetBool("isWeak", isWeak);
    }
    private bool isHitStopping = false;

    private IEnumerator HitStop(float duration)
    {
        // 🚫 Evita que se ejecute más de un hitstop simultáneo
        if (isHitStopping)
            yield break;

        isHitStopping = true;

        float originalScale = Time.timeScale;
        Time.timeScale = 0.05f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalScale;
        isHitStopping = false;
    }


    private void Die()
    {
        Debug.Log("☠️ El jugador ha muerto. Respawn en Zone.");
        currentHealth = maxHealth;

        // 💡 2️⃣ Actualizar UI inmediatamente
        UpdateHealthUI();
        UpdateWeakState();
        NF_CameraManager.instance.ForceResetToDefaultCamera();
        //StartCoroutine(gameController.Respawn(1f, "Zone"));
        CA_PlayerController.Instance.Die();
    }
}
