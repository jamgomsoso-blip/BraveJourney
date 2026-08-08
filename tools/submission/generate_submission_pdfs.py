from __future__ import annotations

from pathlib import Path

from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_LEFT
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import mm
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    Flowable,
    Image,
    KeepTogether,
    PageBreak,
    Paragraph,
    SimpleDocTemplate,
    Spacer,
    Table,
    TableStyle,
)


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "output" / "pdf"
COMICS_DIR = ROOT / "Assets" / "BraveJourney" / "Resources" / "Comics"
FONT_DIR = ROOT / "Assets" / "BraveJourney" / "Fonts"

GAME_URL = "https://jamgomsoso-blip.github.io/BraveJourney/"
SOURCE_URL = "https://github.com/jamgomsoso-blip/BraveJourney"
VIDEO_URL = "업로드 후 YouTube 링크 입력"
VERSION_DATE = "2026. 08. 08."

NAVY = colors.HexColor("#0D1726")
INK = colors.HexColor("#152235")
RED = colors.HexColor("#D4383A")
GOLD = colors.HexColor("#E9B44C")
MINT = colors.HexColor("#57C7B6")
ICE = colors.HexColor("#EEF3F7")
PALE_RED = colors.HexColor("#FCEBEC")
PALE_GOLD = colors.HexColor("#FFF4D9")
MID = colors.HexColor("#607083")
WHITE = colors.white


def register_fonts() -> None:
    pdfmetrics.registerFont(
        TTFont("NanumGothic", str(FONT_DIR / "NanumGothic-Regular.ttf"))
    )
    pdfmetrics.registerFont(
        TTFont("SongMyung", str(FONT_DIR / "SongMyung-Regular.ttf"))
    )


def make_styles():
    base = getSampleStyleSheet()
    return {
        "body": ParagraphStyle(
            "BodyKR",
            parent=base["BodyText"],
            fontName="NanumGothic",
            fontSize=9.4,
            leading=15,
            textColor=INK,
            spaceAfter=3 * mm,
        ),
        "small": ParagraphStyle(
            "SmallKR",
            parent=base["BodyText"],
            fontName="NanumGothic",
            fontSize=7.8,
            leading=11.5,
            textColor=MID,
        ),
        "h1": ParagraphStyle(
            "H1KR",
            parent=base["Heading1"],
            fontName="SongMyung",
            fontSize=24,
            leading=30,
            textColor=NAVY,
            spaceAfter=5 * mm,
        ),
        "h2": ParagraphStyle(
            "H2KR",
            parent=base["Heading2"],
            fontName="NanumGothic",
            fontSize=13,
            leading=18,
            textColor=RED,
            spaceBefore=3 * mm,
            spaceAfter=2.5 * mm,
        ),
        "h3": ParagraphStyle(
            "H3KR",
            parent=base["Heading3"],
            fontName="NanumGothic",
            fontSize=10.5,
            leading=15,
            textColor=NAVY,
            spaceBefore=2 * mm,
            spaceAfter=1.5 * mm,
        ),
        "table": ParagraphStyle(
            "TableKR",
            parent=base["BodyText"],
            fontName="NanumGothic",
            fontSize=8.2,
            leading=12,
            textColor=INK,
        ),
        "table_white": ParagraphStyle(
            "TableWhiteKR",
            parent=base["BodyText"],
            fontName="NanumGothic",
            fontSize=8.2,
            leading=12,
            textColor=WHITE,
            alignment=TA_CENTER,
        ),
        "callout": ParagraphStyle(
            "CalloutKR",
            parent=base["BodyText"],
            fontName="NanumGothic",
            fontSize=10.2,
            leading=16,
            textColor=NAVY,
            alignment=TA_LEFT,
        ),
        "center": ParagraphStyle(
            "CenterKR",
            parent=base["BodyText"],
            fontName="NanumGothic",
            fontSize=9,
            leading=14,
            textColor=INK,
            alignment=TA_CENTER,
        ),
    }


