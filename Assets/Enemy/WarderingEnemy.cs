using UnityEngine;

public class WanderingArmor : Enemy
{
    [Header("Patrol Settings")]
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private float waitTimeAtPoint = 1f;

    [Header("Chase Settings")]
    [SerializeField] private float detectionRadius = 4f;
    [SerializeField] private float maxChaseDistance = 8f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float loseTargetDelay = 1f;

    [Header("Attack Settings")]
    [SerializeField] private float contactDamage = 1f;
    [SerializeField] private float aoeDamage = 1f;
    [SerializeField] private float aoeCooldown = 5f;
    [SerializeField] private float aoeRadius = 1f;
    [SerializeField] private Animator animator;

    private Vector2 spawnPoint;
    private Vector2 leftPoint;
    private Vector2 rightPoint;
    
    private enum AIState { Patrolling, Chasing, Returning }
    private enum PatrolState { GoingLeft, WaitingLeft, GoingRight, WaitingRight }
    
    private AIState currentAIState = AIState.Patrolling;
    private PatrolState currentPatrolState = PatrolState.GoingRight;
    private Vector2 lastPatrolPosition;
    
    private float waitTimer = 0f;
    private float aoeTimer;
    private float loseTargetTimer = 0f;
    private Transform playerTransform;

    // Variável para controlar mudanças de estado de animação
    private AIState previousAIState;

    protected override void Start()
    {
        base.Start();
        health = 3f;
        damage = contactDamage;
        aoeTimer = aoeCooldown;
        
        SetupPatrolPoints();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        previousAIState = currentAIState;
    }

    private void SetupPatrolPoints()
    {
        spawnPoint = transform.position;
        leftPoint = new Vector2(spawnPoint.x - patrolDistance, spawnPoint.y);
        rightPoint = new Vector2(spawnPoint.x + patrolDistance, spawnPoint.y);
        lastPatrolPosition = transform.position;
    }

    protected override void Update()
    {
        base.Update();

        if (PauseManager.isPaused) return;

        if (!isRecoiling)
        {
            UpdateAI();
        }

        // Sistema de ataque AoE
        aoeTimer -= Time.deltaTime;
        if (aoeTimer <= 0)
        {
            if (animator != null)
                animator.SetTrigger("attack");
            Invoke(nameof(ExecuteAoEAttack), 0.5f);
            aoeTimer = aoeCooldown;
        }
    }

    private void UpdateAI()
    {
        // Detecta mudança de estado para limpar animações
        if (previousAIState != currentAIState)
        {
            ResetAllAnimations();
            previousAIState = currentAIState;
        }

        switch (currentAIState)
        {
            case AIState.Patrolling:
                HandlePatrolling();
                break;
            case AIState.Chasing:
                HandleChasing();
                break;
            case AIState.Returning:
                HandleReturning();
                break;
        }
    }

