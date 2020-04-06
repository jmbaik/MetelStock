using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetelStockLib.core
{
    public class UserInfo
    {
        public string UserName { get; set; } //사용자명
        public string UserId { get; set; } //사용자 아이디
        public string[] Accounts { get; set; } //계좌목록
        public string ConnectedServer { get; set; } //접속서버 - 모의투자 or 실서버
    }

    public class ChartData
    {
        public double HighPrice { get; set; } //고가
        public double LowPrice { get; set; } //저가
        public double OpenPrice { get; set; } //시가
        public double ClosePrice { get; set; } //종가
        public long Volume { get; set; } //거래량
        public string Time { get; set; } //시간

        public ChartData(double highPrice, double lowPrice, double openPrice, double closePrice, long volume, string time)
        {
            this.HighPrice = highPrice;
            this.LowPrice = lowPrice;
            this.OpenPrice = openPrice;
            this.ClosePrice = closePrice;
            this.Volume = volume;
            this.Time = time;
        }
    }

    public class Balance
    {
        public string ItemCode { get; set; } //종목코드
        public string ItemName { get; set; } //종목명
        public long HoldingQuantity { get; set; } //보유수량
        public double BuyingPrice { get; set; } //매입단가
        public long BuyingAmount { get; set; } //매입금액
        public long CurrentPrice { get; set; } //현재가
        public long EvaluatedAmount { get; set; } //평가금액
        public long ProfitAmount { get; set; } //손익금액
        public double ProfitRate { get; set; } //손익률
    }

    [Serializable]
    public class Group //관심종목 그룹
    {
        public static int lastID = 0;

        public int _ID { get; set; } //그룹번호
        public string Name { get; set; } //그룹명
        public List<StockItem> InterestItemList { get; set; } //관심종목 리스트

        public Group()
        {
            InterestItemList = new List<StockItem>();

            lastID++;
            _ID = lastID;
        }
    }

    /*
    [Serializable]
    public class StockItem
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public long Price { get; set; }
        public long NetChange { get; set; }
        public double UpDownRate { get; set; }
        public long Volume { get; set; }
        public long OpenPrice { get; set; }
        public long HighPrice { get; set; }
        public long LowPrice { get; set; }
    }
    */

    
    [Serializable]
    public class StockItem //종목정보
    {
        private string itemCode = "";
        private string itemName = "";
        private long price = 0;
        private long netChange = 0;
        private double upDownRate = 0;
        private long volume = 0;
        private long openPrice = 0;
        private long highPrice = 0;
        private long lowPrice = 0;
        private long ma5 = 0;
        private long ma10 = 0;
        private long ma20 = 0;
        private long ma60 = 0;
        private long ma120 = 0;
        private long ma240 = 0;

        public string ItemCode { get => itemCode; set => itemCode = value; }
        public string ItemName { get => itemName; set => itemName = value; }
        public long Price { get => price; set => price = value; }
        public long NetChange { get => netChange; set => netChange = value; }
        public double UpDownRate { get => upDownRate; set => upDownRate = value; }
        public long Volume { get => volume; set => volume = value; }
        public long OpenPrice { get => openPrice; set => openPrice = value; }
        public long HighPrice { get => highPrice; set => highPrice = value; }
        public long LowPrice { get => lowPrice; set => lowPrice = value; }
        public long Ma5 { get => ma5; set => ma5 = value; }
        public long Ma10 { get => ma10; set => ma10 = value; }
        public long Ma20 { get => ma20; set => ma20 = value; }
        public long Ma60 { get => ma60; set => ma60 = value; }
        public long Ma120 { get => ma120; set => ma120 = value; }
        public long Ma240 { get => ma240; set => ma240 = value; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"itemCode:{itemCode} ");
            sb.Append($"itemName:{itemName} ");
            sb.Append($"price:{price} ");
            sb.Append($"netChange:{netChange} ");
            sb.Append($"upDownRate:{upDownRate} ");
            sb.Append($"volume:{volume} ");
            sb.Append($"openPrice:{openPrice} ");
            sb.Append($"highPrice:{highPrice} ");
            sb.Append($"lowPrice:{lowPrice} ");
            sb.Append($"ma5:{ma5} ");
            sb.Append($"ma10:{ma10} ");
            sb.Append($"ma20:{ma20} ");
            sb.Append($"ma60:{ma60} ");
            sb.Append($"ma120:{ma120} ");
            sb.Append($"ma240:{ma240} ");
            return sb.ToString();
        }
    }
    
}
