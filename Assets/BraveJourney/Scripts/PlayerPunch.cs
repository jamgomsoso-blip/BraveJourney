using UnityEngine;

public class PlayerPunch : MonoBehaviour
{
    [Header("Punch")]
    [SerializeField] private KeyCode punchKey = KeyCode.A;
    [SerializeField] private float attackOffsetX = 1f;
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.35f;

    private float attackCooldownTimer;

    private void Update()
    {
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        if (
            Input.GetKeyDown(punchKey) &&
            attackCooldownTimer <= 0f
        )
        {
            attackCooldownTimer = attackCooldown;
            TryPunch();
        }
    }

    private void TryPunch()
    {
        Vector2 attackCenter =
            (Vector2)transform.position +
            Vector2.right * attackOffsetX;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                attackCenter,
                attackRadius
            );

        foreach (Collider2D hit in hits)
        {
            BossHealth bossHealth =
                hit.GetComponentInParent<BossHealth>();

            if (
                bossHealth == null ||
                bossHealth.IsDefeated
            )
            {
                continue;
            }

            if (!bossHealth.IsStunned)
            {
                Debug.Log(
                    "공격 실패: 보스가 스턴 상태가 아닙니다."
                );
                return;
            }

            bool didDamage =
                bossHealth.TakeDamage(
                    attackDamage,
                    false
                );

            if (didDamage)
            {
                Debug.Log("PUNCH HIT");
            }
            else
            {
                Debug.Log(
                    "공격 실패: 이번 스턴에서 이미 공격했습니다."
                );
            }

            return;
        }

        Debug.Log(
            "공격 실패: 보스가 공격 범위 밖입니다."
        );
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 attackCenter =
            transform.position +
            Vector3.right * attackOffsetX;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            attackCenter,
            attackRadius
        );
    }
}