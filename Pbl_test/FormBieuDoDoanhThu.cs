using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace Pbl_test
{
    // Define an enum for chart view types
    public enum ViewType
    {
        Day,
        Month,
        Year,
        CompareMonths
    }

    public partial class FormBieuDoDoanhThu : Form
    {
        private string connectionString = @"Data Source=DESKTOP-HHKF7O6;Initial Catalog=POBOLO;Integrated Security=True;TrustServerCertificate=True";
        private ViewType currentViewType;

        // Store initial filter parameters
        private DateTime? initialFromDate;
        private DateTime? initialToDate;
        private int? initialIdNhanVien;
        private int? initialYearFilter;
        private List<(int year, int month)> initialMonthsToCompare;

        private RadioButton rbDayView;
        private RadioButton rbMonthView;
        private RadioButton rbYearView;

        public FormBieuDoDoanhThu(DateTime? fromDate = null, DateTime? toDate = null, int? idNhanVien = null, int? yearFilter = null, ViewType viewType = ViewType.Day, List<(int year, int month)> monthsToCompare = null)
        {
            InitializeComponent();
            InitializeCustomComponents();

            // Store initial parameters
            initialFromDate = fromDate;
            initialToDate = toDate;
            initialIdNhanVien = idNhanVien;
            initialYearFilter = yearFilter;
            initialMonthsToCompare = monthsToCompare;

            currentViewType = viewType;

            // If comparing months, override the viewType and disable radio buttons
            if (initialMonthsToCompare != null && initialMonthsToCompare.Count > 0)
            {
                currentViewType = ViewType.CompareMonths;
                // Disable radio buttons for CompareMonths view
                if (rbDayView != null && rbMonthView != null && rbYearView != null)
                {
                    rbDayView.Enabled = false;
                    rbMonthView.Enabled = false;
                    rbYearView.Enabled = false;
                }
            }
            else // Enable radio buttons for Day, Month views
            {
                if (rbDayView != null && rbMonthView != null && rbYearView != null)
                {
                    rbDayView.Enabled = true;
                    rbMonthView.Enabled = true;
                    rbYearView.Enabled = true;
                }
            }

            // Set initial state of radio buttons if they exist
            if (rbDayView != null && rbMonthView != null && rbYearView != null)
            {
                if (currentViewType == ViewType.Day)
                    rbDayView.Checked = true;
                else if (currentViewType == ViewType.Month)
                    rbMonthView.Checked = true;
                else if (currentViewType == ViewType.Year)
                    rbYearView.Checked = true;
            }

            LoadChart(initialFromDate, initialToDate, initialIdNhanVien, initialYearFilter, initialMonthsToCompare);
        }

        private void InitializeCustomComponents()
        {
            this.chartDoanhThu = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.chartDoanhThu)).BeginInit();
            this.SuspendLayout();

            // Radio Buttons Panel
            Panel radioPanel = new Panel();
            radioPanel.Dock = DockStyle.Top;
            radioPanel.Height = 30;
            this.Controls.Add(radioPanel);

            // Radio Button - Day View
            rbDayView = new RadioButton();
            rbDayView.Text = "Thống kê theo ngày";
            rbDayView.AutoSize = true;
            rbDayView.Location = new Point(10, 5);
            rbDayView.CheckedChanged += RadioButton_CheckedChanged;
            radioPanel.Controls.Add(rbDayView);

            // Radio Button - Month View
            rbMonthView = new RadioButton();
            rbMonthView.Text = "Thống kê theo tháng";
            rbMonthView.AutoSize = true;
            rbMonthView.Location = new Point(rbDayView.Right + 20, 5);
            rbMonthView.CheckedChanged += RadioButton_CheckedChanged;
            radioPanel.Controls.Add(rbMonthView);

            // Radio Button - Year View
            rbYearView = new RadioButton();
            rbYearView.Text = "Thống kê theo năm";
            rbYearView.AutoSize = true;
            rbYearView.Location = new Point(rbMonthView.Right + 20, 5);
            rbYearView.CheckedChanged += RadioButton_CheckedChanged;
            radioPanel.Controls.Add(rbYearView);

            // chartDoanhThu
            this.chartDoanhThu.Dock = System.Windows.Forms.DockStyle.Fill; // Use Fill
            this.chartDoanhThu.Name = "chartDoanhThu";
            this.chartDoanhThu.TabIndex = 0;
            this.chartDoanhThu.Text = "Biểu đồ doanh thu"; // This text is less important now, title is set in LoadChart

            // Add a ChartArea
            ChartArea chartArea = new ChartArea();
            // Adjust the position of the chart area to make space for the title at the top
            // Position parameters are X, Y, Width, Height (as percentage of chart image)
            // Y = 20 means start 20% down from the top of the chart control.
            // Height = 80 means take up 80% of the height, leaving 20% at the top for the title.
            chartArea.Position = new ElementPosition(0, 20, 100, 80);
            this.chartDoanhThu.ChartAreas.Add(chartArea);

            // FormBieuDoDoanhThu
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.chartDoanhThu);
            this.Name = "FormBieuDoDoanhThu";
            this.Text = "Biểu đồ doanh thu";
            ((System.ComponentModel.ISupportInitialize)(this.chartDoanhThu)).EndInit();
            this.ResumeLayout(false);
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (rbDayView.Checked)
            {
                currentViewType = ViewType.Day;
            }
            else if (rbMonthView.Checked)
            {
                currentViewType = ViewType.Month;
            }
            else if (rbYearView.Checked)
            {
                currentViewType = ViewType.Year;
            }
            // Note: CompareMonths view is set from constructor and radio buttons are disabled

            // Reload the chart with the updated view type and stored parameters
            LoadChart(initialFromDate, initialToDate, initialIdNhanVien, initialYearFilter, initialMonthsToCompare);
        }

        private void LoadChart(DateTime? fromDate, DateTime? toDate, int? idNhanVien, int? yearFilter, List<(int year, int month)> monthsToCompare = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Detect if DonHang.KhuyenMai exists
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

                    string kmExpr = hasKhuyenMaiColumn == 1 ? "ISNULL(dh.KhuyenMai, 0)" : "0";
                    string soGheExpr = "CASE WHEN dh.maGhe IS NULL OR dh.maGhe = '' THEN 0 ELSE LEN(dh.maGhe) - LEN(REPLACE(dh.maGhe, ',', '')) + 1 END";
                    string doanhThuComponent = $"({soGheExpr}) * dh.giaVe - (({soGheExpr}) * dh.giaVe * {kmExpr} / 100)";

                    string query;
                    if (monthsToCompare != null && monthsToCompare.Count > 0)
                    {
                        // Query for comparing specific months
                        query = @"
                        SELECT 
                            YEAR(dh.ngayDat) AS Nam, 
                            MONTH(dh.ngayDat) AS Thang,
                            SUM(" + doanhThuComponent + @") AS DoanhThu
                        FROM DonHang dh
                        JOIN NhanVien nv ON dh.IDNhanVien = nv.IDNhanVien
                        WHERE 1=1";

                        // Add conditions for each specific month
                        query += " AND (";
                        for (int i = 0; i < monthsToCompare.Count; i++)
                        {
                            query += $"(YEAR(dh.ngayDat) = @year{i} AND MONTH(dh.ngayDat) = @month{i})";
                            if (i < monthsToCompare.Count - 1)
                            {
                                query += " OR ";
                            }
                        }
                        query += ")";

                        if (idNhanVien.HasValue)
                            query += " AND dh.IDNhanVien = @idNhanVien";

                        query += " GROUP BY YEAR(dh.ngayDat), MONTH(dh.ngayDat) ORDER BY Nam, Thang";

                    }
                    else if (currentViewType == ViewType.Month)
                    {
                        query = @"
                        SELECT 
                            DATEFROMPARTS(YEAR(dh.ngayDat), MONTH(dh.ngayDat), 1) AS Thang,
                            SUM(" + doanhThuComponent + @") AS DoanhThu
                        FROM DonHang dh
                        JOIN NhanVien nv ON dh.IDNhanVien = nv.IDNhanVien
                        WHERE 1=1";
                    }
                    else if (currentViewType == ViewType.Year)
                    {
                        query = @"
                        SELECT 
                            YEAR(dh.ngayDat) AS Nam,
                            SUM(" + doanhThuComponent + @") AS DoanhThu
                        FROM DonHang dh
                        JOIN NhanVien nv ON dh.IDNhanVien = nv.IDNhanVien
                        WHERE 1=1";
                    }
                    else
                    {
                        query = @"
                        SELECT 
                            CAST(dh.ngayDat AS DATE) AS Ngay,
                            SUM(" + doanhThuComponent + @") AS DoanhThu
                        FROM DonHang dh
                        JOIN NhanVien nv ON dh.IDNhanVien = nv.IDNhanVien
                        WHERE 1=1";
                    }

                    // Thêm điều kiện lọc
                    if (fromDate.HasValue)
                        query += " AND dh.ngayDat >= @fromDate";
                    if (toDate.HasValue)
                        query += " AND dh.ngayDat <= @toDate";
                    if (idNhanVien.HasValue)
                        query += " AND dh.IDNhanVien = @idNhanVien";
                    if (yearFilter.HasValue)
                        query += " AND YEAR(dh.ngayDat) = @yearFilter";

                    if (currentViewType == ViewType.Month)
                        query += " GROUP BY DATEFROMPARTS(YEAR(dh.ngayDat), MONTH(dh.ngayDat), 1) ORDER BY Thang";
                    else if (currentViewType == ViewType.Year)
                        query += " GROUP BY YEAR(dh.ngayDat) ORDER BY YEAR(dh.ngayDat)";
                    else if (monthsToCompare == null || monthsToCompare.Count == 0)
                        query += " GROUP BY CAST(dh.ngayDat AS DATE) ORDER BY Ngay";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (fromDate.HasValue)
                            cmd.Parameters.Add("@fromDate", SqlDbType.DateTime).Value = fromDate.Value;
                        if (toDate.HasValue)
                            cmd.Parameters.Add("@toDate", SqlDbType.DateTime).Value = toDate.Value;
                        if (idNhanVien.HasValue)
                            cmd.Parameters.Add("@idNhanVien", SqlDbType.Int).Value = idNhanVien.Value;
                        if (yearFilter.HasValue)
                            cmd.Parameters.Add("@yearFilter", SqlDbType.Int).Value = yearFilter.Value;

                        if (monthsToCompare != null && monthsToCompare.Count > 0)
                        {
                            for (int i = 0; i < monthsToCompare.Count; i++)
                            {
                                cmd.Parameters.Add($"@year{i}", SqlDbType.Int).Value = monthsToCompare[i].year;
                                cmd.Parameters.Add($"@month{i}", SqlDbType.Int).Value = monthsToCompare[i].month;
                            }
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            chartDoanhThu.Series.Clear();
                            var series = new Series("Doanh thu");
                            series.ChartType = SeriesChartType.Column;

                            if (monthsToCompare != null && monthsToCompare.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    series.Points.AddXY($"{row["Nam"]}-{row["Thang"]}", row["DoanhThu"]);
                                }
                            }
                            else if (currentViewType == ViewType.Month)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    series.Points.AddXY(((DateTime)row["Thang"]).ToString("MM/yyyy"), row["DoanhThu"]);
                                }
                            }
                            else if (currentViewType == ViewType.Year)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    series.Points.AddXY(row["Nam"].ToString(), row["DoanhThu"]);
                                }
                            }
                            else
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    series.Points.AddXY(((DateTime)row["Ngay"]).ToString("dd/MM/yyyy"), row["DoanhThu"]);
                                }
                            }

                            chartDoanhThu.Series.Add(series);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu biểu đồ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private System.Windows.Forms.DataVisualization.Charting.Chart chartDoanhThu;

        private void FormBieuDoDoanhThu_Load(object sender, EventArgs e)
        {

        }
    }
}