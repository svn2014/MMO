
using hundsun.mcapi;
using hundsun.t2sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
namespace Quant.Entrust
{
    public unsafe class EntrustHunsun : CT2CallbackInterface
    {
        //结构
        public class EntrustPara
        {
            public string combino;
            public string marketno;
            public string stockcode;
            public double volume;
            public double price;

            public string entrustdirection;
            public string futuredirection;
        }
        public class QueryPara
        {
            public string account_code;
            public string combi_no;
            public string stock_code;
        }

        //枚举
        public enum OptionFunctionCode
        {
            Logon = 10001,          //登录
            Entrust = 91005,        //期权委托下单
            Withdraw = 91106,       //期权委托撤单
            QueryEntrust = 32004,     //期权委托查询
            QueryPosition = 31004   //期权持仓查询
        }

        //静态
        private static EntrustHunsun _Instance = null;
        public static EntrustHunsun GetInstance()
        {
            if (_Instance == null)
            {
                _Instance = new EntrustHunsun();
            }
            return _Instance;
        }

        #region 委托

        public void Subcribe(string user, string pwd)
        {
            subcallback = new UFXSubCallback(this);
            subcribe = this.connMain.NewSubscriber(subcallback, "ufx_demo", 50000, 2000, 100);
            if (subcribe == null)
            {
                Debug.Print(string.Format("订阅创建失败 {0}", connMain.GetMCLastError()));
                return;
            }
            CT2SubscribeParamInterface args = new CT2SubscribeParamInterface();
            args.SetTopicName("ufx_topic");
            args.SetReplace(false);
            args.SetFilter("operator_no", user);

            CT2Packer req = new CT2Packer(2);
            req.BeginPack();
            req.AddField("login_operator_no", Convert.ToSByte('S'), 16, 4);
            req.AddField("password", Convert.ToSByte('S'), 16, 4);

            req.AddStr(user);
            req.AddStr(pwd);

            req.EndPack();

            CT2UnPacker unpacker = null;
            int ret = subcribe.SubscribeTopicEx(args, 50000, out unpacker, req);
            req.Dispose();
            if (ret > 0)
            {
                Debug.Print("订阅成功");
                subcribeid = ret;
            }
            else
            {
                if (unpacker != null)
                {
                    Debug.Print("订阅失败");
                    this.ShowUnPacker(unpacker);
                }
            }
        }
        public void Logon(string user, string pwd, string accno, string combino)
        {
            this.curraccno = accno;
            this.currcombino = combino;

            CT2Packer packer = new CT2Packer(2);
            packer.BeginPack();

            //字段名
            packer.AddField("operator_no", Convert.ToSByte('S'), 16, 4);
            packer.AddField("password", Convert.ToSByte('S'), 16, 4);
            packer.AddField("mac_address", Convert.ToSByte('S'), 32, 4);
            packer.AddField("ip_address", Convert.ToSByte('S'), 32, 4);
            packer.AddField("hd_volserial", Convert.ToSByte('S'), 10, 4);
            packer.AddField("op_station", Convert.ToSByte('S'), 255, 4);
            packer.AddField("authorization_id", Convert.ToSByte('S'), 64, 4);
            packer.AddField("login_time", Convert.ToSByte('S'), 6, 4);
            packer.AddField("verification_code", Convert.ToSByte('S'), 32, 4);

            //参数值
            packer.AddStr(user);
            packer.AddStr(pwd);
            packer.AddStr("mac");
            packer.AddStr("ip");
            packer.AddStr("vol");
            packer.AddStr("op_station");
            packer.AddStr("");
            packer.AddStr("");
            packer.AddStr("");

            packer.EndPack();

            this.sendpacker(OptionFunctionCode.Logon, packer);
        }
        public void Entrust(EntrustPara param)
        {
            Debug.Print(string.Format("正在下单:c={0},p={1},v={2}", param.stockcode, param.price, param.volume));
            CT2Packer packer = new CT2Packer(2);
            packer.BeginPack();
            packer.AddField("user_token", Convert.ToSByte('S'), 512, 4);
            packer.AddField("combi_no", Convert.ToSByte('S'), 8, 4);
            packer.AddField("market_no", Convert.ToSByte('S'), 3, 4);
            packer.AddField("stock_code", Convert.ToSByte('S'), 16, 4);
            packer.AddField("entrust_direction", Convert.ToSByte('S'), 3, 4);
            packer.AddField("futures_direction", Convert.ToSByte('S'), 1, 4);
            packer.AddField("price_type", Convert.ToSByte('S'), 1, 4);
            packer.AddField("entrust_price", Convert.ToSByte('F'), 11, 4);
            packer.AddField("entrust_amount", Convert.ToSByte('F'), 16, 2);
            packer.AddField("covered_flag", Convert.ToSByte('S'), 1, 2);

            packer.AddStr(this.token);
            packer.AddStr(this.currcombino);
            packer.AddStr(param.marketno);
            packer.AddStr(param.stockcode);
            packer.AddStr(param.entrustdirection);
            packer.AddStr(param.futuredirection); //futrue direction  '1'-开仓; '2'-平仓。
            packer.AddStr("0");                   //0=限价
            packer.AddDouble(param.price);
            packer.AddDouble(param.volume);
            packer.AddStr("0");                   //covered_flag，备兑标志，0=非备兑

            packer.EndPack();

            this.sendpacker(OptionFunctionCode.Entrust, packer);
        }
        public int QueryEntrust(QueryPara param)
        {
            CT2Packer packer = new CT2Packer(2);
            packer.BeginPack();
            packer.AddField("user_token", Convert.ToSByte('S'), 512, 4);
            packer.AddField("account_code", Convert.ToSByte('S'), 32, 4);
            packer.AddField("combi_no", Convert.ToSByte('S'), 8, 4);
            packer.AddField("stock_code", Convert.ToSByte('S'), 16, 4);

            packer.AddStr(this.token);
            packer.AddStr(this.curraccno);
            packer.AddStr(this.currcombino);
            packer.AddStr(param.stock_code);

            packer.EndPack();

            return this.sendpacker(OptionFunctionCode.QueryEntrust, packer,false);
        }
        public int QueryPosition(QueryPara param)
        {
            CT2Packer packer = new CT2Packer(2);
            packer.BeginPack();
            packer.AddField("user_token", Convert.ToSByte('S'), 512, 4);
            packer.AddField("account_code", Convert.ToSByte('S'), 32, 4);
            packer.AddField("combi_no", Convert.ToSByte('S'), 8, 4);
            packer.AddField("stock_code", Convert.ToSByte('S'), 16, 4);

            packer.AddStr(this.token);
            packer.AddStr(this.curraccno);
            packer.AddStr(this.currcombino);
            packer.AddStr(param.stock_code);

            packer.EndPack();

            return this.sendpacker(OptionFunctionCode.QueryPosition, packer, false);
        }
        public void UpdateOrderBook(int iRet, List<OrderBook> orderbooklist)
        {
            //清除旧记录
            orderbooklist.Clear();

            //读取新记录
            CT2BizMessage lpMsg; //外部所指向的消息对象的内存由SDK内部管理，外部切勿释放
            this.connMain.RecvBizMsg(iRet, out lpMsg, 5000, 1);

            int iRetCode = lpMsg.GetErrorNo();      //获取返回码
            int iErrorCode = lpMsg.GetReturnCode(); //获取错误码
            int iFunction = lpMsg.GetFunction();
            if (iRetCode != 0)
            {
                Debug.Print("异步接收出错：" + lpMsg.GetErrorNo().ToString() + lpMsg.GetErrorInfo());
            }
            else
            {
                //读packer
                CT2UnPacker unpacker = null;
                unsafe
                {
                    int iLen = 0;
                    void* lpdata = lpMsg.GetContent(&iLen);
                    unpacker = new CT2UnPacker(lpdata, (uint)iLen);
                }

                //解析
                int count = unpacker.GetDatasetCount();
                for (int k = 1; k < count; k++)
                {
                    unpacker.SetCurrentDatasetByIndex(k);
                    while (unpacker.IsEOF() == 0)
                    {
                        OrderBook ob = new OrderBook();
                        switch (iFunction)
                        {
                            case 31004: //持仓查询
                                ob.tradecode = unpacker.GetStr("stock_code");
                                ob.volume = unpacker.GetInt("enable_amount");
                                string positionflag = unpacker.GetStr("position_flag");
                                ob.tradedirection = (positionflag == "1") ? TradeDirection.BUY : TradeDirection.SELL;
                                if (ob.volume > 0)
                                    orderbooklist.Add(ob);
                                break;
                            case 32004: //委托查询
                                ob.entrustno = unpacker.GetInt("entrust_no");
                                ob.tradecode = unpacker.GetStr("stock_code");
                                string entrustdirection = unpacker.GetStr("entrust_direction"); //1=买，2=卖，3=...，4=...；
                                string futuresdirection = unpacker.GetStr("futures_direction"); //1=开，2=平；
                                string entruststate = unpacker.GetStr("entrust_state");                //委托状态
                                ob.tradedirection = (entrustdirection == "1") ? TradeDirection.BUY : TradeDirection.SELL;
                                ob.futuredirection = (futuresdirection == "1") ? FutureDirection.OPEN : FutureDirection.COVER;
                                ob.price = unpacker.GetDouble("entrust_price");
                                int entrustvol = unpacker.GetInt("entrust_amount");
                                int dealvol = unpacker.GetInt("deal_amount");
                                ob.volume = entrustvol - dealvol;
                                switch (entruststate)
	                            {
                                    case "1":    //未报
                                    case "4":    //已报
                                    case "6":    //部成
                                        orderbooklist.Add(ob);
                                        break;
                                    case "5":    //废单                                    
                                    case "7":    //已成
                                    case "8":    //部撤
                                    case "9":    //已撤
                                    case "a":    //待撤
                                    case "A":    //未撤
                                    case "B":    //待撤
                                    case "C":    //正撤
                                    case "D":    //撤认
                                    case "E":    //撤废
                                    case "F":    //已撤
                                        break;
		                            default:
                                        break;
	                            }
                                break;
                            default:
                                break;
                        }
                        unpacker.Next();
                    }
                }
            }
        }
        public void Withdraw(int enrtustno)
        {
            Debug.Print(string.Format("正在撤单:no={0}", enrtustno));
            CT2Packer packer = new CT2Packer(2);
            packer.BeginPack();
            packer.AddField("user_token", Convert.ToSByte('S'), 512, 4);
            packer.AddField("entrust_no", Convert.ToSByte('I'), 8, 4);

            packer.AddStr(token);
            packer.AddInt(enrtustno);
            packer.EndPack();

            this.sendpacker(OptionFunctionCode.Withdraw, packer);
        }
        #endregion


