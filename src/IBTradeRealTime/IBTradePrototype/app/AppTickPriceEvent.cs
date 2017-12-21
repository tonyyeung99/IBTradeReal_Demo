using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.app
{
    public class AppTickPriceEvent : AppEvent
    {
        public int tickerId { get; set; }
        public int field { get; set; }
        public DateTime time { get; set; }
        //public AppEventType type { get; set; }
        public double value { get; set; }

        public AppTickPriceEvent()
        {
            Type = AppEventType.TickerPrice;

        }
    }





}
