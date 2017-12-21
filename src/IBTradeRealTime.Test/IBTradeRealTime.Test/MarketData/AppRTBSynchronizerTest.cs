using IBTradeRealTime.message;
using IBTradeRealTime.MarketData;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Deedle;
using IBTradeRealTime.util;
using IBTradeRealTime.UI;
using IBTradeRealTime.Test.UI;


namespace IBTradeRealTime.Test.MarketData
{

    [TestFixture]
    public class AppRTBSynchronizerTest
    {

        public static void main(String[] args){
            AppRTBSynchronizerTest test= new AppRTBSynchronizerTest();
            test.test_isRTBarMergeNeed();
        }

        [Test]
        public void test_isRTBarMergeNeed()
        {
            DateTime afterSetTime = new DateTime(2015, 11, 21, 9, 15, 55, DateTimeKind.Local);
            DateTime afterSetTime2 = new DateTime(2015, 11, 21, 9, 16, 0, DateTimeKind.Local);
            DateTime beforeSetTime = new DateTime(2015, 11, 21, 9, 10, 55, DateTimeKind.Local);
            DateTime beforeSetTime2 = new DateTime(2015, 11, 21, 9, 11, 0, DateTimeKind.Local);
            IBMessage afterSetTimeMessage = createTimeMessage(afterSetTime);
            IBMessage beforeSetTimeMessage = createTimeMessage(beforeSetTime);
            IBMessage afterSetTimeMessage2 = createTimeMessage(afterSetTime2);
            IBMessage beforeSetTimeMessage2 = createTimeMessage(beforeSetTime2);
            String strTickerStartTimeOfDay = "09:15:00";

            //[Test : method return true, message time equal to RtbDataStartTime]
            IAppMDManager appMDManager = getAppRTBSynchronizer_for_isRTBarMergeNeed(afterSetTime);
            AppRTBSynchronizer synchronizer = new AppRTBSynchronizer(appMDManager);
            //IAppRTBSynchronizer synchronizer = appMDManager.getAppRTBSynchronizer();
            Assert.AreEqual(true, synchronizer.isRTBarMergeNeed(strTickerStartTimeOfDay ));

            //[Test : method return true, message time later than RtbDataStartTime]
            appMDManager = getAppRTBSynchronizer_for_isRTBarMergeNeed(afterSetTime);
            synchronizer = new AppRTBSynchronizer(appMDManager);
            Assert.AreEqual(true, synchronizer.isRTBarMergeNeed( strTickerStartTimeOfDay));

            //[Test : method return false, RtbDataStartTime before the strTickerStartTimeOfDay]
            appMDManager = getAppRTBSynchronizer_for_isRTBarMergeNeed(beforeSetTime);
            synchronizer = new AppRTBSynchronizer(appMDManager);
            Assert.AreEqual(false, synchronizer.isRTBarMergeNeed( strTickerStartTimeOfDay ));

            //[Test : method return false, RtbDataStartTime before the strTickerStartTimeOfDay]
            appMDManager = getAppRTBSynchronizer_for_isRTBarMergeNeed(beforeSetTime);
            synchronizer = new AppRTBSynchronizer(appMDManager);
            Assert.AreEqual(false, synchronizer.isRTBarMergeNeed( strTickerStartTimeOfDay));
        }

        [Test]
        public void test_mergeRTBHistDataIfValid_positive1()
        {
            //[Test : merge success, histDataSeries and RTBDataSeries is continuous]
            IAppRTBSynchronizer synchronizer = getSynchronizer_for_mergeRTBHistDataIfValid_positive1();
            synchronizer.mergeRTBHistDataIfValid();
            IAppMDManager appMDManager = synchronizer.getAppMDManager();
            Series<DateTime, MarketDataElement> TimeBarSeries = appMDManager.getTimeBarSeries();
            Assert.AreEqual(8, TimeBarSeries.KeyCount);
            Assert.AreEqual(true, TimeBarSeries.TryGet(new DateTime(2015, 10, 21, 9, 15, 0, DateTimeKind.Local)).HasValue);
            Assert.AreEqual(true, TimeBarSeries.TryGet(new DateTime(2015, 10, 21, 9, 19, 0, DateTimeKind.Local)).HasValue);
            Assert.AreEqual(true, TimeBarSeries.TryGet(new DateTime(2015, 10, 21, 9, 20, 0, DateTimeKind.Local)).HasValue);
            Assert.AreEqual(true, TimeBarSeries.TryGet(new DateTime(2015, 10, 21, 9, 22, 0, DateTimeKind.Local)).HasValue);
            Assert.AreEqual(true, synchronizer.getIsDataMerged());
        }

