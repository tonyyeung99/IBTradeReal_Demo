using IBTradeRealTime.MarketData;
using IBTradeRealTime.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.Strategy
{
    public abstract class AbstractStrategy : IStrategy
    {
        protected String stgName;
        protected int stgIndex;

        public AbstractStrategy(String stgName, int stgIndex)
        {
            this.stgName = stgName;
            this.stgIndex = stgIndex;
        }

        public String getName() { return this.stgName; }
        public abstract bool isStrategyCanStop();
        public abstract void init(Object callObject, Dictionary<String, String> intArgs, TickerInfo info);
        public abstract void calculate_signals(Object sender, MarketDataEventArgs arg);
        public abstract void closeYTDTrade();
        public abstract void stgDailyReset();
    }
}
