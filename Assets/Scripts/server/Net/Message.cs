using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProtoCmd;

public class Message
{
    public CmdNumber m_cmd_no;
    public byte[] m_cmd;

    public Message(CmdNumber cmd_no, byte[] cmd)
    {
        m_cmd_no = cmd_no;
        m_cmd = cmd;
        //reference = 1;
    }

    public Message()
    {
        m_cmd_no = CmdNumber.ClientCmdNone;
        m_cmd = null;
    }

    public bool CheckFill()
    {
        return ((m_cmd_no != CmdNumber.ClientCmdNone) && (m_cmd != null));
    }

    public void SetDefault() 
    {
        m_cmd_no = CmdNumber.ClientCmdNone;
        m_cmd = null;
        //reference -= 1;
    }

    private void InitData(CmdNumber cmd_no, byte[] cmd) 
    {
        m_cmd = cmd;
        m_cmd_no = cmd_no;
        //reference = 1;
    }

    ////这个对象的引用数,当为0的时候,表示是可以回收的了
    //public int reference = 0;


    //public void Remain()
    //{
    //    reference += 1;
    //}
    //public void Release() 
    //{
    //    reference -= 1;
    //    if (reference == 0)
    //    {
    //        //Debug.LogWarning("add msg to pool");
    //        RecoverMessage(this);
    //    }
    //}



    //#region 消息池 单纯测试用的 by lhc

    //private static List<Message> m_lstMessagePool = new List<Message>();
    ////操作的对象锁
    //private static object m_objLock = new object();


    //public static Message CreateMessage(CmdNumber cmd_no, byte[] cmd)
    //{
    //    lock (m_objLock) 
    //    {
    //        Message resultMsg = null;
            
    //        if (m_lstMessagePool.Count > 0)
    //        {
    //            resultMsg = m_lstMessagePool[0];
    //            m_lstMessagePool.RemoveAt(0);
    //            resultMsg.InitData(cmd_no, cmd);
    //        }
    //        else 
    //        {
    //            resultMsg = new Message(cmd_no, cmd);
    //        }

    //        //Debug.LogWarning("cache msglist count: " + m_lstMessagePool.Count);
    //        return resultMsg;
    //    }
    //}
    //public static void RecoverMessage(Message msg)
    //{
    //    lock (m_objLock) 
    //    {
    //        m_lstMessagePool.Add(msg);
    //        msg.SetDefault();
    //    }
    //}
    //#endregion

}
