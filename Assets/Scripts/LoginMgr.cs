using UnityEngine;
using System.Collections;
using System.Net;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using ProtoCmd;

public enum RoleClass
{
	eFighter = 1,
	eAssassin = 2,
	eRanger = 3,
	eMage = 4,
	eUnknow = 5,//用作尚未选角色
}

/// <summary>
/// 登录场景管理器
/// </summary>
public class LoginMgr 
{
	public enum eLoginState
	{
		注册帐号,
		登陆指定帐号,
		快速登陆新帐号,
		快速登陆最近账号,
		快速登录,             //快速登录,这个状态下不用切换到选中角色面板,用最近的角色进入
		//中青宝平台服登陆,
		断线重新登录,         //服务器主动断线,客户端要重新走1次登录流程 by lhc
		第三方平台服登录,     // 第3平台的登录,比如中青宝,9u什么的 by lhc
		第三方平台断线重登,  // 
		第三方平台快速登录,
		
	}
	
	[HideInInspector]
	public eLoginState m_LoginState = eLoginState.快速登陆新帐号;
	
	private static LoginMgr m_Instance;
	public static LoginMgr GetInstance()
	{
		if (m_Instance == null) 
		{
			m_Instance = new LoginMgr();
		}
		return m_Instance;
	}
	
	//储存准备登陆的帐号
	[HideInInspector]
	public string UserLoginName = string.Empty;
	
	//储存准备登陆的密码
	[HideInInspector]
	public string UserLoginPWD = string.Empty;
	
	//储存准备注册的帐号
	[HideInInspector]
	public string UserRegisterName = string.Empty;
	
	//储存准备注册的密码
	[HideInInspector]
	public string UserRegisterPWD = string.Empty;
	
	//private int m_version = 20140822;
	//private string m_strVersion = "0.1.2.14530";
	//test env
	public string m_ip = "119.147.215.27";
	public ushort m_port = 7000;
	private ushort m_game = 3;
	public ushort m_zone = 7;	// lyb=5, csc=4, zdf=3
	//production env 
	//public string m_ip = "114.119.6.48";
    //public ushort m_port = 8000;
    //private ushort m_game = 3;
    //public ushort m_zone = 50;	// lyb=5, csc=4, zdf=3
	
	//public string Version { get { return ConfigMgr.GetInstance().GetPlatformInfo().gameVersion; } }
	public string Version { get { return "1.3.1"; } }
	public string IP { get { return m_ip; } }
	public ushort Port { get { return m_port; } }
	public ushort Game { get { return m_game; } }
	public ushort Zone { get { return m_zone; } }
	
	//当前储存的名字
	string curCreateName = "角色名";
	
	//玩家信息数据
	public List<SelectPlayerData> list_PlayerData = new List<SelectPlayerData>();
	
	
	//是否第一次显示列表
	[HideInInspector]
	public bool b_FirstShowPlayerList = true;
	
	
	//玩家ID，临时储存，交换处理
	private int m_iAccountID = 0;
	
	public int AccountID
	{
		get { return m_iAccountID; }
		set { m_iAccountID = value; }
	}
	
	
	//public static string m_ServConfigFile = Application.persistentDataPath + "/ServerCharactorConfig_";
	//public ServerConfigFile m_PlayerServerConfig;
	
	////是否用来帐号登陆
	//private bool b_UseOldAccountLogin = false;
	
	//public void SetServiceData(ZoneData data)
	//{
	//    //Debug.LogError("setIp:" + data.m_strIP);
	//    m_ip = data.m_strIP;
	//    m_port = (ushort)data.m_iPort;
	//    m_zone = (ushort)data.nZoneID;
	//    //保存配置
	//    saveAllIpData();
	//}
	
	
	//public bool CheckZoneDataEqualsLastData(ZoneData data)
	//{
	//    if (data.m_strIP.Equals(m_ip) &&
	//        m_port == (ushort)data.m_iPort &&
	//        m_zone == (ushort)data.nZoneID)
	//    {
	//        return true;
	
	//    }
	
	
	//    return false;
	//}
	
	//public ZoneData GetLastLoginZoneData()
	//{
	//    ServiceListData serviceList = ServiceListData.GetInstance();
	//    return serviceList.GetZoneData(m_ip, (int)m_port, (int)m_zone);
	//}
	
	
	
	void Awake()
	{
		m_Instance = this;
		//LoadPlayerServerConfig();
	}
	
	//public string PlayerServerConfig
	//{
	//    get
	//    {
	//        return m_ServConfigFile + PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家准备登陆的帐号) + ".json";
	//    }
	//}
	
	//public bool IsSameAccount()
	//{
	//    return PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家准备登陆的帐号) == PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆帐号);
	//}
	
	/// <summary>
	/// 读取本地玩家数据 by lsj
	/// </summary>
	//public void LoadPlayerServerConfig()
	//{
	//    m_PlayerServerConfig = CommonTool.ReadJsonFile<ServerConfigFile>(PlayerServerConfig);
	//}
	
	///// <summary>
	///// 获取服务器角色配置 by lsj
	///// </summary>
	///// <param name="data"></param>
	///// <returns></returns>
	//public ServerConfig GetServerCharactorConfig(ZoneData data)
	//{
	//    ServerConfig config = null;
	//    if (m_PlayerServerConfig == null) {
	//        return config;
	//    }
	//    foreach (ServerConfig sc in m_PlayerServerConfig.m_configList)
	//    {
	//        if (sc.nServerId == data.ServerId && sc.nZonePageId == data.ZoneRange)
	//        {
	//            config = sc;
	//            break;
	//        }
	//    }
	//    return config;
	//}
	
	///// <summary>
	///// 保存角色数据
	///// </summary>
	///// <param name="data"></param>
	//public void SaveCharactorServerConfig(ZoneData data) {
	//    LoadPlayerServerConfig();
	//    if (m_PlayerServerConfig == null)
	//    {
	//        m_PlayerServerConfig = new ServerConfigFile();
	//    }
	//    var serverConifg = GetServerCharactorConfig(data);
	//    if (serverConifg == null)
	//    {
	//        serverConifg = new ServerConfig();
	//        serverConifg.nChartNum = 1;
	//        serverConifg.nServerId = data.ServerId;
	//        serverConifg.nZonePageId = data.ZoneRange;
	//        m_PlayerServerConfig.m_configList.Add(serverConifg);
	//    }
	//    else
	//    {
	//        serverConifg.nChartNum++;
	//    }
	//    CommonTool.WriteJsonFile<ServerConfigFile>(m_PlayerServerConfig, PlayerServerConfig);
	//}
	
	
	public void InitConfigData()
	{
		//LoadServerConfig();
	}
	
