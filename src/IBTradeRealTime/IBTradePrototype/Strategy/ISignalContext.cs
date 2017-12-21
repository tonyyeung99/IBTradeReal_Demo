using IBTradeRealTime.app;
using IBTradeRealTime.AppOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.Strategy
{
    public interface ISignalContext
    {
        int CurrentMarketPosition { get; set; }
        String PendingSignalSide1 { get; set; }
        String PendingSignalSide2 { get; set; }
        double PendingSignalPrice1 { get; set; }
        double PendingSignalPrice2 { get; set; }
        IAppOrder PendingOrder1 { get; set; }
        IAppOrder PendingOrder2 { get; set; }
        double PnL { get; set; }
        
        Boolean isSignalBidirection();
        void setPendingSignal1(String side, double price, IAppOrder order, String remark);
        void setPendingSignal2(String side, double price, IAppOrder order, String remark);
        String getCompleteSignalSide();
        double getFilledPrice();
        double getQuantityForExecution();
        IAppOrder getClosedAppOrder();
        bool isPendingOrderSet();
        void completePendingSignal();
        void flushCompletSignal();
        //Boolean isEmptyPositionAndSignal();
        IAppOrder[] getPendingOrders();
        int getIndexClosedAppOrder();


        Boolean isPositionSignalBothEmpty();
        Boolean isPositionEmptySignalOnOpen();
        Boolean isPositionEmptySignalOnClose();
        Boolean isPositionOnSignalEmpty();
        Boolean isPositionOnSignalOnOpen();
        Boolean isPositionOnSignalOnClose();

    }
}
