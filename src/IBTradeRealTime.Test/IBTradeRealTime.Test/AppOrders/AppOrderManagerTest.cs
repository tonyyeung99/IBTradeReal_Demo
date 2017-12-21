using IBApi;
using IBTradeRealTime.app;
using IBTradeRealTime.AppOrders;
using IBTradeRealTime.message;
using IBTradeRealTime.UI;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TWSLib;

namespace IBTradeRealTime.Test.AppOrders
{
    [TestFixture]
    class AppOrderManagerTest
    {
        [Test]
        public void test_addAppOrUpdateOrder()
        {
            IAppOrderManager orderManager = new AppOrderManager(getStgManager());
            IAppOrder appOrder = createOrder_for_addAppOrUpdateOrder();
            orderManager.addAppOrUpdateOrder(appOrder);
            IAppOrder retAppOrder = orderManager.AppOrderStore[appOrder.OrderId];
            Assert.IsNotNull(retAppOrder);
            Assert.AreEqual(appOrder.OrderId, retAppOrder.OrderId);

        }
        /*
        public void test_getAppOrderFromStore()
        {
        }

        public void test_getOpenAppOrderFromStore()
        {
        }

        public void test_getClosedAppOrderFromStore()
        {
        }
        */
        [Test]
        public void test_addOpenAppOrder()
        {
            IAppStrategyManager stgManager = getStgManager();
            IAppOrderManager orderManager = new AppOrderManager(stgManager);
            IAppOrder appOrder = createOrder_for_addAppOrUpdateOrder();
            int stgIndex = 2;
            orderManager.addOpenAppOrder(stgIndex, appOrder);
            String[] stgNames = stgManager.getStgNames();
            List<IAppOrder> lstStgOpenOrders = orderManager.StoreStgOpenOrders[stgNames[stgIndex]];
            IAppOrder retAppOrder = lstStgOpenOrders[lstStgOpenOrders.Count - 1];
            Assert.IsNotNull(retAppOrder);
            Assert.AreEqual(appOrder.OrderId, retAppOrder.OrderId);
        }

        [Test]
        public void test_closeOpenAppOrder()
        {
            IAppStrategyManager stgManager = getStgManager();
            IAppOrderManager orderManager = new AppOrderManager(stgManager);
            IAppOrder appOrder = createOrder_for_addAppOrUpdateOrder();
            int stgIndex = 2;
            orderManager.closeOpenAppOrder(stgIndex, appOrder);
            String[] stgNames = stgManager.getStgNames();
            List<IAppOrder> lstStgOpenOrders = orderManager.StoreStgOpenOrders[stgNames[stgIndex]];
            List<IAppOrder> lstStgClosedOrders = orderManager.StoreStgClosedOrders[stgNames[stgIndex]];

            IAppOrder retAppOrder = lstStgClosedOrders[lstStgClosedOrders.Count - 1];
            Assert.AreEqual(0, lstStgOpenOrders.Count);
            Assert.IsNotNull(retAppOrder);
            Assert.AreEqual(appOrder.OrderId, retAppOrder.OrderId);

        }

        [Test]
        public void test_updatePosition()
        {
            List<IAppExecution> lstExecution = createExecution_for_updatePosition();
            IAppStrategyManager stgManager = getStgManager();
            IAppOrderManager orderManager = new AppOrderManager(stgManager);
            int stgIndex = 2;
            int numExe = 5;

            int[] accBuyQ = new int[numExe];
            int[] accSellQ = new int[numExe];
            int[] netQ = new int[numExe];
            double[] accBuyMoney = new double[numExe];
            double[] accSellMoney = new double[numExe];
            double[] totalPnL = new double[numExe];

            accBuyQ[0] = 0;
            accSellQ[0] = 6;
            netQ[0] = -6;
            accBuyMoney[0] = 0;
            accSellMoney[0] = 6 * 22100;
            totalPnL[0] = 0;

            accBuyQ[1] = 3;
            accSellQ[1] = 6;
            netQ[1] = -3;
            accBuyMoney[1] = 3 * 22000;
            accSellMoney[1] = 6 * 22100;
            totalPnL[1] = 0;

            accBuyQ[2] = 6;
            accSellQ[2] = accSellQ[1];
            netQ[2] = 0;
            accBuyMoney[2] = accBuyMoney[1] + 3 * 21900;
            accSellMoney[2] = accSellMoney[1];
            totalPnL[2] = 6 * 22100 - (3 * 22000 + 3 * 21900);

            accBuyQ[3] = 7;
            accSellQ[3] = accSellQ[2];
            netQ[3] = 1;
            accBuyMoney[3] = accBuyMoney[2] + 23000;
            accSellMoney[3] = accSellMoney[2];
            totalPnL[3] = totalPnL[2];

            accBuyQ[4] = accBuyQ[3];
            accSellQ[4] = 7;
            netQ[4] = 0;
            accBuyMoney[4] = accBuyMoney[3];
            accSellMoney[4] = accSellMoney[3] + 23500;
            totalPnL[4] = totalPnL[3] + (23500 - 23000);

            orderManager.updatePosition(stgIndex, lstExecution[0]);
            String[] stgNames = stgManager.getStgNames();
            String stgName = stgNames[stgIndex];
            AppPosition position = orderManager.StoreStgPositions[stgName];
            Assert.AreEqual(accSellQ[0], position.AccSellQ);
            Assert.AreEqual(accSellMoney[0], position.AccSellMoney);
            Assert.AreEqual(netQ[0], position.NetQ);

            orderManager.updatePosition(stgIndex, lstExecution[1]);
            Assert.AreEqual(accBuyQ[1], position.AccBuyQ);
            Assert.AreEqual(accBuyMoney[1], position.AccBuyMoney);
            Assert.AreEqual(netQ[1], position.NetQ);
            Assert.AreEqual(totalPnL[1], position.TotalPnL);

            orderManager.updatePosition(stgIndex, lstExecution[2]);
            Assert.AreEqual(accBuyQ[2], position.AccBuyQ);
            Assert.AreEqual(accBuyMoney[2], position.AccBuyMoney);
            Assert.AreEqual(netQ[2], position.NetQ);
            Assert.AreEqual(totalPnL[2], position.TotalPnL);

            orderManager.updatePosition(stgIndex, lstExecution[3]);
            Assert.AreEqual(accBuyQ[3], position.AccBuyQ);
            Assert.AreEqual(accBuyMoney[3], position.AccBuyMoney);
            Assert.AreEqual(netQ[3], position.NetQ);
            Assert.AreEqual(totalPnL[3], position.TotalPnL);

            orderManager.updatePosition(stgIndex, lstExecution[4]);
            Assert.AreEqual(accSellQ[4], position.AccSellQ);
            Assert.AreEqual(accSellMoney[4], position.AccSellMoney);
            Assert.AreEqual(netQ[4], position.NetQ);
            Assert.AreEqual(totalPnL[4], position.TotalPnL);
        }

