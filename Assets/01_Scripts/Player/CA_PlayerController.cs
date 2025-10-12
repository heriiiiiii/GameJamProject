using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_PlayerController : MonoBehaviour
{
    [Header("Movimiento Horizontal")]
    private Rigidbody2D rb;
    [SerializeField] private float walkSpeed = 1;
    private float xAxis;

    [Header("Ground Check Settings")]
    [SerializeField] private float jumpForce = 45;
    private int jumpBufferCounter = 0;
    [SerializeField] private int jumpBufferFrames;
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;
    private float airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;

    [SerializeField] private Transform groundChechPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;

    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(8f, 16f);

    [Header("Dash Mechanics")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    private bool canDash = true;  // Asegurarnos de que puede hacer dash desde el principio
    private bool dashed;

    [Header("Wall Mechanics")]
    [SerializeField] private float wallSlidingSpeed = 2f;  // Velocidad de deslizamiento
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Transform wallCheck;
    private bool isWallSliding;  // Si está deslizándose por la pared

    public static CA_PlayerController Instance;
    CA_PlayerStateList pState;

    private float gravity;
    private bool isFacingRight = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;  // Asigna la instancia si es la primera vez
        }
        else if (Instance != this)
        {
            Destroy(gameObject);  // Si ya existe una instancia, destruye este objeto
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        pState = GetComponent<CA_PlayerStateList>();

        gravity = rb.gravityScale;
    }

    void Update()
    {
        GetInputs();
        UpdateJumpVariables();

        if (pState.dashing) return;  // Si está en estado de dash, no actualizamos movimiento

        if (!isWallSliding)  // Si no estamos deslizándonos por la pared, continuamos con el movimiento normal
        {
            Flip();
        }

        Move();
        Jump();
        StartDash();

        WallSlide();  // Deslizarse por la pared
        WallJump();   // Salto en la pared
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");  // Obtener la entrada horizontal (teclas de dirección)
    }

    private void Move()
    {
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
    }

    void StartDash()
    {
        if (Input.GetKeyDown(KeyCode.C) && canDash && !dashed && !isWallSliding)
        {
            StartCoroutine(Dash());
            dashed = true;
        }

        if (Grounded())
        {
            dashed = false;  // Resetear la variable dashed cuando el jugador toca el suelo
        }
    }

    IEnumerator Dash()
    {
        canDash = false;  // Deshabilitar el dash temporalmente
        pState.dashing = true;  // Establecer que está en dash
        rb.gravityScale = 0;  // Eliminar la gravedad durante el dash
        rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);  // Desplazarse en la dirección del jugador

        yield return new WaitForSeconds(dashTime);  // Esperar el tiempo de duración del dash

        rb.gravityScale = gravity;  // Restaurar la gravedad
        pState.dashing = false;  // Terminar el estado de dash

        yield return new WaitForSeconds(dashCooldown);  // Esperar el cooldown

        canDash = true;  // Habilitar el dash nuevamente después del cooldown
    }

    public bool Grounded()
    {
        if (Physics2D.Raycast(groundChechPoint.position, Vector2.down, groundCheckY, whatIsGround) ||
            Physics2D.Raycast(groundChechPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround) ||
            Physics2D.Raycast(groundChechPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsWalled()
    {
        // Verifica si estamos tocando una pared usando OverlapCircle
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    void WallSlide()
    {
        // Si estamos tocando la pared y no estamos en el suelo, podemos deslizar
        if (IsWalled() && !Grounded() && xAxis != 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));  // Limitar la velocidad de caída
            rb.gravityScale = 0;  // Eliminar la gravedad durante el deslizamiento
        }
        else
        {
            isWallSliding = false;
            rb.gravityScale = gravity;  // Restaurar la gravedad cuando no estamos en la pared
        }
    }

    void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;  // Determina la dirección del salto dependiendo de la pared
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);  // Salto en la dirección opuesta

            wallJumpingCounter = 0f;

            // Cambiar la dirección del personaje
            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;

                Invoke(nameof(StopWallJumping), wallJumpingDuration);  // Detener el salto de pared
            }
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;  // Detener el salto en la pared
    }

    void Flip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
        }
    }

    void Jump()
    {
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            pState.jumping = false;
        }

        if (!pState.jumping)
        {
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);
                pState.jumping = true;
            }
            else if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
            {
                pState.jumping = true;
                airJumpCounter++;
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);
            }
        }
    }

    void UpdateJumpVariables()
    {
        if (Grounded())
        {
            pState.jumping = false;  // El jugador no está saltando si está en el suelo
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;  // Se restablece el contador cuando se presiona el salto
        }
        else if (jumpBufferCounter > 0)
        {
            jumpBufferCounter--;  // Solo decrementa si el contador es mayor que 0
        }
    }
}
