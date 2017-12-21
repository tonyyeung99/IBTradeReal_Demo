using Deedle;
using IBTradeRealTime.MarketData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.StrategyHelper
{
    class cmResult
    {
        public double maxThreshold { get; set; }
        public double averageThreshold { get; set; }
    }

    class CCMHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static List<CMRow> calculate(List<CMRow> rows, Series<DateTime, MarketDataElement> series, int period, DateTime startTime)
        {

            Series<DateTime, MarketDataElement> lastNData = MarketDateElementHelper.getLastNMarketData(series, period);
            MarketDataElement firstData = MarketDateElementHelper.getFirstAvaMarketData(lastNData, startTime);
            MarketDataElement lastData = series.GetAt(series.KeyCount - 1);
            double open = firstData.open;
            double close = lastData.close;
            double delta = Math.Abs(close - open);

            CMRow row = new CMRow();
            row.startTime = firstData.time;
            row.startPt = firstData.open;
            row.endTime = lastData.time;
            row.endPt = lastData.close;
            row.deltaPt = delta;
            rows.Add(row);
            return rows;
        }

        public static List<CMRow> cloneRows(List<CMRow> rows)
        {
            List<CMRow> cRows = new List<CMRow>();
            foreach (CMRow row in rows)
            {
                CMRow cRow = row.Clone() as CMRow;
                cRows.Add(cRow);
            }
            return cRows;
        }

        public static List<CMRow> firstNRow(List<CMRow> rows, int n)
        {
            List<CMRow> resultRow = new List<CMRow>();
            for (int i = 0; i < n; i++)
            {
                resultRow.Add(rows[i]);
            }
            return resultRow;
        }

        public static double findEnterPtTriggerMomentum(List<CMRow> rows, Series<DateTime, MarketDataElement> series, int period, DateTime startTime, double delta, String buySell)
        {
            Series<DateTime, MarketDataElement> lastNData = MarketDateElementHelper.getLastNMarketData(series, period-1);
            MarketDataElement firstData = MarketDateElementHelper.getFirstAvaMarketData(lastNData, startTime);

            MarketDataElement lastData = series.GetAt(series.KeyCount - 1);
            //log.Info("firstData.open : " + firstData.open);
            double open = firstData.open;
            if (AppConstant.BUY_SIGNAL.Equals(buySell))
                return open + delta;
            else
                return open - delta;
        }

        public static cmResult calculateMaxAndAvg(List<CMRow> rows, int period, double pctRemove)
        {

            List<CMRow> _clnRows = new List<CMRow>();
            _clnRows = cloneRows(rows);
            CMRow currentRow = rows[rows.Count - 1];
            _clnRows.Reverse();
            List<CMRow> lastNRows = firstNRow(_clnRows, period);
            lastNRows.Sort(delegate(CMRow x, CMRow y)
            {
                if (x.deltaPt > y.deltaPt)
                    return 1;
                if (x.deltaPt < y.deltaPt)
                    return -1;
                if (x.deltaPt == y.deltaPt)
                    return 0;
                return 0;
            });

            double halfPctRemove = pctRemove / 2;
            //halfPctRemove = Math.Round(halfPctRemove, 1);
            int numExcluded = Convert.ToInt32(Math.Floor(period * halfPctRemove));
            int numTooHigh = numExcluded * 2;
            int indexTooHighInclude = period - numTooHigh - 1;
            int startIncludeIndex = 0 + numExcluded;
            int endIncludeIndex = period - numExcluded - 1;

            double average = 0;
            double valueTooHigh = 0;
            for (int i = 0; i < lastNRows.Count; i++)
            {
                if (i >= startIncludeIndex && i <= endIncludeIndex)
                {
                    average += lastNRows[i].deltaPt;
                }

                if (i == indexTooHighInclude)
                    valueTooHigh = lastNRows[i].deltaPt;
            }

            average = average / (lastNRows.Count - numExcluded * 2);
            cmResult result = new cmResult();
            result.averageThreshold = average;
            result.maxThreshold = valueTooHigh;
            return result;
            //if (currentRow.deltaPt >= valueTooHigh && currentRow.deltaPt >= average * 3 )
            //    return true;
            //return false;
        }

        public static double findMaxDeltaTriggerMomentum(List<CMRow> rows, int period, double averageThreshold, double pctRemove)
        {
            cmResult result = calculateMaxAndAvg(rows, period, pctRemove);
            //log.Info("result.averageThreshold=" + result.averageThreshold);
            //log.Info("result.maxThreshold=" + result.maxThreshold);
            return Math.Max(result.averageThreshold * averageThreshold, result.maxThreshold);
            /*
            List<CMRow> _clnRows = new List<CMRow>();
            _clnRows = cloneRows(rows);
            CMRow currentRow = rows[rows.Count - 1];

            _clnRows.Reverse();
            List<CMRow> lastNRows = firstNRow(_clnRows, period);

            lastNRows.Sort(delegate(CMRow x, CMRow y)
            {
                if (x.deltaPt > y.deltaPt)
                    return 1;
                if (x.deltaPt < y.deltaPt)
                    return -1;
                if (x.deltaPt == y.deltaPt)
                    return 0;
                return 0;
            });

            int numExcluded = Convert.ToInt32(Math.Floor(period * 0.1));
            int numTooHigh = numExcluded * 2;
            int indexTooHighInclude = period - numTooHigh - 1;
            int startIncludeIndex = 0 + numExcluded;
            int endIncludeIndex = period - numExcluded - 1;

            double average = 0;
            double valueTooHigh = 0;
            for (int i = 0; i < lastNRows.Count; i++)
            {
                if (i >= startIncludeIndex && i <= endIncludeIndex)
                {
                    average += lastNRows[i].deltaPt;
                }

                if (i == indexTooHighInclude)
                    valueTooHigh = lastNRows[i].deltaPt;
            }
            average = average / (lastNRows.Count - numExcluded * 2) * 2;

            return Math.Max(average, valueTooHigh);
            */
        }

        public static Boolean isMomentumDetected(List<CMRow> rows, int period, double averageThreshold, double pctRemove)
        {
            cmResult result = calculateMaxAndAvg(rows, period, pctRemove);
            CMRow currentRow = rows[rows.Count - 1];
            if (currentRow.deltaPt >= result.maxThreshold && currentRow.deltaPt >= result.averageThreshold * averageThreshold)
                return true;
            return false;
            /*
            List<CMRow> _clnRows =  new List<CMRow> ();
            _clnRows = cloneRows(rows);
            CMRow currentRow = rows[rows.Count - 1];
            _clnRows.Reverse();
            List<CMRow> lastNRows = firstNRow(_clnRows, period);
            lastNRows.Sort(delegate(CMRow x, CMRow y)
            {
                if (x.deltaPt > y.deltaPt)
                    return 1;
                if (x.deltaPt < y.deltaPt)
                    return -1;
                if (x.deltaPt == y.deltaPt)
                    return 0;
                return 0;
            });

            int numExcluded = Convert.ToInt32(Math.Floor(period * 0.1));
            int numTooHigh = numExcluded * 2;
            int indexTooHighInclude = period -numTooHigh-1;
            int startIncludeIndex = 0 + numExcluded ;
            int endIncludeIndex = period - numExcluded -1;

            double average = 0;
            double valueTooHigh = 0;
            for (int i = 0; i < lastNRows.Count; i++)
            {
                if (i >= startIncludeIndex && i <= endIncludeIndex) {
                    average += lastNRows[i].deltaPt;
                }
                
                if (i == indexTooHighInclude)
                    valueTooHigh = lastNRows[i].deltaPt;
            }
            average = average / (lastNRows.Count - numExcluded * 2);
            if (currentRow.deltaPt >= valueTooHigh && currentRow.deltaPt >= average * 3 )
                return true;
            return false;
             */
        }

        public static CMRow mergeRow(CMRow row1, CMRow row2)
        {
            double direction1 = row1.endPt - row1.startPt;
            double direction2 = row2.endPt - row2.startPt;

            if (direction1 * direction2 > 0 && row1.isMomentumDetected && row2.isMomentumDetected)
            {
                CMRow newRow = new CMRow();
                newRow.startPt = row1.startPt;
                newRow.startTime = row1.startTime;
                newRow.endPt = row2.endPt;
                newRow.endTime = row2.endTime;
                newRow.deltaPt = Math.Abs(newRow.endPt - newRow.startPt);
                newRow.isMomentumDetected = true;
                return newRow;
            }
            else
            {
                return null;
            }
            return null;
        }

        public static CCMRow mergeRow(CCMRow row1, CCMRow row2)
        {
            double direction1 = row1.endPt - row1.startPt;
            double direction2 = row2.endPt - row2.startPt;

            Boolean isConflict = false;
            if (row1.startTime <= row2.startTime && row1.endTime >= row2.startTime)
            {
                isConflict = true;
            }


            if (direction1 * direction2 > 0 && row1.isMomentumDetected && row2.isMomentumDetected && isConflict)
            {
                CCMRow newRow = (CCMRow)row2.Clone();
                newRow.startPt = row1.startPt;
                newRow.startTime = row1.startTime;
                newRow.endPt = row2.endPt;
                newRow.endTime = row2.endTime;
                newRow.deltaPt = Math.Abs(newRow.endPt - newRow.startPt);
                newRow.isMomentumDetected = true;
                return newRow;
            }
            else
            {
                return null;
            }
            return null;
        }

        public static CMRow resolveConflictRow(CMRow row1, CMRow row2)
        {
            Boolean isConflict = false;
            if (row1.startTime <= row2.startTime && row1.endTime >= row2.startTime)
            {
                isConflict = true;
            }

            if (isConflict)
            {
                CMRow newRow = (CMRow)row2.Clone();
                newRow.startTime = row1.endTime.AddMinutes(1);
                return newRow;
            }
            else
            {
                return null;
            }
            return null;
        }

        public static CCMRow resolveConflictRow(CCMRow row1, CCMRow row2)
        {
            Boolean isConflict = false;
            if (row1.startTime <= row2.startTime && row1.endTime >= row2.startTime)
            {
                isConflict = true;
            }

            if (isConflict)
            {
                CCMRow newRow = (CCMRow)row2.Clone();
                newRow.startTime = row1.endTime.AddMinutes(1);
                return newRow;
            }
            else
            {
                return null;
            }
            return null;
        }

        public static CCMRow resolveConflictRow(List<CCMRow> rows, CCMRow row2)
        {
            Boolean isConflict = false;

            CCMRow row1 = rows[rows.Count - 2];

            for (int i = 0; i < rows.Count - 1; i++)
            {
                CCMRow tempRow = rows[i];

                if (tempRow.startTime <= row2.startTime && tempRow.endTime >= row2.startTime)
                {
                    isConflict = true;
                    break;
                }
            }

            if (isConflict)
            {
                CCMRow newRow = (CCMRow)row2.Clone();
                newRow.startTime = row1.endTime.AddMinutes(1);
                return newRow;
            }
            else
            {
                return null;
            }
            return null;
        }
    }
}
