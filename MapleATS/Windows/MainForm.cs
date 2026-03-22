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
        public MainForm()
        {
            instance = this;

            this.Text = "MapleATS 이미지 뷰어";
            this.Size = new Size(800, 600);

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom, // 비율 유지
            };

            this.Controls.Add(pictureBox);
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
