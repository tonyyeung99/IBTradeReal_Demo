using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.message
{
    public class AdvisorDataMessage : IBMessage
    {
        private int faDataType;
        private string data;

        public AdvisorDataMessage(int faDataType, string data)
        {
            Type = MessageType.ReceiveFA;
            FaDataType = faDataType;
            Data = data;
        }

        public int FaDataType
        {
            get { return faDataType; }
            set { faDataType = value; }
        }

        public string Data
        {
            get { return data; }
            set { data = value; }
        }
    }
}
