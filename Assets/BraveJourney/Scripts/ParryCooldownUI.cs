using UnityEngine;

public class ParryCooldownUI : MonoBehaviour
{
    [SerializeField] private Font uiFont;

    private PlayerController playerController;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerController =
            GetComponent<PlayerController>();
        playerHealth =
            GetComponent<PlayerHealth>();
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
    }
}
