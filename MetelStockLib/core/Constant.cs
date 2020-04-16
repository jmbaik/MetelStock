using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetelStockLib.core
{
    public static class RqName
    {
        public const string 주식기본정보요청 = "주식기본정보요청"; //opt10001
        public const string 주식틱차트조회요청 = "주식틱차트조회요청"; //opt10079
        public const string 주식일봉차트조회요청 = "주식일봉차트조회요청"; //opt10081
        public const string 주식분봉차트조회요청 = "주식분봉차트조회요청"; //opt10080
        public const string 계좌평가현황요청 = "계좌평가현황요청"; //OPW00004
        public const string 당일거래량상위요청 = "당일거래량상위요청"; // opt10030
        public const string 자동매매당일거래상위요청 = "자동매매당일거래상위요청"; // opt10030
        public const string 전일거래량상위요청 = "전일거래량상위요청"; // opt10031
        public const string 자동매매분봉차트요청 = "자동매매분봉차트요청"; //opt10080
        public const string 거래대금상위요청 = "거래대금상위요청";  //opt10032
        public const string 자동매매거래대금상위요청 = "자동매매거래대금상위요청";  //opt10032
        public const string 주식주문 = "주식주문";
        public const string 실시간미체결요청 = "실시간미체결요청";

    }

    public static class ErrorCode
    {
        public const int 정상처리 = 0;
        public const int 실패 = -10;
        public const int 사용자정보교환실패 = -100;
        public const int 서버접속실패 = -101;
        public const int 버전처리실패 = -102;
        public const int 개인방화벽실패 = -103;
        public const int 메모리보호실패 = -104;
        public const int 함수입력값오류 = -105;
        public const int 통신연결종료 = -106;
        public const int 시세조회과부하 = -200;
        public const int 전문작성초기화실패 = -201;
        public const int 전문작성입력값오류 = -202;
        public const int 데이터없음 = -203;
        public const int 조회가능한종목수초과 = -204; //한번에 조회 가능한 종목개수는 최대 100종목.        
        public const int 데이터수신실패 = -205;
        public const int 조회가능한FID수초과 = -206; //.한번에 조회 가능한 FID개수는 최대 100개.      
        public const int 실시간해제오류 = -207;
        public const int 입력값오류 = -300;
        public const int 계좌비밀번호없음 = -301;
        public const int 타인계좌사용오류 = -302;
        public const int 주문가격이20억원을초과 = -303;
        public const int 주문가격이50억원을초과 = -304;
        public const int 주문수량이총발행주수의1퍼센트초과오류 = -305;
        public const int 주문수량은총발행주수의3퍼센트초과오류 = -306;
        public const int 주문전송실패 = -307;
        public const int 주문전송과부하 = -308;
        public const int 주문수량300계약초과 = -309;
        public const int 주문수량500계약초과 = -310;
        public const int 주문전송제한과부하 = -311;
        public const int 계좌정보없음 = -340;
        public const int 종목코드없음 = -500;
    }
}
