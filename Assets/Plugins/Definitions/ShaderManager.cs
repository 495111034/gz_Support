using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace resource
{
    /// <summary>
    /// Shader 管理器
    /// 
    ///     . 说明:
    ///         . shader 必须位于 Resources 目录下, 或者位于 GraphicSettings 中, 运行期才可以被找到
    ///         . ShaderManager 用于统一获取 shader，避免每次查找而影响性能,  并对部分 shader 进行适配替换
    /// 
    /// </summary>
    /// 

    
    public static class ShaderManager
    {

        static Dictionary<string, Shader> _cahces = new Dictionary<string, Shader>();
        static Dictionary<string, Shader> _cahces2 = new Dictionary<string, Shader>();
#if UNITY_EDITOR
        public static bool LastFindIsError = false;
#endif
        // 查找普通 shader
        public static Shader Find(string name)
        {
#if UNITY_EDITOR
            LastFindIsError = false;
#endif
            if (!_cahces.TryGetValue(name, out var sdr))
            {
                UnityEngine.Profiling.Profiler.BeginSample($"Shader.Find({name})");
                sdr = _cahces[name] = Shader.Find(name);
                UnityEngine.Profiling.Profiler.EndSample();
                if (sdr)
                {
#if UNITY_EDITOR
                    var path = UnityEditor.AssetDatabase.GetAssetPath(sdr).ToLower();
                    if (!path.StartsWith("assets/resources/shader"))
                    {
                        LastFindIsError = true;
                        Log.LogWarning($"错误的shader路径，{name} -> {path}, 只能使用assets/resources/shader下的shader");
                    }
#endif
                }
                else if(!_cahces2.TryGetValue(name, out sdr))
                {
                    Log.LogError($"Shader.Find({name}) == null");
                }
            }
            return sdr;
        }

        public static void Init()
        {
        }

        public static void InitByAssetBundle(Shader[] shaders) 
        {
            foreach (var s in shaders) 
            {
                _cahces2[s.name] = s;
            }
        }


        public static string logmem()
        {
            var g1 = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() / 1024;
            var g2 = UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong() / 1024;
            var u1 = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024;
            var u2 = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / 1024;
            return $"gc={g1}/{g2}KB, unity={u1}/{u2}KB";
        }     
    }

    public static class ShaderNameHash
    {
        public static int MainTex;
        public static int LightColor0;
        public static int WorldSpaceLightPos0;
        public static int TintColor;

        public static Dictionary<string, int> ShaderNameToId = new Dictionary<string, int>();

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            MainTex = ShaderNameId("_MainTex");           
            LightColor0 = ShaderNameId("_LightColor0");
            WorldSpaceLightPos0 = ShaderNameId("_WorldSpaceLightPos0");
            TintColor = ShaderNameId("_Color");
        }

        public static int ShaderNameId(string shaderPropertyName)
        {
            if (!ShaderNameToId.TryGetValue(shaderPropertyName, out int value))
            {
                ShaderNameToId[shaderPropertyName] = value  = Shader.PropertyToID(shaderPropertyName);
            }
            return value;
        }
    }
}
