using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float jumpForce = 7.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Slide")]
    [SerializeField] private float slideColliderHeightRatio = 0.5f;

    [Header("Parry")]
    [SerializeField] private float parryDuration = 0.3f;
    [SerializeField] private float parryCooldown = 1f;

    private Rigidbody2D playerRigidbody;
    private Animator playerAnimator;
    private Animator doubleJumpEffectAnimator;
    private CapsuleCollider2D playerCollider;

    private Vector2 normalColliderSize;
    private Vector2 normalColliderOffset;

    private int jumpCount;
    private const int MaxJumpCount = 2;

    private bool isSliding;
    public bool IsSliding => isSliding;

    private bool isParrying;
    private float parryTimer;
    private float parryCooldownTimer;
    public bool IsParrying => isParrying;
    public float ParryCooldownRemaining =>
    Mathf.Max(parryCooldownTimer, 0f);
    public float ParryCooldownDuration =>
    parryCooldown;
    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        playerCollider = GetComponent<CapsuleCollider2D>();

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

        // 착지하면 점프 횟수를 초기화한다.
        if (isGrounded && playerRigidbody.linearVelocity.y <= 0.05f)
        {
            jumpCount = 0;
        }

        // 패링 쿨타임을 줄인다.
        if (parryCooldownTimer > 0f)
        {
            parryCooldownTimer -= Time.deltaTime;
        }

        // Space를 누르면 패링을 시작한다.
        if (
            Input.GetKeyDown(KeyCode.Space) &&
            isGrounded &&
            !isSliding &&
            !isParrying &&
            parryCooldownTimer <= 0f
        )
        {
            StartParry();
        }

        // 패링 지속시간을 계산한다.
        if (isParrying)
        {
            parryTimer -= Time.deltaTime;

            if (parryTimer <= 0f)
            {
                EndParry();
            }
        }

        // E를 누르고 있는 동안 슬라이딩을 유지한다.
        bool wantsToSlide =
            Input.GetKey(KeyCode.E) &&
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

        // W를 누르면 최대 두 번까지 점프한다.
        if (
            Input.GetKeyDown(KeyCode.W) &&
            jumpCount < MaxJumpCount &&
            !isSliding &&
            !isParrying
        )
        {
            bool isDoubleJump = jumpCount == 1;

            playerRigidbody.linearVelocity = new Vector2(
                playerRigidbody.linearVelocity.x,
                jumpForce
            );

            jumpCount++;

            if (isDoubleJump)
            {
                if (playerAnimator != null)
                {
                    playerAnimator.SetTrigger("DoubleJump");
                }

                if (doubleJumpEffectAnimator != null)
                {
                    doubleJumpEffectAnimator.SetTrigger("PlayEffect");
                }
            }
        }

        // 현재 상태를 Animator에 전달한다.
        if (playerAnimator != null)
        {
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
    }

    private void FixedUpdate()
    {
        playerRigidbody.linearVelocity = new Vector2(
            moveSpeed,
            playerRigidbody.linearVelocity.y
        );
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

        Vector2 slideOffset = normalColliderOffset;
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

    private void OnDisable()
    {
        if (playerCollider != null)
        {
            playerCollider.size = normalColliderSize;
            playerCollider.offset = normalColliderOffset;
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