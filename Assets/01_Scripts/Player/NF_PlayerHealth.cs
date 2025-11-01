using System.Collections;
using UnityEngine;
using Cinemachine;
public class NF_PlayerHealth : MonoBehaviour
{
    [Header("Player Health Settings")]
    public int currentHealth = 3;
    public int maxHealth = 3;

    private NF_GameController gameController;
    private NF_Knockback knockback;
    private CinemachineImpulseSource impulseSource;

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
    }
    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        NF_CameraShakeManager.instance.CameraShake(impulseSource);
        // Resta vida
        currentHealth -= damage;

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
    public void HealToFull()
    {
        currentHealth = maxHealth;
    }

    private IEnumerator HitStop(float duration)
    {
        float originalScale = Time.timeScale;

        // Ralentiza el tiempo para dar sensación de impacto
        Time.timeScale = 0.05f;

        // Espera sin depender del tiempo del juego (para que funcione durante el hitstop)
        yield return new WaitForSecondsRealtime(duration);

        // Restaura el tiempo
        Time.timeScale = originalScale;
    }

    private void Die()
    {
        Debug.Log("☠️ El jugador ha muerto. Respawn en Zone.");
        //StartCoroutine(gameController.Respawn(1f, "Zone"));
        CA_PlayerController.Instance.Die();
    }
}
