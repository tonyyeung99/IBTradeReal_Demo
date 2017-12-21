using IBTradeRealTime.app;
using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using IBTradeRealTime.message;
using IBTradeRealTime.UI;
using IBTradeRealTime.Strategy;
using IBTradeRealTime.util;

namespace IBTradeRealTime.AppOrders
{
    public interface IAppOrderManager
    {
        void injectStrategyManager(IAppStrategyManager stgManager);
        IAppStrategyManager getStgManager(); 	    
        ConcurrentDictionary<int, IAppOrder> AppOrderStore { get; set; }
        ConcurrentDictionary<String, List<IAppOrder>> StoreStgOpenOrders { get; set; }
        ConcurrentDictionary<String, List<IAppOrder>> StoreStgClosedOrders { get; set; }
        ConcurrentDictionary<String, AppPosition> StoreStgPositions { get; set; }
        ConcurrentDictionary<String, String> ProcessedExecution { get; set; }
        ConcurrentDictionary<String, OrderRecord> OrderRepositry { get; set; }
        OrderPersistHelper OrderPersister { get; set; }
        void addAppOrUpdateOrder(IAppOrder order);
        //AppOrder getAppOrderFromStore(int orderId);
        //AppOrder getOpenAppOrderFromStore(int stgIndex, int orderId);
        //AppOrder getClosedAppOrderFromStore(int stgIndex, int orderId);
        void addOpenAppOrder(int stgIndex, IAppOrder order);
        void closeOpenAppOrder(int stgIndex, IAppOrder order);
        void updatePosition(int stgIndex, IAppExecution execution);
        AppExecution updateAppOrderExecution(ExecutionMessage message);
        Boolean isExecutionProcessed(String executionId);
        void markExeProcessed(String executionId);
        void addOrderRecordToRepositry(String orderKey, OrderRecord orderR);
        void dailyReset();
    }
}
