using AxKHOpenAPILib;
using MetelStockLib.db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
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
        public event EventHandler<OnReceivedStockItemsEventArgs> OnReceivedStockItems; //관심종목 제거 시
        public event EventHandler<EventArgs> OnLoged; // 로그온이후 
        public event EventHandler<OnReceivedAutoTrading3MDataEventArgs> OnReceivedAutoTrading3MData; //차트데이터 수신 시

        private static int screenNum = 5000;

        private OpenSystemTrading()
        {
            requestTrDataManager = RequestTrDataManager.GetInstance();
            requestTrDataManager.Run();

            LoadInterestListFromFile(); //저장된 관심 종목 리스트 로딩
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
            //추가 
            axKHOpenAPI.OnReceiveRealData += AxKHOpenAPI_OnReceiveRealData;
        }

        private void AxKHOpenAPI_OnReceiveRealData(object sender, _DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
            if (e.sRealType.Equals("주식체결"))
            {
                // "20;10;11;12;13;14;15;16;17;18;25;26;27;28;29;30;31;32;228;311;290";
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
                string ymdhm = DateTime.Now.ToString("yyyyMMdd") + 체결시간;
                ChartData chart = new ChartData(Math.Abs(double.Parse(고가)), Math.Abs(double.Parse(저가)), Math.Abs(double.Parse(시가))
                    , Math.Abs(double.Parse(현재가)), Math.Abs(long.Parse(거래량)), ymdhm);
                dbm.mergeMfBunPrc(item_cd, "noname", chart);
                SendLogMessage(str);
            }
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
                string itemName = axKHOpenAPI.GetMasterCodeName(itemCode);

                int cnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                List<ChartData> chartDataList = new List<ChartData>();
                for (int i = 0; i < cnt; i++)
                {
                    string time = "";
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

                OnReceivedAutoTrading3MData?.Invoke(this, new OnReceivedAutoTrading3MDataEventArgs(itemCode, itemName, chartDataList, hasNext));
                // dbms 분종 데이터 넣기
                dbm.mergeMfAt3M(itemCode, chartDataList);
                Console.WriteLine("[응답]자동분봉차트요청 mergeMfAt3M작업완료 [" + DateTime.Now.ToString() +"]");
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

                        StockItem stockItem = new StockItem()
                        {
                            ItemCode = itemCode,
                            ItemName = itemName,
                            Price = currentPrice,
                            NetChange = netChange,
                            UpDownRate = updownRate,
                            Volume = volume     
                        };
                        stockList.Add(stockItem);
                    }

                    OnReceivedStockItems?.Invoke(this, new OnReceivedStockItemsEventArgs(e.sRQName, stockList));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            } 
        }

        public void RequestRealReg()
        {
            Task requestTask = new Task(() =>
            {
                // realReg
                string itemCodes = "086280;064960;028050;020150;012450;011070;005380;000990;000270;009540;267250";
                string fids = "20;10;11;12;13;14;15;16;17;18;25;26;27;28;29;30;31;32;228;311;290";
                axKHOpenAPI.SetRealReg(GetScreenNum(), itemCodes, fids, "0");

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
                string srcNo = GetScreenNum();
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
                string sScreenNo = GetScreenNum();
                if (isContinue) //연속조회여부 
                {
                    result = axKHOpenAPI.CommRqData(RqName.주식틱차트조회요청, STrCode, 2, sScreenNo);
                }
                else
                {
                    result = axKHOpenAPI.CommRqData(RqName.주식틱차트조회요청, STrCode, 0, sScreenNo);
                }

                if (result == ErrorCode.정상처리)
                {
                    Console.WriteLine("주식틱차트조회요청 성공");
                    dbm.insertActH(RqName.주식틱차트조회요청, STrCode, sScreenNo, "RequestTickChartData", itemCode);
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
                string sScreenNo = GetScreenNum();
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
                string sScreenNo = GetScreenNum();
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
                string sScreenNo = GetScreenNum();
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

        public void RequestHighTodayVolume(string marketType, string sortBy="3")
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
                axKHOpenAPI.SetInputValue("정렬구분", sortBy);  // 1: 거래량, 2: 거래회전율, 3: 거래대금
                axKHOpenAPI.SetInputValue("관리종목포함", "5"); // 증거금100 제외
                axKHOpenAPI.SetInputValue("신용구분", "0");
                axKHOpenAPI.SetInputValue("거래량구분", "0");
                axKHOpenAPI.SetInputValue("가격구분", "0");
                axKHOpenAPI.SetInputValue("거래대금구분", "0");
                axKHOpenAPI.SetInputValue("장운영구분", "0");

                const string rqName = RqName.당일거래량상위요청;
                const string STrCode = "opt10030";
                string sScreenNo = GetScreenNum();
                int result = axKHOpenAPI.CommRqData(rqName, STrCode, 0, sScreenNo);

                if (result == ErrorCode.정상처리)
                {
                    Console.WriteLine("당일거래량상위요청 성공");
                    dbm.insertActH(rqName, STrCode, sScreenNo, "RequestHighTodayVolume", "");
                }
                else
                {
                    Console.WriteLine("당일거래량상위요청 실패");
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
                string sScreenNo = GetScreenNum();
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

        public static string GetScreenNum()
        {
            if (screenNum >= 9999)
                screenNum = 5000;
            screenNum++;
            return screenNum.ToString();
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
