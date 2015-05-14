using UnityEngine;
using System.Collections;
using System.Text;

public class ECommonTool {

    public static byte[] Utf8StringToBytes(string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }

    public static string Utf8BytesToString(byte[] bts)
    {
        if (bts == null)
            return "";
        return Encoding.UTF8.GetString(bts);
    }

}
