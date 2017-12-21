using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.message
{
    public class ServerTimeMessage : IBMessage
    {
        protected long time;

        public long Time
        {
            get { return time; }
            set { time = value; }
        }

        public ServerTimeMessage(long time)
        {
            Type = MessageType.ServerTime;
            this.time = time;
        }
    }
}
