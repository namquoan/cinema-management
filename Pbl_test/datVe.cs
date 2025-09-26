using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Globalization;

namespace Pbl_test
{
    public partial class datVe : UserControl
    {
        private string connectionString = @"Data Source=DESKTOP-HHKF7O6;Initial Catalog=POBOLO;Integrated Security=True;TrustServerCertificate=True";
        private List<int> danhSachGheDaChon = new List<int>();
        private int idNhanVien; // Lấy từ tài khoản đăng nhập thực tế
        private Dictionary<string, int> xuatChieuToPhong = new Dictionary<string, int>();

        public datVe(Label label4 = null, int loggedInNhanVienId = 1)
        {
            InitializeComponent();

            this.idNhanVien = loggedInNhanVienId; // Assign the passed ID

            // Cấu hình FlowLayoutPanel
            panelGhe.FlowDirection = FlowDirection.LeftToRight;
            panelGhe.WrapContents = true;
            panelGhe.AutoScroll = true;
            panelGhe.Padding = new Padding(10);

            this.Load += datVe_Load;
            cboChonPhim.SelectedIndexChanged += cboChonPhim_SelectedIndexChanged;
            cboChonNgay.SelectedIndexChanged += cboChonNgay_SelectedIndexChanged;
            cboChonPhong.SelectedIndexChanged += cboChonPhong_SelectedIndexChanged;
            cboChonSuat.SelectedIndexChanged += cboChonSuat_SelectedIndexChanged;
            btnDatVe.Click += btnDatVe_Click;
            this.label4 = label4;
        }

        private void datVe_Load(object sender, EventArgs e)
        {
            LoadPhim();
            cboChonNgay.Items.Clear();
            cboChonPhong.Items.Clear();
            cboChonSuat.Items.Clear();
            panelGhe.Controls.Clear();
            panelGhe.Visible = false;
        }

