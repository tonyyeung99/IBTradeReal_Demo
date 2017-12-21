using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.util
{
    public class InMemoryNumIDFactory : IIDFactory
    {
        int mNextID = 0;

        public void init()
        {
        }

        public InMemoryNumIDFactory(int startAt)
        {
            mNextID = startAt;
        }

        public int getNext()
        {
            int retVal = 0;
            lock (this)
            {
                retVal = mNextID++;
            }

            return  retVal;
        }

    }
}
