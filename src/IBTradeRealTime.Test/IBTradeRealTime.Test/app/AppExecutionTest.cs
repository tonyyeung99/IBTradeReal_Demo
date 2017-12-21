using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using IBTradeRealTime.app;
using IBTradeRealTime.AppOrders;

namespace IBTradeRealTime.Test.app
{
    [TestFixture]
    class AppExecutionTest
    {

        [Test]
        public void testAppExe() 
        {
            AppExecution exe = new AppExecution();
            exe.OrderId = 1;
            exe.ExecId = "ABC";
            exe.LastExecTime = "2015-10-10";
            exe.Side = "BUY";
            exe.AvgPrice = 100;
            exe.ExeShare = 2000;
            
            double value = 100 * 2000;
            Assert.AreEqual(value, exe.AvgPrice * exe.ExeShare);
        }

    }
}
