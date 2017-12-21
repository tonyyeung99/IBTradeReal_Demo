using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.message
{
    class AccountDownloadEndMessage : IBMessage
    {
        private string account;

        public AccountDownloadEndMessage(string account)
        {
            Type = MessageType.AccountDownloadEnd;
            Account = account;
        }

        public string Account
        {
            get { return account; }
            set { account = value; }
        }
    }
}
