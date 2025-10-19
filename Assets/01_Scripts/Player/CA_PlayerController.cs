using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CA_PlayerController : MonoBehaviour
{
    [Header("Movimiento Horizontal")]
    private Rigidbody2D rb;
    [SerializeField] private float walkSpeed = 1;
    private float xAxis, yAxis;

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

    //ATTACKPLAYER
    bool attack = false;
    float timeBetweenAttack, timeSinceAttack;
    [Header("Attacking")]
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] float damage;
    [Space(5)]
    [Header("Recoil")]
    [SerializeField] int recoilXSteps = 5;
    [SerializeField] int recoilYSteps = 5;
    [SerializeField] float recoilXSpeed = 100;
    [SerializeField] float recoilYSpeed = 100;
    int stepsXRecoiled, stepsYRecoiled;
    [SerializeField] float recoilDuration = 0.15f; // duración del recoil
    float recoilTimer = 0f;

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
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
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
        Attack();
        Recoil();
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");  // Obtener la entrada horizontal (teclas de dirección)
        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetKeyDown(KeyCode.X);
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

    //New Script Attack
    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if (attack && timeSinceAttack >= timeBetweenAttack)
        {
            timeSinceAttack = 0;
            //Colocar Anim Attack
            if (yAxis == 0 || yAxis < 0 && Grounded())
            {
                Hit(SideAttackTransform, SideAttackArea, ref pState.recoillingX, recoilXSpeed);
            }
            else if (yAxis > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoillingY, recoilYSpeed);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoillingY, recoilYSpeed);
            }
        }
    }
    void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);

        if (objectsToHit.Length > 0)
        {
            _recoilDir = true;
        }

        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<CA_RecolEnemy>() != null)
            {
                objectsToHit[i].GetComponent<CA_RecolEnemy>().EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength);
            }

            // ✅ NUEVO: Detectar enemigos de rebote
            if (objectsToHit[i].GetComponent<EnemigoRebote>() != null)
            {
                objectsToHit[i].GetComponent<EnemigoRebote>().RecibirAtaque();
            }
        }
    }
    void Recoil()
    {
        // Si no está en recoil, no hacemos nada
        if (!pState.recoillingX && !pState.recoillingY) return;

        // Aplicar fuerza de retroceso
        if (pState.recoillingX)
        {
            float direction = isFacingRight ? -1f : 1f;
            rb.velocity = new Vector2(direction * 5f, rb.velocity.y);
        }

        if (pState.recoillingY)
        {
            rb.velocity = new Vector2(rb.velocity.x, 8f);
        }

        // ?? Limitar duración del recoil
        recoilTimer += Time.deltaTime;
        if (recoilTimer >= recoilDuration)
        {
            // Desactivar recoil después del tiempo
            pState.recoillingX = false;
            pState.recoillingY = false;
            recoilTimer = 0;
        }
    }

    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoillingX = false;
    }
    void StopRecoilY()
    {
        stepsXRecoiled = 0;
        pState.recoillingY = false;
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

        if (Input.GetKeyDown(KeyCode.Z) && wallJumpingCounter > 0f)
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
            pState.lookingRight = false;
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
            pState.lookingRight = true;
        }
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Z) && rb.velocity.y > 0)
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
            else if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetKeyDown(KeyCode.Z))
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

        if (Input.GetKeyDown(KeyCode.Z))
        {
            jumpBufferCounter = jumpBufferFrames;  // Se restablece el contador cuando se presiona el salto
        }
        else if (jumpBufferCounter > 0)
        {
            jumpBufferCounter--;  // Solo decrementa si el contador es mayor que 0
        }
    }
}