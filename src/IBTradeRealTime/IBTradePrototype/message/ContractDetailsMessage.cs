using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;

namespace IBTradeRealTime.message
{
    public class ContractDetailsMessage : IBMessage
    {
        private int requestId;
        private ContractDetails contractDetails;

        public ContractDetailsMessage(int requestId, ContractDetails contractDetails)
        {
            Type = MessageType.ContractData;
            RequestId = requestId;
            ContractDetails = contractDetails;
        }

        public ContractDetails ContractDetails
        {
            get { return contractDetails; }
            set { contractDetails = value; }
        }

        public int RequestId
        {
            get { return requestId; }
            set { requestId = value; }
        }
    }
}
