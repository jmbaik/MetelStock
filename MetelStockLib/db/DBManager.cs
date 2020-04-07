using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetelStockLib.core;
using Oracle.ManagedDataAccess.Client;

namespace MetelStockLib.db
{
    public class DBManager
    {
        public event EventHandler<OnReceivedLogMessageEventArgs> OnReceivedDBLogMessage; //로그 수신 시

        //private const string conn_str = "Data Source=METSTOCK;User Id=metstock;Password=man100;";
        private const string conn_str = "User Id=metstock; Password=man100; Data Source=(DESCRIPTION =   (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.222.6)(PORT = 1521))   (CONNECT_DATA =     (SERVER = DEDICATED)     (SID = meteldb)   ) );";
        OracleConnection conn = null;

        private void connect()
        {
            if(conn == null)
                conn = new OracleConnection(conn_str);

            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("db 접속 실패 : " + ex.Message);
                conn = null;
            }
        }

        private void SendLogMessage(string logMessage) //Event를 이용해 로그 메세지 전달
        {
            logMessage = DateTime.Now.ToString("[HH:mm:ss] ") + logMessage;
            OnReceivedDBLogMessage?.Invoke(this, new OnReceivedLogMessageEventArgs(logMessage));
        }

        private int executeQuery(string query)
        {
            // 성공 1 실패 -1
            int result = 1;
            if (conn == null) connect();
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                result = -1;
                Console.WriteLine(query);
                Console.WriteLine("에러:"+ex.Message);
                SendLogMessage(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            SendLogMessage("Success ::: " + query);
            return result;
        }

        private int countQuery(string query)
        {
            int result = -1;
            using(OracleConnection connection = new OracleConnection(conn_str))
            {
                connection.Open();
                OracleCommand cmd = new OracleCommand(query, connection);
                result = int.Parse(cmd.ExecuteScalar().ToString());
            }
            return result;
        }

        public int insert_대상항목(StockItem stockItem)
        {
            int result = 0;
            string ymd = DateTime.Now.ToString("yyyyMMdd");
            string countQry = $@"
                   SELECT COUNT(0) FROM MF_TARGET_ITEM WHERE YMD='{ymd}' and ITEM_CD ='{stockItem.ItemCode}'             
                                ";
            int cnt = countQuery(countQry);
            if(cnt == 0)
            {
                string query = $@"
                        INSERT INTO METSTOCK.MF_TARGET_ITEM(
                           YMD, ITEM_CD, ITEM_NM,
                           PRC, NET_CHG, UD_RT,
                           VOL, REG_ID, REG_DT)
                        VALUES( '{ymd}',
                         '{stockItem.ItemCode}',
                         '{stockItem.ItemName}',
                         {stockItem.Price},
                         {stockItem.NetChange},
                         {stockItem.UpDownRate},
                         {stockItem.Volume},
                         'sys',
                         sysdate)";
                result = executeQuery(query);
            }

            return result;
        }

        public int insertActH(string rqName, string trName, string scrNo, string evtName, string param)
        {
            string ymd = DateTime.Now.ToString("yyyyMMdd");
            string qry = $@"
                    INSERT INTO METSTOCK.MF_ACT_H (
                       YMD, EVTSEQ, RQNAME, 
                       TRNAME, SCRNO, EVTNAME, 
                       PARAMS, REG_ID, REG_DT) 
                    VALUES ('{ymd}',
                     SEQ_MF_ACT_H.NEXTVAL,
                        '{rqName}',
                        '{trName}',
                        '{scrNo}',
                     '{evtName}',
                     '{param}',
                     'sys',
                     sysdate)";
            int result = executeQuery(qry);
            return result;
        }

        private string mergeChartDatQuery(string dmType, string itemCode, string itemName, ChartData chart, Dictionary<string,long> ma)
        {
            string tableName = dmType.Equals("M") ? "MF_BUN_PRC" : "MF_DAY_PRC";
            string keyName = dmType.Equals("M") ? "YMDHM" : "YMD";
            string qry = $@"
                        MERGE INTO {tableName} d
                        USING (
                          Select
                            '{chart.Time}' as {keyName},
                            '{itemCode}' as ITEM_CD,
                            '{itemName}' as ITEM_NM,
                            {chart.ClosePrice} as PRC,
                            {chart.OpenPrice} as S_PRC,
                            {chart.HighPrice} as H_PRC,
                            {chart.LowPrice} as L_PRC,
                            {chart.Volume} as VOL,
                            {ma["ma5"]} as MA5,
                            {ma["ma10"]} as MA10,
                            {ma["ma20"]} as MA20,
                            {ma["ma60"]} as MA60,
                            {ma["ma120"]} as MA120,
                            {ma["ma240"]} as MA240,
                            'sys' as REG_ID,
                            sysdate as REG_DT
                          From Dual) s
                        ON
                          (d.YMDHM = s.YMDHM and 
                          d.ITEM_CD = s.ITEM_CD )
                        WHEN MATCHED
                        THEN
                        UPDATE SET
                          d.ITEM_NM = s.ITEM_NM,
                          d.PRC = s.PRC,
                          d.S_PRC = s.S_PRC,
                          d.H_PRC = s.H_PRC,
                          d.L_PRC = s.L_PRC,
                          d.VOL = s.VOL,
                          d.MA5 = s.MA5,
                          d.MA10 = s.MA10,
                          d.MA20 = s.MA20,
                          d.MA60 = s.MA60,
                          d.MA120 = s.MA120,
                          d.MA240 = s.MA240,
                          d.UPD_ID = 'sys',
                          d.UPD_DT = sysdate
                        WHEN NOT MATCHED
                        THEN
                        INSERT (
                          YMDHM, ITEM_CD, ITEM_NM,
                          PRC, S_PRC, H_PRC,
                          L_PRC, VOL, MA5,
                          MA10, MA20, MA60,
                          MA120, MA240, REG_ID,
                          REG_DT)
                        VALUES (
                          s.YMDHM, s.ITEM_CD, s.ITEM_NM,
                          s.PRC, s.S_PRC, s.H_PRC,
                          s.L_PRC, s.VOL, s.MA5,
                          s.MA10, s.MA20, s.MA60,
                          s.MA120, s.MA240, s.REG_ID,
                          s.REG_DT) ";
            return qry;
        }
        private double maverage(double[] c_arr, int curIndex, int cha)
        {
            double ret = 0, sum=0;
            if(curIndex > cha-1)
            {
                for (int i = 0; i < cha; i++)
                {
                    sum += c_arr[curIndex - i];
                }
                ret = sum / cha;
            }
            return ret;
        }
        public void mergeDMPrc(string dmGubun, string itemCode, string itemName, List<ChartData> chartDataList)
        {
            int cnt = chartDataList.Count;
            double[] c_arr = new double[cnt];
            for (int i = 0; i < cnt; i++)
            {
                c_arr[i] = chartDataList[i].ClosePrice;
            }

            using (OracleConnection connection = new OracleConnection(conn_str))
            {
                connection.Open();
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = connection;            
                for (int i = cnt-1 ; i >=0; i--)
                {
                    ChartData chart = chartDataList[i];
                    double dma20=0, dma5=0, dma10=0, dma60=0,dma120=0,dma240=0;
 
                    Dictionary<string, long> ma = new Dictionary<string, long>();
                    ma["ma20"] = Convert.ToInt64(maverage(c_arr, i, 20));
                    ma["ma5"] = Convert.ToInt64(maverage(c_arr, i, 5));
                    ma["ma10"] = Convert.ToInt64(maverage(c_arr, i, 10));
                    ma["ma60"] = Convert.ToInt64(maverage(c_arr, i, 60));
                    ma["ma120"] = Convert.ToInt64(maverage(c_arr, i, 120));
                    ma["ma240"] = Convert.ToInt64(maverage(c_arr, i, 240));
                    string q = mergeChartDatQuery(dmGubun, itemCode, itemName, chart, ma);
                    cmd.CommandText = q;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            };
        }


        public void mergeMfBunPrc(string itemCode, string itemName, ChartData chart)
        {
            using (OracleConnection connection = new OracleConnection(conn_str))
            {
                connection.Open();
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = connection;
                Dictionary<string, long> ma = new Dictionary<string, long>();
                ma["ma20"] = 0;
                ma["ma5"] =  0;
                ma["ma10"] = 0;
                ma["ma60"] = 0;
                ma["ma120"] =0;
                ma["ma240"] = 0;
                string q = mergeChartDatQuery("M", itemCode, itemName, chart, ma);
                cmd.CommandText = q;
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            };
        }
    }

}
