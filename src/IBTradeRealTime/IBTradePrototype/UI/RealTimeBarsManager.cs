using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

using IBTradeRealTime.backend;
using IBTradeRealTime.message;

using IBApi;
using System.Windows.Forms;
using IBTradeRealTime.util;
using System.Diagnostics;
using System.Threading;
using IBTradeRealTime.MarketData;

namespace IBTradeRealTime.UI
{
    public class RealTimeBarsManager : HistoricalDataManager, IRealTimeBarsManagerBridge
    {
        public const int RT_BARS_ID_BASE = 40000000;

        //private MarketDataHelper marketDataHelper;
        private IAppMDManager MDManager;

        //private IBTradeApp appForm;
        //public RealTimeBarsManager(IBClient ibClient, Chart rtBarsChart, DataGridView rtBarsGrid, MarketDataHelper marketDataHelper, IBTradeApp appForm)
        public RealTimeBarsManager()
        {
        }
        public RealTimeBarsManager(IBClient ibClient, Chart rtBarsChart, DataGridView rtBarsGrid, IAppMDManager MDManager, IBTradeApp appForm)
            : base(ibClient, rtBarsChart, rtBarsGrid, appForm)
        {
            this.MDManager = MDManager;
            //this.marketDataHelper = marketDataHelper;
            //this.appForm = appForm;
        }

        public void AddRequest(Contract contract, string whatToShow, bool useRTH)
        {
            Clear();
            //ibClient.ClientSocket.reqHistoricalData(currentTicker, contract, endDateTime, durationString, barSizeSetting, whatToShow, useRTH, 1);
            currentTicker++;
            ibClient.ClientSocket.reqRealTimeBars(currentTicker + RT_BARS_ID_BASE, contract, 5, whatToShow, useRTH, null);
        }

        public override void Clear()
        {
            ibClient.ClientSocket.cancelRealTimeBars(currentTicker + RT_BARS_ID_BASE);
            base.Clear();
        }

        public void ResetRequest(Contract contract, string whatToShow, bool useRTH){
            ibClient.ClientSocket.cancelRealTimeBars(currentTicker + RT_BARS_ID_BASE);
            currentTicker++;
            ibClient.ClientSocket.reqRealTimeBars(currentTicker + RT_BARS_ID_BASE, contract, 5, whatToShow, useRTH, null);
        }

        public override void UpdateUI(IBMessage message)
        {
            barCounter++;
            RealTimeBarMessage rtBar = (RealTimeBarMessage)message;

            /*
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0);

            DateTime dt = start.AddMilliseconds(rtBar.Timestamp * 1000).ToLocalTime();

            //***for new MD logiic***
            DateTime searchEndToMin = marketDataHelper.getTimeToMin(dt);
            //DateTime searchEndToMin = marketDataHelper.getTimeToMin(dt.AddSeconds(5));
            DateTime searchStart = marketDataHelper.getGapStartTime(searchEndToMin);
            Boolean fullMinute = false;
            double second = dt.Second;
            if (dt.Second == 0)
                fullMinute=true;
             */ 

            //appForm.strategyManager.handleRealTimeBarMessage(message);
            MDManager.handleRealTimeBarMessage(message);
            PopulateGrid(message);
        }
    }
}
