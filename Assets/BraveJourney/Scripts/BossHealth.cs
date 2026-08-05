using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    [Header("Stun")]
    [SerializeField] private float stunDuration = 3f;

    private int currentHealth;
    private bool isDefeated;
    private bool isStunned;
    private bool punchUsedThisStun;

    private Coroutine stunCoroutine;
    private BossShooter bossShooter;
    private SpriteRenderer spriteRenderer;
    private Collider2D bossCollider;
    private Color normalColor = Color.white;

    public bool IsDefeated => isDefeated;
    public bool IsStunned => isStunned;

    private void Awake()
    {
        currentHealth = maxHealth;

        bossShooter = GetComponent<BossShooter>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossCollider = GetComponent<Collider2D>();

        if (spriteRenderer != null)
        {
            normalColor = spriteRenderer.color;
        }
    }

    public bool TakeDamage(
        int damage,
        bool causeStun = true
    )
    {
        if (isDefeated || damage <= 0)
        {
            return false;
        }

        // 반사 투사체는 피해 없이 스턴만 발생시킨다.
        if (causeStun)
        {
            // 이미 스턴 중이면 새로운 스턴을 중복 적용하지 않는다.
            if (isStunned)
            {
                return false;
            }

            StartStun();
            return true;
        }

        // 주먹은 스턴 상태에서만 피해를 준다.
        if (!isStunned)
        {
            return false;
        }

        // 한 번의 스턴에서 주먹 피해는 한 번만 허용한다.
        if (punchUsedThisStun)
        {
            Debug.Log(
                "공격 실패: 이번 스턴에서 이미 공격했습니다."
            );

            return false;
        }

        punchUsedThisStun = true;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(
            "주먹 명중 - 보스 체력: " +
            currentHealth +
            " / " +
            maxHealth
        );

        if (currentHealth <= 0)
        {
            DefeatBoss();
        }

        return true;
    }

    private void StartStun()
    {
        isStunned = true;
        punchUsedThisStun = false;

        stunCoroutine = StartCoroutine(
            StunRoutine()
        );
    }

    private IEnumerator StunRoutine()
    {
        if (bossShooter != null)
        {
            bossShooter.enabled = false;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow;
        }

        Debug.Log(
            "BOSS STUN - 주먹 공격 가능"
        );

        yield return new WaitForSeconds(
            stunDuration
        );

        if (isDefeated)
        {
            yield break;
        }

        isStunned = false;
        punchUsedThisStun = false;

        if (bossShooter != null)
        {
            bossShooter.enabled = true;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        stunCoroutine = null;

        Debug.Log("BOSS STUN END");
    }

    private void DefeatBoss()
    {
        isDefeated = true;
        isStunned = false;

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

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
        bossStyle.fontStyle = FontStyle.Bold;
        bossStyle.alignment =
            TextAnchor.UpperCenter;
        bossStyle.normal.textColor =
            Color.white;

        if (!isDefeated)
        {
            string bossState =
                isStunned ? "  [STUN]" : "";

            GUI.Label(
                new Rect(
                    Screen.width / 2 - 150,
                    20,
                    300,
                    50
                ),
                "BOSS : " +
                currentHealth +
                bossState,
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