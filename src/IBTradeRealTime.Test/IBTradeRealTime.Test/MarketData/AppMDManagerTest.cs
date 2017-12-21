using Deedle;
using IBTradeRealTime.MarketData;
using IBTradeRealTime.message;
using IBTradeRealTime.util;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.Test.MarketData
{
    [TestFixture]
    class AppMDManagerTest
    {
        [Test]
        public void test_isRTBarProcessStart()
        {
            //[Test: return ture, RTBDataStartTime is invalid Time and the message time is in 0 second.]
            DateTime validTime = new DateTime(2015, 11, 21, 9, 50, 0, DateTimeKind.Local);
            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.setRtbDataStartTime(AppConstant.INVALID_TIME);
            IBMessage validStartMessage = createMessage_for_isRTBarProcessStart(validTime);
            Assert.IsTrue(appMDManager.isRTBarProcessStart(validStartMessage));
            Assert.AreEqual(validTime, appMDManager.getRtbDataStartTime());

            //[Test: return false, RTBDataStartTime is invalid Time and the message time is not in 0 second.]
            DateTime invalidTime = new DateTime(2015, 11, 21, 9, 50, 5, DateTimeKind.Local);
            appMDManager = new AppMDManager(null);
            appMDManager.setRtbDataStartTime(AppConstant.INVALID_TIME);
            IBMessage invalidStartMessage = createMessage_for_isRTBarProcessStart(invalidTime);
            Assert.IsFalse(appMDManager.isRTBarProcessStart(invalidStartMessage));
            Assert.AreEqual(AppConstant.INVALID_TIME, appMDManager.getRtbDataStartTime());
        }

        [Test]
        public void test_buildAndUpdateSynMinuteBar_normal()
        {
            DateTime time1 = new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Local);
            DateTime time2 = new DateTime(2015, 10, 21, 10, 0, 5, DateTimeKind.Local);
            DateTime time3 = new DateTime(2015, 10, 21, 10, 0, 10, DateTimeKind.Local);
            DateTime time4 = new DateTime(2015, 10, 21, 10, 0, 55, DateTimeKind.Local);
            DateTime time5 = new DateTime(2015, 10, 21, 10, 1, 0, DateTimeKind.Local);

            IBMessage message1 = createMessage_for_isRTBarProcessStart_Open(time1);
            IBMessage message2 = createMessage_for_isRTBarProcessStart_High(time2);
            IBMessage message3 = createMessage_for_isRTBarProcessStart_Low(time3);
            IBMessage message4 = createMessage_for_isRTBarProcessStart_Close(time4);
            IBMessage message5 = createMessage_for_isRTBarProcessStart_Open(time5);

            //[Test: initial action test, currentTempRTBar is null, currentCompleteRTBar is null and the time is in 0 second.]
            AppMDManager appMDManager = createManager_buildAndUpdateSynMinuteBar_begin();
            appMDManager.buildAndUpdateSynMinuteBar(message1);
            Assert.AreEqual(20100, appMDManager.currentTempRTBar.open);
            Assert.AreEqual(20200, appMDManager.currentTempRTBar.high);
            Assert.AreEqual(20000, appMDManager.currentTempRTBar.low);
            Assert.AreEqual(20100, appMDManager.currentTempRTBar.close);
            Assert.AreEqual(time1, appMDManager.currentTempRTBar.time);
            Assert.IsNull(appMDManager.currentCompleteRTBar);

            //[Test: make a new high synthesis test, high is a new high, and the time is not in 0 second.]
            appMDManager.buildAndUpdateSynMinuteBar(message2);
            Assert.AreEqual(20100, appMDManager.currentTempRTBar.open);
            Assert.AreEqual(20250, appMDManager.currentTempRTBar.high);
            Assert.AreEqual(20000, appMDManager.currentTempRTBar.low);
            Assert.AreEqual(20105, appMDManager.currentTempRTBar.close);
            Assert.AreEqual(time1, appMDManager.currentTempRTBar.time);
            Assert.IsNull(appMDManager.currentCompleteRTBar);

            //[Test: make a new low synthesis test, low is a new low, and the time is not in 0 second.]
            appMDManager.buildAndUpdateSynMinuteBar(message3);
            Assert.AreEqual(20100, appMDManager.currentTempRTBar.open);
            Assert.AreEqual(20250, appMDManager.currentTempRTBar.high);
            Assert.AreEqual(19990, appMDManager.currentTempRTBar.low);
            Assert.AreEqual(20110, appMDManager.currentTempRTBar.close);
            Assert.AreEqual(time1, appMDManager.currentTempRTBar.time);
            Assert.IsNull(appMDManager.currentCompleteRTBar);

            //[Test: close minute test, the time is in 55 second.]
            appMDManager.buildAndUpdateSynMinuteBar(message4);
            Assert.AreEqual(20100, appMDManager.currentTempRTBar.open);
            Assert.AreEqual(20250, appMDManager.currentTempRTBar.high);
            Assert.AreEqual(19990, appMDManager.currentTempRTBar.low);
            Assert.AreEqual(20155, appMDManager.currentTempRTBar.close);
            Assert.AreEqual(time1, appMDManager.currentTempRTBar.time);
            Assert.AreEqual(20100, appMDManager.currentCompleteRTBar.open);
            Assert.AreEqual(20250, appMDManager.currentCompleteRTBar.high);
            Assert.AreEqual(19990, appMDManager.currentCompleteRTBar.low);
            Assert.AreEqual(20155, appMDManager.currentCompleteRTBar.close);
            Assert.AreEqual(time1, appMDManager.currentCompleteRTBar.time);

            //[Test: open minute test, the time is in 55 second.]
            appMDManager.buildAndUpdateSynMinuteBar(message5);
            Assert.AreEqual(20100, appMDManager.currentTempRTBar.open);
            Assert.AreEqual(20200, appMDManager.currentTempRTBar.high);
            Assert.AreEqual(20000, appMDManager.currentTempRTBar.low);
            Assert.AreEqual(20100, appMDManager.currentTempRTBar.close);
            Assert.AreEqual(time5, appMDManager.currentTempRTBar.time);
            Assert.IsNull(appMDManager.currentCompleteRTBar);
        }

        [Test]
        public void test_buildAndUpdateSynMinuteBar_negative()
        {
            DateTime time1 = new DateTime(2015, 10, 21, 10, 0, 5, DateTimeKind.Local);
            IBMessage message1 = createMessage_for_isRTBarProcessStart_Open(time1);

            //[Test: invalid initial action test, currentTempRTBar is null, currentCompleteRTBar is null and the time is not in 0 second.]
            AppMDManager appMDManager = createManager_buildAndUpdateSynMinuteBar_begin();
            appMDManager.buildAndUpdateSynMinuteBar(message1);
            Assert.IsNull(appMDManager.currentTempRTBar);
            Assert.IsNull(appMDManager.currentCompleteRTBar);
        }

        [Test]
        public void test_isDataReady_positive1()
        {
            //[Test: need syn action and data ready case, synchronizer.getIsDataMerged is true, synchronizer.getNeedMergeFlag is true.]
            IAppRTBSynchronizer synchronizer = getAppRTBSynchronizer_for_isDataReady_positive1();
            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.injectAppRTBSynchronizer(synchronizer);
            Assert.IsTrue(appMDManager.isDataReady());
        }

        [Test]
        public void test_isDataReady_positive2()
        {
            DateTime time1 = new DateTime(2015, 10, 21, 10, 0, 5, DateTimeKind.Local);
            //[Test: non-syn action and data ready case, synchronizer.getIsDataMerged is false, synchronizer.getNeedMergeFlag is false, Timeseries is not null.]
            Series<DateTime, MarketDataElement> TimeSeries = null;
            TimeSeries = addMarketDataElementWithTimeOnlyToSeries(time1, TimeSeries);

            IAppRTBSynchronizer synchronizer = getAppRTBSynchronizer_for_isDataReady_positive2();
            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.injectAppRTBSynchronizer(synchronizer);
            appMDManager.setTimeBarSeries(TimeSeries);
            Assert.IsTrue(appMDManager.isDataReady());
        }

        [Test]
        public void test_isDataReady_negative1()
        {
            //[Test: need syn action and data not ready case, synchronizer.getIsDataMerged is false, synchronizer.getNeedMergeFlag is true.]
            IAppRTBSynchronizer synchronizer = getAppRTBSynchronizer_for_isDataReady_negative1();
            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.injectAppRTBSynchronizer(synchronizer);
            Assert.IsFalse(appMDManager.isDataReady());
        }

        [Test]
        public void test_isDataReady_negative2()
        {
            //[Test: non-syn action and data not ready case, synchronizer.getIsDataMerged is false, synchronizer.getNeedMergeFlag is false, Timeseries is null.]
            IAppRTBSynchronizer synchronizer = getAppRTBSynchronizer_for_isDataReady_negative2();
            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.injectAppRTBSynchronizer(synchronizer);
            Assert.IsFalse(appMDManager.isDataReady());
        }

        [Test]
        public void test_checkIsRTBarMergeNeed_positive1()
        {
            //[Test: initial and postive case, initFlag is false and isRTBarMergeNeed is true => initFlag is true, getIsRTBarMergeNeed is true]
            IAppRTBSynchronizer synchronizer = getSyn_checkIsRTBarMergeNeed();
            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.injectRTBInitFlag(false);
            TickerInfo info = new TickerInfo();
            info.startTime="true";
            appMDManager.injectAppRTBSynchronizer(synchronizer);
            appMDManager.tickerInfo = info;
            appMDManager.checkIsRTBarMergeNeed();
            Assert.IsTrue(appMDManager.getRTBInitFlag());
            Assert.IsTrue(appMDManager.getIsRTBarMergeNeed());
        }

        [Test]
        public void test_checkIsRTBarMergeNeed_negative1()
        {
            //[Test: initial and negative case, initFlag is false and isRTBarMergeNeed is false => initFlag is true, getIsRTBarMergeNeed is false]
            IAppRTBSynchronizer synchronizer = getSyn_checkIsRTBarMergeNeed();
            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.injectRTBInitFlag(false);
            TickerInfo info = new TickerInfo();
            info.startTime = "false";
            appMDManager.injectAppRTBSynchronizer(synchronizer);
            appMDManager.tickerInfo = info;
            appMDManager.checkIsRTBarMergeNeed();
            Assert.IsTrue(appMDManager.getRTBInitFlag());
            Assert.IsFalse(appMDManager.getIsRTBarMergeNeed());

        }

        [Test]
        public void test_checkIsRTBarMergeNeed_negative2()
        {
            //[Test: non-initial and negative case, initFlag is true and isRTBarMergeNeed is false => initFlag is true, getIsRTBarMergeNeed is false]
            IAppRTBSynchronizer synchronizer = getSyn_checkIsRTBarMergeNeed();
            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.injectRTBInitFlag(true);
            TickerInfo info = new TickerInfo();
            info.startTime = "false";
            appMDManager.injectAppRTBSynchronizer(synchronizer);
            appMDManager.tickerInfo = info;
            appMDManager.checkIsRTBarMergeNeed();
            Assert.IsTrue(appMDManager.getRTBInitFlag());
            Assert.IsFalse(appMDManager.getIsRTBarMergeNeed());
        }

        [Test]
        public void test_updateRTBarSeriesActions_init()
        {
            //[Test: Init Case, TimeBarSeries is null and currentCompleteRTB is not null]
            DateTime time1 = new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Local);
            AppMDManager appMDManager = createManager_updateRTBarSeriesActions_init(time1);
            appMDManager.updateRTBarSeriesActions();
            MarketDataElement element = appMDManager.getTimeBarSeries().Get(time1);
            Assert.AreEqual(time1, element.time);
            Assert.AreEqual(1, appMDManager.getTimeBarSeries().KeyCount);            
        }

        [Test]
        public void test_updateRTBarSeriesActions_cont()
        {
            //[Test: Continuou Case, TimeBarSeries is not null and currentCompleteRTB is not null]
            DateTime time1 = new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Local);
            DateTime time2 = new DateTime(2015, 10, 21, 10, 1, 0, DateTimeKind.Local);
            AppMDManager appMDManager = createManager_updateRTBarSeriesActions_cont(time1, time2);
            appMDManager.updateRTBarSeriesActions();
            MarketDataElement element1 = appMDManager.getTimeBarSeries().Get(time1);
            MarketDataElement element2 = appMDManager.getTimeBarSeries().Get(time2);
            Assert.AreEqual(time1, element1.time);
            Assert.AreEqual(time2, element2.time);
            Assert.AreEqual(2, appMDManager.getTimeBarSeries().KeyCount);
        }

        [Test]
        public void test_updateRTBarSeriesActions_cont_negative()
        {
            //[Test: Continuou Case, TimeBarSeries is not null and currentCompleteRTB is not null]
            DateTime time1 = new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Local);
            DateTime time2 = new DateTime(2015, 10, 21, 10, 1, 0, DateTimeKind.Local);
            AppMDManager appMDManager = createManager_updateRTBarSeriesActions_cont_negative(time1, time2);
            appMDManager.updateRTBarSeriesActions();
            MarketDataElement element1 = appMDManager.getTimeBarSeries().Get(time1);
            Assert.AreEqual(time1, element1.time);
            Assert.AreEqual(1, appMDManager.getTimeBarSeries().KeyCount);
        }



        private IBMessage createMessage_for_isRTBarProcessStart(DateTime datetime)
        {
            DateTime dt = datetime.ToUniversalTime();
            DateTime dt0 = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan span = dt.Subtract(dt0);
            IBMessage RTBMessage = new RealTimeBarMessage(1000, Convert.ToInt64(span.TotalSeconds), 20100, 20200, 20000, 20100, 500, 20100, 0);
            return RTBMessage;
        }


        AppMDManager createManager_buildAndUpdateSynMinuteBar_begin(){
            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.currentCompleteRTBar = null;
            appMDManager.currentTempRTBar = null;
            return appMDManager;
        }

        private IBMessage createMessage_for_isRTBarProcessStart_Open(DateTime datetime)
        {
            DateTime dt = datetime.ToUniversalTime();
            DateTime dt0 = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan span = dt.Subtract(dt0);
            IBMessage RTBMessage = new RealTimeBarMessage(1000, Convert.ToInt64(span.TotalSeconds), 20100, 20200, 20000, 20100, 500, 20100, 0);
             // public RealTimeBarMessage(int reqId, long date, double open, double high, double low, double close, long volume, double WAP, int count)
            return RTBMessage;
        }

        private IBMessage createMessage_for_isRTBarProcessStart_High(DateTime datetime)
        {
            DateTime dt = datetime.ToUniversalTime();
            DateTime dt0 = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan span = dt.Subtract(dt0);
            IBMessage RTBMessage = new RealTimeBarMessage(1000, Convert.ToInt64(span.TotalSeconds), 20105, 20250, 20005, 20105, 500, 20100, 0);
            // public RealTimeBarMessage(int reqId, long date, double open, double high, double low, double close, long volume, double WAP, int count)
            return RTBMessage;
        }

        private IBMessage createMessage_for_isRTBarProcessStart_Low(DateTime datetime)
        {
            DateTime dt = datetime.ToUniversalTime();
            DateTime dt0 = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan span = dt.Subtract(dt0);
            IBMessage RTBMessage = new RealTimeBarMessage(1000, Convert.ToInt64(span.TotalSeconds), 20110, 20200, 19990, 20110, 500, 20100, 0);
            // public RealTimeBarMessage(int reqId, long date, double open, double high, double low, double close, long volume, double WAP, int count)
            return RTBMessage;
        }

        private IBMessage createMessage_for_isRTBarProcessStart_Close(DateTime datetime)
        {
            DateTime dt = datetime.ToUniversalTime();
            DateTime dt0 = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan span = dt.Subtract(dt0);
            IBMessage RTBMessage = new RealTimeBarMessage(1000, Convert.ToInt64(span.TotalSeconds), 20110, 20200, 20000, 20155, 500, 20100, 0);
            // public RealTimeBarMessage(int reqId, long date, double open, double high, double low, double close, long volume, double WAP, int count)
            return RTBMessage;
        }

        private IAppRTBSynchronizer getAppRTBSynchronizer_for_isDataReady_positive1()
        {
            var mock = new Mock<IAppRTBSynchronizer>();
            mock.Setup(foo => foo.getIsDataMerged()).Returns(true);
            mock.Setup(foo => foo.getNeedMergeFlag()).Returns(true);
            IAppRTBSynchronizer fooObj = mock.Object;
            return fooObj;
        }

        private IAppRTBSynchronizer getAppRTBSynchronizer_for_isDataReady_positive2() 
        {
            var mock = new Mock<IAppRTBSynchronizer>();
            mock.Setup(foo => foo.getIsDataMerged()).Returns(false);
            mock.Setup(foo => foo.getNeedMergeFlag()).Returns(false);            
            IAppRTBSynchronizer fooObj = mock.Object;
            return fooObj; 
        }

        private IAppRTBSynchronizer getAppRTBSynchronizer_for_isDataReady_negative1()
        {
            var mock = new Mock<IAppRTBSynchronizer>();
            mock.Setup(foo => foo.getIsDataMerged()).Returns(false);
            mock.Setup(foo => foo.getNeedMergeFlag()).Returns(true);
            IAppRTBSynchronizer fooObj = mock.Object;
            return fooObj;
        }

        private IAppRTBSynchronizer getAppRTBSynchronizer_for_isDataReady_negative2()
        {
            var mock = new Mock<IAppRTBSynchronizer>();
            mock.Setup(foo => foo.getIsDataMerged()).Returns(false);
            mock.Setup(foo => foo.getNeedMergeFlag()).Returns(false);
            IAppRTBSynchronizer fooObj = mock.Object;
            return fooObj;
        }

        private IAppRTBSynchronizer getSyn_checkIsRTBarMergeNeed()
        {
            var mock = new Mock<IAppRTBSynchronizer>();
            mock.Setup(foo => foo.isRTBarMergeNeed("true")).Returns(true);
            mock.Setup(foo => foo.isRTBarMergeNeed("false")).Returns(false);
            IAppRTBSynchronizer fooObj = mock.Object;
            return fooObj;
        }

        private static Series<DateTime, MarketDataElement> addMarketDataElementWithTimeOnlyToSeries(DateTime dateTime, Series<DateTime, MarketDataElement> series)
        {
            MarketDataElement currentElement = MarketDataUtil.createHLOC(dateTime, "HLOC", "HSI", "FUT");
            currentElement.volume = 900;
            MarketDataUtil.setHLOC(currentElement, 20200, 20000, 20050, 20150);

            if (series == null)
            {
                series = new SeriesBuilder<DateTime, MarketDataElement>() { { dateTime, currentElement } }.Series;
            }
            else
            {
                if (!series.ContainsKey(dateTime))
                {
                    series = series.Merge(new SeriesBuilder<DateTime, MarketDataElement>() { { dateTime, currentElement } }.Series);

                }
            }
            return series;
        }

        private IBMessage createMessage_General(DateTime datetime)
        {
            DateTime dt = datetime.ToUniversalTime();
            DateTime dt0 = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan span = dt.Subtract(dt0);
            IBMessage RTBMessage = new RealTimeBarMessage(1000, Convert.ToInt64(span.TotalSeconds), 20100, 20200, 20000, 20100, 500, 20100, 0);
            return RTBMessage;
        }

        AppMDManager createManager_updateRTBarSeriesActions_init(DateTime dt)
        {

            RTDataBar   currentCompleteBar = new RTDataBar();
            currentCompleteBar.time = dt;
            currentCompleteBar.open = 20100;
            currentCompleteBar.close = 20100;
            currentCompleteBar.high = 20200;
            currentCompleteBar.low = 20000;
            currentCompleteBar.volume = 500;

            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.currentCompleteRTBar = currentCompleteBar;
            appMDManager.currentTempRTBar = null;
            return appMDManager;
        }

        AppMDManager createManager_updateRTBarSeriesActions_cont(DateTime dt1, DateTime dt2)
        {
            RTDataBar currentCompleteBar = new RTDataBar();
            currentCompleteBar.time = dt2;
            currentCompleteBar.open = 20100;
            currentCompleteBar.close = 20100;
            currentCompleteBar.high = 20200;
            currentCompleteBar.low = 20000;
            currentCompleteBar.volume = 500;

            Series<DateTime, MarketDataElement> series = null;
            series = addMarketDataElementWithTimeOnlyToSeries(dt1, series);
            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.currentCompleteRTBar = currentCompleteBar;
            appMDManager.currentTempRTBar = null;
            appMDManager.setTimeBarSeries(series);
            return appMDManager;
        }

        AppMDManager createManager_updateRTBarSeriesActions_cont_negative(DateTime dt1, DateTime dt2)
        {
            RTDataBar currentCompleteBar = null;
            Series<DateTime, MarketDataElement> series = null;
            series = addMarketDataElementWithTimeOnlyToSeries(dt1, series);
            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.currentCompleteRTBar = currentCompleteBar;
            appMDManager.currentTempRTBar = null;
            appMDManager.setTimeBarSeries(series);
            return appMDManager;
        }


    }
}
