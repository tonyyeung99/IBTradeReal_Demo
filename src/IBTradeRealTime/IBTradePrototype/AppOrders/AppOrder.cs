using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.AppOrders
{
    public class AppOrder : IAppOrder
    {
        public AppOrder()
        {
            Executions = new List<IAppExecution>();
        }
        public int StrategyIndex { get; set; }
        public String StratgeyShortName { get; set; }
        public String BuySell { get; set; }
        public double EnterPrice { get; set; }
        public String TriggerRule { get; set; }
        public String Remark { get; set; }
        public double TriggerPrice { get; set; }
        public DateTime TriggerTime { get; set; }
        public Order IBOrder { get; set; }
        public int Position { get; set; } 

        public int OrderId { get; set; }
        public int TotalQuantity { get; set; }
        public String OrderType { get; set; }
        public double LmtPrice { get; set; }
        public double AuxPrice { get; set; }
        public List<IAppExecution> Executions { get; set; }
        public String Stuatus { get; set; }
        public void addExecution(IAppExecution execution)
        {
            Executions.Add(execution);
        }

        public object Clone()
        {
            AppOrder newAppOrder = (AppOrder)this.MemberwiseClone();
            List<IAppExecution> newExecutions = new List<IAppExecution>();
            foreach (IAppExecution exe in Executions)
            {
                newExecutions.Add(exe);
            }
            newAppOrder.Executions = newExecutions;
            return newAppOrder;
        }
    }
}
