using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pbl_test
{
    public partial class chucnangnhanvien : Form
    {
        private int loggedInNhanVienId; // Thêm biến để lưu ID nhân viên

        // Hàm tạo mặc định (có thể giữ lại nếu cần, nhưng không khuyến khích sử dụng trực tiếp từ Form1)
        // public chucnangnhanvien()
        // {
        //     InitializeComponent();
        //     ShowControl(new Tranhg_chủ());
        //     // Cần xem xét IDNhanVien sẽ là gì nếu dùng hàm tạo này
        // }

        // Thêm hàm tạo mới để nhận ID nhân viên từ form đăng nhập
        public chucnangnhanvien(int nhanVienId)
        {
            InitializeComponent();
            this.loggedInNhanVienId = nhanVienId; // Lưu ID nhân viên
            ShowControl(new Tranhg_chủ()); // Mặc định hiển thị trang chủ khi mở form
        }

        private void ShowControl(UserControl control)
        {
            panelContentNV.Controls.Clear(); // Xóa control cũ
            control.Dock = DockStyle.Fill; // Để UserControl lấp đầy panel
            panelContentNV.Controls.Add(control); // Thêm UserControl vào panel
        }

        private void guna2HtmlLabel1_Click(object sender, EventArgs e)
        {
            ShowControl(new Tranhg_chủ());
        }

        private void lblDatVe_Click(object sender, EventArgs e)
        {
            // Truyền ID nhân viên khi tạo datVe
            ShowControl(new datVe(loggedInNhanVienId: this.loggedInNhanVienId));
        }

        private void panelContentNV_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblDangXuat_Click(object sender, EventArgs e)
        {
            // Logic đăng xuất đã được xử lý trong btnDangXuat_Click
            btnDangXuat_Click(sender, e);
        }

        private void btnTrangChu_Click(object sender, EventArgs e)
        {
            ShowControl(new Tranhg_chủ());
        }

        private void btnDatVe_Click(object sender, EventArgs e)
        {
            // Truyền ID nhân viên khi tạo datVe
            ShowControl(new datVe(loggedInNhanVienId: this.loggedInNhanVienId));
        }

        private void btnDangXuat_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
