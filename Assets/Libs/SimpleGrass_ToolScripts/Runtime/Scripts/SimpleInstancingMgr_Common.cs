//#define ENABLE_PROFILER

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.IO;
using System.Collections;
using System.Threading;
using UnityEngine.Profiling;//test


namespace SimpleGrass
{

    public partial class SimpleInstancingMgr : MonoBehaviour
    {
        public delegate long Delegate_GetCodeByMD5(string codeMD5, long defVal, ref bool existedMD5);
        public delegate int Delegate_GetLightMapNum(int protoIndex, long meshCode);
        public delegate bool Delegate_GetLightMapIndexes(int protoIndex, long code, out List<int> outLightmapIndex);
#region BuildPrototype
        
        //参考：LoadProtoTypeData()
        static public PrototypeInfo BuildPrototypeInfo(int verNum, string sceneTag, int prototypeIdx, ProtoTypeData protoTypeData,
            float[] MeshDistanceLod, float[] DensityLodsDistance, float[] DensityLodsCutdown,
            Delegate_GetCodeByMD5 GetCodeByMD5Fun, Delegate_GetLightMapNum GetLightMapNumFun)
        {
            if(protoTypeData == null)
            {
                return null;
            }

            PrototypeInfo protoInfo = new PrototypeInfo();
            int prototypeTag = protoTypeData.Tag;
            //load: cull max dis
            protoInfo.prototypeName = protoTypeData.ProtoKey;
            protoInfo.protoTypeCullMaxDists = protoTypeData.CullingMaxDist;
            ///
            protoInfo.density = protoTypeData.Density;
            protoInfo.UseLightProb = protoTypeData.UseLightProb;
            GameObject protosPrefab = protoTypeData.GrassPrebab;

            //情景1：尝试加载自定义的原型数据（扫描扩展的数据）
            if (protosPrefab == null && protoTypeData.Custom != null)
            {
                //LoadCustomProtoTypeData
               BuildCustomProtoTypeData(verNum, sceneTag, prototypeIdx, ref protoTypeData, ref protoInfo,GetCodeByMD5Fun,GetLightMapNumFun);
                return null;
            }

            //情景2：正常种植、扫描扩展的带预制体的数据。
            Debug.Assert(protosPrefab != null);
            string prefabName = protosPrefab.name;
            bool isCustomTrue = protoInfo.prototypeName.StartsWith(Common.CustomPrefix_Prefab);//扫描的预制体原型时
            if (isCustomTrue)
            {
                prefabName = protoInfo.prototypeName.Substring(Common.CustomPrefix_Prefab.Length);
            }
            protoInfo.isCustom = isCustomTrue;

             ////从原型的预制体中，获取基础信息

            //使用LOD的原型，获取LODGROUP数据。
            LODGroup lod = protosPrefab.GetComponent<LODGroup>();
            if (lod != null)
            {
                _BuildPrototypeInfo_LODGROUP(protosPrefab, prefabName, ref protoInfo, verNum, sceneTag, prototypeIdx, prototypeTag,
            MeshDistanceLod, DensityLodsDistance, DensityLodsCutdown, GetCodeByMD5Fun, GetLightMapNumFun);

            }
            ///////////////没使用LOD的原型
            else
            {
                _BuildPrototypeInfo_Normal(protosPrefab, prefabName, ref protoInfo, verNum, sceneTag, prototypeIdx, prototypeTag,
            MeshDistanceLod, DensityLodsDistance, DensityLodsCutdown, GetCodeByMD5Fun, GetLightMapNumFun);
            }

            return protoInfo;
        }

        static void _BuildPrototypeInfo_Normal(GameObject protosPrefab, string prefabName,
                    ref PrototypeInfo protoInfo,
                    int verNum, string sceneTag, int prototypeIdx, int prototypeTag,
                    float[] MeshDistanceLod, float[] DensityLodsDistance, float[] DensityLodsCutdown,
                    Delegate_GetCodeByMD5 GetCodeByMD5Fun, Delegate_GetLightMapNum GetLightMapNumFun)
        {
            List<Material[]> listMeshMaterial = new List<Material[]>();
            List<Mesh> listMesh = new List<Mesh>();
            List<float> listLodHeight = new List<float>();
            Dictionary<long, int> dictRender = new Dictionary<long, int>();
            listLodHeight.Clear();

            ///////////////没使用LOD的原型    
            {
                protoInfo.protoTypeLodGroupSize = 0.0f;
                protoInfo.protoTypeLods = null;

                LodInfo[] lodInfo = new LodInfo[1];
                LodInfo info = new LodInfo();
                info.lodLevel = 0;

                listMeshMaterial.Clear();
                listMesh.Clear();
                dictRender.Clear();
                MeshRenderer[] meshRenderers = protosPrefab.GetComponentsInChildren<MeshRenderer>();
                MeshFilter[] meshFilters = protosPrefab.GetComponentsInChildren<MeshFilter>();
                if (meshRenderers.Length == meshFilters.Length)
                {
                    for (int i = 0; i != meshRenderers.Length; ++i)
                    {
                        if (meshRenderers[i].sharedMaterials != null && meshFilters[i].sharedMesh != null)
                        {
                            listMeshMaterial.Add(meshRenderers[i].sharedMaterials);
                            listMesh.Add(meshFilters[i].sharedMesh);
                        }
                        else
                        {
                            string logStr = "GRASS预制体有错： " + protoInfo.prototypeName + "  name: " + protosPrefab.name;
                            Common.Debug(logStr,true);
                            if (Common.EnableDebugOutLog())
                            {
                                string str = string.Format("## [ERROR]#_BuildPrototypeInfo_Normal: Fail Prefab, {0}", logStr);
                                Common.OutputDebugLog(str,true);
                            }
                        }
                    }
                }
                info.sharedMeshs = listMesh.ToArray();
                info.sharedMaterials = listMeshMaterial.ToArray();


                int useNum = 0;
                int renderNum = 0;
                for (int idx = 0; idx < info.sharedMeshs.Length; ++idx)
                {
                    string meshName = info.sharedMeshs[idx].name;
                    string codeMD5 = "";
                    long code = Common.GetMeshHashCode_NoLightmapIdx(verNum, ref sceneTag, ref protoInfo.prototypeName, ref prefabName, ref meshName, -1, ref codeMD5);//meshRenderName
                    bool existedMD5 = false;
                    code = GetCodeByMD5Fun(codeMD5, code, ref existedMD5);
                    if (!dictRender.ContainsKey(code))
                    {
                        dictRender.Add(code, 0);
                        ++renderNum;
                        useNum += GetLightMapNumFun(prototypeIdx, code);
                    }
                    if (!existedMD5)
                    {
                        if (Common.EnableDebugOutLog())
                        {
                            string str = string.Format("## [ERROR]#_BuildPrototypeInfo_Normal: Fail CodeByMD5, scene:{0} prototype:{1},grassprefab:{2}, mesh:{3}", sceneTag, protoInfo.prototypeName, prefabName, meshName);
                            Common.OutputDebugLog(str,true);
                        }
                    }
                }
                bool useLightMap = useNum > 0;
                int mulNum = useNum <= 0 ? 1 : useNum + 1;//增加一个没Lightmap的使用数据
                //增加lightProb的使用数据
                if (protoInfo.UseLightProb)
                {
                    mulNum = mulNum + 1;
                }

                int renderLen = (renderNum) * mulNum;
                info.vertexCacheList = new VertexCache[renderLen];
                info.materialBlockList = new MaterialBlock[renderLen];
                info.meshNameCodes = new long[renderLen];
                info.inLodGroup = false;
                info.useLightMap = useLightMap;
                lodInfo[0] = info;

                protoInfo.lodInfoList = lodInfo;
                protoInfo.isSingleMode = false;
                protoInfo.mode = Common.ProtoMode.BatchMode;
                protoInfo.useDensityLod = (!protoInfo.isCustom);//目前：正常种植的植被，使用密度LOD
                Common.InitDensityLods(ref protoInfo.protoTypeDensityLods, ref protoInfo.protoTypeDensityCutdown, DensityLodsDistance, DensityLodsCutdown);
            }

            //return protoInfo;
        }

