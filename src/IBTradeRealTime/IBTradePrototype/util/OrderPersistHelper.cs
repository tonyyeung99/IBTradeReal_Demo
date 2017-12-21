using IBTradeRealTime.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IBTradeRealTime.util
{
    public class OrderPersistHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private String filePath;

       public OrderPersistHelper(String argFilePath)
       {
           //this.filePath = AppConstant.FILE_ORDER_REP;
           this.filePath = argFilePath;

       }

       public void renameOldfile(){
           int delimitIndex = filePath.LastIndexOf(".");
           String prefix = filePath.Substring(0, delimitIndex);
           String suffix = filePath.Substring(delimitIndex );
           String strTimestamp = String.Format("{0:_yyyyMMdd_HHmmss_fff}",DateTime.Now);
           String newFileName = prefix + strTimestamp + suffix;
           if(File.Exists(filePath)){
            System.IO.File.Move(filePath, newFileName);
           }

           //String.Format("{0:MM/dd/yyyy hh:mm:ss tt}", dt);
       }
       /*
       public void Save(String filename, List<String> contents)
       {

           if (!System.IO.File.Exists(filename))
               System.IO.File.Create(filename).Close();

           System.IO.StreamWriter file = new System.IO.StreamWriter(filename,true);

           foreach (String prop in contents)
               if (!String.IsNullOrWhiteSpace(prop))
                   file.WriteLine(prop);
           file.Close();
       }*/
       async public Task Save(String filename, List<String> contents)
       {
           lock (AppConstant.OrderRepositoryLock)
           {
               using (FileStream sourceStream = new FileStream(filePath,FileMode.Append, FileAccess.Write, FileShare.ReadWrite,
               bufferSize: 4096, useAsync: true))
               {
                   StreamWriter writer = new StreamWriter(sourceStream);
                   List<Task> tasks = new List<Task>();
                   foreach (String prop in contents)
                       if (!String.IsNullOrWhiteSpace(prop))
                       {
                           String result = prop.Replace("\n", String.Empty).Replace("\t", String.Empty).Replace("\r", String.Empty);
                           result = result + Environment.NewLine;
                           tasks.Add(WriteTextAsyncLine(writer, result));
                       }
                   foreach (Task task in tasks)
                   {
                       task.Wait();
                   }
                   writer.Close();
               }
           }
       }
       async Task WriteTextAsyncLine(StreamWriter writer, string text)
       {
           int count = 100;
           while (true)
           {
               try
               {
                   await writer.WriteAsync(text);
               }
               catch (InvalidOperationException e)
               {
                   count--;
                   Thread.Sleep(100);
                   if (count <= 0)
                   {
                       log.Error("Persist Order : Retry 100 times and skip persist action");
                       return;
                   }
                   continue;
               }
               break;
           }
       }

       private List<String> ConvertRowsToString(List<OrderRecord> orderRows){
           List<String> stringRows = new List<String>();
           foreach (OrderRecord orderRow in orderRows)
           {
               StringBuilder builder = new StringBuilder();
               builder.Append(orderRow.orderId).Append(AppConstant.SEPERATOR);
               builder.Append(formatDateString(orderRow.orderTime)).Append(AppConstant.SEPERATOR);
               builder.Append(orderRow.sno); 
               String stringRow = builder.ToString();
               stringRows.Add(stringRow);
           }
           return stringRows;
       }

       private String formatDateString(DateTime dt)
       {
           return String.Format("{0:MM/dd/yyyy hh:mm:ss tt}", dt);
       }
       /*
       public void SaveRows(List<OrderRecord> orderRows)
       {
           Save(this.filePath , ConvertRowsToString(orderRows));
       }*/

       async public Task SaveRows(List<OrderRecord> orderRows)
       {
           await Save(this.filePath, ConvertRowsToString(orderRows));
           return;
       }
        /*
       public void SaveLastRow(List<OrderRecord> orderRows)
       {
           List<OrderRecord> lastRow = new List<OrderRecord>();
           lastRow.Add(orderRows[orderRows.Count - 1]);
           Save(this.filePath , ConvertRowsToString(lastRow));
       }*/
       async public Task SaveLastRow(List<OrderRecord> orderRows)
       {
           List<OrderRecord> lastRow = new List<OrderRecord>();
           lastRow.Add(orderRows[orderRows.Count - 1]);
           await Save(this.filePath, ConvertRowsToString(lastRow));
       }
    }
}
