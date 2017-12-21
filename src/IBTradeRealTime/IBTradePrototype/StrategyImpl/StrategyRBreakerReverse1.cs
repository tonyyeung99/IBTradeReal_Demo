using Deedle;
using IBApi;
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
    public class StrategyRBreakerReverse1 : AdvStrategy
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static public DateTime INVALID_TIME = new DateTime(1970, 1, 1, 0, 0, 0);
        public const String STG2_SHORT_NAME = AppConstant.STG2_SHORT_NAME;

        private double cutloss = 0.0;
        
        //private double currentOpen;

        private Boolean sentOrder = true;

        //**********[Start] Strategy Index******************
        private double sellCheck = 0.0;
        private double buyCheck = 0.0;
        private double revSell = 0.0;
        private double revBuy = 0.0;
        private double trendSell = 0.0;
        private double trendBuy = 0.0;

        private double lastClose = 0.0;
        private double lastHigh = 0.0;
        private double lastLow = 0.0;

        private double f1 = 0.0;
        private double f2 = 0.0;
        private double f3 = 0.0;
        //-------------[End] Strategy Index-------------

        //**********[Start] Strategy Flags******************
        private Boolean rangesCalculated = false;
        private Boolean dayClosed = false;
        private Boolean dataIsReady = false;
        //-------------[End] Strategy Flags-------------

        //**********[Start] Strategy Variables******************
        private int numWin = 0;
        private int numLoss = 0;
        private Boolean lossFilter = true;
        //private int orderSize = 0;
        //-------------[End] Strategy Variables-------------

        //**********[Start] Strategy Constants******************
        private const String TYPE_REVBUY = "REVBUY";
        private const String TYPE_REVSELL = "REVSELL";
        private const String TYPE_TRENDBUY = "TRENDBUY";
        private const String TYPE_TRENDSELL = "TRENDSELL";
        //-------------[End] Strategy Constanats-------------


        //private String nowExitReason1 = "";
        //private String nowExitReason2 = "";
        //private String nowEnterReason1 = "";
        //private String nowEnterReason2 = "";
        //private String nowEnterTradeType = "";
        //private String enterZone = "";


        public StrategyRBreakerReverse1(String strategyName, int stgIndex)
            : base(strategyName, stgIndex)
        {
        }


        public override void init(Object caller, Dictionary<String, String> inputArgs, TickerInfo info)
        {
            base.init(caller, inputArgs, info); 
            cutloss = Double.Parse(inputArgs["PARA1"]);
            f1 = Double.Parse(inputArgs["PARA2"]);
            f2 = Double.Parse(inputArgs["PARA3"]);
            f3 = Double.Parse(inputArgs["PARA4"]);
            //orderSize = 1;
        }

        //new
        public override void stgDailyReset()
        {
            log.Info("stgDailyReset Test 1");
            dailyReset();
            sellCheck = 0.0;
            buyCheck = 0.0;
            revSell = 0.0;
            revBuy = 0.0;
            trendSell = 0.0;
            trendBuy = 0.0;
            lastClose = 0.0;
            lastHigh = 0.0;
            lastLow = 0.0;
            /*
            f1 = 0.0;
            f2 = 0.0;
            f3 = 0.0;
            */ 
            rangesCalculated = false;
            dayClosed = false;
            dataIsReady = false;
            numWin = 0;
            numLoss = 0;
            lossFilter = true;
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
            if (execution.CurrentMarketPosition == 0 && immediateOrderExit)
            {
                if (immediatePnLAfterPositionClose < 0)
                    numLoss++;
                else
                    numWin++;

                if (numLoss >= 2 && numWin == 0)
                    lossFilter = false;      
            }
        }

        public override void calculate_signals_impl(Object sender, MarketDataEventArgs args)
        {            
            AppEvent appEvent = eventManager.storeEventQueue[stgName].Take();
            var watch = Stopwatch.StartNew();
            if (appEvent.Type.Equals(AppEventType.TickerPrice))
            {
                AppTickPriceEvent tickPriceEvent = (AppTickPriceEvent)appEvent;
                updateTick(tickPriceEvent);
            }
            else if (appEvent.Type.Equals(AppEventType.DailyReset))
            {
                stgDailyReset();
                return;
            }
            else
                return;

            if (!MDManager.isDataReady())
                return;

            if (!dataIsReady)
                log.Info("Data is Ready.");
            dataIsReady = true;
            Series<DateTime, MarketDataElement> seriesSelected = MDManager.getTimeBarSeries();
            cacluateRanges();
            calculateCurrentMax(seriesSelected);
            checkStgExitOrderCompleted(seriesSelected);
            checkStgEnterOrderCompleted(seriesSelected);
            exitTradeStrategy(seriesSelected);
            dayEndCloseTrade(seriesSelected);
            enterTradeStrategy(seriesSelected);
            watch.Stop();
            double ticks = watch.ElapsedTicks;
            log.Info("[Strategy] calculate_signals_impl running for = " + watch.ElapsedTicks * 1000000 / Stopwatch.Frequency + " micro second");
        }

        private void clearCache()
        {
            //nowExitReason1 = "";
            //nowExitReason2 = "";
            //nowEnterReason1 = "";
            //nowEnterReason2 = "";
            //nowEnterTradeType = "";
            //enterZone = "";
        }

        private void enterTradeStrategy(Series<DateTime, MarketDataElement> series)
        {
            if (!execution.isPositionSignalBothEmpty())
            {
                return;
            }

            if (dayClosed)
                return;

            if (series == null)
                return;

            MarketDataElement lastItem = (MarketDataElement)series.GetAt(series.KeyCount - 1);
            double tickClose = calculateMid();
            if (tickClose.Equals(double.NaN))
                return;

            if (lastItem.high > currentDayHigh)
                currentDayHigh = lastItem.high;

            if (double.MinValue.Equals(currentDayLow))
                currentDayLow = lastItem.low;

            if (lastItem.low < currentDayLow)
                currentDayLow = lastItem.low;

            /******************************/
            Boolean enterSignal = false;
            String signalDir = "";
            if (tickClose < trendBuy && tickClose > revSell && currentDayHigh >= sellCheck)
            {
                enterSignal = true;
                signalDir = "D";
            }
            if (tickClose > trendSell && tickClose < revBuy && currentDayLow <= buyCheck)
            {
                enterSignal = true;
                signalDir = "R";
            }

            /******************************/

            if (enterSignal && lossFilter)
            {
                if ("R".Equals(signalDir))
                {
                    String reason = "Place the {0} trade because RevBuyLine={1};";
                    reason = reason.Replace("{0}", "BUY").Replace("{1}", revBuy.ToString());
                    log.Info("[Strategy] : Enter BUY Generated !!! | MarketDataElement=" + lastItem.toString());
                    stgHelper.placeStopTrade(AppConstant.BUY_SIGNAL, "Rev Buy Rule", revBuy, reason, 1, 1);

                }

                if ("D".Equals(signalDir))
                {
                    String reason = "Place the {0} trade because RevSellLine={1};";
                    reason = reason.Replace("{0}", "SELL").Replace("{1}", revSell.ToString());
                    log.Info("[Strategy] : Enter SELL Generated !!! | MarketDataElement=" + lastItem.toString());
                    stgHelper.placeStopTrade(AppConstant.SELL_SIGNAL, "Rev Sell Rule", revSell, reason, 1, 1);
                }
            }
        }

        private void exitTradeStrategy(Series<DateTime, MarketDataElement> series)
        {
            if (!execution.isPositionOnSignalEmpty())
            {
                return;
            }

            double tickClose = calculateMid();
            if (tickClose.Equals(double.NaN))
                return;

            MarketDataElement lastItem = (MarketDataElement)series.GetAt(series.KeyCount - 1);


            double enterPrice = lastExecutePrice;

            if (execution.CurrentMarketPosition > 0)
            {
                String reason = "Cut Loss {0} trade| tickClose = {1} | cutLossPrice = {2} ";
                reason = reason.Replace("{0}", "SELL").Replace("{1}", tickClose.ToString()).Replace("{2}", buyCheck.ToString());
                stgHelper.placeStopTrade(AppConstant.SELL_SIGNAL, "Cutloss Loss Rule", buyCheck, "", 1, 1);

                reason = "Take Profit {0} trade| tickClose = {1} | profitPrice = {2} ";
                reason = reason.Replace("{0}", "SELL").Replace("{1}", tickClose.ToString()).Replace("{2}", revSell.ToString());
                stgHelper.placeLimitTrade(AppConstant.SELL_SIGNAL, "Profit Target Rule", revSell, "", 1, 2);
            }

            if (execution.CurrentMarketPosition < 0)
            {
                String reason = "Cut Loss {0} trade| tickClose = {1} | cutLossPrice = {2} ";
                reason = reason.Replace("{0}", "BUY").Replace("{1}", tickClose.ToString()).Replace("{2}", sellCheck.ToString());
                stgHelper.placeStopTrade(AppConstant.BUY_SIGNAL, "Cutloss Loss Rule", sellCheck, "", 1, 1);

                reason = "Take Profit {0} trade| tickClose = {1} | profitPrice = {2} ";
                reason = reason.Replace("{0}", "BUY").Replace("{1}", tickClose.ToString()).Replace("{2}", revBuy.ToString());
                stgHelper.placeLimitTrade(AppConstant.BUY_SIGNAL, "Profit Target Rule", revBuy, "", 1, 2);
            }
        }

        protected void dayEndCloseTrade(Series<DateTime, MarketDataElement> series)
        {
            if (dayClosed)
                return;

            MarketDataElement lastItem = (MarketDataElement)series.GetAt(series.KeyCount - 1);
            DateTime current = lastItem.time;
            String strCurrent = String.Format("{0:yyyyMMdd}", current);
            DateTime dayEnd = new DateTime(current.Year, current.Month, current.Day, 16, 13, 0);
            if (current < dayEnd)
            {
                return;
            }
            dayClosed = true;
            double tickClose = calculateMid();
            if (tickClose.Equals(double.NaN))
                return;

            stgHelper.cancelPendingTrades();

            if (execution.isPositionSignalBothEmpty())
            {
                return;
            }
            double enterPrice = lastExecutePrice;
            if (execution.CurrentMarketPosition>0)
            {
                String reason = "[Strategy] : Dayend Exit Sell Generated !!! | MarketDataElement=" + lastItem.ToString();
                log.Info("[Strategy] : Cut Loss SELL Exit Generated !!!");
                log.Info(reason);
                if (sentOrder)
                {
                    stgHelper.placeMarketTrade(AppConstant.SELL_SIGNAL, "Day Exit Rule", tickClose, reason, 1, 1);
                    clearCache();
                }
            }

            if (execution.CurrentMarketPosition < 0)
            {
                String reason = "[Strategy] : Dayend Exit BUY Generated !!! | MarketDataElement=" + lastItem.ToString();
                log.Info("[Strategy] : Cut Loss BUY Exit Generated !!!");
                log.Info(reason);
                if (sentOrder)
                {
                    stgHelper.placeMarketTrade(AppConstant.BUY_SIGNAL, "Day Exit Rule", tickClose, reason, 1, 1);
                    clearCache();
                }
            }            
        }

        private MarketDataElement getPreTradeDayMData()
        {
            MarketDataElement data = new MarketDataElement();
            /*
            data.open = 23415;
            data.close = 23046;
            data.high = 23419;
            data.low = 23036;
             */
            int reqID = MDManager.reqHistDataAdHoc(DateTime.Now);
            while(true){
                if (MDManager.getHistDataAdHoc(reqID) != null)
                {
                    data = MDManager.getHistDataAdHoc(reqID);
                    break;
                }
            }
            return data;
        }

        private void cacluateRanges()
        {
            if (!rangesCalculated)
            {
                MarketDataElement preData = getPreTradeDayMData();
                log.Info("[cacluateRanges]: preData="+ preData.ToString());
                lastClose = preData.close;
                lastHigh = preData.high;
                lastLow = preData.low;
                sellCheck = lastHigh + f1 * (lastClose - lastLow);
                buyCheck = lastLow - f1 * (lastHigh - lastClose);
                revSell = ((1 + f2) / 2) * (lastHigh + lastClose) - f2 * lastLow;
                revBuy = ((1 + f2) / 2) * (lastLow + lastClose) - f2 * lastHigh;
                trendSell = buyCheck - f3 * (sellCheck - buyCheck);
                trendBuy = sellCheck + f3 * (sellCheck - buyCheck);
                sellCheck = Math.Floor(sellCheck);
                buyCheck = Math.Floor(buyCheck);
                revSell = Math.Floor(revSell);
                revBuy = Math.Floor(revBuy);
                trendSell = Math.Floor(trendSell);
                trendBuy = Math.Floor(trendBuy);
                rangesCalculated = true;
/*
                trendBuy = 22500;
                sellCheck = 22450;
                revSell = 22300;

                revBuy = 22030;
                buyCheck = 22020;
                trendSell = 22000;
 */
                /*
                trendBuy = 22400;
                sellCheck = 22340;
                revSell = 22330;

                revBuy = 21390;
                buyCheck = 21380;
                trendSell = 21300;*/
                /*
                double anchor = 22346;
                trendBuy = anchor + 25;
                sellCheck = anchor + 15;
                revSell = anchor + 5;

                revBuy = anchor - 5;
                buyCheck = anchor - 15;
                trendSell = anchor -25;
                */
                log.Info("-----------------------------------------------");
                log.Info("[cacluateRanges]: trendBuy=" + trendBuy.ToString());
                log.Info("[cacluateRanges]: sellCheck=" + sellCheck.ToString());
                log.Info("[cacluateRanges]: revSell=" + revSell.ToString());
                log.Info("-----------------------------------------------");
                log.Info("[cacluateRanges]: revBuy=" + revBuy.ToString());
                log.Info("[cacluateRanges]: buyCheck=" + buyCheck.ToString());
                log.Info("[cacluateRanges]: trendSell=" + trendSell.ToString());
                log.Info("-----------------------------------------------");
            }
        }
    }
}
