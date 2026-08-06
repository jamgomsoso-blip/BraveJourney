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

        playerHealth = GetComponent<PlayerHealth>();
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

        GUIStyle style =
            new GUIStyle(GUI.skin.label);

        style.font = uiFont != null
            ? uiFont
            : GUI.skin.font;

        style.fontSize = 20;
        style.fontStyle =
            FontStyle.Bold;

        style.alignment = TextAnchor.MiddleLeft;

        style.normal.textColor =
            Color.white;

        string cooldownText;

        if (
            playerController
                .ParryCooldownRemaining <= 0f
        )
        {
            cooldownText = "패링  준비 완료";
        }
        else
        {
            cooldownText =
                "패링  " +
                playerController
                    .ParryCooldownRemaining
                    .ToString("0.0") +
                "초";
        }

        Rect panelRect = new Rect(
            18f,
            72f,
            190f,
            42f
        );

        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.62f);
        GUI.DrawTexture(panelRect, Texture2D.whiteTexture);
        GUI.color = previousColor;

        GUI.Label(
            new Rect(
                panelRect.x + 16f,
                panelRect.y,
                panelRect.width - 24f,
                panelRect.height
            ),
            cooldownText,
            style
        );
    }
}