        private void LoadPhim()
        {
            cboChonPhim.Items.Clear();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT PhimID, TenPhim FROM Phim", conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    cboChonPhim.Items.Add(new ComboBoxItem(reader["TenPhim"].ToString(), (int)reader["PhimID"]));
                }
                reader.Close();
            }
        }

        private void cboChonPhim_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboChonNgay.Items.Clear();
            cboChonPhong.Items.Clear();
            cboChonSuat.Items.Clear();
            panelGhe.Controls.Clear();
            panelGhe.Visible = false;
            danhSachGheDaChon.Clear();

            if (cboChonPhim.SelectedItem == null) return;
            int phimId = (int)((ComboBoxItem)cboChonPhim.SelectedItem).Value;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT DISTINCT NgayChieu FROM XuatChieu WHERE PhimID=@PhimID ORDER BY NgayChieu", conn);
                cmd.Parameters.AddWithValue("@PhimID", phimId);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    DateTime ngay = Convert.ToDateTime(reader["NgayChieu"]).Date;
                    cboChonNgay.Items.Add(ngay.ToString("dd/MM/yyyy"));
                }
                reader.Close();
            }
            cboChonNgay.SelectedItem = null; // Clear day selection
            cboChonSuat.SelectedItem = null; // Clear showtime selection
        }

        private void cboChonNgay_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboChonSuat.Items.Clear(); // Clear existing showtime items
            panelGhe.Controls.Clear();
            panelGhe.Visible = false;
            danhSachGheDaChon.Clear(); // Clear selected seats when day changes

            if (cboChonPhim.SelectedItem == null || cboChonNgay.SelectedItem == null) return;

            int phimId = (int)((ComboBoxItem)cboChonPhim.SelectedItem).Value;
            DateTime ngayChieu = DateTime.ParseExact(cboChonNgay.SelectedItem.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Query to get all showtime details (XuatChieuID, GioBatDau, GioKetThuc, PhongID, KhuyenMai) for the selected film and date
                var cmd = new SqlCommand("SELECT XuatChieuID, GioBatDau, GioKetThuc, PhongID, ISNULL(KhuyenMai, 0) AS KhuyenMai FROM XuatChieu WHERE PhimID=@PhimID AND CAST(NgayChieu AS DATE)=CAST(@NgayChieu AS DATE)", conn);
                cmd.Parameters.AddWithValue("@PhimID", phimId);
                cmd.Parameters.AddWithValue("@NgayChieu", ngayChieu);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string xuatChieuId = reader["XuatChieuID"].ToString();
                    string gioBatDau = reader["GioBatDau"].ToString();
                    string gioKetThuc = reader["GioKetThuc"].ToString();
                    int phongId = Convert.ToInt32(reader["PhongID"]);
                    float khuyenMai = Convert.ToSingle(reader["KhuyenMai"]); // Retrieve KhuyenMai
                    string text = $"{xuatChieuId} - {gioBatDau} - Phòng {phongId}";
                    string value = $"{xuatChieuId}|{phongId}"; // Combine XuatChieuID and PhongID for uniqueness
                    cboChonSuat.Items.Add(new ComboBoxItem(text, value, khuyenMai)); // Pass khuyenMai
                }
                reader.Close();
            }
            cboChonSuat.SelectedItem = null; // Clear showtime selection
        }

        private void cboChonPhong_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboChonSuat.Items.Clear();
            panelGhe.Controls.Clear();
            panelGhe.Visible = false;

            if (cboChonPhong.SelectedItem == null || cboChonNgay.SelectedItem == null || cboChonPhim.SelectedItem == null) return;

            int phimId = (int)((ComboBoxItem)cboChonPhim.SelectedItem).Value;
            DateTime ngayChieu = DateTime.ParseExact(cboChonNgay.SelectedItem.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            int phongId = (int)((ComboBoxItem)cboChonPhong.SelectedItem).Value;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT XuatChieuID, GioBatDau, GioKetThuc FROM XuatChieu WHERE PhimID=@PhimID AND NgayChieu=@NgayChieu AND PhongID=@PhongID", conn);
                cmd.Parameters.AddWithValue("@PhimID", phimId);
                cmd.Parameters.AddWithValue("@NgayChieu", ngayChieu);
                cmd.Parameters.AddWithValue("@PhongID", phongId);
                var reader = cmd.ExecuteReader();
                xuatChieuToPhong.Clear();
                while (reader.Read())
                {
                    string xuatChieuId = reader["XuatChieuID"].ToString();
                    int phongIdFromDb = Convert.ToInt32(reader["PhongID"]);
                    string text = $"{xuatChieuId} ({reader["GioBatDau"]}-{reader["GioKetThuc"]})";
                    cboChonSuat.Items.Add(new ComboBoxItem(text, xuatChieuId));
                    xuatChieuToPhong[xuatChieuId] = phongIdFromDb;
                }
                reader.Close();
            }
        }

        private void cboChonSuat_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelGhe.Controls.Clear();
            panelGhe.Visible = false;
            danhSachGheDaChon.Clear();

            if (cboChonSuat.SelectedItem == null || cboChonPhim.SelectedItem == null || cboChonNgay.SelectedItem == null) return;

            var item = (ComboBoxItem)cboChonSuat.SelectedItem;
            string combinedValue = item.Value.ToString();
            string[] parts = combinedValue.Split('|');
            string xuatChieuIdStr = parts[0];
            int phongId = int.Parse(parts[1]);

            int phimId = (int)((ComboBoxItem)cboChonPhim.SelectedItem).Value;
            DateTime ngayChieu = DateTime.ParseExact(cboChonNgay.SelectedItem.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);

            // DEBUG: Display parameters before calling LoadGhe
            //MessageBox.Show($"Selected XuatChieuID: {xuatChieuIdStr}, Selected NgayChieu: {ngayChieu.ToShortDateString()}, Selected PhimID: {phimId}, Selected PhongID: {phongId}", "Debug Info - cboChonSuat_SelectedIndexChanged");

            int soGhe = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Get SoGhe from PhongChieu table based on PhongID
                var cmdGetSoGhe = new SqlCommand("SELECT SoGhe FROM PhongChieu WHERE PhongID=@PhongID", conn);
                cmdGetSoGhe.Parameters.AddWithValue("@PhongID", phongId);
                var resultSoGhe = cmdGetSoGhe.ExecuteScalar();
                if (resultSoGhe != null && int.TryParse(resultSoGhe.ToString(), out int soGheDb))
                {
                    soGhe = soGheDb;
                }
            }

            if (soGhe > 0)
            {
                LoadGhe(soGhe, xuatChieuIdStr, ngayChieu, phimId, phongId); // Pass all relevant IDs to LoadGhe
                panelGhe.Visible = true;
            }
            else
            {
                panelGhe.Controls.Clear();
                panelGhe.Visible = false;
            }
        }

        private void btnDatVe_Click(object sender, EventArgs e)
        {
            if (cboChonPhim.SelectedItem == null || cboChonNgay.SelectedItem == null ||
                cboChonSuat.SelectedItem == null || danhSachGheDaChon.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn đủ thông tin và ít nhất một ghế!", "Thông báo",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int phimId = (int)((ComboBoxItem)cboChonPhim.SelectedItem).Value;
            string tenPhim = cboChonPhim.SelectedItem.ToString();
            DateTime ngayChieu = DateTime.ParseExact(cboChonNgay.SelectedItem.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            
            // Parse combined XuatChieuID and PhongID from the selected ComboBoxItem
            string combinedValue = ((ComboBoxItem)cboChonSuat.SelectedItem).Value.ToString();
            string[] parts = combinedValue.Split('|');
            string xuatChieuIdStr = parts[0];
            int phongId = int.Parse(parts[1]);

            string suatChieu = cboChonSuat.SelectedItem.ToString();

            string tenPhong = $"Phòng {phongId}";
            float giaVe = 70000;
            string maGhe = string.Join(",", danhSachGheDaChon);
            
            // Get promotion from selected showtime
            float khuyenMai = ((ComboBoxItem)cboChonSuat.SelectedItem).KhuyenMai;

            // Calculate total price based on the formula: Tổng tiền = số vé * giá vé - số vé * giá vé * khuyến mãi/100
            float tongTien = danhSachGheDaChon.Count * giaVe - (danhSachGheDaChon.Count * giaVe * khuyenMai / 100);

            // Lưu vào database
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string hasKmSql = "SELECT CASE WHEN COL_LENGTH('DonHang','KhuyenMai') IS NULL THEN 0 ELSE 1 END";
                int hasKhuyenMai = 0;
                using (SqlCommand checkCmd = new SqlCommand(hasKmSql, conn))
                {
                    object r = checkCmd.ExecuteScalar();
                    if (r != null && int.TryParse(r.ToString(), out int parsed)) hasKhuyenMai = parsed;
                }

                string insertSql;
                if (hasKhuyenMai == 1)
                {
                    insertSql = @"
                    INSERT INTO DonHang (XuatChieuID, PhimID, NgayChieu, maGhe, giaVe, ngayDat, IDNhanVien, PhongID, KhuyenMai)
                    SELECT @XuatChieuID, @PhimID, @NgayChieu, @maGhe, @giaVe, @ngayDat, @IDNhanVien, @PhongID, @KhuyenMai
                    FROM XuatChieu xc 
                    WHERE xc.XuatChieuID = @XuatChieuID AND CAST(xc.NgayChieu AS DATE) = CAST(@NgayChieu AS DATE) AND xc.PhimID = @PhimID AND xc.PhongID = @PhongID";
                }
                else
                {
                    insertSql = @"
                    INSERT INTO DonHang (XuatChieuID, PhimID, NgayChieu, maGhe, giaVe, ngayDat, IDNhanVien, PhongID)
                    SELECT @XuatChieuID, @PhimID, @NgayChieu, @maGhe, @giaVe, @ngayDat, @IDNhanVien, @PhongID
                    FROM XuatChieu xc 
                    WHERE xc.XuatChieuID = @XuatChieuID AND CAST(xc.NgayChieu AS DATE) = CAST(@NgayChieu AS DATE) AND xc.PhimID = @PhimID AND xc.PhongID = @PhongID";
                }

                var cmd = new SqlCommand(insertSql, conn);
                cmd.Parameters.AddWithValue("@XuatChieuID", xuatChieuIdStr); // Use parsed XuatChieuID
                cmd.Parameters.AddWithValue("@PhimID", phimId);
                cmd.Parameters.AddWithValue("@NgayChieu", ngayChieu);
                cmd.Parameters.AddWithValue("@maGhe", maGhe);
                cmd.Parameters.AddWithValue("@giaVe", giaVe);
                cmd.Parameters.AddWithValue("@ngayDat", DateTime.Now);
                cmd.Parameters.AddWithValue("@IDNhanVien", idNhanVien);
                cmd.Parameters.AddWithValue("@PhongID", phongId); // Add PhongID parameter
                if (hasKhuyenMai == 1)
                {
                    cmd.Parameters.AddWithValue("@KhuyenMai", khuyenMai); // Add KhuyenMai parameter only if column exists
                }
                cmd.ExecuteNonQuery();
            }

            // Hiển thị hóa đơn
            HoaDon hoaDon = new HoaDon();

            // Truyền dữ liệu vào form hóa đơn
            hoaDon.TenPhim = tenPhim;
            hoaDon.NgayChieu = ngayChieu;
            hoaDon.SuatChieu = suatChieu;
            hoaDon.PhongChieu = tenPhong;
            hoaDon.DanhSachGhe = maGhe;
            hoaDon.GiaVe = giaVe;
            hoaDon.TongTien = tongTien;
            hoaDon.NgayDat = DateTime.Now;
            hoaDon.NhanVien = idNhanVien.ToString();
            hoaDon.KhuyenMai = khuyenMai; // Pass KhuyenMai to HoaDon form

            hoaDon.ShowDialog();

            MessageBox.Show("Đặt vé thành công!");
            danhSachGheDaChon.Clear();
            panelGhe.Controls.Clear();
            panelGhe.Visible = false;
        }
        private void LoadGhe(int soLuongGhe, string xuatChieuId, DateTime ngayChieu, int phimId, int phongId)
        {
            panelGhe.Controls.Clear();
            int gheMotHang = 10;
            // Lấy danh sách ghế đã đặt cho suất chiếu hiện tại
            List<int> gheDaDat = new List<int>();

            // DEBUG: Display the parameters being used for the query
            //MessageBox.Show($"Loading seats for XuatChieuID: {xuatChieuId}, NgayChieu: {ngayChieu.ToShortDateString()}, PhimID: {phimId}, PhongID: {phongId}", "Debug Info - LoadGhe");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT d.maGhe 
                    FROM DonHang d 
                    INNER JOIN XuatChieu xc ON d.XuatChieuID = xc.XuatChieuID 
                    WHERE d.XuatChieuID = @XuatChieuID 
                    AND CAST(xc.NgayChieu AS DATE) = CAST(@NgayChieu AS DATE)
                    AND xc.PhimID = @PhimID
                    AND xc.PhongID = @PhongID", conn); // Add PhongID to WHERE clause
                cmd.Parameters.AddWithValue("@XuatChieuID", xuatChieuId);
                cmd.Parameters.AddWithValue("@NgayChieu", ngayChieu);
                cmd.Parameters.AddWithValue("@PhimID", phimId);
                cmd.Parameters.AddWithValue("@PhongID", phongId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string maGheStr = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        if (!string.IsNullOrEmpty(maGheStr))
                        {
                            var gheList = maGheStr.Split(',').Select(g => int.TryParse(g.Trim(), out int ghe) ? ghe : -1).Where(g => g != -1).ToList();
                            gheDaDat.AddRange(gheList);
                        }
                    }
                }
            }

            for (int i = 0; i < soLuongGhe; i++)
            {
                Button btnGhe = new Button();
                btnGhe.Width = 50;
                btnGhe.Height = 50;
                btnGhe.Text = (i + 1).ToString();
                btnGhe.Margin = new Padding(8);
                btnGhe.FlatStyle = FlatStyle.Flat;
                btnGhe.FlatAppearance.BorderColor = Color.Purple;
                btnGhe.FlatAppearance.BorderSize = 2;
                btnGhe.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                int gheSo = i + 1;
                if (gheDaDat.Contains(gheSo))
                {
                    btnGhe.BackColor = Color.Gray;
                    btnGhe.Enabled = false;
                }
                else
                {
                    btnGhe.BackColor = Color.White;
                    btnGhe.Click += (s, e) =>
                    {
                        if (danhSachGheDaChon.Contains(gheSo))
                        {
                            btnGhe.BackColor = Color.White;
                            danhSachGheDaChon.Remove(gheSo);
                        }
                        else
                        {
                            btnGhe.BackColor = Color.MediumSlateBlue;
                            danhSachGheDaChon.Add(gheSo);
                        }
                    };
                }
                panelGhe.Controls.Add(btnGhe);
                if ((i + 1) % gheMotHang == 0)
                {
                    panelGhe.SetFlowBreak(btnGhe, true);
                }
            }
        }
        private class ComboBoxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public float KhuyenMai { get; set; }
            public ComboBoxItem(string text, object value)
            {
                Text = text;
                Value = value;
                KhuyenMai = 0;
            }
            public ComboBoxItem(string text, object value, float khuyenMai)
            {
                Text = text;
                Value = value;
                KhuyenMai = khuyenMai;
            }
            public override string ToString() => Text;
        }
    }
}