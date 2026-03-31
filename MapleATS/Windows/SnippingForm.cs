using System;
using System.Drawing;
using System.Windows.Forms;
using MapleATS.Util;

namespace MapleATS.Windows
{
    /// <summary>
    /// 화면 캡처 영역을 지정하기 위한 투명 오버레이 폼입니다.
    /// </summary>
    public class SnippingForm : Form
    {
        private Point startPoint;
        private Rectangle selectionRect;
        private bool isSelecting = false;

        public SnippingForm()
        {
            // 폼 설정: 전체 화면 덮기, 테두리 없음, 항상 위에 표시
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Cursor = Cursors.Cross;

            // 투명도 및 배경색 설정
            this.Opacity = 0.5;
            this.BackColor = Color.Black;

            // 주 모니터가 아닌 모든 모니터를 포함할 수도 있지만, 우선 기본 화면 전체 영역으로 설정
            this.WindowState = FormWindowState.Maximized;

            // 더블 버퍼링 활성화로 깜빡임 방지
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isSelecting = true;
                startPoint = e.Location;
                selectionRect = new Rectangle(startPoint, new Size(0, 0));
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isSelecting)
            {
                int x = Math.Min(startPoint.X, e.X);
                int y = Math.Min(startPoint.Y, e.Y);
                int width = Math.Abs(startPoint.X - e.X);
                int height = Math.Abs(startPoint.Y - e.Y);

                selectionRect = new Rectangle(x, y, width, height);
                this.Invalidate(); // 다시 그리기 요청
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (isSelecting)
            {
                isSelecting = false;

                // 영역이 너무 작지 않은지 확인 후 저장
                if (selectionRect.Width > 5 && selectionRect.Height > 5)
                {
                    // 전역 메모리에 저장
                    AppMemory.Instance.CaptureArea = selectionRect;
                }

                this.Close(); // 드래그 종료 시 폼 닫기
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (isSelecting)
            {
                // 선택 영역 그리기 (빨간색 테두리의 사각형)
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, selectionRect);
                }

                // 선택 영역 내부를 약간 더 밝게 표시 (투명도 조절로 선택된 느낌 강조)
                using (Brush brush = new SolidBrush(Color.FromArgb(100, Color.White)))
                {
                    e.Graphics.FillRectangle(brush, selectionRect);
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // ESC 키를 누르면 취소하고 닫기
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
