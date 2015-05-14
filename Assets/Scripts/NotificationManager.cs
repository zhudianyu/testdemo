using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public enum  NotiCalendarUnit
{
	Era = 2,
	Year = 4,
	Month = 8,
	Day = 16,
	Hour = 32,
	Minute = 64,
	Second = 128,
	Week = 256,
	Weekday = 512,
	WeekdayOrdinal = 1024,
	Quarter = 2048
}

public class NotificationManager : MonoBehaviour {


	public static string m_strToken{ private set; get;}         //设备token
	public delegate void DelegateCallBackGetToken(string token);
	public DelegateCallBackGetToken OnCallBackGetDeviceToken;   //获得设备token后回调，如绑定帐号和设备token用

	bool m_bGetToken=false;
	const string DeviceTokenKey="DeviceToken";
    int getTokenFrameCount = 0;
#if UNITY_ANDROID && !CommonSDK
    static AndroidJavaClass localNotificationPlugins;
	static AndroidJavaClass LocalNotificationPlugins
	{
		get{
			if(localNotificationPlugins==null)
			{
				localNotificationPlugins=new AndroidJavaClass("com.cjx.notification.Notifier");
			}
			return localNotificationPlugins;
		}
	}

	static AndroidJavaClass remoteNotificationPlugins;
	static AndroidJavaClass RemoteNotificationPlugins
	{
		get{
			if(remoteNotificationPlugins==null)
			{
				remoteNotificationPlugins=new AndroidJavaClass("com.cjx.GeTuiPlugins.GeTui");
			}
			return remoteNotificationPlugins;
		}
	}
#endif

    /// <summary>
    /// 初始化并使用本地通知
    /// </summary>
    /// <param name="msgtitle">标题，安卓才显示</param>
    /// <param name="msgContent">内容</param>
    /// <param name="fireTime">触发时间</param>
    /// <param name="notificationID">通知id，相同的id在安卓平台会被覆盖</param>
    /// <param name="isRepeate">是否反复触发</param>
    /// <param name="repeateInteval">触发周期</param>
	public static void InitLocationNotification(string msgtitle,string msgContent ,DateTime fireTime,int notificationID,bool isRepeate=false,NotiCalendarUnit repeateInteval=NotiCalendarUnit.Day)
    {
#if UNITY_IPHONE&& !CommonSDK
		LocalNotification localNotification = new LocalNotification ();
		localNotification.fireDate = fireTime;
		localNotification.alertBody = msgContent;
		localNotification.hasAction = true;
		localNotification.applicationIconBadgeNumber=1;
		localNotification.soundName = LocalNotification.defaultSoundName;
		if(isRepeate)
		{
			localNotification.repeatCalendar=CalendarIdentifier.ChineseCalendar;
			localNotification.repeatInterval=(CalendarUnit)((int)repeateInteval);
		}
		NotificationServices.ScheduleLocalNotification (localNotification);
#elif UNITY_ANDROID&& !CommonSDK
        GetDurationSecondTime(DateTime.Now,fireTime);
		GetIntevalSeconds( repeateInteval);
		LocalNotificationPlugins.CallStatic("scheduleLocalNotification",GetDurationSecondTime(DateTime.Now,fireTime),msgtitle,msgContent,msgContent,notificationID,isRepeate,GetIntevalSeconds(repeateInteval));
#endif
    }

    /// <summary>
    /// 处理本地消息
    /// </summary>
	public static void AnalyseLocationNotification()
    {
#if UNITY_IPHONE&& !CommonSDK
		Debug.LogError (NotificationServices.localNotifications.Length);
		for(int a=0;a<NotificationServices.localNotifications.Length;++a)
		{
			Debug.Log("b:"+NotificationServices.localNotifications[a].userInfo.Count);
			foreach(string str in NotificationServices.localNotifications[a].userInfo.Keys)
			{
				Debug.Log("key: "+str);
			}
			foreach(string str in NotificationServices.localNotifications[a].userInfo.Values)
			{
				Debug.Log("value: "+str);
			}
		}
#endif
    }

