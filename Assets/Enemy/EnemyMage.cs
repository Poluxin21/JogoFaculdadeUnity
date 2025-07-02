using UnityEngine;

public class EnemyMage : Enemy
{
    [Header("Mage Settings")]
    [SerializeField] private float visionRange = 5f;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private Animator animator;

    private Transform targetPoint;
    private float attackTimer;

    protected override void Start() 
    {
        base.Start();
        targetPoint = pointA;
    }

    protected override void Update()
    {
        base.Update();

        if (PauseManager.isPaused) return;

        if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) <= visionRange)
        {
            LookAtPlayer();
            TryAttack();
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);
        
        FlipSprite(targetPoint.position.x > transform.position.x);
        
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            targetPoint = targetPoint == pointA ? pointB : pointA;
        }
    }

    private void LookAtPlayer()
    {
        bool shouldFaceRight = PlayerController.Instance.transform.position.x > transform.position.x;
        FlipSprite(shouldFaceRight);
    }

    private void FlipSprite(bool faceRight)
    {
        Vector3 scale = transform.localScale;
        scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private void TryAttack()
    {
        if (attackTimer <= 0)
        {
            Debug.Log("Iniciando ataque!");
            animator.SetTrigger("Attack");
            Invoke(nameof(ShootProjectile), 0.3f);
            attackTimer = attackCooldown;
        }
        else
        {
            attackTimer -= Time.deltaTime;
        }
    }

    private void ShootProjectile()
    {
        Debug.Log("Tentando atirar projétil!");
        
        if (projectilePrefab == null)
        {
            Debug.LogError("ProjectilePrefab está nulo!");
            return;
        }
        
        if (firePoint == null)
        {
            Debug.LogError("FirePoint está nulo!");
            return;
        }
        
        if (PlayerController.Instance == null)
        {
            Debug.LogError("PlayerController.Instance está nulo!");
            return;
        }
        
        Vector2 direction = (PlayerController.Instance.transform.position - firePoint.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        Debug.Log($"Projétil criado! Direção: {direction}");
        
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * 6f;
            Debug.Log($"Velocidade aplicada: {rb.linearVelocity}");
        }
        else
        {
            Debug.LogError("Projétil não tem Rigidbody2D!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}