        [Test]
        public void test_mergeRTBHistDataIfValid_positive2()
        {
            //[Test : merge success, histDataSeries and RTBDataSeries is not continuous]
            IAppRTBSynchronizer synchronizer = getSynchronizer_for_mergeRTBHistDataIfValid_positive2();
            synchronizer.mergeRTBHistDataIfValid();
            IAppMDManager appMDManager = synchronizer.getAppMDManager();
            Series<DateTime, MarketDataElement> TimeBarSeries = appMDManager.getTimeBarSeries();
            Assert.AreEqual(10, TimeBarSeries.KeyCount);
            Assert.AreEqual(true, TimeBarSeries.TryGet(new DateTime(2015, 10, 21, 9, 15, 0, DateTimeKind.Local)).HasValue);
            Assert.AreEqual(true, TimeBarSeries.TryGet(new DateTime(2015, 10, 21, 9, 23, 0, DateTimeKind.Local)).HasValue);
            Assert.AreEqual(true, TimeBarSeries.TryGet(new DateTime(2015, 10, 21, 9, 24, 0, DateTimeKind.Local)).HasValue);
            Assert.AreEqual(true, TimeBarSeries.TryGet(new DateTime(2015, 10, 21, 9, 26, 0, DateTimeKind.Local)).HasValue);
            Assert.AreEqual(true, synchronizer.getIsDataMerged());
        }

        [Test]
        public void test_mergeRTBHistDataIfValid_positive3()
        {
            //[Test : merge success, histDataSeries and RTBDataSeries is not continuous, datas in two series are overlapped]
            IAppRTBSynchronizer synchronizer = getSynchronizer_for_mergeRTBHistDataIfValid_positive3();
            synchronizer.mergeRTBHistDataIfValid();
            IAppMDManager appMDManager = synchronizer.getAppMDManager();
            Series<DateTime, MarketDataElement> TimeBarSeries = appMDManager.getTimeBarSeries();
            Assert.AreEqual(10, TimeBarSeries.KeyCount);
            Assert.AreEqual(true, TimeBarSeries.TryGet(new DateTime(2015, 10, 21, 9, 15, 0, DateTimeKind.Local)).HasValue);
            Assert.AreEqual(true, TimeBarSeries.TryGet(new DateTime(2015, 10, 21, 9, 23, 0, DateTimeKind.Local)).HasValue);
            Assert.AreEqual(true, TimeBarSeries.TryGet(new DateTime(2015, 10, 21, 9, 24, 0, DateTimeKind.Local)).HasValue);
            Assert.AreEqual(true, TimeBarSeries.TryGet(new DateTime(2015, 10, 21, 9, 26, 0, DateTimeKind.Local)).HasValue);
            Assert.AreEqual(true, synchronizer.getIsDataMerged());
        }

        [Test]
        public void test_mergeRTBHistDataIfValid_negative1()
        {
            //[Test : merge unsuccess, needMerge Flag is set to false]
            IAppRTBSynchronizer synchronizer = getSynchronizer_for_mergeRTBHistDataIfValid_negative1();
            synchronizer.setNeedMergeFlag(false);
            synchronizer.setIsDataMerged(false);
            synchronizer.mergeRTBHistDataIfValid();
            IAppMDManager appMDManager = synchronizer.getAppMDManager();
            Series<DateTime, MarketDataElement> TimeBarSeries = appMDManager.getTimeBarSeries();
            Assert.IsNull(TimeBarSeries);
            Assert.AreEqual(false, synchronizer.getIsDataMerged());

            //[Test : merge unsuccess, data merged Flag is set to true]
            synchronizer = getSynchronizer_for_mergeRTBHistDataIfValid_negative1();
            synchronizer.setNeedMergeFlag(true);
            synchronizer.setIsDataMerged(true);
            synchronizer.mergeRTBHistDataIfValid();
            appMDManager = synchronizer.getAppMDManager();
            TimeBarSeries = appMDManager.getTimeBarSeries();
            Assert.IsNull(TimeBarSeries);
        }

