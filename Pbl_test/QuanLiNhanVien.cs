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
        		private string connectionString = @"Data Source=DESKTOP-HHKF7O6;Initial Catalog=POBOLO;Integrated Security=True;TrustServerCertificate=True";
        private float originalWidth;
        private float originalHeight;
        private Dictionary<Control, Rectangle> controlOriginalBounds = new Dictionary<Control, Rectangle>();
        private Dictionary<Control, Font> controlOriginalFonts = new Dictionary<Control, Font>();
        private bool isResizingInitialized = false;
        private string oldUsername = "";

        public QuanLiNhanVien()
        {
            InitializeComponent();
            this.Load += QuanLiNhanVien_Load;
            this.SizeChanged += QuanLiNhanVien_SizeChanged;
            this.VisibleChanged += QuanLiNhanVien_VisibleChanged;
            guna2DataGridView1.CellClick += guna2DataGridView1_CellClick;

            this.Load += QuanLiNhanVien_Load;
            this.Resize += QuanLiNhanVien_Resize;
        }
        private void EnsureSingleSearchBox()
        {
            try
            {
                if (guna2Panel2 == null || txtTim == null) return;
                // Bring the intended search box to front
                txtTim.BringToFront();
                // Hide any other text boxes that overlap the search box area
                foreach (Control c in guna2Panel2.Controls)
                {
                    if (c == txtTim) continue;
                    // Consider both standard and Guna2 text boxes
                    bool isTextbox = c is TextBoxBase || c.GetType().Name.Contains("TextBox");
                    if (!isTextbox) continue;
                    if (c.Bounds.IntersectsWith(txtTim.Bounds))
                    {
                        c.Visible = false;
                    }
                }
            }
            catch { /* no-op */ }
        }
        private void QuanLiNhanVien_Resize(object sender, EventArgs e)
        {
            if (!isResizingInitialized || originalWidth == 0 || originalHeight == 0) return;
            float widthRatio = (float)this.Width / originalWidth;
            float heightRatio = (float)this.Height / originalHeight;
            ResizeControls(this, widthRatio, heightRatio);
            EnsureSingleSearchBox();
        }
        private void SaveOriginalBounds(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                controlOriginalBounds[control] = new Rectangle(control.Location, control.Size);
                controlOriginalFonts[control] = control.Font;
                if (control.Controls.Count > 0)
                {
                    SaveOriginalBounds(control);
                }
            }
        }
        private void ResizeControls(Control parent, float widthRatio, float heightRatio)
        {
            foreach (Control control in parent.Controls)
            {
                if (controlOriginalBounds.ContainsKey(control))
                {
                    Rectangle original = controlOriginalBounds[control];
                    int newX = (int)(original.X * widthRatio);
                    int newY = (int)(original.Y * heightRatio);
                    int newWidth = (int)(original.Width * widthRatio);
                    int newHeight = (int)(original.Height * heightRatio);
                    control.Location = new Point(newX, newY);
                    control.Size = new Size(newWidth, newHeight);

                    // Resize font
                    if (controlOriginalFonts.ContainsKey(control))
                    {
                        Font originalFont = controlOriginalFonts[control];
                        float newFontSize = Math.Max(8, Math.Min(originalFont.Size * Math.Min(widthRatio, heightRatio), 24));
                        control.Font = new Font(originalFont.FontFamily, newFontSize, originalFont.Style);
                    }

                    if (control is DataGridView dgv)
                    {
                        dgv.Font = new Font(control.Font.FontFamily, control.Font.Size, control.Font.Style);
                    }
                }
                if (control.Controls.Count > 0)
                {
                    ResizeControls(control, widthRatio, heightRatio);
                }
            }
        }
        private void QuanLiNhanVien_Load(object sender, EventArgs e)
        {
            originalWidth = this.Width;
            originalHeight = this.Height;
            controlOriginalBounds.Clear();
            controlOriginalFonts.Clear();
            SaveOriginalBounds(this);
            isResizingInitialized = true;
            LoadData();
            guna2DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            EnsureSingleSearchBox();
        }

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
        private void QuanLiNhanVien_SizeChanged(object sender, EventArgs e)
        {
            if (!isResizingInitialized || originalWidth == 0 || originalHeight == 0)
                return;
            float widthRatio = (float)this.Width / originalWidth;
            float heightRatio = (float)this.Height / originalHeight;
            ResizeAllControls(this, widthRatio, heightRatio);
        }
        private void ResizeAllControls(Control parent, float widthRatio, float heightRatio)
        {
            foreach (Control control in parent.Controls)
            {
                if (controlOriginalBounds.ContainsKey(control))
                {
                    Rectangle originalBounds = controlOriginalBounds[control];
                    int newX = (int)(originalBounds.X * widthRatio);
                    int newY = (int)(originalBounds.Y * heightRatio);
                    int newWidth = (int)(originalBounds.Width * widthRatio);
                    int newHeight = (int)(originalBounds.Height * heightRatio);
                    control.Bounds = new Rectangle(newX, newY, newWidth, newHeight);

                    // Resize font
                    if (controlOriginalFonts.ContainsKey(control))
                    {
                        Font originalFont = controlOriginalFonts[control];
                        float ratio = Math.Min(widthRatio, heightRatio);
                        float newFontSize = Math.Max(8, Math.Min(originalFont.Size * ratio, 24));
                        control.Font = new Font(originalFont.FontFamily, newFontSize, originalFont.Style);
                    }

                    // Đặc biệt cho DataGridView
                    if (control is DataGridView dgv)
                    {
                        dgv.Font = new Font(control.Font.FontFamily, control.Font.Size, control.Font.Style);
                    }
                }
                if (control.HasChildren)
                {
                    ResizeAllControls(control, widthRatio, heightRatio);
                }
            }
        }

        private void QuanLiNhanVien_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible && guna2DataGridView1.RowCount == 0)
            {
                LoadData();
            }
            EnsureSingleSearchBox();
        }

        //private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        //{
        //    if (e.RowIndex >= 0)
        //    {
        //        DataGridViewRow row = guna2DataGridView1.Rows[e.RowIndex];
        //        txthoTen.Text = row.Cells["hoTen"].Value?.ToString();
        //        txtngaySinh.Text = row.Cells["NgaySinh"].Value?.ToString();
        //        txtgioiTinh.Text = row.Cells["GioiTinh"].Value?.ToString();
        //        txtsoDienThoai.Text = row.Cells["SoDienThoai"].Value?.ToString();
        //        txtCCCD.Text = row.Cells["CCCD"].Value?.ToString();
        //        txtusername.Text = row.Cells["UserName"].Value?.ToString();
        //        txtpassword.Text = row.Cells["PassWord"].Value?.ToString();
        //    }
        //    string username = txtusername.Text;
        //    if (!string.IsNullOrEmpty(username))
        //    {
        //        using (SqlConnection conn = new SqlConnection(connectionString))
        //        {
        //            conn.Open();
        //            string query = "SELECT AnhDaiDien FROM TaiKhoan WHERE username = @Username";
        //            using (SqlCommand cmd = new SqlCommand(query, conn))
        //            {
        //                cmd.Parameters.AddWithValue("@Username", username);
        //                using (SqlDataReader reader = cmd.ExecuteReader())
        //                {
        //                    if (reader.Read() && reader["AnhDaiDien"] != DBNull.Value && reader["AnhDaiDien"] is byte[])
        //                    {
        //                        byte[] imageBytes = (byte[])reader["AnhDaiDien"];
        //                        using (MemoryStream ms = new MemoryStream(imageBytes))
        //                        {
        //                            guna2PictureBox1.Image?.Dispose(); //xoa anh cu
        //                            guna2PictureBox1.Image = Image.FromStream(ms);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        guna2PictureBox1.Image?.Dispose();
        //                        guna2PictureBox1.Image = null;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = guna2DataGridView1.Rows[e.RowIndex];
                txthoTen.Text = row.Cells["hoTen"].Value?.ToString();
                // Sửa dòng này để chỉ lấy ngày (không lấy giờ)
                if (row.Cells["NgaySinh"].Value != null && DateTime.TryParse(row.Cells["NgaySinh"].Value.ToString(), out DateTime ngaySinh))
                {
                    txtngaySinh.Text = ngaySinh.ToString("dd/MM/yyyy");
                }
                else
                {
                    txtngaySinh.Text = "";
                }
                txtgioiTinh.Text = row.Cells["GioiTinh"].Value?.ToString();
                txtsoDienThoai.Text = row.Cells["SoDienThoai"].Value?.ToString();
                txtCCCD.Text = row.Cells["CCCD"].Value?.ToString();
                txtusername.Text = row.Cells["UserName"].Value?.ToString();
                txtpassword.Text = row.Cells["PassWord"].Value?.ToString();
                oldUsername = txtusername.Text;
            }
            string username = txtusername.Text;
            if (!string.IsNullOrEmpty(username))
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"IF COL_LENGTH('TaiKhoan','AnhDaiDien') IS NOT NULL
BEGIN
    SELECT AnhDaiDien FROM TaiKhoan WHERE username = @Username
