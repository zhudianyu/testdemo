using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MonoXml;
using System.Security;

public class ZoneData
{
    public int nZoneID;
    public string strZoneName;
    //public int nRegionID;
    //public string strRegionName;
    public byte byZoneStatus;   //1表示顺畅，2表示繁忙，0表示维护
    public byte byZoneFlag;     //1表示新服，2表示推荐服务器，0表示普通服务器（不显示）

    public string m_strIP;      //ip地址
    public int m_iPort;      //端口
    public bool m_bShowGuestEntrance;
    public int ZoneRange;   // 所在页面的id
    public int ServerId;    // 服务器 id
}

/// <summary>
/// 服务器切页列表 by lsj
/// </summary>
public class ZonePage
{
    public int id;
    public string name;
    public List<ZoneData> server_list = new List<ZoneData>();
}

/// <summary>
/// 服务器config，用于保存对应服务器的角色数量 by lsj
/// </summary>
public class ServerConfig {
	public int nServerId;
	public int nZonePageId;
	public int nChartNum;
	public ServerConfig() { }
}

public class ServerConfigFile
{
	public List<ServerConfig> m_configList = new List<ServerConfig>();
	public ServerConfigFile() { }
}

public class ServiceListData
{

    private static ServiceListData m_Instance;

    public static ServiceListData GetInstance()
    {
        if (m_Instance == null)
            m_Instance = new ServiceListData();
        return m_Instance;
    }

    public List<ZoneData> m_lstService = new List<ZoneData>();
    public List<ZonePage> m_ZonePageList = new List<ZonePage>();


    public string m_strSdkUrl;
    public ZoneData GetZoneData(string ip, int port, int zoneId)
    {
        int count = m_lstService.Count;
        for (int i = 0; i < count; i++)
        {
            if (m_lstService[i].m_strIP.Equals(ip) &&
                m_lstService[i].nZoneID == zoneId &&
                m_lstService[i].m_iPort == port)
            {
                return m_lstService[i];
            }
        }
        return null;
    }

    public ZoneData GetNewService()
    {
        int count = m_lstService.Count;
        for (int i = 0; i < count; i++)
        {
            if (m_lstService[i].byZoneFlag == 1)
            {
                return m_lstService[i];
            }
        }
        return null;

    }


    public void AnalysisServiceXMLData(string data)
    {
        if (m_lstService.Count > 0)
            m_lstService.Clear();
		if (m_ZonePageList.Count > 0)
		{
			m_ZonePageList.Clear();
		}
		//Debug.LogError(data);
		// 如果第一位不为<，则去掉
		if (data.Length > 0) {
			if (data[0] != '<') {
				data = data.Remove(0, 1);
				Debug.LogError("下发的serverList格式有误！");
			}
		}
		//Debug.LogError(data);
        SecurityParser xmlParser = new SecurityParser();
        xmlParser.LoadXml(data);
        SecurityElement xmlElem = xmlParser.ToXml();
        foreach (SecurityElement child in xmlElem.Children)
        {
            if (child.Tag == "Sdk")
            {
                m_strSdkUrl = child.Attribute("IP");
            }

            if (child.Tag == "ZoneRange")
            {
                var zone_page = new ZonePage();
				zone_page.id = int.Parse(child.Attribute("ID"));
                zone_page.name = child.Attribute("name");

                foreach (SecurityElement e in child.Children)
                {
                    if (e.Tag == "Zone")
                    {
                        ZoneData zoneData = new ZoneData();

                        //if(child.Attribute("ID") != null)
                        zoneData.nZoneID = int.Parse(e.Attribute("ID"));

                        //if (child.Attribute("Name") != null)
                        zoneData.strZoneName = e.Attribute("Name");

                        //if (child.Attribute("Port") != null)
                        zoneData.m_iPort = int.Parse(e.Attribute("Port"));

                        //if (child.Attribute("State") != null)
                        zoneData.byZoneStatus = byte.Parse(e.Attribute("State"));

                        //if (child.Attribute("Flag") != null)
                        zoneData.byZoneFlag = byte.Parse(e.Attribute("Flag"));

                        //if (child.Attribute("IP") != null)

						zoneData.ServerId = int.Parse(e.Attribute("ServerID"));

                        zoneData.m_strIP = e.Attribute("IP");

                        zoneData.m_bShowGuestEntrance = e.Attribute("ShowGuest") == "1" ? true : false;

                        zoneData.ZoneRange = zone_page.id;

                        m_lstService.Add(zoneData);
                        zone_page.server_list.Add(zoneData);
                    }
                }
                m_ZonePageList.Add(zone_page);
            }
        }
    }
}
