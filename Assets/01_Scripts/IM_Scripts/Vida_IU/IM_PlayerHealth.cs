using UnityEngine;
using UnityEngine.UI;

public class IM_PlayerHealth : MonoBehaviour
{
    [Header("⚙️ Health Settings")]
    public float maxHealth = 10f;
    private float currentHealth;

    [Header("🎨 UI References")]
    [SerializeField] private Image healthFill;     // La imagen de la barra de vida (Fill)
    [SerializeField] private Animator uiAnimator;  // Animator del ícono del personaje

    void Awake()
    {
        // Seguridad por si olvidas asignar algo en el inspector
        if (healthFill == null)
            Debug.LogWarning("⚠️ Health Fill no está asignado en el inspector.", this);

        if (uiAnimator == null)
            Debug.LogWarning("⚠️ UI Animator no está asignado en el inspector.", this);
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        UpdateWeakState(); // 🔹 Forzar estado correcto desde inicio
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthUI();
        UpdateWeakState();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthUI();
        UpdateWeakState();
    }

    private void UpdateHealthUI()
    {
        if (healthFill != null)
            healthFill.fillAmount = currentHealth / maxHealth;
    }

    /// <summary>
    /// Cambia automáticamente la animación del ícono según la vida actual.
    /// </summary>
    private void UpdateWeakState()
    {
        if (uiAnimator == null) return;

        bool isWeak = currentHealth <= 2f;
        uiAnimator.SetBool("isWeak", isWeak);
    }

    private void Die()
    {
        Debug.Log("💀 Player ha muerto.");
        // Aquí podrías agregar efectos o reinicio de escena.
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Damage"))
        {
            TakeDamage(2f);
        }
    }
}
