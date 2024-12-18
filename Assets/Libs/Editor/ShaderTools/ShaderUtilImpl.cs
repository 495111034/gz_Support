using System;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace BDFramework.Editor.Unity3dEx
{
    /// <summary>
    /// 反射UnityShader相关辅助接口
    /// </summary>
    public class ShaderUtilImpl
    {
        delegate string[] GetShaderGlobalKeywords_type(Shader shader);

        static GetShaderGlobalKeywords_type GetShaderGlobalKeywords_impl;

        /// <summary>
        /// 获取shader 的global KeyWords
        /// </summary>
        /// <param name="shader"></param>
        /// <returns></returns>
        public static string[] GetShaderGlobalKeywords(Shader shader)
        {
            if (GetShaderGlobalKeywords_impl == null)
            {
                GetShaderGlobalKeywords_impl = Delegate.CreateDelegate(typeof(GetShaderGlobalKeywords_type), typeof(UnityEditor.ShaderUtil).GetMethod("GetShaderGlobalKeywords", BindingFlags.Static | BindingFlags.NonPublic)) as GetShaderGlobalKeywords_type;
            }

            return GetShaderGlobalKeywords_impl(shader) as string[];
        }

        delegate string[] GetAllGlobalKeywords_type();

        static GetAllGlobalKeywords_type GetAllGlobalKeywords_impl;

        /// <summary>
        /// 获取所有的GlobalKeyWord
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllGlobalKeywords()
        {
            if (GetAllGlobalKeywords_impl == null)
            {
                GetAllGlobalKeywords_impl = Delegate.CreateDelegate(typeof(GetAllGlobalKeywords_type), typeof(UnityEditor.ShaderUtil).GetMethod("GetAllGlobalKeywords", BindingFlags.Static | BindingFlags.NonPublic)) as GetAllGlobalKeywords_type;
            }

            return GetAllGlobalKeywords_impl() as string[];
        }

        delegate string[] GetShaderLocalKeywords_type(Shader shader);

        static GetShaderLocalKeywords_type GetShaderLocalKeywords_impl;

        /// <summary>
        /// 获取本地的keywords
        /// </summary>
        /// <param name="shader"></param>
        /// <returns></returns>
        public static string[] GetShaderLocalKeywords(Shader shader)
        {
            if (GetShaderLocalKeywords_impl == null)
            {
                GetShaderLocalKeywords_impl = Delegate.CreateDelegate(typeof(GetShaderLocalKeywords_type), typeof(UnityEditor.ShaderUtil).GetMethod("GetShaderLocalKeywords", BindingFlags.Static | BindingFlags.NonPublic)) as GetShaderLocalKeywords_type;
            }

            return GetShaderLocalKeywords_impl(shader) as string[];
        }

#if UNITY_2019_1_OR_NEWER
        delegate void GetShaderVariantEntriesFiltered_type(Shader shader, int maxEntries, string[] filterKeywords, ShaderVariantCollection excludeCollection, out int[] passTypes, out string[] keywordLists, out string[] remainingKeywords);
#else
        delegate void GetShaderVariantEntriesFiltered_type(Shader shader, ShaderVariantCollection excludeCollection, out int[] passTypes, out string[] keywordLists);
#endif
        static GetShaderVariantEntriesFiltered_type GetShaderVariantEntriesFiltered_impl;

        /// <summary>
        /// 获取shader的相关数据
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="maxEntries"></param>
        /// <param name="filterKeywords"></param>
        /// <param name="excludeCollection"></param>
        /// <param name="passTypes"></param>
        /// <param name="keywordLists"></param>
        /// <param name="remainingKeywords"></param>
        public static void GetShaderVariantEntriesFiltered(Shader shader, ShaderVariantCollection excludeCollection, out int[] passTypes, out List<string> keywordList)
        {
            if (GetShaderVariantEntriesFiltered_impl == null)
            {
#if UNITY_2019_1_OR_NEWER
                var method = typeof(UnityEditor.ShaderUtil).GetMethod("GetShaderVariantEntriesFiltered", BindingFlags.Static | BindingFlags.NonPublic);
#else
                var method = typeof(UnityEditor.ShaderUtil).GetMethod("GetShaderVariantEntries", BindingFlags.Static | BindingFlags.NonPublic);
#endif
                var delegate_ins = Delegate.CreateDelegate(typeof(GetShaderVariantEntriesFiltered_type), method);
                GetShaderVariantEntriesFiltered_impl = delegate_ins as GetShaderVariantEntriesFiltered_type;
            }
            //
#if UNITY_2019_1_OR_NEWER
            GetShaderVariantEntriesFiltered_impl(shader, 1024, new string[] { }, excludeCollection, out passTypes, out var keywordLists, out var remainingKeywords);
            keywordList = new List<string>(keywordLists);
            keywordList.Remove("");

            //var ilist = new List<int>(passTypes);
            //passTypes = ilist.Distinct().ToArray();

            //keywordList.AddRange(remainingKeywords);
#else
            GetShaderVariantEntriesFiltered_impl(shader, excludeCollection, out passTypes, out keywordLists);
#endif
        }

        public class ShaderVariantEntriesData
        {
            public int[] passTypes;
            public List<string> keywordLists;
        }


        delegate ShaderVariantEntriesData GetShaderVariantEntriesFilteredInternal_type(Shader shader, int maxEntries, string[] filterKeywords, ShaderVariantCollection excludeCollection);

        static GetShaderVariantEntriesFilteredInternal_type GetShaderVariantEntriesFilteredInternal_impl;

        /// <summary>
        /// 获取shader的相关数据
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="maxEntries"></param>
        /// <param name="filterKeywords"></param>
        /// <param name="excludeCollection"></param>
        /// <param name="passTypes"></param>
        /// <param name="keywordLists"></param>
        /// <param name="remainingKeywords"></param>
        public static ShaderVariantEntriesData GetShaderVariantEntriesFilteredInternal(Shader shader, /*int maxEntries, string[] filterKeywords,*/ ShaderVariantCollection excludeCollection)
        {
            int[] passTypes;
            //string[] keywordLists;
            //string[] remainingKeywords;
            GetShaderVariantEntriesFiltered(shader, /*maxEntries, filterKeywords,*/ excludeCollection, out passTypes, out var keywordLists);//, out remainingKeywords);

            var list = new List<int>(passTypes.Distinct());
            list.Sort();

            var list_keywordLists = new List<string>();
            //var set = new HashSet<string>();
            foreach (var line in keywordLists)
            {
                var arr = line;//line.Split(' ');
                list_keywordLists.Add(arr);
            }
            list_keywordLists.Sort();
            //set.Add("FOG_LINEAR");
            //set.Add("INSTANCING_ON");
            //var list2 = new List<string>(set);
            //list2.Sort();
            return new ShaderVariantEntriesData()
            {
                passTypes = list.ToArray(),
                keywordLists = list_keywordLists,
                //remainingKeywords = remainingKeywords,
            };
        }
    }
}