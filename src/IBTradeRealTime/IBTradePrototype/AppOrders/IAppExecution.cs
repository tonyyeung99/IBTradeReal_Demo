using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.AppOrders
{
    public interface IAppExecution
    {
        int OrderId { get; set; }
        string ExecId { get; set; }
        String LastExecTime { get; set; }
        string Side { get; set; }
        double AvgPrice { get; set; }
        int ExeShare { get; set; }
    }
}