        [Test]
        public void test_mergeRTBHistDataIfValid_negative2()
        {
            //[Test : merge unsuccess, hist data is not fully loaded yet, missing the last hist data]
            IAppRTBSynchronizer synchronizer = getSynchronizer_for_mergeRTBHistDataIfValid_negative2();
            synchronizer.mergeRTBHistDataIfValid();
            IAppMDManager appMDManager = synchronizer.getAppMDManager();
            Series<DateTime, MarketDataElement> TimeBarSeries = appMDManager.getTimeBarSeries();
            Assert.IsNull(TimeBarSeries);
            Assert.AreEqual(false, synchronizer.getIsDataMerged());
        }

        [Test]
        public void test_mergeRTBHistDataIfValid_negative3()
        {
            //[Test : merge unsuccess, hist data is null]
            IAppRTBSynchronizer synchronizer = getSynchronizer_for_mergeRTBHistDataIfValid_negative3();
            synchronizer.mergeRTBHistDataIfValid();
            IAppMDManager appMDManager = synchronizer.getAppMDManager();
            Series<DateTime, MarketDataElement> TimeBarSeries = appMDManager.getTimeBarSeries();
            Assert.IsNull(TimeBarSeries);
            Assert.AreEqual(false, synchronizer.getIsDataMerged());
        }

        [Test]
        public void test_mergeRTBHistDataIfValid_negative4()
        {
            //[Test : merge unsuccess, RTB data serie is null]
            IAppRTBSynchronizer synchronizer = getSynchronizer_for_mergeRTBHistDataIfValid_negative4();
            synchronizer.mergeRTBHistDataIfValid();
            IAppMDManager appMDManager = synchronizer.getAppMDManager();
            Series<DateTime, MarketDataElement> TimeBarSeries = appMDManager.getTimeBarSeries();
            Assert.IsNull(TimeBarSeries);
            Assert.AreEqual(false, synchronizer.getIsDataMerged());
        }

        [Test]
        public void test_reqHistDataIfValid()
        {
            //[Test : Request Hist Data valid, needMergeFlag is true, isReqHistDataSent is false, current message time is after the hist data send time]
            DateTime processStartTime = new DateTime(2015, 10, 21, 10, 15, 0, DateTimeKind.Local);
            IAppMDManager appMDManager = getAppRTBSynchronizer_for_reqHistDataIfValid(processStartTime);
            AppRTBSynchronizer synchronizer = new AppRTBSynchronizer(appMDManager);
            synchronizer.setNeedMergeFlag(true);
            synchronizer.setIsReqHistDataSent(false);
            DateTime validStartTime = new DateTime(2015, 10, 21, 10, 15, 35, DateTimeKind.Local);
            //Console.Write(""+ synchronizer.);
            synchronizer.reqHistDataIfValid(validStartTime, "MIDPOINT", null);
            Assert.IsTrue(synchronizer.getIsReqHistDataSent());

            //[Test : Request Hist Data not valid, needMergeFlag is true, isReqHistDataSent is false, current message time is before the hist data send time]
            appMDManager = getAppRTBSynchronizer_for_reqHistDataIfValid(processStartTime);
            synchronizer = new AppRTBSynchronizer(appMDManager);
            synchronizer.setNeedMergeFlag(true);
            synchronizer.setIsReqHistDataSent(false);
            DateTime invalidStartTime = new DateTime(2015, 10, 21, 10, 15, 25, DateTimeKind.Local);
            synchronizer.reqHistDataIfValid(invalidStartTime, "MIDPOINT", null);
            Assert.IsFalse(synchronizer.getIsReqHistDataSent());
        }

        public void test_mergeRTBHistData()
        {
            //no need
        }

