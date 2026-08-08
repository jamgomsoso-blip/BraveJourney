using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class StageProfile
{
    public int StageNumber;
    public string StageTitle;
    public string BossName;
    public int BossHealth;
    public float RunDuration;
    public float[] ObstacleTimes;
    public float[] RunnerHazardTimes;
    public StageHazardType[] RunnerHazardPattern;
    public StageHazardTheme[] RunnerHazardThemes;
    public float FirstShotDelay;
    public float FireInterval;
    public string[] PressureDialogues;
    public string ReflectedDialogue;
    public float FirstHazardDelay;
    public float HazardInterval;
    public float WarningDuration;
    public float DangerDuration;
    public float FallingSpeed;
    public float[] BossTargetOffsets;
    public StageHazardType[] BossHazardPattern;
    public StageHazardTheme[] BossHazardThemes;
}

public static class StageProfileCatalog
{
    public const int FirstStageNumber = 1;
    public const int LastStageNumber = 7;
    private const float TargetRunDuration = 23f;
    private const float CourseTimingMargin = 1f;

    public static bool TryGetCurrent(out StageProfile profile)
    {
        string sceneName =
            SceneManager.GetActiveScene().name;

        if (!TryParseStageNumber(sceneName, out int stageNumber))
        {
            profile = null;
            return false;
        }

        profile = Create(stageNumber);
        return profile != null;
    }

    public static StageProfile GetCurrentOrDefault()
    {
        return TryGetCurrent(out StageProfile profile)
            ? profile
            : Create(FirstStageNumber);
    }

    public static bool TryParseStageNumber(
        string sceneName,
        out int stageNumber
    )
    {
        stageNumber = 0;

        if (
            string.IsNullOrWhiteSpace(sceneName) ||
            !sceneName.StartsWith("Stage")
        )
        {
            return false;
        }

        string numberText = sceneName.Substring(5);

        return
            int.TryParse(numberText, out stageNumber) &&
            stageNumber >= FirstStageNumber &&
            stageNumber <= LastStageNumber;
    }

