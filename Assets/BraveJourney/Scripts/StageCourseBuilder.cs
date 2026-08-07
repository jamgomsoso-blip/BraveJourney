using UnityEngine;

public sealed class StageCourseBuilder : MonoBehaviour
{
    private static readonly Color[] SafetyColors =
    {
        new Color(1f, 0.72f, 0.08f, 1f),
        new Color(0.95f, 0.28f, 0.08f, 1f),
        new Color(0.28f, 0.62f, 0.82f, 1f),
        new Color(0.88f, 0.14f, 0.12f, 1f)
    };

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

    [Header("Fairness Timing")]
    [SerializeField] private float jumpRecoveryBeforeGroundHazard = 1.1f;
    [SerializeField] private float fallingRecoveryBeforeHazard = 1.4f;
    [SerializeField] private float slideRecoveryBeforeHazard = 1f;
    [SerializeField] private float hazardRecoveryBeforeObstacle = 1.5f;
    [SerializeField] private float obstacleMinimumSpacing = 1.65f;
    [SerializeField] private float obstacleRelocationRange = 3f;
    [SerializeField] private float obstacleRelocationStep = 0.1f;

    [Header("Obstacle Schedule (Seconds)")]
    [SerializeField] private float[] additionalObstacleTimes =
    {
        7.9f,
        9.1f,
        10.3f,
        11.5f,
        12.7f,
        16.2f,
        17.4f,
        18.6f,
        19.8f,
        21f,
        24.1f,
        25.3f,
        26.5f,
        27.7f,
        31.8f,
        32.8f
    };

    [SerializeField] private float[] runnerHazardTimes =
    {
        6.8f,
        15f,
        23f,
        30.5f
    };

    private Transform generatedCourseRoot;
    private StageProfile stageProfile;
    private float courseStartX;
    private float courseEndX;
    private bool courseBuilt;

    public static void EnsureForScene(
        Transform triggerTransform,
        Transform bossTransform
    )
    {
        if (
            triggerTransform == null ||
            bossTransform == null ||
            FindFirstObjectByType<StageCourseBuilder>() != null
        )
        {
            return;
        }

        StageCourseBuilder builder =
            triggerTransform.gameObject.AddComponent<StageCourseBuilder>();

        builder.bossStartTrigger = triggerTransform;
        builder.boss = bossTransform;
    }

    private void Start()
    {
        stageProfile = StageProfileCatalog.GetCurrentOrDefault();
        ApplyStageProfile();
        ResolveMissingReferences();

        StagePresentation.EnsureForScene(Camera.main);
        GameAudioFeedback.EnsureForScene();
        GameAudioFeedback.SetBossBattle(false);

        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        courseStartX = player.position.x;
        courseEndX =
            courseStartX + runnerSpeed * targetRunDuration;

        PositionBossArea();
        BuildCourse();
    }

    private void ApplyStageProfile()
    {
        targetRunDuration = stageProfile.RunDuration;
        additionalObstacleTimes = stageProfile.ObstacleTimes;
        runnerHazardTimes = stageProfile.RunnerHazardTimes;
    }

