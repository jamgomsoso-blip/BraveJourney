using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays the polished 2D player art while the existing Animator continues to
/// own gameplay timing and state duration. This component is visual only.
/// </summary>
public sealed class PlayerVisualAnimator : MonoBehaviour
{
    private const float TargetWorldHeight = 1.6f;
    private const float GroundAnchorY = TargetWorldHeight * 0.5f;
    private const int EffectFrameCount = 4;

    private static readonly Dictionary<string, Sprite[]> FrameCache =
        new Dictionary<string, Sprite[]>();

    private SpriteRenderer sourceRenderer;
    private SpriteRenderer visualRenderer;
    private SpriteRenderer effectRenderer;
    private Transform visualRoot;
    private Transform effectRoot;

    private Sprite[] currentFrames;
    private string currentStateName;
    private int frameIndex;
    private float frameTimer;
    private float framesPerSecond;
    private bool loops;
    private bool hasVisuals;
    private bool facesLeft;

    public bool HasVisuals => hasVisuals;

    public static PlayerVisualAnimator EnsureOn(
        GameObject playerObject
    )
    {
        if (playerObject == null)
        {
            return null;
        }

        PlayerVisualAnimator visualAnimator =
            playerObject.GetComponent<PlayerVisualAnimator>();

        return visualAnimator != null
            ? visualAnimator
            : playerObject.AddComponent<PlayerVisualAnimator>();
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetFrameCache()
    {
        FrameCache.Clear();
    }

    private void Awake()
    {
        sourceRenderer = GetComponent<SpriteRenderer>();
        Sprite[] sprintFrames = LoadStateFrames(
            "Sprint",
            GetFrameCount("Sprint")
        );

        hasVisuals =
            sourceRenderer != null &&
            HasFrames(sprintFrames);

        if (!hasVisuals)
        {
            enabled = false;
            return;
        }

        BuildRenderers();
        facesLeft = sourceRenderer.flipX;
        ApplyFacing();
        sourceRenderer.enabled = false;
    }

    private void Update()
    {
        if (
            !hasVisuals ||
            currentFrames == null ||
            currentFrames.Length == 0
        )
        {
            return;
        }

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(framesPerSecond, 1f);

        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;

            if (frameIndex >= currentFrames.Length - 1)
            {
                if (!loops)
                {
                    frameTimer = 0f;
                    break;
                }

                frameIndex = 0;
            }
            else
            {
                frameIndex++;
            }

            ApplyCurrentFrame();
        }

        ApplyFacing();
    }

    public void SetFacingDirection(float horizontalDirection)
    {
        if (Mathf.Abs(horizontalDirection) <= 0.01f)
        {
            return;
        }

        bool nextFacesLeft = horizontalDirection < 0f;

        if (facesLeft == nextFacesLeft)
        {
            return;
        }

        facesLeft = nextFacesLeft;

        if (currentFrames != null && currentFrames.Length > 0)
        {
            ApplyCurrentFrame();
            return;
        }

        ApplyFacing();
    }

    public bool PlayState(
        string stateName,
        bool restart = false
    )
    {
        if (!hasVisuals || string.IsNullOrEmpty(stateName))
        {
            return false;
        }

        if (!restart && currentStateName == stateName)
        {
            return true;
        }

        int frameCount = GetFrameCount(stateName);
        Sprite[] nextFrames = LoadStateFrames(
            stateName,
            frameCount
        );

        if (!HasFrames(nextFrames))
        {
            return false;
        }

        currentStateName = stateName;
        currentFrames = nextFrames;
        frameIndex = 0;
        frameTimer = 0f;
        framesPerSecond = GetFramesPerSecond(stateName);
        loops = DoesStateLoop(stateName);

        ApplyCurrentFrame();
        return true;
    }

    private void ApplyCurrentFrame()
    {
        if (
            visualRenderer == null ||
            currentFrames == null ||
            currentFrames.Length == 0
        )
        {
            return;
        }

        visualRenderer.sprite = currentFrames[
            Mathf.Clamp(frameIndex, 0, currentFrames.Length - 1)
        ];

        float parentWorldScale = Mathf.Max(
            Mathf.Abs(transform.lossyScale.y),
            0.001f
        );
        float spriteHeight = Mathf.Max(
            visualRenderer.sprite.bounds.size.y,
            0.001f
        );
        float localScale =
            TargetWorldHeight /
            (spriteHeight * parentWorldScale);

        visualRoot.localScale = new Vector3(
            localScale,
            localScale,
            1f
        );
        visualRoot.localPosition = new Vector3(
            0f,
            GroundAnchorY,
            0f
        );

        ApplyEffectFrame();
    }

