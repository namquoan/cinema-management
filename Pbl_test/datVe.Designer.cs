namespace Pbl_test
{
    partial class datVe
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.cboChonPhim = new System.Windows.Forms.ComboBox();
            this.cboChonNgay = new System.Windows.Forms.ComboBox();
            this.cboChonSuat = new System.Windows.Forms.ComboBox();
            this.cboChonPhong = new System.Windows.Forms.ComboBox();
            this.btnDatVe = new System.Windows.Forms.Button();
            this.panelGhe = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.panelChon = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panelChon.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();

            // panel1
            this.panel1.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 60);
            this.panel1.TabIndex = 10;

            // label5
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(20, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(250, 38);
            this.label5.TabIndex = 0;
            this.label5.Text = "Đặt Vé Xem Phim";

            // panelChon
            this.panelChon.BackColor = System.Drawing.Color.FromArgb(240, 240, 245);
            this.panelChon.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelChon.Padding = new System.Windows.Forms.Padding(20, 20, 20, 20);
            this.panelChon.Controls.Add(this.label1);
            this.panelChon.Controls.Add(this.cboChonPhim);
            this.panelChon.Controls.Add(this.label2);
            this.panelChon.Controls.Add(this.cboChonNgay);
            this.panelChon.Controls.Add(this.label3);
            this.panelChon.Controls.Add(this.cboChonSuat);
            this.panelChon.Controls.Add(this.btnDatVe);
            this.panelChon.Size = new System.Drawing.Size(800, 200);

            // label1
            this.SetupLabel(this.label1, "Chọn phim:", 40, 30);
            // cboChonPhim
            this.SetupComboBox(this.cboChonPhim, 180, 25);
            // label2
            this.SetupLabel(this.label2, "Chọn ngày:", 40, 80);
            // cboChonNgay
            this.SetupComboBox(this.cboChonNgay, 180, 75);
            // label3
            this.SetupLabel(this.label3, "Chọn suất:", 40, 130);
            // cboChonSuat
            this.SetupComboBox(this.cboChonSuat, 180, 125);
            // label4
            this.SetupLabel(this.label4, "Chọn phòng:", 20, 100);
            // cboChonPhong
            this.cboChonPhong.Visible = false;
            this.cboChonPhong.Enabled = false;
            this.cboChonSuat.Visible = true;
            this.cboChonSuat.Enabled = true;
            // Đảm bảo panelChon.Controls thứ tự: phim, ngày, suất, phòng, đặt vé
            this.panelChon.Controls.Clear();
            this.panelChon.Controls.Add(this.label1); // Chọn phim
            this.panelChon.Controls.Add(this.cboChonPhim);
            this.panelChon.Controls.Add(this.label2); // Chọn ngày
            this.panelChon.Controls.Add(this.cboChonNgay);
            this.panelChon.Controls.Add(this.label3); // Chọn suất
            this.panelChon.Controls.Add(this.cboChonSuat);
            this.panelChon.Controls.Add(this.btnDatVe);

            // btnDatVe
            this.btnDatVe.Text = "Đặt Vé";
            this.btnDatVe.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnDatVe.Size = new System.Drawing.Size(160, 40);
            this.btnDatVe.Location = new System.Drawing.Point(700, 125);
            this.btnDatVe.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.btnDatVe.ForeColor = System.Drawing.Color.White;
            this.btnDatVe.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            // panel2
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.panelGhe);
            this.panel2.Padding = new System.Windows.Forms.Padding(20);

            // panelGhe
            this.panelGhe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGhe.AutoScroll = true;
            this.panelGhe.BackColor = System.Drawing.Color.White;
            this.panelGhe.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelGhe.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panelGhe.WrapContents = true;
            this.panelGhe.Padding = new System.Windows.Forms.Padding(10);

            // datVe
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panelChon);
            this.Controls.Add(this.panel1);
            this.MinimumSize = new System.Drawing.Size(800, 660);
            this.Name = "datVe";
            this.Size = new System.Drawing.Size(800, 660);

            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panelChon.ResumeLayout(false);
            this.panelChon.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void SetupLabel(System.Windows.Forms.Label lbl, string text, int x, int y)
        {
            lbl.AutoSize = true;
            lbl.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            lbl.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
            lbl.Location = new System.Drawing.Point(x, y);
            lbl.Text = text;
        }

        private void SetupComboBox(System.Windows.Forms.ComboBox cbo, int x, int y)
        {
            cbo.BackColor = System.Drawing.Color.White;
            cbo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbo.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            cbo.ForeColor = System.Drawing.Color.Black;
            cbo.Location = new System.Drawing.Point(x, y);
            cbo.Size = new System.Drawing.Size(400, 31);
        }

        #endregion

        private System.Windows.Forms.ComboBox cboChonPhim;
        private System.Windows.Forms.ComboBox cboChonNgay;
        private System.Windows.Forms.ComboBox cboChonSuat;
        private System.Windows.Forms.ComboBox cboChonPhong;
        private System.Windows.Forms.Button btnDatVe;
        private System.Windows.Forms.FlowLayoutPanel panelGhe;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panelChon;
        private System.Windows.Forms.Panel panel2;
    }
}