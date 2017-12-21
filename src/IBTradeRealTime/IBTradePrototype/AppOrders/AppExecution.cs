using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.AppOrders
{
    public class AppExecution : IAppExecution
    {
        public int OrderId { get; set; }
        public string ExecId { get; set; }
        public String LastExecTime { get; set; }
        public string Side { get; set; }
        public double AvgPrice { get; set; }
        public int ExeShare { get; set; }
    }
}
