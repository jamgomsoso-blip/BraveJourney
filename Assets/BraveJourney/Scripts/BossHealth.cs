using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    private int currentHealth;
    private bool isDefeated;

    private BossShooter bossShooter;
    private SpriteRenderer spriteRenderer;
    private Collider2D bossCollider;

    public bool IsDefeated => isDefeated;

    private void Awake()
    {
        currentHealth = maxHealth;

        bossShooter =
            GetComponent<BossShooter>();

        spriteRenderer =
            GetComponent<SpriteRenderer>();

        bossCollider =
            GetComponent<Collider2D>();
    }

    public void TakeDamage(int damage)
    {
        if (
            isDefeated ||
            damage <= 0
        )
        {
            return;
        }

        currentHealth -= damage;
        currentHealth =
            Mathf.Max(currentHealth, 0);

        Debug.Log(
            "보스 체력: " +
            currentHealth +
            " / " +
            maxHealth
        );

        if (currentHealth <= 0)
        {
            DefeatBoss();
        }
    }

    private void DefeatBoss()
    {
        isDefeated = true;

        if (bossShooter != null)
        {
            bossShooter.enabled = false;
        }

        if (bossCollider != null)
        {
            bossCollider.enabled = false;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
        }

        Projectile[] projectiles =
            FindObjectsOfType<Projectile>();

        foreach (Projectile projectile in projectiles)
        {
            Destroy(projectile.gameObject);
        }

        PlayerController playerController =
            FindObjectOfType<PlayerController>();

        if (playerController != null)
        {
            playerController.enabled = false;

            Rigidbody2D playerRigidbody =
                playerController.GetComponent<Rigidbody2D>();

            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity =
                    Vector2.zero;
            }
        }

        Debug.Log("STAGE CLEAR");
    }

    private void OnGUI()
    {
        GUIStyle bossStyle =
            new GUIStyle(GUI.skin.label);

        bossStyle.fontSize = 28;
        bossStyle.fontStyle =
            FontStyle.Bold;

        bossStyle.alignment =
            TextAnchor.UpperCenter;

        bossStyle.normal.textColor =
            Color.white;

        if (!isDefeated)
        {
            GUI.Label(
                new Rect(
                    Screen.width / 2 - 150,
                    20,
                    300,
                    50
                ),
                "BOSS : " + currentHealth,
                bossStyle
            );

            return;
        }

        bossStyle.fontSize = 48;

        bossStyle.alignment =
            TextAnchor.MiddleCenter;

        GUI.Label(
            new Rect(
                0,
                Screen.height / 2 - 50,
                Screen.width,
                100
            ),
            "STAGE CLEAR",
            bossStyle
        );
    }
}