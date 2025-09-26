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
    public partial class HoaDon : Form
    {
        public string TenPhim { get; set; }
        public DateTime NgayChieu { get; set; }
        public string SuatChieu { get; set; }
        public string PhongChieu { get; set; }
        public string DanhSachGhe { get; set; }
        public float GiaVe { get; set; }
        public float TongTien { get; set; }
        public DateTime NgayDat { get; set; }
        public string NhanVien { get; set; }
        public float KhuyenMai { get; set; }

        public HoaDon()
        {
            InitializeComponent();
        }

        private void HoaDon_Load(object sender, EventArgs e)
        {
            // Hiển thị thông tin lên các controls
            lblTenPhim.Text = TenPhim;
            lblNgayChieu.Text = NgayChieu.ToString("dd/MM/yyyy");
            lblSuatChieu.Text = SuatChieu;
            lblPhongChieu.Text = PhongChieu;
            lblDanhSachGhe.Text = DanhSachGhe;
            lblGiaVe.Text = GiaVe.ToString("N0") + " VND";
            lblTongTien.Text = TongTien.ToString("N0") + " VND";
            lblNgayDat.Text = NgayDat.ToString("dd/MM/yyyy HH:mm");
            lblNhanVien.Text = NhanVien;
            lblKhuyenMai.Text = KhuyenMai.ToString("N0") + " %";

            // Có thể thêm mã tạo hóa đơn tự động ở đây
            lblMaHoaDon.Text = "HD" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private void btnInHoaDon_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
