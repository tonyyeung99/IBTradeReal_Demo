using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using IBTradeRealTime.message;
using IBTradeRealTime.backend;
using IBTradeRealTime.util;
using IBApi;
using IBTradeRealTime.app;


namespace IBTradeRealTime.UI
{
    class AccountDetailManager
    {
        private const int ACCOUNT_ID_BASE = 50000000;

        private const int ACCOUNT_SUMMARY_ID = ACCOUNT_ID_BASE + 1;

        private const string ACCOUNT_SUMMARY_TAGS = "AccountType,NetLiquidation,TotalCashValue,SettledCash,AccruedCash,BuyingPower,EquityWithLoanValue,PreviousEquityWithLoanValue,"
             +"GrossPositionValue,ReqTEquity,ReqTMargin,SMA,InitMarginReq,MaintMarginReq,AvailableFunds,ExcessLiquidity,Cushion,FullInitMarginReq,FullMaintMarginReq,FullAvailableFunds,"
             +"FullExcessLiquidity,LookAheadNextChange,LookAheadInitMarginReq ,LookAheadMaintMarginReq,LookAheadAvailableFunds,LookAheadExcessLiquidity,HighestSeverity,DayTradesRemaining,Leverage";

        private const string ACCOUNT_VALUE_TAGS = "AccountCode,AvailableFunds,Currency,BuyingPower,CashBalance,RealizedPnL,UnrealizedPnL,EquityWithLoanValue,ExcessLiquidity,"
             +"InitMarginReq,MaintMarginReq,FuturesPNL,GrossPositionValue,Leverage-S,ExchangeRate";
        private IBClient ibClient;
        private List<string> managedAccounts;
        private ComboBox accountSelector;
        private DataGridView accountSummaryGrid;
        private DataGridView accountValueGrid;
        private DataGridView accountPortfolioGrid;
        private DataGridView positionsGrid;

        private IBTradeApp appForm;

        private bool accountSummaryRequestActive = false;
        private bool accountUpdateRequestActive = false;
        private string currentAccountSubscribedToTupdate;

        private Dictionary<String, String> valid_account_values;

        private IAppStrategyManager appStrategyManager;

        public AccountDetailManager(IBClient ibClient, DataGridView accountValueGrid, DataGridView accountPortfolioGrid, IBTradeApp appForm)
        {
            IbClient = ibClient;
            AccountValueGrid = accountValueGrid;
            this.accountPortfolioGrid = accountPortfolioGrid;
            this.appForm = appForm;
            String[] tags = ACCOUNT_VALUE_TAGS.Split(',');
            valid_account_values = new Dictionary<string, string>();
            foreach (String tag in tags)
            {
                valid_account_values.Add(tag, tag);
            }
            appStrategyManager = appForm.appStrategyManager;
        }

        private Boolean isInValidAccountValueTag(String key)
        {
            if (valid_account_values.ContainsKey(key.Trim()))
            {
                return true;
            }
            return false;            
        }

        public void UpdateUI(IBMessage message)
        {
            switch (message.Type)
            {
                case MessageType.AccountSummary:
                    HandleAccountSummary((AccountSummaryMessage)message);
                    break;
                case MessageType.AccountSummaryEnd:
                    HandleAccountSummaryEnd();
                    break;
                case MessageType.AccountValue:
                    HandleAccountValue((AccountValueMessage)message);
                    break;
                case MessageType.PortfolioValue:
                    HandlePortfolioValue((UpdatePortfolioMessage)message);
                    break;
                case MessageType.AccountDownloadEnd:
                    break;
                case MessageType.Position:
                    HandlePosition((PositionMessage)message);
                    break;
                case MessageType.PositionEnd:
                    break;
            }
        }

        private void HandleAccountSummaryEnd()
        {
            accountSummaryRequestActive = false;
        }

        private void  HandleAccountSummary(AccountSummaryMessage summaryMessage)
        {
            for (int i = 0; i < accountSummaryGrid.Rows.Count; i++)
            {
                if (accountSummaryGrid[0, i].Value.Equals(summaryMessage.Tag) && accountSummaryGrid[3, i].Value.Equals(summaryMessage.Account))
                {
                    accountSummaryGrid[1, i].Value = summaryMessage.Value;
                    accountSummaryGrid[2, i].Value = summaryMessage.Currency;
                    return;
                }
            }
            accountSummaryGrid.Rows.Add(1);
            accountSummaryGrid[0, accountSummaryGrid.Rows.Count-1].Value = summaryMessage.Tag;
            accountSummaryGrid[1, accountSummaryGrid.Rows.Count - 1].Value = summaryMessage.Value;
            accountSummaryGrid[2, accountSummaryGrid.Rows.Count - 1].Value = summaryMessage.Currency;
            accountSummaryGrid[3, accountSummaryGrid.Rows.Count - 1].Value = summaryMessage.Account;
        }

