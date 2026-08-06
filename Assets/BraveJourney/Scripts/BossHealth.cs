using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    [Header("Stun")]
    [SerializeField] private float stunDuration = 3f;

    [Header("Presentation")]
    [SerializeField] private string bossName = "주임";
    [SerializeField] private Font uiFont;
    [SerializeField] private RuntimeAnimatorController placeholderController;
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string stunnedStateName = "HitDamage";
    [SerializeField] private string defeatedStateName = "Die";

    private int currentHealth;
    private bool isDefeated;
    private bool isStunned;
    private bool kickUsedThisStun;

    private Coroutine stunCoroutine;
    private BossShooter bossShooter;
    private BossHazardController bossHazardController;
    private Animator bossAnimator;
    private SpriteRenderer spriteRenderer;
    private Collider2D bossCollider;
    private Color normalColor = Color.white;

    public bool IsDefeated => isDefeated;
    public bool IsStunned => isStunned;
    public string BossName => bossName;

    private void Awake()
    {
        currentHealth = maxHealth;

        bossShooter = GetComponent<BossShooter>();
        bossHazardController =
            GetComponent<BossHazardController>();

        bossAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossCollider = GetComponent<Collider2D>();

        if (
            bossAnimator == null &&
            placeholderController != null
        )
        {
            bossAnimator = gameObject.AddComponent<Animator>();
            bossAnimator.runtimeAnimatorController =
                placeholderController;
        }

        if (spriteRenderer != null)
        {
            normalColor = spriteRenderer.color;
        }

        PlayBossAnimation(idleStateName);
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

        // 발차기는 스턴 상태에서만 피해를 준다.
        if (!isStunned)
        {
            return false;
        }

        // 한 번의 스턴에서 발차기 피해는 한 번만 허용한다.
        if (kickUsedThisStun)
        {
            Debug.Log(
                "공격 실패: 이번 스턴에서 이미 공격했습니다."
            );

            return false;
        }

        kickUsedThisStun = true;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(
            "발차기 명중 - 보스 체력: " +
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
        kickUsedThisStun = false;

        SetBossAttackEnabled(false);
        ClearActiveProjectiles();
        PlayBossAnimation(stunnedStateName);

        stunCoroutine = StartCoroutine(
            StunRoutine()
        );
    }

    private IEnumerator StunRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow;
        }

        Debug.Log(
            "BOSS STUN - 발차기 공격 가능"
        );

        yield return new WaitForSeconds(
            stunDuration
        );

        if (isDefeated)
        {
            yield break;
        }

        isStunned = false;
        kickUsedThisStun = false;

        SetBossAttackEnabled(true);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        PlayBossAnimation(idleStateName);

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

        SetBossAttackEnabled(false);

        if (bossCollider != null)
        {
            bossCollider.enabled = false;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
        }

        PlayBossAnimation(defeatedStateName);

        Projectile[] projectiles =
            FindObjectsByType<Projectile>(
                FindObjectsSortMode.None
            );

        foreach (Projectile projectile in projectiles)
        {
            Destroy(projectile.gameObject);
        }

        PlayerController playerController =
            FindFirstObjectByType<PlayerController>();

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

    private void SetBossAttackEnabled(bool isEnabled)
    {
        if (bossShooter != null)
        {
            bossShooter.enabled = isEnabled;
        }

        if (bossHazardController != null)
        {
            bossHazardController.enabled = isEnabled;
        }
    }

    private void ClearActiveProjectiles()
    {
        Projectile[] projectiles =
            FindObjectsByType<Projectile>(
                FindObjectsSortMode.None
            );

        foreach (Projectile projectile in projectiles)
        {
            projectile.gameObject.SetActive(false);
            Destroy(projectile.gameObject);
        }
    }

    private void OnGUI()
    {
        GUIStyle bossStyle = CreateStyle(
            24,
            TextAnchor.MiddleCenter
        );

        Rect panelRect = new Rect(
            Screen.width * 0.5f - 190f,
            18f,
            380f,
            72f
        );

        DrawPanel(panelRect, new Color(0f, 0f, 0f, 0.72f));

        string bossState =
            isStunned ? "  [기절]" : string.Empty;

        GUI.Label(
            new Rect(
                panelRect.x,
                panelRect.y + 2f,
                panelRect.width,
                32f
            ),
            bossName + bossState,
            bossStyle
        );

        Rect healthBackground = new Rect(
            panelRect.x + 24f,
            panelRect.y + 43f,
            panelRect.width - 48f,
            14f
        );

        DrawPanel(
            healthBackground,
            new Color(0.18f, 0.18f, 0.18f, 1f)
        );

        float healthRatio =
            maxHealth > 0
                ? (float)currentHealth / maxHealth
                : 0f;

        DrawPanel(
            new Rect(
                healthBackground.x,
                healthBackground.y,
                healthBackground.width * healthRatio,
                healthBackground.height
            ),
            isStunned
                ? new Color(1f, 0.85f, 0.2f, 1f)
                : new Color(0.9f, 0.18f, 0.18f, 1f)
        );

        if (!isDefeated)
        {
            return;
        }

        bossStyle.fontSize = 44;

        GUI.Label(
            new Rect(
                0f,
                Screen.height * 0.5f - 70f,
                Screen.width,
                70f
            ),
            "보스 처치!",
            bossStyle
        );

        bossStyle.fontSize = 22;

        GUI.Label(
            new Rect(
                0f,
                Screen.height * 0.5f,
                Screen.width,
                44f
            ),
            "다음 스테이지로 이동 중...",
            bossStyle
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

    private void PlayBossAnimation(string stateName)
    {
        if (
            bossAnimator == null ||
            string.IsNullOrWhiteSpace(stateName)
        )
        {
            return;
        }

        string fullStateName =
            "Base Layer." + stateName;

        int fullStateHash =
            Animator.StringToHash(fullStateName);

        if (bossAnimator.HasState(0, fullStateHash))
        {
            bossAnimator.Play(fullStateName, 0, 0f);
            return;
        }

        int shortStateHash =
            Animator.StringToHash(stateName);

        if (bossAnimator.HasState(0, shortStateHash))
        {
            bossAnimator.Play(stateName, 0, 0f);
        }
    }
}
