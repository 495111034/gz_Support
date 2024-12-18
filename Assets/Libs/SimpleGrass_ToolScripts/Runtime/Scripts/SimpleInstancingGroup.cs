using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace SimpleGrass
{
    public class SimpleInstance
    {
        public Vector3 pos = Vector3.zero;     //每个实例的LODGroup世界坐标
        public float maxScale = 0;             //每个实例的lossyScale最大值
        public Bounds meshBounds = new Bounds();//每个实例的MESH包围盒，用来做视锥裁剪
        public Dictionary<long, Matrix4x4[]> matxDict = new Dictionary<long, Matrix4x4[]>(); //key:meshcode
        public Dictionary<long, Vector4[]> lightmapScaleOffsetDict = new Dictionary<long, Vector4[]>();//key:meshcode
        public Dictionary<long, float[]> lightProbOcclusionDict = new Dictionary<long, float[]>();//key:meshcode

        public int lodLevel = -1;
    }

    public class SimpleInstancingGroup
    {
        SimpleInstancingMgr InstMgr;
        int orgInstNum = 0;
        public int OrgInstNum
        {
            get { return orgInstNum; }
            set { orgInstNum = value; }
        }

        int visibleInstNum = 0;
        public int VisibleInstNum
        {
            get { return visibleInstNum; }
            set { visibleInstNum = value; }
        }

        int realVisibleInstNum = 0;
        public int RealVisibleInstNum
        {
            get { return realVisibleInstNum; }
            set { realVisibleInstNum = value; }
        }

        int visibleMeshNum = 0;
        public int VisibleMeshNum
        {
            get { return visibleMeshNum; }
            set { visibleMeshNum = value; }
        }

        int prototypeIndex;
        public int PrototypeIndex
        {
            get { return prototypeIndex; }
            set { prototypeIndex = value; }
        }

        bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible && (lodLevel != -1); }
            set { isVisible = value; }
        }

        int lodNum = -1;
        public int LodNum
        {
            get { return lodNum; }
            set { lodNum = value; }
        }
        //int[] lod0Codes = new int[3];
        public bool SpereIntersect = false;
        float SpereRadius = 0;
        BoundingSphere boundingSpere;
        public BoundingSphere BoundingSpere
        {
            get { return boundingSpere; }
            set {
                boundingSpere = value;
                SpereRadius = boundingSpere.radius;
            }
        }
        //所有MESH包围盒的中心点
        public Vector3 worldPosition = Vector3.zero;


        //float maxScaleVale = 0.0f;
        //Vector3 maxScale = Vector3.one;
        ////求LOD参考的块中实例使用最大的比例值
        //public Vector3 MaxScale
        //{
        //    get { return maxScale; }
        //    set
        //    {
        //        maxScale = value;
        //        maxScaleVale = Mathf.Abs(maxScale.x);
        //        maxScaleVale = Mathf.Max(maxScaleVale, Mathf.Abs(maxScale.y));
        //        maxScaleVale = Mathf.Max(maxScaleVale, Mathf.Abs(maxScale.z));
        //    }
        //}

        float maxScale = 0.0f;
        //求LOD参考的块中实例使用最大的比例值
        public float MaxScale
        {
            get { return maxScale; }
            set { maxScale = value;}
        }

        int lodLevel = -1;
        public int LodLevel
        {
            get { return lodLevel; }
        }

        bool enableLod = false;
        public bool EnableLod
        {
            get { return enableLod; }
            set { enableLod = value; }
        }

        bool isSingleMode = false;
        public bool IsSingleMode
        {
            get { return isSingleMode; }
            set { isSingleMode = value; }
        }

        public Common.ProtoMode protoMode = Common.ProtoMode.BatchMode;


        int densityLod = -1;
        public int DensityLod
        {
            get { return densityLod; }
        }

        float densityCutDown = 1;
        public float DensityCutDown
        {
            get { return densityCutDown; }
        }

        bool isInInteract = false;
        public bool IsInInteract
        {
            get { return isInInteract; }
        }

        //批量模式
        public Dictionary<long, Matrix4x4[]> instMatxDict = new Dictionary<long, Matrix4x4[]>(); //key:meshcode
        public Dictionary<long, Vector4[]> instColorDict = new Dictionary<long, Vector4[]>();//key:meshcode
        public Dictionary<long, Vector4[]> instLightmapScaleOffsetDict = new Dictionary<long, Vector4[]>();//key:meshcode
        public Dictionary<long, float[]> instLightProbeOcclusionDict = new Dictionary<long, float[]>();//key:meshcode
        //单例模式
        public List<SimpleInstance> instList = new List<SimpleInstance>();


        private bool checkLoded = false;
        private bool validateLod = false;
        private bool checkDensityLoded = false;
        private bool validateDensityLod = false;
        public float tmpDistanceFromCamera = 0;
        public SimpleInstancingGroup(SimpleInstancingMgr instancingMgr)
        {
            this.InstMgr = instancingMgr;
        }

        #region Unity Functions      
        void OnDestroy()
        {
            instMatxDict.Clear();
            instColorDict.Clear();
            instLightmapScaleOffsetDict.Clear();
            instLightProbeOcclusionDict.Clear();

            instList.Clear();
        }
        #endregion

        #region UpdateLod-Density
        public void UpdateLod(Vector3 cameraPosition, Vector3 heroPosition,ref int retDirtyProtoIndex, bool lockVisit = false)
        {
            bool priorInInteract = this.isInInteract;
            this.isInInteract = false;
            if (! isVisible)
                return;
            float fLength = (cameraPosition - worldPosition).magnitude;
            tmpDistanceFromCamera = fLength;
            //判断：最远裁剪距离            
            SimpleInstancingMgr.PrototypeInfo protoType = InstMgr.prototypeList[PrototypeIndex];
            float maxDist = protoType.protoTypeCullMaxDists; 
            maxDist = Math.Max(SpereRadius, maxDist);
            if (Common.IsTest)
            {
                maxDist = Common.TestCullDistance;
            }

            //不支持GPU Instancing 特殊处理
            float denLODScale = 1.0f;
            if (!InstMgr.UseInstancing)// && 
            {
                maxDist = Math.Min(maxDist, Common.NotGPUInstancing_MaxDistance);
                //非批量模式或自定义的，为了效果距离不受BIAS影响
                if (!protoType.isCustom && !protoType.isSingleMode)
                {
                    fLength *= Common.NotGPUInstancing_DistanceBias;
                    denLODScale = 0.3f;
                }
            }

            //距离：参考不同画质下LODBIAS值
            if (fLength * Common.LodBias > maxDist)
            {
                if (lodLevel != -1)
                {
                    lodLevel = -1;                    
                    if (!lockVisit)
                    {
                        InstMgr.AddDirty(this.prototypeIndex);
                    }

                    retDirtyProtoIndex = this.prototypeIndex;                    
                }
                return;
            }

            //LOD
            int lod = 0;
            //lodgroup检查
            if (IsValidateLod())
            {                
                //批量模式：使用LODGROUP
                if (this.protoMode == Common.ProtoMode.BatchMode_LOD)
                {
                    lod = -1;
                    lod = GetLODByDistance(fLength, Common.LodBias_Quality);
                }
                //singlemode 
                //单个模式：：使用LODGROUP 
                else
                {
                    lod = -1;
                    float lodGroupSize = protoType.protoTypeLodGroupSize;
                    //整个组进行LOD判断
                    lod = GetLODByRelaiveHeight(fLength, lodGroupSize);
                    //如果组是有效LOD，考虑每个实例进行LOD判断（为了减少渲染面数）
                    if (lod != -1 && this.instList.Count > 0)
                    {
                        CheckAllInstLOD(cameraPosition, true, ref retDirtyProtoIndex,lockVisit);
                    }
                }
            }else
            {
                if ( this.instList.Count > 0)
                {
                    CheckAllInstLOD(cameraPosition,false, ref retDirtyProtoIndex, lockVisit);
                }
            }
            ///////////密度相关////////////
            //原型密度
            //if (QualityUtils.IsBadGPU)
            //{
            //    densityCutDown = 0.5f;
            //}
            //else
            //{
                densityCutDown = GetProtoTypeDensity(ref protoType);
                float magnitude = (heroPosition - this.worldPosition).magnitude;
                if (magnitude > 25)
                {
                    densityCutDown = Mathf.Lerp(densityCutDown, 0.05f, tmpDistanceFromCamera * 0.012f);
                }
            //}
            //密度LOD
            int denLod = -1;
            bool isValidateDenLod = IsValidateDensityLod();
            if (isValidateDenLod)
            {
                float lodCutDown = 1.0f;
                denLod = GetDensityLODByDist(fLength , ref lodCutDown,denLODScale);
                densityCutDown *= lodCutDown;
            }
            //原型有修改密度 或 使用密度LOD（目前批量的草）
            if (protoType.density != 1.0f || isValidateDenLod)
            {
                densityCutDown *= Common.LodBias_Quality;
            }

            //不支持GPU Instancing 特殊处理
            if (!InstMgr.UseInstancing && !protoType.isCustom)
            {
                densityCutDown *= Common.NotGPUInstancing_DensityFactor;
            }

            if (lodLevel != lod || densityLod != denLod)
            {
                if (!lockVisit)
                {
                    InstMgr.AddDirty(this.prototypeIndex);
                }
                retDirtyProtoIndex = this.prototypeIndex;
            }

            densityLod = denLod;
            lodLevel = lod;

            if (InstMgr.EnableInteract && heroPosition != Vector3.zero)
            {
                this.isInInteract = ((heroPosition - this.worldPosition).magnitude <= this.SpereRadius + Common.HeroInteractRadius);
                if (priorInInteract != this.isInInteract)
                {
                    if (!lockVisit)
                    {
                        InstMgr.AddDirty(this.prototypeIndex);
                    }
                    retDirtyProtoIndex = this.prototypeIndex;
                }
            }

            
        }

        private bool IsValidateLod()
        {
            if (!checkLoded)
            {
                validateLod = false;
                SimpleInstancingMgr.PrototypeInfo protoInfo = InstMgr.prototypeList[PrototypeIndex];
                //if (enableLod && lodNum > 0 && 
                //     (InstMgr.protoTypeLods[PrototypeIndex] != null) && (InstMgr.protoTypeLods[PrototypeIndex].Length == lodNum))
                 if (enableLod && lodNum > 0 &&
                     (protoInfo.protoTypeLods != null) && (protoInfo.protoTypeLods.Length == lodNum))
                {
                    validateLod = true;
                }

                checkLoded = true;
                return validateLod;
            } else
            {
                return validateLod;
            }
        }

        private bool IsValidateDensityLod()
        {
            if(InstMgr && !InstMgr.UseDensityLod)
            {
                return false;
            }

            if (!checkDensityLoded)
            {
                validateDensityLod = false;
                SimpleInstancingMgr.PrototypeInfo protoInfo = InstMgr.prototypeList[PrototypeIndex];                
                 if (protoInfo.useDensityLod &&
                    (protoInfo.protoTypeDensityLods != null) && (protoInfo.protoTypeDensityCutdown != null)
                     &&   (protoInfo.protoTypeDensityLods.Length == protoInfo.protoTypeDensityCutdown.Length))
                {
                    validateDensityLod = true;
                }

                checkDensityLoded = true;
                return validateDensityLod;
            } else
            {
                return validateDensityLod;
            }
        }
 
        private int GetLODByRelaiveHeight(float distance,float lodGroupSize)
        {
            float sizeVal = lodGroupSize * maxScale;//float sizeVal = lodGroupSize * maxScaleVale;
            float relativeHeight = Common.DistanceToRelativeHeight(distance, sizeVal);
            //var distance = (lodGroup.transform.TransformPoint(lodGroup.localReferencePoint) - camera.transform.position).magnitude;
            //return DistanceToRelativeHeight(camera, (distance / QualitySettings.lodBias), GetWorldSpaceSize(lodGroup));
            int lodIndex = -1;
            SimpleInstancingMgr.PrototypeInfo protoInfo = InstMgr.prototypeList[PrototypeIndex];
            for (int i = 0; i < lodNum; ++i)
            {
                float screenRelativeTransitionHeight = protoInfo.protoTypeLods[i];
                if (relativeHeight >= screenRelativeTransitionHeight)
                {
                    lodIndex = i;
                    lodIndex = Math.Max(lodIndex, Common.MaximumLODLevel);
                    break;
                }
            }            
            return lodIndex;
        }

        private void CheckAllInstLOD(Vector3 cameraPosition, bool checkLodGroup,ref int retDirtyProtoIndex, bool lockVisit)
        {
            //判断：最远裁剪距离
            SimpleInstancingMgr.PrototypeInfo protoInfo = InstMgr.prototypeList[PrototypeIndex];
            float maxDist = protoInfo.protoTypeCullMaxDists;
            if (Common.IsTest)
            {
                maxDist = Common.TestCullDistance;
            }

            float lodGroupSize = protoInfo.protoTypeLodGroupSize;
            bool dirty = false;
            //块包围球与视锥相交
            SpereIntersect = Common.TestPlanesSphereIntersect(Common.INTERNAL_ReusableCameraPlanes, ref this.boundingSpere);

            for (int i = 0; i < instList.Count; ++i)
            {
                int lod = -1;
                //远距离裁剪
                float fLength = (cameraPosition - instList[i].pos).magnitude;
                if (fLength * Common.LodBias > maxDist)
                {
                    lod = -1;                    
                }
                else
                {
                    //视锥再裁剪。（块的视锥裁剪后，再详细裁剪）
                    bool checkViewPort = true;
                    if(SpereIntersect) //块包围球与视锥相交，才进一步剔除
                    {
                         // checkViewPort = GeometryUtility.TestPlanesAABB(Common.INTERNAL_ReusableCameraPlanes, instList[i].meshBounds);
                         checkViewPort = Common.TestPlanesAABB_Unity(Common.INTERNAL_ReusableCameraPlanes, ref instList[i].meshBounds);
                       // checkViewPort = Common.TestPlanesAABB(Common.INTERNAL_ReusableCameraPlanes, ref instList[i].meshBounds);
                    }                                                              
                    if (!checkViewPort)
                    {
                        lod = -1;
                    }
                    else
                    {
                        //检查LODGROUP
                        if(checkLodGroup)
                        {
                            //计算LODLEVEL
                            float sizeVal = lodGroupSize * instList[i].maxScale;
                            float relativeHeight = Common.DistanceToRelativeHeight(fLength, sizeVal);
                            for (int j = 0; j < lodNum; ++j)
                            {
                                float screenRelativeTransitionHeight = protoInfo.protoTypeLods[j];
                                if (relativeHeight >= screenRelativeTransitionHeight)
                                {
                                    lod = j;
                                    lod = Math.Max(lod, Common.MaximumLODLevel);
                                    break;
                                }
                            }
                        }else
                        {
                            lod = 0;//
                        }                        
                    }                    
                }

                if (instList[i].lodLevel != lod)
                {
                    instList[i].lodLevel = lod;
                    dirty = true;
                }
            }//end for

            if(dirty)
            {
                //InstMgr.AddDirty(this.prototypeIndex, lockVisit);
                if (!lockVisit)
                {
                    InstMgr.AddDirty(this.prototypeIndex);
                }
                retDirtyProtoIndex = this.prototypeIndex;
            }
               
        }

        private int GetDensityLODByDist(float distance, ref float denCutdown, float denLODScale = 1.0f)
        {
            
            int lodIndex = -1;
            SimpleInstancingMgr.PrototypeInfo protoInfo = InstMgr.prototypeList[PrototypeIndex];
            
            for (int i = InstMgr.DensityLodsDistance.Length-1; i >= 0; --i)
            {
                if (distance >= InstMgr.DensityLodsDistance[i] * denLODScale)
                {
                    lodIndex = i;

                    denCutdown = InstMgr.DensityLodsCutdown[i];                   
                    break;
                }
            }        
            
            if(InstMgr.GradualDensity)
            {
                if(lodIndex >= 0 && lodIndex + 1 <= InstMgr.DensityLodsDistance.Length - 1)
                {
                    float d1 = InstMgr.DensityLodsDistance[lodIndex] * denLODScale;
                    float d2 = InstMgr.DensityLodsDistance[lodIndex+1] * denLODScale;
                    float c1 = InstMgr.DensityLodsCutdown[lodIndex];
                    float c2 = InstMgr.DensityLodsCutdown[lodIndex+1];
                    denCutdown = Mathf.Lerp(c1, c2, (distance - d1) / (d2 - d1));                     
                }
            }
            return lodIndex;            
        }

        private int GetDensityLODByDist_old(float distance, ref float denCutdown, float denLODScale = 1.0f)
        {

            int lodIndex = -1;
            SimpleInstancingMgr.PrototypeInfo protoInfo = InstMgr.prototypeList[PrototypeIndex];
            for (int i = protoInfo.protoTypeDensityLods.Length - 1; i >= 0; --i)
            {
                if (distance >= protoInfo.protoTypeDensityLods[i] * denLODScale)
                {
                    lodIndex = i;

                    denCutdown = protoInfo.protoTypeDensityCutdown[i];
                    break;
                }
            }

            if (InstMgr.GradualDensity)
            {
                if (lodIndex >= 0 && lodIndex + 1 <= protoInfo.protoTypeDensityLods.Length - 1)
                {
                    float d1 = protoInfo.protoTypeDensityLods[lodIndex] * denLODScale;
                    float d2 = protoInfo.protoTypeDensityLods[lodIndex + 1] * denLODScale;
                    float c1 = protoInfo.protoTypeDensityCutdown[lodIndex];
                    float c2 = protoInfo.protoTypeDensityCutdown[lodIndex + 1];
                    denCutdown = Mathf.Lerp(c1, c2, (distance - d1) / (d2 - d1));
                }
            }
            return lodIndex;
        }

        private float GetProtoTypeDensity(ref SimpleInstancingMgr.PrototypeInfo protoType)
        {            
            float density = protoType.density;
            //正常放置的植被，进行密度纠正
            if (! protoType.isCustom)
            {
               // density = Common.CorrectPrototypeDensity(density, InstMgr.DataVerNum);
                if (InstMgr.TestDensity > 0)
                {
                    density = InstMgr.TestDensity;
                }
            }            
            return density;
        }

        private int GetLODByDistance(float distanc,float lodScale = 1.0f)
        {
            int lodIndex = -1;            
            SimpleInstancingMgr.PrototypeInfo protoInfo = InstMgr.prototypeList[PrototypeIndex];
            if (protoInfo.MeshLods == null )
            {
                return -1;
            }

            int maxLod = protoInfo.MeshLods.Length;
            for (int i = 0; i <= maxLod - 1; ++i)
            {
                if (distanc  <= protoInfo.MeshLods[i] * lodScale)
                {
                    lodIndex = i;
                    break;
                }
            }
            if(lodIndex == -1)
            {
                lodIndex = maxLod;
            }
            return lodIndex;
        }

        #endregion
    }


}