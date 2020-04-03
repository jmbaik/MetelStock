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
using System.Windows.Forms.DataVisualization.Charting;

namespace MetelStockLib.controls
{
    public partial class MetFinChart : UserControl
    {
        /*** property setting **************************************/
        // public delegate void chartDataGet_dele(object sender, string args);
        // public event chartDataGet_dele OnEventMFChart;
        public void requestChartData()
        {
            if (itemCode.Length == 0)
            {
                MessageBox.Show("종목코드가 없습니다");
                return;
            }
            if (dayminute.Equals("D"))
            {
                if (ymd.Length == 0)
                {
                    MessageBox.Show("일자가 없습니다");
                    return;
                }
            }
            if(dayminute.Equals("D"))
                ost.RequestDayChartData(ItemCode, ymd);
            if(dayminute.Equals("M"))
                ost.RequestMinuteChartData(ItemCode, tickType);
        }

        private string itemCode="", ymd="", dayminute="M", tickType="3:3분", maTerm="20";
        [Category("UserProperty"), Description("종목코드")]
        public string ItemCode
        {
            get
            {
                return itemCode;
            }
            set
            {
                this.itemCode = value;
            }
        }
        [Category("UserProperty"), Description("일자")]
        public string Ymd
        {
            get
            {
                return ymd;
            }
            set
            {
                this.ymd = value;
            }
        }
        [Category("UserProperty"), Description("일봉분봉구분")]
        public string DayMinute
        {
            get
            {
                return dayminute;
            }
            set
            {
                this.dayminute = value;
            }
        }
        [Category("UserProperty"), Description("틱구분")]
        public string TickType
        {
            get
            {
                return tickType;
            }
            set
            {
                this.tickType = value;
            }
        }
        [Category("UserProperty"), Description("이평선")]
        public string MaTerm
        {
            get
            {
                return maTerm;
            }
            set
            {
                this.maTerm = value;
            }
        }
        //***********************************************/
        OpenSystemTrading ost; 

        Series priceSeries;
        Series volumeSeries;
        Series maSeries; //Moving Average

        ChartArea priceChartArea;
        ChartArea volumeChartArea;

        int priceChartMinX, priceChartMinY, priceChartMaxX, priceChartMaxY;

        public MetFinChart()
        {
            InitializeComponent();

            ost = OpenSystemTrading.GetInstance();
            ost.OnReceivedChartData += OST_OnReceivedChartData;

            priceSeries = metChart.Series["priceSeries"];
            volumeSeries = metChart.Series["volumeSeries"];
            maSeries = metChart.Series["maSeries"];

            priceChartArea = metChart.ChartAreas["priceChartArea"];
            volumeChartArea = metChart.ChartAreas["volumeChartArea"];

            priceSeries["priceUpColor"] = "Red";
            priceSeries["PriceDownColor"] = "Blue";

            metChart.MouseMove += Chart_MouseMove;

        }
        private void GetPriceChartRange()
        {
            if (priceChartArea != null)
            {
                priceChartMinX = (int)priceChartArea.Position.X;
                priceChartMaxX = (int)(priceChartArea.Position.X + priceChartArea.Position.Width * metChart.Width / 100);
                priceChartMinY = (int)priceChartArea.Position.Y;
                priceChartMaxY = (int)(priceChartArea.Position.Y + priceChartArea.Position.Height * metChart.Height / 100);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            GetPriceChartRange();
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GetPriceChartRange();
        }

        private void Chart_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender.Equals(metChart))
            {
                Point mousePoint = new Point(e.X, e.Y);
                priceChartArea.CursorX.SetCursorPixelPosition(mousePoint, true);
                priceChartArea.CursorY.SetCursorPixelPosition(mousePoint, true);

                try
                {
                    if (mousePoint.X >= priceChartMinX && mousePoint.X <= priceChartMaxX && mousePoint.Y >= priceChartMinY && mousePoint.Y <= priceChartMaxY)
                    {
                        if (priceChartArea.AxisY != null)
                        {
                            double value = priceChartArea.AxisY.PixelPositionToValue(e.Y);
                            if (value <= priceChartArea.AxisY.Maximum && value >= priceChartArea.AxisY.Minimum)
                            {
                                lab_Y_Axis_cur.Visible = true;
                                lab_Y_Axis_cur.Location = new Point(20, e.Y);
                                lab_Y_Axis_cur.Text = string.Format("{0:#,###}", value);
                            }
                            else
                            {
                                lab_Y_Axis_cur.Visible = false;
                            }
                        }
                    }
                    else
                    {
                        lab_Y_Axis_cur.Visible = false;
                    }
                }
                catch (Exception)
                {

                }

            }
        }

        private void OST_OnReceivedChartData(object sender, OnReceivedChartDataEventArgs e)
        {
            try
            {
                List<ChartData> ChartDataList = e.ChartDataList;
                ChartDataList.Reverse();
                priceSeries.Points.Clear();
                volumeSeries.Points.Clear();

                foreach (ChartData data in ChartDataList)
                {
                    int index = priceSeries.Points.AddXY(data.Time, data.HighPrice);
                    priceSeries.Points[index].YValues[1] = data.LowPrice;
                    priceSeries.Points[index].YValues[2] = data.OpenPrice;
                    priceSeries.Points[index].YValues[3] = data.ClosePrice;
                    volumeSeries.Points.AddXY(data.Time, data.Volume);

                    priceSeries.Points[index].ToolTip = "일자 : " + data.Time + "\n"
                                                         + "시가 : " + String.Format("{0:#,###}", data.OpenPrice) + "\n"
                                                         + "고가 : " + String.Format("{0:#,###}", data.HighPrice) + "\n"
                                                         + "저가 : " + String.Format("{0:#,###}", data.LowPrice) + "\n"
                                                         + "종가 : " + String.Format("{0:#,###}", data.ClosePrice) + "\n"
                                                         + "거래량 : " + String.Format("{0:#,###}", data.Volume);

                    if (data.OpenPrice < data.ClosePrice) //시가대비 종가 상승
                        priceSeries.Points[index].Color = Color.Red;
                    else
                        priceSeries.Points[index].Color = Color.Blue;

                }

                metChart.DataManipulator.FinancialFormula(FinancialFormula.MovingAverage, "20", "priceSeries:Y4", "maSeries:Y");
                metChart.DataManipulator.FinancialFormula(FinancialFormula.MovingAverage, "60", "priceSeries:Y4", "maSeries60:Y");
                metChart.DataManipulator.FinancialFormula(FinancialFormula.MovingAverage, "120", "priceSeries:Y4", "maSeries120:Y");
                metChart.DataManipulator.FinancialFormula(FinancialFormula.MovingAverage, "240", "priceSeries:Y4", "maSeries240:Y");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

    }
}
