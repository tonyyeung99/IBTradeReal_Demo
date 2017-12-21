using IBApi;
using IBTradeRealTime.AppOrders;
using IBTradeRealTime.backend;
using IBTradeRealTime.MarketData;
using IBTradeRealTime.UI;
using IBTradeRealTime.util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.app
{
    public interface IAppStrategyManager
    {

        //ConcurrentDictionary<String, OrderRecord> OrderRepositry { get; set; }
        String UserAccount { get; set; }
        long TimeDiffServer { get; set; }
        Contract CurrentContract { get; set; }
        IIBTradeAppBridge ParentUI { get; set; }
        IBClient ibClient { get; set; }

        void injectStgNames(String[] stgNames);
        String[] getStgNames();
        void injectActiveStgNamesMap(ConcurrentDictionary<String, String> activeStgNamesMap);
        ConcurrentDictionary<String, String> getActiveStgNamesMap();
        void injectAppMDManager(IAppMDManager appMDManager);
        IAppMDManager getAppMDManager();
        void injectAppEventManager(IAppEventManager appEventManager);
        IAppEventManager getAppEventManager();
        void injectAppOrderManager(IAppOrderManager appOrderManager);
        IAppOrderManager getAppOrderManager();
        void injectOrderManager(OrderManager appOrderManager);
        void setOrderManager(OrderManager appOrderManager);
        OrderManager getOrderManager();

       void injectAppMainteanceManager(IAppMainteanceManager appMainteanceManager);
        IAppMainteanceManager getAppMainteanceManager();


        void calculateTimeDiffServer(long time);
        DateTime adjustClientTime(DateTime clientTime);

        void startStrategy(String name, int i, Dictionary<String, String> inputArgs, String userAccount, Contract contract, TickerInfo info);
        void stopStrategy(int index);

        DateTime getProxyNowTime();

        void dailyReset();
        void dailyDayEndExport();

        List<String[]> getPositionSummary();
    }
}
