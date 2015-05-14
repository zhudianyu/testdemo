using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProtoCmd;

public class SendMsgQueue{

    public object m_lockObj = new object();      //队列操作的对象锁

    //private List<uint> m_lstSendMsg = new List<uint>(); //当前发送到消息列表

    //private List<Message> m_lstSendMsg = new List<Message>();


    private Dictionary<uint, Message> m_dictSendMsg = new Dictionary<uint, Message>();


    public void SendQueueMsg() 
    {
        lock (m_lockObj) 
        {
            foreach (Message msg in m_dictSendMsg.Values)
            {
                if (GameNetwork.m_service != null)
                {
                    GameNetwork.m_service.Send(msg);
                }
            }
            m_dictSendMsg.Clear();
        }

        
    }


    public void ClearQueueList() 
    {
        lock (m_lockObj) 
        {
            m_dictSendMsg.Clear();
        }
    }

    public bool CheckBackpackMsg(uint msgid) 
    {
        lock (m_lockObj) 
        {
            int totalCount = m_dictSendMsg.Count;
            if (totalCount == 0)
            {
                return true;
            }
            else if (totalCount > 0)
            {
                if (m_dictSendMsg.ContainsKey(msgid)) 
                {
                    m_dictSendMsg.Clear();
                    return true;
                }
            }
            return false;

            //if (m_lstSendMsg.Count > 0)
            //{
            //    if (m_lstSendMsg.Contains(msgid)) 
            //    {
            //        Debug.Log("ContainsMsg:" + msgid);
            //        m_lstSendMsg.Clear();
            //        return true;
            //    }
            //    //uint topMsgId = m_lstSendMsg.Peek();
            //    //Debug.Log("topMsgId:" + topMsgId + ",revMsgId:" + msgid);
            //    //if (topMsgId == msgid) 
            //    //{
            //    //    m_lstSendMsg.Dequeue();
            //    //    return true;
            //    //}
            //    Debug.Log("cacheMsgCount:" + m_lstSendMsg.Count);
            //}
            ////test code 
            //return false;
        }
    }





    public bool Push(Message msg) 
    {
        //return false;

        lock (m_lockObj) 
        {
            if (!isPassLockCmd(msg.m_cmd_no))
            {
                //uint cmdNumber = (uint)cmd;

                //if (m_lstSendMsg.Contains(cmdNumber)) 
                //{
                uint msgId = (uint)msg.m_cmd_no;
                if (m_dictSendMsg.ContainsKey(msgId))
                {
                    m_dictSendMsg[msgId] = msg;
                }
                else 
                {
                    m_dictSendMsg.Add(msgId, msg);
                }
                return true;
            }
            return false;
        }
    }


    public bool isPassLockCmd(CmdNumber cmd)
    {
        //判定该命令是否为需要加锁的命令
        if (cmd == CmdNumber.PlayerMoveClientCmd_C
            || cmd == CmdNumber.CastSkillClientCmd_C
            || cmd == CmdNumber.SyncSkillToOtherCmd_CS
            || cmd == CmdNumber.RespondPingClientCmd_C
            || cmd == CmdNumber.WerwolfTransferClientCmd_C
            || cmd == CmdNumber.ReqInitDeviceCmd_C

            || cmd == CmdNumber.PlayerVerifyVerLoginClientCmd_C //登录相关
            || cmd == CmdNumber.LoginAccessLoginClientCmd_C
            //|| cmd == CmdNumber.PlayerSelectClientCmd_C
            || cmd == CmdNumber.PlayerRequestLoginClientCmd_C

            || cmd == CmdNumber.LogoutClientCmd_C

            || cmd == CmdNumber.AccountRegisterClientCmd_CS

            || cmd == CmdNumber.ReconnectLoginClientCmd_C)
        {
            return true;
        }
        return false;
    }



}
