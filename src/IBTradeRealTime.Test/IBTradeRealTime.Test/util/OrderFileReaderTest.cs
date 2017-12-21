using IBTradeRealTime.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.util
{
    [TestFixture]
    class OrderFileReaderTest
    {
        [Test]
        public void test_getOrderRecorderFromFile()
        {
            List<OrderRecord> records = OrderFileReader.getOrderRecorderFromFile(@"..\data\OrderRepositry_Reader_Test.csv");
            Assert.AreEqual(records.Count, 13);
            OrderRecord record1 = records[0];
            OrderRecord record2 = records[6];
            OrderRecord record3 = records[12];
            Assert.AreEqual(new DateTime(2015, 12, 8, 10, 52, 05), record1.orderTime);
            Assert.AreEqual(33, record1.orderId);
            Assert.AreEqual("S1_RND1", record1.sno); 

            Assert.AreEqual(new DateTime(2015, 12, 8, 11, 33, 05), record2.orderTime);
            Assert.AreEqual(43, record2.orderId);
            Assert.AreEqual("S1_RND1", record2.sno);

            Assert.AreEqual(new DateTime(2015, 12, 8, 11, 58, 30), record3.orderTime);
            Assert.AreEqual(49, record3.orderId);
            Assert.AreEqual("S1_RND1", record3.sno);

        }
    }
}
