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
        CustomSampler sampler = null;//test

        //每次最大draw的实例个数。（限制1023)
        public static int instancingPackageMaxSize = 500;//1000

        public static int ThreadSleep_LodCalculate = 130;// < lodCalculateFrequency = 150;
        
        public enum UseLodType { LT_NONE, LT_ByGroup, LT_ByInstance }

        public SimpleGrassSys SimpleSys;

        public Camera mainCamera = null;

        #region 阴影设置
        private int enableShadowID = Shader.PropertyToID("_UseRoleRealtimeShadow");
        private int shadowMapID = Shader.PropertyToID("_RoleRealtimeShadowMap");
        private int shadowMatrixID = Shader.PropertyToID("_RoleShadowMatrix");
        private int shadowIntensityID = Shader.PropertyToID("_RoleRealtimeShadowIntensity");
        public ProjectorShadow shadowProjector = null;
        public float shadowIntensity = 1.0f;
        public bool isReceiveShadow = true;
        #endregion

        #region 植被数据定义        
        //事例的数据分包(1000个）
        public class InstancingPackage
        {
            public int instancingCount = 0;
            public Data_Matx worldMatrix;
            public Data_Vector4 color;
            public Data_Vector4 lightmapScaleOffset;
            public Data_Float lightProbOcclusion;
            public bool isInInteract = false;
        }

        //MESH下材质相关的数据
        public class MaterialBlock
        {
            public int subMeshCount = 1;
            public Material[] material;
            public Material[] material_Interact;
            public MaterialPropertyBlock propertyBlock;

            public int runtimePackageIndex = 0;
            public List<InstancingPackage> packageList;

            public Material GetMaterial(int matIndex, bool isInInteract)
            {
                //  return material[matIndex];
                if (!isInInteract)
                {
                    return material[matIndex];
                }

                ////交互材质
                if (material_Interact == null)
                {
                    material_Interact = new Material[material.Length];
                    for (int index = 0; index < material_Interact.Length; ++index)
                    {
                        material_Interact[index] = new Material(material[index]);
                        material_Interact[index].EnableKeyword("_CUSTOM_INTERACT");
                    }
                }
                return material_Interact[matIndex];
            }
        }

        //mesh相关的数据
        public class VertexCache
        {
            public int sceneTag = 0;
            public long nameCode = 0;
            public Mesh mesh = null;
            public Dictionary<int, MaterialBlock> instanceMatBlockList;//材质标识的hashcode为KEY
            public Material[] materials = null;

            public ShadowCastingMode shadowcastingMode;
            public bool receiveShadow = false;
            public int layer = 0;
            public int lightmapIndex = -1;
            public bool isDirty = false;
        }
        private Dictionary<long, VertexCache> vertexCachePool = new Dictionary<long, VertexCache>();//mesh相关的hashcode为KEY

        public class LodInfo
        {
            public int lodLevel;
            public Mesh[] sharedMeshs;
            public Material[][] sharedMaterials;
            public long[] meshNameCodes;
            public VertexCache[] vertexCacheList;
            public MaterialBlock[] materialBlockList;
            public bool inLodGroup = false;
            public bool useLightMap = false;
        }

        public class PrototypeInfo
        {
            public string prototypeName = "";
            public LodInfo[] lodInfoList;

            public float[] protoTypeLods = null;

            public float protoTypeCullMaxDists = 0;

            public float protoTypeLodGroupSize = 0;

            public bool isSingleMode = false;
            //默认显示密度
            public float density = 1.0f;
            //密度LOD
            public bool useDensityLod = false;
            public float[] protoTypeDensityLods = null;//密度LOD，削减距离
            public float[] protoTypeDensityCutdown = null;//密度LOD，削减比率
            //距离LOD
            public float[] MeshLods = null;//距离LOD，距离等级

            public bool isCustom = false;

            public bool isReady = true;

            public bool UseLightProb = false;
            //模式
            public Common.ProtoMode mode = Common.ProtoMode.BatchMode;
        }

        public class GroupSortData
        {
            public int index = -1;
            public float dist = 0;
            public bool isVisible = true;
        }

        public class CompareGroupByDist : IComparer<GroupSortData>
        {
            public int Compare(GroupSortData x, GroupSortData y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                if (x.dist < y.dist) return -1;
                if (x.dist > y.dist) return 1;
                return 0;
            }
        }

        private SimpleInstancingGroup[] instancingGroupAry = null;
        private List<GroupSortData> instancingGroupSort = null;
        private CompareGroupByDist compareGroupByDist = new CompareGroupByDist();
        [NonSerialized]
        public List<PrototypeInfo> prototypeList = null;

        #endregion

        #region 系统数据定义
        private bool bInited = false;

        //加载数据的版本号
        private int dataVerNum = 0;
        public int DataVerNum
        {
            get { return dataVerNum; }
            set { dataVerNum = value; }
        }

        [HideInInspector]
        [SerializeField]
        bool useInstancing = true;
        public bool UseInstancing
        {
            get { return useInstancing; }
            set { useInstancing = value; }
        }
        
        [HideInInspector]
        [SerializeField]
        bool useDensityLod = true;
        public bool UseDensityLod
        {
            get { return useDensityLod || TestUseDensityLod; }
            set { useDensityLod = value; }
        }

        [NonSerialized]
        bool isReady = false;
        public bool IsReady
        {
            get { return isReady; }
            set { isReady = value; }
        }

        [SerializeField]
        public bool stopInteract = false;
        public bool EnableInteract
        {
            get { return !stopInteract; }
        }
        [HideInInspector]
        public float[] MeshDistanceLod = null;

        public bool GradualDensity = true;
        public float[] DensityLodsDistance = new float[3] { 50f, 100, 150f };
        public float[] DensityLodsCutdown = new float[3] { 1.0f, 0.5f, 0.05f };

        public SimpleGrassLightmapDic lightmapDic = new SimpleGrassLightmapDic();

        [Header("Wind For Grass")]
        public bool enableWind = false;
        [ColorUsage(true, true)]
        public Color windColor = Color.yellow;
        public float windSpeed = 0;
        public float waveSize = 0;
        public float windAmount = 0;
        public float maxDistance = 80;

        [Header("===Debug===")]
        [SerializeField]
        public bool TestUseDensityLod = true;

        [SerializeField]
        public float TestDensity = -1f;

        public bool debugDrawGizmos = true;

        public bool enableOutLog = false;

        [HideInInspector]
        public bool isWorldMode = false;

        [HideInInspector]
        public Pool dataPool = null;

        private bool isEnabled = false;
        private bool isInLoadDataTask = false;        

        private float lodCalculateFrequency = 0.15f;
        private float lodFrequencyCount = 0.0f;

        private float dirtyCalculateFrequency = 0.15f;
        private float dirtyFrequencyCount = 0.0f;
        
        #endregion

        #region 线程定义
        /*线程，需要设置为后台线程*/
        [HideInInspector]
        public bool StopUpdatLodThread = false;
        [HideInInspector]
        public bool StopUpdatLodThread_End = false;
        bool beginUpdatingThread = false;
        private Thread updateLodDataThread = null;
        [HideInInspector]
        public bool LoadInstGroupDataThreadOver = false;
        private Thread loadInstGroupDataThread = null;
        #endregion

        #region 线程阻塞流程
        private EventWaitHandle suspendHandle = null;
        //private EventWaitHandle suspendHandle = new EventWaitHandle(true, EventResetMode.ManualReset);

        private Common.CUSTOMTHREAD_STATUS customThreadState = Common.CUSTOMTHREAD_STATUS.csNone;
        #endregion

        #region 裁剪相关定义
        [HideInInspector]
        public Dictionary<int, bool> protoTypeDirty = new Dictionary<int, bool>();
        [HideInInspector]
        public int protoTypeNum = 0;
        private bool[] protoTypeDirtyBits = null;

        private Byte[] groupVisibleDirty_Cull = null;
        private bool groupVisibleDirty = true;

        private HashSet<int> collectGroupVisibleDirty = new HashSet<int>();
        private int[] collectGroupVisibleDirtyIndexes = null;
        private int collectGroupVisibleDirtyLen = 0;

        private bool[] protoTypeDirty_Culling = null;
        private bool hasProtoTypeDirty_Culling = true;

        private bool IsDirty
        {
            get { return protoTypeDirty.Count > 0; }
        }

        private int visibledGroupCount = 0;
        private int VisibledGroupCount
        {
            get { return Math.Max(0, visibledGroupCount); }
        }

        BoundingSphere[] boundingSphere;
        int usedBoundingSphereCount = 0;
        public CullingGroup cullingGroup = null;
        #endregion              

        #region Unity Functions
        private void OnEnable()
        {
            // Init();                    
        }

        void Start()
        {
            Common.ClearOutputDebugLog();
            Common.ENABLE_DEBUGOUTLOG = this.enableOutLog;
#if ENABLE_PROFILER
            sampler = CustomSampler.Create("MyCustomSampler");
#endif
            //  
            try
            {
                useInstancing = SystemInfo.supportsInstancing;
            }
            catch (Exception e)
            {
                useInstancing = false;
            }

            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2)
            {
                useInstancing = false;
            }

            TestDensity = -1f;
            TestUseDensityLod = true;
            
            //unity打开，不使用密度LOD功能。
            useDensityLod = (Application.installMode != ApplicationInstallMode.Editor);

            if (Application.isPlaying)
            {
                GameObject root = GameObject.Find("SimpleGrass");
                if (root != null)
                {
                    root.SetActive(false);
                }

                root = GameObject.Find("SimpleGrass_Scan");
                if (root != null)
                {
                    root.SetActive(false);
                }
            }
            //不显示，就不需要初始化
            if(!Common.ValidGrassRender())
            {
                return;
            }
            Init();
        }

        void Update()
        {            
            Common.ENABLE_DRAWGIZMOS = this.debugDrawGizmos;
            Common.ENABLE_DEBUGOUTLOG = this.enableOutLog;            
            if (isWorldMode)
            {
                return;
            }

            //释放相关内存
            if (!Common.ValidGrassRender())
            {
                if (bInited)//数据已加载完毕
                {
                    ClearData();
                }
                return;
            }
            //Debug.Log("######## Update()");
            if (!bInited)
            {
                Init();
                return;
            }

            if (!isReady)
            {
                return;
            }

            ////test
            //if(Common.IsTest)
            //{
            //    if (cullingGroup != null && cullingGroup.targetCamera != Camera.main)
            //    {
            //        Debug.Log("######## cullingGroup.targtCamera Changed");
            //    }
            //}

            UpdateCullingDirtyData();

            ////test
            UnityEngine.Profiling.Profiler.BeginSample("XXXXX-UpdateLod");
            UpdateLod();
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("XXXXX-UpdateRenderData");
            if (Common.UseSort)
            {
                UpdateRenderData_TestSort();
            }
            else
            {
                UpdateRenderData();
            }

            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("XXXXX-Render");
            Render();
            UnityEngine.Profiling.Profiler.EndSample();
        }

        void OnDestroy()
        {
            ClearData();
        }
       
        void OnDrawGizmos()
        {
            if (!Common.ENABLE_DRAWGIZMOS)
                return;

            if (isWorldMode)
            {
                return;
            }
            SimpleInstancingMgr.OnDrawGizmos_GroupSpere(this, instancingGroupAry);
        }

        public void Init(bool worldDataPool = true)
        {
            if (isWorldMode)
            {
                if (!bInited)
                {
                    prototypeList = new List<PrototypeInfo>();
                    dataPool = null;
                    if (worldDataPool)
                    {
                        dataPool = new Pool("instancing", 5, instancingPackageMaxSize);
                    }
                    bInited = true;
                }
                return;
            }
           

            if (bInited || isInLoadDataTask)
            {
                return;
            }
            
            if (!prepareToLoadData())
            {
                return;
            }

            visibledGroupCount = 0;
            bInited = false;
            StopUpdatLodThread = false;
            StopUpdatLodThread_End = false;
            beginUpdatingThread = false;

            isEnabled = false;
            isReady = false;
            isInLoadDataTask = false;

            if (Camera.main == null)
            {
                return;
            }

            prototypeList = new List<PrototypeInfo>();
            dataPool = new Pool("instancing", 5, instancingPackageMaxSize);

            mainCamera = Camera.main;

            InitializeCullingGroup();
         
            //qualityLevel = QualitySettings.GetQualityLevel();
            //Common.LodBias = 1.0f / QualitySettings.lodBias;
            //Common.LodBias_Quality = QualitySettings.lodBias;
            //Common.MaximumLODLevel = QualitySettings.maximumLODLevel;
            //检测画质是否变化         
            Common.ResetQualityLevelData();

            Common.CameraHalfAngle = Mathf.Tan(Mathf.Deg2Rad * mainCamera.fieldOfView * 0.5F);

            //
            LoadDataByTask();
            //bInited = true;
        }

        public void ClearData()
        {
            if (loadInstGroupDataThread != null)
            {
                loadInstGroupDataThread.Abort();
                loadInstGroupDataThread = null;
                LoadInstGroupDataThreadOver = true;
            }

            if (updateLodDataThread != null)
            {
                StopUpdatLodThread = true;
                updateLodDataThread.Abort();
                updateLodDataThread = null;
            }
            // 清理工作
            if (cullingGroup != null)
            {
                cullingGroup.enabled = false;
                cullingGroup.onStateChanged = null;
                cullingGroup.Dispose();
                cullingGroup = null;
            }

            if (prototypeList != null)
            {
                prototypeList.Clear();
                prototypeList = null;
            }

            if (vertexCachePool != null)
                vertexCachePool.Clear();

            if (dataPool != null)
                dataPool.Clear();

            if (instancingGroupAry != null)
            {
                instancingGroupAry = null;
            }

            if (instancingGroupSort != null)
            {
                instancingGroupSort.Clear();
                instancingGroupSort = null;
            }

            if (groupVisibleDirty_Cull != null)
            {
                groupVisibleDirty_Cull = null;
            }

            if (collectGroupVisibleDirty != null)
            {
                collectGroupVisibleDirty.Clear();
            }

            ClearProtoTypeDirty_Culling();
            collectGroupVisibleDirtyLen = 0;
            bInited = false;
            isInLoadDataTask = false;
            visibledGroupCount = 0;
            isReady = false;
        }
        #endregion

        #region  裁剪相关
        private void InitializeCullingGroup()
        {
            isEnabled = SimpleInstancingMgr.InitializeCullingGroup(mainCamera, ref cullingGroup,
           ref boundingSphere, ref usedBoundingSphereCount, CullingStateChanged);
        }

        public bool AddCullingBoundingSphere(SimpleInstancingGroup group, ref bool isVisible)
        {
            if (usedBoundingSphereCount > boundingSphere.Length - 1)
            {
                return false;
            }
            boundingSphere[usedBoundingSphereCount++] = group.BoundingSpere;
            cullingGroup.SetBoundingSphereCount(usedBoundingSphereCount);
            isVisible = cullingGroup.IsVisible(usedBoundingSphereCount - 1);
            group.IsVisible = isVisible;
            return true;
        }

        private void CullingStateChanged(CullingGroupEvent evt)
        {
            int groupIndex = -1;
            int evtIndex = evt.index;
            Debug.Assert(evtIndex < usedBoundingSphereCount);
            Debug.Assert(evtIndex < instancingGroupAry.Length);

            bool isVisible = evt.isVisible;
            bool wasVisible = groupVisibleDirty_Cull[evtIndex] == Common.GROUP_VISIBLE;

            if (isVisible)//evt.hasBecomeVisible
            {
                groupVisibleDirty_Cull[evtIndex] = Common.GROUP_VISIBLE;
                if (!wasVisible)
                {
                    groupIndex = evtIndex;

                    ++visibledGroupCount;
                    groupVisibleDirty = true;
                    if (collectGroupVisibleDirty.Add(evtIndex) && collectGroupVisibleDirtyLen < collectGroupVisibleDirtyIndexes.Length)
                    {
                        collectGroupVisibleDirtyIndexes[collectGroupVisibleDirtyLen] = evtIndex;
                        ++collectGroupVisibleDirtyLen;
                    }
                }
            }
            else// if (wasVisible && !isVisible)//evt.hasBecomeInvisible
            {
                groupVisibleDirty_Cull[evtIndex] = Common.GROUP_INVISIBLE;
                if (wasVisible)
                {
                    groupIndex = evtIndex;
                    --visibledGroupCount;

                    groupVisibleDirty = true;
                    if (collectGroupVisibleDirty.Add(evtIndex) && collectGroupVisibleDirtyLen < collectGroupVisibleDirtyIndexes.Length)
                    {
                        collectGroupVisibleDirtyIndexes[collectGroupVisibleDirtyLen] = evtIndex;
                        ++collectGroupVisibleDirtyLen;
                    }
                }
            }

            //if (evt.hasBecomeVisible)
            //{
            //    //instancingGroupAry[evt.index].IsVisible = true;

            //    groupVisibleDirty_Culling[evt.index] = true;//dlc test
            //    groupIndex = evt.index;

            //    ++visibledGroupCount; 
            //}
            //else if (evt.hasBecomeInvisible)
            //{
            //    //instancingGroupAry[evt.index].IsVisible = false;
            //    groupVisibleDirty_Culling[evt.index] = false;//dlc test
            //    groupIndex = evt.index;
            //    --visibledGroupCount;
            //}

            //instancingGroupAry[evt.index].PrototypeIndex;
            //if (evt.currentDistance > cullingMaxDistance)
            //{
            //    instancingGroupAry[evt.index].IsVisible = false;

            //    groupIndex = evt.index;
            //}            
            //更新vertexCache为dirty
            if (groupIndex != -1)
            {
                // AddDirty(instancingGroupAry[evt.index].PrototypeIndex);
                int protoTypeIndex = instancingGroupAry[evt.index].PrototypeIndex;
                //if (!protoTypeDirty_Culling.ContainsKey(protoTypeIndex))
                //{
                //    protoTypeDirty_Culling.Add(protoTypeIndex, true);
                //}               
                if (protoTypeIndex < this.protoTypeNum)
                {
                    protoTypeDirty_Culling[protoTypeIndex] = true;
                    hasProtoTypeDirty_Culling = true;
                }
            }

        }
        #endregion

        #region 加载数据相关

        private bool prepareToLoadData()
        {
            // 判断，是否已加载
            if (isReady )
            {
                return false;
            }

            //判断，是否已在加载协程任务中
            if (isInLoadDataTask)
            {
                return false;
            }
            //判断，保存数据的有效性
            SimpleSaveData savedProfile = SimpleSys.SaveDataProfile;
            if (savedProfile == null)
                return false;

            int prototypeCount = savedProfile.ProtoTypes.Count;
            if (prototypeCount == 0)
            {
                return false;
            }
            return true;
        }

        private void LoadDataByTask()
        {
            if(!isEnabled || !prepareToLoadData())
            {
                return;
            }
            ////判断，是否已加载或不可用
            //if (isReady || !isEnabled)
            //{
            //    return;
            //}

            ////判断，是否已在加载协程任务中
            //if (isInLoadDataTask)
            //{
            //    return;
            //}

            ////判断，保存数据的有效性
            //SimpleSaveData savedProfile = SimpleSys.SaveDataProfile;
            //if (savedProfile == null)
            //    return;

            //int prototypeCount = savedProfile.ProtoTypes.Count;
            //if (prototypeCount == 0)
            //{
            //    return;
            //}

            //创建Coroutine加载数据
            isInLoadDataTask = true;
            StartCoroutine(Init01_StartLoad());
        }

        //prototype,预制体资源路径  
        IEnumerator Init01_StartLoad()
        {
            ///01
            Common.OutputDebugLog("## 01_StartLoad-begin");
            //prototype,预制体资源路径  
            LoadProtoTypeData();
            yield return null;

            StartCoroutine(Init02_LoadInstGroupData());
        }

        //加载实例组数据
        IEnumerator Init02_LoadInstGroupData()
        {
            ////02
            Common.OutputDebugLog("## 02_LoadInstGroupData-begin");
            LoadInstGroupDataThreadOver = false;
            loadInstGroupDataThread = new Thread(this.LoadInstGroupData_Thread);
            loadInstGroupDataThread.Name = "LOADInstGroupData";
            loadInstGroupDataThread.IsBackground = true;
            loadInstGroupDataThread.Priority = System.Threading.ThreadPriority.Normal;
            loadInstGroupDataThread.Start();
            yield return new WaitUntil(() => LoadInstGroupDataThreadOver);

            Common.OutputDebugLog("## 02_LoadInstGroupData-over");
            //释放线程
            if (loadInstGroupDataThread != null)
            {
                loadInstGroupDataThread.Abort();
                loadInstGroupDataThread = null;
            }

            /////03
            StartCoroutine(Init03_LoadInstGroupSpheres());
        }


        /// <summary>
        ///  加载组的裁剪球
        /// </summary>
        /// <returns></returns>
        IEnumerator Init03_LoadInstGroupSpheres()
        {
            Common.OutputDebugLog("## 03_LoadInstGroupSpheres-begin");
            LoadInstGroupSpheres();
            yield return null;

            StartCoroutine(Init04_LoadProtoTypeMeshData());
        }

        //prototype,加载MESH、材质相关数据
        IEnumerator Init04_LoadProtoTypeMeshData()
        {
            Common.OutputDebugLog("## 04_LoadProtoTypeMeshData-begin");
            //prototype,加载MESH、材质相关数据
            LoadProtoTypeMeshData();
            yield return StartCoroutine(Init05_LoadOver());
        }

        /// <summary>
        /// 启动后台线程： 定时更新Group的：LOD相关数据、显示密度、Group排序
        /// </summary>
        /// <returns></returns>
        IEnumerator Init05_LoadOver()
        {
            Common.OutputDebugLog("## 05_LoadOver");
            isReady = true;

            updateLodDataThread = new Thread(this.UpdateLodDataByGroupInThread);
            updateLodDataThread.Name = "UpdateLOD";
            updateLodDataThread.IsBackground = true;
            updateLodDataThread.Priority = System.Threading.ThreadPriority.Normal;
            updateLodDataThread.Start();

            bInited = true;

            yield return null;
        }


        /// <summary>
        /// 整理每个数据块中实例的数据(块信息、实例矩阵、实例光照信息）
        /// </summary>
        private void LoadInstGroupData_Thread()
        {
            try
            {
                int beginPrototypeIndex = 0;
                SimpleSaveData savedProfile = SimpleSys.SaveDataProfile;
                if (savedProfile.IsSavedBuffer())
                {                    
                    if (SimpleSaveDataHelper.LoadInstGroupDataByBuffer_Thread(this, savedProfile, ref instancingGroupAry, ref instancingGroupSort,
                         ref Common.TestGroupNum, ref Common.TestInstanceNum, ref Common.TestMeshNum, ref Common.TestInstNum_ByBuff, beginPrototypeIndex))
                    {
                        LoadInstGroupDataThreadOver = true;
                    }
                    return;
                }

                int num = savedProfile.GetAllChuckNum();//所有块的数量

                instancingGroupAry = new SimpleInstancingGroup[num];
                instancingGroupSort = new List<GroupSortData>();

                Common.TestInstNum_ByBuff.Clear();
                if( BuildGroupDatas(this, this.prototypeList, savedProfile, ref instancingGroupAry, ref instancingGroupSort,
                   ref Common.TestGroupNum, ref Common.TestInstanceNum, ref Common.TestMeshNum, beginPrototypeIndex))
                {
                    LoadInstGroupDataThreadOver = true;
                }                                
            }
            catch (Exception e)
            {

            }
        }

        //private void LoadInstGroupData_Thread_old_20230301()
        //{
        //    try
        //    {
        //        SimpleSaveData savedProfile = SimpleSys.SaveDataProfile;
        //        if((savedProfile.ChunkData == null || savedProfile.ChunkData.Count == 0) &&
        //           (savedProfile.ChunkDataBuff != null && savedProfile.ChunkDataBuff.Length > 0))
        //        {
        //            int beginProtypeIndex = 0;
        //            if (SimpleSaveDataHelper.LoadInstGroupDataByBuff_Thread(this, savedProfile, ref instancingGroupAry, ref instancingGroupSort,
        //                 ref Common.TestGroupNum, ref Common.TestInstanceNum, ref Common.TestMeshNum, ref Common.TestInstNum_ByBuff, beginProtypeIndex))
        //            {
        //                LoadInstGroupDataThreadOver = true;
        //            }
        //            return;
        //        }

        //        int num = savedProfile.GetAllChuckNum();//所有块的数量

        //        instancingGroupAry = new SimpleInstancingGroup[num];
        //        instancingGroupSort = new List<GroupSortData>();
        //        ///初始化，裁剪

        //        int prototypeIndex = -1;
        //        int tmpIndex = -1;

        //        List<InstanceChunkData> instanceChunkDataList = savedProfile.ChunkData;
        //        //
        //        Common.TestInstNum_ByBuff.Clear();
        //        Common.TestGroupNum = instanceChunkDataList.Count;
        //        Common.TestInstanceNum = 0;
        //        Common.TestMeshNum = 0;
        //        bool isSingleMode = false;
        //        bool useLightProbe = false;
        //        Common.ProtoMode protoMode = Common.ProtoMode.BatchMode;
        //        for (int i = 0; i < instanceChunkDataList.Count; ++i)
        //        {
        //            prototypeIndex = instanceChunkDataList[i].PrototypeIndex;
        //            if (tmpIndex != prototypeIndex)
        //            {
        //                tmpIndex = prototypeIndex;

        //                PrototypeInfo protoInfo = prototypeList[prototypeIndex];
        //                isSingleMode = protoInfo.isSingleMode;
        //                protoMode = protoInfo.mode;
        //                useLightProbe = protoInfo.UseLightProb;
        //            }

        //            SimpleInstancingGroup group = new SimpleInstancingGroup(this);
        //            group.PrototypeIndex = prototypeIndex;
        //            Vector3 center = new Vector3(instanceChunkDataList[i].CullingCollider[0], instanceChunkDataList[i].CullingCollider[1], instanceChunkDataList[i].CullingCollider[2]);

        //            group.worldPosition = center;
        //            group.MaxScale = instanceChunkDataList[i].maxScale;

        //            //创建每块的裁剪使用的球
        //            UnityEngine.BoundingSphere bounding = new UnityEngine.BoundingSphere();
        //            bounding.position = center;
        //            bounding.radius = instanceChunkDataList[i].CullingCollider[3];
        //            group.BoundingSpere = bounding;

        //            instancingGroupAry[i] = group;
        //            if (Common.UseSort)
        //            {
        //                GroupSortData sortData = new GroupSortData();
        //                sortData.index = i;
        //                sortData.dist = 0;
        //                sortData.isVisible = true;
        //                instancingGroupSort.Add(sortData);
        //            }

        //            //增加group裁剪球体
        //            //test AddCullingBoundingSphere(group);

        //            //加载块内数据   
        //            //新增BatchModeLOD
        //            if (protoMode == Common.ProtoMode.BatchMode_LOD)
        //            {
        //                group.IsSingleMode = false;
        //                group.protoMode = protoMode;
        //            }

        //            //LODGroup模式, 一个实例对应一个SimpleInstance，解析矩阵及光照信息
        //            if (isSingleMode)
        //            {
        //                group.IsSingleMode = true;
        //                group.protoMode = protoMode;
        //                #region LODGroup模式
        //                List<InstData> instList = instanceChunkDataList[i].InstanceList;
        //                BuildOneGroupData_SingleMode(ref group, ref instList,ref Common.TestInstanceNum,ref Common.TestMeshNum, useLightProbe);
        //                #endregion LODGroup模式
        //            }
        //            //////////////////批量模式
        //            else
        //            {
        //                group.protoMode = protoMode;
        //                #region 批量模式
        //                List<InstData> instList = instanceChunkDataList[i].InstanceList;
        //                BuildOnGroupData_BatchMode(ref group, ref instList, ref Common.TestInstanceNum, ref Common.TestMeshNum, useLightProbe);
        //                #endregion 批量模式
        //            }
        //        }

        //        LoadInstGroupDataThreadOver = true;
        //    }
        //    catch (Exception e)
        //    {

        //    }
        //}


        private void LoadInstGroupSpheres()
        {
            SimpleSaveData savedProfile = SimpleSys.SaveDataProfile;

            int num = savedProfile.GetAllChuckNum();//所有块的数量

            this.groupVisibleDirty_Cull = new Byte[num];
            Array.Clear(this.groupVisibleDirty_Cull, 0, num);

            this.collectGroupVisibleDirtyIndexes = new int[num];
            this.collectGroupVisibleDirtyLen = 0;

            ///初始化，裁剪
            boundingSphere = new BoundingSphere[num];
            cullingGroup.SetBoundingSpheres(boundingSphere);

            for (int i = 0; i < instancingGroupAry.Length; ++i)
            {
                bool isVisible = false;
                if (AddCullingBoundingSphere(instancingGroupAry[i], ref isVisible))
                {
                    if (isVisible)
                        groupVisibleDirty_Cull[i] = Common.GROUP_VISIBLE;
                    else
                        groupVisibleDirty_Cull[i] = Common.GROUP_INVISIBLE;
                }
            }

            groupVisibleDirty = true;
        }


        private void LoadProtoTypeData()
        {
            SimpleSaveData savedProfile = SimpleSys.SaveDataProfile;
            int verNum = savedProfile.Ver;
            string sceneTag = "";
            this.dataVerNum = verNum;

            int protoGrassCount = savedProfile.ProtoTypes.Count;
            //UnityEngine.Profiling.Profiler.BeginSample("Calculate lod");

            List<Material[]> listMeshMaterial = new List<Material[]>();
            List<Mesh> listMesh = new List<Mesh>();
            List<float> listLodHeight = new List<float>();
            Dictionary<long, int> dictRender = new Dictionary<long, int>();
            //加载原型类型数据
            Delegate_GetCodeByMD5 GetCodeByMD5Fun = savedProfile.GetCodeByMD5;
            Delegate_GetLightMapNum GetLightMapNumFun = savedProfile.GetLightMapNum;

            float[] densityLodsDistance = this.DensityLodsDistance;//Common.DensityLods
            float[] densityLodsCutdown = this.DensityLodsCutdown;  //Common.DensityCutdown
            for (int prototypeIdx = 0; prototypeIdx < protoGrassCount; ++prototypeIdx)
            {
                ProtoTypeData protoTypeData = savedProfile.ProtoTypes[prototypeIdx];
                PrototypeInfo protoInfo = SimpleInstancingMgr.BuildPrototypeInfo
                    (verNum, sceneTag,prototypeIdx, protoTypeData, this.MeshDistanceLod, densityLodsDistance,
                    densityLodsCutdown, GetCodeByMD5Fun, GetLightMapNumFun);
                if(protoInfo == null)
                {
                    Debug.LogError("##protoInfo=NULL, [LoadProtoTypeData->BuildPrototypeInfo] name:" + protoTypeData.ProtoKey);
                    if (Common.EnableDebugOutLog())
                    {
                        Common.OutputDebugLog("## [Error]# protoInfo=NULL, [LoadProtoTypeData->BuildPrototypeInfo] name:" + protoTypeData.ProtoKey,true);
                    }
                }
                prototypeList.Add(protoInfo);
            }
            //UnityEngine.Profiling.Profiler.EndSample();

            InitProtoTypeDirty(prototypeList.Count);            
        }

        /// <summary>
        /// 初始原型类型的：VertexCache及相关MaterialBlock
        /// VertexCache: code = verNum, prototypeName,  prefabName, meshName, lodLevel
        /// MaterialBlock: code = sharedMaterials名称hashcode
        /// </summary>

        private void LoadProtoTypeMeshData()
        {
            SimpleSaveData savedProfile = SimpleSys.SaveDataProfile;
            int verNum = savedProfile.Ver;
            string sceneTag = "";
            for (int prototypeIdx = 0; prototypeIdx < prototypeList.Count; ++prototypeIdx)
            {
                SimpleInstancingMgr.BuildProtoTypeMeshData(verNum, sceneTag, prototypeIdx, prototypeIdx, this,
                    ref this.prototypeList, savedProfile.ProtoTypes, this.lightmapDic,
                    savedProfile.GetCodeByMD5, savedProfile.GetLightMapNum, savedProfile.GetLightMapIndexes);
            }
        }       

        public InstancingPackage CreatePackage(int maxInstNum, bool useLightMap,bool useLightProbe)
        {
            InstancingPackage package = new InstancingPackage();

            int size = instancingPackageMaxSize;

            CreateInstanceData(package, size, useLightMap, useLightProbe);
            package.instancingCount = 0;
            return package;

        }


        private void CreateInstanceData(InstancingPackage package, int maxInstNum, bool useLightMap,bool useLightProbe)
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

        public int BuildPackage(InstancingPackage package, int maxInstNum, bool useLightMap,bool useLightProbe)
        {
            int size = instancingPackageMaxSize;
            package.worldMatrix = dataPool.GetMatx();//dataPool.PullMatx();
            package.color = dataPool.GetVec4();
            if (useLightMap)
            {
                package.lightmapScaleOffset = dataPool.GetVec4();//dataPool.PullVec4();
            }

            if(useLightProbe)
            {
                package.lightProbOcclusion = dataPool.GetFloat();
            }
            return size;
        }


        #endregion

        #region 更新update相关

        private void UpdateCullingDirtyData()
        {
            if (!beginUpdatingThread && (groupVisibleDirty || HasProtoTypeDirty_Culling()))
            {
                if (groupVisibleDirty)
                {
                    for(int i = 0; i < collectGroupVisibleDirtyLen; ++i)
                    {
                        int idx = collectGroupVisibleDirtyIndexes[i];
                        instancingGroupAry[idx].IsVisible = groupVisibleDirty_Cull[idx] == Common.GROUP_VISIBLE;
                    }
                    collectGroupVisibleDirty.Clear();
                    groupVisibleDirty = false;

                    collectGroupVisibleDirtyLen = 0;
                }

                if (hasProtoTypeDirty_Culling)
                {
                    for (int i = 0; i < this.protoTypeNum; ++i)
                    {
                        if (protoTypeDirty_Culling[i])
                        {
                            AddDirty(i);
                        }
                    }
                }
                ClearProtoTypeDirty_Culling();
            }
        }

        private void UpdateLod()
        {
            #region 线程阻塞流程
            //唤醒或挂起
            //Check2Wake_UpdateLodThread();
            //Check2Block_UpdateLodThread();
            //if (customThreadState == Common.CUSTOMTHREAD_STATUS.csBlocked)
            //{
            //    return;
            //}
            #endregion

            if (!beginUpdatingThread)
            {
                lodFrequencyCount += Time.deltaTime;
            }

            if (lodFrequencyCount > lodCalculateFrequency)
            {
                Common.INTERNAL_ReusableCameraPlanes = Common.CalculateFrustumPlanes(mainCamera, Common.INTERNAL_ReusableCameraPlanes, Time.time);
                //检测画质是否变化         
                Common.ResetQualityLevelData();
                //更新英雄位置和相机位置等信息
                Common.ResetPositionData(mainCamera, this.EnableInteract);
                //遍历组对象
                {
                    #region 线程阻塞流程
                    //Check2WillBlock_UpdateLodThread();
                    #endregion
                    //更新LOD
                    beginUpdatingThread = true;

                    lodFrequencyCount = 0.0f;
                }
            }
        }

        private void UpdateLodDataByGroupInThread()
        {
#if ENABLE_PROFILER
                Profiler.BeginThreadProfiling("My threads", "UpdateLOD");
#endif
            try
            {
                while (!StopUpdatLodThread
                       //  && (Thread.CurrentThread.ThreadState & ThreadState.AbortRequested )!= ThreadState.AbortRequested 
                       //  && ((Thread.CurrentThread.ThreadState & ThreadState.Aborted) != ThreadState.Aborted)
                       //  && ((Thread.CurrentThread.ThreadState & ThreadState.Stopped) != ThreadState.Stopped)
                       //  && ((Thread.CurrentThread.ThreadState & ThreadState.StopRequested) != ThreadState.StopRequested)
                       )
                {
                    #region 线程阻塞流程
                    //try
                    //{
                    //    suspendHandle.WaitOne();
                    //}
                    //catch(Exception e)
                    //{

                    //}
                    #endregion


                    if (beginUpdatingThread)
                    {
                        //test
#if ENABLE_PROFILER
                            sampler.Begin();
#endif
                        //
                        //以组为单元进行遍历
                        for (int i = 0; i < instancingGroupAry.Length; ++i)
                        {
                            if (Common.IsTest && (i >= Common.TestLoadNum))
                            {
                                return;
                            }

                            SimpleInstancingGroup instancingGroup = instancingGroupAry[i];
                            if (instancingGroup == null)
                            {
                                continue;
                            }

                            Vector3 dis_verctor = (instancingGroup.worldPosition - Common.CameraPosition).normalized;
                            instancingGroup.IsVisible = Vector3.Dot(dis_verctor, Common.CameraForward) > (isReceiveShadow ? 0.4f : 0.5f);

                            //初始化组LOD相关
                            if (instancingGroup.LodNum == -1)
                            {
                                Debug.Assert(instancingGroup.PrototypeIndex < prototypeList.Count);
                                PrototypeInfo protoInfo = prototypeList[instancingGroup.PrototypeIndex];
                                //当前组，使用的LOD
                                bool enableLod = false;
                                int lodNum = 0;
                                if (protoInfo.lodInfoList != null && protoInfo.lodInfoList.Length > 0)
                                {
                                    enableLod = protoInfo.lodInfoList[0].inLodGroup;
                                    lodNum = protoInfo.lodInfoList.Length;
                                }
                                instancingGroup.EnableLod = enableLod;
                                instancingGroup.LodNum = lodNum;
                            }
                            int retDirtyProtoIndex = -1;
                            instancingGroup.UpdateLod(Common.CameraPosition, Common.HeroPosition, ref retDirtyProtoIndex);

                            if (Common.UseSort)
                            {
                                bool isVisible = instancingGroup.IsVisible;
                                instancingGroupSort[i].isVisible = isVisible;
                                instancingGroupSort[i].index = i;
                                instancingGroupSort[i].dist = instancingGroup.tmpDistanceFromCamera;
                                if (!isVisible)
                                {
                                    instancingGroupSort[i].dist = Common.InVisibleDistConst;
                                }
                            }
                        }

                        if (Common.UseSort)
                        {
                            SortInstancingGroup();
                        }

                        beginUpdatingThread = false;

#if ENABLE_PROFILER
                        sampler.End();
#endif
                        #region 线程阻塞流程
                        // if (!Check2ReadyBlock_UpdateLodThread())
                        //{
                        //    //线程在下次计算将至时，休眠
                        //    Thread.Sleep(ThreadSleep_LodCalculate);//130ms   
                        //}
                        #endregion

                        //线程在下次计算将至时，休眠
                        Thread.Sleep(ThreadSleep_LodCalculate);//130ms   
                    }
                    else
                    {
                        #region 线程阻塞流程
                        // if (!Check2ReadyBlock_UpdateLodThread())
                        //{
                        //    Thread.Sleep(20);//1ms                           
                        //}
                        #endregion

                        Thread.Sleep(20);//1ms    
                    }
                }

                StopUpdatLodThread_End = true;
            }
            catch (Exception e)
            {
                // Debug.Log(e.ToString());
            }
#if ENABLE_PROFILER
            Profiler.EndThreadProfiling();
#endif
        }
        private void SortInstancingGroup()
        {
            instancingGroupSort.Sort(compareGroupByDist);
        }

        private void UpdateRenderData()
        {
            if (!IsDirty)
            {
                return;
            }
            // if (!beginUpdatingThread)
            {
                dirtyFrequencyCount += Time.deltaTime;
            }
            if (!beginUpdatingThread && dirtyFrequencyCount > dirtyCalculateFrequency)
            {
                //重置渲染数据
                //ResetRender();
                SimpleInstancingMgr.ResetRenderCache(dataPool, ref protoTypeDirty, ref prototypeList);

                //以组为单元进行遍历
                Common.TestVisibleGroupNum = 0;
                Common.TestVisibleInstanceNum = 0;
                Common.TestVisibleMeshNum = 0;
                Common.TestRealVisibleInstNum = 0;
                for (int i = 0; i < instancingGroupAry.Length; ++i)
                {
                    if (Common.IsTest && (i >= Common.TestLoadNum))
                    {
                        continue;
                    }

                    SimpleInstancingGroup instancingGroup = instancingGroupAry[i];
                    if (instancingGroup == null || !instancingGroup.IsVisible)
                    {
                        continue;
                    }

                    Common.TestVisibleGroupNum += 1;

                    int prototypeIndex = instancingGroup.PrototypeIndex;
                    if (!this.IsProtoTypeDirty(prototypeIndex))
                    {
                        Common.TestVisibleInstanceNum += instancingGroup.VisibleInstNum;
                        Common.TestRealVisibleInstNum += instancingGroup.RealVisibleInstNum;
                        continue;
                    }

                    int lodLevel = instancingGroup.LodLevel;

                    Debug.Assert(prototypeIndex < prototypeList.Count);
                    PrototypeInfo protoInfo = prototypeList[prototypeIndex];
                    Debug.Assert(lodLevel < protoInfo.lodInfoList.Length);

                    bool useLightProbe = protoInfo.UseLightProb;
                    if (instancingGroup.IsSingleMode)
                    {
                        //SimpleInstancingMgr.UpdateRenderData_SingleMode(prototypeIndex, instancingGroup, this.prototypeList, this);
                    }
                    else
                    {
                        //当前组，使用的LOD
                        if (lodLevel >= 0 && lodLevel < protoInfo.lodInfoList.Length)
                        {
                            LodInfo lod = protoInfo.lodInfoList[lodLevel];
                            SimpleInstancingMgr.UpdateRenderData_BatchMode(lod, instancingGroup, this, useLightProbe);
                        }
                    }

                    Common.TestVisibleInstanceNum += instancingGroup.VisibleInstNum;
                    Common.TestVisibleMeshNum += instancingGroup.VisibleMeshNum;
                    Common.TestRealVisibleInstNum += instancingGroup.RealVisibleInstNum;
                }

                ClearDirty();
                dirtyFrequencyCount = 0.0f;
            }
        }

        /// <summary>
        /// 整理显示数据： 每个GROUP适合显示条件的实例数据，进行归类整理到MaterialBlock中的packgae中。
        /// </summary>
        private void UpdateRenderData_TestSort()
        {
            if (!IsDirty)
            {
                return;
            }
            // if (!beginUpdatingThread)
            {
                dirtyFrequencyCount += Time.deltaTime;
            }
            if (!beginUpdatingThread && dirtyFrequencyCount > dirtyCalculateFrequency)
            {
                //重置渲染数据
                //ResetRender();
                SimpleInstancingMgr.ResetRenderCache(dataPool, ref protoTypeDirty, ref prototypeList);

                //以组为单元进行遍历
                Common.TestVisibleGroupNum = 0;
                Common.TestVisibleInstanceNum = 0;
                Common.TestVisibleMeshNum = 0;
                Common.TestRealVisibleInstNum = 0;
                int index = 0;
                //for (int i = 0; i < instancingGroupAry.Length; ++i)
                for (int i = 0; i < instancingGroupSort.Count; ++i)
                {
                    //不可见的距离设置为无限远,都排序到最后
                    bool isVisible = instancingGroupSort[i].isVisible;
                    if (!isVisible)
                    {
                        break;
                    }
                    //add
                    index = instancingGroupSort[i].index;

                    if (index < 0)
                    {
                        continue;
                    }
                    //

                    if (Common.IsTest && (i >= Common.TestLoadNum))
                    {
                        continue;
                    }

                    SimpleInstancingGroup instancingGroup = instancingGroupAry[index];//SimpleInstancingGroup instancingGroup = instancingGroupAry[i];
                    if (instancingGroup == null)
                    // if (instancingGroup == null || !instancingGroup.IsVisible)
                    {
                        continue;
                    }

                    Common.TestVisibleGroupNum += 1;

                    int prototypeIndex = instancingGroup.PrototypeIndex;
                    if (!this.IsProtoTypeDirty(prototypeIndex))
                    {
                        Common.TestVisibleInstanceNum += instancingGroup.VisibleInstNum;
                        Common.TestVisibleMeshNum += instancingGroup.VisibleMeshNum;
                        Common.TestRealVisibleInstNum += instancingGroup.RealVisibleInstNum;
                        continue;
                    }

                    int lodLevel = instancingGroup.LodLevel;

                    Debug.Assert(prototypeIndex < prototypeList.Count);
                    PrototypeInfo protoInfo = prototypeList[prototypeIndex];
                    Debug.Assert(lodLevel < protoInfo.lodInfoList.Length);

                    bool useLightProbe = protoInfo.UseLightProb;
                    if (instancingGroup.IsSingleMode)
                    {
                        //SimpleInstancingMgr.UpdateRenderData_SingleMode(prototypeIndex, instancingGroup, prototypeList, this);
                    }
                    else
                    {
                        //当前组，使用的LOD
                        if (protoInfo.lodInfoList != null && lodLevel >= 0 && lodLevel < protoInfo.lodInfoList.Length)
                        {
                            LodInfo lod = protoInfo.lodInfoList[lodLevel];
                            SimpleInstancingMgr.UpdateRenderData_BatchMode(lod, instancingGroup, this, useLightProbe);
                        }
                    }
                    Common.TestVisibleInstanceNum += instancingGroup.VisibleInstNum;
                    Common.TestVisibleMeshNum += instancingGroup.VisibleMeshNum;
                    Common.TestRealVisibleInstNum += instancingGroup.RealVisibleInstNum;
                }

                ClearDirty();
                dirtyFrequencyCount = 0.0f;
            }
        }
        
        private void testSortInGroup(ref Matrix4x4[] mat, ref Vector4[] vet)
        {
            float[] distAry = new float[mat.Length];
            Vector3 pos = Vector3.zero;
            for (int i = 0; i != mat.Length; ++i)
            {
                pos.x = mat[i].m03;
                pos.y = mat[i].m13;
                pos.z = mat[i].m23;
                distAry[i] = (Common.CameraPosition - pos).sqrMagnitude;
            }
            QuickSortArray(ref distAry, ref mat, ref vet, 0, mat.Length - 1);
        }

        private void QuickSortArray(ref float[] array, ref Matrix4x4[] mat, ref Vector4[] vet, int start, int end)
        {
            // 
            //若数组中数小于等于0直接返回
            if (start >= end) return;
            //定义一个基准值
            float pivot = array[start];
            Matrix4x4 pivotMat = mat[start];
            Vector4 pivotVet = vet[start];
            //定义2个索引指向数组的而开头和结束
            int left = start;
            int right = end;
            //按照从小到大的排序，直到2数相遇结束排序
            while (left < right)
            {
                //第一轮比较
                //把所有left右边的数都和基准值比较,获得最左边数在排序后位于数组中的位置（索引）
                while (left < right && array[right] >= pivot)
                {
                    right--;
                }
                //将该数放到数组中的该位置
                array[left] = array[right];
                mat[left] = mat[right];
                vet[left] = vet[right];
                //第二轮比较
                //把所有left右边的数都和基准值比较,获得最左边数在排序后位于数组中的位置（索引）
                while (left < right && array[left] <= pivot)
                {
                    left++;
                }
                //将该数放到数组中的该位置
                array[right] = array[left];

                mat[right] = mat[left];
                vet[right] = vet[left];
            }
            //将2轮比较之后的数组的起始值再赋为基准值（已经得到最大值，并在最后一位）
            array[left] = pivot;
            mat[left] = pivotMat;
            vet[left] = pivotVet;
            //递归该方法（每次剔除一个排好的数）
            QuickSortArray(ref array, ref mat, ref vet, start, left - 1);
            QuickSortArray(ref array, ref mat, ref vet, left + 1, end);
        }

        #endregion 更新

        #region 渲染相关       
        public void Render()
        {
            try
            {
                int drawCount = 0;
                //UnityEngine.Profiling.Profiler.BeginSample("XXXXX-Render-Doing");
                if (Common.IsTest)//测试
                {
                    Common.TestDrawCount = 0;
                }

                //SetWindProperty(enableWind);

                SetWindProperty();

                List<InstancingPackage> packageList = null;
                InstancingPackage package = null;
                MaterialBlock block = null;
                //mesh为单位
                var enumerator = vertexCachePool.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    VertexCache vertexCache = enumerator.Current.Value;

                    //以使用的材质为单位
                    var blockIT = vertexCache.instanceMatBlockList.GetEnumerator();
                    while (blockIT.MoveNext())// foreach (var block in vertexCache.instanceMatBlockList)
                    {
                        block = blockIT.Current.Value;
   
                        packageList = block.packageList;
                        int subMeshCount = block.subMeshCount;
                        //以每次最大渲染实例数量限制为单位（package）
                        for (int i = 0; i != packageList.Count; ++i)
                        {
                            package = packageList[i];
                            bool isInInteract = package.isInInteract;                            
                            if (package.instancingCount == 0)
                                continue;
                            //以每个subMesh为单位
                            for (int j = 0; j != subMeshCount; ++j)
                            {
                                drawCount++;
                                Material useMaterial = block.GetMaterial(j, isInInteract);
                                //SetWindProperty(useMaterial, enableWind);
                               // block.propertyBlock.SetFloat(Common.PID_CUSTOM_INTERACT, isInInteract ? 1.0f : 0.0f);
                                if (useInstancing)
                                {
                                    if (vertexCache.lightmapIndex == Common.INDEXTAG_USELIGHTPROBE)
                                    {
                                        block.propertyBlock.SetFloatArray(Common.PID_LIGHTPROBOCCLUSION, package.lightProbOcclusion.floatPtr);//"_LightProbOcclusion"                                      
                                    }                                  

                                    if (vertexCache.lightmapIndex >= 0)
                                    {
                                        block.propertyBlock.SetVectorArray(Common.PID_LIGHTMAPST_CUSTOM, package.lightmapScaleOffset.vector4Ptr);//"_LightmapST_Custom"
                                    }
                                    block.propertyBlock.SetVectorArray(Common.PID_SURF_COLOR, package.color.vector4Ptr);
                                    //block.propertyBlock.SetVectorArray(Common.PID_LIGHTMAPST_CUSTOM, package.lightmapScaleOffset.vector4Ptr);
                                    if (Common.IsTest)//测试
                                    {
                                        SetRealtimeShadowProperty(useMaterial, isReceiveShadow);
                                        Graphics.DrawMeshInstanced(vertexCache.mesh,
                                            j,
                                            useMaterial,//block.material[j],
                                            package.worldMatrix.matrPtr,
                                            package.instancingCount,
                                           block.propertyBlock,
                                            Common.TestShadowCastingMode,
                                            Common.TestReceiveShadow,
                                            vertexCache.layer
                                            );
                                        Common.TestDrawCount += 1;
                                    }
                                    else
                                    {
                                        int instNum = Math.Min(package.instancingCount, package.worldMatrix.matrPtr.Length);
                                        SetRealtimeShadowProperty(useMaterial, isReceiveShadow);
                                        Graphics.DrawMeshInstanced(vertexCache.mesh,
                                            j,
                                            useMaterial,//block.material[j],
                                            package.worldMatrix.matrPtr,
                                            instNum,
                                            block.propertyBlock,
                                            vertexCache.shadowcastingMode, // ShadowCastingMode.Off, //
                                            vertexCache.receiveShadow,// false,//
                                            vertexCache.layer);
                                    }
                                }
                                else
                                {
                                    int instNum = Math.Min(package.instancingCount, package.worldMatrix.matrPtr.Length);
                                    for (int kk = 0; kk < instNum; ++kk)
                                    {
                                        if (vertexCache.lightmapIndex == Common.INDEXTAG_USELIGHTPROBE)
                                        {
                                         
                                            block.propertyBlock.SetFloat(Common.PID_LIGHTPROBOCCLUSION, package.lightProbOcclusion.floatPtr[kk]);//"_LightProbOcclusion"
                                        }
                                        if (vertexCache.lightmapIndex >= 0)
                                        {
                                            block.propertyBlock.SetVector(Common.PID_UNITY_LIGHTMAPST, package.lightmapScaleOffset.vector4Ptr[kk]);//"unity_LightmapST"
                                        }
                                        Graphics.DrawMesh(vertexCache.mesh,
                                            package.worldMatrix.matrPtr[kk],
                                            useMaterial,//block.material[j],
                                            vertexCache.layer,
                                            null,
                                            j,
                                            block.propertyBlock, false, false);
                                    }
                                }
                            }
                            //  package.instancingCount = 0;
                        }
                        //  block.Value.runtimePackageIndex = 0;
                    }

                    vertexCache.isDirty = false;
                }
                // UnityEngine.Profiling.Profiler.EndSample();

            }
            catch (Exception e)
            {
                //Debug.LogException(e);
            }
        }

        public void SetWindProperty(bool enable)
        {
            if (enable)
            {
                Vector4 windProperty = new Vector4();
                windProperty.x = windSpeed;
                windProperty.y = waveSize;
                windProperty.z = windAmount;
                windProperty.w = maxDistance * maxDistance;
                Shader.SetGlobalVector("_WindParams", windProperty);
                Shader.SetGlobalColor("_WavingColor", windColor);
            }
        }

        public void SetWindProperty()
        {
            SetWindProperty(enableWind);
        }
        public void SetWindProperty(Material material, bool enable)
        {
            if(enable)
            {
                Vector4 windProperty = new Vector4();
                windProperty.x = windSpeed;
                windProperty.y = waveSize;
                windProperty.z = windAmount;
                windProperty.w = maxDistance * maxDistance;
                material.SetVector("_WindParams", windProperty);
                material.SetColor("_WavingColor", windColor);
            }
        }

        public void SetRealtimeShadowProperty(Material material, bool enable)
        {
            if (material == null)
            {
                return;
            }
            if(enable)
            {
                material.SetFloat(enableShadowID, 1);
                material.SetMatrix(shadowMatrixID, shadowProjector.WorldToProjectorMatrix);
                material.SetTexture(shadowMapID, shadowProjector.ShadowRT);
                material.SetFloat(shadowIntensityID, shadowIntensity);
            }
            else
            {
                material.SetFloat(enableShadowID, 0);
            }
        }

        public void ResetInstanceData(InstancingPackage package, bool useLightMap, bool useLightProbe)
        {
            if (package.worldMatrix == null)
                package.worldMatrix = dataPool.GetMatx();//dataPool.PullMatx();
            else if (package.worldMatrix.isUsed == false)
                package.worldMatrix = dataPool.GetMatx();//package.worldMatrix.isUsed = true;

            if (package.color == null)
                package.color = dataPool.GetVec4();
            else if (package.color.isUsed == false)
                package.color = dataPool.GetVec4();//package.worldMatrix.isUsed = true;

            if (useLightMap)
            {
                if (package.lightmapScaleOffset == null)
                    package.lightmapScaleOffset = dataPool.GetVec4();////dataPool.PullVec4();
                else if (package.lightmapScaleOffset.isUsed == false)
                    package.lightmapScaleOffset = dataPool.GetVec4();// package.lightmapScaleOffset.isUsed = true;
            }

            if (useLightProbe)
            {
                if (package.lightProbOcclusion == null)
                    package.lightProbOcclusion = dataPool.GetFloat();
                else if (package.lightProbOcclusion.isUsed == false)
                    package.lightProbOcclusion = dataPool.GetFloat();
            }

        }

        public void InitProtoTypeDirty(int prototypeAmount)
        {
            this.protoTypeNum = prototypeAmount;

            this.protoTypeDirtyBits = new bool[protoTypeNum];

            this.protoTypeDirty_Culling = new bool[protoTypeNum];
        }

        public void AddDirty(int protoTypeIndex)
        {
            if (!protoTypeDirty.ContainsKey(protoTypeIndex))
            {
                protoTypeDirty.Add(protoTypeIndex, true);

                if (protoTypeIndex < this.protoTypeNum)
                {
                    protoTypeDirtyBits[protoTypeIndex] = true;
                }
            }
        }

        public void ClearDirty()
        {
            protoTypeDirty.Clear();

            Array.Clear(protoTypeDirtyBits, 0, this.protoTypeNum);
        }

        public void ClearDirtyBy(int beginPrototypeIndex = -1, int endPrototypeIndex = -1)
        {
            bool handleAll = (beginPrototypeIndex == -1 && endPrototypeIndex == -1);
            if (handleAll)
            {
                protoTypeDirty.Clear();

                Array.Clear(protoTypeDirtyBits, 0, this.protoTypeNum);
            }else
            {
                for(int index = beginPrototypeIndex; index <= endPrototypeIndex; ++index)
                {
                    if (protoTypeDirty.ContainsKey(index))
                    {
                        protoTypeDirty.Remove(index);                        
                    }
                    if (index >=0 && index < this.protoTypeNum)
                    {
                        protoTypeDirtyBits[index] = false;
                    }
                }                
            }
        }

        public bool IsProtoTypeDirty(int protoTypeIndex)
        {
            if (protoTypeIndex < this.protoTypeNum)
            {
                return protoTypeDirtyBits[protoTypeIndex] == true;
            }
            return false;
            // return protoTypeDirty.ContainsKey(protoTypeIndex);
        }
       
        private bool HasProtoTypeDirty_Culling()
        {
            return hasProtoTypeDirty_Culling;
        }

        private void ClearProtoTypeDirty_Culling()
        {
            hasProtoTypeDirty_Culling = false;
            if (protoTypeDirty_Culling != null)
            {
                Array.Clear(protoTypeDirty_Culling, 0, this.protoTypeNum);
            }
        }
        #endregion

        #region 其它方法
        private void Check2Wake_UpdateLodThread()
        {
            if (VisibledGroupCount > 0)
            {
                if (customThreadState == Common.CUSTOMTHREAD_STATUS.csBlocked)
                {
                    try
                    {
                        //唤醒线程
                        suspendHandle.Set();
                        // Debug.Log("=====wake grass update lod---");
                    }
                    catch (Exception e)
                    {

                    }
                }
                customThreadState = Common.CUSTOMTHREAD_STATUS.csNone;
            }
        }

        private void Check2Block_UpdateLodThread()
        {
            if (VisibledGroupCount == 0 && customThreadState == Common.CUSTOMTHREAD_STATUS.csReadyBlock)
            {
                //阻塞线程,线程让出CPU时间片
                try
                {
                    suspendHandle.Reset();
                    customThreadState = Common.CUSTOMTHREAD_STATUS.csBlocked;
                    // Debug.Log("=====block grass update lod---");
                }
                catch (Exception e)
                {

                }
            }
        }

        private void Check2WillBlock_UpdateLodThread()
        {
            if (VisibledGroupCount == 0)
            {
                if (customThreadState != Common.CUSTOMTHREAD_STATUS.csBlocked)
                {
                    //将阻塞线程
                    customThreadState = Common.CUSTOMTHREAD_STATUS.csWillBlock;
                    // Debug.Log("=====WillBlock grass update lod---");
                }
            }
        }

        private bool Check2ReadyBlock_UpdateLodThread()
        {
            if (customThreadState == Common.CUSTOMTHREAD_STATUS.csWillBlock)
            {
                //准备阻塞线程
                customThreadState = Common.CUSTOMTHREAD_STATUS.csReadyBlock;
                //Debug.Log("=====ReadyBlock grass update lod---");
                return true;
            }
            return false;
        }
   
        #endregion
    }
}
