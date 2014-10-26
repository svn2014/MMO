
using Quant.Entrust;
using Quant.Market;
using System.Collections.Generic;
using System.Diagnostics;
using WindQuantLibrary;
namespace Quant
{
    public abstract class ASecurity
    {
        public Exchange exchange = Exchange.NONE;
        public SecurityType type = SecurityType.NONE;

        public string windcode = "";
        public string tradecode = "";
        public string name = "";
        public string description = "";
        public BidAskBook bidaskbook = new BidAskBook();
        public List<OrderBook> entrustbook = new List<OrderBook>();
        public List<OrderBook> positionbook = new List<OrderBook>();

        protected MarketWind _wind = MarketWind.GetInstance();
        public EntrustHunsun _hunsun = EntrustHunsun.GetInstance();

        public ASecurity(string tradecode, Exchange exchange)
        {
            this.tradecode = tradecode;
            this.exchange = exchange;
            switch (exchange)
            {
                case Exchange.NONE:
                    break;
                case Exchange.SH:
                    this.windcode = this.tradecode + ".SH";
                    break;
                case Exchange.SZ:
                    this.windcode = this.tradecode + ".SZ";
                    break;
                default:
                    break;
            }
        }

        public abstract void UpdateBidAskBook();

        public void UpdateOrderBook()
        {
            EntrustHunsun.QueryPara param = new EntrustHunsun.QueryPara();
            //param.account_code
            //param.combi_no
            param.stock_code = this.tradecode;
            int iret = _hunsun.QueryEntrust(param);
            _hunsun.UpdateOrderBook(iret, this.entrustbook);
            iret = _hunsun.QueryPosition(param);
            _hunsun.UpdateOrderBook(iret, this.positionbook);
        }

        public void PrintMe()
        {
            Debug.Print(string.Format("Code={0}, Name={1}, Exchange={2}", this.tradecode, this.name, this.exchange));
            this.bidaskbook.PrintMe();
        }

        //Wind callback
        public int QuantCallbackRealtime(QuantEvent quantEvent)
        {
            //public class QuantEvent
            //{
            //  public eWQErr ErrCode; // 错误码
            //  public long EventID; // 流水号
            //  public eWQEventType EventType; // Event类型
            //  public QuantData quantData; // 包含的数据
            //  public long RequestID; // 对应的request ID
            //  public int Version; // 版本号，以备今后扩充
            //  public QuantEvent();
            //}

            //public class QuantData
            //{
            //    public DateTime[] ArrDateTime;
            //    public string[] ArrWindCode;
            //    public string[] ArrWindFields;
            //    public object MatrixData;
            //    public QuantData();
            //}

            if (quantEvent.ErrCode !=0)
            {
                Debug.Print("更新订单簿时出错：" + quantEvent.ErrCode);
                return -1;
            }

            if (quantEvent.quantData == null)
            {
                Debug.Print("订单簿没有数据：quantEvent.quantData == null");
                return -1;
            }

            this.bidaskbook.Update(quantEvent.quantData.ArrWindCode[0], quantEvent.quantData.ArrWindFields, quantEvent.quantData.MatrixData);
            
            return 0;
        }
    }
}
