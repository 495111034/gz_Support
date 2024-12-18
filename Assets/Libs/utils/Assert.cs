using System;
using System.Diagnostics;


/// <summary>
/// 断言
/// </summary>
public static class MyAssert
{    
    public static void AssertTrue(bool exp, string msg = null)
    {
        if (!exp) throw new Exception("Asset Failed: " + msg);
    }
}

