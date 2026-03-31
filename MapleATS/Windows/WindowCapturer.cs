using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace MapleATS.Windows
{
    /// <summary>
    /// 게임 클라이언트 등 특정 윈도우 창의 화면을 캡처하는 도우미 클래스입니다.
    /// 완전한 비활성창(백그라운드 가려짐 등)은 캡처하지 않고, 필요 시 타겟 창을 최상단에 올린 후 화면 영역을 스크린샷 뜹니다.
    /// </summary>
    public static class WindowCapturer
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        /// <summary>
        /// 지정한 제목을 가진 창을 찾아서 포그라운드로 올린 뒤, 해당 영역만큼 화면에서 캡처하여 JPG 이미지 포맷의 byte[] 로 변환합니다.
        /// 대상 창이 최소화 되어있거나 다른 창에 가려져 있어도 강제로 띄운 뒤 캡처합니다.
        /// </summary>
        /// <param name="windowTitle">캡처할 창의 제목 표시줄 문자열 (일부 혹은 정확한 제목)</param>
        /// <returns>이미지 바이트 배열. 실패 시 null</returns>
        public static byte[]? CaptureToByteArray(string windowTitle)
        {
            if (!OperatingSystem.IsWindows())
            {
                return null;
            }

            // 1. 대상 윈도우 창 핸들 검색
            IntPtr hWnd = FindWindow(null, windowTitle);
            if (hWnd == IntPtr.Zero)
            {
                // 타겟을 찾지 못함
                return null;
            }

            // 2. 비활성 최소화 상태일 수 있으므로 복구(Restore) 및 앞으로(Foreground) 꺼냅니다.
            ShowWindow(hWnd, SW_RESTORE);
            SetForegroundWindow(hWnd);

            // 운영체제가 창을 화면 맨 앞에 렌더링하고 애니메이션 처리할 시간을 잠깐 줍니다.
            Thread.Sleep(300);

            // 3. 윈도우 창의 화면 좌표 구하기
            if (GetWindowRect(hWnd, out RECT rect))
            {
                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;

                // 오류 방지(크기가 비정상일 경우 취소)
                if (width <= 0 || height <= 0) return null;

                // 4. 화면 영역 캡처
                using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                {
                    using (Graphics graphics = Graphics.FromImage(bmp))
                    {
                        // 활성화 된 창 영역의 스크린을 복사
                        graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
                    }

                    // 5. 메모리 스트림을 통해 바이트 배열로 직렬화 (전송 최적화를 위해 JPEG 권장)
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // 향후 Client.Send_Image 에 그대로 덤프하여 보낼 수 있음
                        bmp.Save(ms, ImageFormat.Jpeg);
                        return ms.ToArray();
                    }
                }
            }

            return null;
        }
    }
}
