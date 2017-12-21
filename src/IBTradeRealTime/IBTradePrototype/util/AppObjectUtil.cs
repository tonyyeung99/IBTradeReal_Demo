using IBApi;
using IBTradeRealTime.AppOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.util
{
    public class AppObjectUtil
    {
        public static Boolean isOrderClosed(IAppOrder order)
        {
            int numFilled = 0;
            foreach (IAppExecution exe in order.Executions)
            {
                numFilled += exe.ExeShare;
            }
            if (order.TotalQuantity == numFilled)
                return true;
            else
                return false;
        }

        public static Order createMktOrder(String action, String userAccount, int orderSize)
        {
            Order order = new Order();
            order.OrderId = 0;
            order.Action = action;
            order.OrderType = AppConstant.ORDER_TYPE_MARKET;
            order.TotalQuantity = orderSize;
            order.Account = userAccount;
            order.Tif = "DAY";
            order.TriggerMethod = 0;
            order.Transmit = true;
            return order;
        }

        public static Order createStopOrder(String action, double triggerPrice, String userAccount, int orderSize)
        {
            Order order = new Order();
            order.OrderId = 0; 
            order.Action = action;
            order.OrderType = AppConstant.ORDER_TYPE_STOP;
            order.AuxPrice = triggerPrice;
            order.TotalQuantity = orderSize;
            order.Account = userAccount;
            order.Tif = "DAY";
            order.TriggerMethod = 0;
            order.Transmit = true;
            order.OutsideRth = true;
            return order;
        }

        public static Order createLimitOrder(String action, double triggerPrice, String userAccount, int orderSize)
        {
            Order order = new Order();
            order.OrderId = 0;
            order.Action = action;
            order.OrderType = AppConstant.ORDER_TYPE_LIMIT;
            order.LmtPrice = triggerPrice;
            order.TotalQuantity = orderSize;
            order.Account = userAccount;
            order.Tif = "DAY";
            order.TriggerMethod = 0;
            order.Transmit = true;
            return order;
        }

        public static AppOrder createAppOrder(String stgShortName, Order order, int stgIndex)
        {
            AppOrder appOrder = new AppOrder();
            appOrder.StrategyIndex = stgIndex;
            appOrder.OrderId = order.OrderId;
            appOrder.OrderType = order.OrderType;
            appOrder.BuySell = order.Action;
            appOrder.TotalQuantity = order.TotalQuantity;
            appOrder.LmtPrice = order.LmtPrice;
            appOrder.AuxPrice = order.AuxPrice;
            appOrder.StratgeyShortName = stgShortName;
            return appOrder;
        }

        /*
         AppOrder GetOrder(String action, String orderType);
         * Order GetOrder(String action, String orderType, int orderSize);
           public Order GetOppositeStopOrder(StrategySignalContext execution, Order parentOrder, double cutlosspt)
           public Order GetOppositeStopOrder(StrategySignalContext execution, Order parentOrder, double cutlosspt, int orderSize)
           public Order GetStopOrder(StrategySignalContext execution, String action, double triggerPrice)
           static Boolean isOrderClosed(AppOrder order)
         */ 
    }
}
