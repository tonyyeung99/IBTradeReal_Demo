using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBTradeRealTime.app;
using NUnit.Framework;
using IBTradeRealTime.AppOrders;
using IBTradeRealTime.util;

namespace IBTradeRealTime.util
{
    [TestFixture]
    class AppObjectUtilTest
    {
        [Test]
        public void testIsOrderClosed_with1contract()
        {
            IAppOrder order1Contract = createOrder_1Contract_1Complete();
            Assert.AreEqual(true,AppObjectUtil.isOrderClosed(order1Contract));
        }

        [Test]
        public void testIsOrderClosed_with10contract()
        {
            IAppOrder order10Contract = createOrder_10Contract_10Complete();
            Assert.AreEqual(true,AppObjectUtil.isOrderClosed(order10Contract));
        }

        [Test]
        public void testIsOrderClosed_with10contract_5Complete()
        {
            IAppOrder order10Contract = createOrder_10Contract_5Complete();
            Assert.AreEqual(false, AppObjectUtil.isOrderClosed(order10Contract));
        }
        //ToDo: createMktOrder()
        //ToDo: createStopOrder()
        //ToDo: createLimitOrder()
        //ToDo: createAppOrder()

        private IAppOrder createOrder_1Contract_1Complete()
        {
            IAppOrder appOrder = new AppOrder();
            appOrder.StrategyIndex = 1;
            appOrder.OrderId = 1001;
            appOrder.OrderType = AppConstant.ORDER_TYPE_LIMIT;
            appOrder.BuySell = AppConstant.BUY_SIGNAL;
            appOrder.TotalQuantity = 1;
            appOrder.LmtPrice = 20000;
            appOrder.AuxPrice = 0;
            appOrder.StratgeyShortName = "Test Strategy 1";
            appOrder.addExecution(createExecution1Contract());
            return appOrder;
        }

        private IAppOrder createOrder_10Contract_10Complete()
        {
            IAppOrder appOrder = new AppOrder();
            appOrder.StrategyIndex = 3;
            appOrder.OrderId = 1001;
            appOrder.OrderType = AppConstant.ORDER_TYPE_LIMIT;
            appOrder.BuySell = AppConstant.BUY_SIGNAL;
            appOrder.TotalQuantity = 10;
            appOrder.LmtPrice = 20000;
            appOrder.AuxPrice = 0;
            appOrder.addExecution(createExecution1Contract());
            appOrder.addExecution(createExecution4Contract());
            appOrder.addExecution(createExecution5Contract());
            appOrder.StratgeyShortName = "Test Strategy 1";
            return appOrder;
        }

        private IAppOrder createOrder_10Contract_5Complete()
        {
            IAppOrder appOrder = new AppOrder();
            appOrder.StrategyIndex = 3;
            appOrder.OrderId = 1001;
            appOrder.OrderType = AppConstant.ORDER_TYPE_LIMIT;
            appOrder.BuySell = AppConstant.BUY_SIGNAL; 
            appOrder.TotalQuantity = 10;
            appOrder.LmtPrice = 20000;
            appOrder.AuxPrice = 0;
            appOrder.addExecution(createExecution1Contract());
            appOrder.addExecution(createExecution4Contract());
            appOrder.StratgeyShortName = "Test Strategy 1";
            return appOrder;
        }

        private IAppExecution createExecution1Contract()
        {
            IAppExecution execution = new AppExecution();
            execution.OrderId = 1001;
            execution.ExecId = "1002";
            execution.LastExecTime = "2015-11-11 01:01:01";
            execution.Side = AppConstant.BUY_SIGNAL;
            execution.AvgPrice = 20010;
            execution.ExeShare = 1;            
            return execution;
        }

        private IAppExecution createExecution4Contract()
        {
            IAppExecution execution = new AppExecution();
            execution.OrderId = 1001;
            execution.ExecId = "1003";
            execution.LastExecTime = "2015-11-11 01:02:01";
            execution.Side = AppConstant.BUY_SIGNAL;
            execution.AvgPrice = 20009;
            execution.ExeShare = 4;
            return execution;
        }

        private IAppExecution createExecution5Contract()
        {
            IAppExecution execution = new AppExecution();
            execution.OrderId = 1001;
            execution.ExecId = "1004";
            execution.LastExecTime = "2015-11-11 01:03:01";
            execution.Side = AppConstant.BUY_SIGNAL;
            execution.AvgPrice = 20008;
            execution.ExeShare = 5;
            return execution;
        }
    }


}
