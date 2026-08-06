using UnityEngine;

public class BossShooter : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Timing")]
    [SerializeField] private float firstShotDelay = 1f;
    [SerializeField] private float fireInterval = 1.5f;

    [Header("Pressure Dialogue")]
    [SerializeField] private string[] pressureDialogues =
    {
        "야근하고 가!",
        "이것만 끝내고 가!",
        "다들 하는데 왜 못 해?",
        "주말에 잠깐 나올 수 있지?",
        "요즘 회사가 어렵잖아!",
        "어딜 가려고?!",
        "퇴사는 승인 못 해!"
    };

    private Transform playerTarget;
    private BossHealth bossHealth;
    private float fireTimer;
    private int dialogueIndex;

    private void Awake()
    {
        bossHealth = GetComponent<BossHealth>();
    }

    private void OnEnable()
    {
        FindPlayerTarget();
        fireTimer = firstShotDelay;
        dialogueIndex = 0;
    }

    private void Update()
    {
        if (
            bossHealth != null &&
            (bossHealth.IsStunned || bossHealth.IsDefeated)
        )
        {
            return;
        }

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
            FindFirstObjectByType<PlayerController>();

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

        newProjectile.Initialize(
            playerTarget,
            GetNextDialogue()
        );
    }

    private string GetNextDialogue()
    {
        if (
            pressureDialogues == null ||
            pressureDialogues.Length == 0
        )
        {
            return string.Empty;
        }

        string dialogue =
            pressureDialogues[
                dialogueIndex % pressureDialogues.Length
            ];

        dialogueIndex =
            (dialogueIndex + 1) %
            pressureDialogues.Length;

        return dialogue;
    }
}
