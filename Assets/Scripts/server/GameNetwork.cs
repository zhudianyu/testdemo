//#define IS_CHECK_CONNECT
using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using ProtoCmd;
using System.IO;
using System.Text;
using System.Collections.Generic;
using ProtoBuf;

public class GameNetwork : MonoBehaviour
{
    public static GameNetwork Instance;

    private bool isConectServer = false;


    public static ServiceClient m_service;

    private string m_ip = "114.119.6.48";
	private ushort m_port = 8000;
	private ushort m_game = 3;

    public string IP { get { return LoginMgr.GetInstance().IP; } }
    public ushort Port { get { return LoginMgr.GetInstance().Port; } }
    public ushort Game { get { return LoginMgr.GetInstance().Game; } }

    public ushort Zone { get { return LoginMgr.GetInstance().Zone; } }

    public ushort m_zone { private set; get; }	// lyb=5, csc=4, zdf=3

    private string m_strUsername = "";

    private string m_strPassword = "";

    //连接失败
    private bool b_ConnectFail = false;
    //连接成功
    //private bool b_ConnectSuccess = false;
    //是否服务器主动要求断线
    private bool b_ConnectServerSelfBreak = false;
    //是否已经连接成功了平台服
    private bool b_ConnectPlantfromSuccess = false;
    //是否可以开始连接接入服
    private bool b_ConnectAccessSuccess = false;



	private bool m_bConnectAccessError = false;

    /// <summary>
    /// 在平台服务登录成功后会返回这个接入服的相关东西,这里做个保存引用,在重连的时候会用到
    /// </summary>
    private ServerReturnLoginSuccessLoginClientCmd m_AccessServiceInfo;

    /// <summary>
    /// 在断线重连的时候需要发送这个key过去
    /// </summary>
    private int m_iReconnectKey;


    private bool m_bIsReconnect = false;


    private bool m_bIsShowReLoginDlg = false;


    /// <summary>
    /// 接收消息的时候错误
    /// </summary>
    [HideInInspector]
    public bool m_bReceiveError = false;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        Instance = this;
        m_SendQueue = new SendMsgQueue();
        mHeartbeat = new ConnectHeartbeat(OnTimeout);

