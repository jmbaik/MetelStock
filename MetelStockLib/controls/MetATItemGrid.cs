using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetelStockLib.core;
using MetelStockLib.db;

namespace MetelStockLib.controls
{
    public partial class MetATItemGrid : UserControl
    {
        OpenSystemTrading ost;
        DBManager dbm;

        public MetATItemGrid()
        {
            InitializeComponent();
            ost = OpenSystemTrading.GetInstance();
            dbm = new DBManager();
        }

        public void SetGrid()
        {
            dg대상종목.DataSource = dbm.getTableMfAtItem(OpenSystemTrading.getYmd()).DefaultView;

            
        }
    }
}
