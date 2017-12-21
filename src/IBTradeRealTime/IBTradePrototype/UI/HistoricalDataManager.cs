using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

using IBTradeRealTime.message;
using IBTradeRealTime.backend;

using IBApi;
using System.Globalization;
using IBTradeRealTime.util;
using System.Diagnostics;
using IBTradeRealTime.MarketData;
using IBTradeRealTime.app;
using Deedle;

namespace IBTradeRealTime.UI
{
    public class HistoricalDataManager : DataManager
    {
        public const int HISTORICAL_ID_BASE = 30000000;

        private string fullDatePattern = "yyyyMMdd  HH:mm:ss";
        private string yearMonthDayPattern = "yyyyMMdd";

        protected int barCounter = -1;
        protected DataGridView gridView;

        private List<HistoricalDataMessage> historicalData;
        protected IBTradeApp appForm;
        private IAppMDManager appMDManager;
        private IAppRTBSynchronizer RTBSynchronizer;

        public HistoricalDataManager()
        {
        }
        public HistoricalDataManager(IBClient ibClient, Chart chart, DataGridView gridView, IBTradeApp appForm)
            : base(ibClient, chart)
        {
            /*
            Chart historicalChart = (Chart)uiControl;
            historicalChart.Series[0]["PriceUpColor"] = "Green";
            historicalChart.Series[0]["PriceDownColor"] = "Red";
             */ 
            this.gridView = gridView;
            this.appForm = appForm;
            AppStrategyManager stgManager = appForm.appStrategyManager;
            this.appMDManager = stgManager.getAppMDManager();
            this.RTBSynchronizer = appMDManager.getAppRTBSynchronizer();
        }

        public void AddRequest(Contract contract, string endDateTime, string durationString, string barSizeSetting, string whatToShow, int useRTH, int dateFormat)
        {
            //Clear();

            if(AppConstant.USE_OUT_REGULAR_DATA)
                ibClient.ClientSocket.reqHistoricalData(currentTicker + HISTORICAL_ID_BASE, contract, endDateTime, durationString, barSizeSetting, whatToShow, 0, 1, new List<TagValue>());
            else 
                ibClient.ClientSocket.reqHistoricalData(currentTicker + HISTORICAL_ID_BASE, contract, endDateTime, durationString, barSizeSetting, whatToShow, useRTH, 1, new List<TagValue>());
        }

        public int AddRequestWithID(Contract contract, string endDateTime, string durationString, string barSizeSetting, string whatToShow, int useRTH, int dateFormat)
        {
            int reqID = currentTicker + HISTORICAL_ID_BASE;

            //if (AppConstant.USE_OUT_REGULAR_DATA)
                ibClient.ClientSocket.reqHistoricalData(reqID, contract, endDateTime, durationString, barSizeSetting, whatToShow, 1, 1, new List<TagValue>());
            //else
            //    ibClient.ClientSocket.reqHistoricalData(reqID, contract, endDateTime, durationString, barSizeSetting, whatToShow, useRTH, 1, new List<TagValue>());
            return reqID;
        }

        public override void Clear()
        {
            barCounter = -1;
            /*
            Chart historicalChart = (Chart)uiControl;
            historicalChart.Series[0].Points.Clear();
             */ 
            gridView.Rows.Clear();
            historicalData = new List<HistoricalDataMessage>();
        }

        public override void NotifyError(int requestId)
        {
        }

