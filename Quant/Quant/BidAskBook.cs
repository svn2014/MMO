using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quant
{
    public class BidAskBook
    {
        public double bid1 = 0;
        public double ask1 = 0;
        public double bidsize1 = 0;
        public double asksize1 = 0;
        public double bidaskspread = 0;
        public double bidaskspreadpct = 0;

        public double last = 0;
        public double presettle = 0;
        public double open = 0;

        //DateTime time;
        public string code = "";
        private string[] fields;

        public void Update(string code, string[] fields, object matrix)
        {
            this.code = code;
            this.fields = fields;

            double[] data;
            data = (double[])matrix;

            for (int i = 0; i < data.Length; i++)
            {
                string field = fields[i];
                switch (field.ToLower())
                {
                    case "rt_pre_settle":
                        this.presettle = data[i];
                        break;
                    case "rt_open":
                        this.open = data[i];
                        break;
                    case "rt_last":
                        this.last = data[i];
                        break;
                    case "rt_ask1":
                        this.ask1 = data[i];;
                        break;
                    case "rt_bid1":
                        this.bid1 = data[i];
                        break;
                    case "rt_bsize1":
                        this.bidsize1 = data[i];
                        break;
                    case "rt_asize1":
                        this.asksize1 = data[i];
                        break;
                    default:
                        break;
                }
            }

            this.bidaskspread = ask1 - bid1;
            this.bidaskspreadpct = this.bidaskspread / this.bid1;
        }

        public void PrintMe()
        {
            Debug.Print(string.Format("last={0},presettle={1},open={2},ask1={3},asize1={4},bid1={5},bsize1={6},sprd={7},pct={8}", this.last, this.presettle, this.open, this.ask1, this.asksize1, this.bid1, this.bidsize1, this.bidaskspread, this.bidaskspreadpct));
        }
    }
}
