using System;
using System.Collections;
using WindQuantLibrary;

namespace Quant.Market
{
    public class MarketWind
    {
        //Static
        private static MarketWind _Instance = null;
        public static MarketWind GetInstance()
        {
            if (_Instance == null)
            {
                _Instance = new MarketWind();
            }

            return _Instance;
        }

        //Instance
        private WindQuantAPI _windapi = null;
        public MarketWind()
        {
            this._windapi = new WindQuantAPI(QuantCallback);
            this._windapi.Authorize("", "", true);
        }

        public void GetOptionSet(string windcode, WindQuantCallback callback)
        {
            try
            {
                if (windcode.IndexOf(".")==-1)
                {
                    windcode += ".SH";
                }

                long reqid = this._windapi.WSET("OptionChain", "date=" + DateTime.Today.ToString("yyyyMMdd") + ";us_code=" + windcode + ";option_var=;month=全部;call_put=全部", callback);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetBidAskBook(ASecurity s)
        {
            try
            {
                //注意：snaponly参数，false=持续更新
                long reqid = this._windapi.WSQ(s.windcode, "rt_time,rt_pre_settle,rt_open,rt_last,rt_ask1,rt_bid1,rt_bsize1,rt_asize1", false, s.QuantCallbackRealtime);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private int QuantCallback(QuantEvent quantEvent)
        {
            //switch (quantEvent.EventID)
            //{
            //    case 0: //initial
            //    case 1: //login
            //        break;
            //    case 2:
            //        break;
            //    default:
            //        break;
            //}
            //string s = System.String.Format("Received event: type={0}, err={1}, reqid={2}, evtid={3}.",
            //quantEvent.EventType, quantEvent.ErrCode, quantEvent.RequestID, quantEvent.EventID);

            //s += Environment.NewLine;

            //QuantData data = quantEvent.quantData;
            //if (data != null)
            //{
            //    int codenum = data.ArrWindCode.Length;
            //    int indnum = data.ArrWindFields.Length;
            //    int timenum = data.ArrDateTime.Length;
            //    s += "Windcode = " + Environment.NewLine;
            //    foreach (String code in data.ArrWindCode)
            //        s += "  " + code + Environment.NewLine;
            //    s += "Indicators = " + Environment.NewLine;
            //    foreach (String field in data.ArrWindFields)
            //        s += "  " + field + Environment.NewLine;
            //    s += "Times = " + Environment.NewLine;
            //    foreach (DateTime time in data.ArrDateTime)
            //        s += "  " + time.ToString() + Environment.NewLine;

                //s += "Data = " + Environment.NewLine;
                //s += MatrixDataToString(data.MatrixData, codenum, indnum, timenum);
                //s += Environment.NewLine;
            //}
            return 0;
        }
    }
}