    private void ApplyEffectFrame()
    {
        effectRenderer.enabled = false;

        string effectName = null;
        int effectIndex = 0;
        float effectScale = 0f;
        Vector3 effectPosition = Vector3.zero;
        bool renderBehindPlayer = true;

        switch (currentStateName)
        {
            case "Run":
                effectName = "RunDust";
                effectIndex = frameIndex % EffectFrameCount;
                effectScale = 0.26f;
                effectPosition = new Vector3(-0.42f, 0.12f, 0f);
                break;

            case "Sprint":
                effectName = "SprintSpeed";
                effectIndex = frameIndex % EffectFrameCount;
                effectScale = 0.43f;
                effectPosition = new Vector3(-0.65f, 0.18f, 0f);
                break;

            case "FrontFlip":
                effectName = "DoubleJump";
                effectIndex =
                    (frameIndex / 4) % EffectFrameCount;
                effectScale = 0.52f;
                effectPosition = new Vector3(0f, 0.8f, 0f);
                break;

            case "PunchC":
                if (frameIndex == 3 || frameIndex == 4)
                {
                    effectName = "CombatImpact";
                    effectIndex = frameIndex - 3;
                    effectScale = 0.25f;
                    effectPosition = new Vector3(0.72f, 0.92f, 0f);
                    renderBehindPlayer = false;
                }
                break;

            case "KickC":
                if (frameIndex == 3 || frameIndex == 4)
                {
                    effectName = "CombatImpact";
                    effectIndex = frameIndex == 3 ? 2 : 3;
                    effectScale = 0.34f;
                    effectPosition = new Vector3(0.92f, 0.6f, 0f);
                    renderBehindPlayer = false;
                }
                break;
        }

        if (string.IsNullOrEmpty(effectName))
        {
            return;
        }

        Sprite[] effectFrames = LoadEffectFrames(effectName);

        if (!HasFrames(effectFrames))
        {
            return;
        }

        effectRenderer.sprite = effectFrames[
            Mathf.Clamp(effectIndex, 0, effectFrames.Length - 1)
        ];
        effectRenderer.sortingOrder =
            sourceRenderer.sortingOrder +
            (renderBehindPlayer ? -1 : 1);

        if (facesLeft)
        {
            effectPosition.x *= -1f;
        }

        effectRoot.localPosition = effectPosition;
        effectRoot.localScale = new Vector3(
            effectScale,
            effectScale,
            1f
        );
        effectRenderer.enabled = true;
    }

    private void ApplyFacing()
    {
        if (sourceRenderer != null)
        {
            sourceRenderer.flipX = facesLeft;
        }

        if (visualRenderer != null)
        {
            visualRenderer.flipX = facesLeft;
        }

        if (effectRenderer != null)
        {
            effectRenderer.flipX = facesLeft;
        }
    }

    private void BuildRenderers()
    {
        GameObject visualObject =
            new GameObject("Player2DVisual");

        visualRoot = visualObject.transform;
        visualRoot.SetParent(transform, false);

        visualRenderer =
            visualObject.AddComponent<SpriteRenderer>();
        CopyRendererSettings(visualRenderer);

        GameObject effectObject =
            new GameObject("Player2DEffect");

        effectRoot = effectObject.transform;
        effectRoot.SetParent(transform, false);

        effectRenderer =
            effectObject.AddComponent<SpriteRenderer>();
        CopyRendererSettings(effectRenderer);
        effectRenderer.enabled = false;
    }

    private void CopyRendererSettings(SpriteRenderer target)
    {
        target.sortingLayerID = sourceRenderer.sortingLayerID;
        target.sortingOrder = sourceRenderer.sortingOrder;
        target.sharedMaterial = sourceRenderer.sharedMaterial;
        target.color = sourceRenderer.color;
        target.flipX = sourceRenderer.flipX;
        target.flipY = sourceRenderer.flipY;
        target.maskInteraction = sourceRenderer.maskInteraction;
    }

    private static Sprite[] LoadStateFrames(
        string stateName,
        int frameCount
    )
    {
        return LoadFrames(
            "State/" + stateName,
            "Player2D/Frames/Player_" + stateName + "_",
            frameCount
        );
    }

    private static Sprite[] LoadEffectFrames(string effectName)
    {
        return LoadFrames(
            "Effect/" + effectName,
            "Player2D/Effects/Player_" + effectName + "_",
            EffectFrameCount
        );
    }

    private static Sprite[] LoadFrames(
        string cacheKey,
        string resourcePrefix,
        int frameCount
    )
    {
        Sprite[] cachedFrames;

        if (FrameCache.TryGetValue(cacheKey, out cachedFrames))
        {
            return cachedFrames;
        }

        Sprite[] frames = new Sprite[frameCount];

        for (int index = 0; index < frameCount; index++)
        {
            frames[index] = Resources.Load<Sprite>(
                resourcePrefix + index.ToString("00")
            );
        }

        FrameCache.Add(cacheKey, frames);
        return frames;
    }

    private static int GetFrameCount(string stateName)
    {
        switch (stateName)
        {
            case "Run":
            case "PunchC":
            case "KickC":
                return 8;
            case "Sprint":
                return 6;
            case "FrontFlip":
                return 16;
            case "Die":
                return 12;
            default:
                return 4;
        }
    }

    private static float GetFramesPerSecond(string stateName)
    {
        switch (stateName)
        {
            case "Idle":
                return 6f;
            case "Run":
                return 12f;
            case "Sprint":
                return 14f;
            case "FrontFlip":
                return 24f;
            case "PunchC":
            case "KickC":
                return 16f;
            case "HitDamage":
                return 14f;
            case "Die":
                return 18f;
            default:
                return 12f;
        }
    }

    private static bool DoesStateLoop(string stateName)
    {
        switch (stateName)
        {
            case "Idle":
            case "Run":
            case "Sprint":
            case "Jump":
            case "FrontFlip":
                return true;
            default:
                return false;
        }
    }

    private static bool HasFrames(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0)
        {
            return false;
        }

        for (int index = 0; index < frames.Length; index++)
        {
            if (frames[index] == null)
            {
                return false;
            }
        }

        return true;
    }
}
