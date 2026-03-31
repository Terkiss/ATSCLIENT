using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MapleATS.Windows;


namespace MapleATS.Util
{

    public class AppMemory
    {
        public static AppMemory Instance = new AppMemory();

        private AppMemory() { }

        public MainForm MainForm;

        private System.Drawing.Rectangle _captureArea;

        /// <summary>
        /// 드래그 앤 드롭으로 설정된 캡처 영역 좌표가 변경될 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<System.Drawing.Rectangle>? OnCaptureAreaChanged;

        /// <summary>
        /// 드래그 앤 드롭으로 설정된 캡처 영역 좌표입니다. 
        /// 값을 설정하면 OnCaptureAreaChanged 이벤트를 통해 구독자들에게 알림을 보냅니다.
        /// </summary>
        public System.Drawing.Rectangle CaptureArea 
        { 
            get => _captureArea;
            set
            {
                _captureArea = value;
                // 옵저버들에게 알림 방송 (Publish)
                OnCaptureAreaChanged?.Invoke(value);
            }
        }
    }
}