END
ELSE
BEGIN
    SELECT CAST(NULL AS VARBINARY(MAX)) AS AnhDaiDien WHERE 1=0
END";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read() && reader["AnhDaiDien"] != DBNull.Value && reader["AnhDaiDien"] is byte[])
                            {
                                byte[] imageBytes = (byte[])reader["AnhDaiDien"];
                                using (MemoryStream ms = new MemoryStream(imageBytes))
                                {
                                    guna2PictureBox1.Image?.Dispose(); //xoa anh cu
                                    guna2PictureBox1.Image = Image.FromStream(ms);
                                }
                            }
                            else
                            {
                                guna2PictureBox1.Image?.Dispose();
                                guna2PictureBox1.Image = null;
                            }
                        }
                    }
                }
            }
        }

        private void LoadData()
        {
            try
            {
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

                    guna2DataGridView1.AutoGenerateColumns = false;
                    guna2DataGridView1.Rows.Clear();
                    foreach (DataRow row in dt.Rows)
                    {
                        string ngaySinhDisplay = string.Empty;
                        if (row["NgaySinh"] != DBNull.Value && DateTime.TryParse(row["NgaySinh"].ToString(), out DateTime ngaySinhDt))
                        {
                            ngaySinhDisplay = ngaySinhDt.ToString("dd/MM/yyyy");
                        }
                        guna2DataGridView1.Rows.Add(
                            row["hoTen"].ToString(),
                            ngaySinhDisplay,
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

                    // Evenly distribute column widths and center content
                    if (guna2DataGridView1.Columns.Count > 0)
                    {
                        foreach (DataGridViewColumn col in guna2DataGridView1.Columns)
                        {
                            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                            col.FillWeight = 1; // equal share
                            col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            // Nudge header text slightly right to account for sort arrow/glyph
                            col.HeaderCell.Style.Padding = new Padding(8, 0, 16, 0);
                            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message);
            }
        }

        private void guna2HtmlLabel6_Click(object sender, EventArgs e) { }
        private void guna2TextBox6_TextChanged(object sender, EventArgs e) { }
        private void guna2HtmlLabel1_Click(object sender, EventArgs e) { }

        private void btnTim_Click(object sender, EventArgs e)
        {
            string tenCanTim = txtTim.Text.ToLower().Trim();

            foreach (DataGridViewRow row in guna2DataGridView1.Rows)
            {
                if (row.IsNewRow)
                    continue;
                if (row.Cells["hoTen"].Value != null &&
                    row.Cells["hoTen"].Value.ToString().ToLower().Contains(tenCanTim))
                {
                    row.Visible = true;
                }
                else
                {
                    row.Visible = false;
                }
            }
        }

        private void txtTim_TextChanged(object sender, EventArgs e) { }
        private void btnlayAnh_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtusername.Text))
            {
                MessageBox.Show("Vui lòng nhập hoặc chọn username!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files (.jpg; *.jpeg; *.png)|.jpg; *.jpeg; *.png";
                openFileDialog.Title = "Chọn ảnh đại diện nhân viên";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = openFileDialog.FileName;
                        using (Image image = Image.FromFile(filePath))
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                image.Save(ms, image.RawFormat);
                                byte[] imageBytes = ms.ToArray();

                                using (SqlConnection connection = new SqlConnection(connectionString))
                                {
                                    connection.Open();
                                    string query = @"IF COL_LENGTH('TaiKhoan','AnhDaiDien') IS NOT NULL
BEGIN
    EXEC sp_executesql N'UPDATE TaiKhoan SET AnhDaiDien = @Image WHERE username = @Username', N'@Image varbinary(max), @Username nvarchar(255)', @Image=@Image, @Username=@Username
END";
                                    using (SqlCommand cmd = new SqlCommand(query, connection))
                                    {
                                        cmd.Parameters.AddWithValue("@Image", imageBytes);
                                        cmd.Parameters.AddWithValue("@Username", txtusername.Text.Trim());

                                        int result = cmd.ExecuteNonQuery();
                                        if (result > 0)
                                        {
                                            MessageBox.Show("Cập nhật ảnh đại diện thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else
                                        {
                                            MessageBox.Show("Không tìm thấy tài khoản!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi lưu ảnh: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
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

        private void ntbReset_Click(object sender, EventArgs e)
        {
            LoadData();
        }
        private void btnSua_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtusername.Text))
                {
                    MessageBox.Show("Vui lòng chọn một nhân viên để sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txthoTen.Text) ||
                    string.IsNullOrWhiteSpace(txtngaySinh.Text) ||
                    string.IsNullOrWhiteSpace(txtgioiTinh.Text) ||
                    string.IsNullOrWhiteSpace(txtsoDienThoai.Text) ||
                    string.IsNullOrWhiteSpace(txtCCCD.Text) ||
                    string.IsNullOrWhiteSpace(txtpassword.Text))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
        UPDATE TaiKhoan
        SET Ten = @Ten,
            NgaySinh = @NgaySinh,
            GioiTinh = @GioiTinh,
            SoDienThoai = @SoDienThoai,
            SoChungMinh = @CCCD,
            password = @PassWord,
            username = @UserName
        WHERE username = @OldUserName";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        DateTime ngaySinh;
                        if (!DateTime.TryParseExact(txtngaySinh.Text, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out ngaySinh))
                        {
                            MessageBox.Show("Ngày sinh không hợp lệ! Định dạng đúng: dd/MM/yyyy");
                            return;
                        }
                        cmd.Parameters.AddWithValue("@Ten", txthoTen.Text);
                        cmd.Parameters.AddWithValue("@NgaySinh", ngaySinh.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@GioiTinh", txtgioiTinh.Text);
                        cmd.Parameters.AddWithValue("@SoDienThoai", txtsoDienThoai.Text);
                        cmd.Parameters.AddWithValue("@CCCD", txtCCCD.Text);
                        cmd.Parameters.AddWithValue("@PassWord", txtpassword.Text);
                        cmd.Parameters.AddWithValue("@UserName", txtusername.Text);
                        cmd.Parameters.AddWithValue("@OldUserName", oldUsername);

                        try
                        {
                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Cập nhật nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Không tìm thấy nhân viên để cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        catch (SqlException ex)
                        {
                            if (ex.Message.Contains("REFERENCE constraint") && ex.Message.Contains("FK_NhanVien_TaiKhoan"))
                            {
                                MessageBox.Show("Không thể sửa username!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                MessageBox.Show("Lỗi khi cập nhật nhân viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật nhân viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSua_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtusername.Text))
                {
                    MessageBox.Show("Vui lòng chọn một nhân viên để sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txthoTen.Text) ||
                    string.IsNullOrWhiteSpace(txtngaySinh.Text) ||
                    string.IsNullOrWhiteSpace(txtgioiTinh.Text) ||
                    string.IsNullOrWhiteSpace(txtsoDienThoai.Text) ||
                    string.IsNullOrWhiteSpace(txtCCCD.Text) ||
                    string.IsNullOrWhiteSpace(txtpassword.Text))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
        UPDATE TaiKhoan
        SET Ten = @Ten,
            NgaySinh = @NgaySinh,
            GioiTinh = @GioiTinh,
            SoDienThoai = @SoDienThoai,
            SoChungMinh = @CCCD,
            password = @PassWord,
            username = @UserName
        WHERE username = @OldUserName";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        DateTime ngaySinh;
                        if (!DateTime.TryParseExact(txtngaySinh.Text, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out ngaySinh))
                        {
                            MessageBox.Show("Ngày sinh không hợp lệ! Định dạng đúng: dd/MM/yyyy");
                            return;
                        }
                        cmd.Parameters.AddWithValue("@Ten", txthoTen.Text);
                        cmd.Parameters.AddWithValue("@NgaySinh", ngaySinh.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@GioiTinh", txtgioiTinh.Text);
                        cmd.Parameters.AddWithValue("@SoDienThoai", txtsoDienThoai.Text);
                        cmd.Parameters.AddWithValue("@CCCD", txtCCCD.Text);
                        cmd.Parameters.AddWithValue("@PassWord", txtpassword.Text);
                        cmd.Parameters.AddWithValue("@UserName", txtusername.Text);
                        cmd.Parameters.AddWithValue("@OldUserName", oldUsername);

                        try
                        {
                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Cập nhật nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Không tìm thấy nhân viên để cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        catch (SqlException ex)
                        {
                            if (ex.Message.Contains("REFERENCE constraint") && ex.Message.Contains("FK_NhanVien_TaiKhoan"))
                            {
                                MessageBox.Show("Không thể sửa username!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                MessageBox.Show("Lỗi khi cập nhật nhân viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật nhân viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}