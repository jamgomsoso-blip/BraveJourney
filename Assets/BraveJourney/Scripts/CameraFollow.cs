using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float horizontalOffset = 5f;
    [SerializeField] private float bossBattleHorizontalPadding = 0.8f;

    private float fixedY;
    private float fixedZ;
    private PlayerController playerController;
    private Transform boss;
    private bool hasFramedBossBattle;

    private void Start()
    {
        fixedY = transform.position.y;
        fixedZ = transform.position.z;

        if (player != null)
        {
            playerController =
                player.GetComponent<PlayerController>();
        }

        BossHealth bossHealth =
            FindFirstObjectByType<BossHealth>(
                FindObjectsInactive.Include
            );

        boss = bossHealth != null
            ? bossHealth.transform
            : null;
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        // 보스전이 시작되면 현재 위치에서 카메라 정지
        if (
            playerController != null &&
            playerController.IsBossBattle
        )
        {
            if (!hasFramedBossBattle)
            {
                FrameBossBattle();
                hasFramedBossBattle = true;
            }

            return;
        }

        transform.position = new Vector3(
            player.position.x + horizontalOffset,
            fixedY,
            fixedZ
        );
    }

    private void FrameBossBattle()
    {
        if (boss == null)
        {
            BossHealth bossHealth =
                FindFirstObjectByType<BossHealth>(
                    FindObjectsInactive.Include
                );

            boss = bossHealth != null
                ? bossHealth.transform
                : null;
        }

        if (boss == null)
        {
            return;
        }

        float leftEdge = player.position.x;
        float rightEdge = boss.position.x;

        SpriteRenderer playerRenderer =
            player.GetComponentInChildren<SpriteRenderer>();

        SpriteRenderer bossRenderer =
            boss.GetComponentInChildren<SpriteRenderer>();

        if (playerRenderer != null)
        {
            leftEdge = playerRenderer.bounds.min.x;
        }

        if (bossRenderer != null)
        {
            rightEdge = bossRenderer.bounds.max.x;
        }

        transform.position = new Vector3(
            (leftEdge + rightEdge) * 0.5f,
            fixedY,
            fixedZ
        );

        Camera cameraComponent = GetComponent<Camera>();

        if (cameraComponent == null || !cameraComponent.orthographic)
        {
            return;
        }

        float requiredHalfWidth =
            (rightEdge - leftEdge) * 0.5f +
            bossBattleHorizontalPadding;

        float currentHalfWidth =
            cameraComponent.orthographicSize *
            cameraComponent.aspect;

        if (requiredHalfWidth > currentHalfWidth)
        {
            cameraComponent.orthographicSize =
                requiredHalfWidth / cameraComponent.aspect;
        }
    }
}
