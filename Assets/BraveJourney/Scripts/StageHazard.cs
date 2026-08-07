using UnityEngine;

public enum StageHazardType
{
    Ground,
    Falling
}

public enum StageHazardTheme
{
    Standard,
    Leak,
    Subsidence,
    Formwork,
    Material,
    Stamp
}

public sealed class StageHazard : MonoBehaviour
{
    private enum HazardState
    {
        Waiting,
        Warning,
        GroundActive,
        FallingActive,
        Finished
    }

    private const float FallingStartY = 4f;
    private const float FallingBlockSize = 1.2f;
    private const float RunnerLandingGraceDuration = 0.45f;

    private HazardState state;
    private StageHazardType hazardType;
    private StageHazardTheme hazardTheme;

    private GameObject warningObject;
    private GameObject warningLabelObject;
    private GameObject dangerObject;
    private SpriteRenderer warningRenderer;
    private SpriteRenderer warningFrameRenderer;
    private SpriteRenderer warningLabelBackgroundRenderer;
    private Transform dangerTransform;
    private TextMesh warningLabel;
    private PlayerController runnerPlayerController;

    private float groundSurfaceY;
    private float warningDuration;
    private float activeDuration;
    private float fallingSpeed;
    private float stateTimer;
    private bool hasStarted;
    private bool isConfigured;
    private bool waitingForSafeLanding;
    private float landingGraceTimer;

    public bool IsFinished =>
        state == HazardState.Finished;

    public bool IsWaitingForSafeLanding =>
        waitingForSafeLanding;

    public void ConfigureRunner(
        Transform player,
        Sprite visualSprite,
        StageHazardType type,
        float targetX,
        float groundY,
        float runnerSpeed,
        float warningTime = 0.85f,
        StageHazardTheme theme = StageHazardTheme.Standard,
        Font uiFont = null,
        float fallingHazardSpeed = 12f
    )
    {
        runnerPlayerController = player != null
            ? player.GetComponent<PlayerController>()
            : null;

        ConfigureCommon(
            visualSprite,
            type,
            targetX,
            groundY,
            warningTime,
            1.2f,
            fallingHazardSpeed,
            theme,
            uiFont
        );

        float fallTravelTime =
            type == StageHazardType.Falling
                ? Mathf.Abs(
                    FallingStartY -
                    GetFallingRestY()
                ) / fallingSpeed
                : 0.1f;

        float leadDistance =
            runnerSpeed *
            (warningDuration + fallTravelTime + 0.1f);

        GameObject triggerObject =
            new GameObject("WarningTrigger");

        triggerObject.transform.SetParent(
            transform,
            false
        );

        triggerObject.transform.localPosition =
            new Vector3(-leadDistance, 0f, 0f);

        BoxCollider2D triggerCollider =
            triggerObject.AddComponent<BoxCollider2D>();

        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector2(1f, 12f);

        HazardTriggerRelay relay =
            triggerObject.AddComponent<HazardTriggerRelay>();

        relay.Initialize(this, player);
    }

    public void ConfigureImmediate(
        Sprite visualSprite,
        StageHazardType type,
        float targetX,
        float groundY,
        float warningTime,
        float dangerTime,
        float fallSpeed,
        StageHazardTheme theme = StageHazardTheme.Standard,
        Font uiFont = null
    )
    {
        ConfigureCommon(
            visualSprite,
            type,
            targetX,
            groundY,
            warningTime,
            dangerTime,
            fallSpeed,
            theme,
            uiFont
        );

        BeginWarning();
    }

    public void BeginWarning()
    {
        if (!isConfigured || hasStarted)
        {
            return;
        }

        hasStarted = true;
        state = HazardState.Warning;
        stateTimer = 0f;
        waitingForSafeLanding = false;
        landingGraceTimer = 0f;
        SetWarningVisible(true);
        GameAudioFeedback.Play(GameSoundCue.Warning);
    }

