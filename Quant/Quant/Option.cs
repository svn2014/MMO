
using Quant.Market;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using WindQuantLibrary;

namespace Quant
{
    public class Option:ASecurity
    {
        #region Static
        public static Hashtable htOptionSets = new Hashtable();
        public static void GetOptions(string underlyingcode)
        {
            if (htOptionSets.ContainsKey(underlyingcode))
                return;

            MarketWind.GetInstance().GetOptionSet(underlyingcode, QuantCallbackDataSet);
        }
        private static int QuantCallbackDataSet(QuantEvent quantEvent)
        {
            if (quantEvent.ErrCode != 0)
            {
                Debug.Print("获取期权列表时出错：" + quantEvent.ErrCode);
                return -1;
            }

            if (quantEvent.quantData == null)
            {
                Debug.Print("期权列表没有数据：quantEvent.quantData == null");
                return -1;
            }

            object[] data;
            data = (object[])quantEvent.quantData.MatrixData;

            List<Option> list = new List<Option>();
            for (int i = 0; i < data.Length / 13; i++)
            {
                string code = data[3+i*13].ToString();
                if (code.Contains("."))
                    code = code.Substring(0,code.IndexOf("."));   //转换Wind代码和交易代码

                Option o = new Option(code, Exchange.SH);
                o.underlying = data[0 + i * 13].ToString();
                o.name = data[4 + i * 13].ToString();
                list.Add(o);
            }

            if (list.Count > 0 && !htOptionSets.Contains(list[0].underlying))
                htOptionSets.Add(list[0].underlying, list);

            return 0;
        }
        #endregion

        //Instance
        public double MMCenterPrice = 0;
        public string underlying = "";
        public Option(string code, Exchange exchange)
            : base(code, exchange)
        { 
            //Option parameters
        }

        public void GetInfo(string code)
        {
            foreach (DictionaryEntry de in htOptionSets)
            {
                List<Option> list = (List<Option>)de.Value;
                Option op = list.Find(delegate(Option o) { return o.tradecode == code; });
                this.name = op.name;
                this.underlying = op.underlying;
            }
        }

        public override void UpdateBidAskBook()
        {
            base._wind.GetBidAskBook(this);
        }
    }
}