        [Test]
        public void test_updateAppOrderExecution()
        {
            IAppStrategyManager stgManager = getStgManager();
            IAppOrderManager orderManager = new AppOrderManager(stgManager);
            IAppOrder appOrder = createOrder_Sell3Contract();
            orderManager.AppOrderStore.AddOrUpdate(appOrder.OrderId, appOrder, (key, oldValue) => oldValue);
            ExecutionMessage exeMessage = getExeMessage_for_Sell3Contract();
            orderManager.updateAppOrderExecution(exeMessage);
            IAppOrder retAppOrder = orderManager.AppOrderStore[appOrder.OrderId];

            IAppExecution appExe = retAppOrder.Executions[0];
            Assert.AreEqual(1, retAppOrder.Executions.Count);
            Assert.AreEqual(23000, appExe.AvgPrice);
            Assert.AreEqual("1102", appExe.ExecId);
            Assert.AreEqual(3, appExe.ExeShare);
            Assert.AreEqual("2015-11-11 01:07:01", appExe.LastExecTime);
            Assert.AreEqual(1001, appExe.OrderId);
            Assert.AreEqual(AppConstant.SELL_SIGNAL, appExe.Side);
        }

        [Test]
        public void test_isExecutionProcessed()
        {
            IAppStrategyManager stgManager = getStgManager();
            IAppOrderManager orderManager = new AppOrderManager(stgManager);
            ExecutionMessage exeMessage = getExeMessage_for_Sell3Contract();
            String execId = exeMessage.Execution.ExecId;

            Assert.IsFalse(orderManager.isExecutionProcessed(execId));
            orderManager.ProcessedExecution.AddOrUpdate(execId, execId, (key, oldValue) => oldValue);
            Assert.IsTrue(orderManager.isExecutionProcessed(execId));            
        }

        [Test]
        public void test_markExeProcessed()
        {
            IAppStrategyManager stgManager = getStgManager();
            IAppOrderManager orderManager = new AppOrderManager(stgManager);
            ExecutionMessage exeMessage = getExeMessage_for_Sell3Contract();
            String execId = exeMessage.Execution.ExecId;

            Assert.IsFalse(orderManager.ProcessedExecution.ContainsKey(execId));
            orderManager.markExeProcessed(execId);
            Assert.IsTrue(orderManager.ProcessedExecution.ContainsKey(execId));
        }

        [Test]
        public void test_addOrderRecordToRepositry()
        {
            IAppStrategyManager stgManager = getStgManager();
            IAppOrderManager orderManager = new AppOrderManager(stgManager);
            IAppOrder appOrder = createOrder_Sell3Contract();

            Order order = appOrder.IBOrder;
            String strOrderId = order.OrderId.ToString();
            OrderRecord orderR = new OrderRecord();
            orderR.orderId = order.OrderId; 
            orderR.sno = appOrder.StratgeyShortName;
            orderR.orderTime = new DateTime(2015,12,11,03,03,10);


            Assert.IsFalse(orderManager.OrderRepositry.ContainsKey(strOrderId));
            orderManager.addOrderRecordToRepositry(strOrderId, orderR);
            Assert.IsTrue(orderManager.OrderRepositry.ContainsKey(strOrderId));
            OrderRecord retOrderR = orderManager.OrderRepositry[strOrderId];
            Assert.AreEqual(order.OrderId, retOrderR.orderId);
            Assert.AreEqual(orderR.orderTime, retOrderR.orderTime);
            Assert.AreEqual(appOrder.StratgeyShortName, retOrderR.sno);
        }