    public void CancelHazard()
    {
        Destroy(gameObject);
    }

    private void ConfigureCommon(
        Sprite visualSprite,
        StageHazardType type,
        float targetX,
        float groundY,
        float warningTime,
        float dangerTime,
        float fallSpeed,
        StageHazardTheme theme,
        Font uiFont
    )
    {
        hazardType = type;
        hazardTheme = theme;
        groundSurfaceY = groundY;
        warningDuration = Mathf.Max(warningTime, 0.1f);
        activeDuration = Mathf.Max(dangerTime, 0.1f);
        fallingSpeed = Mathf.Max(fallSpeed, 0.1f);

        transform.position = new Vector3(targetX, 0f, 0f);

        CreateWarningVisual(visualSprite, uiFont);
        CreateDangerVisual(visualSprite, uiFont);

        state = HazardState.Waiting;
        isConfigured = true;
    }

    private void CreateWarningVisual(
        Sprite visualSprite,
        Font uiFont
    )
    {
        warningObject = new GameObject("Warning");
        warningObject.transform.SetParent(transform, false);

        warningRenderer =
            warningObject.AddComponent<SpriteRenderer>();

        warningRenderer.sprite = StagePresentation.PixelSprite;
        warningRenderer.sortingOrder = 8;
        Color warningColor = GetWarningColor();
        warningColor.a = 0.45f;
        warningRenderer.color = warningColor;

        if (hazardType == StageHazardType.Ground)
        {
            warningObject.transform.localPosition =
                new Vector3(0f, groundSurfaceY + 0.08f, 0f);

            warningObject.transform.localScale =
                new Vector3(2.4f, 0.16f, 1f);
        }
        else
        {
            float warningCenterY =
                (FallingStartY + groundSurfaceY) * 0.5f;

            warningObject.transform.localPosition =
                new Vector3(0f, warningCenterY, 0f);

            warningObject.transform.localScale =
                new Vector3(
                    1.45f,
                    FallingStartY - groundSurfaceY,
                    1f
                );
        }

        CreateWarningFrame();
        CreateWarningLabel(uiFont);
        SetWarningVisible(false);
    }

    private void CreateDangerVisual(
        Sprite visualSprite,
        Font uiFont
    )
    {
        dangerObject = new GameObject("Danger");
        dangerObject.layer = 6;
        dangerObject.transform.SetParent(transform, false);

        dangerTransform = dangerObject.transform;

        SpriteRenderer dangerRenderer =
            dangerObject.AddComponent<SpriteRenderer>();

        Sprite officeHazardSprite =
            OfficeSpriteCatalog.GetHazardSprite(
                hazardType,
                hazardTheme
            );

        dangerRenderer.sprite =
            officeHazardSprite != null
                ? officeHazardSprite
                : StagePresentation.PixelSprite;
        dangerRenderer.sortingOrder = 7;
        dangerRenderer.color =
            officeHazardSprite != null
                ? Color.white
                : GetDangerColor();

        BoxCollider2D dangerCollider =
            dangerObject.AddComponent<BoxCollider2D>();

        dangerCollider.isTrigger = true;
        dangerObject.AddComponent<ObstacleDamage>();

        if (hazardType == StageHazardType.Ground)
        {
            float targetWidth =
                hazardTheme == StageHazardTheme.Material
                    ? 2.7f
                    : 2.4f;
            float targetHeight =
                hazardTheme == StageHazardTheme.Subsidence
                    ? 0.42f
                    : 0.55f;
            Vector2 spriteSize =
                dangerRenderer.sprite != null
                    ? dangerRenderer.sprite.bounds.size
                    : Vector2.one;

            dangerTransform.localPosition =
                new Vector3(
                    0f,
                    groundSurfaceY + targetHeight * 0.5f,
                    0f
                );

            dangerTransform.localScale =
                new Vector3(
                    spriteSize.x > 0f
                        ? targetWidth / spriteSize.x
                        : targetWidth,
                    spriteSize.y > 0f
                        ? targetHeight / spriteSize.y
                        : targetHeight,
                    1f
                );
        }
        else
        {
            dangerTransform.localPosition =
                new Vector3(0f, FallingStartY, 0f);

            float sizeMultiplier =
                hazardTheme == StageHazardTheme.Material
                    ? 1.45f
                    : hazardTheme == StageHazardTheme.Stamp
                        ? 1.2f
                        : 1f;

            Vector2 spriteSize =
                dangerRenderer.sprite != null
                    ? dangerRenderer.sprite.bounds.size
                    : Vector2.one;
            float targetSize =
                FallingBlockSize * sizeMultiplier;
            float uniformScale = targetSize /
                Mathf.Max(
                    spriteSize.x,
                    spriteSize.y,
                    0.001f
                );

            dangerTransform.localScale =
                new Vector3(
                    uniformScale,
                    uniformScale,
                    1f
                );

            if (hazardTheme == StageHazardTheme.Stamp)
            {
                dangerTransform.localRotation =
                    Quaternion.Euler(0f, 0f, -12f);
            }
        }

        CreateDangerLabel(uiFont);

        dangerObject.SetActive(false);
    }

