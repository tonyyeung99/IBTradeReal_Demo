using IBTradeRealTime.MarketData;
using IBTradeRealTime.message;
using IBTradeRealTime.util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.util
{
    [TestFixture]
    class MarketDataUtilTest
    {

        [Test]
        public void test_isThisMessageEndOfMinute(){
            IBMessage message = createEndOfMinuteMessage();
            Assert.AreEqual(true, MarketDataUtil.isThisMessageEndOfMinute(message));

            message = createStartOfMinuteMessage();
            Assert.AreEqual(false, MarketDataUtil.isThisMessageEndOfMinute(message)); 
        }

        [Test]
        public void test_isThisMessageStartOfMinute(){
            IBMessage message = createStartOfMinuteMessage();
            Assert.AreEqual(true, MarketDataUtil.isThisMessageStartOfMinute(message));
            
            message = createEndOfMinuteMessage();
            Assert.AreEqual(false, MarketDataUtil.isThisMessageStartOfMinute(message));
        }

        [Test]
        public void test_convertBarToMarketDataElement(){

            RTDataBar RTBar = new RTDataBar();
            RTBar.high = 20200;
            RTBar.open = 20100;
            RTBar.low = 20000;
            RTBar.close = 20150;
            RTBar.volume = 99999;
            RTBar.time = new DateTime(2015, 10, 21, 9, 49, 0);
            MarketDataElement element = MarketDataUtil.convertBarToMarketDataElement(RTBar);
            Assert.AreEqual(RTBar.high, element.high) ;
            Assert.AreEqual(RTBar.low, element.low);
            Assert.AreEqual(RTBar.open, element.open);
            Assert.AreEqual(RTBar.close, element.close);
            Assert.AreEqual(RTBar.volume, element.volume);
            Assert.AreEqual(RTBar.time, element.time);
        }

        //ToDo: getStartTime()

        private IBMessage createEndOfMinuteMessage()
        {
            DateTime dt = new DateTime(2015, 11, 21, 9, 15, 55, DateTimeKind.Local).ToUniversalTime();
            DateTime dt0 = new DateTime(1970, 1, 1, 0, 0, 0).ToUniversalTime();
            TimeSpan span = dt.Subtract(dt0);
            IBMessage RTBMessage = new RealTimeBarMessage(1000, Convert.ToInt64(span.TotalSeconds), 20100, 20200, 20000, 20100, 500, 20100, 0);
            return RTBMessage;
        }

        private IBMessage createStartOfMinuteMessage()
        {
            DateTime dt = new DateTime(2015, 11, 21, 9, 16, 0, DateTimeKind.Local).ToUniversalTime();
            DateTime dt0 = new DateTime(1970, 1, 1, 0, 0, 0).ToUniversalTime();
            TimeSpan span = dt.Subtract(dt0);
            IBMessage RTBMessage = new RealTimeBarMessage(1000, Convert.ToInt64(span.TotalSeconds), 20100, 20200, 20000, 20100, 500, 20100, 0);
            return RTBMessage;
        }

        /*
        private IBMessage createEndOfMinuteMessage()
        {
            DateTime dt = new DateTime(2015, 11, 21, 10, 1, 55);
            DateTime dt0 = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan span = dt.Subtract(dt0);
            IBMessage RTBMessage = new RealTimeBarMessage(1000, Convert.ToInt64(span.TotalSeconds), 20100, 20200, 20000, 20100, 500, 20100, 0);
            return RTBMessage;
        }

       private IBMessage createStartOfMinuteMessage(){
           DateTime dt = new DateTime(2015, 11, 21, 10, 0, 0);
           DateTime dt0 = new DateTime(1970, 1, 1, 0, 0, 0);
           TimeSpan span = dt.Subtract(dt0);
           IBMessage RTBMessage = new RealTimeBarMessage(1000, Convert.ToInt64(span.TotalSeconds), 20100, 20200, 20000, 20100, 500, 20100, 0);
            return RTBMessage;
        }
         */ 
    }
}
