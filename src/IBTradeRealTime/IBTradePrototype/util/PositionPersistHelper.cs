using IBTradeRealTime.AppOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.util
{
    public class PositionPersistHelper
    {
        private String filePath;
        private String suffix;

       public PositionPersistHelper(String strFilePath, String suffix)
       {
           //this.filePath = AppConstant.FILE_POSITION_RESULT_PREFIX;
           this.filePath = strFilePath;
           this.suffix = suffix;
       }

       public void Save(String filename, List<String> contents)
       {

           if (!System.IO.File.Exists(filename))
               System.IO.File.Create(filename).Close();

           System.IO.StreamWriter file = new System.IO.StreamWriter(filename,false);

           foreach (String prop in contents)
               if (!String.IsNullOrWhiteSpace(prop))
                   file.WriteLine(prop);
           file.Close();
       }

       private List<String> ConvertRowsToString(List<IAppOrder> orderRows)
       {
           List<String> stringRows = new List<String>();

           stringRows.Add("Execution Time, Current_Position, Buy_Sell, Trigger_Price, Execution_Price, PNL, REASON, RULE");
           double pnl = 0.0;
           foreach (AppOrder orderRow in orderRows)
           {
               String executionTime = "";
               String enterExit = "";
               String buySell = "";
               double triggerPrice = 0.0;
               double exePrice = 0.0;

               int currentPosition = 0;
               String reason = "";
               String rule = "";

               //enterExit = orderRow.EnterExit;
               buySell = orderRow.BuySell;
               triggerPrice = orderRow.TriggerPrice;
               reason = orderRow.Remark;
               rule = orderRow.TriggerRule;
               currentPosition = orderRow.Position;
               double numTrades = 0.0;
               double exeAmount = 0.0;
               foreach (AppExecution exe in orderRow.Executions)
               {
                   executionTime = exe.LastExecTime;
                   numTrades += exe.ExeShare;
                   exeAmount += exe.ExeShare * exe.AvgPrice;
               }
               exePrice = exeAmount / numTrades;


               if (AppConstant.BUY_SIGNAL.Equals(buySell))
               {
                   pnl = pnl - exePrice * numTrades;

               }
               else
               {
                   pnl = pnl + exePrice * numTrades;
               }
               /*

               if (currentPosition==0)
               {
                   if(AppConstant.BUY_SIGNAL.Equals(buySell)){
                       pnl = pnl - exePrice * numTrades;
               
                   }else{
                       pnl = exePrice * numTrades - pnl;
                   }
               }

               if (currentPosition!= 0)
               {
                   if (AppConstant.BUY_SIGNAL.Equals(buySell))
                   {
                       pnl = orderRow.EnterPrice * numTrades - exePrice * numTrades;

                   }
                   else
                   {
                       pnl = exePrice * numTrades - orderRow.EnterPrice * numTrades;
                   }
               }*/

               StringBuilder builder = new StringBuilder();
               builder.Append(executionTime).Append(AppConstant.SEPERATOR);
               builder.Append(currentPosition.ToString()).Append(AppConstant.SEPERATOR);
               builder.Append(buySell).Append(AppConstant.SEPERATOR);
               builder.Append(triggerPrice.ToString()).Append(AppConstant.SEPERATOR);
               builder.Append(exePrice.ToString()).Append(AppConstant.SEPERATOR);
               if (currentPosition == 0)
                 builder.Append(pnl.ToString()).Append(AppConstant.SEPERATOR);
               else
                 builder.Append(0.ToString()).Append(AppConstant.SEPERATOR);
               builder.Append(reason).Append(AppConstant.SEPERATOR);
               builder.Append(rule);

               String stringRow = builder.ToString();
               stringRows.Add(stringRow);
               if (currentPosition == 0)
               {
                   pnl = 0;
               }
           }
           /*
           foreach (OrderRecord orderRow in orderRows)
           {
               StringBuilder builder = new StringBuilder();
               builder.Append(orderRow.orderId).Append(AppConstant.SEPERATOR);
               builder.Append(formatDateString(orderRow.orderTime)).Append(AppConstant.SEPERATOR);
               builder.Append(orderRow.sno); 
               String stringRow = builder.ToString();
               stringRows.Add(stringRow);
           }*/
           return stringRows;
       }

       private String formatDateString(DateTime dt)
       {
           return String.Format("{0:MM/dd/yyyy hh:mm:ss tt}", dt);
       }

       public void SaveRows(String stgName, List<IAppOrder> orderRows)
       {
           //String fullPath = this.filePath + stgName + AppConstant.FILE_POSITION_RESULT_EXT;
           String fullPath = this.filePath + stgName + suffix + AppConstant.FILE_POSITION_RESULT_EXT;
           Save(fullPath, ConvertRowsToString(orderRows));
           //Save(this.filePath , ConvertRowsToString(orderRows));
       }
    }

}