class CoverPage(Flowable):
    def __init__(self, image_path: Path, eyebrow: str, title: str, subtitle: str, label: str):
        super().__init__()
        self.image_path = image_path
        self.eyebrow = eyebrow
        self.title = title
        self.subtitle = subtitle
        self.label = label
        self.width, self.height = A4

    def wrap(self, avail_width, avail_height):
        return avail_width, avail_height

    def draw(self):
        c = self.canv
        page_w, page_h = A4
        c.saveState()
        c.setFillColor(NAVY)
        c.rect(0, 0, page_w, page_h, fill=1, stroke=0)

        image_h = 128 * mm
        c.drawImage(
            str(self.image_path),
            0,
            page_h - image_h,
            width=page_w,
            height=image_h,
            preserveAspectRatio=False,
            mask="auto",
        )
        c.setFillColor(colors.Color(0.02, 0.04, 0.08, alpha=0.38))
        c.rect(0, page_h - image_h, page_w, image_h, fill=1, stroke=0)
        c.setFillColor(RED)
        c.rect(16 * mm, page_h - image_h - 5 * mm, 62 * mm, 10 * mm, fill=1, stroke=0)
        c.setFont("NanumGothic", 9)
        c.setFillColor(WHITE)
        c.drawCentredString(47 * mm, page_h - image_h - 1.8 * mm, self.label)

        y = page_h - image_h - 29 * mm
        c.setFillColor(MINT)
        c.setFont("NanumGothic", 10)
        c.drawString(18 * mm, y, self.eyebrow)
        c.setFillColor(WHITE)
        c.setFont("SongMyung", 33)
        c.drawString(18 * mm, y - 20 * mm, self.title)
        c.setFillColor(colors.HexColor("#DDE7F0"))
        c.setFont("NanumGothic", 12)
        c.drawString(18 * mm, y - 32 * mm, self.subtitle)

        c.setStrokeColor(colors.HexColor("#34445A"))
        c.line(18 * mm, 23 * mm, page_w - 18 * mm, 23 * mm)
        c.setFillColor(colors.HexColor("#A9B7C8"))
        c.setFont("NanumGothic", 8.5)
        c.drawString(18 * mm, 16 * mm, f"BraveJourney · 제출용 문서 · {VERSION_DATE}")
        c.restoreState()


def header_footer(canvas, doc):
    if doc.page == 1:
        return
    canvas.saveState()
    page_w, page_h = A4
    canvas.setStrokeColor(colors.HexColor("#D8E0E8"))
    canvas.line(18 * mm, page_h - 13 * mm, page_w - 18 * mm, page_h - 13 * mm)
    canvas.setFont("NanumGothic", 7.5)
    canvas.setFillColor(MID)
    canvas.drawString(18 * mm, page_h - 10 * mm, "BRAVEJOURNEY")
    canvas.drawRightString(page_w - 18 * mm, 10 * mm, str(doc.page))
    canvas.restoreState()


def p(text: str, style):
    return Paragraph(text, style)


def section_title(number: str, title: str, styles):
    return KeepTogether(
        [
            p(f"<font color='#D4383A'>{number}</font>  {title}", styles["h1"]),
            Table([[""]], colWidths=[18 * mm], rowHeights=[1.2 * mm], style=[("BACKGROUND", (0, 0), (-1, -1), RED)]),
            Spacer(1, 4 * mm),
        ]
    )


def info_table(rows, styles, widths=(40 * mm, 125 * mm)):
    data = []
    for key, value in rows:
        data.append([p(f"<b>{key}</b>", styles["table"]), p(value, styles["table"])])
    table = Table(data, colWidths=list(widths), hAlign="LEFT")
    table.setStyle(
        TableStyle(
            [
                ("BACKGROUND", (0, 0), (0, -1), ICE),
                ("GRID", (0, 0), (-1, -1), 0.45, colors.HexColor("#CED7E0")),
                ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
                ("LEFTPADDING", (0, 0), (-1, -1), 3 * mm),
                ("RIGHTPADDING", (0, 0), (-1, -1), 3 * mm),
                ("TOPPADDING", (0, 0), (-1, -1), 2.4 * mm),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 2.4 * mm),
            ]
        )
    )
    return table


def data_table(headers, rows, styles, widths):
    data = [[p(f"<b>{h}</b>", styles["table_white"]) for h in headers]]
    for row in rows:
        data.append([p(str(cell), styles["table"]) for cell in row])
    table = Table(data, colWidths=widths, repeatRows=1, hAlign="LEFT")
    table.setStyle(
        TableStyle(
            [
                ("BACKGROUND", (0, 0), (-1, 0), NAVY),
                ("ROWBACKGROUNDS", (0, 1), (-1, -1), [WHITE, ICE]),
                ("GRID", (0, 0), (-1, -1), 0.4, colors.HexColor("#CDD6DF")),
                ("VALIGN", (0, 0), (-1, -1), "TOP"),
                ("LEFTPADDING", (0, 0), (-1, -1), 2.5 * mm),
                ("RIGHTPADDING", (0, 0), (-1, -1), 2.5 * mm),
                ("TOPPADDING", (0, 0), (-1, -1), 2.2 * mm),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 2.2 * mm),
            ]
        )
    )
    return table


