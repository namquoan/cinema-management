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

namespace Pbl_test
{
    public partial class Form1 : Form
    {
        // Chuỗi kết nối SQL Server
        private string strcon = @"Data Source=DESKTOP-24LGCGN;Initial Catalog=POBOLO;Integrated Security=True;TrustServerCertificate=True";
        private SqlConnection sqlcon;

        public Form1()
        {
            InitializeComponent();

            // Khởi tạo kết nối SQL
            sqlcon = new SqlConnection(strcon);
            try
            {
                sqlcon.Open();
                MessageBox.Show("Kết nối SQL thành công!");
                sqlcon.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối SQL: " + ex.Message);
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            chucnangquanli f = new chucnangquanli();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            using (SqlConnection conn = new SqlConnection(strcon))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM TaiKhoan WHERE username = @username AND password = @password";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string role = reader["role"].ToString();

                        // Mở form theo vai trò
                        if (role == "Nhân viên")
                        {
                            chucnangnhanvien nv = new chucnangnhanvien();
                            this.Hide();
                            nv.ShowDialog();
                            this.Show();
                        }
                        else if (role == "Quản lý")
                        {
                            chucnangquanli ql = new chucnangquanli();
                            this.Hide();
                            ql.ShowDialog();
                            this.Show();
                        }
                        else
                        {
                            MessageBox.Show("Không xác định vai trò.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Sai tài khoản hoặc mật khẩu.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
            this.Hide();
            f.ShowDialog();
            this.Show();
        }
    }
}
