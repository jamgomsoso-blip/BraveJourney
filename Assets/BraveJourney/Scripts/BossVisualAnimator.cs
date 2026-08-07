using System.Collections.Generic;
using UnityEngine;

public sealed class BossVisualAnimator : MonoBehaviour
{
    private enum VisualState
    {
        Idle,
        Attack,
        Stunned,
        Defeated
    }

    private const int FrameCount = 4;
    private const float TargetWorldHeight = 2f;
    private const float IdleBreathDuration = 1.8f;
    private const float IdleBreathAmount = 0.01f;

    private sealed class FrameSet
    {
        public Sprite[] Idle;
        public Sprite[] Attack;
        public Sprite[] Stunned;
        public Sprite[] Defeated;
    }

    private static readonly Dictionary<string, FrameSet> FrameCache =
        new Dictionary<string, FrameSet>();

    private SpriteRenderer sourceRenderer;
    private SpriteRenderer visualRenderer;
    private Transform visualRoot;
    private FrameSet frameSet;
    private string characterFolder;

    private VisualState state;
    private int frameIndex;
    private float frameTimer;
    private float stateTimer;
    private bool hasVisuals;

    public static BossVisualAnimator EnsureOn(GameObject bossObject)
    {
        if (bossObject == null)
        {
            return null;
        }

        BossVisualAnimator visualAnimator =
            bossObject.GetComponent<BossVisualAnimator>();

        return visualAnimator != null
            ? visualAnimator
            : bossObject.AddComponent<BossVisualAnimator>();
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
        BossHealth bossHealth = GetComponent<BossHealth>();
        ConfigureForBoss(
            bossHealth != null ? bossHealth.BossName : string.Empty
        );
    }

    public void ConfigureForBoss(string bossName)
    {
        string nextCharacterFolder =
            GetCharacterFolder(bossName);

        if (
            frameSet != null &&
            characterFolder == nextCharacterFolder
        )
        {
            return;
        }

        characterFolder = nextCharacterFolder;
        frameSet = LoadFrames(characterFolder);

        hasVisuals =
            HasFrames(frameSet.Idle) &&
            HasFrames(frameSet.Attack) &&
            HasFrames(frameSet.Stunned) &&
            HasFrames(frameSet.Defeated);

        if (!hasVisuals || sourceRenderer == null)
        {
            enabled = false;
            return;
        }

        if (visualRenderer == null)
        {
            BuildVisualRenderer();
        }

        sourceRenderer.enabled = false;
        enabled = true;
        PlayIdle();
    }

    private void Update()
    {
        if (!hasVisuals)
        {
            return;
        }

        stateTimer += Time.deltaTime;

        if (state == VisualState.Idle)
        {
            ApplyIdleBreathing();
            return;
        }

        frameTimer += Time.deltaTime;
        float frameDuration = GetFrameDuration();

        if (frameTimer < frameDuration)
        {
            return;
        }

        frameTimer -= frameDuration;

        if (state == VisualState.Defeated)
        {
            frameIndex = Mathf.Min(
                frameIndex + 1,
                FrameCount - 1
            );
            ApplyCurrentFrame();
            return;
        }

        frameIndex++;

        if (
            state == VisualState.Attack &&
            frameIndex >= FrameCount
        )
        {
            SetState(VisualState.Idle);
            return;
        }

        frameIndex %= FrameCount;
        ApplyCurrentFrame();
    }

    public void PlayIdle()
    {
        if (state == VisualState.Defeated)
        {
            return;
        }

        SetState(VisualState.Idle);
    }

    public void PlayAttack()
    {
        if (
            state == VisualState.Stunned ||
            state == VisualState.Defeated
        )
        {
            return;
        }

        SetState(VisualState.Attack);
    }

    public void PlayStunned()
    {
        if (state == VisualState.Defeated)
        {
            return;
        }

        SetState(VisualState.Stunned);
    }

    public void PlayDefeated()
    {
        SetState(VisualState.Defeated);
    }

    public void SetFacingLeft(bool facesLeft)
    {
        if (visualRenderer != null)
        {
            // Generated frames face left by default.
            visualRenderer.flipX = !facesLeft;
        }
    }

    public void SetTint(Color tint)
    {
        if (visualRenderer != null)
        {
            visualRenderer.color = tint;
        }
    }

    private void SetState(VisualState nextState)
    {
        if (!hasVisuals)
        {
            return;
        }

        state = nextState;
        frameIndex = 0;
        frameTimer = 0f;
        stateTimer = 0f;
        ApplyCurrentFrame();
    }

