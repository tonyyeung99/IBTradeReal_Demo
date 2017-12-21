using IBTradeRealTime.app;
using IBTradeRealTime.MarketData;
using IBTradeRealTime.Strategy;
using IBTradeRealTime.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.StrategyImpl
{
    abstract public class BTStrategy : AbstractStrategy
    {
        protected AppStrategyManager stgManager;

        protected Dictionary<String, String> inputArgs;

        public BTStrategy(String stgName, int stgIndex)
            : base(stgName, stgIndex)
        {

        }

        public override bool isStrategyCanStop()
        {
            return false;
        }

        public override void init(Object caller, Dictionary<String, String> inputArgs, TickerInfo info)
        {
            stgManager = (AppStrategyManager)caller;
            this.inputArgs = inputArgs;
            foreach (KeyValuePair<String, String> item in inputArgs)
            {
            }
        }

        public override void calculate_signals(Object sender, MarketDataEventArgs args)
        {

        }


        public override void closeYTDTrade()
        {
        }

        public override void stgDailyReset()
        {
        }


        protected double getValidProfit(double exitPrice, MarketDataElement data, String buySell)
        {
            if (exitPrice >= data.low && exitPrice <= data.high)
                return exitPrice;

            if (AppConstant.SELL_SIGNAL.Equals(buySell))
            {

                if (exitPrice <= data.low && exitPrice <= data.high)
                {
                    return data.open;
                }
            }
            else
            {
                if (exitPrice >= data.low && exitPrice >= data.high)
                {
                    return data.open;
                }
            }

            return exitPrice;
        }

        protected double getValidCutLossPt(double exitPrice, MarketDataElement data, String buySell)
        {
            if (AppConstant.BUY_SIGNAL.Equals(buySell))
            {
                if (exitPrice <= data.low)
                {
                    return data.low;
                }
            }
            else
            {
                if (exitPrice >= data.high)
                {
                    return data.high;
                }
            }
            return exitPrice;
        }
    }
}
