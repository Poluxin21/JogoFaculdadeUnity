using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Move")]
    [SerializeField] private float walkspeed = 1;

    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 45f;
    private int jumpBufferCount = 0;
    [SerializeField] private int jumpBufferFrames = 10;
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime = 0.2f;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.1f;
    [SerializeField] private float groundCheckX = 0.3f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Attack")]
    [SerializeField] private float timeBetweenAttacks = 0.5f;
    [SerializeField] private float attackDuration = 0.3f;
    private float attackTimer = 0f;
    private bool isAttacking = false;

    [Header("Attack Settings")]
    [SerializeField] private Transform SideAttackTransform;
    [SerializeField] private Vector2 SideAttackArea;
    [SerializeField] private Transform UpAttackTransform;
    [SerializeField] private Vector2 UpAttackArea;
    [SerializeField] private Transform DownAttackTransform;
    [SerializeField] private Vector2 DownAttackArea;
    [SerializeField] private LayerMask attackableLayerMask;
    [SerializeField] private float damage;

    [Header("Recoil Settings")]
    [SerializeField] private float recoilDuration = 0.2f;
    [SerializeField] private float recoilForce = 15f;
    private float recoilTimer = 0f;

    [Header("Health Settings")]
    public int health;
    public int maxHealth;
    [SerializeField] float hitFlashSpeed;
    [SerializeField] private float invincibilityDuration = 1f;

    float healTimer;
    [SerializeField] float timeToHeal;

    [Header("Mana Settings")]
    [SerializeField] Image manaStorage;
    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;

    [Header("Death Settings")]
    [SerializeField] private FadeUI deathPanel; // Referência ao DeathPanel
    [SerializeField] private float deathPanelDelay = 1f; // Tempo antes do painel aparecer

    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallback;

    private Rigidbody2D rb;
    private float xAxis, yAxis;
    private float gravity;
    Animator anim;

    public static PlayerController Instance;
    [HideInInspector] public PlayerStateList pState;
    private SpriteRenderer sr;

    private bool canDash = true;
    private bool dashed;
    private bool wasGrounded;

    bool restoreTime;
    float restoreTimeSpeed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        // Inicializar componentes primeiro
        pState = GetComponent<PlayerStateList>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Verificar se os componentes foram encontrados
        if (pState == null)
        {
            Debug.LogError("PlayerStateList component not found on " + gameObject.name);
        }
        if (sr == null)
        {
            Debug.LogError("SpriteRenderer component not found on " + gameObject.name);
        }
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on " + gameObject.name);
        }
        if (anim == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }

        // Inicializar health apenas uma vez
        health = maxHealth;

        // Verificar se o deathPanel foi atribuído
        if (deathPanel == null)
        {
            Debug.LogWarning("DeathPanel não está atribuído no PlayerController!");
        }
    }

    void Start()
    {
        // Verificar se os componentes estão válidos antes de usar
        if (rb != null)
        {
            gravity = rb.gravityScale;
        }

        // Inicializar estado alive
        if (pState != null)
        {
            pState.alive = true;
        }

        // Inicializar mana
        mana = Mathf.Clamp(mana, 0, 1);
        if (manaStorage != null)
        {
            manaStorage.fillAmount = mana;
        }
        else
        {
            Debug.LogWarning("Mana Storage UI não está atribuído no inspector!");
        }

        // Validações de segurança
        if (jumpBufferFrames <= 0) jumpBufferFrames = 10;
        if (coyoteTime <= 0) coyoteTime = 0.2f;
        if (groundCheckY <= 0) groundCheckY = 0.1f;
        if (timeBetweenAttacks <= 0) timeBetweenAttacks = 0.5f;
        if (attackDuration <= 0) attackDuration = 0.3f;
        if (recoilDuration <= 0) recoilDuration = 0.2f;

        // Verificar transforms de ataque
        if (SideAttackTransform == null) Debug.LogWarning("SideAttackTransform não está atribuído!");
        if (UpAttackTransform == null) Debug.LogWarning("UpAttackTransform não está atribuído!");
        if (DownAttackTransform == null) Debug.LogWarning("DownAttackTransform não está atribuído!");
        if (groundCheckPoint == null) Debug.LogWarning("GroundCheckPoint não está atribuído!");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (SideAttackTransform != null) Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        if (UpAttackTransform != null) Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        if (DownAttackTransform != null) Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);

        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.yellow;
            // Linha central
            Gizmos.DrawLine(groundCheckPoint.position,
                groundCheckPoint.position + Vector3.down * groundCheckY);
            // Linha direita
            Gizmos.DrawLine(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0),
                groundCheckPoint.position + new Vector3(groundCheckX, -groundCheckY, 0));
            // Linha esquerda
            Gizmos.DrawLine(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0),
                groundCheckPoint.position + new Vector3(-groundCheckX, -groundCheckY, 0));
        }
    }

    void Update()
    {
        if (PauseManager.isPaused) return;

        if (pState.cutScene) return;

        // Se o player não está vivo, não processa inputs nem ações
        if (!pState.alive) return;

        GetInputs();

        if (pState != null && pState.isDash) return;

        UpdateTimers();
        RestoreTimeScale();
        FlashWhileInvincible();
        Move();
        Jump();
        UpdateJumpVariables();
        Heal();

        if (pState != null && pState.healing) return;

        Flip();
        StartDash();
        Attack();
    }

    private void FixedUpdate()
    {
        if (PauseManager.isPaused) return;

        if (pState.cutScene) return;

        // Se o player não está vivo, não processa física
        if (!pState.alive) return;

        if (pState != null && (pState.isDash || pState.healing)) return;
        HandleRecoil();
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
    }

    private void UpdateTimers()
    {
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (recoilTimer > 0)
            recoilTimer -= Time.deltaTime;

        // Controle de estado de ataque
        if (isAttacking && attackTimer <= (timeBetweenAttacks - attackDuration))
        {
            isAttacking = false;
        }
    }

    private void Move()
    {
        if (rb == null) return;

        if ((pState != null && pState.healing) || isAttacking || recoilTimer > 0)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(walkspeed * xAxis, rb.linearVelocity.y);

        if (anim != null)
        {
            anim.SetBool("IsWalk", rb.linearVelocity.x != 0 && Grounded());
        }
    }

    void StartDash()
    {
        if (Input.GetButtonDown("Dash") && canDash && !dashed && !isAttacking)
        {
            StartCoroutine(Dash());
            dashed = true;
        }

        if (Grounded())
        {
            dashed = false;
        }
    }

    void Attack()
    {
        if (Input.GetButtonDown("Attack") && attackTimer <= 0 && !isAttacking)
        {
            isAttacking = true;
            attackTimer = timeBetweenAttacks;

            if (anim != null)
            {
                anim.SetTrigger("ToAttack");
            }

            if (yAxis == 0 || (yAxis < 0 && Grounded()))
            {
                if (SideAttackTransform != null)
                    StartCoroutine(ExecuteAttackAfterDelay(SideAttackTransform, SideAttackArea, 0.1f));
            }
            else if (yAxis > 0)
            {
                if (UpAttackTransform != null)
                    StartCoroutine(ExecuteAttackAfterDelay(UpAttackTransform, UpAttackArea, 0.1f));
            }
            else if (yAxis < 0 && !Grounded())
            {
                if (DownAttackTransform != null)
                    StartCoroutine(ExecuteAttackAfterDelay(DownAttackTransform, DownAttackArea, 0.1f));
            }
        }
    }

    private IEnumerator ExecuteAttackAfterDelay(Transform attackTransform, Vector2 attackArea, float delay)
    {
        yield return new WaitForSeconds(delay);
        Hit(attackTransform, attackArea);
    }

    void Hit(Transform _attackTransform, Vector2 _attackArea)
    {
        if (_attackTransform == null) return;

        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayerMask);
        List<Enemy> hitEnemies = new List<Enemy>();

        bool hitSomething = false;

        for (int i = 0; i < objectsToHit.Length; i++)
        {
            Enemy e = objectsToHit[i].GetComponent<Enemy>();
            if (e && !hitEnemies.Contains(e))
            {
                Vector2 knockbackDirection = (objectsToHit[i].transform.position - transform.position).normalized;

                e.EnemyHit(damage, knockbackDirection, recoilForce);
                hitEnemies.Add(e);
                hitSomething = true;

                Mana += manaGain;
                HitStopTime(0.1f, 50, 0.05f);
            }
        }

        if (hitSomething)
        {
            ApplyRecoil();
        }
    }

    private void ApplyRecoil()
    {
        if (rb == null || pState == null) return;

        recoilTimer = recoilDuration;

        Vector2 recoilDirection = pState.lookingRight ? Vector2.left : Vector2.right;

        if (yAxis < 0 && !Grounded())
        {
            recoilDirection = Vector2.up;
        }

        rb.linearVelocity = new Vector2(recoilDirection.x * recoilForce,
                                       recoilDirection.y > 0 ? recoilForce : rb.linearVelocity.y);
    }

    private void HandleRecoil()
    {
        if (rb == null) return;

        if (recoilTimer <= 0)
        {
            rb.gravityScale = gravity;
        }
        else if (yAxis < 0 && !Grounded() && recoilTimer > 0)
        {
            rb.gravityScale = 0;
        }
    }

    void FlashWhileInvincible()
    {
        if (sr == null || pState == null) return;

        sr.material.color = pState.invincible ?
            Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time * hitFlashSpeed, 1.0f)) :
            Color.white;
    }

    IEnumerator Dash()
    {
        if (rb == null || pState == null || anim == null) yield break;

        canDash = false;
        pState.isDash = true;
        anim.SetTrigger("ToDash");
        rb.gravityScale = 0;

        float dashDirection = pState.lookingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0);

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = gravity;
        pState.isDash = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public IEnumerator WalkIntoNewScene(Vector2 exitDir, float delay)
    {
        pState.cutScene = true;

        if (exitDir.y != 0)
        {
            rb.linearVelocityY = jumpForce * exitDir.y;
        }

        if (exitDir.x != 0)
        {
            xAxis = exitDir.x > 0 ? 1 : -1;

            Move();
        }
        Flip();
        yield return new WaitForSeconds(delay);
        pState.cutScene = false;
    }

    void Flip()
    {
        if (pState == null) return;

        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
            pState.lookingRight = false;
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
            pState.lookingRight = true;
        }
    }

    public bool Grounded()
    {
        if (groundCheckPoint == null) return false;

        // Verificação simples com 3 raycasts (centro, esquerda, direita)
        return Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0),
                Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0),
                Vector2.down, groundCheckY, whatIsGround);
    }

    void Jump()
    {
        if (rb == null) return;

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            if (pState != null) pState.isJump = false;
        }

        if (pState != null && !pState.isJump)
        {
            if (jumpBufferCount > 0 && (Grounded() || coyoteTimeCounter > 0))
            {
                PerformJump();
                jumpBufferCount = 0;
            }
            else if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
            {
                PerformJump();
                airJumpCounter++;
            }
        }

        if (anim != null)
        {
            anim.SetBool("isJump", !Grounded());
        }
    }

    void PerformJump()
    {
        if (rb == null || pState == null) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        pState.isJump = true;
    }

    void UpdateJumpVariables()
    {
        if (pState == null) return;

        bool currentlyGrounded = Grounded();

        if (currentlyGrounded)
        {
            pState.isJump = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCount = jumpBufferFrames;
        }
        else if (jumpBufferCount > 0)
        {
            jumpBufferCount--;
        }

        wasGrounded = currentlyGrounded;
    }

    public void TakeDamage(float _damage)
    {
        if (pState != null && pState.invincible) return;

        Health -= Mathf.RoundToInt(_damage);
        StartCoroutine(StopTakingDamage());
    }

    IEnumerator StopTakingDamage()
    {
        if (pState == null || anim == null) yield break;

        pState.invincible = true;
        anim.SetTrigger("TakeDamage");
        yield return new WaitForSeconds(invincibilityDuration);
        pState.invincible = false;
    }

    public int Health
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);

                if (onHealthChangedCallback != null)
                {
                    onHealthChangedCallback.Invoke();
                }

                // Verifica se o player morreu
                if (health <= 0 && pState != null && pState.alive)
                {
                    StartCoroutine(Death());
                }
            }
        }
    }

    void RestoreTimeScale()
    {
        if (restoreTime)
        {
            if (Time.timeScale < 1)
            {
                Time.timeScale += Time.unscaledDeltaTime * restoreTimeSpeed;
            }
            else
            {
                Time.timeScale = 1;
                restoreTime = false;
            }
        }
    }

    public void HitStopTime(float _newTimeScale, int _restoreSpeed, float _delay)
    {
        restoreTimeSpeed = _restoreSpeed;
        if (_delay > 0)
        {
            StopCoroutine(StartTimeAgain(_delay));
            StartCoroutine(StartTimeAgain(_delay));
        }
        else
        {
            restoreTime = true;
        }
        Time.timeScale = _newTimeScale;
    }

    IEnumerator StartTimeAgain(float _delay)
    {
        yield return new WaitForSecondsRealtime(_delay);
        restoreTime = true;
    }

    void Heal()
    {
        if (pState == null) return;

        if (Input.GetButton("Healing") && Health < maxHealth && Mana > 0 && Grounded() && !pState.isDash && !isAttacking)
        {
            pState.healing = true;

            healTimer += Time.deltaTime;
            if (healTimer >= timeToHeal)
            {
                Health++;
                healTimer = 0;
            }
            Mana -= Time.deltaTime * manaDrainSpeed;
        }
        else
        {
            pState.healing = false;
            healTimer = 0;
        }
    }

    IEnumerator Death()
    {
        if (pState == null || anim == null) yield break;

        // Define que o player não está mais vivo
        pState.alive = false;

        // Para o player completamente
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0; // Remove a gravidade para não cair
        }

        // Restaura o tempo normal caso esteja alterado
        Time.timeScale = 1f;

        // Ativa a animação de morte
        anim.SetTrigger("Death");

        // Aguarda a animação de morte terminar
        yield return new WaitForSeconds(0.9f);

        // Aguarda o delay adicional antes de mostrar o painel
        yield return new WaitForSeconds(deathPanelDelay);

        // Mostra o painel de morte com fade in
        if (deathPanel != null)
        {
            deathPanel.FadeIn(1f); // Fade in de 1 segundo
        }
        else
        {
            Debug.LogError("DeathPanel não está atribuído no PlayerController!");
        }
    }

    float Mana
    {
        get { return mana; }
        set
        {
            if (mana != value)
            {
                mana = Mathf.Clamp(value, 0, 1);
                if (manaStorage != null)
                {
                    manaStorage.fillAmount = mana;
                }
            }
        }
    }
}