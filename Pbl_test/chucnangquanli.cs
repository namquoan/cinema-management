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
    public partial class chucnangquanli : Form
    {
        public chucnangquanli()
        {
            InitializeComponent();
        }
        private void ShowControl(UserControl control)
        {
            panelContent.Controls.Clear(); // Xóa control cũ
            control.Dock = DockStyle.Fill; // Để UserControl lấp đầy panel
            panelContent.Controls.Add(control); // Thêm UserControl vào panel
        }

        private void btnquanLiNhanVien_Click(object sender, EventArgs e)
        {
            ShowControl(new QuanLiNhanVien());
        }

        private void btnquanLiPhim_Click(object sender, EventArgs e)
        {
            ShowControl(new QuanliPhim());
        }

        private void chucnangquanli_Load(object sender, EventArgs e)
        {
            ShowControl(new Tranhg_chủ());
        }

        private void btnbaoCaoDoanhThu_Click(object sender, EventArgs e)
        {
            ShowControl(new baoCaoDoanhThu());
        }

        private void btntrangChu_Click(object sender, EventArgs e)
        {
            ShowControl(new Tranhg_chủ());
        }
    }
}
