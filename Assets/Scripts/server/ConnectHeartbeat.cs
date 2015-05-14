using UnityEngine;
using System.Collections;
using ProtoCmd;

public delegate void CallbackFunc4();

//连接的心跳
/// <summary>
/// //心跳机制改成10s 发一次,如果在30s内有收到任何一条消息,就算成功
/// </summary>
public class ConnectHeartbeat{


    private CallbackFunc4 m_timeout_callbakc = null;

    private bool m_bStartCheckTimeout = false;
    private float m_fSendSpaceTime = 10.0f; //发送ping的时间间隔
    private float m_fTimeOut = 30.0f;       //超时没收到任何服务器消息的时间

    private float m_fLastSendTime;  //最后发送的时间


    private bool m_bResetTimes = false;


    public ConnectHeartbeat(CallbackFunc4 timeout) 
    {
        m_timeout_callbakc = timeout;
    }


    //根据配置文件来配置发送心跳的时间规则
    public void LoadByConfigData() 
    {
        //Debug.LogError("LoadByConfigData  Heartbeat");

        m_fSendSpaceTime = 10.0f;
        m_fTimeOut = 30.0f;
    }


    //重置相关参数,相当于request/response成功
    public void ResetCheckTimeout() 
    {
        m_fLastSendTime = Time.realtimeSinceStartup;
    }


    public void StartCheckConnect()
    {
        m_bStartCheckTimeout = true;
        m_bResetTimes = true;
        //ResetCheckTimeout();
    }

    public void StopCheckConnect()
    {
        m_bStartCheckTimeout = false;
        m_bResetTimes = true;
        //ResetCheckTimeout();
    }

    private void SendPing() 
    {
        GameNetwork.Instance.SendCmd(CmdNumber.RequestPingClientCmd_C, null);
    }

    private void timeOut()
    {
        //停止心跳
        StopCheckConnect();
        if (m_timeout_callbakc != null)
        {
            m_timeout_callbakc();
        }

    }

    //是否为验证本
    //public bool IfSyncServerFb()
    //{
    //    if (GlobalRuntimeDataMgr.GetInstance().IsInFightScene()) 
    //    {
    //        return CombatSyncNetWork.GetInstance().IfSyncServerHP();
    //    }
    //    return false;

    //}

    


    public void UpdateHeartbeat(bool m_bReceiveError) 
    {
        //if (Time.frameCount % 10 != 0)
        //    return;

        //if (!IfSyncServerFb())
        //{
        //    //Debug.LogError("hello");
        //    return;
        //}



        

        //if (!m_bStartCheckTimeout)
        //    return;

        //if (m_bResetTimes)
        //{
        //    m_bResetTimes = false;
        //    ResetCheckTimeout();
        //}

        ////if (m_bReceiveError)
        ////{
        ////    m_bReceiveError = false;
        ////    Debug.Log("UpdateCheckConnect OnTimeout m_bReceiveError: " + m_bReceiveError);

        ////    timeOut();
        ////    return;
        ////}


        //float time = Time.realtimeSinceStartup;
        //if (time - m_fLastSendTime >= m_fSendSpaceTime) 
        //{
        //    //Debug.LogError("send ping XD");
        //    //10s 发一次
        //    m_fLastSendTime = time;
        //    SendPing();

        //}else if (time - m_fLastSendTime >= m_fTimeOut)
        //{
        //    timeOut();
        //}





    }















}
