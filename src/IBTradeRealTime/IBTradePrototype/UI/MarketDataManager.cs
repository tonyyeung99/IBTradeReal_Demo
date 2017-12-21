using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IBApi;
using IBTradeRealTime.backend;
using System.Windows.Forms;
using IBTradeRealTime.message;
using IBTradeRealTime.util;
using IBTradeRealTime.app;
using IBTradeRealTime.MarketData;

namespace IBTradeRealTime.UI
{
    public class MarketDataManager : DataManager
    {
        public const int TICK_ID_BASE = 10000000;

        private const int DESCRIPTION_INDEX = 1;

        private const int BID_PRICE_INDEX = 3;
        private const int ASK_PRICE_INDEX = 4;
        private const int CLOSE_PRICE_INDEX = 11;
        private const int LAST_PRICE_INDEX = 6;

        private const int LOW_PRICE_INDEX = 10;
        private const int HIGH_PRICE_INDEX = 9;

        private const int BID_SIZE_INDEX = 2;
        private const int ASK_SIZE_INDEX = 5;
        private const int LAST_SIZE_INDEX = 7;
        private const int VOLUME_SIZE_INDEX = 8;

        private List<Contract> activeRequests = new List<Contract>();

        //private StrategyManager stgManager;
        private IAppStrategyManager appStrategyManager;

        public MarketDataManager(IBClient client, DataGridView dataGrid, IAppStrategyManager appStgManager)
            : base(client, dataGrid)
        {
            this.appStrategyManager = appStgManager;
        }

        public void AddRequest(Contract contract, string genericTickList)
        {
            activeRequests.Add(contract);
            ibClient.ClientSocket.reqMktData(TICK_ID_BASE + (currentTicker++), contract, genericTickList, false, new List<TagValue>());

            if (!uiControl.Visible)
                uiControl.Visible = true;
        }

        public override void NotifyError(int requestId)
        {
            activeRequests.RemoveAt(GetIndex(requestId));
            currentTicker -= 1;
        }

        public override void Clear()
        {
            ((DataGridView)uiControl).Rows.Clear();
            activeRequests.Clear();
            uiControl.Visible = false;
            currentTicker = 1;
        }

        public void StopActiveRequests(bool clearTable)
        {
            for (int i = 1; i < currentTicker; i++)
            {
                ibClient.ClientSocket.cancelMktData(i + TICK_ID_BASE);
            }
            if (clearTable)
                Clear();
        }

        private void checkToAddRow(int requestId)
        {
            DataGridView grid = (DataGridView)uiControl;
            if (grid.Rows.Count < (requestId - TICK_ID_BASE))
            {
                grid.Rows.Add(GetIndex(requestId), 0);
                Contract contract = activeRequests[GetIndex(requestId)];
                grid[DESCRIPTION_INDEX, GetIndex(requestId)].Value = contract.LocalSymbol;
                //grid[DESCRIPTION_INDEX, GetIndex(requestId)].Value = Utils.ContractToString(activeRequests[GetIndex(requestId)]);
            }
        }

        private int GetIndex(int requestId)
        {
            return requestId - TICK_ID_BASE - 1;
        }

        public override void UpdateUI(IBMessage message)
        {
            IAppMDManager appMDMManager = appStrategyManager.getAppMDManager();
            IAppEventManager appEventManager = appStrategyManager.getAppEventManager();
            MarketDataMessage dataMessage = (MarketDataMessage)message;
            checkToAddRow(dataMessage.RequestId);
            DataGridView grid = (DataGridView)uiControl;
            if (grid.Rows.Count >= dataMessage.RequestId - TICK_ID_BASE)
            {
                if (message is TickPriceMessage)
                {
                    TickPriceMessage priceMessage = (TickPriceMessage)message;
                    switch (dataMessage.Field)
                    {
                        case 1:
                            {
                                //BID
                                appEventManager.putTickPriceEvents(priceMessage, dataMessage);
                                //stgManager.handleTickMessage(message);
                                grid[BID_PRICE_INDEX, GetIndex(dataMessage.RequestId)].Value = priceMessage.Price;
                                break;
                            }
                        case 2:
                            {
                                //ASK
                                appEventManager.putTickPriceEvents(priceMessage, dataMessage);
                                //stgManager.handleTickMessage(message);
                                grid[ASK_PRICE_INDEX, GetIndex(dataMessage.RequestId)].Value = priceMessage.Price;
                                break;
                            }
                        case 4:
                            {
                                //LAST
                                appEventManager.putTickPriceEvents(priceMessage, dataMessage);
                                //stgManager.handleTickMessage(message);
                                grid[LAST_PRICE_INDEX, GetIndex(dataMessage.RequestId)].Value = priceMessage.Price;
                                break;
                            }
                        case 6:
                            {
                                //HIGH
                                grid[HIGH_PRICE_INDEX, GetIndex(dataMessage.RequestId)].Value = priceMessage.Price;
                                break;
                            }
                        case 7:
                            {
                                //LOW
                                grid[LOW_PRICE_INDEX, GetIndex(dataMessage.RequestId)].Value = priceMessage.Price;
                                break;
                            }
                        case 9:
                            {
                                //CLOSE
                                grid[CLOSE_PRICE_INDEX, GetIndex(dataMessage.RequestId)].Value = priceMessage.Price;
                                break;
                            }
                    }
                }
                else if (dataMessage is TickSizeMessage)
                {
                    TickSizeMessage sizeMessage = (TickSizeMessage)message;
                    switch (dataMessage.Field)
                    {
                        case 0:
                            {
                                //BID SIZE
                                grid[BID_SIZE_INDEX, GetIndex(dataMessage.RequestId)].Value = sizeMessage.Size;
                                break;
                            }
                        case 3:
                            {
                                //ASK SIZE
                                grid[ASK_SIZE_INDEX, GetIndex(dataMessage.RequestId)].Value = sizeMessage.Size;
                                break;
                            }
                        case 5:
                            {
                                //LAST SIZE
                                
                                grid[LAST_SIZE_INDEX, GetIndex(dataMessage.RequestId)].Value = sizeMessage.Size;
                                break;
                            }
                        case 8:
                            {
                                //VOLUME
                                //stgManager.handleTickMessage(message);
                                grid[VOLUME_SIZE_INDEX, GetIndex(dataMessage.RequestId)].Value = sizeMessage.Size;
                                break;
                            }
                    }
                }
            }
        }

    }
}
