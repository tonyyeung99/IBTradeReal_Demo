using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.MarketData
{
    public class MarketDataElement
    {
        public String elementType { get; set; }
        public double high { get; set; }
        public double open { get; set; }
        public double low { get; set; }
        public double close { get; set; }
        public double volume { get; set; }

        public double bid { get; set; }
        public double ask { get; set; }

        public String symbol { get; set; }
        public String contractType { get; set; }
        public String contractCallPut { get; set; }

        public double strike { get; set; }

        public DateTime time { get; set; }

        public String toString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("ElementType=").Append(elementType)
                                        .Append("|").Append("symbol=").Append(symbol)
                                        .Append("|").Append("contractType=").Append(contractType)
                                        .Append("|").Append("time=").Append(String.Format("{0:d/M/yyyy HH:mm:ss ffff}", time))
                                        .Append("|").Append("contractCallPut=").Append(contractCallPut)
                                        .Append("|").Append("high=").Append(high)
                                        .Append("|").Append("open=").Append(open)
                                        .Append("|").Append("low=").Append(low)
                                        .Append("|").Append("close=").Append(close)
                                        .Append("|").Append("volume=").Append(volume)
                                        .Append("|").Append("bid=").Append(bid)
                                        .Append("|").Append("ask=").Append(ask)
                                        .Append("|").Append("strike=").Append(strike);
            return builder.ToString();
        }
    }
}
