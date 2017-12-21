using IBTradeRealTime.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IBTradeRealTime.app
{
    public class StrategyArg
    {
        public IStrategy strategy { get; set; }
        public int number { get; set; }
        public CancellationToken ct { get; set; }
    }
}
