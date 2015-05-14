using UnityEngine;
using System.Collections;

public class SelectRoleMgr {
    private static SelectRoleMgr instance;
    public static SelectRoleMgr Instance 
    {
        get 
        {
            if (instance == null)
                instance = new SelectRoleMgr();
            return instance;
        }
    }




    private RoleClass curRole = RoleClass.eUnknow;
    /// <summary>
    /// 获取当前选择的角色
    /// </summary>
    public int GetCurSelectRole()
    {
        return (int)curRole;
    }


    public void SetSelectRole(RoleClass role) 
    {
        curRole = role;
    }

}
