using System;
using UnityEngine;

public sealed class BossComicCutscene : MonoBehaviour
{
    private enum ComicPhase
    {
        Prologue,
        Intro,
        Victory,
        Defeat
    }

    private sealed class ComicTextBlock
    {
        public readonly string Text;
        public readonly Rect NormalizedRect;
        public readonly int FontSize;
        public readonly float Rotation;

        public ComicTextBlock(
            string text,
            float x,
            float y,
            float width,
            float height,
            int fontSize = 25,
            float rotation = 0f
        )
        {
            Text = text;
            NormalizedRect = new Rect(
                x,
                y,
                width,
                height
            );
            FontSize = fontSize;
            Rotation = rotation;
        }
    }

    private sealed class ComicPage
    {
        public readonly Texture2D Artwork;
        public readonly ComicPhase Phase;
        public readonly string Title;
        public readonly string ContinueLabel;
        public readonly ComicTextBlock[] TextBlocks;

        public ComicPage(
            Texture2D artwork,
            ComicPhase phase,
            string title,
            string continueLabel,
            ComicTextBlock[] textBlocks
        )
        {
            Artwork = artwork;
            Phase = phase;
            Title = title;
            ContinueLabel = continueLabel;
            TextBlocks = textBlocks;
        }
    }

    private const float ArtworkAspect = 1672f / 941f;

    private static bool hasShownPrologue;

    private ComicPage currentPage;
    private Font uiFont;
    private Action completion;
    private bool isShowing;
    private float previousTimeScale = 1f;
    private int openedFrame;

    public bool IsShowing => isShowing;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetSessionState()
    {
        hasShownPrologue = false;
    }

    public static bool SupportsStage(int stageNumber)
    {
        return
            stageNumber >= StageProfileCatalog.FirstStageNumber &&
            stageNumber <= StageProfileCatalog.LastStageNumber;
    }

    public static BossComicCutscene EnsureForScene(
        GameObject owner
    )
    {
        BossComicCutscene existing =
            FindFirstObjectByType<BossComicCutscene>();

        if (existing != null)
        {
            return existing;
        }

        GameObject cutsceneObject =
            new GameObject("BossComicCutscene");

        return cutsceneObject.AddComponent<BossComicCutscene>();
    }

    public void ShowIntro(
        StageProfile profile,
        Font font,
        Action onComplete
    )
    {
        ShowPage(
            profile,
            ComicPhase.Intro,
            font,
            onComplete
        );
    }

    public void ShowPrologue(
        StageProfile profile,
        Font font
    )
    {
        if (
            hasShownPrologue ||
            profile == null ||
            profile.StageNumber != StageProfileCatalog.FirstStageNumber
        )
        {
            return;
        }

        hasShownPrologue = true;

        ShowPage(
            profile,
            ComicPhase.Prologue,
            font,
            null
        );
    }

    public void ShowVictory(
        StageProfile profile,
        Font font,
        Action onComplete
    )
    {
        ShowPage(
            profile,
            ComicPhase.Victory,
            font,
            onComplete
        );
    }

    public void ShowDefeat(
        StageProfile profile,
        Font font,
        Action onComplete
    )
    {
        ShowPage(
            profile,
            ComicPhase.Defeat,
            font,
            onComplete
        );
    }

