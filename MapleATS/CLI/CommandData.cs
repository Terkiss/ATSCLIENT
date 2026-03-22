using System;

namespace MapleATS.CLI
{
    public enum InputType
    {
        Keyboard,
        Mouse
    }

    public class CommandData
    {
        public int Id { get; set; }
        public InputType InputType { get; set; }
        public string KeyOrButton { get; set; } = string.Empty;
        public int Delay { get; set; }
        public string Action { get; set; } = string.Empty;
        
        public int X { get; set; }
        public int Y { get; set; }

        public override string ToString()
        {
            if (KeyOrButton.Equals("MOVE", StringComparison.OrdinalIgnoreCase))
            {
                return $"{InputType} / {KeyOrButton},sleep,{Delay},{X},{Y} (Id: {Id})";
            }
            return $"{InputType} / {KeyOrButton},sleep,{Delay},{Action} (Id: {Id})";
        }
    }
}
