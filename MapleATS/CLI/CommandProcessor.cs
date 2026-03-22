using System;
using System.Collections.Generic;
using MapleATS.Util;

namespace MapleATS.CLI
{
    public class CommandProcessor
    {
        private static readonly Dictionary<int, CommandData> CommandRepository = new Dictionary<int, CommandData>();
        private static readonly HashSet<int> PressedCommandIds = new HashSet<int>();
        private static int _nextId = 1;

        public static int AddCommand(string commandString)
        {
            try
            {
                string[] parts = commandString.Split(',');
                if (parts.Length < 4)
                {
                    TeruTeruLogger.LogError($"잘못된 명령 형식입니다: {commandString}");
                    return -1;
                }

                string keyOrButton = parts[0].Trim().ToUpperInvariant();
                
                if (!parts[1].Trim().Equals("sleep", StringComparison.OrdinalIgnoreCase))
                {
                    TeruTeruLogger.LogWarning($"'sleep' 키워드가 누락되거나 잘못되었습니다: {parts[1]}");
                }

                if (!int.TryParse(parts[2].Trim(), out int delay))
                {
                    TeruTeruLogger.LogError($"잘못된 지연 시간 형식입니다: {parts[2]}");
                    return -1;
                }

                InputType inputType = DetermineInputType(keyOrButton);

                var command = new CommandData
                {
                    Id = _nextId++,
                    InputType = inputType,
                    KeyOrButton = keyOrButton,
                    Delay = delay
                };

                if (keyOrButton == "MOVE")
                {
                    if (parts.Length != 5)
                    {
                        TeruTeruLogger.LogError($"MOVE 명령 형식 오류 (<MOVE>,sleep,<Delay(ms)>,<X>,<Y>): {commandString}");
                        return -1;
                    }
                    if (!int.TryParse(parts[3].Trim(), out int x) || !int.TryParse(parts[4].Trim(), out int y))
                    {
                        TeruTeruLogger.LogError($"좌표 형식 오류: {parts[3]}, {parts[4]}");
                        return -1;
                    }
                    command.X = x;
                    command.Y = y;
                }
                else
                {
                    if (parts.Length != 4)
                    {
                        TeruTeruLogger.LogError($"명령 형식 오류 (<Key/Button>,sleep,<Delay(ms)>,<Action>): {commandString}");
                        return -1;
                    }
                    string action = parts[3].Trim().ToLowerInvariant();
                    if (action != "on" && action != "off" && keyOrButton != "WHEEL")
                    {
                        TeruTeruLogger.LogError($"잘못된 액션 형식입니다: {action} (on 또는 off 필요)");
                        return -1;
                    }
                    if (keyOrButton == "WHEEL" && action != "on" && action != "off")
                    {
                        TeruTeruLogger.LogError($"WHEEL 액션 형식 오류 (on=휠업, off=휠다운): {action}");
                        return -1;
                    }
                    command.Action = action;
                }

                CommandRepository[command.Id] = command;
                TeruTeruLogger.LogInfo($"명령 추가됨: {command}");
                return command.Id;
            }
            catch (Exception ex)
            {
                TeruTeruLogger.LogError($"명령 추가 중 오류 발생: {ex.Message}");
                return -1;
            }
        }

        private static InputType DetermineInputType(string keyOrButton)
        {
            if (keyOrButton == "LBUTTON" || keyOrButton == "RBUTTON" || keyOrButton == "MBUTTON" || keyOrButton == "MOVE" || keyOrButton == "WHEEL")
            {
                return InputType.Mouse;
            }
            return InputType.Keyboard;
        }

        public static void RemoveCommand(int id)
        {
            if (CommandRepository.TryGetValue(id, out var command))
            {
                if (PressedCommandIds.Contains(id))
                {
                    var offCommand = new CommandData
                    {
                        Id = command.Id,
                        InputType = command.InputType,
                        KeyOrButton = command.KeyOrButton,
                        Delay = 0,
                        Action = "off"
                    };

                    if (command.InputType == InputType.Keyboard)
                        KeyboardInputEngine.Execute(offCommand, true);
                    else
                        MouseInputEngine.Execute(offCommand, true);

                    PressedCommandIds.Remove(id);
                }

                CommandRepository.Remove(id);
                TeruTeruLogger.LogAttention($"명령 삭제됨 (Id: {id})");
            }
            else
            {
                TeruTeruLogger.LogWarning($"삭제할 명령 Id를 찾을 수 없습니다: {id}");
            }
        }

        public static void ExecuteCommand(int id)
        {
            if (CommandRepository.TryGetValue(id, out var command))
            {
                // 버그 수정: 특정 '커맨드 Id' 단위가 아니라 해당 '키보드 자판(KeyOrButton)'이 현재 눌린 상태인지 추적
                bool isKeyPressed = false;
                List<int> pressedIdsToClear = new List<int>();

                foreach (var pId in PressedCommandIds)
                {
                    if (CommandRepository.TryGetValue(pId, out var pCmd) && 
                        pCmd.KeyOrButton.Equals(command.KeyOrButton, StringComparison.OrdinalIgnoreCase))
                    {
                        isKeyPressed = true;
                        
                        // 현재 처리하는 동작이 "off" 라면 이 기기(Key/Button)를 on시켰던 이전 매크로 Id들을 해제 대상으로 지정
                        if (command.Action.Equals("off", StringComparison.OrdinalIgnoreCase))
                        {
                            pressedIdsToClear.Add(pId);
                        }
                    }
                }

                // isKeyPressed 여부를 제대로 판별해서 엔진에 전달
                if (command.InputType == InputType.Keyboard)
                {
                    KeyboardInputEngine.Execute(command, isKeyPressed);
                }
                else
                {
                    MouseInputEngine.Execute(command, isKeyPressed);
                }

                if (command.KeyOrButton != "MOVE" && command.KeyOrButton != "WHEEL")
                {
                    if (command.Action.Equals("on", StringComparison.OrdinalIgnoreCase))
                    {
                        PressedCommandIds.Add(id);
                    }
                    else if (command.Action.Equals("off", StringComparison.OrdinalIgnoreCase))
                    {
                        // 기존에 on 상태로 기록된 해당 키의 커맨드 Id들을 Set에서 깨끗하게 지워줌
                        foreach (var clearId in pressedIdsToClear)
                        {
                            PressedCommandIds.Remove(clearId);
                        }
                    }
                }
            }
            else
            {
                TeruTeruLogger.LogWarning($"실행할 명령 Id를 찾을 수 없습니다: {id}");
            }
        }

        public static IEnumerable<CommandData> GetAllCommands()
        {
            return CommandRepository.Values;
        }
    }
}
