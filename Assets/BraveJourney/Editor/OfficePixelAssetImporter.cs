using UnityEditor;
using UnityEngine;

public sealed class OfficePixelAssetImporter : AssetPostprocessor
{
    private const string OfficeAssetPath =
        "Assets/BraveJourney/Resources/Office/";
    private const string OfficeBackgroundPath =
        "Assets/BraveJourney/Resources/Office2D/";
    private const string BossSpritePath =
        "Assets/BraveJourney/Resources/Boss2D/";
    private const string PlayerSpritePath =
        "Assets/BraveJourney/Resources/Player2D/";
    private const string ComicAssetPath =
        "Assets/BraveJourney/Resources/Comics/";

    private void OnPreprocessTexture()
    {
        bool isPixelAsset =
            assetPath.StartsWith(OfficeAssetPath);
        bool isOfficeBackground =
            assetPath.StartsWith(OfficeBackgroundPath);
        bool isBossSprite =
            assetPath.StartsWith(BossSpritePath);
        bool isPlayerSprite =
            assetPath.StartsWith(PlayerSpritePath);
        bool isComicAsset =
            assetPath.StartsWith(ComicAssetPath);

        if (
            !isPixelAsset &&
            !isOfficeBackground &&
            !isBossSprite &&
            !isPlayerSprite &&
            !isComicAsset
        )
        {
            return;
        }

        TextureImporter importer =
            (TextureImporter)assetImporter;

        importer.textureShape =
            TextureImporterShape.Texture2D;
        importer.textureType =
            TextureImporterType.Sprite;
        importer.sRGBTexture = true;
        importer.spriteImportMode =
            SpriteImportMode.Single;
        importer.spritePixelsPerUnit =
            isPixelAsset ? 16f : 100f;
        importer.filterMode =
            isPixelAsset
                ? FilterMode.Point
                : FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression =
            isPixelAsset
                ? TextureImporterCompression.Uncompressed
                : TextureImporterCompression.CompressedHQ;
        importer.maxTextureSize =
            isPixelAsset
                ? 64
                : isBossSprite || isPlayerSprite
                    ? 512
                    : 2048;
        importer.npotScale =
            TextureImporterNPOTScale.None;

        TextureImporterSettings settings =
            new TextureImporterSettings();

        importer.ReadTextureSettings(settings);
        settings.spriteAlignment =
            (int)SpriteAlignment.Center;
        settings.spritePivot =
            new Vector2(0.5f, 0.5f);
        settings.spriteMeshType =
            SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
    }
}
