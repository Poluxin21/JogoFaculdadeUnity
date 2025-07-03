using UnityEngine;

public class GolemEnemy : Enemy
{
    [Header("Golem Settings")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private Animator animator;

    private float attackTimer;

    protected override void Start()
    {
        base.Start();
        health = 5f;
        damage = 2f;
    }

    protected override void Update()
    {
        base.Update();

        if (PauseManager.isPaused) return;

        if (PlayerController.Instance.pState.alive == false) return;

        if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) <= detectionRange)
        {
            MoveTowardPlayer();

            if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) <= attackRange)
            {
                TryAttack();
            }
        }

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
    }

    private void MoveTowardPlayer()
    {
        if (isRecoiling) return;

        Vector2 dir = (PlayerController.Instance.transform.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * speed, rb.linearVelocity.y);

        Vector3 scale = transform.localScale;
        scale.x = dir.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private void TryAttack()
    {
        if (attackTimer <= 0)
        {
            animator.SetTrigger("Attack");
            Invoke(nameof(ExecuteAttack), 0.5f);
            attackTimer = attackCooldown;
        }
    }

    private void ExecuteAttack()
    {
        Vector2 dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 attackPos = (Vector2)transform.position + dir * 1f;

        Collider2D hit = Physics2D.OverlapCircle(attackPos, 1f, LayerMask.GetMask("Player"));
        if (hit && hit.CompareTag("Player"))
        {
            PlayerController.Instance.TakeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return;
        Gizmos.color = Color.red;
        Vector2 dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 attackPos = (Vector2)transform.position + dir * 1f;
        Gizmos.DrawWireSphere(attackPos, 1f);
    }
}
