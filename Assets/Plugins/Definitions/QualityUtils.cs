using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public static class QualityUtils
{
    static bool _is_low_mem;
    static bool __isSetMemeLevel = false;
    public static bool IsBadGPU = false;
    // 是否低内存
    public static bool IsLowMem
    {
        get
        {

            //if(SystemInfo.deviceModel.Contains("Netease"))
            //{
            //    return true;
            //}

            var low_memory_level = 2048;
#if UNITY_ANDROID
            low_memory_level = 2048;
#elif UNITY_IPHONE
            low_memory_level = 1200;
#elif UNITY_WEBGL
            if (Config.IsMobileDevice)
                low_memory_level = 256;
            else
                low_memory_level = 1024;
#else
            low_memory_level = 4096;
#endif

            if (!__isSetMemeLevel)
            {
#if UNITY_WEBGL
                if (Config.IsMobileDevice)
                {
                    _is_low_mem = true;
                    return _is_low_mem;
                }
#endif
                var size = SystemInfo.systemMemorySize;
                _is_low_mem = size <= low_memory_level;
            }

            __isSetMemeLevel = true;
            return _is_low_mem;
        }
    }  
    
}
