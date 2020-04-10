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
            // 정배열 0인덱스가 가장 이른 시간인 경우 
            /*
            if(curIndex > cha-1)
            {
                for (int i = 0; i < cha; i++)
                {
                    sum += c_arr[curIndex - i];
                }
                ret = sum / cha;
            }
            */
            // 역배열 0인덱스가 가장 최신 시간인 경우 
            int totalCount = c_arr.Length;
            if(curIndex <= totalCount - cha)
            {
                for (int i = 0; i < cha; i++)
                {
                    sum += c_arr[curIndex + i];
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
                for (int i = 0 ; i < cnt; i++)
                {
                    ChartData chart = chartDataList[i];
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

        /*************************************************************************************************/
        /*** 자동매매 Auto Trading Part *******************************************************************/  

        public List<StockItem> getMfAtItem()
        {
            List<StockItem> stockList = new List<StockItem>();
            using (OracleConnection connection = new OracleConnection(conn_str))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = "SELECT ITEM_CD, ITEM_NM FROM MF_AT_ITEM WHERE REAL_YN='Y'";
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            StockItem stock = new StockItem();
                            stock.ItemCode = reader["ITEM_CD"].ToString();
                            stock.ItemName = reader["ITEM_NM"].ToString();
                            stockList.Add(stock);
                        }
                    }
                }
            }
            return stockList;
        }
        
        private string queryMergeAT3M(string itemCode, ChartData chart, Dictionary<string, long> dic)
        {
            long bvol = dic["bvol"];
            string query = $@"
                        MERGE INTO MF_AT_3M d
                        USING (
                          Select
                            '{chart.Time}' as YMDHM,
                            '{itemCode}' as ITEM_CD,
                            {chart.ClosePrice} as PRC,
                            {chart.OpenPrice} as S_PRC,
                            {chart.HighPrice} as H_PRC,
                            {chart.LowPrice} as L_PRC,
                            {chart.Volume} as VOL,
                            {bvol} as BVOL,
                            {dic["ma5"]} as MA5,
                            {dic["bma5"]} as BMA5,
                            {dic["ma10"]} as MA10,
                            {dic["bma10"]} as BMA10,
                            {dic["ma20"]} as MA20,
                            {dic["bma20"]} as BMA20,
                            {dic["ma60"]} as MA60,
                            {dic["ma120"]} as MA120
                          From Dual) s
                        ON
                          (d.YMDHM = s.YMDHM and 
                          d.ITEM_CD = s.ITEM_CD )
                        WHEN MATCHED
                        THEN
                        UPDATE SET
                          d.PRC = s.PRC,
                          d.S_PRC = s.S_PRC,
                          d.H_PRC = s.H_PRC,
                          d.L_PRC = s.L_PRC,
                          d.VOL = s.VOL,
                          d.BVOL = s.BVOL,
                          d.VOL_BRT = DECODE(s.BVOL,0,0,s.VOL/s.BVOL),
                          d.MA5 = s.MA5,
                          d.BMA5 = s.BMA5,
                          d.MA10 = s.MA10,
                          d.BMA10 = s.BMA10,
                          d.MA20 = s.MA20,
                          d.BMA20 = s.BMA20,
                          d.MA60 = s.MA60,
                          d.MA120 = s.MA120,
                          d.UPD_ID = 'auto',
                          d.UPD_DT = sysdate
                        WHEN NOT MATCHED
                        THEN
                        INSERT (
                          YMDHM, ITEM_CD, PRC,
                          S_PRC, H_PRC, L_PRC,
                          VOL, BVOL, VOL_BRT,
                          MA5, BMA5, MA10, BMA10, MA20, BMA20,
                          MA60, MA120, REG_ID, REG_DT)
                        VALUES (
                          s.YMDHM, s.ITEM_CD, s.PRC,
                          s.S_PRC, s.H_PRC, s.L_PRC,
                          s.VOL, s.BVOL, decode(s.BVOL,0,0,s.VOL/s.BVOL),
                          s.MA5, s.BMA5, s.MA10, s.BMA10, s.MA20, s.BMA20,
                          s.MA60, s.MA120, 'auto', sysdate)
                    ";
            return query;
        }
        public void mergeMfAt3M(string itemCode, List<ChartData> chartDataList)
        {
            int cnt = chartDataList.Count;
            double[] c_arr = new double[cnt];
            // 최신데이터부터 0인덱스 순으로 받는다. 
            for (int i = 0; i < cnt; i++)
            {
                c_arr[i] = chartDataList[i].ClosePrice;
            }

            using (OracleConnection connection = new OracleConnection(conn_str))
            {
                connection.Open();
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = connection;
                for (int i = 0; i< cnt; i++)
                {
                    ChartData chart = chartDataList[i];
                    Dictionary<string, long> ma = new Dictionary<string, long>();
                    ma["bvol"] = i == (cnt - 1) ? 0 : chartDataList[i + 1].Volume;
                    ma["bma5"] = i == (cnt - 1) ? 0 : Convert.ToInt64(maverage(c_arr, i+1, 5));
                    ma["bma10"] = i == (cnt - 1) ? 0 : Convert.ToInt64(maverage(c_arr, i + 1, 10));
                    ma["bma20"] = i == (cnt - 1) ? 0 : Convert.ToInt64(maverage(c_arr, i + 1, 20));
                    ma["ma5"] = Convert.ToInt64(maverage(c_arr, i, 5));
                    ma["ma10"] = Convert.ToInt64(maverage(c_arr, i, 10));
                    ma["ma20"] = Convert.ToInt64(maverage(c_arr, i, 20));
                    ma["ma60"] = Convert.ToInt64(maverage(c_arr, i, 60));
                    ma["ma120"] = Convert.ToInt64(maverage(c_arr, i, 120));
                    ma["ma240"] = Convert.ToInt64(maverage(c_arr, i, 240));
                    string q = queryMergeAT3M(itemCode, chart, ma);
                    cmd.CommandText = q;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            };
        }
    }

}