	public void InitLoginUI()
	{
		InitConfigData();
		//Logger.Info(LoggerType.LOG_CLIENT, "实例化登陆UI界面");
		//UIManager.GetInstance().ShowWndUI(EnWndID.eLogin_Prepare);
		//UIManager.GetInstance().ShowWndUI(EnWndID.eTips_TipsMessageMgr, true, true);
	}
	
	void saveAllIpData()
	{
		//保存ip 端口 zone
		System.IO.File.WriteAllText(Application.persistentDataPath + "/ip.ini", m_ip);
		System.IO.File.WriteAllText(Application.persistentDataPath + "/port.ini", m_port.ToString());
		System.IO.File.WriteAllText(Application.persistentDataPath + "/zone.ini", m_zone.ToString());
	}
	
	/*
   void OnGUI()
   {
       
       m_ip = GUI.TextArea(new Rect(10, 20, 100, 30), m_ip);
       if (GUI.Button(new Rect(120, 20, 100, 30), "save ip"))
       {
           System.IO.File.WriteAllText(Application.persistentDataPath + "/ip.ini", m_ip);
       }

       string aa = GUI.TextArea(new Rect(260, 20, 100, 30), m_port.ToString());
       m_port = ushort.Parse(aa);
       if (GUI.Button(new Rect(370, 20, 100, 30), "save port"))
       {
           System.IO.File.WriteAllText(Application.persistentDataPath + "/port.ini", m_port.ToString());
       }

       string bb = GUI.TextArea(new Rect(510, 20, 100, 30), m_zone.ToString());
       m_zone = ushort.Parse(bb);
       if (GUI.Button(new Rect(620, 20, 100, 30), "save zone"))
       {
           System.IO.File.WriteAllText(Application.persistentDataPath + "/zone.ini", m_zone.ToString());
       }
        


       if (GUI.Button(new Rect(120, 20, 100, 30), "Test ZqGame"))
       {
           LoginMgr.GetInstance().m_LoginState = LoginMgr.eLoginState.第三方平台快速登录;
           LoginMgr.GetInstance().TestThirdPartLogin();
       }

   }
    */
	/// <summary>
	/// 删除的时候消除引用
	/// </summary>
	void OnDestroy()
	{
		m_Instance = null;
	}
	
	/// <summary>
	/// 初始化登陆管理对象
	/// </summary>
	public void InitLoginNetwork()
	{
		/*************************
         * 监听服务器回调函数       
         ************************/
		
		//返回一个可用的名字
		GameNetwork.m_service.RegParseFun(SetUIRandomName, CmdNumber.CreateRandomRoleNameClientCmd_CS);
		
		//判断名字有效性
		GameNetwork.m_service.RegParseFun(ParseCheckNameSelect, CmdNumber.CheckNameSelectClientCmd_CS);
		
		//处理角色选择之后的返回结果
		GameNetwork.m_service.RegParseFun(ParsePlayerListSelect, CmdNumber.PlayerListSelectClientCmd_S);
		
		//创建角色失败
		GameNetwork.m_service.RegParseFun(OnCreateSelectResultClientCmd, CmdNumber.CreateSelectResultClientCmd_S);
		
		//注册帐号返回结果
		GameNetwork.m_service.RegParseFun(RegisterAccountReturn, CmdNumber.AccountRegisterClientCmd_CS);
		
		//服务器异常返回 
		GameNetwork.m_service.RegParseFun(ServerConnetFail, CmdNumber.ClientCmdNone);
		
		//Debug.LogError("InitLoginNetwork");
		//通用错误码处理
		GameNetwork.m_service.RegParseFun(OnRespCommonError, CmdNumber.RspErrorCmd_S);
        //登录返回信息
        GameNetwork.m_service.RegParseFun(OnRspPlatSDKInfoCommand, CmdNumber.RspPlatSDKInfoCommand_S);
	}
	