    private void ResolveMissingReferences()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.Find("Player");
            player = playerObject != null
                ? playerObject.transform
                : null;
        }

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

        if (bossStartTrigger == null)
        {
            GameObject triggerObject =
                GameObject.Find("BossStartTrigger");

            bossStartTrigger = triggerObject != null
                ? triggerObject.transform
                : null;
        }

        lowJumpObstacleTemplate = ResolveTemplate(
            lowJumpObstacleTemplate,
            "JumpObstacle_Low"
        );

        highJumpObstacleTemplate = ResolveTemplate(
            highJumpObstacleTemplate,
            "JumpObstacle_High"
        );

        slideObstacleTemplate = ResolveTemplate(
            slideObstacleTemplate,
            "SlideObstacle_Test"
        );

        if (uiFont == null && boss != null)
        {
            BossHealth bossHealth =
                boss.GetComponent<BossHealth>();

            if (bossHealth != null)
            {
                uiFont = bossHealth.UiFont;
            }
        }
    }

    private static GameObject ResolveTemplate(
        GameObject currentTemplate,
        string objectName
    )
    {
        return currentTemplate != null
            ? currentTemplate
            : GameObject.Find(objectName);
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
            new GameObject(
                "GeneratedStage" +
                stageProfile.StageNumber.ToString("00") +
                "Course"
            );

        generatedCourseRoot = rootObject.transform;

        BuildOfficeFloor();

        GameObject[] obstaclePattern =
        {
            lowJumpObstacleTemplate,
            lowJumpObstacleTemplate,
            slideObstacleTemplate,
            highJumpObstacleTemplate,
            lowJumpObstacleTemplate,
            slideObstacleTemplate,
            lowJumpObstacleTemplate,
            highJumpObstacleTemplate
        };

        float lastObstacleTime = float.NegativeInfinity;

        for (
            int index = 0;
            index < additionalObstacleTimes.Length;
            index++
        )
        {
            float requestedTime = additionalObstacleTimes[index];

            if (
                requestedTime <= 0f ||
                requestedTime >= targetRunDuration - 1f
            )
            {
                continue;
            }

            GameObject template =
                obstaclePattern[
                    index % obstaclePattern.Length
                ];

            if (
                !TryResolveSafeObstacleTime(
                    requestedTime,
                    template,
                    lastObstacleTime,
                    out float spawnTime
                )
            )
            {
                continue;
            }

            lastObstacleTime = spawnTime;

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
                "CourseObstacle_" +
                GetObstacleTypeName(template) +
                "_" +
                (index + 1).ToString("00");

            ApplyObstacleVariation(
                obstacle,
                template,
                index
            );
        }

        BuildRunnerHazards();
        DisableTemplateObstacles();
    }

    private void BuildOfficeFloor()
    {
        GameObject groundObject =
            GameObject.Find("Ground");

        if (groundObject != null)
        {
            SpriteRenderer groundRenderer =
                groundObject.GetComponent<SpriteRenderer>();

            if (groundRenderer != null)
            {
                groundRenderer.enabled = false;
            }
        }

        Sprite floorSprite =
            OfficeSpriteCatalog.Platform48;

        if (floorSprite == null)
        {
            return;
        }

        const float tileWidth = 8f;
        const float tileHeight = 0.72f;
        const float tileOverlap = 0.025f;
        float floorStartX = courseStartX - 18f;
        float floorEndX =
            courseEndX + bossSpacing + 24f;

        int tileCount = Mathf.CeilToInt(
            (floorEndX - floorStartX) /
            (tileWidth - tileOverlap)
        );

        Vector2 floorSpriteSize = floorSprite.bounds.size;
        Vector3 floorScale = new Vector3(
            floorSpriteSize.x > 0f
                ? tileWidth / floorSpriteSize.x
                : 1f,
            floorSpriteSize.y > 0f
                ? tileHeight / floorSpriteSize.y
                : 1f,
            1f
        );

        for (int index = 0; index < tileCount; index++)
        {
            GameObject tileObject =
                new GameObject(
                    "OfficeFloorTile_" +
                    index.ToString("000")
                );

            tileObject.transform.SetParent(
                generatedCourseRoot,
                false
            );

            tileObject.transform.position =
                new Vector3(
                    floorStartX +
                    index * (tileWidth - tileOverlap) +
                    tileWidth * 0.5f,
                    groundSurfaceY - tileHeight * 0.5f,
                    0f
                );

            tileObject.transform.localScale =
                floorScale;

            SpriteRenderer tileRenderer =
                tileObject.AddComponent<SpriteRenderer>();

            tileRenderer.sprite = floorSprite;
            tileRenderer.color = Color.white;
            tileRenderer.sortingOrder = -1;
        }
    }

    private bool TryResolveSafeObstacleTime(
        float requestedTime,
        GameObject obstacleTemplate,
        float previousObstacleTime,
        out float resolvedTime
    )
    {
        float searchStep = Mathf.Max(obstacleRelocationStep, 0.05f);
        int searchCount = Mathf.CeilToInt(
            Mathf.Max(obstacleRelocationRange, 0f) / searchStep
        );

        for (int stepIndex = 0; stepIndex <= searchCount; stepIndex++)
        {
            float offset = stepIndex * searchStep;

            if (
                IsObstacleCandidateSafe(
                    requestedTime + offset,
                    obstacleTemplate,
                    previousObstacleTime
                )
            )
            {
                resolvedTime = requestedTime + offset;
                return true;
            }

            if (
                stepIndex > 0 &&
                IsObstacleCandidateSafe(
                    requestedTime - offset,
                    obstacleTemplate,
                    previousObstacleTime
                )
            )
            {
                resolvedTime = requestedTime - offset;
                return true;
            }
        }

        resolvedTime = 0f;
        return false;
    }

    private bool IsObstacleCandidateSafe(
        float obstacleTime,
        GameObject obstacleTemplate,
        float previousObstacleTime
    )
    {
        return
            obstacleTime > 0f &&
            obstacleTime < targetRunDuration - 1f &&
            obstacleTime >=
                previousObstacleTime + obstacleMinimumSpacing &&
            IsObstacleTimingSafe(obstacleTime, obstacleTemplate);
    }

    private bool IsObstacleTimingSafe(
        float obstacleTime,
        GameObject obstacleTemplate
    )
    {
        bool isSlide =
            obstacleTemplate == slideObstacleTemplate;

        for (
            int index = 0;
            index < runnerHazardTimes.Length;
            index++
        )
        {
            StageHazardType hazardType = GetPatternItem(
                stageProfile.RunnerHazardPattern,
                index,
                index % 2 == 0
                    ? StageHazardType.Ground
                    : StageHazardType.Falling
            );

            float recoveryBeforeHazard = isSlide
                ? slideRecoveryBeforeHazard
                : hazardType == StageHazardType.Ground
                    ? jumpRecoveryBeforeGroundHazard
                    : fallingRecoveryBeforeHazard;

            float timeUntilHazard =
                runnerHazardTimes[index] - obstacleTime;

            if (
                timeUntilHazard >= 0f &&
                timeUntilHazard < recoveryBeforeHazard
            )
            {
                return false;
            }

            if (
                timeUntilHazard < 0f &&
                -timeUntilHazard <
                hazardRecoveryBeforeObstacle
            )
            {
                return false;
            }
        }

        return true;
    }

    private string GetObstacleTypeName(GameObject template)
    {
        return template == slideObstacleTemplate
            ? "Slide"
            : "Jump";
    }

    private void DisableTemplateObstacles()
    {
        lowJumpObstacleTemplate.SetActive(false);
        highJumpObstacleTemplate.SetActive(false);
        slideObstacleTemplate.SetActive(false);
    }

    private void ApplyObstacleVariation(
        GameObject obstacle,
        GameObject template,
        int index
    )
    {
        SpriteRenderer obstacleRenderer =
            obstacle.GetComponent<SpriteRenderer>();

        bool isHigh =
            template == highJumpObstacleTemplate;
        bool isSlide =
            template == slideObstacleTemplate;

        Sprite officeSprite =
            OfficeSpriteCatalog.GetObstacleSprite(
                isHigh,
                isSlide,
                index
            );

        if (obstacleRenderer != null)
        {
            if (officeSprite != null)
            {
                float visualScale =
                    isSlide
                        ? 1.3f
                        : isHigh
                            ? 1.45f
                            : 1.6f;

                AddOfficeObstacleVisual(
                    obstacle,
                    obstacleRenderer,
                    officeSprite,
                    visualScale,
                    !isSlide
                );
            }
            else
            {
                obstacleRenderer.sprite =
                    StagePresentation.PixelSprite;
                obstacleRenderer.color =
                    SafetyColors[
                        index % SafetyColors.Length
                    ];
            }
        }

        Vector3 scale = obstacle.transform.localScale;
        int variation = index % 3;

        if (template == lowJumpObstacleTemplate)
        {
            float multiplier = 0.9f + variation * 0.1f;
            scale.x *= multiplier;
            scale.y *= multiplier;
        }
        else if (template == slideObstacleTemplate)
        {
            scale.x *= 0.9f + variation * 0.08f;
        }
        else if (template == highJumpObstacleTemplate)
        {
            scale.y *= 0.82f + variation * 0.08f;
        }

        obstacle.transform.localScale = scale;

        if (officeSprite == null)
        {
            AddObstacleDetails(
                obstacle,
                template,
                index
            );
        }
    }

    private static void AddOfficeObstacleVisual(
        GameObject obstacle,
        SpriteRenderer sourceRenderer,
        Sprite officeSprite,
        float visualScale,
        bool alignBottom
    )
    {
        sourceRenderer.enabled = false;

        GameObject visualObject =
            new GameObject("OfficeObstacleVisual");

        visualObject.transform.SetParent(
            obstacle.transform,
            false
        );

        Vector2 spriteSize = officeSprite.bounds.size;

        if (alignBottom)
        {
            visualObject.transform.localPosition =
                new Vector3(
                    0f,
                    Mathf.Max(visualScale - 1f, 0f) *
                    0.5f,
                    0f
                );
        }

        float uniformScale = spriteSize.y > 0f
            ? visualScale / spriteSize.y
            : visualScale;

        visualObject.transform.localScale =
            new Vector3(uniformScale, uniformScale, 1f);

        SpriteRenderer visualRenderer =
            visualObject.AddComponent<SpriteRenderer>();

        visualRenderer.sprite = officeSprite;
        visualRenderer.color = Color.white;
        visualRenderer.sortingLayerID =
            sourceRenderer.sortingLayerID;
        visualRenderer.sortingOrder =
            sourceRenderer.sortingOrder;
        visualRenderer.sharedMaterial =
            sourceRenderer.sharedMaterial;
    }

    private void AddObstacleDetails(
        GameObject obstacle,
        GameObject template,
        int index
    )
    {
        SpriteRenderer obstacleRenderer =
            obstacle.GetComponent<SpriteRenderer>();

        if (obstacleRenderer == null)
        {
            return;
        }

        int sortingOrder = obstacleRenderer.sortingOrder + 1;
        bool isSlide = template == slideObstacleTemplate;
        int stripeCount = isSlide ? 5 : 3;

        for (int stripeIndex = 0;
            stripeIndex < stripeCount;
            stripeIndex++)
        {
            GameObject stripeObject = new GameObject(
                "SafetyStripe_" + stripeIndex
            );

            stripeObject.transform.SetParent(
                obstacle.transform,
                false
            );

            float normalizedX =
                stripeCount <= 1
                    ? 0f
                    : (float)stripeIndex /
                        (stripeCount - 1) - 0.5f;

            stripeObject.transform.localPosition =
                new Vector3(normalizedX * 0.76f, 0f, -0.01f);

            stripeObject.transform.localScale =
                new Vector3(
                    isSlide ? 0.11f : 0.14f,
                    0.82f,
                    1f
                );

            stripeObject.transform.localRotation =
                Quaternion.Euler(0f, 0f, -24f);

            SpriteRenderer stripeRenderer =
                stripeObject.AddComponent<SpriteRenderer>();

            stripeRenderer.sprite = StagePresentation.PixelSprite;
            stripeRenderer.color =
                index % 2 == 0
                    ? new Color(0.08f, 0.09f, 0.11f, 0.9f)
                    : new Color(1f, 0.86f, 0.18f, 0.92f);

            stripeRenderer.sortingOrder = sortingOrder;
        }

        GameObject highlightObject =
            new GameObject("TopHighlight");

        highlightObject.transform.SetParent(
            obstacle.transform,
            false
        );

        highlightObject.transform.localPosition =
            new Vector3(0f, 0.42f, -0.02f);

        highlightObject.transform.localScale =
            new Vector3(0.88f, 0.08f, 1f);

        SpriteRenderer highlightRenderer =
            highlightObject.AddComponent<SpriteRenderer>();

        highlightRenderer.sprite = StagePresentation.PixelSprite;
        highlightRenderer.color =
            new Color(1f, 1f, 1f, 0.42f);
        highlightRenderer.sortingOrder = sortingOrder + 1;
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

            StageHazardType hazardType =
                GetPatternItem(
                    stageProfile.RunnerHazardPattern,
                    index,
                    index % 2 == 0
                        ? StageHazardType.Ground
                        : StageHazardType.Falling
                );

            StageHazardTheme hazardTheme =
                GetPatternItem(
                    stageProfile.RunnerHazardThemes,
                    index,
                    StageHazardTheme.Standard
                );

            hazard.ConfigureRunner(
                player,
                templateRenderer.sprite,
                hazardType,
                targetX,
                groundSurfaceY,
                runnerSpeed,
                Mathf.Max(stageProfile.WarningDuration, 0.75f),
                hazardTheme,
                uiFont,
                stageProfile.FallingSpeed
            );
        }
    }

    private static T GetPatternItem<T>(
        T[] pattern,
        int index,
        T fallback
    )
    {
        if (pattern == null || pattern.Length == 0)
        {
            return fallback;
        }

        return pattern[index % pattern.Length];
    }

}
