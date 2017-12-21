using Deedle;
using IBTradeRealTime.app;
using IBTradeRealTime.AppOrders;
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
    abstract public class AdvStrategy : BTStrategy
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected double bid = 0.0;
        protected double ask = 0.0;
        protected double lastExecutePrice ;
        protected double lastSignalPrice;
        protected double immediatePnLAfterPositionClose;
        protected Boolean immediateOrderExit;
        protected Boolean immediateOrderEnter;

        protected double currentOpenPrice = 0;
        protected double currentDayHigh = 0;
        protected double currentDayLow = double.MinValue;

        protected IAppEventManager eventManager { get; set; }
        protected IAppOrderManager orderManager { get; set; }
        protected IAppMDManager MDManager { get; set; }
        protected SignalContext execution;
        protected StrategyHelper stgHelper;
        protected String WTS = "";

        public AdvStrategy(String strategyName, int stgIndex)
            : base(strategyName, stgIndex)
        {
        }

        public override void init(Object caller, Dictionary<String, String> inputArgs, TickerInfo info)
        {
            stgManager = (AppStrategyManager)caller;
            execution = new SignalContext();
            eventManager = stgManager.getAppEventManager();
            MDManager = stgManager.getAppMDManager();
            stgHelper = new StrategyHelper(stgManager, stgName, stgIndex, execution);
            WTS = info.whatToShow;
        }

        public override void stgDailyReset()
        {
        }

        public void dailyReset(){
            bid = 0.0;
            ask = 0.0;
            lastExecutePrice = 0.0;
            lastSignalPrice = 0.0;
            immediatePnLAfterPositionClose = 0.0; ;
            immediateOrderExit = false;
            immediateOrderEnter = false;
            currentOpenPrice = 0;
            currentDayHigh = 0;
            currentDayLow = double.MinValue;
            //execution = new SignalContext();
            execution.reset();
        }
        
        public override void calculate_signals(Object sender, MarketDataEventArgs args)
        {
            pre_calculate_signals(sender, args);
            calculate_signals_impl(sender, args);
            post_calculate_signals(sender, args);
        }

        private void pre_calculate_signals(Object sender, MarketDataEventArgs args)
        {
            immediatePnLAfterPositionClose = 0;
            immediateOrderExit = false;
            immediateOrderEnter = false;
        }
        private void post_calculate_signals(Object sender, MarketDataEventArgs args)
        {
        }
        abstract public void calculate_signals_impl(Object sender, MarketDataEventArgs args);
 

        protected void checkExitOrderCompleted(Series<DateTime, MarketDataElement> series)
        {
            if (!execution.isPositionOnSignalOnClose())
                return;

            immediatePnLAfterPositionClose = 0;
            int index = execution.getIndexClosedAppOrder();

            log.Info("[Strategy] : Exit Strategy Executed!!!");
            if (index == 1)
            {
                log.Info("Executed Rule = " + execution.PendingOrder1.TriggerRule);
                log.Info("Executed Rule Remark = " + execution.PendingOrder1.Remark);
            }
            else
            {
                log.Info("Executed Rule = " + execution.PendingOrder2.TriggerRule);
                log.Info("Executed Rule Remark = " + execution.PendingOrder2.Remark);
            }
            log.Info("******************************************************************");
            lastExecutePrice = execution.getFilledPrice();
            lastSignalPrice = execution.getSignalPriceForExecution();
            execution.completePendingSignal();
            stgHelper.cancelPendingTrades();
            execution.flushCompletSignal();
            if (execution.CurrentMarketPosition == 0)
            {
                immediatePnLAfterPositionClose = execution.PnL;
                execution.PnL = 0;
            }
            immediateOrderExit = true;
        }

        protected void checkEnterOrderCompleted(Series<DateTime, MarketDataElement> series)
        {
            if (!execution.isPositionEmptySignalOnClose())
                return;
            lastExecutePrice = execution.getFilledPrice();
            lastSignalPrice = execution.getSignalPriceForExecution();
            log.Info("[Strategy] : Enter Strategy Executed!!!");
            log.Info("[Strategy] : Exit Strategy Executed!!!");
            int index = execution.getIndexClosedAppOrder();
            if (index == 1)
            {
                log.Info("Executed Rule = " + execution.PendingOrder1.TriggerRule);
                log.Info("Executed Rule Remark = " + execution.PendingOrder1.Remark);
            }
            else
            {
                log.Info("Executed Rule = " + execution.PendingOrder2.TriggerRule);
                log.Info("Executed Rule Remark = " + execution.PendingOrder2.Remark);
            }
            log.Info("******************************************************************");
            String dir = execution.getCompleteSignalSide();
            execution.completePendingSignal();
            execution.flushCompletSignal();
            immediateOrderEnter = true;
        }

        protected void calculateCurrentOpen(Series<DateTime, MarketDataElement> series)
        {
            if (currentOpenPrice != 0)
                return;
            MarketDataElement lastItem = (MarketDataElement)series.GetAt(series.KeyCount - 1);
            MarketDataElement openItem = MarketDateElementHelper.getFirstAvaMarketData(series, MarketDataUtil.getStartTime(lastItem.time));
            currentOpenPrice = openItem.open;
            log.Info("[calculateCurrentOpen] : currentOpen = " + currentOpenPrice);
        }

        protected void calculateCurrentMax(Series<DateTime, MarketDataElement> series)
        {
            if (series == null)
               return;
            MarketDataElement lastItem = (MarketDataElement)series.GetAt(series.KeyCount - 1);

            if (lastItem.high > currentDayHigh)
                currentDayHigh = lastItem.high;

            if (double.MinValue.Equals(currentDayLow))
                currentDayLow = lastItem.low;

            if (lastItem.low < currentDayLow)
                currentDayLow = lastItem.low;
        }

        protected void updateTick(AppTickPriceEvent tick)
        {
            if (WTS.Equals(AppConstant.WTS_TRADES))
            {
                if (tick.field == 4 && tick.value >= 0)
                {
                    bid = tick.value;
                    ask = tick.value;
                }
            }
            else if (WTS.Equals(AppConstant.WTS_MIDPOINT))
            {
                if (tick.field == 1 && tick.value >= 0)
                    bid = tick.value;
                if (tick.field == 2 && tick.value >= 0)
                    ask = tick.value;
            }
        }

        protected double calculateMid()
        {
            if (bid == 0.0 || ask == 0.0)
            {
                return double.NaN;

            }
            return (bid + ask) / 2;
        }
    }
}
