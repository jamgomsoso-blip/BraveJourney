using UnityEngine;

public sealed class CombatSpeechBubble : MonoBehaviour
{
    private const int BaseSortingOrder = 110;
    private const int BurstTextureWidth = 256;
    private const int BurstTextureHeight = 144;

    private static Sprite burstSprite;

    private Transform bubbleRoot;
    private SpriteRenderer outerRenderer;
    private SpriteRenderer borderRenderer;
    private SpriteRenderer fillRenderer;
    private TextMesh textMesh;

    private float remainingTime;
    private float worldHeight;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetRuntimeSprite()
    {
        burstSprite = null;
    }

    public static void Show(
        Transform speaker,
        string message,
        Font font,
        Color accentColor,
        float height,
        float duration = 1.45f
    )
    {
        if (
            speaker == null ||
            string.IsNullOrWhiteSpace(message)
        )
        {
            return;
        }

        CombatSpeechBubble bubble =
            speaker.GetComponent<CombatSpeechBubble>();

        if (bubble == null)
        {
            bubble =
                speaker.gameObject
                    .AddComponent<CombatSpeechBubble>();
        }

        bubble.ShowMessage(
            message,
            font,
            accentColor,
            height,
            duration
        );
    }

    public static void Hide(Transform speaker)
    {
        if (speaker == null)
        {
            return;
        }

        CombatSpeechBubble bubble =
            speaker.GetComponent<CombatSpeechBubble>();

        if (
            bubble != null &&
            bubble.bubbleRoot != null
        )
        {
            bubble.remainingTime = 0f;
            bubble.bubbleRoot.gameObject.SetActive(false);
        }
    }

    private void Awake()
    {
        BuildVisuals();
    }