        private IAppStrategyManager getStgManager()
        {
            var stgManagerMock = new Mock<IAppStrategyManager>();
            String[] names = { "S1_RND1", "S2_RBREAK_REVERSE1", "S3_RBREAK_TREND1" };
            stgManagerMock.Setup(foo => foo.getStgNames()).Returns(names);
            IAppStrategyManager stgManagerObj = stgManagerMock.Object;
            return stgManagerObj;
        }

        private IAppOrder createOrder_for_addAppOrUpdateOrder()
        {
            IAppOrder appOrder = new AppOrder();
            appOrder.StrategyIndex = 3;
            appOrder.OrderId = 1001;
            appOrder.OrderType = AppConstant.ORDER_TYPE_LIMIT;
            appOrder.BuySell = AppConstant.SELL_SIGNAL;
            appOrder.TotalQuantity = 10;
            appOrder.LmtPrice = 22000;
            appOrder.AuxPrice = 0;
            appOrder.StratgeyShortName = "Test Strategy 1";
            return appOrder;
        }

        private List<IAppExecution> createExecution_for_updatePosition()
        {
            List<IAppExecution> lstExecution = new List<IAppExecution>();

            IAppExecution execution1 = new AppExecution();
            execution1.OrderId = 1002;
            execution1.ExecId = "1104";
            execution1.LastExecTime = "2015-11-11 01:03:01";
            execution1.Side = AppConstant.SELL_SIGNAL;
            execution1.AvgPrice = 22100;
            execution1.ExeShare = 6;

            IAppExecution execution2 = new AppExecution();
            execution2.OrderId = 1003;
            execution2.ExecId = "1105";
            execution2.LastExecTime = "2015-11-11 01:04:01";
            execution2.Side = AppConstant.BUY_SIGNAL;
            execution2.AvgPrice = 22000;
            execution2.ExeShare = 3;

            IAppExecution execution3 = new AppExecution();
            execution3.OrderId = 1004;
            execution3.ExecId = "1106";
            execution3.LastExecTime = "2015-11-11 01:05:01";
            execution3.Side = AppConstant.BUY_SIGNAL;
            execution3.AvgPrice = 21900;
            execution3.ExeShare = 3;


            IAppExecution execution4 = new AppExecution();
            execution4.OrderId = 1005;
            execution4.ExecId = "1107";
            execution4.LastExecTime = "2015-11-11 01:06:01";
            execution4.Side = AppConstant.BUY_SIGNAL;
            execution4.AvgPrice = 23000;
            execution4.ExeShare = 1;

            IAppExecution execution5 = new AppExecution();
            execution5.OrderId = 1006;
            execution5.ExecId = "1108";
            execution5.LastExecTime = "2015-11-11 01:07:01";
            execution5.Side = AppConstant.SELL_SIGNAL;
            execution5.AvgPrice = 23500;
            execution5.ExeShare = 1;

            lstExecution.Add(execution1);
            lstExecution.Add(execution2);
            lstExecution.Add(execution3);
            lstExecution.Add(execution4);
            lstExecution.Add(execution5);
            return lstExecution;
        }

        private ExecutionMessage getExeMessage_for_Sell3Contract(){
            ExecutionMessage message = new ExecutionMessage(3001, getContract(), getExecution_for_Sell3Contract());
            return message;
        
        }

        private Execution getExecution_for_Sell3Contract()
        {
            Execution exeObj = new ExecutionMock();
            exeObj.OrderId = 1001;
            exeObj.ExecId =  "1102";
            exeObj.Time = "2015-11-11 01:07:01";
            exeObj.AvgPrice = 23000;
            exeObj.Shares = 3;
            return exeObj;
        }

        private IAppOrder createOrder_Sell3Contract()
        {
            IAppOrder appOrder = new AppOrder();
            appOrder.StrategyIndex = 2;
            appOrder.OrderId = 1001;
            appOrder.OrderType = AppConstant.ORDER_TYPE_LIMIT;
            appOrder.BuySell = AppConstant.SELL_SIGNAL;
            appOrder.TotalQuantity = 3;
            appOrder.LmtPrice = 23100;
            appOrder.AuxPrice = 0;
            appOrder.StratgeyShortName = "Test Strategy 1";

            Order IBOrder = new Order();
            IBOrder.OrderId = 1001;
            appOrder.IBOrder = IBOrder;
            return appOrder;
        }

        private Contract getContract()
        {
            Contract contract = new Contract();
            contract.SecType = "FUT";
            contract.Symbol = "HSI";
            contract.Exchange = "HKFE";
            contract.Currency = "HKD";
            contract.PrimaryExch = "";
            contract.LocalSymbol = "HSIZ15";
            return contract;
        }


    }
}
