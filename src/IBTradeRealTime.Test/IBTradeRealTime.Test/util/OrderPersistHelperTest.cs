using IBTradeRealTime.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IBTradeRealTime.util
{
    [TestFixture]
    class OrderPersistHelperTest
    {
        private const String WRITER_PATH = @"..\data\OrderRepositry_Writer_Test.csv";
        private const String MOVER_PATH = @"..\data\OrderRepositry_Mover_Test.csv";
        private const String MOVER_DIR = @"..\data";
        private const String MOVER_WILDCARD = "OrderRepositry_Mover_Test_*.csv";
        private const String ORDER_BACKUP_REP_WILDCARD = "OrderRepositry_2*.csv";

        /*
        [Test]
        public void test_deleteOrderRepositryFile()
        {
            var dir = new DirectoryInfo(MOVER_DIR);
            foreach (var file in dir.EnumerateFiles(ORDER_BACKUP_REP_WILDCARD))
            {
                file.Delete();
            }
        }*/

        [Test]
        public void test_SaveLastRow(){

            File.Delete(WRITER_PATH);
            List<Task> tasks = new List<Task>();
            for (int i = 1; i <= 3; i++)
            {
                List<OrderRecord> records = create_records(i);
                Action<Object> action = ThreadPersistorTest;
                Task task = new Task(action, (object)records);
                tasks.Add(task);
                task.Start();
            }
            foreach (Task task in tasks)
            {
                task.Wait();
            }
            List<OrderRecord> returnRecords = OrderFileReader.getOrderRecorderFromFile(WRITER_PATH);
            Assert.AreEqual(27, returnRecords.Count );

            Dictionary<int, OrderRecord> map = convertListToMap(returnRecords);
            for (int i = 1; i <= 3; i++)
            {
                for (int j = 1; j <= 9; j++)
                {
                    int index = i*10+j;
                    OrderRecord mapRecord = map[index];
                    Assert.AreEqual(index, mapRecord.orderId);
                    Assert.AreEqual(new DateTime(2015, 12, 08, 0, i, j), mapRecord.orderTime);
                    Assert.AreEqual("S_RND" + i, mapRecord.sno);
                }
            }
        }

        [Test]
        public void test_renameOldfile()
        {
            deleteMoverFiles();
            createMoverFile();
            OrderPersistHelper helper = new OrderPersistHelper(MOVER_PATH);
            helper.renameOldfile();
            var dir = new DirectoryInfo(MOVER_DIR);

            FileInfo[] infos = dir.GetFiles(MOVER_WILDCARD);
            Console.WriteLine(infos.Length);
            Assert.Greater(infos.Length, 0);
        }


        static void ThreadPersistorTest(Object stateInfo)
        {
            List<OrderRecord> orderRecords = (List<OrderRecord>)stateInfo;
            OrderPersistHelper helper = new OrderPersistHelper(WRITER_PATH);    
            foreach (OrderRecord record in orderRecords)
            {
                List<OrderRecord> saveRecord = new List<OrderRecord>();
                saveRecord.Add(record);
                helper.SaveLastRow(saveRecord);
            }
        }


        private void deleteWriterFile()
        {
            File.Delete(WRITER_PATH);  
        }

        private void deleteMoverFiles()
        {
            File.Delete(MOVER_PATH);
            var dir = new DirectoryInfo(MOVER_DIR);
            foreach (var file in dir.EnumerateFiles(MOVER_WILDCARD))
            {
                file.Delete();
            }
        }
        private void createMoverFile()
        {

            FileStream stream = File.Create(MOVER_PATH);
            stream.Close();
            
        }

        private List<OrderRecord> create_records(int i)
        {
            List<OrderRecord> records = new List<OrderRecord>();
            for (int j = 1; j < 10; j++)
            {
                OrderRecord record1 = new OrderRecord();
                record1.orderId = i * 10 + j;
                record1.orderTime = new DateTime(2015, 12, 08, 0, i, j);
                record1.sno = "S_RND" + i;
                records.Add(record1);
            }
            return records;
        }

        private Dictionary<int, OrderRecord> convertListToMap(List<OrderRecord> lstRecords)
        {
            Dictionary<int, OrderRecord> map = new Dictionary<int, OrderRecord>();
            foreach (OrderRecord record in lstRecords)
            {
                map.Add(record.orderId, record);
            }
            return map;
        }
    }
}
