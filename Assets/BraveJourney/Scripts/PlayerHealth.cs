using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibleDuration = 1f;

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
    }

    private void GameOver()
    {
        isGameOver = true;

        Debug.Log(
            "GAME OVER - R키를 눌러 재시작"
        );

        if (playerController != null)
        {
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
        GUIStyle lifeStyle = new GUIStyle(
            GUI.skin.label
        );

        lifeStyle.fontSize = 28;
        lifeStyle.fontStyle = FontStyle.Bold;
        lifeStyle.normal.textColor = Color.white;

        GUI.Label(
            new Rect(20, 20, 300, 50),
            "LIFE : " + currentHealth,
            lifeStyle
        );

        if (!isGameOver)
        {
            return;
        }

        GUIStyle gameOverStyle = new GUIStyle(
            GUI.skin.label
        );

        gameOverStyle.fontSize = 48;
        gameOverStyle.fontStyle = FontStyle.Bold;
        gameOverStyle.alignment =
            TextAnchor.MiddleCenter;

        gameOverStyle.normal.textColor =
            Color.white;

        GUI.Label(
            new Rect(
                0,
                Screen.height / 2 - 70,
                Screen.width,
                70
            ),
            "GAME OVER",
            gameOverStyle
        );

        gameOverStyle.fontSize = 24;

        GUI.Label(
            new Rect(
                0,
                Screen.height / 2,
                Screen.width,
                50
            ),
            "R : RESTART",
            gameOverStyle
        );
    }
}