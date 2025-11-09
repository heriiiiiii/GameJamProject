using System.Collections;
using UnityEngine;

public class NF_Knockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    [Tooltip("Fuerza lateral del empuje")]
    public float horizontalForce = 12f;     // 🔹 antes 10.5
    [Tooltip("Impulso vertical leve para simular rebote")]
    public float verticalForce = 7.5f;      // 🔹 antes 6.5
    [Tooltip("Duración del bloqueo de control")]
    public float duration = 0.18f;          // 🔹 un poquito más de impacto
    [Tooltip("Gravedad temporalmente reducida para suavizar el arco del rebote")]
    public float gravityDuringKnockback = 1.5f;
    public bool IsBeingKnockedBack { get; private set; }

    private Rigidbody2D rb;
    private CA_PlayerController playerController;
    private float originalGravity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<CA_PlayerController>();
        originalGravity = rb.gravityScale;
    }

    public void CallKnockback(Vector2 hitDirection)
    {
        if (IsBeingKnockedBack) return;
        StartCoroutine(DoKnockback(hitDirection));
    }

    private IEnumerator DoKnockback(Vector2 hitDirection)
    {
        IsBeingKnockedBack = true;

        // 🔒 Bloquear control temporal
        if (playerController != null)
            playerController.enabled = false;

        // 🔹 Aplicar impulso corto y fuerte
        Vector2 force = new Vector2(hitDirection.x * horizontalForce, verticalForce);
        rb.velocity = Vector2.zero;
        rb.gravityScale = gravityDuringKnockback;
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(duration);

        // 🔓 Restaurar control y gravedad
        rb.gravityScale = originalGravity;
        if (playerController != null)
            playerController.enabled = true;

        IsBeingKnockedBack = false;
    }
}