        [Test]
        public void test_updatePreMergeRTBarSeries()
        {
            DateTime validTime = new DateTime(2015, 10, 21, 9, 23, 0, DateTimeKind.Local);
            DateTime invalidTime = new DateTime(2015, 10, 21, 9, 22, 0, DateTimeKind.Local);

            //[Test : merge sucess]
            RTDataBar RTBar = new RTDataBar();
            RTBar.high = 20200;
            RTBar.open = 20100;
            RTBar.low = 20000;
            RTBar.close = 20150;
            RTBar.volume = 99999;
            RTBar.time = validTime;
            IAppRTBSynchronizer synchronizer = getSynchronizer_for_updatePreMergeRTBarSeries();
            synchronizer.setIsDataMerged(false);
            synchronizer.updatePreMergeRTBarSeries(RTBar);
            Assert.AreEqual(4, synchronizer.preMergeRTBarSeries.KeyCount);

            //[Test : merge unsucess, the time of real time bar is already in the sereise]
            RTBar = new RTDataBar();
            RTBar.high = 20200;
            RTBar.open = 20100;
            RTBar.low = 20000;
            RTBar.close = 20150;
            RTBar.volume = 99999;
            RTBar.time = invalidTime;
            synchronizer = getSynchronizer_for_updatePreMergeRTBarSeries();
            synchronizer.setIsDataMerged(false);
            synchronizer.updatePreMergeRTBarSeries(RTBar);
            Assert.AreEqual(3, synchronizer.preMergeRTBarSeries.KeyCount);

            //[Test : merge unsucess, the isDataMerged flag is already set to ture]
            RTBar = new RTDataBar();
            RTBar.high = 20200;
            RTBar.open = 20100;
            RTBar.low = 20000;
            RTBar.close = 20150;
            RTBar.volume = 99999;
            RTBar.time = validTime;
            synchronizer = getSynchronizer_for_updatePreMergeRTBarSeries();
            synchronizer.setIsDataMerged(true);
            synchronizer.updatePreMergeRTBarSeries(RTBar);
            Assert.AreEqual(3, synchronizer.preMergeRTBarSeries.KeyCount);
        }

        private IBMessage createStartAfterSetTimeMessage()
        {
            DateTime dt = new DateTime(2015, 11, 21, 9, 15, 35, DateTimeKind.Local).ToUniversalTime();
            DateTime dt0 = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan span = dt.Subtract(dt0);
            IBMessage RTBMessage = new RealTimeBarMessage(1000, Convert.ToInt64(span.TotalSeconds), 20100, 20200, 20000, 20100, 500, 20100, 0);
            return RTBMessage;
        }

        private IBMessage createTimeMessage(DateTime dt)
        {
            //DateTime dt = new DateTime(2015, 11, 21, 9, 10, 55);
            DateTime dt0 = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan span = dt.Subtract(dt0);
            IBMessage RTBMessage = new RealTimeBarMessage(1000, Convert.ToInt64(span.TotalSeconds), 20100, 20200, 20000, 20100, 500, 20100, 0);
            return RTBMessage;
        }

        /*
        private DateTime extractDateTimeFromMessage(IBMessage message)
        {
            RealTimeBarMessage rtBar = (RealTimeBarMessage)message;
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime dt = start.AddMilliseconds(rtBar.Timestamp * 1000).ToLocalTime();
            return dt;
        }*/

        private IAppMDManager getAppRTBSynchronizer_for_isRTBarMergeNeed(DateTime dt)
        {
            var mock = new Mock<IAppMDManager>();
            mock.Setup(foo => foo.getRtbDataStartTime()).Returns(dt);
            IAppMDManager fooObj = mock.Object;
            return fooObj;           
        }

        private IAppMDManager getAppRTBSynchronizer_for_reqHistDataIfValid(DateTime dt)
        {
            IAppMDManager appMDManager = new AppMDManager(new IBTradeApp());
            appMDManager.setRtbDataStartTime(dt);
            IIBTradeAppBridge mock = new IBTradeAppMock();
            appMDManager.injectParentUI(mock);
            return appMDManager;
        }

