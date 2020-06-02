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

        public List<StockItem> getMfAtItem(string ymd)
        {
            List<StockItem> stockList = new List<StockItem>();
            using (OracleConnection connection = new OracleConnection(conn_str))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand())
                {
                    string q = $@"
                            SELECT MM.ITEM_CD, MM.ITEM_NM FROM (
                                SELECT ITEM_CD, ITEM_NM FROM MF_AT_ITEM WHERE REAL_YN='Y' AND YMD='{ymd}'
                                UNION ALL
                                SELECT A.ITEM_CD, B.ITEM_NM FROM MF_AT_MST A, MF_AT_ITEM B WHERE A.ITEM_CD=B.ITEM_CD 
                                AND A.YMD=B.YMD AND NVL(A.DONE,'N')='N' AND B.YMD='{ymd}'
                            ) MM GROUP BY MM.ITEM_CD, MM.ITEM_NM
                                ";
                    cmd.Connection = connection;
                    cmd.CommandText = q;
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

        public void mergeListMfAtItem(string ymd, List<StockItem> stockList)
        {
            int cnt = stockList.Count;
            using (OracleConnection connection = new OracleConnection(conn_str))
            {
                connection.Open();
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = connection;
                for (int i = 0; i < cnt; i++)
                {
                    StockItem stock = stockList[i];
                    string realYn = stock.NetChange < 0 ? "N" : "Y";
                    string q = $@"
                        MERGE INTO METSTOCK.MF_AT_ITEM d
                        USING (
                          Select
                            '{ymd}' as YMD,
                            '{stock.ItemCode}' as ITEM_CD,
                            '{stock.ItemName}' as ITEM_NM,
                            {stock.NetChange} as NET,
                            {stock.UpDownRate} as UD_RT,
                            {stock.Volume} as VOL,
                            {stock.TrAmount} as TR_AMT,
                            {stock.Bvolume} as BVOL,
                            {stock.Rnk} as RNK,
                            {stock.Brnk} as BRNK,
                            '{realYn}' as REAL_YN
                          From DUAL) s
                        ON
                          (d.YMD = s.YMD and 
                          d.ITEM_CD = s.ITEM_CD )
                        WHEN MATCHED
                        THEN
                        UPDATE SET
                          d.ITEM_NM = s.ITEM_NM,
                          d.NET = s.NET,
                          d.UD_RT = s.UD_RT,
                          d.VOL = s.VOL,
                          d.TR_AMT = s.TR_AMT,
                          d.BVOL = s.BVOL,
                          d.RNK = s.RNK,
                          d.BRNK = s.BRNK,
                          d.REAL_YN = s.REAL_YN,
                          d.UPD_ID = 'auto',
                          d.UPD_DT = sysdate
                        WHEN NOT MATCHED
                        THEN
                        INSERT (
                          YMD, ITEM_CD, ITEM_NM,
                          NET, UD_RT, VOL,
                          TR_AMT, BVOL, RNK,
                          BRNK, REAL_YN,
                          REG_ID, REG_DT)
                        VALUES (
                          s.YMD, s.ITEM_CD, s.ITEM_NM,
                          s.NET, s.UD_RT, s.VOL,
                          s.TR_AMT, s.BVOL, s.RNK,
                          s.BRNK, s.REAL_YN,
                          'auto', sysdate)
                    ";
                    cmd.CommandText = q;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            };
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
                            {dic["ma120"]} as MA120,
                            {dic["bprc"]} as BPRC,
                            {dic["s9_prc"]} as S9_PRC
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
                          d.VOL_BRT = ROUND(DECODE(s.BVOL,0,0,s.VOL/s.BVOL),2),
                          d.MA5 = s.MA5,
                          d.BMA5 = s.BMA5,
                          d.MA10 = s.MA10,
                          d.BMA10 = s.BMA10,
                          d.MA20 = s.MA20,
                          d.BMA20 = s.BMA20,
                          d.MA60 = s.MA60,
                          d.MA120 = s.MA120,
                          d.BPRC = s.BPRC,
                          d.S9_PRC = s.S9_PRC,
                          d.UPD_ID = 'auto',
                          d.UPD_DT = sysdate
                        WHEN NOT MATCHED
                        THEN
                        INSERT (
                          YMDHM, ITEM_CD, PRC,
                          S_PRC, H_PRC, L_PRC,
                          VOL, BVOL, VOL_BRT,
                          MA5, BMA5, MA10, BMA10, MA20, BMA20,
                          MA60, MA120, BPRC, S9_PRC, REG_ID, REG_DT)
                        VALUES (
                          s.YMDHM, s.ITEM_CD, s.PRC,
                          s.S_PRC, s.H_PRC, s.L_PRC,
                          s.VOL, s.BVOL, ROUND(decode(s.BVOL,0,0,s.VOL/s.BVOL),2),
                          s.MA5, s.BMA5, s.MA10, s.BMA10, s.MA20, s.BMA20,
                          s.MA60, s.MA120, s.BPRC, s.S9_PRC, 'auto', sysdate)
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
                long s9_prc = 0;
                for (int i = cnt-1; i>= 0; i--)
                {
                    ChartData chart = chartDataList[i];
                    Dictionary<string, long> ma = new Dictionary<string, long>();
                    ma["bvol"] = i == (cnt - 1) ? 0 : chartDataList[i + 1].Volume;
                    ma["bma5"] = i == (cnt - 1) ? 0 : Convert.ToInt64(maverage(c_arr, i + 1, 5));
                    ma["bma10"] = i == (cnt - 1) ? 0 : Convert.ToInt64(maverage(c_arr, i + 1, 10));
                    ma["bma20"] = i == (cnt - 1) ? 0 : Convert.ToInt64(maverage(c_arr, i + 1, 20));
                    ma["bprc"] = i == (cnt - 1) ? 0 : Convert.ToInt64(c_arr[i + 1]);
                    if (chart.Time.Substring(8, 6).Equals("090000"))
                    {
                        s9_prc = Convert.ToInt64(chart.ClosePrice);
                    }
                    ma["s9_prc"] = s9_prc;
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

        public void moveHistoryMfAtItem()
        {
            using (OracleConnection connection = new OracleConnection(conn_str))
            {
                connection.Open();
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = connection;
                string q = $@"INSERT INTO MF_AT_ITEM_H SELECT * FROM MF_AT_ITEM ORDER BY YMD, ITEM_CD";
                cmd.CommandText = q;
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();

                string deleteQuery = $@"DELETE FROM MF_AT_ITEM";
                cmd.CommandText = deleteQuery;
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            };
        }

        public List<ATOrder> getMstForOrder(string ymd, string itemCode)
        {
            List<ATOrder> orderList = new List<ATOrder>();
            using (OracleConnection connection = new OracleConnection(conn_str))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand())
                {
                    string q = $@"
                            SELECT A.YMD, A.ITEM_CD, B.ITEM_NM, A.MM_SEQ, A.MM_TYPE, A.MM_PRC, A.MM_QTY, A.YMDHM 
                            FROM MF_AT_MST A , MF_AT_ITEM B
                            WHERE A.YMD=B.YMD AND A.ITEM_CD=B.ITEM_CD AND A.YMD='{ymd}' AND A.ITEM_CD='{itemCode}' 
                            AND A.YMDHM > TO_CHAR(SYSDATE-3/24/60,'YYYYMMDDHH24MISS')
                                ";
                    cmd.Connection = connection;
                    cmd.CommandText = q;
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ATOrder order = new ATOrder();
                            order.일자 = reader["YMD"].ToString();
                            order.종목코드 = reader["ITEM_CD"].ToString();
                            order.종목명 = reader["ITEM_NM"].ToString();
                            order.일련번호 = int.Parse(reader["MM_SEQ"].ToString());
                            order.매매구분 = reader["MM_TYPE"].ToString();
                            order.주문가격 = int.Parse(reader["MM_PRC"].ToString());
                            order.주문수량 = int.Parse(reader["MM_QTY"].ToString());
                            order.시간 = reader["YMDHM"].ToString();
                            orderList.Add(order);
                        }
                    }
                }
            }
            return orderList;
        }

        public void updateMstForOrder(ATOrder order)
        {
            using (OracleConnection connection = new OracleConnection(conn_str))
            {
                connection.Open();
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = connection;
                string q = $@"update MF_AT_MST SET ACCT='{order.계좌}', CC_PRC={order.주문가격}, CC_QTY={order.주문수량}, CC_STATUS=1, CC_DT=SYSDATE
                                WHERE YMD={order.일자} AND ITEM_CD={order.종목코드} AND MM_SEQ={order.일련번호} ";
                cmd.CommandText = q;
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            };
        }

        public DataTable getMfAtMst(string ymd, string itemCode)
        {
            DataTable result = null;
            string whereItemCode = itemCode == null || itemCode.Length == 0 ? "" : $" AND A.ITEM_CD='{itemCode}' ";
            string query = $@"
                    SELECT 
                    A.YMD, SUBSTR(A.YMDHM,9) AS HMS, DECODE(A.MM_TYPE,'B','매수','매도') AS MM_TYPE, A.MM_QTY
                    , A.PRC, A.S_PRC, A.H_PRC, A.L_PRC
                    , A.VOL, A.BVOL, A.VOL_BRT, A.MA5, A.MA10, A.MA20, A.MA60, A.MA120, A.DONE, A.RET_MSG, A.REG_DT, A.UPD_DT
                    FROM MF_AT_MST A , MF_AT_ITEM B 
                    WHERE A.YMD=B.YMD AND A.ITEM_CD=B.ITEM_CD 
                    AND A.YMD='{ymd}' {whereItemCode} 
                    ORDER BY A.ITEM_CD, A.YMDHM, A.MM_SEQ
                        ";
            using (OracleConnection connection = new OracleConnection(conn_str))
            {
                connection.Open();
                OracleDataAdapter adapter = new OracleDataAdapter();
                adapter.SelectCommand = new OracleCommand(query, connection);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                result = ds.Tables[0];
            };
            return result;
        }
        public DataTable getTableMfAtItem(string ymd)
        {
            DataTable result = null;
            string query = $@"
                    SELECT A.ITEM_CD, A.ITEM_NM, 0 AS PRC
                    FROM MF_AT_ITEM A
                    WHERE A.YMD='{ymd}'
                    AND A.REAL_YN='Y' 
                    ORDER BY A.ITEM_NM
                    ";
            using (OracleConnection connection = new OracleConnection(conn_str))
            {
                connection.Open();
                OracleDataAdapter adapter = new OracleDataAdapter();
                adapter.SelectCommand = new OracleCommand(query, connection);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                result = ds.Tables[0];
            };
            return result;
        }

    }
}
