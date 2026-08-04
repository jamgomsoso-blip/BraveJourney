using UnityEngine;

public class BossShooter : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Timing")]
    [SerializeField] private float firstShotDelay = 1f;
    [SerializeField] private float fireInterval = 1.5f;

    private Transform playerTarget;
    private float fireTimer;

    private void OnEnable()
    {
        FindPlayerTarget();
        fireTimer = firstShotDelay;
    }

    private void Update()
    {
        if (playerTarget == null)
        {
            FindPlayerTarget();
        }

        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f)
        {
            FireProjectile();
            fireTimer = fireInterval;
        }
    }

    private void FindPlayerTarget()
    {
        PlayerController playerController =
            FindObjectOfType<PlayerController>();

        if (playerController == null)
        {
            playerTarget = null;
            return;
        }

        Transform projectileTarget =
            playerController.transform.Find(
                "ProjectileTarget"
            );

        // 목표점이 있으면 가슴 위치를 사용하고,
        // 없으면 기존 Player 위치를 대신 사용한다.
        if (projectileTarget != null)
        {
            playerTarget = projectileTarget;
        }
        else
        {
            playerTarget =
                playerController.transform;
        }
    }

    private void FireProjectile()
    {
        if (
            projectilePrefab == null ||
            firePoint == null ||
            playerTarget == null
        )
        {
            return;
        }

        Transform projectileParent = null;

        if (Camera.main != null)
        {
            projectileParent =
                Camera.main.transform;
        }

        Projectile newProjectile =
            Instantiate(
                projectilePrefab,
                firePoint.position,
                Quaternion.identity,
                projectileParent
            );

        newProjectile.Initialize(playerTarget);
    }
}