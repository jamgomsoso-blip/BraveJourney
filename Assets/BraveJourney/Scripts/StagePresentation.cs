using UnityEngine;

public sealed class StagePresentation : MonoBehaviour
{
    private const int BackdropOrder = -100;
    private const int BackgroundOrder = -95;
    private const float BackgroundHeight = 10.4f;
    private const float BackgroundCenterY = 0.35f;
    private const float ScrollFactor = 0.18f;

    private static Texture2D pixelTexture;
    private static Sprite pixelSprite;

    private Camera targetCamera;
    private Transform presentationRoot;
    private Transform backgroundLayer;
    private float backgroundPanelWidth = 32f;

    public static Sprite PixelSprite
    {
        get
        {
            EnsurePixelSprite();
            return pixelSprite;
        }
    }

    public static StagePresentation EnsureForScene(
        Camera targetCamera
    )
    {
        if (targetCamera == null)
        {
            return null;
        }

        StagePresentation existing =
            targetCamera.GetComponent<StagePresentation>();

        return existing != null
            ? existing
            : targetCamera.gameObject
                .AddComponent<StagePresentation>();
    }

    private void Start()
    {
        BuildPresentation();
    }

    private void LateUpdate()
    {
        if (
            targetCamera == null ||
            backgroundLayer == null
        )
        {
            return;
        }

        Vector3 localPosition =
            backgroundLayer.localPosition;

        localPosition.x = -Mathf.Repeat(
            targetCamera.transform.position.x *
            ScrollFactor,
            backgroundPanelWidth
        );

        backgroundLayer.localPosition = localPosition;
    }

    private void BuildPresentation()
    {
        if (presentationRoot != null)
        {
            return;
        }

        targetCamera = GetComponent<Camera>();

        if (targetCamera != null)
        {
            targetCamera.clearFlags =
                CameraClearFlags.SolidColor;
            targetCamera.backgroundColor =
                new Color(0.77f, 0.82f, 0.85f, 1f);
        }

        GameObject rootObject =
            new GameObject("Office2DPresentation");

        presentationRoot = rootObject.transform;
        presentationRoot.SetParent(transform, false);
        presentationRoot.localPosition =
            new Vector3(0f, 0f, 10f);

        CreateSprite(
            presentationRoot,
            "OfficeBackdrop",
            PixelSprite,
            Vector2.zero,
            new Vector2(50f, 20f),
            new Color(0.77f, 0.82f, 0.85f, 1f),
            BackdropOrder
        );

        GameObject layerObject =
            new GameObject("OfficeBackgroundScroll");

        backgroundLayer = layerObject.transform;
        backgroundLayer.SetParent(
            presentationRoot,
            false
        );

        Sprite officeBackground =
            OfficeSpriteCatalog.OfficeBackground;

        if (officeBackground == null)
        {
            return;
        }

        float aspect =
            officeBackground.rect.height > 0f
                ? officeBackground.rect.width /
                    officeBackground.rect.height
                : 3f;

        backgroundPanelWidth =
            BackgroundHeight * aspect;

        for (int index = -1; index <= 1; index++)
        {
            CreateSprite(
                backgroundLayer,
                "OfficeBackground_" + index,
                officeBackground,
                new Vector2(
                    index * backgroundPanelWidth,
                    BackgroundCenterY
                ),
                new Vector2(
                    backgroundPanelWidth,
                    BackgroundHeight
                ),
                Color.white,
                BackgroundOrder
            );
        }

        LateUpdate();
    }

    private static GameObject CreateSprite(
        Transform parent,
        string objectName,
        Sprite sprite,
        Vector2 localPosition,
        Vector2 size,
        Color color,
        int sortingOrder
    )
    {
        GameObject spriteObject =
            new GameObject(objectName);

        spriteObject.transform.SetParent(parent, false);
        spriteObject.transform.localPosition =
            new Vector3(
                localPosition.x,
                localPosition.y,
                0f
            );

        Vector2 spriteSize = sprite.bounds.size;

        spriteObject.transform.localScale =
            new Vector3(
                spriteSize.x > 0f
                    ? size.x / spriteSize.x
                    : size.x,
                spriteSize.y > 0f
                    ? size.y / spriteSize.y
                    : size.y,
                1f
            );

        SpriteRenderer renderer =
            spriteObject.AddComponent<SpriteRenderer>();

        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        return spriteObject;
    }

    private static void EnsurePixelSprite()
    {
        if (pixelSprite != null)
        {
            return;
        }

        pixelTexture = new Texture2D(
            1,
            1,
            TextureFormat.RGBA32,
            false
        );

        pixelTexture.name = "BraveJourneyPixel";
        pixelTexture.filterMode = FilterMode.Point;
        pixelTexture.wrapMode = TextureWrapMode.Clamp;
        pixelTexture.SetPixel(0, 0, Color.white);
        pixelTexture.Apply();
        pixelTexture.hideFlags =
            HideFlags.HideAndDontSave;

        pixelSprite = Sprite.Create(
            pixelTexture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f
        );

        pixelSprite.name =
            "BraveJourneyPixelSprite";
        pixelSprite.hideFlags =
            HideFlags.HideAndDontSave;
    }
}
