using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetelStockLib.core
{
    public class OnReceivedLogMessageEventArgs : EventArgs
    {
        public String Message { get; set; }

        public OnReceivedLogMessageEventArgs(String Message)
        {
            this.Message = Message;
        }
    }

    public class OnReceivedUserInfoEventArgs : EventArgs
    {
        public UserInfo UserInfo { get; set; }
        public string Message { get; set; }

        public OnReceivedUserInfoEventArgs(UserInfo userInfo)
        {
            this.UserInfo = userInfo;
            this.Message = "사용자 정보를 수신하였습니다.";
        }
    }

    public class OnReceivedChartDataEventArgs : EventArgs
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public List<ChartData> ChartDataList { get; set; }
        public bool HasNext { get; set; }
        public string Message { get; set; }

        public OnReceivedChartDataEventArgs(string itemCode, string itemName, List<ChartData> chartDataList, bool hasNext)
        {
            this.ItemCode = itemCode;
            this.ItemName = itemName;
            this.ChartDataList = chartDataList;
            this.HasNext = hasNext;
            this.Message = "차트데이터를 수신하였습니다.";
        }
    }
    public class OnReceivedAutoTrading3MDataEventArgs : EventArgs
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public List<ChartData> ChartDataList { get; set; }
        public bool HasNext { get; set; }
        public string Message { get; set; }

        public OnReceivedAutoTrading3MDataEventArgs(string itemCode, string itemName, List<ChartData> chartDataList, bool hasNext)
        {
            this.ItemCode = itemCode;
            this.ItemName = itemName;
            this.ChartDataList = chartDataList;
            this.HasNext = hasNext;
            this.Message = "자동매매 차트데이터를 수신하였습니다.";
        }
    }
    public class OnReceivedAccountBalanceEventArgs : EventArgs
    {
        public long 예수금 { get; set; }
        public long D2추정예수금 { get; set; }
        public long 예탁자산평가액 { get; set; }
        public long 총매입금액 { get; set; }
        public long 추정예탁자산 { get; set; }
        public List<Balance> BalanceList { get; set; }
        public string Message { get; set; }

        public OnReceivedAccountBalanceEventArgs(long 예수금, long D2추정예수금, long 예탁자산평가액, long 총매입금액, long 추정예탁자산, List<Balance> BalanceList)
        {
            this.예수금 = 예수금;
            this.D2추정예수금 = D2추정예수금;
            this.예탁자산평가액 = 예탁자산평가액;
            this.총매입금액 = 총매입금액;
            this.추정예탁자산 = 추정예탁자산;
            this.BalanceList = BalanceList;
            this.Message = "계좌잔고 데이터를 수신하였습니다.";
        }
    }

    public class OnReceivedItemInfoEventArgs : EventArgs
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
        public string Message { get; set; }

        public OnReceivedItemInfoEventArgs(string itemCode, string itemName, long price, long netChange, double upDownRate, long volume, long openPrice, long highPrice, long lowPrice)
        {
            ItemCode = itemCode;
            ItemName = itemName;
            Price = price;
            NetChange = netChange;
            UpDownRate = upDownRate;
            Volume = volume;
            OpenPrice = openPrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
            Message = "주식 기본 정보를 수신하였습니다.";
        }
    }

    public class OnCreatedNewGroupEventArgs : EventArgs
    {
        public Group Group { get; set; }
        public string Message { get; set; }

        public OnCreatedNewGroupEventArgs(Group group)
        {
            this.Group = group;
            this.Message = "새 그룹이 생성되었습니다.";
        }
    }

    public class OnRemovedGroupEventArgs : EventArgs
    {
        public Group Group { get; set; }
        public string Message { get; set; }

        public OnRemovedGroupEventArgs(Group group)
        {
            this.Group = group;
            this.Message = "그룹이 삭제 되었습니다.";
        }
    }

    public class OnChangedGroupNameEventArgs : EventArgs
    {
        public Group Group { get; set; }
        public string Message { get; set; }

        public OnChangedGroupNameEventArgs(Group group)
        {
            this.Group = group;
            this.Message = "그룹명이 변경되었습니다.";
        }
    }

    public class OnAddInterestItemEventArgs : EventArgs
    {
        public Group Group { get; set; }
        public StockItem InterestItem { get; set; }
        public string Message { get; set; }

        public OnAddInterestItemEventArgs(Group group, StockItem interestItem)
        {
            this.Group = group;
            this.InterestItem = interestItem;
            this.Message = "관심종목이 추가 되었습니다.";
        }
    }

    public class OnRemoveInterestItemEventArgs : EventArgs
    {
        public Group Group { get; set; }
        public StockItem InterestItem { get; set; }
        public string Message { get; set; }

        public OnRemoveInterestItemEventArgs(Group group, StockItem interestItem)
        {
            this.Group = group;
            this.InterestItem = interestItem;
            this.Message = "관심종목이 제거 되었습니다.";
        }
    }

    public class OnReceivedStockItemsEventArgs : EventArgs
    {
        public string EventName { get; set; }
        public List<StockItem> StockItemList { get; set; }
        public string Message { get; set; }

        public OnReceivedStockItemsEventArgs(string eventName, List<StockItem> stockItemList)
        {
            this.EventName = eventName;
            this.StockItemList = stockItemList;
            this.Message = "종목 정보를 가져왔습니다";
        }
    }

    public class OnReceivedATOrderListEventArgs : EventArgs
    {
        public List<ATOrder> AtOrderList { get; set; }
        public string Message { get; set; }

        public OnReceivedATOrderListEventArgs(List<ATOrder> atOrderList)
        {
            this.AtOrderList = atOrderList;
            this.Message = "자동매매 오더리스트를 가져왔습니다.";
        }
    }

    public class OnReceivedATOrderEventArgs : EventArgs
    {
        public ATOrder AtOrder { get; set; }
        public string Message { get; set; }

        public OnReceivedATOrderEventArgs(ATOrder atOrder)
        {
            this.AtOrder = atOrder;
            this.Message = "자동매매 오더를 가져왔습니다.";
        }
    }
    public class OnReceiveSignalMstEventArgs : EventArgs
    {
        public string YMD { get; set; }
        public string ItemCode { get; set; }
        public string Message { get; set; }

        public OnReceiveSignalMstEventArgs(string ymd, string itemCode)
        {
            this.YMD = ymd;
            this.ItemCode = ItemCode;
            this.Message = "마스터 시그널 가져옵니다.";
        }
    }

    public class OnReceiveConditionVerEventArgs : EventArgs
    {
        public List<Condition> ConditionItemList { get; set; }
        public string Message { get; set; }

        public OnReceiveConditionVerEventArgs(List<Condition> conditionItemList)
        {
            this.ConditionItemList = conditionItemList;
            this.Message = "조건검색식 리스트를 가져옵니다.";
        }
    }

    public class OnReceiveConditionItemListEventArgs : EventArgs
    {
        public ConditionItemList ConditionItemList { get; set; }
        public string Message { get; set; }

        public OnReceiveConditionItemListEventArgs(ConditionItemList conditionItemList)
        {
            this.ConditionItemList = conditionItemList;
            this.Message = "조건 종목 리스트를 가져옵니다.";
        }
    }

    public class OnReceiveConditionItemEventArgs : EventArgs
    {
        public ConditionItem conditionItem { get; set; }
        public string Message { get; set; }
        public OnReceiveConditionItemEventArgs(ConditionItem conditionItem)
        {
            this.conditionItem = conditionItem;
            this.Message = "조건 종목을 가져옵니다.";
        }
    }

}