        static void _BuildPrototypeInfo_LODGROUP(GameObject protosPrefab, string prefabName,
                    ref PrototypeInfo protoInfo, int verNum, string sceneTag, int prototypeIdx, int prototypeTag,
                    float[] MeshDistanceLod, float[] DensityLodsDistance, float[] DensityLodsCutdown,
                    Delegate_GetCodeByMD5 GetCodeByMD5Fun, Delegate_GetLightMapNum GetLightMapNumFun)
        {

            List<Material[]> listMeshMaterial = new List<Material[]>();
            List<Mesh> listMesh = new List<Mesh>();
            List<float> listLodHeight = new List<float>();
            Dictionary<long, int> dictRender = new Dictionary<long, int>();
            listLodHeight.Clear();

            ////从原型的预制体中，获取基础信息

            //使用LOD的原型，获取LODGROUP数据。
            LODGroup lod = protosPrefab.GetComponent<LODGroup>();
            if (lod != null)
            {
                //load: mesh lodgroup size
                protoInfo.protoTypeLodGroupSize = lod.size;

                LodInfo[] lodInfo = new LodInfo[lod.lodCount];
                LOD[] lods = lod.GetLODs();
                for (int i = 0; i != lods.Length; ++i)
                {
                    if (lods[i].renderers == null)
                    {
                        continue;
                    }
                    listLodHeight.Add(lods[i].screenRelativeTransitionHeight);
                    LodInfo info = new LodInfo();
                    info.lodLevel = i;

                    listMeshMaterial.Clear();
                    listMesh.Clear();
                    dictRender.Clear();
                    int useNum = 0;
                    int renderNum = 0;
                    foreach (var render in lods[i].renderers)
                    {
                        if (render is MeshRenderer)
                        {
                            if (render.sharedMaterials != null)
                            {
                                MeshFilter meshFilter = render.gameObject.GetComponentInChildren<MeshFilter>();
                                if (meshFilter != null && meshFilter.sharedMesh != null)
                                {
                                    listMeshMaterial.Add(render.sharedMaterials);
                                    listMesh.Add(meshFilter.sharedMesh);
                                }
                                else
                                {
                                    string logStr = "GRASS预制体有错： " + protoInfo.prototypeName + "  name: " + protosPrefab.name;
                                    Common.Debug(logStr,true);
                                    if (Common.EnableDebugOutLog())
                                    {
                                        string str = string.Format("## [ERROR]#_BuildPrototypeInfo_LODGROUP: Fail Prefab, {0}", logStr);
                                        Common.OutputDebugLog(str,true);
                                    }
                                }
                            }

                        }
                    }
                    for (int idx = 0; idx < listMesh.Count; ++idx)
                    {
                        string meshName = listMesh[idx].name;//string meshRenderName = listMeshRenderer[idx].name;           
                        string codeMD5 = "";
                        long code = Common.GetMeshHashCode_NoLightmapIdx(verNum, ref sceneTag, ref protoInfo.prototypeName, ref prefabName, ref meshName, info.lodLevel, ref codeMD5);//meshRenderName
                        bool existedMD5 = false;
                        code = GetCodeByMD5Fun(codeMD5, code, ref existedMD5);
                        if (!dictRender.ContainsKey(code))
                        {
                            dictRender.Add(code, 0);
                            ++renderNum;
                            useNum += GetLightMapNumFun(prototypeIdx, code);
                        }
                        if (!existedMD5)
                        {
                            if (Common.EnableDebugOutLog())
                            {
                                string str = string.Format("## [ERROR]#_BuildPrototypeInfo_LODGROUP: Fail CodeByMD5, scene:{0} prototype:{1},grassprefab:{2}, mesh:{3}", sceneTag, protoInfo.prototypeName, prefabName, meshName);
                                Common.OutputDebugLog(str,true);
                            }
                        }

                    }
                    bool useLightMap = useNum > 0;
                    int mulNum = useNum <= 0 ? 1 : useNum + 1;//增加一个没Lightmap的使用数据                                                              
                    if (protoInfo.UseLightProb)//增加lightProb的使用数据
                    {
                        mulNum = mulNum + 1;
                    }
                    int renderLen = renderNum * mulNum;//int renderLen = lods[i].renderers.Length * mulNum;
                    info.vertexCacheList = new VertexCache[renderLen];
                    info.materialBlockList = new MaterialBlock[renderLen];
                    info.meshNameCodes = new long[renderLen];

                    info.sharedMeshs = listMesh.ToArray();
                    info.sharedMaterials = listMeshMaterial.ToArray();
                    info.inLodGroup = true;
                    info.useLightMap = useLightMap;
                    lodInfo[i] = info;
                }
                protoInfo.lodInfoList = lodInfo;

                protoInfo.protoTypeLods = listLodHeight.ToArray();

                protoInfo.isSingleMode = true;
                protoInfo.useDensityLod = false;
                protoInfo.mode = Common.ProtoMode.SingleMode;
                //批量使用LOD的模型
                if (!protoInfo.isCustom && prototypeTag == (int)Common.ProtoMode.BatchMode_LOD)
                {
                    protoInfo.isSingleMode = false;
                    protoInfo.useDensityLod = true;
                    protoInfo.mode = Common.ProtoMode.BatchMode_LOD;

                    Common.InitDensityLods(ref protoInfo.protoTypeDensityLods, ref protoInfo.protoTypeDensityCutdown, DensityLodsDistance, DensityLodsCutdown);
                    Common.InitDistanceMeshLods(ref protoInfo.MeshLods, MeshDistanceLod);
                }
                // Common.InitDensityLods(ref protoInfo.protoTypeDensityLods, ref protoInfo.protoTypeDensityCutdown);

            }
            // return protoInfo;
        }

        private static void BuildCustomProtoTypeData(int verNum, string sceneTag, int prototypeIdx, 
            ref ProtoTypeData protoTypeData, ref PrototypeInfo protoInfo,
            Delegate_GetCodeByMD5 GetCodeByMD5Fun, Delegate_GetLightMapNum GetLightMapNumFun)
        {
            protoInfo.isCustom = true;
            string protoTypeName = protoTypeData.ProtoKey;
            int prototypeMode = Common.GetCustomPrototypeModeByScan(protoTypeName);

            //获取LODGROUP模式            
            if (prototypeMode == 1)
            {
                _BuildCustomProtoTypeData_LODGROUP(verNum, sceneTag, prototypeIdx,
                         ref protoTypeData, ref protoInfo, GetCodeByMD5Fun, GetLightMapNumFun);
            }
            else if (prototypeMode == 2)
            {
                _BuildCustomProtoTypeData_Normal(verNum, sceneTag, prototypeIdx,
                         ref protoTypeData, ref protoInfo, GetCodeByMD5Fun, GetLightMapNumFun);
            }
        }

        private static void _BuildCustomProtoTypeData_Normal(int verNum, string sceneTag, int prototypeIdx,
           ref ProtoTypeData protoTypeData, ref PrototypeInfo protoInfo,
           Delegate_GetCodeByMD5 GetCodeByMD5Fun, Delegate_GetLightMapNum GetLightMapNumFun)
        {
            string protoTypeName = protoTypeData.ProtoKey;
            string prefabName = Common.GetCustomPrefabNameByScan(protoTypeName);
            List<float> listLodHeight = new List<float>();
            Dictionary<long, int> dictRender = new Dictionary<long, int>();

            listLodHeight.Clear();
       
            {
                ProtoTypeCustom customPtr = protoTypeData.Custom;
                if (customPtr.LODs.Count == 1)
                {
                    CustomLod customLodPtr = customPtr.LODs[0];
                    protoInfo.protoTypeLodGroupSize = 0.0f;
                    protoInfo.protoTypeLods = null;

                    LodInfo[] lodInfo = new LodInfo[1];
                    LodInfo info = new LodInfo();
                    info.lodLevel = 0;

                    info.sharedMeshs = customLodPtr.Meshs.ToArray();
                    info.sharedMaterials = new Material[customLodPtr.Mats.Count][];
                    for (int iMat = 0; iMat != customLodPtr.Mats.Count; ++iMat)
                    {
                        info.sharedMaterials[iMat] = customLodPtr.Mats[iMat].Mats;
                    }

                    int useNum = 0;
                    int renderNum = 0;
                    dictRender.Clear();
                    for (int idx = 0; idx < info.sharedMeshs.Length; ++idx)
                    {
                        if (info.sharedMeshs[idx] != null)
                        {
                            string meshName = info.sharedMeshs[idx].name;
                            string codeMD5 = "";
                            long code = Common.GetMeshHashCode_NoLightmapIdx(verNum, ref sceneTag, ref protoInfo.prototypeName, ref prefabName, ref meshName, -1, ref codeMD5);//meshRenderName
                            bool existedMD5 = false;
                            code = GetCodeByMD5Fun(codeMD5, code, ref existedMD5);
                            if (!dictRender.ContainsKey(code))
                            {
                                dictRender.Add(code, 0);
                                ++renderNum;
                                useNum += GetLightMapNumFun(prototypeIdx, code);
                            }
                            if (!existedMD5)
                            {
                                if (Common.EnableDebugOutLog())
                                {
                                    string str = string.Format("## [ERROR]#_BuildCustomProtoTypeData_Normal: Fail CodeByMD5, scene:{0} prototype:{1},grassprefab:{2}, mesh:{3}", sceneTag, protoInfo.prototypeName, prefabName, meshName);
                                    Common.OutputDebugLog(str,true);
                                }
                            }
                        }
                        else
                        {   string logStr = "GRASS预制体有错： " + protoInfo.prototypeName;
                            Common.Debug(logStr,true);
                            if (Common.EnableDebugOutLog())
                            {
                                string str = string.Format("## [ERROR]#_BuildCustomProtoTypeData_Normal: Fail Prefab, {0}", logStr);
                                Common.OutputDebugLog(str,true);
                            }
                        }
                    }
                    bool useLightMap = useNum > 0;
                    int mulNum = useNum <= 0 ? 1 : useNum + 1;//增加一个没Lightmap的使用数据

                    int renderLen = (renderNum) * mulNum;
                    info.vertexCacheList = new VertexCache[renderLen];
                    info.materialBlockList = new MaterialBlock[renderLen];
                    info.meshNameCodes = new long[renderLen];
                    info.inLodGroup = false;
                    info.useLightMap = useLightMap;
                    lodInfo[0] = info;

                    protoInfo.lodInfoList = lodInfo;

                    protoInfo.isSingleMode = false;
                    protoInfo.mode = Common.ProtoMode.BatchMode;

                    protoInfo.useDensityLod = false;
                    // Common.InitDensityLods(ref protoInfo.protoTypeDensityLods, ref protoInfo.protoTypeDensityCutdown);
                }
            }
        }


