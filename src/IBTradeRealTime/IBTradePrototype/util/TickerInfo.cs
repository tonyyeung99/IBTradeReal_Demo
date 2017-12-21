using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.util
{
    public class TickerInfo
    {
        public String tickerID { get; set; }
        public String contractID { get; set; }
        public String symbol { get; set; }
        public String type { get; set; }
        public String exchange { get; set; }
        public String pExchange { get; set; }
        public String currency { get; set; }
        public String lSymbol { get; set; }
        public String whatToShow { get; set; }
        public String startTime { get; set; }
        public String lunchEndTime { get; set; }
        public String endTime { get; set; }
    }
}
