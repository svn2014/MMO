using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Quant.Entrust;
using System;

namespace Quant.Strategy
{
    public class StrategyMM1
    {
        /// <summary>
        /// 参数
        /// </summary>
        private double C_SPRD_PCT = 0.01;   //报价在中心价上下各1%
        private double C_MIN_SPRD = 0.001;  //ask-bid最小值
        private int C_MIN_VOL = 5;          //最小下单数量
        private string C_HUNDSUN_USER = "0730";
        private string C_HUNDSUN_PWD = "0";
        private string C_HUNDSUN_ACC_NO = "1101";
        private string C_HUNDSUN_COMBI_NO = "1101_001";

        public string name = "";
        public string description = "";
        private bool flgStop = false;

        public StrategyMM1()
        {
            name = "做市商策略1";
            description = "在最后成交价上下固定区间报价";
            EntrustHunsun.GetInstance().Subcribe(C_HUNDSUN_USER, C_HUNDSUN_PWD);
            EntrustHunsun.GetInstance().Logon(C_HUNDSUN_USER, C_HUNDSUN_PWD, C_HUNDSUN_ACC_NO, C_HUNDSUN_COMBI_NO);
        }

        private List<Option> optionlist = new List<Option>();
        public void Add(string underlyingcode)
        {
            Debug.Print("正在读取{0}对应的期权",underlyingcode);
            Option.GetOptions(underlyingcode);
            Thread.Sleep(5000);
            if (Option.htOptionSets.Contains(underlyingcode+".SH"))
            {
                List<Option> optionlist = (List<Option>)Option.htOptionSets[underlyingcode + ".SH"];
                foreach (Option o in optionlist)
                {
                    this.Add(o);
                }
            }
        }
        public void Add(Option o)
        {
            int idx = optionlist.FindIndex(delegate(Option op) { return op.tradecode == o.tradecode; });
            if (idx < 0)
                optionlist.Add(o);
            else
                Debug.Print(string.Format("期权{0}已加载",o.windcode));
        }

        public void Run()
        {
            Debug.Print("Run");
            flgStop = false;
            if (optionlist.Count==0)
            {
                Debug.Print("未加入期权标的");
                return;
            }

            Debug.Print(string.Format("准备做市，共加入{0}个期权", optionlist.Count));
            foreach (Option o in optionlist)
            {
                o.UpdateBidAskBook();
            }

            for (int i = 0; i < 3; i++)
            {
                Debug.Print(string.Format("正在准备...{0}", 3 - i));
                Thread.Sleep(1000);
            }

            Debug.Print("开始做市");
            long iCnt=0;
            while (!this.flgStop)
            {
                foreach (Option o in optionlist)
                {
                    this.makemarket(o);
                }

                Thread.Sleep(100);
                Debug.Print(iCnt++.ToString());
                if (iCnt > 10000)
                    iCnt = 0;
            }
        }

        public void Stop()
        {
            this.flgStop = true;
            Debug.Print("做市已停止");
        }

        private void makemarket(Option o)
        {
            double centerpx = o.bidaskbook.last;
            if (centerpx == 0)
                centerpx = o.bidaskbook.open;
            if (centerpx == 0)
                centerpx = o.bidaskbook.presettle;

            if (centerpx == o.MMCenterPrice)
            {
                //没有新成交
                return;
            }

            //成交有变化
            o.MMCenterPrice = centerpx;
            double ask1 = centerpx * (1 + C_SPRD_PCT);
            double bid1 = centerpx * (1 - C_SPRD_PCT);
            if (ask1 - bid1 < C_MIN_SPRD)
                bid1 = ask1 - C_MIN_SPRD;

            o.UpdateOrderBook();
            this.sendorder(o, TradeDirection.SELL, ask1, C_MIN_VOL);    //TODO，下单量建议使用固定金额法
            this.sendorder(o, TradeDirection.BUY, bid1, C_MIN_VOL);
        }

        private void sendorder(Option o, TradeDirection tradedirection, double price, int volume)
        {
            //查询同向委托
            List<OrderBook> entrustlist = o.entrustbook.FindAll(delegate(OrderBook b) { return b.tradecode == o.tradecode && b.tradedirection == tradedirection; });

            //有同向委托
            bool flg = false;
            if (entrustlist != null && entrustlist.Count > 0)
            {
                //检查若委托价量相同，则跳过
                foreach (OrderBook b in entrustlist)
                {
                    if (Math.Abs(b.price-price)<0.0001 && b.volume == volume)
                    {
                        //委托价量相同
                        Debug.Print(string.Format("委托已经存在:c={0},t={1}.p={2},v={3}", o.tradecode, tradedirection.ToString(), price, volume));
                        flg = true;
                        continue;
                    }
                    else
                    {
                        //委托价量不同，则撤单
                        o._hunsun.Withdraw(b.entrustno);
                    }
                }

                //相同价量委托已存在，跳过
                if (flg)
                    return;
            }

            //无同向委托，或者已被撤单，查反向持仓
            OrderBook revposition = null;
            if (tradedirection == TradeDirection.BUY)
            {
                //欲委托买入，先查询当前空头持仓，优先买入平仓
                revposition = o.positionbook.Find(delegate(OrderBook b) { return b.tradecode == o.tradecode && b.tradedirection == TradeDirection.SELL; });
            }
            else
            {
                //欲委托卖出，先查询当前多头持仓，优先卖出平仓
                revposition = o.positionbook.Find(delegate(OrderBook b) { return b.tradecode == o.tradecode && b.tradedirection == TradeDirection.BUY; });
            }

            EntrustHunsun.EntrustPara para = new EntrustHunsun.EntrustPara();
            para.marketno = ((int)Exchange.SH).ToString();
            para.stockcode = o.tradecode;
            para.volume = volume;
            para.price = price;

            if (revposition == null)
            {
                //无反向持仓,开仓
                para.entrustdirection = ((int)tradedirection).ToString();
                para.futuredirection = ((int)FutureDirection.OPEN).ToString();                
                o._hunsun.Entrust(para);
                Debug.Print(string.Format("开仓：T={0},c={1},p={2},v={3}", tradedirection.ToString(), o.tradecode, price, volume));
            }
            else
            {
                //有反向持仓
                if (revposition.volume >= volume)
                {
                    //持有足够反向头寸，平仓
                    para.entrustdirection = ((int)tradedirection).ToString();
                    para.futuredirection = ((int)FutureDirection.COVER).ToString();
                    o._hunsun.Entrust(para);
                    Debug.Print(string.Format("平仓：T={0},c={1},p={2},v={3}", tradedirection.ToString(), o.tradecode, price, volume));
                }
                else
                {
                    //持有反向头寸不足，先平后开
                    para.entrustdirection = ((int)tradedirection).ToString();
                    para.futuredirection = ((int)FutureDirection.COVER).ToString();
                    para.volume = revposition.volume;
                    o._hunsun.Entrust(para);
                    Debug.Print(string.Format("平仓：T={0},c={1},p={2},v={3}", tradedirection.ToString(), o.tradecode, price, revposition.volume));

                    para.entrustdirection = ((int)tradedirection).ToString();
                    para.futuredirection = ((int)FutureDirection.OPEN).ToString();
                    para.volume = volume - revposition.volume;
                    o._hunsun.Entrust(para);
                    Debug.Print(string.Format("开仓：T={0},c={1},p={2},v={3}", tradedirection.ToString(), o.tradecode, price, volume - revposition.volume));
                }
            }
        }
    }
}