        private static void _BuildCustomProtoTypeData_LODGROUP(int verNum, string sceneTag, int prototypeIdx,
           ref ProtoTypeData protoTypeData, ref PrototypeInfo protoInfo,
           Delegate_GetCodeByMD5 GetCodeByMD5Fun, Delegate_GetLightMapNum GetLightMapNumFun)
        {
            string protoTypeName = protoTypeData.ProtoKey;
            string prefabName = Common.GetCustomPrefabNameByScan(protoTypeName);
            List<Material[]> listMeshMaterial = new List<Material[]>();
            List<Mesh> listMesh = new List<Mesh>();
            List<float> listLodHeight = new List<float>();
            Dictionary<long, int> dictRender = new Dictionary<long, int>();

            listLodHeight.Clear();
            //获取LODGROUP模式            
            // if (prototypeMode == 1)
            {
                //load: mesh lodgroup size
                ProtoTypeCustom customPtr = protoTypeData.Custom;
                protoInfo.protoTypeLodGroupSize = customPtr.LODSize;
                int lodNum = customPtr.LodNum;
                if (lodNum > 0)
                {
                    LodInfo[] lodInfo = new LodInfo[lodNum];
                    for (int i = 0; i != lodNum; ++i)
                    {
                        CustomLod customLodPtr = customPtr.LODs[i];
                        listLodHeight.Add(customLodPtr.RelaHeight);
                        LodInfo info = new LodInfo();
                        info.lodLevel = i;

                        listMeshMaterial.Clear();
                        listMesh.Clear();
                        dictRender.Clear();
                        int useNum = 0;
                        int renderNum = 0;

                        for (int idx = 0; idx < customLodPtr.Meshs.Count; ++idx)
                        {
                            if (customLodPtr.Meshs[idx] != null)
                            {
                                string meshName = customLodPtr.Meshs[idx].name;
                                string codeMD5 = "";
                                long code = Common.GetMeshHashCode_NoLightmapIdx(verNum, ref sceneTag, ref protoInfo.prototypeName, ref prefabName, ref meshName, info.lodLevel, ref codeMD5);
                                bool existedMD5 = false;
                                code = GetCodeByMD5Fun(codeMD5, code, ref existedMD5);
                                if (!dictRender.ContainsKey(code))
                                {
                                    dictRender.Add(code, 0);
                                    ++renderNum;
                                    useNum += GetLightMapNumFun(prototypeIdx, code);
                                }

                                if (!existedMD5)
                                {
                                    if (Common.EnableDebugOutLog())
                                    {
                                        string str = string.Format("## [ERROR]#_BuildCustomProtoTypeData_LODGROUP: Fail CodeByMD5, scene:{0} prototype:{1},grassprefab:{2}, mesh:{3}", sceneTag, protoInfo.prototypeName, prefabName, meshName);
                                        Common.OutputDebugLog(str,true);
                                    }
                                }
                            }
                            else
                            {
                                string logStr = "GRASS原型预制体有错： " + protoInfo.prototypeName;
                                Common.Debug(logStr,true);
                                if (Common.EnableDebugOutLog())
                                {
                                    string str = string.Format("## [ERROR]#_BuildCustomProtoTypeData_LODGROUP: {0}", logStr);
                                    Common.OutputDebugLog(str,true);
                                }
                            }

                        }
                        bool useLightMap = useNum > 0;
                        int mulNum = useNum <= 0 ? 1 : useNum + 1;//增加一个没Lightmap的使用数据

                        int renderLen = renderNum * mulNum;
                        info.vertexCacheList = new VertexCache[renderLen];
                        info.materialBlockList = new MaterialBlock[renderLen];
                        info.meshNameCodes = new long[renderLen];

                        info.sharedMeshs = customLodPtr.Meshs.ToArray();

                        info.sharedMaterials = new Material[customLodPtr.Mats.Count][];
                        for (int iMat = 0; iMat != customLodPtr.Mats.Count; ++iMat)
                        {
                            info.sharedMaterials[iMat] = customLodPtr.Mats[iMat].Mats;
                        }
                        info.inLodGroup = true;
                        info.useLightMap = useLightMap;
                        lodInfo[i] = info;
                    }
                    protoInfo.lodInfoList = lodInfo;

                    protoInfo.protoTypeLods = listLodHeight.ToArray();
                    protoInfo.isSingleMode = true;
                    protoInfo.mode = Common.ProtoMode.SingleMode;

                    protoInfo.useDensityLod = false;
                    // Common.InitDensityLods(ref protoInfo.protoTypeDensityLods, ref protoInfo.protoTypeDensityCutdown);
                }
            }

        }

