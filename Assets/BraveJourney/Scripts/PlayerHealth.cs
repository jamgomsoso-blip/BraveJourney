using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibleDuration = 1f;
    [SerializeField] private Font uiFont;

    private int currentHealth;
    private bool isInvincible;
    private float invincibleTimer;
    private bool isGameOver;

    private PlayerController playerController;
    private Rigidbody2D playerRigidbody;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        currentHealth = maxHealth;

        playerController =
            GetComponent<PlayerController>();

        playerRigidbody =
            GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;

            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
            }
        }

        if (
            isGameOver &&
            Input.GetKeyDown(KeyCode.R)
        )
        {
            RestartGame();
        }
    }

    public void TakeDamage(int damage)
    {
        if (
            isInvincible ||
            isGameOver ||
            damage <= 0
        )
        {
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(
            "플레이어 체력: " +
            currentHealth +
            " / " +
            maxHealth
        );

        if (currentHealth <= 0)
        {
            GameOver();
            return;
        }

        isInvincible = true;
        invincibleTimer = invincibleDuration;

        if (playerController != null)
        {
            playerController.PlayHitDamage();
        }
    }

    private void GameOver()
    {
        isGameOver = true;

        Debug.Log(
            "GAME OVER - R키를 눌러 재시작"
        );

        if (playerController != null)
        {
            playerController.PlayDeath();
            playerController.enabled = false;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity =
                Vector2.zero;
        }
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    private void OnGUI()
    {
        GUI.depth = isGameOver ? -100 : 0;

        GUIStyle lifeStyle = CreateStyle(
            24,
            TextAnchor.MiddleLeft
        );

        Rect lifePanel = new Rect(18f, 18f, 190f, 48f);

        DrawPanel(
            lifePanel,
            new Color(0f, 0f, 0f, 0.72f)
        );

        GUI.Label(
            new Rect(
                lifePanel.x + 16f,
                lifePanel.y,
                lifePanel.width - 24f,
                lifePanel.height
            ),
            "LIFE  " + currentHealth + " / " + maxHealth,
            lifeStyle
        );

        if (!isGameOver)
        {
            return;
        }

        DrawPanel(
            new Rect(
                0f,
                0f,
                Screen.width,
                Screen.height
            ),
            new Color(0f, 0f, 0f, 0.78f)
        );

        GUIStyle gameOverStyle = CreateStyle(
            52,
            TextAnchor.MiddleCenter
        );

        GUI.Label(
            new Rect(
                0,
                Screen.height / 2 - 70,
                Screen.width,
                70
            ),
            "퇴사 실패",
            gameOverStyle
        );

        gameOverStyle.fontSize = 22;

        GUI.Label(
            new Rect(
                0,
                Screen.height / 2,
                Screen.width,
                50
            ),
            "R - 다시 출근하기",
            gameOverStyle
        );
    }

    private GUIStyle CreateStyle(
        int fontSize,
        TextAnchor alignment
    )
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);

        style.font = uiFont != null
            ? uiFont
            : GUI.skin.font;

        style.fontSize = fontSize;
        style.fontStyle = FontStyle.Bold;
        style.alignment = alignment;
        style.normal.textColor = Color.white;

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
