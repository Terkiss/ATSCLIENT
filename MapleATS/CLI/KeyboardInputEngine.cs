using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using MapleATS.Util;

namespace MapleATS.CLI
{
    /// <summary>
    /// Windows 네이티브 API (SendInput)를 활용하여 실제 하드웨어 수준의 키보드 입력을 발생시키는 엔진 클래스입니다.
    /// 구형 API인 keybd_event 대신 비동기 환경에서도 안정적이고 최신 방식인 SendInput API를 사용합니다.
    /// </summary>
    public class KeyboardInputEngine
    {
        // -----------------------------------------------------
        // Windows API 구조체 정의 (SendInput 사용을 위함)
        // -----------------------------------------------------

        /// <summary>
        /// SendInput API에 전달될 입력 이벤트의 최상위 구조체입니다.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type; // 입력 유형 (키보드: 1, 마우스: 0, 하드웨어: 2)
            public InputUnion U; // 실제 입력 데이터 (공용체)
            public static int Size => Marshal.SizeOf(typeof(INPUT)); // 구조체의 바이트 크기
        }

        /// <summary>
        /// 마우스, 키보드, 하드웨어 입력 데이터를 동일한 메모리 공간에 겹쳐서 저장하는 공용체입니다.
        /// (C++의 union을 C#에서 재현한 형태)
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        /// <summary>
        /// 키보드 이벤트에 대한 구체적인 정보를 담는 구조체입니다.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;         // 가상 키 코드 (Virtual Key Code) 예: 'A' = 0x41
            public ushort wScan;       // 하드웨어 스캔 코드
            public uint dwFlags;       // 키보드 동작 플래그 (0: 누름, 0x0002: 뗌 등)
            public uint time;          // 타임스탬프 (0이면 시스템 제공 시간 사용)
            public IntPtr dwExtraInfo; // 추가 메시지 정보 포인터
        }

        // 구동 구색을 맞추기 위한 마우스 및 하드웨어 빈 구조체
        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        // -----------------------------------------------------
        // Windows 네이티브 함수 및 상수
        // -----------------------------------------------------

        /// <summary>
        /// 시스템의 입력 스트림에 키보드 또는 마우스 이벤트를 합성하여 운영체제 단에 강제로 이벤트를 삽입합니다.
        /// </summary>
        [DllImport("user32.dll")]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        const uint INPUT_KEYBOARD = 1;       // 이 입력이 키보드 이벤트임을 나타내는 상수
        const uint KEYEVENTF_KEYUP = 0x0002; // 키를 떼는 동작임을 나타내는 플래그

        private static readonly Random _random = new Random();

        /// <summary>
        /// CommandData 객체에 정의된 키, 액션, 대기 시간을 분석하여 실제로 키보드를 누르거나 뗍니다.
        /// </summary>
        /// <param name="command">실행할 명령어 데이터 (키보드 키, 지연시간, on/off 판단용)</param>
        /// <param name="isPressed">해당 명령 Id가 HashSet에 의해 현재 이미 눌린 상태로 기록되어 있는지 여부</param>
        public static void Execute(CommandData command, bool isPressed)
        {
            try
            {
                // 입력할 키 값이 정의되어 있지 않다면 무시합니다.
                if (string.IsNullOrEmpty(command.KeyOrButton)) return;

                // 유저가 설정한 지연 시간(Delay)이 존재하면 해당 ms만큼 쓰레드를 대기시킵니다.
                if (command.Delay > 0)
                {
                    Thread.Sleep(command.Delay);
                }

                // 문자열 키 이름("A", "ENTER", "SPACE" 등)을 시스템 키 열거형 값으로 안전하게 변환합니다.
                if (!Enum.TryParse(command.KeyOrButton, true, out Keys key))
                {
                    TeruTeruLogger.LogError($"유효하지 않은 키보드 키 이름입니다: {command.KeyOrButton}");
                    return;
                }

                ushort vk = (ushort)key;

                if (command.Action.Equals("on", StringComparison.OrdinalIgnoreCase))
                {
                    // Action이 on일 경우 키 누름 처리 (키가 계속 눌림 유지됨)
                    SendKey(vk, false);
                    TeruTeruLogger.LogInvisible($"[KEY DOWN] {command.KeyOrButton} (Id: {command.Id})");
                }
                else if (command.Action.Equals("off", StringComparison.OrdinalIgnoreCase))
                {
                    if (isPressed)
                    {
                        // 기존에 "on"명령으로 눌려있는 키였다면 떼는 행동만 수행
                        SendKey(vk, true);
                        TeruTeruLogger.LogInvisible($"[KEY UP] {command.KeyOrButton} (Id: {command.Id})");
                    }
                    else
                    {
                        // 기존에 눌린 적이 없는 상태(즉 한 번의 단순 클릭)에서 off를 호출한 경우:
                        // 누르고 일정 랜덤 시간(0~100ms) 대기 후 떼어 자연스러운 타건(키 스트로크)을 시뮬레이션 함
                        SendKey(vk, false);
                        int randomDelay = _random.Next(0, 100);
                        Thread.Sleep(randomDelay);
                        SendKey(vk, true);
                        TeruTeruLogger.LogInvisible($"[KEY STROKE] {command.KeyOrButton} (Delay: {randomDelay}ms) (Id: {command.Id})");
                    }
                }
            }
            catch (Exception ex)
            {
                // 치명적인 예외가 터져도 크래시 나지 않고 건너뛰도록 처리
                TeruTeruLogger.LogError($"키보드 입력 중 예외 발생: {ex.Message} (명령 Id: {command.Id})");
            }
        }

        /// <summary>
        /// SendInput API를 직접 호출하여 하나의 Virtual Key 코드를 누름/뗌 상태로 OS에 전송합니다.
        /// </summary>
        /// <param name="vk">입력할 가상 키(Virtual Key) 코드</param>
        /// <param name="keyUp">true인 경우 키 떼기(KeyUp), false인 경우 키 누름(KeyDown)</param>
        private static void SendKey(ushort vk, bool keyUp)
        {
            // 한 번에 하나의 키보드 이벤트를 전송할 배열 선언
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_KEYBOARD;         // 키보드 신호
            inputs[0].U.ki.wVk = vk;                 // 누를 키의 코드값 세팅
            inputs[0].U.ki.wScan = 0;
            inputs[0].U.ki.dwFlags = keyUp ? KEYEVENTF_KEYUP : 0; // keyUp이 true면 0x0002(KEYUP) 플래그 셋, false면 플래그 0(누름)
            inputs[0].U.ki.time = 0;                 // OS 시스템 시간 자동 채택
            inputs[0].U.ki.dwExtraInfo = IntPtr.Zero;

            // 만든 입력 신호 데이터 배열을 OS 단에 던짐
            SendInput(1, inputs, INPUT.Size);
        }
    }
}
