using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.app
{
    public class AppTimeEvent : AppEvent
    {
        public DateTime eventTime { get; set; }
        public AppTimeEvent()
        {
            Type = AppEventType.TimeEvent;
        }
    }
}
