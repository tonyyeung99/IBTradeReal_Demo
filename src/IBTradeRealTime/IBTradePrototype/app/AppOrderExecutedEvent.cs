using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.app
{
    public class AppOrderExecutedEvent : AppEvent
    {
        public String TickerName { get; set; }
        public String BQty { get; set; }
        public String SQty { get; set; }
        public String Price { get; set; }
        public String Time { get; set; }
        public String Status { get; set; }
        public String SNo { get; set; }

        public AppOrderExecutedEvent()
        {
            Type = AppEventType.OrderExecuted;

        }
    }
}
