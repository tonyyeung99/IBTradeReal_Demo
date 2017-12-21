using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.UI
{
    public interface IRealTimeBarsManagerBridge
    {
        void ResetRequest(Contract contract, string whatToShow, bool useRTH);

    }
}