        //连接
        private CT2Connection connMain;
        private CT2Connection connSub;
        private CT2SubscribeInterface subcribe;
        private UFXSubCallback subcallback;
        public int subcribeid;
        public string token;
        private string curraccno = "";
        private string currcombino = "";

        public EntrustHunsun()
        {
            try
            {
                CT2Configinterface config = new CT2Configinterface();
                config.Load("t2sdk.ini");

                //连接主
                connMain = new CT2Connection(config);
                connMain.Create2BizMsg(this);
                int ret = connMain.Connect(5000);
                if (ret != 0)
                {
                    Debug.Print(string.Format("Main:连接{0}失败 错误号 {1} 错误信息 {2}", config.GetString("t2sdk", "servers", ""), ret, connMain.GetErrorMsg(ret)));
                }
                else
                {
                    Debug.Print((string.Format("Main:连接{0}成功", config.GetString("t2sdk", "servers", ""))));
                }

                //连接子
                connSub = new CT2Connection(config);
                connSub.Create(null);
                ret = connSub.Connect(5000);
                if (ret != 0)
                {
                    Debug.Print(string.Format("Sub:连接{0}失败 错误号 {1} 错误信息 {2}", config.GetString("t2sdk", "servers", ""), ret, connSub.GetErrorMsg(ret)));
                }
                else
                {
                    Debug.Print((string.Format("Sub:连接{0}成功", config.GetString("t2sdk", "servers", ""))));
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        //发送packer
        private int sendpacker(OptionFunctionCode functioncode, CT2Packer packer, bool IsAsync=true)
        {
            CT2BizMessage BizMessage = new CT2BizMessage();     //构造消息
            BizMessage.SetFunction((int)functioncode);          //设置功能号
            BizMessage.SetPacketType(0);                        //设置消息类型为请求

            unsafe
            {
                BizMessage.SetContent(packer.GetPackBuf(), packer.GetPackLen());
            }

            /************************************************************************/
            /* 此处使用异步发送 同步发送可以参考下面注释代码
             * connection.SendBizMsg(BizMessage, 0);
             * 1=异步，0=同步
            /************************************************************************/
            int iRet = this.connMain.SendBizMsg(BizMessage, (IsAsync) ? 1 : 0);
            if (iRet < 0)
            {
                Debug.Print(string.Format("发送错误 错误码 {0} 错误信息 {1}", iRet, connMain.GetErrorMsg(iRet)));
            }
            packer.Dispose();
            BizMessage.Dispose();

            return iRet;
        }

        //显示
        public void ShowUnPacker(CT2UnPacker lpUnPack)
        {
            int count = lpUnPack.GetDatasetCount();
            for (int k = 0; k < count; k++)
            {
                Debug.Print(string.Format("第[{0}]个数据集", k));
                lpUnPack.SetCurrentDatasetByIndex(k);
                String strInfo = string.Format("记录行数：           {0}", lpUnPack.GetRowCount());
                Debug.Print(strInfo);
                strInfo = string.Format("列行数：			 {0}", lpUnPack.GetColCount());
                Debug.Print(strInfo);
                while (lpUnPack.IsEOF() == 0)
                {
                    for (int i = 0; i < lpUnPack.GetColCount(); i++)
                    {
                        String colName = lpUnPack.GetColName(i);
                        sbyte colType = lpUnPack.GetColType(i);
                        if (!colType.Equals('R'))
                        {
                            String colValue = lpUnPack.GetStrByIndex(i);
                            String str = string.Format("{0}:			[{1}]", colName, colValue);
                            Debug.Print(str);
                        }
                        else
                        {
                            int colLength = 0;
                            unsafe
                            {
                                void* colValue = (char*)lpUnPack.GetRawByIndex(i, &colLength);
                                string str = string.Format("{0}:			[{1}]({2})", colName, Marshal.PtrToStringAuto(new IntPtr(colValue)), colLength);
                            }
                        }
                    }
                    lpUnPack.Next();
                }
            }
        }

        #region CT2CallbackInterface
        public override void OnClose(CT2Connection lpConnection)
        {
            Debug.Print(MethodBase.GetCurrentMethod().Name);
        }

        public override void OnConnect(CT2Connection lpConnection)
        {
            Debug.Print(MethodBase.GetCurrentMethod().Name);
        }

        public override void OnReceivedBiz(CT2Connection lpConnection, int hSend, string lppStr, CT2UnPacker lppUnPacker, int nResult)
        {
            Debug.Print(MethodBase.GetCurrentMethod().Name);
        }

        public override void OnReceivedBizEx(CT2Connection lpConnection, int hSend, CT2RespondData lpRetData, string lppStr, CT2UnPacker lppUnPacker, int nResult)
        {
            Debug.Print(MethodBase.GetCurrentMethod().Name);
        }

        public override void OnReceivedBizMsg(CT2Connection lpConnection, int hSend, CT2BizMessage lpMsg)
        {
            int iRetCode = lpMsg.GetErrorNo();//获取返回码
            int iErrorCode = lpMsg.GetReturnCode();//获取错误码
            int iFunction = lpMsg.GetFunction();
            if (iRetCode != 0)
            {
                Debug.Print("异步接收出错：" + lpMsg.GetErrorNo().ToString() + lpMsg.GetErrorInfo());
            }
            else
            {
                if (iFunction == 620000)//1.0消息中心心跳
                {
                    Debug.Print("收到心跳！==>" + iFunction);
                    lpMsg.ChangeReq2AnsMessage();
                    connMain.SendBizMsg(lpMsg, 1);
                    return;
                }
                else if (iFunction == 620003 || iFunction == 620025) //收到发布过来的行情
                {
                    Debug.Print("收到主推消息！==>" + iFunction);
                    int iKeyInfo = 0;
                    void* lpKeyInfo = lpMsg.GetKeyInfo(&iKeyInfo);
                    CT2UnPacker unPacker = new CT2UnPacker(lpKeyInfo, (uint)iKeyInfo);
                    //this.ShowUnPacker(unPacker);
                    unPacker.Dispose();

                }
                else if (iFunction == 620001)
                {
                    Debug.Print("收到订阅应答！==>");
                    return;
                }
                else if (iFunction == 620002)
                {
                    Debug.Print("收到取消订阅应答！==>");
                    return;
                }

                CT2UnPacker unpacker = null;
                unsafe
                {
                    int iLen = 0;
                    void* lpdata = lpMsg.GetContent(&iLen);
                    unpacker = new CT2UnPacker(lpdata, (uint)iLen);
                }
                if (iFunction == 10001)
                {
                    int code = unpacker.GetInt("ErrCode");
                    if (code == 0)
                    {
                        unpacker.SetCurrentDatasetByIndex(1);
                        token = unpacker.GetStr("user_token");
                    }
                }
                //this.ShowUnPacker(unpacker);
            }
        }

        public override void OnRegister(CT2Connection lpConnection)
        {
            //3
            Debug.Print(MethodBase.GetCurrentMethod().Name);
        }

        public override void OnSafeConnect(CT2Connection lpConnection)
        {
            //2
            Debug.Print(MethodBase.GetCurrentMethod().Name);
        }

        public override void OnSent(CT2Connection lpConnection, int hSend, void* lpData, int nLength, int nQueuingData)
        {
            //4
            Debug.Print(MethodBase.GetCurrentMethod().Name);
        }
        #endregion
    }

    public unsafe class UFXSubCallback : CT2SubCallbackInterface
    {
        private EntrustHunsun ufxengine = null;
        public UFXSubCallback(EntrustHunsun ufx)
        {
            this.ufxengine = ufx;
        }

        public override void OnReceived(CT2SubscribeInterface lpSub, int subscribeIndex, void* lpData, int nLength, tagSubscribeRecvData lpRecvData)
        {
            //Debug.Print("/*********************************收到主推数据 begin***************************/");
            //string strInfo = string.Format("附加数据长度：       {0}", lpRecvData.iAppDataLen);
            //Debug.Print(strInfo);
            //if (lpRecvData.iAppDataLen > 0)
            //{
            //    unsafe
            //    {
            //        strInfo = string.Format("附加数据：           {0}", Marshal.PtrToStringAuto(new IntPtr(lpRecvData.lpAppData)));
            //        Debug.Print(strInfo);
            //    }
            //}
            //Debug.Print("过滤字段部分：\n");
            //if (lpRecvData.iFilterDataLen > 0)
            //{
            //    CT2UnPacker lpUnpack = new CT2UnPacker(lpRecvData.lpFilterData, (uint)lpRecvData.iFilterDataLen);
            //    //ufxengine.ShowUnPacker(lpUnpack);
            //    lpUnpack.Dispose();
            //}
            //CT2UnPacker lpUnPack1 = new CT2UnPacker((void*)lpData, (uint)nLength);
            //if (lpUnPack1 != null)
            //{
            //    //ufxengine.ShowUnPacker(lpUnPack1);
            //    lpUnPack1.Dispose();
            //}
            //Debug.Print("/*********************************收到主推数据 end ***************************/");
        }

        public override void OnRecvTickMsg(CT2SubscribeInterface lpSub, int subscribeIndex, string TickMsgInfo)
        {
            Debug.Print(MethodBase.GetCurrentMethod().Name);
        }
    }
}
