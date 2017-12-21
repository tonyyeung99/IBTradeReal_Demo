using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.message
{
    public class ScannerEndMessage : IBMessage
    {
        private int requestId;

        public ScannerEndMessage(int requestId)
        {
            Type = MessageType.ScannerDataEnd;
            RequestId = requestId;
        }

        public int RequestId
        {
            get { return requestId; }
            set { requestId = value; }
        }
    }
}
