using Deedle;
using IBTradeRealTime.app;
using IBTradeRealTime.message;
using IBTradeRealTime.UI;
using IBTradeRealTime.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.MarketData
{
    public interface IAppMDManager
    {
        void handleRealTimeBarMessage(IBMessage message);

        Boolean isDataReady();
        Series<DateTime, MarketDataElement> getTimeBarSeries();
        void setTimeBarSeries(Series<DateTime, MarketDataElement> series);
        //Series<DateTime, MarketDataElement> getPreMergeRTBarSeries();
        //Series<DateTime, MarketDataElement> getPreMergeHistBarSeries();
        void buildAndUpdateSynMinuteBar(IBMessage message);
        void updateRTBarSeriesActions();

        RTDataBar currentCompleteRTBar { get; set; }
        RTDataBar currentTempRTBar { get; set; }
        TickerInfo tickerInfo { get; set; }
        DateTime getRtbDataStartTime();
        void setRtbDataStartTime(DateTime time);
        DateTime getHistDataEndTime();
        void setHistDataEndTime(DateTime time);
        Boolean getRTBInitFlag();
        void injectRTBInitFlag(Boolean RTBInitFlag);
        Boolean getIsRTBarMergeNeed();
        void injectIsRTBarMergeNeed(Boolean isRTBarMergeNeed);
        void injectStrategyManager(IAppStrategyManager stgManager);
        IAppStrategyManager getStgManager();
        void setParentUI(IIBTradeAppBridge parentInfo);
        void injectParentUI(IIBTradeAppBridge parentInfo);
        IIBTradeAppBridge getParentUI(); 

        Boolean isRTBarProcessStart(IBMessage message);//first 0 second bar reach    
        void checkIsRTBarMergeNeed();
        void synchronizeRTBDataActionsIfNeeded(RTDataBar completeRTBar, DateTime currentBarTime, Boolean mergeDataIsNeed);
        
        Boolean isHistDataAdHocRequested(int requestId);

        int reqHistDataAdHoc(DateTime current);
        MarketDataElement getHistDataAdHoc(int reqIndex);
        void updateHistDataAdHoc(HistoricalDataMessage message);

        IAppRTBSynchronizer getAppRTBSynchronizer();
        void injectAppRTBSynchronizer(IAppRTBSynchronizer appRTBSynchronizer);

        void dailyReset();
    }
}