	/// <summary>
	/// 通用返回错误码的处理
	/// </summary>
	bool OnRespCommonError(Message message)
	{
		RspErrorCmd rev = (RspErrorCmd)GameNetwork.Instance.AnalysisMessage(message, typeof(RspErrorCmd)); //new RspErrorCmd();
		//Debug.LogError("errorType:" + (ErrorCode)rev.error);
		//Logger.Info(LoggerType.LOG_CLIENT, "OnResBattleMsTeam error code:" + rev.error);
		
		Debug.LogError("OnRespCommonError:" + rev.error);
		
		//switch (rev.error)
		//{
		//    case (uint)ErrorCode.ERR_ENTERFB_LEVEL_INVALID:
		//        //Debug.Log("leve  ===================");
		//        int lv = DictMgr.GetInstance().LevelInfoDic[(int)rev.levelid].ConditonLv;
		//        string str = "需要等级达到" + lv + "才能开启本关卡";
		//        TipsMessageMgr.GetInstance().DisplayTipsByCustom(13007, str);
		//        break;
		//    case (uint)ErrorCode.ERR_ENTERFB_UNLOCK_INVALID:
		//        int id = DictMgr.GetInstance().LevelInfoDic[(int)rev.levelid].PreLevelID[0];
		//        string name = DictMgr.GetInstance().LevelInfoDic[id].levelName;
		//        str = "需要通关" + name + "才能开启本关卡";
		//        TipsMessageMgr.GetInstance().DisplayTipsByCustom(13009, str);
		//        break;
		//    case (uint)ErrorCode.ERR_ENTERFB_TIME_INVALID:
		//        str = "进入副本时间非法";
		//        Debug.Log("进入副本时间非法");
		//        //   TipsMessageMgr.GetInstance().DisplayTipsByCustom(13009, str);
		//        break;
		//    case (uint)ErrorCode.ERR_ENTERFB_POWER_INVALID:
		//        //体力不足时，直接弹出购买弹框
		//        uint iCostDiamond = CommonAttrMgr.GetInstance().GetCommonAttrFromMemoryByKey((uint)GameAttr.ATTR_RS_LEVEL_BUY_COST);
		//        int iAPBought = DictMgr.GetInstance().m_GlobalVariablesDatas["AP_BOUGHT"].m_iIntValue;
		//        uint iBoughtCount = CommonAttrMgr.GetInstance().GetCommonAttrFromMemoryByKey((uint)GameAttr.ATTR_RS_LEVEL_BUY_POWER_TIMES);
		//        uint iCanBoughtTime = CommonAttrMgr.GetInstance().GetCommonAttrFromMemoryByKey((uint)GameAttr.ATTR_CONFIG_POWER_TIMES);
		//        string fomatContext = LanguageMgr.Instance.GetText("BUY_ENERGY_CONFIRM2", iCostDiamond, iAPBought, iBoughtCount, iCanBoughtTime);
		//        Msgbox.Show(MessageBox.EnMsgBoxType.YesNo, true, "", fomatContext, BuyTiliMessageBoxCallBack, "返回", "确定");
		//        //TipsMessageMgr.GetInstance().DisplayTipsById(13005);
		//        break;
		//    case (uint)ErrorCode.ERR_ENTERFB_NUM_INVALID:
		//        TipsMessageMgr.GetInstance().DisplayTipsById(15002);
		//        break;
		//    case (uint)ErrorCode.ERR_USERMAX:
		//        //Debug.LogError("ERR_USERMAX");
		//        QueueLoginMgr.GetInstance().openConnecttingUI();
		//        break;
		//    case (uint)ErrorCode.ERR_ACCESSNOSTART: //网关服没开
		//    case (uint)ErrorCode.ERR_NOGAMESERVER:  //服务器维护
		//    case (uint)ErrorCode.ERR_NOFINDSERVER:  //没有找到相应服务器
		//        TipsMessageMgr.GetInstance().DisplayTipsById((int)rev.error);
		
		//        Debug.LogError("state:" + GlobalRuntimeDataMgr.GetInstance().GetGameState());
		//        if (GlobalRuntimeDataMgr.GetInstance().GetGameState() == GlobalRuntimeDataMgr.GameState.Login)
		//        {
		//            UIManager.GetInstance().SendMsgToWndUI(EnWndID.eLogin_Prepare, EnWndMsgID.eLogin_HideLoginIndicator, null);
		//        }
		//        //else if(GlobalRuntimeDataMgr
		//        break;
		
		//    case (uint)ErrorCode.ERR_GAME_VERSION:
		//        Msgbox.Show(MessageBox.EnMsgBoxType.eMsgOnly, true, LanguageMgr.Instance.GetText("TEXT_PROMPT"), LanguageMgr.Instance.GetText("TEXT_GAME_FORCE_UPDATE"), ForceUpdate);
		//        UIManager.GetInstance().SendMsgToWndUI(EnWndID.eLogin_Prepare, EnWndMsgID.eLogin_HideLoginIndicator, null);
		//        break;
		//    case (uint)ErrorCode.ERR_GUEST_LOGIN:
		//        TipsMessageMgr.GetInstance().DisplayTipsById((int)rev.error);
		//        UIManager.GetInstance().SendMsgToWndUI(EnWndID.eLogin_Prepare, EnWndMsgID.eLogin_HideLoginIndicator, null);
		//        break;
		
		//    default:
		//        Debug.Log("通用错误提示处理：" + rev.error.ToString());
		//        TipsMessageMgr.GetInstance().DisplayTipsById((int)rev.error);
		//        break;
		//}
		
		return true;
	}
	
	///// <summary>
	///// 强制玩家更新MSGBOX
	///// </summary>
	//private void ForceUpdate(MessageBox.EnButtonClick enClick)
	//{
	//    string url = DictMgr.GetInstance().m_GlobalVariablesDatas["URL_FORCE_UPDATE"].m_strStringValue;
	//    Application.OpenURL(url);
	//}
	
	
	
	///// <summary>
	///// 对购买体力弹框操作的回调
	///// </summary>
	//private void BuyTiliMessageBoxCallBack(MessageBox.EnButtonClick enClick)
	//{
	//    if (enClick == MessageBox.EnButtonClick.eYes)
	//    {
	//        FieldSceneNetwork.GetInstance().RequestBuyAllTili();
	//    }
	//}
	
	#region ===================================主界面快速登陆部分===================================
	
	
	//public void PrepareLogin()
	//{
	//    Debug.Log("开始加载游戏");
	//    if (Application.loadedLevelName == "GameManager")
	//    {
	//        LoadSceneMgr.Instance.LoadSceneAdditive("Project_Scene_Denglu");
	//        CommonTool.AddGameObject("Prefabs/Base/PrepareGameMgr", null, "PrepareGameManager");
	//    }
	//}
	
	
	
	public void ReLogin()
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			m_LoginState = eLoginState.第三方平台断线重登;
			
