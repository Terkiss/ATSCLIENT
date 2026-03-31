using System;
using System.Threading;
using System.Collections.Generic;
using MapleATS.CLI.Utils;

namespace MapleATS.CLI
{
    /// <summary>
    /// 프로그램 시작 시 제어권을 넘겨받아 CLI 환경을 초기화하고 실행하는 클래스입니다.
    /// </summary>
    public class ATS_CLI
    {
        public static void Start()
        {
            // [옵저버 패턴] 캡처 영역 변경 감지 구독 (Subscribe)
            AppMemory.Instance.OnCaptureAreaChanged += OnCaptureAreaUpdated;

            // 콘솔 창 크기를 가로 80자, 세로 20자로 설정
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    Console.SetWindowSize(80, 20);
                    Console.SetBufferSize(80, 300); // 스크롤 버퍼 여유분
                }
                else
                {
                    Console.SetWindowSize(80, 20);
                }
            }
            catch (Exception)
            {
                // 최신 Windows Terminal 등 특정 터미널 환경에서 예외 방지
            }

            PrintWelcomeMessage();
            RunLoop();
        }

        /// <summary>
        /// 옵저버 콜백: UI 스레드에서 캡처 영역이 설정되면 터미널(워커 스레드) 환경에 로그를 출력합니다.
        /// </summary>
        private static void OnCaptureAreaUpdated(System.Drawing.Rectangle rect)
        {
            Console.WriteLine(); // 현재 입력 줄 구분
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[옵저버 알람 수신] 새로운 캡처 영역이 지정됨! (X:{rect.X}, Y:{rect.Y}, W:{rect.Width}, H:{rect.Height}) - 워커 스레드에서 비동기 처리 돌입 가능!");
            Console.ResetColor();
            Console.Write("ATS> "); // 프롬프트 다시 출력
        }

        private static void PrintWelcomeMessage()
        {
            // 콘솔 색상을 변경하여 ASCII 아트 출력
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
      _    _____ ____  
     / \  |_   _/ ___| 
    / _ \   | | \___ \ 
   / ___ \  | |  ___) |
  /_/   \_\ |_| |____/ 
");
            Console.ResetColor();

            Console.WriteLine("==================================================");
            Console.WriteLine("        Welcome to ATS (Automated Trading System) ");
            Console.WriteLine("==================================================");
            Console.WriteLine("CLI 시스템이 성공적으로 초기화되었습니다.");
            Console.WriteLine("명령어를 입력하려면 언제든지 도움말(help)을 입력하세요.");
            Console.WriteLine();
        }

        private static void RunLoop()
        {
            while (true)
            {
                Console.Write("ATS> ");

                // 키 한 개를 먼저 읽어서 '/' 인지 확인
                var firstKey = Console.ReadKey(false);
                if (firstKey.KeyChar == '/')
                {
                    Console.WriteLine(); // 줄바꿈 추가
                    HandleMenuCommand();
                    continue;
                }

                // 엔터키인지 확인
                if (firstKey.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    continue;
                }

                // 백스페이스 키 처리 시 입력 보정 (이전 글자 지워진 것을 화면에서 처리하기 어려우므로 빈 줄 처리는 제외)
                string restOfInput = Console.ReadLine() ?? string.Empty;
                string input = "";

                if (firstKey.Key == ConsoleKey.Backspace)
                {
                    input = restOfInput;
                }
                else
                {
                    input = firstKey.KeyChar + restOfInput;
                }

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                string command = input.Trim().ToLower();

                if (command == "exit" || command == "quit")
                {
                    Console.WriteLine("ATS CLI 시스템을 종료합니다...");
                    // 필요한 정리 작업 수행 후 종료
                    Environment.Exit(0);
                    break;
                }
                else if (command == "help")
                {
                    Console.WriteLine("사용 가능한 명령어:");
                    Console.WriteLine("  help                    - 도움말 표시");
                    Console.WriteLine("  /                       - 명령어 팔레트 표시");
                    Console.WriteLine("  add <Key>,sleep,<Delay>,<Action> - 명령 추가 (예: add A,sleep,500,on)");
                    Console.WriteLine("  rem <Id>                - 명령 삭제 (예: rem 1)");
                    Console.WriteLine("  run <Id>                - 명령 실행 (예: run 1)");
                    Console.WriteLine("  list                    - 모든 명령 목록 표시");
                    Console.WriteLine("  exit                    - 시스템 종료");
                    Console.WriteLine("  quit                    - 시스템 종료");
                }
                else if (command.StartsWith("add "))
                {
                    string cmdStr = input.Substring(4).Trim();
                    CommandProcessor.AddCommand(cmdStr);
                }
                else if (command.StartsWith("rem "))
                {
                    if (int.TryParse(command.Substring(4).Trim(), out int id))
                        CommandProcessor.RemoveCommand(id);
                    else
                        Console.WriteLine("잘못된 Id 형식입니다.");
                }
                else if (command.StartsWith("run "))
                {
                    if (int.TryParse(command.Substring(4).Trim(), out int id))
                        CommandProcessor.ExecuteCommand(id);
                    else
                        Console.WriteLine("잘못된 Id 형식입니다.");
                }
                else if (command == "list")
                {
                    var commands = CommandProcessor.GetAllCommands();
                    Console.WriteLine("--- 현재 등록된 명령 목록 ---");
                    foreach (var c in commands)
                    {
                        Console.WriteLine(c.ToString());
                    }
                    Console.WriteLine("----------------------------");
                }
                else
                {
                    Console.WriteLine($"알 수 없는 명령어입니다: {input}");
                }
            }
        }

        private static void HandleMenuCommand()
        {
            List<string> options = new List<string>
            {
                "키보드명령 생성 테스트",
                "키입력 테스트",
                "도움말 보기 (Help)",
                "시스템 종료 (Exit)",
                "GUI 화면 캡처 테스트 열기"
            };

            int selectedIndex = CommandPalette.ShowMenu("ATS 명령어 팔레트", options);

            if (selectedIndex == 0)
            {
                Console.WriteLine("선택됨: 키보드명령 생성 테스트");
                var commands = KeyCommandGenerate.GenerateCommandsFromText("Hello World and Sex, 1234567890", 100);
                foreach (var command in commands)
                {
                    Console.WriteLine(command.ToString());
                }
            }
            else if (selectedIndex == 1)
            {
                Console.WriteLine("선택됨: 키입력 테스트");
                Thread.Sleep(4000);
                var commands = KeyCommandGenerate.GenerateCommandsFromText("Hello World and Sex, 1234567890 11111111111111111 안녕하세요 테루키스입니다.", 0);
                foreach (var command in commands)
                {
                    var id = CommandProcessor.AddCommand(command);
                    CommandProcessor.ExecuteCommand(id);
                }
            }
            else if (selectedIndex == 2)
            {
                Console.WriteLine("선택됨: 도움말 보기 (Help). 'help'를 입력하세요.");
            }
            else if (selectedIndex == 3)
            {
                Console.WriteLine("ATS CLI 시스템을 종료합니다...");
                Environment.Exit(0);
            }
            else if (selectedIndex == 4)
            {
                Console.WriteLine("선택됨: GUI 화면 캡처 테스트 실행");
                if (MainForm.instance != null)
                {
                    MainForm.instance.Invoke(new Action(() => {
                        MainForm.instance.OnTestCaptureClick(null, EventArgs.Empty);
                    }));
                }
                else
                {
                    Console.WriteLine("MainForm 인스턴스가 존재하지 않습니다.");
                }
            }
            else
                Console.WriteLine("메뉴 선택이 취소되었습니다.");
        }
    }
}
