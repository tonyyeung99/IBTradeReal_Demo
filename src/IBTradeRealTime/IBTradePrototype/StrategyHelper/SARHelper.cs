using Deedle;
using IBTradeRealTime.MarketData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.StrategyHelper
{
    class SARHelper
    {
        public static List<CSARRow> addFirstRow(SARRow sarRow)
        {
            List<CSARRow> rows = new List<CSARRow>();
            CSARRow row = new CSARRow();
            row.direction = sarRow.direction;
            row.endSarValue = sarRow.sarValue;
            row.endTime = sarRow.time;
            row.startSarValue = sarRow.sarValue;
            row.startTime = sarRow.time;
            rows.Add(row);
            return rows;
        }

        public static List<CSARRow> addNewRow(SARRow sarRow, List<CSARRow> rows)
        {
            CSARRow lastRow = rows[rows.Count - 1];
            if (lastRow.direction == sarRow.direction)
            {
                lastRow.endSarValue = sarRow.sarValue;
                lastRow.endTime = sarRow.time;
                return rows;
            }
            else
            {
                CSARRow row = new CSARRow();
                row.direction = sarRow.direction;
                row.endSarValue = sarRow.sarValue;
                row.endTime = sarRow.time;
                row.startSarValue = sarRow.sarValue;
                row.startTime = sarRow.time;
                rows.Add(row);
                return rows;
            }


        }


        public static List<SARRow> addFirstRow(Series<DateTime, MarketDataElement> series, int period, DateTime startTime, double af, double max_af)
        {
            List<SARRow> resultRow = new List<SARRow>();
            MarketDataElement lastData = series.GetAt(series.KeyCount - 1);
            Series<DateTime, MarketDataElement> lastNData = MarketDateElementHelper.getLastNMarketData(series, period);
            List<MarketDataElement> lstData = lastNData.Values.ToList();
            SARRow row = new SARRow();
            foreach (MarketDataElement data in lstData)
            {
                if (row.sarValue == 0)
                    row.sarValue = Math.Min(data.open, data.close);
                else
                {
                    row.sarValue = Math.Min(row.sarValue, Math.Min(data.open, data.close));
                }

                if (row.extremePoint == 0)
                    row.extremePoint = Math.Max(data.open, data.close);
                else
                {
                    row.extremePoint = Math.Max(row.extremePoint, Math.Max(data.open, data.close));
                }
            }

            if (Math.Min(lastData.open, lastData.close) > row.sarValue)
                row.direction = 1;
            else
                row.direction = -1;

            row.delta_EP_SAR = row.extremePoint - row.sarValue;
            row.accFactor = af;
            row.product_af_delta = row.accFactor * row.delta_EP_SAR;
            row.time = lastData.time;
            resultRow.Add(row);
            return resultRow;
        }

        public static List<SARRow> addNewRow(List<SARRow> rows, Series<DateTime, MarketDataElement> series, DateTime startTime, double af, double max_af)
        {
            MarketDataElement currentData = series.GetAt(series.KeyCount - 1);
            MarketDataElement lastData = series.Get(currentData.time, Lookup.Smaller);
            MarketDataElement last2ndData = series.Get(lastData.time, Lookup.Smaller);
            SARRow lastRow = rows[rows.Count - 1];
            //SARRow last2ndRow = rows[rows.Count - 2];
            int last2ndDirection = 0;
            if (rows.Count > 1)
                last2ndDirection = rows[rows.Count - 2].direction;
            else
                last2ndDirection = 1;

            SARRow currentRow = new SARRow();
            double currentLow = Math.Min(currentData.open, currentData.close);
            double lastTLow = Math.Min(lastData.open, lastData.close);
            double last2ndTLow = Math.Min(last2ndData.open, last2ndData.close);

            double currentHigh = Math.Max(currentData.open, currentData.close);
            double lastTHigh = Math.Max(lastData.open, lastData.close);
            double last2ndTHigh = Math.Max(last2ndData.open, last2ndData.close);
            // sarValue
            if (lastRow.direction == 1 && last2ndDirection == 1)
            {
                double newSAR = lastRow.sarValue + lastRow.product_af_delta;
                currentRow.sarValue = Math.Min(newSAR, Math.Min(lastTLow, last2ndTLow));
            }

            if (lastRow.direction == -1 && last2ndDirection == -1)
            {
                double newSAR = lastRow.sarValue + lastRow.product_af_delta;
                currentRow.sarValue = Math.Max(newSAR, Math.Max(lastTHigh, last2ndTHigh));
            }

            if (lastRow.direction * last2ndDirection == -1)
            {
                currentRow.sarValue = lastRow.extremePoint;
                /*
                if (lastRow.direction == 1)
                {
                    currentRow.sarValue = lastRow.extremePoint + Math.Abs(currentData.low - lastRow.extremePoint) / 75;
                }
                else
                {
                    currentRow.sarValue = lastRow.extremePoint - Math.Abs(lastRow.extremePoint - currentData.high) / 75;
                }*/


            }

            //ep, delta_EP_SAR
            if (lastRow.direction == 1)
            {
                currentRow.extremePoint = Math.Max(currentHigh, lastRow.extremePoint);
            }
            else if (lastRow.direction == -1)
            {
                currentRow.extremePoint = Math.Min(currentLow, lastRow.extremePoint);
            }
            currentRow.delta_EP_SAR = currentRow.extremePoint - currentRow.sarValue;

            //direction
            if (lastRow.direction == 1)
            {
                if (currentLow > currentRow.sarValue)
                    currentRow.direction = 1;
                else
                    currentRow.direction = -1;
            }

            if (lastRow.direction == -1)
            {
                if (currentHigh < currentRow.sarValue)
                    currentRow.direction = -1;
                else
                    currentRow.direction = 1;
            }

            //af
            if (currentRow.direction == 1 && lastRow.direction == 1)
            {
                if (currentRow.extremePoint > lastRow.extremePoint)
                {
                    if (lastRow.accFactor >= max_af)
                        currentRow.accFactor = max_af;
                    else
                        currentRow.accFactor = lastRow.accFactor + af;
                }
                else
                    currentRow.accFactor = lastRow.accFactor;
            }

            if (currentRow.direction == -1 && lastRow.direction == -1)
            {
                if (currentRow.extremePoint < lastRow.extremePoint)
                {
                    if (lastRow.accFactor >= max_af)
                        currentRow.accFactor = max_af;
                    else
                        currentRow.accFactor = lastRow.accFactor + af;
                }
                else
                    currentRow.accFactor = lastRow.accFactor;
            }

            if (currentRow.direction * lastRow.direction == -1)
            {
                currentRow.accFactor = af;
            }
            currentRow.accFactor = Math.Round(currentRow.accFactor, 3);
            //product_af_delta
            currentRow.product_af_delta = currentRow.accFactor * currentRow.delta_EP_SAR;
            currentRow.time = currentData.time;
            rows.Add(currentRow);
            return rows;
        }
    }
}