        public override void UpdateUI(IBMessage message)
        {
            switch (message.Type)
            {
                case MessageType.HistoricalData:
                    var watch = Stopwatch.StartNew();
                    RTBSynchronizer.updatePreMergeHistBarSeries(message);
                    Series<DateTime, MarketDataElement> series = appMDManager.getTimeBarSeries();
                    HistoricalDataMessage histMsg = (HistoricalDataMessage)message;
                    DateTime time = DateTime.ParseExact(histMsg.Date.Trim(), "yyyyMMdd  HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
                    if (series != null && series.ContainsKey(time))
                    {
                        PopulateHistGrid(message);
                    }
                    appForm.showMessage("test 1 : " + watch.ElapsedMilliseconds + " millsecond");
                    break;
                case MessageType.HistoricalDataEnd:
                    PaintChart();
                    break;
            }
        }

        private void PaintChart()
        {
            DateTime dt;
            Chart historicalChart = (Chart)uiControl;
            for (int i = 0; i < historicalData.Count; i++)
            {
                if (historicalData[i].Date.Length == fullDatePattern.Length)
                    DateTime.TryParseExact(historicalData[i].Date, fullDatePattern, null, DateTimeStyles.None, out dt);
                else if (historicalData[i].Date.Length == yearMonthDayPattern.Length)
                    DateTime.TryParseExact(historicalData[i].Date, yearMonthDayPattern, null, DateTimeStyles.None, out dt);
                else
                    continue;

                // adding date and high
                historicalChart.Series[0].Points.AddXY(dt, historicalData[i].High);
                // adding low
                historicalChart.Series[0].Points[i].YValues[1] = historicalData[i].Low;
                //adding open
                historicalChart.Series[0].Points[i].YValues[2] = historicalData[i].Open;
                // adding close
                historicalChart.Series[0].Points[i].YValues[3] = historicalData[i].Close;
                PopulateGrid(historicalData[i]);
            }
        }

        //used to update the historical data grid
        protected void PopulateHistGrid(IBMessage message)
        {
            HistoricalDataMessage bar = (HistoricalDataMessage)message;

           // DateTime start = new DateTime(1970, 1, 1, 0, 0, 0);

            //DateTime dt = start.AddMilliseconds(bar.Timestamp * 1000).ToLocalTime();
            //String strDT = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

            gridView.Rows.Add(1);
            //gridView[0, gridView.Rows.Count - 1].Value = bar.Date;

            //gridView[0, gridView.Rows.Count - 1].Value = strDT;
            //gridView[0, gridView.Rows.Count - 1].Value = bar.RequestId;
            gridView[1, gridView.Rows.Count - 1].Value = bar.Date;
            gridView[2, gridView.Rows.Count - 1].Value = bar.Close ;
            gridView[3, gridView.Rows.Count - 1].Value = bar.Open;
            gridView[4, gridView.Rows.Count - 1].Value = bar.High;
            gridView[5, gridView.Rows.Count - 1].Value = bar.Low;
            gridView[6, gridView.Rows.Count - 1].Value = bar.Volume;
        }
        protected void PopulateGrid(IBMessage message)
        {
            HistoricalDataMessage bar = (HistoricalDataMessage)message;

            RealTimeBarMessage rtBar = (RealTimeBarMessage)message;

            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0);

            DateTime dt = start.AddMilliseconds(rtBar.Timestamp * 1000).ToLocalTime();
            String strDT = String.Format("{0:yyyyMMdd HH:mm:ss}", dt);

            gridView.Rows.Add(1);
            //gridView[0, gridView.Rows.Count - 1].Value = bar.Date;

            gridView[0, gridView.Rows.Count - 1].Value = strDT;
            gridView[1, gridView.Rows.Count - 1].Value = bar.Close ;
            gridView[2, gridView.Rows.Count - 1].Value = bar.Open;
            gridView[3, gridView.Rows.Count - 1].Value = bar.High;
            gridView[4, gridView.Rows.Count - 1].Value = bar.Low;
            gridView[5, gridView.Rows.Count - 1].Value = rtBar.LongVolume;

            /*
            gridView[0, gridView.Rows.Count - 1].Value = bar.Date;
            gridView[1, gridView.Rows.Count - 1].Value = bar.Open;
            gridView[2, gridView.Rows.Count - 1].Value = bar.High;
            gridView[3, gridView.Rows.Count - 1].Value = bar.Low;
            gridView[4, gridView.Rows.Count - 1].Value = bar.Close;
            gridView[5, gridView.Rows.Count - 1].Value = bar.Volume;
            gridView[6, gridView.Rows.Count - 1].Value = bar.Wap;
             * */
        }
    }
}
