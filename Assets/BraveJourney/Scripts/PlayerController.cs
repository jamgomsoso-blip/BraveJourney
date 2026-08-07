using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float jumpForce = 7.5f;

    [Header("Boss Battle Movement")]
    [SerializeField] private float bossMoveSpeed = 6f;
    [SerializeField] private float bossScreenPadding = 0.3f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Slide")]
    [SerializeField] private float slideColliderHeightRatio = 0.5f;

    [Header("Parry")]
    [SerializeField] private float parryDuration = 0.2f;
    [SerializeField] private float parryCooldown = 1f;

    [Header("Parry Timing Assist")]
    [SerializeField] private float parryAssistLeadTime = 0.65f;
    [SerializeField] private float parryAssistActivationTime = 0.16f;

    [Header("Animation States")]
    [SerializeField] private string sprintStateName = "Sprint";
    [SerializeField] private string bossIdleStateName = "Idle";
    [SerializeField] private string bossRunStateName = "Run";
    [SerializeField] private string bossRunToIdleStateName = "RunToIdle";
    [SerializeField] private string bossRunToIdleClipName = "IdleTransition";
    [SerializeField] private string jumpStateName = "Jump";
    [SerializeField] private string doubleJumpStateName = "FrontFlip";
    [SerializeField] private string slideStateName = "Slide";
    [SerializeField] private string parryStateName = "PunchC";
    [SerializeField] private string kickStateName = "KickC";
    [SerializeField] private string hitDamageStateName = "HitDamage";
    [SerializeField] private string dieStateName = "Die";
    [SerializeField] private float animationCrossFadeTime = 0.03f;
    [SerializeField] private float fallbackActionDuration = 0.35f;

    private Rigidbody2D playerRigidbody;
    private Animator playerAnimator;
    private Animator doubleJumpEffectAnimator;
    private CapsuleCollider2D playerCollider;
    private Camera mainCamera;

    private Vector2 normalColliderSize;
    private Vector2 normalColliderOffset;

    private int jumpCount;
    private const int MaxJumpCount = 2;

    private bool isSliding;
    private bool isParrying;
    private bool isParryAnimating;
    private bool isBossBattle;
    private bool isKicking;
    private bool isHitReacting;
    private bool isGrounded;
    private bool isDead;
    private bool bossWasMoving;
    private bool isPlayingBossRunToIdle;
    private bool isParryQueued;

    private float parryTimer;
    private float parryAnimationTimer;
    private float parryCooldownTimer;
    private float normalGravityScale;
    private float kickTimer;
    private float hitReactionTimer;
    private float bossRunToIdleTimer;

    private bool hasIsGroundedParameter;
    private bool hasVerticalVelocityParameter;
    private bool hasIsSlidingParameter;
    private bool hasIsParryingParameter;
    private bool hasDoubleJumpParameter;

    private string currentAnimationStateName;

    private const int BaseLayerIndex = 0;
    private const string BaseLayerName = "Base Layer";

    public bool IsSliding => isSliding;
    public bool IsParrying => isParrying;
    public bool IsBossBattle => isBossBattle;
    public bool IsGrounded => isGrounded;
    public bool IsKicking => isKicking;
    public bool IsDead => isDead;
    public bool IsParryQueued => isParryQueued;

    public float ParryAssistLeadTime =>
        parryAssistLeadTime;

    public float ParryAssistActivationTime =>
        parryAssistActivationTime;

    public float ParryCooldownRemaining =>
        Mathf.Max(parryCooldownTimer, 0f);

    public float ParryCooldownDuration =>
        parryCooldown;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        mainCamera = Camera.main;

        normalGravityScale = playerRigidbody.gravityScale;

        CacheAnimatorParameters();

        Transform effectTransform =
            transform.Find("DoubleJumpEffect");

        if (effectTransform != null)
        {
            doubleJumpEffectAnimator =
                effectTransform.GetComponent<Animator>();
        }

        if (playerCollider != null)
        {
            normalColliderSize = playerCollider.size;
            normalColliderOffset = playerCollider.offset;
        }

        PlayAnimationState(sprintStateName, true);
    }

    private void Update()
    {
        isGrounded =
            groundCheck != null &&
            Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            ) != null;

        if (
            isGrounded &&
            playerRigidbody.linearVelocity.y <= 0.05f
        )
        {
            jumpCount = 0;
        }

        UpdateHitReaction();

        if (isHitReacting)
        {
            UpdateAnimator(isGrounded);
            return;
        }

        UpdateKick();
        UpdateParry(isGrounded);
        HandleSlide(isGrounded);
        HandleJump();
        UpdateAnimator(isGrounded);
    }

    private void UpdateParry(bool isGrounded)
    {
        if (parryCooldownTimer > 0f)
        {
            parryCooldownTimer -= Time.deltaTime;
        }

        bool canParry =
            isBossBattle || isGrounded;

        bool canPrepareParry =
            canParry &&
            !isSliding &&
            !isParrying &&
            !isParryAnimating &&
            !isKicking &&
            parryCooldownTimer <= 0f;

        UpdateQueuedParry(canPrepareParry);

        if (
            Input.GetKeyDown(KeyCode.Space) &&
            canPrepareParry &&
            !isParryQueued
        )
        {
            if (!TryQueueAssistedParry())
            {
                StartParry();
            }
        }

        if (isParrying)
        {
            parryTimer -= Time.deltaTime;

            if (parryTimer <= 0f)
            {
                EndParry();
            }
        }

        if (isParryAnimating)
        {
            parryAnimationTimer -= Time.deltaTime;

            if (parryAnimationTimer <= 0f)
            {
                isParryAnimating = false;
            }
        }
    }

    private void HandleSlide(bool isGrounded)
    {
        bool slideKeyPressed;

        if (isBossBattle)
        {
            slideKeyPressed =
                Input.GetKey(KeyCode.E) ||
                Input.GetKey(KeyCode.DownArrow);
        }
        else
        {
            slideKeyPressed =
                Input.GetKey(KeyCode.E);
        }

        bool wantsToSlide =
            slideKeyPressed &&
            isGrounded &&
            !isParrying &&
            !isParryAnimating &&
            !isKicking;

        if (wantsToSlide && !isSliding)
        {
            StartSlide();
        }
        else if (!wantsToSlide && isSliding)
        {
            EndSlide();
        }
    }

    private void HandleJump()
    {
        bool jumpKeyPressed;

        if (isBossBattle)
        {
            jumpKeyPressed =
                Input.GetKeyDown(KeyCode.W) ||
                Input.GetKeyDown(KeyCode.UpArrow);
        }
        else
        {
            jumpKeyPressed =
                Input.GetKeyDown(KeyCode.W);
        }

        if (
            !jumpKeyPressed ||
            jumpCount >= MaxJumpCount ||
            isParrying ||
            isParryAnimating ||
            isKicking
        )
        {
            return;
        }

        bool isDoubleJump = jumpCount == 1;

        if (isSliding)
        {
            EndSlide();
        }

        playerRigidbody.linearVelocity =
            new Vector2(
                playerRigidbody.linearVelocity.x,
                jumpForce
            );

        jumpCount++;
        GameAudioFeedback.Play(GameSoundCue.Jump);

        if (!isDoubleJump)
        {
            PlayAnimationState(jumpStateName, true);
            return;
        }

        SetAnimatorTriggerIfAvailable(
            "DoubleJump",
            hasDoubleJumpParameter
        );

        PlayAnimationState(doubleJumpStateName, true);

        if (doubleJumpEffectAnimator != null)
        {
            doubleJumpEffectAnimator.SetTrigger(
                "PlayEffect"
            );
        }
    }

    private bool TryQueueAssistedParry()
    {
        if (
            !isBossBattle ||
            !Projectile.TryGetIncomingParryTime(
                transform,
                out float secondsToContact
            ) ||
            secondsToContact > parryAssistLeadTime ||
            secondsToContact <= parryAssistActivationTime
        )
        {
            return false;
        }

        isParryQueued = true;
        return true;
    }

    private void UpdateQueuedParry(bool canPrepareParry)
    {
        if (!isParryQueued)
        {
            return;
        }

        if (
            !canPrepareParry ||
            !Projectile.TryGetIncomingParryTime(
                transform,
                out float secondsToContact
            )
        )
        {
            isParryQueued = false;
            return;
        }

        if (secondsToContact > parryAssistActivationTime)
        {
            return;
        }

        isParryQueued = false;
        StartParry();
    }

    public bool TryStartKick()
    {
        if (
            !isBossBattle ||
            isParrying ||
            isParryAnimating ||
            isKicking ||
            isHitReacting ||
            isDead ||
            !isActiveAndEnabled
        )
        {
            return false;
        }

        if (isSliding)
        {
            EndSlide();
        }

        isKicking = true;
        kickTimer =
            GetAnimationDuration(
                kickStateName,
                fallbackActionDuration
            );

        StopBossRunToIdle();
        PlayAnimationState(kickStateName, true);

        return true;
    }

    public void PlayHitDamage()
    {
        if (isDead || !isActiveAndEnabled)
        {
            return;
        }

        CancelCurrentAction();

        isHitReacting = true;
        hitReactionTimer =
            GetAnimationDuration(
                hitDamageStateName,
                fallbackActionDuration
            );

        PlayAnimationState(hitDamageStateName, true);
    }

    public void PlayDeath()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        CancelCurrentAction();
        PlayAnimationState(dieStateName, true);
    }

    private void UpdateKick()
    {
        if (!isKicking)
        {
            return;
        }

        kickTimer -= Time.deltaTime;

        if (kickTimer > 0f)
        {
            return;
        }

        isKicking = false;
    }

    private void UpdateHitReaction()
    {
        if (!isHitReacting)
        {
            return;
        }

        hitReactionTimer -= Time.deltaTime;

        if (hitReactionTimer > 0f)
        {
            return;
        }

        isHitReacting = false;
    }

    private void CancelCurrentAction()
    {
        isParryQueued = false;

        if (isParrying)
        {
            EndParry();
        }

        isParryAnimating = false;
        parryAnimationTimer = 0f;

        isKicking = false;
        kickTimer = 0f;
        isHitReacting = false;
        hitReactionTimer = 0f;

        if (isSliding)
        {
            EndSlide();
        }

        StopBossRunToIdle();
    }

    private void FixedUpdate()
    {
        if (isBossBattle)
        {
            HandleBossMovement();
            return;
        }

        playerRigidbody.linearVelocity =
            new Vector2(
                moveSpeed,
                playerRigidbody.linearVelocity.y
            );
    }

    private void HandleBossMovement()
    {
        float horizontalInput = ReadBossHorizontalInput();

        float horizontalVelocity =
            horizontalInput * bossMoveSpeed;

        if (mainCamera != null && mainCamera.orthographic)
        {
            float cameraHalfWidth =
                mainCamera.orthographicSize *
                mainCamera.aspect;

            float playerHalfWidth =
                playerCollider != null
                    ? playerCollider.bounds.extents.x
                    : 0.5f;

            float minimumX =
                mainCamera.transform.position.x -
                cameraHalfWidth +
                playerHalfWidth +
                bossScreenPadding;

            float maximumX =
                mainCamera.transform.position.x +
                cameraHalfWidth -
                playerHalfWidth -
                bossScreenPadding;

            float nextX =
                playerRigidbody.position.x +
                horizontalVelocity *
                Time.fixedDeltaTime;

            if (nextX < minimumX)
            {
                playerRigidbody.position =
                    new Vector2(
                        minimumX,
                        playerRigidbody.position.y
                    );

                horizontalVelocity = 0f;
            }
            else if (nextX > maximumX)
            {
                playerRigidbody.position =
                    new Vector2(
                        maximumX,
                        playerRigidbody.position.y
                    );

                horizontalVelocity = 0f;
            }
        }

        playerRigidbody.linearVelocity =
            new Vector2(
                horizontalVelocity,
                playerRigidbody.linearVelocity.y
            );
    }

    public void StartBossBattle()
    {
        if (isBossBattle)
        {
            return;
        }

        isBossBattle = true;
        GameAudioFeedback.SetBossBattle(true);

        if (isSliding)
        {
            EndSlide();
        }

        playerRigidbody.gravityScale =
            normalGravityScale;

        playerRigidbody.linearVelocity =
            Vector2.zero;

        bossWasMoving = false;
        StopBossRunToIdle();
        PlayAnimationState(bossIdleStateName, true);

        Debug.Log("BOSS BATTLE START");
    }

    private void StartSlide()
    {
        isSliding = true;

        if (playerCollider == null)
        {
            return;
        }

        float normalBottom =
            normalColliderOffset.y -
            normalColliderSize.y * 0.5f;

        Vector2 slideSize = normalColliderSize;

        slideSize.y =
            normalColliderSize.y *
            slideColliderHeightRatio;

        Vector2 slideOffset =
            normalColliderOffset;

        slideOffset.y =
            normalBottom +
            slideSize.y * 0.5f;

        playerCollider.size = slideSize;
        playerCollider.offset = slideOffset;
    }

    private void EndSlide()
    {
        isSliding = false;

        if (playerCollider == null)
        {
            return;
        }

        playerCollider.size = normalColliderSize;
        playerCollider.offset = normalColliderOffset;
    }

    private void StartParry()
    {
        isParryQueued = false;
        isParrying = true;
        isParryAnimating = true;
        parryTimer = parryDuration;
        parryAnimationTimer =
            GetAnimationDuration(
                parryStateName,
                fallbackActionDuration
            );
        parryCooldownTimer = parryCooldown;

        StopBossRunToIdle();
        PlayAnimationState(parryStateName, true);
    }

    private void EndParry()
    {
        isParrying = false;
    }

    private void UpdateAnimator(bool isGrounded)
    {
        if (playerAnimator == null)
        {
            return;
        }

        SetAnimatorBoolIfAvailable(
            "IsGrounded",
            hasIsGroundedParameter,
            isGrounded
        );

        SetAnimatorFloatIfAvailable(
            "VerticalVelocity",
            hasVerticalVelocityParameter,
            playerRigidbody.linearVelocity.y
        );

        SetAnimatorBoolIfAvailable(
            "IsSliding",
            hasIsSlidingParameter,
            isSliding
        );

        SetAnimatorBoolIfAvailable(
            "IsParrying",
            hasIsParryingParameter,
            isParrying
        );

        UpdateAnimationState(isGrounded);
    }

    private void UpdateAnimationState(bool isGrounded)
    {
        if (
            playerAnimator == null ||
            isParrying ||
            isParryAnimating ||
            isKicking ||
            isHitReacting ||
            isDead
        )
        {
            return;
        }

        bool isAnimationGrounded =
            isGrounded &&
            playerRigidbody.linearVelocity.y <= 0.05f;

        if (isBossBattle)
        {
            UpdateBossBattleAnimation(isAnimationGrounded);
            return;
        }

        StopBossRunToIdle();

        if (isSliding)
        {
            PlayAnimationState(slideStateName);
            return;
        }

        if (!isAnimationGrounded)
        {
            PlayAirborneAnimation();
            return;
        }

        PlayAnimationState(sprintStateName);
    }

    private void UpdateBossBattleAnimation(
        bool isAnimationGrounded
    )
    {
        if (isSliding)
        {
            StopBossRunToIdle();
            PlayAnimationState(slideStateName);
            bossWasMoving = false;
            return;
        }

        if (!isAnimationGrounded)
        {
            StopBossRunToIdle();
            PlayAirborneAnimation();
            bossWasMoving = false;
            return;
        }

        bool isMoving =
            Mathf.Abs(ReadBossHorizontalInput()) > 0.01f;

        if (isMoving)
        {
            StopBossRunToIdle();
            PlayAnimationState(bossRunStateName);
            bossWasMoving = true;
            return;
        }

        if (bossWasMoving && !isPlayingBossRunToIdle)
        {
            StartBossRunToIdle();
        }

        bossWasMoving = false;

        if (isPlayingBossRunToIdle)
        {
            bossRunToIdleTimer -= Time.deltaTime;

            if (bossRunToIdleTimer > 0f)
            {
                return;
            }

            isPlayingBossRunToIdle = false;
        }

        PlayAnimationState(bossIdleStateName);
    }

    private void PlayAirborneAnimation()
    {
        if (jumpCount >= MaxJumpCount)
        {
            PlayAnimationState(doubleJumpStateName);
            return;
        }

        PlayAnimationState(jumpStateName);
    }

    private void StartBossRunToIdle()
    {
        bossRunToIdleTimer =
            GetAnimationDuration(
                bossRunToIdleClipName,
                fallbackActionDuration
            );

        isPlayingBossRunToIdle =
            PlayAnimationState(
                bossRunToIdleStateName,
                true
            );

        if (!isPlayingBossRunToIdle)
        {
            bossRunToIdleTimer = 0f;
        }
    }

    private void StopBossRunToIdle()
    {
        isPlayingBossRunToIdle = false;
        bossRunToIdleTimer = 0f;
    }

    private float ReadBossHorizontalInput()
    {
        float horizontalInput = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalInput -= 1f;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            horizontalInput += 1f;
        }

        return horizontalInput;
    }

    private bool PlayAnimationState(
        string stateName,
        bool restart = false
    )
    {
        if (
            playerAnimator == null ||
            string.IsNullOrEmpty(stateName)
        )
        {
            return false;
        }

        string resolvedStateName =
            ResolveAnimationStateName(stateName);

        if (string.IsNullOrEmpty(resolvedStateName))
        {
            return false;
        }

        if (
            !restart &&
            currentAnimationStateName == stateName
        )
        {
            return true;
        }

        if (animationCrossFadeTime > 0f)
        {
            playerAnimator.CrossFadeInFixedTime(
                resolvedStateName,
                animationCrossFadeTime,
                BaseLayerIndex,
                0f
            );
        }
        else
        {
            playerAnimator.Play(
                resolvedStateName,
                BaseLayerIndex,
                0f
            );
        }

        currentAnimationStateName = stateName;
        return true;
    }

    private string ResolveAnimationStateName(
        string stateName
    )
    {
        int shortNameHash =
            Animator.StringToHash(stateName);

        if (
            playerAnimator.HasState(
                BaseLayerIndex,
                shortNameHash
            )
        )
        {
            return stateName;
        }

        string fullStateName =
            BaseLayerName + "." + stateName;

        int fullPathHash =
            Animator.StringToHash(fullStateName);

        if (
            playerAnimator.HasState(
                BaseLayerIndex,
                fullPathHash
            )
        )
        {
            return fullStateName;
        }

        return null;
    }

    private float GetAnimationDuration(
        string clipName,
        float fallbackDuration
    )
    {
        if (
            playerAnimator == null ||
            playerAnimator.runtimeAnimatorController == null
        )
        {
            return fallbackDuration;
        }

        AnimationClip[] clips =
            playerAnimator
                .runtimeAnimatorController
                .animationClips;

        foreach (AnimationClip clip in clips)
        {
            if (clip.name != clipName)
            {
                continue;
            }

            return Mathf.Max(clip.length, 0.05f);
        }

        return fallbackDuration;
    }

    private void CacheAnimatorParameters()
    {
        if (playerAnimator == null)
        {
            return;
        }

        foreach (
            AnimatorControllerParameter parameter in
            playerAnimator.parameters
        )
        {
            if (parameter.name == "IsGrounded")
            {
                hasIsGroundedParameter = true;
            }
            else if (parameter.name == "VerticalVelocity")
            {
                hasVerticalVelocityParameter = true;
            }
            else if (parameter.name == "IsSliding")
            {
                hasIsSlidingParameter = true;
            }
            else if (parameter.name == "IsParrying")
            {
                hasIsParryingParameter = true;
            }
            else if (parameter.name == "DoubleJump")
            {
                hasDoubleJumpParameter = true;
            }
        }
    }

    private void SetAnimatorBoolIfAvailable(
        string parameterName,
        bool hasParameter,
        bool value
    )
    {
        if (!hasParameter)
        {
            return;
        }

        playerAnimator.SetBool(parameterName, value);
    }

    private void SetAnimatorFloatIfAvailable(
        string parameterName,
        bool hasParameter,
        float value
    )
    {
        if (!hasParameter)
        {
            return;
        }

        playerAnimator.SetFloat(parameterName, value);
    }

    private void SetAnimatorTriggerIfAvailable(
        string parameterName,
        bool hasParameter
    )
    {
        if (!hasParameter)
        {
            return;
        }

        playerAnimator.SetTrigger(parameterName);
    }

    private void OnDisable()
    {
        if (playerCollider != null)
        {
            playerCollider.size = normalColliderSize;
            playerCollider.offset = normalColliderOffset;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.gravityScale =
                normalGravityScale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}
