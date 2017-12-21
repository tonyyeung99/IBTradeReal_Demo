using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.MarketData
{
    public class TickDataBar : ICloneable
    {
        public double bid { get; set; }
        public double ask { get; set; }
        public double high { get; set; }
        public double open { get; set; }
        public double low { get; set; }
        public double close { get; set; }
        public double volume { get; set; }
        public double startVolume { get; set; }
        public double endVolume { get; set; }
        public DateTime time { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public String toString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("ElementType=").Append("|").Append("time=").Append(String.Format("{0:d/M/yyyy HH:mm:ss ffff}", time))
                                        .Append("|").Append("high=").Append(high)
                                        .Append("|").Append("open=").Append(open)
                                        .Append("|").Append("low=").Append(low)
                                        .Append("|").Append("close=").Append(close)
                                        .Append("|").Append("volume=").Append(volume)
                                        .Append("|").Append("startVolume=").Append(startVolume)
                                        .Append("|").Append("endVolume=").Append(endVolume)
                                        .Append("|").Append("bid=").Append(bid)
                                        .Append("|").Append("ask=").Append(ask);
            return builder.ToString();
        }
    }
}
