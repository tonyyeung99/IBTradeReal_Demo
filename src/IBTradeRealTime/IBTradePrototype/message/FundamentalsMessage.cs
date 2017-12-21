using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.message
{
    public class FundamentalsMessage : IBMessage
    {
        private string data;

        public FundamentalsMessage(string data)
        {
            Type = MessageType.FundamentalData;
            Data = data;
        }

        public string Data
        {
            get { return data; }
            set { data = value; }
        }
    }
}
