using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.message
{
    public class AccountSummaryEndMessage : IBMessage
    {
        private int requestId;

        public AccountSummaryEndMessage(int requestId)
        {
            Type = MessageType.AccountSummaryEnd;
            RequestId = requestId;
        }

        public int RequestId
        {
            get { return requestId; }
            set { requestId = value; }
        }
    }
}