    private static StageProfile Create(int stageNumber)
    {
        switch (stageNumber)
        {
            case 1:
                return CreateProfile(
                    1,
                    "출근길 탈출",
                    "주임",
                    3,
                    34f,
                    new[]
                    {
                        7.9f, 9.1f, 10.3f, 11.5f, 12.7f,
                        16.2f, 17.4f, 18.6f, 19.8f, 21f,
                        24.1f, 25.3f, 26.5f, 27.7f,
                        31.8f, 32.8f
                    },
                    new[] { 6.8f, 15f, 23f, 30.5f },
                    new[]
                    {
                        StageHazardType.Ground,
                        StageHazardType.Falling
                    },
                    new[] { StageHazardTheme.Standard },
                    1.2f,
                    2.2f,
                    new[]
                    {
                        "야근하고 가!",
                        "이것만 끝내고 가!",
                        "다들 하는데 왜 못 해?",
                        "주말에 잠깐 나올 수 있지?"
                    },
                    "퇴사하렵니다!",
                    2.5f,
                    4.5f,
                    0.9f,
                    1.1f,
                    12f,
                    new[] { 0f, 1.8f, -1.4f },
                    new[]
                    {
                        StageHazardType.Ground,
                        StageHazardType.Falling
                    },
                    new[] { StageHazardTheme.Standard }
                );

            case 2:
                return CreateProfile(
                    2,
                    "누수 구역 탈출",
                    "대리",
                    3,
                    32f,
                    new[]
                    {
                        5.4f, 7.2f, 9f, 10.8f, 12.6f,
                        15.2f, 17f, 18.8f, 20.6f, 22.4f,
                        25f, 26.8f, 28.6f, 30.4f
                    },
                    new[] { 6.3f, 11.8f, 18.4f, 24.8f, 29.4f },
                    new[]
                    {
                        StageHazardType.Ground,
                        StageHazardType.Ground,
                        StageHazardType.Falling
                    },
                    new[] { StageHazardTheme.Leak },
                    1.35f,
                    2.1f,
                    new[]
                    {
                        "천장에서 좀 새는 것뿐이야!",
                        "양동이 놓고 계속 일해!",
                        "이 정도 물은 문제없어!",
                        "퇴근 전에 누수부터 막아!"
                    },
                    "물 새는 회사는 못 다닙니다!",
                    2.4f,
                    4.3f,
                    1f,
                    1.15f,
                    10f,
                    new[] { 0f, 1.5f, -1.5f, 0.8f },
                    new[]
                    {
                        StageHazardType.Ground,
                        StageHazardType.Ground,
                        StageHazardType.Falling
                    },
                    new[] { StageHazardTheme.Leak }
                );

            case 3:
                return CreateProfile(
                    3,
                    "침하 구역 탈출",
                    "과장",
                    4,
                    33f,
                    CreateDenseObstacleSchedule(33f, 0.25f),
                    new[] { 5.8f, 11.4f, 16.7f, 22.1f, 27.4f, 31f },
                    new[] { StageHazardType.Ground },
                    new[] { StageHazardTheme.Subsidence },
                    1.25f,
                    2f,
                    new[]
                    {
                        "바닥이 조금 꺼졌을 뿐이야!",
                        "보고서는 나중에 써!",
                        "안전보다 일정이 먼저야!",
                        "그 틈 그냥 뛰어넘어!"
                    },
                    "안전 없는 일정은 거부합니다!",
                    2.2f,
                    4f,
                    0.85f,
                    1.2f,
                    11f,
                    new[] { 0f, 1.3f, -1.3f, 2.1f, -2f },
                    new[] { StageHazardType.Ground },
                    new[] { StageHazardTheme.Subsidence }
                );

            case 4:
                return CreateProfile(
                    4,
                    "거푸집 구역 탈출",
                    "차장",
                    4,
                    34f,
                    CreateDenseObstacleSchedule(34f, 0.55f),
                    new[] { 6f, 10.8f, 15.6f, 20.4f, 25.2f, 30f, 32.2f },
                    new[] { StageHazardType.Falling },
                    new[] { StageHazardTheme.Formwork },
                    1.2f,
                    1.9f,
                    new[]
                    {
                        "거푸집 아래로 빨리 지나가!",
                        "낙하물은 네가 알아서 피해!",
                        "공정 늦으면 네 책임이야!",
                        "오늘 안에 무조건 끝내!"
                    },
                    "위험한 지시는 따르지 않습니다!",
                    2.1f,
                    3.8f,
                    0.95f,
                    1.1f,
                    12f,
                    new[] { 0f, 1.8f, -1.8f, 0.9f, -0.9f },
                    new[] { StageHazardType.Falling },
                    new[] { StageHazardTheme.Formwork }
                );

            case 5:
                return CreateProfile(
                    5,
                    "자재 적치장 탈출",
                    "부장",
                    4,
                    35f,
                    CreateDenseObstacleSchedule(35f, 0.9f),
                    new[] { 6.2f, 11.2f, 16.2f, 21.2f, 26.2f, 31.2f, 33.3f },
                    new[]
                    {
                        StageHazardType.Falling,
                        StageHazardType.Falling,
                        StageHazardType.Ground
                    },
                    new[] { StageHazardTheme.Material },
                    1.1f,
                    1.8f,
                    new[]
                    {
                        "자재는 일단 쌓아 둬!",
                        "떨어져도 생산은 멈추지 마!",
                        "예산 없으니 그냥 버텨!",
                        "오늘 물량 전부 처리해!"
                    },
                    "사람보다 물량이 먼저일 수 없습니다!",
                    2f,
                    3.6f,
                    1.05f,
                    1.35f,
                    9f,
                    new[] { 0f, 2f, -2f, 1f, -1f },
                    new[]
                    {
                        StageHazardType.Falling,
                        StageHazardType.Falling,
                        StageHazardType.Ground
                    },
                    new[] { StageHazardTheme.Material }
                );

            case 6:
                return CreateProfile(
                    6,
                    "결재실 탈출",
                    "부사장",
                    5,
                    32f,
                    CreateDenseObstacleSchedule(32f, 0.4f),
                    new[] { 5.5f, 9.8f, 14.1f, 18.4f, 22.7f, 27f, 30.2f },
                    new[]
                    {
                        StageHazardType.Falling,
                        StageHazardType.Ground
                    },
                    new[] { StageHazardTheme.Stamp },
                    1f,
                    1.75f,
                    new[]
                    {
                        "퇴사 결재 반려!",
                        "사유가 부족하니 다시 써!",
                        "후임 구하기 전엔 못 나가!",
                        "승인 도장은 내가 찍는다!"
                    },
                    "퇴사에 승인은 필요 없습니다!",
                    1.9f,
                    3.4f,
                    0.8f,
                    1.1f,
                    14f,
                    new[] { 0f, 1.4f, -1.4f, 2.2f, -2.2f },
                    new[]
                    {
                        StageHazardType.Falling,
                        StageHazardType.Ground
                    },
                    new[] { StageHazardTheme.Stamp }
                );

            case 7:
                return CreateProfile(
                    7,
                    "대표실 최종 탈출",
                    "대표",
                    5,
                    30f,
                    CreateDenseObstacleSchedule(30f, 0.7f),
                    new[] { 5.2f, 8.9f, 12.6f, 16.3f, 20f, 23.7f, 27.4f },
                    new[]
                    {
                        StageHazardType.Ground,
                        StageHazardType.Ground,
                        StageHazardType.Falling,
                        StageHazardType.Falling,
                        StageHazardType.Falling
                    },
                    new[]
                    {
                        StageHazardTheme.Leak,
                        StageHazardTheme.Subsidence,
                        StageHazardTheme.Formwork,
                        StageHazardTheme.Material,
                        StageHazardTheme.Stamp
                    },
                    0.9f,
                    1.6f,
                    new[]
                    {
                        "퇴사는 승인 사항이다!",
                        "네가 나가면 누가 일하지?",
                        "회사를 가족처럼 생각해!",
                        "여기서 끝낼 수 있을 것 같아?"
                    },
                    "퇴사는 통보입니다!",
                    1.7f,
                    3.1f,
                    0.75f,
                    1.15f,
                    14f,
                    new[] { 0f, 1.6f, -1.6f, 2.3f, -2.3f, 0.8f },
                    new[]
                    {
                        StageHazardType.Ground,
                        StageHazardType.Ground,
                        StageHazardType.Falling,
                        StageHazardType.Falling,
                        StageHazardType.Falling
                    },
                    new[]
                    {
                        StageHazardTheme.Leak,
                        StageHazardTheme.Subsidence,
                        StageHazardTheme.Formwork,
                        StageHazardTheme.Material,
                        StageHazardTheme.Stamp
                    }
                );
        }

        return null;
    }

