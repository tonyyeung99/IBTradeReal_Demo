using IBTradeRealTime.message;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace IBTradeRealTime.app
{
    public class AppEventManager : IAppEventManager
    {
        IAppStrategyManager stgManager;

        private static System.Timers.Timer timer;

        public AppEventManager(IAppStrategyManager stgManager)
        {
            this.stgManager = stgManager;
            String[] stgNames = stgManager.getStgNames();
            storeEventQueue = new Dictionary<String, BlockingCollection<AppEvent>>();
            foreach (String name in stgNames)
            {
                storeEventQueue.Add(name, new BlockingCollection<AppEvent>());
            }
        }

        public Dictionary<String, BlockingCollection<AppEvent>> storeEventQueue { get; set; }
        

        public void injectStrategyManager(IAppStrategyManager stgManager)
        {
            this.stgManager = stgManager;
        }

        public IAppStrategyManager getStgManager()
        {
            return stgManager;
        }

        
        public void putTimeEvents(DateTime time)
        {
            ConcurrentDictionary<String, String> stgNamesMap = stgManager.getActiveStgNamesMap();
            AppTimeEvent timeEvent = new AppTimeEvent();
            timeEvent.eventTime = time;
            stgNamesMap.Keys.ToArray();
            foreach (String name in stgNamesMap.Keys)
            {
                storeEventQueue[name].Add(timeEvent);
            }

            IAppMainteanceManager maintenanceManager = stgManager.getAppMainteanceManager();
            if (maintenanceManager != null)
            {
                maintenanceManager.storeEventQueue.Add(timeEvent);
            }
        }

        public void putTickPriceEvents(TickPriceMessage priceMessage, MarketDataMessage dataMessage)
        {
            ConcurrentDictionary<String, String> stgNamesMap = stgManager.getActiveStgNamesMap();
            AppTickPriceEvent tickPriceEvent = new AppTickPriceEvent();
            tickPriceEvent.time = stgManager.getProxyNowTime();
            tickPriceEvent.value = priceMessage.Price;
            tickPriceEvent.field = dataMessage.Field;
            tickPriceEvent.tickerId = priceMessage.RequestId;
            stgNamesMap.Keys.ToArray();
            foreach (String name in stgNamesMap.Keys)
            {
                storeEventQueue[name].Add(tickPriceEvent);
            }
        }
        public void putOrderExeEvents(ExecutionMessage message, String stgNo)
        {
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

            AppOrderExecutedEvent exeEvent = new AppOrderExecutedEvent();
            exeEvent.TickerName = message.Contract.LocalSymbol;
            exeEvent.BQty = bqty.ToString();
            exeEvent.SQty = sqty.ToString();
            exeEvent.Price = message.Execution.Price.ToString();
            exeEvent.SNo = stgNo;           
            exeEvent.Status = "Filled";
            exeEvent.Time = message.Execution.Time;
            
            IAppMainteanceManager maintenanceManager = stgManager.getAppMainteanceManager();
            if (maintenanceManager != null)
            {
                maintenanceManager.storeEventQueue.Add(exeEvent);
            }
        }

        public void startGenerateTimeEvents()
        {
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += onTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        public void onTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (stgManager.TimeDiffServer.Equals(long.MaxValue))
                return;
            DateTime currentTime = stgManager.getProxyNowTime();
            DateTime adjustTime = stgManager.adjustClientTime(currentTime).ToLocalTime();
            putTimeEvents(adjustTime);
        }
    }
}
