using UnityEngine;
using UnityEngine.SceneManagement;

public class StageTransition : MonoBehaviour
{
    [SerializeField] private float transitionDelay = 2f;

    private BossHealth bossHealth;
    private bool isTransitioning;
    private float transitionTimer;

    private void Update()
    {
        if (isTransitioning)
        {
            transitionTimer -= Time.deltaTime;

            if (transitionTimer <= 0f)
            {
                LoadNextStage();
            }

            return;
        }

        if (bossHealth == null)
        {
            bossHealth =
                FindFirstObjectByType<BossHealth>();

            if (bossHealth == null)
            {
                return;
            }
        }

        if (!bossHealth.IsDefeated)
        {
            return;
        }

        isTransitioning = true;
        transitionTimer = transitionDelay;

        Debug.Log("STAGE TRANSITION START");
    }

    private void LoadNextStage()
    {
        int currentSceneIndex =
            SceneManager.GetActiveScene().buildIndex;

        int nextSceneIndex =
            currentSceneIndex + 1;

        Debug.Log(
            "LOAD NEXT STAGE : " +
            currentSceneIndex +
            " → " +
            nextSceneIndex
        );

        if (
            nextSceneIndex <
            SceneManager.sceneCountInBuildSettings
        )
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}