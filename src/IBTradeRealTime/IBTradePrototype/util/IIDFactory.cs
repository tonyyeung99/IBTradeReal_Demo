using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.util
{
    public interface IIDFactory
    {
        int getNext();
        void init();
    }
}
