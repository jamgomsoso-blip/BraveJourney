using UnityEngine;

public sealed class StageCourseBuilder : MonoBehaviour
{
    [Header("Course References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform boss;
    [SerializeField] private Transform bossStartTrigger;
    [SerializeField] private GameObject lowJumpObstacleTemplate;
    [SerializeField] private GameObject highJumpObstacleTemplate;
    [SerializeField] private GameObject slideObstacleTemplate;
    [SerializeField] private Font uiFont;

    [Header("Course Timing")]
    [SerializeField] private float runnerSpeed = 12f;
    [SerializeField] private float targetRunDuration = 34f;
    [SerializeField] private float bossSpacing = 12f;
    [SerializeField] private float groundSurfaceY = -3.5f;

    [Header("Obstacle Schedule (Seconds)")]
    [SerializeField] private float[] additionalObstacleTimes =
    {
        8f,
        11f,
        13.5f,
        16.5f,
        19f,
        21.5f,
        24.5f,
        27f,
        29.5f,
        32f
    };

    [SerializeField] private float[] runnerHazardTimes =
    {
        6.8f,
        15f,
        23f,
        30.5f
    };

    private Transform generatedCourseRoot;
    private PlayerController playerController;
    private float courseStartX;
    private float courseEndX;
    private bool courseBuilt;

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        courseStartX = player.position.x;
        courseEndX =
            courseStartX + runnerSpeed * targetRunDuration;

        playerController =
            player.GetComponent<PlayerController>();

        PositionBossArea();
        BuildCourse();
    }

    private bool ValidateReferences()
    {
        if (
            player != null &&
            boss != null &&
            bossStartTrigger != null &&
            lowJumpObstacleTemplate != null &&
            highJumpObstacleTemplate != null &&
            slideObstacleTemplate != null
        )
        {
            return true;
        }

        Debug.LogError(
            "StageCourseBuilder의 필수 참조가 연결되지 않았습니다."
        );

        return false;
    }

    private void PositionBossArea()
    {
        bossStartTrigger.position = new Vector3(
            courseEndX,
            bossStartTrigger.position.y,
            bossStartTrigger.position.z
        );

        boss.position = new Vector3(
            courseEndX + bossSpacing,
            boss.position.y,
            boss.position.z
        );
    }

    private void BuildCourse()
    {
        if (courseBuilt)
        {
            return;
        }

        courseBuilt = true;

        GameObject rootObject =
            new GameObject("GeneratedStage01Course");

        generatedCourseRoot = rootObject.transform;

        GameObject[] templates =
        {
            lowJumpObstacleTemplate,
            slideObstacleTemplate,
            highJumpObstacleTemplate
        };

        for (
            int index = 0;
            index < additionalObstacleTimes.Length;
            index++
        )
        {
            float spawnTime = additionalObstacleTimes[index];

            if (
                spawnTime <= 0f ||
                spawnTime >= targetRunDuration - 1f
            )
            {
                continue;
            }

            GameObject template =
                templates[index % templates.Length];

            Vector3 spawnPosition =
                template.transform.position;

            spawnPosition.x =
                courseStartX + runnerSpeed * spawnTime;

            GameObject obstacle = Instantiate(
                template,
                spawnPosition,
                template.transform.rotation,
                generatedCourseRoot
            );

            obstacle.name =
                "CourseObstacle_" + (index + 1).ToString("00");
        }

        BuildRunnerHazards();
    }

    private void BuildRunnerHazards()
    {
        SpriteRenderer templateRenderer =
            lowJumpObstacleTemplate
                .GetComponent<SpriteRenderer>();

        if (
            templateRenderer == null ||
            templateRenderer.sprite == null
        )
        {
            Debug.LogError(
                "위험요소 경고에 사용할 스프라이트가 없습니다."
            );

            return;
        }

        for (
            int index = 0;
            index < runnerHazardTimes.Length;
            index++
        )
        {
            float targetX =
                courseStartX +
                runnerSpeed * runnerHazardTimes[index];

            GameObject hazardObject =
                new GameObject(
                    "RunnerHazard_" +
                    (index + 1).ToString("00")
                );

            hazardObject.transform.SetParent(
                generatedCourseRoot,
                false
            );

            StageHazard hazard =
                hazardObject.AddComponent<StageHazard>();

            hazard.ConfigureRunner(
                player,
                templateRenderer.sprite,
                index % 2 == 0
                    ? StageHazardType.Ground
                    : StageHazardType.Falling,
                targetX,
                groundSurfaceY,
                runnerSpeed
            );
        }
    }

    private void OnGUI()
    {
        if (player == null || courseEndX <= courseStartX)
        {
            return;
        }

        float progress = Mathf.Clamp01(
            (player.position.x - courseStartX) /
            (courseEndX - courseStartX)
        );

        Rect panelRect = new Rect(
            Screen.width * 0.5f - 170f,
            102f,
            340f,
            54f
        );

        DrawPanel(panelRect, new Color(0f, 0f, 0f, 0.58f));

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.font = uiFont != null ? uiFont : GUI.skin.font;
        style.fontSize = 18;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;

        string phaseText =
            playerController != null &&
            playerController.IsBossBattle
                ? "STAGE 01  ·  주임 보스전"
                : "STAGE 01  ·  출근길 탈출";

        GUI.Label(
            new Rect(
                panelRect.x,
                panelRect.y,
                panelRect.width,
                28f
            ),
            phaseText,
            style
        );

        Rect progressBackground = new Rect(
            panelRect.x + 20f,
            panelRect.y + 36f,
            panelRect.width - 40f,
            8f
        );

        DrawPanel(
            progressBackground,
            new Color(0.18f, 0.18f, 0.18f, 1f)
        );

        DrawPanel(
            new Rect(
                progressBackground.x,
                progressBackground.y,
                progressBackground.width * progress,
                progressBackground.height
            ),
            new Color(0.15f, 0.8f, 0.72f, 1f)
        );
    }

    private static void DrawPanel(Rect rect, Color color)
    {
        Color previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previousColor;
    }
}
