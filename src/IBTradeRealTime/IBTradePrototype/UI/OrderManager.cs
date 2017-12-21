using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IBApi;

using IBTradeRealTime.backend;
using IBTradeRealTime.message;
using IBTradeRealTime.UI;
using IBTradeRealTime.util;
using System.Windows.Forms;
using IBTradeRealTime.app;
using IBTradeRealTime.AppOrders;

namespace IBTradeRealTime.UI
{
    public class OrderManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //private OrderDialog orderDialog;
        private IBClient ibClient;
        private List<string> managedAccounts;

        private List<OpenOrderMessage> openOrders = new List<OpenOrderMessage>();

        private DataGridView liveOrdersGrid;
        private DataGridView tradeLogGrid;
        private IBTradeApp appForm;

        private AppStrategyManager appStgManager;

        public OrderManager(IBClient ibClient, DataGridView liveOrdersGrid, DataGridView tradeLogGrid, IBTradeApp appForm)
        {
            this.ibClient = ibClient;
            //this.orderDialog = new OrderDialog(this);
            this.liveOrdersGrid = liveOrdersGrid;
            this.tradeLogGrid = tradeLogGrid;
            this.appForm = appForm;
            this.appStgManager = appForm.appStrategyManager;

        }


        public List<string> ManagedAccounts
        {
            get { return managedAccounts; }
            set
            {
                //orderDialog.SetManagedAccounts(value);
                managedAccounts = value;
            }
        }

        public void CancelOrder(Order order)
        {
             ibClient.ClientSocket.cancelOrder(order.OrderId);
        }

        public void PlaceOrder(Contract contract, Order order)
        {
            if (order.OrderId != 0)
            {
                ibClient.ClientSocket.placeOrder(order.OrderId, contract, order);
            }
            else
            {
                ibClient.ClientSocket.placeOrder(ibClient.IncreaseOrderId(), contract, order);
                //ibClient.ClientSocket.placeOrder(ibClient.NextOrderId, contract, order);
                //ibClient.NextOrderId++;
            }
        }

        public void UpdateUI(IBMessage message)
        {
            switch (message.Type)
            {
                case MessageType.OpenOrder:
                    handleOpenOrder((OpenOrderMessage)message);
                    break;
                case MessageType.OpenOrderEnd:
                    break;
                case MessageType.OrderStatus:
                    handleOrderStatus((OrderStatusMessage)message);
                    break;
                case MessageType.ExecutionData:
                    HandleExecutionMessage((ExecutionMessage)message);
                    break;
                case MessageType.CommissionsReport:
                    HandleCommissionMessage((CommissionMessage)message);
                    break;
            }
        }

        public void OpenOrderDialog()
        {
            //orderDialog.ShowDialog();
        }

        public void EditOrder()
        {
            if (liveOrdersGrid.SelectedRows.Count > 0 && (int)(liveOrdersGrid.SelectedRows[0].Cells[2].Value) != 0 && (int)(liveOrdersGrid.SelectedRows[0].Cells[1].Value) == ibClient.ClientId)
            {
                DataGridViewRow selectedRow = liveOrdersGrid.SelectedRows[0];
                int orderId = (int)selectedRow.Cells[2].Value;
                for (int i = 0; i < openOrders.Count; i++)
                {
                    if (openOrders[i].OrderId == orderId)
                    {
                        //orderDialog.SetOrderContract(openOrders[i].Contract);
                        //orderDialog.SetOrder(openOrders[i].Order);
                    }
                }

                //orderDialog.ShowDialog();
            }
        }

        public void CancelSelection()
        {
            if (liveOrdersGrid.SelectedRows.Count > 0)
            {
                for (int i = 0; i < liveOrdersGrid.SelectedRows.Count; i++)
                {
                    int orderId = (int)liveOrdersGrid.SelectedRows[i].Cells[2].Value;
                    int clientId = (int)liveOrdersGrid.SelectedRows[i].Cells[1].Value;
                    OpenOrderMessage openOrder = GetOpenOrderMessage(orderId, clientId);
                    if (openOrder != null)
                        ibClient.ClientSocket.cancelOrder(openOrder.OrderId);
                }
            }
        }

        private OpenOrderMessage GetOpenOrderMessage(int orderId, int clientId)
        {
            for (int i = 0; i < openOrders.Count; i++)
            {
                if (openOrders[i].Order.OrderId == orderId && openOrders[i].Order.ClientId == clientId)
                    return openOrders[i];
            }
            return null;
        }

