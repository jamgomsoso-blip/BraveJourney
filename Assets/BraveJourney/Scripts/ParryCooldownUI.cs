using UnityEngine;

public class ParryCooldownUI : MonoBehaviour
{
    private PlayerController playerController;

    private void Awake()
    {
        playerController =
            GetComponent<PlayerController>();
    }

    private void OnGUI()
    {
        if (playerController == null)
        {
            return;
        }

        GUIStyle style =
            new GUIStyle(GUI.skin.label);

        style.fontSize = 24;
        style.fontStyle =
            FontStyle.Bold;

        style.normal.textColor =
            Color.white;

        string cooldownText;

        if (
            playerController
                .ParryCooldownRemaining <= 0f
        )
        {
            cooldownText = "PARRY : READY";
        }
        else
        {
            cooldownText =
                "PARRY : " +
                playerController
                    .ParryCooldownRemaining
                    .ToString("0.0");
        }

        GUI.Label(
            new Rect(
                20,
                60,
                300,
                50
            ),
            cooldownText,
            style
        );
    }
}