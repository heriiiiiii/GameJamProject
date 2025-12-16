using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NF_ParticleController : MonoBehaviour
{
    [Header("Movimiento en tierra")]
    [SerializeField] private ParticleSystem movementParticle;

    [Range(0, 10)]
    [SerializeField] private int occurAfterVelocity = 1;

    [Range(0, 0.2f)]
    [SerializeField] private float dustFormationPeriod = 0.1f;

    [SerializeField] private Rigidbody2D playerRB;

    private float counter;
    private bool isOnGround;

    [Header("Caída y aterrizaje")]
    [SerializeField] private ParticleSystem fallParticle;

    [Header("Wall Slide")]
    [SerializeField] private ParticleSystem wallSlideParticle;
    [SerializeField] private CA_PlayerController playerController;
    [SerializeField] private float wallSlideSpawnRate = 0.25f;
    private float wallSlideTimer = 0f;

    [Header("Salto y Doble Salto")]
    [SerializeField] private ParticleSystem jumpDustFX;
    [SerializeField] private ParticleSystem jumpPropulsionFX;
    [SerializeField] private float jumpDetectThreshold = 0.1f;

    private bool wasGrounded = false;
    private float lastYVelocity = 0f;
    private bool jumpLock = false;

    void Update()
    {
        counter += Time.deltaTime;

        // --- Movimiento sobre el suelo ---
        if (isOnGround && Mathf.Abs(playerRB.velocity.x) > occurAfterVelocity)
        {
            if (counter > dustFormationPeriod)
            {
                if (movementParticle != null)
                    movementParticle.Play();
                counter = 0;
            }
        }

        // --- Deslizamiento por pared ---
        if (playerController != null && playerController.IsWallSliding())
        {
            wallSlideTimer += Time.deltaTime;

            if (wallSlideTimer >= wallSlideSpawnRate)
            {
                wallSlideTimer = 0f;
                if (wallSlideParticle != null)
                    wallSlideParticle.Play();
            }
        }
        else
        {
            wallSlideTimer = 0f;
        }

        // --- Detección de salto y doble salto ---
        if (playerController == null) return;

        bool grounded = playerController.Grounded();
        float verticalVel = playerRB.velocity.y;

        // --- Salto desde el suelo ---
        if (!grounded && wasGrounded && verticalVel > jumpDetectThreshold)
        {
            jumpLock = true;
            PlayJumpFX();
        }

        // --- Doble salto (flag del PlayerController) ---
        if (playerController.ConsumeDoubleJumpFXFlag())
        {
            jumpLock = true;
            PlayJumpFX();
        }

        // --- Aterrizaje ---
        if (grounded && !wasGrounded && lastYVelocity < -0.1f)
        {
            PlayLandFX();
            jumpLock = false; // liberar para el siguiente ciclo
        }

        wasGrounded = grounded;
        lastYVelocity = verticalVel;
    }

    // --- Detección de suelo (solo para partículas de movimiento) ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (fallParticle != null)
                fallParticle.Play();
            isOnGround = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isOnGround = false;
        }
    }

    // ============================================================
    // 🎇 FUNCIONES DE EFECTOS
    // ============================================================
    public void PlayJumpFX()
    {
        if (jumpDustFX != null)
            jumpDustFX.Play();
        if (jumpPropulsionFX != null)
            jumpPropulsionFX.Play();
    }

    public void PlayLandFX()
    {
        if (fallParticle != null)
            fallParticle.Play();
    }

    public void PlayWallSlideFX()
    {
        if (wallSlideParticle != null)
            wallSlideParticle.Play();
    }
}