    /// <summary>
    /// 处理远程消息
    /// </summary>
	public static void AnalyseRemoteNotification()
    {
#if UNITY_IPHONE&& !CommonSDK
		Debug.LogError (NotificationServices.remoteNotifications.Length);
		for(int a=0;a<NotificationServices.remoteNotifications.Length;++a)
		{
			Debug.Log("b:"+NotificationServices.remoteNotifications[a].userInfo.Count);
			foreach(string str in NotificationServices.remoteNotifications[a].userInfo.Keys)
			{
				Debug.Log("key: "+str);
			}
			foreach(string str in NotificationServices.remoteNotifications[a].userInfo.Values)
			{
				Debug.Log("value: "+str);
			}
		}
#endif
    }

    /// <summary>
    /// 初始化并使用远程消息
    /// </summary>
    /// <param name="onGetToken"></param>
	public static void InitRemoteNotification(DelegateCallBackGetToken onGetToken=null)
    {

#if UNITY_IPHONE&& !CommonSDK
		NotificationServices.RegisterForRemoteNotificationTypes(RemoteNotificationType.Alert | 
		                                                        RemoteNotificationType.Badge | 
		                                                        RemoteNotificationType.Sound);
		m_strToken = NotificationManager.GetLocalStoreDeviceToken ();
		
		if(string.IsNullOrEmpty(m_strToken))
		{
			GameObject go=new GameObject("NotificationHelp");
			NotificationManager notification=go.AddComponent<NotificationManager>();
			notification.OnCallBackGetDeviceToken=onGetToken;
		}else
			onGetToken(m_strToken);
#elif UNITY_ANDROID&& !CommonSDK
        GameObject go=new GameObject("NotificationHelp");
		NotificationManager notification=go.AddComponent<NotificationManager>();
		notification.OnCallBackGetDeviceToken=onGetToken;
		RemoteNotificationPlugins.CallStatic("InitRemoteNotification",go.name);
#endif
    }

    /// <summary>
    /// 注销并停止使用所有消息
    /// </summary>
	public static void CancelAllNotification()
	{
		NotificationManager.CancelLocalNotification ();
		NotificationManager.CancelRemoteNotification ();
	}

    /// <summary>
    /// 注销并停止使用本地消息
    /// </summary>
	public static void CancelLocalNotification()
    {
#if UNITY_IPHONE&& !CommonSDK
		LocalNotification local=new LocalNotification();
		local.applicationIconBadgeNumber=-1;
		NotificationServices.PresentLocalNotificationNow(local);
		NotificationServices.CancelAllLocalNotifications();
		NotificationServices.ClearLocalNotifications();
#elif UNITY_ANDROID&& !CommonSDK
        for (int a=0;a<100;++a)
		{
			LocalNotificationPlugins.CallStatic("cancelLocalNotification",a);
		}
#endif
    }

    /// <summary>
    /// 注销并停止使用远程消息
    /// </summary>
	public static void CancelRemoteNotification()
    {
#if UNITY_IPHONE&& !CommonSDK
		LocalNotification local=new LocalNotification();
		local.applicationIconBadgeNumber=-1;
		NotificationServices.PresentLocalNotificationNow(local);
		NotificationServices.ClearRemoteNotifications();
		NotificationServices.UnregisterForRemoteNotifications ();
#elif UNITY_ANDROID&& !CommonSDK
        remoteNotificationPlugins.CallStatic("CancelRemoteNotification");	
#endif
    }

    /// <summary>
    /// 计算循环周期的秒数
    /// </summary>
    /// <param name="repeateInteval"></param>
    /// <returns></returns>
    public static int GetIntevalSeconds(NotiCalendarUnit repeateInteval)
    {
        int seconds = -1;
        DateTime time1 = DateTime.Now;
        DateTime time2 = DateTime.Now;
        switch (repeateInteval)
        {
            case NotiCalendarUnit.Day:
                time2 = time2.AddDays(1);
                break;
            case NotiCalendarUnit.Hour:
                time2 = time2.AddHours(1);
                break;
            case NotiCalendarUnit.Minute:
                time2 = time2.AddMinutes(1);
                break;
            case NotiCalendarUnit.Second:
                time2 = time2.AddSeconds(1);
                break;
            case NotiCalendarUnit.Week:
                time2 = time2.AddDays(7);
                break;
            case NotiCalendarUnit.Month:
                time2 = time2.AddDays(30);
                break;
            default:
                time2 = time2.AddDays(1);
                break;
        }
        TimeSpan ts1 = new TimeSpan(time1.Ticks);
        TimeSpan ts2 = new TimeSpan(time2.Ticks);
        TimeSpan ts3 = ts1.Subtract(ts2).Duration();
        seconds = (int)ts3.TotalSeconds;//秒数
        return seconds;
    }