        //CreateService();
        //RegAllFun();
    }


    void onUpdateService() 
    {



        //连接失败
        if (b_ConnectFail)
        {
            //HideLoginIndicator();
            b_ConnectFail = false;
            isConectServer = false;
        }

        //服务器主动要求断线
        if (b_ConnectServerSelfBreak)
        {
            b_ConnectServerSelfBreak = false;
            if (b_ConnectPlantfromSuccess)
            {
                ToConnectFailUI();
            }
        }

        //连接成功
        if (b_ConnectAccessSuccess)
        {
            //Debug.LogError("connect access success");
            b_ConnectAccessSuccess = false;
            b_ConnectPlantfromSuccess = true;
            if (m_bIsReconnect)
            {
                Debug.Log("断线重连到接入服成功,需要发送key和account_id验证");
                m_bIsReconnect = false;
                ChangeReconnectState(ReconnectState.Connecting);
                //如果是重连,在连接接入服成功后,需要发送ReconnectLoginClientCmd_S命令,带key和accountid,然后服务端返回ReturnReconnectLoginClientCmd_S 消息,ret为1才表示成功
                SendReconnectKey();
            }
            else
            {
                Debug.Log("异步线程返回接入服信息，主线程开始连接接入服步骤!");
                Debug.Log("设置准备登陆的帐号密码---- m_strUsername:" + m_strUsername + "  m_strPassword: " + m_strPassword);
                m_service.m_processor.SetAccontAndPassword(m_strUsername, m_strPassword);
                m_service.m_processor.ConnectAccessSuccess();
            }
        }

        //这里检测tcp连接是否超时
        if (m_service != null && m_service.m_processor != null)
        {
            m_service.m_processor.UpdateCreateTcpTimeout();
        }


        //#if !UNITY_EDITOR ////美术那边调摄像机时可能导致断线,弹出对话框重连后可能导致那边调整的数据没法保存,所以在编辑模式下不做处理
        mHeartbeat.UpdateHeartbeat(m_bReceiveError);

        //UpdateCheckConnect();
        //连接接入服失败
        if (m_bConnectAccessError)
        {
            Debug.Log("connect access service error!");
            Debug.Log("Show MsgDlg from m_bConnectAccessError");
            m_bConnectAccessError = false;
            ChangeReconnectState(ReconnectState.ConnectFail);
            //Msgbox.Show(MessageBox.EnMsgBoxType.eMsgOnly, true, "提示", "连接接入服失败!", OnReconnectClick);
            //停止超时检测
            //StopCheckReconnectTimeout();

        }
        UpdateReconnect();

        //#endif



        if (isConectServer && m_service != null)
        {
            m_service.Process();
        }
        else if (!isConectServer)
        {

        }
        else if (m_service == null)
        {
            //  Debug.Log("m_service == null");
        }
    }



    void Update() 
    {
        if (Time.frameCount % 10 != 0)
            return;
        onUpdateService();



    }


    void OnApplicationQuit()
    {
        //Logger.Error(LoggerType.LOG_CLIENT, "GameNetwork  OnApplicationQuit");

        //SendLogoutCmd();

        //DestoryThread();

        //ProtoCmd.ErrorCode code = ProtoCmd.ErrorCode.ERR_APPPLCHECK_INVALID;

    }

    public void OnDestroy()
    {
        Debug.Log("on gamennetwork destroy");
        //Logger.Error(LoggerType.LOG_CLIENT, "GameNetwork  OnDestroy");
        SendLogoutCmd();
        DestoryThread();
    }

    //public void OnGameDestroy() 
    //{
    //    Debug.Log("on gamennetwork destroy");
    //    Logger.Error(LoggerType.LOG_CLIENT, "GameNetwork  OnDestroy");
    //    SendLogoutCmd();
    //    DestoryThread();
    //}



    private bool m_bIsSendLogout = false;



    //发送登出界面
    public void SendLogoutCmd()
    {
        LogoutClientCmd cmd = new LogoutClientCmd();
        SendCmd(CmdNumber.LogoutClientCmd_C, cmd);
        m_bIsSendLogout = true;
    }



    /// <summary>
    /// 创建客户端Service连接
    /// </summary>
    void CreateService()
    {
        m_service = new ServiceClient();

        //ServiceClient.OnResult on_result = new ServiceClient.OnResult(OnResult);

        ServiceClient.OnResult on_result = new ServiceClient.OnResult(LoginMgr.GetInstance().OnResult);
        m_service.Attach(on_result);

        m_service.Initilize(Game, Zone, LoginMgr.GetInstance().Version);//, true);
        Debug.Log("Create Success !   m_game:" + Game + "   m_zone:" + Zone.ToString());
        RegAllFun();

        //监听服务器主动断线
        m_service.m_processor.OnConnectServerSelfBreak = OnServerSelfBreak;
    }

    /// <summary>
    /// 注册所有监听函数，创建登陆管理对象
    /// </summary>
    private void RegAllFun()
    {
        m_service.RegParseFun(ParseSelectReturnSelect, CmdNumber.SelectReturnSelectClientCmd_S);
        m_service.RegParseFun(ParseLoginStepSelect, CmdNumber.LoginStepSelectClientCmd_S);

        m_service.RegParseFun(ParseReturnLogin, CmdNumber.ServerReturnLoginSuccessLoginClientCmd_S);

        m_service.RegParseFun(ParseReconnectKey, CmdNumber.SendReconnectKeyClientCmd_S);

        m_service.RegParseFun(ParseReconnectLoginResult, CmdNumber.ReturnReconnectLoginClientCmd_S);

        m_service.RegParseFun(OnReceiveServicePingRequest, CmdNumber.RequestPingClientCmd_S);

        m_service.RegParseFun(OnReceiveBackpackMsg, CmdNumber.RespondMsgClientCmd_S);

        //心跳相应
        //m_service.RegParseFun(OnReceiveConnectCheck, CmdNumber.RespondPingClientCmd_S);

        //MallManager.GetSingleton().InitNetwork();

        LoginMgr.GetInstance().InitLoginNetwork();


        		m_service.RegParseFun(onUpdatePlayerMainData,CmdNumber.UpdatePlayerMainDataClientCmd_S);


        ////初始化通用属性模块
        //CommonAttrMgr.GetInstance();

        //QueueLoginMgr.GetInstance();
    }


    #region Connect and Disconnect

    public bool Connect(string strIp, int nport)
    {
        if (m_service == null)
        {
			//Debug.Log("Recreate Service");
            SetConnectInfo(Game, Zone, LoginMgr.GetInstance().Version);
        }

        if (m_service.Connect(strIp, nport))
        {
            return true;
        }

        return false;
    }


    private void ConnectEx(string strIp, int nport)
    {
        Debug.Log("ConnectEx : Begin Connect Service: " + (m_service == null));
        if (m_service == null)
        {
            Debug.Log("ConnectEx : Recreate Service  注册监听函数");
            SetConnectInfo(this.Game, Zone, LoginMgr.GetInstance().Version);
            //注册监听函数
            m_service.m_processor.OnConnectExSuccess = new ServiceProcessor.OnConnectExSuccessFunc(CallBackConnectEx);
            m_service.m_processor.OnConnectFailSuccess = new ServiceProcessor.OnConnectFailFunc(CallBackConnectFail);
        }

        m_service.ConnectEx(strIp, nport);
    }

    public void CallBackConnectEx()
    {
        Debug.Log("CallBackConnectEx CallBackConnectEx");


        isConectServer = true;
    }

    public void CallBackConnectFail()
    {
        Debug.Log("CallBackConnectFail CallBackConnectFail");

        b_ConnectFail = true;

    }




    public void DisConnect()
    {
        if (m_service != null)
        {
            m_service.DisConnect();
        }
    }


    //现在用的 
    public void StartConectServerEx(string strIp, int nPort)
    {
        Debug.Log("连接-- strIp " + strIp + "  nPort:" + nPort.ToString());
        ConnectEx(strIp, nPort);
    }



    public void StartConnectServer()
    {
        //初始化排队的监听
        
        // isConectServer = Connect(m_ip, m_port);
        b_ConnectPlantfromSuccess = false;
        //Debug.Log("连接-              - strIp " + IP + "  nPort:" + Port);
        StartConectServerEx(IP, LoginMgr.GetInstance().Port);
        //ShowLoginIndicator();
    }


    /// <summary>
    /// 设置登陆信息
    /// </summary>
    public void SetConnectInfo(string ip, ushort nGame, ushort nPort, ushort nZone, string nVersion)
    {
        m_ip = ip;
        m_game = nGame;
        m_port = nPort;
        m_zone = nZone;
    }



    /// <summary>
    /// 这个地方还有问题，重复操作了，等做完登陆之后再处理。by xzp to do
    /// </summary>
    public void SetConnectInfo(ushort nGame, ushort nZone, string nVersion)
    {
        m_game = nGame;
        m_zone = nZone;

        if (m_service != null)
        {
            m_service.Initilize(nGame, nZone, nVersion);//, true);
        }
        else
        {
            CreateService();
            RegAllFun();
        }
    }

    public void DestoryThread()
    {
        if (m_service != null)
            m_service.DestoryThread();


    }

    public void UnRegisterCallback() 
    {
        //CommonAttrMgr.Destroy();
        //MallManager.DestoryData();

    }

    #endregion




    #region  Parse login info functions






    // 角色选择结果返回
    private bool ParseSelectReturnSelect(Message message)
    {
        // SelectReturnSelectClientCmd rev = SelectReturnSelectClientCmd.ParseFrom(message.m_cmd);


        SelectReturnSelectClientCmd rev = (SelectReturnSelectClientCmd)GameNetwork.Instance.AnalysisMessage(message, typeof(SelectReturnSelectClientCmd)); //new SelectReturnSelectClientCmd();
        //MemoryStream mem1 = new MemoryStream(message.m_cmd);
        //DTOSerializer dtoSerializer = new DTOSerializer();
        //rev = (SelectReturnSelectClientCmd)dtoSerializer.Deserialize(mem1, null, rev.GetType());

        Logger.Error(LoggerType.LOG_CLIENT, "ParseSelectReturnSelect");
        if (rev.result == (int)SelectResultRet.SelectResultRet_OK)
        {

        }
        else
        {
            Logger.Error(LoggerType.LOG_CLIENT, "select return error :" + rev.result);
        }
        return true;
    }

    private bool ParseLoginStepSelect(Message message)
    {
        //LoginStepSelectClientCmd rev = LoginStepSelectClientCmd.ParseFrom(message.m_cmd);


        LoginStepSelectClientCmd rev = (LoginStepSelectClientCmd)GameNetwork.Instance.AnalysisMessage(message, typeof(LoginStepSelectClientCmd)); //new LoginStepSelectClientCmd();
        //MemoryStream mem1 = new MemoryStream(message.m_cmd);
        //DTOSerializer dtoSerializer = new DTOSerializer();
        //rev = (LoginStepSelectClientCmd)dtoSerializer.Deserialize(mem1, null, rev.GetType());

        Debug.Log("进入登录场景流程=" + rev.step);

        if (rev.step == LoginStep.LoginStep_Done)
        {
            Logger.Error(LoggerType.LOG_CLIENT, "login step is done!");
        }
        else if (rev.step == LoginStep.LoginStep_Control)
        {

        }
        else if (rev.step == LoginStep.LoginStep_Access)
        {
            Debug.Log("init listener");
            // 成功登录到接入服

            //PlayerAttributeNetWork.GetInstance().Init();
            //PlayerLoginSceneNetWork.GetInstance().Init();
            //SpriteNetWork.GetInstance().Init();
            //SkillLearnNetWork.GetInstance().Init();
            //FieldSceneNetwork.GetInstance().Init();//注册场景相关消息，这里需要在登录的时候注册

            //ArenaNetWork.GetInstance().Init();		//ydh
            //VipWingNetWork.GetInstance().Init();	//ydh
            //FbResultNetWork.GetInstance().Init();	//ydh
            //TeamWorkNet.GetInstance().Init();		//ydh

            //StoryNetWork.GetInstance().Init();
            ////ArenaNetWork.GetInstance().Init();
            //GameItemNetWork.GetInstance().Init(); //注册背包消息
            //GearNetwork.GetInstance().Init();
            //CombatSyncNetWork.GetInstance().Init();//初始化同步信息
            //FriendNetWork.GetInstance().Init();
            //TaskNetWork.GetInstance().Init();
            //MarketManager.GetSingleton().Init();// 初始化拍卖行管理器
            //MailNetwork.GetInstance().Init();//初始化邮件系统
            //LegionNetwork.GetInstance().Init();//初始化军团系统
            //GiftNetWork.GetInstance().Init(); //初始化礼包系统
            //PayNetWork.GetInstance().Init();  //充值系统
            //SacrificeNetwork.GetInstance().Init();
            //CaiPiaoNetwork.GetInstance().Init();
            //LegionResFightNetWork.GetInstance().Init();
            //TraderMgr.GetInstance().InitNetWork();   //商人系统
            //AuguryMgr.GetInstance().InitNetWork();   //占卜系统
            //BonusActivityMgr.Instance.Init();
            //ActiveGiftNetwork.GetInstance().Init();
            //HorseUINetWork.GetInstance().Init();
            //MultiFubenNetwork.GetInstance().Init();
            //UnderGroundNetWork.Instance.Init();
            //MultiplArenaNetWork.GetInstance().Init();
            //NewBattleMgr.Instance.Init();
            //ChatNetWork.GetInstance().Init();
            //LegionMgr.Instance.Init();
            //ChatNetWork.GetInstance().Init()
            PayNetWork.GetInstance().Init();
            GameNetwork.m_service.RegParseFun(TestMain.Instance.onEnterGameScene, CmdNumber.AddMapMapScreenClientCmd_S);

            UnityEngine.Random.seed = (int)DateTime.Now.Ticks;//使用unity随机数类，这里设置随机种子,接下来可以任意地方调用UnityEngine.Random.RandomRange(min,max)

        }
        else if (rev.step == LoginStep.LoginStep_Relation)
        {
            Logger.Error(LoggerType.LOG_CLIENT, "login step is not done!" + rev.step.ToString());
        }
        else
        {
            Logger.Error(LoggerType.LOG_CLIENT, "login step is not done!" + rev.step.ToString());
        }

        return true;
    }

    private bool ParseReconnectKey(Message message)
    {
        SendReconnectKeyClientCmd rev = (SendReconnectKeyClientCmd)GameNetwork.Instance.AnalysisMessage(message, typeof(SendReconnectKeyClientCmd));

        m_iReconnectKey = rev.key;
        return true;
    }


    private bool ParseReconnectLoginResult(Message message)
    {

        ReturnReconnectLoginClientCmd rev = (ReturnReconnectLoginClientCmd)AnalysisMessage(message, typeof(ReturnReconnectLoginClientCmd));

        Debug.Log("ParseReconnectLoginResult: ret:" + rev.ret);
        //Debug.Log("ParseReconnectLoginResult ret:" + rev.ret);        
        if (rev.ret == 1)
        {
            Debug.Log("重连成功");
            OnReconnectSuccess();
            ChangeReconnectState(ReconnectState.ConnectSuccess);
        }
        else
        {
            Debug.Log("重连失败");
            OnReconnectFail();
        }
        return true;
    }


    private bool OnReceiveServicePingRequest(Message message) 
    {
        RespondPingClientCmd cmd = new RespondPingClientCmd();
        SendCmd(CmdNumber.RespondPingClientCmd_C, cmd);
        return true;
    }


    private bool OnReceiveBackpackMsg(Message message) 
    {

        RespondMsgClientCmd rev = (RespondMsgClientCmd)AnalysisMessage(message, typeof(RespondMsgClientCmd));
        if (m_reconnectState != ReconnectState.None) 
        {
            Debug.LogWarning("reconnect state is:" + m_reconnectState);
            //return true;
        }

        if (m_SendQueue.CheckBackpackMsg(rev.msg_id))
        {
            //ConnectingUI.ChangeState(UIConnecting.ConnectingState.Over);
        }

        return true;
    }




    /// <summary>
    /// 分析平台服返回的成功信息，连接接入服
    /// </summary>
    private bool ParseReturnLogin(Message message)
    {
        //ServerReturnLoginSuccessLoginClientCmd rev = ServerReturnLoginSuccessLoginClientCmd.ParseFrom(messgae.m_cmd);
        Debug.Log("分析平台服返回的成功信息，连接接入服!");

        ServerReturnLoginSuccessLoginClientCmd rev = (ServerReturnLoginSuccessLoginClientCmd)GameNetwork.Instance.AnalysisMessage(message, typeof(ServerReturnLoginSuccessLoginClientCmd)); //new ServerReturnLoginSuccessLoginClientCmd();
        m_AccessServiceInfo = rev;


        //LoginMgr.GetInstance().GetPlayerData().nAccountID = rev.account_id;

        LoginMgr.GetInstance().AccountID = rev.account_id;
        #if CommonSDK
        CommonSDKPlaform.Instance.SetUserID(rev.plat_userid.ToString());
        #endif

        //BaseGamePlatform.GameAccount= rev.account;//保存游戏帐号


        //GlobalRuntimeDataMgr.GetInstance().GetPlayerData().nAccountID = rev.account_id;

        /*   
           Debug.Log("rev.AccountId:" + rev.AccountId);
           Debug.Log("rev.Ip:" + rev.Ip);
           Debug.Log("rev.Port:" + rev.Port);
           Debug.Log("rev.LoginId:" + rev.LoginId);
           Debug.Log("rev.State:" + rev.State);
        */

        Debug.Log("my account name : " + rev.account + "   my account id : " + rev.account_id);


        //Debug.LogError("state:" + LoginMgr.GetInstance().m_LoginState);
        //保存用户名和密码信息，密码暂时用123456
        if (LoginMgr.GetInstance().m_LoginState == LoginMgr.eLoginState.快速登陆新帐号 || LoginMgr.GetInstance().m_LoginState == LoginMgr.eLoginState.快速登录)
        {
            //PlayerPrefsManager.SetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆帐号, rev.account);
            // PlayerPrefsManager.SetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆密码, "123456");
            LoginMgr.GetInstance().RecordPlayerAccountAndPassword(rev.account, "123456");

            m_strUsername = rev.account;
            m_strPassword = "123456";
        }
        else if (LoginMgr.GetInstance().m_LoginState == LoginMgr.eLoginState.登陆指定帐号)
        {
            string account = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家准备登陆的帐号);
            string pwd = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家准备登陆的密码);
            LoginMgr.GetInstance().RecordPlayerAccountAndPassword(account, pwd);
        }
        

        m_service.m_processor.SetID((uint)rev.account_id);
        m_service.m_processor.SetLoginID((uint)rev.login_id);

        Debug.Log("access service ip:" + rev.ip + ",port:" + rev.port);
        //rev.ip = "198.162.15.1";
        m_service.m_processor.ConnectAccessEx(rev.ip, (ushort)rev.port, new TcpServer.OnConnectSuccessFunc(CallBackAccessSuccess), new TcpServer.OnConnectFailFunc(CallBackAccessFail));

        return true;
    }





    /// <summary> 
    /// 登陆接入服成功，异步回调
    /// </summary>
    private void CallBackAccessSuccess()
    {
        //Debug.LogError("connect success xd");
        b_ConnectAccessSuccess = true;
        m_bReceiveError = false;
        isConectServer = true;
        if (!m_bIsReconnect)
        {    
            //开始心跳检测
            mHeartbeat.StartCheckConnect();
            //StartCheckConnect();
        }
    }

    /// <summary>
    /// 登陆接入服失败，异步回调
    /// </summary>
    private void CallBackAccessFail()
    {
        Debug.LogError("登陆接入服失败，CallBackAccessFail!");
        m_bConnectAccessError = true;
    }

    #endregion

    public void Login(string strUserName, string strPassword)
    {
        Debug.Log("准备登陆:" + strUserName + "   " + strPassword);
        //CpaPlatformReport.CpaLogin(strUserName);//统计登录
        if (m_service == null)
        {
            Debug.Log("m_service==null");
            return;
        }

        m_service.Login(strUserName, strPassword);
    }

    /// <summary>
    /// 平台登录
    /// </summary>
    public void LoginByPlatform()
    {
        m_service.LoginByPlatform();
    }


    ///// <summary>
    ///// 根据平台进行登陆
    ///// </summary>
    ///// <param name="platformId">平台ID</param>
    //public void LoginByPlatform(ClientPlatformType platformId)
    //{
    //    m_service.LoginByPlatform(ClientPlatformType.ClientPlatformType_ZQGame);
    //}


    ///// <summary>
    ///// 服务器主动断线处理
    ///// </summary>
    //private void OnServerDisconnect()
    //{
    //    //UIManager.GetInstance().ShowMessageBox(MessageBox.EnMsgBoxType.eMsgOnly, true, "提示", "服务器主动断线", null);
    //    Msgbox.Show(MessageBox.EnMsgBoxType.eMsgOnly, true, "提示", "服务器主动断线", null);
    //    Debug.Log("服务器主动断线");
    //}

    /// <summary>
    /// 监听回调的服务器主动断线
    /// </summary>
    /// <returns></returns>
    public void OnServerSelfBreak()
    {
        b_ConnectServerSelfBreak = true;
    }



    public void SendReconnectKey()
    {
        ReconnectLoginClientCmd send = new ReconnectLoginClientCmd();
        send.acc_id = m_AccessServiceInfo.account_id;
        send.check_key = m_iReconnectKey;
        //send.map_id = GameApp.GetInstance().LastMapID;
        Debug.Log("SendReconnectKey account_id:" + send.acc_id + ",check_key:" + send.check_key + ",mapid:" + send.map_id);
        SendCmd(CmdNumber.ReconnectLoginClientCmd_C, send);
    }


    /// <summary>
    /// 获取用户当前登陆帐号
    /// </summary>
    public string GetAccount()
    {
        return m_strUsername;
    }

    /// <summary>
    /// 获取用户当前密码
    /// </summary>
    public string GetPassword()
    {
        return m_strPassword;
    }



    public SendMsgQueue m_SendQueue;

    private DTOSerializer dtoSerializer = new DTOSerializer();
    /// <summary>
    /// 发送命令 by lhc
    /// </summary>
    /// <param name="cmdNumber"></param>
    /// <param name="cmd"></param>
    public void SendCmd(CmdNumber cmdNumber, IExtensible cmd)
    {
        byte[] bytes = null;
        if (cmd != null)
        {
            MemoryStream mem5 = new MemoryStream();
            dtoSerializer.Serialize(mem5, cmd);
            bytes = MemoryStreamToBytes(mem5, 0);
        }
        Message sendmsg = new Message(cmdNumber, bytes);
        if (!CheckCanSendMsg(sendmsg))
        {
            //缓冲没发送的消息,在重连成功后发送
            m_service.PushNoSendMsg(sendmsg);
            return;
        }
        //Debug.Log("CheckCanSendMsg2222");
        if (m_service != null)
        {
            bool isSend = m_service.Send(sendmsg);
            if (!isSend)
            {
                OnSendMsgError(sendmsg);
            }
            else
            {
                //发送到消息加到队列,同时锁屏
                if (m_SendQueue.Push(sendmsg)) 
                {
                    //if (!ConnectingUI.Visiable) 
                    //{
                    //    ConnectingUI.Visiable = true;
                    //    ConnectingUI.ChangeState(UIConnecting.ConnectingState.Ready);
                    //}
                }
            }
        }
        else
            Debug.LogWarning("Send cmd:" + cmdNumber + " error, GameNetwork.m_service not init!");
    }

    public byte[] MemoryStreamToBytes(MemoryStream memStream, int offset)
    {
        memStream.Seek(offset, SeekOrigin.Begin);
        int buffLength = (int)memStream.Length - offset;
        if (buffLength < 0)
            buffLength = 0;

        byte[] bytes = new byte[buffLength];
        memStream.Read(bytes, 0, buffLength);
        memStream.Seek(0, SeekOrigin.Begin);

        return bytes;
    }

    /// <summary>
    /// 对于PlayerMoveClientCmd_C命令会无视掉..
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public bool CheckCanSendMsg(Message msg)
    {
        if (m_SendQueue.isPassLockCmd(msg.m_cmd_no))
        {
            return true;
        }

        if (!isConectServer)
        {
            //这里直接菊花,重连
            ToReconnectByModel();
            return false;
        }
        return true;

    }
    /// <summary>
    /// 真正发消息的时候失败
    /// </summary>
    /// <param name="msg"></param>
    private void OnSendMsgError(Message msg)
    {
        if (bToAutoLogin)
        {
            Debug.LogError("what?");
            return;
        }
        if (!m_SendQueue.isPassLockCmd(msg.m_cmd_no))
        {
            m_service.PushNoSendMsg(msg);
            if (m_bIsShowReLoginDlg)
            {
                //已经弹出重新登录对话框了
                return;
            }
		

            ////显示菊花
            ToReconnectByModel();
            //ConnectingUI.Visiable = true;
            //ConnectingUI.ChangeState(UIConnecting.ConnectingState.ToReconnect);

            //ConnectUI.SetBtnData("", null);
            //ConnectUI.SetData("提示", "正在连接到服务器...");
            //if (m_reconnectState == ReconnectState.None)
            //{
            //    ChangeReconnectState(ReconnectState.ReadyConnect);
            //}
        }
    }


    public object AnalysisMessage(Message message, System.Type type)
    {
        MemoryStream mem1 = new MemoryStream(message.m_cmd);
        //DTOSerializer dtoSerializer = new DTOSerializer();
        return dtoSerializer.Deserialize(mem1, null, type);
    }


    #region 断线后自动重连规则

    //现在重连规则是,玩家断线后再次发送msg到service时，会触发重连(缓存消息) 
    //然后尝试连接5次,每次间隔5s 如果5次都没连上,弹出断线连接对话框,同时点确定后退出app

    public enum ReconnectState
    {
        None = -1,
        ReadyConnect = 0,
        ConnectTcp = 1,
        Connecting = 2,
        ConnectSuccess = 3,
        ConnectFail = 4,
        ConnectEnd = 5,     //重连次数达到上限后调用这个
    }



    

    //重连状态. 0.可以创建连接;1.正在重连中;2.重连成功,3:重连失败
    private ReconnectState m_reconnectState = ReconnectState.None;
    //重连的间隔时间(1次失败后,间隔2s再去尝试)
    private float m_fReconnectIntervalTime = 3.0f;
    //重连失败开始记录的时间
    private float m_fReconnectFailStartTime = 0.0f;

    private int m_iRconnectTotalTimes = 10;

    private int m_iReconnectTimes = 10;          //如果重连5次失败..弹出对话框，点击后退出app


    private float m_fStartConnectTime = 0.0f;
    private float m_fTimeOutTime = 20.0f;   //如果超过20s没断线重连成功,需要断线,然后重新走登录流程


    private bool m_bIsBackgroundConnect = false;    //是否需要在后台偷偷重连

    private void UpdateReconnect()
    {
        if (Time.frameCount % 10 != 0)
            return;

        //Debug.LogError("update reconnect:" + m_reconnectState);
        switch (m_reconnectState)
        {
            case ReconnectState.ReadyConnect:
                StartReconnectAccessService();
                break;
            case ReconnectState.Connecting:
                UpdateCheckReconnectTimeout();
                break;
            case ReconnectState.ConnectFail:
                UpdateReconnectFail();
                break;
        }
    }

    /// <summary>
    /// 所有修改重连状态值都通过这个方法
    /// </summary>
    /// <param name="state"></param>
    private void ChangeReconnectState(ReconnectState state)
    {
        Debug.Log("ChangeReconnectState 当前重连状态: " + state.ToString());

        
        switch (state)
        {
            case ReconnectState.ReadyConnect:
                //m_iReconnectTimes = m_iRconnectTotalTimes;
                m_reconnectState = state;
                break;
            case ReconnectState.ConnectTcp:
                m_bIsReconnect = true;
                m_reconnectState = state;
                break;

            case ReconnectState.Connecting:
                m_fStartConnectTime = Time.realtimeSinceStartup;
                m_reconnectState = state;
                Debug.LogError("start time:" + m_fStartConnectTime);
                break;
            case ReconnectState.ConnectFail:
                m_fReconnectFailStartTime = Time.realtimeSinceStartup;
                m_iReconnectTimes -= 1;
                m_reconnectState = state;
                Debug.LogError("times:" + m_iReconnectTimes + ",inbg:" + m_bIsBackgroundConnect);
                if (m_iReconnectTimes <= 0) 
                {
                    if (m_bIsBackgroundConnect)
                    {

                    }
                    else 
                    {
                        ChangeReconnectState(ReconnectState.ConnectEnd);
                    }
                }
                break;
            case ReconnectState.ConnectSuccess:
                Debug.LogError("UIConnecting.ConnectingState.Over");
                //ConnectingUI.ChangeState(UIConnecting.ConnectingState.Over);
                m_iReconnectTimes = m_iRconnectTotalTimes;
                m_reconnectState = state;
                ChangeReconnectState(ReconnectState.None);
                break;
            case ReconnectState.ConnectEnd:
                m_reconnectState = state;
                Debug.LogError("ToConnectEndUI");
                //ToConnectEndUI();
                break;
            default:
                m_reconnectState = state;
                break;


        }
        
    }

    private void OnAppQuit(GameObject go) 
    {
        Application.Quit();
    }


    private void StartReconnectAccessService()
    {
        Debug.Log("Start ReconnectAccessService");

        //先把当前连接销毁了
        if (m_service != null)
        {
            m_service.m_processor.DestoryThread();
        }
        m_service.m_processor.SetID((uint)m_AccessServiceInfo.account_id);
        m_service.m_processor.SetLoginID((uint)m_AccessServiceInfo.login_id);
        m_service.m_processor.ReConnectAccessService(m_AccessServiceInfo.ip, (ushort)m_AccessServiceInfo.port, new TcpServer.OnConnectSuccessFunc(CallBackAccessSuccess), new TcpServer.OnConnectFailFunc(CallBackAccessFail));
        ChangeReconnectState(ReconnectState.ConnectTcp);
    }



    //重连失败的update
    private void UpdateReconnectFail()
    {
        float time = Time.realtimeSinceStartup;
        if (time - m_fReconnectFailStartTime >= m_fReconnectIntervalTime)
        {
            if (m_iReconnectTimes <= 0) 
            {
                //Debug.LogError("xd00000");
                if (m_bIsBackgroundConnect)
                {
                    //m_bIsBackgroundConnect = false;
                    //Debug.LogError("xd1");
                    //m_bIsBackgroundConnect = false;
                    ////这里走正常的菊花流程
                    ChangeReconnectState(ReconnectState.None);
                    //ConnectingUI.Visiable = true;
                    //ConnectingUI.ChangeState(UIConnecting.ConnectingState.ToReconnect);
                    return;
                }
            }

            ChangeReconnectState(ReconnectState.ReadyConnect);
        }
    }

    //重连中的update
    private void UpdateCheckReconnectTimeout()
    {
        float time = Time.realtimeSinceStartup;
        Debug.LogError("ChecktimeOut:" + (time - m_fStartConnectTime));
        if (time - m_fStartConnectTime >= m_fTimeOutTime)
        {
            ChangeReconnectState(ReconnectState.ConnectFail);
            //ToReconnectTimeOutUI();
        }
    }



    //public void ToConnectEndUI()
    //{
    //    ConnectUI.SetData("提示", "您已经断开连接了!");
    //    ConnectUI.SetBtnData("确定", OnAppQuit);
    //    ConnectingUI.Visiable = false;
    //}


    ////发送重连消息超时触发(暂时也是调用重新登录
    //public void ToReconnectTimeOutUI() 
    //{
    //    ChangeReconnectState(ReconnectState.None);
    //    m_service.m_processor.DestoryThread();
    //    ConnectUI.SetBtnData("确定", OnAppQuit);
    //    ConnectUI.SetData("提示", "连接超时,您已经断开连接了!");
    //    ConnectingUI.Visiable = false;
    //}



    //显示网络中断,玩家点确定按钮后是重新登录的对话框(服务器主动断线会触发
    public void ToConnectFailUI() 
    {
        if (m_bIsSendLogout) 
        {
            m_bIsSendLogout = false;
            return;
        }
        
        //ConnectUI.SetBtnData("确定", OnReLoginClick);
        //string tips = LanguageMgr.Instance.GetText("CONNECT_FAIL");
        //ConnectUI.SetData("提示", tips);
        //服务器主动断线..不需要再重连了
        ChangeReconnectState(ReconnectState.None);

        //ConnectingUI.Visiable = false;

        m_bIsShowReLoginDlg = true;

    }

    //UIConnectMsg connectUI = null;
    //private UIConnectMsg ConnectUI
    //{
    //    get
    //    {
    //        if (connectUI == null)
    //        {
    //            connectUI = UIConnectMsg.createConnectUI(); //(UIConnectMsg)UIManager.GetInstance().FindWndBaseByEnWndID(EnWndID.eConnectPrompt);
    //            connectUI.OnInitUIData();
    //        }
    //        connectUI.gameObject.SetActive(true);

    //        return connectUI;
    //    }
    //}

    //UIConnecting _uiConnecting = null;
    //public UIConnecting ConnectingUI 
    //{
    //    get 
    //    {
    //        if (_uiConnecting == null) 
    //        {
    //            _uiConnecting = UIConnecting.CreateUI();
    //        }
    //        //_uiConnecting.ChangeState(UIConnecting.ConnectingState.None);
    //        return _uiConnecting;
    //    }

    //}

    //public void CloseConnectingUI() 
    //{
    //    ConnectingUI.ChangeState(UIConnecting.ConnectingState.Over);
    //}

    /// <summary>
    /// 开始重连
    /// </summary>
    public void StartToReconnect(int totalTimes, bool isRunInBg) 
    {
        Debug.LogError("state:" + m_reconnectState);
        if (m_reconnectState == ReconnectState.None)
        {
            m_iRconnectTotalTimes = totalTimes;
            m_bIsBackgroundConnect = isRunInBg;
            m_iReconnectTimes = m_iRconnectTotalTimes;
            Debug.LogError("total times:" + m_iReconnectTimes);
            ChangeReconnectState(ReconnectState.ReadyConnect);
        }
        //停止心跳检查
        mHeartbeat.StopCheckConnect();
    }


    //这里判定是否验证本或非验证本
    public void ToReconnectByModel() 
    {
        //if (mHeartbeat.IfSyncServerFb())
        //{
        //    Debug.LogError("Test Log by lhc ToReconnectNoUI");
        //    //验证本
        //    ConnectingUI.Visiable = true;
        //    ConnectingUI.ChangeState(UIConnecting.ConnectingState.ToReconnectNoUI);
        //}
        //else 
        //{
        //    Debug.LogError("Test Log by lhc ToReconnectUI");
        //    //非验证本
        //    ConnectingUI.Visiable = true;
        //    ConnectingUI.ChangeState(UIConnecting.ConnectingState.Ready);
        //}

    }








    //重新登录,服务器主动断线会调用这个进行登录
    public void OnReLoginClick(GameObject go)
    {
        //PlayerManager.Instance.ClearOtherPlayer();
        m_SendQueue.ClearQueueList();
        //ConnectUI.OnCloseUI();
        bToAutoLogin = true;
        m_bIsShowReLoginDlg = false;
        //跳转到loading界面
        //GameMain.Instance.SwitchAccount();

        //ConnectUI.OnCloseUI();
        //GlobalRuntimeDataMgr runtimeMgr = GlobalRuntimeDataMgr.GetInstance();
        //if (runtimeMgr.IsInPVE())
        //{
        //    if (MonsterManager.Instance != null)
        //    {
        //        MonsterManager.Instance.ClearAllMonster();
        //    }
        //}
        ////场景上的玩家清空
        //PlayerManager.Instance.SelfExit();
        //GameNetwork.m_service = null;
        //LoginMgr loginMgr = LoginMgr.GetInstance();
        //if (loginMgr == null)
        //{
        //    GameObject loginObj = new GameObject();
        //    loginObj.name = "LoginManager";
        //    loginMgr = loginObj.AddComponent<LoginMgr>();
        //}
        //loginMgr.InitConfigData();
        //loginMgr.ReLogin();
    }

    #endregion


    #region 心跳测试连接是否正常代码


    public ConnectHeartbeat mHeartbeat;        //心跳检测

    public void ResetHeartbeat()
    {
        mHeartbeat.ResetCheckTimeout();
    }

    public void LoadHeartbeatConfig()
    {
        mHeartbeat.LoadByConfigData();
    }

    //private bool m_bStartCheckTimeout = false;

    //private float m_fResponseTime = 0.0f;

    //private float m_fLastSendTime = 0.0f;

    //private float intervalTime = 30.0f;

    ///// <summary>
    ///// 如果发送后服务器没返回的次数>=该值,判断为断线
    ///// </summary>
    //private int m_iTimeoutCount = 1;

    ///// <summary>
    ///// 发送的时候会+1,接收到的时候会-1,每次发送的时候会检测这个值是否超过限定的数量,如果超过,判断为连接超时
    ///// </summary>
    //private int m_iSendCount = 0;


    //private void StartCheckConnect()
    //{
    //    m_bStartCheckTimeout = true;
    //    m_iSendCount = 0;
    //}

    //private void StopCheckConnect()
    //{
    //    m_bStartCheckTimeout = false;
    //    m_iSendCount = 0;
    //}

    ///// <summary>
    ///// 显示登录时等待旋转圈
    ///// </summary>
    //private void ShowLoginIndicator()
    //{
    //    UIManager.GetInstance().SendMsgToWndUI(EnWndID.eLogin_Prepare, EnWndMsgID.eLogin_ShowLoginIndicator, null);
    //}

    ///// <summary>
    ///// 隐藏登录时等待旋转圈
    ///// </summary>
    //private void HideLoginIndicator()
    //{
    //    UIManager.GetInstance().SendMsgToWndUI(EnWndID.eLogin_Prepare, EnWndMsgID.eLogin_HideLoginIndicator, null);
    //}

    //private void UpdateCheckConnect()
    //{
    //    if (!m_bStartCheckTimeout)
    //        return;

    //    if (m_bReceiveError)
    //    {
    //        m_bReceiveError = false;
    //        Debug.Log("UpdateCheckConnect OnTimeout m_bReceiveError: " + m_bReceiveError);
    //        OnTimeout();

    //        return;
    //    }


    //    float time = Time.realtimeSinceStartup;
    //    if (time - m_fLastSendTime >= intervalTime)
    //    {
    //        if (!IsTimeout())
    //        {
    //            SendCheckConnect();
    //            m_fLastSendTime = time;
    //        }
    //    }
    //}



    //private void SendCheckConnect()
    //{
    //    //Debug.Log("SendCheckConnect cmd");
    //    m_iSendCount += 1;
    //    SendCmd(CmdNumber.RequestPingClientCmd_C, null);
    //}



    //private bool OnReceiveConnectCheck(Message message)
    //{

    //    m_iSendCount -= 1;
    //    //服务器相应的时间差...
    //    float time = Time.realtimeSinceStartup;
    //    m_fResponseTime = time - m_fLastSendTime;
    //    //Debug.Log("OnReceiveConnectCheck... responsetime:" + m_fResponseTime);
    //    return true;
    //}


    //private bool IsTimeout()
    //{
    //    //Debug.Log("检测是否已超时:" + m_iSendCount + ", timeout:" + m_iTimeoutCount);
    //    if (m_iSendCount >= m_iTimeoutCount)
    //    {
    //        onNoReceivePingData();
    //        //OnTimeout();

    //        //超时
    //        return true;
    //    }
    //    return false;
    //}
    //void OnGUI() 
    //{
    //    if (GUI.Button(new Rect(350, 20, 80, 30), "Test")) 
    //    {
    //        m_iSendCount = 20;
    //    }
    //}

    ///// <summary>
    ///// 发出消息后超过一定时间没收到服务器消息
    ///// </summary>
    //private void onNoReceivePingData() 
    //{
    //    Debug.LogError("onNoReceivePingData");
    //    //mHeartbeat.StopCheckConnect();
    //    //StopCheckConnect();

    //    //先把当前连接销毁了
    //    if (m_service != null)
    //    {
    //        m_service.m_processor.DestoryThread();
    //    }

    //    //连接接入服标识
    //    b_ConnectAccessSuccess = false;

    //    ConnectUI.SetBtnData("", null);
    //    ConnectUI.SetData("提示", "正在连接到服务器...");
    //    if (m_reconnectState == ReconnectState.None)
    //    {
    //        //Debug.LogError("ReadyConnect");
    //        ChangeReconnectState(ReconnectState.ReadyConnect);
    //    }
    //}


    private void OnTimeout()
    {
        //onNoReceivePingData();

        mHeartbeat.StopCheckConnect();
        //StopCheckConnect();

        b_ConnectFail = true;
        //先把当前连接销毁了
        if (m_service != null) 
        {
            m_service.m_processor.DestoryThread();
        }
        m_SendQueue.ClearQueueList();
        //连接接入服标识
        b_ConnectAccessSuccess = false;

        ToReconnectByModel();

        //ConnectingUI.Visiable = true;
        //ConnectingUI.ChangeState(UIConnecting.ConnectingState.ToReconnect);



        //如果玩家在副本下,需要把怪物的AI攻击脚本暂时去掉
        //GlobalRuntimeDataMgr globalRuntime = GlobalRuntimeDataMgr.GetInstance();

        //if (globalRuntime.IsInPVE())
        //{
        //    if (MonsterManager.Instance != null)
        //    {
        //        MonsterManager.Instance.SetAllMonsterAIEnable(false);
        //    }
        //}

        ////玩家不能通过遥感移动
        //if (PlayerManager.Instance.GetMainActorView() != null)
        //{
        //    MoveController moveController = PlayerManager.Instance.GetMainActorView().GetComponent<MoveController>();
        //    if (moveController != null)
        //    {
        //        moveController.enabled = false;
        //    }
        //}
        Debug.LogError("OnTimeout");


    }

    bool onUpdatePlayerMainData(Message message)
    {

        UpdatePlayerMainDataClientCmd rev = (UpdatePlayerMainDataClientCmd)GameNetwork.Instance.AnalysisMessage(message, typeof(UpdatePlayerMainDataClientCmd));
        TestMain.Instance.playerID = rev.main_player_data.player_id;
        Debug.LogError("update id:" + TestMain.Instance.playerID);

        return true;
    }


    #endregion





    private void OnReconnectSuccess()
    {
		//GlobalRuntimeDataMgr runtimeMgr = GlobalRuntimeDataMgr.GetInstance();
        m_service.HandNoSendMsg();
        m_SendQueue.SendQueueMsg();

        mHeartbeat.StartCheckConnect();
        //StartCheckConnect();
        //TipsMessageMgr.GetInstance().DisplayTipsByCustom(11003, "重连成功!");
    }

    private void OnReconnectFail()
    {
        //按超时来处理,把tcp的连接close掉,弹出断线对话框
        OnTimeout();
        m_SendQueue.ClearQueueList();
        //TipsMessageMgr.GetInstance().DisplayTipsByCustom(11003, "重连失败!");
        //重连失败
        ToConnectFailUI();


    }

    public static bool bToAutoLogin = false;

    public void AutoLoginGame() 
    {
        Debug.LogError("AutoLoginGame");

        //GlobalRuntimeDataMgr.GetInstance().ResetDefaultValue();
        //GameNetwork.m_service = null;
        //LoginMgr loginMgr = LoginMgr.GetInstance();
        //if (loginMgr == null)
        //{
        //    GameObject loginObj = new GameObject();
        //    loginObj.name = "LoginManager";
        //    loginMgr = loginObj.AddComponent<LoginMgr>();
        //}
        //loginMgr.InitConfigData();
        //loginMgr.ReLogin();
    }
}
