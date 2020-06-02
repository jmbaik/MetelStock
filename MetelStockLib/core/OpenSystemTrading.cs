using AxKHOpenAPILib;
using MetelStockLib.db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MetelStockLib.core
{
    public class OpenSystemTrading
    {
        AxKHOpenAPI axKHOpenAPI;

        private static OpenSystemTrading mfost;
        public RequestTrDataManager requestTrDataManager;

        public UserInfo userInfo;
        public List<Group> groupList = new List<Group>(); //관심 그룹 리스트
        public List<RealOrder> realOrderList = new List<RealOrder>();
        // 추가
        private DBManager dbm = new DBManager();

        //Events
        public event EventHandler<OnReceivedLogMessageEventArgs> OnReceivedLogMessage; //로그 수신 시
        public event EventHandler<OnReceivedUserInfoEventArgs> OnReceivedUserInfo; //사용자 정보 수신 시
        public event EventHandler<OnReceivedChartDataEventArgs> OnReceivedChartData; //차트데이터 수신 시
        public event EventHandler<OnReceivedAccountBalanceEventArgs> OnReceivedAccountBalance; //계좌평가현황 수신 시
        public event EventHandler<OnReceivedItemInfoEventArgs> OnReceivedItemInfo; //주식 기본정보 수신 시
        public event EventHandler<OnCreatedNewGroupEventArgs> OnCreatedNewGroup; //새 관심 그룹 생성 시 
        public event EventHandler<OnRemovedGroupEventArgs> OnRemovedGroup; //관심 그룹 삭제 시
        public event EventHandler<OnChangedGroupNameEventArgs> OnChangedGroupName; //관심 그룹명 변경 시
        public event EventHandler<OnAddInterestItemEventArgs> OnAddInterestItem; //관심종목 추가 시
        public event EventHandler<OnRemoveInterestItemEventArgs> OnRemoveInterestItem; //관심종목 제거 시

        // 추가 이벤트 
        public event EventHandler<OnReceivedStockItemsEventArgs> OnReceivedStockItems; //관심종목 받아올때
        public event EventHandler<EventArgs> OnLoged; // 로그온이후 
        public event EventHandler<OnReceivedAutoTrading3MDataEventArgs> OnReceivedAutoTrading3MData; //차트데이터 수신 시
        public event EventHandler<OnReceivedATOrderListEventArgs> OnReceivedOutstandingOrderList;       //미체결
        public event EventHandler<OnReceivedATOrderEventArgs> OnReceivedAutoTradingOrderBalance;   //잔고
        public event EventHandler<OnReceivedATOrderEventArgs> OnReceivedAutoTradingOrderAccept;         //접수
        public event EventHandler<OnReceivedATOrderEventArgs> OnReceivedAutoTradingOrderConclusion;     //체결
        public event EventHandler<OnReceiveSignalMstEventArgs> OnReceiveSignalMst;
        public event EventHandler<OnReceiveConditionVerEventArgs> OnReceiveConditionVer;
        public event EventHandler<OnReceiveConditionItemListEventArgs> OnReceiveTrCondition;
        public event EventHandler<OnReceiveConditionItemEventArgs> OnReceiveRealCondition;

        private const string ChartMinuteType = "3:3분";
        private static string[] NotWorkingDays = new string[]{"20200415","20200430","20200501","20200505","20200930","20201001"
                            ,"20200102","20201003","20201009","20201225","20201231"};
        public bool IsRealCondition = false;

        public static string getYmd()
        {
            string result = "";
            DateTime startDt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 0, 0);
            DateTime endDt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

            DayOfWeek dayOfWeek = DateTime.Now.DayOfWeek;
            if (dayOfWeek == DayOfWeek.Sunday || dayOfWeek == DayOfWeek.Saturday)
            {
                int dy_cha = dayOfWeek == DayOfWeek.Sunday ? - 2 : -1;
                if (NotWorkingDays.Contains(DateTime.Now.AddDays(dy_cha).ToString("yyyyMMdd")))
                {
                    if (NotWorkingDays.Contains(DateTime.Now.AddDays(dy_cha-1).ToString("yyyyMMdd")))
                    {
                        result = DateTime.Now.AddDays(dy_cha-2).ToString("yyyyMMdd");
                    } else
                    {
                        result = DateTime.Now.AddDays(dy_cha-1).ToString("yyyyMMdd");
                    }
                }
                else
                {
                    result = DateTime.Now.AddDays(dy_cha).ToString("yyyyMMdd");
                }
            }
            else
            {
                // 공휴일인 경우
                if (NotWorkingDays.Contains(DateTime.Now.ToString("yyyyMMdd")))
                {
                    if (NotWorkingDays.Contains(DateTime.Now.AddDays(-1).ToString("yyyyMMdd")))
                    {
                        result = DateTime.Now.AddDays(-2).ToString("yyyyMMdd");
                    }
                    else
                    {
                        result = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                    }
                }
                else
                {
                    if(startDt <= DateTime.Now && DateTime.Now <= endDt)
                    {
                        result = DateTime.Now.ToString("yyyyMMdd");
                    } else
                    {
                        //월요일 새벽인 경우 웬만하면 하지 마라
                        // 어제가 공휴일인 경우 
                        if (NotWorkingDays.Contains(DateTime.Now.AddDays(-1).ToString("yyyyMMdd")))
                        {
                            result = DateTime.Now.AddDays(-2).ToString("yyyyMMdd");
                        } else
                        {
                            result = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                        }
                    }
                }
            }
            return result;
        }

        private OpenSystemTrading()
        {
            requestTrDataManager = RequestTrDataManager.GetInstance();
            requestTrDataManager.Run();

            //LoadInterestListFromFile(); //저장된 관심 종목 리스트 로딩
        }

        public static OpenSystemTrading GetInstance() //싱글톤으로 jost 객체관리
        {
            if (mfost == null)
                mfost = new OpenSystemTrading();

            return mfost;
        }

        public void SetAxKHOpenAPI(AxKHOpenAPI axKHOpenAPI)
        {
            this.axKHOpenAPI = axKHOpenAPI;

            axKHOpenAPI.OnEventConnect += AxKHOpenAPI_OnEventConnect;
            axKHOpenAPI.OnReceiveTrData += AxKHOpenAPI_OnReceiveTrData;
            axKHOpenAPI.OnReceiveChejanData += AxKHOpenAPI_OnReceiveChejanData;
            //추가 
            // axKHOpenAPI.OnReceiveRealData += AxKHOpenAPI_OnReceiveRealData;
            axKHOpenAPI.OnReceiveConditionVer += AxKHOpenAPI_OnReceiveConditionVer;
            axKHOpenAPI.OnReceiveTrCondition += AxKHOpenAPI_OnReceiveTrCondition;
            axKHOpenAPI.OnReceiveRealCondition += AxKHOpenAPI_OnReceiveRealCondition;
        }

        public void Start()
        {
            if (axKHOpenAPI != null)
                axKHOpenAPI.CommConnect();
            else
            {
                string message = "Metel Finance OpenSystemTrading에 axKHOpenAPI를 등록해주세요.";
                SendLogMessage(message);
            }
        }

        public void GetUserInfo()
        {
            string userName = axKHOpenAPI.GetLoginInfo("USER_NAME");
            string userId = axKHOpenAPI.GetLoginInfo("USER_ID");
            string accountList = axKHOpenAPI.GetLoginInfo("ACCLIST");
            string server = string.Empty;

            if (axKHOpenAPI.GetLoginInfo("GetServerGubun").Equals("1"))
                server = "모의서버";
            else
                server = "실서버";

            string[] accounts = accountList.Split(';');

            this.userInfo = new UserInfo()
            {
                UserName = userName,
                UserId = userId,
                Accounts = accounts,
                ConnectedServer = server
            };

            OnReceivedUserInfo?.Invoke(this, new OnReceivedUserInfoEventArgs(userInfo));
        }

        private void AxKHOpenAPI_OnEventConnect(object sender, _DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            if (e.nErrCode == ErrorCode.정상처리)
            {
                GetUserInfo();
                SendLogMessage("로그인 성공");
                OnLoged?.Invoke(this, new EventArgs());
            }
            else if (e.nErrCode == ErrorCode.사용자정보교환실패)
                SendLogMessage("로그인 실패 : 사용자 정보교환 실패");
            else if (e.nErrCode == ErrorCode.서버접속실패)
                SendLogMessage("로그인 실패 : 서버접속 실패");
            else if (e.nErrCode == ErrorCode.버전처리실패)
                SendLogMessage("로그인 실패 : 버전처리 실패");
        }

        private void AxKHOpenAPI_OnReceiveTrData(object senderm, _DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (e.sRQName.Equals(RqName.주식틱차트조회요청))
            {
                string itemCode = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "종목코드").Trim();
                string itemName = axKHOpenAPI.GetMasterCodeName(itemCode);

                int cnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                List<ChartData> chartDataList = new List<ChartData>();
                for (int i = 0; i < cnt; i++)
                {
                    string time = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim();
                    long closePrice = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가")));
                    long openPrice = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "시가")));
                    long highPrice = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "고가")));
                    long lowPrice = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "저가")));
                    long volume = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래량")));

                    chartDataList.Add(new ChartData(highPrice, lowPrice, openPrice, closePrice, volume, time));
                }

                bool hasNext = false;
                if (e.sPrevNext.Equals("2")) //추가데이터가 존재할 경우
                    hasNext = true;

                OnReceivedChartData?.Invoke(this, new OnReceivedChartDataEventArgs(itemCode, itemName, chartDataList, hasNext));
                SendLogMessage(itemName + " 주식틱차트조회요청 결과 수신");
            }
            else if (e.sRQName.Equals(RqName.주식일봉차트조회요청) || e.sRQName.Equals(RqName.주식분봉차트조회요청))
            {
                string dayminute = e.sRQName.Equals(RqName.주식일봉차트조회요청) ? "D" : "M";
                string itemCode = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "종목코드").Trim();
                string itemName = axKHOpenAPI.GetMasterCodeName(itemCode);

                int cnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                List<ChartData> chartDataList = new List<ChartData>();
                for (int i = 0; i < cnt; i++)
                {
                    string time = "";
                    if(dayminute.Equals("D"))
                        time = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "일자").Trim();
                    else
                        time = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim();
                    long closePrice = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가")));
                    long openPrice = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "시가")));
                    long highPrice = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "고가")));
                    long lowPrice = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "저가")));
                    long volume = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래량")));

                    chartDataList.Add(new ChartData(highPrice, lowPrice, openPrice, closePrice, volume, time));
                }

                bool hasNext = false;
                if (e.sPrevNext.Equals("2")) //추가데이터가 존재할 경우
                    hasNext = true;

                OnReceivedChartData?.Invoke(this, new OnReceivedChartDataEventArgs(itemCode, itemName, chartDataList, hasNext));
                if (dayminute.Equals("D"))
                {
                    // dbms 일봉 데이터 넣기
                    dbm.mergeDMPrc("D",itemCode, itemName, chartDataList);
                    SendLogMessage(itemName + " 주식일봉차트조회요청 결과 수신");
                }
                else
                {
                    // dbms 분종 데이터 넣기
                    dbm.mergeDMPrc("M", itemCode, itemName, chartDataList);
                    SendLogMessage(itemName + " 주식분봉차트조회요청 결과 수신");
                }
            } else if (e.sRQName.Equals(RqName.자동매매분봉차트요청))
            {
                string itemCode = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "종목코드").Trim();
                //string itemName = axKHOpenAPI.GetMasterCodeName(itemCode);
                string itemName = "";
                int cnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);
                string _ymd = getYmd();

                List<ChartData> chartDataList = new List<ChartData>();
                for (int i = 0; i < cnt; i++)
                {
                    try
                    {
                        string time = "";
                        time = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim();
                        long closePrice = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가")));
                        long openPrice = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "시가")));
                        long highPrice = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "고가")));
                        long lowPrice = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "저가")));
                        long volume = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래량")));

                        chartDataList.Add(new ChartData(highPrice, lowPrice, openPrice, closePrice, volume, time));
                    } catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                bool hasNext = false;
                if (e.sPrevNext.Equals("2")) //추가데이터가 존재할 경우
                    hasNext = true;

                // dbms 분종 데이터 넣기
                dbm.mergeMfAt3M(itemCode, chartDataList);
                // 자동매매 오더 
                Thread.Sleep(300);
                List<ATOrder> orderList = dbm.getMstForOrder(_ymd, itemCode);
                for (int i = 0; i < orderList.Count; i++)
                {
                    ATOrder order = orderList[i];
                    order.계좌 = userInfo.Accounts[0];
                    decimal dd = 1000000 / order.주문가격;
                    int 주문수량 =(int)Math.Truncate(dd);
                    int mmtype = order.매매구분.Equals("B") ? 1 : 2;
                    //int 주문가격 = order.매매구분.Equals("B") ? order.주문가격+100: order.주문가격;
                    MMType _mm_type = order.매매구분.Equals("B") ? MMType.신규매수 : MMType.신규매도;
                    // RequestOrder(order.계좌, _mm_type, itemCode, 주문수량, 주문가격, PriceType.지정가);
                    // dbm.updateMstForOrder(order);
                    RealOrder realOrder = new RealOrder(order.계좌, order.종목코드, order.종목명, order.매매구분, order.주문가격, 주문수량, PriceType.지정가);
                    realOrderList.Add(realOrder);
                    setRealRegAppend(itemCode);
                    Console.WriteLine($"분봉차트조회 실시간등록 {itemCode}|매매구분:{order.매매구분}|주문가격:{order.주문가격}|주문수량:{주문수량}");
                    OnReceiveSignalMst?.Invoke(this, new OnReceiveSignalMstEventArgs(_ymd, itemCode));                    
                    /*
                    int result = axKHOpenAPI.SendOrder(RqName.주식주문, ScreenNo.주식주문, order.계좌, mmtype, itemCode, 주문수량, 주문가격, 거래구분, "");
                    if(result == 0)
                    {
                        dbm.updateMstForOrder(order);
                        Console.WriteLine($"주문 매매구분:{order.매매구분} 종목:{itemName}[{itemCode}]| 가격:{order.주문가격} | 수량:{order.주문수량} [{DateTime.Now.ToString()}]");
                    }
                    else
                    {
                        Console.WriteLine($"주문실패 매매구분:{order.매매구분} 종목:{itemName}[{itemCode}]| 가격:{order.주문가격} | 수량:{order.주문수량} [{DateTime.Now.ToString()}]");
                    }
                    */
                }

                Console.WriteLine($"[응답]자동분봉차트요청 mergeMfAt3M작업완료 {itemName}[{itemCode}] [" + DateTime.Now.ToString() +"]");
            }
            else if (e.sRQName.Equals(RqName.계좌평가현황요청))
            {
                try
                {
                    long 예수금 = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "예수금"));
                    long D2추정예수금 = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "D+2추정예수금"));
                    long 예탁자산평가액 = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "예탁자산평가액"));
                    long 총매입금액 = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "총매입금액"));
                    long 추정예탁자산 = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "추정예탁자산"));

                    int cnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                    List<Balance> balanceList = new List<Balance>();
                    for (int i = 0; i < cnt; i++)
                    {
                        string itemCode = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                        string itemName = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                        long holdingQuantity = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유수량"));
                        double buyingPrice = double.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평균단가"));
                        long buyingAmount = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입금액"));
                        long currentPrice = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가"));
                        long evaluatedAmount = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가금액"));
                        long profitAmount = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "손익금액"));
                        double profitRate = double.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "손익율"));

                        Balance balance = new Balance()
                        {
                            ItemCode = itemCode,
                            ItemName = itemName,
                            HoldingQuantity = holdingQuantity,
                            BuyingPrice = buyingPrice,
                            BuyingAmount = buyingAmount,
                            CurrentPrice = currentPrice,
                            EvaluatedAmount = evaluatedAmount,
                            ProfitAmount = profitAmount,
                            ProfitRate = profitRate
                        };
                        balanceList.Add(balance);
                    }

                    OnReceivedAccountBalance?.Invoke(this, new OnReceivedAccountBalanceEventArgs(예수금, D2추정예수금, 예탁자산평가액, 총매입금액, 추정예탁자산, balanceList));
                    SendLogMessage("계좌평가현황요청 결과 수신");
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
            else if (e.sRQName.Equals(RqName.주식기본정보요청))
            {
                try
                {
                    string itemCode = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "종목코드").Trim();
                    string itemName = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "종목명").Trim();
                    long price = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "현재가")));
                    long netChange = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "전일대비"));
                    double upDownRate = double.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "등락율"));
                    long volume = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "거래량"));
                    long openPrice = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "시가")));
                    long highPrice = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "고가")));
                    long lowPrice = Math.Abs(long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, 0, "저가")));

                    OnReceivedItemInfo?.Invoke(this, new OnReceivedItemInfoEventArgs(itemCode, itemName, price, netChange, upDownRate, volume, openPrice, highPrice, lowPrice));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }else if (e.sRQName.Equals(RqName.당일거래량상위요청) || e.sRQName.Equals(RqName.전일거래량상위요청))
            {
                try
                {
                    int cnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);
                    List<StockItem> stockList = new List<StockItem>();
                    for (int i = 0; i < cnt; i++)
                    {
                        string itemCode = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                        string itemName = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                        long currentPrice = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가"));
                        long netChange = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일대비"));
                        double updownRate = double.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "등락률"));
                        long volume = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래량"));

                        long bVolume = 0;
                        long trAmt = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래금액"));
                        int rnk = 0;
                        int brnk = 0;

                        if (itemName.StartsWith("KODEX")) continue;

                        if (updownRate > 0)
                        {
                            StockItem stockItem = new StockItem()
                            {
                                ItemCode = itemCode,
                                ItemName = itemName,
                                Price = currentPrice,
                                NetChange = netChange,
                                UpDownRate = updownRate,
                                Volume = volume,
                                Bvolume = bVolume,
                                TrAmount = trAmt,
                                Rnk = rnk,
                                Brnk = brnk
                            };
                            stockList.Add(stockItem);
                        }
                        if (stockList.Count > 33)
                            break;
                    }
                    dbm.mergeListMfAtItem(getYmd(), stockList);
                    OnReceivedStockItems?.Invoke(this, new OnReceivedStockItemsEventArgs(e.sRQName, stockList));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            } else if (e.sRQName.Equals(RqName.자동매매당일거래상위요청))
            {
                // 3분마다 실행되는 자동매매 타입
                int cnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);
                List<StockItem> stockList = new List<StockItem>();
                for (int i = 0; i < cnt; i++)
                {
                    string itemCode = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                    string itemName = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                    long currentPrice = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가"));
                    long netChange = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일대비"));
                    double updownRate = double.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "등락률"));
                    long volume = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래량"));

                    long bVolume = 0;
                    long trAmt = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래금액"));
                    int rnk = 0;
                    int brnk = 0;

                    if (itemName.StartsWith("KODEX")) continue;

                    if(updownRate > 0)
                    {
                        StockItem stockItem = new StockItem()
                        {
                            ItemCode = itemCode,
                            ItemName = itemName,
                            Price = currentPrice,
                            NetChange = netChange,
                            UpDownRate = updownRate,
                            Volume = volume,
                            Bvolume = bVolume,
                            TrAmount = trAmt,
                            Rnk = rnk,
                            Brnk = brnk
                        };
                        stockList.Add(stockItem);
                    }

                    if (stockList.Count > 33)
                        break;
                }
                dbm.mergeListMfAtItem(getYmd(), stockList);
                List<StockItem> autoStockList = dbm.getMfAtItem(getYmd());
                foreach (StockItem item in autoStockList)
                {
                    RequestMinuteChartData(item.ItemCode, ChartMinuteType, RqName.자동매매분봉차트요청);
                    Console.WriteLine("자동매매 timer 시작 RequestMinuteChartData ::: " + item.ItemCode + "[" + DateTime.Now.ToString() + "]");
                }
            } else if (e.sRQName.Equals(RqName.자동매매거래대금상위요청))
            {
                // 5분마다 실행되는 자동매매 타입
                int cnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                List<StockItem> stockList = new List<StockItem>();
                for (int i = 0; i < cnt; i++)
                {
                    string itemCode = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                    string itemName = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                    long currentPrice = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가"));
                    long netChange = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일대비"));
                    double updownRate = double.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "등락률"));
                    long volume = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재거래량"));
                    long bVolume = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일거래량"));
                    long trAmt = long.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래대금"));
                    int rnk = int.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재순위"));
                    int brnk = int.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일순위"));

                    if (itemName.StartsWith("KODEX")) continue;
                    if(updownRate > 0)
                    {
                        StockItem stockItem = new StockItem()
                        {
                            ItemCode = itemCode,
                            ItemName = itemName,
                            Price = currentPrice,
                            NetChange = netChange,
                            UpDownRate = updownRate,
                            Volume = volume,
                            Bvolume = bVolume,
                            TrAmount = trAmt,
                            Rnk = rnk,
                            Brnk = brnk
                        };
                        stockList.Add(stockItem);
                    }

                    if (stockList.Count > 30)
                        break;
                }
                dbm.mergeListMfAtItem(getYmd(), stockList);
                List<StockItem> autoStockList = dbm.getMfAtItem(getYmd());
                foreach (StockItem item in autoStockList)
                {
                    RequestMinuteChartData(item.ItemCode, ChartMinuteType, RqName.자동매매분봉차트요청);
                    Console.WriteLine("자동매매 timer 시작 RequestMinuteChartData ::: " + item.ItemCode + "[" + DateTime.Now.ToString() + "]");
                }
            } else if (e.sRQName.Equals(RqName.실시간미체결요청))
            {
                int cnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);
                List<ATOrder> orderList = new List<ATOrder>();
                for (int i = 0; i < cnt; i++)
                {
                    string 주문번호 = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "주문번호").Trim();
                    string 종목코드 = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                    string 종목명 = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                    int 주문수량 = int.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "주문수량"));
                    int 주문가격 = int.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "주문가격"));
                    int 현재가 = int.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가"));
                    int 미체결수량 = int.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "미체결수량"));
                    string 주문구분 = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "주문구분").Trim();
                    string 원주문번호 = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "원주문번호").Trim();
                    string 시간 = axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "시간").Trim();

                    ATOrder order = new ATOrder();
                    order.종목코드 = 종목코드;
                    order.종목명 = 종목명;
                    order.주문번호 = 주문번호;
                    order.주문가격 = 주문가격;
                    order.주문수량 = 주문수량;
                    order.미체결수량 = 미체결수량;
                    order.현재가 = 현재가;
                    order.매매구분 = 주문구분;
                    order.원주문번호 = 원주문번호;
                    order.시간 = 시간;
                    orderList.Add(order);
                }
                OnReceivedOutstandingOrderList?.Invoke(this, new OnReceivedATOrderListEventArgs(orderList));
            }
        }

        private void AxKHOpenAPI_OnReceiveChejanData(object sender, _DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            /* 실시간 목록 주문 체결 ************************/
            if (e.sGubun.Equals("0")) //접수 or 체결
            {
                string chejanGubun = axKHOpenAPI.GetChejanData(913).Trim();     //주문내역인지 체결내역인지 가져옴
                string 계좌번호 = axKHOpenAPI.GetChejanData(9201).Trim();
                string 주문번호 = axKHOpenAPI.GetChejanData(9203).Trim();
                string 종목코드 = axKHOpenAPI.GetChejanData(9001).Trim();
                string 종목명 = axKHOpenAPI.GetChejanData(302).Trim();         
                int 주문수량 = Common.parseInt(axKHOpenAPI.GetChejanData(900));
                int 주문가격 = Common.parseInt(axKHOpenAPI.GetChejanData(901));
                int 미체결수량 = Common.parseInt(axKHOpenAPI.GetChejanData(902));
                string 주문구분 = axKHOpenAPI.GetChejanData(905);
                string 거래구분 = axKHOpenAPI.GetChejanData(906);
                string 주문체결시간 = axKHOpenAPI.GetChejanData(908).Trim();

                int 체결가 = Common.parseInt(axKHOpenAPI.GetChejanData(910));
                int 체결량 = Common.parseInt(axKHOpenAPI.GetChejanData(911));
                int 현재가 = Common.parseInt(axKHOpenAPI.GetChejanData(10));
                int 단위체결량 = Common.parseInt(axKHOpenAPI.GetChejanData(915));

                // 추가
                string 매매구분 = axKHOpenAPI.GetChejanData(907).Trim();
                string 원주문번호 = axKHOpenAPI.GetChejanData(904).Trim();

                ATOrder order = new ATOrder();
                order.계좌번호 = 계좌번호;
                order.주문번호 = 주문번호;
                order.종목코드 = 종목코드;
                order.종목명 = 종목명;
                order.주문수량 = 주문수량;
                order.주문가격 = 주문가격;
                order.미체결수량 = 미체결수량;
                order.주문구분 = 주문구분;
                order.거래구분 = 거래구분;
                order.체결가 = 체결가;
                order.체결량 = 체결량;
                order.단위체결량 = 단위체결량;
                order.현재가 = 현재가;
                
                order.매매구분 = 매매구분;
                order.원주문번호 = 원주문번호;
                order.시간 = 주문체결시간;

                //if (체결량.Length > 0) //체결
                if(chejanGubun.Equals("접수"))
                {
                    OnReceivedAutoTradingOrderAccept?.Invoke(this, new OnReceivedATOrderEventArgs(order));
                    Console.WriteLine($"체결량 = {체결량}");
                }
                else if(chejanGubun.Equals("체결")) //체결
                {
                    OnReceivedAutoTradingOrderConclusion?.Invoke(this, new OnReceivedATOrderEventArgs(order));
                    Console.WriteLine($"체결량 = {체결량}");
                }

            }
            else if (e.sGubun.Equals("1")) //잔고전달
            {
                string account = axKHOpenAPI.GetChejanData(9201);    // 계좌번호
                string itemCode = axKHOpenAPI.GetChejanData(9001);    // 종목코드
                string itemName = axKHOpenAPI.GetChejanData(302).Trim();    // 종목명
                string balanceQty = axKHOpenAPI.GetChejanData(930);    // 보유수량
                string buyingPrice = axKHOpenAPI.GetChejanData(931);    // 매입단가
                string totalBuyingPrice = axKHOpenAPI.GetChejanData(932);    // 총매입가
                string orderAvailableQty = axKHOpenAPI.GetChejanData(933);    // 주문가능수량
                string tradingType = axKHOpenAPI.GetChejanData(946);    // 매수매도구분
                string prifitRate = axKHOpenAPI.GetChejanData(8019);    // 손익률
                
                ATOrder order = new ATOrder();
                order.계좌번호 = account;
                order.종목코드 = itemCode;
                order.종목명 = itemName;
                order.보유수량 = Common.parseInt(balanceQty);
                order.매입단가 = Common.parseInt(buyingPrice);
                order.총매입가 = Common.parseInt(totalBuyingPrice);
                order.주문가능수량 = Common.parseInt(orderAvailableQty);
                order.거래구분 = tradingType;
                order.손익률 = Common.parseDouble(prifitRate);

                OnReceivedAutoTradingOrderBalance?.Invoke(this, new OnReceivedATOrderEventArgs(order));
            }
        }

        public bool SendOrder(string account, MMType mType, string itemCode, int qty, int price, PriceType priceType)
        {
            string _priceType = priceType.ToString().Length == 1 ? "0" + priceType.ToString() : priceType.ToString();
            int orderResult = axKHOpenAPI.SendOrder(RqName.주식주문
                                            , ScreenNo.주식주문
                                            , account
                                            , (int)mType
                                            , itemCode
                                            , qty
                                            , price
                                            , _priceType      //지정가
                                            , ""
                                            );
            return orderResult == 0 ? true : false;
        }
        public bool SendOrder(RealOrder order)
        {
            int mType = order.OrderType.Equals("B") ? (int)MMType.신규매수 : (int)MMType.신규매도;
            string _priceType = order.PriceType.ToString().Length == 1 ? "0" + order.PriceType.ToString() : order.PriceType.ToString();
            int orderResult = axKHOpenAPI.SendOrder(RqName.주식주문
                                            , ScreenNo.주식주문
                                            , order.Account
                                            , mType
                                            , order.ItemCode
                                            , order.Qty
                                            , order.Price
                                            , _priceType      //지정가
                                            , ""
                                            );
            return orderResult == 0 ? true : false;
        }

        public void RequestOrder(string account, MMType mType, string itemCode, int qty, int price, PriceType priceType)
        {
            Task requestTask = new Task(() =>
            {
                string _priceType = priceType.ToString().Length == 1 ? "0" + priceType.ToString() : priceType.ToString();
                axKHOpenAPI.SendOrder(RqName.주식주문
                                                , ScreenNo.주식주문
                                                , account
                                                , (int)mType
                                                , itemCode
                                                , qty
                                                , price
                                                , _priceType      //지정가
                                                , ""
                                                );
            });

            requestTrDataManager.RequestTrDataOnFirst(requestTask);            
        }

        private void AxKHOpenAPI_OnReceiveRealData(object sender, _DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
            if (e.sRealType.Equals("주식체결"))
            {
                // "20;10;11;12;13;14;15;16;17;18;25;26;27;28;29;30;31;32;228;311;290";
                /*
                string item_cd = e.sRealKey;
                string 체결시간 = axKHOpenAPI.GetCommRealData(item_cd, 20); //체결시간
                string 현재가 = axKHOpenAPI.GetCommRealData(item_cd, 10); //현재가
                string 전일대비 = axKHOpenAPI.GetCommRealData(item_cd, 11); //전일대비
                string 등락율 = axKHOpenAPI.GetCommRealData(item_cd, 12); //등락율
                string 누적거래량 = axKHOpenAPI.GetCommRealData(item_cd, 13);
                string 누적거래대금 = axKHOpenAPI.GetCommRealData(item_cd, 14);
                string 거래량 = axKHOpenAPI.GetCommRealData(item_cd, 15);
                string 시가 = axKHOpenAPI.GetCommRealData(item_cd, 16);
                string 고가 = axKHOpenAPI.GetCommRealData(item_cd, 17);
                string 저가 = axKHOpenAPI.GetCommRealData(item_cd, 18);
                string 전일대비기호 = axKHOpenAPI.GetCommRealData(item_cd, 25);
                string 전일거래량대비 = axKHOpenAPI.GetCommRealData(item_cd, 26);
                string 최우선매도호가 = axKHOpenAPI.GetCommRealData(item_cd, 27);
                string 최우선매수호가 = axKHOpenAPI.GetCommRealData(item_cd, 28);

                string 거래대금증감 = axKHOpenAPI.GetCommRealData(item_cd, 29);
                string 전일거래량대비비율 = axKHOpenAPI.GetCommRealData(item_cd, 30);
                string 거래회전율 = axKHOpenAPI.GetCommRealData(item_cd, 31);
                string 거래비용 = axKHOpenAPI.GetCommRealData(item_cd, 32);
                string 체결강도 = axKHOpenAPI.GetCommRealData(item_cd, 228);
                string 시가총액 = axKHOpenAPI.GetCommRealData(item_cd, 311);
                string 장구분 = axKHOpenAPI.GetCommRealData(item_cd, 290);


                string str = item_cd;
                str += "체결시간 : " + 체결시간;
                str += "현재가 : " + 현재가;
                str += "전일대비 : " + 전일대비;
                str += "등락율 : " + 등락율;
                str += "시가 : " + 시가;
                str += "고가 : " + 고가;
                str += "저가 : " + 저가;
                str += "거래량 : " + 거래량;
                str += "체결강도 : " + 체결강도;
                str += "장구분 : " + 장구분;
                str += "최우선매도호가 : " + 최우선매도호가;
                str += "최우선매수호가 : " + 최우선매수호가;
                str += "누적거래량 : " + 누적거래량;
                str += "누적거래대금 : " + 누적거래대금;
                str += "전일대비기호 : " + 전일대비기호;
                str += "전일거래량대비 : " + 전일거래량대비;
                str += "거래대금증감 : " + 거래대금증감;
                str += "전일거래량대비비율 : " + 전일거래량대비비율;
                str += "거래회전율 : " + 거래회전율;
                str += "거래비용 : " + 거래비용;
                str += "시가총액 : " + 시가총액;
                */
                string itemCode = e.sRealKey.Trim();
                string price = axKHOpenAPI.GetCommRealData(itemCode, 10);
                string lowPrice = axKHOpenAPI.GetCommRealData(itemCode, 18);
                string openPrice = axKHOpenAPI.GetCommRealData(itemCode, 16);
                string highPrice = axKHOpenAPI.GetCommRealData(itemCode, 17);

                foreach (RealOrder order in realOrderList)
                {
                    if(itemCode.Equals(order.ItemCode))
                    {
                        order.PriceType = PriceType.시장가;
                        // order.Price = 0;
                        // SendOrder(order);
                        Console.WriteLine($"실시간 감시: 종목코드:{itemCode} | order.Price:{order.Price} | 현재가:{price} [{DateTime.Now.ToString()}]");
                    }
                }
                //string ymdhm = DateTime.Now.ToString("yyyyMMdd") + 체결시간;
                /*
                ChartData chart = new ChartData(Math.Abs(double.Parse(고가)), Math.Abs(double.Parse(저가)), Math.Abs(double.Parse(시가))
                    , Math.Abs(double.Parse(현재가)), Math.Abs(long.Parse(거래량)), ymdhm);
                    */
                // dbm.mergeMfBunPrc(item_cd, "noname", chart);
                // SendLogMessage(str);
            }
        }

        private void setRealRegNew(string itemCodes)
        {
            string fids = "9001;302;10;11;25;12;13";
            axKHOpenAPI.SetRealReg(ScreenNo.실시간등록, itemCodes, fids, "0");
        }
        private void setRealRegAppend(string itemCodes)
        {
            string fids = "9001;302;10;11;25;12;13";
            axKHOpenAPI.SetRealReg(ScreenNo.실시간등록, itemCodes, fids, "1");
        }

        public void RequestRealReg()
        {
            Task requestTask = new Task(() =>
            {
                // realReg
                string itemCodes = "086280;064960;028050;020150;012450;011070;005380;000990;000270;009540;267250";
                string fids = "20;10;11;12;13;14;15;16;17;18;25;26;27;28;29;30;31;32;228;311;290";
                axKHOpenAPI.SetRealReg(ScreenNo.실시간등록, itemCodes, fids, "0");

                //실시간 해지 
                //axKHOpenAPI.SetRealRemove("ALL", "ALL");
                /*
                    OpenAPI.SetRealRemove("0150", "039490");  // "0150"화면에서 "039490"종목해지
                    OpenAPI.SetRealRemove("ALL", "ALL");  // 모든 화면에서 실시간 해지
                    OpenAPI.SetRealRemove("0150", "ALL");  // 모든 화면에서 실시간 해지
                    OpenAPI.SetRealRemove("ALL", "039490");  // 모든 화면에서 실시간 해지
                */
            });

            requestTrDataManager.RequestTrData(requestTask);
        }

        public void RequestItemInfo(string itemCode) //주식기본정보요청 : opt10001
        {
            Task requestItemInfoTask = new Task(() =>
            {
                axKHOpenAPI.SetInputValue("종목코드", itemCode);
                string srcNo = ScreenNo.주식기본정보요청;
                string trName = "opt10001";
                int result = axKHOpenAPI.CommRqData(RqName.주식기본정보요청, trName, 0, srcNo);

                if (result == ErrorCode.정상처리)
                {
                    Console.WriteLine("주식기본정보요청 성공");
                    dbm.insertActH(RqName.주식기본정보요청, trName, srcNo, "RequestItemInfo", itemCode);
                }
                else
                {
                    Console.WriteLine("주식기본정보요청 실패");
                }
            });

            requestTrDataManager.RequestTrData(requestItemInfoTask);
        }

        public void RequestTickChartData(string itemCode, string tickUnit, bool isContinue) //주식틱차트조회 : opt10079
        {
            Task requestChartDataTask = new Task(() =>
            {
                axKHOpenAPI.SetInputValue("종목코드", itemCode);
                axKHOpenAPI.SetInputValue("팀범위", tickUnit);
                axKHOpenAPI.SetInputValue("수정주가구분", "0");
                int result = -1;
                const string STrCode = "opt10079";
                if (isContinue) //연속조회여부 
                {
                    result = axKHOpenAPI.CommRqData(RqName.주식틱차트조회요청, STrCode, 2, ScreenNo.주식틱차트조회요청);
                }
                else
                {
                    result = axKHOpenAPI.CommRqData(RqName.주식틱차트조회요청, STrCode, 0, ScreenNo.주식틱차트조회요청);
                }

                if (result == ErrorCode.정상처리)
                {
                    Console.WriteLine("주식틱차트조회요청 성공");
                    dbm.insertActH(RqName.주식틱차트조회요청, STrCode, ScreenNo.주식틱차트조회요청, "RequestTickChartData", itemCode);
                }
                else
                {
                    Console.WriteLine("주식틱차트조회요청 실패");
                }
            });

            requestTrDataManager.RequestTrData(requestChartDataTask);
        }

        public void RequestDayChartData(string itemCode, string ymd)
        {
            Task requestChartDataTask = new Task(() =>
            {
                axKHOpenAPI.SetInputValue("종목코드", itemCode);

                if(ymd.Length >0)
                    axKHOpenAPI.SetInputValue("기준일자", ymd);
                else
                    axKHOpenAPI.SetInputValue("기준일자", DateTime.Now.ToString("yyyyMMdd"));

                axKHOpenAPI.SetInputValue("수정주가구분", "0");

                const string STrCode = "opt10081";
                string sScreenNo = ScreenNo.주식일봉차트조회요청;
                int result = axKHOpenAPI.CommRqData(RqName.주식일봉차트조회요청, STrCode, 0, sScreenNo);

                if (result == ErrorCode.정상처리)
                {
                    Console.WriteLine("주식일봉차트조회요청 성공");
                    dbm.insertActH(RqName.주식일봉차트조회요청, STrCode, sScreenNo, "RequestDayChartData", itemCode);
                }
                else
                {
                    Console.WriteLine("주식일봉차트조회요청 실패");
                }
            });

            requestTrDataManager.RequestTrData(requestChartDataTask);
        }

        public void RequestMinuteChartData(string itemCode, string tickType, string rqName=RqName.주식분봉차트조회요청)
        {
            Task requestChartDataTask = new Task(() =>
            {
                axKHOpenAPI.SetInputValue("종목코드", itemCode);
                axKHOpenAPI.SetInputValue("틱범위", tickType);
                axKHOpenAPI.SetInputValue("수정주가구분", "0");
                const string STrCode = "opt10080";
                string sScreenNo = ScreenNo.주식분봉차트조회요청;
                int result = axKHOpenAPI.CommRqData(rqName, STrCode, 0, sScreenNo);

                if (result == ErrorCode.정상처리)
                {
                    Console.WriteLine(rqName + " 성공");
                    // dbm.insertActH(rqName, STrCode, sScreenNo, "RequestMinuteChartData", itemCode);
                }
                else
                {
                    Console.WriteLine(rqName + " 실패");
                }
            });

            requestTrDataManager.RequestTrData(requestChartDataTask);
        }

        public void RequestAccountBalance(string account)
        {
            Task requestAccountBalanceTask = new Task(() =>
            {
                axKHOpenAPI.SetInputValue("계좌번호", account);
                axKHOpenAPI.SetInputValue("비밀번호", "");
                axKHOpenAPI.SetInputValue("상장폐지조회구분", "0");
                axKHOpenAPI.SetInputValue("비밀번호입력매체구분", "00");

                const string rqName = RqName.계좌평가현황요청;
                const string STrCode = "OPW00004";
                string sScreenNo = ScreenNo.계좌평가현황요청;
                int result = axKHOpenAPI.CommRqData(rqName, STrCode, 0, sScreenNo);

                if (result == ErrorCode.정상처리)
                {
                    Console.WriteLine("계좌평가현황요청 성공");
                    dbm.insertActH(rqName, STrCode, sScreenNo, "RequestAccountBalance", account);
                }
                else
                {
                    Console.WriteLine("계좌평가현황요청 실패");
                }
            });

            requestTrDataManager.RequestTrData(requestAccountBalanceTask);
        }

        public void RequestOutstandingOrderList()
        {
            Task requestTask = new Task(() =>
            {
                // 미체결내역 요청
                axKHOpenAPI.SetInputValue("계좌번호", userInfo.Accounts[0]);
                axKHOpenAPI.SetInputValue("체결구분", "1"); //미체결데이터만 요청
                axKHOpenAPI.SetInputValue("매매구분", "0");  //전체
                int result = axKHOpenAPI.CommRqData(RqName.실시간미체결요청, "opt10075", 0, ScreenNo.실시간미체결요청);

                if (result == ErrorCode.정상처리)
                {
                    Console.WriteLine("실시간미체결요청 성공");
                }
                else
                {
                    Console.WriteLine("실시간미체결요청 실패");
                }
            });

            requestTrDataManager.RequestTrData(requestTask);
        }

        public void RequestHighTodayVolume(bool isAutomatic,string marketType, string sortBy="3")
        {
            Task requestTask = new Task(() =>
            {
                string _mt = "";
                switch (marketType)
                {
                    case "전체":
                        _mt = "000";
                        break;
                    case "코스피":
                        _mt = "001";
                        break;
                    case "코스닥":
                        _mt = "101";
                        break;
                    default:
                        break;
                }
                axKHOpenAPI.SetInputValue("시장구분", _mt);
                axKHOpenAPI.SetInputValue("정렬구분", sortBy);  // 1: 거래량, 2: 거래회전율, 3: 거래대금
                axKHOpenAPI.SetInputValue("관리종목포함", "5"); // 증거금100 제외
                axKHOpenAPI.SetInputValue("신용구분", "0");
                axKHOpenAPI.SetInputValue("거래량구분", "0");
                axKHOpenAPI.SetInputValue("가격구분", "0");
                axKHOpenAPI.SetInputValue("거래대금구분", "0");
                axKHOpenAPI.SetInputValue("장운영구분", "0");

                string rqName = (isAutomatic) ? RqName.자동매매당일거래상위요청 : RqName.당일거래량상위요청;
                const string STrCode = "opt10030";
                string sScreenNo = (isAutomatic) ? ScreenNo.자동매매당일거래상위요청 : ScreenNo.당일거래량상위요청;
                int result = axKHOpenAPI.CommRqData(rqName, STrCode, 0, sScreenNo);

                if (result == ErrorCode.정상처리)
                {
                    Console.WriteLine($"{rqName} 성공" + DateTime.Now.ToString());
                    // dbm.insertActH(rqName, STrCode, sScreenNo, "RequestHighTodayVolume", "");
                }
                else
                {
                    Console.WriteLine($"{rqName} 실패" + DateTime.Now.ToString());
                }
            });

            requestTrDataManager.RequestTrData(requestTask);
        }

        public void RequestHighVolume0186(bool isAutomatic, string marketType)
        {
            Task requestTask = new Task(() =>
            {
                string _mt = "";
                switch (marketType)
                {
                    case "전체":
                        _mt = "000";
                        break;
                    case "코스피":
                        _mt = "001";
                        break;
                    case "코스닥":
                        _mt = "101";
                        break;
                    default:
                        break;
                }
                axKHOpenAPI.SetInputValue("시장구분", _mt);
                axKHOpenAPI.SetInputValue("관리종목포함", "0"); // 관리종목 미포함

                string rqName = (isAutomatic) ? RqName.자동매매거래대금상위요청 : RqName.거래대금상위요청;
                const string STrCode = "opt10032";
                string sScreenNo = (isAutomatic) ? ScreenNo.자동매매거래대금상위요청 : ScreenNo.거래대금상위요청;
                int result = axKHOpenAPI.CommRqData(rqName, STrCode, 0, sScreenNo);

                if (result == ErrorCode.정상처리)
                {
                    Console.WriteLine($"{rqName} 성공");
                    // dbm.insertActH(rqName, STrCode, sScreenNo, "RequestHighTodayVolume", "");
                }
                else
                {
                    Console.WriteLine($"{rqName} 실패");
                }
            });

            requestTrDataManager.RequestTrData(requestTask);
        }

        public void RequestHighYesterDayVolume(string marketType, string sortBy = "2")
        {
            Task requestTask = new Task(() =>
            {
                string _mt = "";
                switch (marketType)
                {
                    case "전체":
                        _mt = "000";
                        break;
                    case "코스피":
                        _mt = "001";
                        break;
                    case "코스닥":
                        _mt = "100";
                        break;
                    default:
                        break;
                }
                axKHOpenAPI.SetInputValue("시장구분", _mt);
                axKHOpenAPI.SetInputValue("조회구분", sortBy);  // 조회구분 = 1:전일거래량 상위100종목, 2:전일거래대금 상위100종목
                axKHOpenAPI.SetInputValue("순위시작", "0"); // 증거금100 제외
                axKHOpenAPI.SetInputValue("순위끝", "100");

                const string sRqName = RqName.전일거래량상위요청;
                const string STrCode = "opt10031";
                string sScreenNo = ScreenNo.전일거래량상위요청;
                int result = axKHOpenAPI.CommRqData(sRqName, STrCode, 0, sScreenNo);

                if (result == ErrorCode.정상처리)
                {
                    Console.WriteLine("전일거래량상위요청 성공");
                    dbm.insertActH(sRqName, STrCode, sScreenNo, "RequestHighYesterDayVolume", "");
                }
                else
                {
                    Console.WriteLine("전일거래량상위요청 실패");
                }
            });

            requestTrDataManager.RequestTrData(requestTask);
        }

        public void RequestAutoTrading()
        {
            axKHOpenAPI.SetRealRemove("ALL", "ALL");
            List<StockItem> autoStockList = dbm.getMfAtItem(getYmd());
            foreach (StockItem item in autoStockList)
            {
                RequestMinuteChartData(item.ItemCode, ChartMinuteType, RqName.자동매매분봉차트요청);
                Console.WriteLine("자동매매 timer 시작 RequestMinuteChartData ::: " + item.ItemCode + "[" + DateTime.Now.ToString() + "]");
            }            
        }

        /*** 조건검색 관련 ***/
        public void RequestCondition(bool isRealCondition)
        {
            IsRealCondition = isRealCondition;
            Task requestTask = new Task(() =>
            {
                int result = axKHOpenAPI.GetConditionLoad();
                if(result == 1)
                {
                    Console.WriteLine("사용자 조건검색 요청 성공");
                } else
                {
                    Console.WriteLine("[Fail] 사용자 조건검색 요청 실패");
                }
            });
            requestTrDataManager.RequestTrData(requestTask);
        }

        private void AxKHOpenAPI_OnReceiveConditionVer(object sender, _DKHOpenAPIEvents_OnReceiveConditionVerEvent e)
        {
            List<Condition> conditionList = new List<Condition>();
            string conditionNames = axKHOpenAPI.GetConditionNameList();
            string[] conditionArray = conditionNames.Split(';');
            foreach (string condition in conditionArray)
            {
                if (condition.Length > 0)
                {
                    string[] names = condition.Split('^');
                    Condition ci = new Condition(int.Parse(names[0]), names[1]);
                    conditionList.Add(ci);
                }
            }
            OnReceiveConditionVer?.Invoke(this, new OnReceiveConditionVerEventArgs(conditionList));
        }
        private void AxKHOpenAPI_OnReceiveRealCondition(object sender, _DKHOpenAPIEvents_OnReceiveRealConditionEvent e)
        {
            Condition condition = new Condition(int.Parse(e.strConditionIndex), e.strConditionName);
            ConditionItem conditionItem = new ConditionItem(e.sTrCode, e.strType, condition);
            OnReceiveRealCondition?.Invoke(this, new OnReceiveConditionItemEventArgs(conditionItem));
        }
        private void AxKHOpenAPI_OnReceiveTrCondition(object sender, _DKHOpenAPIEvents_OnReceiveTrConditionEvent e)
        {
            List<string> itemList = new List<string>();
            string strCodeList = e.strCodeList;
            string[] strCodeArray = strCodeList.Split(';');
            foreach (string itemCode in strCodeArray)
            {
                if (itemCode.Length > 0)
                {
                    itemList.Add(itemCode);
                }
            }
            Condition condition = new Condition(e.nIndex, e.strConditionName);
            ConditionItemList ciList = new ConditionItemList(itemList, condition);
            OnReceiveTrCondition?.Invoke(this, new OnReceiveConditionItemListEventArgs(ciList));
        }

        //관심종목 관련
        public void CreateNewGroup() //새 관심 그룹 생성
        {
            Group group = new Group();
            group.Name = "새 그룹" + group._ID;

            groupList.Add(group);
            OnCreatedNewGroup?.Invoke(this, new OnCreatedNewGroupEventArgs(group));
            SendLogMessage("새 그룹 생성 " + group._ID);
        }

        public void RemoveGroup(int groupID) //선택 관심 그룹 삭제
        {
            Group group = groupList.Find(o => o._ID == groupID);
            if (group != null)
            {
                groupList.Remove(group);
                OnRemovedGroup?.Invoke(this, new OnRemovedGroupEventArgs(group));
                SendLogMessage("선택 그룹 삭제 " + group._ID);
            }
            else
                SendLogMessage("그룹 삭제 실패 : 존재하지 않는 그룹");
        }

        public void ChangeGroupName(Group group, string groupName)
        {
            if (group != null)
            {
                string originalName = group.Name;
                group.Name = groupName;
                OnChangedGroupName?.Invoke(this, new OnChangedGroupNameEventArgs(group));
                SendLogMessage("그룹명 변경 : " + originalName + " -> " + group.Name);
            }
        }

        public void AddInterestItem(Group group, StockItem stockItem) //그룹에 관심 종목 추가
        {
            if (group != null)
            {
                StockItem item = group.InterestItemList.Find(o => o.ItemCode.Equals(stockItem.ItemCode));
                if (item == null)
                {
                    group.InterestItemList.Add(stockItem);
                    OnAddInterestItem?.Invoke(this, new OnAddInterestItemEventArgs(group, stockItem));
                    SendLogMessage("관심 등록 성공 : " + stockItem.ItemName);
                }
                else
                {
                    SendLogMessage("관심 등록 실패 : 이미 등록된 종목 - " + stockItem.ItemName);
                }
            }
        }

        public void RemoveInterestItem(Group group, StockItem stockItem) //그룹에서 관심 종목 제거
        {
            if (group != null)
            {
                if (group.InterestItemList.Contains(stockItem))
                {
                    group.InterestItemList.Remove(stockItem);
                    OnRemoveInterestItem?.Invoke(this, new OnRemoveInterestItemEventArgs(group, stockItem));
                    SendLogMessage("관심 삭제 성공 : " + stockItem.ItemName);
                }
                else
                {
                    SendLogMessage("관심 종목 삭제 실패 : 그룹에 존재하지 않는 종목 - " + stockItem.ItemName);
                }
            }
        }

        public void SaveInterestListToFile() //파일에 관심 목록 저장
        {
            if (groupList != null)
                FileIOManager.SaveInterestGroupList(groupList);
        }

        public void LoadInterestListFromFile() //파일로부터 관심 목록 로딩
        {
            groupList = FileIOManager.LoadInterestGroupList(); //파일로부터 그룹리스트 호출
            if (groupList == null) //저장된 데이터가 없을 경우
                groupList = new List<Group>();

            foreach (Group group in groupList) //마지막 그룹번호 동기화
            {
                Group.lastID = Math.Max(group._ID, Group.lastID);
            }
        }

        private void SendLogMessage(string logMessage) //Event를 이용해 로그 메세지 전달
        {
            logMessage = DateTime.Now.ToString("[HH:mm:ss] ") + logMessage;
            OnReceivedLogMessage?.Invoke(this, new OnReceivedLogMessageEventArgs(logMessage));
        }

        public bool IsLogin()
        {
            if (axKHOpenAPI.GetConnectState() == 1)
                return true;
            else
                return false;
        }

    }
}