    // Método para resetar todas as animações
    private void ResetAllAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("walk", false);
            animator.SetBool("run", false);
        }
    }

    private void HandlePatrolling()
    {
        if (CanSeePlayer())
        {
            StartChasing();
            return;
        }

        Patrol();
    }

    private void HandleChasing()
    {
        if (playerTransform == null)
        {
            StartReturning();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer > maxChaseDistance)
        {
            loseTargetTimer += Time.deltaTime;
            if (loseTargetTimer >= loseTargetDelay)
            {
                StartReturning();
                return;
            }
        }
        else
        {
            loseTargetTimer = 0f;
        }

        Vector2 targetPosition = playerTransform.position;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, chaseSpeed * Time.deltaTime);
        
        FlipSprite(targetPosition);
        
        // Garante que apenas a animação de corrida está ativa
        if (animator != null)
        {
            animator.SetBool("walk", false);
            animator.SetBool("run", true);
        }
    }

    private void HandleReturning()
    {
        if (CanSeePlayer())
        {
            StartChasing();
            return;
        }

        float distanceToLastPosition = Vector2.Distance(transform.position, lastPatrolPosition);
        
        if (distanceToLastPosition > 0.2f)
        {
            transform.position = Vector2.MoveTowards(transform.position, lastPatrolPosition, speed * Time.deltaTime);
            FlipSprite(lastPatrolPosition);
            
            if (animator != null)
            {
                animator.SetBool("run", false);
                animator.SetBool("walk", true);
            }
        }
        else
        {
            currentAIState = AIState.Patrolling;
            if (animator != null)
            {
                animator.SetBool("walk", false);
                animator.SetBool("run", false);
            }
        }
    }

    private bool CanSeePlayer()
    {
        if (playerTransform == null)
            return false;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer <= detectionRadius)
        {
            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, detectionRadius, LayerMask.GetMask("Ground", "Wall"));
            
            return hit.collider == null || hit.collider.CompareTag("Player");
        }
        
        return false;
    }

    private void StartChasing()
    {
        if (currentAIState == AIState.Patrolling)
        {
            lastPatrolPosition = transform.position;
        }
        
        currentAIState = AIState.Chasing;
        loseTargetTimer = 0f;
        
        // Limpa animações antigas e define a nova
        if (animator != null)
        {
            animator.SetBool("walk", false);
            animator.SetBool("run", true);
        }
    }

    private void StartReturning()
    {
        currentAIState = AIState.Returning;
        loseTargetTimer = 0f;
        
        if (animator != null)
        {
            animator.SetBool("run", false);
            animator.SetBool("walk", true);
        }
    }

    private void Patrol()
    {
        Vector2 targetPosition = GetCurrentTarget();
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
        
        if (distanceToTarget > 0.1f && !IsWaiting())
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            
            FlipSprite(targetPosition);
            
            if (animator != null)
            {
                animator.SetBool("run", false);
                animator.SetBool("walk", true);
            }
        }
        else if (!IsWaiting())
        {
            StartWaiting();
        }
        else
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                AdvanceToNextState();
            }
        }
    }

    private Vector2 GetCurrentTarget()
    {
        switch (currentPatrolState)
        {
            case PatrolState.GoingLeft:
            case PatrolState.WaitingLeft:
                return leftPoint;
            case PatrolState.GoingRight:
            case PatrolState.WaitingRight:
                return rightPoint;
            default:
                return rightPoint;
        }
    }

    private bool IsWaiting()
    {
        return currentPatrolState == PatrolState.WaitingLeft || currentPatrolState == PatrolState.WaitingRight;
    }

    private void StartWaiting()
    {
        waitTimer = waitTimeAtPoint;
        
        if (animator != null)
        {
            animator.SetBool("walk", false);
            animator.SetBool("run", false);
        }
        
        if (currentPatrolState == PatrolState.GoingLeft)
            currentPatrolState = PatrolState.WaitingLeft;
        else if (currentPatrolState == PatrolState.GoingRight)
            currentPatrolState = PatrolState.WaitingRight;
    }

    private void AdvanceToNextState()
    {
        switch (currentPatrolState)
        {
            case PatrolState.WaitingLeft:
                currentPatrolState = PatrolState.GoingRight;
                break;
            case PatrolState.WaitingRight:
                currentPatrolState = PatrolState.GoingLeft;
                break;
        }
    }

    private void FlipSprite(Vector2 targetPosition)
    {
        Vector3 scale = transform.localScale;
        if (targetPosition.x > transform.position.x)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else
        {
            scale.x = -Mathf.Abs(scale.x);
        }
        transform.localScale = scale;
    }

    private void ExecuteAoEAttack()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, aoeRadius, LayerMask.GetMask("Player"));
        if (hit != null && hit.CompareTag("Player"))
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.TakeDamage(aoeDamage);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") && 
            PlayerController.Instance != null && 
            !PlayerController.Instance.pState.invincible)
        {
            PlayerController.Instance.TakeDamage(contactDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxChaseDistance);
        
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(leftPoint, 0.3f);
            Gizmos.DrawWireSphere(rightPoint, 0.3f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(lastPatrolPosition, 0.2f);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(leftPoint, rightPoint);
        }
        else
        {
            Vector2 previewSpawn = transform.position;
            Vector2 previewLeft = new Vector2(previewSpawn.x - patrolDistance, previewSpawn.y);
            Vector2 previewRight = new Vector2(previewSpawn.x + patrolDistance, previewSpawn.y);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(previewLeft, 0.3f);
            Gizmos.DrawWireSphere(previewRight, 0.3f);
            Gizmos.DrawLine(previewLeft, previewRight);
        }
        
        if (Application.isPlaying)
        {
            Vector3 textPos = transform.position + Vector3.up * 2f;
            UnityEditor.Handles.Label(textPos, $"Estado: {currentAIState}");
        }
    }
}