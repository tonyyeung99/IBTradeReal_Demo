using IBTradeRealTime.app;
using IBTradeRealTime.message;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.Test.app
{
    [TestFixture]
    class AppEventManagerTest
    {
        
        [Test]
        public void test_putTimeEvents(){
            IAppStrategyManager stgManager = getAppStrategyManager_for_putEvents();
            AppEventManager eventManager = new AppEventManager(stgManager);
            DateTime afterSetTime = new DateTime(2015, 11, 21, 9, 15, 55, DateTimeKind.Local);
            DateTime afterSetTime2 = new DateTime(2015, 11, 21, 9, 16, 0, DateTimeKind.Local);
            eventManager.putTimeEvents(afterSetTime);
            eventManager.putTimeEvents(afterSetTime2);
            Assert.AreEqual(2,eventManager.storeEventQueue["S1_RND1"].Count);
            Assert.AreEqual(0, eventManager.storeEventQueue["S2_RBREAK_REVERSE1"].Count);
            Assert.AreEqual(2, eventManager.storeEventQueue["S3_RBREAK_TREND1"].Count);
            Assert.AreEqual(2, stgManager.getAppMainteanceManager().storeEventQueue.Count);
        }

        [Test]
        public void test_putTickPriceEvents()
        {
            IAppStrategyManager stgManager = getAppStrategyManager_for_putEvents();
            AppEventManager eventManager = new AppEventManager(stgManager);

            TickPriceMessage tickMessage1 = new TickPriceMessage(200001, 4, 22000, 0);
            TickPriceMessage tickMessage2 = new TickPriceMessage(200002, 1, 22100, 0);

            MarketDataMessage dataMessage = (MarketDataMessage)tickMessage1;
            eventManager.putTickPriceEvents(tickMessage1, dataMessage);
            dataMessage = (MarketDataMessage)tickMessage2;
            eventManager.putTickPriceEvents(tickMessage2, dataMessage);
            Assert.AreEqual(2, eventManager.storeEventQueue["S1_RND1"].Count);
            Assert.AreEqual(0, eventManager.storeEventQueue["S2_RBREAK_REVERSE1"].Count);
            Assert.AreEqual(2, eventManager.storeEventQueue["S3_RBREAK_TREND1"].Count);
            Assert.AreEqual(0, stgManager.getAppMainteanceManager().storeEventQueue.Count);
        }

        private IAppStrategyManager getAppStrategyManager_for_putEvents()
        {
            var mock = new Mock<IAppStrategyManager>();
            ConcurrentDictionary<String, String> activeStg = new ConcurrentDictionary<String, String>();
            activeStg.AddOrUpdate("S1_RND1", "S1_RND1", (key, oldvalue) => "S1_RND1");
            activeStg.AddOrUpdate("S3_RBREAK_TREND1", "S3_RBREAK_TREND1", (key, oldvalue) => "S3_RBREAK_TREND1");
            String[] stgNames = { "S1_RND1", "S2_RBREAK_REVERSE1", "S3_RBREAK_TREND1" };
            mock.Setup(foo => foo.getActiveStgNamesMap()).Returns(activeStg);
            mock.Setup(foo => foo.getStgNames()).Returns(stgNames);
            mock.Setup(foo => foo.getAppMainteanceManager()).Returns(new AppMainteanceManager(null));
            IAppStrategyManager fooObj = mock.Object;
            return fooObj;
        }

        //ToDo : onTimedEvent()
    }
}
