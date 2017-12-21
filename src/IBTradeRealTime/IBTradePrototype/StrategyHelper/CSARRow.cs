using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.StrategyHelper
{
    class CSARRow
    {
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public double startSarValue { get; set; }
        public double endSarValue { get; set; }
        public int direction { get; set; }

        public String ToString()
        {
            String objString = "SARRow : startTime={1}| endTime={2}| startSarValue={3}| endSarValue={4}| direction={5}";
            objString = objString.Replace("{1}", startTime.ToString()).Replace("{2}", endTime.ToString());
            objString = objString.Replace("{3}", startSarValue.ToString()).Replace("{4}", endSarValue.ToString()).Replace("{5}", direction.ToString());
            return objString;
        }
    }
}
