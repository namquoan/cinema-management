using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace Pbl_test
{
    public partial class FormBangThoiGian : Form
    {
        // Variable to store the selected showtime details (Date, Start Time, Room ID)
        private (DateTime? NgayChieu, TimeSpan? GioBatDau, int? PhongID) selectedShowtimeDetails = (null, null, null);

        private string connectionString = @"Data Source=DESKTOP-HHKF7O6;Initial Catalog=POBOLO;Integrated Security=True;TrustServerCertificate=True";

        private QuanliPhim _parentForm;
        private int _phongId; // Thêm biến lưu trữ phòng ID
        private int _phimId; // Thêm biến lưu trữ phim ID

        public FormBangThoiGian(QuanliPhim parentForm, int phongId, int phimId)
        {
            InitializeComponent();
            _parentForm = parentForm;
            _phongId = phongId;
            _phimId = phimId; // Lưu phim ID được truyền vào
            dgvThoiGianBieu.CellClick += dgvThoiGianBieu_CellClick;
            btnXacNhan.Click += btnXacNhan_Click;

            // Add event handler for DefaultValuesNeeded for the new row
            dgvThoiGianBieu.DefaultValuesNeeded += dgvThoiGianBieu_DefaultValuesNeeded;

            // Cấu hình DataGridView
            ConfigureDataGridView();

            // Setup ComboBoxes for date selection
            SetupDateComboBoxes();

            // Add event handlers for ComboBoxes
            cboYear.SelectedIndexChanged += DateComboBox_SelectedIndexChanged;
            cboMonth.SelectedIndexChanged += DateComboBox_SelectedIndexChanged;
            cboDay.SelectedIndexChanged += DateComboBox_SelectedIndexChanged;
        }

        private void ConfigureDataGridView()
        {
            dgvThoiGianBieu.AutoGenerateColumns = false;
            dgvThoiGianBieu.Columns.Clear();
            dgvThoiGianBieu.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvThoiGianBieu.AllowUserToAddRows = true; // Allow adding new rows manually

            // Add columns manually
            // Add Time column as an editable text box
            dgvThoiGianBieu.Columns.Add(new DataGridViewTextBoxColumn { Name = "Time", HeaderText = "Khung giờ", DataPropertyName = "Time" }); // Make Khung giờ column editable

            // Add Khuyến mãi column
            dgvThoiGianBieu.Columns.Add(new DataGridViewTextBoxColumn { Name = "Promotion", HeaderText = "Khuyến mãi", DataPropertyName = "Promotion" }); // Make Khuyến mãi column editable

            // Change Room to a ComboBox column
            DataGridViewComboBoxColumn roomColumn = new DataGridViewComboBoxColumn();
            roomColumn.Name = "Room";
            roomColumn.HeaderText = "Phòng";
            roomColumn.DataPropertyName = "PhongID"; // Ensure this matches the DataTable column name
            roomColumn.DataSource = GetRoomList(); // Method to get list of rooms
            roomColumn.DisplayMember = "PhongID"; // Display PhongID
            roomColumn.ValueMember = "PhongID";     // Use PhongID as the value
            dgvThoiGianBieu.Columns.Add(roomColumn);

            // Add Phim column to display the movie title for existing showtimes
            dgvThoiGianBieu.Columns.Add(new DataGridViewTextBoxColumn { Name = "Phim", HeaderText = "Phim", DataPropertyName = "TenPhim", ReadOnly = true }); // Add Phim column

            // Add ThoiLuong column
            dgvThoiGianBieu.Columns.Add(new DataGridViewTextBoxColumn { Name = "ThoiLuong", HeaderText = "Thời lượng", DataPropertyName = "ThoiLuong", ReadOnly = true }); // Add ThoiLuong column

            // Add ShowtimeID column (hidden, but used for data binding and logic)
            // Although hidden, define it to ensure DataPropertyName can link to DataTable
            dgvThoiGianBieu.Columns.Add(new DataGridViewTextBoxColumn { Name = "ShowtimeID", HeaderText = "Mã Xuất Chiếu", DataPropertyName = "XuatChieuID", Visible = true, ReadOnly = true }); // Hiển thị lại cột Mã Xuất Chiếu

            // Set column widths or AutoSizeMode
            dgvThoiGianBieu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Make all columns fill the available space

            // Add event handler for CellValueChanged, specifically for the Room column
            dgvThoiGianBieu.CellValueChanged += dgvThoiGianBieu_CellValueChanged;
            // Add event handler for CurrentCellDirtyStateChanged to commit changes in ComboBox immediately
            dgvThoiGianBieu.CurrentCellDirtyStateChanged += dgvThoiGianBieu_CurrentCellDirtyStateChanged;
        }

        private void SetupDateComboBoxes()
        {
            // Populate Year ComboBox (e.g., current year +/- a few years)
            int currentYear = DateTime.Now.Year;
            for (int year = currentYear; year <= currentYear + 5; year++)
            {
                cboYear.Items.Add(year);
            }
            cboYear.SelectedItem = currentYear;

            // Populate Month ComboBox
            for (int month = 1; month <= 12; month++)
            {
                // Only add months from the current month onwards if the current year is selected
                if ((int)cboYear.SelectedItem == currentYear && month < DateTime.Now.Month)
                {
                    continue; // Skip past months in the current year
                }
                cboMonth.Items.Add(month);
            }
            cboMonth.SelectedItem = DateTime.Now.Month;

            // Populate Day ComboBox based on initial Year and Month
            PopulateDayComboBox();
            cboDay.SelectedItem = DateTime.Now.Day;
        }

        private void PopulateDayComboBox()
        {
            cboDay.Items.Clear();
            if (cboYear.SelectedItem != null && cboMonth.SelectedItem != null)
            {
                int year = (int)cboYear.SelectedItem;
                int month = (int)cboMonth.SelectedItem;
                int daysInMonth = DateTime.DaysInMonth(year, month);
                int startDay = 1;

                // If the selected year is the current year and the selected month is the current month,
                // start populating from the current day.
                if (year == DateTime.Now.Year && month == DateTime.Now.Month)
                {
                    startDay = DateTime.Now.Day;
                }

                for (int day = startDay; day <= daysInMonth; day++)
                {
                    cboDay.Items.Add(day);
                }
            }
        }

        private void DateComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Repopulate days if year or month changes
            if (sender == cboYear || sender == cboMonth)
            {
                PopulateDayComboBox();
            }

            // Attempt to load showtimes if a full date is selected
            if (cboYear.SelectedItem != null && cboMonth.SelectedItem != null && cboDay.SelectedItem != null)
            {
                int year = (int)cboYear.SelectedItem;
                int month = (int)cboMonth.SelectedItem;
                int day = (int)cboDay.SelectedItem;

                DateTime selectedDate;
                try
                {
                    selectedDate = new DateTime(year, month, day);
                }
                catch (ArgumentOutOfRangeException)
                {
                    MessageBox.Show("Ngày đã chọn không hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the selected date is in the past
                if (selectedDate < DateTime.Today)
                {
                    MessageBox.Show("Không thể chọn ngày trong quá khứ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    // Optionally reset the day combo box to today's date or clear the selection
                    // For now, just return to prevent loading past data
                    dgvThoiGianBieu.DataSource = null; // Clear grid for past dates
                    return; // Stop execution
                }

                LoadShowtimesForSelectedDate();
            }
            else
            {
                dgvThoiGianBieu.DataSource = null; // Clear grid if date is incomplete
            }
        }

        private void LoadShowtimesForSelectedDate()
        {
            // Get selected date from ComboBoxes
            if (cboYear.SelectedItem == null || cboMonth.SelectedItem == null || cboDay.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn ngày, tháng, và năm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int year, month, day;
            if (!int.TryParse(cboYear.SelectedItem?.ToString(), out year) ||
                !int.TryParse(cboMonth.SelectedItem?.ToString(), out month) ||
                !int.TryParse(cboDay.SelectedItem?.ToString(), out day))
            {
                MessageBox.Show("Ngày không hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime selectedDate;
            try
            {
                selectedDate = new DateTime(year, month, day);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Ngày đã chọn không hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get movie length (not directly needed for generating the initial list of slots, but useful for context/validation later)
            int movieLengthMinutes = GetMovieLength(_phimId);
            // Add 15 minutes for ads and cleaning (also for later use)
            int totalShowtimeLength = movieLengthMinutes + 15;

            // Create a new DataTable for display
            DataTable displayTable = new DataTable();

            // Define columns explicitly to ensure correct schema
            displayTable.Columns.Clear(); // Clear any existing columns
            displayTable.Columns.Add("Time", typeof(string)); // Store start time as string for editing
            displayTable.Columns.Add("PhongID", typeof(int)); // Store Room ID
            displayTable.Columns.Add("XuatChieuID", typeof(string)); // Store generated/existing ShowtimeID
            displayTable.Columns.Add("TenPhim", typeof(string)); // Store movie title
            displayTable.Columns.Add("ThoiLuong", typeof(string)); // Store calculated duration
            displayTable.Columns.Add("PhimID", typeof(int)); // Store movie ID (hidden)

            // Create a dictionary to temporarily hold existing showtime data
            Dictionary<TimeSpan, (int phongId, string xuatChieuId, string tenPhim, int phimId)> existingShowtimeData = new Dictionary<TimeSpan, (int, string, string, int)>();

            // Get existing showtimes for the selected date and room (to pre-fill if they exist)
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Select all showtimes for the selected date (regardless of room initially, though the form is tied to one room)
                // We will filter by room when populating the display table.
                string query = @"SELECT XC.GioBatDau, XC.PhongID, XC.XuatChieuID AS XuatChieuID, P.TenPhim, XC.PhimID, P.DoDaiPhim
                               FROM XuatChieu XC
                               JOIN Phim P ON XC.PhimID = P.PhimID
                               WHERE XC.NgayChieu = @ngayChieu"; // Show all movies for the selected date
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ngayChieu", selectedDate);
                    System.Diagnostics.Debug.WriteLine("Executing query in LoadShowtimesForSelectedDate: " + query);
                    System.Diagnostics.Debug.WriteLine("Parameters: @ngayChieu=" + selectedDate);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        System.Diagnostics.Debug.WriteLine("Query executed successfully.");
                        // Debug: Print column names returned by the reader
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            System.Diagnostics.Debug.WriteLine($"Column {i}: {reader.GetName(i)}");
                        }
                        while (reader.Read())
                        {
                            TimeSpan startTime = (TimeSpan)reader["GioBatDau"];
                            int phongId = (int)reader["PhongID"];
                            string xuatChieuId = reader.GetString(reader.GetOrdinal("XuatChieuID"));
                            string tenPhim = reader["TenPhim"].ToString();
                            int phimId = (int)reader["PhimID"];
                            TimeSpan movieDuration = (TimeSpan)reader["DoDaiPhim"]; // Read the movie duration
                            int totalShowtimeLengthMinutes = (int)movieDuration.TotalMinutes + 15; // Calculate total duration with buffer

                            // Debug: Print values read from the database
                            System.Diagnostics.Debug.WriteLine($"Read from DB - GioBatDau: {startTime}, PhongID: {phongId}, XuatChieuID: {xuatChieuId}, TenPhim: {tenPhim}, PhimID: {phimId}, DoDaiPhim: {movieDuration}");

                            // Store existing showtimes by start time, including room ID and XuatChieuID
                            DataRow existingRow = displayTable.NewRow();
                            existingRow["Time"] = startTime.ToString(@"hh\:mm"); // Store start time as string
                            existingRow["PhongID"] = phongId; // Store Room ID
                            existingRow["XuatChieuID"] = xuatChieuId; // Store generated/existing ShowtimeID
                            existingRow["TenPhim"] = tenPhim; // Store movie title
                            existingRow["ThoiLuong"] = TimeSpan.FromMinutes(totalShowtimeLengthMinutes).ToString(@"hh\:mm"); // Display calculated duration
                            existingRow["PhimID"] = phimId; // Store movie ID (hidden)
                            displayTable.Rows.Add(existingRow);
                        }
                    }
                }
            }


            int currentMovieLengthMinutes = GetMovieLength(_phimId);
            string currentMovieDurationString = TimeSpan.FromMinutes(currentMovieLengthMinutes + 15).ToString(@"hh\:mm");
            string currentMovieTitle = GetMovieTitle(_phimId);

            foreach (DataRow row in displayTable.Rows)
            {
                if (row["XuatChieuID"] != DBNull.Value && !string.IsNullOrEmpty(row["XuatChieuID"].ToString()))
                {
                    if (row["PhimID"] != DBNull.Value && (int)row["PhimID"] == _phimId)
                    {
                        row["TenPhim"] = "→ " + row["TenPhim"].ToString();
                    }
                }
            }
            dgvThoiGianBieu.DataSource = displayTable;
            if (dgvThoiGianBieu.Columns.Contains("Room"))
            {
                dgvThoiGianBieu.Columns["Room"].HeaderText = "Phòng";
            }

            if (dgvThoiGianBieu.Columns.Contains("ShowtimeID"))
            {
                dgvThoiGianBieu.Columns["ShowtimeID"].HeaderText = "Mã Xuất Chiếu";
            }
        }

        private void dgvThoiGianBieu_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // This method is primarily for row highlighting now.
            if (e.RowIndex >= 0)
            {
                // Keep highlighting the current row on click
                 foreach (DataGridViewRow row in dgvThoiGianBieu.Rows)
                 {
                     row.DefaultCellStyle.BackColor = dgvThoiGianBieu.DefaultCellStyle.BackColor;
                     row.DefaultCellStyle.ForeColor = dgvThoiGianBieu.DefaultCellStyle.ForeColor;
                 }
                 dgvThoiGianBieu.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightBlue; // Highlight color
                 dgvThoiGianBieu.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
                 dgvThoiGianBieu.Rows[e.RowIndex].Selected = true; // Explicitly select the row
            }
        }

        private void btnXacNhan_Click(object sender, EventArgs e)
        {
            // Check if a row is selected or if the new row is being edited
            DataGridViewRow rowToProcess = null;
            if (dgvThoiGianBieu.CurrentRow != null && dgvThoiGianBieu.CurrentRow.IsNewRow)
            {
                // If the user is in the new row, process that row
                rowToProcess = dgvThoiGianBieu.CurrentRow;
            }
            else if (dgvThoiGianBieu.SelectedRows.Count > 0)
            {
                // If an existing row is selected, process the first selected row
                rowToProcess = dgvThoiGianBieu.SelectedRows[0];
            }

            if (rowToProcess == null)
            {
                MessageBox.Show("Vui lòng chọn hoặc nhập một suất chiếu vào bảng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the length of the movie for the current showtime being added once at the beginning
            int movieLengthMinutes = GetMovieLength(_phimId);

            // Extract details from the DataRow
            DateTime selectedDate;
            TimeSpan startTime;
            int selectedRoomId;
            string existingShowtimeId = null;

            // Get Date from ComboBoxes (already validated in LoadShowtimesForSelectedDate, but re-check for safety)
            if (cboYear.SelectedItem == null || cboMonth.SelectedItem == null || cboDay.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn ngày, tháng, và năm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int year, month, day;
            if (!int.TryParse(cboYear.SelectedItem?.ToString(), out year) ||
                !int.TryParse(cboMonth.SelectedItem?.ToString(), out month) ||
                !int.TryParse(cboDay.SelectedItem?.ToString(), out day))
            {
                MessageBox.Show("Ngày không hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                selectedDate = new DateTime(year, month, day);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Ngày đã chọn không hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get Start Time from the "Time" column of the selected row
            if (rowToProcess.Cells["Time"].Value == null || rowToProcess.Cells["Time"].Value == DBNull.Value)
            {
                MessageBox.Show("Không thể lấy giờ bắt đầu từ hàng đã chọn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!TimeSpan.TryParse(rowToProcess.Cells["Time"].Value.ToString(), out startTime))
            {
                MessageBox.Show("Định dạng giờ bắt đầu không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get Room ID from the "PhongID" column of the selected row
            if (rowToProcess.Cells["Room"].Value == null || rowToProcess.Cells["Room"].Value == DBNull.Value)
            {
                MessageBox.Show("Vui lòng chọn phòng cho suất chiếu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            selectedRoomId = (int)rowToProcess.Cells["Room"].Value;

            // Get existing ShowtimeID from the "XuatChieuID" column (it's hidden but in the DataTable)
            if (rowToProcess.Cells["ShowtimeID"].Value != null && rowToProcess.Cells["ShowtimeID"].Value != DBNull.Value)
            {
                existingShowtimeId = rowToProcess.Cells["ShowtimeID"].Value.ToString();
            }

            // --- Implement Duplicate Check ---
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                TimeSpan newShowtimeDuration = TimeSpan.FromMinutes(movieLengthMinutes + 15); // Add 15 mins buffer
                TimeSpan newShowtimeEndTime = startTime.Add(newShowtimeDuration);

                // Query for existing showtimes in the same room and on the same date
                string overlapCheckQuery = @"SELECT XC.GioBatDau, P.DoDaiPhim
                                               FROM XuatChieu XC
                                               JOIN Phim P ON XC.PhimID = P.PhimID
                                               WHERE XC.NgayChieu = @NgayChieu AND XC.PhongID = @PhongID";

                using (SqlCommand overlapCmd = new SqlCommand(overlapCheckQuery, conn))
                {
                    overlapCmd.Parameters.AddWithValue("@NgayChieu", selectedDate.Date);
                    overlapCmd.Parameters.AddWithValue("@PhongID", selectedRoomId);

                    using (SqlDataReader reader = overlapCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TimeSpan existingStartTime = (TimeSpan)reader["GioBatDau"];
                            TimeSpan existingMovieDuration = (TimeSpan)reader["DoDaiPhim"];
                            int existingMovieLengthMinutes = (int)existingMovieDuration.TotalMinutes;
                            TimeSpan existingShowtimeDuration = TimeSpan.FromMinutes(existingMovieLengthMinutes + 15);
                            TimeSpan existingShowtimeEndTime = existingStartTime.Add(existingShowtimeDuration);

                            // Check for overlap
                            // Overlap exists if (StartA < EndB and EndA > StartB)
                            if ((startTime < existingShowtimeEndTime) && (newShowtimeEndTime > existingStartTime))
                            {
                                 // If a duplicate exists and it's for a NEW showtime, prevent adding.
                                 if (string.IsNullOrEmpty(existingShowtimeId))
                                 {
                                      MessageBox.Show("Suất chiếu bị trùng lặp thời gian với suất chiếu khác trong cùng phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                      return; // Stop execution
                                 }
                            }
                        }
                    }
                }

                // The original check for exact start time duplicate can be removed or kept as an additional check
                // I will keep the overlap check which is more comprehensive.

            }
            // --- End Duplicate Check ---

            // If we reach here, either it's a new showtime with no overlap, or it's an existing showtime.

            string showtimeIdToSave = existingShowtimeId;

            // If it's a new showtime, generate a new ShowtimeID
            if (string.IsNullOrEmpty(showtimeIdToSave))
            {
                 int sequenceNumber = GetNextShowtimeSequence(selectedDate);
                showtimeIdToSave = $"S{sequenceNumber}";
            }

            // Get and parse the end time from the selected showtime
            TimeSpan endTime = startTime.Add(TimeSpan.FromMinutes(movieLengthMinutes + 15)); // Add movie length + 15 mins ads

            // Call LuuSuatChieu with the full details
            float khuyenMai = 0;
            if (rowToProcess.Cells["Promotion"].Value != null && float.TryParse(rowToProcess.Cells["Promotion"].Value.ToString(), out float parsedKhuyenMai))
            {
                khuyenMai = parsedKhuyenMai;
            }
            LuuSuatChieu(showtimeIdToSave, _phimId, selectedRoomId, selectedDate, startTime, endTime, khuyenMai);

            // Pass the generated/existing ShowtimeID back to the parent form
            _parentForm.SetXuatChieu(showtimeIdToSave); // Assuming SetXuatChieu expects ShowtimeID

            // Reload the showtimes to update the grid
            LoadShowtimesForSelectedDate();

            // Close the form
            this.Close();
        }

        // Modified to save a single showtime with full details
        private void LuuSuatChieu(string xuatChieuId, int phimId, int phongId, DateTime ngayChieu, TimeSpan gioBatDau, TimeSpan gioKetThuc, float khuyenMai)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Check if the showtime already exists for this exact combination
                string checkQuery = "SELECT COUNT(*) FROM XuatChieu WHERE XuatChieuID = @XuatChieuID AND PhimID = @PhimID AND PhongID = @PhongID AND NgayChieu = @NgayChieu";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@XuatChieuID", xuatChieuId);
                checkCmd.Parameters.AddWithValue("@PhimID", phimId);
                checkCmd.Parameters.AddWithValue("@PhongID", phongId);
                checkCmd.Parameters.AddWithValue("@NgayChieu", ngayChieu);

                int count = (int)checkCmd.ExecuteScalar();

                if (count == 0)
                {
                    // Insert the new showtime
                    string insertQuery = @"INSERT INTO XuatChieu (XuatChieuID, PhimID, PhongID, NgayChieu, GioBatDau, GioKetThuc, KhuyenMai)
                                         VALUES (@XuatChieuID, @PhimID, @PhongID, @NgayChieu, @GioBatDau, @GioKetThuc, @KhuyenMai)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@XuatChieuID", xuatChieuId);
                        cmd.Parameters.AddWithValue("@PhimID", phimId);
                        cmd.Parameters.AddWithValue("@PhongID", phongId);
                        cmd.Parameters.AddWithValue("@NgayChieu", ngayChieu);
                        cmd.Parameters.AddWithValue("@GioBatDau", gioBatDau);
                        cmd.Parameters.AddWithValue("@GioKetThuc", gioKetThuc);
                        cmd.Parameters.AddWithValue("@KhuyenMai", khuyenMai); // Add KhuyenMai parameter

                        try
                        {
                            cmd.ExecuteNonQuery();
                            MessageBox.Show($"Đã thêm suất chiếu: {xuatChieuId} - {ngayChieu:yyyy-MM-dd}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            System.Diagnostics.Debug.WriteLine($"Suất chiếu {xuatChieuId} đã được thêm thành công vào DB."); // Debugging line
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi khi thêm {xuatChieuId} ngày {ngayChieu:yyyy-MM-dd}: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    // If the showtime exists, update it with the new promotion value
                    string updateQuery = @"UPDATE XuatChieu
                                         SET GioBatDau = @GioBatDau, GioKetThuc = @GioKetThuc, KhuyenMai = @KhuyenMai
                                         WHERE XuatChieuID = @XuatChieuID AND PhimID = @PhimID AND PhongID = @PhongID AND NgayChieu = @NgayChieu";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@GioBatDau", gioBatDau);
                        cmd.Parameters.AddWithValue("@GioKetThuc", gioKetThuc);
                        cmd.Parameters.AddWithValue("@KhuyenMai", khuyenMai); // Add KhuyenMai parameter for update
                        cmd.Parameters.AddWithValue("@XuatChieuID", xuatChieuId);
                        cmd.Parameters.AddWithValue("@PhimID", phimId);
                        cmd.Parameters.AddWithValue("@PhongID", phongId);
                        cmd.Parameters.AddWithValue("@NgayChieu", ngayChieu);

                        try
                        {
                            cmd.ExecuteNonQuery();
                            MessageBox.Show($"Đã cập nhật suất chiếu: {xuatChieuId} - {ngayChieu:yyyy-MM-dd}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            System.Diagnostics.Debug.WriteLine($"Suất chiếu {xuatChieuId} đã được cập nhật thành công trong DB."); // Debugging line
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi khi cập nhật {xuatChieuId} ngày {ngayChieu:yyyy-MM-dd}: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        // Method to get the list of rooms
        private DataTable GetRoomList()
        {
            DataTable roomTable = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT PhongID FROM PhongChieu"; // Select only PhongID
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                adapter.Fill(roomTable);
            }
            return roomTable;
        }

        // Helper method to get movie length (in minutes) from the database
        private int GetMovieLength(int phimId)
        {
            int length = 0;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Assuming your Phim table has a column named 'DoDaiPhim' (integer, minutes)
                string query = "SELECT DoDaiPhim FROM Phim WHERE PhimID = @PhimID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PhimID", phimId);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    TimeSpan duration = (TimeSpan)result;
                    length = (int)duration.TotalMinutes;
                }
            }
            return length;
        }

        // Helper method to determine the next sequential showtime number (S1, S2, etc.)
        private int GetNextShowtimeSequence(DateTime date)
        {
            int maxSequence = 0;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Get max S* for all showtimes (globally, not per day)
                string query = @"SELECT MAX(CAST(SUBSTRING(XuatChieuID, 2, LEN(XuatChieuID) - 1) AS INT))
                                FROM XuatChieu
                                WHERE XuatChieuID LIKE 'S[0-9]%'"; // Removed NgayChieu filter
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // cmd.Parameters.AddWithValue("@ngayChieu", date); // No longer needed
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        maxSequence = Convert.ToInt32(result);
                    }
                }
            }
            return maxSequence + 1;
        }

        // Helper method to get movie title from the database
        private string GetMovieTitle(int phimId)
        {
            string title = "";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT TenPhim FROM Phim WHERE PhimID = @PhimID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PhimID", phimId);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    title = result.ToString();
                }
            }
            return title;
        }

        private void dgvThoiGianBieu_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // Commit changes in the ComboBox immediately when the dirty state changes
            if (dgvThoiGianBieu.IsCurrentCellDirty)
            {
                dgvThoiGianBieu.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dgvThoiGianBieu_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Check if the changed cell is in the Room column and not the header row
            if (e.RowIndex >= 0 && dgvThoiGianBieu.Columns[e.ColumnIndex].Name == "Room")
            {
                DataGridViewRow row = dgvThoiGianBieu.Rows[e.RowIndex];
                DataGridViewComboBoxCell roomCell = row.Cells["Room"] as DataGridViewComboBoxCell;

                // Ensure the new room value is not null or DBNull
                if (roomCell != null && roomCell.Value != null && roomCell.Value != DBNull.Value)
                {
                    int selectedRoomId = (int)roomCell.Value;

                    // Get the selected date from ComboBoxes
                    if (cboYear.SelectedItem == null || cboMonth.SelectedItem == null || cboDay.SelectedItem == null)
                    {
                        // This shouldn't happen if LoadShowtimesForSelectedDate is called after date selection,
                        // but as a fallback, clear the selected room and exit.
                        roomCell.Value = DBNull.Value; // Reset the cell value
                        MessageBox.Show("Vui lòng chọn ngày, tháng, và năm trước khi chọn phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int year, month, day;
                    if (!int.TryParse(cboYear.SelectedItem?.ToString(), out year) ||
                        !int.TryParse(cboMonth.SelectedItem?.ToString(), out month) ||
                        !int.TryParse(cboDay.SelectedItem?.ToString(), out day))
                    {
                        roomCell.Value = DBNull.Value; // Reset the cell value
                        MessageBox.Show("Ngày không hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DateTime selectedDate;
                    try
                    {
                        selectedDate = new DateTime(year, month, day);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        roomCell.Value = DBNull.Value; // Reset the cell value
                        MessageBox.Show("Ngày đã chọn không hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Get the StartTime (TimeSpan) directly from the "Time" cell's value
                    DataGridViewCell timeCell = row.Cells["Time"];
                    if (timeCell == null || timeCell.Value == null)
                    {
                        MessageBox.Show("Could not retrieve time slot information from the grid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        roomCell.Value = DBNull.Value; // Reset the cell value
                        row.Cells["ShowtimeID"].Value = DBNull.Value; // Clear ShowtimeID
                        selectedShowtimeDetails = (null, null, null); // Clear stored selection
                        // Remove highlight if this row was highlighted
                        row.DefaultCellStyle.BackColor = dgvThoiGianBieu.DefaultCellStyle.BackColor;
                        row.DefaultCellStyle.ForeColor = dgvThoiGianBieu.DefaultCellStyle.ForeColor;
                        return;
                    }

                    // Get the string representation of the cell value and parse it into a TimeSpan
                    string timeValueString = timeCell.Value.ToString();
                    TimeSpan startTime;
                    try
                    {
                        startTime = TimeSpan.Parse(timeValueString);
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show($"Định dạng giờ không hợp lệ: {timeValueString}. Vui lòng nhập theo định dạng HH:mm hoặc hh:mm.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        roomCell.Value = DBNull.Value; // Reset the cell value
                        row.Cells["ShowtimeID"].Value = DBNull.Value; // Clear any preview
                        selectedShowtimeDetails = (null, null, null); // Clear stored selection
                        // Remove highlight
                        row.DefaultCellStyle.BackColor = dgvThoiGianBieu.DefaultCellStyle.BackColor;
                        row.DefaultCellStyle.ForeColor = dgvThoiGianBieu.DefaultCellStyle.ForeColor;
                        return;
                    }
                    catch (Exception ex)
                    {
                        // Catch any other exceptions during retrieval
                        MessageBox.Show($"An unexpected error occurred retrieving time cell value: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        roomCell.Value = DBNull.Value; // Reset the cell value
                        row.Cells["ShowtimeID"].Value = DBNull.Value; // Clear ShowtimeID
                        selectedShowtimeDetails = (null, null, null); // Clear stored selection
                        // Remove highlight if this row was highlighted
                        row.DefaultCellStyle.BackColor = dgvThoiGianBieu.DefaultCellStyle.BackColor;
                        row.DefaultCellStyle.ForeColor = dgvThoiGianBieu.DefaultCellStyle.ForeColor;
                        return;
                    }

                    // Keep highlighting the row when a room is selected
                    foreach (DataGridViewRow otherRow in dgvThoiGianBieu.Rows)
                    {
                        if (otherRow.Index != e.RowIndex)
                        {
                            otherRow.DefaultCellStyle.BackColor = dgvThoiGianBieu.DefaultCellStyle.BackColor;
                            otherRow.DefaultCellStyle.ForeColor = dgvThoiGianBieu.DefaultCellStyle.ForeColor;
                        }
                    }
                    row.DefaultCellStyle.BackColor = Color.LightBlue; // Highlight color
                    row.DefaultCellStyle.ForeColor = Color.Black;

                    // Store the selected showtime details in the dedicated variable
                    // Assuming single selection is intended, clear and add the new selection.
                    selectedShowtimeDetails = (selectedDate, startTime, selectedRoomId);

                    // Optionally, update the ShowtimeID cell here as a preview if desired
                    // string generatedShowtimeIdPreview = GenerateShowtimeIdPreview(selectedDate, selectedRoomId); // You'd need a preview method
                    // row.Cells["ShowtimeID"].Value = generatedShowtimeIdPreview;

                }
                else if (roomCell != null && (roomCell.Value == null || roomCell.Value == DBNull.Value))
                {
                    // If the room selection is cleared, also clear the stored selection and highlight
                    selectedShowtimeDetails = (null, null, null); // Clear stored selection

                    // Clear any previous ShowtimeID preview and remove highlight
                    row.Cells["ShowtimeID"].Value = DBNull.Value;
                    row.DefaultCellStyle.BackColor = dgvThoiGianBieu.DefaultCellStyle.BackColor;
                    row.DefaultCellStyle.ForeColor = dgvThoiGianBieu.DefaultCellStyle.ForeColor;
                }
            }
        }

        private void dgvThoiGianBieu_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells["Promotion"].Value = "Không";
            e.Row.Cells["Room"].Value = DBNull.Value;
            e.Row.Cells["ShowtimeID"].Value = DBNull.Value;

            // Populate default values for Phim and Thời lượng for the new row
            int movieLengthMinutes = GetMovieLength(_phimId);
            string movieDurationString = TimeSpan.FromMinutes(movieLengthMinutes + 15).ToString(@"hh\:mm");
            string currentMovieTitle = GetMovieTitle(_phimId);

            e.Row.Cells["Phim"].Value = currentMovieTitle;
            e.Row.Cells["ThoiLuong"].Value = movieDurationString;
        }
    }
}