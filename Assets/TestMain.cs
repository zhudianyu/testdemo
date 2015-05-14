using UnityEngine;
using System.Collections;
using ProtoCmd;
using System.Text;

public class RoleData 
{
    public string m_strName;
    public RoleClass role;
}
public class TestMain : MonoBehaviour {
    private static TestMain instance;

    public static TestMain Instance 
    {
        get { return instance; }
    }

    void Awake()
    {
        //PlayerPrefs.DeleteAll();
        instance = this;
        
#if UNITY_ANDROID
        gameObject.AddComponent<CommonSDKPlaform>();
#endif

    }

    private RoleData m_Master = null;
    private RoleData m_Fighter = null;
    private RoleData m_Assasin = null;

    public string m_strCurName = "";

    public bool isCreateNew = false;



    public int moneyTotal = 0;
    public int diamondTotal = 0;


    void Start() 
    {
        playerAccount = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆帐号);
        playerPwd = PlayerPrefsManager.GetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆密码);
    }

    public bool onEnterGameScene(Message message) 
    {
        ChangeMainState(3);



        return true;
    }


    public void ChangeMainState(int state) 
    {
        m_iState = state;
        switch (m_iState) 
        {
            case 2:
                initSelectUserState();
                break;
        }
    }

    public void setRandName(string _name) 
    {
        m_strCurName = _name;
    }

    private void initSelectUserState() 
    {
        m_strCurName = "";
        LoginMgr loginMgr = LoginMgr.GetInstance();
        foreach (SelectPlayerData pData in loginMgr.list_PlayerData)
        {
            RoleClass role = (RoleClass)pData.role;
            if (role == RoleClass.eAssassin)
            {
                m_Assasin = new RoleData();
                m_Assasin.role = role;
                m_Assasin.m_strName = pData.name;
            }
            else if (role == RoleClass.eFighter)
            {
                m_Fighter = new RoleData();
                m_Fighter.role = role;
                m_Fighter.m_strName = pData.name;
            }
            else if (role == RoleClass.eMage)
            {
                m_Master = new RoleData();
                m_Master.role = role;
                m_Master.m_strName = pData.name;
            }
        }

    }


    private int m_iState = 1;   //1:登录界面;2:选择角色界面;3:进入到游戏

    private void checkDestroyLastService()
    {
        if (GameNetwork.m_service != null)
        {
            GameNetwork.Instance.DestoryThread();
            GameNetwork.Instance.UnRegisterCallback();
            GameNetwork.m_service = null;
        }
    }


    public string playerAccount = "";
    public string playerPwd = "";

    void onDrawLoginState() 
    {
        LoginMgr.GetInstance().m_ip = GUI.TextField(new Rect(10, 10, 250, 80), LoginMgr.GetInstance().m_ip.ToString());
        LoginMgr.GetInstance().m_port = ushort.Parse(GUI.TextField(new Rect(300, 10, 280, 80), LoginMgr.GetInstance().m_port.ToString()));
        LoginMgr.GetInstance().m_zone = ushort.Parse(GUI.TextField(new Rect(600, 10, 280, 80), LoginMgr.GetInstance().m_zone.ToString()));
        /*
        GUI.Label(new Rect(170, 70, 150, 30), "帐号");

        GUI.Label(new Rect(170, 120, 150, 30), "密码");

        playerAccount = GUI.TextField(new Rect(10, 70, 150, 30), playerAccount);
        playerPwd = GUI.TextField(new Rect(10, 120, 150, 30), playerPwd);
        */

        if (GUI.Button(new Rect(10, 180, 150, 40), "Login"))
        {
            checkDestroyLastService();

#if UNITY_ANDROID
            CommonSDKPlaform.Instance.Login();
#else
            PlayerPrefsManager.SetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆帐号, playerAccount);
            PlayerPrefsManager.SetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆密码, playerPwd);


            LoginMgr.GetInstance().OldPlayerLogin();
#endif
        }




        if (GUI.Button(new Rect(10, 380, 150, 40), "LoginWeiXin"))
        {
            checkDestroyLastService();

#if UNITY_ANDROID
            CommonSDKPlaform.Instance.LoginWeiXin();
#else
            PlayerPrefsManager.SetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆帐号, playerAccount);
            PlayerPrefsManager.SetStringValue(enum_Str_PlayerPrefs.str_玩家上次登陆密码, playerPwd);


            LoginMgr.GetInstance().OldPlayerLogin();