    /// <summary>
    /// 计算下一个设置时间的dateTime
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="minute"></param>
    /// <param name="second"></param>
    /// <returns></returns>
	public static DateTime GetNextDateTime(int hour,int minute=0,int second=0)
	{
		DateTime time=DateTime.Now;
		time=time.AddMilliseconds(100-time.Millisecond);

		int deltaSecond=second-time.Second;
		if(deltaSecond<0)
			deltaSecond+=60;
		time=time.AddSeconds(deltaSecond);

		int deltaMinute=minute-time.Minute;
		if(deltaMinute<0)
			deltaMinute+=60;
		time=time.AddMinutes(deltaMinute);

		
		int deltaHour=hour-time.Hour;
		if(deltaHour<0)
			deltaHour+=24;
		time=time.AddHours(deltaHour);
		return time;
	}


    /// <summary>
    /// 计算两个DateTime的秒数时间差
    /// </summary>
    /// <param name="time1"></param>
    /// <param name="time2"></param>
    /// <returns></returns>
	static int GetDurationSecondTime(DateTime time1,DateTime time2)
	{
		TimeSpan ts1 = new TimeSpan(time1.Ticks);
		TimeSpan ts2 = new TimeSpan(time2.Ticks);
		TimeSpan ts3 = ts1.Subtract(ts2).Duration();
		return (int)ts3.TotalSeconds;
	}

    /// <summary>
    /// 获得设备token
    /// </summary>
    /// <returns></returns>
	static string GetLocalStoreDeviceToken()
	{
		string token=null;
		if(PlayerPrefs.HasKey(NotificationManager.DeviceTokenKey))
		{
			token=PlayerPrefs.GetString(NotificationManager.DeviceTokenKey);
		}
		return token;
	}

    /// <summary>
    /// 保存设备token到本地
    /// </summary>
    /// <param name="token"></param>
	static void SetLocalStoreDeviceToken(string token)
	{
		PlayerPrefs.SetString(NotificationManager.DeviceTokenKey,token);
	}

   

    /// <summary>
    /// 获得设备token后回调事件
    /// </summary>
    /// <param name="token"></param>
	void OnGetDeviceToken(string token)
	{
		if(OnCallBackGetDeviceToken!=null)
			OnCallBackGetDeviceToken(token);
		Destroy (gameObject);
	}

    /// <summary>
    /// ios平台异步获得设备token
    /// </summary>
	void CheckGetDeviceToken()
	{
		if(!m_bGetToken)
        {
#if UNITY_IPHONE&& !CommonSDK
            ++getTokenFrameCount;
            if(getTokenFrameCount>600)
            {
               Destroy (gameObject); 
            }
			byte[] token= NotificationServices.deviceToken;
			if(token!=null)
			{
				//m_strToken = "%" + System.BitConverter.ToString(token).Replace('-', '%');
				m_strToken = System.BitConverter.ToString(token).Replace("-", "");
				Debug.LogError("m_strToken: "+m_strToken);
				m_bGetToken=true;
				NotificationManager.SetLocalStoreDeviceToken(m_strToken);
				if(OnCallBackGetDeviceToken!=null)
					OnCallBackGetDeviceToken(m_strToken);//回调得到token处理
				Destroy (gameObject);
			}
#endif
        }

	}

    void Start()
    {
       DontDestroyOnLoad(gameObject);//避免换场景的时候删了
    }

	// Update is called once per frame
	void Update ()
	{
		CheckGetDeviceToken ();
	}


}
