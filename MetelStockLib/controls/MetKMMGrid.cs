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

namespace MetelStockLib.controls
{
    public partial class MetKMMGrid : UserControl
    {
        OpenSystemTrading ost;
        public MetKMMGrid()
        {
            InitializeComponent();

            ost = OpenSystemTrading.GetInstance();
            ost.OnReceivedAutoTradingOrderAccept += OST_OnReceivedAutoTradingOrderAccept;
            ost.OnReceivedAutoTradingOrderBalance += OST_OnReceivedAutoTradingOrderBalance;
            ost.OnReceivedAutoTradingOrderConclusion += OST_OnReceivedAutoTradingOrderConclusion;
        }

        private void OST_OnReceivedAutoTradingOrderConclusion(object sender, OnReceivedATOrderEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OST_OnReceivedAutoTradingOrderBalance(object sender, OnReceivedATOrderEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OST_OnReceivedAutoTradingOrderAccept(object sender, OnReceivedATOrderEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void SetDataGrid_매매진행현황(ATOrder order)
        {
            int cnt = updateDataGridRow(order, dg매매진행현황);
            if (cnt == 0)
            {
                int rowIndex = dg매매진행현황.Rows.Add();
                dg매매진행현황["매매진행_진행상황", rowIndex].Value = "매도감시";
                dg매매진행현황["매매진행_종목코드", rowIndex].Value = order.종목코드;
                dg매매진행현황["매매진행_종목명", rowIndex].Value = order.종목명;
                dg매매진행현황["매매진행_매수가", rowIndex].Value = order.매수가;
            }
        }

        private int updateDataGridRow(ATOrder order, DataGridView dg)
        {
            int result = 0;

            foreach (DataGridViewRow row in dg.Rows)
            {
                if (dg.Equals(dg매매진행현황) && Common.equalClauseInDataGridRow("잔고_종목코드", order.종목코드, row))
                {
                    if (order.보유수량 > 0)
                    {
                        row.Cells["잔고_보유수량"].Value = order.보유수량;
                        row.Cells["잔고_주문가능수량"].Value = order.주문가능수량;
                        row.Cells["잔고_매입단가"].Value = order.매입단가;
                        row.Cells["잔고_총매입가"].Value = order.총매입가;
                        row.Cells["잔고_손익률"].Value = order.손익률;
                    }
                    else
                    {
                        dg.Rows.Remove(row);
                    }
                    result++;
                }
            }
            return result;
        }

    }
}
