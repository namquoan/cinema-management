using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pbl_test
{
    public partial class QuanliPhim : UserControl
    {
        private float originalWidth;
        private float originalHeight;
        private Dictionary<Control, Rectangle> controlOriginalBounds = new Dictionary<Control, Rectangle>();
        private Dictionary<Control, Font> controlOriginalFonts = new Dictionary<Control, Font>();
        private bool isResizingInitialized = false;
        public QuanliPhim()
        {
            InitializeComponent();
            this.Load += QuanLiPhim_Load;
            this.VisibleChanged += QuanLiPhim_VisibleChanged;
            this.SizeChanged += QuanLiPhim_SizeChanged;
            guna2DataGridView1.CellClick += guna2DataGridView1_CellClick;

            this.Load += QuanLiPhim_Load;
            this.Resize += QuanLiPhim_Resize;
        }
        private string connectionString = @"Data Source=DESKTOP-HHKF7O6;Initial Catalog=POBOLO;Integrated Security=True;TrustServerCertificate=True";   
        private Image selectedImage;
        private int selectedPhongId = 1; // Giá trị mặc định cho PhongID

        private void kiemTraKetNoi()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Đã kết nối thành công, không cần thông báo
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối cơ sở dữ liệu: " + ex.Message);
            }
        }

        private void LoadDanhSachPhim()
        {
            try
            {
                string query = @"
SELECT p.PhimID, p.TenPhim, p.GioiHanDoTuoi, p.DaoDien, p.TheLoai, p.QuocGia, p.DoDaiPhim
FROM Phim p
LEFT JOIN XuatChieu xc ON p.PhimID = xc.PhimID";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Debug: Print column names in the DataTable
                    System.Diagnostics.Debug.WriteLine("DataTable Columns:");
                    foreach (DataColumn column in dt.Columns)
                    {
                        System.Diagnostics.Debug.WriteLine($"- {column.ColumnName}");
                    }

                    guna2DataGridView1.AutoGenerateColumns = false;
                    guna2DataGridView1.Rows.Clear(); // Giữ lại clear vì đang thêm dòng thủ công

                    // Sử dụng Distinct để tránh trùng lặp phim nếu có nhiều suất chiếu
                    var uniqueRows = dt.AsEnumerable()
                        .GroupBy(row => row.Field<int>("PhimID")) // Group by PhimID
                        .Select(g => g.First());

                    foreach (DataRow row in uniqueRows)
                    {
                        // Lấy giá trị DoDaiPhim và định dạng
                        object doDaiPhimValue = row["DoDaiPhim"];
                        string thoiLuongFormatted = "N/A"; // Mặc định
                        if (doDaiPhimValue != null && doDaiPhimValue != DBNull.Value)
                        {
                            // Giả định DoDaiPhim là TimeSpan hoặc có thể chuyển đổi thành TimeSpan
                            try
                            {
                                TimeSpan duration = (TimeSpan)doDaiPhimValue;
                                thoiLuongFormatted = $"{duration.Hours:D2}:{duration.Minutes:D2}"; // Định dạng HH:mm
                            }
                            catch (InvalidCastException)
                            {
                                // Xử lý nếu DoDaiPhim không phải TimeSpan (ví dụ: int phút)
                                if (int.TryParse(doDaiPhimValue.ToString(), out int minutes))
                                {
                                    TimeSpan duration = TimeSpan.FromMinutes(minutes);
                                    thoiLuongFormatted = $"{duration.Hours:D2}:{duration.Minutes:D2}"; // Định dạng HH:mm
                                }
                            }
                            catch (Exception ex)
                            {
                                // Ghi log hoặc xử lý lỗi định dạng khác
                                System.Diagnostics.Debug.WriteLine($"Lỗi định dạng DoDaiPhim: {ex.Message}");
                            }
                        }

                        guna2DataGridView1.Rows.Add(
                            row["PhimID"].ToString(),
                            row["TenPhim"].ToString(),
                            row["GioiHanDoTuoi"].ToString(),
                            row["DaoDien"].ToString(),
                            row["TheLoai"].ToString(),
                            row["QuocGia"].ToString(),
                            thoiLuongFormatted // Sử dụng Thời lượng đã định dạng thay vì XuatChieuID
                        );
                    }
                    guna2DataGridView1.Refresh();

                    if (guna2DataGridView1.Rows.Count == 0)
                    {
                        MessageBox.Show("Không có dữ liệu phim để hiển thị.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu phim: " + ex.Message);
            }
        }

        private void LoadComboBoxData()
        {
            try
            {
                string queryTheLoai = "SELECT DISTINCT TheLoai FROM Phim";
                string queryGioiHanDoTuoi = "SELECT DISTINCT GioiHanDoTuoi FROM Phim";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Load TheLoai
                    SqlCommand cmdTheLoai = new SqlCommand(queryTheLoai, conn);
                    SqlDataReader readerTheLoai = cmdTheLoai.ExecuteReader();
                    while (readerTheLoai.Read())
                    {
                        string theLoai = readerTheLoai["TheLoai"].ToString();
                        if (!cboTheLoai.Items.Contains(theLoai))
                        {
                            cboTheLoai.Items.Add(theLoai);
                        }
                    }
                    readerTheLoai.Close();

                    // Load GioiHanDoTuoi
                    SqlCommand cmdGioiHan = new SqlCommand(queryGioiHanDoTuoi, conn);
                    SqlDataReader readerGioiHan = cmdGioiHan.ExecuteReader();
                    while (readerGioiHan.Read())
                    {
                        string gioiHan = readerGioiHan["GioiHanDoTuoi"].ToString();
                        if (!cbogioiHanDoTuoi.Items.Contains(gioiHan))
                        {
                            cbogioiHanDoTuoi.Items.Add(gioiHan);
                        }
                    }
                    readerGioiHan.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu ComboBox: " + ex.Message);
            }
        }
        private void QuanLiPhim_Resize(object sender, EventArgs e)
        {
            if (!isResizingInitialized || originalWidth == 0 || originalHeight == 0) return;
            float widthRatio = (float)this.Width / originalWidth;
            float heightRatio = (float)this.Height / originalHeight;
            ResizeControls(this, widthRatio, heightRatio);
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
        private void QuanLiPhim_Load(object sender, EventArgs e)
        {
            originalWidth = this.Width;
            originalHeight = this.Height;
            controlOriginalBounds.Clear();
            controlOriginalFonts.Clear();
            SaveOriginalBounds(this);
            isResizingInitialized = true;
            kiemTraKetNoi();

            guna2DataGridView1.Columns.Clear(); // Clear existing columns

            // Manually add columns with desired names and data bindings
            guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PhimID",
                HeaderText = "Mã Phim",
                DataPropertyName = "PhimID",
                Visible = true,
                Width = 50
            });
            guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TenPhim",
                HeaderText = "Tên Phim",
                DataPropertyName = "TenPhim",
                Visible = true,
                Width = 100
            });
            guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "GioiHanDoTuoi",
                HeaderText = "Giới hạn Độ tuổi",
                DataPropertyName = "GioiHanDoTuoi",
                Visible = true,
                Width = 80
            });
            guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DaoDien",
                HeaderText = "Đạo diễn",
                DataPropertyName = "DaoDien",
                Visible = true,
                Width = 100
            });
            guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TheLoai",
                HeaderText = "Thể loại",
                DataPropertyName = "TheLoai",
                Visible = true,
                Width = 100
            });
            guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "QuocGia",
                HeaderText = "Quốc Gia",
                DataPropertyName = "QuocGia",
                Visible = true,
                Width = 100
            });
            guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Duration",
                HeaderText = "Thời lượng",
                DataPropertyName = "DoDaiPhim",
                Visible = true,
                Width = 80
            });

            LoadDanhSachPhim();

            LoadComboBoxData();

            txtPhimId.ReadOnly = true;
            txtPhimId.BackColor = Color.LightGray;
            txtxuatChieu.ReadOnly = true;
            txtxuatChieu.BackColor = Color.LightGray;
            guna2DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
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

        private void QuanLiPhim_SizeChanged(object sender, EventArgs e)
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

        private void QuanLiPhim_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible && guna2DataGridView1.RowCount == 0)
            {
                LoadDanhSachPhim();
            }
        }

        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = guna2DataGridView1.Rows[e.RowIndex];

                // Lấy dữ liệu từ hàng được chọn
                string phimId = row.Cells["PhimID"].Value.ToString();
                string tenPhim = row.Cells["TenPhim"].Value.ToString();
                string gioiHanDoTuoi = row.Cells["GioiHanDoTuoi"].Value.ToString();
                string daoDien = row.Cells["DaoDien"].Value.ToString();
                string theLoai = row.Cells["TheLoai"].Value.ToString();
                string quocGia = row.Cells["QuocGia"].Value.ToString();
                string thoiLuong = row.Cells["Duration"].Value.ToString(); // Get the formatted duration

                // Hiển thị dữ liệu lên các controls
                txtPhimId.Text = phimId;
                txttenPhim.Text = tenPhim;
                txtDaoDien.Text = daoDien;
                txtquocGia.Text = quocGia;
                txtxuatChieu.Text = thoiLuong; // Display the formatted duration in the text box

                if (cbogioiHanDoTuoi.Items.Contains(gioiHanDoTuoi))
                    cbogioiHanDoTuoi.SelectedItem = gioiHanDoTuoi;
                else
                    cbogioiHanDoTuoi.Text = gioiHanDoTuoi;

                if (cboTheLoai.Items.Contains(theLoai))
                    cboTheLoai.SelectedItem = theLoai;
                else
                    cboTheLoai.Text = theLoai;

                // Load ảnh phim
                LoadImageForSelectedPhim();

                txtThoiLuong.Text = thoiLuong; // thoiLuong là giá trị thời lượng lấy từ dòng được chọn
            }
        }

        // Phương thức riêng để tải hình ảnh
        private void LoadImageForSelectedPhim()
        {
            try
            {
                string phimId = txtPhimId.Text;
                if (!string.IsNullOrEmpty(phimId) && int.TryParse(phimId, out int phimIdInt))
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "SELECT Image FROM Phim WHERE PhimID = @PhimID";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@PhimID", phimIdInt);
                            object result = cmd.ExecuteScalar();

                            if (result != null && result != DBNull.Value)
                            {
                                try
                                {
                                    byte[] imageBytes = (byte[])result;
                                    using (MemoryStream ms = new MemoryStream(imageBytes))
                                    {
                                        // Giải phóng ảnh cũ
                                        if (guna2PictureBox1.Image != null)
                                        {
                                            Image oldImage = guna2PictureBox1.Image;
                                            guna2PictureBox1.Image = null;
                                            oldImage.Dispose();
                                        }

                                        guna2PictureBox1.Image = Image.FromStream(ms);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Lỗi khi xử lý hình ảnh - thông báo chi tiết để debug
                                    MessageBox.Show($"Lỗi khi xử lý hình ảnh: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    guna2PictureBox1.Image = null;
                                }
                            }
                            else
                            {
                                // Không có hình ảnh
                                if (guna2PictureBox1.Image != null)
                                {
                                    Image oldImage = guna2PictureBox1.Image;
                                    guna2PictureBox1.Image = null;
                                    oldImage.Dispose();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải hình ảnh: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Đảm bảo xóa hình ảnh cũ nếu có lỗi
                if (guna2PictureBox1.Image != null)
                {
                    Image oldImage = guna2PictureBox1.Image;
                    guna2PictureBox1.Image = null;
                    oldImage.Dispose();
                }
            }
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            string tenCanTim = txttimKiemPhim.Text.ToLower().Trim();

            foreach (DataGridViewRow row in guna2DataGridView1.Rows)
            {
                if (row.IsNewRow)
                    continue;
                if (row.Cells["tenPhim"].Value != null &&
                    row.Cells["tenPhim"].Value.ToString().ToLower().Contains(tenCanTim))
                {
                    row.Visible = true;
                }
                else
                {
                    row.Visible = false;
                }
            }
        }

        private void btnlayAnh_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPhimId.Text))
            {
                MessageBox.Show("Vui lòng chọn một phim trước khi thêm ảnh!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files (*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";
                openFileDialog.Title = "Chọn ảnh phim";

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
                                    string query = "UPDATE Phim SET Image = @Image WHERE PhimID = @PhimID";
                                    using (SqlCommand cmd = new SqlCommand(query, connection))
                                    {
                                        cmd.Parameters.AddWithValue("@Image", imageBytes);
                                        cmd.Parameters.AddWithValue("@PhimID", int.Parse(txtPhimId.Text));

                                        int result = cmd.ExecuteNonQuery();
                                        if (result > 0)
                                        {
                                            MessageBox.Show("Lưu ảnh thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                            // Hiển thị ảnh ngay sau khi lưu
                                            if (guna2PictureBox1.Image != null)
                                            {
                                                Image oldImage = guna2PictureBox1.Image;
                                                guna2PictureBox1.Image = null;
                                                oldImage.Dispose();
                                            }
                                            guna2PictureBox1.Image = new Bitmap(image);
                                        }
                                        else
                                        {
                                            MessageBox.Show("Không tìm thấy phim để cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void btnThem_Click(object sender, EventArgs e)
        {
            try
            {
                LoadComboBoxData();

                if (string.IsNullOrWhiteSpace(txttenPhim.Text) ||
                    string.IsNullOrWhiteSpace(txtDaoDien.Text) ||
                    string.IsNullOrWhiteSpace(txtquocGia.Text) ||
                    cboTheLoai.SelectedIndex == -1 ||
                    cbogioiHanDoTuoi.SelectedIndex == -1 ||
                    string.IsNullOrWhiteSpace(txtThoiLuong.Text))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!");
                    return;
                }

                // Kiểm tra định dạng thời lượng
                if (!TimeSpan.TryParse(txtThoiLuong.Text, out TimeSpan thoiLuong))
                {
                    MessageBox.Show("Thời lượng phải nhập theo định dạng hh:mm, ví dụ 2:08 hoặc 02:08!");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 1. Thêm phim vào bảng Phim
                    string queryPhim = @"
INSERT INTO Phim (TenPhim, GioiHanDoTuoi, DaoDien, TheLoai, QuocGia, DoDaiPhim)
OUTPUT INSERTED.PhimID
VALUES (@TenPhim, @GioiHanDoTuoi, @DaoDien, @TheLoai, @QuocGia, @DoDaiPhim)";

                    SqlCommand cmdPhim = new SqlCommand(queryPhim, conn);
                    cmdPhim.Parameters.AddWithValue("@TenPhim", txttenPhim.Text);
                    cmdPhim.Parameters.AddWithValue("@GioiHanDoTuoi", cbogioiHanDoTuoi.SelectedItem.ToString());
                    cmdPhim.Parameters.AddWithValue("@DaoDien", txtDaoDien.Text);
                    cmdPhim.Parameters.AddWithValue("@TheLoai", cboTheLoai.SelectedItem.ToString());
                    cmdPhim.Parameters.AddWithValue("@QuocGia", txtquocGia.Text);
                    cmdPhim.Parameters.AddWithValue("@DoDaiPhim", thoiLuong);

                    int phimID = (int)cmdPhim.ExecuteScalar();
                    txtPhimId.Text = phimID.ToString(); // Cập nhật lại mã phim cho textbox

                    MessageBox.Show("Thêm phim thành công! Hãy chọn suất chiếu cho phim này bằng nút 'Chọn XC'.");
                    LoadDanhSachPhim();

                    // Xóa dữ liệu sau khi thêm (không xóa txtPhimId)
                    txttenPhim.Clear();
                    txtDaoDien.Clear();
                    txtquocGia.Clear();
                    cboTheLoai.SelectedIndex = -1;
                    cbogioiHanDoTuoi.SelectedIndex = -1;
                    txtxuatChieu.Clear();
                    txtThoiLuong.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm phim: " + ex.Message);
            }
        }

        private (TimeSpan startTime, TimeSpan endTime) GetTimeSlotsFromShowtimeId(string showtimeId)
        {
            // Định nghĩa các khung giờ cố định cho S1-S8
            switch (showtimeId.ToUpper())
            {
                case "S1":
                    return (new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0));  // 8:00 - 10:00
                case "S2":
                    return (new TimeSpan(10, 30, 0), new TimeSpan(12, 30, 0)); // 10:30 - 12:30
                case "S3":
                    return (new TimeSpan(13, 0, 0), new TimeSpan(15, 0, 0));  // 13:00 - 15:00
                case "S4":
                    return (new TimeSpan(15, 30, 0), new TimeSpan(17, 30, 0)); // 15:30 - 17:30
                case "S5":
                    return (new TimeSpan(18, 0, 0), new TimeSpan(20, 0, 0));  // 18:00 - 20:00
                case "S6":
                    return (new TimeSpan(20, 30, 0), new TimeSpan(22, 30, 0)); // 20:30 - 22:30
                case "S7":
                    return (new TimeSpan(23, 0, 0), new TimeSpan(1, 0, 0));   // 23:00 - 1:00
                case "S8":
                    return (new TimeSpan(1, 30, 0), new TimeSpan(3, 30, 0));  // 1:30 - 3:30
                default:
                    return (TimeSpan.Zero, TimeSpan.Zero);
            }
        }

        private void btnXoa_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPhimId.Text))
            {
                MessageBox.Show("Vui lòng chọn một phim để xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa phim này? Thao tác này sẽ chuyển trạng thái phim thành 'Đã chiếu'!", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        int phimId = int.Parse(txtPhimId.Text);
                        // Kiểm tra còn suất chiếu nào trong tương lai không
                        string checkQuery = @"SELECT COUNT(*) FROM XuatChieu WHERE PhimID = @PhimID AND NgayChieu >= @Now";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@PhimID", phimId);
                            checkCmd.Parameters.AddWithValue("@Now", DateTime.Now.Date);
                            int count = (int)checkCmd.ExecuteScalar();
                            if (count > 0)
                            {
                                MessageBox.Show("Phim vẫn còn suất chiếu trong tương lai, không thể chuyển trạng thái!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                        // Nếu không còn suất chiếu tương lai, chuyển trạng thái
                        string updateQuery = @"IF COL_LENGTH('Phim','TrangThai') IS NOT NULL
BEGIN
    EXEC sp_executesql N'UPDATE Phim SET TrangThai = N''Đã chiếu'' WHERE PhimID = @PhimID', N'@PhimID int', @PhimID=@PhimID
END";
                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@PhimID", phimId);
                            int rows = updateCmd.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                MessageBox.Show("Đã chuyển trạng thái phim thành 'Đã chiếu'!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadDanhSachPhim();
                            }
                            else
                            {
                                MessageBox.Show("Không tìm thấy phim để cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi cập nhật trạng thái phim: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Phương thức này được gọi từ FormBangThoiGian để cập nhật txtxuatChieu
        public void SetXuatChieu(string xuatChieuText)
        {
            txtxuatChieu.Text = xuatChieuText;
        }

        private void btnChonXC_Click(object sender, EventArgs e)
        {
            int phimId = GetPhimId();
            if (phimId == 0)
            {
                MessageBox.Show("Vui lòng chọn hoặc thêm phim trước khi chọn suất chiếu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            FormBangThoiGian form = new FormBangThoiGian(this, GetPhongId(), phimId);
            form.ShowDialog();
            // Khi form đóng lại, phương thức SetXuatChieu sẽ được gọi từ FormBangThoiGian
            // để cập nhật txtxuatChieu
        }

        // Thêm phương thức GetPhimId để lấy mã phim hiện tại
        public int GetPhimId()
        {
            if (string.IsNullOrEmpty(txtPhimId.Text))
                return 0;

            if (int.TryParse(txtPhimId.Text, out int phimId))
                return phimId;

            return 0;
        }

        // Thêm phương thức GetPhongId để lấy mã phòng chiếu
        public int GetPhongId()
        {
            return selectedPhongId; // Trả về phòng chiếu mặc định hoặc đã chọn
        }

        // Phương thức để đặt phòng chiếu (sử dụng khi cần)
        public void SetPhongId(int phongId)
        {
            selectedPhongId = phongId;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            LoadDanhSachPhim();
        }

        private void btnSua_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtPhimId.Text))
                {
                    MessageBox.Show("Vui lòng chọn một phim để sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txttenPhim.Text) ||
                    string.IsNullOrWhiteSpace(txtDaoDien.Text) ||
                    string.IsNullOrWhiteSpace(txtquocGia.Text) ||
                    cboTheLoai.SelectedIndex == -1 ||
                    cbogioiHanDoTuoi.SelectedIndex == -1 ||
                    string.IsNullOrWhiteSpace(txtThoiLuong.Text))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!");
                    return;
                }

                // Kiểm tra định dạng thời lượng
                if (!TimeSpan.TryParse(txtThoiLuong.Text, out TimeSpan thoiLuong))
                {
                    MessageBox.Show("Thời lượng phải nhập theo định dạng hh:mm, ví dụ 2:08 hoặc 02:08!");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
        UPDATE Phim
        SET TenPhim = @TenPhim,
            GioiHanDoTuoi = @GioiHanDoTuoi,
            DaoDien = @DaoDien,
            TheLoai = @TheLoai,
            QuocGia = @QuocGia,
            DoDaiPhim = @DoDaiPhim
        WHERE PhimID = @PhimID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TenPhim", txttenPhim.Text);
                        cmd.Parameters.AddWithValue("@GioiHanDoTuoi", cbogioiHanDoTuoi.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@DaoDien", txtDaoDien.Text);
                        cmd.Parameters.AddWithValue("@TheLoai", cboTheLoai.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@QuocGia", txtquocGia.Text);
                        cmd.Parameters.AddWithValue("@DoDaiPhim", thoiLuong);
                        cmd.Parameters.AddWithValue("@PhimID", int.Parse(txtPhimId.Text));

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Cập nhật phim thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadDanhSachPhim();
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy phim để cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật phim: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                    // Đặc biệt cho DataGridView
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

        private void btnReset_Click_1(object sender, EventArgs e)
        {
            LoadDanhSachPhim();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // This method is referenced by the designer but appears to have no implementation.
            // Add any intended background work logic here.
        }
    }
}