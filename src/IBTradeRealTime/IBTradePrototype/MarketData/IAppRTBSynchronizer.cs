using Deedle;
using IBApi;
using IBTradeRealTime.message;
using IBTradeRealTime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.MarketData
{
    public interface IAppRTBSynchronizer
    {
        //AppRTBSynchronizer(AppMDManager appMDManager);
        Series<DateTime, MarketDataElement> preMergeRTBarSeries { get; set; }
        Series<DateTime, MarketDataElement> preMergeHistBarSeries { get; set; }
        Boolean isRTBarMergeNeed( String strTickerStartTimeOfDay );
        void mergeRTBHistDataIfValid();
        void reqHistDataIfValid(DateTime current, String whatToShow, Contract contract);
        void updatePreMergeRTBarSeries(RTDataBar bar);
        Boolean updatePreMergeHistBarSeries(IBMessage message);
        Boolean updateTimeBarSeries(IBMessage message);
        Boolean getIsDataMerged();
        void setIsDataMerged(Boolean isDataMerged);
        Boolean getNeedMergeFlag();
        void setNeedMergeFlag(Boolean needMergeFlag);
        Boolean getIsReqHistDataSent();
        void setIsReqHistDataSent(Boolean IsReqHistDataSent);
        IAppMDManager getAppMDManager();
        void dailyReset();
    }
}