    private void Update()
    {
        if (!isConfigured)
        {
            return;
        }

        switch (state)
        {
            case HazardState.Warning:
                UpdateWarning();
                break;

            case HazardState.GroundActive:
                UpdateGroundDanger();
                break;

            case HazardState.FallingActive:
                UpdateFallingDanger();
                break;
        }
    }

    private void UpdateWarning()
    {
        stateTimer += Time.deltaTime;

        float alpha = Mathf.Lerp(
            0.25f,
            0.8f,
            Mathf.PingPong(stateTimer * 4f, 1f)
        );

        Color warningColor = GetWarningColor();
        warningColor.a = alpha;
        warningRenderer.color = warningColor;

        if (warningLabel != null)
        {
            Color labelColor = warningColor;
            labelColor.a = 1f;
            warningLabel.color = labelColor;
        }

        if (warningFrameRenderer != null)
        {
            Color frameColor = warningColor;
            frameColor.a = Mathf.Min(alpha + 0.15f, 1f);
            warningFrameRenderer.color = frameColor;
        }

        if (stateTimer < warningDuration)
        {
            return;
        }

        if (ShouldWaitForSafeLanding())
        {
            return;
        }

        SetWarningVisible(false);
        dangerObject.SetActive(true);
        stateTimer = 0f;

        state = hazardType == StageHazardType.Ground
            ? HazardState.GroundActive
            : HazardState.FallingActive;
    }

    private bool ShouldWaitForSafeLanding()
    {
        if (
            hazardType != StageHazardType.Ground ||
            runnerPlayerController == null
        )
        {
            return false;
        }

        if (!runnerPlayerController.IsGrounded)
        {
            waitingForSafeLanding = true;
            landingGraceTimer = RunnerLandingGraceDuration;
            stateTimer = warningDuration;
            return true;
        }

        if (!waitingForSafeLanding)
        {
            return false;
        }

        landingGraceTimer -= Time.deltaTime;
        stateTimer = warningDuration;

        if (landingGraceTimer > 0f)
        {
            return true;
        }

        waitingForSafeLanding = false;
        return false;
    }