    private void LateUpdate()
    {
        if (
            bubbleRoot == null ||
            !bubbleRoot.gameObject.activeSelf
        )
        {
            return;
        }

        bubbleRoot.position =
            transform.position +
            Vector3.up * worldHeight;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            bubbleRoot.gameObject.SetActive(false);
        }
    }

    private void ShowMessage(
        string message,
        Font font,
        Color accentColor,
        float height,
        float duration
    )
    {
        if (bubbleRoot == null)
        {
            BuildVisuals();
        }

        worldHeight = height;
        remainingTime = Mathf.Max(duration, 0.2f);

        float width = Mathf.Clamp(
            1.75f + message.Length * 0.15f,
            2.7f,
            5.5f
        );

        SetRendererSize(
            outerRenderer,
            width + 0.3f,
            1.18f
        );
        SetRendererSize(
            borderRenderer,
            width + 0.16f,
            1.05f
        );
        SetRendererSize(
            fillRenderer,
            width,
            0.92f
        );

        borderRenderer.color = accentColor;

        textMesh.text = message;
        textMesh.color = OfficeHudTheme.Ink;

        if (font != null)
        {
            textMesh.font = font;

            MeshRenderer textRenderer =
                textMesh.GetComponent<MeshRenderer>();

            if (textRenderer != null)
            {
                textRenderer.sharedMaterial = font.material;
            }
        }

        bubbleRoot.position =
            transform.position +
            Vector3.up * worldHeight;
        bubbleRoot.gameObject.SetActive(true);
    }

    private void BuildVisuals()
    {
        if (bubbleRoot != null)
        {
            return;
        }

        GameObject rootObject =
            new GameObject(
                gameObject.name + "_SpeechBurst"
            );

        bubbleRoot = rootObject.transform;

        outerRenderer = CreateBurstLayer(
            "Outer",
            new Color(0.04f, 0.05f, 0.07f, 0.98f),
            BaseSortingOrder
        );

        borderRenderer = CreateBurstLayer(
            "Accent",
            OfficeHudTheme.Red,
            BaseSortingOrder + 1
        );

        fillRenderer = CreateBurstLayer(
            "Paper",
            new Color(1f, 0.98f, 0.92f, 0.99f),
            BaseSortingOrder + 2
        );

        GameObject textObject =
            new GameObject("Text");

        textObject.transform.SetParent(
            bubbleRoot,
            false
        );
        textObject.transform.localPosition =
            new Vector3(0f, -0.015f, 0f);

        textMesh = textObject.AddComponent<TextMesh>();
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = 64;
        textMesh.characterSize = 0.05f;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.richText = false;

        MeshRenderer renderer =
            textMesh.GetComponent<MeshRenderer>();

        if (renderer != null)
        {
            renderer.sortingOrder =
                BaseSortingOrder + 3;
        }

        bubbleRoot.gameObject.SetActive(false);
    }

    private SpriteRenderer CreateBurstLayer(
        string objectName,
        Color color,
        int sortingOrder
    )
    {
        GameObject layerObject =
            new GameObject(objectName);

        layerObject.transform.SetParent(
            bubbleRoot,
            false
        );

        SpriteRenderer renderer =
            layerObject.AddComponent<SpriteRenderer>();

        renderer.sprite = GetBurstSprite();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        return renderer;
    }

    private static void SetRendererSize(
        SpriteRenderer renderer,
        float width,
        float height
    )
    {
        if (renderer == null || renderer.sprite == null)
        {
            return;
        }

        Vector2 size = renderer.sprite.bounds.size;

        renderer.transform.localScale =
            new Vector3(
                size.x > 0f ? width / size.x : width,
                size.y > 0f ? height / size.y : height,
                1f
            );
    }

    private static Sprite GetBurstSprite()
    {
        if (burstSprite != null)
        {
            return burstSprite;
        }

        const int pointCount = 32;
        Vector2 center = new Vector2(
            BurstTextureWidth * 0.5f,
            BurstTextureHeight * 0.5f
        );
        Vector2[] points = new Vector2[pointCount];

        for (int index = 0; index < pointCount; index++)
        {
            float angle =
                Mathf.PI * 2f * index / pointCount;
            float radius = index % 2 == 0
                ? 1f
                : 0.8f;

            points[index] = center + new Vector2(
                Mathf.Cos(angle) *
                BurstTextureWidth * 0.47f * radius,
                Mathf.Sin(angle) *
                BurstTextureHeight * 0.44f * radius
            );
        }

        Texture2D texture = new Texture2D(
            BurstTextureWidth,
            BurstTextureHeight,
            TextureFormat.RGBA32,
            false
        );

        texture.name = "RuntimeComicSpeechBurst";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;

        Color[] pixels =
            new Color[BurstTextureWidth * BurstTextureHeight];

        for (int y = 0; y < BurstTextureHeight; y++)
        {
            for (int x = 0; x < BurstTextureWidth; x++)
            {
                int insideSamples = 0;

                for (int sampleY = 0; sampleY < 2; sampleY++)
                {
                    for (int sampleX = 0; sampleX < 2; sampleX++)
                    {
                        Vector2 samplePoint = new Vector2(
                            x + 0.25f + sampleX * 0.5f,
                            y + 0.25f + sampleY * 0.5f
                        );

                        if (IsPointInsidePolygon(samplePoint, points))
                        {
                            insideSamples++;
                        }
                    }
                }

                pixels[y * BurstTextureWidth + x] =
                    new Color(1f, 1f, 1f, insideSamples / 4f);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);

        burstSprite = Sprite.Create(
            texture,
            new Rect(
                0f,
                0f,
                BurstTextureWidth,
                BurstTextureHeight
            ),
            new Vector2(0.5f, 0.5f),
            128f,
            0u,
            SpriteMeshType.FullRect
        );

        burstSprite.name = "RuntimeComicSpeechBurstSprite";
        burstSprite.hideFlags = HideFlags.HideAndDontSave;

        return burstSprite;
    }

    private static bool IsPointInsidePolygon(
        Vector2 point,
        Vector2[] polygon
    )
    {
        bool inside = false;
        int previous = polygon.Length - 1;

        for (int index = 0; index < polygon.Length; index++)
        {
            Vector2 currentPoint = polygon[index];
            Vector2 previousPoint = polygon[previous];

            bool crosses =
                (currentPoint.y > point.y) !=
                (previousPoint.y > point.y) &&
                point.x <
                (previousPoint.x - currentPoint.x) *
                (point.y - currentPoint.y) /
                (previousPoint.y - currentPoint.y) +
                currentPoint.x;

            if (crosses)
            {
                inside = !inside;
            }

            previous = index;
        }

        return inside;
    }

    private void OnDestroy()
    {
        if (bubbleRoot != null)
        {
            Destroy(bubbleRoot.gameObject);
        }
    }
}
