using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.StrategyHelper
{
    class SAPaFRow : PaFRow
    {
        public DateTime startTime { get; set; }
        public double maxLevel { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
