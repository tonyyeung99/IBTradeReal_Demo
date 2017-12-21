using IBApi;
using IBTradeRealTime.AppOrders;
using IBTradeRealTime.backend;
using IBTradeRealTime.MarketData;
using IBTradeRealTime.Strategy;
using IBTradeRealTime.StrategyImpl;
using IBTradeRealTime.UI;
using IBTradeRealTime.util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IBTradeRealTime.app
{
    public class AppStrategyManager : IAppStrategyManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IAppMDManager appMDManager;
        IAppEventManager appEventManager;
        IAppMainteanceManager appMaintenanceManager;
        IAppOrderManager appOrderManager;
        OrderManager orderManager;
        //private String[] stgNames = { AppConstant.STG1_SHORT_NAME, AppConstant.STG2_SHORT_NAME, AppConstant.STG3_SHORT_NAME, AppConstant.STG4_SHORT_NAME, AppConstant.STG5_SHORT_NAME, AppConstant.STG6_SHORT_NAME, AppConstant.STG7_SHORT_NAME };
        //private String[] stgNames = { AppConstant.STG1_SHORT_NAME, AppConstant.STG2_SHORT_NAME, AppConstant.STG3_SHORT_NAME};
        private String[] stgNames = { AppConstant.STG1_SHORT_NAME, AppConstant.STG2_SHORT_NAME, AppConstant.STG3_SHORT_NAME, AppConstant.STG4_SHORT_NAME };

        private Dictionary<String, IStrategy> storeStg;
        ConcurrentDictionary<String, String> activeStgNamesMap;
        private ConcurrentDictionary<String, CancellationTokenSource> StoreCancelToken; 


        private int numMaxStg = 3;

        public AppStrategyManager(IIBTradeAppBridge appForm)
        {
            
            appMDManager = new AppMDManager(appForm);
            appEventManager = new AppEventManager(this);
            appOrderManager = new AppOrderManager(this);
            appMaintenanceManager = new AppMainteanceManager(this);
            appMaintenanceManager.startManager();
            ParentUI = appForm;
            //OrderRepositry = new ConcurrentDictionary<string, OrderRecord>();
            activeStgNamesMap = new ConcurrentDictionary<String, String>();
            storeStg = new Dictionary<string, IStrategy>();
            storeStg.Add(AppConstant.STG1_SHORT_NAME, new StrategyRandom1(AppConstant.STG1_SHORT_NAME, 0));
            storeStg.Add(AppConstant.STG2_SHORT_NAME, new StrategyRBreakerReverse1(AppConstant.STG2_SHORT_NAME, 1));
            storeStg.Add(AppConstant.STG3_SHORT_NAME, new StrategyRBreakerTrend1(AppConstant.STG3_SHORT_NAME, 2));
            storeStg.Add(AppConstant.STG4_SHORT_NAME, new StrategyRandom4(AppConstant.STG4_SHORT_NAME, 3));
            /*
            storeStg.Add(AppConstant.STG5_SHORT_NAME, new StrategyStastic1(AppConstant.STG5_SHORT_NAME, 4));
            storeStg.Add(AppConstant.STG6_SHORT_NAME, new StrategyRBreaker1(AppConstant.STG6_SHORT_NAME, 5));
            storeStg.Add(AppConstant.STG7_SHORT_NAME, new StrategyRBreakerTrend2(AppConstant.STG7_SHORT_NAME, 6));
             */
            StoreCancelToken = new ConcurrentDictionary<string, CancellationTokenSource>();
            TimeDiffServer = long.MaxValue;
            appEventManager.startGenerateTimeEvents();

        }

        //new dailyReset
        public void dailyReset()
        {            
            appMDManager.dailyReset();
            appOrderManager.dailyReset();
            ParentUI.dailyReset();
        }

        public List<String[]> getPositionSummary()
        {
            return ParentUI.getPositionSummary();
        }

        //new dailyReset
        public void dailyDayEndExport()
        {
            ConcurrentDictionary<String, String> stgNamesMap = getActiveStgNamesMap();
            stgNamesMap.Keys.ToArray();
            foreach (String name in stgNamesMap.Keys)
            {
                exportReport(name);
            }
        }
        //new dailyReset
        public void exportReport(int stgIndex)
        {
            String stgName = stgNames[stgIndex];
            exportReport(stgName);
        }

        //new dailyReset
        public void exportReport(String name)
        {
            IAppOrderManager appOrderManager = getAppOrderManager();
            String prefix =  AppConstant.FILE_POSITION_RESULT_PREFIX;
            String suffix = String.Format("{0:_yyyyMMdd}", DateTime.Now);
            PositionPersistHelper helper = new PositionPersistHelper(prefix, suffix);
            helper.SaveRows(name, appOrderManager.StoreStgClosedOrders[name]);
        }

        
        //public ConcurrentDictionary<String, OrderRecord> OrderRepositry { get; set; }
        public String UserAccount { get; set; }
        public Contract CurrentContract { get; set; }
        public IIBTradeAppBridge ParentUI { get; set; }
        public IBClient ibClient { get; set; }
        public long TimeDiffServer { get; set; }

        public void calculateTimeDiffServer(long serverTimeStamp)
        {
            if (!TimeDiffServer.Equals(long.MaxValue))
                return;
            DateTime clientTime = getProxyNowTime();
            DateTime serverTime = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime st = serverTime.AddMilliseconds(serverTimeStamp * 1000).ToLocalTime();
            TimeDiffServer = (st - clientTime).Ticks;
            //DateTime revisedTime = DateTime.Now.AddSeconds(30).AddTicks(TimeDiffServer);
        }

        public DateTime adjustClientTime(DateTime clientTime)
        {
            return clientTime.AddTicks(TimeDiffServer);
        }

        public void injectStgNames(String[] stgNames)
        {
            this.stgNames = stgNames;
        }
        public String[] getStgNames()
        {
            return this.stgNames;
        }
        public void injectActiveStgNamesMap(ConcurrentDictionary<String, String> activeStgNamesMap)
        {
            this.activeStgNamesMap = activeStgNamesMap;
        }
        public ConcurrentDictionary<String, String> getActiveStgNamesMap()
        {
            //activeStgNamesMap.AddOrUpdate(AppConstant.STG1_SHORT_NAME, AppConstant.STG1_SHORT_NAME, (key, oldValue) => oldValue);
            return this.activeStgNamesMap;
        }

        public void injectAppMDManager(IAppMDManager appMDManager)
        {
            this.appMDManager = appMDManager;
        }
        public IAppMDManager getAppMDManager()
        {
            return appMDManager;
        }
        public void injectAppEventManager(IAppEventManager eventManager)
        {
            this.appEventManager = eventManager;
        }
        public IAppEventManager getAppEventManager()
        {
            return this.appEventManager;
        }

        public void injectAppOrderManager(IAppOrderManager appOrderManager)
        {
            this.appOrderManager = appOrderManager;
        }
        public IAppOrderManager getAppOrderManager()
        {
            return this.appOrderManager;
        }

        public void injectOrderManager(OrderManager orderManager)
        {
            this.orderManager = orderManager;
        }
        public void setOrderManager(OrderManager orderManager)
        {
            this.orderManager = orderManager;
        }

        public OrderManager getOrderManager()
        {
            return orderManager;
        }

        public void injectAppMainteanceManager(IAppMainteanceManager appMainteanceManager)
        {
            this.appMaintenanceManager = appMainteanceManager;
        }
        public IAppMainteanceManager getAppMainteanceManager()
        {
            return appMaintenanceManager;
        }

        public void startStrategy(String name, int i, Dictionary<String, String> inputArgs, String userAccount, Contract contract, TickerInfo info)
        {            
            this.UserAccount = userAccount;
            this.CurrentContract = contract;
            String stgName = stgNames[i];

            CancellationTokenSource cts = new CancellationTokenSource();
            StoreCancelToken.AddOrUpdate(stgName, cts, (key, oldValue) => cts);
            IStrategy stg = storeStg[stgName];
            stg.init(this, inputArgs, info);
            Action<object> actionStrategy = strategyHandler;
            Task strategyTask = new Task(actionStrategy, new StrategyArg() { number = i, strategy = stg, ct = cts.Token });
            activeStgNamesMap.AddOrUpdate(stgName,stgName, (key, oldValue) => oldValue);
            strategyTask.Start();
        }

        public void strategyHandler(Object arg)
        {
            StrategyArg stgArg = (StrategyArg)arg;
            IStrategy stg = stgArg.strategy;
            CancellationToken ct = stgArg.ct;
            int number = stgArg.number;
            while (true)
            {
                stg.calculate_signals(null, null);
                if (ct.IsCancellationRequested)
                    break;
            }
            StrategyOnOff onOff = new StrategyOnOff();
            onOff.stgIndex = stgArg.number;
            onOff.isOn = false;
            ParentUI.HandleStrategyOnOff(onOff);
        }

        public void stopStrategy(int index)
        {
            String stgName = stgNames[index];
            CancellationTokenSource cts = StoreCancelToken[stgName];
            String value;            
            cts.Cancel();
            Thread.Sleep(2000);
            activeStgNamesMap.TryRemove(stgName, out value);
        }

        public DateTime getProxyNowTime()
        {
            return DateTime.Now;
        }
    }
}