        private IAppRTBSynchronizer getSynchronizer_for_mergeRTBHistDataIfValid_positive1()
        {
            Series<DateTime, MarketDataElement> preMergeRTBarSeries = null;
            Series<DateTime, MarketDataElement> preMergeHistBarSeries = null;
            Series<DateTime, MarketDataElement> TimeBarSeries = null;

            DateTime histTime1 = new DateTime(2015, 10, 21, 9, 15, 0, DateTimeKind.Local);
            DateTime histTime2 = new DateTime(2015, 10, 21, 9, 16, 0, DateTimeKind.Local);
            DateTime histTime3 = new DateTime(2015, 10, 21, 9, 17, 0, DateTimeKind.Local);
            DateTime histTime4 = new DateTime(2015, 10, 21, 9, 18, 0, DateTimeKind.Local);
            DateTime histTime5 = new DateTime(2015, 10, 21, 9, 19, 0, DateTimeKind.Local);

            DateTime RTBTime1 = new DateTime(2015, 10, 21, 9, 20, 0, DateTimeKind.Local);
            DateTime RTBTime2 = new DateTime(2015, 10, 21, 9, 21, 0, DateTimeKind.Local);
            DateTime RTBTime3 = new DateTime(2015, 10, 21, 9, 22, 0, DateTimeKind.Local);


            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime1, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime2, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime3, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime4, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime5, preMergeHistBarSeries);

            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime1, preMergeRTBarSeries);
            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime2, preMergeRTBarSeries);
            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime3, preMergeRTBarSeries);

            AppMDManager appMDManager = new AppMDManager(new IBTradeApp());
            appMDManager.setTimeBarSeries(TimeBarSeries);
            appMDManager.setRtbDataStartTime(RTBTime1);
            appMDManager.setHistDataEndTime(histTime5);
            IAppRTBSynchronizer synchronizer = appMDManager.getAppRTBSynchronizer();
            synchronizer.preMergeHistBarSeries = preMergeHistBarSeries;
            synchronizer.preMergeRTBarSeries = preMergeRTBarSeries;
            synchronizer.setNeedMergeFlag(true);
            synchronizer.setIsDataMerged(false);
            return synchronizer;
        }

        private IAppRTBSynchronizer getSynchronizer_for_mergeRTBHistDataIfValid_positive2()
        {
            Series<DateTime, MarketDataElement> preMergeRTBarSeries = null;
            Series<DateTime, MarketDataElement> preMergeHistBarSeries = null;
            Series<DateTime, MarketDataElement> TimeBarSeries = null;

            DateTime histTime1 = new DateTime(2015, 10, 21, 9, 15, 0, DateTimeKind.Local);
            DateTime histTime2 = new DateTime(2015, 10, 21, 9, 16, 0, DateTimeKind.Local);
            DateTime histTime3 = new DateTime(2015, 10, 21, 9, 17, 0, DateTimeKind.Local);
            DateTime histTime4 = new DateTime(2015, 10, 21, 9, 18, 0, DateTimeKind.Local);
            DateTime histTime5 = new DateTime(2015, 10, 21, 9, 21, 0, DateTimeKind.Local);
            DateTime histTime6 = new DateTime(2015, 10, 21, 9, 22, 0, DateTimeKind.Local);
            DateTime histTime7 = new DateTime(2015, 10, 21, 9, 23, 0, DateTimeKind.Local);

            DateTime RTBTime1 = new DateTime(2015, 10, 21, 9, 24, 0, DateTimeKind.Local);
            DateTime RTBTime2 = new DateTime(2015, 10, 21, 9, 25, 0, DateTimeKind.Local);
            DateTime RTBTime3 = new DateTime(2015, 10, 21, 9, 26, 0, DateTimeKind.Local);


            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime1, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime2, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime3, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime4, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime5, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime6, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime7, preMergeHistBarSeries);

            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime1, preMergeRTBarSeries);
            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime2, preMergeRTBarSeries);
            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime3, preMergeRTBarSeries);

            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.setTimeBarSeries(TimeBarSeries);
            appMDManager.setRtbDataStartTime(RTBTime1);
            appMDManager.setHistDataEndTime(histTime7);
            IAppRTBSynchronizer synchronizer = appMDManager.getAppRTBSynchronizer();
            synchronizer.preMergeHistBarSeries = preMergeHistBarSeries;
            synchronizer.preMergeRTBarSeries = preMergeRTBarSeries;
            synchronizer.setNeedMergeFlag(true);
            synchronizer.setIsDataMerged(false);
            return synchronizer;
        }

        private IAppRTBSynchronizer getSynchronizer_for_mergeRTBHistDataIfValid_positive3()
        {
            Series<DateTime, MarketDataElement> preMergeRTBarSeries = null;
            Series<DateTime, MarketDataElement> preMergeHistBarSeries = null;
            Series<DateTime, MarketDataElement> TimeBarSeries = null;

            DateTime histTime1 = new DateTime(2015, 10, 21, 9, 15, 0, DateTimeKind.Local);
            DateTime histTime2 = new DateTime(2015, 10, 21, 9, 16, 0, DateTimeKind.Local);
            DateTime histTime3 = new DateTime(2015, 10, 21, 9, 17, 0, DateTimeKind.Local);
            DateTime histTime4 = new DateTime(2015, 10, 21, 9, 18, 0, DateTimeKind.Local);
            DateTime histTime5 = new DateTime(2015, 10, 21, 9, 21, 0, DateTimeKind.Local);
            DateTime histTime6 = new DateTime(2015, 10, 21, 9, 22, 0, DateTimeKind.Local);
            DateTime histTime7 = new DateTime(2015, 10, 21, 9, 23, 0, DateTimeKind.Local);
            DateTime histTime8 = new DateTime(2015, 10, 21, 9, 24, 0, DateTimeKind.Local);

            DateTime RTBTime1 = new DateTime(2015, 10, 21, 9, 24, 0, DateTimeKind.Local);
            DateTime RTBTime2 = new DateTime(2015, 10, 21, 9, 25, 0, DateTimeKind.Local);
            DateTime RTBTime3 = new DateTime(2015, 10, 21, 9, 26, 0, DateTimeKind.Local);


            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime1, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime2, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime3, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime4, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime5, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime6, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime7, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime8, preMergeHistBarSeries);

            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime1, preMergeRTBarSeries);
            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime2, preMergeRTBarSeries);
            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime3, preMergeRTBarSeries);

            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.setTimeBarSeries(TimeBarSeries);
            appMDManager.setRtbDataStartTime(RTBTime1);
            appMDManager.setHistDataEndTime(histTime7);
            IAppRTBSynchronizer synchronizer = appMDManager.getAppRTBSynchronizer();
            synchronizer.preMergeHistBarSeries = preMergeHistBarSeries;
            synchronizer.preMergeRTBarSeries = preMergeRTBarSeries;
            synchronizer.setNeedMergeFlag(true);
            synchronizer.setIsDataMerged(false);
            return synchronizer;
        }

        private IAppRTBSynchronizer getSynchronizer_for_mergeRTBHistDataIfValid_negative1()
        {
            Series<DateTime, MarketDataElement> preMergeRTBarSeries = null;
            Series<DateTime, MarketDataElement> preMergeHistBarSeries = null;
            Series<DateTime, MarketDataElement> TimeBarSeries = null;

            DateTime histTime1 = new DateTime(2015, 10, 21, 9, 15, 0, DateTimeKind.Local);
            DateTime histTime2 = new DateTime(2015, 10, 21, 9, 16, 0, DateTimeKind.Local);
            DateTime histTime3 = new DateTime(2015, 10, 21, 9, 17, 0, DateTimeKind.Local);
            DateTime histTime4 = new DateTime(2015, 10, 21, 9, 18, 0, DateTimeKind.Local);
            DateTime histTime5 = new DateTime(2015, 10, 21, 9, 21, 0, DateTimeKind.Local);
            DateTime histTime6 = new DateTime(2015, 10, 21, 9, 22, 0, DateTimeKind.Local);
            DateTime histTime7 = new DateTime(2015, 10, 21, 9, 23, 0, DateTimeKind.Local);

            DateTime RTBTime1 = new DateTime(2015, 10, 21, 9, 24, 0, DateTimeKind.Local);
            DateTime RTBTime2 = new DateTime(2015, 10, 21, 9, 25, 0, DateTimeKind.Local);
            DateTime RTBTime3 = new DateTime(2015, 10, 21, 9, 26, 0, DateTimeKind.Local);

            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime1, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime2, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime3, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime4, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime5, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime6, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime7, preMergeHistBarSeries);

            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime1, preMergeRTBarSeries);
            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime2, preMergeRTBarSeries);
            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime3, preMergeRTBarSeries);

            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.setTimeBarSeries(TimeBarSeries);
            appMDManager.setRtbDataStartTime(RTBTime1);
            appMDManager.setHistDataEndTime(histTime7);
            IAppRTBSynchronizer synchronizer = appMDManager.getAppRTBSynchronizer();
            synchronizer.preMergeHistBarSeries = preMergeHistBarSeries;
            synchronizer.preMergeRTBarSeries = preMergeRTBarSeries;
            synchronizer.setNeedMergeFlag(false);
            synchronizer.setIsDataMerged(false);
            return synchronizer;
        }

        private IAppRTBSynchronizer getSynchronizer_for_mergeRTBHistDataIfValid_negative2()
        {
            Series<DateTime, MarketDataElement> preMergeRTBarSeries = null;
            Series<DateTime, MarketDataElement> preMergeHistBarSeries = null;
            Series<DateTime, MarketDataElement> TimeBarSeries = null;

            DateTime histTime1 = new DateTime(2015, 10, 21, 9, 15, 0, DateTimeKind.Local);
            DateTime histTime2 = new DateTime(2015, 10, 21, 9, 16, 0, DateTimeKind.Local);
            DateTime histTime3 = new DateTime(2015, 10, 21, 9, 17, 0, DateTimeKind.Local);
            DateTime histTime4 = new DateTime(2015, 10, 21, 9, 18, 0, DateTimeKind.Local);
            DateTime histTime5 = new DateTime(2015, 10, 21, 9, 21, 0, DateTimeKind.Local);
            DateTime histTime6 = new DateTime(2015, 10, 21, 9, 22, 0, DateTimeKind.Local);
            DateTime histTime7 = new DateTime(2015, 10, 21, 9, 23, 0, DateTimeKind.Local);

            DateTime RTBTime1 = new DateTime(2015, 10, 21, 9, 24, 0, DateTimeKind.Local);
            DateTime RTBTime2 = new DateTime(2015, 10, 21, 9, 25, 0, DateTimeKind.Local);
            DateTime RTBTime3 = new DateTime(2015, 10, 21, 9, 26, 0, DateTimeKind.Local);

            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime1, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime2, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime3, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime4, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime5, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime6, preMergeHistBarSeries);
            //missing add histTime7

            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime1, preMergeRTBarSeries);
            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime2, preMergeRTBarSeries);
            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime3, preMergeRTBarSeries);

            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.setTimeBarSeries(TimeBarSeries);
            appMDManager.setRtbDataStartTime(RTBTime1);
            appMDManager.setHistDataEndTime(histTime7);
            IAppRTBSynchronizer synchronizer = appMDManager.getAppRTBSynchronizer();
            synchronizer.preMergeHistBarSeries = preMergeHistBarSeries;
            synchronizer.preMergeRTBarSeries = preMergeRTBarSeries;
            synchronizer.setNeedMergeFlag(true);
            synchronizer.setIsDataMerged(false);
            return synchronizer;
        }

        private IAppRTBSynchronizer getSynchronizer_for_mergeRTBHistDataIfValid_negative3()
        {
            Series<DateTime, MarketDataElement> preMergeRTBarSeries = null;
            Series<DateTime, MarketDataElement> preMergeHistBarSeries = null;
            Series<DateTime, MarketDataElement> TimeBarSeries = null;

            DateTime histTime1 = new DateTime(2015, 10, 21, 9, 15, 0, DateTimeKind.Local);
            DateTime histTime2 = new DateTime(2015, 10, 21, 9, 16, 0, DateTimeKind.Local);
            DateTime histTime3 = new DateTime(2015, 10, 21, 9, 17, 0, DateTimeKind.Local);
            DateTime histTime4 = new DateTime(2015, 10, 21, 9, 18, 0, DateTimeKind.Local);
            DateTime histTime5 = new DateTime(2015, 10, 21, 9, 21, 0, DateTimeKind.Local);
            DateTime histTime6 = new DateTime(2015, 10, 21, 9, 22, 0, DateTimeKind.Local);
            DateTime histTime7 = new DateTime(2015, 10, 21, 9, 23, 0, DateTimeKind.Local);

            DateTime RTBTime1 = new DateTime(2015, 10, 21, 9, 24, 0, DateTimeKind.Local);
            DateTime RTBTime2 = new DateTime(2015, 10, 21, 9, 25, 0, DateTimeKind.Local);
            DateTime RTBTime3 = new DateTime(2015, 10, 21, 9, 26, 0, DateTimeKind.Local);

            //missing add all hist data completely

            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime1, preMergeRTBarSeries);
            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime2, preMergeRTBarSeries);
            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime3, preMergeRTBarSeries);

            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.setTimeBarSeries(TimeBarSeries);
            appMDManager.setRtbDataStartTime(RTBTime1);
            appMDManager.setHistDataEndTime(histTime7);
            IAppRTBSynchronizer synchronizer = appMDManager.getAppRTBSynchronizer();
            synchronizer.preMergeHistBarSeries = preMergeHistBarSeries;
            synchronizer.preMergeRTBarSeries = preMergeRTBarSeries;
            synchronizer.setNeedMergeFlag(true);
            synchronizer.setIsDataMerged(false);
            return synchronizer;
        }

        private IAppRTBSynchronizer getSynchronizer_for_mergeRTBHistDataIfValid_negative4()
        {
            Series<DateTime, MarketDataElement> preMergeRTBarSeries = null;
            Series<DateTime, MarketDataElement> preMergeHistBarSeries = null;
            Series<DateTime, MarketDataElement> TimeBarSeries = null;

            DateTime histTime1 = new DateTime(2015, 10, 21, 9, 15, 0, DateTimeKind.Local);
            DateTime histTime2 = new DateTime(2015, 10, 21, 9, 16, 0, DateTimeKind.Local);
            DateTime histTime3 = new DateTime(2015, 10, 21, 9, 17, 0, DateTimeKind.Local);
            DateTime histTime4 = new DateTime(2015, 10, 21, 9, 18, 0, DateTimeKind.Local);
            DateTime histTime5 = new DateTime(2015, 10, 21, 9, 21, 0, DateTimeKind.Local);
            DateTime histTime6 = new DateTime(2015, 10, 21, 9, 22, 0, DateTimeKind.Local);
            DateTime histTime7 = new DateTime(2015, 10, 21, 9, 23, 0, DateTimeKind.Local);

            DateTime RTBTime1 = new DateTime(2015, 10, 21, 9, 24, 0, DateTimeKind.Local);
            DateTime RTBTime2 = new DateTime(2015, 10, 21, 9, 25, 0, DateTimeKind.Local);
            DateTime RTBTime3 = new DateTime(2015, 10, 21, 9, 26, 0, DateTimeKind.Local);

            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime1, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime2, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime3, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime4, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime5, preMergeHistBarSeries);
            preMergeHistBarSeries = addMarketDataElementWithTimeOnlyToSeries(histTime6, preMergeHistBarSeries);

            //missing add all RTB data completely

            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.setTimeBarSeries(TimeBarSeries);
            appMDManager.setRtbDataStartTime(RTBTime1);
            appMDManager.setHistDataEndTime(histTime7);
            IAppRTBSynchronizer synchronizer = appMDManager.getAppRTBSynchronizer();
            synchronizer.preMergeHistBarSeries = preMergeHistBarSeries;
            synchronizer.preMergeRTBarSeries = preMergeRTBarSeries;
            synchronizer.setNeedMergeFlag(true);
            synchronizer.setIsDataMerged(false);
            return synchronizer;
        }

        private IAppRTBSynchronizer getSynchronizer_for_updatePreMergeRTBarSeries()
        {
            Series<DateTime, MarketDataElement> preMergeRTBarSeries = null;
            Series<DateTime, MarketDataElement> preMergeHistBarSeries = null;
            Series<DateTime, MarketDataElement> TimeBarSeries = null;

            DateTime RTBTime1 = new DateTime(2015, 10, 21, 9, 20, 0, DateTimeKind.Local);
            DateTime RTBTime2 = new DateTime(2015, 10, 21, 9, 21, 0, DateTimeKind.Local);
            DateTime RTBTime3 = new DateTime(2015, 10, 21, 9, 22, 0, DateTimeKind.Local);

            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime1, preMergeRTBarSeries);
            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime2, preMergeRTBarSeries);
            preMergeRTBarSeries = addMarketDataElementWithTimeOnlyToSeries(RTBTime3, preMergeRTBarSeries);

            AppMDManager appMDManager = new AppMDManager(null);
            appMDManager.setTimeBarSeries(TimeBarSeries);
            appMDManager.setRtbDataStartTime(RTBTime1);

            IAppRTBSynchronizer synchronizer = appMDManager.getAppRTBSynchronizer();
            synchronizer.preMergeHistBarSeries = preMergeHistBarSeries;
            synchronizer.preMergeRTBarSeries = preMergeRTBarSeries;
            synchronizer.setNeedMergeFlag(true);
            synchronizer.setIsDataMerged(false);
            return synchronizer;
        }
        private static Series<DateTime, MarketDataElement>  addMarketDataElementWithTimeOnlyToSeries(DateTime dateTime, Series<DateTime, MarketDataElement> series)
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
    }
}
