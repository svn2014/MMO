using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quant
{
    public enum Exchange
    { 
        NONE,
        SH=1,
        SZ=2
    }

    public enum SecurityType
    {
        NONE,
        ETF,
        OPTION
    }

    public enum OptionType
    { 
        CALL,
        PUT
    }

    public enum TradeDirection
    {
        NONE=0,
        BUY=1,
        SELL=2
    }

    public enum FutureDirection
    {
        NONE=0,
        OPEN=1,
        COVER=2
    }
}
