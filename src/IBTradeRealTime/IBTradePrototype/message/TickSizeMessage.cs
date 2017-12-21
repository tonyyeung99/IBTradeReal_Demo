using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.message
{
    public class TickSizeMessage : MarketDataMessage
    {
        private int size;

        public TickSizeMessage(int requestId, int field, int size)
            : base(MessageType.TickSize, requestId, field)
        {
            Size = size;
        }

        public int Size
        {
            get { return size; }
            set { size = value; }
        }
    }
}
