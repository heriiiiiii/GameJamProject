using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;

public class CA_PlayerController : MonoBehaviour
{
    [Header("🔊 Audio del Jugador")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip dashClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip jumpClip;

    private bool isWalkingSoundPlaying = false;

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
    private int attackIndex = 0;
    private float comboResetTime = 0.8f; // tiempo máximo entre ataques para mantener el combo
    private float lastAttackTime = 0f;
    [SerializeField] float comboBufferWindow = 0.25f; // ventana para “guardar” el input
    bool queuedAttack = false;
    float bufferTimer = 0f;
    [SerializeField] float idleResetTime = 0.6f; // tiempo sin presionar para volver a idle
    float attackIdleTimer = 0f;
    bool isAttacking = false;
    [SerializeField] float chainWindowStart = 0.35f;  // desde el 35% de la animación
    [SerializeField] float chainWindowEnd = 0.80f;  // hasta el 80% de la animación
    [SerializeField] float postAttackIdleDelay = 0.10f; // margen al final antes de forzar Idle

    bool isInAttack;      // indica si estás en un clip de ataque
    bool canChainWindow;

    [Space(5)]
    [Header("Recoil")]
    [SerializeField] float recoilXSpeed = 100;
    [SerializeField] float recoilYSpeed = 100;
    int stepsXRecoiled, stepsYRecoiled;
    [SerializeField] float recoilDuration = 0.15f; // duración del recoil
    float recoilTimer = 0f;
    private float lastRecoilDirection = 0f;

    private float gravity;
    public bool isFacingRight = true;

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

    private float idleTimer = 0f;
    private bool longIdlePlayed = false;
    [SerializeField] private float longIdleDelay = 6f; // tiempo quieto antes del longidle
    [SerializeField] private bool debugLongIdle = true;

    bool isCompletelyIdle = true;

    private NF_Knockback knockback;

    [Header("Death Settings")]
    [SerializeField] private float deathAnimationDuration = 1.5f; // duración del clip "Death"
    private bool isDead = false;
    [Header("Camera Stuff")]
    [SerializeField] private GameObject _cameraFollowGO;

    private NF_CameraFollowOBJECT _cameraFollowObject;
    public float _fallSpeedYDampingChangeThreshold;


    [SerializeField] private NF_DeathTransition deathTransition;

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
        knockback = GetComponent<NF_Knockback>();
        gravity = rb.gravityScale;
        _cameraFollowObject = _cameraFollowGO.GetComponent<NF_CameraFollowOBJECT>();
        _fallSpeedYDampingChangeThreshold = NF_CameraManager.instance._fallSpeedYDampingChangeThreshold;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

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
        if (knockback.IsBeingKnockedBack)
        {

        }
        GetInputs();
        UpdateJumpVariables();

        if (pState.dashing) return;

        if (wallJumpLockTimer <= 0f)
            Move();

        StartDash();

        // 1) Primero determinamos si estamos en pared
        WallSlide();

        bool touchingWall = IsWalled() && !Grounded();
        anim.SetBool("OnWall", touchingWall);

        // --- WALL INTERACTION (solo si la habilidad está desbloqueada) ---
        touchingWall = false;

        if (canUseWallJump)
        {
            touchingWall = IsWalled() && !Grounded();
            anim.SetBool("OnWall", touchingWall);

            if (touchingWall)
            {
                // Si el jugador presiona hacia arriba
                if (Input.GetAxisRaw("Vertical") > 0f)
                {
                    anim.SetBool("Climb", true);

                    // Movimiento vertical controlado (puedes ajustar la velocidad)
                    rb.velocity = new Vector2(rb.velocity.x, 3.5f);
                }
                else
                {
                    anim.SetBool("Climb", false);

                    // Movimiento de anclaje / deslizamiento lento
                    rb.velocity = new Vector2(rb.velocity.x, -1.5f);
                }

                // 🔹 Anclar el cuerpo en la pared
                rb.gravityScale = 0f;
            }
            else
            {
                anim.SetBool("Climb", false);
                rb.gravityScale = gravity;
            }
        }
        else
        {
            // 🚫 Sin wall abilities: asegura que se comporta normalmente
            anim.SetBool("OnWall", false);
            anim.SetBool("Climb", false);
            rb.gravityScale = gravity;
        }

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
        // ================== 💤 LONG IDLE HANDLER ==================
        AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo(0);

        // 1️⃣ Detectar si está completamente idle
        bool isCompletelyIdleNow = Grounded() &&
                                   Mathf.Abs(rb.velocity.x) < 0.05f &&
                                   !isAttacking &&
                                   !pState.dashing &&
                                   !pState.recoillingX &&
                                   !pState.recoillingY &&
                                   !Input.anyKey;

        // 2️⃣ Si está completamente quieto y realmente en el estado base Idle
        if (isCompletelyIdleNow && currentState.IsName("HV_idle 0"))
        {
            idleTimer += Time.deltaTime;

            // Cuando pasa el tiempo de espera
            if (idleTimer >= longIdleDelay)
            {
                anim.ResetTrigger("LongIdle");
                anim.SetTrigger("LongIdle");
                idleTimer = 0f;
                longIdlePlayed = true; // 🔹 marcamos que ya se ejecutó este ciclo

                if (debugLongIdle)
                    Debug.Log("▶️ LongIdle ejecutado");
            }
        }
        else
        {
            // Si hace cualquier acción o sale del idle, reinicia
            idleTimer = 0f;

            // 🔸 Si ya terminó el longidle y vuelve a Idle, permitir reproducirlo otra vez
            if (currentState.IsName("HV_idle 0") && longIdlePlayed)
            {
                longIdlePlayed = false;
                if (debugLongIdle)
                    Debug.Log("🔁 LongIdle reseteado (puede reproducirse otra vez)");
            }
        }

        // 3️⃣ Al terminar la animación de longidle, forzar retorno a Idle limpio
        if (currentState.IsName("HV_longidle") && currentState.normalizedTime >= 1f)
        {
            anim.ResetTrigger("LongIdle");
            anim.Play("HV_idle 0", 0, 0f);
        }


        var st = anim.GetCurrentAnimatorStateInfo(0);
        isInAttack = IsAttackState(st);

        if (isInAttack)
        {
            // t en [0..1] dentro del ciclo actual del clip
            float t = st.normalizedTime % 1f;

            // Dentro de la ventana de encadenamiento
            canChainWindow = (t >= chainWindowStart && t <= chainWindowEnd);

            // Si llegamos al final del clip…
            if (t >= (1f - postAttackIdleDelay))
            {
                if (queuedAttack)                 // había input guardado
                {
                    attackPressed = true;         // dispara el siguiente golpe
                    queuedAttack = false;
                }
                else if (t >= 1f)                 // se acabó y no hubo input
                {
                    // Forzar vuelta limpia a Idle y resetear combo
                    ResetCombo();
                    anim.ResetTrigger("Attack");
                    anim.Play("HV_idle 0", 0, 0f);
                    isAttacking = false;
                    canChainWindow = false;
                }
            }
        }
        else
        {
            canChainWindow = false;
        }

        Attack();
        Recoil();

        if (isAttacking)
        {
            attackIdleTimer += Time.deltaTime;

            // Si no presionas más en ese tiempo, vuelve a Idle y resetea combo
            if (attackIdleTimer >= idleResetTime && !attackPressed && attackIndex == 1)
            {
                ResetCombo();
                anim.ResetTrigger("Attack");
                anim.Play("HV_idle 0", 0, 0f);
                isAttacking = false;
            }
        }
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
            anim.ResetTrigger("DoubleJump");
            anim.SetBool("IsFalling", false);
            anim.SetTrigger("Land");
        }
        wasGrounded = grounded;

    }
    // 🔓 Habilita o deshabilita TODAS las mecánicas relacionadas con pared
    public void SetWallJumpAbilities(bool enabled)
    {
        canUseWallJump = enabled;

        if (enabled)
        {
            Debug.Log("🧗 Wall abilities habilitadas: Wall Slide + Wall Jump activos");

            // Asegura que pueda interactuar con capas de pared
            rb.gravityScale = gravity; // restablece gravedad por si estaba bloqueada
            isWallSliding = false;
            wallJumpArmed = true;

            // opcional: resetear animaciones relacionadas
            anim.SetBool("OnWall", false);
            anim.SetBool("Climb", false);
        }
        else
        {
            Debug.Log("🚫 Wall abilities deshabilitadas");
            isWallSliding = false;
            anim.SetBool("OnWall", false);
            anim.SetBool("Climb", false);
        }
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

        if (rb.velocity.y < _fallSpeedYDampingChangeThreshold && !NF_CameraManager.instance.IsLerpingYDamping && !NF_CameraManager.instance.LerpedFromPlayerFalling)
        {
            NF_CameraManager.instance.LerpYDamping(true);
        }
        if (rb.velocity.y >= 0f && !NF_CameraManager.instance.IsLerpingYDamping && NF_CameraManager.instance.LerpedFromPlayerFalling)
        {
            NF_CameraManager.instance.LerpedFromPlayerFalling = false;
            NF_CameraManager.instance.LerpYDamping(false);
        }
    }
    private void FixedUpdate()
    {
        // 🚫 No sobrescribir velocidad si está dashing o en recoil lateral
        if (pState.dashing || pState.recoillingX)
            return;

        // ✅ Movimiento horizontal normal
        if (!isWallSliding && wallJumpLockTimer <= 0f)
        {
            rb.velocity = new Vector2(xAxis * walkSpeed, rb.velocity.y);
        }

        // 🔽 Deslizamiento por pared (mantiene la caída controlada)
        if (isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, -wallSlidingSpeed);
        }

        bool isMovingOnGround = Grounded() && Mathf.Abs(rb.velocity.x) > 0.1f;

        if (isMovingOnGround && !isWalkingSoundPlaying)
        {
            if (walkClip != null)
            {
                audioSource.clip = walkClip;
                audioSource.loop = true;
                audioSource.Play();
                isWalkingSoundPlaying = true;
            }
        }
        else if (!isMovingOnGround && isWalkingSoundPlaying)
        {
            audioSource.Stop();
            isWalkingSoundPlaying = false;
        }

    }


    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.X))
        {
            // Si estamos libres para atacar: atacar YA
            if (canAttack && (!isInAttack || canChainWindow))
            {
                attackPressed = true;
            }
            else
            {
                // Guardar input para el siguiente hueco (buffer)
                queuedAttack = true;
                bufferTimer = comboBufferWindow;   // ya lo tienes declarado
            }
        }

    }

    // --- FX flags ---
    private bool _doubleJumpFXFlag = false;
    public bool ConsumeDoubleJumpFXFlag()
    {
        if (_doubleJumpFXFlag)
        {
            _doubleJumpFXFlag = false;
            return true;
        }
        return false;
    }

    private void Move()
    {
        if (pState.dashing || pState.recoillingX) return;
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
    }
    void StartDash()
    {

        // 🔒 Evita relanzar si ya estás en dash (pState.dashing) o si el cooldown está activo
        if (canUseDash && Input.GetKeyDown(KeyCode.C) && canDash && !pState.dashing && !isWallSliding)
        {
            canDash = false;          // bloquea YA (no esperes al coroutine)
            StartCoroutine(Dash());
            // NO uses 'dashed' para animación; si quieres conservarlo para lógica de “una vez por salto”, lo puedes dejar,
            // pero no lo uses para bloquear la animación.
            dashed = true;
        }

        if (Grounded())
        {
            // opcional: si quieres permitir un dash por salto, aquí reseteas 'dashed'
            dashed = false;
        }
    }
    IEnumerator Dash()
    {
        pState.dashing = true;
        if (dashClip) audioSource.PlayOneShot(dashClip, 1f);
        float prevGrav = rb.gravityScale;
        rb.gravityScale = 0f;

        // Dirección del dash (usa input o facing)
        float dir = Mathf.Sign(xAxis == 0 ? transform.localScale.x : xAxis);
        rb.velocity = new Vector2(dir * dashSpeed, 0f);

        // 🎬 Activar animación solo una vez
        anim.ResetTrigger("Dash");
        anim.SetTrigger("Dash");

        // ✨ Instanciar dashEffect detrás del jugador, orientado correctamente
        if (dashEffect)
        {
            float offsetX = -0.6f * dir; // 🔹 retrasa el efecto detrás del jugador
            Vector3 spawnPos = transform.position + new Vector3(offsetX, 0f, 0f);

            // Crear el efecto
            GameObject effect = Instantiate(dashEffect, spawnPos, Quaternion.identity);

            // 🔄 Alinear su escala según la dirección del dash
            Vector3 scale = effect.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * dir;
            effect.transform.localScale = scale;

            // 🔥 (Opcional) destruir efecto después de 1s para evitar acumulación
            Destroy(effect, 1f);
        }

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = prevGrav;
        pState.dashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }


    // ====== Ataque ======
    void Attack()
    {
        // 🚫 Si no presionó ataque o está en cooldown, salir
        if (!attackPressed || !canAttack) return;
        attackPressed = false;

        // 🔄 Reinicia el combo si pasó demasiado tiempo sin atacar
        if (Time.time - lastAttackTime > comboResetTime)
            attackIndex = 0;

        // Avanza al siguiente golpe del combo
        attackIndex++;
        if (attackIndex > 3) attackIndex = 1;

        // 🔹 Ataque hacia arriba
        if (yAxis > 0)
        {
            // 🧠 Daño a enemigos
            Collider2D[] hitObjects = Physics2D.OverlapBoxAll(UpAttackTransform.position, UpAttackArea, 0, attackableLayer);
            foreach (Collider2D obj in hitObjects)
            {
                CA_RecolEnemy enemy = obj.GetComponent<CA_RecolEnemy>();
                if (enemy != null)
                    enemy.EnemyHit(damage, (transform.position - obj.transform.position).normalized, recoilYSpeed);
            }

            // ⚙️ Retroceso del jugador
            Hit(UpAttackTransform, UpAttackArea, ref pState.recoillingY, recoilYSpeed);

            // ✨ Efecto visual
            SlashEffectAtAngle(slashEffect, 80, UpAttackTransform, true);
        }

        // 🔹 Ataque hacia abajo (pogo)
        else if (yAxis < 0 && !Grounded())
        {
            Collider2D[] hitObjects = Physics2D.OverlapBoxAll(DownAttackTransform.position, DownAttackArea, 0, attackableLayer);
            foreach (Collider2D obj in hitObjects)
            {
                CA_RecolEnemy enemy = obj.GetComponent<CA_RecolEnemy>();
                if (enemy != null)
                    enemy.EnemyHit(damage, (transform.position - obj.transform.position).normalized, recoilYSpeed);
            }

            Hit(DownAttackTransform, DownAttackArea, ref pState.recoillingY, recoilYSpeed);
            SlashEffectAtAngle(slashEffect, -80, DownAttackTransform, true);
        }

        // 🔹 Ataque lateral (frontal)
        else
        {
            Collider2D[] hitObjects = Physics2D.OverlapBoxAll(SideAttackTransform.position, SideAttackArea, 0, attackableLayer);
            foreach (Collider2D obj in hitObjects)
            {
                CA_RecolEnemy enemy = obj.GetComponent<CA_RecolEnemy>();
                if (enemy != null)
                    enemy.EnemyHit(damage, (transform.position - obj.transform.position).normalized, recoilXSpeed);
            }

            // ⚙️ Recoil del jugador (retroceso lateral)
            Hit(SideAttackTransform, SideAttackArea, ref pState.recoillingX, recoilXSpeed);

            Instantiate(slashEffect, SideAttackTransform.position, Quaternion.identity, SideAttackTransform);
        }

        // 🎬 Animaciones del combo
        anim.ResetTrigger("Attack");
        anim.SetInteger("AttackIndex", attackIndex);
        anim.SetTrigger("Attack");

        if (attackClip) audioSource.PlayOneShot(attackClip, 0.8f);

        // ⚙️ Estado interno
        canAttack = false;
        isAttacking = true;
        attackIdleTimer = 0f;

        // 🕒 Cooldown para siguiente ataque
        StartCoroutine(ResetAttackCooldown());

        lastAttackTime = Time.time;
    }



    bool IsAttackState(AnimatorStateInfo st)
    {
        // Usa los nombres EXACTOS de tus clips/estados
        return st.IsName("HV_atack1") || st.IsName("HV_atack2") || st.IsName("HV_atack3");
    }

    public void ResetCombo()
    {
        attackIndex = 0;
        anim.SetInteger("AttackIndex", 0);
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
    public bool IsWallSliding()
    {
        return isWallSliding;
    }

    void WallSlide()
    {
        if (!canUseWallJump) return;
        bool enteringSlide = false;

        // ✅ Detectar si está tocando pared y no está en el suelo
        if (wallRegrabBlockTimer <= 0f && IsWalled() && !Grounded() && Mathf.Abs(xAxis) > 0.05f)
        {
            if (!isWallSliding) enteringSlide = true;

            // 🔍 Detectar lado de la pared con raycasts cortos
            Vector2 pos = transform.position;
            float dist = 0.6f;

            RaycastHit2D hitR = Physics2D.Raycast(pos, Vector2.right, dist, wallLayer);
            RaycastHit2D hitL = Physics2D.Raycast(pos, Vector2.left, dist, wallLayer);

            if (hitR.collider != null)
                lastWallSide = +1; // pared a la derecha
            else if (hitL.collider != null)
                lastWallSide = -1; // pared a la izquierda
            else
                lastWallSide = 0;

            // 🔄 Ajustar orientación para mirar hacia la pared
            if (lastWallSide != 0)
            {
                Vector3 ls = transform.localScale;
                ls.x = Mathf.Abs(ls.x) * -lastWallSide; // mira hacia la pared
                transform.localScale = ls;
                isFacingRight = (ls.x > 0);
            }

            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, -wallSlidingSpeed);
            rb.gravityScale = 0f; // evita caída rápida

            // 🔹 Notificar animación
            anim.SetBool("OnWall", true);
            anim.SetBool("Climb", false);

            // --- Gate del wall-jump al ENTRAR ---
            if (enteringSlide)
            {
                wallJumpArmed = !Input.GetButton("Jump");
                jumpBufferCounter = 0;
            }
        }
        else
        {
            // ⛔ Dejar de deslizar
            if (isWallSliding)
            {
                anim.SetBool("OnWall", false);
                anim.SetBool("Climb", false);
            }

            isWallSliding = false;
            rb.gravityScale = gravity;
        }

        wasWallSliding = isWallSliding;
    }
    void WallJump()
    {
        if (!canUseWallJump) return;

        // 🔹 Si está deslizándose en pared
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

        // 🔹 Si presiona salto mientras puede saltar desde pared
        if (wallJumpArmed && Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            // 🧱 Verificar que realmente haya una pared válida
            if (lastWallSide == 0)
            {
                float dx = wallCheck ? (wallCheck.position.x - transform.position.x) : 0f;
                lastWallSide = (dx > 0f) ? +1 : -1;
            }

            // 🧭 Dirección opuesta a la pared
            float dir = -lastWallSide;

            // 🚀 Aplicar impulso de salto
            rb.velocity = new Vector2(dir * wallJumpingPower.x, wallJumpingPower.y);

            // 🔄 Ajustar orientación visual (mira hacia donde salta)
            Vector3 ls = transform.localScale;
            ls.x = Mathf.Abs(ls.x) * (dir > 0 ? 1 : -1);
            transform.localScale = ls;
            isFacingRight = dir > 0;

            // 💥 Efecto visual
            Vector2 wn = (lastWallSide == +1) ? Vector2.left : Vector2.right;
            PlayClimbBurstForced(wn);

            // 🔒 Lock de control para evitar flip inmediato
            wallJumpLockTimer = wallJumpInputLock;
            wallRegrabBlockTimer = wallRegrabBlock;

            // 🚫 Desactivar estado de slide / escalada
            anim.SetBool("OnWall", false);
            anim.SetBool("Climb", false);
            isWallSliding = false;

            // 🔐 Desarmar el gate para evitar doble salto
            wallJumpArmed = false;

            // ⏱️ Esperar un tiempo corto para liberar el control
            Invoke(nameof(StopWallJumping), wallJumpingDuration);
            StartCoroutine(ReleaseWallAfterJump());
        }
    }
    IEnumerator ReleaseWallAfterJump()
    {
        // 🔹 Desactiva temporalmente el wall slide y restaura gravedad
        isWallSliding = false;
        yield return new WaitForSeconds(0.05f); // delay de seguridad
        rb.gravityScale = gravity;
    }

    private void StopWallJumping()
    {
        isWallJumping = false;  // Detener el salto en la pared
    }

    // ====== Utilidades varias ======
    void Flip()
    {
        // Solo permitir el giro si hay movimiento horizontal real
        if (xAxis < 0 && isFacingRight)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
            isFacingRight = false;
            pState.lookingRight = false;

            // 🔹 Avisar a la cámara que el jugador giró
            _cameraFollowObject.CallTurn();
        }
        else if (xAxis > 0 && !isFacingRight)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
            isFacingRight = true;
            pState.lookingRight = true;

            // 🔹 Avisar a la cámara que el jugador giró
            _cameraFollowObject.CallTurn();
        }
    }


    void Jump()
    {

        if (isWallSliding || (IsWalled() && !Grounded())) return;


        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            pState.jumping = false;
        }

        if (!pState.jumping)
        {
            // 🔹 Primer salto desde el suelo
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                if (jumpClip) audioSource.PlayOneShot(jumpClip, 0.9f);
                pState.jumping = true;
                jumpBufferCounter = 0;

                if (anim != null)
                {
                    anim.ResetTrigger("DoubleJump");
                    anim.ResetTrigger("Land");
                    anim.ResetTrigger("Jump");
                    anim.SetTrigger("Jump"); // animación normal
                    anim.SetBool("IsFalling", false);
                }

                wasGrounded = false;
                airTime = 0f;

            }

            // 🔹 Segundo salto (doble salto)
            else if (canUseDoubleJump && !Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
            {
                airJumpCounter++;

                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                if (jumpClip) audioSource.PlayOneShot(jumpClip, 0.9f);
                pState.jumping = true;

                if (anim != null)
                {
                    anim.ResetTrigger("Jump");
                    anim.ResetTrigger("Land");
                    anim.ResetTrigger("DoubleJump");
                    anim.SetTrigger("DoubleJump"); // 🎯 doble salto
                    anim.SetBool("IsFalling", false);
                }

                wasGrounded = false;
                airTime = 0f;
                _doubleJumpFXFlag = true;  // <- avisar al ParticleController que hubo doble salto

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
            StartCoroutine(HandleObstacleCollision(collision));
        }
        else if (collision.CompareTag("CheckpointParkour"))
        {
            NF_GameController gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<NF_GameController>();
            gc.UpdateCheckpoint(collision.transform.position, "Parkour");
        }
    }
    public void Die()
    {
        if (isDead) return; // 🔒 evitar múltiples muertes
        isDead = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Animator anim = GetComponentInChildren<Animator>();
        NF_GameController gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<NF_GameController>();
        NF_DeathTransition transition = deathTransition; // referencia desde inspector

        // ⚙️ Detener completamente el movimiento
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // 🎬 Reproducir animación de muerte
        if (anim != null)
        {
            anim.ResetTrigger("Attack");
            anim.ResetTrigger("Jump");
            anim.ResetTrigger("DoubleJump");
            anim.ResetTrigger("Land");
            anim.ResetTrigger("Dash");
            anim.SetTrigger("DeathTrigger");
        }

        Debug.Log("☠️ Player ha muerto. Reproduciendo animación de muerte...");

        // 💫 Lanzar secuencia de muerte con transición visual
        StartCoroutine(DeathTransitionSequence(anim, rb, gc, transition));
    }

    private IEnumerator DeathTransitionSequence(Animator anim, Rigidbody2D rb, NF_GameController gc, NF_DeathTransition transition)
    {
        // 🕒 Esperar brevemente la animación de muerte (solo el impacto inicial)
        yield return new WaitForSeconds(0.5f);

        // =============================
        // 💫 EFECTO DE TRANSICIÓN VISUAL (fade negro)
        // =============================
        if (transition != null)
        {
            yield return transition.PlayDeathTransition(() =>
            {
                // 🕳️ Respawn en el último Zone al morir
                gc.StartCoroutine(gc.Respawn(0f, "Zone"));
                gc.HealPlayerAtSpawn(); // ❤️ restaura salud completa
            });
        }
        else
        {
            // fallback sin transición
            gc.StartCoroutine(gc.Respawn(0.5f, "Zone"));
            gc.HealPlayerAtSpawn();
            yield return new WaitForSeconds(1.0f);
        }

        // =============================
        // 🔓 Restaurar movimiento y estado
        // =============================

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 1f;
        }

        // 🔹 Reproducir idle inmediato al reaparecer
        if (anim != null)
        {
            anim.ResetTrigger("DeathTrigger");
            anim.Play("HV_idle 0", 0, 0f);
        }

        // 🔹 Resetear variables de control
        isDead = false;
        this.enabled = true;

        // 🛡️ Invulnerabilidad breve tras respawn
        StartCoroutine(TemporaryInvulnerability(invulnerabilityTime * 0.5f));
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathAnimationDuration);

        // 🩸 Respawn
        NF_GameController gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<NF_GameController>();
        StartCoroutine(gc.Respawn(1f, "Zone"));

        // 🔄 Restaurar propiedades
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = gravity;
        isDead = false;

        if (anim != null)
        {
            anim.ResetTrigger("DeathTrigger");
            anim.Play("HV_idle 0", 0, 0f);
        }
    }
    private IEnumerator HandleObstacleCollision(Collider2D obstacle)
    {
        // 🚫 Evitar recibir daño repetido
        isInvulnerable = true;

        // 🧭 Dirección del golpe
        Vector2 hitDirection = (transform.position - obstacle.transform.position).normalized;

        // 💀 Determinar si el jugador morirá con este golpe antes de aplicar daño
        bool willDie = playerHealthScript.currentHealth - 1 <= 0;

        // 💥 Aplicar daño real
        playerHealthScript.TakeDamage(1, hitDirection);

        // 🔍 Referencias
        NF_GameController gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<NF_GameController>();
        NF_DeathTransition transition = deathTransition; // referencia asignada en el inspector

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Animator anim = GetComponentInChildren<Animator>();

        bool prevFacingRight = isFacingRight;

        // =============================
        // 🔒 BLOQUEAR TODO MOVIMIENTO Y ANIMACIÓN
        // =============================
        if (this.enabled)
            this.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (anim != null)
            anim.speed = 0f; // pausa animaciones

        // =============================
        // 💫 EJECUTAR TRANSICIÓN DE MUERTE
        // =============================
        if (transition != null)
        {
            yield return transition.PlayDeathTransition(() =>
            {
                // 🕳️ Seleccionar tipo de respawn según si morirá o no
                if (willDie)
                {
                    // 💀 Murió completamente → Zone checkpoint
                    gc.StartCoroutine(gc.Respawn(0f, "Zone"));
                    gc.HealPlayerAtSpawn(); // ❤️ restaura salud completa
                    Debug.Log("🔁 Respawn en Zone (vida restaurada).");
                }
                else
                {
                    // ⚠️ Solo tocó obstáculo → Parkour checkpoint
                    gc.StartCoroutine(gc.Respawn(0f, "Parkour"));
                    Debug.Log("🏁 Respawn en Parkour (obstáculo).");
                }

                // =============================
                // 🧍‍♂️ ANIMACIÓN Y ESTADO AL REAPARECER
                // =============================
                if (anim != null)
                {
                    anim.speed = 1f;
                    anim.ResetTrigger("Attack");
                    anim.ResetTrigger("Jump");
                    anim.ResetTrigger("DoubleJump");
                    anim.ResetTrigger("Land");
                    anim.ResetTrigger("Dash");
                    anim.ResetTrigger("DeathTrigger");
                    anim.Play("HV_idle 0", 0, 0f); // Idle inmediato
                }

                // 🔓 Descongelar físicas y control
                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                    rb.gravityScale = 1f;
                }

                Vector3 ls = transform.localScale;
                ls.x = Mathf.Abs(ls.x) * (prevFacingRight ? 1 : -1);
                transform.localScale = ls;
                isFacingRight = prevFacingRight;

                this.enabled = true;
            });
        }
        else
        {
            // Fallback sin transición
            if (willDie)
            {
                gc.StartCoroutine(gc.Respawn(0f, "Zone"));
                gc.HealPlayerAtSpawn();
            }
            else
            {
                gc.StartCoroutine(gc.Respawn(0f, "Parkour"));
            }
        }

        // 🛡️ Invulnerabilidad post-respawn sin bloquear movimiento
        StartCoroutine(TemporaryInvulnerability(invulnerabilityTime * 0.4f));
    }

    private IEnumerator TemporaryInvulnerability(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }

}
