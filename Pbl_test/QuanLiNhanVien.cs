using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;

namespace Pbl_test
{
    public partial class QuanLiNhanVien : UserControl
    {
        public QuanLiNhanVien()
        {
            InitializeComponent();
            this.Load += QuanLiNhanVien_Load;
            this.VisibleChanged += QuanLiNhanVien_VisibleChanged;
            guna2DataGridView1.CellClick += guna2DataGridView1_CellClick;
        }

        private void TestConnection()
        {
            try
            {
                string connectionString = @"Data Source=DESKTOP-24LGCGN;Initial Catalog=POBOLO;Integrated Security=True;TrustServerCertificate=True";
                //using (SqlConnection conn = new SqlConnection(connectionString))
                //{
                //    conn.Open();
                //    MessageBox.Show("Kết nối thành công!");
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
            }
        }

        private void LoadData()
        {
            try
            {
                string connectionString = @"Data Source=LAPTOP-R3BTDCFC\SQLEXPRESS;Initial Catalog=pbl3_test01;Integrated Security=True;TrustServerCertificate=True";
                string query = @"
                    SELECT 
                        tk.Ten AS hoTen,
                        tk.NgaySinh,
                        tk.GioiTinh,
                        tk.SoDienThoai,
                        tk.SoChungMinh AS CCCD,
                        tk.username AS UserName,
                        tk.password AS PassWord
                    FROM TaiKhoan tk
                    JOIN NhanVien nv ON tk.username = nv.username";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    //string dataCheck = "Dữ liệu trong DataTable:\n";
                    //foreach (DataRow row in dt.Rows)
                    //{
                    //    dataCheck += $"Họ Tên: {row["hoTen"]}, Ngày Sinh: {row["NgaySinh"]}, Username: {row["UserName"]}\n";
                    //}
                    //MessageBox.Show(dataCheck);

                    //MessageBox.Show($"Số dòng dữ liệu: {dt.Rows.Count}");

                    guna2DataGridView1.AutoGenerateColumns = false;
                    guna2DataGridView1.Rows.Clear();
                    foreach (DataRow row in dt.Rows)
                    {
                        guna2DataGridView1.Rows.Add(
                            row["hoTen"].ToString(),
                            row["NgaySinh"].ToString(),
                            row["GioiTinh"].ToString(),
                            row["SoDienThoai"].ToString(),
                            row["CCCD"].ToString(),
                            row["UserName"].ToString(),
                            row["PassWord"].ToString()
                        );
                    }
                    guna2DataGridView1.Refresh();

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("Không có dữ liệu để hiển thị.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message);
            }
        }

        private void QuanLiNhanVien_Load(object sender, EventArgs e)
        {
            TestConnection();

            if (guna2DataGridView1.Columns.Count == 0)
            {
                guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "hoTen",
                    HeaderText = "Họ Tên",
                    DataPropertyName = "hoTen",
                    Visible = true,
                    Width = 100
                });
                guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "NgaySinh",
                    HeaderText = "Ngày Sinh",
                    DataPropertyName = "NgaySinh",
                    Visible = true,
                    Width = 100
                });
                guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "GioiTinh",
                    HeaderText = "Giới Tính",
                    DataPropertyName = "GioiTinh",
                    Visible = true,
                    Width = 80
                });
                guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "SoDienThoai",
                    HeaderText = "Số Điện Thoại",
                    DataPropertyName = "SoDienThoai",
                    Visible = true,
                    Width = 100
                });
                guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "CCCD",
                    HeaderText = "CCCD",
                    DataPropertyName = "CCCD",
                    Visible = true,
                    Width = 100
                });
                guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "UserName",
                    HeaderText = "UserName",
                    DataPropertyName = "UserName",
                    Visible = true,
                    Width = 100
                });
                guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "PassWord",
                    HeaderText = "PassWord",
                    DataPropertyName = "PassWord",
                    Visible = true,
                    Width = 100
                });
            }

            LoadData();
        }

        private void QuanLiNhanVien_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible && guna2DataGridView1.RowCount == 0)
            {
                LoadData();
            }
        }

        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = guna2DataGridView1.Rows[e.RowIndex];

                txthoTen.Text = row.Cells["hoTen"].Value?.ToString();
                txtngaySinh.Text = row.Cells["NgaySinh"].Value?.ToString();
                txtgioiTinh.Text = row.Cells["GioiTinh"].Value?.ToString();
                txtsoDienThoai.Text = row.Cells["SoDienThoai"].Value?.ToString();
                txtCCCD.Text = row.Cells["CCCD"].Value?.ToString();
                txtusername.Text = row.Cells["UserName"].Value?.ToString();
                txtpassword.Text = row.Cells["PassWord"].Value?.ToString();
            }
        }

        //private void btnThem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        string connectionString = @"Data Source=LAPTOP-R3BTDCFC\SQLEXPRESS;Initial Catalog=pbl3_test01;Integrated Security=True;TrustServerCertificate=True";
        //        using (SqlConnection conn = new SqlConnection(connectionString))
        //        {
        //            conn.Open();

        //            string queryTaiKhoan = @"
        //        INSERT INTO TaiKhoan (Ten, NgaySinh, GioiTinh, SoDienThoai, SoChungMinh, username, password)
        //        VALUES (@Ten, @NgaySinh, @GioiTinh, @SoDienThoai, @SoChungMinh, @username, @password)";
        //            SqlCommand cmdTaiKhoan = new SqlCommand(queryTaiKhoan, conn);
        //            cmdTaiKhoan.Parameters.AddWithValue("@Ten", txthoTen.Text);
        //            cmdTaiKhoan.Parameters.AddWithValue("@NgaySinh", txtngaySinh.Text);
        //            cmdTaiKhoan.Parameters.AddWithValue("@GioiTinh", txtgioiTinh.Text);
        //            cmdTaiKhoan.Parameters.AddWithValue("@SoDienThoai", txtsoDienThoai.Text);
        //            cmdTaiKhoan.Parameters.AddWithValue("@SoChungMinh", txtCCCD.Text);
        //            cmdTaiKhoan.Parameters.AddWithValue("@username", txtusername.Text);
        //            cmdTaiKhoan.Parameters.AddWithValue("@password", txtpassword.Text);
        //            int rowsAffectedTaiKhoan = cmdTaiKhoan.ExecuteNonQuery();

        //            if (rowsAffectedTaiKhoan > 0)
        //            {
        //                MessageBox.Show("Thêm vào TaiKhoan thành công!");
        //            }
        //            else
        //            {
        //                MessageBox.Show("Không có dòng nào được thêm vào TaiKhoan.");
        //            }

        //            string queryNhanVien = "INSERT INTO NhanVien (username) VALUES (@username)";
        //            SqlCommand cmdNhanVien = new SqlCommand(queryNhanVien, conn);
        //            cmdNhanVien.Parameters.AddWithValue("@username", txtusername.Text);
        //            int rowsAffectedNhanVien = cmdNhanVien.ExecuteNonQuery();

        //            if (rowsAffectedNhanVien > 0)
        //            {
        //                MessageBox.Show("Thêm vào NhanVien thành công!");
        //            }
        //            else
        //            {
        //                MessageBox.Show("Không có dòng nào được thêm vào NhanVien.");
        //            }

        //            LoadData();

        //            txthoTen.Clear();
        //            txtngaySinh.Clear();
        //            txtgioiTinh.Clear();
        //            txtsoDienThoai.Clear();
        //            txtCCCD.Clear();
        //            txtusername.Clear();
        //            txtpassword.Clear();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Lỗi khi thêm dữ liệu: " + ex.Message);
        //    }
        //}

        private void guna2HtmlLabel6_Click(object sender, EventArgs e) { }
        private void guna2TextBox6_TextChanged(object sender, EventArgs e) { }
        private void guna2HtmlLabel1_Click(object sender, EventArgs e) { }
        private void btnTim_Click(object sender, EventArgs e) { }
        private void txtTim_TextChanged(object sender, EventArgs e) { }
        private void btnlayAnh_Click(object sender, EventArgs e) { }
        private bool CheckIfUsernameExists(string username, SqlConnection conn)
        {
            string checkQuery = "SELECT COUNT(*) FROM TaiKhoan WHERE username = @username";
            SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@username", username);
            int count = (int)checkCmd.ExecuteScalar();
            return count > 0;
        }
        private bool CheckIfCCCDExists(string cccd, SqlConnection conn)
        {
            string checkQuery = "SELECT COUNT(*) FROM TaiKhoan WHERE SoChungMinh = @SoChungMinh";
            SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@SoChungMinh", cccd);
            int count = (int)checkCmd.ExecuteScalar();
            return count > 0;
        }
        private bool CheckIfPhoneExists(string phone, SqlConnection conn)
        {
            string checkQuery = "SELECT COUNT(*) FROM TaiKhoan WHERE SoDienThoai = @SoDienThoai";
            SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@SoDienThoai", phone);
            int count = (int)checkCmd.ExecuteScalar();
            return count > 0;
        }
        private void btnThem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Sự kiện btnThem_Click được gọi!");

            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(txthoTen.Text) || string.IsNullOrWhiteSpace(txtngaySinh.Text) ||
                    string.IsNullOrWhiteSpace(txtgioiTinh.Text) || string.IsNullOrWhiteSpace(txtsoDienThoai.Text) ||
                    string.IsNullOrWhiteSpace(txtCCCD.Text) || string.IsNullOrWhiteSpace(txtusername.Text) ||
                    string.IsNullOrWhiteSpace(txtpassword.Text))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!");
                    return;
                }

                // Kiểm tra định dạng ngày sinh
                DateTime ngaySinh;
                if (!DateTime.TryParse(txtngaySinh.Text, out ngaySinh))
                {
                    MessageBox.Show("Ngày sinh không hợp lệ! Vui lòng nhập theo định dạng MM/dd/yyyy.");
                    return;
                }

                string connectionString = @"Data Source=LAPTOP-R3BTDCFC\SQLEXPRESS;Initial Catalog=pbl3_test01;Integrated Security=True;TrustServerCertificate=True";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Kiểm tra username đã tồn tại chưa
                    if (CheckIfUsernameExists(txtusername.Text, conn))
                    {
                        MessageBox.Show("Username đã tồn tại! Vui lòng chọn username khác.");
                        return;
                    }

                    // Kiểm tra CCCD đã tồn tại chưa
                    if (CheckIfCCCDExists(txtCCCD.Text, conn))
                    {
                        MessageBox.Show("CCCD đã tồn tại! Vui lòng nhập CCCD khác.");
                        return;
                    }
                    if (CheckIfPhoneExists(txtsoDienThoai.Text, conn))
                    {
                        MessageBox.Show("Số điện thoại đã tồn tại! Vui lòng nhập số điện thoại khác.");
                        return;
                    }

                    string queryTaiKhoan = @"
                INSERT INTO TaiKhoan (Ten, NgaySinh, GioiTinh, SoDienThoai, SoChungMinh, username, password, role)
                VALUES (@Ten, @NgaySinh, @GioiTinh, @SoDienThoai, @SoChungMinh, @username, @password, @role)";
                    SqlCommand cmdTaiKhoan = new SqlCommand(queryTaiKhoan, conn);
                    cmdTaiKhoan.Parameters.AddWithValue("@Ten", txthoTen.Text);
                    cmdTaiKhoan.Parameters.AddWithValue("@NgaySinh", ngaySinh.ToString("yyyy-MM-dd"));
                    cmdTaiKhoan.Parameters.AddWithValue("@GioiTinh", txtgioiTinh.Text);
                    cmdTaiKhoan.Parameters.AddWithValue("@SoDienThoai", txtsoDienThoai.Text);
                    cmdTaiKhoan.Parameters.AddWithValue("@SoChungMinh", txtCCCD.Text);
                    cmdTaiKhoan.Parameters.AddWithValue("@username", txtusername.Text);
                    cmdTaiKhoan.Parameters.AddWithValue("@password", txtpassword.Text);
                    cmdTaiKhoan.Parameters.AddWithValue("@role", "Nhân viên");
                    int rowsAffectedTaiKhoan = cmdTaiKhoan.ExecuteNonQuery();

                    if (rowsAffectedTaiKhoan > 0)
                    {
                        MessageBox.Show("Thêm vào TaiKhoan thành công!");
                    }
                    else
                    {
                        MessageBox.Show("Không có dòng nào được thêm vào TaiKhoan.");
                        return;
                    }

                    string queryNhanVien = "INSERT INTO NhanVien (username) VALUES (@username)";
                    SqlCommand cmdNhanVien = new SqlCommand(queryNhanVien, conn);
                    cmdNhanVien.Parameters.AddWithValue("@username", txtusername.Text);
                    int rowsAffectedNhanVien = cmdNhanVien.ExecuteNonQuery();

                    if (rowsAffectedNhanVien > 0)
                    {
                        MessageBox.Show("Thêm vào NhanVien thành công!");
                    }
                    else
                    {
                        MessageBox.Show("Không có dòng nào được thêm vào NhanVien.");
                        return;
                    }

                    LoadData();

                    txthoTen.Clear();
                    txtngaySinh.Clear();
                    txtgioiTinh.Clear();
                    txtsoDienThoai.Clear();
                    txtCCCD.Clear();
                    txtusername.Clear();
                    txtpassword.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm dữ liệu: " + ex.Message);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra xem đã chọn nhân viên để xóa chưa
                if (string.IsNullOrWhiteSpace(txtusername.Text))
                {
                    MessageBox.Show("Vui lòng chọn một nhân viên để xóa (điền username)!");
                    return;
                }

                // Xác nhận trước khi xóa
                DialogResult result = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhân viên với username '{txtusername.Text}'?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                {
                    return;
                }

                string connectionString = @"Data Source=LAPTOP-R3BTDCFC\SQLEXPRESS;Initial Catalog=pbl3_test01;Integrated Security=True;TrustServerCertificate=True";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Xóa bản ghi trong bảng NhanVien trước
                    string queryNhanVien = "DELETE FROM NhanVien WHERE username = @username";
                    SqlCommand cmdNhanVien = new SqlCommand(queryNhanVien, conn);
                    cmdNhanVien.Parameters.AddWithValue("@username", txtusername.Text);
                    int rowsAffectedNhanVien = cmdNhanVien.ExecuteNonQuery();

                    // Xóa bản ghi trong bảng TaiKhoan
                    string queryTaiKhoan = "DELETE FROM TaiKhoan WHERE username = @username";
                    SqlCommand cmdTaiKhoan = new SqlCommand(queryTaiKhoan, conn);
                    cmdTaiKhoan.Parameters.AddWithValue("@username", txtusername.Text);
                    int rowsAffectedTaiKhoan = cmdTaiKhoan.ExecuteNonQuery();

                    // Kiểm tra xem có xóa thành công không
                    if (rowsAffectedNhanVien > 0 && rowsAffectedTaiKhoan > 0)
                    {
                        MessageBox.Show("Xóa nhân viên thành công!");
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy nhân viên để xóa!");
                        return;
                    }

                    // Làm mới giao diện
                    LoadData();

                    // Xóa nội dung các TextBox
                    txthoTen.Clear();
                    txtngaySinh.Clear();
                    txtgioiTinh.Clear();
                    txtsoDienThoai.Clear();
                    txtCCCD.Clear();
                    txtusername.Clear();
                    txtpassword.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa dữ liệu: " + ex.Message);
            }
        }
    }
}