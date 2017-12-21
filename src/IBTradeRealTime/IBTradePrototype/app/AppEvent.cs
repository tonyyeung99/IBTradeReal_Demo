using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.app
{
    public abstract class AppEvent
    {
        protected AppEventType type;

        public AppEventType Type
        {
            get { return type; }
            set { type = value; }
        }
    }
}
