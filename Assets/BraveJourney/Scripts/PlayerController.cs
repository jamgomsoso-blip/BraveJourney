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
    [SerializeField] private float parryDuration = 0.1f;
    [SerializeField] private float parryCooldown = 1f;

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
    private bool isBossBattle;

    private float parryTimer;
    private float parryCooldownTimer;
    private float normalGravityScale;

    public bool IsSliding => isSliding;
    public bool IsParrying => isParrying;
    public bool IsBossBattle => isBossBattle;

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
    }

    private void Update()
    {
        bool isGrounded =
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

        if (
            Input.GetKeyDown(KeyCode.Space) &&
            canParry &&
            !isSliding &&
            !isParrying &&
            parryCooldownTimer <= 0f
        )
        {
            StartParry();
        }

        if (!isParrying)
        {
            return;
        }

        parryTimer -= Time.deltaTime;

        if (parryTimer <= 0f)
        {
            EndParry();
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
            !isParrying;

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
            isParrying
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

        if (!isDoubleJump)
        {
            return;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("DoubleJump");
        }

        if (doubleJumpEffectAnimator != null)
        {
            doubleJumpEffectAnimator.SetTrigger(
                "PlayEffect"
            );
        }
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
        float horizontalInput = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalInput -= 1f;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            horizontalInput += 1f;
        }

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

        if (isSliding)
        {
            EndSlide();
        }

        playerRigidbody.gravityScale =
            normalGravityScale;

        playerRigidbody.linearVelocity =
            Vector2.zero;

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
        isParrying = true;
        parryTimer = parryDuration;
        parryCooldownTimer = parryCooldown;
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

        playerAnimator.SetBool(
            "IsGrounded",
            isGrounded
        );

        playerAnimator.SetFloat(
            "VerticalVelocity",
            playerRigidbody.linearVelocity.y
        );

        playerAnimator.SetBool(
            "IsSliding",
            isSliding
        );

        playerAnimator.SetBool(
            "IsParrying",
            isParrying
        );
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