#endif
        }


    }



    public uint playerID = 0;

    void onDrawSelRole() 
    {
        LoginMgr loginMgr = LoginMgr.GetInstance();
        if (loginMgr.list_PlayerData == null)
            return;

        string masterName = "创建法师";
        string fighterName = "创建战士";
        string assasinName = "创建刺客";
        if (m_Master != null) 
        {
            masterName = m_Master.m_strName;
        }
        if (m_Fighter != null)
        {
            fighterName = m_Fighter.m_strName;
        }

        if (m_Assasin != null)
        {
            assasinName = m_Assasin.m_strName;
        }

        //playerAccount = GUI.TextField(new Rect(10, 70, 150, 30), playerAccount);
        //playerPwd = GUI.TextField(new Rect(10, 120, 150, 30), playerPwd);

        if (GUI.Button(new Rect(10, 70, 250, 80), masterName)) 
        {
            SelectRoleMgr.Instance.SetSelectRole(RoleClass.eMage);
            if (m_Master != null)
            {
                isCreateNew = false;
                //Debug.LogError("enter game");
                m_strCurName = m_Master.m_strName;
                
            }
            else 
            {
                isCreateNew = true;
                //Debug.LogError("create master");
                loginMgr.SendMessageToAskForRandomName(RoleClass.eMage);
            }

        }
        if (GUI.Button(new Rect(10, 220, 250, 80), fighterName))
        {
            SelectRoleMgr.Instance.SetSelectRole(RoleClass.eFighter);
            if (m_Fighter != null)
            {
                isCreateNew = false;
                //Debug.LogError("enter game");
                m_strCurName = m_Fighter.m_strName;
            }
            else
            {
                isCreateNew = true;

                //Debug.LogError("create m_Fighter");
                loginMgr.SendMessageToAskForRandomName(RoleClass.eFighter);
            }
        }
        if (GUI.Button(new Rect(10, 350, 250, 80), assasinName))
        {
            SelectRoleMgr.Instance.SetSelectRole(RoleClass.eAssassin);
            if (m_Assasin != null)
            {
                isCreateNew = false;
                //Debug.LogError("enter game");
                m_strCurName = m_Assasin.m_strName;
            }
            else
            {
                isCreateNew = true;

                //Debug.LogError("create m_Assasin");
                loginMgr.SendMessageToAskForRandomName(RoleClass.eAssassin);
            }
        }


        GUI.Label(new Rect(10, 400, 150, 80), m_strCurName);

        if (isCreateNew)
        {
            if (GUI.Button(new Rect(10, 500, 250, 80), "创建角色"))
            {
                loginMgr.CheckPlayerSelectName(m_strCurName);
            }

        }
        else 
        {
            if (GUI.Button(new Rect(10, 500, 250, 80), "进入游戏"))
            {
                if (SelectRoleMgr.Instance.GetCurSelectRole() == (int)RoleClass.eUnknow)
                {
                    Debug.LogError("请点击选择角色");
                    return;
                }

                SelectPlayerData playerData = null;
                bool b_HaveRole = false;

                foreach(SelectPlayerData pData in loginMgr.list_PlayerData)
                {
                    if (SelectRoleMgr.Instance.GetCurSelectRole() == pData.role)
                    {
                        playerData = pData;
                        b_HaveRole = true;
                    }
                }

                if (b_HaveRole)
                {
                    playerID = (uint)   playerData.charid;
                    LoginMgr.GetInstance().SendMessageToEnterGame(playerData);
                }
                else 
                {
                    Debug.LogError("no find role:" + SelectRoleMgr.Instance.GetCurSelectRole());
                }
            }
        }
    }


    void onDrawInGame() 
    {
        GUI.Label(new Rect(230, 30, 400, 40), "你已经进入游戏了............流程跑完..");

        if (GUI.Button(new Rect(120, 20, 300, 130), "测试一毛钱冲钱!"))
        {
            Debug.Log("OnPromptClick  CommonSDK 以分单位");
            ReqPayForGoodsCommand payInfo = new ReqPayForGoodsCommand();
            payInfo.pay_info = new PayForGoods();
            payInfo.pay_info.player_id = playerID;
            payInfo.pay_info.platform_id = CommonSDKPlaform.Instance.GetLoginData().platform_id;
            payInfo.pay_info.goods_id = 101;
            payInfo.pay_info.goods_num = 1;
            payInfo.pay_info.rmb = 10;
            payInfo.pay_info.virtual_currency = 10;
            payInfo.pay_info.actual_vc = 10;
            payInfo.pay_info.ext_data = Encoding.UTF8.GetBytes("一毛冲值！");
            PayNetWork.GetInstance().SendMessageToRequestOrderID(payInfo);
        }

        if (GUI.Button(new Rect(420, 20, 300, 130), "测试一块钱冲钱!"))
        {
            Debug.Log("OnPromptClick  CommonSDK 以分单位");
            ReqPayForGoodsCommand payInfo = new ReqPayForGoodsCommand();
            payInfo.pay_info = new PayForGoods();
            payInfo.pay_info.player_id = playerID;
            payInfo.pay_info.platform_id = CommonSDKPlaform.Instance.GetLoginData().platform_id;
            payInfo.pay_info.goods_id = 102;
            payInfo.pay_info.goods_num = 1;
            payInfo.pay_info.rmb = 100;
            payInfo.pay_info.virtual_currency = 100;
            payInfo.pay_info.actual_vc = 100;
            payInfo.pay_info.ext_data = Encoding.UTF8.GetBytes("一块冲值！");
            PayNetWork.GetInstance().SendMessageToRequestOrderID(payInfo);
        }


        GUI.Label(new Rect(530, 130, 400, 50), "目前金钱数："+moneyTotal.ToString());
        GUI.Label(new Rect(630, 130, 400, 50), "目前钻石数：" + diamondTotal.ToString());


        if (GUI.Button(new Rect(420, 320, 300, 130), "发送充值后的凭证"))
        {
            CommonSDKPlaform.Instance.SendMessageToTestPayResultOrder();
        }
    }

    void OnGUI() 
    {
        switch (m_iState) 
        {
            case 1:
                onDrawLoginState();
                break;
            case 2:
                onDrawSelRole();
                break;
            case 3:
                onDrawInGame();
                break;
        }
        

    }

}
