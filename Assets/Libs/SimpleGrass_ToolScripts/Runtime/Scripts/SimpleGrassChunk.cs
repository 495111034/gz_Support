using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleGrass
{

    [System.Serializable]
    
    public class GrassChunkBaseProp
    {
        public bool RandomRot = true;
        public float RandomRotMin = -360;
        public float RandomRotMax = 360;
        
        //[Tooltip("随机缩放比例")]
       // public Vector2 MinMaxScale = new Vector2(1.0f,1.0f);
        
        //[Tooltip("放置半径")]
        public float BrushRadius = 1;

       //[Tooltip("密度")]        
        public Vector2 Density = new Vector2(1f,1f);
        //[Tooltip("是否在交点法线上种植")]
        public bool OnNormal = true;

        //是否跟随对象
        public bool MoveWithObject = false;
             
        //开始半径位置
        public float StartRadi = 0.1f;
        public float StartLength = 1f;
        public float MinRadi = 0.2f;
        //最小回避距离
        public float MinAvoidDist = 0.1f;
        //位置发散参数
        //public Vector2 PosSpread = new Vector2(8, 8);
        //角度发散参数
        // public Vector2 AngleMinMaxSpread = new Vector2(45, 45);
        //是否是有交互
        public bool Interactive = false;

    }

    public class SimpleGrassChunk : MonoBehaviour
    {
        //草预制体
        //string grassPrefabPath;

        //[HideInInspector]
        //public string GrassPrefabPath
        //{
        //    get { return grassPrefabPath; }
        //    set { grassPrefabPath = value; }
        //}

        public GameObject grassPrefab;
        public GameObject GrassPrefab
        {
            get { return grassPrefab; }
            set { grassPrefab = value;}
        }

        public GameObject selfPrefab;
        public GameObject SelfPrefab
        {
            get { return selfPrefab; }
            set { selfPrefab = value;}
        }

        public Vector3 ColliderScale = new Vector3(3, 2, 3);

       // [SerializeField]
        //float[] cullingLodDistance = new float[3] { Common.Lod0, Common.Lod1, Common.Lod2 };

        //public float[] CullingLodDistance
        //{
        //    get { return cullingLodDistance; }
        //    set { cullingLodDistance = value; }
        //}

        [SerializeField]
        float cullingMaxDistance = Common.DefMaxDistance;
        public float CullingMaxDistance
        {
            get { return cullingMaxDistance; }
            set { cullingMaxDistance = value; }
        }

        [SerializeField]
        float mergeChunkDistance = Common.MergeChunkDistance;
        public float MergeChunkDistance
        {
            get { return mergeChunkDistance; }
            set { mergeChunkDistance = value; }
        }

        [SerializeField]
        bool castShadows = false;
        public bool CastShadows
        {
            get { return castShadows; }
            set { castShadows = value; }
        }

        [SerializeField]
        bool receiveShadows = false;
        public bool ReceiveShadows
        {
            get { return receiveShadows; }
            set { receiveShadows = value; }
        }

       
        [HideInInspector]
        [SerializeField]
        int layerID = 0;
        public int LayerID
        {
            get { return layerID; }
            set { layerID = value; }
        }

        //[SerializeField]
        //bool useCell = true;//100
        //public bool UseCell
        //{
        //    get { return useCell; }
        //    set { useCell = value; }
        //}

        //基本属性
        [HideInInspector]
        public GrassChunkBaseProp Prop = new GrassChunkBaseProp();

        //最终比例
        [HideInInspector]
        public float EndScale = 1.0f;

        //
        [HideInInspector]
        public float RayCastDist = 10f;

        [HideInInspector]
        public Vector3 HitPos = new Vector3(0f, 0f, 0f);

        [HideInInspector]
        public Vector3 HitNorm = new Vector3(0f, 0f, 0f);

        [HideInInspector]
        public GameObject PaintOnObj = null;

        [HideInInspector]
        public Collider ChunkCollider = null;


        //半径和长度，递减比例
        private float lower_length_by = 0.8f;
        private float lower_radi_by = 0.7f;

        public MaterialPropertyBlock[] childMaterialBlocks;
        public Vector3[] childColors;

        // Use this for initialization
        public static Ray testray;
        void Start()
        {
            UpdateCollider();

        }

        //public void Update()
        //{
        //    //?? this.UpdateCollider();
        //}

        void OnDrawGizmos()
        {
            if (ChunkCollider != null)
            {                
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(this.transform.position, (ChunkCollider as BoxCollider).bounds.size);
            }
             
         }

        public void MakeBranckes(bool isRoot, float startRadi, float startLen, System.Func<SimpleGrassChunk, GameObject> FunCreateGrassPrefab)
        {
            if (Application.isPlaying)
                return;

            //if (Prop.MoveWithObject)
            //{

            //}else
            {
                ////生成主
                //if ( isRoot ){
                //    GenerateOne(isRoot, startRadi, Prop.StartLength, startLen, new Vector3(EndScale, EndScale, EndScale));
                //} 
                //另个模式. 按密度刷相同比例的实例               
                if(isRoot && startRadi == this.Prop.MinRadi)
                {
                    //块                    
                    float dval = Mathf.Round(Random.Range(Prop.Density.x, Prop.Density.y));
                    int count = (int)(dval);
                    if (count <= 0)
                        return;
                    count = count - 1;

                    //根据密度刷
                    for (int i = 0; i < count; ++i)
                    {
                        Generate(false, startRadi, startLen, FunCreateGrassPrefab);
                    }

                    return;
                }

                float radiA = this.lower_radi_by * startRadi;
                float lenA = this.lower_length_by * startLen;
                if (radiA < this.Prop.MinRadi)
                    return;

                //向上偏移中心点(在底部中心）               
               // float offsetY = this.transform.position.y * 2;
               // offsetY = 0;

                //块
                GameObject grassChunkObj = this.transform.gameObject;
                int childCount = (int)Random.Range(Prop.Density.x,Prop.Density.y);
                if (childCount <= 0)
                    return;

                //根据密度刷
                for (int i = 0; i < childCount; ++i)
                {
                    Generate(false, radiA, lenA, FunCreateGrassPrefab);                  
                }

            }

            //Undo.RegisterCreatedObjectUndo(TEMP,"undo grass");
        }


        public void Generate(bool isRoot, float radi,float len, System.Func<SimpleGrassChunk, GameObject> FunCreateGrassPrefab)
        {            
            if (!isRoot)
            {
                float find_x = 0f;
                float find_z = 0f;
                float find_y = 0f;
               // float scale_mod = 1;
                //根据发散参数，随机一个查找平面上的相对位置
                float fPosSpreadX = this.Prop.BrushRadius;
                float fPosSpreadY = this.Prop.BrushRadius;
                find_x = Random.Range(-fPosSpreadX, fPosSpreadX + Random.Range(-fPosSpreadX / 2, fPosSpreadX / 2));
                find_z = Random.Range(-fPosSpreadY, fPosSpreadY + Random.Range(-fPosSpreadY / 2, fPosSpreadY / 2));
                int sign = 1;
                if (this.HitNorm.y < 0)
                {
                    sign = -1;
                }
                //初始射线（初始草的up朝向的平面）
                //Vector3 startPos = new Vector3(find_x, 0, find_z);
                RaycastHit outHit;
                if (!rayTest(sign,find_x,find_z, this.RayCastDist,out outHit))
                {       return;                  
                }
                //要回避的距离判断。（四周探针测试）
                if(this.Prop.MinAvoidDist != 0)
                {
                    RaycastHit outHit2;
                    float MOD1 = this.Prop.MinAvoidDist;
                    if (!rayTest(sign, find_x + MOD1, find_z + MOD1, this.RayCastDist, out outHit2))
                    {
                        return;
                    }

                    if (!rayTest(sign, find_x - MOD1, find_z - MOD1, this.RayCastDist, out outHit2))
                    {
                        return;
                    }

                    if (!rayTest(sign, find_x + MOD1, find_z - MOD1, this.RayCastDist, out outHit2))
                    {
                        return;
                    }

                    if (!rayTest(sign, find_x - MOD1, find_z + MOD1, this.RayCastDist, out outHit2))
                    {
                        return;
                    }
                }
                //
                
                if (!this.Prop.OnNormal)
                {
                    if (this.PaintOnObj != null && (outHit.collider != null) && outHit.collider.gameObject == this.PaintOnObj.gameObject)
                    {
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    if (this.PaintOnObj != null && (outHit.collider != null) && outHit.collider.gameObject == this.PaintOnObj.gameObject)
                    {
                    }
                    else
                    {
                        return;
                    }
                }
                //可以放置
                //GameObject instanceObj = new GameObject("Instance");
                //instanceObj.transform.parent = this.transform;
                //instanceObj.transform.localPosition = Vector3.zero;
                //instanceObj.transform.localEulerAngles = Vector3.zero;
                //instanceObj.transform.localScale = Vector3.one;

                GameObject curGrass1 = FunCreateGrassPrefab(this);//(GameObject)Object.Instantiate(this.GrassPrefab);
                curGrass1.name = this.GrassPrefab.name;
                curGrass1.transform.parent = this.transform;
                curGrass1.transform.localPosition = Vector3.zero;
                curGrass1.transform.localEulerAngles = Vector3.zero;

                //设置位置
                if (!this.Prop.OnNormal)
                {
                    if (this.PaintOnObj != null && (outHit.collider != null ) && outHit.collider.gameObject == this.PaintOnObj.gameObject)
                    {
                        find_y = -this.transform.position.y + outHit.point.y;
                        curGrass1.transform.localPosition = new Vector3(find_x, find_y, find_z);
                    }
                }
                else
                {
                    if (this.PaintOnObj != null && (outHit.collider != null) && outHit.collider.gameObject == this.PaintOnObj.gameObject)
                    {
                        curGrass1.transform.position = outHit.point;
                    }
                 }
                //设置角度(向外发散效果）
                //float RANGE_X = Random.Range(-this.Prop.AngleMinMaxSpread.x, this.Prop.AngleMinMaxSpread.y);
                //float RANGE_Y = Random.Range(-this.Prop.AngleMinMaxSpread.x, this.Prop.AngleMinMaxSpread.y);
                //curGrass1.transform.Rotate(RANGE_X, 0, RANGE_Y);

                 //设置比例
                Vector3 scaler = new Vector3(radi, len, radi) * EndScale;
                curGrass1.transform.localScale = scaler;

                //设置随机旋转角度
                if (Prop.RandomRot)
                {
                    Vector3 localup = new Vector3(0, 1, 0);
                    curGrass1.transform.Rotate(localup, Random.Range(Prop.RandomRotMin, Prop.RandomRotMax));
                }
            }
            else
            {
                //第一个，直接固定比例
                //GameObject instanceObj = new GameObject("Instance");
                //instanceObj.transform.parent = this.transform;
                //instanceObj.transform.localPosition = Vector3.zero;
                //instanceObj.transform.localEulerAngles = Vector3.zero;
                //instanceObj.transform.localScale = Vector3.one;


                GameObject curGrass1 = FunCreateGrassPrefab(this);//(GameObject)Object.Instantiate(this.GrassPrefab);
                curGrass1.name = this.GrassPrefab.name;
                curGrass1.transform.parent = this.transform;
                curGrass1.transform.localPosition = Vector3.zero;
                curGrass1.transform.localEulerAngles = Vector3.zero;
                // curGrass1.transform.Translate(0, 0.5f * len, 0);
                // curGrass1.transform.Translate(0, INfiniDyForestC.Length_scale * 0.5f * len, 0);
                // curGrass1.transform.localScale = new Vector3(EndScale, EndScale, EndScale);
                curGrass1.transform.localScale = new Vector3(radi, radi, radi) * EndScale;


                if (Prop.RandomRot)
                {
                    Vector3 localup = new Vector3(0, 1, 0);
                    curGrass1.transform.Rotate(localup, Random.Range(Prop.RandomRotMin, Prop.RandomRotMax));
                }

            }

            //递归生成Branck, 比例和长度，变化
            MakeBranckes(isRoot, radi, len, FunCreateGrassPrefab);
        }

        public void BuildCollider()
        {
            //块，增加碰撞盒
            if(ChunkCollider == null)
            {
                this.gameObject.AddComponent<BoxCollider>().isTrigger = true;
                ChunkCollider = this.gameObject.GetComponent<BoxCollider>();
                (ChunkCollider as BoxCollider).size = ColliderScale;
                ChunkCollider.enabled = false;


                //InteractCollider interactBox = this.gameObject.AddComponent<InteractCollider>();
                //interactBox.ChunkHandler = this;
                //interactBox.enabled = false;//暂时没用先屏蔽
            }

            if (this.transform.localScale != Vector3.one)
            {
                ColliderScale = this.transform.localScale;
            }
            (ChunkCollider as BoxCollider).size = ColliderScale;

            UpdateCollider();
        }

        private void UpdateCollider()
        {
            if (this.ChunkCollider == null)
                return;

            if (Application.isPlaying)
            {
                if (!Prop.Interactive)
                {
                    if (this.ChunkCollider.enabled)
                    {
                        this.ChunkCollider.enabled = false;
                    }
                }
                else
                {
                    if (!this.ChunkCollider.enabled)
                    {
                        this.ChunkCollider.enabled = true;
                    }
                }
            }
            else
            {
                if (!this.ChunkCollider.enabled)
                {
                    this.ChunkCollider.enabled = true;   //enable in editor
                }
            }            //GrassCollider.enabled = false;
        }
        
         


        private bool rayTest(int sign, float find_x, float find_z, float rayDist, out RaycastHit outHit)
        {
            Vector3 startPos = new Vector3(find_x, 0, find_z);
            Ray ray = new Ray(this.transform.position + Quaternion.FromToRotation(Vector3.up, -this.HitNorm) * startPos
                               + (rayDist * this.HitNorm), -this.HitNorm);

            testray = ray;
            if (!this.Prop.OnNormal)
            {
                startPos = new Vector3(this.transform.position.x + find_x, this.transform.position.y + (sign * rayDist), this.transform.position.z + find_z);
                ray = new Ray(startPos, -sign * Vector3.up);
            }
            RaycastHit hit = new RaycastHit();
            
            if (Physics.Raycast(ray, out hit, rayDist * 2))
            {
                //如果射线相交，其它物体上就不处理
                if (this.PaintOnObj != null && hit.collider.gameObject != this.PaintOnObj.gameObject)
                {
                    outHit = hit;
                    return false;
                }
            }
            //}else
            //{
            //    outHit = hit;
            //    return true;
            //}

            outHit = hit;
            return true;
        }
    }

}