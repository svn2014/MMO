using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quant
{
    public class OrderBook
    {
        //委托，持仓
        public int entrustno = 0;
        public string tradecode;
        public TradeDirection tradedirection = TradeDirection.NONE;
        public FutureDirection futuredirection = FutureDirection.NONE;
        public double price = 0;
        public int volume = 0;
    }
}
