using System.Collections.Generic;
//using System.Text;
//using System.Collections;
//using System;

public class MessageQueue
{
    private Queue<Message> m_CmdQueue;
    private Locker m_lock;

    public MessageQueue()
    {
        m_CmdQueue = new Queue<Message>();
        m_lock = new Locker();
    }

    /// <summary>
    /// 向队尾添加新消息
    /// </summary>
    /// <param name="message"></param>
    public void Enqueue(Message message)
    {
        m_lock.Lock();
        m_CmdQueue.Enqueue(message);
        m_lock.UnLock();
    }

    /// <summary>
    /// 从队首移除消息并获取被移除的消息
    /// </summary>
    /// <returns></returns>
    public Message Dequeue()
    {
        m_lock.Lock();
        Message message = m_CmdQueue.Dequeue();
        m_lock.UnLock();
        return message;
    }

    /// <summary>
    /// 判断消息队列是否为空
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            return (m_CmdQueue.Count == 0);
        }
    }

    public int GetCount( )
    {
        return m_CmdQueue.Count;
    }
}

