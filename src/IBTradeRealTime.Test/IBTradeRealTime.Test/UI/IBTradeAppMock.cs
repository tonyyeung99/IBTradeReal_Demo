using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBTradeRealTime.UI;
using IBTradeRealTime.util;
using IBTradeRealTime.message;
using IBApi;


namespace IBTradeRealTime.Test.UI
{
    class IBTradeAppMock : IIBTradeAppBridge
    {
        public IRealTimeBarsManagerBridge getRealTimeBarsManager() {return null; }
        public String GetWTS() {return "";}
        public void setTickerInfo(TickerInfo info) {}
        public void setIDFactory(int seedID) {}
        public void showMessage(String message) {}
        public void HandleMessage(IBMessage message) {}
        public void HandleStrategyOnOff(StrategyOnOff onOff) {}
        public void addHistDataRequest(Contract contract, String strEndTime, String duration, String barSize, String whatToShow, int outsideRTH) {}
        public int addHistDataRequestWithID(Contract contract, String strEndTime, String duration, String barSize, String whatToShow, int outsideRTH) { return 0; }
        public Contract GetMDContract() { return null; }
        public TickerInfo getTickerInfo() { return null; }
        public void dailyReset(){}
        public List<String[]> getPositionSummary() { return null; }
        public UserPref getUserPref() { return null; }
    }
}
