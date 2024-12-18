using System.Text;
using System.Net;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;
using System.Linq;
using BDFramework.Editor.Unity3dEx;
using static BDFramework.Editor.Unity3dEx.ShaderUtilImpl;

namespace BDFramework.Editor.AssetBundle
{
    public class ShaderVariantsCollectionTools
    {
        //系统带的、material.shaderKeywords拿不到的宏添到这里,含有这些关键字的shader会启用对应变体
        static HashSet<string> ForceEnabledGlobalKeywords = new HashSet<string>() {};

        static HashSet<string> ForceDisabledGlobalKeywords = new HashSet<string>() { };

        /// <summary>
        /// shader数据map
        /// </summary>
        //private Dictionary<Shader, List<ShaderUtilImpl.ShaderVariantEntriesData>> shaderDataMap = new Dictionary<Shader, List<ShaderUtilImpl.ShaderVariantEntriesData>>();

        /// <summary>
        /// 搜集keywords
        /// </summary>
        public List<ShaderVariantCollection> CollectionKeywords(Shader shader,  string[] matPaths, List<ShaderVariantCollection.ShaderVariant> scene_variants)
        {
            //var shaders = new HashSet<Shader>();
            //var shaderCollection = ScriptableObject.CreateInstance<ShaderVariantCollections>();
            
            
            var collects = new List<ShaderVariantCollection>();
            var test = new ShaderVariantCollection();
            test.name = shader.name.Replace(' ', '_').Replace('/', '_') + "__view_all";
            collects.Add(test);

            void add(ShaderVariantCollection.ShaderVariant shaderVaraint)
            {
                if (shaderVaraint.keywords == null || shaderVaraint.keywords.Length == 0)
                {
                    return;
                }

                int empty = 0;
                foreach (var k in shaderVaraint.keywords) 
                {
                    if (string.IsNullOrWhiteSpace(k)) 
                    {
                        ++empty;
                    }
                }
                if (empty == shaderVaraint.keywords.Length) 
                {
                    return;
                }

                if (!test.Contains(shaderVaraint) && test.Add(shaderVaraint))
                {
                    var newcollect = new ShaderVariantCollection();
                    newcollect.name = shader.name.Replace(' ', '_').Replace('/', '_') + "__" + (collects.Count + 1000);
                    collects.Add(newcollect);
                    //                                   
                    newcollect.Add(shaderVaraint);
                }
            }
            //
            if (scene_variants != null)
            {
                //Log.LogInfo($"{shader.name} use scene_variants");
                foreach (var v in scene_variants)
                {
                    add(v);
                }
            }
            
            if(scene_variants == null || !shader.name.StartsWith("BF/Scene/"))
            {
                //Log.LogInfo($"{shader.name} use mats");
                var shaderData = ShaderUtilImpl.GetShaderVariantEntriesFilteredInternal(shader, new ShaderVariantCollection());
                var passTypes = shaderData.passTypes;
                //遍历所有mat的KeyWords 
                foreach (var path in matPaths)
                {
                    if (path.StartsWith("Assets/Resources"))
                    {
                        continue;
                    }
                    //Material;
                    var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                    var keywords = GetMaterialKeywords(material, material.shaderKeywords, null);
                    if (keywords != null && keywords.Length > 0)
                    {
                        //收集shaderVaraint
                        //Log.Log2File($"material={material.name}, {material.shader.name}, passTypes={passTypes.Length}, keywords={keywords.Length}");
                        foreach (var pt in passTypes)
                        {
                            var shaderVaraint = CreateVariant(material.shader, (PassType)pt, keywords);
                            add(shaderVaraint);
                        }
                    }
                }
            }
            //            
            for (var i = collects.Count - 1; i >= 0; --i) 
            {
                if (collects[i].variantCount == 0) 
                {
                    collects.swap_tail_and_fast_remove(i);
                }
            }
            collects.Sort((a, b) => { return a.name.CompareTo(b.name);});
            //
            return collects;
        }


        /// <summary>
        /// 收集passtype-keyword
        /// </summary>
        /// <param name="passType"></param>
        /// <param name="material"></param>
        string[] GetMaterialKeywords(Material material, string[] keywords0, string[] keywords1)
        {
            //var shader = material.shader;
            var keywords = new HashSet<string>(keywords0);
            if (keywords1 != null)
            {
                foreach (var k in keywords1)
                {
                    keywords.Add(k);
                }
            }

            //keywords.Add("FOG_LINEAR");
            //Instancing
            if (material.enableInstancing)
            {
                keywords.Add("INSTANCING_ON");
            }
            //
            //添加mat中的keyword
            //foreach (var key in material.shaderKeywords)
            //{
            //    keywords.Add(key);
            //}

            //打开的global keyword
            foreach (var key in ForceEnabledGlobalKeywords)
            {
                //if (shaderAllkeyworlds.Contains(key) /*&& Shader.IsKeywordEnabled(key)*/)
                {
                    keywords.Add(key);
                }
            }

            //关闭的global keyword
            foreach (var key in ForceDisabledGlobalKeywords)
            {
                keywords.Remove(key);
            }

            var arr = keywords.Distinct().ToList();
            arr.Sort();
            return arr.ToArray();
        }

        /// <summary>
        /// 创建Variant
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="passType"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        ShaderVariantCollection.ShaderVariant CreateVariant(Shader shader, PassType passType, string[] keywords)
        {
            // foreach (var k in keywords)
            // {
            //     Debug.Log($"{shader.name}:{passType}:{k}");
            // }
            try
            {
                // var variant = new ShaderVariantCollection.ShaderVariant(shader, passType, keywords);//这构造函数就是个摆设,铁定抛异常(╯‵□′)╯︵┻━┻
                var variant = new ShaderVariantCollection.ShaderVariant();
                variant.shader = shader;
                variant.passType = passType;
                variant.keywords = keywords;
                return variant;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                return new ShaderVariantCollection.ShaderVariant();
            }
        }

        Dictionary<Shader, List<string>> shaderKeyworldsDic = new Dictionary<Shader, List<string>>();

        /// <summary>
        /// 获取所有的GlobalKeyword
        /// </summary>
        /// <param name="shader"></param>
        /// <returns></returns>
        List<string> GetShaderAllKeyworlds(Shader shader)
        {
            List<string> keywords = null;
            shaderKeyworldsDic.TryGetValue(shader, out keywords);
            if (keywords == null)
            {
                keywords = new List<string>(ShaderUtilImpl.GetShaderGlobalKeywords(shader));
                shaderKeyworldsDic.Add(shader, keywords);
            }

            return keywords;
        }
    }
}