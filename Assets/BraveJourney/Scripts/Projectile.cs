using UnityEngine;
using UnityEngine.UI;

public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float reflectedSpeedMultiplier = 2f;
    [SerializeField] private float lifeTime = 8f;

    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Dialogue")]
    [SerializeField] private string hostileDialogue = "";
    [SerializeField] private string reflectedDialogue = "퇴사하렵니다!";
    [SerializeField] private Color hostileColor = Color.red;
    [SerializeField] private Color reflectedColor = Color.cyan;
    [SerializeField] private Font dialogueFont;
    [SerializeField] private int dialogueFontSize = 64;
    [SerializeField] private float dialogueCharacterSize = 0.3f;
    [SerializeField] private Vector3 dialogueLocalPosition =
        new Vector3(0f, 2.2f, 0f);

    private Transform target;
    private Vector3 moveDirection;

    private bool isReflected;
    private bool hasResolved;

    private SpriteRenderer spriteRenderer;
    private TextMesh dialogueTextMesh;
    private Text dialogueText;

    private void Awake()
    {
        spriteRenderer =
            GetComponent<SpriteRenderer>();

        dialogueTextMesh =
            GetComponentInChildren<TextMesh>(true);

        dialogueText =
            GetComponentInChildren<Text>(true);

        EnsureDialogueText();
    }

    private void Start()
    {
        ApplyProjectileVisual(false);
    }

    public void Initialize(Transform playerTarget)
    {
        Initialize(playerTarget, hostileDialogue);
    }

    public void Initialize(
        Transform playerTarget,
        string dialogue
    )
    {
        target = playerTarget;

        if (!string.IsNullOrWhiteSpace(dialogue))
        {
            hostileDialogue = dialogue;
        }

        UpdateDirectionToTarget();
        ApplyProjectileVisual(false);
    }

    private void Update()
    {
        if (!isReflected && target != null)
        {
            // 반사되기 전에는 플레이어를 계속 따라간다.
            UpdateDirectionToTarget();
        }

        transform.localPosition +=
            moveDirection *
            speed *
            Time.deltaTime;

        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateDirectionToTarget()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetLocalPosition;

        if (transform.parent != null)
        {
            targetLocalPosition =
                transform.parent.InverseTransformPoint(
                    target.position
                );
        }
        else
        {
            targetLocalPosition =
                target.position;
        }

        moveDirection =
            (
                targetLocalPosition -
                transform.localPosition
            ).normalized;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasResolved)
        {
            return;
        }

        if (!isReflected)
        {
            ParryHitbox parryHitbox =
                other.GetComponent<ParryHitbox>();

            if (
                parryHitbox != null &&
                parryHitbox.CanReflect
            )
            {
                Reflect();
                return;
            }

            PlayerHealth playerHealth =
                other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                hasResolved = true;

                playerHealth.TakeDamage(damage);

                Destroy(gameObject);
            }

            return;
        }

        BossHealth bossHealth =
            other.GetComponentInParent<BossHealth>();

        if (bossHealth != null)
        {
            hasResolved = true;

            bossHealth.TakeDamage(damage);

            Destroy(gameObject);
        }
    }

    private void Reflect()
    {
        isReflected = true;
        target = null;

        speed *= reflectedSpeedMultiplier;

        ApplyProjectileVisual(true);

        BossHealth bossHealth =
            FindFirstObjectByType<BossHealth>();

        if (bossHealth != null)
        {
            Vector3 bossLocalPosition;

            if (transform.parent != null)
            {
                bossLocalPosition =
                    transform.parent.InverseTransformPoint(
                        bossHealth.transform.position
                    );
            }
            else
            {
                bossLocalPosition =
                    bossHealth.transform.position;
            }

            moveDirection =
                (
                    bossLocalPosition -
                    transform.localPosition
                ).normalized;
        }
        else
        {
            moveDirection = Vector3.right;
        }
    }

    private void ApplyProjectileVisual(bool reflected)
    {
        Color projectileColor =
            reflected ? reflectedColor : hostileColor;

        string dialogue =
            reflected ? reflectedDialogue : hostileDialogue;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = projectileColor;
        }

        if (dialogueTextMesh != null)
        {
            if (!string.IsNullOrEmpty(dialogue))
            {
                dialogueTextMesh.text = dialogue;
            }

            dialogueTextMesh.color = projectileColor;
        }

        if (dialogueText != null)
        {
            if (!string.IsNullOrEmpty(dialogue))
            {
                dialogueText.text = dialogue;
            }

            dialogueText.color = projectileColor;
        }
    }

    private void EnsureDialogueText()
    {
        if (dialogueTextMesh != null)
        {
            ConfigureDialogueText(dialogueTextMesh);
            return;
        }

        GameObject dialogueObject =
            new GameObject("DialogueText");

        dialogueObject.transform.SetParent(
            transform,
            false
        );

        dialogueObject.transform.localPosition =
            dialogueLocalPosition;

        float inverseScale =
            Mathf.Abs(transform.localScale.x) > 0.001f
                ? 1f / Mathf.Abs(transform.localScale.x)
                : 1f;

        dialogueObject.transform.localScale =
            new Vector3(
                inverseScale,
                inverseScale,
                1f
            );

        dialogueTextMesh =
            dialogueObject.AddComponent<TextMesh>();

        ConfigureDialogueText(dialogueTextMesh);
    }

    private void ConfigureDialogueText(TextMesh textMesh)
    {
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = dialogueFontSize;
        textMesh.characterSize = dialogueCharacterSize;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.richText = false;

        if (dialogueFont != null)
        {
            textMesh.font = dialogueFont;
        }

        MeshRenderer textRenderer =
            textMesh.GetComponent<MeshRenderer>();

        if (textRenderer == null)
        {
            return;
        }

        if (dialogueFont != null)
        {
            textRenderer.sharedMaterial =
                dialogueFont.material;
        }

        if (spriteRenderer != null)
        {
            textRenderer.sortingLayerID =
                spriteRenderer.sortingLayerID;

            textRenderer.sortingOrder =
                spriteRenderer.sortingOrder + 1;
        }
    }
}
