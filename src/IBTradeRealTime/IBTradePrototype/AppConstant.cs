using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime
{
    public class AppConstant
    {
        public static  Object OrderRepositoryLock = new Object(); 
        public const Boolean USE_OUT_REGULAR_DATA = false;

        public const String SEPERATOR = ",";
        public const String WTS_TRADES = "TRADES";
        public const String WTS_MIDPOINT = "MIDPOINT";

        public static int PRICE_FEED_BACK_DATE_LIMIT = 7; //DAYS
        public static String BUY_SIGNAL = "BUY";
        public static String SELL_SIGNAL = "SELL";

        public static String EXE_BUY_SIGNAL = "BOT";
        public static String EXE_SELL_SIGNAL = "SLD";

        public static String ORDER_TYPE_STOP = "STOP";
        public static String ORDER_TYPE_LIMIT= "LMT";
        public static String ORDER_TYPE_MARKET = "MKT";

        public static String DATA_OPEN = "DATA_OPEN";
        public static String DATA_CLOSE = "DATA_CLOSE";
        public static String DATA_HIGH = "DATA_HIGH";
        public static String DATA_LOW = "DATA_LOW";
        //new constant
        public static String RUNNING_STATUS_RUNNING = "RUNNING_STATUS_RUNNING";
        public static String RUNNING_STATUS_UNKOWN = "RUNNING_STATUS_UNKOWN";
        public static String RUNNING_STATUS_STOP = "RUNNING_STATUS_STOP";
        public static String TREND_RISE = "TREND_RISE";
        public static String TREND_FALL = "TREND_FALL";

        public const String METRIC_POSITION_BUY = "BUY";
        public const String METRIC_POSITION_SELL = "SELL";
        public const String METRIC_POSITION_ALL = "ALL";

        public const String RESULT_ITEM_PNL = "PNL";
        public const String RESULT_ITEM_TOTAL_PROFIT = "TOTAL_PROFIT";
        public const String RESULT_ITEM_TOTAL_LOSS = "TOTAL_LOSS";
        public const String RESULT_ITEM_PROFIT_OVER_LOSS = "PROFIT_OVER_LOSS";
        public const String RESULT_ITEM_NUM_TRADES = "NUM_TRADES";
        public const String RESULT_ITEM_PERCENT_WIN = "PERCENT_WIN";
        public const String RESULT_ITEM_WIN_TRADES = "WIN_TRADES";
        public const String RESULT_ITEM_LOSE_TRADES = "LOSE_TRADES";
        public const String RESULT_ITEM_EVEN_TRADES = "EVEN_TRADES";
        public const String RESULT_ITEM_TRADE_EXPECTANCY = "TRADE_EXPECTANCY";
        public const String RESULT_ITEM_AVG_PROFIT_WIN = "AVG_PROFIT_WIN";
        public const String RESULT_ITEM_AVG_LOSS_LOSE = "AVG_LOSS_LOSE";
        public const String RESULT_ITEM_STD_DEV_PNL = "STD_DEV_PNL";
        public const String RESULT_ITEM_HIGHEST_PROFIT = "HIGHEST_PROFIT";
        public const String RESULT_ITEM_HIGHEST_LOSS = "HIGHEST_LOSS";
        public const String RESULT_ITEM_AVG_TIME_BEW_TRADES = "AVG_TIME_BEW_TRADES";
        public const String RESULT_ITEM_AVG_TIME_IN_MKT = "AVG_TIME_IN_MKT";
        public const String RESULT_ITEM_AVG_TIME_ON_WIN = "AVG_TIME_ON_WIN";
        public const String RESULT_ITEM_AVG_TIME_ON_LOSE = "AVG_TIME_ON_LOSE";
        public const String RESULT_ITEM_AVG_TIME_ON_EVEN = "AVG_TIME_ON_EVEN";
        public const String RESULT_ITEM_PERCENT_IN_MKT = "PERCENT_IN_MKT";
        public const String RESULT_ITEM_BROKERAGE_FEE = "BROKERAGE_FEE";
        public const String RESULT_ITEM_HIGHEST_NUM_CWIN = "HIGHEST_NUM_CWIN";
        public const String RESULT_ITEM_HIGHEST_NUM_CLOSE = "HIGHEST_NUM_CLOSE";
        public const String RESULT_ITEM_DRAWNDOWN = "DRAWNDOWN";
        //public const String RESULT_ITEM_HIGHEST_GAIN = "HIGHEST_GAIN";
        public const String RESULT_ITEM_RETURN_CAPITAL = "RETURN_CAPITAL";

        public const String RESULT_SEPERATOR = ",";
        public const String RESULT_HEADER_TESTRUN_ID = "TEST_RUN_ID";
        public const String RESULT_HEADER_STRATEGY = "STRATEGY";
        public const String RESULT_HEADER_TRADE_TYPE = "TRADE_TYPE";

        public const String RESULT_POS_HEADER_TIME = "TIME";
        public const String RESULT_POS_HEADER_ENTER_EXIT = "ENTER_OR_EXIT";
        public const String RESULT_POS_HEADER_BUY_SELL = "BUY_OR_SELL";
        public const String RESULT_POS_HEADER_CLOSE = "CLOSE_PRICE";
        public const String RESULT_POS_HEADER_PNL = "PNL";
        public const String RESULT_POS_HEADER_REASON = "REASON";


        public const String PARATYPE_CALLER_TESTCASE = "PARATYPE_CALLER_TESTCASE";
        public const String CONTRACT_EXCHANGE_HKEX = "HKEX";

        public const String STG1_SHORT_NAME = "S1_RND1";
        public const String STG2_SHORT_NAME = "S2_RBREAK_REVERSE1";
        public const String STG3_SHORT_NAME = "S3_RBREAK_TREND1";
        public const String STG4_SHORT_NAME = "S4_RND4";
        static public int[] STG_NUM_PARAS = { 4, 4, 4, 4 };
        /*
        public const String STG1_SHORT_NAME = "S1_RND1";
        public const String STG2_SHORT_NAME = "S2_TF1";
        public const String STG3_SHORT_NAME = "S3_MR1";
        public const String STG4_SHORT_NAME = "S4_MOM1";
        public const String STG5_SHORT_NAME = "S5_STAT1";
        public const String STG6_SHORT_NAME = "S6_RBREAK1";
        public const String STG7_SHORT_NAME = "S7_RBREAKTREND2";

        static public int[] STG_NUM_PARAS = { 4, 5, 2, 4, 4, 4, 4 };
         */ 

        public const String TICK1_NAME = "EUR DEMO";
        public const String TICK1_ID = "";
        public const String TICK1_CONTRACT_ID = "";
        public const String TICK1_SYMBOL = "EUR";
        public const String TICK1_TYPE = "CASH";
        public const String TICK1_EXCHANGE = "IDEALPRO";
        public const String TICK1_P_EXCHANGE = "";
        public const String TICK1_CURRENCY = "USD";
        public const String TICK1_L_SYMBOL = "EUR.USD";
        public const String TICK1_WTS = "MIDPOINT";
        //public const String TICK1_START_TIME = "05:00:00";
        public const String TICK1_START_TIME = "00:00:00";
        public const String TICK1_LUNCH_TIME_END = "10:00:00";
        public const String TICK1_END_TIME = "23:00:00";

        public const String TICK2_NAME = "HSI";
        public const String TICK2_ID = "";
        public const String TICK2_CONTRACT_ID = "";
        public const String TICK2_SYMBOL = "HSI";
        public const String TICK2_TYPE = "FUT";
        public const String TICK2_EXCHANGE = "HKFE";
        public const String TICK2_P_EXCHANGE = "";
        public const String TICK2_CURRENCY = "HKD";
        public const String TICK2_L_SYMBOL = "HSIK16";
        public const String TICK2_WTS = "TRADES";
        public const String TICK2_START_TIME = "09:15:00";
        public const String TICK2_LUNCH_TIME_END = "13:00:00";
        public const String TICK2_END_TIME = "16:15:00";

        public static DateTime INVALID_TIME = new DateTime(1970, 1, 1);

        //public const String FILE_STG_ROOT = @"D:\trade_data\AlgoEdge\";
        //public const String FILE_STG_MOMENTUM = @"D:\trade_data\AlgoEdge\StrategyMomentum.properties";
        //public const String FILE_RESULT = @"D:\trade_data\logs\RESULT";
        // public const String FILE_POS_RESULT = @"D:\trade_data\logs\POS";
        // public const String FILE_PAF_RESULT = @"D:\trade_data\logs\PAF";
        //public const String FILE_ORDER_REP = @"D:\trade_data\logs\IBTradeApp\OrderRepositry.csv";
        //public const String FILE_POSITION_RESULT_PREFIX = @"D:\trade_data\logs\IBTradeApp\PositionResult_";

        public static String FILE_ORDER_REP = "";
        public static String FILE_POSITION_RESULT_PREFIX = "";
        public const String FILE_POSITION_RESULT_EXT = ".csv";

        public const String EMAIL_SENDER = "tonydothk@gmail.com";
        public const String EMAIL_RECEIVER = "tonydothk@gmail.com";
        public const String EMAIL_PASSWORD = "si8narna";
        public const String EMAIL_SMTP_SERVER = "smtp.gmail.com";

        public const int HB_EMAIL_ELPASED_TIME = 60;

    }
}
