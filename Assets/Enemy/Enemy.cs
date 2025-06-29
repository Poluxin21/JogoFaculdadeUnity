using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;

    [SerializeField] protected float speed;
    [SerializeField] protected float damage;

    // Novo: dano bônus se player estiver de costas
    [SerializeField] protected float bonusBackstabDamage = 10f;

    // Novo: cooldown entre ataques
    [SerializeField] private float attackCooldown = 1f;
    private float lastAttackTime = -999f;

    protected float recoilTimer;
    protected Rigidbody2D rb;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }

        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
    }

    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;
        if (!isRecoiling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
            isRecoiling = true;
        }
    }

    protected void OnTriggerStay2D(Collider2D _other)
    {
        if (_other.CompareTag("Player") && !PlayerController.Instance.pState.invincible)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Attack();
                PlayerController.Instance.HitStopTime(0, 5, 0.5f);
                lastAttackTime = Time.time;
            }
        }
    }

    protected virtual void Attack()
    {
        float finalDamage = damage;

        Vector2 toPlayer = PlayerController.Instance.transform.position - transform.position;
        float directionToPlayer = Mathf.Sign(toPlayer.x);

        bool playerDeCostas = (directionToPlayer > 0 && !PlayerController.Instance.pState.lookingRight) ||
                              (directionToPlayer < 0 && PlayerController.Instance.pState.lookingRight);

        if (playerDeCostas)
        {
            finalDamage += bonusBackstabDamage;
            Debug.Log("Backstab! Dano aumentado.");
        }

        PlayerController.Instance.TakeDamage(finalDamage);
    }
}
