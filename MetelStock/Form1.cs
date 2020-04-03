using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetelStockLib.core;

namespace MetelStock
{
    public partial class Form1 : Form
    {
        OpenSystemTrading ost;

        private string itemCode = "";
        public Form1()
        {
            InitializeComponent();
            ost = OpenSystemTrading.GetInstance();
            ost.SetAxKHOpenAPI(axKHOpenAPI1);

            // *** 초기 세팅 *********************************************
            lbItemInfo.Text = "종목명 : 종목코드 (일자) ";
            txtYmd.Text = DateTime.Now.ToString("yyyyMMdd");
            cbAvgTerm.SelectedIndex = 2;
            cbTickType.SelectedIndex = 1;
            cbDayMinute.SelectedIndex = 0;

            // 이벤트 
            로그인ToolStripMenuItem.Click += Login;
            ost.OnLoged += UI_OnLoged;
            ost.OnReceivedStockItems += UI_OnOnReceivedStockItems;

            dg급거종목.CellClick += UI_OnCellClick;
            dg급거종목.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            txtYmd.TextChanged += UI_Text_Changed;
            cbAvgTerm.SelectedIndexChanged += UI_Cb_Changed;
            cbDayMinute.SelectedIndexChanged += UI_Cb_Changed;
            cbTickType.SelectedIndexChanged += UI_Cb_Changed;

        }

        private void UI_Cb_Changed(object sender, EventArgs e)
        {
            chartRequestData();
        }

        private void UI_Text_Changed(object sender, EventArgs e)
        {
            if (sender.Equals(txtYmd))
            {
                chartRequestData();
            }
        }

        private void UI_OnCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (sender.Equals(dg급거종목))
            {
                int rowIndex = e.RowIndex;
                if(rowIndex >= 0)
                {
                    itemCode = (string)dg급거종목["급거종목_종목코드", rowIndex].Value;
                    string itemName = (string)dg급거종목["급거종목_종목명", rowIndex].Value;
                    lbItemInfo.Text = String.Format("{0} : {1} ", itemName, itemCode);
                    chartRequestData();
                }
            }
        }

        private void chartRequestData()
        {
            if(itemCode.Length == 0)
            {
                MessageBox.Show("종목코드가 존재하지 않습니다.");
                return;
            }
            metFinChart1.DayMinute = ((string)cbDayMinute.SelectedItem).Equals("일봉") ? "D" : "M";
            metFinChart1.Ymd = txtYmd.Text;
            metFinChart1.TickType = (string)cbTickType.SelectedItem;
            metFinChart1.ItemCode = itemCode;
            metFinChart1.MaTerm = (string)cbAvgTerm.SelectedItem;
            metFinChart1.requestChartData();
        }

        private void UI_OnLoged(object sender, EventArgs e)
        {
            if(ost.IsLogin())
                ost.RequestHighTodayVolume("전체");
            else
            {
                MessageBox.Show("로그온 실패");
            }
        }

        private void UI_OnOnReceivedStockItems(object sender, OnReceivedStockItemsEventArgs e)
        {
            // 당일거래량상위요청 :: 전일거래량상위요청
            if (e.EventName.Equals("당일거래량상위요청"))
            {
                List<StockItem> stockList = e.StockItemList;
                foreach (StockItem stock in stockList)
                {
                    int rowIndex = dg급거종목.Rows.Add();
                    dg급거종목["급거종목_종목코드", rowIndex].Value = stock.ItemCode;
                    dg급거종목["급거종목_종목명", rowIndex].Value = stock.ItemName;
                    dg급거종목["급거종목_현재가", rowIndex].Value = String.Format("{0:#,###}", stock.Price);
                    dg급거종목["급거종목_대비", rowIndex].Value = String.Format("{0:#,###}",stock.NetChange);
                    dg급거종목["급거종목_등락률", rowIndex].Value = stock.UpDownRate; // String.Format("{0:0.##}",stock.UpDownRate);
                    dg급거종목["급거종목_거래량", rowIndex].Value = stock.Volume;
                }

            }
        }

        private void Login(object sender, EventArgs e)
        {
 
            ost.Start();
        }
    }
}
