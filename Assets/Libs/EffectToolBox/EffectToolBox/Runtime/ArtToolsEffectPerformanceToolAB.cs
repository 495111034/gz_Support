using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;
using System;

namespace GameSupport.EffectToolBox
{
    public class ArtToolsEffectPerformanceToolAB : MonoBehaviour
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
            public float AverageRenderTime;
            public float MaxRenderTimeOccurTime;
            public int ActiveRendererCount;
            public int TotalParticleSystemCount;
            public int MaterialCount;
            public int MaxParticleCount;
            public int TextureMemoryBytes;
            public int TextureMemoryCount;            
        }

        public int InstanceCount = 20;
        public float TestDuration = 5;
        public int TestCount = 5;
        public int TopNFrame = 5;
        public string searchPattern = "PS*";
        public const string FX_PATH = "/data/assets/gameres/effect/prefab";

        private List<EffectPerformanceReport> mReportList = new List<EffectPerformanceReport>();
        private List<EffectPerformanceReport> mSubReportList = new List<EffectPerformanceReport>();
        private List<GameObject> mFxInstanceList = new List<GameObject>();
        private List<string> mPathList = new List<string>();

        private bool isCoroutineRunning = false;
        private bool isRunning = false;
        private float TestStart = 5;
        void Start()
        {
            TestStart = Time.realtimeSinceStartup;
        }

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


        private void OnDestroy()
        {
        }


        IEnumerator StartAnalyze(string dataPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(dataPath);
            FileInfo[] files = directoryInfo.GetFiles(searchPattern + ".prefab.ab", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                //string fxPath = files[mReportList.Count].DirectoryName.Substring(files[mReportList.Count].DirectoryName.LastIndexOf("assets"));
                //fxPath.Replace("\\", "/");
                //fxPath = fxPath + "/" + files[mReportList.Count].Name;
                string fxPath = files[i].FullName;
                fxPath.Replace("\\", "/");
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
            Debug.LogError("Analyzing Success , outputFile :" + outputFile);
        }

        public void ExportCsv(string path)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("\"{0}\",", "特效路径");
            sb.AppendFormat("\"{0}\",", "加载时间");
            sb.AppendFormat("\"{0}\",", "实例化时间");
            sb.AppendFormat("\"{0}\",", "单帧最大时间");
            sb.AppendFormat("\"{0}\",", "最耗时N帧平均时间");
            sb.AppendFormat("\"{0}\",", "帧平均时间");
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
                sb.AppendFormat("\"{0}\",", mReportList[i].AverageRenderTime);
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

        private void Update()
        {
            if(isRunning)
            {
                return;
            }

            float t2 = Time.realtimeSinceStartup;
            if((t2 - TestStart) > TestDuration)
            {
                isRunning = true;
                string path = getAssetDataPath();
                StartCoroutine(StartAnalyze(path));
            }            
        }


        IEnumerator AnalyzeSingleFx(string fxPath)
        {
            Debug.Log("AnalyzeSingleFx： " + fxPath);
            isCoroutineRunning = true;
            for (int testSeq = 0; testSeq < TestCount; testSeq++)
            {
                EffectPerformanceReport report = new EffectPerformanceReport();
                report.Path = fxPath;

                //清除缓存资源
                System.GC.Collect();
                AsyncOperation async = Resources.UnloadUnusedAssets();
                yield return async;
                //AssetBundle.UnloadAllAssetBundles(true);
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();

                //加载
                float t1 = Time.realtimeSinceStartup;
                //GameObject fxAsset = Resources.Load<GameObject>(fxPath);
                UnityEngine.Profiling.Profiler.BeginSample("AnalyzeSingleFx:LoadFromFile");
                GameObject fxAsset = null;
                AssetBundle loadedAssetBundle = null;
                try
                {
                    loadedAssetBundle = AssetBundle.LoadFromFile(fxPath);
                    if (!loadedAssetBundle)
                    {

                        isCoroutineRunning = false;
                        yield break;
                    }

                    string[] assetNames = loadedAssetBundle.GetAllAssetNames();

                    if (assetNames.Length == 0)
                    {
                        isCoroutineRunning = false;
                        yield break;
                    }
                    fxAsset = loadedAssetBundle.LoadAsset<GameObject>(assetNames[0]);
                }
                catch (System.Exception e)
                {
                    isCoroutineRunning = false;
                    yield break;
                }
                UnityEngine.Profiling.Profiler.EndSample();
                float t2 = Time.realtimeSinceStartup;
                report.LoadTime = (t2 - t1) * 1000;
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
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
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                UnityEngine.Profiling.Profiler.BeginSample("AnalyzeSingleFx:Instantiate");
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
                UnityEngine.Profiling.Profiler.EndSample();
                float t3 = Time.realtimeSinceStartup;
                report.InstantiateTime = (t3 - t2) * 1000f / InstanceCount;
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();

                //渲染
                ParticleSystem[] systems = fxInstance.GetComponentsInChildren<ParticleSystem>();
                float maxDuration = 0;

                var psAnimation = fxInstance.GetComponentsInChildren<Animation>();
                if (psAnimation.Length > 0)
                {
                    foreach (var ps in psAnimation)
                    {
                        if (ps.clip != null)
                        {
                            if (ps.clip.length > maxDuration)
                            {
                                maxDuration = ps.clip.length;
                            }
                        }
                    }
                }
                var psAnimator = fxInstance.GetComponentsInChildren<Animator>();
                if (psAnimator.Length > 0)
                {
                    foreach (var ps in psAnimator)
                    {
                        if (ps.runtimeAnimatorController != null)
                        {
                            if (ps.GetCurrentAnimatorStateInfo(0).length > maxDuration)
                            {
                                maxDuration = ps.GetCurrentAnimatorStateInfo(0).length;
                            }
                        }
                    }
                }   
                foreach (var ps in systems)
                {
                    var time = ps.main.duration + ps.main.startLifetime.constant + ps.main.startDelay.constant;
                    if (time > maxDuration)
                    {
                        maxDuration = time;                      
                    }
                }

                float t4 = 0;
                int frame = 0;
                report.MinRenderTime = float.MaxValue;
                report.MaxRenderTime = float.MinValue;
                List<float> timeList = new List<float>();
                maxDuration = maxDuration > 0 ? maxDuration : TestDuration;
                while (t4 < maxDuration)
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
                avg = 0;
                for (int i = 0; i < timeList.Count; i++)
                {
                    avg += timeList[i];
                }
                report.AverageRenderTime = avg / timeList.Count / InstanceCount;
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
                loadedAssetBundle.Unload(true);
                UnityEngine.Object.DestroyImmediate(loadedAssetBundle);
                loadedAssetBundle = null;
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();

                mSubReportList.Add(report);
            }
            EffectPerformanceReport reportTotal = new EffectPerformanceReport();
            reportTotal.Path = fxPath;

            for (int i = 0; i < mSubReportList.Count; i++)
            {
                reportTotal.LoadTime += mSubReportList[i].LoadTime;
                reportTotal.InstantiateTime += mSubReportList[i].InstantiateTime;
                reportTotal.MinRenderTime += mSubReportList[i].MinRenderTime;
                reportTotal.MaxRenderTime += mSubReportList[i].MaxRenderTime;
                reportTotal.TopAverageRenderTime += mSubReportList[i].TopAverageRenderTime;
                reportTotal.MaxRenderTimeOccurTime += mSubReportList[i].MaxRenderTimeOccurTime;
                reportTotal.AverageRenderTime += mSubReportList[i].AverageRenderTime;

                reportTotal.ActiveRendererCount = mSubReportList[i].ActiveRendererCount;
                reportTotal.TotalParticleSystemCount = mSubReportList[i].TotalParticleSystemCount;
                reportTotal.MaterialCount = mSubReportList[i].MaterialCount;
                reportTotal.MaxParticleCount = mSubReportList[i].MaxParticleCount;
                reportTotal.TextureMemoryBytes = mSubReportList[i].TextureMemoryBytes;
                reportTotal.TextureMemoryCount = mSubReportList[i].TextureMemoryCount;

                if(i == mSubReportList.Count - 1)
                {
                    reportTotal.LoadTime = reportTotal.LoadTime / TestCount;
                    reportTotal.InstantiateTime = reportTotal.InstantiateTime / TestCount;
                    reportTotal.MinRenderTime = reportTotal.MinRenderTime / TestCount;
                    reportTotal.MaxRenderTime = reportTotal.MaxRenderTime / TestCount;
                    reportTotal.TopAverageRenderTime = reportTotal.TopAverageRenderTime / TestCount;
                    reportTotal.MaxRenderTimeOccurTime = reportTotal.MaxRenderTimeOccurTime / TestCount;
                    reportTotal.AverageRenderTime = reportTotal.AverageRenderTime / TestCount;
                }
            }
            mSubReportList.Clear();
            mReportList.Add(reportTotal);
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
                                    textureInfo.Bytes = (int)(UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tex) / 1024);
                                }
                                else
                                {
                                    textureInfo.Bytes = (int)(UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tex) * 4 / 1024);
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