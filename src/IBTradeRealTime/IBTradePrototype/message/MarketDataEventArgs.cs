using IBTradeRealTime.app;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.MarketData
{
    public class MarketDataEventArgs : EventArgs
    {
        public DateTime dataTime { set; get; }
        public TickEvent tick { set; get; }
        public MarketDataEventArgs(DateTime dataTime, TickEvent tick)
        {
            this.dataTime = dataTime;
            this.tick = tick;
        }
    }
}
