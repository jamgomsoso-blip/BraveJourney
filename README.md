<h1 align="center">BraveJourney</h1>

<p align="center"><b>사표 한 장을 들고 사무실 장애물과 직급 보스를 돌파하는 2D 액션 러너</b></p>
<p align="center">NAN 2026 · NHN Game × AI 해커톤 사전과제</p>

<p align="center">
  <a href="https://jamgomsoso-blip.github.io/BraveJourney/"><strong>🎮 브라우저에서 플레이</strong></a> ·
  <a href="output/pdf/BraveJourney_Game_Guide.pdf">📄 게임 소개 PDF</a> ·
  <a href="output/pdf/BraveJourney_AI_Usage_Report.pdf">🤖 AI 활용 기술 PDF</a> ·
  <a href="https://youtu.be/hNZlRZPIoc8">▶ 플레이 영상</a>
</p>

<p align="center">
  <img src="Assets/BraveJourney/Resources/Comics/Prologue.png" alt="BraveJourney 프롤로그" width="900">
</p>

## 게임 소개

끝나지 않는 야근에 지친 주인공이 사표를 들고 회사의 직급 보스들을 차례로 돌파하는 Unity WebGL 게임입니다. 각 스테이지는 23초의 오피스 러닝 구간과 패링 중심 보스전으로 구성됩니다.

진행 순서: `주임 → 대리 → 과장 → 차장 → 부장 → 부사장 → 대표`

## 조작법

| 구간 | 키 | 기능 |
|---|---|---|
| 컷신 | `Enter` | 다음 컷 / 보스전 시작 |
| 러닝 | `W` | 점프 / 공중에서 한 번 더 눌러 2단 점프 |
| 러닝 | `E` | 누르는 동안 슬라이드 |
| 공통 | `Space` | 패링 |
| 보스전 | `←` `→` | 좌우 이동 |
| 보스전 | `W` | 점프 / 2단 점프 |
| 보스전 | `E` | 슬라이드 |
| 보스전 | `A` | 스턴된 보스 가까이에서 발차기 공격 |
| 패배 | `R` | 현재 스테이지 다시 시작 |
| 최종 엔딩 | `R` | Stage01부터 다시 시작 |

보스 투사체를 `Space`로 반사하면 보스가 스턴됩니다. 스턴 중 보스 가까이에서 `A`를 누르면 피해가 적용되며, 한 번의 스턴에는 한 번만 피해를 줄 수 있습니다.

## 실행 방법

### 웹 플레이

1. [GitHub Pages](https://jamgomsoso-blip.github.io/BraveJourney/)를 Chrome 또는 Edge로 엽니다.
2. 첫 로딩이 끝나면 게임 화면을 한 번 클릭합니다.
3. 키보드로 플레이합니다. 소리가 들리지 않으면 브라우저 음소거를 해제합니다.

### Unity에서 실행

1. Unity Hub에서 이 저장소 폴더를 엽니다.
2. Unity `6000.3.16f1`을 사용합니다.
3. `Assets/BraveJourney/Scenes/Stage01.unity`를 연 뒤 Play를 누릅니다.

`Library`, `Temp`, `Builds` 등 Unity가 다시 생성하는 폴더는 저장소에서 제외되어 있습니다. 전체 게임 소스와 원본 에셋은 `Assets`, `Packages`, `ProjectSettings`에 포함되어 있습니다.

## 주요 구조

- `PlayerController`, `PlayerPunch`, `PlayerHealth`: 이동, 2단 점프, 슬라이드, 패링, 근접 공격, 하트·패배
- `BossHealth`, `BossShooter`, `Projectile`: 보스 체력·스턴, 발사 중지, 투사체 반사
- `StageProfileCatalog`, `StageCourseBuilder`, `StageHazard`: 7개 스테이지와 장애물·공격 패턴
- `BossComicCutscene`, `StageTransition`: 프롤로그, 인트로·승리·패배 컷신, 스테이지 전환
- `PlayerVisualAnimator`, `BossVisualAnimator`: 2D 프레임 애니메이션

## 제출 문서

- [게임 소개 및 설명 문서](output/pdf/BraveJourney_Game_Guide.pdf)
- [AI 활용 기술 문서](output/pdf/BraveJourney_AI_Usage_Report.pdf)
- 플레이 영상: [YouTube에서 보기](https://youtu.be/hNZlRZPIoc8)

## 외부 에셋 및 라이선스

- [Pixel Prototype Player Sprites](https://deadrevolver.itch.io/pixel-prototype-player-sprites) — Dead Revolver, CC0 1.0
- NanumGothic — SIL Open Font License 1.1 (`Assets/BraveJourney/Fonts/OFL.txt`)
- SongMyung — SIL Open Font License 1.1 (`Assets/BraveJourney/Fonts/SongMyung-OFL.txt`)
- `Conspiracy Theory`, `Final Boss Battle` — Rod Kim, YouTube Audio Library Soundtrack
- Unity 공식 패키지 — 버전은 `Packages/manifest.json`과 `Packages/packages-lock.json`에 고정

> 두 BGM은 YouTube Studio 오디오 보관함 라이선스 트랙으로 사용했으며, 저작자 표시는 필수가 아닙니다. 음악 파일 자체를 게임과 별도로 제공·배포하지 않습니다.
