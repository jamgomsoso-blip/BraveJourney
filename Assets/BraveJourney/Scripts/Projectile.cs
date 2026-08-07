using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Projectile : MonoBehaviour
{
    private const float ProjectileScaleMultiplier = 1.9f;

    [Header("Movement")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float reflectedSpeedMultiplier = 2f;
    [SerializeField] private float lifeTime = 8f;

    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Parry Timing Assist")]
    [SerializeField] private float parryContactDistance = 0.6f;

    [Header("Dialogue")]
    [SerializeField] private string hostileDialogue = "";
    [SerializeField] private string reflectedDialogue = "퇴사하렵니다!";
    [SerializeField] private Color hostileColor = Color.red;
    [SerializeField] private Color reflectedColor = Color.cyan;
    [SerializeField] private Color hostileTextColor =
        new Color(0.68f, 0.04f, 0.04f, 1f);
    [SerializeField] private Color reflectedTextColor =
        new Color(0f, 0.42f, 0.42f, 1f);
    [SerializeField] private Font dialogueFont;
    [SerializeField] private int dialogueFontSize = 64;
    [SerializeField] private float dialogueCharacterSize = 0.06f;
    [SerializeField] private float dialogueMinimumCharacterSize = 0.04f;
    [SerializeField] private Vector3 dialogueLocalPosition =
        new Vector3(0f, 1.35f, 0f);

    [Header("Dialogue Bubble")]
    [SerializeField] private Color dialogueBackgroundColor =
        new Color(1f, 0.98f, 0.9f, 0.96f);
    [SerializeField] private Color bubbleOuterColor =
        new Color(0.08f, 0.09f, 0.12f, 1f);
    [SerializeField] private float bubbleMinimumWidth = 1.8f;
    [SerializeField] private float bubbleMaximumWidth = 4.6f;
    [SerializeField] private float bubbleWidthPerCharacter = 0.14f;
    [SerializeField] private float bubbleHeight = 0.52f;
    [SerializeField] private float bubbleHorizontalPadding = 0.48f;
    [SerializeField] private float bubbleVerticalPadding = 0.16f;
    [SerializeField] private float bubbleBorderSize = 0.1f;
    [SerializeField] private float bubbleOuterBorderSize = 0.18f;
    [SerializeField] private float bubbleTailSize = 0.24f;

    private Transform target;
    private Vector3 moveDirection;

    private bool isReflected;
    private bool hasResolved;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer bubbleOuterRenderer;
    private SpriteRenderer bubbleBorderRenderer;
    private SpriteRenderer bubbleFillRenderer;
    private SpriteRenderer bubbleTailOuterRenderer;
    private SpriteRenderer bubbleTailBorderRenderer;
    private SpriteRenderer bubbleTailFillRenderer;
    private TextMesh dialogueTextMesh;
    private Text dialogueText;

    private static Sprite solidColorSprite;
    private static readonly List<Projectile> ActiveProjectiles =
        new List<Projectile>();

    public bool IsReflected => isReflected;
    public Font DialogueFont => dialogueFont;
    public Color HostileTextColor => hostileTextColor;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetActiveProjectiles()
    {
        ActiveProjectiles.Clear();
    }

    private void OnEnable()
    {
        if (!ActiveProjectiles.Contains(this))
        {
            ActiveProjectiles.Add(this);
        }
    }

    private void OnDisable()
    {
        ActiveProjectiles.Remove(this);
    }

    private void Awake()
    {
        spriteRenderer =
            GetComponent<SpriteRenderer>();

        dialogueTextMesh =
            GetComponentInChildren<TextMesh>(true);

        dialogueText =
            GetComponentInChildren<Text>(true);

        if (dialogueTextMesh != null)
        {
            dialogueTextMesh.gameObject.SetActive(false);
        }

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(false);
        }

        transform.localScale *=
            ProjectileScaleMultiplier;
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
        Initialize(
            playerTarget,
            dialogue,
            reflectedDialogue
        );
    }

    public void Initialize(
        Transform playerTarget,
        string dialogue,
        string reflectedResponse
    )
    {
        target = playerTarget;

        if (!string.IsNullOrWhiteSpace(dialogue))
        {
            hostileDialogue = dialogue;
        }

        if (!string.IsNullOrWhiteSpace(reflectedResponse))
        {
            reflectedDialogue = reflectedResponse;
        }

        UpdateDirectionToTarget();
        ApplyProjectileVisual(false);
    }

    public static bool TryGetIncomingParryTime(
        Transform playerRoot,
        out float secondsToContact
    )
    {
        secondsToContact = float.PositiveInfinity;

        if (playerRoot == null)
        {
            return false;
        }

        bool foundProjectile = false;

        for (
            int index = ActiveProjectiles.Count - 1;
            index >= 0;
            index--
        )
        {
            Projectile projectile = ActiveProjectiles[index];

            if (projectile == null)
            {
                ActiveProjectiles.RemoveAt(index);
                continue;
            }

            if (
                projectile.isReflected ||
                projectile.hasResolved ||
                projectile.target == null ||
                (
                    projectile.target != playerRoot &&
                    !projectile.target.IsChildOf(playerRoot)
                )
            )
            {
                continue;
            }

            float travelDistance = Mathf.Max(
                Vector3.Distance(
                    projectile.transform.position,
                    projectile.target.position
                ) - projectile.parryContactDistance,
                0f
            );

            float contactTime =
                travelDistance /
                Mathf.Max(projectile.speed, 0.01f);

            if (contactTime < secondsToContact)
            {
                secondsToContact = contactTime;
                foundProjectile = true;
            }
        }

        return foundProjectile;
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
        Transform playerSpeaker = target;

        if (playerSpeaker != null)
        {
            PlayerController playerController =
                playerSpeaker
                    .GetComponentInParent<PlayerController>();

            if (playerController != null)
            {
                playerSpeaker =
                    playerController.transform;
            }
        }

        isReflected = true;
        target = null;

        GameAudioFeedback.Play(
            GameSoundCue.ParrySuccess
        );

        speed *= reflectedSpeedMultiplier;

        ApplyProjectileVisual(true);

        CombatSpeechBubble.Show(
            playerSpeaker,
            reflectedDialogue,
            dialogueFont,
            reflectedTextColor,
            1.65f
        );

        BossHealth bossHealth =
            FindFirstObjectByType<BossHealth>();

        if (bossHealth != null)
        {
            CombatSpeechBubble.Hide(
                bossHealth.transform
            );

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

        if (spriteRenderer != null)
        {
            Sprite officeProjectile =
                reflected
                    ? OfficeSpriteCatalog
                        .ReflectedProjectile
                    : OfficeSpriteCatalog
                        .HostileProjectile;

            if (officeProjectile != null)
            {
                spriteRenderer.sprite =
                    officeProjectile;
                spriteRenderer.color = Color.white;
            }
            else
            {
                spriteRenderer.color =
                    projectileColor;
            }
        }

    }

    private void EnsureDialogueText()
    {
        if (dialogueTextMesh != null)
        {
            ConfigureDialogueText(dialogueTextMesh);
            EnsureDialogueBubble(dialogueTextMesh.transform);
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
        EnsureDialogueBubble(dialogueObject.transform);
    }

    private void ConfigureDialogueText(TextMesh textMesh)
    {
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = dialogueFontSize;
        textMesh.characterSize = dialogueCharacterSize;
        textMesh.fontStyle = FontStyle.Normal;
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
                spriteRenderer.sortingOrder + 4;
        }
    }

    private void EnsureDialogueBubble(
        Transform presentationTransform
    )
    {
        bubbleOuterRenderer = CreateBubbleRenderer(
            presentationTransform,
            "DialogueBubbleOuter",
            1
        );

        bubbleBorderRenderer = CreateBubbleRenderer(
            presentationTransform,
            "DialogueBubbleBorder",
            2
        );

        bubbleFillRenderer = CreateBubbleRenderer(
            presentationTransform,
            "DialogueBubbleFill",
            3
        );

        bubbleTailOuterRenderer = CreateBubbleRenderer(
            presentationTransform,
            "DialogueBubbleTailOuter",
            1
        );

        bubbleTailBorderRenderer = CreateBubbleRenderer(
            presentationTransform,
            "DialogueBubbleTailBorder",
            2
        );

        bubbleTailFillRenderer = CreateBubbleRenderer(
            presentationTransform,
            "DialogueBubbleTailFill",
            3
        );
    }

    private SpriteRenderer CreateBubbleRenderer(
        Transform parent,
        string objectName,
        int sortingOffset
    )
    {
        Transform existing = parent.Find(objectName);
        GameObject bubbleObject;

        if (existing != null)
        {
            bubbleObject = existing.gameObject;
        }
        else
        {
            bubbleObject = new GameObject(objectName);
            bubbleObject.transform.SetParent(parent, false);
        }

        SpriteRenderer renderer =
            bubbleObject.GetComponent<SpriteRenderer>();

        if (renderer == null)
        {
            renderer =
                bubbleObject.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = GetSolidColorSprite();

        if (spriteRenderer != null)
        {
            renderer.sortingLayerID =
                spriteRenderer.sortingLayerID;

            renderer.sortingOrder =
                spriteRenderer.sortingOrder + sortingOffset;
        }

        return renderer;
    }

    private void UpdateDialogueBubble(
        string dialogue,
        Color borderColor
    )
    {
        if (
            bubbleOuterRenderer == null ||
            bubbleBorderRenderer == null ||
            bubbleFillRenderer == null ||
            bubbleTailOuterRenderer == null ||
            bubbleTailBorderRenderer == null ||
            bubbleTailFillRenderer == null
        )
        {
            return;
        }

        Vector2 textSize = FitDialogueText(dialogue);
        float fallbackWidth =
            bubbleMinimumWidth +
            Mathf.Max(dialogue?.Length ?? 0, 1) *
            bubbleWidthPerCharacter;

        float requiredWidth =
            textSize.x > 0f
                ? textSize.x + bubbleHorizontalPadding
                : fallbackWidth;

        float requiredHeight =
            textSize.y > 0f
                ? textSize.y + bubbleVerticalPadding
                : bubbleHeight;

        float bubbleWidth = Mathf.Clamp(
            requiredWidth,
            bubbleMinimumWidth,
            bubbleMaximumWidth
        );
        float resolvedHeight = Mathf.Max(
            bubbleHeight,
            requiredHeight
        );

        bubbleFillRenderer.transform.localScale =
            new Vector3(
                bubbleWidth,
                resolvedHeight,
                1f
            );

        bubbleBorderRenderer.transform.localScale =
            new Vector3(
                bubbleWidth + bubbleBorderSize,
                resolvedHeight + bubbleBorderSize,
                1f
            );

        bubbleOuterRenderer.transform.localScale =
            new Vector3(
                bubbleWidth + bubbleOuterBorderSize,
                resolvedHeight + bubbleOuterBorderSize,
                1f
            );

        float tailY =
            -(resolvedHeight * 0.5f + bubbleTailSize * 0.15f);

        ConfigureBubbleTail(
            bubbleTailOuterRenderer,
            tailY,
            bubbleTailSize
        );
        ConfigureBubbleTail(
            bubbleTailBorderRenderer,
            tailY,
            bubbleTailSize * 0.76f
        );
        ConfigureBubbleTail(
            bubbleTailFillRenderer,
            tailY,
            bubbleTailSize * 0.52f
        );

        bubbleFillRenderer.color = dialogueBackgroundColor;
        bubbleBorderRenderer.color = borderColor;
        bubbleOuterRenderer.color = bubbleOuterColor;
        bubbleTailFillRenderer.color = dialogueBackgroundColor;
        bubbleTailBorderRenderer.color = borderColor;
        bubbleTailOuterRenderer.color = bubbleOuterColor;
    }

    private Vector2 FitDialogueText(string dialogue)
    {
        if (dialogueTextMesh == null)
        {
            return Vector2.zero;
        }

        dialogueTextMesh.characterSize = dialogueCharacterSize;
        Vector2 textSize = MeasureDialogueText();
        float maximumTextWidth = Mathf.Max(
            bubbleMaximumWidth - bubbleHorizontalPadding,
            0.1f
        );

        if (textSize.x > maximumTextWidth)
        {
            float fittedCharacterSize = Mathf.Max(
                dialogueMinimumCharacterSize,
                dialogueCharacterSize *
                maximumTextWidth /
                textSize.x
            );

            dialogueTextMesh.characterSize = fittedCharacterSize;
            textSize = MeasureDialogueText();
        }

        return textSize;
    }

    private Vector2 MeasureDialogueText()
    {
        MeshRenderer textRenderer =
            dialogueTextMesh.GetComponent<MeshRenderer>();

        if (
            textRenderer == null ||
            !textRenderer.enabled
        )
        {
            return Vector2.zero;
        }

        Bounds worldBounds = textRenderer.bounds;
        Vector3 lossyScale =
            dialogueTextMesh.transform.lossyScale;

        return new Vector2(
            worldBounds.size.x /
            Mathf.Max(Mathf.Abs(lossyScale.x), 0.001f),
            worldBounds.size.y /
            Mathf.Max(Mathf.Abs(lossyScale.y), 0.001f)
        );
    }

    private static void ConfigureBubbleTail(
        SpriteRenderer tailRenderer,
        float localY,
        float size
    )
    {
        tailRenderer.transform.localPosition =
            new Vector3(0f, localY, 0f);
        tailRenderer.transform.localRotation =
            Quaternion.Euler(0f, 0f, 45f);
        tailRenderer.transform.localScale =
            new Vector3(size, size, 1f);
    }

    private static Sprite GetSolidColorSprite()
    {
        if (solidColorSprite != null)
        {
            return solidColorSprite;
        }

        Texture2D texture = Texture2D.whiteTexture;

        solidColorSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            texture.width
        );

        solidColorSprite.name = "RuntimeDialogueBubbleSprite";
        solidColorSprite.hideFlags = HideFlags.HideAndDontSave;

        return solidColorSprite;
    }
}
