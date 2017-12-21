using IBApi;
using IBTradeRealTime.app;
using IBTradeRealTime.AppOrders;
using IBTradeRealTime.Strategy;
using IBTradeRealTime.UI;
using IBTradeRealTime.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.StrategyImpl
{
    public class StrategyHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IAppStrategyManager appStgManager;
        private IAppOrderManager appOrderManager;
        private String stgName;
        private int stgIndex;
        private SignalContext execution;
        private static Object OrderIDLock = new Object();

        public StrategyHelper(IAppStrategyManager appStgManager, String stgName, int stgIndex, SignalContext execution)
        {
            this.appStgManager = appStgManager;
            this.appOrderManager = appStgManager.getAppOrderManager(); 
            this.stgName = stgName;
            this.stgIndex = stgIndex;
            this.execution = execution;
        }

        private AppOrder createAppOrder(String buySell, String triggerRule, double triggerPrice, String remark, Order order, DateTime currentTimeStamp)
        {
            AppOrder appOrder = AppObjectUtil.createAppOrder(stgName, order, stgIndex);
            appOrder.IBOrder = order;
            appOrder.TriggerRule = triggerRule;
            appOrder.TriggerPrice = triggerPrice;
            appOrder.TriggerTime = currentTimeStamp;
            appOrder.Remark = remark;
            return appOrder;
        }

        private Order createMktOrder(String buySell, int size )
        {
            Order order = AppObjectUtil.createMktOrder(buySell, appStgManager.UserAccount, size);
            order.OrderId = appStgManager.ibClient.IncreaseOrderId();
            return order;
        }

        private Order createStopOrder(String buySell, double triggerPrice, int size)
        {
            Order order = AppObjectUtil.createStopOrder(buySell, triggerPrice, appStgManager.UserAccount, size);
            order.OrderId = appStgManager.ibClient.IncreaseOrderId();
            return order;
        }

        private Order createLimitOrder(String buySell, double triggerPrice, int size)
        {
            Order order = AppObjectUtil.createLimitOrder(buySell, triggerPrice, appStgManager.UserAccount, size);
            order.OrderId = appStgManager.ibClient.IncreaseOrderId();
            return order;
        }

        private void setupOrderAndContext(AppOrder appOrder, String buySell, double triggerPrice, String remark, int indexExecution)
        {
            appOrderManager.addAppOrUpdateOrder(appOrder);
            appOrderManager.addOpenAppOrder(stgIndex, appOrder);
            if(indexExecution==1)
                execution.setPendingSignal1(buySell, triggerPrice, appOrder, remark);
            else
                execution.setPendingSignal2(buySell, triggerPrice, appOrder, remark);
        }

        private void persistOrderRecord(AppOrder appOrder, DateTime currentTimeStamp)
        {
            Order order = appOrder.IBOrder;
            OrderRecord orderR = new OrderRecord();
            orderR.orderId = order.OrderId; ;
            orderR.sno = stgName;
            orderR.orderTime = currentTimeStamp;
            appOrderManager.addOrderRecordToRepositry(order.OrderId.ToString(), orderR);
            List<OrderRecord> orderRecords = new List<OrderRecord>();
            orderRecords.Add(orderR);
            appOrderManager.OrderPersister.SaveLastRow(orderRecords);
        }

        public void placeMarketTrade(String buySell, String triggerRule, double triggerPrice, String remark, int size, int indexExecution)
        {
            DateTime currentTimeStamp = DateTime.Now;
            AppOrder appOrder = null; 
            lock (OrderIDLock)
            {                
                Order order = createMktOrder(buySell, size);
                appOrder = createAppOrder(buySell, triggerRule, triggerPrice, remark, order, currentTimeStamp);
                setupOrderAndContext(appOrder, buySell, triggerPrice, remark, indexExecution);
                appStgManager.getOrderManager().PlaceOrder(appStgManager.CurrentContract, order);
            }
            persistOrderRecord(appOrder, currentTimeStamp);
            
        }


        public void placeLimitTrade(String buySell, String triggerRule, double triggerPrice, String remark, int size, int indexExecution)
        {
            DateTime currentTimeStamp = DateTime.Now;
            AppOrder appOrder = null;
            lock (OrderIDLock)
            {
                Order order = createLimitOrder(buySell, triggerPrice, size);
                appOrder = createAppOrder(buySell, triggerRule, triggerPrice, remark, order, currentTimeStamp);
                setupOrderAndContext(appOrder, buySell, triggerPrice, remark, indexExecution);
                appStgManager.getOrderManager().PlaceOrder(appStgManager.CurrentContract, order);
            }
            persistOrderRecord(appOrder, currentTimeStamp);

        }

        public void placeStopTrade(String buySell, String triggerRule, double triggerPrice, String remark, int size, int indexExecution)
        {
            DateTime currentTimeStamp = DateTime.Now;
            AppOrder appOrder = null;
            lock (OrderIDLock)
            {
                Order order = createStopOrder(buySell, triggerPrice, size);
                appOrder = createAppOrder(buySell, triggerRule, triggerPrice, remark, order, currentTimeStamp);
                setupOrderAndContext(appOrder, buySell, triggerPrice, remark, indexExecution);
                appStgManager.getOrderManager().PlaceOrder(appStgManager.CurrentContract, order);
            }
            persistOrderRecord(appOrder, currentTimeStamp);
        }

        public void modifyPendingTrade(int index, double triggerPoint, String triggerRule, string remark)
        {
            DateTime currentTimeStamp = DateTime.Now;
            Contract contract = appStgManager.CurrentContract;
            IAppOrder tempOrder = null;
            if(index==1){
                tempOrder = execution.PendingOrder1;
            }else{
                tempOrder = execution.PendingOrder2;
            }
            tempOrder.IBOrder.AuxPrice = triggerPoint;
            tempOrder.TriggerRule = triggerRule;
            tempOrder.TriggerPrice = triggerPoint;
            tempOrder.Remark = remark;
            appStgManager.getOrderManager().PlaceOrder(contract, tempOrder.IBOrder);
        }

        public void cancelPendingTrade(int index)
        {
            IAppOrder tempOrder = null;
            if (index == 1)
            {
                tempOrder = execution.PendingOrder1;
            }
            else
            {
                tempOrder = execution.PendingOrder2;
            }
            if (tempOrder != null && !AppObjectUtil.isOrderClosed(tempOrder))
            {
                appStgManager.getOrderManager().CancelOrder(tempOrder.IBOrder);
            }
        }

        public void cancelPendingTrades()
        {
            IAppOrder[] appOrders = execution.getPendingOrders();
            for (int i = 0; i < appOrders.Length; i++)
            {
                if(appOrders[i]!=null && !AppObjectUtil.isOrderClosed(appOrders[i])){
                    appStgManager.getOrderManager().CancelOrder(appOrders[i].IBOrder);
                }
            }
        }
    }
}
