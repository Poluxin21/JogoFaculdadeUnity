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
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private bool useCircleCast = true;
    [SerializeField] private bool useBoxCast = true;
    [SerializeField] private Vector2 boxCastSize = new Vector2(0.8f, 0.1f);

    [Header("Attack")]
    [SerializeField] private float timeBetweenAttacks = 0.5f; // Tempo fixo entre ataques
    [SerializeField] private float attackDuration = 0.3f; // Duração da animação de ataque
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
    [SerializeField] private float recoilDuration = 0.2f; // Duração fixa do recoil
    [SerializeField] private float recoilForce = 15f; // Força do recoil
    private float recoilTimer = 0f;

    [Header("Health Settings")]
    public int health;
    public int maxHealth;
    [SerializeField] float hitFlashSpeed;
    [SerializeField] private float invincibilityDuration = 1f; // Duração da invencibilidade

    float healTimer;
    [SerializeField] float timeToHeal;

    [Header("Mana Settings")]
    [SerializeField] Image manaStorage;
    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;

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
        }
        else
        {
            Instance = this;
        }

        Health = maxHealth;
    }

    void Start()
    {
        pState = GetComponent<PlayerStateList>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        gravity = rb.gravityScale;
        Mana = mana;
        manaStorage.fillAmount = Mana;
        
        // Validações de segurança
        if (jumpBufferFrames <= 0) jumpBufferFrames = 10;
        if (coyoteTime <= 0) coyoteTime = 0.2f;
        if (groundCheckY <= 0) groundCheckY = 0.1f;
        if (groundCheckRadius <= 0) groundCheckRadius = 0.15f;
        if (timeBetweenAttacks <= 0) timeBetweenAttacks = 0.5f;
        if (attackDuration <= 0) attackDuration = 0.3f;
        if (recoilDuration <= 0) recoilDuration = 0.2f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (SideAttackTransform != null) Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        if (UpAttackTransform != null) Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        if (DownAttackTransform != null) Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);

        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            if (useCircleCast)
            {
                Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
            }

            if (useBoxCast)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(groundCheckPoint.position + Vector3.down * 0.05f, boxCastSize);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(groundCheckPoint.position,
                groundCheckPoint.position + Vector3.down * groundCheckY);
            Gizmos.DrawLine(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0),
                groundCheckPoint.position + new Vector3(groundCheckX, -groundCheckY, 0));
            Gizmos.DrawLine(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0),
                groundCheckPoint.position + new Vector3(-groundCheckX, -groundCheckY, 0));
        }
    }

    void Update()
    {
        GetInputs();

        if (pState.isDash) return;

        UpdateTimers();
        RestoreTimeScale();
        FlashWhileInvincible();
        Move();
        Jump();
        UpdateJumpVariables();
        Heal();

        if (pState.healing) return;

        Flip();
        StartDash();
        Attack();
    }

    private void FixedUpdate()
    {
        if (pState.isDash || pState.healing) return;
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
        if (pState.healing || isAttacking || recoilTimer > 0)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(walkspeed * xAxis, rb.linearVelocity.y);
        anim.SetBool("IsWalk", rb.linearVelocity.x != 0 && Grounded());
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
        // Só pode atacar se não estiver atacando e o timer permitir
        if (Input.GetButtonDown("Attack") && attackTimer <= 0 && !isAttacking)
        {
            isAttacking = true;
            attackTimer = timeBetweenAttacks;
            
            anim.SetTrigger("ToAttack");

            // Determina direção do ataque baseado no input
            if (yAxis == 0 || (yAxis < 0 && Grounded()))
            {
                // Ataque lateral
                StartCoroutine(ExecuteAttackAfterDelay(SideAttackTransform, SideAttackArea, 0.1f));
            }
            else if (yAxis > 0)
            {
                // Ataque para cima
                StartCoroutine(ExecuteAttackAfterDelay(UpAttackTransform, UpAttackArea, 0.1f));
            }
            else if (yAxis < 0 && !Grounded())
            {
                // Ataque para baixo (no ar)
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
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayerMask);
        List<Enemy> hitEnemies = new List<Enemy>();

        bool hitSomething = false;

        for (int i = 0; i < objectsToHit.Length; i++)
        {
            Enemy e = objectsToHit[i].GetComponent<Enemy>();
            if (e && !hitEnemies.Contains(e))
            {
                // Calcula direção do knockback
                Vector2 knockbackDirection = (objectsToHit[i].transform.position - transform.position).normalized;
                
                e.EnemyHit(damage, knockbackDirection, recoilForce);
                hitEnemies.Add(e);
                hitSomething = true;

                // Ganha mana ao acertar inimigo
                Mana += manaGain;
                
                // Hit stop effect
                HitStopTime(0.1f, 50, 0.05f);
            }
        }

        // Aplica recoil no player se acertou algo
        if (hitSomething)
        {
            ApplyRecoil();
        }
    }

    private void ApplyRecoil()
    {
        recoilTimer = recoilDuration;
        
        // Recoil baseado na direção que o player está olhando
        Vector2 recoilDirection = pState.lookingRight ? Vector2.left : Vector2.right;
        
        // Se for ataque para baixo, adiciona impulso para cima
        if (yAxis < 0 && !Grounded())
        {
            recoilDirection = Vector2.up;
        }
        
        rb.linearVelocity = new Vector2(recoilDirection.x * recoilForce, 
                                       recoilDirection.y > 0 ? recoilForce : rb.linearVelocity.y);
    }

    private void HandleRecoil()
    {
        // O recoil agora é controlado pelo timer, não por steps
        if (recoilTimer <= 0)
        {
            // Restaura controle normal
            rb.gravityScale = gravity;
        }
        else if (yAxis < 0 && !Grounded() && recoilTimer > 0)
        {
            // Mantém o player no ar durante recoil vertical
            rb.gravityScale = 0;
        }
    }

    void FlashWhileInvincible()
    {
        sr.material.color = pState.invincible ?
            Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time * hitFlashSpeed, 1.0f)) :
            Color.white;
    }

    IEnumerator Dash()
    {
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

    void Flip()
    {
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

        bool isGrounded = false;

        if (useBoxCast)
        {
            RaycastHit2D boxHit = Physics2D.BoxCast(
                groundCheckPoint.position,
                boxCastSize,
                0f,
                Vector2.down,
                0.1f,
                whatIsGround
            );
            if (boxHit.collider != null)
            {
                isGrounded = true;
            }
        }

        if (!isGrounded && useCircleCast)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheckPoint.position + Vector3.down * 0.05f,
                groundCheckRadius,
                whatIsGround
            );
        }

        if (!isGrounded)
        {
            isGrounded = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround)
                || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0),
                    Vector2.down, groundCheckY, whatIsGround)
                || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0),
                    Vector2.down, groundCheckY, whatIsGround);
        }

        return isGrounded;
    }

    void Jump()
    {
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            pState.isJump = false;
        }

        if (!pState.isJump)
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

        anim.SetBool("isJump", !Grounded());
    }

    void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        pState.isJump = true;
    }

    void UpdateJumpVariables()
    {
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
        if (pState.invincible) return;
        
        Health -= Mathf.RoundToInt(_damage);
        StartCoroutine(StopTakingDamage());
    }

    IEnumerator StopTakingDamage()
    {
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
        pState.alive = false;
        Time.timeScale = 1f;
        anim.SetTrigger("Death");
        
        yield return new WaitForSeconds(0.9f);
    }

    float Mana
    {
        get { return mana; }
        set
        {
            if (mana != value)
            {
                mana = Mathf.Clamp(value, 0, 1);
                manaStorage.fillAmount = Mana;
            }
        }
    }
}