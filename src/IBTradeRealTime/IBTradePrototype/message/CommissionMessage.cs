using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;

namespace IBTradeRealTime.message
{
    public class CommissionMessage : IBMessage
    {
        private CommissionReport commissionReport;

        public CommissionMessage(CommissionReport commissionReport)
        {
            Type = MessageType.CommissionsReport;
            CommissionReport = commissionReport;
        }

        public CommissionReport CommissionReport
        {
            get { return commissionReport; }
            set { commissionReport = value; }
        }
    }
}
