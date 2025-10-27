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
    [SerializeField] GameObject dashEffect;

    [Header("Wall Mechanics")]
    [SerializeField] private float wallSlidingSpeed = 2f;  // Velocidad de deslizamiento
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Transform wallCheck;
    private bool isWallSliding;  // Si está deslizándose por la pared

    public static CA_PlayerController Instance;
    CA_PlayerStateList pState;

    // 🔒 Lock tras wall-jump y bloqueo de regrapheo
    [SerializeField] private float wallJumpInputLock = 0.14f; // tiempo corto para recuperar control
    [SerializeField] private float wallRegrabBlock = 0.10f;   // bloqueo breve para re-agarrar misma pared
    private float wallJumpLockTimer = 0f;                     // bloquea input horizontal/flip tras wall-jump
    private float wallRegrabBlockTimer = 0f;                  // evita re-agarrar pared inmediatamente
    private int lastWallSide = 0;                             // -1 izquierda, +1 derecha, 0 desconocido

    private bool wallJumpArmed = true;
    private bool wasWallSliding = false;
    //ATTACKPLAYER
    float timeSinceAttack;
    [Header("Attacking")]
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] float damage;
    bool canAttack = true;
    bool attackPressed = false;
    [SerializeField] GameObject slashEffect;
    [SerializeField] float timeBetweenAttack = 0.3f;

    [Space(5)]
    [Header("Recoil")]
    [SerializeField] float recoilXSpeed = 100;
    [SerializeField] float recoilYSpeed = 100;
    int stepsXRecoiled, stepsYRecoiled;
    [SerializeField] float recoilDuration = 0.15f; // duración del recoil
    float recoilTimer = 0f;
    private float lastRecoilDirection = 0f;

    private float gravity;
    private bool isFacingRight = true;

    [Header("Desbloqueo de habilidades")]
    public bool canUseDash = false;
    public bool canUseWallJump = false;
    public bool canUseDoubleJump = false;


    // --- VFX: Climb / Wall-Jump Burst ---
    [Header("VFX Climb")]
    [SerializeField] private ParticleSystem climbGrabBurst;   // tu Particle System (Stretched Billboard)
    [SerializeField] private Transform burstAnchor;           // punto (mano/pecho) donde sale el chorro
    [SerializeField] private Vector2 volXRange = new Vector2(2.4f, 3.6f); // empuje hacia atrás
    [SerializeField] private Vector2 volYRange = new Vector2(0.8f, 1.4f); // leve lift hacia arriba
    [SerializeField] private int burstCount = 14;

    private NF_PlayerHealth playerHealthScript;
    [SerializeField] private float invulnerabilityTime = 1f; // Tiempo en segundos sin recibir daño tras tocar obstáculo
    private bool isInvulnerable = false;

    private Animator anim;
    [SerializeField] private float landingMinAirTime = 0.08f; // tiempo mínimo en el aire para permitir landing
    [SerializeField] private float fallThreshold = -0.05f;     // umbral para considerar que está cayendo
    private bool wasGrounded = true;
    private float airTime = 0f;
    private bool prevGrounded = false;
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
        playerHealthScript = GetComponent<NF_PlayerHealth>();
        anim = GetComponentInChildren<Animator>();

        gravity = rb.gravityScale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (SideAttackTransform) Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        if (UpAttackTransform) Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        if (DownAttackTransform) Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    void Update()
    {
        GetInputs();
        UpdateJumpVariables();

        if (pState.dashing) return;

        if (wallJumpLockTimer <= 0f)
            Move();

        StartDash();

        // 1) Primero determinamos si estamos en pared
        WallSlide();

        // 2) Si suelta Jump estando en pared, armamos el wall-jump
        if (isWallSliding && Input.GetButtonUp("Jump"))
            wallJumpArmed = true;

        // 3) Intento de wall-jump (requiere nueva pulsación + gate armado)
        WallJump();

        // 4) Recién ahora el salto normal/air (y adentro de Jump() ya bloqueamos si estamos en pared)
        Jump();

        if (!isWallSliding && wallJumpLockTimer <= 0f && !pState.recoillingX)
            Flip();
        UpdateAnimatorState();
        Attack();
        Recoil();

        if (wallJumpLockTimer > 0f) wallJumpLockTimer -= Time.deltaTime;
        if (wallRegrabBlockTimer > 0f) wallRegrabBlockTimer -= Time.deltaTime;
        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        bool grounded = Grounded();
        bool falling = !grounded && rb.velocity.y < -0.1f;
        if (anim != null)
        {
            anim.SetBool("IsFalling", falling);
        }

        // 3) Aterrizaje (solo una vez cuando toca suelo)
        if (anim != null && grounded && !wasGrounded)
        {
            anim.ResetTrigger("Jump");
            anim.SetBool("IsFalling", false);
            anim.SetTrigger("Land");
        }
        wasGrounded = grounded;

    }
    void UpdateAnimatorState()
    {
        if (anim == null) return;

        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        bool grounded = Grounded();

        if (!grounded)
        {
            // 🪂 Si no está en el suelo, controla salto y caída
            if (wasGrounded)
            {
                wasGrounded = false;
                airTime = 0f;
            }

            airTime += Time.deltaTime;

            if (rb.velocity.y < -0.05f && !anim.GetBool("IsFalling"))
            {
                anim.SetBool("IsFalling", true);
            }
        }
        else
        {
            // ⬇️ Aterrizaje (solo si estaba en el aire antes)
            if (!wasGrounded && airTime > 0.05f)
            {
                anim.SetBool("IsFalling", false);
                anim.ResetTrigger("Jump");
                anim.SetTrigger("Land");
            }

            // 🧍‍♂️ Si está en el suelo, decide entre idle o walk
            if (Mathf.Abs(rb.velocity.x) < 0.05f)
            {
                // Detenido en el suelo = Idle0
                anim.SetFloat("Speed", 0f);
            }
            else
            {
                // En movimiento = Idle (caminar)
                anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            }

            wasGrounded = true;
            airTime = 0f;
        }
    }



    private void FixedUpdate()
    {
        // ⛔ No sobrescribir velocidad mientras haya dash o recoil lateral
        if (pState.dashing || pState.recoillingX) return;

        if (!isWallSliding && wallJumpLockTimer <= 0f)
        {
            rb.velocity = new Vector2(xAxis * walkSpeed, rb.velocity.y);
        }
    }


    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");

        // Solo marca que se presionó el botón (una vez)
        if (Input.GetKeyDown(KeyCode.X))
        {
            attackPressed = true;
        }
    }

    private void Move()
    {
        if (pState.dashing || pState.recoillingX) return;
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
    }


    void StartDash()
    {
        if (canUseDash && Input.GetKeyDown(KeyCode.C) && canDash && !dashed && !isWallSliding)
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
        canDash = false;           // Deshabilitar el dash temporalmente
        pState.dashing = true;     // Establecer que está en dash
        rb.gravityScale = 0;       // Eliminar la gravedad durante el dash

        // Dirección: usa input si hay, si no usa facing actual
        rb.velocity = new Vector2(Mathf.Sign(xAxis == 0 ? transform.localScale.x : xAxis) * dashSpeed, 0);

        Instantiate(dashEffect, transform);
        yield return new WaitForSeconds(dashTime);  // Duración del dash

        rb.gravityScale = gravity;  // Restaurar la gravedad
        pState.dashing = false;     // Terminar el estado de dash

        yield return new WaitForSeconds(dashCooldown);  // Cooldown
        canDash = true;            // Habilitar el dash nuevamente
    }

    // ====== Ataque ======
    void Attack()
    {
        if (!attackPressed || !canAttack) return;

        // Consumimos la entrada
        attackPressed = false;
        canAttack = false;

        // --- Ataque lateral ---
        if (yAxis == 0 || (yAxis < 0 && Grounded()))
        {
            Hit(SideAttackTransform, SideAttackArea, ref pState.recoillingX, recoilXSpeed);
            Instantiate(slashEffect, SideAttackTransform);
        }
        // --- Ataque hacia arriba ---
        else if (yAxis > 0)
        {
            Collider2D[] hitObjects = Physics2D.OverlapBoxAll(UpAttackTransform.position, UpAttackArea, 0, attackableLayer);
            foreach (Collider2D obj in hitObjects)
            {
                if (obj.GetComponent<CA_RecolEnemy>() != null)
                    obj.GetComponent<CA_RecolEnemy>().EnemyHit(damage, (transform.position - obj.transform.position).normalized, recoilYSpeed);
            }
            SlashEffectAtAngle(slashEffect, 80, UpAttackTransform, true);
        }
        // --- Ataque hacia abajo ---
        else if (yAxis < 0 && !Grounded())
        {
            Hit(DownAttackTransform, DownAttackArea, ref pState.recoillingY, recoilYSpeed);
            SlashEffectAtAngle(slashEffect, -80, DownAttackTransform, true);
        }

        StartCoroutine(ResetAttackCooldown());
    }

    IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(timeBetweenAttack);
        canAttack = true;
    }

    void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);

        if (objectsToHit.Length > 0)
        {
            _recoilDir = true;

            // ⚡ Si el ataque fue hacia abajo, restaurar el doble salto
            if (yAxis < 0 && !Grounded())
            {
                airJumpCounter = 0;
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, 8f));
            }

            // ✅ Guardar dirección de recoil en base al enemigo golpeado
            float hitDirection = Mathf.Sign(transform.position.x - objectsToHit[0].transform.position.x);
            lastRecoilDirection = hitDirection;
        }

        for (int i = 0; i < objectsToHit.Length; i++)
        {
            CA_RecolEnemy enemy = objectsToHit[i].GetComponent<CA_RecolEnemy>();
            if (enemy != null)
            {
                enemy.EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength);
            }
        }
    }

    void SlashEffectAtAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform, bool resetScale = false)
    {
        GameObject effectInstance = Instantiate(_slashEffect, _attackTransform.position, Quaternion.identity, _attackTransform);
        effectInstance.transform.eulerAngles = new Vector3(0, 0, _effectAngle);

        if (resetScale)
        {
            effectInstance.transform.localScale = new Vector3(0.3f, 0.26f, 0.26f);
        }
        else
        {
            effectInstance.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
        }
    }

    void Recoil()
    {
        if (!pState.recoillingX && !pState.recoillingY) return;

        // --- Recoil lateral ---
        if (pState.recoillingX)
        {
            // ✅ Usamos la dirección real del último enemigo golpeado
            float direction = lastRecoilDirection != 0 ? lastRecoilDirection : (isFacingRight ? -1f : 1f);
            rb.velocity = new Vector2(direction * 5f, rb.velocity.y);
        }

        // --- Recoil vertical solo si fue un ataque hacia abajo ---
        if (pState.recoillingY && yAxis < 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 8f);
        }

        // ⏳ Duración del recoil
        recoilTimer += Time.deltaTime;
        if (recoilTimer >= recoilDuration)
        {
            pState.recoillingX = false;
            pState.recoillingY = false;
            recoilTimer = 0;
            lastRecoilDirection = 0f;
        }
    }

    // ====== Pared / Wall ======
    private bool IsWalled()
    {
        // Verifica si estamos tocando una pared usando OverlapCircle
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    void WallSlide()
    {
        bool enteringSlide = false;

        // Bloquea re-grapheo de pared por un instante tras wall-jump
        if (wallRegrabBlockTimer <= 0f && IsWalled() && !Grounded() && xAxis != 0f)
        {
            if (!isWallSliding) enteringSlide = true;

            // Determinar lado de la pared con raycasts cortos
            Vector2 pos = transform.position;
            float dist = 0.6f;

            RaycastHit2D hitR = Physics2D.Raycast(pos, Vector2.right, dist, wallLayer);
            RaycastHit2D hitL = Physics2D.Raycast(pos, Vector2.left, dist, wallLayer);

            if (hitR.collider != null) lastWallSide = +1;     // pared a la derecha
            else if (hitL.collider != null) lastWallSide = -1;// pared a la izquierda

            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, -wallSlidingSpeed);
            rb.gravityScale = 0;  // Eliminar la gravedad durante el deslizamiento

            // --- Gate del wall-jump al ENTRAR al slide ---
            if (enteringSlide)
            {
                // Si entras con Jump apretado, desarma el wall-jump hasta que sueltes.
                wallJumpArmed = !Input.GetButton("Jump");

                // ❗ Limpia el buffer de salto para que no dispare un salto “fantasma”
                jumpBufferCounter = 0;
            }

        }
        else
        {
            isWallSliding = false;
            rb.gravityScale = gravity;  // Restaurar la gravedad cuando no estamos en la pared
        }

        wasWallSliding = isWallSliding;
    }


    void WallJump()
    {
        if (!canUseWallJump) return;

        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingCounter = wallJumpingTime;
            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (wallJumpArmed && Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            // Asegurar que conocemos el lado de la pared
            if (lastWallSide == 0)
            {
                float dx = wallCheck ? (wallCheck.position.x - transform.position.x) : 0f;
                lastWallSide = (dx > 0f) ? +1 : -1;
            }

            isWallJumping = true;

            // Efecto partículas: normal opuesta al lado de la pared
            Vector2 wn = (lastWallSide == +1) ? Vector2.left : Vector2.right;
            PlayClimbBurstForced(wn);

            // Dirección de salto: SIEMPRE al lado contrario de la pared
            float dir = -lastWallSide;

            // Impulso del wall-jump
            rb.velocity = new Vector2(dir * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            // 🔒 Bloqueos cortos
            wallJumpLockTimer = wallJumpInputLock;   // lock corto de input/flip
            wallRegrabBlockTimer = wallRegrabBlock;  // evita re-agarrar pared de inmediato

            // Flip visual coherente con dir
            Vector3 ls = transform.localScale;
            ls.x = Mathf.Abs(ls.x) * (dir > 0 ? 1f : -1f);
            transform.localScale = ls;
            isFacingRight = dir > 0;

            // Evitar reenganche inmediato
            Invoke(nameof(StopWallJumping), wallJumpingDuration);
            wallJumpArmed = false;
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;  // Detener el salto en la pared
    }

    // ====== Utilidades varias ======
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
        if (isWallSliding || IsWalled()) return;

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            pState.jumping = false;
        }

        if (!pState.jumping)
        {
            // 🔹 Salto desde el suelo
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                pState.jumping = true;
                jumpBufferCounter = 0;

                if (anim != null)
                {
                    anim.ResetTrigger("Land");
                    anim.ResetTrigger("Jump");
                    anim.SetTrigger("Jump");
                    anim.SetBool("IsFalling", false);
                }

                wasGrounded = false;
                airTime = 0f;
            }
            // 🔹 Doble salto
            else if (canUseDoubleJump && !Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
            {
                airJumpCounter++;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                pState.jumping = true;

                if (anim != null)
                {
                    anim.ResetTrigger("Land");
                    anim.ResetTrigger("Jump");
                    anim.SetTrigger("Jump");
                    anim.SetBool("IsFalling", false);
                }

                wasGrounded = false;
                airTime = 0f;
            }
        }
    }



    void UpdateJumpVariables()
    {
        if (Grounded())
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else if (jumpBufferCounter > 0)
        {
            jumpBufferCounter--;
        }
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

    // ====== Burst util ======
    bool TryGetWallNormal(out Vector2 wallNormal)
    {
        wallNormal = Vector2.zero;
        float dist = 0.6f; // un poco más largo para no fallar
        Vector2 pos = transform.position;

        // derecha
        var hitR = Physics2D.Raycast(pos, Vector2.right, dist, wallLayer);
        if (hitR.collider != null) { wallNormal = hitR.normal; return true; }

        // izquierda
        var hitL = Physics2D.Raycast(pos, Vector2.left, dist, wallLayer);
        if (hitL.collider != null) { wallNormal = hitL.normal; return true; }

        // fallback por posición del wallCheck
        if (wallCheck != null)
        {
            float dx = wallCheck.position.x - pos.x;
            if (Mathf.Abs(dx) > 0.05f)
            {
                wallNormal = dx > 0 ? Vector2.left : Vector2.right;
                return true;
            }
        }
        return false;
    }

    int _lastBurstFrame = -999;

    void PlayClimbBurstForced(Vector2 wallNormal)
    {
        if (!climbGrabBurst) { Debug.LogWarning("climbGrabBurst no asignado"); return; }
        if (_lastBurstFrame == Time.frameCount) return;      // evita doble disparo en el mismo frame
        _lastBurstFrame = Time.frameCount;

        // Posición/orientación del sistema
        var t = climbGrabBurst.transform;
        t.position = burstAnchor ? burstAnchor.position : transform.position;
        t.up = -wallNormal.normalized;

        // Velocity over Lifetime OFF
        var vol = climbGrabBurst.velocityOverLifetime;
        vol.enabled = false;

        // Limpia, arranca y emite N partículas con velocidad por-partícula
        climbGrabBurst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        climbGrabBurst.Play();

        int count = Random.Range(burstCount - 2, burstCount + 2);

        // Dirección base (opuesta a la pared)
        Vector2 baseDir = (-wallNormal).normalized;

        for (int i = 0; i < count; i++)
        {
            var ep = new ParticleSystem.EmitParams();
            ep.applyShapeToPosition = true;
            ep.position = t.position;

            float back = Random.Range(volXRange.x, volXRange.y);  // 2.4–3.6
            float lift = Random.Range(volYRange.x, volYRange.y);  // 0.8–1.4

            Vector2 v = baseDir * back + Vector2.up * lift;
            ep.velocity = v;
            climbGrabBurst.Emit(ep, 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle") && !isInvulnerable)
        {
            StartCoroutine(HandleObstacleCollision());
        }
        else if (collision.CompareTag("CheckpointParkour"))
        {
            NF_GameController gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<NF_GameController>();
            gc.UpdateCheckpoint(collision.transform.position, "Parkour");
        }
    }

    private IEnumerator HandleObstacleCollision()
    {
        isInvulnerable = true; // ✅ Evita recibir más daño por un momento
        playerHealthScript.TakeDamage(1);

        if (playerHealthScript.currentHealth > 0)
        {
            NF_GameController gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<NF_GameController>();
            StartCoroutine(gc.Respawn(0.5f, "Parkour"));
        }

        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false; // ✅ Vuelve a permitir recibir daño
    }
}
