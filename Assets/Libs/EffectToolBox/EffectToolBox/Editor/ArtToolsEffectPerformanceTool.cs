using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;
using System;
using System.Reflection;

namespace GameSupport.EffectToolBox
{
    public class ArtToolsEffectPerformanceTool : MonoBehaviour
    {
        public class TextureInfo
        {
            public string Name;
            public int Width;
            public int Height;
            public int Bytes;
        }

        public class EffectPerformanceReport
        {
            public string Path;
            public float LoadTime;
            public float InstantiateTime;
            public float MinRenderTime;
            public float MaxRenderTime;
            public float TopAverageRenderTime;
            public float MaxRenderTimeOccurTime;
            public int ActiveRendererCount;
            public int TotalParticleSystemCount;
            public int MaterialCount;
            public int MaxParticleCount;
            public int TextureMemoryBytes;
            public int TextureMemoryCount;            
        }

        public int InstanceCount = 1;
        public float TestDuration = 5;
        public int TopNFrame = 5;
        public string searchPattern = "PS*";
        public bool isNeedAllDirectories = true;
        public const string FX_PATH = "/Assets/gameres/effect/prefab";

        private List<EffectPerformanceReport> mReportList = new List<EffectPerformanceReport>();
        private List<GameObject> mFxInstanceList = new List<GameObject>();
        private List<string> mPathList = new List<string>();

        private bool isCoroutineRunning = false;

        public string getAssetDataPath()
        {
            if (!Application.isMobilePlatform)
            {
                return Path.GetDirectoryName(Application.dataPath) + FX_PATH;
            }
            else
            {
                return Application.persistentDataPath + FX_PATH;
            }
        }

        public string getFileDataPath()
        {
            if (!Application.isMobilePlatform)
            {
                return Path.GetDirectoryName(Application.dataPath);
            }
            else
            {
                return Application.persistentDataPath;
            }
        }


        void Start()
        {
            string path = getAssetDataPath();
            StartCoroutine(StartAnalyze(path));
        }

        IEnumerator StartAnalyze(string dataPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(dataPath);
            FileInfo[] files = directoryInfo.GetFiles(searchPattern + ".prefab", isNeedAllDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            for (int i = 0; i < files.Length; i++)
            {
                string fxPath = files[i].DirectoryName.Substring(files[i].DirectoryName.LastIndexOf("Assets"));
                fxPath.Replace("\\", "/");
                fxPath = fxPath + "/" + files[i].Name;
                mPathList.Add(fxPath);
            }

            while (mReportList.Count < mPathList.Count)
            {
                if (!isCoroutineRunning)
                {
                    this.transform.name = string.Format("Analyzing...({0}/{1})", mReportList.Count, mPathList.Count);
                    yield return StartCoroutine(AnalyzeSingleFx(mPathList[mReportList.Count]));
                }
            }
            this.transform.name = string.Format("Analyzing...({0}/{1})", mReportList.Count, mPathList.Count);
            string strTime = DateTime.Now.ToString("yyMMdd_hhmmss");
            string fileName = string.Format("/EffectReport_{0}.csv", strTime);
            string outputFile = getFileDataPath() + fileName;
  
            ExportCsv(outputFile);
            EditorUtility.DisplayDialog("提示", "Analyzing Success , outputFile :" + outputFile , "ok");
            AssetDatabase.Refresh();
        }

        public void ExportCsv(string path)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("\"{0}\",", "特效路径");
            sb.AppendFormat("\"{0}\",", "加载时间");
            sb.AppendFormat("\"{0}\",", "实例化时间");
            sb.AppendFormat("\"{0}\",", "单帧最大时间");
            sb.AppendFormat("\"{0}\",", "最耗时N帧平均时间");
            sb.AppendFormat("\"{0}\",", "激活的渲染器数量");
            sb.AppendFormat("\"{0}\",", "发射器数量");
            sb.AppendFormat("\"{0}\",", "不同材质个数");
            sb.AppendFormat("\"{0}\",", "最多粒子个数");
            sb.AppendFormat("\"{0}\",", "贴图个数");
            sb.AppendFormat("\"{0}\",", "贴图内存KB");
            sb.AppendFormat("\n");

            for (int i = 0; i < mReportList.Count; i++)
            {
                sb.AppendFormat("\"{0}\",", mReportList[i].Path);
                sb.AppendFormat("\"{0}\",",mReportList[i].LoadTime);
                sb.AppendFormat("\"{0}\",",mReportList[i].InstantiateTime);
                sb.AppendFormat("\"{0}\",",mReportList[i].MaxRenderTime);
                sb.AppendFormat("\"{0}\",",mReportList[i].TopAverageRenderTime);
                sb.AppendFormat("\"{0}\",",mReportList[i].ActiveRendererCount);
                sb.AppendFormat("\"{0}\",",mReportList[i].TotalParticleSystemCount);
                sb.AppendFormat("\"{0}\",",mReportList[i].MaterialCount);
                sb.AppendFormat("\"{0}\",",mReportList[i].MaxParticleCount);
                sb.AppendFormat("\"{0}\",", mReportList[i].TextureMemoryCount);
                sb.AppendFormat("\"{0}\",",mReportList[i].TextureMemoryBytes);
                sb.AppendFormat("\n");
            }

            try
            {
                StreamWriter st = File.CreateText(path);
                st.Write(sb.ToString());
                st.Close();
            }
            catch (Exception e)
            {
                throw;
            };
        }


