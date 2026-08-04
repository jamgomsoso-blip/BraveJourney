using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float reflectedSpeedMultiplier = 2f;
    [SerializeField] private float lifeTime = 8f;

    [Header("Damage")]
    [SerializeField] private int damage = 1;

    private Transform target;
    private Vector3 moveDirection;

    private bool isReflected;
    private bool hasResolved;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer =
            GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
    }

    public void Initialize(Transform playerTarget)
    {
        target = playerTarget;

        UpdateDirectionToTarget();
    }

    private void Update()
    {
        if (!isReflected && target != null)
        {
            // 반사되기 전에는 플레이어를 계속 따라간다.
            UpdateDirectionToTarget();
        }

        transform.localPosition +=
            moveDirection *
            speed *
            Time.deltaTime;

        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateDirectionToTarget()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetLocalPosition;

        if (transform.parent != null)
        {
            targetLocalPosition =
                transform.parent.InverseTransformPoint(
                    target.position
                );
        }
        else
        {
            targetLocalPosition =
                target.position;
        }

        moveDirection =
            (
                targetLocalPosition -
                transform.localPosition
            ).normalized;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasResolved)
        {
            return;
        }

        if (!isReflected)
        {
            ParryHitbox parryHitbox =
                other.GetComponent<ParryHitbox>();

            if (
                parryHitbox != null &&
                parryHitbox.CanReflect
            )
            {
                Reflect();
                return;
            }

            PlayerHealth playerHealth =
                other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                hasResolved = true;

                playerHealth.TakeDamage(damage);

                Destroy(gameObject);
            }

            return;
        }

        BossHealth bossHealth =
            other.GetComponent<BossHealth>();

        if (bossHealth != null)
        {
            hasResolved = true;

            bossHealth.TakeDamage(damage);

            Destroy(gameObject);
        }
    }

    private void Reflect()
    {
        isReflected = true;
        target = null;

        speed *= reflectedSpeedMultiplier;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.cyan;
        }

        BossHealth bossHealth =
            FindObjectOfType<BossHealth>();

        if (bossHealth != null)
        {
            Vector3 bossLocalPosition;

            if (transform.parent != null)
            {
                bossLocalPosition =
                    transform.parent.InverseTransformPoint(
                        bossHealth.transform.position
                    );
            }
            else
            {
                bossLocalPosition =
                    bossHealth.transform.position;
            }

            moveDirection =
                (
                    bossLocalPosition -
                    transform.localPosition
                ).normalized;
        }
        else
        {
            moveDirection = Vector3.right;
        }
    }
}