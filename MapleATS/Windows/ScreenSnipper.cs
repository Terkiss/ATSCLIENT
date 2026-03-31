using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace MapleATS.Windows
{
    /// <summary>
    /// 화면 캡처 유틸리티 클래스입니다.
    /// </summary>
    public static class ScreenSnipper
    {
        /// <summary>
        /// 지정된 영역을 캡처하여 JPEG 바이트 배열로 반환합니다.
        /// </summary>
        /// <param name="rect">캡처할 사각형 영역</param>
        /// <returns>캡처된 이미지의 JPEG 바이트 배열</returns>
        public static byte[] CaptureRegion(Rectangle rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return Array.Empty<byte>();
            }

            using (Bitmap bitmap = new Bitmap(rect.Width, rect.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    // CopyFromScreen을 사용하여 화면의 지정된 영역을 비트맵에 복사
                    g.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    // JPEG 형식으로 저장 (압축 효율 및 일반적인 용도 고려)
                    bitmap.Save(ms, ImageFormat.Jpeg);
                    return ms.ToArray();
                }
            }
        }
    }
}
