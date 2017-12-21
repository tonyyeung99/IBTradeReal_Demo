using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using IBTradeRealTime;
using IBTradeRealTime.Strategy;
using IBTradeRealTime.app;
using IBTradeRealTime.UI;

using IBApi;
using IBTradeRealTime.util;
using IBTradeRealTime.app;
using IBTradeRealTime.AppOrders;

namespace IBTradeRealTime.Test.Strategy
{
    [TestFixture]
    class SignalContextTest
    {
        
        [Test]
        public void test_IBTradeApp()
        {
            //var mock = new Mock<IBTradeApp>();
            //IBTradeApp obj = mock.Object;
            //Assert.AreEqual(true, false);
        }        

        [Test]
        public void test_IsSignalBidirection_Positive()
        {
            ISignalContext context = new SignalContext();
            context.setPendingSignal1(AppConstant.BUY_SIGNAL,  20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(true,context.isSignalBidirection());
        }

        [Test]
        public void test_IsSignalBidirection_Negative()
        {
            ISignalContext context = new SignalContext();
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_1Complete(), "Buy Order");
            Assert.AreEqual(false, context.isSignalBidirection());
        }

        [Test]
        public void test_GetCompleteSignalSide_Positive()
        {
            ISignalContext context = new SignalContext();
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_1Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(AppConstant.BUY_SIGNAL, context.getCompleteSignalSide());

            context = new SignalContext();
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_1Complete(), "Sell Order");
            Assert.AreEqual(AppConstant.SELL_SIGNAL, context.getCompleteSignalSide());

            context = new SignalContext();
            context.setPendingSignal1(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_10Complete(), "Sell Order");
            Assert.AreEqual(AppConstant.BUY_SIGNAL, context.getCompleteSignalSide());
        }

        [Test]
        public void test_GetFilled_Price()
        {
            ISignalContext context = new SignalContext();
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_1Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(20010, context.getFilledPrice());

            context = new SignalContext();
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_1Complete(), "Sell Order");
            Assert.AreEqual(22010, context.getFilledPrice());

            context = new SignalContext();
            double filledPrice = (20010.0d * 1 + 20009 * 4 + 20008 * 5) / 10;
            context.setPendingSignal1(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            context.setPendingSignal2(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_10Complete(), "Buy Order");
            Assert.AreEqual(filledPrice, context.getFilledPrice());
        }

        [Test]
        public void test_IsPendingOrderSet_Positive()
        {
            ISignalContext context = new SignalContext();
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPendingOrderSet());

            context = new SignalContext();
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            Assert.AreEqual(true, context.isPendingOrderSet());

