using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.Test.app
{
    [TestFixture]
    class AInitialTest
    {
        private const String MOVER_DIR = @"..\data";
        private const String ORDER_BACKUP_REP_WILDCARD = "OrderRepositry_2*.csv";

        [Test]
        public void test_deleteOrderRepositryFile()
        {
            var dir = new DirectoryInfo(MOVER_DIR);
            foreach (var file in dir.EnumerateFiles(ORDER_BACKUP_REP_WILDCARD))
            {
                file.Delete();
            }
        }
    }
}
