using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace Pbl_test
{
    public partial class baoCaoDoanhThu : UserControl
    {
        private string connectionString = @"Data Source=DESKTOP-HHKF7O6;Initial Catalog=POBOLO;Integrated Security=True;TrustServerCertificate=True";
        private float originalWidth;
        private float originalHeight;
        private Dictionary<Control, Rectangle> controlOriginalBounds;
        private Dictionary<Control, Font> controlOriginalFonts;
        private bool isLoaded = false;

        public baoCaoDoanhThu()
        {
            InitializeComponent();
            comboBoxYear.SelectedIndexChanged += comboBoxYear_SelectedIndexChanged;
            controlOriginalBounds = new Dictionary<Control, Rectangle>();
            controlOriginalFonts = new Dictionary<Control, Font>();
            this.Load += BaoCaoDoanhThu_Load;
            this.Resize += BaoCaoDoanhThu_Resize;

            // Add mutual exclusivity for quarter and month
            comboBoxQuarter.SelectedIndexChanged += comboBoxQuarter_SelectedIndexChanged;
            comboBoxMonth.SelectedIndexChanged += comboBoxMonth_SelectedIndexChanged;

            // Use a compact date format to avoid text overlapping the dropdown arrow
            dateTimePickerStartDate.Format = DateTimePickerFormat.Custom;
            dateTimePickerStartDate.CustomFormat = "dd/MM/yyyy";
            dateTimePickerEndDate.Format = DateTimePickerFormat.Custom;
            dateTimePickerEndDate.CustomFormat = "dd/MM/yyyy";

            // Khởi tạo mặc định 7 ngày gần nhất
            dateTimePickerStartDate.Value = DateTime.Today.AddDays(-7);
            dateTimePickerEndDate.Value = DateTime.Today;
            // Lấy giới hạn thời gian thực
            dateTimePickerStartDate.MaxDate = DateTime.Today;
            dateTimePickerEndDate.MaxDate = DateTime.Today;
        }

        private void BaoCaoDoanhThu_Load(object sender, EventArgs e)
        {
            // Clear the DataGridView initially
            // dataGridViewRevenue.DataSource = null;

            // Create an empty DataTable with the correct column structure
            DataTable emptyDataTable = new DataTable();
            emptyDataTable.Columns.Add("Mã hóa đơn", typeof(string));
            emptyDataTable.Columns.Add("Tên nhân viên", typeof(string));
            emptyDataTable.Columns.Add("Ngày đặt", typeof(DateTime));
            emptyDataTable.Columns.Add("Giá vé", typeof(float));
            emptyDataTable.Columns.Add("Số ghế", typeof(int));
            emptyDataTable.Columns.Add("Tổng tiền", typeof(float));

            dataGridViewRevenue.DataSource = emptyDataTable;

            DataGridViewCellStyle customStyle = new DataGridViewCellStyle();
            customStyle.BackColor = Color.FromArgb(182, 61, 61);
            customStyle.ForeColor = Color.White;
            customStyle.Font = new Font("Verdana", 8, FontStyle.Bold);
            customStyle.SelectionBackColor = Color.FromArgb(255, 192, 192);
            customStyle.SelectionForeColor = Color.IndianRed;
            customStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            customStyle.WrapMode = DataGridViewTriState.True;

            // Gán style cho toàn bộ dữ liệu
            dataGridViewRevenue.DefaultCellStyle = customStyle;

            // Ensure alternating rows also have visible text
            DataGridViewCellStyle alternatingRowStyle = new DataGridViewCellStyle();
            alternatingRowStyle.BackColor = Color.White; // As per Designer.cs
            alternatingRowStyle.ForeColor = Color.Black; // Explicitly set to Black
            dataGridViewRevenue.AlternatingRowsDefaultCellStyle = alternatingRowStyle;

            // Giao diện tiêu đề cột (header)
            dataGridViewRevenue.EnableHeadersVisualStyles = false; // Cho phép chỉnh style
            dataGridViewRevenue.ColumnHeadersDefaultCellStyle.BackColor = Color.Maroon;
            dataGridViewRevenue.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridViewRevenue.ColumnHeadersDefaultCellStyle.Font = new Font("Verdana", 8, FontStyle.Bold);



            // Set the empty DataTable as the DataSource to display headers
            dataGridViewRevenue.DataSource = emptyDataTable;

            // Lưu kích thước ban đầu
            originalWidth = this.Width;
            originalHeight = this.Height;

            // Lưu vị trí và kích thước ban đầu của các control
            foreach (Control control in this.Controls)
            {
                controlOriginalBounds[control] = new Rectangle(
                    control.Location.X,
                    control.Location.Y,
                    control.Width,
                    control.Height
                );
                controlOriginalFonts[control] = control.Font;
            }

            // Đặt AutoSize cho nhãn Sum để nó tự điều chỉnh kích thước theo nội dung
            Sum.AutoSize = true;

            // Tải dữ liệu comboBox nhân viên
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                SELECT nv.IDNhanVien, tk.Ten 
                FROM NhanVien nv
                JOIN TaiKhoan tk ON nv.username = tk.username";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Thêm dòng "Tất cả nhân viên"
                    DataRow row = dt.NewRow();
                    row["IDNhanVien"] = DBNull.Value;
                    row["Ten"] = "Tất cả";
                    dt.Rows.InsertAt(row, 0);

                    cboNV.DataSource = dt;
                    cboNV.DisplayMember = "Ten";
                    cboNV.ValueMember = "IDNhanVien";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách nhân viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Khởi tạo ComboBox năm
            comboBoxYear.Items.Clear();
            comboBoxYear.Items.Add("Tất cả");

            int currentYear = DateTime.Now.Year;
            int startYear = currentYear - 2;

            for (int year = currentYear; year >= startYear; year--)
            {
                comboBoxYear.Items.Add(year.ToString());
            }

            comboBoxYear.SelectedIndex = 0;

            // Khởi tạo ComboBox tháng và quý
            comboBoxMonth.Items.Clear();
            comboBoxQuarter.Items.Clear();
            comboBoxMonth.Items.Add("Tất cả");
            comboBoxQuarter.Items.Add("Tất cả");
            comboBoxMonth.SelectedIndex = 0;
            comboBoxQuarter.SelectedIndex = 0;

            // Kích hoạt sự kiện thay đổi lựa chọn năm để điền các tháng và quý ban đầu
            comboBoxYear_SelectedIndexChanged(comboBoxYear, EventArgs.Empty);

            // Enable all controls by default
            dateTimePickerStartDate.Enabled = true;
            dateTimePickerEndDate.Enabled = true;
            comboBoxYear.Enabled = true;
            comboBoxMonth.Enabled = true;
            comboBoxQuarter.Enabled = true;

            isLoaded = true;

            // Load data for the default date range (last 7 days) on load
            // LoadData(dateTimePickerStartDate.Value.Date, dateTimePickerEndDate.Value.Date.AddDays(1).AddMilliseconds(-1), null, null);
        }

        private void BaoCaoDoanhThu_Resize(object sender, EventArgs e)
        {
            if (!isLoaded || originalWidth == 0 || originalHeight == 0) return;

            float widthRatio = this.Width / originalWidth;
            float heightRatio = this.Height / originalHeight;

            // Áp dụng thay đổi kích thước cho tất cả controls bằng hàm đệ quy
            ApplyResize(this, widthRatio, heightRatio);
        }

        // Hàm đệ quy để áp dụng thay đổi kích thước cho control và các control con
        private void ApplyResize(Control parentControl, float widthRatio, float heightRatio)
        {
            foreach (Control control in parentControl.Controls)
            {
                if (controlOriginalBounds.ContainsKey(control))
                {
                    Rectangle originalBounds = controlOriginalBounds[control];

                    // Tính toán vị trí và kích thước mới
                    int newX = (int)(originalBounds.X * widthRatio);
                    int newY = (int)(originalBounds.Y * heightRatio);
                    int newWidth = (int)(originalBounds.Width * widthRatio);
                    int newHeight = (int)(originalBounds.Height * heightRatio);

                    // Áp dụng kích thước mới cho control
                    control.Location = new Point(newX, newY);
                    control.Size = new Size(newWidth, newHeight);

                    // Điều chỉnh font size (chỉ áp dụng nếu AutoSize của control là false hoặc là DataGridView)
                    // Nếu AutoSize là true, font size sẽ tự điều chỉnh theo kích thước control mới
                    if (!control.AutoSize || control is DataGridView)
                    {
                        // Lấy font gốc từ dictionary hoặc font hiện tại nếu không có trong dictionary
                        Font originalFont = controlOriginalFonts.ContainsKey(control) ? controlOriginalFonts[control] : control.Font;
                        float ratio = Math.Min(widthRatio, heightRatio);
                        float newFontSize = Math.Max(originalFont.Size * ratio, 8); // Kích thước font tối thiểu là 8

                        // Tránh lỗi khi kích thước font không hợp lệ
                        if (newFontSize > 0 && !float.IsNaN(newFontSize) && !float.IsInfinity(newFontSize))
                        {
                            // Tạo font mới với kích thước đã tính toán, giữ nguyên style
                            control.Font = new Font(originalFont.FontFamily, newFontSize, originalFont.Style);
                        }
                    }
                }

                // Tiếp tục đệ quy cho các control con
                if (control.Controls.Count > 0)
                {
                    ApplyResize(control, widthRatio, heightRatio);
                }
            }
        }

        private void LoadData(DateTime? from = null, DateTime? to = null, int? idNhanVien = null, int? yearFilter = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Kiểm tra cột KhuyenMai có tồn tại trong bảng DonHang không
                    string columnExistsSql = "SELECT CASE WHEN COL_LENGTH('DonHang','KhuyenMai') IS NULL THEN 0 ELSE 1 END";
                    int hasKhuyenMaiColumn = 0;
                    using (SqlCommand existsCmd = new SqlCommand(columnExistsSql, conn))
                    {
                        object result = existsCmd.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out int parsed))
                        {
                            hasKhuyenMaiColumn = parsed;
                        }
                    }

                    // Xây dựng câu truy vấn linh hoạt
                    string khuyenMaiExpr = hasKhuyenMaiColumn == 1 ? "ISNULL(dh.KhuyenMai, 0)" : "0";
                    string soGheExpr = "CASE \n                        WHEN dh.maGhe IS NULL OR dh.maGhe = '' THEN 0\n                        ELSE LEN(dh.maGhe) - LEN(REPLACE(dh.maGhe, ',', '')) + 1\n                    END";
                    string tongTienExpr = $"CASE \n                        WHEN dh.maGhe IS NULL OR dh.maGhe = '' THEN 0\n                        ELSE ({soGheExpr}) * dh.giaVe - (({soGheExpr}) * dh.giaVe * {khuyenMaiExpr} / 100)\n                    END";

                    string query = @"
                SELECT
                    dh.maVe AS [Mã hóa đơn],
                    ISNULL(tk.Ten, N'Không xác định') AS [Tên nhân viên],
                    dh.ngayDat AS [Ngày đặt],
                    dh.giaVe AS [Giá vé],
                    " + soGheExpr + @" AS [Số ghế],
                    " + khuyenMaiExpr + @" AS [Khuyến mãi],
                    " + tongTienExpr + @" AS [Tổng tiền]
                FROM DonHang dh
                LEFT JOIN NhanVien nv ON dh.IDNhanVien = nv.IDNhanVien
                LEFT JOIN TaiKhoan tk ON nv.username = tk.username
                WHERE dh.maVe IS NOT NULL AND dh.maVe != '' -- Đảm bảo mã hóa đơn không rỗng
                AND dh.ngayDat IS NOT NULL -- Đảm bảo ngày đặt không rỗng
                AND dh.giaVe IS NOT NULL AND dh.giaVe > 0 -- Đảm bảo giá vé hợp lệ
                AND (dh.maGhe IS NOT NULL AND dh.maGhe != '') -- Đảm bảo có thông tin ghế
                ";

                    // Thêm điều kiện nếu có
                    if (from.HasValue)
                        query += " AND dh.ngayDat >= @from";
                    if (to.HasValue)
                        query += " AND dh.ngayDat <= @to";
                    if (idNhanVien.HasValue)
                        query += " AND dh.IDNhanVien = @idNhanVien";
                    if (yearFilter.HasValue)
                        query += " AND YEAR(dh.ngayDat) = @yearFilter";

                    query += " ORDER BY dh.ngayDat DESC";

                    // Debugging: Print the generated SQL query
                    Console.WriteLine($"Generated SQL Query: {query}");

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (from.HasValue)
                            cmd.Parameters.Add("@from", SqlDbType.DateTime).Value = from.Value;
                        if (to.HasValue)
                            cmd.Parameters.Add("@to", SqlDbType.DateTime).Value = to.Value;
                        if (idNhanVien.HasValue)
                            cmd.Parameters.Add("@idNhanVien", SqlDbType.Int).Value = idNhanVien.Value;
                        if (yearFilter.HasValue)
                            cmd.Parameters.Add("@yearFilter", SqlDbType.Int).Value = yearFilter.Value;

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            // Đảm bảo không gọi dataGridViewRevenue.Rows.Clear() khi DataGridView đang data-bound.
                            // Việc gán một DataTable mới vào DataSource sẽ tự động làm mới hiển thị.

                            dataGridViewRevenue.DataSource = dt;

                            // Ensure the name column is wide enough to avoid truncation
                            var nameColumn = dataGridViewRevenue.Columns["Tên nhân viên"]; // matches SELECT alias
                            if (nameColumn != null)
                            {
                                nameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                nameColumn.MinimumWidth = 180;
                                nameColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                                nameColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                            }

                            // Format date columns if present
                            var ngayDatCol = dataGridViewRevenue.Columns["Ngày đặt"]; 
                            if (ngayDatCol != null)
                            {
                                ngayDatCol.DefaultCellStyle.Format = "dd/MM/yyyy";
                            }

                            // Calculate and display the total revenue
                            float totalRevenue = 0;
                            if (dt.Rows.Count > 0 && dt.Columns.Contains("Tổng tiền"))
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    if (row["Tổng tiền"] != DBNull.Value && float.TryParse(row["Tổng tiền"].ToString(), out float rowTotal))
                                    {
                                        totalRevenue += rowTotal;
                                    }
                                }
                            }
                            // Display the total revenue in the Sum label
                            Sum.Text = totalRevenue.ToString("N0") + " VND"; // Format as currency, adjust format if needed
                        }
                    }

                    // Căn giữa, auto size và fill cột cuối cùng
                    foreach (DataGridViewColumn col in dataGridViewRevenue.Columns)
                    {
                        col.HeaderCell.Style.Padding = new Padding(5);
                        col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        if (col.Name != "Tên nhân viên")
                        {
                            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        }
                    }

                    if (dataGridViewRevenue.Columns.Count > 0)
                    {
                        dataGridViewRevenue.Columns[dataGridViewRevenue.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBoxYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxMonth.Items.Clear();
            comboBoxQuarter.Items.Clear();

            // Luôn thêm mục "Tất cả" đầu tiên
            comboBoxMonth.Items.Add("Tất cả");
            comboBoxQuarter.Items.Add("Tất cả");

            // Nếu một năm cụ thể được chọn (chỉ số của item được chọn > 0)
            if (comboBoxYear.SelectedIndex > 0)
            {
                // Thêm tất cả 12 tháng (từ 1 đến 12)
                for (int month = 1; month <= 12; month++)
                {
                    comboBoxMonth.Items.Add($"Tháng {month}");
                }

                // Thêm tất cả 4 quý (từ 1 đến 4)
                for (int quarter = 1; quarter <= 4; quarter++)
                {
                    comboBoxQuarter.Items.Add($"Quý {quarter}");
                }
            }
            // Nếu SelectedIndex là 0 (tức là "Tất cả" năm), các ComboBox tháng và quý sẽ chỉ chứa mục "Tất cả" đã thêm ở trên.

            // Đảm bảo lựa chọn mặc định sau khi cập nhật danh sách là "Tất cả"
            // Dòng này cần được giữ lại để mặc định là "Tất cả" cho tháng/quý khi thay đổi năm.
            comboBoxMonth.SelectedIndex = 0;
            comboBoxQuarter.SelectedIndex = 0;
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            // Debugging: Check isLoaded status
            // Console.WriteLine($"buttonSearch_Click: isLoaded = {isLoaded}");

            // Only load data if the form has finished loading
            // if (!isLoaded)
            // {
            //     return;
            // }

            DateTime? startDate = null;
            DateTime? endDate = null;
            bool hasDateSelection = false;
            bool hasMonthSelection = false;
            bool hasQuarterSelection = false;
            int? idNhanVien = null;
            int? yearFilter = null;

            // Date selection only if enabled
            if (checkBoxStartDate.Checked)
            {
                startDate = dateTimePickerStartDate.Value.Date;
                hasDateSelection = true;
            }
            if (checkBoxEndDate.Checked)
            {
                endDate = dateTimePickerEndDate.Value.Date.AddDays(1).AddMilliseconds(-1);
                hasDateSelection = true;
            }
            if (checkBoxStartDate.Checked && checkBoxEndDate.Checked && startDate > endDate)
            {
                MessageBox.Show("Ngày bắt đầu không thể lớn hơn ngày kết thúc!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Year filter
            if (comboBoxYear.SelectedIndex > 0)
            {
                yearFilter = int.Parse(comboBoxYear.SelectedItem.ToString());
            }

            // Month filter
            if (comboBoxYear.SelectedIndex > 0 && comboBoxMonth.SelectedIndex > 0)
            {
                string selectedMonth = comboBoxMonth.SelectedItem.ToString();
                int month = int.Parse(selectedMonth.Split(' ')[1]);
                int year = int.Parse(comboBoxYear.SelectedItem.ToString());
                DateTime monthStart = new DateTime(year, month, 1);
                DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);
                startDate = monthStart;
                endDate = monthEnd;
                hasMonthSelection = true;
            }

            // Quarter filter
            if (comboBoxYear.SelectedIndex > 0 && comboBoxQuarter.SelectedIndex > 0)
            {
                string selectedQuarter = comboBoxQuarter.SelectedItem.ToString();
                int quarter = int.Parse(selectedQuarter.Split(' ')[1]);
                int year = int.Parse(comboBoxYear.SelectedItem.ToString());
                int startMonth = (quarter - 1) * 3 + 1;
                DateTime quarterStart = new DateTime(year, startMonth, 1);
                DateTime quarterEnd = quarterStart.AddMonths(3).AddDays(-1);
                startDate = quarterStart;
                endDate = quarterEnd;
                hasQuarterSelection = true;
            }

            // Staff filter
            if (cboNV.SelectedIndex > 0 && cboNV.SelectedValue != null && int.TryParse(cboNV.SelectedValue.ToString(), out int tempId))
            {
                idNhanVien = tempId;
            }

            // Debugging: Print filter parameters
            Console.WriteLine($"Search Parameters: startDate={startDate}, endDate={endDate}, idNhanVien={idNhanVien}, yearFilter={yearFilter}");

            // If no filters, load all data
            if (!hasDateSelection && !hasMonthSelection && !hasQuarterSelection && yearFilter == null && idNhanVien == null)
            {
                LoadData(null, null, null, null);
                return;
            }

            // Load data with the selected filters
            LoadData(startDate, endDate, idNhanVien, yearFilter);
        }

        private void btnXemChiTiet_Click(object sender, EventArgs e)
        {
            if (dataGridViewRevenue.SelectedRows.Count > 0)
            {
                string maVe = dataGridViewRevenue.SelectedRows[0].Cells["Mã hóa đơn"].Value.ToString();

                DataRow donHangRow = GetDonHangByMaVe(maVe);
                if (donHangRow != null)
                {
                    // Lấy đủ dữ liệu cần thiết từ dòng đơn hàng
                    string maGhe = donHangRow["MaGhe"].ToString();
                    string xuatChieuID = donHangRow["XuatChieuID"].ToString();
                    string giaVe = donHangRow["GiaVe"].ToString();
                    string phongID = donHangRow["PhongID"].ToString();
                    string ngayChieu = donHangRow["NgayChieu"].ToString();
                    string phimID = donHangRow["PhimID"].ToString();
                    string ngayDat = donHangRow["NgayDat"].ToString();
                    string tongTien = (int.Parse(giaVe) * maGhe.Split(',').Length).ToString();
                    string idNhanVien = donHangRow["IDNhanVien"].ToString();

                    FormChiTietDonHang form = new FormChiTietDonHang(
                        maVe, maGhe, xuatChieuID, giaVe,
                        phongID, ngayChieu, phimID, ngayDat,
                        tongTien, idNhanVien
                    );
                    form.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Không tìm thấy thông tin đơn hàng!");
                }
            }
        }

        private DataRow GetDonHangByMaVe(string maVe)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT dh.*, xc.NgayChieu, xc.PhimID, xc.PhongID
                    FROM DonHang dh
                    JOIN XuatChieu xc ON dh.XuatChieuID = xc.XuatChieuID
                    WHERE dh.MaVe = @MaVe";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                adapter.SelectCommand.Parameters.AddWithValue("@MaVe", maVe);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                if (dt.Rows.Count > 0)
                    return dt.Rows[0];
                else
                    return null;
            }
        }

        private void cboNV_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Remove automatic data loading
            // Data will only be loaded when the search button is clicked
        }

        // Add event handlers for the checkboxes
        private void checkBoxStartDate_CheckedChanged(object sender, EventArgs e)
        {
            dateTimePickerStartDate.Enabled = checkBoxStartDate.Checked;
        }
        private void checkBoxEndDate_CheckedChanged(object sender, EventArgs e)
        {
            dateTimePickerEndDate.Enabled = checkBoxEndDate.Checked;
        }

        private void comboBoxQuarter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxQuarter.SelectedIndex > 0)
            {
                comboBoxMonth.SelectedIndex = 0;
                comboBoxMonth.Enabled = false;
            }
            else
            {
                comboBoxMonth.Enabled = true;
            }
        }
        private void comboBoxMonth_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxMonth.SelectedIndex > 0)
            {
                comboBoxQuarter.SelectedIndex = 0;
                comboBoxQuarter.Enabled = false;
            }
            else
            {
                comboBoxQuarter.Enabled = true;
            }
        }

        private void comboBoxQuarter_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

            private void btnBieuDo_Click(object sender, EventArgs e)
        {
            // Lấy các tham số lọc tương tự như khi tìm kiếm
            DateTime? startDate = null;
            DateTime? endDate = null;
            int? idNhanVien = null;
            int? yearFilter = null;

            // Lọc theo ngày
            if (checkBoxStartDate.Checked)
                startDate = dateTimePickerStartDate.Value.Date;
            if (checkBoxEndDate.Checked)
                endDate = dateTimePickerEndDate.Value.Date.AddDays(1).AddMilliseconds(-1);

            // Lọc theo năm
            if (comboBoxYear.SelectedIndex > 0)
                yearFilter = int.Parse(comboBoxYear.SelectedItem.ToString());

            // Lọc theo tháng
            if (comboBoxYear.SelectedIndex > 0 && comboBoxMonth.SelectedIndex > 0)
            {
                string selectedMonth = comboBoxMonth.SelectedItem.ToString();
                int month = int.Parse(selectedMonth.Split(' ')[1]);
                int year = int.Parse(comboBoxYear.SelectedItem.ToString());
                startDate = new DateTime(year, month, 1);
                endDate = startDate.Value.AddMonths(1).AddDays(-1);
            }

            // Lọc theo quý
            if (comboBoxYear.SelectedIndex > 0 && comboBoxQuarter.SelectedIndex > 0)
            {
                string selectedQuarter = comboBoxQuarter.SelectedItem.ToString();
                int quarter = int.Parse(selectedQuarter.Split(' ')[1]);
                int year = int.Parse(comboBoxYear.SelectedItem.ToString());
                int startMonth = (quarter - 1) * 3 + 1;
                startDate = new DateTime(year, startMonth, 1);
                endDate = startDate.Value.AddMonths(3).AddDays(-1);
            }

            // Lọc theo nhân viên
            if (cboNV.SelectedIndex > 0 && cboNV.SelectedValue != null && int.TryParse(cboNV.SelectedValue.ToString(), out int tempId))
            {
                idNhanVien = tempId;
            }

            // Mở form biểu đồ với các tham số lọc
            FormBieuDoDoanhThu formBieuDo = new FormBieuDoDoanhThu(startDate, endDate, idNhanVien, yearFilter);
            formBieuDo.ShowDialog();
        }
        

        private void Sum_Click(object sender, EventArgs e)
        {

        }
    }
}