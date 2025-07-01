using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Move")]
    [SerializeField]
    private float walkspeed = 1;

    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;

    [Header("Jump")][SerializeField] private float jumpForce = 45f;
    private int jumpBufferCount = 0;
    [SerializeField] private int jumpBufferFrames = 10;
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime = 0.2f;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;

    [Header("Ground Check")]
    [SerializeField]
    private Transform groundCheckPoint;

    [SerializeField] private float groundCheckY = 0.1f; // Reduzido para evitar atravessar
    [SerializeField] private float groundCheckX = 0.3f; // Ajustado
    [SerializeField] private LayerMask whatIsGround;

    // M�ltiplos m�todos de detec��o para Composite Collider Outline
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private bool useCircleCast = true;
    [SerializeField] private bool useBoxCast = true; // M�todo adicional
    [SerializeField] private Vector2 boxCastSize = new Vector2(0.8f, 0.1f);

    [Header("Attack")] private bool attack = false;
    private float timeBtAttack, timeSAtk;

    [Header("Attack Settings:")]
    [SerializeField]
    private Transform SideAttackTransform;

    [SerializeField] private Vector2 SideAttackArea;

    [SerializeField] private Transform UpAttackTransform;
    [SerializeField] private Vector2 UpAttackArea;

    [SerializeField] private Transform DownAttackTransform;
    [SerializeField] private Vector2 DownAttackArea;
    [SerializeField] private LayerMask attackableLayerMask;

    [SerializeField] private float damage;

    [Header("Recoil Settings:")]
    [SerializeField] private int recoilXSteps = 5;
    [SerializeField] private int recoilYSteps = 5;

    [SerializeField] private float recoilXSpeed = 100;
    [SerializeField] private float recoilYSpeed = 100;

    private int stepsXRecoiled, stepsYRecoiled;
    [Space(5)]

    [Header("Health Settings")]
    public int health;
    public int maxHealth;
    [SerializeField] float hitFlashSpeed;

    float healTimer;
    [SerializeField] float timeToHeal;

    [Header("Mana Settings")]
    [SerializeField] Image manaStorage;
    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;

    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallback;
    [Space(5)]

    private Rigidbody2D rb;
    private float xAxis, yAxis;
    private float gravity;
    Animator anim;

    public static PlayerController Instance;
    private static readonly int ToDash = Animator.StringToHash("ToDash");
    [HideInInspector] public PlayerStateList pState;
    private SpriteRenderer sr;

    private bool canDash = true;
    private bool dashed;
    private bool wasGrounded; // Para detectar mudan�as no estado de ch�o

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

        // Valida��o de valores padr�o
        if (jumpBufferFrames <= 0) jumpBufferFrames = 10;
        if (coyoteTime <= 0) coyoteTime = 0.2f;
        if (groundCheckY <= 0) groundCheckY = 0.1f;
        if (groundCheckRadius <= 0) groundCheckRadius = 0.15f;
        if (boxCastSize.x <= 0) boxCastSize.x = 0.8f;
        if (boxCastSize.y <= 0) boxCastSize.y = 0.1f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (SideAttackTransform != null) Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        if (UpAttackTransform != null) Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        if (DownAttackTransform != null) Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);

        // Desenhar �rea de detec��o de ch�o
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

            // Raycasts de backup
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
        Recoil();
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetButtonDown("Attack");
    }

    private void Move()
    {
        if (pState.healing)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(walkspeed * xAxis, rb.linearVelocity.y);
        anim.SetBool("IsWalk", rb.linearVelocity.x != 0 && Grounded());
    }

    void StartDash()
    {
        if (Input.GetButtonDown("Dash") && canDash && !dashed)
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
        timeSAtk += Time.deltaTime;

        if (attack && timeSAtk >= timeBtAttack)
        {
            timeSAtk = 0;
            anim.SetTrigger("ToAttack");

            if ((yAxis == 0 || (yAxis < 0 && Grounded())))
            {
                Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, recoilXSpeed);
            }
            else if (yAxis > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, recoilYSpeed);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, recoilYSpeed);
            }
        }
    }

    void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {
        Collider2D[] objectsToHit =
            Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayerMask);
        List<Enemy> hitEnemies = new List<Enemy>();

        if (objectsToHit.Length > 0)
        {
            _recoilDir = true;
        }

        for (int i = 0; i < objectsToHit.Length; i++)
        {
            Enemy e = objectsToHit[i].GetComponent<Enemy>();
            if (e && !hitEnemies.Contains(e))
            {
                e.EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized,
                    _recoilStrength);
                hitEnemies.Add(e);
            }

            if (objectsToHit[i].CompareTag("Enemy"))
            {
                Mana += manaGain;
            }
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

        // Melhorar dire��o do dash
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

        // M�todo 1: BoxCast (melhor para Composite Collider Outline)
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

        // M�todo 2: CircleCast como backup
        if (!isGrounded && useCircleCast)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheckPoint.position + Vector3.down * 0.05f,
                groundCheckRadius,
                whatIsGround
            );
        }

        // M�todo 3: Raycasts m�ltiplos como �ltimo recurso
        if (!isGrounded)
        {
            isGrounded = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround)
                || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0),
                    Vector2.down, groundCheckY, whatIsGround)
                || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0),
                    Vector2.down, groundCheckY, whatIsGround)
                || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX * 0.5f, 0, 0),
                    Vector2.down, groundCheckY, whatIsGround)
                || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX * 0.5f, 0, 0),
                    Vector2.down, groundCheckY, whatIsGround);
        }

        return isGrounded;
    }

    void Jump()
    {
        // Debug logs apenas quando necess�rio
        if (Input.GetButtonDown("Jump"))
        {
            Debug.Log($"Jump - Grounded: {Grounded()}, Buffer: {jumpBufferCount}, Coyote: {coyoteTimeCounter:F2}");
        }

        // Pulo vari�vel - soltar o bot�o reduz a for�a
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            pState.isJump = false;
        }

        if (!pState.isJump)
        {
            // Pulo normal (ch�o ou coyote time)
            if (jumpBufferCount > 0 && (Grounded() || coyoteTimeCounter > 0))
            {
                PerformJump();
                jumpBufferCount = 0;
            }
            // Pulo no ar
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
        Debug.Log($"Pulo executado! Velocidade Y: {rb.linearVelocity.y}");
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

        // Jump buffer
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

    void Recoil()
    {
        if (pState.recoilingX)
        {
            if (pState.lookingRight)
            {
                rb.linearVelocity = new Vector2(-recoilXSpeed, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(recoilXSpeed, rb.linearVelocity.y);
            }
        }

        if (pState.recoilingY)
        {
            rb.gravityScale = 0;
            if (yAxis < 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, recoilYSpeed);
            }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -recoilYSpeed);
            }

            airJumpCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }

        if (pState.recoilingX && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }

        if (pState.recoilingY && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }

        if (Grounded())
        {
            StopRecoilY();
        }
    }

    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }

    void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }

    // CORRE��O: M�todo TakeDamage corrigido
    public void TakeDamage(float _damage)
    {
        // S� toma dano se n�o estiver invenc�vel
        if (!pState.invincible)
        {
            Debug.Log($"Player tomou {_damage} de dano! Vida atual: {Health}");
            Health -= Mathf.RoundToInt(_damage);
            StartCoroutine(StopTakingDamage());
        }
        else
        {
            Debug.Log("Player est� invenc�vel, dano ignorado!");
        }
    }

    IEnumerator StopTakingDamage()
    {
        pState.invincible = true;
        anim.SetTrigger("TakeDamage");
        yield return new WaitForSeconds(invincibilityTime); // Usar vari�vel configur�vel
        pState.invincible = false;
        Debug.Log("Player n�o est� mais invenc�vel!");
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

                // Verificar se o player morreu
                if (health <= 0)
                {
                    Die();
                }
            }
        }
    }

    // CORRE��O: M�todo Die adicionado
    void Die()
    {
        Debug.Log("Player morreu!");
        anim.SetTrigger("Death");
        // Desabilitar controles
        enabled = false;
        // Adicionar aqui l�gica de morte (restart level, game over screen, etc.)
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
        if (Input.GetButton("Healing") && Health < maxHealth && Mana > 0 && Grounded() && !pState.isDash)
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