using UnityEngine;
using UnityEditor;
using UnityEditor.Macros;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleGrass
{
    public class TmpLightMapData
    {
        public int protoTypeIndex;
        public Dictionary<long, long> protoTypeLightMapIndex = new Dictionary<long, long>();
        public Dictionary<long, long> protoTypeLightMapIndex2 = new Dictionary<long, long>();
        public Dictionary<long, int> protoTypeLightMapNum = new Dictionary<long, int>();
    }    

    public class SimpleGrassSysDataHandler
    {

        //hashcode和MD5对照表
        public static Dictionary<long, String> TmpHashCode_MD5Dict = new Dictionary<long, string>();

        public class CompareGroupByProtoIndex : IComparer<InstanceChunkData>
        {
            public int Compare(InstanceChunkData x, InstanceChunkData y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                if (x.PrototypeIndex < y.PrototypeIndex) return -1;
                if (x.PrototypeIndex > y.PrototypeIndex) return 1;
                return 0;
            }
        }

        static private CompareGroupByProtoIndex compareGroupByProtoIndex = new CompareGroupByProtoIndex();
        


        //保存数据到ASSET文件中
        static public void SaveData(ref GameObject root, ref GameObject rootCollider, ref SimpleSaveData saveData, bool saveToBuffer, Vector3 optimizeChunkParams, bool optimizeSaveChunk)
        {
            TmpHashCode_MD5Dict.Clear();

            int priorVer = saveData.Ver;

            saveData.Clear();

            saveData.Ver = Common.Ver;//版本号

            string sceneTag = "";

            //保存种植的物件(SimpleGrass节点下的数据）
            SaveData_SimpleGrassNode(ref root, ref saveData, priorVer,true, sceneTag);

            //保存Terrian地型采集来的植被数据
            GameObject terrainSampleRoot = GameObject.Find("TerrainSample");
            if (terrainSampleRoot != null)
            {
                SaveData_SimpleGrassNode(ref terrainSampleRoot, ref saveData, priorVer,false, sceneTag);
            }
            
            //清除旧数据
            if (rootCollider)
            {
                for (int i = 0; i < rootCollider.transform.childCount; i++)
                {
                    GameObject.DestroyImmediate(rootCollider.transform.GetChild(i).gameObject);
                }
            }
            
            //扫描：附加的扫描节点  
            bool scanning = false;         
            if (SaveDataScanning(ref saveData, ref rootCollider, sceneTag))
            {
                scanning = true;
            }

            //扫描所有其它的SimpleGrassProtoInfo的扫描目录
            if(SaveDataScanning_Ext(ref saveData,ref rootCollider, sceneTag))
            {
                scanning = true;
            }

            //植被组，进一步优化合并
            if (optimizeSaveChunk)
            {
                float optimizeRadius = optimizeChunkParams.x;// 3.0f;
                float optimizeMaxOffset = optimizeChunkParams.y;// 3.0f;
                float optimizeContainScale = optimizeChunkParams.z;// 2.0f / 3.0f;            
                _TryOptimizeMergeChunk(saveData, optimizeRadius, optimizeMaxOffset, optimizeContainScale);
            }

            if (scanning)
            {
                //扫描时原始节点可能没有次序，要重新排序
                saveData.ChunkData.Sort(compareGroupByProtoIndex);
            }

            //保存hashCode和MD5的对照表
            saveData.Codes.Clear();
            saveData.CodeMD5.Clear();
            foreach (var val in TmpHashCode_MD5Dict)
            {
                saveData.Codes.Add(val.Key);
                saveData.CodeMD5.Add(val.Value);
            }
            TmpHashCode_MD5Dict.Clear();

            //植被数据保存了BYTE模式
            saveData.ChunkDataBuff = null;
            if (saveToBuffer)
            {
                SimpleSaveDataHelper.BuildDataBuffer(saveData);
            }

            EditorUtility.SetDirty(saveData);
            AssetDatabase.SaveAssets();
        }

        static public void SaveData_SimpleGrassNode(ref GameObject root, ref SimpleSaveData saveData,int priorVer, bool tryMergeChunk, string sceneTag)
        {           
            if(!root)
            {
                return;
            }
            int verNum = saveData.Ver;
            int childCount = root.transform.childCount;
            GameObject obj = null;
            GameObject objChunck = null;
            string protoTypeName = "";

            Vector3 minBound = Vector3.zero;
            Vector3 maxBound = Vector3.zero;
            //遍历ProtoType 原型
            for (int i = 0; i < childCount; ++i)
            {
                Dictionary<long, long> protoTypeLightMapIndex = new Dictionary<long, long>();
                Dictionary<long, long> protoTypeLightMapIndex2 = new Dictionary<long, long>();
                Dictionary<long, int> protoTypeLightMapNum = new Dictionary<long, int>();

                obj = root.transform.GetChild(i).gameObject;
                int chunckCount = obj.transform.childCount;
                if (chunckCount == 0)
                    continue;

                //修复：节点增加，原型自定义信息
                SimpleGrassProtoInfo protoInfoPtr = RepairProtoInfo(obj);
                if (!protoInfoPtr)
                {
                    continue;
                }                
                //(正常放置的植被)旧版本，密度进行削减
               // float correctDen = Common.CorrectPrototypeDensity(protoInfoPtr.Density, priorVer);
               // protoInfoPtr.Density = correctDen;                

                bool isFirstChunk = false;
                bool isFirstGrass = false;
                protoTypeName = obj.name;

                int protoTypeIdx = saveData.ChunkDict_FindAndNew(protoTypeName);
                string strVal = saveData.ProtoTypes[protoTypeIdx].ChunkPrebab;
                isFirstChunk = (strVal.CompareTo("") == 0);

                protoTypeIdx = saveData.GrassDict_FindAndNew(protoTypeName);
                GameObject objVal = saveData.ProtoTypes[protoTypeIdx].GrassPrebab;
                isFirstGrass = (objVal == null);

                //一颗实例,MESH的包围盒
                ///遍历块               
                int chunkNum = 0;
                List<int> meshLodIdxList = new List<int>();
                List<MeshRenderer> meshRenderList = new List<MeshRenderer>();
                List<MeshFilter> meshFilterList = new List<MeshFilter>();

                for (int chunckIdx = 0; chunckIdx < chunckCount; ++chunckIdx)
                {
                    objChunck = obj.transform.GetChild(chunckIdx).gameObject;
                    //数据检查
                    int grassCount = objChunck.transform.childCount;
                                            
                    SimpleGrassChunk chuckScriptPrefab = GetSimpleGrassChunkPrefab(objChunck);
                    SimpleGrassChunk chuckScript = objChunck.GetComponent<SimpleGrassChunk>();
                    InstanceChunkData chunckData = new InstanceChunkData();
                    chunckData.PrototypeIndex = protoTypeIdx;
                    chunckData.worldPos = objChunck.transform.position;
                    //对应实例预制体
                    if (isFirstChunk)
                    {
                        string chunkPrefabPath = "";
                        if (chuckScriptPrefab != null && chuckScriptPrefab.selfPrefab != null)
                        {
                            chunkPrefabPath = AssetDatabase.GetAssetPath(chuckScriptPrefab);
                            saveData.ProtoTypes[protoTypeIdx].ChunkPrebab = chunkPrefabPath;
                            isFirstChunk = false;
                        }else
                        {
                            saveData.ProtoTypes[protoTypeIdx].ChunkPrebab = "null";
                            isFirstChunk = false;
                        }

                        {
                            saveData.ProtoTypes[protoTypeIdx].CullingMaxDist = protoInfoPtr.CullingMaxDistance;
                            saveData.ProtoTypes[protoTypeIdx].CastShadows = protoInfoPtr.CastShadows;
                            saveData.ProtoTypes[protoTypeIdx].ReceiveShadows = protoInfoPtr.ReceiveShadows;
                            saveData.ProtoTypes[protoTypeIdx].Layer = protoInfoPtr.LayerID;
                            saveData.ProtoTypes[protoTypeIdx].Density= protoInfoPtr.Density;
                            saveData.ProtoTypes[protoTypeIdx].UseLightProb = protoInfoPtr.useLightProbe;
                            //批量放置的植被，是否使用：距离LOD模式显示。
                            saveData.ProtoTypes[protoTypeIdx].Tag = protoInfoPtr.batchMeshLOD ? (int)Common.ProtoMode.BatchMode_LOD : 0;
                        }
                    }

                    if (isFirstGrass)
                    {
                        if (chuckScriptPrefab.GrassPrefab != null)
                        {
                            saveData.ProtoTypes[protoTypeIdx].GrassPrebab = chuckScriptPrefab.GrassPrefab;
                            isFirstGrass = false;
                        }
                    }

                    if (grassCount == 0)
                    {
                        continue;
                    }

                    //实例(GRASS)
                    bool boundFirst = true;
                    Bounds cullingChunkBound = new Bounds(Vector3.zero, Vector3.zero);

                    string prefabName = chuckScriptPrefab.GrassPrefab.name;
                    float mergeChunkDistance = protoInfoPtr.MergeChunkDistance;

                    GameObject objGrass = null;

                    //块下所有实例数据
                    List<Vector3> trsList = null;
                    List<Vector4> lsoList = null;
                    List<Vector4> colorList = null;
                    List<Vector3> grassPosList = new List<Vector3>();
                    float maxLodScale = 0.0f;
                    for (int grassIdx = 0; grassIdx < grassCount; ++grassIdx)
                    {
                        objGrass = objChunck.transform.GetChild(grassIdx).gameObject;

                        InstData grassData = new InstData();

                        Dictionary<long, List<Vector3>> objTRS = new Dictionary<long, List<Vector3>>();
                        Dictionary<long, List<Vector4>> objLSO = new Dictionary<long, List<Vector4>>();
                        Dictionary<long, List<Vector4>> objColor = new Dictionary<long, List<Vector4>>();

                        //草，世界矩阵                        
                        TryGetMeshData(saveData.Ver,objGrass, ref meshRenderList, ref meshFilterList, ref meshLodIdxList);
                        int[] meshLodIdxAry = meshLodIdxList.ToArray();
                        MeshRenderer[] meshRenderAry = meshRenderList.ToArray();
                        MeshFilter[] meshFilterAry = meshFilterList.ToArray();

                        //保存meshrender所在节点的父节点(LODGROUP距离计算的参数点或父节点）
                        TryGetLodGroupData(ref meshRenderAry, ref grassData);
                        maxLodScale = Mathf.Max(grassData.maxScale, maxLodScale);

                        bool grassBoundFirst = true;
                        Bounds cullingGrassBound = new Bounds(Vector3.zero, Vector3.zero);
                        bool isLod = (meshLodIdxAry.Length == meshRenderAry.Length);
                        int lodIndex = -1;

                       
                        for (int meshIdx = 0; meshIdx < meshRenderAry.Length; ++meshIdx)
                        {
                            //增加lightmap信息
                            int lightmapIndex = -1;
                            if(isLod)
                            {
                                lodIndex = meshLodIdxAry[meshIdx];
                            }
                            string meshName = meshFilterAry[meshIdx].sharedMesh.name; //string meshRenderName = meshRenderAry[meshIdx].name;
                            string codeMD5_nolm = "";
                            long code = Common.GetMeshHashCode_NoLightmapIdx(verNum,ref sceneTag,ref protoTypeName, ref prefabName, ref meshName,lodIndex,ref codeMD5_nolm);//meshRenderName
                            AppendHashCode_MD5(code, codeMD5_nolm);
                            //Common.Debug("1==" + verNum.ToString() + "  " + protoTypeName + "  " + prefabName + "  " + meshName + "  " + lodIndex.ToString() + " ** " + code);

                            Vector4 lightmapScaleOffset = Vector4.zero;
                            long protoTypeLMIndex = 0;
                            long protoTypeLMIndex2 = 0;
                            int protoTypeLMNum = 0;
                            if (protoTypeLightMapIndex.ContainsKey(code))
                            {
                                protoTypeLMIndex = protoTypeLightMapIndex[code];
                            }
                            if (protoTypeLightMapIndex2.ContainsKey(code))
                            {
                                protoTypeLMIndex2 = protoTypeLightMapIndex2[code];
                            }

                            if (protoTypeLightMapNum.ContainsKey(code))
                            {
                                protoTypeLMNum = protoTypeLightMapNum[code];
                            }
                            if (!protoInfoPtr.useLightProbe)
                            {
                                Common.GetLightmapInfo2(meshRenderAry[meshIdx], protoTypeLMIndex, protoTypeLMIndex2, protoTypeLMNum,
                                   out lightmapIndex, out lightmapScaleOffset, out protoTypeLMIndex, out protoTypeLMIndex2, out protoTypeLMNum);
                            }else
                            {
                                lightmapIndex = Common.INDEXTAG_USELIGHTPROBE;// - 2;
                             }

                            protoTypeLightMapIndex[code] = protoTypeLMIndex;
                            protoTypeLightMapIndex2[code] = protoTypeLMIndex2;
                            protoTypeLightMapNum[code] = protoTypeLMNum;

                            ///
                            Transform parentTF = meshRenderAry[meshIdx].gameObject.transform;
                            string codeMD5 = "";
                            long meshCode = Common.GetMeshHashCode(verNum, ref sceneTag, ref protoTypeName, ref prefabName, ref meshName, lodIndex, lightmapIndex,ref codeMD5);
                            AppendHashCode_MD5(meshCode, codeMD5);
                            // Common.Debug("2==" + verNum.ToString() + "  " + protoTypeName + "  " + prefabName + "  " + meshName + "  " + lodIndex.ToString() + " lm: " + lightmapIndex.ToString() + "  ** " + code);
                            if (objTRS.TryGetValue(meshCode, out trsList))
                            {
                                trsList.Add(parentTF.position);
                                trsList.Add(parentTF.rotation.eulerAngles);
                                trsList.Add(parentTF.lossyScale);
                            }
                            else
                            {
                                List<Vector3> newtrsList = new List<Vector3>();
                                newtrsList.Add(parentTF.position);
                                newtrsList.Add(parentTF.rotation.eulerAngles);
                                newtrsList.Add(parentTF.lossyScale);
                                objTRS.Add(meshCode, newtrsList);
                            }

                            ////lightmap 相关
                            if (!protoInfoPtr.useLightProbe)
                            {
                                if (objLSO.TryGetValue(meshCode, out lsoList))
                                {
                                    lsoList.Add(lightmapScaleOffset);
                                }
                                else
                                {
                                    List<Vector4> newlsoList = new List<Vector4>();
                                    newlsoList.Add(lightmapScaleOffset);
                                    objLSO.Add(meshCode, newlsoList);
                                }
                            }

                            Vector3 color = chuckScript.childColors[grassIdx];
                            if (objColor.TryGetValue(meshCode, out colorList))
                            {
                                colorList.Add(color);
                            }
                            else
                            {
                                List<Vector4> newcolorList = new List<Vector4>();
                                newcolorList.Add(color);
                                objColor.Add(meshCode, newcolorList);
                            }

                            if (grassBoundFirst)
                            {
                                cullingGrassBound = meshRenderAry[meshIdx].bounds;
                                grassBoundFirst = false;
                            }
                            else
                            {
                                cullingGrassBound.Encapsulate(meshRenderAry[meshIdx].bounds);
                            }

                            if (boundFirst)
                            {
                                cullingChunkBound = meshRenderAry[meshIdx].bounds;
                                boundFirst = false;
                            }
                            else
                            {
                                cullingChunkBound.Encapsulate(meshRenderAry[meshIdx].bounds);
                            }
                        }

                        grassData.Collider[0] = cullingGrassBound.center;
                        grassData.Collider[1] = cullingGrassBound.size;

                        int ii = 0;
                        foreach (var trs in objTRS)
                        {
                            grassData.meshCodes.Add(trs.Key);
                            grassData.AppendDetailTRS(ii, trs.Value);
                            //light map index 相关
                            if (objLSO.TryGetValue(trs.Key, out lsoList))
                            {
                                grassData.AppendDetailLSO(ii, lsoList);
                            }
                            if (objColor.TryGetValue(trs.Key, out colorList))
                            {
                                grassData.AppendDetailColor(ii, colorList);
                            }
                            ++ii;
                        }

                        ///test lightprob
                        Vector3 grassPos = objGrass.gameObject.transform.position;
                        grassPosList.Add(grassPos);                        

                        chunckData.InstanceList.Add(grassData);
                    }//end for grass

                    if (protoInfoPtr.useLightProbe)
                    {
                        List<float> retLightProbeOcclusion = new List<float>();
                        if (Common.GetLightOcclusionProbes(grassPosList, retLightProbeOcclusion))
                        {
                            for(int idx = 0; idx < retLightProbeOcclusion.Count; ++idx)
                            {
                                chunckData.InstanceList[idx].lightProbOcc = (float)Math.Round(retLightProbeOcclusion[idx],3);
                            }
                        }
                    }
                    
                    //if (cullingChunkBound != null)
                    {
                        float radius = cullingChunkBound.size.x > cullingChunkBound.size.y ? cullingChunkBound.size.x : cullingChunkBound.size.y;
                        radius = radius > cullingChunkBound.size.z ? radius : cullingChunkBound.size.z;
                        chunckData.CullingCollider[0] = cullingChunkBound.center.x;
                        chunckData.CullingCollider[1] = cullingChunkBound.center.y;
                        chunckData.CullingCollider[2] = cullingChunkBound.center.z;
                        chunckData.CullingCollider[3] = radius * 0.5f;
                    }

                    chunckData.maxScale = maxLodScale;

                    //尝试合并靠近的块数据
                    if (tryMergeChunk && TryMergeChunkData(saveData, chunckData, mergeChunkDistance))
                    {

                    }
                    else
                    {
                        saveData.ChunkData.Add(chunckData);
                        ++chunkNum;
                    }
                }


                //各个类型的信息
                saveData.ProtoTypes[protoTypeIdx].ChuckNum = chunkNum;//“块”数量

                List<long> MeshCodeList = new List<long>();
                List<long> LightMapIndexList = new List<long>();
                List<long> LightMapIndexList2 = new List<long>();
                List<int> LightMapNumList = new List<int>();
                foreach (var data in protoTypeLightMapNum)
                {
                    if (protoTypeLightMapIndex.ContainsKey(data.Key))
                    {
                        MeshCodeList.Add(data.Key);
                        LightMapNumList.Add(data.Value);

                        long val = 0;
                        protoTypeLightMapIndex.TryGetValue(data.Key, out val);
                        LightMapIndexList.Add(val);

                        protoTypeLightMapIndex2.TryGetValue(data.Key, out val);
                        LightMapIndexList2.Add(val);
                    }
                }
                saveData.ProtoTypes[protoTypeIdx].MeshCode = MeshCodeList;
                saveData.ProtoTypes[protoTypeIdx].LightMapNum = LightMapNumList;//使用不同的lightmapindex数量
                saveData.ProtoTypes[protoTypeIdx].LightMapIndex = LightMapIndexList; //lightmapindex值                
                saveData.ProtoTypes[protoTypeIdx].LightMapIndex2 = LightMapIndexList2; //lightmapindex值              
            }           
        }


        static void MergeGrasses(InstanceChunkData existedChunk, InstanceChunkData chunkData)
        {
            int insCount = chunkData.InstanceList.Count;
            int existedInsCount = existedChunk.InstanceList.Count;
            if(existedInsCount == 0)
            {
                for (int insIdx = 0; insIdx < insCount; ++insIdx)
                {
                    existedChunk.InstanceList.Add(chunkData.InstanceList[insIdx]);
                }
                return;
            }
            ////           
            int insertIdx = 1;
            for (int i = 0; i < insCount; ++i)
            {
                if (insertIdx > existedChunk.InstanceList.Count)
                {
                    insertIdx = 1;
                }
                existedChunk.InstanceList.Insert(insertIdx, chunkData.InstanceList[i]);
                insertIdx += 2;               
            }
            
        }
        static bool TryMergeChunkData(SimpleSaveData saveData, InstanceChunkData chunkData, float mergeChunkDistance)
        {
            int protoTypeIdx = chunkData.PrototypeIndex;
            Vector3 chunkPos = chunkData.worldPos;//chunkData.TRS[0];

            int count = saveData.ChunkData.Count;
            float mergeChunkDistanceSQR = mergeChunkDistance * mergeChunkDistance;
            if (mergeChunkDistance <= 0)
            {
                return false;
            }

            for (int i = 0; i < count; ++i)
            {
                if (saveData.ChunkData[i].PrototypeIndex == protoTypeIdx && (saveData.ChunkData[i] != chunkData) )
                {
                    InstanceChunkData existedChunk = saveData.ChunkData[i];
                    Vector3 pos = existedChunk.worldPos;//existedChunk.TRS[0];
                    float distance = (chunkPos - pos).sqrMagnitude;
                    bool isContained = Common.IsContainSphere(existedChunk.CullingCollider, chunkData.CullingCollider);
                    if (distance < mergeChunkDistanceSQR || isContained)//Common.MergeChunkDistanceSQR
                    {
                        if ((existedChunk.InstanceList.Count + chunkData.InstanceList.Count) <= Common.MergeChunkInstanceLimit)
                        {
                            _MergeChunkData(ref existedChunk, ref chunkData);
                            ////合并裁剪包围球
                            //Vector3 mergeCullingCenter = Vector3.zero;
                            //float mergeCullingRadius = 0.0f;                            
    
                            //Common.CalMergeSphereBound(existedChunk.CullingCollider, chunkData.CullingCollider, out mergeCullingCenter, out mergeCullingRadius);
                            //existedChunk.CullingCollider[0] = mergeCullingCenter.x;
                            //existedChunk.CullingCollider[1] = mergeCullingCenter.y;
                            //existedChunk.CullingCollider[2] = mergeCullingCenter.z;
                            //existedChunk.CullingCollider[3] = mergeCullingRadius;
                            ////数据 交叉保存，提升渲染密度控制时显示效果
                            //MergeGrasses(existedChunk, chunkData);
                            ////修正属性
                            //existedChunk.maxScale = Mathf.Max(existedChunk.maxScale, chunkData.maxScale);

                            ////删除
                            //chunkData.InstanceList.Clear();
                            chunkData = null;

                            return true;
                        }

                    }//end distance < Common.MergeChunkDistance
                }

            }//end for

            return false;
        }

        static void _MergeChunkData(ref InstanceChunkData existedChunk, ref InstanceChunkData chunkData)
        {
            //合并裁剪包围球
            Vector3 mergeCullingCenter = Vector3.zero;
            float mergeCullingRadius = 0.0f;

            Common.CalMergeSphereBound(existedChunk.CullingCollider, chunkData.CullingCollider, out mergeCullingCenter, out mergeCullingRadius);
            existedChunk.CullingCollider[0] = mergeCullingCenter.x;
            existedChunk.CullingCollider[1] = mergeCullingCenter.y;
            existedChunk.CullingCollider[2] = mergeCullingCenter.z;
            existedChunk.CullingCollider[3] = mergeCullingRadius;
            //数据 交叉保存，提升渲染密度控制时显示效果
            MergeGrasses(existedChunk, chunkData);
            //修正属性
            existedChunk.maxScale = Mathf.Max(existedChunk.maxScale, chunkData.maxScale);

            //删除
            chunkData.InstanceList.Clear();

        }

        static bool  _TryOptimizeMergeChunk(SimpleSaveData saveData,  float optimizeRadius,float optimizeMaxOffset, float optimizeContainScale)
        {           
            int count = saveData.ChunkData.Count;
            List<InstanceChunkData> willDelList = new List<InstanceChunkData>();

            for (int j = 0; j < count; ++j)
            {
                InstanceChunkData chunkData = saveData.ChunkData[j];
                int protoTypeIdx = chunkData.PrototypeIndex;
                Vector3 chunkPos = chunkData.worldPos;
                float rA = chunkData.CullingCollider[3];
                bool willSamllChunk = (rA <= optimizeRadius);
                float minDistance = 999999f;
                int mergeIndex = -1;
                for (int i = 0; i < count; ++i)
                {
                    InstanceChunkData existedChunk = saveData.ChunkData[i];
                    if (existedChunk.PrototypeIndex == protoTypeIdx && (existedChunk != chunkData) && (existedChunk.InstanceList.Count > 0))
                    {                        
                        Vector3 pos = existedChunk.worldPos;
                        float rB = existedChunk.CullingCollider[3];
                        float distance = (chunkPos - pos).magnitude;
                        bool isContained = Common.IsContainSphere(existedChunk.CullingCollider, chunkData.CullingCollider);
                        if ((existedChunk.InstanceList.Count + chunkData.InstanceList.Count) <= Common.MergeChunkInstanceLimit)
                        {
                            //被包含
                            if(isContained)
                            {
                                mergeIndex = i;
                                break;
                            }
                            //优化较小的组,合并到适合范围的最近的组。
                            if(willSamllChunk)
                            {
                                if ((distance - rA - rB <= optimizeMaxOffset) && distance < minDistance)
                                {
                                    minDistance = distance;
                                    mergeIndex = i;
                                }
                            }
                            //优化大部分被包含，合并到适合范围的最近的组。                            
                            else if (optimizeContainScale >= 0.0f && rB >= rA && distance <= (rA + rB - (optimizeContainScale * 2 * rA)) && distance < minDistance)
                            {
                                minDistance = distance;
                                mergeIndex = i;
                            }
                        }                        
                    }
                }

                //chunkData ==>saveData.ChunkData[mergeIndex]
                if (mergeIndex != -1)
                {
                    InstanceChunkData existedChunk = saveData.ChunkData[mergeIndex];
                    _MergeChunkData(ref existedChunk, ref chunkData);
                    willDelList.Add(chunkData);
                }
            }


            for (int i = 0; i < willDelList.Count; ++i)
            {
                saveData.ChunkData.Remove(willDelList[i]);
            }

            //更新组数量
            count = saveData.ChunkData.Count;
            int prototypeCount = saveData.ProtoTypes.Count;
            for (int prototypeIndex = 0; prototypeIndex < prototypeCount; ++prototypeIndex)
            {
                int chunckNum = 0;
                for (int j = 0; j < count; ++j)
                {
                    if (saveData.ChunkData[j].PrototypeIndex == prototypeIndex)
                    {
                        ++chunckNum;
                    }
                }
                saveData.ProtoTypes[prototypeIndex].ChuckNum = chunckNum;
            }

            return true;
        }

        static public bool SaveDataScanning( ref SimpleSaveData saveData,ref GameObject rootCollider, string sceneTag)
        {
            GameObject scanRoot = GameObject.Find("SimpleGrass_Scan");
            if (scanRoot == null)
            {
                return false;
            }

            //int mergeChunkDistance = 10;
            Dictionary<int, TmpLightMapData> protoTypeLightMaps = new Dictionary<int, TmpLightMapData>();

            //克隆保存碰撞体
            //SaveDataCollider(scanRoot, ref rootCollider);
            //递归扫描节点            
            ScanningGameObjectData(ref scanRoot, ref saveData, ref protoTypeLightMaps, null, sceneTag);

            //整理LIGHTMAP数据           
            foreach (var lm in protoTypeLightMaps)
            {
                List<long> MeshCodeList = new List<long>();
                List<long> LightMapIndexList = new List<long>();
                List<long> LightMapIndexList2 = new List<long>();
                List<int> LightMapNumList = new List<int>();
                int protoTypeIdx = lm.Key;
                foreach (var data in lm.Value.protoTypeLightMapNum)
                {
                    if (lm.Value.protoTypeLightMapIndex.ContainsKey(data.Key))
                    {
                        MeshCodeList.Add(data.Key);
                        LightMapNumList.Add(data.Value);

                        long val = 0;
                        lm.Value.protoTypeLightMapIndex.TryGetValue(data.Key, out val);
                        LightMapIndexList.Add(val);

                        lm.Value.protoTypeLightMapIndex2.TryGetValue(data.Key, out val);
                        LightMapIndexList2.Add(val);
                    }
                }
                saveData.ProtoTypes[protoTypeIdx].MeshCode = MeshCodeList;
                saveData.ProtoTypes[protoTypeIdx].LightMapNum = LightMapNumList;//使用不同的lightmapindex数量
                saveData.ProtoTypes[protoTypeIdx].LightMapIndex = LightMapIndexList; //lightmapindex值                
                saveData.ProtoTypes[protoTypeIdx].LightMapIndex2 = LightMapIndexList2; //lightmapindex值 
            }

            //EditorUtility.SetDirty(saveData);
            //AssetDatabase.SaveAssets();
            return true;
        }

        static public void ScanningOneNode_Ext(ref SimpleSaveData saveData,ref Dictionary<int, TmpLightMapData>  protoTypeLightMaps, GameObject objNode, ref GameObject rootCollider, string sceneTag)
        {
            //不扫描： SimpleGrass_Scan，SimpleGrass 节点
            if (!objNode.activeSelf || objNode.name.CompareTo("SimpleGrass_Scan") == 0 || objNode.name.CompareTo("SimpleGrass") == 0 || objNode.name.CompareTo("TerrainSample") == 0)
            {
                return;
            }
            //判断：节点是否是可扫描目录
            SimpleGrassProtoInfo useProtoInfo = objNode.GetComponent<SimpleGrassProtoInfo>();
            if(!useProtoInfo)
            {
                //递归扫描子节点
                int childCount = objNode.transform.childCount;
                for (int i = 0; i != childCount; ++i)
                {
                    GameObject node = objNode.transform.GetChild(i).gameObject;
                    if (node.activeSelf)
                    {
                        ScanningOneNode_Ext(ref saveData,ref protoTypeLightMaps,node, ref rootCollider, sceneTag);
                    }
                }
                return;
            }
            //克隆保存碰撞体
            //SaveDataCollider(objNode, ref rootCollider);
            //递归扫描节点, 收集信息
            ScanningGameObjectData(ref objNode,ref saveData, ref protoTypeLightMaps, useProtoInfo, sceneTag);
        }

        static public bool SaveDataScanning_Ext(ref SimpleSaveData saveData, ref GameObject rootCollider, string sceneTag)
        {
            //int mergeChunkDistance = 10;
            Dictionary<int, TmpLightMapData> protoTypeLightMaps = new Dictionary<int, TmpLightMapData>();

            foreach (GameObject rootObj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                ScanningOneNode_Ext(ref saveData, ref protoTypeLightMaps, rootObj, ref rootCollider, sceneTag);

            }
                                         
            //整理LIGHTMAP数据           
            foreach (var lm in protoTypeLightMaps)
            {
                List<long> MeshCodeList = new List<long>();
                List<long> LightMapIndexList = new List<long>();
                List<long> LightMapIndexList2 = new List<long>();
                List<int> LightMapNumList = new List<int>();
                int protoTypeIdx = lm.Key;
                foreach (var data in lm.Value.protoTypeLightMapNum)
                {
                    if (lm.Value.protoTypeLightMapIndex.ContainsKey(data.Key))
                    {
                        MeshCodeList.Add(data.Key);
                        LightMapNumList.Add(data.Value);

                        long val = 0;
                        lm.Value.protoTypeLightMapIndex.TryGetValue(data.Key, out val);
                        LightMapIndexList.Add(val);

                        lm.Value.protoTypeLightMapIndex2.TryGetValue(data.Key, out val);
                        LightMapIndexList2.Add(val);
                    }
                }
                saveData.ProtoTypes[protoTypeIdx].MeshCode = MeshCodeList;
                saveData.ProtoTypes[protoTypeIdx].LightMapNum = LightMapNumList;//使用不同的lightmapindex数量
                saveData.ProtoTypes[protoTypeIdx].LightMapIndex = LightMapIndexList; //lightmapindex值                
                saveData.ProtoTypes[protoTypeIdx].LightMapIndex2 = LightMapIndexList2; //lightmapindex值 
            }

            //EditorUtility.SetDirty(saveData);
            //AssetDatabase.SaveAssets();
            return true;
        }

        static public void SaveDataCollider(GameObject obj, ref GameObject rootCollider)
        {
            if (rootCollider == null)
            {
                GameObject root = new GameObject(SimpleEditorCommon.SaveColliderNodeName);
                // root.tag = "EditorOnly";
                root.transform.localPosition = Vector3.zero;
                root.transform.localEulerAngles = Vector3.zero;
                root.transform.localScale = Vector3.one;
                rootCollider = root;
            }

            //按Layer分类
            int childCount = rootCollider.transform.childCount;
            GameObject parentNode = null;
            for (int i = 0; i != childCount; ++i)
            {
                if (rootCollider.transform.GetChild(i).gameObject.layer == obj.layer)
                {
                    parentNode = rootCollider.transform.GetChild(i).gameObject;
                    continue;
                }
            }

            Collider[] Colliders = obj.GetComponentsInChildren<Collider>();
            if (Colliders.Length > 0 && parentNode == null)
            {
                parentNode = new GameObject(SimpleEditorCommon.ColliderParentPrefix + obj.layer.ToString());
                parentNode.layer = obj.layer;
                parentNode.transform.SetParent(rootCollider.transform);
            }

            Dictionary<GameObject, GameObject> cacheDatas = new Dictionary<GameObject, GameObject>();
            for (int i = 0; i != Colliders.Length; ++i)
            {
                GameObject cloneNode = null;
                if (!cacheDatas.TryGetValue(Colliders[i].gameObject, out cloneNode))
                {
                    //拷贝GameObject
                    cloneNode = new GameObject(Colliders[i].gameObject.name);
                    cloneNode.transform.localScale = Colliders[i].gameObject.transform.lossyScale;
                    cloneNode.transform.position = Colliders[i].gameObject.transform.position;
                    cloneNode.transform.rotation = Colliders[i].gameObject.transform.rotation;
                    cloneNode.layer = Colliders[i].gameObject.layer;

                    cloneNode.transform.SetParent(parentNode.transform);

                    cacheDatas[Colliders[i].gameObject] = cloneNode;
                }

                if (Colliders[i].GetType() == typeof(CapsuleCollider))
                {
                    CapsuleCollider newCollider = cloneNode.AddComponent<CapsuleCollider>();
                    CapsuleCollider collider = Colliders[i] as CapsuleCollider;

                    newCollider.enabled = collider.enabled;
                    newCollider.direction = collider.direction;
                    newCollider.material = collider.material;
                    newCollider.sharedMaterial = collider.sharedMaterial;
                    newCollider.tag = collider.tag;
                    newCollider.name = collider.name;
                    newCollider.isTrigger = collider.isTrigger;
                    newCollider.center = collider.center;
                    newCollider.radius = collider.radius;
                    newCollider.height = collider.height;
                    //newCollider.center = collider.bounds.center;
                    //newCollider.radius = Mathf.Max(collider.bounds.size.x, collider.bounds.size.z) * 0.5f;
                    //newCollider.height = collider.bounds.size.y;
                }
                else if (Colliders[i].GetType() == typeof(BoxCollider))
                {
                    BoxCollider newCollider = cloneNode.AddComponent<BoxCollider>();
                    BoxCollider collider = Colliders[i] as BoxCollider;

                    newCollider.enabled = collider.enabled;
                    newCollider.material = collider.material;
                    newCollider.sharedMaterial = collider.sharedMaterial;
                    newCollider.tag = collider.tag;
                    newCollider.name = collider.name;
                    newCollider.isTrigger = collider.isTrigger;
                    newCollider.center = collider.center;
                    newCollider.size = collider.size;
                    //newCollider.center = collider.bounds.center;
                    //newCollider.size = collider.bounds.size;
                }
                else if (Colliders[i].GetType() == typeof(SphereCollider))
                {
                    SphereCollider newCollider = cloneNode.AddComponent<SphereCollider>();
                    SphereCollider collider = Colliders[i] as SphereCollider;

                    newCollider.enabled = collider.enabled;
                    newCollider.material = collider.material;
                    newCollider.sharedMaterial = collider.sharedMaterial;
                    newCollider.tag = collider.tag;
                    newCollider.name = collider.name;
                    newCollider.isTrigger = collider.isTrigger;
                    newCollider.center = collider.center;
                    newCollider.radius = collider.radius;
                    //newCollider.center = collider.bounds.center;
                    //newCollider.radius = Mathf.Max(collider.bounds.size.x, collider.bounds.size.z) * 0.5f;
                }
                else if (Colliders[i].GetType() == typeof(MeshCollider))
                {
                    MeshCollider newCollider = cloneNode.AddComponent<MeshCollider>();
                    MeshCollider collider = Colliders[i] as MeshCollider;

                    newCollider.enabled = collider.enabled;
                    newCollider.material = collider.material;
                    newCollider.sharedMaterial = collider.sharedMaterial;
                    newCollider.tag = collider.tag;
                    newCollider.name = collider.name;
                    newCollider.convex = collider.convex;
                    newCollider.isTrigger = collider.isTrigger;
                    newCollider.sharedMesh = collider.sharedMesh;
                    newCollider.cookingOptions = collider.cookingOptions;
                }
            }
        }

        static bool BuildCustomAllMeshHashCode(MeshFilter[] allMeshFilterAry,  ref string retCode)
        {
            if (allMeshFilterAry.Length == allMeshFilterAry.Length)
            {
                StringBuilder sb = new StringBuilder();
                for (int meshIdx = 0; meshIdx < allMeshFilterAry.Length; ++meshIdx)
                {                   
                    string assetMesh = AssetDatabase.GetAssetPath(allMeshFilterAry[meshIdx].sharedMesh);
                    sb.Append(assetMesh);
                    sb.Append(allMeshFilterAry[meshIdx].sharedMesh.name);
                }
                retCode = sb.ToString().GetHashCode().ToString();
                return true;
            }
            return false;
        }

        static bool BuildCustomAllMeshHashCodeExt(MeshRenderer[] allMeshRenderAry, MeshFilter[] allMeshFilterAry, ref List<Mesh> sharedMeshs, ref List<MatList> sharedMaterials, ref string retCode)
        {
            if (allMeshRenderAry.Length > 0 && allMeshRenderAry.Length == allMeshFilterAry.Length)
            {
                StringBuilder sb = new StringBuilder();
                for (int meshIdx = 0; meshIdx < allMeshFilterAry.Length; ++meshIdx)
                {
                    sharedMeshs.Add(allMeshFilterAry[meshIdx].sharedMesh);

                    MatList matList = new MatList();
                    matList.Mats = allMeshRenderAry[meshIdx].sharedMaterials;
                    sharedMaterials.Add(matList);

                    string assetMesh = AssetDatabase.GetAssetPath(allMeshFilterAry[meshIdx].sharedMesh);
                    sb.Append(assetMesh);
                    sb.Append(allMeshFilterAry[meshIdx].sharedMesh.name);
                }
                retCode = sb.ToString().GetHashCode().ToString();                
                return true;
            }
            return false;
        }

        static int  BuildCustomProtoType(SimpleGrassProtoInfo DefProtoInfo,ref SimpleSaveData saveData, string protoTypeName, string prefabPath, out bool isNew)
        {
            bool isVal = false;
            int protoTypeIdx = saveData.ChunkDict_FindKey(protoTypeName);
            if (protoTypeIdx < 0)
            {
                protoTypeIdx = saveData.ChunkDict_FindAndNew(protoTypeName);
                saveData.ProtoTypes[protoTypeIdx].ChunkPrebab = prefabPath;

                if(DefProtoInfo)
                {
                    saveData.ProtoTypes[protoTypeIdx].CullingMaxDist = DefProtoInfo.CullingMaxDistance;
                    saveData.ProtoTypes[protoTypeIdx].CastShadows = DefProtoInfo.CastShadows;
                    saveData.ProtoTypes[protoTypeIdx].ReceiveShadows = DefProtoInfo.ReceiveShadows;
                    saveData.ProtoTypes[protoTypeIdx].Layer = DefProtoInfo.LayerID;
                    saveData.ProtoTypes[protoTypeIdx].Density = DefProtoInfo.Density;
                    saveData.ProtoTypes[protoTypeIdx].UseLightProb = DefProtoInfo.useLightProbe;
                }
                else
                {
                    saveData.ProtoTypes[protoTypeIdx].CullingMaxDist = SimpleEditorCommon.Def_CullingMaxDist;
                    saveData.ProtoTypes[protoTypeIdx].CastShadows = SimpleEditorCommon.Def_CastShadows;
                    saveData.ProtoTypes[protoTypeIdx].ReceiveShadows = SimpleEditorCommon.Def_ReceiveShadows;
                    saveData.ProtoTypes[protoTypeIdx].Layer = SimpleEditorCommon.Def_Layer;
                    saveData.ProtoTypes[protoTypeIdx].Density = 1.0f;
                    saveData.ProtoTypes[protoTypeIdx].UseLightProb = false;
                }                                           
                isVal = true;
            }
            isNew = isVal;
            return protoTypeIdx;
        }

        static void ScanningGameObjectData( ref GameObject scanRoot, ref SimpleSaveData saveData,//string protoType, string PrefabNameStr, string PrefabPath,
             ref Dictionary<int, TmpLightMapData> protoTypeLightMaps,SimpleGrassProtoInfo DefProtoInfo, string sceneTag)
        {

            SimpleGrassProtoInfo useProtoInfo = DefProtoInfo;
            if(scanRoot.GetComponent<SimpleGrassProtoInfo>())
            {
                useProtoInfo = scanRoot.GetComponent<SimpleGrassProtoInfo>();
            }

            float mergeChunkDistance = SimpleEditorCommon.Def_MergeChunkDistance;
            if (useProtoInfo)
            {
                mergeChunkDistance = useProtoInfo.MergeChunkDistance;
            }

            //情景1： 有关联的预制体
            string prefabPath = "";// PrefabPath;

            string protoTypeName = "";//protoType;
            string prefabName = "";//PrefabNameStr;
            if (string.IsNullOrEmpty(protoTypeName))
            {
                var prefabInst = UnityEditor.PrefabUtility.GetPrefabInstanceHandle(scanRoot);
                if (prefabInst)
                {
                    prefabPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(scanRoot);
                    var Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (Prefab)
                    {
                        string retCode = "";
                        retCode = prefabPath.GetHashCode().ToString();
                        protoTypeName =  Common.CustomPrefix_Prefab + retCode;
                        prefabName = retCode;

                        //构造，原型对象
                        bool isNew = false;
                        int protoTypeIdx = BuildCustomProtoType(useProtoInfo, ref saveData, protoTypeName, prefabPath, out isNew);
                        if (saveData.ProtoTypes[protoTypeIdx].GrassPrebab == null)
                        {
                            saveData.ProtoTypes[protoTypeIdx].GrassPrebab = Prefab;
                        }
                    }
                }
                //Debug.Log(prefabPath);
            }//end情景1

            //情景2：无预制体，但带有LODGROUP
            bool mustCustomDefine = (string.IsNullOrEmpty(protoTypeName) || string.IsNullOrEmpty(prefabName));
            //有LODGROUP的对象，可以参考作为一个原型种类  protoTypeName = "Scan_LOD_" + allmesh（assetpath+name);
            if (mustCustomDefine)
            {
                LODGroup lod = scanRoot.GetComponent<LODGroup>();
                if (lod)
                {
                    MeshFilter[] allMeshFilterAry = scanRoot.GetComponentsInChildren<MeshFilter>();
                    if (allMeshFilterAry.Length > 0)
                    {
                        string retCode = "";
                        BuildCustomAllMeshHashCode(allMeshFilterAry, ref retCode);
                        //protoTypeName = "Scan_" + （assetpath+name);
                        protoTypeName = Common.CustomPrefix_LOD + retCode;
                        prefabName = retCode;
                        bool isNew = false;
                        int protoTypeIdx = BuildCustomProtoType(useProtoInfo, ref saveData, protoTypeName, "", out isNew);
                        if (isNew)
                        {
                            ProtoTypeCustom custom = new ProtoTypeCustom();
                            custom.LODSize = lod.size;

                            LOD[] lods = lod.GetLODs();
                            custom.LodNum = 0;
                            for (int i = 0; i != lods.Length; ++i)
                            {
                                if (lods[i].renderers == null)
                                {
                                    continue;
                                }
                                List<MatList> listMeshMats = new List<MatList>();
                                List<Mesh> listMesh = new List<Mesh>();
                                foreach (var render in lods[i].renderers)
                                {
                                    if (render is MeshRenderer)
                                    {
                                        MatList oneMats = new MatList();
                                        oneMats.Mats = render.sharedMaterials;
                                        listMeshMats.Add(oneMats);
                                        MeshFilter meshFilter = render.gameObject.GetComponentInChildren<MeshFilter>();
                                        listMesh.Add(meshFilter.sharedMesh);
                                    }
                                }
                                if (listMeshMats.Count > 0 && listMeshMats.Count == listMesh.Count)
                                {
                                    custom.LodNum += 1;
                                    CustomLod lodData = new CustomLod();
                                    lodData.RelaHeight = lods[i].screenRelativeTransitionHeight;
                                    lodData.Meshs = listMesh;
                                    lodData.Mats = listMeshMats;
                                    custom.LODs.Add(lodData);
                                }
                            }
                            saveData.ProtoTypes[protoTypeIdx].Custom = custom;
                        }//end isNew                                                                                 
                    }
                }
            }//end 情景2


            mustCustomDefine = (string.IsNullOrEmpty(protoTypeName) || string.IsNullOrEmpty(prefabName));

            //情景1和情景2(有预制体或LODGROUP，可以理解为可能是一颗草）
            if(!mustCustomDefine)
            {
                ScanningGameObjectData_SaveOneGrass(protoTypeName, prefabName, ref scanRoot, ref saveData, ref protoTypeLightMaps, (int)mergeChunkDistance, sceneTag);
                return;
            }

            //情景3：MESH
           // Debug.Log("ScanningGameObjectData: 情景3 ---todo" );
            ScanningGameObjectData_SaveOneGrassExt(useProtoInfo, ref scanRoot, ref saveData, ref protoTypeLightMaps, sceneTag);


            //递归扫描子节点
            int childCount = scanRoot.transform.childCount;
            for (int i = 0; i != childCount; ++i)
            {
                GameObject node = scanRoot.transform.GetChild(i).gameObject;
                if (node.activeSelf)
                {
                    ScanningGameObjectData(ref node, ref saveData, ref protoTypeLightMaps, useProtoInfo, sceneTag);
                }
            }
        }

        static void ScanningGameObjectData_SaveOneGrass(string protoType, string PrefabNameStr, ref GameObject scanRoot, ref SimpleSaveData saveData,
             ref Dictionary<int, TmpLightMapData> protoTypeLightMaps,
             int mergeChunkDistance, string sceneTag)
        {

            int protoTypeIdx = saveData.ChunkDict_FindKey(protoType);

            //MESH            
            List<int> meshLodIdxList = new List<int>();
            List<MeshRenderer> meshRenderList = new List<MeshRenderer>();
            List<MeshFilter> meshFilterList = new List<MeshFilter>();
            TryGetMeshData(saveData.Ver, scanRoot, ref meshRenderList, ref meshFilterList, ref meshLodIdxList);
            int[] meshLodIdxAry = meshLodIdxList.ToArray();
            MeshRenderer[] meshRenderAry = meshRenderList.ToArray();
            MeshFilter[] meshFilterAry = meshFilterList.ToArray();

            //MeshRenderer[] meshRenderAry = scanRoot.GetComponentsInChildren<MeshRenderer>();
            //MeshFilter[] meshFilterAry = scanRoot.GetComponentsInChildren<MeshFilter>();
            if (meshRenderAry.Length > 0 && meshRenderAry.Length == meshFilterAry.Length)
            {
                //原型类型                
                if (!protoTypeLightMaps.ContainsKey(protoTypeIdx))
                {
                    protoTypeLightMaps[protoTypeIdx] = new TmpLightMapData();
                }
                //块
                InstanceChunkData chunckData = new InstanceChunkData();
                chunckData.PrototypeIndex = protoTypeIdx;
                chunckData.worldPos = scanRoot.transform.position;

                bool bOk = ScanningGameObjectData_SaveAllMesh(ref saveData, sceneTag, protoTypeIdx, ref protoType, ref PrefabNameStr, 
                    ref meshRenderAry, ref meshFilterAry,ref meshLodIdxAry, ref protoTypeLightMaps,
                    ref chunckData);

                if(bOk)
                {
                    //尝试合并靠近的块数据
                    if (TryMergeChunkData(saveData, chunckData, mergeChunkDistance))
                    {

                    }
                    else
                    {
                        saveData.ChunkData.Add(chunckData);
                        saveData.ProtoTypes[protoTypeIdx].ChuckNum += 1;
                    }
                }
            }
        }


        static void ScanningGameObjectData_SaveOneGrassExt(SimpleGrassProtoInfo DefProtoInfo, ref GameObject scanRoot, ref SimpleSaveData saveData,
             ref Dictionary<int, TmpLightMapData> protoTypeLightMaps,string sceneTag)
        {
            string prefabPath = "";
            string protoTypeName = "";
            string prefabName = "";            
            
            //MESH                        
            MeshRenderer[] meshRenderAry = scanRoot.GetComponents<MeshRenderer>();
            MeshFilter[] meshFilterAry = scanRoot.GetComponents<MeshFilter>();
            int[] lodIDAry = null;
            if (meshRenderAry.Length > 0 && meshRenderAry.Length == meshFilterAry.Length)
            {
                //情景3：单独的MESH， 无关联预制体，无所归属的LODGROUP
                List<Mesh> sharedMeshs = new List<Mesh>();
                List<MatList> sharedMaterials = new List<MatList>();
      
                //protoTypeName = "Scan_" + hashcode(mesh的AssetPath + name)
                string retCode = "";
                BuildCustomAllMeshHashCodeExt(meshRenderAry, meshFilterAry, ref sharedMeshs, ref sharedMaterials, ref retCode);

                protoTypeName = Common.CustomPrefix_Mesh + retCode;
                prefabName = retCode;
        
                //原型类型
                bool isNew = false;
                int protoTypeIdx = BuildCustomProtoType(DefProtoInfo,ref saveData, protoTypeName, prefabPath, out isNew);
                if (isNew)
                {
                    saveData.ProtoTypes[protoTypeIdx].Custom = new ProtoTypeCustom();
                    saveData.ProtoTypes[protoTypeIdx].Custom.LodNum = 0;
                    CustomLod customLod = new CustomLod();
                    customLod.Meshs = sharedMeshs;
                    customLod.Mats = sharedMaterials;
                    saveData.ProtoTypes[protoTypeIdx].Custom.LODs.Add(customLod);
                }


                //原型类型                
                if (!protoTypeLightMaps.ContainsKey(protoTypeIdx))
                {
                    protoTypeLightMaps[protoTypeIdx] = new TmpLightMapData();
                }

                //块
                InstanceChunkData chunckData = new InstanceChunkData();
                chunckData.PrototypeIndex = protoTypeIdx;
                chunckData.worldPos = scanRoot.transform.position;

                bool bOk = ScanningGameObjectData_SaveAllMesh(ref saveData,sceneTag, protoTypeIdx, ref protoTypeName, ref prefabName,
                    ref meshRenderAry, ref meshFilterAry,ref lodIDAry, ref protoTypeLightMaps,
                    ref chunckData);

                if (bOk)
                {
                    float mergeChunkDistance = SimpleEditorCommon.Def_MergeChunkDistance;
                    if (DefProtoInfo)
                    {
                        mergeChunkDistance = DefProtoInfo.MergeChunkDistance;
                    }

                    //尝试合并靠近的块数据
                    if (TryMergeChunkData(saveData, chunckData, mergeChunkDistance))
                    {

                    }
                    else
                    {
                        saveData.ChunkData.Add(chunckData);
                        saveData.ProtoTypes[protoTypeIdx].ChuckNum += 1;
                    }
                }
            }
           
        }

        static bool ScanningGameObjectData_SaveAllMesh(ref SimpleSaveData saveData,string sceneTag, int protoTypeIdx,ref string protoTypeName, ref string prefabName, 
            ref MeshRenderer[] meshRenderAry, ref MeshFilter[] meshFilterAry,ref int[] meshLodIdxAry,
             ref Dictionary<int, TmpLightMapData> protoTypeLightMaps,
             ref InstanceChunkData chunckData)
        {
            if (meshRenderAry.Length > 0 && meshRenderAry.Length == meshFilterAry.Length)
            {
                bool useLightProbe = saveData.ProtoTypes[protoTypeIdx].UseLightProb;
                Vector3 grassPos = meshRenderAry[0].gameObject.transform.position;

                int verNum = saveData.Ver;
                //ver_1
                bool useLodID = false;
                if(meshLodIdxAry != null && meshLodIdxAry.Length == meshRenderAry.Length)
                {
                    useLodID = true;
                }
                //
                // 草
                InstData grassData = new InstData();

                List<Mesh> sharedMeshs = new List<Mesh>();
                List<MatList> sharedMaterials = new List<MatList>();

                //保存LODGROUP相关信息
                float maxLodScale = 0.0f;
                TryGetLodGroupData(ref meshRenderAry, ref grassData);
                maxLodScale = Mathf.Max(grassData.maxScale, maxLodScale);

                bool grassBoundFirst = true;
                Bounds cullingGrassBound = new Bounds(Vector3.zero, Vector3.zero);

                Dictionary<long, List<Vector3>> objTRS = new Dictionary<long, List<Vector3>>();
                Dictionary<long, List<Vector4>> objLSO = new Dictionary<long, List<Vector4>>();
                List<Vector3> trsList = null;
                List<Vector4> lsoList = null;
                 //Vector3 maxScale = Vector3.zero;
                for (int meshIdx = 0; meshIdx < meshRenderAry.Length; ++meshIdx)
                {
                    int lodIndex = -1;
                    if(useLodID)
                    {
                        lodIndex = meshLodIdxAry[meshIdx];
                    }
                    //增加lightmap信息                    
                    string meshName = meshFilterAry[meshIdx].sharedMesh.name; //string meshRenderName = meshRenderAry[meshIdx].name;
                    string codeMD5_nolm = "";
                    long code = Common.GetMeshHashCode_NoLightmapIdx(verNum,ref sceneTag,ref protoTypeName, ref prefabName, ref meshName, lodIndex,ref codeMD5_nolm);//meshRenderName 
                    AppendHashCode_MD5(code, codeMD5_nolm);
                    Vector4 lightmapScaleOffset = Vector4.zero;
                    int lightmapIndex = TryGetLightMapData(code, meshRenderAry[meshIdx],
                                                           ref protoTypeLightMaps[protoTypeIdx].protoTypeLightMapIndex,
                                                           ref protoTypeLightMaps[protoTypeIdx].protoTypeLightMapIndex2,
                                                           ref protoTypeLightMaps[protoTypeIdx].protoTypeLightMapNum,
                                                           out lightmapScaleOffset);

                    Transform parentTF = meshRenderAry[meshIdx].gameObject.transform;
                    string codeMD5 = "";
                    long meshCode = Common.GetMeshHashCode(verNum, ref sceneTag, ref protoTypeName, ref prefabName, ref meshName, lodIndex, lightmapIndex,ref codeMD5);
                    AppendHashCode_MD5(meshCode, codeMD5);
                    if (objTRS.TryGetValue(meshCode, out trsList))
                    {
                        trsList.Add(parentTF.position);
                        trsList.Add(parentTF.rotation.eulerAngles);
                        trsList.Add(parentTF.lossyScale);
                    }
                    else
                    {
                        List<Vector3> newtrsList = new List<Vector3>();
                        newtrsList.Add(parentTF.position);
                        newtrsList.Add(parentTF.rotation.eulerAngles);
                        newtrsList.Add(parentTF.lossyScale);
                        objTRS.Add(meshCode, newtrsList);
                    }

                    ////lightmap 相关
                    if (!useLightProbe)
                    {
                        if (objLSO.TryGetValue(meshCode, out lsoList))
                        {
                            lsoList.Add(lightmapScaleOffset);
                        }
                        else
                        {
                            List<Vector4> newlsoList = new List<Vector4>();
                            newlsoList.Add(lightmapScaleOffset);
                            objLSO.Add(meshCode, newlsoList);
                        }
                    }

                    if (grassBoundFirst)
                    {
                        cullingGrassBound = meshRenderAry[meshIdx].bounds;
                        grassBoundFirst = false;
                    }
                    else
                    {
                        cullingGrassBound.Encapsulate(meshRenderAry[meshIdx].bounds);
                    }
                }

                grassData.Collider[0] = cullingGrassBound.center;
                grassData.Collider[1] = cullingGrassBound.size;

                int ii = 0;
                foreach (var trs in objTRS)
                {
                    grassData.meshCodes.Add(trs.Key);
                    grassData.AppendDetailTRS(ii, trs.Value);
                    //light map index 相关
                    if (objLSO.TryGetValue(trs.Key, out lsoList))
                    {
                        grassData.AppendDetailLSO(ii, lsoList);
                    }
                    ++ii;
                }
                
                chunckData.InstanceList.Add(grassData);
                ///lightprobe                               
                if (useLightProbe)
                {
                    List<float> retLightProbeOcclusion = new List<float>();
                    List<Vector3> grassPosList = new List<Vector3>();
                    grassPosList.Add(grassPos);
                    if (Common.GetLightOcclusionProbes(grassPosList, retLightProbeOcclusion))
                    {
                        chunckData.InstanceList[chunckData.InstanceList.Count-1].lightProbOcc = (float)Math.Round(retLightProbeOcclusion[0],3);                        
                    }
                }

                float radius = cullingGrassBound.size.x > cullingGrassBound.size.y ? cullingGrassBound.size.x : cullingGrassBound.size.y;
                radius = radius > cullingGrassBound.size.z ? radius : cullingGrassBound.size.z;
                chunckData.CullingCollider[0] = cullingGrassBound.center.x;
                chunckData.CullingCollider[1] = cullingGrassBound.center.y;
                chunckData.CullingCollider[2] = cullingGrassBound.center.z;
                chunckData.CullingCollider[3] = radius * 0.5f;

                chunckData.maxScale = maxLodScale;

                return true;
            }

            return false;
        }

        static SimpleGrassChunk GetSimpleGrassChunkPrefab(GameObject objChunck)
        {
            SimpleGrassChunk chuckScript = objChunck.GetComponent<SimpleGrassChunk>();
            SimpleGrassChunk chuckScriptPrefab = chuckScript;
            //相关的预制体对象，部分参数从预制体中获取。
            if (chuckScript && chuckScript.SelfPrefab != null)
            {
                SimpleGrassChunk cmpFromPrefab = chuckScript.SelfPrefab.GetComponent<SimpleGrassChunk>();
                if (cmpFromPrefab != null)
                {
                    chuckScriptPrefab = cmpFromPrefab;
                    return chuckScriptPrefab;
                }
            }

            return chuckScriptPrefab;            
        }
        static SimpleGrassProtoInfo RepairProtoInfo(GameObject protoNode)
        {
            SimpleGrassProtoInfo cmpProtoInfo = protoNode.GetComponent<SimpleGrassProtoInfo>();
            if(cmpProtoInfo)
            {
                return cmpProtoInfo;
            }

            int chunckCount = protoNode.transform.childCount;
            for(int index = 0; index != chunckCount; ++index)
            {
                GameObject objChunck = protoNode.transform.GetChild(index).gameObject;
                SimpleGrassChunk chuckScript = objChunck.GetComponent<SimpleGrassChunk>();
                //相关的预制体对象，部分参数从预制体中获取。
                if (chuckScript && chuckScript.SelfPrefab != null)
                {
                    SimpleGrassChunk cmpFromPrefab = chuckScript.SelfPrefab.GetComponent<SimpleGrassChunk>();
                    if (cmpFromPrefab != null)
                    {
                        chuckScript = cmpFromPrefab;
                        cmpProtoInfo = SimpleGrassSysEditor.CreateAndResetProtoInfo(protoNode, chuckScript);
                        if(cmpProtoInfo)
                        {
                            return cmpProtoInfo;
                        }
                    }                    
                }
            }
            return null;                        
        }

       
        static bool TryGetLodGroupData(ref MeshRenderer[] meshRenderAry, ref InstData grassData)
        {
            if (meshRenderAry.Length > 0)
            {
                if (meshRenderAry[0].transform.parent != null)
                {
                    Vector3 scale = meshRenderAry[0].transform.parent.lossyScale;

                    LODGroup lodGroup = meshRenderAry[0].transform.parent.GetComponent<LODGroup>();
                    if (lodGroup != null)
                    {
                        grassData.pos = lodGroup.transform.TransformPoint(lodGroup.localReferencePoint);
                        scale = lodGroup.transform.lossyScale;
                    }
                    else
                    {
                        grassData.pos = meshRenderAry[0].transform.parent.position;
                    }

                    float val = Mathf.Abs(scale.x);
                    val = Mathf.Max(val, Mathf.Abs(scale.y));
                    val = Mathf.Max(val, Mathf.Abs(scale.z));
                    grassData.maxScale = val;
                    return true;
                }
            }
            return false;
        }

        static int TryGetLightMapData(long code, MeshRenderer meshRender,
             ref Dictionary<long, long> protoTypeLightMapIndex,
             ref Dictionary<long, long> protoTypeLightMapIndex2,
             ref Dictionary<long, int> protoTypeLightMapNum,
             out Vector4 lightmapScaleOffset)
        {
            int lightmapIndex = -1;
            lightmapScaleOffset = Vector4.zero;
            long protoTypeLMIndex = 0;
            long protoTypeLMIndex2 = 0;
            int protoTypeLMNum = 0;
            if (protoTypeLightMapIndex.ContainsKey(code))
            {
                protoTypeLMIndex = protoTypeLightMapIndex[code];
            }
            if (protoTypeLightMapIndex2.ContainsKey(code))
            {
                protoTypeLMIndex2 = protoTypeLightMapIndex2[code];
            }

            if (protoTypeLightMapNum.ContainsKey(code))
            {
                protoTypeLMNum = protoTypeLightMapNum[code];
            }

            Common.GetLightmapInfo2(meshRender, protoTypeLMIndex, protoTypeLMIndex2, protoTypeLMNum,
              out lightmapIndex, out lightmapScaleOffset, out protoTypeLMIndex, out protoTypeLMIndex2, out protoTypeLMNum);


            protoTypeLightMapIndex[code] = protoTypeLMIndex;
            protoTypeLightMapIndex2[code] = protoTypeLMIndex2;
            protoTypeLightMapNum[code] = protoTypeLMNum;

            return lightmapIndex;
        }



        static void TryGetMeshData(int vernum, GameObject objGrass,
            ref List<MeshRenderer> meshRenders, ref List<MeshFilter> meshFilters, ref List<int> meshLodIdxs)
        {
            meshRenders.Clear();
            meshFilters.Clear();
            meshLodIdxs.Clear();
            //meshrender记载关联的LODIndex
            if (vernum >= Common.Ver_1_SameMeshInLODS)
            {                
                LODGroup lod = objGrass.GetComponent<LODGroup>();
                if (lod != null)
                {
                    LOD[] lods = lod.GetLODs();
                    for (int i = 0; i != lods.Length; ++i)
                    {
                        for (int idx = 0; idx < lods[i].renderers.Length; ++idx)
                        {
                            Renderer render = lods[i].renderers[idx];
                            if (render is MeshRenderer)
                            {
                                meshRenders.Add((MeshRenderer)render);
                                MeshFilter meshFilter = render.gameObject.GetComponentInChildren<MeshFilter>();
                                meshFilters.Add(meshFilter);
                                meshLodIdxs.Add(i);
                            }
                        }
                    }
                    return;
                }
            }

            //////////ver <= 0
            MeshRenderer[] meshRenderAry = objGrass.GetComponentsInChildren<MeshRenderer>();
            MeshFilter[] meshFilterAry = objGrass.GetComponentsInChildren<MeshFilter>();
            meshRenders.AddRange(meshRenderAry);
            meshFilters.AddRange(meshFilterAry);
         }

        static void AppendHashCode_MD5(long hashCode, string md5)
        {
            if (TmpHashCode_MD5Dict.ContainsKey(hashCode))
            {
                TmpHashCode_MD5Dict[hashCode] = md5;
            }else
            {
                TmpHashCode_MD5Dict.Add(hashCode, md5);
            }
        }
    }
}

