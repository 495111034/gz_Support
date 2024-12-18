using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleGrass
{

 // [ExecuteInEditMode]
    public class SimpleGrassSys : MonoBehaviour
    {
       // public static bool UseWorld = false;

        [SerializeField]
        [HideInInspector]
        SimpleGrassProfile profile = null;
        public SimpleGrassProfile Profile
        {
            get { return profile; }
            set
            {
                if (value != profile)
                {
                    profile = value;
                    if (profile != null)
                    {
                        profile.Load(this);                       
                    }
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        public SimpleSaveData saveDataProfile = null;
        public SimpleSaveData SaveDataProfile
        {
            get { return saveDataProfile; }
            set
            {
                if (value != saveDataProfile)
                {
                    saveDataProfile = value;
                    //if (saveDataProfile != null)
                    //{
                    //    profile.Load(this);
                    //}
                }
            }
        }

        //[SerializeField]
        //[HideInInspector]
        //WorldGrassData saveWorldDataProfile = null;
        //public WorldGrassData SaveWorldDataProfile
        //{
        //    get { return saveWorldDataProfile; }
        //    set
        //    {
        //        if (value != saveWorldDataProfile)
        //        {
        //            saveWorldDataProfile = value;
        //        }
        //    }
        //}

        //[SerializeField]
        //[HideInInspector]
        //WorldGrassLightmaps saveWorldLightmapsProfile = null;
        //public WorldGrassLightmaps SaveWorldLightmapsProfile
        //{
        //    get { return saveWorldLightmapsProfile; }
        //    set
        //    {
        //        if (value != saveWorldLightmapsProfile)
        //        {
        //            saveWorldLightmapsProfile = value;
        //        }
        //    }
        //}

        [Tooltip("是否开启提示")]
        [SerializeField]
        [HideInInspector]
        bool hintInfo = true;
        public bool HintInfo
        {
            get { return hintInfo; }
            set { hintInfo = value; }
        }

        [Tooltip("是否开启编辑模式")]
        [HideInInspector]
        [SerializeField]
        bool editorMode = true;
        public bool EditorMode
        {
            get { return editorMode; }
            set { editorMode = value; }
        }

        [Tooltip("种类名称，不能重复")]
        [HideInInspector]
        [SerializeField]
        string kindName = "";
        public string KindName
        {
            get { return kindName; }
            set { kindName = value; }
        }


        [Tooltip("对象块预制体")]
        [HideInInspector]
        [SerializeField]
        public GameObject GrassChunkPrefab = null;

        [Tooltip("对象块预制体")]
        [HideInInspector]
        [SerializeField]
        public int ChunkPrefabIndex = -1;

        [Tooltip("操作的视野范围")]
        [HideInInspector]
        [SerializeField]
        public int Editor_ViewDist = 300;

        [Tooltip("放置射线最大允许距离")]
        [HideInInspector]
        [SerializeField]
        public float RayCastDist = 10f;

        [Tooltip("可种植的图层掩码")]
        [HideInInspector]
        [SerializeField]
        public LayerMask PaintingLayerMask = -1;        
                        

        [Tooltip("最终随机缩放比例")]
        [HideInInspector]
        [SerializeField]
        public Vector2 EndMinMaxScale = new Vector2(1.0f, 1.0f);

        [Tooltip("操作的画刷半径")]
        [HideInInspector]
        [SerializeField]
        public float BrushRadius = 5;//maxspread

        [Tooltip("密度")]
        [HideInInspector]
        [SerializeField]
        public Vector2 Density = new Vector2(1f, 5f);

        [Tooltip("开始半径(比例)")]
        [HideInInspector]
        [SerializeField]
        public float StartRadi = 1f;

        [Tooltip("开始高度(比例)")]
        [HideInInspector]
        [SerializeField]
        public float StartLength = 1f;

        [Tooltip("最小半径(比例)")]
        [HideInInspector]
        [SerializeField]
        public float MinRadi = 1f;

        [Tooltip("随机旋转")]
        [HideInInspector]
        [SerializeField]
        public bool RandomRot = true;

        [SerializeField]
        [Tooltip("随机旋转，最小值")]
        [HideInInspector]
        public float RandomRotMin = 0;

        
        [Tooltip("随机旋转，最大值")]
        [HideInInspector]
        [SerializeField]
        public float RandomRotMax = 360;


        [Tooltip("是否在交点法线上种植")]
        [HideInInspector]
        [SerializeField]
        public bool OnNormal = true;

        [Tooltip("种值时探测四周距离的碰撞体，避免放置叠加。")]
        [HideInInspector]
        [SerializeField]
        public float MinAvoidDist = 0.1f;

        [Tooltip("是否跟随父对象")]
        [HideInInspector]
        [SerializeField]
        public bool MoveWithObject = false;

        //
        [Tooltip("是否需要交互，判断运行期[块]碰撞体是否有效")]
        [HideInInspector]
        [SerializeField]
        public bool Interactive = false; 


        [Tooltip("操作画刷进行批量擦除")]
        [HideInInspector]
        [SerializeField]
        public bool BrushErase = true;

        [HideInInspector]
        public bool Erasing = false;
        [HideInInspector]
        public bool Looking = false;


        [HideInInspector]
        [SerializeField]
        public Bounds WorldBounds = new Bounds(Vector3.zero,Vector3.zero);
        [HideInInspector]
        [SerializeField]
        public Bounds RealWorldBounds = new Bounds(Vector3.zero, Vector3.zero);

        [HideInInspector]
        [SerializeField]
        public float WorldCellResolution = 200f;


        [HideInInspector]
        [SerializeField]
        public bool AutoSave = true;

        //保存实例数据为BYTE数组模式
        public bool SaveToBuffer = false;
        //默认优化保存块
        public bool OptimizeSaveChunk = true;
        //x:optimizeRadius, y:optimizeMaxOffset合并小块时，球与球之间允许合并的最大间隔, z:optimizeContainScale允许合并的，被包含比例的最小值
        public Vector3 OptimizeChunkParams = new Vector3(3.0f, 3.0f,2.0f/3.0f);
        void Start()
        {
            editorMode = false;
        }

        public bool RefreshLightmapDatas()
        {
            if (this.saveDataProfile != null)
            {
                SimpleInstancingMgr instMgr = transform.GetComponent<SimpleInstancingMgr>();
                if (instMgr != null)
                {
                    instMgr.lightmapDic.Refresh(this.saveDataProfile);
                    return true;
                }
            }
            return false;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)//SimpleGrassSys.UseWorld && 
            {
                //WorldGrassAreaMgr worldGrassMgr = GetComponent<WorldGrassAreaMgr>();
                //bool useWorld = (worldGrassMgr != null && worldGrassMgr.enabled);
                //if(!useWorld)
                //{
                //    return;
                //}
                //显示世界的包围框
                if ( RealWorldBounds.size != Vector3.zero)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireCube(RealWorldBounds.center, RealWorldBounds.size);// - Vector3.one * 0.1f
                }

                {
                    Gizmos.color = Color.yellow;
                    float maxLen = Mathf.Max(WorldBounds.size.x, WorldBounds.size.z);
                    int num = Mathf.CeilToInt(Mathf.Log (maxLen / WorldCellResolution, 2));                                                      
                    float cellnum = Mathf.Pow(2, num);
                    float editedSize = cellnum * WorldCellResolution;
                    
                    RealWorldBounds.center = WorldBounds.center;
                    RealWorldBounds.size = new Vector3(editedSize, WorldBounds.size.y, editedSize);

                    // maxLen = Mathf.Max(RealWorldBounds.size.x, RealWorldBounds.size.z);
                    float cell = WorldCellResolution;// maxLen / cellnum;
                    Vector3 c = RealWorldBounds.min;
                    c.y = RealWorldBounds.center.y;
                    c.x += cell / 2f;
                    c.z += cell / 2f;
                    Vector3 sz = new Vector3(cell, RealWorldBounds.size.y, cell);
                    Gizmos.DrawWireCube(c, sz);// - Vector3.one * 0.1f
                }
                

                ////显示：生成世界数据的四叉树节点
                //if (saveWorldDataProfile != null)
                //{
                //    saveWorldDataProfile.grassTree.DrawBound(saveWorldDataProfile.grassTree.maxDepth);
                //}
            }
        }
        //private void Update()
        //{
        //    deltaTime += Time.deltaTime;
        //}

        //private void OnEnable()
        //{
        //    deltaTime = 0.0;
        //}

        //private void OnDisable()
        //{
        //    deltaTime = 0.0;
        //}

    }
}