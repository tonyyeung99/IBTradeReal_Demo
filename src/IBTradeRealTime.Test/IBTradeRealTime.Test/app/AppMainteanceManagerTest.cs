using IBApi;
using IBTradeRealTime.app;
using IBTradeRealTime.Test.UI;
using IBTradeRealTime.UI;
using IBTradeRealTime.util;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.Test.app
{
    [TestFixture]
    class AppMainteanceManagerTest
    {
        [Test]
        public void test_getResetTime(){

            DateTime beforeResetTime = new DateTime(2015, 12, 10, 21, 0, 0, DateTimeKind.Local);
            DateTime atResetTime = new DateTime(2015, 12, 11, 0, 10, 0, DateTimeKind.Local);
            DateTime afterResetTime = new DateTime(2015, 12, 11, 11, 0, 0, DateTimeKind.Local);
            IAppMainteanceManager maintenanceManager = new AppMainteanceManager(null);

            DateTime beforeResultTime = new DateTime(2015, 12, 10, 0, 10, 0, DateTimeKind.Local);
            DateTime atResultTime = new DateTime(2015, 12, 11, 0, 10, 0, DateTimeKind.Local);
            DateTime afterResultTime = new DateTime(2015, 12, 11, 0, 10, 0, DateTimeKind.Local);

            //[input time is before reset[T] time, it would return 0 hour of T-1 time.]
            Assert.AreEqual(beforeResultTime, AppMainteanceManager.getResetTime(beforeResetTime));
            //[input time is at reset[T] time, it would return 0 hour of T time.]
            Assert.AreEqual(afterResultTime, AppMainteanceManager.getResetTime(afterResultTime));
            //[input time is after reset[T] time, it would return 0 hour of T time.]
            Assert.AreEqual(atResultTime, AppMainteanceManager.getResetTime(atResetTime));
        }

        [Test]
        public void test_resetDailyAllFlag()
        {
            DateTime ResetTime1st = new DateTime(2015, 12, 10, 21, 0, 0, DateTimeKind.Local);
            DateTime ResetTime2nd = new DateTime(2015, 12, 11, 0, 10, 0, DateTimeKind.Local);
            DateTime ResetTime3rd= new DateTime(2015, 12, 12, 01, 10, 0, DateTimeKind.Local);

            DateTime ResultTime1st = new DateTime(2015, 12, 10, 0, 10, 0, DateTimeKind.Local);
            DateTime ResultTime2nd = new DateTime(2015, 12, 11, 0, 10, 0, DateTimeKind.Local);
            DateTime ResultTime3rd = new DateTime(2015, 12, 12, 0, 10, 0, DateTimeKind.Local);

            IAppMainteanceManager maintenanceManager = new AppMainteanceManager(null);
            maintenanceManager.resetDailyAllFlag(ResetTime1st);
            Assert.AreEqual(1, maintenanceManager.CompleteDailyReset.Count);
            Assert.IsNotNull(maintenanceManager.CompleteDailyReset[ResultTime1st]);

            maintenanceManager.resetDailyAllFlag(ResetTime2nd);
            Assert.AreEqual(2, maintenanceManager.CompleteDailyReset.Count);
            Assert.IsNotNull(maintenanceManager.CompleteDailyReset[ResultTime2nd]);

            maintenanceManager.resetDailyAllFlag(ResetTime3rd);
            Assert.AreEqual(3, maintenanceManager.CompleteDailyReset.Count);
            Assert.IsNotNull(maintenanceManager.CompleteDailyReset[ResultTime3rd]);
        }

        [Test]
        public void test_handleDailyAllFlagsReset()
        {
            DateTime time1 = new DateTime(2015, 12, 10, 10, 0, 0, DateTimeKind.Local);
            DateTime time2 = new DateTime(2015, 12, 10, 12, 57, 1, DateTimeKind.Local);
            DateTime time3 = new DateTime(2015, 12, 14, 02, 0, 1, DateTimeKind.Local);

            DateTime time4 = new DateTime(2015, 12, 10, 0, 10, 0, DateTimeKind.Local);
            DateTime time5 = new DateTime(2015, 12, 11, 0, 10, 0, DateTimeKind.Local);

            AppTimeEvent timeEvent1 = getItemEvent(time1);
            AppTimeEvent timeEvent2 = getItemEvent(time2);
            AppTimeEvent timeEvent3 = getItemEvent(time3);
            //[Case Initial the reset flags]
            IAppMainteanceManager maintenanceManager = getInitMaintenaceManger(null);
            maintenanceManager.dailyAllFlagsInit = false;
            maintenanceManager.handleDailyAllFlagsReset(timeEvent1);
            
            Assert.IsTrue(maintenanceManager.dailyAllFlagsInit);
            Assert.IsNotNull(maintenanceManager.CompleteDailyReset[time4]);

            
            //[Case will not trigger reset all daily flags, when the CompleteDailyReset already contain the record of that day]
            maintenanceManager.lunchTimeRTBReset = true;
            maintenanceManager.morningTimeRTBReset = true;
            maintenanceManager.handleDailyAllFlagsReset(timeEvent2);
            Assert.IsTrue(maintenanceManager.lunchTimeRTBReset);
            Assert.IsTrue(maintenanceManager.morningTimeRTBReset);

            Console.WriteLine(timeEvent3.eventTime.ToString());
            Console.WriteLine(maintenanceManager.dailyAllFlagsInit);
            //[Case will trigger reset all daily flags, when the CompleteDailyReset does not contain the record of that day]
            //TO:DO to implement
            //maintenanceManager.handleDailyAllFlagsReset(timeEvent3);
            //Assert.IsFalse(maintenanceManager.lunchTimeRTBReset);
            //Assert.IsFalse(maintenanceManager.morningTimeRTBReset);
            //Assert.IsNotNull(maintenanceManager.CompleteDailyReset[time5]);
           

            //xxxmaintenanceManager.CompleteDailyReset[time1];

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

        private TickerInfo getTickerInfo()
        {
            TickerInfo info = new TickerInfo();
            info.tickerID = "";
            info.contractID = "";
            info.symbol = "HSI";
            info.type = "FUT";
            info.exchange = "HKFE";
            info.pExchange = "";
            info.currency = "HKD";
            info.lSymbol = "HSIZ15";
            info.whatToShow = "TRADES";
            info.startTime = "09:15:00";
            info.lunchEndTime = "13:00:00";
            return info;
        }

        [Test]
        public void test_handleResetRTBRequest()
        {
            IRealTimeBarsManagerBridge rtsBarManager = new RealTimeBarsManagerMock();
            var uiMock = new Mock<IIBTradeAppBridge>();
            Contract contract = getContract();
            TickerInfo ticker = getTickerInfo();
            uiMock.Setup(foo => foo.getRealTimeBarsManager()).Returns(rtsBarManager);
            uiMock.Setup(foo => foo.GetMDContract()).Returns(contract);
            uiMock.Setup(foo => foo.getTickerInfo()).Returns(ticker);
            IIBTradeAppBridge uiMockObj = uiMock.Object;

            var mock = new Mock<IAppStrategyManager>();
            mock.Setup(foo => foo.ParentUI).Returns(uiMockObj);
            IAppStrategyManager mockObj = mock.Object;

            DateTime time1 = new DateTime(2015, 12, 10, 10, 0, 0, DateTimeKind.Local);
            DateTime time2 = new DateTime(2015, 12, 10, 12, 57, 1, DateTimeKind.Local);
            DateTime time3 = new DateTime(2015, 12, 10, 12, 58, 1, DateTimeKind.Local);

            IAppMainteanceManager maintenanceManager = getInitMaintenaceManger(mockObj);
            AppTimeEvent timeEvent1 = getItemEvent(time1);
            AppTimeEvent timeEvent2 = getItemEvent(time2);
            AppTimeEvent timeEvent3 = getItemEvent(time3);

            //[reset negative, time is before moring time and lunch time]
            maintenanceManager.handleResetRTBRequest(timeEvent1);
            Assert.IsFalse(maintenanceManager.morningTimeRTBReset);
            Assert.IsFalse(maintenanceManager.lunchTimeRTBReset);

            //[reset lunch positive, time is at lunch time]
            maintenanceManager.handleResetRTBRequest(timeEvent2);
            Assert.IsFalse(maintenanceManager.morningTimeRTBReset);
            Assert.IsTrue(maintenanceManager.lunchTimeRTBReset);

            //[reset lunch negative, time is just after lunch time]
            maintenanceManager = getInitMaintenaceManger(mockObj);
            maintenanceManager.handleResetRTBRequest(timeEvent3);
            Assert.IsFalse(maintenanceManager.morningTimeRTBReset);
            Assert.IsFalse(maintenanceManager.lunchTimeRTBReset);


            DateTime time4 = new DateTime(2015, 12, 10, 07, 0, 0, DateTimeKind.Local);
            DateTime time5 = new DateTime(2015, 12, 10, 09, 12, 1, DateTimeKind.Local);
            DateTime time6 = new DateTime(2015, 12, 10, 09, 13, 1, DateTimeKind.Local);

            AppTimeEvent timeEvent4 = getItemEvent(time4);
            AppTimeEvent timeEvent5 = getItemEvent(time5);
            AppTimeEvent timeEvent6 = getItemEvent(time6);

            //[reset morning negative, time is before morning time]
            maintenanceManager = getInitMaintenaceManger(mockObj);
            maintenanceManager.handleResetRTBRequest(timeEvent4);
            Assert.IsFalse(maintenanceManager.morningTimeRTBReset);
            Assert.IsFalse(maintenanceManager.lunchTimeRTBReset);

            //[reset morning positive, time is at morning time]
            maintenanceManager = getInitMaintenaceManger(mockObj);
            maintenanceManager.handleResetRTBRequest(timeEvent5);
            Assert.IsTrue(maintenanceManager.morningTimeRTBReset);
            Assert.IsFalse(maintenanceManager.lunchTimeRTBReset);

            //[reset morning negative, time is after morning time]
            maintenanceManager = getInitMaintenaceManger(mockObj);
            maintenanceManager.handleResetRTBRequest(timeEvent6);
            Assert.IsFalse(maintenanceManager.morningTimeRTBReset);
            Assert.IsFalse(maintenanceManager.lunchTimeRTBReset);

            //[reset morning & lunch, one cycle]
            DateTime time7 = new DateTime(2015, 12, 11, 0, 0, 1, DateTimeKind.Local);
            DateTime time8 = new DateTime(2015, 12, 11, 09, 12, 1, DateTimeKind.Local);
            DateTime time9 = new DateTime(2015, 12, 11, 12, 57, 1, DateTimeKind.Local);

            AppTimeEvent timeEvent7 = getItemEvent(time7);
            AppTimeEvent timeEvent8 = getItemEvent(time8);
            AppTimeEvent timeEvent9 = getItemEvent(time9);
            maintenanceManager = getInitMaintenaceManger(mockObj);

            //[time is 07:00]
            maintenanceManager.handleResetRTBRequest(timeEvent4);
            Assert.IsFalse(maintenanceManager.morningTimeRTBReset);
            Assert.IsFalse(maintenanceManager.lunchTimeRTBReset);

            //[time is 09:12]
            maintenanceManager.handleResetRTBRequest(timeEvent5);
            Assert.IsTrue(maintenanceManager.morningTimeRTBReset);
            Assert.IsFalse(maintenanceManager.lunchTimeRTBReset);

            //[time is 12:57]
            maintenanceManager.handleResetRTBRequest(timeEvent2);
            Assert.IsTrue(maintenanceManager.morningTimeRTBReset);
            Assert.IsTrue(maintenanceManager.lunchTimeRTBReset);

            //[time is 0:0 T+1]
            maintenanceManager.resetDailyAllFlag(time7);
            Assert.IsFalse(maintenanceManager.morningTimeRTBReset);
            Assert.IsFalse(maintenanceManager.lunchTimeRTBReset);

            //[time is  09:12 T+1]
            maintenanceManager.handleResetRTBRequest(timeEvent8);
            Assert.IsTrue(maintenanceManager.morningTimeRTBReset);
            Assert.IsFalse(maintenanceManager.lunchTimeRTBReset);

            //[time is  12:57 T+1]
            maintenanceManager.handleResetRTBRequest(timeEvent9);
            Assert.IsTrue(maintenanceManager.morningTimeRTBReset);
            Assert.IsTrue(maintenanceManager.lunchTimeRTBReset);
        }

        private IAppMainteanceManager getInitMaintenaceManger(IAppStrategyManager stgManager)
        {
            IAppMainteanceManager maintenanceManager = new AppMainteanceManager(stgManager);
            maintenanceManager.lunchTimeRTBReset = false;
            maintenanceManager.morningTimeRTBReset = false;
            maintenanceManager.dailyAllFlagsInit = true;
            return maintenanceManager;
        }

        private AppTimeEvent getItemEvent(DateTime time)
        {
            AppTimeEvent timeEvent = new AppTimeEvent();
            timeEvent.eventTime = time;
            return timeEvent;
        }
    }
}