def callout(text: str, styles, color=PALE_GOLD, accent=GOLD):
    table = Table([[p(text, styles["callout"])]], colWidths=[165 * mm])
    table.setStyle(
        TableStyle(
            [
                ("BACKGROUND", (0, 0), (-1, -1), color),
                ("BOX", (0, 0), (-1, -1), 0.6, accent),
                ("LINEBEFORE", (0, 0), (0, -1), 4, accent),
                ("LEFTPADDING", (0, 0), (-1, -1), 5 * mm),
                ("RIGHTPADDING", (0, 0), (-1, -1), 5 * mm),
                ("TOPPADDING", (0, 0), (-1, -1), 4 * mm),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 4 * mm),
            ]
        )
    )
    return table


def wide_image(path: Path, height=82 * mm):
    return Image(str(path), width=165 * mm, height=height)


def build_game_guide(styles) -> Path:
    out = OUTPUT_DIR / "BraveJourney_Game_Guide.pdf"
    doc = SimpleDocTemplate(
        str(out),
        pagesize=A4,
        rightMargin=22 * mm,
        leftMargin=22 * mm,
        topMargin=20 * mm,
        bottomMargin=18 * mm,
        title="BraveJourney 게임 소개 및 설명",
        author="BraveJourney",
    )

    story = [
        CoverPage(
            COMICS_DIR / "Prologue.png",
            "GAME INTRODUCTION & PLAY GUIDE",
            "BraveJourney",
            "퇴사를 위해 직급 보스를 돌파하는 2D 오피스 액션 러너",
            "게임 소개 및 설명 문서",
        ),
        PageBreak(),
        section_title("01", "게임 개요", styles),
        info_table(
            [
                ("게임 제목", "BraveJourney"),
                ("한 줄 소개", "사표 한 장을 들고 사무실 장애물과 직급 보스를 돌파하는 2D 액션 러너"),
                ("장르", "2D 횡스크롤 액션 러너 · 패링 중심 보스전"),
                ("플랫폼", "WebGL · 데스크톱 웹 브라우저"),
                ("개발 환경", "Unity 6 (6000.3.16f1)"),
                ("플레이 구성", "Stage01~Stage07 · 러닝 구간 약 30~35초 + 직급별 보스전"),
            ],
            styles,
        ),
        Spacer(1, 5 * mm),
        p("<b>이야기</b>", styles["h2"]),
        p(
            "끝나지 않는 야근과 반복되는 업무에 지친 주인공은 마침내 사표를 듭니다. "
            "그러나 퇴사까지 가는 길에는 주임, 대리, 과장, 차장, 부장, 부사장, 대표가 차례로 버티고 있습니다. "
            "주인공은 사무실을 달리고, 쏟아지는 업무를 패링하고, 각 보스를 넘어 자유를 찾아야 합니다.",
            styles["body"],
        ),
        p("<b>핵심 플레이 루프</b>", styles["h2"]),
        data_table(
            ["1. 질주", "2. 회피", "3. 패링", "4. 반격", "5. 다음 직급"],
            [[
                "자동으로 전진",
                "점프·2단 점프·슬라이드",
                "투사체를 반사",
                "스턴 중 근접 공격",
                "컷신 후 다음 스테이지",
            ]],
            styles,
            [33 * mm] * 5,
        ),
        Spacer(1, 4 * mm),
        p("<b>스테이지 순서</b>", styles["h2"]),
        callout(
            "Stage01 주임 → Stage02 대리 → Stage03 과장 → Stage04 차장 → "
            "Stage05 부장 → Stage06 부사장 → Stage07 대표",
            styles,
            color=ICE,
            accent=MINT,
        ),
        PageBreak(),
        section_title("02", "게임 방법", styles),
        p("<b>러닝 구간</b>", styles["h2"]),
        data_table(
            ["키", "기능", "사용 방법"],
            [
                ("W", "점프 / 2단 점프", "공중에서 한 번 더 누르면 2단 점프"),
                ("E", "슬라이드", "누르고 있는 동안 낮은 자세로 장애물 통과"),
                ("Space", "패링", "짧은 판정 시간에 공격을 맞혀 반사"),
            ],
            styles,
            [27 * mm, 45 * mm, 93 * mm],
        ),
        Spacer(1, 5 * mm),
        p("<b>보스전</b>", styles["h2"]),
        data_table(
            ["키", "기능", "사용 방법"],
            [
                ("← / →", "좌우 이동", "보스와 거리를 조절하고 공격 위치 확보"),
                ("W / ↑", "점프 / 2단 점프", "바닥·낙하 공격 회피"),
                ("E / ↓", "슬라이드", "낮은 공격 회피"),
                ("Space", "패링", "보스 투사체를 되돌려 보스를 스턴"),
                ("A", "KickC 근접 공격", "스턴된 보스 가까이에서 사용"),
                ("R", "다시 시작", "패배 또는 최종 클리어 화면에서 Stage01부터 재시작"),
            ],
            styles,
            [27 * mm, 45 * mm, 93 * mm],
        ),
        Spacer(1, 5 * mm),
        callout(
            "<b>보스 공략 핵심</b><br/>투사체가 가까이 왔을 때 Space로 패링 → 반사 투사체 명중으로 보스 스턴 → "
            "보스 가까이 접근 → A로 공격. 한 번의 스턴에는 피해가 한 번만 적용되므로 다시 패링해야 합니다.",
            styles,
            color=PALE_RED,
            accent=RED,
        ),
        Spacer(1, 5 * mm),
        p("<b>목표·종료 조건</b>", styles["h2"]),
        info_table(
            [
                ("목표", "장애물을 피하고 모든 직급 보스를 처치하여 Stage07을 완료"),
                ("실패", "플레이어 하트가 모두 소진되면 패배 컷신 표시"),
                ("성공", "대표 보스 처치 후 최종 승리 컷신과 엔딩 표시"),
                ("재도전", "패배/엔딩 화면에서 R 키로 처음부터 다시 시작"),
            ],
            styles,
        ),
        PageBreak(),
        section_title("03", "실행 방법 및 제출 링크", styles),
        p("<b>웹에서 실행</b>", styles["h2"]),
        info_table(
            [
                ("1", f"Chrome 또는 Edge에서 <link href='{GAME_URL}' color='#D4383A'>{GAME_URL}</link> 접속"),
                ("2", "첫 로딩이 끝날 때까지 기다린 뒤 게임 화면을 한 번 클릭"),
                ("3", "키보드로 조작하며, 사운드 확인을 위해 브라우저 음소거를 해제"),
                ("권장 환경", "PC · 최신 Chrome/Edge · 1280×720 이상 · 키보드 · 안정적인 네트워크"),
            ],
            styles,
        ),
        Spacer(1, 6 * mm),
        p("<b>제출 링크</b>", styles["h2"]),
        info_table(
            [
                ("플레이 링크", f"<link href='{GAME_URL}' color='#D4383A'>{GAME_URL}</link>"),
                ("전체 소스", f"<link href='{SOURCE_URL}' color='#D4383A'>{SOURCE_URL}</link>"),
                ("플레이 영상", f"<font color='#9A6700'>{VIDEO_URL}</font>"),
            ],
            styles,
        ),
        Spacer(1, 6 * mm),
        callout(
            "<b>영상 링크 반영 안내</b><br/>30~60초 실제 플레이 영상을 YouTube 공개 또는 일부 공개로 업로드한 뒤, "
            "위 ‘플레이 영상’ 항목을 최종 URL로 교체합니다.",
            styles,
        ),
        Spacer(1, 8 * mm),
        wide_image(COMICS_DIR / "Stage07_Victory.png", 93 * mm),
        Spacer(1, 2 * mm),
        p("Stage07 대표전 승리 컷신 이미지", styles["small"]),
        PageBreak(),
        section_title("04", "심사자용 빠른 확인", styles),
        data_table(
            ["확인 항목", "체크 포인트"],
            [
                ("브라우저 실행", "별도 설치 없이 URL에서 로딩되고 Stage01 프롤로그가 표시되는가"),
                ("러닝", "자동 전진 중 W 2단 점프와 E 슬라이드가 정상 동작하는가"),
                ("보스 진입", "인트로 컷신 후 보스전 UI·직급명·체력바가 표시되는가"),
                ("패링", "Space로 투사체를 반사하면 보스가 스턴되고 공격을 멈추는가"),
                ("근접 피해", "스턴 중 가까이에서 A 공격 시 1회만 피해가 적용되는가"),
                ("전환", "승리 컷신 후 다음 스테이지로 이동하는가"),
                ("패배", "하트 소진 시 패배 컷신이 유지되고 R로 재시작하는가"),
            ],
            styles,
            [49 * mm, 116 * mm],
        ),
        Spacer(1, 7 * mm),
        callout(
            "첫 접속은 WebGL 데이터 다운로드로 시간이 걸릴 수 있습니다. 로딩 중 탭을 닫지 말고, "
            "키 입력이 반응하지 않으면 게임 화면을 한 번 클릭해 포커스를 맞춰 주세요.",
            styles,
            color=ICE,
            accent=MINT,
        ),
    ]
    doc.build(story, onFirstPage=header_footer, onLaterPages=header_footer)
    return out