        private void HandleAccountValue(AccountValueMessage accountValueMessage)
        {
            for (int i = 0; i < accountValueGrid.Rows.Count; i++)
            {
                if (accountValueGrid[0, i].Value == null)
                    continue;
                if (accountValueGrid[0, i].Value.Equals(accountValueMessage.Key))
                {
                    accountValueGrid[1, i].Value = String.Format("{0:#,###,###.##}", accountValueMessage.Value) + " " + accountValueMessage.Currency; 
                    //accountValueGrid[2, i].Value = accountValueMessage.Currency;
                    return;
                }
            }
            if(isInValidAccountValueTag(accountValueMessage.Key)){
                accountValueGrid.Rows.Add(1);
                accountValueGrid[0, accountValueGrid.Rows.Count - 1].Value = accountValueMessage.Key;
                accountValueGrid[1, accountValueGrid.Rows.Count - 1].Value = String.Format("{0:#,###,###.##}", accountValueMessage.Value) + " " + accountValueMessage.Currency; 
            }
            if ("AccountCode".Equals(accountValueMessage.Key))
            {
                appStrategyManager.UserAccount = accountValueMessage.Value;
            }
        }

        private void HandlePortfolioValue(UpdatePortfolioMessage updatePortfolioMessage)
        {            
            for (int i = 0; i < accountPortfolioGrid.Rows.Count; i++)
            {
                if (accountPortfolioGrid[0, i].Value.Equals(updatePortfolioMessage.Contract.LocalSymbol))
                {
                    accountPortfolioGrid[1, i].Value = updatePortfolioMessage.Position;
                    accountPortfolioGrid[2, i].Value = updatePortfolioMessage.UnrealisedPNL;
                    accountPortfolioGrid[3, i].Value = updatePortfolioMessage.RealisedPNL;
                    return;
                }
            }            
            accountPortfolioGrid.Rows.Add(1);
            accountPortfolioGrid[0, accountPortfolioGrid.Rows.Count - 1].Value = updatePortfolioMessage.Contract.LocalSymbol;
            accountPortfolioGrid[1, accountPortfolioGrid.Rows.Count - 1].Value = updatePortfolioMessage.Position;
            accountPortfolioGrid[2, accountPortfolioGrid.Rows.Count - 1].Value = updatePortfolioMessage.UnrealisedPNL;
            accountPortfolioGrid[3, accountPortfolioGrid.Rows.Count - 1].Value = updatePortfolioMessage.RealisedPNL;
        }

        public void HandlePosition(PositionMessage positionMessage)
        {
            for (int i = 0; i < positionsGrid.Rows.Count; i++)
            {
                if (positionsGrid[0, i].Value.Equals(Utils.ContractToString(positionMessage.Contract)))
                {
                    positionsGrid[1, i].Value = positionMessage.Account;
                    positionsGrid[2, i].Value = positionMessage.Position;
                    positionsGrid[3, i].Value = positionMessage.AverageCost;
                    return;
                }
            }
            positionsGrid.Rows.Add(1);
            positionsGrid[0, positionsGrid.Rows.Count - 1].Value = Utils.ContractToString(positionMessage.Contract);
            positionsGrid[1, positionsGrid.Rows.Count - 1].Value = positionMessage.Account;
            positionsGrid[2, positionsGrid.Rows.Count - 1].Value = positionMessage.Position;
            positionsGrid[3, positionsGrid.Rows.Count - 1].Value = positionMessage.AverageCost;
        }

        public void RequestAccountSummary()
        {
            if (!accountSummaryRequestActive)
            {
                accountSummaryRequestActive = true; 
                ibClient.ClientSocket.reqAccountSummary(ACCOUNT_SUMMARY_ID, "All", ACCOUNT_SUMMARY_TAGS);
            }
            else
            {
                ibClient.ClientSocket.cancelAccountSummary(ACCOUNT_SUMMARY_ID);
            }
        }

        public void SubscribeAccountUpdates()
        {
            if (!accountUpdateRequestActive)
            {
                accountUpdateRequestActive = true;
                accountValueGrid.Rows.Clear();
                ibClient.ClientSocket.reqAccountUpdates(true, currentAccountSubscribedToTupdate);
            }
            else
            {
                ibClient.ClientSocket.reqAccountUpdates(false, currentAccountSubscribedToTupdate);
                currentAccountSubscribedToTupdate = null;
                accountUpdateRequestActive = false;
            }
        }

        public void RequestPositions()
        {
            ibClient.ClientSocket.reqPositions();
        }
        
        public List<string> ManagedAccounts
        {
            get { return managedAccounts; }
            set 
            { 
                managedAccounts = value; 
                SetManagedAccounts(value);
            }
        }

        public void SetManagedAccounts(List<string> managedAccounts)
        {
            if(managedAccounts.Count>0)
                currentAccountSubscribedToTupdate = managedAccounts[0];
        }

        public ComboBox AccountSelector
        {
            get { return accountSelector; }
            set { accountSelector = value; }
        }

        public DataGridView AccountSummaryGrid
        {
            get { return accountSummaryGrid; }
            set { accountSummaryGrid = value; }
        }

        public DataGridView AccountValueGrid
        {
            get { return accountValueGrid; }
            set { accountValueGrid = value; }
        }

        public DataGridView AccountPortfolioGrid
        {
            get { return accountPortfolioGrid; }
            set { accountPortfolioGrid = value; }
        }

        public DataGridView PositionsGrid
        {
            get { return positionsGrid; }
            set { positionsGrid = value; }
        }

        public IBClient IbClient
        {
            get { return ibClient; }
            set { ibClient = value; }
        }
    }
}
