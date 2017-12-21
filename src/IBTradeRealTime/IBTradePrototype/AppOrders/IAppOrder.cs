using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.AppOrders
{
    public interface IAppOrder : ICloneable
    {
        int StrategyIndex { get; set; }
        String StratgeyShortName { get; set; }
        String BuySell { get; set; }
        String TriggerRule { get; set; }
        String Remark { get; set; }
        double TriggerPrice { get; set; }
        DateTime TriggerTime { get; set; }
        Order IBOrder { get; set; }
        int Position { get; set; } 
        int OrderId { get; set; }
        int TotalQuantity { get; set; }
        string OrderType { get; set; }
        double LmtPrice { get; set; }
        double AuxPrice { get; set; }
        List<IAppExecution> Executions { get; set; }
        String Stuatus { get; set; }
        void addExecution(IAppExecution execution);
    }
}
