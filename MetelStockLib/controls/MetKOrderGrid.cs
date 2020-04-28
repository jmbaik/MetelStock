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
    public partial class MetKOrderGrid : UserControl
    {
        OpenSystemTrading ost;

        enum GridType
        {
            주문, 체결, 미체결, 잔고
        }

        public MetKOrderGrid()
        {
            InitializeComponent();
            
            ost = OpenSystemTrading.GetInstance();
            ost.OnReceivedAutoTradingOrderAccept += OST_OnReceivedAutoTradingOrderAccept;
            ost.OnReceivedAutoTradingOrderBalance += OST_OnReceivedAutoTradingOrderBalance;
            ost.OnReceivedAutoTradingOrderConclusion += OST_OnReceivedAutoTradingOrderConclusion;
        }

        private void OST_OnReceivedAutoTradingOrderConclusion(object sender, OnReceivedATOrderEventArgs e)
        {
            // 오더 체결
            ATOrder order = e.AtOrder;
            SetDataGrid_체결(order);
            
        }

        private void OST_OnReceivedAutoTradingOrderBalance(object sender, OnReceivedATOrderEventArgs e)
        {
            // 오더 잔고
            ATOrder order = e.AtOrder;
            SetDataGrid_잔고(order);            
        }

        private void OST_OnReceivedAutoTradingOrderAccept(object sender, OnReceivedATOrderEventArgs e)
        {
            // 오더 접수
            ATOrder order = e.AtOrder;
            // order.주문구분
            AddDataGrid_주문(order);
            AddDataGrid_미체결(order);
        }

        public void AddDataGrid_주문(ATOrder order)
        {
            int rowIndex = dg주문.Rows.Add();
            dg주문["주문_주문번호", rowIndex].Value = order.주문번호;
            dg주문["주문_계좌번호", rowIndex].Value = order.계좌번호;
            dg주문["주문_시간", rowIndex].Value = order.주문시간;
            dg주문["주문_종목코드", rowIndex].Value = order.종목코드;
            dg주문["주문_종목명", rowIndex].Value = order.종목명;
            dg주문["주문_주문량", rowIndex].Value = order.주문수량;
            dg주문["주문_주문가", rowIndex].Value = order.주문가격;
            dg주문["주문_매매구분", rowIndex].Value = order.주문구분;
            dg주문["주문_가격구분", rowIndex].Value = order.가격구분;
        }
        public void SetDataGrid_체결(ATOrder order)
        {
            int cnt = updateDataGridRow(order, dg미체결);
            if(cnt == 0)
            {
                int rowIndex = dg체결.Rows.Add();
                dg체결["체결_주문번호", rowIndex].Value = order.주문번호;
                dg체결["체결_체결시간", rowIndex].Value = order.체결시간;
                dg체결["체결_종목코드", rowIndex].Value = order.종목코드;
                dg체결["체결_종목명", rowIndex].Value = order.종목명;
                dg체결["체결_주문량", rowIndex].Value = order.주문수량;
                dg체결["체결_단위체결량", rowIndex].Value = order.단위체결량;
                dg체결["체결_누적체결량", rowIndex].Value = order.누적체결량;
                dg체결["체결_체결가", rowIndex].Value = order.체결가;
                dg체결["체결_매매구분", rowIndex].Value = order.매매구분;
            }
        }
        public void AddDataGrid_미체결(ATOrder order)
        {
            int idx = dg미체결.Rows.Add();
            dg미체결["미체결_주문번호", idx].Value = order.주문번호;
            dg미체결["미체결_종목코드", idx].Value = order.종목코드;
            dg미체결["미체결_종목명", idx].Value = order.종목명;
            dg미체결["미체결_주문수량", idx].Value = order.주문수량;
            dg미체결["미체결_미체결량", idx].Value = order.미체결수량;
        }
        public void SetDataGrid_잔고(ATOrder order)
        {
            int cnt = updateDataGridRow(order, dg잔고);
            if (cnt == 0)
            {
                int rowIndex = dg잔고.Rows.Add();
                dg잔고["잔고_계좌번호", rowIndex].Value = order.계좌번호;
                dg잔고["잔고_종목코드", rowIndex].Value = order.종목코드;
                dg잔고["잔고_종목명", rowIndex].Value = order.종목명;
                dg잔고["잔고_보유수량", rowIndex].Value = order.보유수량;
                dg잔고["잔고_주문가능수량", rowIndex].Value = order.주문가능수량;
                dg잔고["잔고_매입단가", rowIndex].Value = order.매입단가;
                dg잔고["잔고_총매입가", rowIndex].Value = order.총매입가;
                dg잔고["잔고_손익률", rowIndex].Value = order.손익률;
                dg잔고["잔고_매매구분", rowIndex].Value = order.매매구분;
            }    
        }
        private void removeDataGridRow(GridType _gridType, ATOrder order)
        {
            DataGridView dg=null;
            if(_gridType == GridType.미체결)
            {
                dg = dg미체결;
                if(order.미체결수량 == 0)
                {
                    foreach (DataGridViewRow row in dg.Rows)
                    {
                        if(row.Cells["미체결_주문번호"] != null && row.Cells["미체결_주문번호"].Value.ToString().Equals(order.주문번호))
                        {
                            dg.Rows.Remove(row);
                        }
                        break;
                    }
                }
            }           
        }

        private int updateDataGridRow(ATOrder order, DataGridView dg)
        {
            int result = 0;

            foreach (DataGridViewRow row in dg.Rows)
            {
                if(dg.Equals(dg잔고) && equalClauseInDataGridRow("잔고_종목코드", order.종목코드, row))
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
                if (dg.Equals(dg미체결) && equalClauseInDataGridRow("미체결_주문번호", order.주문번호, row))
                {
                    if (order.미체결수량 > 0)
                    {
                        row.Cells["미체결_미체결량"].Value = order.미체결수량;
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

        private bool equalClauseInDataGridRow(string cname, string val, DataGridViewRow row)
        {
            bool result = false;
            if(row.Cells[cname].Value != null && row.Cells[cname].Value.ToString().Equals(val))
            {
                result = true;
            }
            return result;
        }
        
    }
}