        IEnumerator AnalyzeSingleFx(string fxPath)
        {
            isCoroutineRunning = true;

            EffectPerformanceReport report = new EffectPerformanceReport();
            report.Path = fxPath;

            //清除缓存资源
            System.GC.Collect();
            AsyncOperation async = Resources.UnloadUnusedAssets();
            yield return async;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            //加载
            float t1 = Time.realtimeSinceStartup;
            //GameObject fxAsset = Resources.Load<GameObject>(fxPath);
            GameObject fxAsset = null;
            try
            {
                fxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fxPath);
            }
            catch (System.Exception e)
            {
                isCoroutineRunning = false;
                yield break;
            }
            float t2 = Time.realtimeSinceStartup;
            report.LoadTime = (t2 - t1) * 1000;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            if (fxAsset == null)
            {
                isCoroutineRunning = false;
                yield break;
            }

            report.TotalParticleSystemCount = fxAsset.GetComponentsInChildren<ParticleSystem>(true).Length;
            Renderer[] fxRenderers = fxAsset.GetComponentsInChildren<Renderer>(true);
            Dictionary<Material, bool> fxMaterials = new Dictionary<Material, bool>();
            int activeRendererCount = 0;
            foreach (var renderer in fxRenderers)
            {
                bool has = false;
                if (renderer.sharedMaterial != null && fxMaterials.TryGetValue(renderer.sharedMaterial, out has) == false)
                {
                    fxMaterials.Add(renderer.sharedMaterial, true);
                }
                if (renderer.enabled)
                {
                    activeRendererCount++;
                }
            }
            report.ActiveRendererCount = activeRendererCount;
            report.MaterialCount = fxMaterials.Count;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            //实例化
            GameObject fxInstance = null;
            t2 = Time.realtimeSinceStartup;
            for (int i = 0; i < InstanceCount; i++)
            {
                GameObject go = GameObject.Instantiate(fxAsset);
                go.transform.position = Vector3.zero;
                mFxInstanceList.Add(go);
                if (i == 0)
                {
                    fxInstance = go;
                }
            }
            float t3 = Time.realtimeSinceStartup;
            report.InstantiateTime = (t3 - t2) * 1000f / InstanceCount;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            //渲染
            ParticleSystem[] systems = fxInstance.GetComponentsInChildren<ParticleSystem>();
            float t4 = 0;
            int frame = 0;
            report.MinRenderTime = float.MaxValue;
            report.MaxRenderTime = float.MinValue;
            List<float> timeList = new List<float>();
            while (t4 < TestDuration)
            {
                float dt = Time.deltaTime;
                frame++;
                report.MinRenderTime = Mathf.Min(report.MinRenderTime, dt);
                if (dt > report.MaxRenderTime)
                {
                    report.MaxRenderTime = dt;
                    report.MaxRenderTimeOccurTime = t4;
                }
                timeList.Add(dt * 1000);
                t4 += dt;
                int particleCount = 0;
                for (int i = 0; i < systems.Length; i++)
                {
                    particleCount += systems[i].particleCount;
                }
                if (report.MaxParticleCount < particleCount)
                {
                    report.MaxParticleCount = particleCount;
                }
                yield return new WaitForEndOfFrame();
            }

            report.MinRenderTime *= 1000 / InstanceCount;
            report.MaxRenderTime *= 1000 / InstanceCount;
            timeList.Sort();
            timeList.Reverse();
            float avg = 0;
            int topN = Mathf.Min(TopNFrame, timeList.Count);
            for (int i = 0; i < topN; i++)
            {
                avg += timeList[i];
            }
            report.TopAverageRenderTime = avg / topN / InstanceCount;
            yield return new WaitForEndOfFrame();

            Dictionary<string, TextureInfo> texNames = GetTextureMemoryAndCount(fxInstance);
            foreach (var t in texNames)
            {
                report.TextureMemoryBytes += t.Value.Bytes;
                report.TextureMemoryCount++;
            }

            //清理
            foreach (var fx in mFxInstanceList)
            {
                UnityEngine.Object.DestroyImmediate(fx);
            }            
            mFxInstanceList.Clear();
            
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            mReportList.Add(report);
            isCoroutineRunning = false;
        }

        private Dictionary<string, TextureInfo> GetTextureMemoryAndCount(GameObject go)
        {
            Dictionary<string, TextureInfo> textureInfoDict = new Dictionary<string, TextureInfo>();
            List<Material> matCounts = new List<Material>();
            List<Texture> texCounts = new List<Texture>();

            var rendS = go.GetComponentsInChildren<Renderer>(true);
            if (rendS.Length > 0)
            {
                foreach (var rend in rendS)
                {
                    var matS = rend.sharedMaterials;
                    if (matS.Length > 0)
                    {
                        foreach (var mat in matS)
                        {
                            if (mat != null)
                            {
                                if (!matCounts.Contains(mat))
                                {
                                    matCounts.Add(mat);
                                }
                            }
                        }
                    }
                }
            }

            if (matCounts.Count > 0)
            {
                foreach (var mat in matCounts)
                {
                    int[] textureProperties =  mat.GetTexturePropertyNameIDs();
                    for (int i = 0; i < textureProperties.Length; i++)
                    {
                        var tex = mat.GetTexture(textureProperties[i]);
                        if (tex != null)
                        {
                            //查找list里是否存在某个对象
                            if (!texCounts.Contains(tex))
                            {
                                texCounts.Add(tex);

                                TextureInfo textureInfo = new TextureInfo();
                                textureInfo.Name = tex.name;
                                textureInfo.Width = tex.width;
                                textureInfo.Height = tex.height;
                                //和编辑器测试差4倍
                                if (!Application.isMobilePlatform)
                                {
                                    textureInfo.Bytes = (int)(GetParticleEffectData.GetStorageMemorySize(tex) / 1024);
                                }
                                
                                textureInfoDict[textureInfo.Name] = textureInfo;

                            }
                        }
                    }
                }
            }

            return textureInfoDict;
        }
    }
}