    private void UpdateGroundDanger()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer >= activeDuration)
        {
            FinishHazard();
        }
    }

    private void UpdateFallingDanger()
    {
        float restY = GetFallingRestY();

        dangerTransform.localPosition =
            Vector3.MoveTowards(
                dangerTransform.localPosition,
                new Vector3(0f, restY, 0f),
                fallingSpeed * Time.deltaTime
            );

        if (
            Mathf.Abs(
                dangerTransform.localPosition.y - restY
            ) > 0.01f
        )
        {
            return;
        }

        stateTimer += Time.deltaTime;

        if (stateTimer >= activeDuration)
        {
            FinishHazard();
        }
    }

    private float GetFallingRestY()
    {
        return groundSurfaceY + FallingBlockSize * 0.5f;
    }

    private void FinishHazard()
    {
        dangerObject.SetActive(false);
        state = HazardState.Finished;
        enabled = false;
    }

    private void CreateWarningLabel(Font uiFont)
    {
        warningLabelObject = new GameObject("WarningLabel");
        warningLabelObject.transform.SetParent(transform, false);

        warningLabelObject.transform.localPosition =
            hazardType == StageHazardType.Ground
                ? new Vector3(0f, groundSurfaceY + 1.15f, 0f)
                : new Vector3(0f, FallingStartY + 0.5f, 0f);

        GameObject labelBackground =
            new GameObject("WarningLabelBackground");

        labelBackground.transform.SetParent(
            warningLabelObject.transform,
            false
        );

        labelBackground.transform.localScale =
            new Vector3(3.1f, 0.68f, 1f);

        warningLabelBackgroundRenderer =
            labelBackground.AddComponent<SpriteRenderer>();

        warningLabelBackgroundRenderer.sprite =
            StagePresentation.PixelSprite;
        warningLabelBackgroundRenderer.color =
            new Color(0.03f, 0.04f, 0.06f, 0.88f);
        warningLabelBackgroundRenderer.sortingOrder = 9;

        warningLabel =
            warningLabelObject.AddComponent<TextMesh>();

        warningLabel.text = GetWarningLabel();
        warningLabel.anchor = TextAnchor.MiddleCenter;
        warningLabel.alignment = TextAlignment.Center;
        warningLabel.fontSize = 64;
        warningLabel.characterSize = 0.055f;
        warningLabel.fontStyle = FontStyle.Bold;

        if (uiFont != null)
        {
            warningLabel.font = uiFont;
        }

        MeshRenderer labelRenderer =
            warningLabel.GetComponent<MeshRenderer>();

        if (labelRenderer != null)
        {
            labelRenderer.sortingOrder = 10;

            if (uiFont != null)
            {
                labelRenderer.sharedMaterial = uiFont.material;
            }
        }
    }

    private void CreateWarningFrame()
    {
        GameObject frameObject =
            new GameObject("WarningFrame");

        frameObject.transform.SetParent(transform, false);
        frameObject.transform.localPosition =
            warningObject.transform.localPosition;
        frameObject.transform.localScale =
            warningObject.transform.localScale +
            (hazardType == StageHazardType.Ground
                ? new Vector3(0.25f, 0.3f, 0f)
                : new Vector3(0.22f, 0.1f, 0f));

        warningFrameRenderer =
            frameObject.AddComponent<SpriteRenderer>();

        warningFrameRenderer.sprite =
            StagePresentation.PixelSprite;

        Color frameColor = GetWarningColor();
        frameColor.a = 0.72f;
        warningFrameRenderer.color = frameColor;
        warningFrameRenderer.sortingOrder = 7;

        GameObject innerObject =
            new GameObject("WarningFrameInner");

        innerObject.transform.SetParent(frameObject.transform, false);
        innerObject.transform.localScale =
            hazardType == StageHazardType.Ground
                ? new Vector3(0.92f, 0.38f, 1f)
                : new Vector3(0.72f, 0.97f, 1f);

        SpriteRenderer innerRenderer =
            innerObject.AddComponent<SpriteRenderer>();

        innerRenderer.sprite = StagePresentation.PixelSprite;
        innerRenderer.color =
            new Color(0.03f, 0.04f, 0.06f, 0.7f);
        innerRenderer.sortingOrder = 8;

        frameObject.SetActive(false);
        warningObject.transform.SetSiblingIndex(1);
    }

    private void CreateDangerLabel(Font uiFont)
    {
        if (hazardTheme != StageHazardTheme.Stamp)
        {
            return;
        }

        GameObject labelObject = new GameObject("StampLabel");
        labelObject.transform.SetParent(dangerObject.transform, false);
        labelObject.transform.localPosition = Vector3.zero;
        labelObject.transform.localRotation = Quaternion.identity;
        labelObject.transform.localScale =
            new Vector3(0.72f, 0.72f, 1f);

        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = "반려";
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 64;
        label.characterSize = 0.08f;
        label.fontStyle = FontStyle.Bold;
        label.color = Color.white;

        if (uiFont != null)
        {
            label.font = uiFont;
        }

        MeshRenderer labelRenderer =
            label.GetComponent<MeshRenderer>();

        if (labelRenderer != null)
        {
            labelRenderer.sortingOrder = 9;

            if (uiFont != null)
            {
                labelRenderer.sharedMaterial = uiFont.material;
            }
        }
    }

    private void SetWarningVisible(bool isVisible)
    {
        if (warningObject != null)
        {
            warningObject.SetActive(isVisible);
        }

        if (warningLabelObject != null)
        {
            warningLabelObject.SetActive(isVisible);
        }

        if (warningFrameRenderer != null)
        {
            warningFrameRenderer.gameObject.SetActive(isVisible);
        }
    }

    private string GetWarningLabel()
    {
        switch (hazardTheme)
        {
            case StageHazardTheme.Leak:
                return "누수 경고";
            case StageHazardTheme.Subsidence:
                return "바닥 침하";
            case StageHazardTheme.Formwork:
                return "거푸집 낙하";
            case StageHazardTheme.Material:
                return "대형 자재 낙하";
            case StageHazardTheme.Stamp:
                return "결재 반려";
            default:
                return "공격 경고";
        }
    }

    private Color GetWarningColor()
    {
        switch (hazardTheme)
        {
            case StageHazardTheme.Leak:
                return new Color(0.1f, 0.78f, 1f, 1f);
            case StageHazardTheme.Subsidence:
                return new Color(1f, 0.58f, 0.12f, 1f);
            case StageHazardTheme.Formwork:
                return new Color(1f, 0.82f, 0.1f, 1f);
            case StageHazardTheme.Material:
                return new Color(1f, 0.44f, 0.08f, 1f);
            case StageHazardTheme.Stamp:
                return new Color(1f, 0.12f, 0.28f, 1f);
            default:
                return new Color(1f, 0.72f, 0.05f, 1f);
        }
    }

    private Color GetDangerColor()
    {
        switch (hazardTheme)
        {
            case StageHazardTheme.Leak:
                return new Color(0.05f, 0.4f, 0.95f, 1f);
            case StageHazardTheme.Subsidence:
                return new Color(0.3f, 0.18f, 0.08f, 1f);
            case StageHazardTheme.Formwork:
                return new Color(0.78f, 0.38f, 0.08f, 1f);
            case StageHazardTheme.Material:
                return new Color(0.58f, 0.08f, 0.04f, 1f);
            case StageHazardTheme.Stamp:
                return new Color(0.78f, 0.02f, 0.12f, 1f);
            default:
                return new Color(0.9f, 0.08f, 0.08f, 1f);
        }
    }
}

public sealed class HazardTriggerRelay : MonoBehaviour
{
    private StageHazard owner;
    private Transform expectedPlayer;

    public void Initialize(
        StageHazard hazardOwner,
        Transform player
    )
    {
        owner = hazardOwner;
        expectedPlayer = player;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (
            owner == null ||
            expectedPlayer == null ||
            other.transform != expectedPlayer
        )
        {
            return;
        }

        if (other.GetComponent<PlayerHealth>() == null)
        {
            return;
        }

        owner.BeginWarning();
        gameObject.SetActive(false);
    }
}
