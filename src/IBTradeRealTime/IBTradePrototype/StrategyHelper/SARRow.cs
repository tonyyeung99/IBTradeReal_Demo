using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.StrategyHelper
{
    class SARRow
    {
        public DateTime time { get; set; }
        public double sarValue { get; set; }
        public double extremePoint { get; set; }
        public double delta_EP_SAR { get; set; }
        public double accFactor { get; set; }
        public double product_af_delta { get; set; }
        public int direction { get; set; }

        public String ToString()
        {
            String objString = "SARRow : time={1}| sarValue={2}| extremePoint={3}| delta_EP_SAR={4}| accFactor={5}| product_af_delta={6}| direction={7}";
            objString = objString.Replace("{1}", time.ToString()).Replace("{2}", sarValue.ToString());
            objString = objString.Replace("{3}", extremePoint.ToString()).Replace("{4}", delta_EP_SAR.ToString()).Replace("{5}", accFactor.ToString()).Replace("{6}", product_af_delta.ToString());
            objString = objString.Replace("{7}", direction.ToString());
            return objString;
        }
    }
}
