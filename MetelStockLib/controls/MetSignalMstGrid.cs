using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetelStockLib.db;
using MetelStockLib.core;

namespace MetelStockLib.controls
{
    public partial class MetSignalMstGrid : UserControl
    {
        OpenSystemTrading ost;
        DBManager dbm;

        public MetSignalMstGrid()
        {
            InitializeComponent();
            ost = OpenSystemTrading.GetInstance();
            dbm = new DBManager();

            ost.OnReceiveSignalMst += OST_OnReceiveSignalMst;

        }

        private void OST_OnReceiveSignalMst(object sender, OnReceiveSignalMstEventArgs e)
        {
            dgMst.DataSource = dbm.getMfAtMst(e.YMD, e.ItemCode);
        }

        public void SetGrid(string ymd, string itemCode)
        {
            dgMst.DataSource = dbm.getMfAtMst(ymd, itemCode);
        }
    }
}
