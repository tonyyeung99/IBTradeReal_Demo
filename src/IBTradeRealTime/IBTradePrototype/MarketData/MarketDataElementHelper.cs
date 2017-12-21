using Deedle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.MarketData
{
    class MarketDateElementHelper
    {
        public static Series<DateTime, MarketDataElement> getLastNMarketData(Series<DateTime, MarketDataElement> series, int n)
        {
            MarketDataElement lastItem = series.GetAt(series.KeyCount - 1);
            DateTime tempStartTime = lastItem.time.AddMinutes(-n + 1);
            Series<DateTime, MarketDataElement> tempSeries = series.Between(tempStartTime, lastItem.time);
            if (tempSeries.KeyCount == n)
                return tempSeries;
            //bool found=false;
            while (true)
            {
                //log.Info("testing 3");
                tempStartTime = tempStartTime.AddMinutes(-1);
                tempSeries = series.Between(tempStartTime, lastItem.time);
                if (tempSeries.KeyCount == n)
                    return tempSeries;
            }

        }

        public static MarketDataElement getFirstAvaMarketData(Series<DateTime, MarketDataElement> series, DateTime startTime)
        {
            DateTime endTime = series.GetAt(series.KeyCount - 1).time;
            int diff = (int)(endTime - startTime).TotalMinutes;
            for (int i = 0; i <= diff; i++)
            {
                DateTime tempTime = startTime.AddMinutes(i);
                if (series.TryGet(tempTime).HasValue)
                {
                    return series.Get(tempTime);
                }
            }
            return null;
        }
    }
}
