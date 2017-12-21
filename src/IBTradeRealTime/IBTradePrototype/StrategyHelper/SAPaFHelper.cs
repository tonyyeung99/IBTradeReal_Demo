using IBTradeRealTime.MarketData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.StrategyHelper
{
    class SAPaFHelper
    {
        public const String RISE = "R";
        public const String DROP = "D";
        public const double INVALID_NUM = -1;
        public const double CELL_UNIT = 5;

        public const double REVERSAL_PTS = 20;
        public const double LARGE_EXTEND_THRESHOLD = 80;
        public const double TAILING_BACK_RATIO = 0.5;


        /*static private double getMaxLevel(double max, String direction)
        {
            double residure = max % CELL_UNIT;

            if (RISE.Equals(direction))
                return max - residure;

            if (DROP.Equals(direction))
                return max + (CELL_UNIT - residure);

            return INVALID_NUM;
        }*/

        static private Boolean isOverLastMax(String lastDir, MarketDataElement item, double lastMax)
        {
            if (RISE.Equals(lastDir))
            {
                if (item.high > lastMax)
                    return true;
                else
                    return false;
            }
            else
            {
                if (item.low < lastMax)
                    return true;
                else
                    return false;
            }

        }

        static private Boolean isOverLastMax(String lastDir, double tickPrice, double lastMax)
        {
            if (RISE.Equals(lastDir))
            {
                if (tickPrice > lastMax)
                    return true;
                else
                    return false;
            }
            else
            {
                if (tickPrice < lastMax)
                    return true;
                else
                    return false;
            }

        }

        static private Boolean isReverseThreshold(String lastDir, double maxLevel, MarketDataElement item, double threshold)
        {
            double rThreshold;
            if (RISE.Equals(lastDir))
            {
                rThreshold = maxLevel - threshold;
                if (item.low <= rThreshold)
                    return true;
                else
                    return false;
            }
            else
            {
                rThreshold = maxLevel + threshold;
                if (item.high >= rThreshold)
                    return true;
                else
                    return false;
            }
        }

        static private Boolean isReverseThreshold(String lastDir, double maxLevel, double tickPrice, double threshold)
        {
            double rThreshold;
            if (RISE.Equals(lastDir))
            {
                rThreshold = maxLevel - threshold;
                if (tickPrice <= rThreshold)
                    return true;
                else
                    return false;
            }
            else
            {
                rThreshold = maxLevel + threshold;
                if (tickPrice >= rThreshold)
                    return true;
                else
                    return false;
            }
        }

        static private String getOppDirection(String dir)
        {
            if (RISE.Equals(dir))
                return DROP;
            else
                return RISE;
        }

        static private double getNewReverseMaxValue(String lastDir, MarketDataElement item)
        {
            if (RISE.Equals(lastDir))
                return item.low;
            else
                return item.high;
        }

        static private double getNewReverseMaxValue(String lastDir, double tickPrice)
        {
            if (RISE.Equals(lastDir))
                return tickPrice;
            else
                return tickPrice;
        }

        static private double getUpdateMaxValue(String lastDir, MarketDataElement item)
        {
            if (RISE.Equals(lastDir))
                return item.high;
            else
                return item.low;
        }

        static private double getUpdateMaxValue(String lastDir, double tickPrice)
        {
            if (RISE.Equals(lastDir))
                return tickPrice;
            else
                return tickPrice;
        }

        static public List<SAPaFRow> addFirstNode(MarketDataElement item)
        {
            double open = item.open;
            double close = item.close;
            String direction = "";
            if (open > close)
                direction = DROP;
            else
                direction = RISE;

            List<SAPaFRow> rows = new List<SAPaFRow>();

            SAPaFRow row = new SAPaFRow();
            row.direction = direction;

            if (DROP.Equals(direction))
            {
                row.startPt = item.high;
                row.maxPt = item.low;
            }
            else
            {
                row.startPt = item.low;
                row.maxPt = item.high;
            }
            //row.maxLevel = getMaxLevel(row.maxPt, direction);
            row.startTime = item.time;
            rows.Add(row);
            return rows;
        }

        static public List<SAPaFRow> addNewRow(List<SAPaFRow> rows, MarketDataElement item, double pullBackThreshold)
        {
            SAPaFRow lastRow = rows[rows.Count - 1];
            String lastDir = lastRow.direction;
            double lastStart = lastRow.startPt;
            double lastMax = lastRow.maxPt;
            //double lastMaxLevel = lastRow.maxLevel;
            Boolean lbOverLastMaxLevel = isOverLastMax(lastDir, item, lastMax);
            Boolean lbReverseThreshold = isReverseThreshold(lastDir, lastMax, item, pullBackThreshold);

            String newDir = "";
            double newStart = 0.0;
            double newMax = 0.0;
            //double newMaxLevel = 0.0;

            if (!lbOverLastMaxLevel && lbReverseThreshold)
            {
                newDir = getOppDirection(lastDir);
                newStart = lastMax;
                newMax = getNewReverseMaxValue(lastDir, item);

                SAPaFRow row = new SAPaFRow();
                //newMaxLevel = getMaxLevel(newMax, newDir);
                row.direction = newDir;
                row.startPt = newStart;
                row.maxPt = newMax;
                //row.maxLevel = newMaxLevel;
                row.startTime = item.time;
                rows.Add(row);

            }
            else
            {
                newDir = lastDir;
                newStart = lastStart;
                if (lbOverLastMaxLevel)
                {
                    newMax = getUpdateMaxValue(lastDir, item);

                }
                else
                    newMax = lastMax;

                SAPaFRow row = rows[rows.Count - 1];
                //newMaxLevel = getMaxLevel(newMax, newDir);
                row.direction = newDir;
                row.startPt = newStart;
                row.maxPt = newMax;
                //row.maxLevel = newMaxLevel;
                //row.startTime = item.time;
            }

            return rows;
        }

        static public List<SAPaFRow> addNewRow(List<SAPaFRow> rows, double tickPrice, double pullBackThreshold)
        {
            SAPaFRow lastRow = rows[rows.Count - 1];
            String lastDir = lastRow.direction;
            double lastStart = lastRow.startPt;
            double lastMax = lastRow.maxPt;
            //double lastMaxLevel = lastRow.maxLevel;
            Boolean lbOverLastMaxLevel = isOverLastMax(lastDir, tickPrice, lastMax);
            Boolean lbReverseThreshold = isReverseThreshold(lastDir, lastMax, tickPrice, pullBackThreshold);

            String newDir = "";
            double newStart = 0.0;
            double newMax = 0.0;
            //double newMaxLevel = 0.0;

            if (!lbOverLastMaxLevel && lbReverseThreshold)
            {
                newDir = getOppDirection(lastDir);
                newStart = lastMax;
                newMax = getNewReverseMaxValue(lastDir, tickPrice);

                SAPaFRow row = new SAPaFRow();
                //newMaxLevel = getMaxLevel(newMax, newDir);
                row.direction = newDir;
                row.startPt = newStart;
                row.maxPt = newMax;
                //row.maxLevel = newMaxLevel;
                row.startTime = DateTime.Now;
                rows.Add(row);

            }
            else
            {
                newDir = lastDir;
                newStart = lastStart;
                if (lbOverLastMaxLevel)
                {
                    newMax = getUpdateMaxValue(lastDir, tickPrice);

                }
                else
                    newMax = lastMax;

                SAPaFRow row = rows[rows.Count - 1];
                //newMaxLevel = getMaxLevel(newMax, newDir);
                row.direction = newDir;
                row.startPt = newStart;
                row.maxPt = newMax;
                //row.maxLevel = newMaxLevel;
                //row.startTime = item.time;
            }

            return rows;
        }

        static public String getPaFString(List<SAPaFRow> rows)
        {
            String rowString = "";
            foreach (SAPaFRow row in rows)
            {
                rowString = rowString + row.direction + ";" + row.startPt + ";" + row.maxPt + "|";
            }
            return rowString;
        }

        static public String getPaFTimeString(List<SAPaFRow> rows)
        {
            String rowString = "";
            foreach (SAPaFRow row in rows)
            {
                rowString = rowString + String.Format("{0:MM/dd/yyyy HH:mm:ss}", row.startTime) + ";" + row.direction + ";" + row.startPt + ";" + row.maxPt + "|";
            }
            return rowString;
        }


        static public String getPaFString(List<PaFRow> rows)
        {
            String rowString = "";
            foreach (PaFRow row in rows)
            {
                rowString = rowString + row.direction + ";" + row.startPt + ";" + row.maxPt + "|";
            }
            return rowString;
        }


        static public String getCurrentDir(List<SAPaFRow> rows)
        {
            if (rows != null)
                return rows[rows.Count - 1].direction;
            else
                return null;
        }

        static public double getLastStartPt(List<SAPaFRow> rows, String direction)
        {
            String curDir = rows[rows.Count - 1].direction;
            if (direction.Equals(curDir))
            {
                return rows[rows.Count - 1].startPt;
            }
            else
                if (rows.Count >= 2)
                    return rows[rows.Count - 2].startPt;
                else
                    return INVALID_NUM;
        }

        static public List<SAPaFRow> cloneRows(List<SAPaFRow> rows)
        {
            List<SAPaFRow> cRows = new List<SAPaFRow>();
            foreach (SAPaFRow row in rows)
            {
                SAPaFRow cRow = row.Clone() as SAPaFRow;
                cRows.Add(cRow);
            }
            return cRows;
        }
    }
}
