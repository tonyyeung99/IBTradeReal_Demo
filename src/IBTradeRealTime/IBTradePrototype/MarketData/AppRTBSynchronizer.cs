using Deedle;
using IBApi;
using IBTradeRealTime.message;
using IBTradeRealTime.UI;
using IBTradeRealTime.util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.MarketData
{
    public class AppRTBSynchronizer : IAppRTBSynchronizer
    {
        //private DateTime rtbDataStartTime = TSAppConstant.INVALID_TIME;
        private Boolean needMergeFlag = false;
        private Boolean isDataMerged = false;
        private Boolean isReqHistDataSent = false;
        public Series<DateTime, MarketDataElement> preMergeRTBarSeries{ get; set;}
        public Series<DateTime, MarketDataElement> preMergeHistBarSeries { get; set; }
        IAppMDManager appMDManager;

        public AppRTBSynchronizer(IAppMDManager appMDManager) 
        {
            this.appMDManager = appMDManager;
        }

        public void dailyReset()
        {
            needMergeFlag = false;
            isDataMerged = false;
            isReqHistDataSent = false;
            preMergeRTBarSeries = null;
            preMergeHistBarSeries = null;
        }

        public IAppMDManager getAppMDManager(){
            return this.appMDManager;
        }

        public Boolean getIsDataMerged()
        {
            return isDataMerged;
        }

        public void setIsDataMerged(Boolean isDataMerged)
        {
            this.isDataMerged = isDataMerged;
        }

        public Boolean getNeedMergeFlag()
        {
            return needMergeFlag;
        }

        public void setNeedMergeFlag(Boolean needMergeFlag)
        {
            this.needMergeFlag = needMergeFlag;
        }

        public Boolean getIsReqHistDataSent()
        {
            return isReqHistDataSent;
        }
        
        public void setIsReqHistDataSent(Boolean isReqHistDataSent)
        {
            this.isReqHistDataSent = isReqHistDataSent;
        }

        public Boolean isRTBarMergeNeed(String strTickerStartTimeOfDay) {             
            DateTime rtbDataStartTime = appMDManager.getRtbDataStartTime();
            String strStartTime = String.Format("{0:yyyyMMdd}", rtbDataStartTime) + "  " + strTickerStartTimeOfDay;
            DateTime startTime = DateTime.ParseExact(strStartTime, "yyyyMMdd  HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
            if (rtbDataStartTime.ToLocalTime() >  startTime.ToLocalTime())
            {
                needMergeFlag = true;
                return true;
            }
            return false;
        }

        public void mergeRTBHistDataIfValid()
        {
            if (appMDManager.getTimeBarSeries() != null)
                return;
            DateTime rtbDataStartTime = appMDManager.getRtbDataStartTime();
            //DateTime histDataEndTime = appMDManager.getHistDataEndTime();
            DateTime histDataEndTime = rtbDataStartTime.AddMinutes(-1);
            //Series<DateTime, MarketDataElement> preMergeRTBarSeries = getPreMergeRTBarSeries();
            //Series<DateTime, MarketDataElement> preMergeHistBarSeries getPreMergeHistBarSeries();
            if (preMergeRTBarSeries != null && preMergeHistBarSeries != null && needMergeFlag)
            {
                if (preMergeRTBarSeries.TryGet(rtbDataStartTime).HasValue && preMergeHistBarSeries.TryGet(histDataEndTime).HasValue && isDataMerged == false)
                {
                    mergeRTBHistData();
                    isDataMerged = true;
                }
            }
        }

        public void reqHistDataIfValid(DateTime current, String whatToShow, Contract contract){
            DateTime rtbDataStartTime = appMDManager.getRtbDataStartTime();
            //DateTime histDataEndTime = rtbDataStartTime.AddMinutes(-1);
            DateTime histReqProcessTime = rtbDataStartTime.AddSeconds(30);
            //appMDManager.setHistDataEndTime(histDataEndTime);
            if (current >= histReqProcessTime && isReqHistDataSent == false && needMergeFlag)
            {
                //string whatToShow = parentUI.GetWTS();
                string strEndTime = String.Format("{0:yyyyMMdd HH:mm:ss}", rtbDataStartTime) + " HKT";
                string duration = "1 D";
                string barSize = "1 min";
                int outsideRTH = 1; //use regular trading hour           

                IIBTradeAppBridge parentUI = appMDManager.getParentUI();
                TickerInfo info = appMDManager.tickerInfo;

                //*** 2015-12-09 big change***
                //parentUI.addHistDataRequest(contract, strEndTime, duration, barSize, info.whatToShow, 1);
                parentUI.addHistDataRequest(contract, strEndTime, duration, barSize, whatToShow, 1);
                isReqHistDataSent = true;
                isReqHistDataSent = true;
            }
        }

        public void mergeRTBHistData()
        {
            //Series<DateTime, MarketDataElement> preMergeRTBarSeries = appMDManager.getPreMergeRTBarSeries();
            //Series<DateTime, MarketDataElement> preMergeHistBarSeries = appMDManager.getPreMergeHistBarSeries();
            DateTime rtbDataStartTime = appMDManager.getRtbDataStartTime();
            if (preMergeHistBarSeries.TryGet(rtbDataStartTime).HasValue)
            {
                preMergeHistBarSeries = preMergeHistBarSeries.Before(rtbDataStartTime);
            }
            appMDManager.setTimeBarSeries(preMergeHistBarSeries.Merge(preMergeRTBarSeries));
        }

        public void updatePreMergeRTBarSeries(RTDataBar bar)
        { 
            if (isDataMerged)
                return;
            
            DateTime time = bar.time;
            MarketDataElement currentRTBData = MarketDataUtil.convertBarToMarketDataElement(bar);

            if (preMergeRTBarSeries == null)
            {
                preMergeRTBarSeries = new SeriesBuilder<DateTime, MarketDataElement>() { { time, currentRTBData } }.Series;
            }
            else
            {
                if (!preMergeRTBarSeries.ContainsKey(time))
                {
                    preMergeRTBarSeries = preMergeRTBarSeries.Merge(new SeriesBuilder<DateTime, MarketDataElement>() { { time, currentRTBData } }.Series);

                }
            }
            return;
        }

        public Boolean updateTimeBarSeries(IBMessage message)
        {

            Boolean isAdded = false;
            if ((!isDataMerged && getNeedMergeFlag()) || !getNeedMergeFlag())
                return isAdded;
            Series<DateTime, MarketDataElement> TimeBarSeries = appMDManager.getTimeBarSeries();
            if (TimeBarSeries==null)
                return isAdded;

            HistoricalDataMessage histMessage = (HistoricalDataMessage)message;

            double h = Convert.ToDouble(histMessage.High);
            double l = Convert.ToDouble(histMessage.Low);
            double o = Convert.ToDouble(histMessage.Open);
            double c = Convert.ToDouble(histMessage.Close);
            double vol = Convert.ToDouble(histMessage.Volume);

            DateTime time = DateTime.ParseExact(histMessage.Date.Trim(), "yyyyMMdd  HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
            MarketDataElement currentHsiData = MarketDataUtil.createHLOC(time, "HLOC", "HSI", "FUT");
            currentHsiData.volume = vol;
            currentHsiData.time = time;

            
            if (TimeBarSeries == null)
            {
                TimeBarSeries = new SeriesBuilder<DateTime, MarketDataElement>() { { time, currentHsiData } }.Series;
                isAdded = true;
            }
            else
            {
                MarketDataElement element = TimeBarSeries.GetAt(TimeBarSeries.KeyCount - 1);
                if (!TimeBarSeries.ContainsKey(time))
                {
                    TimeBarSeries = TimeBarSeries.Merge(new SeriesBuilder<DateTime, MarketDataElement>() { { time, currentHsiData } }.Series);
                    isAdded = true;
                }
            }
            return isAdded;
        }

        public Boolean updatePreMergeHistBarSeries(IBMessage message)
        {
            Boolean isAdded = false;
            if (isDataMerged)
                return isAdded;

            HistoricalDataMessage histMessage = (HistoricalDataMessage)message;

            double h = Convert.ToDouble(histMessage.High);
            double l = Convert.ToDouble(histMessage.Low);
            double o = Convert.ToDouble(histMessage.Open);
            double c = Convert.ToDouble(histMessage.Close);
            double vol = Convert.ToDouble(histMessage.Volume);

            DateTime time = DateTime.ParseExact(histMessage.Date.Trim(), "yyyyMMdd  HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
            MarketDataElement currentHistData = MarketDataUtil.createHLOC(time, "HLOC", "HSI", "FUT");
            MarketDataUtil.setHLOC(currentHistData, h, l, o, c);
            currentHistData.volume = vol;
            currentHistData.time = time;
            if (preMergeHistBarSeries == null)
            {
                preMergeHistBarSeries = new SeriesBuilder<DateTime, MarketDataElement>() { { time, currentHistData } }.Series;
                isAdded = true;
            }
            else
            {
                if (!preMergeHistBarSeries.ContainsKey(time))
                {
                    preMergeHistBarSeries = preMergeHistBarSeries.Merge(new SeriesBuilder<DateTime, MarketDataElement>() { { time, currentHistData } }.Series);
                    isAdded = true;
                }
            }
            return isAdded;
        }
    }
}
