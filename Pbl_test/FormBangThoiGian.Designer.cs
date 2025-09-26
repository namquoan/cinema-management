namespace Pbl_test
{
    partial class FormBangThoiGian
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvThoiGianBieu;
        private System.Windows.Forms.Button btnXacNhan;
        private System.Windows.Forms.ComboBox cboYear;
        private System.Windows.Forms.ComboBox cboMonth;
        private System.Windows.Forms.ComboBox cboDay;
        private System.Windows.Forms.Label lblYear;
        private System.Windows.Forms.Label lblMonth;
        private System.Windows.Forms.Label lblDay;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBangThoiGian));
            dgvThoiGianBieu = new DataGridView();
            btnXacNhan = new Button();
            cboYear = new ComboBox();
            cboMonth = new ComboBox();
            cboDay = new ComboBox();
            lblYear = new Label();
            lblMonth = new Label();
            lblDay = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvThoiGianBieu).BeginInit();
            SuspendLayout();
            // 
            // dgvThoiGianBieu
            // 
            dgvThoiGianBieu.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvThoiGianBieu.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvThoiGianBieu.Location = new Point(16, 75);
            dgvThoiGianBieu.Margin = new Padding(4, 6, 4, 6);
            dgvThoiGianBieu.Name = "dgvThoiGianBieu";
            dgvThoiGianBieu.RowHeadersWidth = 51;
            dgvThoiGianBieu.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvThoiGianBieu.Size = new Size(1013, 725);
            dgvThoiGianBieu.TabIndex = 0;
            // 
            // btnXacNhan
            // 
            btnXacNhan.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnXacNhan.Location = new Point(930, 812);
            btnXacNhan.Margin = new Padding(3, 4, 3, 4);
            btnXacNhan.Name = "btnXacNhan";
            btnXacNhan.Size = new Size(100, 38);
            btnXacNhan.TabIndex = 1;
            btnXacNhan.Text = "Xác Nhận";
            btnXacNhan.UseVisualStyleBackColor = true;
            // 
            // cboYear
            // 
            cboYear.FormattingEnabled = true;
            cboYear.Location = new Point(60, 25);
            cboYear.Margin = new Padding(3, 4, 3, 4);
            cboYear.Name = "cboYear";
            cboYear.Size = new Size(90, 28);
            cboYear.TabIndex = 2;
            // 
            // cboMonth
            // 
            cboMonth.FormattingEnabled = true;
            cboMonth.Location = new Point(220, 25);
            cboMonth.Margin = new Padding(3, 4, 3, 4);
            cboMonth.Name = "cboMonth";
            cboMonth.Size = new Size(70, 28);
            cboMonth.TabIndex = 3;
            // 
            // cboDay
            // 
            cboDay.FormattingEnabled = true;
            cboDay.Location = new Point(350, 25);
            cboDay.Margin = new Padding(3, 4, 3, 4);
            cboDay.Name = "cboDay";
            cboDay.Size = new Size(70, 28);
            cboDay.TabIndex = 4;
            // 
            // lblYear
            // 
            lblYear.AutoSize = true;
            lblYear.Location = new Point(16, 29);
            lblYear.Name = "lblYear";
            lblYear.Size = new Size(40, 20);
            lblYear.TabIndex = 5;
            lblYear.Text = "Year:";
            // 
            // lblMonth
            // 
            lblMonth.AutoSize = true;
            lblMonth.Location = new Point(164, 29);
            lblMonth.Name = "lblMonth";
            lblMonth.Size = new Size(55, 20);
            lblMonth.TabIndex = 6;
            lblMonth.Text = "Month:";
            // 
            // lblDay
            // 
            lblDay.AutoSize = true;
            lblDay.Location = new Point(294, 29);
            lblDay.Name = "lblDay";
            lblDay.Size = new Size(38, 20);
            lblDay.TabIndex = 7;
            lblDay.Text = "Day:";
            // 
            // FormBangThoiGian
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1045, 886);
            Controls.Add(lblDay);
            Controls.Add(lblMonth);
            Controls.Add(lblYear);
            Controls.Add(cboDay);
            Controls.Add(cboMonth);
            Controls.Add(cboYear);
            Controls.Add(btnXacNhan);
            Controls.Add(dgvThoiGianBieu);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 6, 4, 6);
            Name = "FormBangThoiGian";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Bảng Thời Gian Biểu";
            ((System.ComponentModel.ISupportInitialize)dgvThoiGianBieu).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
