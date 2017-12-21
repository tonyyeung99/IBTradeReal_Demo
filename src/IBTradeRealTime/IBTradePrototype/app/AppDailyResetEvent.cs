using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.app
{
    class AppDailyResetEvent : AppEvent
    {
        public AppDailyResetEvent()
        {
            Type = AppEventType.DailyReset;
        }
    }
}
