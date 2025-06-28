using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private int jumpBufferFrames;
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;

    [Header("Ground Check")]
    [SerializeField]
    private Transform groundCheckPoint;

    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;

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
    [Space(5)]

    private Rigidbody2D rb;
    private float xAxis, yAxis;
    private float gravity;
    Animator anim;

    public static PlayerController Instance;
    private static readonly int ToDash = Animator.StringToHash("ToDash");
    // PlayerStateList pState;
    [HideInInspector] public PlayerStateList pState;

    private bool canDash = true;
    private bool dashed;


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

        health = maxHealth;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pState = GetComponent<PlayerStateList>();

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        gravity = rb.gravityScale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        UpdateJumpVariables();
        if (pState.isDash) return;
        Move();
        Jump();
        Flip();
        StartDash();
        Attack();
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

    // ReSharper disable Unity.PerformanceAnalysis
    void Attack()
    {
        timeSAtk += Time.deltaTime;
        if (attack && timeSAtk >= timeBtAttack)
        {
            timeSAtk = 0;
            anim.SetTrigger("ToAttack");
        }

        if (yAxis == 0 || yAxis < 0 && Grounded())
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
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        pState.isDash = true;
        anim.SetTrigger("ToDash");
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashSpeed, 0);
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
            transform.localScale = new Vector2(-3, transform.localScale.y);
            pState.lookingRight = false;
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(3, transform.localScale.y);
            pState.lookingRight = true;
        }
    }

    public bool Grounded()
    {
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0),
                Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0),
                Vector2.down, groundCheckY, whatIsGround))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void Jump()
    {
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            pState.isJump = false;
        }

        if (!pState.isJump)
        {
            if (jumpBufferCount > 0 && coyoteTimeCounter > 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce);
                pState.isJump = true;
            }
            else if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
            {
                pState.isJump = true;
                airJumpCounter++;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce);
            }
        }

        anim.SetBool("isJump", !Grounded());
    }

    void UpdateJumpVariables()
    {
        if (Grounded())
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
        else
        {
            jumpBufferCount--;
        }
    }

    void Recoil()
    {
        if (pState.recoilingX)
        {
            if (pState.lookingRight)
            {
                rb.linearVelocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                rb.linearVelocity = new Vector2(recoilXSpeed, 0);
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

    public void TakeDamage(float _damage)
    {
        health -= Mathf.RoundToInt(_damage);
        StartCoroutine(StopTakingDamage());
    }

    IEnumerator StopTakingDamage()
    {
        pState.invincible = true;
        anim.SetTrigger("TakeDamage");
        ClampHealth();
        yield return new WaitForSeconds(1f);
        pState.invincible = false;
    }

    void ClampHealth()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
    }
}