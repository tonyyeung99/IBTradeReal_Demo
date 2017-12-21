using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.app
{
    public enum AppEventType
    {
        TimeEvent = 1,
        TickerPrice = 2,
        DailyReset = 3,
        OrderExecuted = 4
    }
}
