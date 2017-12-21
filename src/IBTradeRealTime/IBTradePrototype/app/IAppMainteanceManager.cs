using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.app
{
    public interface IAppMainteanceManager
    {
        Boolean lunchTimeRTBReset{get; set;}
        Boolean morningTimeRTBReset { get; set; }
        Boolean dailyAllFlagsInit { get; set; }

        BlockingCollection<AppEvent> storeEventQueue { get; set; }
        Dictionary<DateTime, DateTime> CompleteDailyReset { get; set; }
        void startManager();
        void handleResetRTBRequest(AppTimeEvent timeEvent);
        void handleDailyAllFlagsReset(AppTimeEvent timeEvent);
        void handOrderExeEvent(AppOrderExecutedEvent exeEvent);
        void resetDailyAllFlag(DateTime time);
    }
}
