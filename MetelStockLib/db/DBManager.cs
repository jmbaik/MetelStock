using System;
using System.Collections.Generic;
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

    }
}
