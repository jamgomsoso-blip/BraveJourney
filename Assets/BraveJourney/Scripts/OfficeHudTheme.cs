using UnityEngine;

public static class OfficeHudTheme
{
    public static readonly Color Ink =
        new Color(0.08f, 0.12f, 0.17f, 1f);
    public static readonly Color Paper =
        new Color(0.97f, 0.96f, 0.91f, 0.98f);
    public static readonly Color MutedPaper =
        new Color(0.76f, 0.8f, 0.82f, 1f);
    public static readonly Color Red =
        new Color(0.82f, 0.13f, 0.16f, 1f);
    public static readonly Color Cyan =
        new Color(0.08f, 0.55f, 0.66f, 1f);
    public static readonly Color Gold =
        new Color(0.95f, 0.65f, 0.12f, 1f);

    private static Texture2D fullHeartTexture;
    private static Texture2D emptyHeartTexture;

    public static float Scale =>
        Mathf.Clamp(Screen.height / 720f, 0.78f, 1.35f);

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetRuntimeTextures()
    {
        fullHeartTexture = null;
        emptyHeartTexture = null;
    }

    public static void DrawPanel(
        Rect rect,
        Color accentColor,
        bool drawShadow = true
    )
    {
        if (drawShadow)
        {
            DrawRect(
                new Rect(
                    rect.x + 5f,
                    rect.y + 6f,
                    rect.width,
                    rect.height
                ),
                new Color(0.02f, 0.04f, 0.06f, 0.35f)
            );
        }

        DrawRect(rect, Ink);
        DrawRect(
            Inset(rect, 3f),
            Paper
        );
        DrawRect(
            new Rect(
                rect.x + 3f,
                rect.y + 3f,
                rect.width - 6f,
                7f
            ),
            accentColor
        );

        float cornerSize = Mathf.Min(12f, rect.height * 0.18f);

        DrawRect(
            new Rect(
                rect.xMax - cornerSize - 3f,
                rect.y + 10f,
                cornerSize,
                3f
            ),
            new Color(
                accentColor.r,
                accentColor.g,
                accentColor.b,
                0.55f
            )
        );
    }

    public static void DrawHeart(Rect rect, bool filled)
    {
        Texture2D texture = filled
            ? GetHeartTexture(true)
            : GetHeartTexture(false);

        GUI.DrawTexture(
            rect,
            texture,
            ScaleMode.ScaleToFit,
            true
        );
    }

    public static void DrawProgressBar(
        Rect rect,
        float ratio,
        Color fillColor,
        int segmentCount = 0
    )
    {
        ratio = Mathf.Clamp01(ratio);

        DrawRect(rect, Ink);

        Rect inner = Inset(rect, 3f);
        DrawRect(inner, new Color(0.25f, 0.29f, 0.31f, 1f));

        if (ratio > 0f)
        {
            Rect filled = inner;
            filled.width *= ratio;
            DrawRect(filled, fillColor);

            DrawRect(
                new Rect(
                    filled.x,
                    filled.y,
                    filled.width,
                    Mathf.Max(2f, filled.height * 0.22f)
                ),
                new Color(1f, 1f, 1f, 0.2f)
            );
        }

        if (segmentCount <= 1)
        {
            return;
        }

        for (int index = 1; index < segmentCount; index++)
        {
            float x = inner.x +
                inner.width * index / segmentCount;

            DrawRect(
                new Rect(x - 1f, inner.y, 2f, inner.height),
                new Color(0.08f, 0.12f, 0.17f, 0.58f)
            );
        }
    }

    public static GUIStyle CreateTextStyle(
        Font font,
        int fontSize,
        TextAnchor alignment,
        Color textColor,
        FontStyle fontStyle = FontStyle.Bold
    )
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);

        style.font = font != null
            ? font
            : GUI.skin.font;
        style.fontSize = fontSize;
        style.fontStyle = fontStyle;
        style.alignment = alignment;
        style.normal.textColor = textColor;
        style.clipping = TextClipping.Clip;

        return style;
    }

    public static void DrawRect(Rect rect, Color color)
    {
        Color previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private static Rect Inset(Rect rect, float amount)
    {
        return new Rect(
            rect.x + amount,
            rect.y + amount,
            Mathf.Max(0f, rect.width - amount * 2f),
            Mathf.Max(0f, rect.height - amount * 2f)
        );
    }

    private static Texture2D GetHeartTexture(bool filled)
    {
        if (filled && fullHeartTexture != null)
        {
            return fullHeartTexture;
        }

        if (!filled && emptyHeartTexture != null)
        {
            return emptyHeartTexture;
        }

        const int textureSize = 96;
        Texture2D texture = new Texture2D(
            textureSize,
            textureSize,
            TextureFormat.RGBA32,
            false
        );

        texture.name = filled
            ? "OfficeHudFullHeart"
            : "OfficeHudEmptyHeart";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;

        Color interiorColor = filled
            ? Red
            : new Color(0.73f, 0.76f, 0.77f, 1f);
        Color[] pixels = new Color[textureSize * textureSize];

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float normalizedX =
                    (x + 0.5f - textureSize * 0.5f) /
                    (textureSize * 0.35f);
                float normalizedY =
                    (y + 0.5f - textureSize * 0.49f) /
                    (textureSize * 0.35f);

                bool insideOuter = IsInsideHeart(
                    normalizedX / 1.08f,
                    normalizedY / 1.08f
                );
                bool insideInner = IsInsideHeart(
                    normalizedX * 1.02f,
                    normalizedY * 1.02f
                );

                Color pixel = Color.clear;

                if (insideOuter)
                {
                    pixel = insideInner
                        ? interiorColor
                        : Ink;
                }

                pixels[y * textureSize + x] = pixel;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);

        if (filled)
        {
            fullHeartTexture = texture;
        }
        else
        {
            emptyHeartTexture = texture;
        }

        return texture;
    }

    private static bool IsInsideHeart(float x, float y)
    {
        float baseValue = x * x + y * y - 1f;
        return
            baseValue * baseValue * baseValue -
            x * x * y * y * y <= 0f;
    }
}
