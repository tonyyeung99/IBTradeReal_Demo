﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTradeRealTime.message
{
    public class OpenOrderEndMessage : OrderMessage
    {
        public OpenOrderEndMessage()
        {
            Type = MessageType.OpenOrderEnd;
        }
    }
}
