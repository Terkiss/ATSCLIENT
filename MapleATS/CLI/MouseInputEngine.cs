using System;
using System.Runtime.InteropServices;
using System.Threading;
using MapleATS.Util;

namespace MapleATS.CLI
{
    public class MouseInputEngine
    {
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;

        private static readonly Random _random = new Random();

        public static void Execute(CommandData command, bool isPressed)
        {
            try
            {
                if (command.Delay > 0)
                {
                    Thread.Sleep(command.Delay);
                }

                string btn = command.KeyOrButton.ToUpperInvariant();

                if (btn == "MOVE")
                {
                    SetCursorPos(command.X, command.Y);
                    TeruTeruLogger.LogInvisible($"[MOUSE MOVE] X:{command.X}, Y:{command.Y} (Id: {command.Id})");
                    return;
                }
                else if (btn == "WHEEL")
                {
                    // Action on -> 휠 업, off -> 휠 다운
                    int wheelData = (command.Action.Equals("on", StringComparison.OrdinalIgnoreCase)) ? 120 : -120;
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint)wheelData, 0);
                    TeruTeruLogger.LogInvisible($"[MOUSE WHEEL] {command.Action} (Id: {command.Id})");
                    return;
                }

                uint downFlag = 0;
                uint upFlag = 0;

                switch (btn)
                {
                    case "LBUTTON":
                        downFlag = MOUSEEVENTF_LEFTDOWN;
                        upFlag = MOUSEEVENTF_LEFTUP;
                        break;
                    case "RBUTTON":
                        downFlag = MOUSEEVENTF_RIGHTDOWN;
                        upFlag = MOUSEEVENTF_RIGHTUP;
                        break;
                    case "MBUTTON":
                        downFlag = MOUSEEVENTF_MIDDLEDOWN;
                        upFlag = MOUSEEVENTF_MIDDLEUP;
                        break;
                    default:
                        TeruTeruLogger.LogError($"지원하지 않는 마우스 버튼입니다: {btn}");
                        return;
                }

                if (command.Action.Equals("on", StringComparison.OrdinalIgnoreCase))
                {
                    mouse_event(downFlag, 0, 0, 0, 0);
                    TeruTeruLogger.LogInvisible($"[MOUSE DOWN] {btn} (Id: {command.Id})");
                }
                else if (command.Action.Equals("off", StringComparison.OrdinalIgnoreCase))
                {
                    if (isPressed)
                    {
                        mouse_event(upFlag, 0, 0, 0, 0);
                        TeruTeruLogger.LogInvisible($"[MOUSE UP] {btn} (Id: {command.Id})");
                    }
                    else
                    {
                        mouse_event(downFlag, 0, 0, 0, 0);
                        int randomDelay = _random.Next(50, 251);
                        Thread.Sleep(randomDelay);
                        mouse_event(upFlag, 0, 0, 0, 0);
                        TeruTeruLogger.LogInvisible($"[MOUSE CLICK] {btn} (Delay: {randomDelay}ms) (Id: {command.Id})");
                    }
                }
            }
            catch (Exception ex)
            {
                TeruTeruLogger.LogError($"마우스 입력 중 예외 발생: {ex.Message} (명령 Id: {command.Id})");
            }
        }
    }
}
