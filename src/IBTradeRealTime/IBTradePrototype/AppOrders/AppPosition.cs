using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.AppOrders
{
    public class AppPosition
    {
        public String StratgeyShortName { get; set; }

        private int accBuyQ;
        private int accSellQ;
        private int netQ;
        private double accBuyMoney;
        private double accSellMoney;
        private double totalPnL;

        public int AccBuyQ
        {
            get { return accBuyQ; }
            set { accBuyQ = value; }
        }

        public int AccSellQ
        {
            get { return accSellQ; }
            set { accSellQ = value; }
        }

        public int NetQ
        {
            get { return netQ; }
            set { netQ = value; }
        }

        public double AccBuyMoney
        {
            get { return accBuyMoney; }
            set { accBuyMoney = value; }
        }

        public double AccSellMoney
        {
            get { return accSellMoney; }
            set { accSellMoney = value; }
        }

        public double TotalPnL
        {
            get { return totalPnL; }
            set { totalPnL = value; }
        }
    }
}
