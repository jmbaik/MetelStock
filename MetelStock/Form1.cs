using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetelStock
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            lbItemInfo.Text = "종목명 : 종목코드 (일자) ";
            txtYmd.Text = DateTime.Now.ToString("yyyyMMdd");
            cbAvgTerm.SelectedIndex = 2;
            cbTickType.SelectedIndex = 1;
        }
    }
}
