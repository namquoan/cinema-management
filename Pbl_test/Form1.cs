using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Pbl_test;

namespace Pbl_test
{
    //private bool isResizingInitialized = false;

    public partial class Form1 : Form
    {
        // Chuỗi kết nối SQL Server
        private string connectionString = @"Data Source=DESKTOP-HHKF7O6;Initial Catalog=POBOLO;Integrated Security=True;TrustServerCertificate=True";
        private SqlConnection sqlcon;
        private float originalWidth;
        private float originalHeight;
        private Dictionary<Control, Rectangle> controlOriginalBounds = new Dictionary<Control, Rectangle>();
        private Dictionary<Control, Font> controlOriginalFonts = new Dictionary<Control, Font>();
        private bool isResizingInitialized = false;
        bool isReturningFromLogout = false;


        private void StoreOriginalBounds(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                controlOriginalBounds[control] = control.Bounds;
                controlOriginalFonts[control] = control.Font;

                if (control.HasChildren)
                {
                    StoreOriginalBounds(control);
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            this.SizeChanged += Form1_Resize;

            // Gán sự kiện Click cho nút đăng nhập (đã tìm thấy tên là guna2Button1)
            guna2Button1.Click += guna2Button1_Click;

            // Khởi tạo kết nối SQL
            sqlcon = new SqlConnection(connectionString);
            // Bỏ MessageBox kiểm tra kết nối ở đây để không làm phiền người dùng mỗi lần mở form
            // try
            // {
            //     sqlcon.Open();
            //     MessageBox.Show("Kết nối SQL thành công!");
            //     sqlcon.Close();
            // }
            // catch (Exception ex)
            // {
            //     MessageBox.Show("Lỗi kết nối SQL: " + ex.Message);
            // }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            originalWidth = this.Width;
            originalHeight = this.Height;

            SaveControlBounds(this);
            isResizingInitialized = true;
            StoreOriginalBounds(this);
        }

        private void SaveControlBounds(Control parentControl)
        {
            foreach (Control control in parentControl.Controls)
            {
                if (!controlOriginalBounds.ContainsKey(control))
                {
                    controlOriginalBounds[control] = new Rectangle(
                        control.Location.X,
                        control.Location.Y,
                        control.Width,
                        control.Height
                    );

                    controlOriginalFonts[control] = control.Font;
                }

                if (control.Controls.Count > 0)
                {
                    SaveControlBounds(control);
                }
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (!isResizingInitialized || originalWidth == 0 || originalHeight == 0)
                return;

            float widthRatio = (float)this.Width / originalWidth;
            float heightRatio = (float)this.Height / originalHeight;

            ResizeAllControls(this, widthRatio, heightRatio);

            // Căn giữa panelLogin nếu có
            panelLogin.Left = (this.ClientSize.Width - panelLogin.Width) / 2;
            panelLogin.Top = (this.ClientSize.Height - panelLogin.Height) / 2;
        }

        private void ResizeControl(Control control, float widthRatio, float heightRatio)
        {
            Rectangle originalBounds = controlOriginalBounds[control];

            int newX = (int)(originalBounds.X * widthRatio);
            int newY = (int)(originalBounds.Y * heightRatio);
            int newWidth = (int)(originalBounds.Width * widthRatio);
            int newHeight = (int)(originalBounds.Height * heightRatio);

            int maxWidth = (int)(originalBounds.Width * 2.5f);
            int maxHeight = (int)(originalBounds.Height * 2.5f);
            newWidth = Math.Min(newWidth, maxWidth);
            newHeight = Math.Min(newHeight, maxHeight);

            control.Location = new Point(newX, newY);
            control.Size = new Size(newWidth, newHeight);

            // Lấy font gốc
            Font originalFont = controlOriginalFonts.ContainsKey(control) ? controlOriginalFonts[control] : control.Font;
            float newFontSize = originalFont.Size * Math.Min(widthRatio, heightRatio);
            // Clamp font size (ví dụ min 8, max 24)
            newFontSize = Math.Max(8, Math.Min(newFontSize, 24));

            if (control is TextBox textBox)
            {
                textBox.Font = new Font(originalFont.FontFamily, newFontSize, originalFont.Style);
            }
            else if (control is Button button)
            {
                button.Font = new Font(originalFont.FontFamily, newFontSize, originalFont.Style);
            }
            else if (control is Label label)
            {
                label.Font = new Font(originalFont.FontFamily, newFontSize, originalFont.Style);
            }
            else if (control is Guna.UI2.WinForms.Guna2Button gunaButton)
            {
                gunaButton.Font = new Font(originalFont.FontFamily, newFontSize, originalFont.Style);
            }
            else if (control is Guna.UI2.WinForms.Guna2TextBox gunaTextBox)
            {
                gunaTextBox.Font = new Font(originalFont.FontFamily, newFontSize, originalFont.Style);
            }
            else if (control is PictureBox pictureBox)
            {
                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox.Font = new Font(originalFont.FontFamily, newFontSize, originalFont.Style);
            }
            else if (control is Guna.UI2.WinForms.Guna2DataGridView gunaDgv)
            {
                gunaDgv.Font = new Font(originalFont.FontFamily, newFontSize, originalFont.Style);
                foreach (DataGridViewColumn column in gunaDgv.Columns)
                {
                    column.Width = (int)(column.Width * widthRatio);
                }
            }
            else if (control is Guna.UI2.WinForms.Guna2HtmlLabel gunaLabel)
            {
                gunaLabel.Font = new Font(originalFont.FontFamily, newFontSize, originalFont.Style);
            }
        }

        private void ResizeAllControls(Control parentControl, float widthRatio, float heightRatio)
        {
            foreach (Control control in parentControl.Controls)
            {
                if (controlOriginalBounds.ContainsKey(control))
                {
                    Rectangle originalBounds = controlOriginalBounds[control];
                    int newX = (int)(originalBounds.X * widthRatio);
                    int newY = (int)(originalBounds.Y * heightRatio);
                    int newWidth = (int)(originalBounds.Width * widthRatio);
                    int newHeight = (int)(originalBounds.Height * heightRatio);
                    control.Bounds = new Rectangle(newX, newY, newWidth, newHeight);

                    // Giới hạn font theo chiều cao
                    if (controlOriginalFonts.ContainsKey(control))
                    {
                        Font originalFont = controlOriginalFonts[control];
                        float ratio = Math.Min(widthRatio, heightRatio);

                        // Cố gắng giữ font trong giới hạn dễ nhìn
                        float newFontSize = Math.Max(8, Math.Min(originalFont.Size * ratio, 24));
                        control.Font = new Font(originalFont.FontFamily, newFontSize, originalFont.Style);
                    }
                }

                if (control.Controls.Count > 0)
                {
                    ResizeAllControls(control, widthRatio, heightRatio);
                    ResizeControl(control, widthRatio, heightRatio);
                }
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            int loggedInNhanVienId = -1;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                SELECT tk.role, nv.IDNhanVien
                FROM TaiKhoan tk
                LEFT JOIN NhanVien nv ON tk.username = nv.username
                WHERE tk.username = @username AND tk.password = @password
            ";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read()) // Đăng nhập thành công
                    {
                        string role = reader["role"].ToString();
                        if (reader["IDNhanVien"] != DBNull.Value)
                        {
                            loggedInNhanVienId = Convert.ToInt32(reader["IDNhanVien"]);
                        }
                        isReturningFromLogout = false;

                        this.Hide();

                        if (role == "Nhân viên")
                        {
                            using (var nvForm = new chucnangnhanvien(loggedInNhanVienId))
                            {
                                var result = nvForm.ShowDialog();
                                if (result == DialogResult.Cancel) // Đăng xuất
                                {
                                    isReturningFromLogout = true;
                                    //txtUsername.Clear();
                                    txtPassword.Clear();
                                    this.Show();
                                }
                                else
                                {
                                    this.Close(); // Thoát chương trình
                                }
                            }
                        }
                        else if (role == "Quản lý")
                        {
                            using (var ql = new chucnangquanli())
                            {
                                var result = ql.ShowDialog();
                                if (result == DialogResult.Cancel)
                                {
                                    isReturningFromLogout = true;
                                    //txtUsername.Clear();
                                    txtPassword.Clear();
                                    this.Show();
                                }
                                else
                                {
                                    this.Close();
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Không xác định vai trò cho tài khoản này.", "Lỗi Đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            this.Show();
                        }
                    }
                    else
                    {
                        if (!isReturningFromLogout)
                        {
                            MessageBox.Show("Sai tài khoản hoặc mật khẩu.", "Lỗi Đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        txtPassword.Clear();
                        txtUsername.Focus();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi trong quá trình đăng nhập: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = true;
        }
    }
}