using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetelStockLib.core
{
    public class Common
    {
        public static bool equalClauseInDataGridRow(string cname, string val, DataGridViewRow row)
        {
            bool result = false;
            if (row.Cells[cname].Value != null && row.Cells[cname].Value.ToString().Equals(val))
            {
                result = true;
            }
            return result;
        }

        public static int parseInt(string val)
        {
            return (val == null || val.Length == 0) ? 0 : int.Parse(val);
        }

        public static double parseDouble(string val)
        {
            return (val == null || val.Length == 0) ? 0 : double.Parse(val);
        }

        private static int screenNum = 5000;
        public static string GetScreenNum()
        {
            if (screenNum >= 9999)
                screenNum = 5000;
            screenNum++;
            return screenNum.ToString();
        }
    }
}
