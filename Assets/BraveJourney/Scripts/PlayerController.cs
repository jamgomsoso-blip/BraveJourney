using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 6.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D playerRigidbody;

    // 0: 점프하지 않음
    // 1: 첫 번째 점프 사용
    // 2: 두 번째 점프까지 사용
    private int jumpCount;

    private const int MaxJumpCount = 2;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // GroundCheck 주변에 Ground 레이어가 있는지 검사한다.
        bool isGrounded =
            groundCheck != null &&
            Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            ) != null;

        // 바닥에 착지했으면 점프 횟수를 초기화한다.
        if (isGrounded && playerRigidbody.linearVelocity.y <= 0.05f)
        {
            jumpCount = 0;
        }

        // W를 누르면 점프한다.
        // 최대 두 번까지 가능하다.
        if (Input.GetKeyDown(KeyCode.W) && jumpCount < MaxJumpCount)
        {
            playerRigidbody.linearVelocity = new Vector2(
                playerRigidbody.linearVelocity.x,
                jumpForce
            );

            jumpCount++;
        }
    }

    private void FixedUpdate()
    {
        // 캐릭터는 항상 오른쪽으로 달린다.
        // 현재 위아래 속도는 그대로 유지한다.
        playerRigidbody.linearVelocity = new Vector2(
            moveSpeed,
            playerRigidbody.linearVelocity.y
        );
    }

    private void OnDrawGizmosSelected()
    {
        // Scene 화면에서 GroundCheck 범위를 확인하기 위한 원이다.
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