    private void ShowPage(
        StageProfile profile,
        ComicPhase phase,
        Font font,
        Action onComplete
    )
    {
        if (isShowing)
        {
            onComplete?.Invoke();
            return;
        }

        currentPage = CreatePage(profile, phase);

        if (currentPage == null || currentPage.Artwork == null)
        {
            Debug.LogWarning(
                "컷만화 이미지를 찾지 못해 장면을 건너뜁니다: " +
                GetResourcePath(profile, phase)
            );
            onComplete?.Invoke();
            return;
        }

        uiFont = font;
        completion = onComplete;
        previousTimeScale = Time.timeScale;
        openedFrame = Time.frameCount;
        isShowing = true;
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (!isShowing || Time.frameCount == openedFrame)
        {
            return;
        }

        if (currentPage.Phase == ComicPhase.Defeat)
        {
            return;
        }

        if (
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter)
        )
        {
            FinishPage();
        }
    }

    private void FinishPage()
    {
        if (!isShowing)
        {
            return;
        }

        Action onComplete = completion;
        ComicPage finishedPage = currentPage;

        isShowing = false;
        currentPage = null;
        completion = null;
        Time.timeScale = previousTimeScale;
        UnloadArtwork(finishedPage);

        onComplete?.Invoke();
    }

    private void OnDestroy()
    {
        if (isShowing)
        {
            Time.timeScale = previousTimeScale;
            UnloadArtwork(currentPage);
        }
    }

    private void OnGUI()
    {
        if (!isShowing || currentPage == null)
        {
            return;
        }

        GUI.depth = -500;

        DrawSolidRect(
            new Rect(0f, 0f, Screen.width, Screen.height),
            Color.black
        );

        Rect pageRect = GetPageRect();

        GUI.DrawTexture(
            pageRect,
            currentPage.Artwork,
            ScaleMode.StretchToFill,
            false
        );

        foreach (
            ComicTextBlock block in currentPage.TextBlocks
        )
        {
            DrawTextBlock(pageRect, block);
        }

        if (currentPage.Phase == ComicPhase.Defeat)
        {
            DrawDefeatRestartPrompt(pageRect);
        }
        else
        {
            DrawContinueButton(pageRect);
        }
    }

    private Rect GetPageRect()
    {
        float screenAspect =
            (float)Screen.width / Mathf.Max(Screen.height, 1);

        if (screenAspect > ArtworkAspect)
        {
            float width = Screen.height * ArtworkAspect;
            return new Rect(
                (Screen.width - width) * 0.5f,
                0f,
                width,
                Screen.height
            );
        }

        float height = Screen.width / ArtworkAspect;

        return new Rect(
            0f,
            (Screen.height - height) * 0.5f,
            Screen.width,
            height
        );
    }

    private void DrawTitle(Rect pageRect)
    {
        float scale = pageRect.height / 720f;
        Rect titleRect = new Rect(
            pageRect.x + 12f * scale,
            pageRect.y + 10f * scale,
            132f * scale,
            32f * scale
        );

        DrawSolidRect(
            titleRect,
            new Color(0.03f, 0.04f, 0.06f, 0.9f)
        );

        GUIStyle titleStyle = CreateTextStyle(
            Mathf.RoundToInt(16f * scale),
            Color.white,
            TextAnchor.MiddleCenter
        );

        GUI.Label(titleRect, currentPage.Title, titleStyle);
    }

    private void DrawTextBlock(
        Rect pageRect,
        ComicTextBlock block
    )
    {
        Rect normalized = block.NormalizedRect;
        Rect textRect = new Rect(
            pageRect.x + normalized.x * pageRect.width,
            pageRect.y + normalized.y * pageRect.height,
            normalized.width * pageRect.width,
            normalized.height * pageRect.height
        );
        float pageScale = pageRect.height / 720f;
        float horizontalPadding = Mathf.Min(
            12f * pageScale,
            textRect.width * 0.07f
        );
        float verticalPadding = Mathf.Min(
            8f * pageScale,
            textRect.height * 0.12f
        );

        textRect.x += horizontalPadding;
        textRect.y += verticalPadding;
        textRect.width = Mathf.Max(
            1f,
            textRect.width - horizontalPadding * 2f
        );
        textRect.height = Mathf.Max(
            1f,
            textRect.height - verticalPadding * 2f
        );

        int fontSize = Mathf.Max(
            10,
            Mathf.RoundToInt(
                block.FontSize * pageScale * 0.88f
            )
        );

        GUIStyle style = CreateTextStyle(
            fontSize,
            new Color(0.04f, 0.05f, 0.07f, 1f),
            TextAnchor.MiddleCenter
        );
        style.wordWrap = false;
        GUIContent content = new GUIContent(block.Text);

        while (
            style.fontSize > 9 &&
            !FitsInside(style, block.Text, textRect)
        )
        {
            style.fontSize--;
        }

        Matrix4x4 previousMatrix = GUI.matrix;

        if (Mathf.Abs(block.Rotation) > 0.01f)
        {
            GUIUtility.RotateAroundPivot(
                block.Rotation,
                textRect.center
            );
        }

        GUI.Label(textRect, content, style);
        GUI.matrix = previousMatrix;
    }

    private static bool FitsInside(
        GUIStyle style,
        string text,
        Rect rect
    )
    {
        string[] lines = text.Split('\n');
        float widestLine = 0f;
        float totalHeight = 0f;

        foreach (string line in lines)
        {
            Vector2 lineSize = style.CalcSize(
                new GUIContent(line)
            );
            widestLine = Mathf.Max(widestLine, lineSize.x);
            totalHeight += lineSize.y;
        }

        return widestLine <= rect.width &&
            totalHeight <= rect.height;
    }

    private void DrawContinueButton(Rect pageRect)
    {
        float scale = pageRect.height / 720f;
        Rect buttonRect = new Rect(
            pageRect.xMax - 216f * scale,
            pageRect.yMax - 54f * scale,
            204f * scale,
            42f * scale
        );

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            font = uiFont != null ? uiFont : GUI.skin.font,
            fontSize = Mathf.RoundToInt(19f * scale),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.active.textColor = Color.white;

        Color previousBackground = GUI.backgroundColor;
        GUI.backgroundColor =
            new Color(0.08f, 0.1f, 0.14f, 0.94f);

        if (
            GUI.Button(
                buttonRect,
                currentPage.ContinueLabel + "  ▶",
                buttonStyle
            )
        )
        {
            FinishPage();
        }

        GUI.backgroundColor = previousBackground;
    }

    private void DrawDefeatRestartPrompt(Rect pageRect)
    {
        float scale = pageRect.height / 720f;
        Rect panelRect = new Rect(
            pageRect.xMax - 390f * scale,
            pageRect.yMax - 80f * scale,
            374f * scale,
            64f * scale
        );

        DrawSolidRect(
            panelRect,
            new Color(0.025f, 0.03f, 0.045f, 0.94f)
        );

        GUIStyle titleStyle = CreateTextStyle(
            Mathf.RoundToInt(24f * scale),
            new Color(0.95f, 0.2f, 0.18f, 1f),
            TextAnchor.MiddleCenter
        );
        GUIStyle restartStyle = CreateTextStyle(
            Mathf.RoundToInt(17f * scale),
            Color.white,
            TextAnchor.MiddleCenter
        );

        GUI.Label(
            new Rect(
                panelRect.x,
                panelRect.y + 3f * scale,
                panelRect.width,
                30f * scale
            ),
            "퇴사 실패",
            titleStyle
        );
        GUI.Label(
            new Rect(
                panelRect.x,
                panelRect.y + 31f * scale,
                panelRect.width,
                26f * scale
            ),
            "R - 다시 출근하기",
            restartStyle
        );
    }

    private GUIStyle CreateTextStyle(
        int fontSize,
        Color color,
        TextAnchor alignment
    )
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            font = uiFont != null ? uiFont : GUI.skin.font,
            fontSize = fontSize,
            fontStyle = FontStyle.Bold,
            alignment = alignment,
            wordWrap = true,
            clipping = TextClipping.Clip
        };

        style.normal.textColor = color;
        return style;
    }

    private static void DrawSolidRect(Rect rect, Color color)
    {
        Color previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private static void UnloadArtwork(ComicPage page)
    {
        if (page != null && page.Artwork != null)
        {
            Resources.UnloadAsset(page.Artwork);
        }
    }

    private static ComicPage CreatePage(
        StageProfile profile,
        ComicPhase phase
    )
    {
        if (profile == null || !SupportsStage(profile.StageNumber))
        {
            return null;
        }

        int stageNumber = profile.StageNumber;
        string resourcePath = GetResourcePath(profile, phase);
        Texture2D artwork =
            Resources.Load<Texture2D>(resourcePath);

        if (artwork == null)
        {
            Sprite artworkSprite =
                Resources.Load<Sprite>(resourcePath);
            artwork = artworkSprite != null
                ? artworkSprite.texture
                : null;
        }

        string title = CreateTitle(profile, phase);
        string continueLabel =
            CreateContinueLabel(stageNumber, phase);

        return new ComicPage(
            artwork,
            phase,
            title,
            continueLabel,
            CreateTextBlocksForPhase(stageNumber, phase)
        );
    }

    private static string GetResourcePath(
        StageProfile profile,
        ComicPhase phase
    )
    {
        if (phase == ComicPhase.Prologue)
        {
            return "Comics/Prologue";
        }

        if (profile == null)
        {
            return string.Empty;
        }

        return
            "Comics/Stage" +
            profile.StageNumber.ToString("00") +
            "_" +
            phase;
    }

    private static string CreateTitle(
        StageProfile profile,
        ComicPhase phase
    )
    {
        if (phase == ComicPhase.Prologue)
        {
            return "프롤로그 · 사표를 든 이유";
        }

        if (phase == ComicPhase.Defeat)
        {
            return profile.StageNumber + "장 · 퇴사 실패";
        }

        if (
            phase == ComicPhase.Victory &&
            profile.StageNumber ==
            StageProfileCatalog.LastStageNumber
        )
        {
            return "마지막 장";
        }

        return
            profile.StageNumber + "장 · " + profile.BossName;
    }

    private static string CreateContinueLabel(
        int stageNumber,
        ComicPhase phase
    )
    {
        switch (phase)
        {
            case ComicPhase.Prologue:
                return "사표 들고 출근하기";
            case ComicPhase.Intro:
                return "전투 시작";
            case ComicPhase.Defeat:
                return "결과 보기";
            default:
                return
                    stageNumber ==
                    StageProfileCatalog.LastStageNumber
                        ? "엔딩 마치기"
                        : "다음 스테이지";
        }
    }

    private static ComicTextBlock[] CreateTextBlocksForPhase(
        int stageNumber,
        ComicPhase phase
    )
    {
        if (phase == ComicPhase.Prologue)
        {
            return CreatePrologueTextBlocks();
        }

        if (phase == ComicPhase.Defeat)
        {
            return CreateDefeatTextBlocks(stageNumber);
        }

        return CreateTextBlocks(stageNumber, phase);
    }

    private static ComicTextBlock[] CreatePrologueTextBlocks()
    {
        return new[]
        {
            Text(
                "일은 끝나지 않았다.\n나만 남은 사무실.",
                .035f, .035f, .17f, .085f, 19
            ),
            Text(
                "휴가는 반려.\n야근 지시는\n계속됐다.",
                .685f, .045f, .115f, .105f, 16
            ),
            Text(
                "새벽 5시 퇴근",
                .305f, .525f, .095f, .055f, 17
            ),
            Text(
                "아침 6시 출근",
                .448f, .525f, .085f, .055f, 16
            ),
            Text(
                "오늘은 반드시\n그만둔다.",
                .82f, .50f, .15f, .13f, 22
            )
        };
    }

    private static ComicTextBlock[] CreateDefeatTextBlocks(
        int stageNumber
    )
    {
        Rect speechRect = GetDefeatSpeechRect(stageNumber);
        Rect captionRect = GetDefeatCaptionRect(stageNumber);
        Rect leftNarrationRect =
            GetDefeatLeftNarrationRect(stageNumber);
        Rect rightNarrationRect =
            GetDefeatRightNarrationRect(stageNumber);
        string bosses = GetCumulativeBossNames(stageNumber);
        int speechSize = stageNumber >= 5 ? 16 : 19;

        return new[]
        {
            Text(
                "새벽 5시 퇴근",
                leftNarrationRect.x,
                leftNarrationRect.y,
                leftNarrationRect.width,
                leftNarrationRect.height,
                17
            ),
            Text(
                "아침 6시 출근",
                rightNarrationRect.x,
                rightNarrationRect.y,
                rightNarrationRect.width,
                rightNarrationRect.height,
                17
            ),
            Text(
                bosses + "\n“넌 안 돼! 어딜 도망가!”",
                speechRect.x,
                speechRect.y,
                speechRect.width,
                speechRect.height,
                speechSize
            ),
            Text(
                "다시 출근이다.",
                captionRect.x,
                captionRect.y,
                captionRect.width,
                captionRect.height,
                17
            )
        };
    }

    private static Rect GetDefeatSpeechRect(int stageNumber)
    {
        if (stageNumber == 1)
        {
            return new Rect(.675f, .11f, .105f, .10f);
        }

        if (stageNumber == 4)
        {
            return new Rect(.30f, .03f, .43f, .11f);
        }

        return new Rect(.27f, .035f, .46f, .12f);
    }

    private static Rect GetDefeatCaptionRect(int stageNumber)
    {
        switch (stageNumber)
        {
            case 1:
                return new Rect(.17f, .63f, .07f, .05f);
            case 2:
                return new Rect(.335f, .56f, .05f, .04f);
            case 3:
                return new Rect(.65f, .625f, .11f, .06f);
            case 4:
                return new Rect(.61f, .64f, .08f, .06f);
            case 5:
                return new Rect(.61f, .60f, .10f, .06f);
            case 6:
                return new Rect(.58f, .69f, .09f, .05f);
            default:
                return new Rect(.55f, .63f, .05f, .035f);
        }
    }

    private static Rect GetDefeatLeftNarrationRect(
        int stageNumber
    )
    {
        return stageNumber == 1
            ? new Rect(.04f, .06f, .125f, .08f)
            : new Rect(.035f, .045f, .13f, .07f);
    }

    private static Rect GetDefeatRightNarrationRect(
        int stageNumber
    )
    {
        return stageNumber == 1
            ? new Rect(.845f, .10f, .12f, .08f)
            : new Rect(.835f, .045f, .13f, .07f);
    }

    private static string GetCumulativeBossNames(int stageNumber)
    {
        string[] bossNames =
        {
            "주임",
            "대리",
            "과장",
            "차장",
            "부장",
            "부사장",
            "대표"
        };
        int count = Mathf.Clamp(
            stageNumber,
            1,
            bossNames.Length
        );

        return string.Join(" · ", bossNames, 0, count);
    }

    private static ComicTextBlock[] CreateTextBlocks(
        int stageNumber,
        ComicPhase phase
    )
    {
        switch (stageNumber)
        {
            case 1:
                return phase == ComicPhase.Intro
                    ? new[]
                    {
                        Text("사표라고?\n갑자기 왜?", .225f, .055f, .115f, .105f, 19),
                        Text("조금만 더 버티면\n편해질 거야.", .875f, .085f, .095f, .10f, 17),
                        Text("저는 이미\n충분히 버텼습니다.", .035f, .555f, .095f, .105f, 17),
                        Text("그 말을 벌써\n3년째 들었습니다.", .685f, .535f, .105f, .105f, 17)
                    }
                    : VictoryTexts("주임전은 끝났다.", "다음은 대리다!", .07f, .06f, .18f, .12f, .70f, .69f, .17f, .12f);

            case 2:
                return phase == ComicPhase.Intro
                    ? new[]
                    {
                        Text("네가 나가서\n어디 갈 수 있을 것 같아?", .09f, .075f, .22f, .16f, 19),
                        Text("밖은 여기보다\n훨씬 무서워.", .76f, .17f, .16f, .13f, 18),
                        Text("적어도 여기는\n아니겠죠.", .065f, .60f, .16f, .13f, 18)
                    }
                    : new[]
                    {
                        Text("으윽…", .09f, .405f, .045f, .055f, 14),
                        Text("다음은 과장이다!", .78f, .125f, .15f, .09f, 19)
                    };

            case 3:
                return phase == ComicPhase.Intro
                    ? new[]
                    {
                        Text("끝나고\n얘기하자.", .18f, .115f, .075f, .055f, 14),
                        Text("계속하면\n언젠간 끝나!", .46f, .115f, .075f, .055f, 14),
                        Text("그게 대체\n언제입니까?", .15f, .475f, .07f, .05f, 14),
                        Text("조금만 더\n하면 돼.", .50f, .49f, .075f, .05f, 14)
                    }
                    : VictoryTexts("과장전은 끝났다.", "다음은 차장이다!", .64f, .05f, .16f, .11f, .77f, .65f, .14f, .11f);

            case 4:
                return phase == ComicPhase.Intro
                    ? new[]
                    {
                        Text("퇴사는 팀을\n버리는 거야.", .11f, .045f, .13f, .105f, 18),
                        Text("동료들이 고생해도\n상관없다는 거지?", .37f, .055f, .13f, .105f, 17),
                        Text("……", .58f, .095f, .07f, .07f, 20),
                        Text("사람이 부족한 건\n제 잘못이 아닙니다.", .855f, .065f, .105f, .12f, 16)
                    }
                    : VictoryTexts("다음은 부장이다!", "내가… 졌다고?", .25f, .075f, .16f, .13f, .68f, .59f, .13f, .11f);

            case 5:
                return phase == ComicPhase.Intro
                    ? new[]
                    {
                        Text("연봉이 그렇게\n중요해?", .205f, .06f, .13f, .105f, 18),
                        Text("제 인생에는\n중요합니다.", .65f, .055f, .17f, .12f, 18),
                        Text("회사엔 돈보다\n책임감이 중요해!", .235f, .70f, .15f, .13f, 17)
                    }
                    : VictoryTexts("부장전은 끝났다.", "다음은 부사장이다!", .30f, .06f, .15f, .13f, .80f, .50f, .17f, .10f);

            case 6:
                return phase == ComicPhase.Intro
                    ? new[]
                    {
                        Text("지금 나가면 이 업계에서\n버틸 수 있겠나?", .27f, .08f, .145f, .10f, 17),
                        Text("내 전화 한 통이면\n끝이야.", .66f, .17f, .16f, .11f, 18),
                        Text("대표에게도\n못 가게 막겠다.", .20f, .73f, .15f, .10f, 17),
                        Text("전화보다 먼저\n차단하겠습니다.", .58f, .50f, .10f, .08f, 15),
                        Text("인사팀에\n연락해!", .86f, .735f, .11f, .09f, 17)
                    }
                    : new[]
                    {
                        Text("대표님이…!", .13f, .405f, .09f, .075f, 16),
                        Text("이제 마지막,\n대표다!", .79f, .10f, .10f, .11f, 17)
                    };

            case 7:
                return phase == ComicPhase.Intro
                    ? new[]
                    {
                        Text("월급 올려주지!", .18f, .055f, .24f, .13f, 24),
                        Text("설마…?", .21f, .445f, .07f, .055f, 16),
                        Text("0.1%!\n좋지? 가서 일해!", .895f, .445f, .085f, .105f, 15),
                        Text("그 돈은 대표님이\n가지십시오.", .025f, .77f, .11f, .075f, 15),
                        Text("어디 가서\n후회하지 마!", .82f, .77f, .12f, .09f, 16)
                    }
                    : new[]
                    {
                        Text("나는 자유를 찾았다.", .03f, .31f, .20f, .08f, 28),
                        Text("그리고 나의 통장도….", .80f, .50f, .18f, .08f, 26),
                        Text("통장 잔액\n0원", .42f, .70f, .09f, .14f, 24, 10f),
                        Text("퇴사 성공", .03f, .94f, .33f, .04f, 21)
                    };
        }

        return Array.Empty<ComicTextBlock>();
    }

    private static ComicTextBlock Text(
        string text,
        float x,
        float y,
        float width,
        float height,
        int fontSize = 25,
        float rotation = 0f
    )
    {
        return new ComicTextBlock(
            text,
            x,
            y,
            width,
            height,
            fontSize,
            rotation
        );
    }

    private static ComicTextBlock[] VictoryTexts(
        string defeatedText,
        string nextText,
        float firstX,
        float firstY,
        float firstWidth,
        float firstHeight,
        float secondX,
        float secondY,
        float secondWidth,
        float secondHeight
    )
    {
        return new[]
        {
            Text(
                defeatedText,
                firstX,
                firstY,
                firstWidth,
                firstHeight,
                26
            ),
            Text(
                nextText,
                secondX,
                secondY,
                secondWidth,
                secondHeight,
                25
            )
        };
    }
}