        private void HandleCommissionMessage(CommissionMessage message)
        {
            for (int i = 0; i < tradeLogGrid.Rows.Count; i++)
            {
                if (((string)tradeLogGrid[0, i].Value).Equals(message.CommissionReport.ExecId))
                {
                    tradeLogGrid[7, i].Value = message.CommissionReport.Commission;
                    tradeLogGrid[8, i].Value = message.CommissionReport.RealizedPNL;
                }
            }
        }

        private void handleOpenOrder(OpenOrderMessage openOrder)
        {
            /*
            if (openOrder.Order.WhatIf)
                orderDialog.HandleIncomingMessage(openOrder);
            else
            {
                UpdateLiveOrders(openOrder);
                UpdateLiveOrdersGrid(openOrder);
            }
            */
            UpdateLiveOrders(openOrder);
            UpdateLiveOrdersGrid(openOrder);
        }

        private void HandleExecutionMessage(ExecutionMessage message)
        {
            for (int i = 0; i < tradeLogGrid.Rows.Count; i++)
            {
                if (((string)tradeLogGrid[0, i].Value).Equals(message.Execution.ExecId))
                {
                    PopulateTradeLog(i, message);
                }
            }
            tradeLogGrid.Rows.Add(1);
            PopulateTradeLog(tradeLogGrid.Rows.Count - 1, message);
        }

        public void processExecution(ExecutionMessage message)
        {

            IAppOrderManager orderManager = appStgManager.getAppOrderManager();
            String[] stgNames = appStgManager.getStgNames();
            if (!appStgManager.getAppOrderManager().AppOrderStore.ContainsKey(message.Execution.OrderId))
            {
                return;
            }
            if (!orderManager.isExecutionProcessed(message.Execution.ExecId))
            {
                AppExecution execution = orderManager.updateAppOrderExecution(message);
                IAppOrder order = orderManager.AppOrderStore[message.Execution.OrderId];
                AppOrder cloneOrder = (AppOrder)order.Clone();
                if (AppObjectUtil.isOrderClosed(cloneOrder))
                {
                    orderManager.closeOpenAppOrder(order.StrategyIndex, order);
                }
                orderManager.updatePosition(order.StrategyIndex, execution);
                String stgName = stgNames[order.StrategyIndex];
                AppPosition stgPosition = orderManager.StoreStgPositions[stgName];
                log.Info("Total PnL : " + stgPosition.TotalPnL);
                orderManager.markExeProcessed(message.Execution.ExecId);

                //***New for send order executed email***
                IAppEventManager appEventManager = appStgManager.getAppEventManager();
                appEventManager.putOrderExeEvents(message, stgName);
                /*TSAppExecution execution = appForm.StrategyManager.updateAppOrderExecution(message);
                TSAppOrder order = appForm.StrategyManager.AppOrderStore[message.Execution.OrderId]; 
                TSAppOrder cloneOrder = (TSAppOrder)order.Clone();
                if (StrategyManager.isOrderClosed(cloneOrder))
                {
                    appForm.StrategyManager.closeOpenAppOrder(order.StrategyIndex, order);
                    //appForm.StrategyManager.removeOpenAppOrder(order.StrategyIndex, order);
                    //appForm.StrategyManager.addClosedAppOrder(order.StrategyIndex, order);
                }
                appForm.StrategyManager.updatePosition(order.StrategyIndex, execution);
                String stgName = appForm.StrategyManager.StgNames[order.StrategyIndex];
                AppPosition stgPosition = appForm.StrategyManager.StoreStgPositions[stgName];
                log.Info("Total PnL : " + stgPosition.TotalPnL);
                //log.Info("Total PnL : " + appForm.StrategyManager.StgPositions[order.StrategyIndex].TotalPnL);
                appForm.StrategyManager.markExeProcessed(message.Execution.ExecId);*/
            }
            log.Info("Order Executed");
            log.Info("*********************************************");
        }

        private void PopulateTradeLog(int index, ExecutionMessage message)
        {
            /*
            tradeLogGrid[0, index].Value = message.Execution.ExecId;
            tradeLogGrid[1, index].Value = message.Execution.Time;
            tradeLogGrid[2, index].Value = message.Execution.AcctNumber;
            tradeLogGrid[3, index].Value = message.Execution.Side;
            tradeLogGrid[4, index].Value = message.Execution.Shares;
            tradeLogGrid[5, index].Value = message.Contract.Symbol + " " + message.Contract.SecType + " " + message.Contract.Exchange;
            tradeLogGrid[6, index].Value = message.Execution.Price;
             */
            String side = message.Execution.Side;
            int bqty = 0;
            int sqty = 0;
            if ("BOT".Equals(side))
            {
                bqty  = message.Execution.Shares;
            }
            if ("SLD".Equals(side))
            {
                sqty =  message.Execution.Shares;
            }

            OrderRecord record = null;
            if (appStgManager.getAppOrderManager().OrderRepositry.ContainsKey(message.Execution.OrderId.ToString()))
            {
                record = appStgManager.getAppOrderManager().OrderRepositry[message.Execution.OrderId.ToString()];
            }
            tradeLogGrid[0, index].Value = message.Execution.ExecId;
            tradeLogGrid[1, index].Value = message.Contract.LocalSymbol;
            tradeLogGrid[2, index].Value = bqty.ToString();
            tradeLogGrid[3, index].Value = sqty.ToString();
            tradeLogGrid[4, index].Value = message.Execution.Price;
            tradeLogGrid[5, index].Value = message.Execution.Time;
            tradeLogGrid[6, index].Value = "Filled";
            if(record!=null)
                tradeLogGrid[7, index].Value = record.sno; 


      }