        #endregion

#region  BuildGroupDatas
        /// <summary>
        /// 整理每个数据块中实例的数据(块信息、实例矩阵、实例光照信息）
        /// </summary>
        static public bool BuildGroupDatas(SimpleInstancingMgr instancingMgr, 
            List<PrototypeInfo> prototypeList, SimpleSaveData savedProfile,
            ref SimpleInstancingGroup[]  instancingGroupAry, ref List<GroupSortData> instancingGroupSort,
            ref int TestGroupNum, ref int TestInstanceNum, ref int TestMeshNum, int beginProtypeIndex)
        {
            try
            {
                ///初始化，裁剪
                int prototypeIndex = -1;
                int tmpIndex = -1;

                List<InstanceChunkData> instanceChunkDataList = savedProfile.ChunkData;

                //
                TestGroupNum = instanceChunkDataList.Count;
                TestInstanceNum = 0;
                TestMeshNum = 0;
                bool isSingleMode = false;
                bool useLightProbe = false;
                Common.ProtoMode protoMode = Common.ProtoMode.BatchMode;                
                for (int i = 0; i < instanceChunkDataList.Count; ++i)
                {
                    prototypeIndex = beginProtypeIndex + instanceChunkDataList[i].PrototypeIndex;
                    if (tmpIndex != prototypeIndex)
                    {
                        tmpIndex = prototypeIndex;

                        PrototypeInfo protoInfo = prototypeList[prototypeIndex];
                        isSingleMode = protoInfo.isSingleMode;
                        protoMode = protoInfo.mode;
                        useLightProbe = protoInfo.UseLightProb;
                    }

                    SimpleInstancingGroup group = new SimpleInstancingGroup(instancingMgr);
                    group.PrototypeIndex = prototypeIndex;
                    Vector3 center = new Vector3(instanceChunkDataList[i].CullingCollider[0], instanceChunkDataList[i].CullingCollider[1], instanceChunkDataList[i].CullingCollider[2]);

                    group.worldPosition = center;
                    group.MaxScale = instanceChunkDataList[i].maxScale;

                    //创建每块的裁剪使用的球
                    UnityEngine.BoundingSphere bounding = new UnityEngine.BoundingSphere();
                    bounding.position = center;
                    bounding.radius = instanceChunkDataList[i].CullingCollider[3];
                    group.BoundingSpere = bounding;

                    instancingGroupAry[i] = group;
                    if (Common.UseSort)
                    {
                        GroupSortData sortData = new GroupSortData();
                        sortData.index = i;
                        sortData.dist = 0;
                        sortData.isVisible = true;
                        instancingGroupSort.Add(sortData);
                    }

                    //增加group裁剪球体
                    //test AddCullingBoundingSphere(group);


                    //新增BatchModeLOD
                    if (protoMode == Common.ProtoMode.BatchMode_LOD)
                    {
                        group.IsSingleMode = false;
                        group.protoMode = Common.ProtoMode.BatchMode_LOD;
                    }

                    //LODGroup模式, 一个实例对应一个SimpleInstance，解析矩阵及光照信息
                    if (isSingleMode)
                    {
                        group.IsSingleMode = true;
                        group.protoMode = protoMode;
                        #region LODGroup模式
                        List<InstData> instList = instanceChunkDataList[i].InstanceList;
                        //BuildOneGroupData_SingleMode(ref group, ref instList,ref TestInstanceNum,ref TestMeshNum, useLightProbe);
                        #endregion LODGroup模式
                    }
                    //////////////////批量模式
                    else
                    {
                        group.protoMode = protoMode;
                        #region 批量模式
                        List<InstData> instList = instanceChunkDataList[i].InstanceList;
                        BuildOnGroupData_BatchMode(ref group, ref instList, ref TestInstanceNum, ref TestMeshNum, useLightProbe);
                        #endregion 批量模式
                    }

                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            return false;
        }

        //static public bool BuildOneGroupData_SingleMode(ref SimpleInstancingGroup group, ref List<InstData> instList,
        //    ref int TestInstanceNum, ref int TestMeshNum, bool useLightProbe)
        //{
        //    if (instList != null)
        //    {
        //        List<Matrix4x4> tmpMat = new List<Matrix4x4>();
        //        List<Vector4> tmpLSOList = new List<Vector4>();
        //        List<float> tmpLightProbeOccList = new List<float>();

        //        //Common.TestInstanceNum += instList.Count;
        //        TestInstanceNum += instList.Count;
        //        for (int instIdx = 0; instIdx < instList.Count; ++instIdx)
        //        {
        //            SimpleInstance inst = new SimpleInstance();
        //            inst.pos = instList[instIdx].pos;
        //            float lightProbeOcc = instList[instIdx].lightProbOcc; 
        //            //LODGROUP相关数据
        //            inst.maxScale = instList[instIdx].maxScale;
        //            inst.meshBounds.center = instList[instIdx].Collider[0];
        //            inst.meshBounds.size = instList[instIdx].Collider[1];
                   
        //            List<long> codeList = instList[instIdx].meshCodes;
        //            for (int codeIdx = 0; codeIdx < codeList.Count; ++codeIdx)
        //            {
        //                long code = codeList[codeIdx];
        //                int vCount = 0;
        //                DetailData data = instList[instIdx].GetDetailData(codeIdx);
        //                List<Vector3> trsList = data.TRS;
        //                if (trsList != null)
        //                    vCount = trsList.Count;

        //                //实例的世界矩阵信息
        //                tmpMat.Clear();
        //                tmpLSOList.Clear();
        //                tmpLightProbeOccList.Clear();
        //                vCount = vCount / 3;
        //                //Common.TestMeshNum += vCount;
        //                TestMeshNum += vCount;
        //                for (int trsIdx = 0; trsIdx < vCount; ++trsIdx)
        //                {
        //                    //考虑直接保存在profile中
        //                    Matrix4x4 mat = new Matrix4x4();
        //                    Quaternion q = Quaternion.Euler(trsList[trsIdx * 3 + 1]);
        //                    mat.SetTRS(trsList[trsIdx * 3], q, trsList[trsIdx * 3 + 2]);
        //                    tmpMat.Add(mat);
        //                }
        //                //
        //                // if (useLightmap)
        //                if (!useLightProbe)
        //                {
        //                    //light lso                                                                           
        //                    int count = 0;
        //                    if (data.LSO != null)
        //                    {
        //                        count = data.LSO.Count;
        //                    }
        //                    for (int idx = 0; idx < count; ++idx)
        //                    {
        //                        tmpLSOList.Add(data.LSO[idx]);
        //                    }
        //                }
        //                else
        //                {
        //                    int count = vCount;
        //                    for (int idx = 0; idx < count; ++idx)
        //                    {
        //                        tmpLightProbeOccList.Add(lightProbeOcc);
        //                    }
        //                }
        //                inst.matxDict.Add(code, tmpMat.ToArray());
        //                inst.lightmapScaleOffsetDict.Add(code, tmpLSOList.ToArray());

        //                inst.lightProbOcclusionDict.Add(code, tmpLightProbeOccList.ToArray());

        //            }

        //            group.instList.Add(inst);
        //        }
        //        return true;
        //    }
        //    return false;
        //}

        static public bool BuildOnGroupData_BatchMode(ref SimpleInstancingGroup group, ref List<InstData> instList, 
            ref int TestInstanceNum, ref int TestMeshNum,bool useLightProbe)
        {
            if (instList != null)
            {
                List<Matrix4x4> tmpMat = new List<Matrix4x4>();
                List<Vector4> tmpLSOList = new List<Vector4>();
                List<Vector4> tmpColorList = new List<Vector4>();
                Dictionary<long, List<Matrix4x4>> objTmpTRS = new Dictionary<long, List<Matrix4x4>>();
                Dictionary<long, List<Vector4>> objTmpColor = new Dictionary<long, List<Vector4>>();
                Dictionary<long, List<Vector4>> objTmpLSO = new Dictionary<long, List<Vector4>>();
                Dictionary<long, List<float>> objTmpLightProbeOcc = new Dictionary<long, List<float>>();

                //Common.TestInstanceNum += instList.Count;
                TestInstanceNum += instList.Count;
                group.VisibleInstNum = instList.Count;
                group.OrgInstNum = instList.Count;
                for (int instIdx = 0; instIdx < instList.Count; ++instIdx)
                {
                    List<long> codeList = instList[instIdx].meshCodes;
                    float lightProbeOcc = instList[instIdx].lightProbOcc;
                    for (int codeIdx = 0; codeIdx < codeList.Count; ++codeIdx)
                    {
                        long code = codeList[codeIdx];
                        int vCount = 0;
                        DetailData data = instList[instIdx].GetDetailData(codeIdx);
                        List<Vector3> trsList = data.TRS;
                        if (trsList != null)
                            vCount = trsList.Count;

                        //实例的世界矩阵信息
                        List<Matrix4x4> outMat = null;
                        if (!objTmpTRS.TryGetValue(code, out outMat))
                        {
                            outMat = new List<Matrix4x4>();
                            objTmpTRS.Add(code, outMat);
                        }

                        vCount = vCount / 3;
                        //Common.TestMeshNum += vCount;
                        TestMeshNum += vCount;
                        for (int trsIdx = 0; trsIdx < vCount; ++trsIdx)
                        {
                            //考虑直接保存在profile中
                            Matrix4x4 mat = new Matrix4x4();
                            Quaternion q = Quaternion.Euler(trsList[trsIdx * 3 + 1]);
                            mat.SetTRS(trsList[trsIdx * 3], q, trsList[trsIdx * 3 + 2]);
                            outMat.Add(mat);
                        }

                        List<Vector4> outColorList = null;
                        if (!objTmpColor.TryGetValue(code, out outColorList))
                        {
                            outColorList = new List<Vector4>();
                            objTmpColor.Add(code, outColorList);
                        }
                        outColorList.AddRange(data.Color);

                        //
                        //if (useLightmap)
                        if (!useLightProbe)
                        {
                            //light lso
                            List<Vector4> outLSOList = null;
                            if (!objTmpLSO.TryGetValue(code, out outLSOList))
                            {
                                outLSOList = new List<Vector4>();
                                objTmpLSO.Add(code, outLSOList);
                            }
                            int count = 0;
                            if (data.LSO != null)
                            {
                                count = data.LSO.Count;
                            }
                            for (int idx = 0; idx < count; ++idx)
                            {
                                outLSOList.Add(data.LSO[idx]);
                            }
                        }
                        else
                        {
                            //增加LightProbeOCC
                            List<float> ouLightProbeOccList = null;
                            if (!objTmpLightProbeOcc.TryGetValue(code, out ouLightProbeOccList))
                            {
                                ouLightProbeOccList = new List<float>();
                                objTmpLightProbeOcc.Add(code, ouLightProbeOccList);
                            }
                            int count = vCount;
                            for (int idx = 0; idx < count; ++idx)
                            {
                                ouLightProbeOccList.Add(lightProbeOcc);
                            }
                        }
                    }
                }
                //以code为单位，分组的物件矩阵数组
                foreach (var tmp in objTmpTRS)
                {
                    group.instMatxDict.Add(tmp.Key, tmp.Value.ToArray());
                }

                foreach (var tmp in objTmpColor)
                {
                    group.instColorDict.Add(tmp.Key, tmp.Value.ToArray());
                }

                foreach (var tmp in objTmpLSO)
                {
                    group.instLightmapScaleOffsetDict.Add(tmp.Key, tmp.Value.ToArray());
                }

                foreach (var tmp in objTmpLightProbeOcc)
                {
                    group.instLightProbeOcclusionDict.Add(tmp.Key, tmp.Value.ToArray());
                }

                return true;
            }
            return false;
        }
#endregion

#region CullingGroup

        static public bool InitializeCullingGroup(Camera mainCamera, ref CullingGroup cullingGroup, 
            ref BoundingSphere[] boundingSphere, ref int usedBoundingSphereCount,
            CullingGroup.StateChanged OnStateChanged)
        {
            if (cullingGroup == null)
            {
                usedBoundingSphereCount = 0;
                cullingGroup = new CullingGroup();
                cullingGroup.enabled = false;
            }

            if (mainCamera && cullingGroup.targetCamera != mainCamera)//! cullingGroup.enabled || 
            {
                cullingGroup.targetCamera = mainCamera;
                cullingGroup.onStateChanged = OnStateChanged;
                cullingGroup.SetBoundingSpheres(boundingSphere);
                cullingGroup.SetBoundingSphereCount(usedBoundingSphereCount);

                // cullingGroup.SetDistanceReferencePoint(cullingGroup.targetCamera.transform);
                // cullingGroup.SetBoundingDistances (cullingLodDistance);
                cullingGroup.enabled = true;
            }


            return  (cullingGroup != null && cullingGroup.enabled);
        }

        static public void BuildCullingGroupByGroupData(SimpleSaveData savedData, CullingGroup cullingGroup,
            SimpleInstancingGroup[] instancingGroupAry, BoundingSphere[] boundingSphere,ref int usedBoundingSphereCount,
            ref Byte[] groupVisibleDirtyAry)
        {
            for (int i = 0; i < instancingGroupAry.Length; ++i)
            {
                //AddCullingBoundingSphere(instancingGroupAry[i]);

                if (usedBoundingSphereCount > boundingSphere.Length - 1)
                {
                    return;
                }
                boundingSphere[usedBoundingSphereCount++] = instancingGroupAry[i].BoundingSpere;
                cullingGroup.SetBoundingSphereCount(usedBoundingSphereCount);
                bool isVisible = cullingGroup.IsVisible(usedBoundingSphereCount - 1);
                instancingGroupAry[i].IsVisible = isVisible;

                if (isVisible)
                    groupVisibleDirtyAry[i] = Common.GROUP_VISIBLE;
                else
                    groupVisibleDirtyAry[i] = Common.GROUP_INVISIBLE;

            }

        }

#endregion

#region BuildProtoTypeMeshData

        //参考：LoadProtoTypeMeshData()
        static public bool BuildProtoTypeMeshData(int verNum,string sceneTag, int prototypeIdx, int prototypeDataIndex,
            SimpleInstancingMgr instancingMgr, ref List<PrototypeInfo> prototypeList, List<ProtoTypeData> saveDataProtoTypes, 
            SimpleGrassLightmapDic lightmapDic,
            Delegate_GetCodeByMD5 GetCodeByMD5Fun, Delegate_GetLightMapNum GetLightMapNumFun,
            Delegate_GetLightMapIndexes GetLightMapIndexesFun)
        {

            Pool dataPool = instancingMgr.dataPool;
            //SimpleGrassLightmapDic lightmapDic = instancingMgr.lightmapDic;
            Dictionary<long, VertexCache> vertexCachePool = instancingMgr.vertexCachePool;
            bool UseInstancing = instancingMgr.UseInstancing;
            bool EnableInteract = instancingMgr.EnableInteract;

            Dictionary<long, int> dictRender = new Dictionary<long, int>();

            if(prototypeList[prototypeIdx] == null)
            {
                return false;
            }
            LodInfo[] lodInfo = prototypeList[prototypeIdx].lodInfoList;
            string prototypeName = prototypeList[prototypeIdx].prototypeName;
            if (lodInfo == null)
            {
                return false;
            }
            ///lightmap index
            List<int> outLightMapIdexes = new List<int>();

            string prefabName = Common.TryGetPrefabName(saveDataProtoTypes[prototypeDataIndex]);
            Debug.Assert(!string.IsNullOrEmpty(prefabName));

            bool castShadow = saveDataProtoTypes[prototypeDataIndex].CastShadows;
            ShadowCastingMode shadowCastMode = ShadowCastingMode.Off;
            if (castShadow)
            {
                shadowCastMode = ShadowCastingMode.On;
            }

            bool receiveShadows = saveDataProtoTypes[prototypeDataIndex].ReceiveShadows;
            int layerID = saveDataProtoTypes[prototypeDataIndex].Layer;
            bool useLightProb = saveDataProtoTypes[prototypeDataIndex].UseLightProb;
            dictRender.Clear();
            for (int x = 0; x != lodInfo.Length; ++x)
            {
                LodInfo lod = lodInfo[x];
                int indexVal = 0;

                int lodLevel = -1;
                if (lod.inLodGroup)
                {
                    lodLevel = lod.lodLevel;
                }
                for (int i = 0; i != lod.sharedMeshs.Length; ++i)
                {
                    Mesh m = lod.sharedMeshs[i];
                    if (m == null)
                        continue;
                    int identify = Common.GetMatIdentify(lod.sharedMaterials[i]);
                    if (identify == 0)
                        continue;
                    string meshName = m.name;  
                    string codeMD5 = "";
                    long code = Common.GetMeshHashCode_NoLightmapIdx(verNum,ref sceneTag, ref prototypeName, ref prefabName, ref meshName, lodLevel, ref codeMD5);
                    bool existedMD5 = false;
                    code = GetCodeByMD5Fun(codeMD5, code,ref existedMD5);
                    //
                    if (!dictRender.ContainsKey(code))
                    {
                        dictRender.Add(code, 0);
                    }
                    else
                    {
                        continue;
                    }

                    //lightmap相关信息
                    outLightMapIdexes.Clear();
                    int useLightMapNum = 0;
                    if (useLightProb)
                    {
                        outLightMapIdexes.Add(Common.INDEXTAG_USELIGHTPROBE);//-2);
                    }
                    else
                    {
                        GetLightMapIndexesFun(prototypeDataIndex, code, out outLightMapIdexes);//使用到的lightmap索引

                        if (outLightMapIdexes == null)
                        {
                            outLightMapIdexes = new List<int>();                     
                        }
                        outLightMapIdexes.Add(Common.INDEXTAG_NOTUSELIGHTMAP);//-1没使用Lightmap
                    }

                    useLightMapNum = outLightMapIdexes.Count;                    
                    for (int lmIdx = 0; lmIdx < useLightMapNum; ++lmIdx)
                    {
                        int lightmapIdx = outLightMapIdexes[lmIdx];
                        string nameMD5 = "";
                        long nameCode = Common.GetMeshHashCode(verNum, ref sceneTag, ref prototypeName, ref prefabName, ref meshName, lodLevel, lightmapIdx, ref nameMD5);
                        long orgNameCode = nameCode;
                        bool isExistedMD5 = false;
                        nameCode = GetCodeByMD5Fun(nameMD5, nameCode,ref isExistedMD5);
                        //验证：没Lightmap情景（兼容完善）  
                        if(!isExistedMD5 && (lightmapIdx == Common.INDEXTAG_NOTUSELIGHTMAP))//-1
                        {
                            continue;
                        }    
                        
                        int initInstNum = instancingPackageMaxSize;
                        // int identify = Common.GetMatIdentify(lod.sharedMaterials[i]);
                        VertexCache cache = null;
                        if (vertexCachePool.TryGetValue(nameCode, out cache))
                        {
                            MaterialBlock block = null;
                            if (!cache.instanceMatBlockList.TryGetValue(identify, out block))
                            {
                                block = _CreateMatBlock(dataPool, ref lightmapDic, prototypeIdx, cache, lod.sharedMaterials[i], initInstNum, lod.useLightMap, UseInstancing, EnableInteract, useLightProb);
                                cache.instanceMatBlockList.Add(identify, block);
                            }

                            lod.vertexCacheList[indexVal] = cache;
                            lod.materialBlockList[indexVal] = block;
                            lod.meshNameCodes[indexVal] = nameCode;

                            ++indexVal;
                            continue;
                        }
                        VertexCache vertexCache = _CreateVertexCache(ref vertexCachePool, prototypeIdx, prefabName, nameCode, m);
                        vertexCache.sceneTag = sceneTag.GetHashCode();
                        vertexCache.materials = lod.sharedMaterials[i];//meshRenderAry[i].sharedMaterials;
                        vertexCache.receiveShadow = receiveShadows;
                        vertexCache.shadowcastingMode = shadowCastMode;
                        vertexCache.layer = layerID;

                        //vertexCache.maxPackageInsNum = maxInstNum;
                        if (useLightProb)
                        {
                            vertexCache.lightmapIndex = Common.INDEXTAG_USELIGHTPROBE;//-2
                        }
                        else
                        {
                            if (lod.useLightMap)
                                vertexCache.lightmapIndex = lightmapIdx;
                            else
                                vertexCache.lightmapIndex = Common.INDEXTAG_NOTUSELIGHTMAP;// - 1;
                        }

                        MaterialBlock matBlock = _CreateMatBlock(dataPool,ref lightmapDic, prototypeIdx, vertexCache, lod.sharedMaterials[i],
                            initInstNum, lod.useLightMap, UseInstancing, EnableInteract, useLightProb);
                        vertexCache.instanceMatBlockList.Add(identify, matBlock);

                        lod.vertexCacheList[indexVal] = vertexCache;
                        lod.materialBlockList[indexVal] = matBlock;
                        lod.meshNameCodes[indexVal] = nameCode;

                        ++indexVal;
                    }
                }
            }
            prototypeList[prototypeIdx].isReady = true;
            return true;
        }

        private static MaterialBlock _CreateMatBlock(Pool dataPool, ref SimpleGrassLightmapDic lightmapDic, int protoTypeIdx, VertexCache cache, 
            Material[] orgMaterials, int maxInstNum, bool useLightMap,bool UseInstancing, bool EnableInteract,bool useLightProbe)
        {
            Mesh cacheMesh = cache.mesh;
            MaterialBlock block = new MaterialBlock();
            block.material = new Material[cacheMesh.subMeshCount];

            block.subMeshCount = cacheMesh.subMeshCount;
            for (int i = 0; i != cacheMesh.subMeshCount; ++i)
            {
                block.material[i] = new Material(orgMaterials[i]);
                //#if UNITY_5_6_OR_NEWER
                block.material[i].enableInstancing = UseInstancing;
                //#endif

                //if (UseInstancing)
                //{
                //    block.material[i].EnableKeyword("INSTANCING_ON");
                //}
                //else
                //{
                //    block.material[i].DisableKeyword("INSTANCING_ON");
                //}

                block.propertyBlock = new MaterialPropertyBlock();
            }

            block.packageList = new List<InstancingPackage>();
            InstancingPackage package = _CreatePackage(dataPool, maxInstNum, useLightMap, useLightProbe);
            block.packageList.Add(package);

            _PrepareInstancingMaterial(ref lightmapDic, ref cache, ref block, UseInstancing);
            package.instancingCount = 0;

            //交互材质     
            //if (EnableInteract)
            //{
            //    block.material_Interact = new Material[block.material.Length];
            //    for (int index = 0; index < block.material_Interact.Length; ++index)
            //    {
            //        block.material_Interact[index] = new Material(block.material[index]);
            //        block.material_Interact[index].EnableKeyword("CUSTOM_INTERACT");
            //    }
            //}
            return block;
        }

        private static VertexCache _CreateVertexCache(ref Dictionary<long, VertexCache> vertexCachePool,
            int protoTypeIdx, string prefabName, long nameCode, Mesh mesh)
        {
            VertexCache vertexCache = new VertexCache();
            long cacheName = nameCode;//renderName + alias;
            vertexCachePool[cacheName] = vertexCache;
            vertexCache.nameCode = cacheName;
            vertexCache.mesh = mesh;

            vertexCache.instanceMatBlockList = new Dictionary<int, MaterialBlock>();
            return vertexCache;
        }

        private static InstancingPackage _CreatePackage(Pool dataPool, int maxInstNum, bool useLightMap,bool useLightProbe)
        {
            InstancingPackage package = new InstancingPackage();

            int size = instancingPackageMaxSize;

            _CreateInstanceData(dataPool, package, size, useLightMap, useLightProbe);
            package.instancingCount = 0;
            return package;

        }

        private static void _CreateInstanceData(Pool dataPool, InstancingPackage package, int maxInstNum, bool useLightMap,bool useLightProbe)
        {
            package.worldMatrix = dataPool.GetMatx(); //dataPool.PullMatx();// data.worldMatrix = new Matrix4x4[maxInstNum];
            package.color = dataPool.GetVec4();
            if (useLightMap)
            {
                package.lightmapScaleOffset = dataPool.GetVec4();//dataPool.PullVec4();// data.lightmapScaleOffset = new Vector4[maxInstNum];
            }

            if(useLightProbe)
            {
                package.lightProbOcclusion = dataPool.GetFloat();
            }
        }

        private static void _PrepareInstancingMaterial(ref SimpleGrassLightmapDic lightmapDic,ref VertexCache vertexCache, ref MaterialBlock block, bool UseInstancing)
        {
            int subMeshCount = block.subMeshCount;
            for (int j = 0; j != subMeshCount; ++j)
            {
                block.propertyBlock.SetFloat(Common.PID_OPEN_LIGHTPROBEOCC, 0);
                _PrepareInstancingMaterial_Lightmaps(ref lightmapDic, ref vertexCache, ref block.material[j], ref block);
                //if (lightmapDic != null && lightmapDic.Count() > 0)
                //{
                //    if (vertexCache.lightmapIndex >= 0)
                //    {
                //        Texture2D lightmapColor = null;
                //        Texture2D shadowMask = null;
                //        Texture2D lightmapDir = null;
                //        if (lightmapDic.GetLightMaps(vertexCache.lightmapIndex, out lightmapColor, out shadowMask, out lightmapDir))
                //        {
                //            block.material[j].EnableKeyword("LIGHTMAP_ON");
                //            block.material[j].EnableKeyword("DIRLIGHTMAP_COMBINED");
                //            block.material[j].EnableKeyword("SHADOWS_SHADOWMASK");
                //            block.material[j].EnableKeyword("SKIP_LIGHTPROBE_SH");//手工的忽略LIGHTPROBE_SH
                //            block.material[j].EnableKeyword("CUSTOM_GPUINSTANCING");                           
                //            if (lightmapColor != null)
                //            {
                //                block.propertyBlock.SetTexture(Common.PID_UNITY_LIGHTMAP, lightmapColor);
                //            }
                //            if (shadowMask != null)
                //            {
                //                block.propertyBlock.SetTexture(Common.PID_UNITY_SHADOWMASK, shadowMask);
                //            }
                //            if (lightmapDir != null)
                //            {
                //                block.propertyBlock.SetTexture(Common.PID_UNITY_LIGHTMAPIND, lightmapDir);
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    if (vertexCache.lightmapIndex >= 0 && (vertexCache.lightmapIndex < LightmapSettings.lightmaps.Length))
                //    {
                //        block.material[j].EnableKeyword("LIGHTMAP_ON");
                //        block.material[j].EnableKeyword("DIRLIGHTMAP_COMBINED");
                //        block.material[j].EnableKeyword("SHADOWS_SHADOWMASK");
                //        block.material[j].EnableKeyword("SKIP_LIGHTPROBE_SH");//手工的忽略LIGHTPROBE_SH
                //        block.material[j].EnableKeyword("CUSTOM_GPUINSTANCING");                        
                //        if (LightmapSettings.lightmaps[vertexCache.lightmapIndex].lightmapColor != null)
                //        {
                //            block.propertyBlock.SetTexture(Common.PID_UNITY_LIGHTMAP, LightmapSettings.lightmaps[vertexCache.lightmapIndex].lightmapColor);
                //        }
                //        if (LightmapSettings.lightmaps[vertexCache.lightmapIndex].shadowMask != null)
                //        {
                //            block.propertyBlock.SetTexture(Common.PID_UNITY_SHADOWMASK, LightmapSettings.lightmaps[vertexCache.lightmapIndex].shadowMask);
                //        }
                //        if (LightmapSettings.lightmaps[vertexCache.lightmapIndex].lightmapDir != null)
                //        {
                //            block.propertyBlock.SetTexture(Common.PID_UNITY_LIGHTMAPIND, LightmapSettings.lightmaps[vertexCache.lightmapIndex].lightmapDir);
                //        }
                //    }
                //}

                if (!UseInstancing)
                {
                    block.material[j].DisableKeyword("CUSTOM_GPUINSTANCING");
                }
   
                if (vertexCache.lightmapIndex == Common.INDEXTAG_USELIGHTPROBE)
                {
                    block.propertyBlock.SetFloat(Common.PID_OPEN_LIGHTPROBEOCC, 1);

                    block.material[j].DisableKeyword("LIGHTMAP_ON");
                    block.material[j].EnableKeyword("CUSTOM_GPUINSTANCING");                   
                    if (!UseInstancing)
                    {
                        block.material[j].DisableKeyword("CUSTOM_GPUINSTANCING");
                    }
                }
            }
        }

        private static void _PrepareInstancingMaterial_Lightmaps(ref SimpleGrassLightmapDic lightmapDic, 
            ref VertexCache vertexCache, ref Material material, ref MaterialBlock block)
        {
            if (lightmapDic != null && lightmapDic.Count() > 0)
            {
                if (vertexCache.lightmapIndex >= 0)
                {
                    Texture2D lightmapColor = null;
                    Texture2D shadowMask = null;
                    Texture2D lightmapDir = null;
                    if (lightmapDic.GetLightMaps(vertexCache.lightmapIndex, out lightmapColor, out shadowMask, out lightmapDir))
                    {
                        material.EnableKeyword("LIGHTMAP_ON");
                        material.EnableKeyword("DIRLIGHTMAP_COMBINED");
                        material.EnableKeyword("SHADOWS_SHADOWMASK");
                        material.EnableKeyword("SKIP_LIGHTPROBE_SH");//手工的忽略LIGHTPROBE_SH
                        material.EnableKeyword("CUSTOM_GPUINSTANCING");
                        if (lightmapColor != null)
                        {
                            block.propertyBlock.SetTexture(Common.PID_UNITY_LIGHTMAP, lightmapColor);
                        }
                        if (shadowMask != null)
                        {
                            block.propertyBlock.SetTexture(Common.PID_UNITY_SHADOWMASK, shadowMask);
                        }
                        if (lightmapDir != null)
                        {
                            block.propertyBlock.SetTexture(Common.PID_UNITY_LIGHTMAPIND, lightmapDir);
                        }
                    }
                }
            }
            else
            {
                if (vertexCache.lightmapIndex >= 0 && (vertexCache.lightmapIndex < LightmapSettings.lightmaps.Length))
                {
                    material.EnableKeyword("LIGHTMAP_ON");
                    material.EnableKeyword("DIRLIGHTMAP_COMBINED");
                    material.EnableKeyword("SHADOWS_SHADOWMASK");
                    material.EnableKeyword("SKIP_LIGHTPROBE_SH");//手工的忽略LIGHTPROBE_SH
                    material.EnableKeyword("CUSTOM_GPUINSTANCING");
                    if (LightmapSettings.lightmaps[vertexCache.lightmapIndex].lightmapColor != null)
                    {
                        block.propertyBlock.SetTexture(Common.PID_UNITY_LIGHTMAP, LightmapSettings.lightmaps[vertexCache.lightmapIndex].lightmapColor);
                    }
                    if (LightmapSettings.lightmaps[vertexCache.lightmapIndex].shadowMask != null)
                    {
                        block.propertyBlock.SetTexture(Common.PID_UNITY_SHADOWMASK, LightmapSettings.lightmaps[vertexCache.lightmapIndex].shadowMask);
                    }
                    if (LightmapSettings.lightmaps[vertexCache.lightmapIndex].lightmapDir != null)
                    {
                        block.propertyBlock.SetTexture(Common.PID_UNITY_LIGHTMAPIND, LightmapSettings.lightmaps[vertexCache.lightmapIndex].lightmapDir);
                    }
                }
            }
        }


        #endregion

#region  Render相关
        //ResetRender
        public static void ResetRenderCache(Pool dataPool, ref Dictionary<int, bool> protoTypeDirty,
            ref List<PrototypeInfo> prototypeList)
        {
            foreach (int prototypeIndex in protoTypeDirty.Keys)
            {
                PrototypeInfo protoInfo = prototypeList[prototypeIndex];
                LodInfo[] lods = protoInfo.lodInfoList;
                if (lods != null)
                {
                    for (int i = 0; i < lods.Length; ++i)
                    {
                        VertexCache[] cacheList = lods[i].vertexCacheList;
                        if (cacheList != null)
                        {
                            for (int j = 0; j < cacheList.Length; ++j)
                            {
                                _ResetVertexCache(dataPool,cacheList[j]);
                            }
                        }
                    }
                }
            }         
        }

        public static void ResetRenderCacheBy(Pool dataPool, ref Dictionary<int, bool> protoTypeDirty,
            ref List<PrototypeInfo> prototypeList, int beginPrototypeIndex = -1, int endPrototypeIndex = -1)
        {
            bool handleAll = (beginPrototypeIndex == -1 && endPrototypeIndex == -1);
            foreach (int prototypeIndex in protoTypeDirty.Keys)
            {
                bool handle = handleAll || (prototypeIndex >= beginPrototypeIndex && prototypeIndex <= endPrototypeIndex);
                if (handle)
                {
                    PrototypeInfo protoInfo = prototypeList[prototypeIndex];
                    LodInfo[] lods = protoInfo.lodInfoList;
                    if (lods != null)
                    {
                        for (int i = 0; i < lods.Length; ++i)
                        {
                            VertexCache[] cacheList = lods[i].vertexCacheList;
                            if (cacheList != null)
                            {
                                for (int j = 0; j < cacheList.Length; ++j)
                                {
                                    _ResetVertexCache(dataPool, cacheList[j]);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void _ResetVertexCache(Pool dataPool, VertexCache vertexCache)
        {
            if (vertexCache == null)
                return;
            //以使用的材质为单位
            foreach (MaterialBlock block in vertexCache.instanceMatBlockList.Values)
            {
                List<InstancingPackage> packageList = block.packageList;
                //以每次最大渲染实例数量限制为单位（package）
                for (int i = 0; i != packageList.Count; ++i)
                {
                    packageList[i].instancingCount = 0;
                    packageList[i].isInInteract = false;

                    if (packageList[i].worldMatrix != null)
                    {
                        dataPool.RecMatx(packageList[i].worldMatrix);
                        // packageList[i].worldMatrix.isUsed = false;
                        packageList[i].worldMatrix = null;
                    }

                    if (packageList[i].lightmapScaleOffset != null)
                    {
                        dataPool.RecVector4(packageList[i].lightmapScaleOffset);
                        // packageList[i].lightmapScaleOffset.isUsed = false;
                        packageList[i].lightmapScaleOffset = null;
                    }

                    if (packageList[i].color != null)
                    {
                        dataPool.RecVector4(packageList[i].color);
                        // packageList[i].worldMatrix.isUsed = false;
                        packageList[i].color = null;
                    }

                    if (packageList[i].lightProbOcclusion != null)
                    {
                        dataPool.RecFloat(packageList[i].lightProbOcclusion);
                        packageList[i].lightProbOcclusion = null;
                    }
                }
                block.runtimePackageIndex = 0;
            }
        }

        //public static void UpdateRenderData_SingleMode(int protoIndex, SimpleInstancingGroup instancingGroup,
        //    List<PrototypeInfo> prototypeList, SimpleInstancingMgr instancingMgr)
        //{
        //    instancingGroup.RealVisibleInstNum = 0;
        //    instancingGroup.VisibleInstNum = 0;
        //    instancingGroup.VisibleMeshNum = 0;
        //    PrototypeInfo protoInfo = prototypeList[protoIndex];
        //    bool useLightProbe = protoInfo.UseLightProb;
        //    List<SimpleInstance> instList = instancingGroup.instList;
        //    if (protoInfo == null || protoInfo.lodInfoList == null)
        //    {
        //        return;
        //    }
        //    int iLodInfoLen = protoInfo.lodInfoList.Length;
        //    for (int i = 0; i < instList.Count; ++i)
        //    {
        //        int iLodLevel = instList[i].lodLevel;
        //        if (iLodLevel < 0)
        //            continue;
        //        if (iLodLevel >= iLodInfoLen)
        //        {
        //            continue;
        //        }
        //        LodInfo lod = protoInfo.lodInfoList[iLodLevel];
        //        for (int j = 0; j != lod.vertexCacheList.Length; ++j)
        //        {
        //            //lod对应的meshhashcode
        //            long meshNameCode = lod.meshNameCodes[j];

        //            Matrix4x4[] matxAry = null;
        //            if (!instList[i].matxDict.TryGetValue(meshNameCode,out matxAry))
        //                continue;
        //            //if (!instList[i].matxDict.ContainsKey(meshNameCode))
        //            //    continue;
        //            //Matrix4x4[] matxAry = instList[i].matxDict[meshNameCode];
        //            int maxlen = matxAry.Length;
        //            Vector4[] lmOffsetAry = instList[i].lightmapScaleOffsetDict[meshNameCode];
        //            float[] lightProbOcclusionAry = instList[i].lightProbOcclusionDict[meshNameCode];
        //            int instCount = matxAry.Length;
        //            int orgInstCount = instCount;
        //            //if (instancingGroup.DensityLod >= 0)
        //            {
        //                int realCount = (int)(instCount * instancingGroup.DensityCutDown);
        //                if (realCount > 0)
        //                {
        //                    instCount = realCount;
        //                    if (!instancingMgr.UseInstancing)
        //                    {
        //                        instCount = Math.Min(instCount, Common.NotGPUInstancing_MaxNum);
        //                    }
        //                }
        //                else
        //                {
        //                    instCount = Math.Min(instCount, 1);
        //                }
        //            }
        //            instCount = Math.Min(instCount, maxlen);
        //            //test
        //            //if(Common.ForceDensity != 1)
        //            //{
        //            //    int realCount = (int)(instCount * Common.ForceDensity);
        //            //    if(realCount > 0)
        //            //    {
        //            //        instCount = realCount;
        //            //    }
        //            //    else
        //            //    {
        //            //        instCount = Math.Min(instCount, 1);
        //            //    }
        //            //}
        //            //
        //            if (instCount == 0)
        //                continue;
        //            MaterialBlock block = lod.materialBlockList[j];
        //            Debug.Assert(block != null);

        //            int packageIndex = block.runtimePackageIndex;
        //            InstancingPackage package = block.packageList[packageIndex];

        //            instancingMgr.ResetInstanceData(package, lod.useLightMap, useLightProbe);
        //            // Common.TestVisibleInstanceNum += instCount;
        //            instancingGroup.VisibleInstNum += orgInstCount;
        //            instancingGroup.VisibleMeshNum += instCount;//LOD模式一个实例一般为一个MESH显示
        //            instancingGroup.RealVisibleInstNum += instCount;
        //            //需要扩展package
        //            if (package.instancingCount + instCount > instancingPackageMaxSize)// if (package.instancingCount + instCount > dataSize)//instancingPackageMaxSize
        //            {
        //                int sourceIdx = 0;
        //                int leftNum = instCount;
        //                int copyNum = instancingPackageMaxSize - package.instancingCount;
        //                while (leftNum > 0)
        //                {
        //                    Array.Copy(matxAry, sourceIdx,
        //                               package.worldMatrix.matrPtr, package.instancingCount, copyNum);
        //                    if (useLightProbe)
        //                    {
        //                        Array.Copy(lightProbOcclusionAry, sourceIdx,
        //                               package.lightProbOcclusion.floatPtr, package.instancingCount, copyNum);
        //                    }
        //                    else
        //                    {
        //                        if (lod.useLightMap)
        //                        {
        //                            Array.Copy(lmOffsetAry, sourceIdx,
        //                                   package.lightmapScaleOffset.vector4Ptr, package.instancingCount, copyNum);
        //                        }
        //                    }

        //                    package.instancingCount += copyNum;
        //                    // package.isInInteract = package.isInInteract || instancingGroup.IsInInteract;
        //                    sourceIdx += copyNum;
        //                    leftNum -= copyNum;
        //                    if (leftNum > 0)
        //                    {
        //                        ++block.runtimePackageIndex;
        //                        packageIndex = block.runtimePackageIndex;
        //                        if (packageIndex >= block.packageList.Count)
        //                        {
        //                            InstancingPackage newPackage = instancingMgr.CreatePackage(instancingPackageMaxSize, lod.useLightMap, useLightProbe);
        //                            block.packageList.Add(newPackage);
        //                            package = newPackage;
        //                        }
        //                        else
        //                        {
        //                            package = block.packageList[packageIndex];
        //                            instancingMgr.ResetInstanceData(package, lod.useLightMap, useLightProbe);
        //                        }
        //                        if (leftNum > instancingPackageMaxSize)
        //                            copyNum = instancingPackageMaxSize;
        //                        else
        //                            copyNum = leftNum;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //不需要扩展package
        //                Array.Copy(matxAry, 0, package.worldMatrix.matrPtr, package.instancingCount, instCount);
        //                if (useLightProbe)
        //                {
        //                    Array.Copy(lightProbOcclusionAry, 0, package.lightProbOcclusion.floatPtr, package.instancingCount, instCount);
        //                }
        //                else
        //                {
        //                    if (lod.useLightMap)
        //                    {
        //                        Array.Copy(lmOffsetAry, 0, package.lightmapScaleOffset.vector4Ptr, package.instancingCount, instCount);
        //                    }
        //                }
        //                package.instancingCount += instCount;
        //                // package.isInInteract = package.isInInteract || instancingGroup.IsInInteract;
        //            }
        //        }

        //    }
        //}

        public static void UpdateRenderData_BatchMode(LodInfo lod, SimpleInstancingGroup instancingGroup, SimpleInstancingMgr instancingMgr, bool useLightProbe)
        {
            instancingGroup.VisibleMeshNum = 0;

            int num = instancingGroup.OrgInstNum;
            //if (instancingGroup.DensityLod >= 0)
            {
                int realCount = (int)(num * instancingGroup.DensityCutDown);
                if (realCount > 0)
                {
                    num = realCount;
                    if (!instancingMgr.UseInstancing)
                    {
                        num = Math.Min(num, Common.NotGPUInstancing_MaxNum);
                    }
                }
                else
                {
                    num = Math.Min(num, 1);
                }
            }

            instancingGroup.VisibleInstNum = instancingGroup.OrgInstNum;
            instancingGroup.RealVisibleInstNum = num;
            //
            for (int j = 0; j != lod.vertexCacheList.Length; ++j)
            {
                //lod对应的hashcode
                long code = lod.meshNameCodes[j];
                Matrix4x4[] instMatAry = null;
                Vector4[] instColorAry = null;
                if (!instancingGroup.instMatxDict.TryGetValue(code,out instMatAry))
                    continue;

                if (!instancingGroup.instColorDict.TryGetValue(code, out instColorAry))
                    continue;

                int maxlen = instMatAry.Length;
                //if (!instancingGroup.instMatxDict.ContainsKey(code))
                //    continue;
                //int maxlen = instancingGroup.instMatxDict[code].Length;
                int instCount = maxlen;

                //if (instancingGroup.DensityLod >= 0)
                {
                    int realCount = (int)(instCount * instancingGroup.DensityCutDown);
                    if (realCount > 0)
                    {
                        instCount = realCount;
                        if (!instancingMgr.UseInstancing)
                        {
                            instCount = Math.Min(instCount, Common.NotGPUInstancing_MaxNum);
                        }

                    }
                    else
                    {
                        instCount = Math.Min(instCount, 1);
                    }
                }

                instCount = Math.Min(instCount, maxlen);
             
                if (instCount == 0)
                    continue;
                //
                //VertexCache cache = lod.vertexCacheList[j];
                MaterialBlock block = lod.materialBlockList[j];
                Debug.Assert(block != null);

                int packageIndex = block.runtimePackageIndex;
                InstancingPackage package = block.packageList[packageIndex];

                //int dataSize = package.dataSize;
                instancingMgr.ResetInstanceData(package, lod.useLightMap, useLightProbe);

                //test sort
                //  Matrix4x4[] mat = instancingGroup.instMatxDict[meshNameCode];
                // Vector4[] vet = instancingGroup.instLightmapScaleOffsetDict[meshNameCode];
                // testSortInGroup(ref mat, ref vet);
                //
                // Common.TestVisibleInstanceNum += instCount;
                instancingGroup.VisibleMeshNum += instCount;

                //需要扩展package
                if (package.instancingCount + instCount > instancingPackageMaxSize)// if (package.instancingCount + instCount > dataSize)//instancingPackageMaxSize
                {                    
                    float[] lightProbOcclusionAry = null;
                    instancingGroup.instLightProbeOcclusionDict.TryGetValue(code, out lightProbOcclusionAry);
                    int sourceIdx = 0;
                    int leftNum = instCount;
                    //int copyNum = dataSize - package.instancingCount;//
                    int copyNum = instancingPackageMaxSize - package.instancingCount;
                    while (leftNum > 0)
                    {
                        Array.Copy(instMatAry, sourceIdx,package.worldMatrix.matrPtr, package.instancingCount, copyNum);
                        Array.Copy(instColorAry, sourceIdx, package.color.vector4Ptr, package.instancingCount, copyNum);
                        if (useLightProbe && lightProbOcclusionAry != null)
                        {
                            Array.Copy(lightProbOcclusionAry, sourceIdx,
                                   package.lightProbOcclusion.floatPtr, package.instancingCount, copyNum);
                        }
                        else
                        {
                            Vector4[] lightmapScaleOffsetAry = null;
                            instancingGroup.instLightmapScaleOffsetDict.TryGetValue(code, out lightmapScaleOffsetAry);
                            if (lod.useLightMap && lightmapScaleOffsetAry != null)
                            {
                                Array.Copy(lightmapScaleOffsetAry, sourceIdx,
                                       package.lightmapScaleOffset.vector4Ptr, package.instancingCount, copyNum);
                            }
                        }

                        package.instancingCount += copyNum;
                        package.isInInteract = package.isInInteract || instancingGroup.IsInInteract;

                        sourceIdx += copyNum;
                        leftNum -= copyNum;
                        if (leftNum > 0)
                        {
                            ++block.runtimePackageIndex;
                            packageIndex = block.runtimePackageIndex;
                            if (packageIndex >= block.packageList.Count)
                            {
                                InstancingPackage newPackage = instancingMgr.CreatePackage(instancingPackageMaxSize, lod.useLightMap, useLightProbe);//InstancingPackage newPackage = CreatePackage(dataSize,lod.useLightMap);
                                block.packageList.Add(newPackage);
                                // PreparePackageMaterial(newPackage, cache);
                                package = newPackage;
                            }
                            else
                            {
                                package = block.packageList[packageIndex];
                                instancingMgr.ResetInstanceData(package, lod.useLightMap,useLightProbe);
                            }

                            //if (leftNum > package.dataSize)
                            //    copyNum = package.dataSize;
                            //else
                            //    copyNum = leftNum;

                            if (leftNum > instancingPackageMaxSize)
                                copyNum = instancingPackageMaxSize;
                            else
                                copyNum = leftNum;
                        }
                    }
                }
                else
                {
                    float[] lightProbOcclusionAry = null;
                    instancingGroup.instLightProbeOcclusionDict.TryGetValue(code, out lightProbOcclusionAry);
                    //不需要扩展package
                    Array.Copy(instMatAry, 0, package.worldMatrix.matrPtr, package.instancingCount, instCount);
                    Array.Copy(instColorAry, 0, package.color.vector4Ptr, package.instancingCount, instCount);
                    if (useLightProbe && lightProbOcclusionAry != null)
                    {
                        Array.Copy(lightProbOcclusionAry, 0, package.lightProbOcclusion.floatPtr, package.instancingCount, instCount);
                    }
                    else
                    {
                        Vector4[] lightmapScaleOffsetAry = null;
                        instancingGroup.instLightmapScaleOffsetDict.TryGetValue(code, out lightmapScaleOffsetAry);
                        if (lod.useLightMap && lightmapScaleOffsetAry != null)
                        {                           
                            Array.Copy(instancingGroup.instLightmapScaleOffsetDict[code], 0,
                                   package.lightmapScaleOffset.vector4Ptr, package.instancingCount, instCount);
                        }
                    }
                    package.instancingCount += instCount;
                    package.isInInteract = package.isInInteract || instancingGroup.IsInInteract;
                }
            }
        }

        public static void ClearInstancingDataBySceneTag(SimpleInstancingMgr instancingMgr, int sceneTag, bool clearLightmapSetting)
        {
            Dictionary<long, VertexCache> vertexCachePool = instancingMgr.vertexCachePool;
            var enumerator = vertexCachePool.GetEnumerator();
            while (enumerator.MoveNext())
            {
                VertexCache vertexCache = enumerator.Current.Value;
                //
                if (vertexCache.sceneTag == sceneTag)
                {
                    //回收缓存
                    _ResetVertexCache(instancingMgr.dataPool, vertexCache);


                    //清除LIGHTMAP引用
                    if (clearLightmapSetting)
                    {
                        var blockIT = vertexCache.instanceMatBlockList.GetEnumerator();
                        while (blockIT.MoveNext())
                        {
                            MaterialBlock block = blockIT.Current.Value;
                            block.propertyBlock.Clear();
                            //block.propertyBlock.SetTexture(Common.PID_UNITY_LIGHTMAP, null);
                            //block.propertyBlock.SetTexture(Common.PID_UNITY_SHADOWMASK, null);
                            //block.propertyBlock.SetTexture(Common.PID_UNITY_LIGHTMAPIND, null);
                        }
                    }
                }                
            }
        }

        public static void ResetInstancingDataBySceneTag(SimpleInstancingMgr instancingMgr, ref SimpleGrassLightmapDic lightmapDic,int sceneTag)
        {
            Dictionary<long, VertexCache> vertexCachePool = instancingMgr.vertexCachePool;
            var enumerator = vertexCachePool.GetEnumerator();
            while (enumerator.MoveNext())
            {
                VertexCache vertexCache = enumerator.Current.Value;
                if (vertexCache.sceneTag == sceneTag)
                {                    
                    //重置LIGHTMAP引用
                    var blockIT = vertexCache.instanceMatBlockList.GetEnumerator();
                    while (blockIT.MoveNext())
                    {
                        MaterialBlock block = blockIT.Current.Value;
                        int subMeshCount = block.subMeshCount;
                        for (int j = 0; j != subMeshCount; ++j)
                        {
                            _PrepareInstancingMaterial_Lightmaps(ref lightmapDic, ref vertexCache, ref block.material[j], ref block);
                        }                         
                    }
                }
            }

        }

        #endregion

        public static void OnDrawGizmos_GroupSpere(SimpleInstancingMgr instancingMgr, SimpleInstancingGroup[] instancingGroupAry)
        {
            Color LodColor0 = new Color(0f, 0f, 1f);
            Color LodColor1 = new Color(0f, 1f, 0f);
            Color LodColor2 = new Color(1f, 0f, 0f);
            //Gizmos.color = Color.blue;
            if (instancingGroupAry != null)
            {
                for (int i = 0; i < instancingGroupAry.Length; ++i)
                {
                    if (instancingGroupAry[i] == null)
                    {
                        continue;
                    }
                    Gizmos.color = Color.blue;
                    if (instancingGroupAry[i].IsVisible)
                    {
                        if (instancingGroupAry[i].EnableLod)
                        {
                            if (instancingGroupAry[i].LodLevel == 0)
                                Gizmos.color = LodColor0;
                            else if (instancingGroupAry[i].LodLevel == 1)
                                Gizmos.color = LodColor1;
                            else if (instancingGroupAry[i].LodLevel == 2)
                                Gizmos.color = LodColor2;
                        }
                        else
                        {
                            Gizmos.color = LodColor0;
                        }

                        //if (instancingGroupAry[i].SpereIntersect)
                        //    Gizmos.color = Color.white;
                    }
                    else
                    {
                        Gizmos.color = Color.gray;
                    }

                    Gizmos.DrawWireSphere(instancingGroupAry[i].BoundingSpere.position, instancingGroupAry[i].BoundingSpere.radius);

                    //if (instancingMgr.EnableInteract)
                    //{
                    //    if (instancingGroupAry[i].IsInInteract)
                    //    {
                    //        Gizmos.color = Color.gray;
                    //        Gizmos.DrawWireSphere(instancingGroupAry[i].BoundingSpere.position, instancingGroupAry[i].BoundingSpere.radius * 0.9f);
                    //    }

                    //    Vector3 pos = Vector3.zero;
                    //    SimpleGrassGlobal.Global.GetHeroPos(out pos);
                    //    Gizmos.color = Color.gray;
                    //    Gizmos.DrawWireSphere(pos, Common.HeroInteractRadius);
                    //}

                    if (instancingGroupAry[i].IsVisible && instancingGroupAry[i].EnableLod)
                    {
                        for (int j = 0; j < instancingGroupAry[i].instList.Count; ++j)
                        {
                            if (instancingGroupAry[i].instList[j].lodLevel >= 0)
                            {
                                Gizmos.color = Color.gray;
                                Gizmos.DrawWireCube(instancingGroupAry[i].instList[j].meshBounds.center, instancingGroupAry[i].instList[j].meshBounds.size);
                            }
                        }
                    }

                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(instancingGroupAry[i].BoundingSpere.position, 0.2f);
                }
            }
        }

    }
}
