using UnityEngine;
using System.Collections;
using ProtoCmd;

public class PayNetWork {

    private static PayNetWork Instance;
    public static PayNetWork GetInstance()
    {
        if (Instance == null)
        {
            Instance = new PayNetWork();
        }

        return Instance;
    }

    public void Init()
    {
        if (GameNetwork.m_service == null)
        {
            Logger.Error(LoggerType.LOG_CLIENT, "net work is not init");
            return;
        }
        //GameNetwork.m_service.RegParseFun(ParsFirstRechargeData, CmdNumber.UpdataFirstRechargeClientCmd_S);
       // GameNetwork.m_service.RegParseFun(ParsReceivePayData, CmdNumber.RechargeClientCmd_S);

        GameNetwork.m_service.RegParseFun(OnRspPayForGoodsCommand_S, CmdNumber.RspPayForGoodsCommand_S);
        GameNetwork.m_service.RegParseFun(OnMoneyChange, CmdNumber.MoneyChangeClientCmd_S);
        //GameNetwork.m_service.RegParseFun(OnRspGetTokenCommand_S, CmdNumber.RspGetTokenCommand_S);
        
    }


    //充值,发送产品id
    public void Recharge(int productid) 
    {
        //RechargeClientCmd req = new RechargeClientCmd();
        //req.id = (uint)productid;
        //GameNetwork.Instance.SendCmd(CmdNumber.RechargeClientCmd_C, req);
    }


    /// <summary>
    /// 向服务器申请订单
    /// </summary>
    /// <param name="data"></param>
    public void SendMessageToRequestOrderID(ReqPayForGoodsCommand data)
    {
        Debug.Log("SendMessageToRequestOrderID :" + data.pay_info.goods_id);
        GameNetwork.Instance.SendCmd(CmdNumber.ReqPayForGoodsCommand_C,data);
    }



    public void SendMessagePayResultOrder(uint platformID, string orderID, string queryParam)
    {
        ReqGetChargeResultCommand data = new ReqGetChargeResultCommand();
        data.platform_id = platformID;
        data.inner_order_id = System.Text.Encoding.UTF8.GetBytes(orderID);
        data.query_param = System.Text.Encoding.UTF8.GetBytes(queryParam);

        GameNetwork.Instance.SendCmd(CmdNumber.ReqGetChargeResultCommand_C, data);
    }



    /// <summary>
    /// 订单申请返回
    /// </summary>
    private bool OnMoneyChange(Message message)
    {

        MoneyChangeClientCmd rev = (MoneyChangeClientCmd)GameNetwork.Instance.AnalysisMessage(message, typeof(MoneyChangeClientCmd));
        
        if(rev.money_type == 0)
        {
             Debug.Log("获得金钱：" + rev.money_change.ToString() + "   目前金钱:"+rev.money_total.ToString());
             TestMain.Instance.moneyTotal = (int)rev.money_total;
        }
        else if (rev.money_type == 1)
        {
            Debug.Log("获得钻石：" + rev.money_change.ToString() + "   目前钻石:" + rev.money_total.ToString());
            TestMain.Instance.diamondTotal = (int)rev.money_total;
        }

        return true;
    }





    /*
    public void SendMessageToRequestToken(uint platformID,uint uid,)
    {
        ReqGetTokenCommand token = new ReqGetTokenCommand();
        token.platform_id = platformID;
        token.player_id = PlayerManager.Instance.GetMainPlayerProp().uid;
        token.user_name = CommonTool.Utf8StringToBytes(PlayerManager.Instance.GetMainPlayerProp().name);
        GameNetwork.Instance.SendCmd(CmdNumber.ReqGetTokenCommand_C, token);
    }
    

    private bool OnRspGetTokenCommand_S(Message message)
    {
        RspGetTokenCommand rev = (RspGetTokenCommand)GameNetwork.Instance.AnalysisMessage(message, typeof(RspGetTokenCommand));

        Debug.LogError("OnRspPayForGoodsCommand_S :" +   CommonTool.Utf8BytesToString(rev.token));


        return true;
    }
    */



    /// <summary>
    /// 订单申请返回
    /// </summary>
    private bool OnRspPayForGoodsCommand_S(Message message)
    {
        RspPayForGoodsCommand rev = (RspPayForGoodsCommand)GameNetwork.Instance.AnalysisMessage(message, typeof(RspPayForGoodsCommand));

        Debug.LogError("OnRspPayForGoodsCommand_S :" + System.Text.Encoding.UTF8.GetString(rev.pay_info.inner_order));

        CommonSDKPlaform.Instance.Pay(rev);

        return true;
    }


  


    /*
    private bool ParsFirstRechargeData(Message message) 
    {
        FirstRechargeClientCmd rev = (FirstRechargeClientCmd)GameNetwork.Instance.AnalysisMessage(message, typeof(FirstRechargeClientCmd));
        PayMgr.GetInstance().m_RechargeData = rev;
        return true;
    }



    private bool ParsReceivePayData(Message message) 
    {
        RechargeClientCmdRt rev = (RechargeClientCmdRt)GameNetwork.Instance.AnalysisMessage(message, typeof(RechargeClientCmdRt));
        //充值返回 0 成功 其他失败        
        UIManager.GetInstance().SendMsgToWndUI(EnWndID.ePay, EnWndMsgID.ePay_PayResult, rev.code);
        return true;
    }
    */




}
