using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IBTradeRealTime.UI;
using System.Globalization;

namespace IBTradeRealTime.util
{
    public class OrderFileReader
    {
        public static List<OrderRecord> getOrderRecorderFromFile(String fullPath)
        {
            List<String[]> strRecords = getResultDataFromFile(fullPath);
            List<OrderRecord> records = new List<OrderRecord>(); 
            foreach(String[] strRecord in strRecords){
                OrderRecord record = new OrderRecord();
                record.orderId = int.Parse((strRecord[0]));
                record.orderTime = DateTime.ParseExact(strRecord[1], "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                record.sno = strRecord[2];
                records.Add(record);
            }
            return records;
            
        }

        public static List<String[]> getResultDataFromFile(String fullPath)
        {
            List<String[]> resultDatas = new List<String[]>();
            //String fullPath = AppConstant.FILE_ORDER_REP;
            //FileStream file = File.OpenRead(fullPath);
            FileStream file = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
            var reader = new StreamReader(file);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                String key = values[0];
                resultDatas.Add(values);
            }
            file.Close();
            return resultDatas;
        }
    }
}