    private void ApplyCurrentFrame()
    {
        if (visualRenderer == null)
        {
            return;
        }

        Sprite[] frames = GetCurrentFrames();

        if (!HasFrames(frames))
        {
            return;
        }

        Sprite frame = frames[
            Mathf.Clamp(frameIndex, 0, frames.Length - 1)
        ];

        visualRenderer.sprite = frame;

        float parentWorldScale = Mathf.Max(
            Mathf.Abs(transform.lossyScale.y),
            0.001f
        );
        float spriteHeight = Mathf.Max(
            frame.bounds.size.y,
            0.001f
        );
        float localScale =
            TargetWorldHeight /
            (spriteHeight * parentWorldScale);

        visualRoot.localScale =
            new Vector3(localScale, localScale, 1f);
        visualRoot.localPosition = Vector3.zero;
    }

    private void ApplyIdleBreathing()
    {
        if (visualRenderer == null || visualRenderer.sprite == null)
        {
            return;
        }

        Sprite frame = visualRenderer.sprite;
        float parentWorldScale = Mathf.Max(
            Mathf.Abs(transform.lossyScale.y),
            0.001f
        );
        float spriteHeight = Mathf.Max(
            frame.bounds.size.y,
            0.001f
        );
        float baseScale =
            TargetWorldHeight /
            (spriteHeight * parentWorldScale);
        float breathProgress =
            0.5f -
            0.5f * Mathf.Cos(
                stateTimer *
                Mathf.PI *
                2f /
                IdleBreathDuration
            );
        float breathingScale =
            baseScale *
            (1f + breathProgress * IdleBreathAmount);

        visualRoot.localScale = new Vector3(
            baseScale,
            breathingScale,
            1f
        );

        // Pin the sprite's feet while only its upper body rises subtly.
        visualRoot.localPosition = new Vector3(
            0f,
            frame.bounds.extents.y *
            (breathingScale - baseScale),
            0f
        );
    }

    private Sprite[] GetCurrentFrames()
    {
        switch (state)
        {
            case VisualState.Attack:
                return frameSet.Attack;
            case VisualState.Stunned:
                return frameSet.Stunned;
            case VisualState.Defeated:
                return frameSet.Defeated;
            default:
                return frameSet.Idle;
        }
    }

    private float GetFrameDuration()
    {
        switch (state)
        {
            case VisualState.Attack:
                return 0.11f;
            case VisualState.Stunned:
                return 0.15f;
            case VisualState.Defeated:
                return 0.16f;
            default:
                return 0.22f;
        }
    }

    private void BuildVisualRenderer()
    {
        GameObject visualObject =
            new GameObject("Boss2DVisual");

        visualRoot = visualObject.transform;
        visualRoot.SetParent(transform, false);

        visualRenderer =
            visualObject.AddComponent<SpriteRenderer>();
        visualRenderer.sortingLayerID =
            sourceRenderer.sortingLayerID;
        visualRenderer.sortingOrder =
            sourceRenderer.sortingOrder;
        visualRenderer.sharedMaterial =
            sourceRenderer.sharedMaterial;
        visualRenderer.color = sourceRenderer.color;
        visualRenderer.flipX = false;
    }

    private static FrameSet LoadFrames(string folderName)
    {
        FrameSet cachedFrames;

        if (FrameCache.TryGetValue(folderName, out cachedFrames))
        {
            return cachedFrames;
        }

        FrameSet loadedFrames = new FrameSet
        {
            Idle = LoadStateFrames(folderName, "Idle"),
            Attack = LoadStateFrames(folderName, "Attack"),
            Stunned = LoadStateFrames(folderName, "Stunned"),
            Defeated = LoadStateFrames(folderName, "Defeated")
        };

        FrameCache.Add(folderName, loadedFrames);
        return loadedFrames;
    }

    private static Sprite[] LoadStateFrames(
        string folderName,
        string stateName
    )
    {
        Sprite[] frames = new Sprite[FrameCount];
        string rankPath = string.IsNullOrEmpty(folderName)
            ? string.Empty
            : folderName + "/";

        for (int index = 0; index < FrameCount; index++)
        {
            frames[index] = Resources.Load<Sprite>(
                "Boss2D/" +
                rankPath +
                "Frames/Boss_" +
                stateName +
                "_" +
                index.ToString("00")
            );
        }

        return frames;
    }

    private static string GetCharacterFolder(string bossName)
    {
        switch (bossName)
        {
            case "주임":
                return "SeniorStaff";
            case "대리":
                return "AssistantManager";
            case "과장":
                return "Manager";
            case "차장":
                return "DeputyGeneralManager";
            case "부장":
                return "GeneralManager";
            case "부사장":
                return "VicePresident";
            case "대표":
                return string.Empty;
            default:
                // Keep the existing red-tie representative as fallback.
                return string.Empty;
        }
    }

    private static bool HasFrames(Sprite[] frames)
    {
        if (frames == null || frames.Length != FrameCount)
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