            context = new SignalContext();
            context.setPendingSignal1(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPendingOrderSet());
        }

        [Test]
        public void test_IsPendingOrderSet_Negative()
        {
            ISignalContext context = new SignalContext();
            Assert.AreEqual(false, context.isPendingOrderSet());
        }

        [Test]
        public void test_CompletePendingSignal()
        {
            ISignalContext context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_1Complete(), "Buy Order");
            context.completePendingSignal();
            Assert.AreEqual(2, context.CurrentMarketPosition);

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_1Complete(), "Sell Order");
            context.completePendingSignal();
            Assert.AreEqual(0, context.CurrentMarketPosition);

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_10Complete(), "Buy Order");
            context.completePendingSignal();
            Assert.AreEqual(11, context.CurrentMarketPosition);

            context = new SignalContext();
            context.CurrentMarketPosition = -1;
            context.setPendingSignal1(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell10Contract_10Complete(), "Sell Order");
            context.completePendingSignal();
            Assert.AreEqual(-11, context.CurrentMarketPosition);

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.completePendingSignal();
            Assert.AreEqual(1, context.CurrentMarketPosition);

        }


        [Test]
        public void test_GetIndexClosedAppOrder()
        {
            ISignalContext context = new SignalContext();
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_1Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(1, context.getIndexClosedAppOrder());

            context = new SignalContext();
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_1Complete(), "Sell Order");
            Assert.AreEqual(2, context.getIndexClosedAppOrder());    
        }

        [Test]
        public void test_IsPositionSignalBothEmpty_Positive()
        {
            ISignalContext context = new SignalContext();
            context.CurrentMarketPosition = 0;
            Assert.AreEqual(true, context.isPositionSignalBothEmpty());
        }

        [Test]
        public void test_IsPositionSignalBothEmpty_Negative()
        {
            ISignalContext context = new SignalContext();
            context.CurrentMarketPosition = 1;
            Assert.AreEqual(false, context.isPositionSignalBothEmpty());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            Assert.AreEqual(false, context.isPositionSignalBothEmpty());

        }

        [Test]
        public void test_IsPositionEmptySignalOnOpen_Positive()
        {
            ISignalContext context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            Assert.AreEqual(true, context.isPositionEmptySignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPositionEmptySignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPositionEmptySignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_5Complete(), "Buy Order");
            Assert.AreEqual(true, context.isPositionEmptySignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell10Contract_5Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPositionEmptySignalOnOpen());

        }

        [Test]
        public void test_IsPositionEmptySignalOnOpen_Negative()
        {
            ISignalContext context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            Assert.AreEqual(false, context.isPositionEmptySignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_1Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionEmptySignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_1Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionEmptySignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_10Complete(), "Buy Order");
            Assert.AreEqual(false, context.isPositionEmptySignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell10Contract_10Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionEmptySignalOnOpen());
        }

        [Test]
        public void test_IsPositionEmptySignalOnClose_Positive()
        {
            ISignalContext context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_1Complete(), "Buy Order");
            Assert.AreEqual(true, context.isPositionEmptySignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_1Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPositionEmptySignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_1Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPositionEmptySignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_10Complete(), "Buy Order");
            Assert.AreEqual(true, context.isPositionEmptySignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_10Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPositionEmptySignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell10Contract_10Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPositionEmptySignalOnClose());
        }

        [Test]
        public void test_IsPositionEmptySignalOnClose_Negative()
        {
            ISignalContext context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            Assert.AreEqual(false, context.isPositionEmptySignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionEmptySignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_1Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionEmptySignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_5Complete(), "Buy Order");
            Assert.AreEqual(false, context.isPositionEmptySignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionEmptySignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_5Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionEmptySignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell10Contract_5Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionEmptySignalOnClose());
        }

        [Test]
        public void test_IsPositionOnSignalEmpty_Positive()
        {
            ISignalContext context = new SignalContext();
            context.CurrentMarketPosition = 1;
            Assert.AreEqual(true, context.isPositionOnSignalEmpty());
        }

        [Test]
        public void test_IsPositionOnSignalEmpty_Negative()
        {
            ISignalContext context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            Assert.AreEqual(false, context.isPositionOnSignalEmpty());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            Assert.AreEqual(false, context.isPositionOnSignalEmpty());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionOnSignalEmpty());
        }

        [Test]
        public void test_IsPositionOnSignalOnOpen_Positive()
        {
            ISignalContext context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            Assert.AreEqual(true, context.isPositionOnSignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPositionOnSignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_5Complete(), "Buy Order");
            Assert.AreEqual(true, context.isPositionOnSignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell10Contract_5Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPositionOnSignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_5Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPositionOnSignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell10Contract_5Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPositionOnSignalOnOpen());
        }

        [Test]
        public void test_IsPositionOnSignalOnOpen_Negative()
        {

            ISignalContext context = new SignalContext();
            context.CurrentMarketPosition = 1;
            Assert.AreEqual(false, context.isPositionOnSignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_1Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionOnSignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_1Complete(), "Buy Order");
            Assert.AreEqual(false, context.isPositionOnSignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_1Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionOnSignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_10Complete(), "Buy Order");
            Assert.AreEqual(false, context.isPositionOnSignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell10Contract_10Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionOnSignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_10Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionOnSignalOnOpen());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell10Contract_10Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionOnSignalOnOpen());
        }

        [Test]
        public void test_IsPositionOnSignalOnClose_Positive()
        {
            ISignalContext context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_1Complete(), "Buy Order");
            Assert.AreEqual(true, context.isPositionOnSignalOnClose());

            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_1Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPositionOnSignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_1Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPositionOnSignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_10Complete(), "Buy Order");
            Assert.AreEqual(true, context.isPositionOnSignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell10Contract_10Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPositionOnSignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_10Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPositionOnSignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell10Contract_10Complete(), "Sell Order");
            Assert.AreEqual(true, context.isPositionOnSignalOnClose());
        }

        [Test]
        public void test_IsPositionOnSignalOnClose_Negative()
        {
            ISignalContext context = new SignalContext();            
            context.CurrentMarketPosition = 0;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_1Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionOnSignalOnClose());
            
            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            Assert.AreEqual(false, context.isPositionOnSignalOnClose());

            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionOnSignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionOnSignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            Assert.AreEqual(false, context.isPositionOnSignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_5Complete(), "Buy Order");
            Assert.AreEqual(false, context.isPositionOnSignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell10Contract_5Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionOnSignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_10Contract_5Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell1Contract_0Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionOnSignalOnClose());

            context = new SignalContext();
            context.CurrentMarketPosition = 1;
            context.setPendingSignal1(AppConstant.BUY_SIGNAL, 20000, createOrder_Buy1Contract_0Complete(), "Buy Order");
            context.setPendingSignal2(AppConstant.SELL_SIGNAL, 22000, createOrder_Sell10Contract_5Complete(), "Sell Order");
            Assert.AreEqual(false, context.isPositionOnSignalOnClose());
        }

        //ToDo: getClosedAppOrder()
        //ToDo: flushCompletSignal()
        //ToDo: getPendingOrders()
        //ToDo: getSignalPriceForExecution();
        //ToDo: getQuantityForExecution();

        private IAppOrder createOrder_Buy1Contract_1Complete()
        {
            IAppOrder appOrder = new AppOrder();
            appOrder.StrategyIndex = 1;
            appOrder.OrderId = 1001;
            appOrder.OrderType = AppConstant.ORDER_TYPE_STOP;
            appOrder.BuySell = AppConstant.BUY_SIGNAL;
            appOrder.TotalQuantity = 1;
            appOrder.LmtPrice = 0;
            appOrder.AuxPrice = 20000;
            appOrder.StratgeyShortName = "Test Strategy 1";
            appOrder.addExecution(createExecution_Buy1Contract());
            return appOrder;
        }

        private IAppOrder createOrder_Buy1Contract_0Complete()
        {
            IAppOrder appOrder = new AppOrder();
            appOrder.StrategyIndex = 1;
            appOrder.OrderId = 1001;
            appOrder.OrderType = AppConstant.ORDER_TYPE_STOP;
            appOrder.BuySell = AppConstant.BUY_SIGNAL;
            appOrder.TotalQuantity = 1;
            appOrder.LmtPrice = 0;
            appOrder.AuxPrice = 20000;
            appOrder.StratgeyShortName = "Test Strategy 1";
            return appOrder;
        }

        private IAppOrder createOrder_Sell1Contract_1Complete()
        {
            IAppOrder appOrder = new AppOrder();
            appOrder.StrategyIndex = 1;
            appOrder.OrderId = 1001;
            appOrder.OrderType = AppConstant.ORDER_TYPE_STOP;
            appOrder.BuySell = AppConstant.SELL_SIGNAL;
            appOrder.TotalQuantity = 1;
            appOrder.LmtPrice = 0;
            appOrder.AuxPrice = 22000;
            appOrder.StratgeyShortName = "Test Strategy 1";
            appOrder.addExecution(createExecution_Sell1Contract());
            return appOrder;
        }

        private IAppOrder createOrder_Sell1Contract_0Complete()
        {
            IAppOrder appOrder = new AppOrder();
            appOrder.StrategyIndex = 1;
            appOrder.OrderId = 1001;
            appOrder.OrderType = AppConstant.ORDER_TYPE_STOP;
            appOrder.BuySell = AppConstant.SELL_SIGNAL;
            appOrder.TotalQuantity = 1;
            appOrder.LmtPrice = 0;
            appOrder.AuxPrice = 22000;
            appOrder.StratgeyShortName = "Test Strategy 1";
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

        private IAppOrder createOrder_Sell10Contract_10Complete()
        {
            IAppOrder appOrder = new AppOrder();
            appOrder.StrategyIndex = 3;
            appOrder.OrderId = 1001;
            appOrder.OrderType = AppConstant.ORDER_TYPE_LIMIT;
            appOrder.BuySell = AppConstant.SELL_SIGNAL;
            appOrder.TotalQuantity = 10;
            appOrder.LmtPrice = 22000;
            appOrder.AuxPrice = 0;
            appOrder.addExecution(createExecution_Sell1Contract());
            appOrder.addExecution(createExecution_Sell4Contract());
            appOrder.addExecution(createExecution_Sell5Contract());
            appOrder.StratgeyShortName = "Test Strategy 1";
            return appOrder;
        }

        private IAppOrder createOrder_Sell10Contract_5Complete()
        {
            IAppOrder appOrder = new AppOrder();
            appOrder.StrategyIndex = 3;
            appOrder.OrderId = 1001;
            appOrder.OrderType = AppConstant.ORDER_TYPE_LIMIT;
            appOrder.BuySell = AppConstant.SELL_SIGNAL;
            appOrder.TotalQuantity = 10;
            appOrder.LmtPrice = 22000;
            appOrder.AuxPrice = 0;
            appOrder.addExecution(createExecution_Sell1Contract());
            appOrder.addExecution(createExecution_Sell4Contract());
            appOrder.StratgeyShortName = "Test Strategy 1";
            return appOrder;
        }



        private IAppExecution createExecution_Buy1Contract()
        {
            IAppExecution execution = new AppExecution();
            execution.OrderId = 1001;
            execution.ExecId = "1101";
            execution.LastExecTime = "2015-11-11 01:01:01";
            execution.Side = AppConstant.BUY_SIGNAL;
            execution.AvgPrice = 20010;
            execution.ExeShare = 1;
            return execution;
        }

        private IAppExecution createExecution_Sell1Contract()
        {
            IAppExecution execution = new AppExecution();
            execution.OrderId = 1002;
            execution.ExecId = "1102";
            execution.LastExecTime = "2015-11-11 01:01:01";
            execution.Side = AppConstant.SELL_SIGNAL;
            execution.AvgPrice = 22010;
            execution.ExeShare = 1;
            return execution;
        }

        private IAppExecution createExecution_Sell4Contract()
        {
            IAppExecution execution = new AppExecution();
            execution.OrderId = 1002;
            execution.ExecId = "1103";
            execution.LastExecTime = "2015-11-11 01:02:01";
            execution.Side = AppConstant.SELL_SIGNAL;
            execution.AvgPrice = 22009;
            execution.ExeShare = 4;
            return execution;
        }

        private IAppExecution createExecution_Sell5Contract()
        {
            IAppExecution execution = new AppExecution();
            execution.OrderId = 1002;
            execution.ExecId = "1104";
            execution.LastExecTime = "2015-11-11 01:03:01";
            execution.Side = AppConstant.SELL_SIGNAL;
            execution.AvgPrice = 22008;
            execution.ExeShare = 5;
            return execution;
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
