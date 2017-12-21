using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.message
{
    public class UpdateAccountTimeMessage : IBMessage
    {
        private string timestamp;

        public UpdateAccountTimeMessage(string timestamp)
        {
            Type = MessageType.AccountUpdateTime;
            Timestamp = timestamp;
        }

        public string Timestamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }
    }
}
