using UnityEngine;

public class BossStartTrigger : MonoBehaviour
{
    [SerializeField] private GameObject bossObject;

    private bool hasStarted;

    private void Awake()
    {
        if (bossObject == null)
        {
            return;
        }

        StageCourseBuilder.EnsureForScene(
            transform,
            bossObject.transform
        );

        StageTransition.EnsureForScene(gameObject);
        BossComicCutscene cutscene =
            BossComicCutscene.EnsureForScene(gameObject);
        BossHazardController.EnsureOnBoss(bossObject);

        StageProfile profile =
            StageProfileCatalog.GetCurrentOrDefault();
        BossHealth bossHealth =
            bossObject.GetComponent<BossHealth>();

        cutscene.ShowPrologue(
            profile,
            bossHealth != null ? bossHealth.UiFont : null
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasStarted)
        {
            return;
        }

        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            return;
        }

        PlayerController playerController =
            other.GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError(
                "PlayerController를 찾을 수 없습니다."
            );
            return;
        }

        hasStarted = true;

        StageProfile profile =
            StageProfileCatalog.GetCurrentOrDefault();
        BossHealth bossHealth = bossObject != null
            ? bossObject.GetComponent<BossHealth>()
            : null;
        BossComicCutscene cutscene =
            BossComicCutscene.EnsureForScene(gameObject);

        cutscene.ShowIntro(
            profile,
            bossHealth != null ? bossHealth.UiFont : null,
            () => BeginBossBattle(playerController)
        );

        gameObject.SetActive(false);
    }

    private void BeginBossBattle(
        PlayerController playerController
    )
    {
        if (playerController == null)
        {
            return;
        }

        if (bossObject != null)
        {
            bossObject.SetActive(true);
        }

        playerController.StartBossBattle();

        Debug.Log("보스전 구간 진입");
    }
}
