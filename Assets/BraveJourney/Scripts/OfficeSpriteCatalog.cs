using System.Collections.Generic;
using UnityEngine;

public static class OfficeSpriteCatalog
{
    private const string ResourceRoot = "Office/";
    private const string Resource2DPropsRoot =
        "Office2D/Props/";

    private static readonly Dictionary<string, Sprite> Cache =
        new Dictionary<string, Sprite>();

    public static Sprite FloorTile =>
        Load2D("OfficeFloorPlatform_2D");

    public static Sprite OfficeBackground =>
        Resources.Load<Sprite>(
            "Office2D/OfficeBackground_Wide"
        );

    public static Sprite WallTile =>
        Load("OfficeWallTile_16x16");

    public static Sprite WindowTile =>
        Load("OfficeWindowTile_16x16");

    public static Sprite Cubicle =>
        Load("OfficeCubicle_32x16");

    public static Sprite FilingCabinet =>
        Load("OfficeFilingCabinet_16x24");

    public static Sprite CeilingLight =>
        Load("OfficeCeilingLight_32x8");

    public static Sprite Platform16 =>
        FloorTile;

    public static Sprite Platform32 =>
        FloorTile;

    public static Sprite Platform48 =>
        FloorTile;

    public static Sprite HostileProjectile =>
        Load("BossMemoProjectile_12x12");

    public static Sprite ReflectedProjectile =>
        Load("BossReplyProjectile_12x12");

    public static Sprite LargeProjectile =>
        Load("BossLargeStampProjectile_16x16");

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetCache()
    {
        Cache.Clear();
    }

    public static Sprite GetObstacleSprite(
        bool isHigh,
        bool isSlide,
        int variation
    )
    {
        if (isSlide)
        {
            return variation % 2 == 0
                ? Load2D("OfficeDeskLong_2D")
                : Load2D("OfficeCubicle_2D");
        }

        if (isHigh)
        {
            switch (variation % 3)
            {
                case 1:
                    return Load2D("OfficeFilingCabinet_2D");
                case 2:
                    return Load2D("OfficeDrawer_2D");
                default:
                    return Load2D("OfficeCopier_2D");
            }
        }

        switch (variation % 4)
        {
            case 1:
                return Load2D("OfficeChair_2D");
            case 2:
                return Load2D("OfficePrinter_2D");
            case 3:
                return Load2D("OfficeDocumentBox_2D");
            default:
                return Load2D("OfficeComputer_2D");
        }
    }

    public static Sprite GetHazardSprite(
        StageHazardType type,
        StageHazardTheme theme
    )
    {
        if (type == StageHazardType.Ground)
        {
            return theme == StageHazardTheme.Leak
                ? Load2D("OfficeWaterSpill_2D")
                : Load2D("OfficeCable_2D");
        }

        if (theme == StageHazardTheme.Stamp)
        {
            return Load2D("OfficeStamp_2D");
        }

        return Load2D("OfficeFallingDocuments_2D");
    }

    public static Sprite Load(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            return null;
        }

        if (Cache.TryGetValue(assetName, out Sprite sprite))
        {
            return sprite;
        }

        sprite = Resources.Load<Sprite>(
            ResourceRoot + assetName
        );

        Cache[assetName] = sprite;
        return sprite;
    }

    private static Sprite Load2D(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            return null;
        }

        string cacheKey = "2D/" + assetName;

        if (Cache.TryGetValue(cacheKey, out Sprite sprite))
        {
            return sprite;
        }

        sprite = Resources.Load<Sprite>(
            Resource2DPropsRoot + assetName
        );

        Cache[cacheKey] = sprite;
        return sprite;
    }
}
