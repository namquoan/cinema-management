using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace Pbl_test
{
    public partial class FormChiTietDonHang : Form
    {
        public FormChiTietDonHang(string maVe, string maGhe, string xuatChieuID, string giaVe,
                                  string phongID, string ngayChieu, string phimID, string ngayDat,
                                  string tongTien, string idNhanVien)
        {
            InitializeComponent();
            txtMaVe.Text = maVe;
            txtMaGhe.Text = maGhe;
            txtXuatChieu.Text = xuatChieuID;
            txtGiaVe.Text = giaVe;
            txtPhong.Text = phongID;
            txtNgayChieu.Text = ngayChieu;
            txtPhim.Text = phimID;
            txtNgayDat.Text = ngayDat;
            txtTongTien.Text = tongTien;
            txtNhanVien.Text = idNhanVien;
        }
    }
}