def build_ai_report(styles) -> Path:
    out = OUTPUT_DIR / "BraveJourney_AI_Usage_Report.pdf"
    doc = SimpleDocTemplate(
        str(out),
        pagesize=A4,
        rightMargin=22 * mm,
        leftMargin=22 * mm,
        topMargin=20 * mm,
        bottomMargin=18 * mm,
        title="BraveJourney AI 활용 기술 문서",
        author="BraveJourney",
    )

    story = [
        CoverPage(
            COMICS_DIR / "Stage04_Intro.png",
            "AI UTILIZATION TECHNICAL REPORT",
            "BraveJourney",
            "AI 협업 범위 · 주요 프롬프트 · 구현 구조 · 에셋 출처",
            "AI 활용 기술 문서",
        ),
        PageBreak(),
        section_title("01", "AI 활용 개요", styles),
        p(
            "BraveJourney는 1인 개발자가 게임의 방향과 최종 의사결정을 담당하고, AI를 기획 보조·코드 수정·"
            "2D 비주얼 제작·문서화·검증에 활용한 Unity WebGL 프로젝트입니다. AI가 프로젝트를 독립적으로 완성한 것이 아니라, "
            "개발자가 반복적으로 플레이하고 문제를 지적하면 기존 구조를 확인한 뒤 제한된 범위에서 수정하는 협업 방식으로 진행했습니다.",
            styles["body"],
        ),
        p("<b>사용 도구</b>", styles["h2"]),
        data_table(
            ["도구", "활용 영역", "산출물"],
            [
                ("ChatGPT / Codex", "요구사항 정리, Unity C# 분석·수정, 오류 진단, 빌드·브라우저 검증", "게임 로직, UI, 컷신, WebGL 빌드, 문서"),
                ("OpenAI 이미지 생성", "오피스 배경·장애물·보스 캐릭터·컷만화 시안과 변형 제작", "2D PNG 이미지 및 컷신 베이스"),
                ("로컬 개발 도구", "Unity Editor, C# 컴파일, Git, 브라우저 런타임 점검", "실행 가능한 프로젝트와 검증 기록"),
            ],
            styles,
            [43 * mm, 68 * mm, 54 * mm],
        ),
        Spacer(1, 6 * mm),
        p("<b>협업 원칙</b>", styles["h2"]),
        info_table(
            [
                ("구조 우선", "새 스크립트를 무조건 추가하지 않고 기존 PlayerController, PlayerPunch, BossHealth 구조를 먼저 확인"),
                ("사람의 판단", "플레이 감각, 난이도, 대사 맥락, 이미지 선택, 최종 수정 방향은 개발자가 결정"),
                ("반복 검수", "게임 화면과 이미지 원본을 대조하고 글자 넘침·애니메이션 흔들림·오브젝트 크기를 반복 수정"),
                ("검증", "C# 컴파일, Unity WebGL 빌드, 브라우저 로딩 및 한글 표시를 단계별 확인"),
            ],
            styles,
        ),
        PageBreak(),
        section_title("02", "주요 프롬프트 및 지시 사항", styles),
        p(
            "아래는 개발 과정에서 AI에 전달한 핵심 지시를 기능별로 정리한 것입니다. 실제 작업에서는 각 결과를 플레이한 뒤 "
            "문제 화면과 구체적인 수정 의견을 추가해 반복했습니다.",
            styles["body"],
        ),
        data_table(
            ["대상", "대표 지시", "AI 수행 범위"],
            [
                ("전투 구조", "“기존 코드를 먼저 확인하고 새 이동·공격 스크립트를 만들지 말 것. Parry Duration은 0.2초 유지.”", "PlayerController/PlayerPunch/BossHealth/Projectile 연결 분석과 최소 수정"),
                ("보스전", "“반사 투사체 명중 시 보스 스턴, 스턴 중 공격 정지, 가까이에서 한 번만 피해.”", "스턴 상태·발사 중지·스턴 회차별 피해 제한 구현"),
                ("오피스 아트", "“픽셀 배경 대신 2D 사무실 이미지, 책상·의자·복사기·프린터 장애물.”", "배경과 장애물 2D 시안 생성 및 Unity 리소스 배치"),
                ("직급 캐릭터", "“주임·대리·과장·차장·부장·부사장·대표를 같은 그림체로 각각 제작.”", "직급별 외형 변형과 보스 애니메이션 프레임 제작"),
                ("플레이어 애니메이션", "“기존 에셋을 활용해 2단 점프·공격·달리기를 부드럽게. 팔다리 교차와 크기 고정.”", "기존 스프라이트 분석, 프레임 정리, 방향 반전·이펙트 보정"),
                ("컷신", "“보스 전·승리·패배 컷만화를 직급마다 다른 이야기로 제작하고 대사 맥락을 맞출 것.”", "22종 컷신 이미지·오버레이 대사·진행 및 재시작 로직"),
            ],
            styles,
            [34 * mm, 78 * mm, 53 * mm],
        ),
        Spacer(1, 6 * mm),
        callout(
            "프롬프트에는 ‘현재 구조 확인’, ‘수정하지 말아야 할 값’, ‘화면에서 확인할 결과’, ‘실패 조건’을 함께 넣어 "
            "AI가 기존 게임 흐름과 다른 방향으로 확장하지 않도록 제한했습니다.",
            styles,
            color=ICE,
            accent=MINT,
        ),
        PageBreak(),
        section_title("03", "AI 지원 구현 구조", styles),
        p("<b>게임 흐름과 주요 코드</b>", styles["h2"]),
        data_table(
            ["흐름", "주요 클래스", "역할"],
            [
                ("스테이지 설정", "StageProfileCatalog", "Stage01~07의 러닝 시간, 장애물, 직급, 보스 공격 패턴을 데이터화"),
                ("러닝", "StageCourseBuilder / StageHazard", "오피스 장애물과 바닥·낙하 공격 배치 및 경고"),
                ("플레이어", "PlayerController / PlayerPunch / PlayerHealth", "자동 달리기, 2단 점프, 슬라이드, 패링, KickC, 하트·패배 처리"),
                ("보스", "BossStartTrigger / BossHealth / BossShooter", "보스전 진입, 체력·스턴, 투사체 발사 및 스턴 중 정지"),
                ("투사체", "Projectile / ParryHitbox", "패링 감지, 반사 방향 변경, 보스 명중 전달"),
                ("연출", "BossComicCutscene / StageTransition", "프롤로그, 직급별 인트로·승리·패배, 다음 스테이지 전환"),
                ("표현", "PlayerVisualAnimator / BossVisualAnimator / OfficeHudTheme", "2D 프레임 애니메이션, 보스 대기 모션, UI 테마"),
                ("사운드", "GameAudioFeedback", "러닝·보스 BGM 전환과 주요 행동 효과음"),
            ],
            styles,
            [30 * mm, 65 * mm, 70 * mm],
        ),
        Spacer(1, 6 * mm),
        p("<b>패링 보스전 상태 흐름</b>", styles["h2"]),
        callout(
            "보스 발사 → 투사체 접근 → Space 입력(패링 0.2초) → 반사 투사체가 보스 명중 → "
            "BossHealth 스턴 시작 → BossShooter·보스 공격 중지 → 플레이어 근접 접근 → A(KickC) → "
            "해당 스턴 회차의 피해 1회 기록 → 스턴 종료 후 다음 패링",
            styles,
            color=PALE_RED,
            accent=RED,
        ),
        Spacer(1, 6 * mm),
        p("<b>기존 구조 재사용 사례</b>", styles["h2"]),
        p(
            "애니메이터 전환선에 의존하기보다 이미 존재하는 상태 이름(Sprint, Idle, Run, RunToIdle, PunchC, KickC)을 재생하는 흐름을 유지했습니다. "
            "공격 피해는 PlayerPunch에서 보스의 IsStunned 상태와 거리 판정을 확인하고, BossHealth가 스턴 회차별 1회 제한을 최종 보증하도록 역할을 분리했습니다.",
            styles["body"],
        ),
        PageBreak(),
        section_title("04", "AI 이미지 활용과 사람의 수정", styles),
        wide_image(COMICS_DIR / "Stage01_Intro.png", 93 * mm),
        Spacer(1, 3 * mm),
        p("AI 생성 기반 Stage01 주임전 인트로 컷신 베이스. 실제 대사는 Unity 오버레이로 표시됩니다.", styles["small"]),
        Spacer(1, 5 * mm),
        data_table(
            ["영역", "AI 활용", "개발자 검수·수정"],
            [
                ("배경/장애물", "오피스 분위기의 2D 이미지 시안 생성", "픽셀 크기·눈의 피로·장애물 가독성을 플레이 화면에서 재평가"),
                ("보스 캐릭터", "동일 화풍의 7개 직급별 외형 변형", "직급 구분, 방향, 크기, 바닥 접지, Idle 흔들림 수정"),
                ("플레이어", "기존 캐릭터를 참고한 동작 프레임 보조", "팔다리 교차, 높이 변화, 슬라이드 자세, 좌우 반전 검수"),
                ("컷신", "프롤로그·보스 전·승리·패배 만화 이미지", "말풍선 대상과 대사 맥락, 텍스트 넘침, 패배 UI 위치를 이미지별 대조"),
                ("UI", "오피스 만화풍 하트·체력바·말풍선 방향 제안", "실제 WebGL 해상도에서 크기와 보스 잘림 확인"),
            ],
            styles,
            [35 * mm, 62 * mm, 68 * mm],
        ),
        PageBreak(),
        section_title("05", "검증 및 품질 관리", styles),
        data_table(
            ["검증 단계", "확인 내용", "결과"],
            [
                ("C# 컴파일", "Assembly-CSharp 프로젝트 빌드", "경고 0, 오류 0"),
                ("Unity WebGL", "Stage01~Stage07 포함 전체 브라우저 빌드", "성공 · 오류 0"),
                ("브라우저 실행", "로컬 서버에서 WebGL 로딩, 960×600 표시, 한글 컷신 확인", "정상 실행"),
                ("전투 규칙", "패링 0.2초, 스턴 중 발사 정지, 근접·스턴 회차 1회 피해", "코드 경로 확인"),
                ("시각 검수", "말풍선 넘침, 캐릭터 방향·접지, 애니메이션 크기 변화", "피드백 기반 반복 수정"),
            ],
            styles,
            [40 * mm, 88 * mm, 37 * mm],
        ),
        Spacer(1, 7 * mm),
        p("<b>AI 결과를 그대로 사용하지 않은 사례</b>", styles["h2"]),
        p(
            "초기 투사체 텍스트는 화면을 가려 패링 타이밍을 읽기 어려웠고, 픽셀 배경과 장애물은 WebGL 화면에서 작고 눈이 피로했습니다. "
            "개발자 피드백으로 투사체와 대사를 분리하고, 배경·바닥·장애물을 2D 이미지로 바꾸며 UI를 확대했습니다. "
            "캐릭터 애니메이션도 AI 생성 프레임을 그대로 채택하지 않고 발·팔 교차, 좌우 반전, 키 변화, Idle 중심축을 반복 보정했습니다.",
            styles["body"],
        ),
        p("<b>한계와 대응</b>", styles["h2"]),
        info_table(
            [
                ("이미지 일관성", "프레임별 미세한 형태 변화 가능 → 기준 프레임과 크기·중심축을 비교"),
                ("문맥 오류", "말풍선 대상과 대사가 어긋날 수 있음 → 장면별 화자·승패 상태를 코드로 명시"),
                ("자동 수정 위험", "기존 시스템을 우회할 수 있음 → 관련 코드와 Animator 상태를 먼저 조사하도록 지시"),
                ("라이선스", "외부 음악·에셋은 AI가 권리를 보증하지 않음 → 원 출처와 라이선스를 별도 기록·확인"),
            ],
            styles,
        ),
        PageBreak(),
        section_title("06", "외부 에셋 및 오픈소스 출처", styles),
        data_table(
            ["항목", "사용 내용", "출처·라이선스"],
            [
                (
                    "Pixel Prototype Player Sprites",
                    "초기 플레이어 스프라이트와 이동·전투 애니메이션 레퍼런스",
                    "Dead Revolver · CC0 1.0 / 수정·상업적 사용 가능<br/><link href='https://deadrevolver.itch.io/pixel-prototype-player-sprites' color='#D4383A'>deadrevolver.itch.io/pixel-prototype-player-sprites</link>",
                ),
                (
                    "NanumGothic",
                    "게임 UI와 문서 본문 한글 폰트",
                    "NHN Corporation · SIL Open Font License 1.1<br/>Assets/BraveJourney/Fonts/OFL.txt 포함",
                ),
                (
                    "SongMyung",
                    "만화풍 제목 및 강조 한글 폰트",
                    "The SongMyung Project Authors · SIL Open Font License 1.1<br/>Assets/BraveJourney/Fonts/SongMyung-OFL.txt 포함",
                ),
                (
                    "Conspiracy Theory",
                    "러닝 구간 BGM",
                    "Rod Kim · YouTube Audio Library Soundtrack<br/><link href='https://rodkim.bandcamp.com/album/lets-play-youtube-audio-library-soundtrack' color='#D4383A'>Rod Kim — Let’s Play!</link>",
                ),
                (
                    "Final Boss Battle",
                    "보스전 BGM",
                    "Rod Kim · YouTube Audio Library Soundtrack<br/><link href='https://rodkim.bandcamp.com/track/final-boss-battle' color='#D4383A'>Rod Kim — Final Boss Battle</link>",
                ),
                (
                    "Unity Packages",
                    "URP, 2D Animation, SpriteShape, Input System, uGUI 등",
                    "Unity Package Manager 공식 패키지 · 버전은 Packages/manifest.json 및 packages-lock.json에 고정",
                ),
                (
                    "Unity .gitignore",
                    "생성 폴더·캐시·빌드 산출물 제외 규칙",
                    "github/gitignore Unity template · CC0 1.0<br/><link href='https://github.com/github/gitignore/blob/main/Unity.gitignore' color='#D4383A'>github.com/github/gitignore</link>",
                ),
            ],
            styles,
            [39 * mm, 58 * mm, 68 * mm],
        ),
        Spacer(1, 6 * mm),
        callout(
            "<b>음악 라이선스</b><br/>Conspiracy Theory와 Final Boss Battle은 YouTube Studio 오디오 보관함에서 "
            "제공되는 YouTube 오디오 보관함 라이선스 트랙입니다. 저작자 표시는 필수가 아니며, "
            "본 프로젝트에서는 게임 내 BGM으로 포함해 사용합니다. 음악 파일 자체를 게임과 별도로 제공하거나 배포하지 않습니다.",
            styles,
            color=PALE_RED,
            accent=RED,
        ),
        PageBreak(),
        section_title("07", "AI 활용 내역 요약", styles),
        info_table(
            [
                ("기획", "스테이지 순서, 직급별 보스 콘셉트, 컷신 서사와 대사 구조화"),
                ("개발", "플레이어 조작, 패링·스턴·근접 피해 규칙, UI·오디오·스테이지 전환 구현 보조"),
                ("아트", "오피스 2D 배경·장애물·직급 보스·컷만화 이미지 생성 및 변형"),
                ("애니메이션", "기존 스프라이트 활용, 프레임 구성, 좌우 반전, 중심축·크기 보정"),
                ("QA", "컴파일·WebGL 빌드, 브라우저 실행, 이미지/텍스트/조작 상태 대조"),
                ("문서", "심사용 게임 설명과 AI 활용 기술 문서 구성 및 PDF 생성"),
            ],
            styles,
        ),
        Spacer(1, 8 * mm),
        callout(
            "최종 게임의 요구사항 선택, 플레이 감각 평가, 수정 승인, 외부 에셋 선택, 공개 여부 및 제출 책임은 개발자에게 있습니다. "
            "AI는 반복 작업과 기술 구현을 돕는 보조 수단으로 사용했습니다.",
            styles,
            color=ICE,
            accent=MINT,
        ),
        Spacer(1, 10 * mm),
        p(f"소스 저장소: <link href='{SOURCE_URL}' color='#D4383A'>{SOURCE_URL}</link>", styles["body"]),
        p(f"플레이 링크: <link href='{GAME_URL}' color='#D4383A'>{GAME_URL}</link>", styles["body"]),
    ]
    doc.build(story, onFirstPage=header_footer, onLaterPages=header_footer)
    return out


def main() -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    register_fonts()
    styles = make_styles()
    files = [build_game_guide(styles), build_ai_report(styles)]
    for path in files:
        print(path)


if __name__ == "__main__":
    main()