        private void handleOrderStatus(OrderStatusMessage statusMessage)
        {
            for (int i = 0; i < liveOrdersGrid.Rows.Count; i++)
            {
                if (liveOrdersGrid[0, i].Value.Equals(statusMessage.PermId))
                {
                    liveOrdersGrid[7, i].Value = statusMessage.Status;
                    return;
                }
            }
        }

        private void UpdateLiveOrders(OpenOrderMessage orderMesage)
        {
            for (int i = 0; i < openOrders.Count; i++)
            {
                if (openOrders[i].Order.OrderId == orderMesage.OrderId)
                {
                    openOrders[i] = orderMesage;
                    return;
                }
            }
            openOrders.Add(orderMesage);
        }

        private void UpdateLiveOrdersGrid(OpenOrderMessage orderMessage)
        {
            for (int i = 0; i < liveOrdersGrid.Rows.Count; i++)
            {
                if ((int)(liveOrdersGrid[2, i].Value) == orderMessage.Order.OrderId)
                {
                    PopulateOrderRow(i, orderMessage);
                    return;
                }
            }
            liveOrdersGrid.Rows.Add(1);
            PopulateOrderRow(liveOrdersGrid.Rows.Count - 1, orderMessage);
        }

        private void PopulateOrderRow(int rowIndex, OpenOrderMessage orderMessage)
        {
            /*
            liveOrdersGrid[0, rowIndex].Value = orderMessage.Order.PermId;
            liveOrdersGrid[1, rowIndex].Value = orderMessage.Order.ClientId;
            liveOrdersGrid[2, rowIndex].Value = orderMessage.Order.OrderId;
            liveOrdersGrid[3, rowIndex].Value = orderMessage.Order.Account;
            liveOrdersGrid[4, rowIndex].Value = orderMessage.Order.Action;
            liveOrdersGrid[5, rowIndex].Value = orderMessage.Order.TotalQuantity;
            liveOrdersGrid[6, rowIndex].Value = orderMessage.Contract.Symbol + " " + orderMessage.Contract.SecType + " " + orderMessage.Contract.Exchange;
            liveOrdersGrid[7, rowIndex].Value = orderMessage.OrderState.Status;
             */
            String action = orderMessage.Order.Action;
            int bqty = 0;
            int sqty = 0;
            if ("BUY".Equals(action))
            {
                bqty = orderMessage.Order.TotalQuantity;
            }
            if ("SELL".Equals(action))
            {
                sqty = orderMessage.Order.TotalQuantity;
            }
            OrderRecord record = null;
            if (appStgManager.getAppOrderManager().OrderRepositry.ContainsKey(orderMessage.OrderId.ToString()))
            {
                record = appStgManager.getAppOrderManager().OrderRepositry[orderMessage.OrderId.ToString()];
            }
            liveOrdersGrid[0, rowIndex].Value = orderMessage.Order.PermId;
            liveOrdersGrid[1, rowIndex].Value = orderMessage.Contract.LocalSymbol;
            liveOrdersGrid[2, rowIndex].Value = bqty;
            liveOrdersGrid[3, rowIndex].Value = sqty;
            liveOrdersGrid[4, rowIndex].Value = orderMessage.Order.LmtPrice;
            liveOrdersGrid[5, rowIndex].Value = orderMessage.Order.AuxPrice;
            if (record != null)
                liveOrdersGrid[6, rowIndex].Value = String.Format("{0:yyyyMMdd HH:mm:ss}", record.orderTime);
            liveOrdersGrid[7, rowIndex].Value = orderMessage.OrderState.Status;
            //liveOrdersGrid[8, rowIndex].Value = String.Format("{0:0,0.0}", orderMessage.OrderState.Commission);

            liveOrdersGrid[8, rowIndex].Value =  orderMessage.OrderState.Commission;
            if(record!=null)
             liveOrdersGrid[9, rowIndex].Value =  record.sno;
        }
    }
}
