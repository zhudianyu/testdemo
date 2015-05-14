using UnityEngine;

public enum enum_Str_PlayerPrefs
{
    //游戏登陆部分
    str_玩家上次登陆帐号,
    str_玩家上次登陆密码,

    str_玩家上次登陆的角色名,

    str_玩家准备注册的帐号,
    str_玩家准备注册的密码,

    str_玩家准备登陆的帐号,
    str_玩家准备登陆的密码,

    str_玩家第二帐号,
    str_玩家第二密码,

    str_玩家第三帐号,
    str_玩家第三密码,
}


public enum enum_Int_PlayerPrefs
{
    //游戏登陆部分
    int_玩家上次登陆的角色ID,
    int_玩家上次登陆的角色等级,

	// 角色操作
	int_玩家操作方式,

    // 5 低 10 高
    int_同屏显示人数,

    //音乐的开关 0:关闭,1:开启 lhc
    int_音乐开关,

    //音效的开关 0:关闭,1:开启 lhc 
    int_音效开关,

    //0:中;1:高
    int_画面,

    //0:关,1:开
    int_特效,

    //0:关,1:开
    int_战时特效动态优化,

    //0：近距离摄像机 1:远距离摄像机 
    int_摄像机远近,

    //公告版本
    int_Notice_Version,

    //用设备id登录
    int_UseDeviceIdLogin,

    //针对于总资源的版本号
    int_当前资源版本号,   
 
    //在装备选择的品质
    int_equ_Quliaty_1    =  15,
    int_equ_Quliaty_2    =  16,
    int_equ_Quliaty_3    =  17,
    int_equ_Quliaty_4    =  18,


}


/// <summary>
/// 储存客户端预制信息的文件
/// </summary>
public static class PlayerPrefsManager  {

    public const string KEY_USER_FASHION = "user_fashion";
    public const string KYE_USER_WINGS = "user_wings";

    public static void SetUserValue(string key, int userId, int _value) 
    {
        string finalKey = userId + "_" + key;
        PlayerPrefs.SetInt(finalKey, _value);

    }
    public static int GetUserValue(string key, int userId, int defaultValue) 
    {
        string finalKey = userId + "_" + key;
        return PlayerPrefs.GetInt(finalKey, defaultValue);
    }

    public static string GetUserValue(string key, int userId, string defaultValue) 
    {
        string finalKey = userId + "_" + key;
        return PlayerPrefs.GetString(finalKey, defaultValue);

    }

    public static void SetUserValue(string key, int userId, string _value) 
    {
        string finalKey = userId + "_" + key;
        PlayerPrefs.SetString(finalKey, _value);
    }



    /// <summary>
    /// 判断是否存在这个KEY
    /// </summary>
    public static bool ContainIntKey(enum_Int_PlayerPrefs key)
    {
        return PlayerPrefs.HasKey(key.ToString());
    }

    /// <summary>
    /// 判断是否存在这个KEY
    /// </summary>
    public static bool ContainStringKey(enum_Str_PlayerPrefs key)
    {
        return PlayerPrefs.HasKey(key.ToString()) && GetStringValue(key) != "";
    }
    
    /// <summary>
    /// 设置INT值，设置后保存，防止程序损坏丢失。
    /// </summary>
    public static void SetIntValue(enum_Int_PlayerPrefs intPref, int value)
    {
        PlayerPrefs.SetInt(intPref.ToString(), value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 设置STRING值，设置后保存，防止程序损坏丢失。
    /// </summary>
    public static void SetStringValue(enum_Str_PlayerPrefs strPref, string value, bool saveBytes = false)
    {

        if (!saveBytes)
        {
            PlayerPrefs.SetString(strPref.ToString(), value);
        }
        else 
        {
            string saveStr = EncodingUTF8Str(value);
            PlayerPrefs.SetString(strPref.ToString(), saveStr);
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 把字符串转换成byte[] 类型,","分割保存成字符串返回
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string EncodingUTF8Str(string str) 
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(str);
        string saveStr = "";
        int len = data.Length;
        for (int i = 0; i < len; i++) 
        {
            saveStr += i == 0 ? data[i].ToString() : "," + data[i];
        }
        return saveStr;
    }

    /// <summary>
    /// 把传入的格式字符串,按","分割符解析成对应的byte[] 类型,再转成字符串返回
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string DecodeUTF8Str(string str) 
    {
        if (str.IndexOf(',') == -1) return str;

        string[] data = str.Split(',');
        byte[] byteData = new byte[data.Length];
        for (int i = 0; i < data.Length; i++) 
        {
            byteData[i] = byte.Parse(data[i]);
        }
        return System.Text.Encoding.UTF8.GetString(byteData);
    }



    /// <summary>
    /// 获取INT值
    /// </summary>
    public static int GetIntValue(enum_Int_PlayerPrefs intPref)
    {
        return PlayerPrefs.GetInt(intPref.ToString());
    }

    /// <summary>
    /// 获取STRING值
    /// </summary>
    public static string GetStringValue(enum_Str_PlayerPrefs strPref, bool byBytes = false)
    {
        if (!byBytes)
        {
            return PlayerPrefs.GetString(strPref.ToString(),"");
        }
        else 
        {
            string byteStr = PlayerPrefs.GetString(strPref.ToString(), "");
            return DecodeUTF8Str(byteStr);
        }
        
    }

    public static int GetIntValue(string m_strKey) 
    {
        return PlayerPrefs.GetInt(m_strKey, -1);
    }

    public static void SetIntValue(string m_strKey, int iValue) 
    {
        PlayerPrefs.SetInt(m_strKey, iValue);
    }


    /// <summary>
    /// 删除一个string值
    /// </summary>
    public static void DeleteStringValue(enum_Str_PlayerPrefs strPref)
    {
        PlayerPrefs.DeleteKey(strPref.ToString());
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 删除一个int值
    /// </summary>
    public static void DeleteIntValue(enum_Int_PlayerPrefs intPref)
    {
        PlayerPrefs.DeleteKey(intPref.ToString());
        PlayerPrefs.Save();
    }
}
