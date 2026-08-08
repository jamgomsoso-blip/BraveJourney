using UnityEngine;

public class ParryCooldownUI : MonoBehaviour
{
    private const float ControlHintDuration = 7f;

    [SerializeField] private Font uiFont;

    private PlayerController playerController;
    private PlayerHealth playerHealth;
    private bool showFirstStageHints;
    private bool hasStartedBossHint;
    private float runnerHintRemaining;
    private float bossHintRemaining;

    private void Awake()
    {
        playerController =
            GetComponent<PlayerController>();
        playerHealth =
            GetComponent<PlayerHealth>();

        StageProfile profile =
            StageProfileCatalog.GetCurrentOrDefault();

        showFirstStageHints =
            profile != null &&
            profile.StageNumber ==
                StageProfileCatalog.FirstStageNumber;
        runnerHintRemaining = showFirstStageHints
            ? ControlHintDuration
            : 0f;
    }

    private void Update()
    {
        if (
            !showFirstStageHints ||
            playerController == null ||
            (playerHealth != null && playerHealth.IsGameOver) ||
            Time.timeScale <= 0f
        )
        {
            return;
        }

        if (!playerController.IsBossBattle)
        {
            runnerHintRemaining = Mathf.Max(
                runnerHintRemaining - Time.deltaTime,
                0f
            );
            return;
        }

        if (!hasStartedBossHint)
        {
            hasStartedBossHint = true;
            bossHintRemaining = ControlHintDuration;
        }

        bossHintRemaining = Mathf.Max(
            bossHintRemaining - Time.deltaTime,
            0f
        );
    }

    private void OnGUI()
    {
        if (
            playerController == null ||
            (playerHealth != null && playerHealth.IsGameOver)
        )
        {
            return;
        }

        GUI.depth = -20;
        float scale = OfficeHudTheme.Scale;
        bool isReady =
            playerController.ParryCooldownRemaining <= 0f;
        float cooldownRatio =
            playerController.ParryCooldownDuration > 0f
                ? 1f -
                    playerController.ParryCooldownRemaining /
                    playerController.ParryCooldownDuration
                : 1f;

        GUIStyle style =
            OfficeHudTheme.CreateTextStyle(
                uiFont,
                Mathf.RoundToInt(16f * scale),
                TextAnchor.MiddleLeft,
                OfficeHudTheme.Ink
            );
        GUIStyle keyStyle =
            OfficeHudTheme.CreateTextStyle(
                uiFont,
                Mathf.RoundToInt(12f * scale),
                TextAnchor.MiddleCenter,
                Color.white
            );

        string cooldownText =
            isReady
                ? "패링 준비"
                : "재충전 " +
                    playerController
                        .ParryCooldownRemaining
                        .ToString("0.0") +
                    "초";

        Rect panelRect = new Rect(
            18f * scale,
            88f * scale,
            214f * scale,
            56f * scale
        );

        OfficeHudTheme.DrawPanel(
            panelRect,
            isReady
                ? OfficeHudTheme.Cyan
                : OfficeHudTheme.Gold
        );

        Rect keyRect = new Rect(
            panelRect.x + 11f * scale,
            panelRect.y + 18f * scale,
            54f * scale,
            24f * scale
        );

        OfficeHudTheme.DrawRect(keyRect, OfficeHudTheme.Ink);

        GUI.Label(keyRect, "SPACE", keyStyle);

        GUI.Label(
            new Rect(
                panelRect.x + 75f * scale,
                panelRect.y + 14f * scale,
                panelRect.width - 86f * scale,
                26f * scale
            ),
            cooldownText,
            style
        );

        OfficeHudTheme.DrawProgressBar(
            new Rect(
                panelRect.x + 75f * scale,
                panelRect.y + 39f * scale,
                panelRect.width - 87f * scale,
                8f * scale
            ),
            cooldownRatio,
            isReady
                ? OfficeHudTheme.Cyan
                : OfficeHudTheme.Gold
        );

        DrawControlHint(scale);
    }

    private void DrawControlHint(float scale)
    {
        if (!showFirstStageHints)
        {
            return;
        }

        bool showBossHint =
            playerController.IsBossBattle &&
            bossHintRemaining > 0f;
        bool showRunnerHint =
            !playerController.IsBossBattle &&
            runnerHintRemaining > 0f;

        if (!showRunnerHint && !showBossHint)
        {
            return;
        }

        GUI.depth = -15;

        float panelWidth = showBossHint ? 500f : 560f;
        Rect panelRect = new Rect(
            Screen.width * 0.5f - panelWidth * scale * 0.5f,
            Screen.height - 66f * scale,
            panelWidth * scale,
            48f * scale
        );

        OfficeHudTheme.DrawRect(
            new Rect(
                panelRect.x + 4f * scale,
                panelRect.y + 4f * scale,
                panelRect.width,
                panelRect.height
            ),
            new Color(0.02f, 0.04f, 0.06f, 0.24f)
        );
        OfficeHudTheme.DrawRect(
            panelRect,
            new Color(0.06f, 0.09f, 0.13f, 0.82f)
        );
        OfficeHudTheme.DrawRect(
            new Rect(
                panelRect.x,
                panelRect.y,
                panelRect.width,
                3f * scale
            ),
            showBossHint
                ? OfficeHudTheme.Gold
                : OfficeHudTheme.Cyan
        );

        GUIStyle keyStyle =
            OfficeHudTheme.CreateTextStyle(
                uiFont,
                Mathf.RoundToInt(13f * scale),
                TextAnchor.MiddleCenter,
                Color.white
            );
        GUIStyle hintStyle =
            OfficeHudTheme.CreateTextStyle(
                uiFont,
                Mathf.RoundToInt(14f * scale),
                TextAnchor.MiddleLeft,
                Color.white,
                FontStyle.Normal
            );

        if (showBossHint)
        {
            DrawHintItem(
                panelRect,
                14f,
                82f,
                "SPACE",
                "패링",
                keyStyle,
                hintStyle,
                scale
            );
            DrawHintItem(
                panelRect,
                250f,
                44f,
                "A",
                "발차기 (스턴 중)",
                keyStyle,
                hintStyle,
                scale
            );
            return;
        }

        DrawHintItem(
            panelRect,
            14f,
            44f,
            "W",
            "점프 · 두 번 누르면 더블 점프",
            keyStyle,
            hintStyle,
            scale
        );
        DrawHintItem(
            panelRect,
            366f,
            44f,
            "E",
            "슬라이드",
            keyStyle,
            hintStyle,
            scale
        );
    }

    private static void DrawHintItem(
        Rect panelRect,
        float offsetX,
        float keyWidth,
        string keyLabel,
        string description,
        GUIStyle keyStyle,
        GUIStyle hintStyle,
        float scale
    )
    {
        Rect keyRect = new Rect(
            panelRect.x + offsetX * scale,
            panelRect.y + 11f * scale,
            keyWidth * scale,
            27f * scale
        );

        OfficeHudTheme.DrawRect(
            keyRect,
            new Color(0.16f, 0.22f, 0.28f, 0.96f)
        );
        GUI.Label(keyRect, keyLabel, keyStyle);

        GUI.Label(
            new Rect(
                keyRect.xMax + 9f * scale,
                panelRect.y + 8f * scale,
                panelRect.xMax - keyRect.xMax - 16f * scale,
                33f * scale
            ),
            description,
            hintStyle
        );
    }
}
