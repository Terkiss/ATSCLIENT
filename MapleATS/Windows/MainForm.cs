using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace MapleATS.Windows
{
    public class MainForm : Form
    {
        public static MainForm? instance;

        private PictureBox pictureBox;
        private MenuStrip menuStrip;

        public MainForm()
        {
            instance = this;

            this.Text = "MapleATS 이미지 뷰어";
            this.Size = new Size(800, 600);

            // MenuStrip 초기화
            menuStrip = new MenuStrip();
            ToolStripMenuItem imageMenu = new ToolStripMenuItem("이미지");
            
            ToolStripMenuItem setAreaItem = new ToolStripMenuItem("영역 지정 설정", null, OnSetAreaClick);
            ToolStripMenuItem testCaptureItem = new ToolStripMenuItem("영역 지정 캡처 테스트", null, OnTestCaptureClick);

            imageMenu.DropDownItems.Add(setAreaItem);
            imageMenu.DropDownItems.Add(testCaptureItem);
            menuStrip.Items.Add(imageMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom, // 비율 유지
            };

            this.Controls.Add(pictureBox);
        }

        private void OnSetAreaClick(object? sender, EventArgs e)
        {
            // 화면 캡처 영역 설정 폼 띄우기
            using (SnippingForm snippingForm = new SnippingForm())
            {
                snippingForm.ShowDialog();
            }
        }

        public void OnTestCaptureClick(object? sender, EventArgs e)
        {
            Rectangle rect = AppMemory.Instance.CaptureArea;
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                MessageBox.Show("먼저 '영역 지정 설정'을 통해 캡처할 영역을 지정해주세요.", "캡처 영역 미지정", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ScreenSnipper 유틸리티를 사용하여 캡처
            byte[] capturedImage = ScreenSnipper.CaptureRegion(rect);
            if (capturedImage.Length > 0)
            {
                ShowImage(capturedImage);
            }
        }

        public void ShowImage(Image image)
        {
            if (pictureBox.InvokeRequired)
            {
                pictureBox.Invoke(() =>
                {
                    if (pictureBox.Image != null)
                    {
                        pictureBox.Image.Dispose();
                    }
                    pictureBox.Image = (Image)image.Clone();
                });
            }
            else
            {
                if (pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                }
                pictureBox.Image = (Image)image.Clone();
            }
        }

        public void ShowImage(byte[] imageBytes)
        {
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                using (Image img = Image.FromStream(ms))
                {
                    ShowImage((Image)img.Clone());
                }
            }
        }

    }
}
