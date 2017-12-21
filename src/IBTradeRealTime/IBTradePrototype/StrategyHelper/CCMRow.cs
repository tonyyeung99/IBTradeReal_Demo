using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.StrategyHelper
{
    class CCMRow
    {

        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public double deltaPt { get; set; }
        public double startPt { get; set; }
        public double endPt { get; set; }
        public Boolean isMomentumDetected { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public CCMRow(CMRow cmRow)
        {
            this.startTime = cmRow.startTime;
            this.endTime = cmRow.endTime;
            this.deltaPt = cmRow.deltaPt;
            this.startPt = cmRow.startPt;
            this.endPt = cmRow.endPt;
            this.isMomentumDetected = cmRow.isMomentumDetected;
        }

        public String ToString()
        {
            String objString = "CMRow : startTime={1}| endTime={2}| startPt={3}| endPt={4}| deltaPt={5}| isMomentumDetected{6}";
            objString = objString.Replace("{1}", startTime.ToString()).Replace("{2}", endTime.ToString());
            objString = objString.Replace("{3}", startPt.ToString()).Replace("{4}", endPt.ToString()).Replace("{5}", deltaPt.ToString()).Replace("{6}", isMomentumDetected.ToString());

            return objString;
        }
    }
}
