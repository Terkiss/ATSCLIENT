# ATS CLIENT

ATS CLIENT는 **Windows 하드웨어 시뮬레이션(SendInput, mouse_event API)** 을 사용하여 정교한 키보드 및 마우스 입력을 제어하는 자동화 시스템(Macro/ATS) 어플리케이션입니다. 사용자의 텍스트 입력과 특수 명령어를 완벽한 OS 레벨 하드웨어 입력 신호로 번역하여 실행합니다.

## 🚀 핵심 기능 (Features)

### 1. 지능형 키보드/마우스 제어 엔진 (Input Engines)
- **`KeyboardInputEngine`**: 가장 최신의 빠르고 안정적인 `SendInput` 네이티브 API (user32.dll)를 이용해 물리적인 하드웨어 키보드 이벤트를 완벽하게 모방합니다. 단일 타건 시 `Random` 딜레이를 추가하여 진짜 사람과 같은 타건 리듬을 형성합니다.
- **`MouseInputEngine`**: `mouse_event` 및 `SetCursorPos` API를 통해 좌/우/가운데 클릭, 마우스 휠 스크롤(`WHEEL`), 지정된 X/Y 좌표로의 텔레포트(`MOVE`) 로직 등을 지원합니다.

### 2. 스마트 매크로 생성기 (`KeyCommandGenerate`)
사람이 타건하는 일반 문장을 하드웨어 파편화 명령 리스트로 자동 컴파일해주는 강력한 제너레이터를 지원합니다.
- **자연스러운 단일 타건**: 영어 소/대문자 조합 시 자동으로 `LSHIFTKEY` 누름/뗌 처리를 조합하며, 각 문자는 단일 타건(`off` 매크로)으로 번역됩니다.
- **게이밍 꾹 누르기 (RLE Hold)**: `WWW` 나 `111` 처럼 같은 문자가 3번 이상 연속으로 입력되면, 여러 번 연속해서 타건하는 대신 글자의 개수에 비례하여 키를 길게 **꾹 누르고(Hold) 떼는 매크로** 로 자동 변환해 줍니다 (FPS, RPG 게임 친화형 이동/차징 매크로 기능 적용 완료).

### 3. 상태 기반 분산 스케줄러 (`CommandProcessor`)
실행 중인 명령어의 추상화된 고유 ID가 아닌, **물리적인 기기 부품 자체(Ex: 'A' 키보드 버튼 자체)의 눌림 상태**를 독립적인 `HashSet`으로 추적 조사합니다. 이로써 복잡한 매크로 중복 실행에서도 입력 충돌이나 키가 눌린 채로 안 빠지는 Lock-down(유령 키) 현상을 원천 방지합니다.

---

## 💻 커맨드 및 사용 예시 (Usage)

### 매크로 명령어 포맷
모든 커맨드는 내부적으로 아래와 같은 쉼표 분리 문자열 또는 모델(`CommandData`)로 파싱됩니다.
`<Key/Button>,sleep,<Delay(ms)>,<Action>`

* **명령 종류 예시:**
  * `A,sleep,50,off` 👉 A 키를 50ms 대기 후 한 번 쳤다 뗌 (단일 타건)
  * `LSHIFTKEY,sleep,0,on` 👉 Shift 키 꾹 누르기 시작 (상태 유지됨)
  * `LBUTTON,sleep,100,off` 👉 마우스 좌클릭 (단일 클릭)
  * `WHEEL,sleep,0,on` 👉 마우스 휠 위로 굴리기 (`off`는 아래로 굴리기)
  * `MOVE,sleep,0,100,200` 👉 마우스 커서를 `X: 100`, `Y: 200` 위치로 즉시 이동


### C# 프로젝트 내 연동 코드
일반 채팅이나 문장열을 매크로로 변환하여 쏘려면 다음과 같이 사용합니다.

```csharp
// 1. 일반 문자열을 지능형 매크로 명령어 리스트로 번역 (기본 타건 딜레이 50ms)
var commands = KeyCommandGenerate.GenerateCommandsFromText("Hello World and 1111111", 50);

// 2. 엔진 저장소에 등록 후 OS에 하드웨어 명령 하달
foreach (var cmd in commands)
{
    var id = CommandProcessor.AddCommand(cmd);   // 파싱 및 상태 등록
    CommandProcessor.ExecuteCommand(id);         // 실제 하드웨어 엔진에 전송
}
```

---

## 🛠️ 구조 및 코어 파일 로케이션
- `CLI/ATS_CLI.cs`: 유저 입력과 커맨드 시스템을 브릿징해주는 진입점 앱
- `CLI/CommandProcessor.cs`: 라우팅 및 딕셔너리/HashSet 객체 상태 총괄 지휘자
- `CLI/CommandData.cs`: 매크로의 각 요소를 포함하는 DTO 및 상태 모델
- `CLI/KeyCommandGenerate.cs`: String(문자열정규화) 👉 매크로 Array 번역기
- `CLI/KeyboardInputEngine.cs` & `CLI/MouseInputEngine.cs`: Windows 네이티브 하드웨어 조작 래퍼 클래스들
