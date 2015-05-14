using UnityEngine;
using System.Collections;
using ProtoCmd;
using System.Collections.Generic;

public class CommonSDKPlaform:MonoBehaviour
{
	class loginResult {        
		public string token = ""; //session id
		public string uid = "";   //username or user id
        public uint pid = 0;    //platform id
        public string account = "";//for zqgame sdk
        public string szLoginAcccount = ""; //login return
        public string szLoginSession = "";
        public string szLoginDataEx = "";
        public string uiLoginPlatUserID = "";
    }
	
	
	public static CommonSDKPlaform Instance { private set; get; }
	
	#if CommonSDK
	//mono初始化会直接调用这个来初始化sdk
	AndroidJavaObject mainApplication = null;
	AndroidJavaObject sdkBase = null;
	AndroidJavaObject sdkObject = null;
	AndroidJavaObject gameActivity = null;
	#endif
	//static string tempText = "msg";
	static string sendText = "edit";
	
	loginResult m_loginResult = new loginResult();//登陆数据
	
	
	//mono初始化会直接调用这个来初始化sdk
	void Awake()
	{        
		#if CommonSDK
		mainApplication = new AndroidJavaClass("com.talkingsdk.MainApplication").CallStatic<AndroidJavaObject>("getInstance");
		sdkBase = mainApplication.Call<AndroidJavaObject>("getSdkInstance");
		sdkBase.Call("setUnityGameObject", gameObject.name);
		Instance = this;
		
		Debug.LogError("Awake:" + gameObject.name);
		#endif
	}
	
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			//Debug.LogError("Input.GetKeyDown(KeyCode.Escape)");
			KeyBack();
		}
		/*
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Debug.LogError("Input.GetKeyUp(KeyCode.Escape)");
            //KeyBack();
        }


        if (Input.GetKey(KeyCode.Home))
        {
            Debug.LogError("KeyCode.Home");
            KeyBack();
        }
         */
	} 
	
	/// <summary>
	/// 登陆接口
	/// </summary>
	public void Login()
	{
		Debug.LogError("go to login....");
		#if CommonSDK
		sdkBase.Call("login");
		#endif
	}
	
	/// <summary>
	/// 微信登陆接口
	/// </summary>
	public void LoginWeiXin()
	{
		Debug.LogError("go to login....");
		#if CommonSDK
		sdkBase.Call("loginWeiXin");
		#endif
	}
    public void CreateRoleToSDK(string playerName)
    { 
#if CommonSDK
        if (m_loginResult.pid == 34)
        {
            sdkBase.Call("createRole", name);
            Debug.LogError("go to UserUpLever  setServerNo:" + LoginMgr.GetInstance().m_zone.ToString() + "   setLevel:" + 1+ "   setRoleName:" + playerName);        
            AndroidJavaObject playerData = new AndroidJavaObject("com.talkingsdk.models.PlayerData");
            playerData.Call("setServerNo", LoginMgr.GetInstance().m_zone.ToString());
            playerData.Call("setLevel", (int)1);
            playerData.Call("setRoleName", playerName);
            sdkBase.Call("createRole", playerData);      
        }
#endif
    }
	/// <summary>
	/// 支付接口
	/// </summary>
	public void Pay(RspPayForGoodsCommand payGoodsData)
	{
		#if CommonSDK
		AndroidJavaObject payData = new AndroidJavaObject("com.talkingsdk.models.PayData");
		AndroidJavaObject hashmap = new AndroidJavaObject("java.util.HashMap");
		System.IntPtr methodPut = AndroidJNIHelper.GetMethodID(hashmap.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
		
		Debug.Log("m_loginResult.uid :" + m_loginResult.uid);
		
		
		Dictionary<string, string> dict = new Dictionary<string, string>();
		dict.Add("UserId", m_loginResult.uid); //uid
		dict.Add("UserBalance", "0"); //用户余额
		dict.Add("UserGamerVip", "1"); //vip 等级
		dict.Add("UserLevel", "10"); //角色等级
		dict.Add("UserPartyName","testName"); //工会，帮派
		dict.Add("UserRoleName", "Underworld"); //角色名称
		dict.Add("UserRoleId", "123456"); //角色id
		dict.Add("UserServerName", "Underworld"); //所在服务器名称

        dict.Add("LoginAccount", m_loginResult.szLoginAcccount);
        dict.Add("LoginDataEx", m_loginResult.szLoginDataEx);
        dict.Add("LoginSession", m_loginResult.szLoginSession);
		dict.Add("AccessKey", System.Text.Encoding.UTF8.GetString(payGoodsData.pay_info.ext_data)); //拓展字段
		dict.Add("OutOrderID", System.Text.Encoding.UTF8.GetString(payGoodsData.pay_info.out_order)); //平台订单号
		dict.Add("NoticeUrl", System.Text.Encoding.UTF8.GetString(payGoodsData.pay_info.notice_url)); //支付回调地址
		dict.Add("UserServerId", LoginMgr.GetInstance().m_zone.ToString()); 
		dict.Add("GameMoneyAmount", (payGoodsData.pay_info.rmb /10).ToString());
		dict.Add("GameMoneyName", "钻石");
		dict.Add("PayType", "T_Store");
		
		object[] args = new object[2];
		foreach (KeyValuePair<string, string> kvp in dict)
		{
			AndroidJavaObject k = new AndroidJavaObject("java.lang.String", kvp.Key);
			AndroidJavaObject v = new AndroidJavaObject("java.lang.String", kvp.Value);
			args[0] = k;
			args[1] = v;
			AndroidJNI.CallObjectMethod(hashmap.GetRawObject(), methodPut, AndroidJNIHelper.CreateJNIArgArray(args));
		}
		
		
		Debug.LogError("go to pay....  setMyOrderId " +System.Text.Encoding.UTF8.GetString(payGoodsData.pay_info.inner_order)+
		               " payGoodsData.pay_info.out_order: " + System.Text.Encoding.UTF8.GetString(payGoodsData.pay_info.out_order) +
		               " rmb: " + payGoodsData.pay_info.rmb +
		               " setProductCount :" + payGoodsData.pay_info.goods_num+
		               " setProductId :"+ payGoodsData.pay_info.goods_id+
		               " setProductName :" + getProductionNameByID((int)payGoodsData.pay_info.goods_id) +
		               " setSubmitTime:" + System.Text.Encoding.UTF8.GetString(payGoodsData.pay_info.inner_order).Substring(0, 14) +
		               " extData:" + System.Text.Encoding.UTF8.GetString(payGoodsData.pay_info.ext_data)
		               );
		
		//string 类型
		payData.Call("setMyOrderId", System.Text.Encoding.UTF8.GetString(payGoodsData.pay_info.inner_order));
        payData.Call("setProductId", payGoodsData.pay_info.goods_id.ToString());
        payData.Call("setSubmitTime", System.Text.Encoding.UTF8.GetString(payGoodsData.pay_info.inner_order).Substring(0, 14));
        payData.Call("setDescription",getProductionNameByID((int)payGoodsData.pay_info.goods_id));
        payData.Call("setProductName", getProductionNameByID((int)payGoodsData.pay_info.goods_id));

        //int 类型 ,SDK 这边统一以分为单位
        payData.Call("setProductRealPrice", ((int)payGoodsData.pay_info.rmb));
        payData.Call("setProductIdealPrice",  ((int)payGoodsData.pay_info.rmb));
        payData.Call("setProductCount", (int)payGoodsData.pay_info.goods_num);
        payData.Call("setEx", hashmap);
      
        sdkBase.Call("pay", payData);
        
        #endif
    }


    string getProductionNameByID(int id)
    {
     
        return "Recharge";
    }

    public void SetUserID(string id)
    {
#if CommonSDK
        Debug.Log("CommonSDK Platform  SetUserID:"+id.ToString());
        m_loginResult.uid = id;
#endif
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="jsonData"></param>
    public void SetLoginData(string jsonData)
    {
        /*
        Result :{
        "nPlatformID" : 0,
        "szAcccount" : "whtest3",
        "szDataEx" : "whtest3",
        "szSession" : "",
        "uiPlatUserID" : 0
        }
        */

        Debug.LogError("Result :" + jsonData);

        JsonData jd = JsonMapper.ToObject(UnderWorld.Utils.JSON.JsonEncode(UnderWorld.Utils.JSON.JsonDecode(jsonData)));

        /*
        if (((IDictionary)jd).Contains("nPlatformID"))
        {
            Debug.LogError("nPlatformID :" + (jd["nPlatformID"]).ToString());
        }
        */


        if (((IDictionary)jd).Contains("szAcccount"))
        {
            Debug.LogError("szAcccount :" + (jd["szAcccount"]).ToString());
            m_loginResult.szLoginAcccount = (jd["szAcccount"]).ToString();            
        }

        if (((IDictionary)jd).Contains("szDataEx"))
        {
            Debug.LogError("szDataEx :" + (jd["szDataEx"]).ToString());
            m_loginResult.szLoginDataEx = (jd["szDataEx"]).ToString();  
        }

        if (((IDictionary)jd).Contains("szSession"))
        {
            Debug.LogError("szSession :" + (jd["szSession"]).ToString());
            m_loginResult.szLoginSession = (jd["szSession"]).ToString();  
        }

        if (((IDictionary)jd).Contains("uiPlatUserID"))
        {
            Debug.LogError("uiPlatUserID :" + (jd["uiPlatUserID"]).ToString());
            m_loginResult.uiLoginPlatUserID = (jd["uiPlatUserID"]).ToString();  
        }


    }
	/// <summary>
	/// 显示Tool Bar
	/// </summary>
	public void ShowToolBar()
	{
		#if CommonSDK
		Debug.LogError("go to showToolBar....");
		sdkBase.Call("showToolBar");
		#endif
	}
	
	/// <summary>
	/// 隐藏Tool Bar
	/// </summary>
	public void DestroyToolBar()
	{
		#if CommonSDK
		Debug.LogError("go to destroyToolBar....");
		sdkBase.Call("destroyToolBar");
		#endif
	}
	
	/// <summary>
	/// 显示用户中心
	/// </summary>
	public void ShowUserCenter()
	{
		#if CommonSDK
		Debug.LogError("go to showUserCenter....");
		sdkBase.Call("showUserCenter");
		#endif
	}
	
	
	/// <summary>
	/// 切换帐号
	/// </summary>
	public void ChangeAccount()
	{
		#if CommonSDK
		Debug.LogError("go to change account....");
		sdkBase.Call("changeAccount");
		//GameMain.Instance.LogoutAccount();
		#endif
	}
	
	/// <summary>
	/// 登出
	/// </summary>
	public void Logout()
	{
		#if CommonSDK
		Debug.LogError("go to logout....");
		sdkBase.Call("logout");
		#endif
	}
	
	/// <summary>
	/// 进入游戏回调
	/// </summary>
	/// <param name="serverNo"></param>
	/// <param name="playerID"></param>
	/// <param name="playerName"></param>
	public void EnterGame(string serverNo, int playerID, string playerName)
	{
		
		Debug.Log("go to EnterGam  serverNo: " + serverNo + "   setRoleId:" + playerID.ToString() + "   playerName:" + playerName);
		#if CommonSDK
		AndroidJavaObject playerData = new AndroidJavaObject("com.talkingsdk.models.PlayerData");
		playerData.Call("setServerNo", serverNo);
		playerData.Call("setRoleId", playerID);
		playerData.Call("setRoleName", playerName);
		sdkBase.Call("enterGame", playerData);
		#endif
	}
	
	/// <summary>
	/// 玩家升级
	/// </summary>
	public void UserUpLever(string serverNo, int level, string playerName)
	{
		Debug.LogError("go to UserUpLever  setServerNo:" + serverNo + "   setLevel:" + level.ToString() + "   setRoleName:" + playerName);
		#if CommonSDK
		AndroidJavaObject playerData = new AndroidJavaObject("com.talkingsdk.models.PlayerData");
		playerData.Call("setServerNo", serverNo);
		playerData.Call("setLevel", level);
		playerData.Call("setRoleName", playerName);
		sdkBase.Call("userUpLevel", playerData);
		#endif
	}
	
	
	/// <summary>
	/// 设置分数
	/// </summary>
	public void SetRankScore(string sorce, string rank)
	{
		//目前rank字段是没用的，暂时先传个OK过去，以后游泳的时候再打开
		Debug.LogError("SetRankScore:" + sorce + "  rank:" + rank);
		#if CommonSDK
		sdkBase.Call("uploadScore", sorce, rank);
		#endif
	}
	
	
	/// <summary>
	/// 销毁SDK进程
	/// </summary>
	public void DestroyActivity()
	{
		Debug.LogError("DestroyActivity");
		#if CommonSDK
		sdkBase.Call("onActivityDestroy");
		#endif
	}
	
	/// <summary>
	/// 监听返回事件
	/// </summary>
	public void KeyBack()
	{
		Debug.LogError("KeyBack");
		#if CommonSDK
		sdkBase.Call("onKeyBack");
		#endif
	}
	
	
	/// <summary>
	/// 获取登陆数据
	/// </summary>
	/// <returns></returns>
	public PlayerRequestLoginClientCmd GetLoginData()
	{
		PlayerRequestLoginClientCmd cmd = new PlayerRequestLoginClientCmd();
		cmd.platform_id =m_loginResult.pid;
		cmd.app_loginkey = m_loginResult.token;
		cmd.app_uid = m_loginResult.uid;
		cmd.account = m_loginResult.uid;
		cmd.internal_test = 1;
		cmd.session = System.Text.Encoding.UTF8.GetBytes(m_loginResult.token);
		return cmd;
	}
	
	
	/// <summary>
	/// SDK初始化完成
	/// </summary>
	/// <param name="test"></param>
	void OnInitComplete(string result)
	{
		Debug.LogError("OnInitComplete:" + result);
		
		JsonData jd = JsonMapper.ToObject(UnderWorld.Utils.JSON.JsonEncode(UnderWorld.Utils.JSON.JsonDecode(result)));
		m_loginResult.pid = uint.Parse((string)jd["PlatformId"]);
		//UmengPlatformHelp.UmengInit(m_loginResult.pid.ToString());//初始化友盟统计平台
		
	}
	
	/// <summary>
	/// 回调 - 登陆
	/// </summary>
	/// <param name="result"></param>
	void OnLoginResult(string result)
	{
		Debug.LogError("OnLoginSuccess:" + result);
		
		JsonData jd = JsonMapper.ToObject(UnderWorld.Utils.JSON.JsonEncode(UnderWorld.Utils.JSON.JsonDecode(result)));
		
		/*
        m_loginResult.pid = uint.Parse((string)jd["pid"]);
        m_loginResult.token = (string)jd["token"];
        m_loginResult.uid = (string)jd["uid"];
         */
		
		m_loginResult.token = (string)jd["SessionId"];
		string uid = (string)jd["UserId"];
		if (uid == "")
		{
			uid = (string)jd["UserName"];
		}
		m_loginResult.uid = uid;
		
		
		//如果有额外的平台ID，则给它重新赋值
		if (((IDictionary)jd).Contains("Ext"))
		{
			JsonData ext = jd["Ext"];
			if (((IDictionary)ext).Contains("PlatformId"))
			{
				Debug.Log("PlatformId Reset :" + (string)ext["PlatformId"]);
				m_loginResult.pid = uint.Parse((string)ext["PlatformId"]);
			}
		}
		
		LoginMgr.GetInstance().ThirdlyPlatformLogin();
		
		Debug.LogError("UmengPlatformHelp.UmengInit ID:" + m_loginResult.pid.ToString());
	}
	
	
    string payResult = "";
    string orderID = "";

    void OnPayResult(string result)
    {
        Debug.LogError("OnPayResult:" + result);
        JsonData jd = JsonMapper.ToObject(UnderWorld.Utils.JSON.JsonEncode(UnderWorld.Utils.JSON.JsonDecode(result)));

        if (((IDictionary)jd).Contains("MyOrderId"))
        {
            orderID = (string)jd["MyOrderId"];
        }

        //如果有额外的平台ID，则给它重新赋值
        if (((IDictionary)jd).Contains("Ext"))
        {
            JsonData ext = jd["Ext"];
            if (((IDictionary)ext).Contains("PayResult"))
            {
                Debug.Log("PlatformId Reset :" + (string)ext["PayResult"]);
                payResult = (string)ext["PayResult"];
            }
        }

        PayNetWork.GetInstance().SendMessagePayResultOrder(m_loginResult.pid, orderID, payResult);

    }


    /// <summary>
    /// 发送充值凭证，这个只用于验证测试。
    /// </summary>
    public void SendMessageToTestPayResultOrder()
    {
        if (orderID != "")
        {
            PayNetWork.GetInstance().SendMessagePayResultOrder(m_loginResult.pid, orderID, payResult);
        }
        else
        {
            Debug.Log("U have not recharge , please check again !");
        }
    }
    
    void OnLogoutResult(string result)
    {
        Debug.LogError("OnLogoutResult: " + result);
    }

    void OnChangeAccountResult(string result)
    {
        Debug.Log("OnChangeAccount:" + result);
        //GameMain.Instance.LogoutAccount();
    }


    void OnDestroy()
    {
        Debug.Log("CommonSDKPlatform OnDestroy");
        //Debug.LogError("GameMain destroy");
        //if (GameNetwork.Instance != null) 
        //{
        //    GameNetwork.Instance.OnGameDestroy();
        //}
        
#if CommonSDK
       DestroyActivity();
#endif

    }
}
