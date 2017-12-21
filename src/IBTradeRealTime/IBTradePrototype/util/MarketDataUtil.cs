using IBTradeRealTime.MarketData;
using IBTradeRealTime.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.util
{
    public class MarketDataUtil
    {
        public static DateTime getStartTime(DateTime currentTime)
        {
            return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 0, 0, 0);
        }

        public static MarketDataElement createHLOC(DateTime time, String elementType, String symbol, String contractType)
        {
            MarketDataElement tempObj = new MarketDataElement();
            tempObj.time = time;
            tempObj.elementType = elementType;
            tempObj.symbol = symbol;
            tempObj.contractType = contractType;
            return tempObj;
        }

        public static void setHLOC(MarketDataElement data, double high, double low, double open, double close)
        {
            data.high = high;
            data.low = low;
            data.open = open;
            data.close = close;
        }


        public static Boolean isThisMessageEndOfMinute(IBMessage message)
        {
            RealTimeBarMessage rtBar = (RealTimeBarMessage)message;
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime current = start.AddMilliseconds(rtBar.Timestamp * 1000).ToLocalTime();
            if (current.Second == 55)
                return true;
            return false;
        }

        public static Boolean isThisMessageStartOfMinute(IBMessage message)
        {
            RealTimeBarMessage rtBar = (RealTimeBarMessage)message;
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime current = start.AddMilliseconds(rtBar.Timestamp * 1000).ToLocalTime();
            if (current.Second == 0)
                return true;
            return false;
        }

        public static MarketDataElement convertBarToMarketDataElement(RTDataBar bar)
        {
            double h = bar.high;
            double l = bar.low;
            double o = bar.open;
            double c = bar.close;
            double vol = bar.volume;
            DateTime time = bar.time;
            MarketDataElement currentHsiData = createHLOC(time, "HLOC", "HSI", "FUT");
            currentHsiData.volume = vol;
            currentHsiData.time = time;
            MarketDataUtil.setHLOC(currentHsiData, h, l, o, c);
            return currentHsiData;
        }
        /*
        public static void setHLOC(MarketDataElement data, double high, double low, double open, double close)
        {
            data.high = high;
            data.low = low;
            data.open = open;
            data.close = close;
        }*/
    }
}
