using UnityEngine;

public enum StageHazardType
{
    Ground,
    Falling
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

    private HazardState state;
    private StageHazardType hazardType;

    private GameObject warningObject;
    private GameObject dangerObject;
    private SpriteRenderer warningRenderer;
    private Transform dangerTransform;

    private float groundSurfaceY;
    private float warningDuration;
    private float activeDuration;
    private float fallingSpeed;
    private float stateTimer;
    private bool hasStarted;
    private bool isConfigured;

    public bool IsFinished =>
        state == HazardState.Finished;

    public void ConfigureRunner(
        Transform player,
        Sprite visualSprite,
        StageHazardType type,
        float targetX,
        float groundY,
        float runnerSpeed,
        float warningTime = 0.85f
    )
    {
        ConfigureCommon(
            visualSprite,
            type,
            targetX,
            groundY,
            warningTime,
            1.2f,
            12f
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
        float fallSpeed
    )
    {
        ConfigureCommon(
            visualSprite,
            type,
            targetX,
            groundY,
            warningTime,
            dangerTime,
            fallSpeed
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
        warningObject.SetActive(true);
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
        float fallSpeed
    )
    {
        hazardType = type;
        groundSurfaceY = groundY;
        warningDuration = Mathf.Max(warningTime, 0.1f);
        activeDuration = Mathf.Max(dangerTime, 0.1f);
        fallingSpeed = Mathf.Max(fallSpeed, 0.1f);

        transform.position = new Vector3(targetX, 0f, 0f);

        CreateWarningVisual(visualSprite);
        CreateDangerVisual(visualSprite);

        state = HazardState.Waiting;
        isConfigured = true;
    }

    private void CreateWarningVisual(Sprite visualSprite)
    {
        warningObject = new GameObject("Warning");
        warningObject.transform.SetParent(transform, false);

        warningRenderer =
            warningObject.AddComponent<SpriteRenderer>();

        warningRenderer.sprite = visualSprite;
        warningRenderer.sortingOrder = 8;
        warningRenderer.color =
            new Color(1f, 0.72f, 0.05f, 0.45f);

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

        warningObject.SetActive(false);
    }

    private void CreateDangerVisual(Sprite visualSprite)
    {
        dangerObject = new GameObject("Danger");
        dangerObject.layer = 6;
        dangerObject.transform.SetParent(transform, false);

        dangerTransform = dangerObject.transform;

        SpriteRenderer dangerRenderer =
            dangerObject.AddComponent<SpriteRenderer>();

        dangerRenderer.sprite = visualSprite;
        dangerRenderer.sortingOrder = 7;
        dangerRenderer.color =
            new Color(0.9f, 0.08f, 0.08f, 1f);

        BoxCollider2D dangerCollider =
            dangerObject.AddComponent<BoxCollider2D>();

        dangerCollider.isTrigger = true;
        dangerObject.AddComponent<ObstacleDamage>();

        if (hazardType == StageHazardType.Ground)
        {
            dangerTransform.localPosition =
                new Vector3(0f, groundSurfaceY + 0.4f, 0f);

            dangerTransform.localScale =
                new Vector3(2.2f, 0.8f, 1f);
        }
        else
        {
            dangerTransform.localPosition =
                new Vector3(0f, FallingStartY, 0f);

            dangerTransform.localScale =
                Vector3.one * FallingBlockSize;
        }

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

        warningRenderer.color =
            new Color(1f, 0.72f, 0.05f, alpha);

        if (stateTimer < warningDuration)
        {
            return;
        }

        warningObject.SetActive(false);
        dangerObject.SetActive(true);
        stateTimer = 0f;

        state = hazardType == StageHazardType.Ground
            ? HazardState.GroundActive
            : HazardState.FallingActive;
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
