using System.Collections.Generic;
using UnityEngine;

public sealed class BossHazardController : MonoBehaviour
{
    [Header("Attack Timing")]
    [SerializeField] private float firstAttackDelay = 2.5f;
    [SerializeField] private float attackInterval = 4.5f;
    [SerializeField] private float warningDuration = 0.9f;
    [SerializeField] private float dangerDuration = 1.1f;

    [Header("Arena")]
    [SerializeField] private float groundSurfaceY = -3.5f;
    [SerializeField] private float fallingSpeed = 12f;
    [SerializeField] private float[] targetOffsets =
    {
        0f,
        1.8f,
        -1.4f
    };

    private readonly List<StageHazard> activeHazards =
        new List<StageHazard>();

    private BossHealth bossHealth;
    private SpriteRenderer bossRenderer;
    private Transform playerTarget;
    private float attackTimer;
    private int attackIndex;

    private void Awake()
    {
        bossHealth = GetComponent<BossHealth>();
        bossRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        FindPlayerTarget();
        attackTimer = firstAttackDelay;
        attackIndex = 0;
    }

    private void Update()
    {
        if (
            bossHealth == null ||
            bossHealth.IsStunned ||
            bossHealth.IsDefeated
        )
        {
            return;
        }

        if (playerTarget == null)
        {
            FindPlayerTarget();

            if (playerTarget == null)
            {
                return;
            }
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
        {
            return;
        }

        SpawnHazard();
        attackTimer = attackInterval;
    }

    private void FindPlayerTarget()
    {
        PlayerController playerController =
            FindFirstObjectByType<PlayerController>();

        playerTarget = playerController != null
            ? playerController.transform
            : null;
    }

    private void SpawnHazard()
    {
        if (
            bossRenderer == null ||
            bossRenderer.sprite == null
        )
        {
            return;
        }

        PruneFinishedHazards();

        float offset =
            targetOffsets != null && targetOffsets.Length > 0
                ? targetOffsets[
                    attackIndex % targetOffsets.Length
                ]
                : 0f;

        float targetX = ClampToCamera(
            playerTarget.position.x + offset
        );

        StageHazardType hazardType =
            attackIndex % 2 == 0
                ? StageHazardType.Ground
                : StageHazardType.Falling;

        GameObject hazardObject =
            new GameObject(
                "BossHazard_" +
                (attackIndex + 1).ToString("00")
            );

        StageHazard hazard =
            hazardObject.AddComponent<StageHazard>();

        hazard.ConfigureImmediate(
            bossRenderer.sprite,
            hazardType,
            targetX,
            groundSurfaceY,
            warningDuration,
            dangerDuration,
            fallingSpeed
        );

        activeHazards.Add(hazard);
        attackIndex++;
    }

    private float ClampToCamera(float targetX)
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null || !mainCamera.orthographic)
        {
            return targetX;
        }

        float halfWidth =
            mainCamera.orthographicSize *
            mainCamera.aspect;

        float minimumX =
            mainCamera.transform.position.x -
            halfWidth + 1f;

        float maximumX =
            mainCamera.transform.position.x +
            halfWidth - 1f;

        return Mathf.Clamp(targetX, minimumX, maximumX);
    }

    private void PruneFinishedHazards()
    {
        for (
            int index = activeHazards.Count - 1;
            index >= 0;
            index--
        )
        {
            StageHazard hazard = activeHazards[index];

            if (hazard == null || hazard.IsFinished)
            {
                if (hazard != null)
                {
                    Destroy(hazard.gameObject);
                }

                activeHazards.RemoveAt(index);
            }
        }
    }

    private void OnDisable()
    {
        foreach (StageHazard hazard in activeHazards)
        {
            if (hazard != null)
            {
                hazard.CancelHazard();
            }
        }

        activeHazards.Clear();
    }
}