    private static StageProfile CreateProfile(
        int stageNumber,
        string stageTitle,
        string bossName,
        int bossHealth,
        float runDuration,
        float[] obstacleTimes,
        float[] runnerHazardTimes,
        StageHazardType[] runnerHazardPattern,
        StageHazardTheme[] runnerHazardThemes,
        float firstShotDelay,
        float fireInterval,
        string[] pressureDialogues,
        string reflectedDialogue,
        float firstHazardDelay,
        float hazardInterval,
        float warningDuration,
        float dangerDuration,
        float fallingSpeed,
        float[] bossTargetOffsets,
        StageHazardType[] bossHazardPattern,
        StageHazardTheme[] bossHazardThemes
    )
    {
        float adjustedRunDuration = Mathf.Min(
            runDuration,
            TargetRunDuration
        );

        obstacleTimes = FitScheduleToDuration(
            obstacleTimes,
            runDuration,
            adjustedRunDuration
        );
        runnerHazardTimes = FitScheduleToDuration(
            runnerHazardTimes,
            runDuration,
            adjustedRunDuration
        );

        return new StageProfile
        {
            StageNumber = stageNumber,
            StageTitle = stageTitle,
            BossName = bossName,
            BossHealth = bossHealth,
            RunDuration = adjustedRunDuration,
            ObstacleTimes = obstacleTimes,
            RunnerHazardTimes = runnerHazardTimes,
            RunnerHazardPattern = runnerHazardPattern,
            RunnerHazardThemes = runnerHazardThemes,
            FirstShotDelay = firstShotDelay,
            FireInterval = fireInterval,
            PressureDialogues = pressureDialogues,
            ReflectedDialogue = reflectedDialogue,
            FirstHazardDelay = firstHazardDelay,
            HazardInterval = hazardInterval,
            WarningDuration = warningDuration,
            DangerDuration = dangerDuration,
            FallingSpeed = fallingSpeed,
            BossTargetOffsets = bossTargetOffsets,
            BossHazardPattern = bossHazardPattern,
            BossHazardThemes = bossHazardThemes
        };
    }

    private static float[] FitScheduleToDuration(
        float[] sourceTimes,
        float sourceDuration,
        float targetDuration
    )
    {
        if (
            sourceTimes == null ||
            sourceTimes.Length == 0 ||
            sourceDuration <= targetDuration
        )
        {
            return sourceTimes;
        }

        float durationRatio = targetDuration / sourceDuration;
        int targetCount = Mathf.Clamp(
            Mathf.RoundToInt(sourceTimes.Length * durationRatio),
            1,
            sourceTimes.Length
        );
        float sourceRange = Mathf.Max(
            sourceDuration - CourseTimingMargin * 2f,
            0.1f
        );
        float targetRange = Mathf.Max(
            targetDuration - CourseTimingMargin * 2f,
            0.1f
        );
        float[] result = new float[targetCount];

        for (int index = 0; index < targetCount; index++)
        {
            int sourceIndex = targetCount == 1
                ? 0
                : Mathf.RoundToInt(
                    index *
                    (sourceTimes.Length - 1f) /
                    (targetCount - 1f)
                );
            float normalizedTime = Mathf.Clamp01(
                (sourceTimes[sourceIndex] - CourseTimingMargin) /
                sourceRange
            );

            result[index] =
                CourseTimingMargin +
                normalizedTime * targetRange;
        }

        return result;
    }

    private static float[] CreateDenseObstacleSchedule(
        float runDuration,
        float shift
    )
    {
        float[] baseTimes =
        {
            5.2f, 7f, 8.8f, 10.6f, 12.4f,
            15f, 16.8f, 18.6f, 20.4f, 22.2f,
            24.8f, 26.6f, 28.4f, 30.2f, 32f
        };

        int count = 0;

        for (int index = 0; index < baseTimes.Length; index++)
        {
            if (baseTimes[index] + shift < runDuration - 1f)
            {
                count++;
            }
        }

        float[] result = new float[count];

        for (int index = 0; index < count; index++)
        {
            result[index] = baseTimes[index] + shift;
        }

        return result;
    }
}
