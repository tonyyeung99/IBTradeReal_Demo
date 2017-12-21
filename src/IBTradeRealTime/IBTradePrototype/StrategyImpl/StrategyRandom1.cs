using Deedle;
using IBTradeRealTime.app;
using IBTradeRealTime.AppOrders;
using IBTradeRealTime.MarketData;
using IBTradeRealTime.Strategy;
using IBTradeRealTime.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.StrategyImpl
{
    class StrategyRandom1 : AdvStrategy
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //SignalContext execution; 
        //**********[Start] Strategy Variables******************
        double num_short_ma = 0;
        double num_long_ma = 0;
        double cutLoss = 0;
        double profitTarget = 0;
        int orderSize = 0;
        //-------------[End] Strategy Variables-------------

        //**********[Start] Strategy Flags******************

        private Boolean sentOrder = true;
        private Boolean dataIsReady = false;
        //-------------[End] Strategy Flags-------------
        //private String WTS = "";

        //private StrategyHelper stgHelper;

 

        public Series<DateTime, MarketDataElement> series1 { get; set; }

        //private IAppEventManager eventManager { get; set; }
        //private IAppOrderManager orderManager { get; set; }
        //private IAppMDManager MDManager { get; set; }

        //private double lastExecutePrice;
        //private double lastSignalPrice;

        public StrategyRandom1(String stgName, int stgIndex)
            : base(stgName, stgIndex)
        {
        }

        public override void init(Object caller, Dictionary<String, String> inputArgs, TickerInfo info)
        {
            stgManager = (AppStrategyManager)caller;
            execution = new SignalContext();
            stgHelper = new StrategyHelper(stgManager, stgName, stgIndex, execution);
            eventManager = stgManager.getAppEventManager();
            MDManager = stgManager.getAppMDManager();
            WTS = info.whatToShow;
            this.inputArgs = inputArgs;
            num_short_ma = Double.Parse(inputArgs["PARA1"]);
            orderSize = Int32.Parse(inputArgs["PARA2"]);
            cutLoss = Double.Parse(inputArgs["PARA3"]);
            profitTarget = Double.Parse(inputArgs["PARA4"]);
            log.Info( stgName +" Started !");
            log.Info("short_ma = " + num_short_ma);
            log.Info("long_ma = " + num_long_ma);
            log.Info("cutLoss = " + cutLoss);
            log.Info("profitTarget = " + profitTarget);
        }
        public override void calculate_signals_impl(Object sender, MarketDataEventArgs args)
        {
            try
            {                
                AppEvent appEvent = eventManager.storeEventQueue[stgName].Take();
                var watch = Stopwatch.StartNew();
                if (appEvent.Type.Equals(AppEventType.TickerPrice))
                {
                    AppTickPriceEvent tickPriceEvent = (AppTickPriceEvent)appEvent;
                    updateTick(tickPriceEvent);
                }
                else
                    return;
                
                //updateTick(tick);
                if (!MDManager.isDataReady())
                    return;
                if (!dataIsReady)
                    log.Info("Data is Ready.");
                dataIsReady = true;

                series1 = MDManager.getTimeBarSeries();
                checkStgEnterOrderCompleted(series1);
                checkStgExitOrderCompleted(series1);
                cutLossTrade(series1 );
                exitTradeStrategy(series1);
                enterTradeStrategy(series1);
                // log.Info("[Strategy] day end close running for = " + watch.ElapsedMilliseconds + " millsecond");
                watch.Stop();
                double ticks = watch.ElapsedTicks;
                log.Info("[Strategy] calculate_signals_impl running for = " + watch.ElapsedTicks * 1000000 / Stopwatch.Frequency  + " micro second");
            }
            catch (InvalidOperationException e)
            {
                return;
            }
        }
        public void checkStgEnterOrderCompleted(Series<DateTime, MarketDataElement> series)
        {
            checkEnterOrderCompleted(series);
            if (immediateOrderEnter)
            {
                Boolean hello = true;
            }
        }

        public void checkStgExitOrderCompleted(Series<DateTime, MarketDataElement> series)
        {
            checkExitOrderCompleted(series);
        }

        /*
        private void checkExitOrderCompleted(Series<DateTime, MarketDataElement> series)
        {
            if (!execution.isPositionOnSignalOnClose())
                return;
            int index = execution.getIndexClosedAppOrder();
            
            log.Info("[Strategy] : Exit Strategy Executed!!!");
            if (index == 1)
                log.Info("Take Profit Executed!!!");
            else
                log.Info("Cutloss Executed!!!");
            log.Info("******************************************************************");
            lastExecutePrice = execution.getFilledPrice();
            lastSignalPrice = execution.getSignalPriceForExecution();            
            execution.completePendingSignal();
            stgHelper.cancelPendingTrades();
            execution.flushCompletSignal();
        }

        private void checkOpenOrderCompleted(Series<DateTime, MarketDataElement> series)
        {
            if (!execution.isPositionEmptySignalOnClose())
                return;

            log.Info("[Strategy] : Enter Strategy Executed!!!");
            log.Info("******************************************************************");
            lastExecutePrice = execution.getFilledPrice();
            lastSignalPrice = execution.getSignalPriceForExecution();
            execution.completePendingSignal();
            execution.flushCompletSignal();
        }*/

        /*
        private void updateTick(AppTickPriceEvent tick)
        {
            if(WTS.Equals(AppConstant.WTS_TRADES)){
                if (tick.field == 4 && tick.value >=0)
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
        }*/

        private void enterTradeStrategy(Series<DateTime, MarketDataElement> series )
        {
            if (!execution.isPositionSignalBothEmpty())
            {
                return;
            }

            if (series == null)
                return;

            double tickClose = calculateMid();
            if (tickClose.Equals(double.NaN))
                return;

            Random rnd = new Random();
            int number = rnd.Next(0, 19);

            int residure = number % 10;
            int q = number / 10;

            if (residure == 5)
            {
                if (q == 0)
                {
                    String reason = "[Strategy] : residure={0}; q={1}; tickClose={2}".Replace("{0}", residure.ToString()).Replace("{1}", q.ToString()).Replace("{2}", tickClose.ToString());
                    log.Info("[Strategy] : Buy Signal Generated!");
                    log.Info(reason);
                    log.Info("******************************************************************");
                    if (sentOrder)
                    {
                        stgHelper.placeMarketTrade(AppConstant.BUY_SIGNAL, "Random Rule", tickClose, "", orderSize,1);
                        //stgHelper.placeEnterTrade(TSAppConstant.BUY_SIGNAL, "Random Rule", reason, tickClose, orderSize);
                    }                     
                }
                if (q == 1)
                {
                    String reason = "[Strategy] : residure={0}; q={1}; tickClose={2}".Replace("{0}", residure.ToString()).Replace("{1}", q.ToString()).Replace("{2}", tickClose.ToString());
                    log.Info("[Strategy] : Sell Signal Generated!");
                    log.Info(reason);
                    log.Info("******************************************************************");
                    if (sentOrder)
                    {
                        stgHelper.placeMarketTrade(AppConstant.SELL_SIGNAL, "Random Rule", tickClose, "", orderSize, 1);
                        //stgHelper.placeEnterTrade(TSAppConstant.SELL_SIGNAL, "Random Rule", reason, tickClose, orderSize);
                    }
                }
            }
        }

        /*
        private void exitTradeStrategy(Series<DateTime, MarketDataElement> series)
        {

            if (!execution.isPositionOnSignalEmpty())
            {
                return;
            }

            double tickClose = calculateMid();
            if (tickClose.Equals(double.NaN))
                return;

            double enterPrice = lastExecutePrice;
            double idealEnterPrice = lastSignalPrice;
            if (execution.CurrentMarketPosition > 0)
            {
                double profitTargetPt = enterPrice + profitTarget;
                double cutLossTarget = enterPrice - cutLoss;
                String reason = "[Strategy] : Exit with the {0} trade because exceed profit target: profitTargetPt={1}; tickClose={2}; idealEnterPrice={3}; enterPrice={4}";
                reason = reason.Replace("{0}", "SELL").Replace("{1}", profitTargetPt.ToString()).Replace("{2}", tickClose.ToString());
                reason = reason.Replace("{3}", idealEnterPrice.ToString()).Replace("{4}", enterPrice.ToString());
                log.Info("[Strategy] : Exit Sell Generated !!! ");
                log.Info(reason);
                if (sentOrder)
                {
                    stgHelper.placeLimitTrade(AppConstant.SELL_SIGNAL, "Exit Random Trade", profitTargetPt, "", orderSize, 1);
                    stgHelper.placeStopTrade(AppConstant.SELL_SIGNAL, "Cutloss Random Trade", cutLossTarget, "", orderSize, 2);
                }
                    return;
            }

            if (execution.CurrentMarketPosition < 0)
            {
                double profitTargetPt = enterPrice - profitTarget;
                double cutLossTarget = enterPrice + cutLoss;
                String reason = "[Strategy] : Exit with the {0} trade because lower than profit target: profitTargetPt={1}; tickClose={2}; idealEnterPrice={3}; enterPrice={4}";
                reason = reason.Replace("{0}", "BUY").Replace("{1}", profitTargetPt.ToString()).Replace("{2}", tickClose.ToString());
                reason = reason.Replace("{3}", idealEnterPrice.ToString()).Replace("{4}", enterPrice.ToString());
                log.Info("[Strategy] : Exit BUY Generated !!! ");
                log.Info(reason);
                if (sentOrder)
                {
                    stgHelper.placeLimitTrade(AppConstant.BUY_SIGNAL, "Exit Random Trade", profitTargetPt, "", orderSize, 1);
                    stgHelper.placeStopTrade(AppConstant.BUY_SIGNAL, "Cutloss Random Trade", cutLossTarget, "", orderSize, 2);
                }
                return;
            }
        }*/
        
        private void exitTradeStrategy(Series<DateTime, MarketDataElement> series)
        {

            if (!execution.isPositionOnSignalEmpty())
            {
                return;
            }

            double tickClose = calculateMid();
            if (tickClose.Equals(double.NaN))
                return;

            double enterPrice = lastExecutePrice;
            double idealEnterPrice = lastSignalPrice;
            if (execution.CurrentMarketPosition > 0)
            {
                double profitTargetPt = enterPrice + profitTarget;

                if (tickClose > profitTargetPt)
                {
                    String reason = "[Strategy] : Exit with the {0} trade because exceed profit target: profitTargetPt={1}; tickClose={2}; idealEnterPrice={3}; enterPrice={4}";
                    reason = reason.Replace("{0}", "SELL").Replace("{1}", profitTargetPt.ToString()).Replace("{2}", tickClose.ToString());
                    reason = reason.Replace("{3}", idealEnterPrice.ToString()).Replace("{4}", enterPrice.ToString());
                    log.Info("[Strategy] : Exit Sell Generated !!! " );
                    log.Info(reason);
                    if (sentOrder)
                    {
                        stgHelper.placeMarketTrade(AppConstant.SELL_SIGNAL, "Exit Random Trade", tickClose, "", orderSize, 1);

                        //stgHelper.placeExitTrade(TSAppConstant.SELL_SIGNAL, "Profit Target Rule", reason, tickClose, enterPrice, orderSize);
                    }
                    return;
                }


            }
            if (execution.CurrentMarketPosition < 0)
            {
                double profitTargetPt = enterPrice - profitTarget;
                if (tickClose < profitTargetPt)
                {
                    String reason = "[Strategy] : Exit with the {0} trade because lower than profit target: profitTargetPt={1}; tickClose={2}; idealEnterPrice={3}; enterPrice={4}";
                    reason = reason.Replace("{0}", "BUY").Replace("{1}", profitTargetPt.ToString()).Replace("{2}", tickClose.ToString());
                    reason = reason.Replace("{3}", idealEnterPrice.ToString()).Replace("{4}", enterPrice.ToString());
                    log.Info("[Strategy] : Exit BUY Generated !!! ");
                    log.Info(reason);
                    if (sentOrder)
                    {
                        stgHelper.placeMarketTrade(AppConstant.BUY_SIGNAL, "Exit Random Trade", tickClose, "", orderSize, 1);
                        //stgHelper.placeExitTrade(TSAppConstant.BUY_SIGNAL, "Profit Target Rule", reason, tickClose, enterPrice, orderSize);
                    }
                    return;
                }
            }
        }


        private void cutLossTrade(Series<DateTime, MarketDataElement> series)
        {
            if (!execution.isPositionOnSignalEmpty())
            {
                return;
            }

            double tickClose = calculateMid();
            if (tickClose.Equals(double.NaN))
                return;

            double idealEnterPrice = lastSignalPrice;
            double enterPrice = lastExecutePrice;
            if (execution.CurrentMarketPosition > 0)
            {
                double cutLossTarget = lastExecutePrice - cutLoss;
                if (tickClose < cutLossTarget)
                {
                    String reason = "[Strategy] : Cut Loss = " + cutLossTarget + "; tickClose = " + tickClose + "; idealEnterPrice={3}; enterPrice={4}";
                    log.Info("[Strategy] : Cut Loss SELL Exit Generated !!!");
                    reason = reason.Replace("{3}", idealEnterPrice.ToString()).Replace("{4}", enterPrice.ToString());
                    log.Info(reason);
                    if (sentOrder)
                    {
                        stgHelper.placeMarketTrade(AppConstant.SELL_SIGNAL, "Cutloss Random Trade", tickClose, "", orderSize, 1);
                        //stgHelper.placeExitTrade(TSAppConstant.SELL_SIGNAL, "Cut Loss Rule", reason, tickClose, enterPrice, orderSize);
                    }
                    return;
                }
            }

            if (execution.CurrentMarketPosition < 0)
            {
                double cutLossTarget = lastExecutePrice + cutLoss;
                if (tickClose > cutLossTarget)
                {
                    String reason = "[Strategy] : Cut Loss = " + cutLossTarget + "; tickClose = " + tickClose + "; idealEnterPrice={3}; enterPrice={4}";
                    reason = reason.Replace("{3}", idealEnterPrice.ToString()).Replace("{4}", enterPrice.ToString());
                    log.Info("[Strategy] : Cut Loss BUY Exit Generated !!!");
                    log.Info(reason);
                    //Contract contract = manager.contract;
                    //Order order = manager.GetOrder("BUY", "MKT");
                    if (sentOrder)
                    {
                        stgHelper.placeMarketTrade(AppConstant.BUY_SIGNAL, "Cutloss Random Trade", tickClose, "", orderSize, 1);
                        //stgHelper.placeExitTrade(TSAppConstant.BUY_SIGNAL, "Cut Loss Rule", reason, tickClose, enterPrice, orderSize);
                    }
                    return;
                }
            }
        }

        private double calculateMid()
        {
            if (bid == 0.0 || ask == 0.0)
            {
                return double.NaN;

            }
            return (bid + ask) / 2;
        }
    }
}
