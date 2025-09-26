using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pbl_test
{
    public partial class Tranhg_chủ : UserControl
    {
        private float originalWidth;
        private float originalHeight;
        private Dictionary<Control, Rectangle> controlOriginalBounds = new Dictionary<Control, Rectangle>();
        public Tranhg_chủ()
        {
            InitializeComponent();
            this.Load += Tranhg_chủ_Load;
            this.Resize += Tranhg_chủ_Resize;
        }

        private void Tranhg_chủ_Load(object sender, EventArgs e)
        {
            originalWidth = this.Width;
            originalHeight = this.Height;

            foreach (Control control in this.Controls)
            {
                controlOriginalBounds[control] = new Rectangle(
                    control.Location.X,
                    control.Location.Y,
                    control.Width,
                    control.Height
                );
            }
        }
        private void Tranhg_chủ_Resize(object sender, EventArgs e)
        {
            if (originalWidth == 0 || originalHeight == 0) return;

            float widthRatio = this.Width / originalWidth;
            float heightRatio = this.Height / originalHeight;

            foreach (Control control in this.Controls)
            {
                if (controlOriginalBounds.ContainsKey(control))
                {
                    Rectangle originalBounds = controlOriginalBounds[control];

                    int newX = (int)(originalBounds.X * widthRatio);
                    int newY = (int)(originalBounds.Y * heightRatio);
                    int newWidth = (int)(originalBounds.Width * widthRatio);
                    int newHeight = (int)(originalBounds.Height * heightRatio);

                    control.Location = new Point(newX, newY);
                    control.Size = new Size(newWidth, newHeight);

                    control.Font = new Font(control.Font.FontFamily, control.Font.Size * Math.Min(widthRatio, heightRatio));
                }
            }
        }


    }
}
