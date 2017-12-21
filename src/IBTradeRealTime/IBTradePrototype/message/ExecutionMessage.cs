using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;

namespace IBTradeRealTime.message
{
    public class ExecutionMessage : IBMessage
    {
        private int reqId;
        private Contract contract;
        private Execution execution;

        public ExecutionMessage(int reqId, Contract contract, Execution execution)
        {
            Type = MessageType.ExecutionData;
            ReqId = reqId;
            Contract = contract;
            Execution = execution;
        }

        public Contract Contract
        {
            get { return contract; }
            set { contract = value; }
        }

        public Execution Execution
        {
            get { return execution; }
            set { execution = value; }
        }

        public int ReqId
        {
            get { return reqId; }
            set { reqId = value; }
        }

    }
}
