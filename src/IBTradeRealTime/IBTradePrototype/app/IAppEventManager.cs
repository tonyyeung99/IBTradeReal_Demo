using IBTradeRealTime.message;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.app
{
    public interface IAppEventManager
    {
        Dictionary<String, BlockingCollection<AppEvent>> storeEventQueue { get; set; }
        void injectStrategyManager(IAppStrategyManager stgManager);
        IAppStrategyManager getStgManager();
        void putTimeEvents(DateTime time);
        void putTickPriceEvents(TickPriceMessage priceMessage, MarketDataMessage dataMessage);
        void putOrderExeEvents(ExecutionMessage message, String stgNo);
        void startGenerateTimeEvents();
    }
}
