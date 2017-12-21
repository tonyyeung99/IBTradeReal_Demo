using IBTradeRealTime.app;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBTradeRealTime.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            String[] stgNames = { "stratey1", "stratey2", "stratey3" };
            ConcurrentDictionary<String, String> activeStgNamesMap = new ConcurrentDictionary<String, String>();
            activeStgNamesMap.TryAdd("stratey2", "stratey2" );
            activeStgNamesMap.TryAdd("stratey3", "stratey3");
            IAppStrategyManager strategyManager = new AppStrategyManager(null);
            strategyManager.injectActiveStgNamesMap(activeStgNamesMap);
            strategyManager.injectStgNames(stgNames);
            IAppEventManager eventManager = new AppEventManager(strategyManager);
            eventManager.startGenerateTimeEvents();
            Console.ReadLine();
        }
    }
}
