using IBTradeRealTime.AppOrders;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.util
{
    [TestFixture]
    class PositionPersistHelperTest
    {
        private const String  WRITER_PATH = @"..\data\PositionResult_";

        private String writer_suffix_path = "";

        private String getSavePath()
        {
            return WRITER_PATH + "S_RND1" + AppConstant.FILE_POSITION_RESULT_EXT;
        }
        [Test]
        public void test_SaveRows1()
        {
            File.Delete(WRITER_PATH);
            writer_suffix_path = String.Format("{0:_yyyyMMdd}", DateTime.Now);
            PositionPersistHelper helper = new PositionPersistHelper(WRITER_PATH, writer_suffix_path);
            IAppOrder buyOrder = createBuyOrder_10Contract_10Complete(10, "Enter Buy" , "Random Buy Rule");
            IAppOrder sellOrder = createSellOrder_10Contract_10Complete(0,  "Exit Sell" , "Random Sell Rule");
            List<IAppOrder> orders = new List<IAppOrder>();
            orders.Add(buyOrder);
            orders.Add(sellOrder);
            helper.SaveRows("S_RND1", orders);
            List<String[]> records = getContentFromFile();

            String[] record1 = records[1];
            String[] record2 = records[2];

            Assert.AreEqual("10", record1[1].Trim());
            Assert.AreEqual("BUY", record1[2].Trim());
            Assert.AreEqual("20000", record1[3].Trim());
            Assert.AreEqual("20140", record1[4].Trim());
            Assert.AreEqual("0", record1[5].Trim());
            Assert.AreEqual("Enter Buy", record1[6].Trim());
            Assert.AreEqual("Random Buy Rule", record1[7].Trim());

            Assert.AreEqual("0", record2[1].Trim());
            Assert.AreEqual("SELL", record2[2].Trim());
            Assert.AreEqual("20400", record2[3].Trim());
            Assert.AreEqual("20400", record2[4].Trim());
            Assert.AreEqual("2600", record2[5].Trim());
            Assert.AreEqual("Exit Sell", record2[6].Trim());
            Assert.AreEqual("Random Sell Rule", record2[7].Trim());  
        }

        private List<String[]> getContentFromFile()
        {
            List<String[]> resultDatas = new List<String[]>();
            FileStream file = new FileStream(getSavePath(), FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
            var reader = new StreamReader(file);
            while (!reader.EndOfStream){                
                var line = reader.ReadLine();
                var values = line.Split(',');
                String key = values[0];
                resultDatas.Add(values);
            }
            return resultDatas;
        }


        private IAppOrder createBuyOrder_10Contract_10Complete(int position, String reason, String rule )
        {
            IAppOrder appOrder = new AppOrder();
            appOrder.StrategyIndex = 3;
            appOrder.OrderId = 1001;
            appOrder.OrderType = AppConstant.ORDER_TYPE_LIMIT;
            appOrder.BuySell = AppConstant.BUY_SIGNAL;
            appOrder.TotalQuantity = 10;
            appOrder.Position = position;
            appOrder.LmtPrice = 20000;
            appOrder.TriggerPrice = 20000;
            appOrder.AuxPrice = 0;

            appOrder.TriggerRule = rule;
            appOrder.Remark = reason;

            appOrder.addExecution(createBuyExecution1Contract());
            appOrder.addExecution(createBuyExecution4Contract());
            appOrder.addExecution(createBuyExecution5Contract());
            appOrder.StratgeyShortName = "Test Strategy 1";
            return appOrder;
        }

        private IAppOrder createSellOrder_10Contract_10Complete(int position, String reason, String rule)
        {
            IAppOrder appOrder = new AppOrder();
            appOrder.StrategyIndex = 3;
            appOrder.OrderId = 1002;
            appOrder.OrderType = AppConstant.ORDER_TYPE_LIMIT;
            appOrder.BuySell = AppConstant.SELL_SIGNAL;
            appOrder.TotalQuantity = position;
            appOrder.Position = 0;
            appOrder.LmtPrice = 20400;
            appOrder.TriggerPrice = 20400;
            appOrder.AuxPrice = 0;

            appOrder.TriggerRule = rule;
            appOrder.Remark = reason;

            appOrder.addExecution(createSellExecution10Contract());
            appOrder.StratgeyShortName = "Test Strategy 1";
            return appOrder;
        }

        private IAppExecution createSellExecution10Contract()
        {
            IAppExecution execution = new AppExecution();
            execution.OrderId = 1002;
            execution.ExecId = "1104";
            execution.LastExecTime = "2015-11-11 01:01:01";
            execution.Side = AppConstant.SELL_SIGNAL;
            execution.AvgPrice = 20400;
            execution.ExeShare = 10;
            return execution;
        }

        private IAppExecution createBuyExecution1Contract()
        {
            IAppExecution execution = new AppExecution();
            execution.OrderId = 1001;
            execution.ExecId = "1002";
            execution.LastExecTime = "2015-11-11 01:01:01";
            execution.Side = AppConstant.BUY_SIGNAL;
            execution.AvgPrice = 20000;
            execution.ExeShare = 1;
            return execution;
        }

        private IAppExecution createBuyExecution4Contract()
        {
            IAppExecution execution = new AppExecution();
            execution.OrderId = 1001;
            execution.ExecId = "1003";
            execution.LastExecTime = "2015-11-11 01:02:01";
            execution.Side = AppConstant.BUY_SIGNAL;
            execution.AvgPrice = 20100;
            execution.ExeShare = 4;
            return execution;
        }

        private IAppExecution createBuyExecution5Contract()
        {
            IAppExecution execution = new AppExecution();
            execution.OrderId = 1001;
            execution.ExecId = "1004";
            execution.LastExecTime = "2015-11-11 01:03:01";
            execution.Side = AppConstant.BUY_SIGNAL;
            execution.AvgPrice = 20200;
            execution.ExeShare = 5;
            return execution;
        }
    }
}