			Debug.LogError("调用第3方平台登录");
			//第3方平台登录
			//PlatformMgr.GetInstance().m_Platform.Login();
			
		}
		else
		{
			m_LoginState = eLoginState.断线重新登录;
			
			GameNetwork.Instance.SetConnectInfo(m_ip, m_game, m_port, m_zone, Version);
			
			GameNetwork.Instance.StartConnectServer();
		}
	}
	
	
	
	
	/// <summary>
	/// 新帐号登陆
	/// </summary>
	public void NewPlayerLogin()
	{
		m_LoginState = eLoginState.快速登陆新帐号;
		
		GameNetwork.Instance.SetConnectInfo(m_ip, m_game, m_port, m_zone, Version);
		
		GameNetwork.Instance.StartConnectServer();
	}
	
	public void QuickEnternGame()
	{
		m_LoginState = eLoginState.快速登录;
		GameNetwork.Instance.SetConnectInfo(m_ip, m_game, m_port, m_zone, Version);
		GameNetwork.Instance.StartConnectServer();
	}
	
	public void TestThirdPartLogin()
	{
		m_LoginState = eLoginState.第三方平台服登录;
		GameNetwork.Instance.SetConnectInfo(m_ip, m_game, m_port, m_zone, Version);
		GameNetwork.Instance.StartConnectServer();
	}
	
	
	/// <summary>
	/// 用已有账号登陆
	/// </summary>
	public void OldPlayerLogin()
	{
		//GameNetwork.Instance.SetLoginUseOldAccount(true);
		
		
		m_LoginState = eLoginState.快速登陆最近账号;
		
		GameNetwork.Instance.SetConnectInfo(m_ip, m_game, m_port, m_zone, Version);
		
		GameNetwork.Instance.StartConnectServer();
	}
	
	/// <summary>
	/// 第三方平台登录
	/// </summary>
	public void ThirdlyPlatformLogin()
	{
		Debug.Log("call ThirdlyPlatformLogin:" + m_LoginState);
		if (m_LoginState != eLoginState.第三方平台断线重登 && m_LoginState != eLoginState.第三方平台快速登录)
		{
			m_LoginState = eLoginState.第三方平台服登录;
		}
		//ActiveCode.IsUseSDK = true;
		GameNetwork.Instance.SetConnectInfo(m_ip, m_game, m_port, m_zone, Version);
		GameNetwork.Instance.StartConnectServer();
	}
	
	
	/// <summary>
	/// 设置当前登陆基本信息
	/// </summary>
	public void SetConnetBaseInfo()
	{
		GameNetwork.Instance.SetConnectInfo(m_ip, m_game, m_port, m_zone, Version);
	}
	
	
	/// <summary>
	/// 连接平台服回调
	/// </summary>
	public void OnResult(LoginReturn ret_code)
	{
		//Debug.LogError("OnResult 取消监听");
		
		Debug.Log("LoginMgr OnResult : " + ret_code.ToString());
		
		if (ret_code == LoginReturn.LOGIN_RETURN_PLATFORMSUCCESS)// 成功登录到平台服
		{
			// 先关闭激活码窗口
			//ActiveCode.GetSingleton().CloseActiveCodeWnd();
			
			Debug.LogError("state:" + m_LoginState);
			switch (m_LoginState)
			{
				
			case eLoginState.快速登陆新帐号:
			{
				Debug.LogError("登陆步骤：快速登陆新帐号");
				//int useDeviceId = PlayerPrefsManager.GetIntValue(enum_Int_PlayerPrefs.int_UseDeviceIdLogin);
				
				//if (useDeviceId == 1)
				//{
				//    GameNetwork.Instance.Login(SystemInfo.deviceUniqueIdentifier, "123456");
				//}
				//else
				//{
				//    GameNetwork.Instance.Login("", "");
				//}
				
				
				GameNetwork.Instance.Login(TestMain.Instance.playerAccount, TestMain.Instance.playerPwd);
			}
				break;
			case eLoginState.快速登陆最近账号:
			{
				Debug.Log("登陆步骤：快速登陆最近账号");
				int useDeviceId = PlayerPrefsManager.GetIntValue(enum_Int_PlayerPrefs.int_UseDeviceIdLogin);
				string strAccount = "";
				string strPassword = "";
				useDeviceId = 0;
				if (useDeviceId == 1)
				{
					strAccount = SystemInfo.deviceUniqueIdentifier;
					strPassword = "123456";
				}
				else
				{
					strAccount = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆帐号);
					strPassword = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆密码);
					
					
					if (strAccount == "" && strPassword == "")
					{
						//Debug.LogError("what?xd");
						m_LoginState = eLoginState.快速登陆新帐号;
						
						
						PlayerPrefsManager.SetStringValue(enum_Str_PlayerPrefs.str_玩家准备登陆的帐号, "");
						PlayerPrefsManager.SetStringValue(enum_Str_PlayerPrefs.str_玩家准备登陆的密码, "");
					}
				}
				
				GameNetwork.Instance.Login(strAccount, strPassword);
			}
				break;
			case eLoginState.快速登录:
			{
				string strAccount = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆帐号);
				string strPassword = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆密码);
				if (strAccount != "" && strPassword != "")
				{
					GameNetwork.Instance.Login(strAccount, strPassword);
				}
				else
				{
					
					
					
					GameNetwork.Instance.Login(TestMain.Instance.playerAccount, TestMain.Instance.playerPwd);
					//GameNetwork.Instance.Login("", "");
				}
			}
				break;
				
			case eLoginState.注册帐号:
			{
				Debug.Log("登陆步骤：注册帐号");
				string strPreAccount = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家准备注册的帐号);
				string strPrePassword = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家准备注册的密码);
				
				RegisterAccount(strPreAccount, strPrePassword);
			}
				break;
				
			case eLoginState.登陆指定帐号:
			{
				Debug.Log("登陆步骤：登陆指定帐号");
				string strPreAccount2 = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家准备登陆的帐号);
				string strPrePassword2 = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家准备登陆的密码);
				
				GameNetwork.Instance.Login(strPreAccount2, strPrePassword2);
			}
				break;
			case eLoginState.第三方平台断线重登:
			case eLoginState.第三方平台快速登录:
			case eLoginState.第三方平台服登录:
			{
				Debug.Log("state:" + m_LoginState);
				//Debug.Log("登陆步骤：" + PlatformMgr.GetInstance().m_Platform.GetPlatformName() + "平台服登陆");
				GameNetwork.Instance.LoginByPlatform();
			}
				break;
			case eLoginState.断线重新登录:
			{
				Debug.LogError("服务器主动断线,需要重新登录");
				
				if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
				{
					string strAccount = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆帐号);
					string strPassword = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆密码);
					GameNetwork.Instance.Login(strAccount, strPassword);
					
				}
			}
				break;
			default:
				Debug.LogError("登陆步骤：出错了！没有这个步骤！");
				break;
			}
		}
		// 打开激活码窗口 by lsj
		//else if (ret_code == LoginReturn.LOGIN_RETURN_WAITACTIVE)
		//{
		//    ActiveCode.GetSingleton().OpenActiveCodeWnd();
		//}
		//else if (ret_code == LoginReturn.LOGIN_RETURN_UNKNOWN)
		//{
		//    //m_service = null;            
		//    Debug.Log("LOGIN_RETURN_UNKNOWN 未知错误");
		//    TipsMessageMgr.GetInstance().DisplayTipsById(10013);
		//    //UIManager.GetInstance().ShowMessageBox(MessageBox.EnMsgBoxType.eMsgOnly, true, "提示", "未知错误！", null);
		//    //CommonTools.FlashPrompt("LOGIN_RETURN_UNKNOWN 未知错误");
		//}
		//else if (ret_code == LoginReturn.LOGIN_RETURN_PASSWORDERROR)
		//{
		//    TipsMessageMgr.GetInstance().DisplayTipsById(10007);
		//    //UIManager.GetInstance().ShowMessageBox(MessageBox.EnMsgBoxType.eMsgOnly, true, "提示", "用户密码错误", null);
		//    //Debug.Log("用户密码错误");
		//}
		//else if (ret_code == LoginReturn.LOGIN_RETURN_IDINUSE)
		//{
		//    TipsMessageMgr.GetInstance().DisplayTipsById(10014);
		//    //UIManager.GetInstance().ShowMessageBox(MessageBox.EnMsgBoxType.eMsgOnly, true, "提示", "账号正在被使用中！", null);
		//}
		//else if (ret_code == LoginReturn.LOGIN_RETURN_VERSIONERROR)
		//{
		//    TipsMessageMgr.GetInstance().DisplayTipsById(10015);
		//    //UIManager.GetInstance().ShowMessageBox(MessageBox.EnMsgBoxType.eMsgOnly, true, "提示", "版本信息错误，请下载最新版本！", null);
		//}
		//else
		//{
		//    TipsMessageMgr.GetInstance().DisplayTipsById(10016);
		//    //UIManager.GetInstance().ShowMessageBox(MessageBox.EnMsgBoxType.eMsgOnly, true, "提示", "服务器返回 登录失败!", null);
        //}
    }



    /// <summary>
    /// 通用SDK返回的字段，需要客户端记录。
    /// </summary>
    bool OnRspPlatSDKInfoCommand(Message message)
    {
        RspPlatSDKInfoCommand result = (RspPlatSDKInfoCommand)GameNetwork.Instance.AnalysisMessage(message, typeof(RspPlatSDKInfoCommand)); //new RspErrorCmd();

        CommonSDKPlaform.Instance.SetLoginData(System.Text.Encoding.UTF8.GetString(result.sdk_info));

        return true;
    }

	/// <summary>
	/// 服务器异常返回
	/// </summary>
	/// <param name="message"></param>
	/// <returns></returns>
	bool ServerConnetFail(Message message)
	{
		//Debug.LogWarning("Receive ClientCmdNone!");
		//TipsMessageMgr.GetInstance().DisplayTipsByCustom(2, "服务器异常（未启动或启动不正常） 登录失败!");
		//TipsMessageMgr.GetInstance().DisplayTipsById(10014);
		//UIManager.GetInstance().ShowMessageBox(MessageBox.EnMsgBoxType.eMsgOnly, true, "提示", "服务器异常（未启动或启动不正常） 登录失败", null);
		return true;
	}
	
	
	#endregion
	
	#region ===================================注册帐号部分===================================
	
	/// <summary>
	/// 注册一个帐号
	/// </summary>
	public void RegisterAccount(string strAccount, string strPassword)
	{
		GameNetwork.Instance.SetConnectInfo(m_ip, m_game, m_port, m_zone, Version);
		
		AccountRegisterClientCmd data = new AccountRegisterClientCmd();
		data.account = strAccount;
		data.password = strPassword;
		
		GameNetwork.Instance.SendCmd(CmdNumber.AccountRegisterClientCmd_CS, data);
	}
	
	/// <summary>
	/// 注册帐号的返回信息
	/// </summary>
	private bool RegisterAccountReturn(Message message)
	{
		AccountRegisterClientCmd rev = (AccountRegisterClientCmd)GameNetwork.Instance.AnalysisMessage(message, typeof(AccountRegisterClientCmd));
		
		Debug.Log("注册帐号返回结果：" + rev.ret_code.ToString());
		
		switch (rev.ret_code)
		{
		case LoginReturn.LOGON_RETURN_ACCOUNTSUCCESS:
			//UIManager.GetInstance().ShowMessageBox(MessageBox.EnMsgBoxType.eMsgOnly, true, "提示", "注册成功!", null);
			//UIManager.GetInstance().SendMsgToWndUI(EnWndID.eLogin_Prepare, EnWndMsgID.eLogin_ShowRegisterSuccess, null);
			break;
			
		case LoginReturn.LOGIN_RETURN_ACCOUNTEXIST:
			//TipsMessageMgr.GetInstance().DisplayTipsById(10003);
			//UIManager.GetInstance().ShowMessageBox(MessageBox.EnMsgBoxType.eMsgOnly, true, "提示", "帐号已存在", null);
			break;
		}
		
		return true;
	}
	
	
	#endregion
	
	
	
	
	
	#region ===================================创建角色部分===================================
	
	/// <summary>
	/// 请求服务器一个随机的名字
	/// </summary>
	public void SendMessageToAskForRandomName(RoleClass role)
	{
		Debug.Log("SendMessageToAskForRandomName:" + role);
		CreateRandomRoleNameClientCmd data = new CreateRandomRoleNameClientCmd();
		data.sex = 0;
		switch (role)
		{
		case RoleClass.eFighter:
			data.sex = 0;
			break;
		case RoleClass.eAssassin:
			data.sex = 0;
			break;
		case RoleClass.eMage:
			data.sex = 1;
			break;
		}
		GameNetwork.Instance.SendCmd(CmdNumber.CreateRandomRoleNameClientCmd_CS, data);
	}
	
	/// <summary>
	/// 显示回调的随机名字在创建角色UI上
	/// </summary>
	bool SetUIRandomName(Message message)
	{
		CreateRandomRoleNameClientCmd rev = (CreateRandomRoleNameClientCmd)GameNetwork.Instance.AnalysisMessage(message, typeof(CreateRandomRoleNameClientCmd));
		
		Debug.Log("SetUIRandomName : " + rev.name);
		//UIManager.GetInstance().SendMsgToWndUI(EnWndID.eLogin_Prepare, EnWndMsgID.eLogin_SetRandomRoleName, rev.name);
		
		TestMain.Instance.setRandName(rev.name);
		
		
		return true;
	}
	
	
	
	/// <summary>
	/// 发送角色名字判断可用性消息
	/// </summary>
	/// <param name="strName"></param>
	public void CheckPlayerSelectName(string strName)
	{
		//Debug.LogError(strName + "  CharArray length:" + strName.ToCharArray().Length + "   bytes length:" + CommonTool.Utf8StringToBytes(strName).Length);
		
		//用户名长度不得超过18字节
		if (ECommonTool.Utf8StringToBytes(strName).Length > 18)
		{
			Debug.LogError("用户名长度不得超过18字节");
			//TipsMessageMgr.GetInstance().DisplayTipsById(25);
			return;
		}
		
		CheckNameSelectClientCmd send = new CheckNameSelectClientCmd();
		send.name = strName;
		GameNetwork.Instance.SendCmd(CmdNumber.CheckNameSelectClientCmd_CS, send);
	}
	
	public bool checkRolePosition()
	{
		RoleClass role = (RoleClass)SelectRoleMgr.Instance.GetCurSelectRole();
		if (role == RoleClass.eAssassin || role == RoleClass.eFighter || role == RoleClass.eMage)
		{
			return true;
		}
		return false;
	}
	
	/// <summary>
	/// 处理角色名合法判断返回信息，名字无误的话则开始创建角色
	/// </summary>
	private bool ParseCheckNameSelect(Message message)
	{
		//Debug.Log("ParseCheckNameSelect");
		
		if (!checkRolePosition())
		{
			Debug.LogError("选择职业错误");
			//TipsMessageMgr.GetInstance().DisplayTipsByCustom(11003, "选择职业错误!");
			return true;
		}
		
		CheckNameSelectClientCmd rev = (CheckNameSelectClientCmd)GameNetwork.Instance.AnalysisMessage(message, typeof(CheckNameSelectClientCmd));
		//0为正确结果
		if (rev.err_code == 0)
		{
			CreatePlayerData send = new CreatePlayerData();
			send.charid = 0;
			send.role = SelectRoleMgr.Instance.GetCurSelectRole();
			
			send.name = rev.name;
			send.account = LoginMgr.GetInstance().UserLoginName;
			//send.accid = GetPlayerData().nAccountID;
			send.accid = AccountID;
			CreateSelectClientCmd bld = new CreateSelectClientCmd();
			bld.data = send;
			
			SendPlayerCreateRoleMsg(bld);
			
			// 保存创建的角色数量
			//SaveCharactorServerConfig(GetLastLoginZoneData());
			//显示进入游戏页面
			//UIManager.GetInstance().SendMsgToWndUI(EnWndID.eLogin_Prepare, EnWndMsgID.eLogin_ShowGameBeginPage, null);
		}
		//repeat
		//else if (rev.err_code == 1)
		//{
		//    Debug.LogWarning("角色名重复");
		//    TipsMessageMgr.GetInstance().DisplayTipsById(10011);
		//    //UIManager.GetInstance().ShowMessageBox(MessageBox.EnMsgBoxType.eMsgOnly, true, "提示", "角色名重复", null);
		//    //CommonTools.DebugShowLog("the same name!");
		//    //CommonTools.FlashPrompt("角色名重复");
		//}
		////invalid
		//else if (rev.err_code == 2)
		//{
		//    //Debug.LogWarning("无效的角色名");
		//    TipsMessageMgr.GetInstance().DisplayTipsById(10010);
		
		//    //UIManager.GetInstance().ShowMessageBox(MessageBox.EnMsgBoxType.eMsgOnly, true, "提示", "无效的角色名", null);
		//    //CommonTools.DebugShowLog("the invalid name!");
		//    //CommonTools.FlashPrompt("无效的角色名");
		//}
		//else if (rev.err_code == (int)LoginReturn.LOGIN_RETURN_PASSWORDERROR)
		//{
		//    Debug.Log("err code :" + rev.err_code.ToString());
		//    TipsMessageMgr.GetInstance().DisplayTipsById(10007);
		
		//    //Debug.LogWarning("密码错误");
		//    //UIManager.GetInstance().ShowMessageBox(MessageBox.EnMsgBoxType.eMsgOnly, true, "提示", "密码错误。" + rev.err_code, null);
		//}
		//else
		//{
		//    Debug.LogWarning("未知错误，错误代码：" + rev.err_code);
		//    TipsMessageMgr.GetInstance().DisplayTipsByCustom(2, "未知错误，错误代码：" + ((ErrorCode)rev.err_code).ToString());
		//    //TipsMessageMgr.GetInstance().DisplayTipsById(10017);
		//    //UIManager.GetInstance().ShowMessageBox(MessageBox.EnMsgBoxType.eMsgOnly, true, "提示", "未知错误，错误代码：" + rev.err_code, null);
		//}
		
		return true;
	}
	
	
	/// <summary>
	/// 发送角色选择信息
	/// </summary>
	public void SendPlayerSelectMsg(int charid)
	{
		PlayerSelectClientCmd send = new PlayerSelectClientCmd();
		
		send.charid = charid;
		
		GameNetwork.Instance.SendCmd(CmdNumber.PlayerSelectClientCmd_C, send);
	}
	
	
	/// <summary>
	/// 发送创建角色信息
	/// </summary>
	/// <param name="data">角色数据类</param>
	public void SendPlayerCreateRoleMsg(CreateSelectClientCmd data)
	{
		curCreateName = data.data.name;
		GameNetwork.Instance.SendCmd(CmdNumber.CreateSelectClientCmd_C, data);
	}
	
	
	private bool autoEnterGame(PlayerListSelectClientCmd rev)
	{
		int lastRoldId = PlayerPrefsManager.GetIntValue(enum_Int_PlayerPrefs.int_玩家上次登陆的角色ID);
		SelectPlayerData selRole = null;
		if (rev.data.Count > 0)
		{
			foreach (SelectPlayerData data in rev.data)
			{
				if (data.role == lastRoldId)
				{
					selRole = data;
					break;
				}
			}
			
			if (selRole != null)
			{
				Debug.Log("Enter game....");
				//自动选择角色,进入游戏
				SendPlayerSelectMsg(selRole.charid);
				return true;
			}
		}
		return false;
		
	}
	
	public bool m_bIsCreateModel = false;
	
	/// <summary>
	/// 处理返回的角色列表信息，先保存在全局数据里面。
	/// </summary>
	private bool ParsePlayerListSelect(Message message)
	{
		//取消监听
		//QueueLoginMgr.GetInstance().DestroyData();
		
		
		Debug.Log("here to create model");
		playerListData = (PlayerListSelectClientCmd)GameNetwork.Instance.AnalysisMessage(message, typeof(PlayerListSelectClientCmd));
		//GlobalRuntimeDataMgr.GetInstance().list_PlayerData = rev.data;
		list_PlayerData = playerListData.data;
		
		
		sendClientTokenCode();
		
		
		//自动重新登录的处理,在服务器主动断线后会调用重新登录,会调用到这里
		if (m_LoginState == eLoginState.断线重新登录 || m_LoginState == eLoginState.第三方平台断线重登)
		{
			autoEnterGame(playerListData);
			return true;
		}
		else if (m_LoginState == eLoginState.快速登录 || m_LoginState == eLoginState.第三方平台快速登录)
		{
			//Debug.LogError("start autoxd");
			if (autoEnterGame(playerListData))
			{
				//Debug.LogError("return xd");
				return true;
			}
		}
		if (!m_bIsCreateModel)
		{
			Debug.LogError("create player model.....");
			onCreatePlayerModelOver();
			//PrepareGameMgr.GetInstance().StartCreatePlayerModel(onCreatePlayerModelOver);
			m_bIsCreateModel = true;
		}
		else
		{
			onCreatePlayerModelOver();
		}
		
		TestMain.Instance.ChangeMainState(2);
		
		//创建模型
		//PrepareGameMgr.GetInstance().CallBackBake2DBackgroundFinish();
		
		
		
		
		
		
		return true;
	}
	PlayerListSelectClientCmd playerListData = null;
	private void onCreatePlayerModelOver()
	{
		//m_bIsCreateModel = true;
		//PrepareGameMgr.GetInstance().UpdateRoleHUDName(playerListData.data);
		//显示
		//UIManager.GetInstance().HideWndUI(EnWndID.eLogin_Prepare);
		if (b_FirstShowPlayerList)
		{
			
			//登陆成功，返回角色列表，提示玩家登陆成功。
			//TipsMessageMgr.GetInstance().DisplayTipsById(10009);
			
			b_FirstShowPlayerList = false;
			
			Debug.LogError("登录成功,返回角色列表");
			//UIManager.GetInstance().SendMsgToWndUI(EnWndID.eLogin_Prepare, EnWndMsgID.eLogin_ShowPreparePage, null);
			
			//PrepareGameMgr.GetInstance().ShowAllRoleObject();
			
			if (m_LoginState == eLoginState.快速登陆最近账号 || m_LoginState == eLoginState.第三方平台服登录)
			{
				if (playerListData.data.Count > 0)
				{
					int iShowRoleID = PlayerPrefsManager.GetIntValue(enum_Int_PlayerPrefs.int_玩家上次登陆的角色ID);
					
					foreach (SelectPlayerData data in playerListData.data)
					{
						if (data.role == iShowRoleID)
						{
							SelectRoleMgr.Instance.SetSelectRole((RoleClass)iShowRoleID);
							//UIManager.GetInstance().SendMsgToWndUI(EnWndID.eLogin_Prepare, EnWndMsgID.eLogin_TriggerUIToSelectRole, iShowRoleID);
						}
					}
				}
			}
			//假设没有角色列表，给玩家随机一个名字
			//if (rev.data.Count <=2)
			//{
			//    SendMessageToAskForRandomName(RoleClass.eFight);
			//}
			
		}
		else
		{
			/*
            //显示进入游戏页面
            UIManager.GetInstance().SendMsgToWndUI(EnWndID.eLogin_Prepare, EnWndMsgID.eLogin_ShowGameBeginPage, null);

            if (rev.data.Count > 0)
            {
                for (int i = 0; i < rev.data.Count; i++)
                {
                    Debug.Log(rev.data[i].name);
                }
            }*/
			Debug.LogError("DisplayTipsById 10019");
			DelaySendMessageToEnterGame();
			//TipsMessageMgr.GetInstance().DisplayTipsById(10019);
			//Invoke("DelaySendMessageToEnterGame", 1.0f);
			
		}
		
	}
	
	
	
	/// <summary>
	/// 创建角色结果
	/// </summary>
	private bool OnCreateSelectResultClientCmd(Message message)
	{
		CreateSelectResultClientCmd data = (CreateSelectResultClientCmd)GameNetwork.Instance.AnalysisMessage(message, typeof(CreateSelectResultClientCmd));
		
		if (data.error_code == 0)
		{
			Debug.Log("创建角色成功! 角色名：" + curCreateName);
			//CpaPlatformReport.CpaCreateRoles(curCreateName);
		}
		else
		{
			//TipsMessageMgr.GetInstance().DisplayTipsByCustom(10019, ((ErrorCode)data.error_code).ToString());
			Debug.LogError("ParseCreateRoleFail : " + ((ErrorCode)data.error_code).ToString());
		}
		return true;
	}
	
	
	
	/// <summary>
	/// 发送信息告诉服务器当前在什么平台用什么设备，服务器说网关服等自己获取很麻烦。by xzp
	/// </summary>
	private void sengMessageLetServerRecordPlatform(string deviceToken = "")
	{
		//Debug.Log("sengMessageLetServerRecordPlatform m_deviceType:" + PlatformMgr.GetInstance().m_deviceType + "   m_platformType:" + PlatformMgr.GetInstance().m_platformType.ToString());
		
		ReqInitDeviceCmd data = new ReqInitDeviceCmd();
		
		//data.mtype = PlatformMgr.GetInstance().m_deviceType;
		//data.ptype = PlatformMgr.GetInstance().m_platformType;
		data.token = System.Text.Encoding.UTF8.GetBytes(deviceToken);
		GameNetwork.Instance.SendCmd(CmdNumber.ReqInitDeviceCmd_C, data);
	}
	
	private void sendClientTokenCode()
	{
		// Debug.LogError("sendClientTokenCode");
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			NotificationManager.InitRemoteNotification(sengMessageLetServerRecordPlatform);//手持设备获得设备token，用于推送，by cjx
		}
		else
		{
			sengMessageLetServerRecordPlatform();
		}
	}
	
	
	
	
	
	/// <summary>
	/// 延迟进入游戏 
	/// </summary>
	private void DelaySendMessageToEnterGame()
	{
		//进入游戏
		//List<SelectPlayerData> data = GlobalRuntimeDataMgr.GetInstance().list_PlayerData;
		if (list_PlayerData.Count > 0)
		{
			for (int i = 0; i < list_PlayerData.Count; i++)
			{
				if (SelectRoleMgr.Instance.GetCurSelectRole() == list_PlayerData[i].role)
				{
					SendMessageToEnterGame(list_PlayerData[i]);
					//SendPlayerSelectMsg(rev.data[i].charid);
				}
			}
		}
	}
	
	
	/// <summary>
	/// 发送信息进入游戏
	/// </summary>
	public void SendMessageToEnterGame(SelectPlayerData playerData)
	{
		Debug.Log("发送已有角色ID，进入游戏。 : role: " + playerData.role + ",level:" + playerData.level + ",name:" + playerData.name);
		LoginMgr.GetInstance().SendPlayerSelectMsg(playerData.charid);
		
		//保存角色ID、等级、名称
		PlayerPrefsManager.SetIntValue(enum_Int_PlayerPrefs.int_玩家上次登陆的角色ID, playerData.role);
		PlayerPrefsManager.SetIntValue(enum_Int_PlayerPrefs.int_玩家上次登陆的角色等级, playerData.level);
		PlayerPrefsManager.SetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆的角色名, playerData.name, true);
		
		CommonSDKPlaform.Instance.EnterGame(Zone.ToString(), playerData.charid, playerData.name);
		CommonSDKPlaform.Instance.UserUpLever(Zone.ToString(), playerData.level, playerData.name);
	}
	
	
	
	/// <summary>
	/// 判断是否已经创建了该角色
	/// </summary>
	/// <param name="role"></param>
	/// <returns></returns>
	public bool IfRoleCreate(RoleClass role)
	{
		//List<SelectPlayerData> data = GlobalRuntimeDataMgr.GetInstance().list_PlayerData;
		foreach (SelectPlayerData item in list_PlayerData)
		{
			if ((RoleClass)item.role == role)
			{
				//Debug.Log("玩家帐号有当前职业");
				return true;
			}
		}
		//Debug.Log("玩家帐号没有当前职业");
		return false;
	}
	
	
	#endregion
	
	
	
	/// <summary>
	/// 记录玩家的帐号登陆情况
	/// </summary>
	public void RecordPlayerAccountAndPassword(string account, string pass)
	{
		if (!PlayerPrefsManager.ContainStringKey(enum_Str_PlayerPrefs.str_玩家上次登陆帐号))
		{
			RecordLoginAccountAndPasswordByIndex(account, pass, 0);
		}
		else
		{
			if (!PlayerPrefsManager.ContainStringKey(enum_Str_PlayerPrefs.str_玩家第二帐号))
			{
				string lastAccount = GetLoginAccountByIndex(0);
				string lastPass = GetLoginPasswordByIndex(0);
				
				if (lastAccount != account)
				{
					RecordLoginAccountAndPasswordByIndex(account, pass, 0);
					RecordLoginAccountAndPasswordByIndex(lastAccount, lastPass, 1);
				}
			}
			else
			{
				string account0 = GetLoginAccountByIndex(0);
				string pass0 = GetLoginPasswordByIndex(0);
				string account1 = GetLoginAccountByIndex(1);
				string pass1 = GetLoginPasswordByIndex(1);
				if (account0 != account)
				{
					RecordLoginAccountAndPasswordByIndex(account, pass, 0);
					RecordLoginAccountAndPasswordByIndex(account0, pass0, 1);
					RecordLoginAccountAndPasswordByIndex(account1, pass1, 2);
				}
			}
		}
		
	}
	
	/// <summary>
	/// 根据索引来记录帐号密码
	/// </summary>
	void RecordLoginAccountAndPasswordByIndex(string account, string pass, int index)
	{
		if (index == 0)
		{
			PlayerPrefsManager.SetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆帐号, account);
			PlayerPrefsManager.SetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆密码, pass);
		}
		else if (index == 1)
		{
			PlayerPrefsManager.SetStringValue(enum_Str_PlayerPrefs.str_玩家第二帐号, account);
			PlayerPrefsManager.SetStringValue(enum_Str_PlayerPrefs.str_玩家第二密码, pass);
		}
		else if (index == 2)
		{
			PlayerPrefsManager.SetStringValue(enum_Str_PlayerPrefs.str_玩家第三帐号, account);
			PlayerPrefsManager.SetStringValue(enum_Str_PlayerPrefs.str_玩家第三密码, pass);
		}
		else
		{
			Debug.LogError("暂时只能储存3个帐号密码，请查看是否出错。by xzp");
		}
	}
	
	///// <summary>
	///// 删除登陆帐号
	///// </summary>
	///// <param name="index"></param>
	//public void DeleteLoginAccountAndPasswordByIndex(int index)
	//{
	//    Debug.Log("删除帐号索引：" + index);
	//    if (index == 0)
	//    {
	//        string account1 = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家第二帐号);
	//        string pass1 = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家第二密码);
	
	//        string account2 = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家第三帐号);
	//        string pass2 = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家第三密码);
	
	//        RecordLoginAccountAndPasswordByIndex(account1, pass1, 0);
	//        RecordLoginAccountAndPasswordByIndex(account2, pass2, 1);
	
	//        PlayerPrefsManager.DeleteStringValue(enum_Str_PlayerPrefs.str_玩家第三帐号);
	//        PlayerPrefsManager.DeleteStringValue(enum_Str_PlayerPrefs.str_玩家第三密码);
	//    }
	//    else if (index == 1)
	//    {
	//        string account2 = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家第三帐号);
	//        string pass2 = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家第三密码);
	
	//        RecordLoginAccountAndPasswordByIndex(account2, pass2, 1);
	
	//        PlayerPrefsManager.DeleteStringValue(enum_Str_PlayerPrefs.str_玩家第三帐号);
	//        PlayerPrefsManager.DeleteStringValue(enum_Str_PlayerPrefs.str_玩家第三密码);
	
	//    }
	//    else if (index == 2)
	//    {
	//        PlayerPrefsManager.DeleteStringValue(enum_Str_PlayerPrefs.str_玩家第三帐号);
	//        PlayerPrefsManager.DeleteStringValue(enum_Str_PlayerPrefs.str_玩家第三密码);
	//    }
	//    else
	//    {
	//        Debug.LogError("暂时只能储存3个帐号密码，请查看是否出错。by xzp");
	//    }
	
	//    UIManager.GetInstance().SendMsgToWndUI(EnWndID.eLogin_Prepare, EnWndMsgID.eLogin_ReflashAccountManager, null);
	//}
	
	/// <summary>
	/// 根据索引获取帐号
	/// </summary>
	string GetLoginAccountByIndex(int index)
	{
		if (index == 0)
		{
			return PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆帐号);
		}
		else if (index == 1)
		{
			return PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家第二帐号);
		}
		else if (index == 2)
		{
			return PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家第三帐号);
		}
		else
		{
			Debug.LogError("暂时只能储存3个帐号密码，请查看是否出错。by xzp");
			return "";
		}
	}
	
	/// <summary>
	/// 根据索引获取密码
	/// </summary>
	string GetLoginPasswordByIndex(int index)
	{
		if (index == 0)
		{
			return PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆密码);
		}
		else if (index == 1)
		{
			return PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家第二密码);
		}
		else if (index == 3)
		{
			return PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家第三密码);
		}
		else
		{
			Debug.LogError("暂时只能储存3个帐号密码，请查看是否出错。by xzp");
			return "";
		}
	}
	
	
	
	/// <summary>
	/// 加载IP等配置
	/// </summary>
	void LoadServerConfig()
	{
		System.IO.FileInfo fileInfo = new System.IO.FileInfo(Application.persistentDataPath + "/ip.ini");
		if (fileInfo.Exists)
		{
			string ipStr = System.IO.File.ReadAllText(Application.persistentDataPath + "/ip.ini");
			if (!string.IsNullOrEmpty(ipStr))
			{
				m_ip = ipStr;
			}
		}
		else
		{
			System.IO.File.WriteAllText(Application.persistentDataPath + "/ip.ini", m_ip);
		}
		
		
		System.IO.FileInfo fileInfo2 = new System.IO.FileInfo(Application.persistentDataPath + "/port.ini");
		if (fileInfo2.Exists)
		{
			string portStr = System.IO.File.ReadAllText(Application.persistentDataPath + "/port.ini");
			if (!string.IsNullOrEmpty(portStr))
			{
				m_port = ushort.Parse(portStr);
			}
		}
		else
		{
			System.IO.File.WriteAllText(Application.persistentDataPath + "/port.ini", m_port.ToString());
		}
		
		System.IO.FileInfo fileInfo3 = new System.IO.FileInfo(Application.persistentDataPath + "/zone.ini");
		if (fileInfo3.Exists)
		{
			string zoneStr = System.IO.File.ReadAllText(Application.persistentDataPath + "/zone.ini");
			if (!string.IsNullOrEmpty(zoneStr))
			{
				m_zone = ushort.Parse(zoneStr);
			}
		}
		else
		{
			System.IO.File.WriteAllText(Application.persistentDataPath + "/zone.ini", m_zone.ToString());
		}
	}
}
