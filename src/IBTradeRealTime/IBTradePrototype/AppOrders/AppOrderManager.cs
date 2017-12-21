using IBApi;
using IBTradeRealTime.app;
using IBTradeRealTime.message;
using IBTradeRealTime.Strategy;
using IBTradeRealTime.UI;
using IBTradeRealTime.util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.AppOrders
{
    public class AppOrderManager : IAppOrderManager
    {
        IAppStrategyManager stgManager;
        public AppOrderManager(IAppStrategyManager stgManager)
        {
            this.stgManager = stgManager;
            OrderRepositry = new ConcurrentDictionary<string, OrderRecord>();
            OrderPersister = new OrderPersistHelper(AppConstant.FILE_ORDER_REP);
            OrderPersister.renameOldfile();

            String[] stgNames = stgManager.getStgNames();

            AppOrderStore = new ConcurrentDictionary<int, IAppOrder>();
            ProcessedExecution = new ConcurrentDictionary<String, String>();

            StoreStgOpenOrders = new ConcurrentDictionary<string, List<IAppOrder>>();
            foreach (String name in stgNames)
            {
                StoreStgOpenOrders.AddOrUpdate(name, new List<IAppOrder>(), (key, oldValue) => oldValue);
            }

            StoreStgClosedOrders = new ConcurrentDictionary<string, List<IAppOrder>>();
            foreach (String name in stgNames)
            {
                StoreStgClosedOrders.AddOrUpdate(name, new List<IAppOrder>(), (key, oldValue) => oldValue);
            }

            StoreStgPositions = new ConcurrentDictionary<string, AppPosition>();
            foreach (String name in stgNames)
            {
                StoreStgPositions.AddOrUpdate(name, new AppPosition(), (key, oldValue) => oldValue);
            }
        }

        public void dailyReset()
        {
            ProcessedExecution = new ConcurrentDictionary<String, String>();
            OrderRepositry = new ConcurrentDictionary<string, OrderRecord>();
            String[] stgNames = stgManager.getStgNames();
            AppOrderStore  = new ConcurrentDictionary<int, IAppOrder>();
            StoreStgOpenOrders =  new ConcurrentDictionary<string, List<IAppOrder>>();
            foreach (String name in stgNames)
            {
                StoreStgOpenOrders.AddOrUpdate(name, new List<IAppOrder>(), (key, oldValue) => oldValue);
            }
            StoreStgClosedOrders = new ConcurrentDictionary<string, List<IAppOrder>>();
            foreach (String name in stgNames)
            {
                StoreStgClosedOrders.AddOrUpdate(name, new List<IAppOrder>(), (key, oldValue) => oldValue);
            }
            StoreStgPositions = new ConcurrentDictionary<string, AppPosition>();
            foreach (String name in stgNames)
            {
                StoreStgPositions.AddOrUpdate(name, new AppPosition(), (key, oldValue) => oldValue);
            }

        }

        public void injectStrategyManager(IAppStrategyManager stgManager)
        {
            this.stgManager = stgManager;
        }

        public IAppStrategyManager getStgManager()
        {
            return this.stgManager;
        }

        public ConcurrentDictionary<int, IAppOrder> AppOrderStore { get; set; }
        public ConcurrentDictionary<String, List<IAppOrder>> StoreStgOpenOrders { get; set; }
        public ConcurrentDictionary<String, List<IAppOrder>> StoreStgClosedOrders { get; set; }
        public ConcurrentDictionary<String, AppPosition> StoreStgPositions { get; set; }
        public ConcurrentDictionary<String, String> ProcessedExecution { get; set; }
        public ConcurrentDictionary<String, OrderRecord> OrderRepositry { get; set; }
        public OrderPersistHelper OrderPersister { get; set; }

        public void addAppOrUpdateOrder(IAppOrder order)
        {
            lock (AppOrderStore)
            {

                AppOrderStore.AddOrUpdate(order.OrderId, order, (key, oldValue) =>  order );
            }
        }

        /*
        public AppOrder getAppOrderFromStore(int orderId)
        {
            if (AppOrderStore.ContainsKey(orderId))
                return AppOrderStore[orderId];
            return null;
        }

        public AppOrder getOpenAppOrderFromStore(int stgIndex, int orderId)
        {
            String[] stgNames = stgManager.getStgNames();
            List<AppOrder> lstStgOpenOrders = StoreStgOpenOrders[stgNames[stgIndex]];
            AppOrder orderFound = null;
            foreach (AppOrder order in lstStgOpenOrders)
            {
                if (order.OrderId == orderId)
                    orderFound = order;
            }
            return orderFound;
        }

        public AppOrder getClosedAppOrderFromStore(int stgIndex, int orderId)
        {
            String[] stgNames = stgManager.getStgNames();
            List<AppOrder> lstStgClosedOrders = StoreStgClosedOrders[stgNames[stgIndex]];
            AppOrder orderFound = null;
            foreach (AppOrder order in lstStgClosedOrders)
            {
                if (order.OrderId == orderId)
                    orderFound = order;
            }
            return orderFound;
        }*/

        public void addOpenAppOrder(int stgIndex, IAppOrder order)
        {
            String[] stgNames = stgManager.getStgNames();
            List<IAppOrder> lstStgOpenOrders = StoreStgOpenOrders[stgNames[stgIndex]];
            lock (lstStgOpenOrders)
            {
                lstStgOpenOrders.Add(order);
            }
        }

        public void closeOpenAppOrder(int stgIndex, IAppOrder order)
        {
            String[] stgNames = stgManager.getStgNames();
            List<IAppOrder> lstStgOpenOrders = StoreStgOpenOrders[stgNames[stgIndex]];
            List<IAppOrder> lstStgClosedOrder = StoreStgClosedOrders[stgNames[stgIndex]];
            lock (lstStgOpenOrders)
            {
                lock (lstStgClosedOrder)
                {
                    lstStgOpenOrders.Remove(order);
                    lstStgClosedOrder.Add(order);
                }
            }
        }

        public void updatePosition(int stgIndex, IAppExecution execution)
        {
            String[] stgNames = stgManager.getStgNames();
            String side;
            if (AppConstant.BUY_SIGNAL.Equals(execution.Side))
                side = AppConstant.BUY_SIGNAL;
            else
                side = AppConstant.SELL_SIGNAL;

            String stgName = stgNames[stgIndex];
            AppPosition position = StoreStgPositions[stgName];

            lock (position)
            {
                int beforeNetQ = position.NetQ;
                if (AppConstant.BUY_SIGNAL.Equals(side))
                {
                    position.AccBuyQ += execution.ExeShare;
                    position.AccBuyMoney += (execution.ExeShare * execution.AvgPrice);
                    position.NetQ = position.AccBuyQ - position.AccSellQ;
                }
                else
                {
                    position.AccSellQ += execution.ExeShare;
                    position.AccSellMoney += (execution.ExeShare * execution.AvgPrice);
                    position.NetQ = position.AccBuyQ - position.AccSellQ;
                }

                if (beforeNetQ != 0 && position.NetQ == 0)
                {
                    int closedPosition = Math.Min(position.AccBuyQ, position.AccSellQ);
                    double avgBuyPrice = position.AccBuyMoney / position.AccBuyQ;
                    double avgSellPrice = position.AccSellMoney / position.AccSellQ;
                    position.TotalPnL = avgSellPrice * closedPosition - avgBuyPrice * closedPosition;
                }
            }
        }

        public AppExecution updateAppOrderExecution(ExecutionMessage message)
        {
            int orderId = message.Execution.OrderId;
            IAppOrder retOrder = AppOrderStore[orderId];

            String execId = message.Execution.ExecId;
            String lastExecTime = message.Execution.Time;
            String side = retOrder.BuySell;
            double avgPrice = message.Execution.AvgPrice;
            int exeShare = message.Execution.Shares;

            AppExecution execution = new AppExecution();
            execution.OrderId = orderId;
            execution.ExecId = execId;
            execution.LastExecTime = lastExecTime;
            execution.Side = side;
            execution.AvgPrice = avgPrice;
            execution.ExeShare = exeShare;

            retOrder.addExecution(execution);
            return execution;
        }

        public Boolean isExecutionProcessed(String executionId)
        {
            if (ProcessedExecution.ContainsKey(executionId))
                return true;
            else
            {
                return false;
            }
        }

        public void markExeProcessed(String executionId)
        {
            ProcessedExecution.AddOrUpdate(executionId, executionId, (key, oldValue) => oldValue);
        }

        public void addOrderRecordToRepositry(String orderKey, OrderRecord orderR)
        {
            OrderRepositry.AddOrUpdate(orderKey, orderR, (key, oldValue) => oldValue);
        }
    }
}
