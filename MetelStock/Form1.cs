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
using MetelStockLib.db;

namespace MetelStock
{
    public partial class Form1 : Form
    {
        OpenSystemTrading ost;
        DBManager dbm;

        private string selectedDgs = "111"; //선택되어진 datagridvew 조합 급거 1 매도전락 1 백테스트 1
        private string itemCode = "";
        private string realItemCode = "";
        public Form1()
        {
            InitializeComponent();
            ost = OpenSystemTrading.GetInstance();
            ost.SetAxKHOpenAPI(axKHOpenAPI1);
            dbm = new DBManager();
            

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
            ost.OnReceivedLogMessage += UI_OnReceivedRealState;
            ost.OnReceivedItemInfo += UI_OnReceivedItemInfo;
            dbm.OnReceivedDBLogMessage += UI_OnReceivedDbLog;
            
            

            dg급거종목.CellClick += UI_OnCellClick;
            dg급거종목.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            txtYmd.TextChanged += UI_Text_Changed;
            cbAvgTerm.SelectedIndexChanged += UI_Cb_Changed;
            cbDayMinute.SelectedIndexChanged += UI_Cb_Changed;
            cbTickType.SelectedIndexChanged += UI_Cb_Changed;
            btnSave.Click += Btn_Click;
            btnReal.Click += Btn_Click;
            btnSearch.Click += Btn_Click;
            btn검색사용자관심.Click += Btn_Click;
            btnReal.Click += Btn_Click;

        }

        private void UI_OnReceivedItemInfo(object sender, OnReceivedItemInfoEventArgs e)
        {
            realItemCode = e.ItemCode;
            writeRealState("request item info : " + realItemCode);
        }

        private void UI_OnReceivedRealState(object sender, OnReceivedLogMessageEventArgs e)
        {
            writeRealState(e.Message);
        }

        private void writeRealState(string str)
        {
            lboxRealState.Items.Add(str);
            lboxRealState.SelectedIndex = lboxRealState.Items.Count - 1;
        }

        private void UI_OnReceivedDbLog(object sender, OnReceivedLogMessageEventArgs e)
        {
            writeBackTest(e.Message);
        }

        private void writeBackTest(string str)
        {
            lboxBackTest.Items.Add(str);
            lboxBackTest.SelectedIndex = lboxBackTest.Items.Count - 1;
        }

        private void sendLogToBackTest(string msg)
        {
            writeBackTest(msg);
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            if (sender.Equals(btnSave))
            {
                if (selectedDgs.Substring(0,1).Equals("1"))
                {
                    //종목코드, 종목명, 현재가, 대비, 등락률, 거래량
                    DataGridViewRow dr = dg급거종목.SelectedRows[0];
                    StockItem stock = new StockItem();
                    stock.ItemCode = dr.Cells[0].Value.ToString().Trim();
                    stock.ItemName = dr.Cells[1].Value.ToString().Trim();
                    stock.Price = Math.Abs(long.Parse(formatOnlyNumber(dr.Cells[2].Value)));
                    stock.NetChange = long.Parse(formatOnlyNumber(dr.Cells[3].Value));
                    stock.UpDownRate = double.Parse(formatOnlyNumber(dr.Cells[4].Value));
                    stock.Volume = long.Parse(formatOnlyNumber(dr.Cells[5].Value));
                    dbm.insert_대상항목(stock);
                }
            } else if (sender.Equals(btnReal))
            {
                // 실시간 테스트 
                ost.RequestRealReg();
                writeRealState("실시간 테스트 시작 ");


            } else if (sender.Equals(btnSearch))
            {
                ost.RequestHighTodayVolume("전체");
            } else if (sender.Equals(btn검색사용자관심))
            {
                string searchItemCode = txtSearchItemCode.Text;
                if (searchItemCode.Length == 0)
                {
                    MessageBox.Show("종목코드가 없습니다.");
                    return;
                }
                ost.RequestItemInfo(searchItemCode);
            }

        }

        private string formatOnlyNumber(object str)
        {
            if (str == null) return "";
            string result = "";
            if(str.ToString().Length > 0)
            {
                result = str.ToString().Replace(",", "");
            }
            return result;
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
            if (ost.IsLogin())
            {

            }               
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
