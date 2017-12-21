using IBApi;
using IBTradeRealTime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.Test.UI
{
    class RealTimeBarsManagerMock : IRealTimeBarsManagerBridge
    {
        public RealTimeBarsManagerMock()           
        {
        }
        public void ResetRequest(Contract contract, string whatToShow, bool useRTH)
        {
        }
    }
}
