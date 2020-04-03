namespace MetelStockLib.controls
{
    partial class MetFinChart
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.metChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lab_Y_Axis_cur = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.metChart)).BeginInit();
            this.SuspendLayout();
            // 
            // metChart
            // 
            this.metChart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
            chartArea1.AxisX.IsReversed = true;
            chartArea1.Name = "priceChartArea";
            chartArea1.Position.Auto = false;
            chartArea1.Position.Height = 63F;
            chartArea1.Position.Width = 94F;
            chartArea1.Position.X = 3F;
            chartArea1.Position.Y = 3F;
            chartArea2.AlignWithChartArea = "priceChartArea";
            chartArea2.AxisX.IsReversed = true;
            chartArea2.Name = "volumeChartArea";
            chartArea2.Position.Auto = false;
            chartArea2.Position.Height = 30F;
            chartArea2.Position.Width = 94F;
            chartArea2.Position.X = 3F;
            chartArea2.Position.Y = 70F;
            this.metChart.ChartAreas.Add(chartArea1);
            this.metChart.ChartAreas.Add(chartArea2);
            this.metChart.Location = new System.Drawing.Point(32, 26);
            this.metChart.Name = "metChart";
            series1.ChartArea = "priceChartArea";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick;
            series1.Name = "priceSeries";
            series1.YValuesPerPoint = 4;
            series2.ChartArea = "volumeChartArea";
            series2.Name = "volumeSeries";
            series3.BorderWidth = 2;
            series3.ChartArea = "priceChartArea";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series3.Color = System.Drawing.Color.Red;
            series3.Name = "maSeries";
            this.metChart.Series.Add(series1);
            this.metChart.Series.Add(series2);
            this.metChart.Series.Add(series3);
            this.metChart.Size = new System.Drawing.Size(1432, 935);
            this.metChart.TabIndex = 0;
            this.metChart.Text = "chart1";
            // 
            // lab_Y_Axis_cur
            // 
            this.lab_Y_Axis_cur.AutoSize = true;
            this.lab_Y_Axis_cur.ForeColor = System.Drawing.Color.Red;
            this.lab_Y_Axis_cur.Location = new System.Drawing.Point(1407, 356);
            this.lab_Y_Axis_cur.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lab_Y_Axis_cur.Name = "lab_Y_Axis_cur";
            this.lab_Y_Axis_cur.Size = new System.Drawing.Size(0, 24);
            this.lab_Y_Axis_cur.TabIndex = 1;
            // 
            // MetFinChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lab_Y_Axis_cur);
            this.Controls.Add(this.metChart);
            this.Name = "MetFinChart";
            this.Size = new System.Drawing.Size(1490, 994);
            ((System.ComponentModel.ISupportInitialize)(this.metChart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart metChart;
        private System.Windows.Forms.Label lab_Y_Axis_cur;
    }
}
