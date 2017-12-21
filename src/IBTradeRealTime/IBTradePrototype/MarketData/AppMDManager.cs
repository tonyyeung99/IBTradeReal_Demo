using Deedle;
using IBApi;
using IBTradeRealTime.app;
using IBTradeRealTime.message;
using IBTradeRealTime.UI;
using IBTradeRealTime.util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.MarketData
{
    public class AppMDManager : IAppMDManager
    {
        private long offsetTimeServerClient = long.MaxValue; //difference between Server and Application
        //private DateTime dataStartTime = AppConstant.INVALID_TIME; //??
        private DateTime histDataEndTime = AppConstant.INVALID_TIME; //the end time of historical Time Bar (for mergin the historicla time bar)
        private DateTime RTBDataStartTime = AppConstant.INVALID_TIME; //the time of first 0 second Real Time Bar (for merging the historical time bar and Real Time Bar)
        private DateTime currentRTBTime = AppConstant.INVALID_TIME;
        private Boolean isRTBarMergeNeed = false;
        private Boolean RTBInitFlag = false;
        private Boolean UIHistInitFlag = false;
        private Series<DateTime, MarketDataElement> TimeBarSeries;

        private ConcurrentDictionary<String, MarketDataElement> storeHistData = new ConcurrentDictionary<String, MarketDataElement>();
        private ConcurrentDictionary<String, String> storeHistReqIDs = new ConcurrentDictionary<String, String>();

        //private Series<DateTime, MarketDataElement> PreMergeRTBarSeries;
        //private Series<DateTime, MarketDataElement> PreMergeHistBarSeries;

        IAppStrategyManager stgManager;
        private IAppRTBSynchronizer RTBSynchronizer;
        public RTDataBar currentCompleteRTBar { get; set; }
        public RTDataBar currentTempRTBar { get; set; }
        public TickerInfo tickerInfo { get; set; }

        private IIBTradeAppBridge parentUI;

        public AppMDManager(IIBTradeAppBridge ui)
        {
            this.parentUI = ui;
            RTBSynchronizer = new AppRTBSynchronizer(this);
        }

        public void dailyReset()
        {
            histDataEndTime = AppConstant.INVALID_TIME;
            RTBDataStartTime = AppConstant.INVALID_TIME;
            currentRTBTime = AppConstant.INVALID_TIME;
            isRTBarMergeNeed = false;
            RTBInitFlag = false;
            UIHistInitFlag = false;
            TimeBarSeries = null;
            storeHistData =  new ConcurrentDictionary<String, MarketDataElement>();
            storeHistReqIDs  = new ConcurrentDictionary<String, String>();
            currentCompleteRTBar  = null;
            currentTempRTBar = null;
            RTBSynchronizer.dailyReset();
        }

        public void synchronizeRTBDataActionsIfNeeded(RTDataBar completeRTBar, DateTime currentBarTime, Boolean mergeDataIsNeed)
        {
            if (!mergeDataIsNeed)
                return;
            if (completeRTBar != null)
                RTBSynchronizer.updatePreMergeRTBarSeries(completeRTBar);
            RTBSynchronizer.mergeRTBHistDataIfValid();
            RTBSynchronizer.reqHistDataIfValid(currentBarTime, tickerInfo.whatToShow, parentUI.GetMDContract());
        }

        public void updateRTBarSeriesActions()
        {
            if(currentCompleteRTBar==null)
                return;           

            MarketDataElement data = MarketDataUtil.convertBarToMarketDataElement(currentCompleteRTBar);
            if (!isRTBarMergeNeed || TimeBarSeries != null)
            {
                if (TimeBarSeries != null)
                {
                    if (TimeBarSeries.ContainsKey(data.time))
                        return;
                    TimeBarSeries = TimeBarSeries.Merge(new SeriesBuilder<DateTime, MarketDataElement>() { { data.time, data } }.Series);
                }
                else
                    TimeBarSeries = new SeriesBuilder<DateTime, MarketDataElement>() { { data.time, data } }.Series;
            }
        }

        public void checkIsRTBarMergeNeed()
        {
            if (RTBInitFlag)
                return;
            isRTBarMergeNeed = RTBSynchronizer.isRTBarMergeNeed(tickerInfo.startTime);
            RTBInitFlag = true;
        }

        public void updateUI()
        {
            if (!UIHistInitFlag && TimeBarSeries!=null)
            {
                for (int i = 0; i < TimeBarSeries.KeyCount; i++)
                {
                    MarketDataElement data = TimeBarSeries.GetAt(i);
                    string strTime = String.Format("{0:yyyyMMdd  HH:mm:ss}", data.time.ToLocalTime());
                    parentUI.HandleMessage(new HistoricalDataMessage(-1, strTime, data.open, data.high, data.low, data.close, Convert.ToInt32(data.volume), 0, 0, false));
                }
                UIHistInitFlag = true;
            }
            else
            {
                if (TimeBarSeries != null && currentCompleteRTBar != null)
                {
                    string strTime = String.Format("{0:yyyyMMdd  HH:mm:ss}", currentCompleteRTBar.time.ToLocalTime());
                    parentUI.HandleMessage(new HistoricalDataMessage(-1, strTime, currentCompleteRTBar.open, currentCompleteRTBar.high, currentCompleteRTBar.low, currentCompleteRTBar.close, Convert.ToInt32(currentCompleteRTBar.volume), 0, 0, false));
                }
            }
        }




        public void handleRealTimeBarMessage(IBMessage message)
        {     
            RealTimeBarMessage rtBar = (RealTimeBarMessage)message;
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime dt = start.AddMilliseconds(rtBar.Timestamp * 1000).ToLocalTime();
            Boolean isStart = isRTBarProcessStart(message);
            if (isStart)
            {
                checkIsRTBarMergeNeed();
                buildAndUpdateSynMinuteBar(message);
                synchronizeRTBDataActionsIfNeeded(currentCompleteRTBar, dt, isRTBarMergeNeed);
                updateRTBarSeriesActions();
                updateUI();
            }
        }

        public void startStrategy(String name, int i, Dictionary<String, String> inputArgs, String userAccount, Contract contract, TickerInfo info)
        {
            this.tickerInfo = info;
        }

        public void buildAndUpdateSynMinuteBar(IBMessage message)
        {
            HistoricalDataMessage bar = (HistoricalDataMessage)message;
            RealTimeBarMessage rtBar = (RealTimeBarMessage)message;
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime dt = start.AddMilliseconds(rtBar.Timestamp * 1000).ToLocalTime();            

            if (MarketDataUtil.isThisMessageStartOfMinute(message))
            {
                currentTempRTBar = new RTDataBar();
                currentTempRTBar.time = dt;
                currentTempRTBar.open = rtBar.Open;
                currentTempRTBar.close = rtBar.Close;
                currentTempRTBar.high = rtBar.High;
                currentTempRTBar.low = rtBar.Low;
                currentTempRTBar.volume = rtBar.LongVolume;
            }
            else
            {
                if (currentTempRTBar == null)
                    return;
                currentTempRTBar.close = rtBar.Close;
                if (currentTempRTBar.high < rtBar.High)
                    currentTempRTBar.high = rtBar.High;
                if (currentTempRTBar.low > rtBar.Low)
                    currentTempRTBar.low = rtBar.Low;
                currentTempRTBar.volume += rtBar.LongVolume;
                if (MarketDataUtil.isThisMessageEndOfMinute(message))
                {
                    currentCompleteRTBar = currentTempRTBar;
                    return;
                }
            }
            currentCompleteRTBar = null;
            return;
        }

        public void handleTickMessage(IBMessage message)        {

            if (message is TickPriceMessage)
                putTickEvent((TickPriceMessage)message, (MarketDataMessage)message);
        }

        public void putTickEvent(TickPriceMessage message, MarketDataMessage dataMessage)
        {
            
            String[] stgNames = stgManager.getStgNames();
            AppTickPriceEvent tick = new AppTickPriceEvent();
            tick.time = DateTime.Now;
            tick.value = message.Price;
            tick.field = dataMessage.Field;
            tick.tickerId = message.RequestId;

            Dictionary<String, BlockingCollection<AppEvent>> storeEventQueue = stgManager.getAppEventManager().storeEventQueue;
            foreach (String name in stgNames)
            {
                storeEventQueue[name].Add(tick);
            }
        }

        public DateTime getRtbDataStartTime()
        {
            return RTBDataStartTime;
        }

        public void setRtbDataStartTime(DateTime time)
        {
            RTBDataStartTime = time;
        }

        public DateTime getHistDataEndTime()
        {
            return histDataEndTime;
        }

        public void setHistDataEndTime(DateTime time)
        {
            histDataEndTime = time;
        }

        public Boolean getRTBInitFlag()
        {
            return RTBInitFlag;
        }

        public void injectRTBInitFlag(Boolean RTBInitFlag)
        {
            this.RTBInitFlag = RTBInitFlag;
        }

        public Boolean getIsRTBarMergeNeed()
        {
            return isRTBarMergeNeed;
        }

        public void injectIsRTBarMergeNeed(Boolean isRTBarMergeNeed)
        {
            this.isRTBarMergeNeed = isRTBarMergeNeed;
        }

        public void setParentUI(IIBTradeAppBridge parentUI)
        {
            this.parentUI = parentUI;
        }

        public void injectParentUI(IIBTradeAppBridge parentUI)
        {
            this.parentUI = parentUI;
        }

        public IIBTradeAppBridge getParentUI()
        {
            return this.parentUI;
        }

        public Boolean isRTBarProcessStart(IBMessage message)
        {
            if (!AppConstant.INVALID_TIME.Equals(RTBDataStartTime))
                return true;
            RealTimeBarMessage rtBar = (RealTimeBarMessage)message;
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime current = start.AddMilliseconds(rtBar.Timestamp * 1000).ToLocalTime();
            if (RTBDataStartTime.Equals(AppConstant.INVALID_TIME))
            {
                if (current.Second == 0)
                {
                    RTBDataStartTime = current;
                    return true;
                }
                return false;
            }
            return false;
        }
        
        public int reqHistDataAdHoc(DateTime current){
            DateTime currentDay = new DateTime(current.Year, current.Month, current.Day, 0, 0, 0);
            string whatToShow = parentUI.GetWTS();
            string strEndTime = String.Format("{0:yyyyMMdd HH:mm:ss}", currentDay.AddMinutes(1)) + " HKT";
            string duration = "1 D";
            string barSize = "1 day";
            int outsideRTH = 1; //use regular trading hour
            Contract contract = parentUI.GetMDContract();
            int reqID = parentUI.addHistDataRequestWithID(contract, strEndTime, duration, barSize, whatToShow, 1);
            storeHistReqIDs.AddOrUpdate(reqID.ToString(), reqID.ToString(), (key, oldValue) => reqID.ToString());
            return reqID;
        }

        public MarketDataElement getHistDataAdHoc(int reqIndex){
            if (storeHistData.ContainsKey(reqIndex.ToString()))
                return storeHistData[reqIndex.ToString()];
            return null;
        }

        public Boolean isHistDataAdHocRequested(int reqIndex)
        {
            if (storeHistReqIDs.ContainsKey(reqIndex.ToString()))
                return true;
            return false;
        }

        public void updateHistDataAdHoc(HistoricalDataMessage message)
        {
            MarketDataElement dataElement = new MarketDataElement();
            dataElement.high = message.High;
            dataElement.low = message.Low;
            dataElement.open = message.Open;
            dataElement.close = message.Close;
            storeHistData.AddOrUpdate(message.RequestId.ToString(), dataElement, (key, oldValue) => dataElement);
 
        }

        public Boolean isDataReady(){
            if (RTBSynchronizer.getIsDataMerged() || (!RTBSynchronizer.getNeedMergeFlag() && TimeBarSeries != null))
                return true;
            return false;
        }

        public Series<DateTime, MarketDataElement> getTimeBarSeries()
        {
            return TimeBarSeries;
        }

        public void setTimeBarSeries(Series<DateTime, MarketDataElement> series)
        {
            TimeBarSeries = series;
        }

        public IAppRTBSynchronizer getAppRTBSynchronizer()
        {
            return RTBSynchronizer;
        }

        public void injectAppRTBSynchronizer(IAppRTBSynchronizer appRTBSynchronizer)
        {
            RTBSynchronizer = appRTBSynchronizer;
        }

        public void injectStrategyManager(IAppStrategyManager stgManager)
        {
        }

        public IAppStrategyManager getStgManager()
        {
            return this.stgManager;
        }
        /*
        public Series<DateTime, MarketDataElement> getPreMergeRTBarSeries()
        {
            return null;
        }
        public Series<DateTime, MarketDataElement> getPreMergeHistBarSeries()
        {
            return null;
        }*/


    }
}
