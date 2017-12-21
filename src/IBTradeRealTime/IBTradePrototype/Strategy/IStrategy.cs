using IBTradeRealTime.MarketData;
using IBTradeRealTime.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.Strategy
{
    public interface IStrategy
    {
        String getName();
        void init(Object callObject, Dictionary<String, String> inputParas, TickerInfo info);
        void calculate_signals(Object sender, MarketDataEventArgs arg);
        bool isStrategyCanStop();
        void stgDailyReset();
    }
}
