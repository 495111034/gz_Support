using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

using UnityEditor;

namespace GameSupport.EffectToolBox
{
    public class ArtToolsEffectOptimizationChecker_Effect : CustomResChecker
    {

        public bool mIsUseGPUIns;
        public Dictionary<string, GameObject> mGameObjectList = new Dictionary<string, GameObject>();

        private int mNo;

        public ArtToolsEffectOptimizationChecker_Effect()
        {
        }

        public override void Begin()
        {
            if (mScanDirs != null && mScanDirs.Length > 0)
            {
                mAllPath = AssetDatabase.FindAssets("t:Prefab", mScanDirs);
            }
            else
            {
                mAllPath = mScanDirs;
            }

        }


        public override void Checking()
        {
            mNo = 0;
            mData.Clear();
            mGameObjectList.Clear();

            // Debug.Log("=======begin================");
            // var all = mAllPath;

            SearchGameObjects();

            //排序
            SortBy();


            if (mData.Count > 0)
            {
                mResultInfo = "扫描完毕\r\n";// EditorUtility.DisplayDialog("提示：", "扫描完毕", "Yes");
            }
            else
            {
                mResultInfo = "没扫描到对象\r\n";
                //EditorUtility.DisplayDialog("提示：", "没扫描到贴图资源", "Yes");
            }
            // Debug.Log("=======end================");
        }



        public override void End()
        {

        }

        public void SortBy()
        {
            if (mData.Count > 0)
            {
                Dictionary<string, MyData> sortedDic = mData.OrderByDescending(o => o.Value.file).ToDictionary(p => p.Key, o => o.Value);
                mData = sortedDic;
            }
        }

        private void AddData(string strKey, GameObject go, string reson)
        {
            if(mData.ContainsKey(strKey))
            {
                var resonadd = mData[strKey];
                mData[strKey].reason = resonadd + "|"+ reson;
            }
            else
            {
                mData[strKey] = new MyData(strKey, reson, 0, false);
            }

            if (!mGameObjectList.ContainsKey(strKey))
            {
                mNo = mNo + 1;                
                mGameObjectList[strKey] = go;
            };            
        }

        private void CheckOnePrefab(string filePath)
        {

            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
            if (go == null || !go.isStatic)
                return;


            // 找到所有子节点包含Mesh的对象
            var meshRenders = go.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var mesh in meshRenders)
            {
                if (mesh.receiveShadows || mesh.shadowCastingMode != ShadowCastingMode.Off)
                {
                    
                    AddData(filePath, go, "MeshRenderer R&C Shadows is not off");
                }

                if (mesh.motionVectorGenerationMode != MotionVectorGenerationMode.ForceNoMotion)
                {
                    AddData(filePath, go, "MeshRenderer Motion Vector is not off");
                }

                if (mesh.lightProbeUsage != LightProbeUsage.Off || mesh.reflectionProbeUsage != ReflectionProbeUsage.Off)
                {
                    AddData(filePath, go, "MeshRenderer L&R is not off");
                }               
            }

            var meshsCollider = go.GetComponentsInChildren<MeshCollider>(true);
            foreach (var mesh in meshRenders)
            {
              AddData(filePath, go, "MeshCollider R&C Shadows is not off");             
            }

            var psS = go.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in psS)
            {
                if (ps.shape.enabled && (ps.shape.shapeType == ParticleSystemShapeType.Mesh || ps.shape.shapeType == ParticleSystemShapeType.SkinnedMeshRenderer || ps.shape.shapeType == ParticleSystemShapeType.MeshRenderer))
                {
                    AddData(filePath, go, "ParticleSystemRenderer R&C Shadows is not off");
                }
                if (ps.GetComponent<ParticleSystemRenderer>().shadowCastingMode != ShadowCastingMode.Off || ps.GetComponent<ParticleSystemRenderer>().receiveShadows)
                {
                    AddData(filePath, go, "ParticleSystemRenderer Motion Vector is not off");
                }
                if (ps.GetComponent<ParticleSystemRenderer>().motionVectorGenerationMode != MotionVectorGenerationMode.ForceNoMotion)
                {
                }
                if (ps.GetComponent<ParticleSystemRenderer>().lightProbeUsage != LightProbeUsage.Off || ps.GetComponent<ParticleSystemRenderer>().reflectionProbeUsage != ReflectionProbeUsage.Off)
                {
                    AddData(filePath, go, "ParticleSystemRenderer Motion Vector is not off");
                }
            }
        }
        private void SearchGameObjects()
        {
            var all = mAllPath;
            for (var i = 0; i < all.Length; ++i)
            {
                var guid = all[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                CheckOnePrefab(assetPath);    

            }
        }

        public void Process()
        {
            foreach (var obj in mData)
            {
                string filePath = obj.Key;
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
                if (go == null || !go.isStatic)
                    return;
                // 找到所有子节点包含Mesh的对象
                bool isDirty = false;
                MeshRenderer[] renders = go.GetComponentsInChildren<MeshRenderer>(true);
                if (renders.Length > 0)
                {
                    foreach (var render in renders)
                    {
                        if (render.motionVectorGenerationMode != (MotionVectorGenerationMode.ForceNoMotion))
                        {
                            render.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                            isDirty = true;
                        }
                    }
                }
                if (isDirty)
                {
                    EditorUtility.SetDirty(go);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.Default);
        }


    }
}