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
        private long bvolume = 0;
        private long openPrice = 0;
        private long highPrice = 0;
        private long lowPrice = 0;
        private long ma5 = 0;
        private long ma10 = 0;
        private long ma20 = 0;
        private long ma60 = 0;
        private long ma120 = 0;
        private long ma240 = 0;
        private int rnk = 0; //현재순위
        private int brnk = 0; //전일순위
        private long trAmount = 0; //거래대금

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
        public long Bvolume { get => bvolume; set => bvolume = value; }
        public int Rnk { get => rnk; set => rnk = value; }
        public int Brnk { get => brnk; set => brnk = value; }
        public long TrAmount { get => trAmount; set => trAmount = value; }
        public StockItem()
        {
        }
    }

    [Serializable]
    public class ATOrder
    {
        public string 일자 { get; set; }
        public string 주문번호 { get; set; }
        public string 원주문번호 { get; set; }
        public int 일련번호 { get; set; }
        public string 종목코드 { get; set; }
        public string 종목명 { get; set; }
        public int 주문수량 { get; set; }
        public int 주문가격 { get; set; }
        public int 현재가 { get; set; }
        public int 미체결수량 { get; set; }
        public string 매매구분 { get; set; }
        public string 계좌 { get; set; }
        public int 상태 { get; set; }
        public string 시간 { get; set; }

        public ATOrder()
        {
        }
    }

    [Serializable]
    class StockBalance
    {
        public string itemCode { get; set; }
        public string itemName { get; set; }
        public double Amount { get; set; }
        public double BuyingPrice { get; set; }
        public double CurrentPrice { get; set; }
        public double EstimatedProfit { get; set; }
        public double ProfitRate { get; set; }
        public StockBalance(string 종목번호, string 종목명, double 보유수량, double 매입가, double 현재가, double 평가손익, double 수익률)
        {
            this.itemCode = 종목번호;
            this.itemName = 종목명;
            this.Amount = 보유수량;
            this.BuyingPrice = 매입가;
            this.CurrentPrice = 현재가;
            this.EstimatedProfit = 평가손익;
            this.ProfitRate = 수익률;
        }
    }

}
