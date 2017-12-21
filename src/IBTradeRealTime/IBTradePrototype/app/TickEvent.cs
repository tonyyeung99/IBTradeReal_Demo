using IBTradeRealTime.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.app
{
    public class TickEvent
    {
        public int tickerId { get; set; }
        public int field { get; set; }
        public DateTime time { get; set; }
        public MessageType type { get; set; }
        public double value { get; set; }
    }
}
