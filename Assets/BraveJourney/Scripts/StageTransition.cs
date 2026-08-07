using UnityEngine;
using UnityEngine.SceneManagement;

public class StageTransition : MonoBehaviour
{
    [SerializeField] private float transitionDelay = 2f;
    [SerializeField] private Font uiFont;

    private BossHealth bossHealth;
    private bool isTransitioning;
    private bool isEnding;
    private float transitionTimer;

    public bool IsEnding => isEnding;

    public static StageTransition EnsureForScene(
        GameObject owner
    )
    {
        StageTransition existing =
            FindFirstObjectByType<StageTransition>();

        if (existing != null || owner == null)
        {
            return existing;
        }

        GameObject transitionObject =
            new GameObject("StageTransition");

        return transitionObject.AddComponent<StageTransition>();
    }

    private void Update()
    {
        if (isEnding)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartFromBeginning();
            }

            return;
        }

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

            if (uiFont == null)
            {
                uiFont = bossHealth.UiFont;
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
            return;
        }

        BeginEnding();
    }

    private void BeginEnding()
    {
        isTransitioning = false;
        isEnding = true;

        ClearRemainingThreats();
        Time.timeScale = 0f;

        GameAudioFeedback.Play(
            GameSoundCue.GameClear
        );

        Debug.Log(
            "GAME CLEAR - 퇴사 성공 / R 키로 다시 시작"
        );
    }

    private static void ClearRemainingThreats()
    {
        Projectile[] projectiles =
            FindObjectsByType<Projectile>(
                FindObjectsSortMode.None
            );

        foreach (Projectile projectile in projectiles)
        {
            Destroy(projectile.gameObject);
        }

        StageHazard[] hazards =
            FindObjectsByType<StageHazard>(
                FindObjectsSortMode.None
            );

        foreach (StageHazard hazard in hazards)
        {
            hazard.CancelHazard();
        }
    }

    private static void RestartFromBeginning()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    private void OnGUI()
    {
        if (!isEnding)
        {
            return;
        }

        GUI.depth = -200;

        DrawPanel(
            new Rect(0f, 0f, Screen.width, Screen.height),
            new Color(0.025f, 0.035f, 0.06f, 0.96f)
        );

        float centerY = Screen.height * 0.5f;

        Rect quotePanel = new Rect(
            Screen.width * 0.5f - 310f,
            centerY - 190f,
            620f,
            64f
        );

        DrawPanel(
            quotePanel,
            new Color(1f, 1f, 1f, 0.1f)
        );

        GUIStyle quoteStyle = CreateStyle(
            25,
            TextAnchor.MiddleCenter,
            new Color(0.45f, 0.95f, 0.9f, 1f)
        );

        GUI.Label(
            quotePanel,
            "노무사에 신고하겠습니다!",
            quoteStyle
        );

        GUIStyle titleStyle = CreateStyle(
            54,
            TextAnchor.MiddleCenter,
            Color.white
        );

        GUI.Label(
            new Rect(
                0f,
                centerY - 100f,
                Screen.width,
                72f
            ),
            "퇴사 성공",
            titleStyle
        );

        GUIStyle bodyStyle = CreateStyle(
            25,
            TextAnchor.MiddleCenter,
            new Color(0.92f, 0.94f, 1f, 1f)
        );

        GUI.Label(
            new Rect(
                0f,
                centerY - 18f,
                Screen.width,
                50f
            ),
            "드디어 회사를 탈출했습니다",
            bodyStyle
        );

        Rect restartPanel = new Rect(
            Screen.width * 0.5f - 205f,
            centerY + 76f,
            410f,
            58f
        );

        DrawPanel(
            restartPanel,
            new Color(0.12f, 0.75f, 0.68f, 0.22f)
        );

        bodyStyle.fontSize = 22;

        GUI.Label(
            restartPanel,
            "R - 다시 입사하기",
            bodyStyle
        );
    }

    private GUIStyle CreateStyle(
        int fontSize,
        TextAnchor alignment,
        Color textColor
    )
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);

        style.font = uiFont != null
            ? uiFont
            : GUI.skin.font;

        style.fontSize = fontSize;
        style.fontStyle = FontStyle.Bold;
        style.alignment = alignment;
        style.normal.textColor = textColor;

        return style;
    }

    private static void DrawPanel(Rect rect, Color color)
    {
        Color previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previousColor;
    }
}
