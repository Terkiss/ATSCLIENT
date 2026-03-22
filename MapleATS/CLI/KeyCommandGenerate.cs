using System;
using System.Collections.Generic;

namespace MapleATS.CLI
{
    /// <summary>
    /// 일반 텍스트 문자열을 개별 키보드 매크로 명령어 리스트로 변환하는 생성기입니다.
    /// </summary>
    public class KeyCommandGenerate
    {
        /// <summary>
        /// 주어진 텍스트를 한 글자씩 타이핑하는 키보드 명령 문자열 리스트로 변환합니다.
        /// 엔진의 'off' 명령이 자체적으로 자연스러운 타건(랜덤 지연 포함)을 수행하므로 단일 타건(off)만 생성합니다.
        /// </summary>
        public static List<string> GenerateCommandsFromText(string text, int delayMs = 50)
        {
            var commands = new List<string>();

            if (string.IsNullOrEmpty(text))
                return commands;

            foreach (char c in text)
            {
                if (c == ' ')
                {
                    // 빈칸 1번에 1타건 (연속 입력 여부 무시)
                    commands.Add($"SPACE,sleep,{delayMs},off");
                    continue;
                }

                bool isUpper = char.IsUpper(c);
                string keyName = GetKeyNameFromChar(c);

                if (string.IsNullOrEmpty(keyName))
                    continue;

                // 대문자일 때만 쉬프트를 잠시 Hold
                if (isUpper) commands.Add($"LSHIFTKEY,sleep,0,on");

                // 해당 문자 1번 타건
                commands.Add($"{keyName},sleep,{delayMs},off");

                // 쉬프트 Release
                if (isUpper) commands.Add($"LSHIFTKEY,sleep,0,off");
            }

            return commands;
        }

        private static string GetKeyNameFromChar(char c)
        {
            char upper = char.ToUpperInvariant(c);
            
            if (upper >= 'A' && upper <= 'Z')
                return upper.ToString();
            
            if (upper >= '0' && upper <= '9')
                return "D" + upper;

            return string.Empty;
        }
    }
}
