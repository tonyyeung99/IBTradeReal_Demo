using IBTradeRealTime.app;
using IBTradeRealTime.AppOrders;
using IBTradeRealTime.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.Strategy
{
    public class SignalContext : ISignalContext
    {
        public int CurrentMarketPosition { get; set; }
        public String PendingSignalSide1 { get; set; }
        public String PendingSignalSide2 { get; set; }
        public double PendingSignalPrice1 { get; set; }
        public double PendingSignalPrice2 { get; set; }
        public IAppOrder PendingOrder1 { get; set; }
        public IAppOrder PendingOrder2 { get; set; }
        public String PendingSignalRemark1 { get; set; }
        public String PendingSignalRemark2 { get; set; }
        public double PnL { get; set; }
        public SignalContext()
        {
            CurrentMarketPosition = 0;
            PendingSignalSide1 = "";
            PendingSignalSide2 = "";
            PendingSignalPrice1 = 0;
            PendingSignalPrice2 = 0;
            PendingOrder1 = null;
            PendingOrder2 = null;
            PendingSignalRemark1 = "";
            PendingSignalRemark2 = "";
            PnL = 0;
        }

        public void reset(){
            CurrentMarketPosition = 0;
            PendingSignalSide1 = "";
            PendingSignalSide2 = "";
            PendingSignalPrice1 = 0;
            PendingSignalPrice2 = 0;
            PendingOrder1 = null;
            PendingOrder2 = null;
            PendingSignalRemark1 = "";
            PendingSignalRemark2 = "";
            PnL = 0;
        }

        public Boolean isSignalBidirection() {
            if (PendingOrder1 != null && PendingOrder2 != null)
                return true;
            return false;        
        }

        public void setPendingSignal1(String side, double price, IAppOrder order, String remark)
        {
            PendingSignalSide1 = side;
            PendingSignalPrice1 = price;
            PendingOrder1 = order;
            PendingSignalRemark1 = remark;
        }

        public void setPendingSignal2(String side, double price, IAppOrder order, String remark)
        {
            PendingSignalSide2 = side;
            PendingSignalPrice2 = price;
            PendingOrder2 = order;
            PendingSignalRemark2 = remark;
        }

        public String getCompleteSignalSide()
        {
            if (PendingOrder1 != null && AppObjectUtil.isOrderClosed(PendingOrder1))
                return PendingSignalSide1;
            if (PendingOrder2 != null && AppObjectUtil.isOrderClosed(PendingOrder2))
                return PendingSignalSide2;
            return null;
        }

        public double getFilledPrice()
        {
            double totalAmount = 0.0;
            double totalQuantity = 0.0;

            IAppOrder tempOrder = getClosedAppOrder();
            /*
            if (pendingOrder1 != null && AppObjectUtil.isOrderClosed(pendingOrder1))
                tempOrder = pendingOrder1;
            if (pendingOrder2 != null && AppObjectUtil.isOrderClosed(pendingOrder2))
                tempOrder = pendingOrder2;
            */
            if (tempOrder == null)
                return double.NaN;

            foreach (IAppExecution exe in tempOrder.Executions)
            {
                totalAmount += exe.AvgPrice * exe.ExeShare;
                totalQuantity += exe.ExeShare;
            }
            return totalAmount / totalQuantity;
        }

        public IAppOrder getClosedAppOrder()
        {
            IAppOrder tempOrder = null;
            if (PendingOrder1 != null && AppObjectUtil.isOrderClosed(PendingOrder1))
                tempOrder = PendingOrder1;
            if (PendingOrder2 != null && AppObjectUtil.isOrderClosed(PendingOrder2))
                tempOrder = PendingOrder2;
            return tempOrder;
        }

        public bool isPendingOrderSet()
        {
            if (PendingOrder1 != null || PendingOrder2 != null)
                return true;
            return false;
        }

        public void completePendingSignal()
        {
            IAppOrder order = getClosedAppOrder();
            if (order != null)
            {
                if (order != null)
                {
                    lock (order)
                    {
                        int pos = 0;
                        if (AppConstant.BUY_SIGNAL.Equals(order.BuySell))
                        {
                            pos = order.TotalQuantity;
                            PnL = PnL - getFilledPrice() * getQuantityForExecution();
                        }
                        if (AppConstant.SELL_SIGNAL.Equals(order.BuySell))
                        {
                            pos = -order.TotalQuantity;
                            PnL = PnL + getFilledPrice() * getQuantityForExecution();
                        }

                        CurrentMarketPosition += pos;
                    }
                }
                order.Position = CurrentMarketPosition;
            }
        }

        public void flushCompletSignal()
        {
            PendingSignalSide1 = "";
            PendingSignalSide2 = "";
            PendingSignalPrice1 = 0;
            PendingSignalPrice2 = 0;
            PendingOrder1 = null;
            PendingOrder2 = null;
            PendingSignalRemark1 = "";
            PendingSignalRemark2 = "";
        }

        public IAppOrder[] getPendingOrders()
        {
            IAppOrder[] orders = new IAppOrder[2];
            orders[0] = PendingOrder1;
            orders[1] = PendingOrder2;
            return orders;
        }

        public int getIndexClosedAppOrder()
        {
            if (PendingOrder1 != null && AppObjectUtil.isOrderClosed(PendingOrder1))
                return 1;
            if (PendingOrder2 != null && AppObjectUtil.isOrderClosed(PendingOrder2))
                return 2;
            return 0;
        }

        public Boolean isPositionSignalBothEmpty()
        {
            if (CurrentMarketPosition == 0 && !isPendingOrderSet())
                return true;
            return false;
        }

        public Boolean isPositionEmptySignalOnOpen()
        {
            if (CurrentMarketPosition == 0 && isPendingOrderSet() && getClosedAppOrder()==null)
                return true;
            return false;
        }

        public Boolean isPositionEmptySignalOnClose()
        {
            if (CurrentMarketPosition == 0 && isPendingOrderSet() && getClosedAppOrder()!=null)
                return true;
            return false;
        }

        public Boolean isPositionOnSignalEmpty()
        {
            if (CurrentMarketPosition != 0 && !isPendingOrderSet())
                return true;
            return false;
        }

        public Boolean isPositionOnSignalOnOpen()
        {
            if (CurrentMarketPosition != 0 && isPendingOrderSet() && getClosedAppOrder()==null)
                return true;
            return false;
        }
        public Boolean isPositionOnSignalOnClose()
        {
            if (CurrentMarketPosition != 0 && isPendingOrderSet() && getClosedAppOrder()!=null)
                return true;
            return false;
        }

        public double getSignalPriceForExecution()
        {
            int index = getIndexClosedAppOrder();
            if (index == 1)
                return PendingSignalPrice1;
            else if (index == 2)
                return PendingSignalPrice2;
            return 0;
        }

        public double getQuantityForExecution()
        {
            int index = getIndexClosedAppOrder();
            if (index == 1)
                return PendingOrder1.TotalQuantity;
            else if (index == 2)
                return PendingOrder2.TotalQuantity;
            return 0;
        }
    }
}
