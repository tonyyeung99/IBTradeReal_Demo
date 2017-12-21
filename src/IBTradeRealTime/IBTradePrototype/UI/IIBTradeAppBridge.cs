using IBApi;
using IBTradeRealTime.message;
using IBTradeRealTime.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.UI
{
    public interface IIBTradeAppBridge
    {
        IRealTimeBarsManagerBridge getRealTimeBarsManager();
        String GetWTS();
        void setTickerInfo(TickerInfo info);
        void setIDFactory(int seedID);
        void showMessage(String message);
        void HandleMessage(IBMessage message);
        void HandleStrategyOnOff(StrategyOnOff onOff);
        void addHistDataRequest(Contract contract, String strEndTime, String duration, String barSize, String whatToShow, int outsideRTH);
        void dailyReset();
        List<String[]> getPositionSummary();
        int addHistDataRequestWithID(Contract contract, String strEndTime, String duration, String barSize, String whatToShow, int outsideRTH);
        Contract GetMDContract();
        TickerInfo getTickerInfo();
        UserPref getUserPref();
    }
}
