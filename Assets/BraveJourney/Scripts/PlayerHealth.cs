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

        GameAudioFeedback.Play(
            GameSoundCue.PlayerHit
        );

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
        float scale = OfficeHudTheme.Scale;
        float heartSize = 32f * scale;
        float panelWidth =
            76f * scale + maxHealth * heartSize;

        Rect lifePanel = new Rect(
            18f * scale,
            18f * scale,
            panelWidth,
            62f * scale
        );

        OfficeHudTheme.DrawPanel(
            lifePanel,
            OfficeHudTheme.Red
        );

        GUIStyle lifeStyle =
            OfficeHudTheme.CreateTextStyle(
                uiFont,
                Mathf.RoundToInt(15f * scale),
                TextAnchor.MiddleCenter,
                OfficeHudTheme.Ink
            );

        GUI.Label(
            new Rect(
                lifePanel.x + 10f * scale,
                lifePanel.y + 15f * scale,
                54f * scale,
                36f * scale
            ),
            "LIFE",
            lifeStyle
        );

        for (int index = 0; index < maxHealth; index++)
        {
            OfficeHudTheme.DrawHeart(
                new Rect(
                    lifePanel.x +
                    (65f + index * 31f) * scale,
                    lifePanel.y + 17f * scale,
                    heartSize,
                    heartSize
                ),
                index < currentHealth
            );
        }

        if (!isGameOver)
        {
            return;
        }

        OfficeHudTheme.DrawRect(
            new Rect(
                0f,
                0f,
                Screen.width,
                Screen.height
            ),
            new Color(0f, 0f, 0f, 0.78f)
        );

        Rect gameOverPanel = new Rect(
            Screen.width * 0.5f - 230f * scale,
            Screen.height * 0.5f - 105f * scale,
            460f * scale,
            210f * scale
        );

        OfficeHudTheme.DrawPanel(
            gameOverPanel,
            OfficeHudTheme.Red,
            false
        );

        GUIStyle gameOverStyle =
            OfficeHudTheme.CreateTextStyle(
                uiFont,
                Mathf.RoundToInt(46f * scale),
                TextAnchor.MiddleCenter,
                OfficeHudTheme.Red
            );

        GUI.Label(
            new Rect(
                gameOverPanel.x + 20f * scale,
                gameOverPanel.y + 35f * scale,
                gameOverPanel.width - 40f * scale,
                70f * scale
            ),
            "퇴사 실패",
            gameOverStyle
        );

        gameOverStyle.fontSize =
            Mathf.RoundToInt(21f * scale);
        gameOverStyle.normal.textColor =
            OfficeHudTheme.Ink;

        GUI.Label(
            new Rect(
                gameOverPanel.x + 20f * scale,
                gameOverPanel.y + 120f * scale,
                gameOverPanel.width - 40f * scale,
                48f * scale
            ),
            "R - 다시 출근하기",
            gameOverStyle
        );
    }
}
