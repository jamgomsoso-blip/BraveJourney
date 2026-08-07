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
    [SerializeField] private bool faceLeft = true;
    [SerializeField] private Font uiFont;
    [SerializeField] private RuntimeAnimatorController placeholderController;
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string attackStateName = "PunchC";
    [SerializeField] private string stunnedStateName = "HitDamage";
    [SerializeField] private string defeatedStateName = "Die";

    private int currentHealth;
    private bool isDefeated;
    private bool isStunned;
    private bool kickUsedThisStun;

    private Coroutine stunCoroutine;
    private BossShooter bossShooter;
    private BossHazardController bossHazardController;
    private BossVisualAnimator bossVisualAnimator;
    private Animator bossAnimator;
    private SpriteRenderer spriteRenderer;
    private Collider2D bossCollider;
    private Color normalColor = Color.white;
    private int stageNumber = 1;

    public bool IsDefeated => isDefeated;
    public bool IsStunned => isStunned;
    public string BossName => bossName;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public Font UiFont => uiFont;

    private void Awake()
    {
        if (StageProfileCatalog.TryGetCurrent(out StageProfile profile))
        {
            stageNumber = profile.StageNumber;
            maxHealth = profile.BossHealth;
            bossName = profile.BossName;
        }

        currentHealth = maxHealth;

        bossShooter = GetComponent<BossShooter>();
        bossHazardController =
            GetComponent<BossHazardController>();

        bossAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossCollider = GetComponent<Collider2D>();
        bossVisualAnimator =
            BossVisualAnimator.EnsureOn(gameObject);

        if (bossVisualAnimator != null)
        {
            bossVisualAnimator.ConfigureForBoss(bossName);
        }

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
            spriteRenderer.flipX = faceLeft;
            normalColor = spriteRenderer.color;
        }

        if (bossVisualAnimator != null)
        {
            bossVisualAnimator.SetFacingLeft(faceLeft);
            bossVisualAnimator.SetTint(normalColor);
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

        if (bossVisualAnimator != null)
        {
            bossVisualAnimator.SetTint(Color.yellow);
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

        if (bossVisualAnimator != null)
        {
            bossVisualAnimator.SetTint(normalColor);
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

        if (bossVisualAnimator != null)
        {
            bossVisualAnimator.SetTint(Color.gray);
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

        StageHazard[] hazards =
            FindObjectsByType<StageHazard>(
                FindObjectsSortMode.None
            );

        foreach (StageHazard hazard in hazards)
        {
            hazard.CancelHazard();
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

        GameAudioFeedback.Play(
            GameSoundCue.StageClear
        );

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

    public void PlayAttackAnimation()
    {
        if (isStunned || isDefeated)
        {
            return;
        }

        if (bossVisualAnimator != null)
        {
            bossVisualAnimator.PlayAttack();
        }

        PlayLegacyBossAnimation(attackStateName);
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
        GUI.depth = isDefeated ? -90 : -10;
        float scale = OfficeHudTheme.Scale;
        GUIStyle bossStyle =
            OfficeHudTheme.CreateTextStyle(
                uiFont,
                Mathf.RoundToInt(22f * scale),
                TextAnchor.MiddleCenter,
                OfficeHudTheme.Ink
            );

        Rect panelRect = new Rect(
            Screen.width * 0.5f - 230f * scale,
            18f * scale,
            460f * scale,
            88f * scale
        );

        OfficeHudTheme.DrawPanel(
            panelRect,
            isStunned
                ? OfficeHudTheme.Gold
                : OfficeHudTheme.Red
        );

        string bossState =
            isStunned ? "  ·  기절" : string.Empty;

        GUI.Label(
            new Rect(
                panelRect.x + 24f * scale,
                panelRect.y + 12f * scale,
                panelRect.width - 48f * scale,
                34f * scale
            ),
            "BOSS  ·  " + bossName + bossState,
            bossStyle
        );

        Rect healthBackground = new Rect(
            panelRect.x + 26f * scale,
            panelRect.y + 52f * scale,
            panelRect.width - 52f * scale,
            22f * scale
        );

        float healthRatio =
            maxHealth > 0
                ? (float)currentHealth / maxHealth
                : 0f;

        OfficeHudTheme.DrawProgressBar(
            healthBackground,
            healthRatio,
            isStunned
                ? OfficeHudTheme.Gold
                : OfficeHudTheme.Red,
            maxHealth
        );

        if (!isDefeated)
        {
            return;
        }

        OfficeHudTheme.DrawRect(
            new Rect(0f, 0f, Screen.width, Screen.height),
            new Color(0.02f, 0.04f, 0.06f, 0.55f)
        );

        Rect clearPanel = new Rect(
            Screen.width * 0.5f - 260f * scale,
            Screen.height * 0.5f - 105f * scale,
            520f * scale,
            210f * scale
        );

        OfficeHudTheme.DrawPanel(
            clearPanel,
            OfficeHudTheme.Cyan,
            false
        );

        bossStyle.fontSize =
            Mathf.RoundToInt(42f * scale);
        bossStyle.normal.textColor =
            OfficeHudTheme.Cyan;

        GUI.Label(
            new Rect(
                clearPanel.x + 20f * scale,
                clearPanel.y + 34f * scale,
                clearPanel.width - 40f * scale,
                72f * scale
            ),
            bossName + " 처치!",
            bossStyle
        );

        bossStyle.fontSize =
            Mathf.RoundToInt(20f * scale);
        bossStyle.normal.textColor =
            OfficeHudTheme.Ink;

        GUI.Label(
            new Rect(
                clearPanel.x + 20f * scale,
                clearPanel.y + 122f * scale,
                clearPanel.width - 40f * scale,
                45f * scale
            ),
            stageNumber == StageProfileCatalog.LastStageNumber
                ? "퇴사 절차 마무리 중..."
                : "퇴사를 위해.... 다음으로 이동 중....",
            bossStyle
        );
    }

    private void PlayBossAnimation(string stateName)
    {
        if (bossVisualAnimator != null)
        {
            if (stateName == stunnedStateName)
            {
                bossVisualAnimator.PlayStunned();
            }
            else if (stateName == defeatedStateName)
            {
                bossVisualAnimator.PlayDefeated();
            }
            else
            {
                bossVisualAnimator.PlayIdle();
            }
        }

        PlayLegacyBossAnimation(stateName);
    }

    private void PlayLegacyBossAnimation(string stateName)
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
