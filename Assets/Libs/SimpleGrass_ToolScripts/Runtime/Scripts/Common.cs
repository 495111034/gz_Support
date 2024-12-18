using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System;
using System.Security.Cryptography;

namespace SimpleGrass
{

    public class Common
    {
        /// 版本定义                
        public static int Ver = 6;
        public static int Ver_6_ChunkDataBuff = 6;//增加ChunkDataBuff功能

        public static int Ver_5_AreaWorldGrass = 5;//增加World片区功能
        public static int Ver_4_WorldGrass = 4;//增加World功能
        public static int Ver_3_Dentiy = 3;//增加MD5表
        public static int Ver_2_Dentiy = 2;//增加密度控制
        public static int Ver_1_SameMeshInLODS = 1;//完善容错：多个LOD支持使用同样的MESH

        public static bool UseSort = true;//是否进行排序（目前组为单元,离摄像头由近到远）
        public static float DefMaxDistance = 50;//默认最大的裁剪距离
        public static float MergeChunkDistance = 10f; //合并块的判断距离
        public static int MergeChunkInstanceLimit = 1000;//块中最大实例数量限制


        public static float CameraHalfAngle = 0;
        public static int QualityLevel = -1;
        public static float LodBias = 1f;
        public static float LodBias_Quality = 1f;
        public static int MaximumLODLevel = 0;
        public static Vector3 CameraPosition = Vector3.zero;
        public static Vector3 CameraForward = Vector3.zero;
        public static Vector3 HeroPosition = Vector3.zero;
        public static bool HeroValid = false;
        public static float Grass_RenderRatio = 1.0f;

        public static string CustomPrefix_Prefab = "Scan_Prefab_";
        public static string CustomPrefix_LOD = "Scan_LOD_";
        public static string CustomPrefix_Mesh = "Scan_Mesh_";

        //默认的原型纠正的显示密度
        //public static float DefCorrectDensity = 0.4f;//距离 < densityLod[0] 的近距离显示密度
        //削减距离        
        public static float[] DensityLods = new float[3] { 40f, 60f, 70f };
        //削减比率        
         public static float[] DensityCutdown = new float[3] { 0.4f, 0.2f, 0.05f };//0.5f,0.3f,0.05f

        //距离LOD
        public static float[] MeshLods = new float[2] { 40f, 70f };

        public static float NotGPUInstancing_DensityFactor = 0.3f;
        public static float NotGPUInstancing_DistanceBias = 2.0f;
        public static float NotGPUInstancing_MaxDistance = 60f;
        public static int NotGPUInstancing_MaxNum = 10;

        public static float HeroInteractRadius = 1.0f;

        public static float InVisibleDistConst = 999999f;//float.PositiveInfinity

        public static Byte GROUP_VISIBLE = 1;
        public static Byte GROUP_INVISIBLE = 2;
        public static Byte BYTE_ONE = 1;
        public static Byte BYTE_ZERO = 0;

        ///
        public static bool ENABLE_DRAWGIZMOS = false;

        public static int INDEXTAG_USELIGHTPROBE = -2;
        public static int INDEXTAG_NOTUSELIGHTMAP = -1;        

        public enum ProtoMode
        {
            BatchMode = 0,
            SingleMode = 1,
            BatchMode_LOD = 2,
        }

        public enum CUSTOMTHREAD_STATUS
        {
            csNone = 0,
            csWillBlock = 1,
            csReadyBlock = 2,
            csBlocked = 3
        }

        #region 材质属性ID定义
        public static int PID_UNITY_LIGHTMAP = Shader.PropertyToID("unity_Lightmap");
        public static int PID_UNITY_SHADOWMASK = Shader.PropertyToID("unity_ShadowMask");
        public static int PID_UNITY_LIGHTMAPIND = Shader.PropertyToID("unity_LightmapInd");
        public static int PID_UNITY_LIGHTMAPST = Shader.PropertyToID("unity_LightmapST");
        
        public static int PID_CUSTOM_INTERACT = Shader.PropertyToID("_CUSTOM_INTERACT");
        public static int PID_LIGHTMAPST_CUSTOM = Shader.PropertyToID("_LightmapST_Custom");
        public static int PID_SURF_COLOR = Shader.PropertyToID("_SurfColorArray");
        public static int PID_OPEN_LIGHTPROBEOCC = Shader.PropertyToID("_Open_LIGHTPROBEOCC");
        public static int PID_LIGHTPROBOCCLUSION = Shader.PropertyToID("_LightProbOcclusion");

        public static int PID_HEROPOS = Shader.PropertyToID("_HeroPos");
        #endregion

        #region 变量定义 - 测试
        public static bool IsTest = false;
        public static UnityEngine.Rendering.ShadowCastingMode TestShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;//0默认， 1投射阴影，2接收阴影，3投射并接收阴影，4没有投射和接收影响。
        public static bool TestReceiveShadow = false;
        public static int TestGroupNum = 0;
        public static int TestVisibleGroupNum = 0;

        public static Dictionary<int, int> TestInstNum_ByBuff = new Dictionary<int, int>();
        public static int TestInstanceNum = 0;
        public static int TestVisibleInstanceNum = 0;
        public static int TestRealVisibleInstNum = 0;

        public static int TestMeshNum = 0;
        public static int TestVisibleMeshNum = 0;

        public static int TestCullDistance = 0;
        public static int TestDrawCount = 0;
        public static int TestLoadNum = 9999999;
        #endregion

        #region 公共方法-视锥裁剪
        //public static float DistanceToRelativeHeight(float distance, float size)
        //{
        //  //  float dist = distance / LodBias;
        //    //if (camera.orthographic)
        //    //    return size * 0.5F / camera.orthographicSize;

        //    //var halfAngle = Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5F);
        //    float screenRelativeMetric = 2.0f * CameraHalfAngle / LodBias;
        //    float sqrScreenRelativeMetric = screenRelativeMetric * screenRelativeMetric;
        //    //lodgroup: worldReferencePoint
        //    //ref Vector3 worldReferencePoint,ref Vector3 cameraPosition
        //    //float dist = Mathf.Sqrt((worldReferencePoint - cameraPosition).sqrMagnitude * sqrScreenRelativeMetric);
        //    float dist = Mathf.Sqrt(distance * distance * sqrScreenRelativeMetric);

        //    maxDistanc = LOD.screenRelativeHeight worldspacesize
        //    var relativeHeight = size / dist;
        //  //  var relativeHeight = size * 0.5F / (dist * CameraHalfAngle);
        //    return relativeHeight;
        //}
        public static float DistanceToRelativeHeight(float distance, float size)
        {
            float dist = distance * LodBias; //distance / LodBias;
            //if (camera.orthographic)
            //    return size * 0.5F / camera.orthographicSize;

            //var halfAngle = Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5F);
            var relativeHeight = size * 0.5F / (dist * CameraHalfAngle);
            return relativeHeight;
        }

        static float CameraPlanes_CurTime;
        static float CameraPlanes_CalculateFrequency = 0.15f;
        public static Plane[] INTERNAL_ReusableCameraPlanes = new Plane[6];        
        static System.Action<Plane[], Matrix4x4> _ExtractPlanes = null;

        private static System.Action<Plane[], Matrix4x4> ExtractPlanes
        {
            get
            {
                if (_ExtractPlanes == null)
                {
                    MethodInfo info = typeof(GeometryUtility).GetMethod("Internal_ExtractPlanes", BindingFlags.Static | BindingFlags.NonPublic, null, new System.Type[] { typeof(Plane[]), typeof(Matrix4x4) }, null);
                    _ExtractPlanes = System.Delegate.CreateDelegate(typeof(System.Action<Plane[], Matrix4x4>), info) as System.Action<Plane[], Matrix4x4>;
                }

                return _ExtractPlanes;
            }
        }
        public static Plane[] CalculateFrustumPlanes(Camera camera, Plane[] planeArray,float currentTime)
        {
            if (_ExtractPlanes == null)
            {
                CameraPlanes_CurTime = currentTime;
                ExtractPlanes(planeArray, camera.projectionMatrix * camera.worldToCameraMatrix);
            }
            else
            {
                if(currentTime - CameraPlanes_CurTime >= CameraPlanes_CalculateFrequency)//0.15
                {
                    ExtractPlanes(planeArray, camera.projectionMatrix * camera.worldToCameraMatrix);
                    CameraPlanes_CurTime = currentTime;
                }
            }

            //ExtractPlanes(planeArray, camera.projectionMatrix * camera.worldToCameraMatrix);
            return planeArray;
        }

        public static bool TestPlanesSphereIntersect(Plane[] planes, ref BoundingSphere sphere)
        {
            for (int planeIndex = 0; planeIndex < planes.Length; planeIndex++)
            {
                float distance = planes[planeIndex].GetDistanceToPoint(sphere.position);
                if (distance <= sphere.radius)
                    return true;
            }
            return false;
        }

        public static bool TestPlanesAABB_Unity(Plane[] planes, ref Bounds bounds)
        {
            // uint planemask = 63;
            Vector3 center = bounds.center;
            Vector3 extent = bounds.extents;
            // uint mk = 1;
            //while (mk <= planemask)
            {
                for (int i = 0; i < 6; ++i)
                {
                    Vector3 absNormal = new Vector3(Mathf.Abs(planes[i].normal.x), Mathf.Abs(planes[i].normal.y), Mathf.Abs(planes[i].normal.z));
                    float dist = planes[i].GetDistanceToPoint(center);
                    float radius = Vector3.Dot(extent, absNormal);
                    if (dist + radius < 0)
                        return false;
                }

            }
            return true;
        }
        public static bool TestPlanesAABB(Plane[] planes, ref Bounds bounds)
        {
            var min = bounds.min;
            var max = bounds.max;

            return TestPlanesAABBInternalFast(planes, ref min, ref max);
        }

        private static bool TestPlanesAABBInternalFast(Plane[] planes, ref Vector3 boundsMin, ref Vector3 boundsMax)
        {
            Vector3 vmin, vmax;
            var testResult = true;//var testResult = TestPlanesResults.Inside;

            for (int planeIndex = 0; planeIndex < planes.Length; planeIndex++)
            {
                var normal = planes[planeIndex].normal;
                var planeDistance = planes[planeIndex].distance;

                // X axis
                if (normal.x < 0)
                {
                    vmin.x = boundsMin.x;
                    vmax.x = boundsMax.x;
                }
                else
                {
                    vmin.x = boundsMax.x;
                    vmax.x = boundsMin.x;
                }

                // Y axis
                if (normal.y < 0)
                {
                    vmin.y = boundsMin.y;
                    vmax.y = boundsMax.y;
                }
                else
                {
                    vmin.y = boundsMax.y;
                    vmax.y = boundsMin.y;
                }

                // Z axis
                if (normal.z < 0)
                {
                    vmin.z = boundsMin.z;
                    vmax.z = boundsMax.z;
                }
                else
                {
                    vmin.z = boundsMax.z;
                    vmax.z = boundsMin.z;
                }

                var dot1 = normal.x * vmin.x + normal.y * vmin.y + normal.z * vmin.z;
                if (dot1 + planeDistance < 0)
                    return false;// TestPlanesResults.Outside;

                //if (testIntersection)
                {
                    var dot2 = normal.x * vmax.x + normal.y * vmax.y + normal.z * vmax.z;
                    if (dot2 + planeDistance <= 0)
                        return true;// testResult = TestPlanesResults.Intersect;
                }
            }

            return testResult;
        }

        #endregion

        #region  公共方法        
        public static void CalMergeBoundSize(Vector3 center1, Vector3 size1, Vector3 center2, Vector3 size2, out Vector3 mergeCenter, out Vector3 mergeSize)
        {
            mergeCenter = (center1 + center2) / 2.0f;
            Vector3 min1 = center1 - (size1 * 0.5f);
            Vector3 max1 = center1 + (size1 * 0.5f);

            Vector3 min2 = center2 - (size2 * 0.5f);
            Vector3 max2 = center2 + (size2 * 0.5f);

            Vector3 newMin = Vector3.zero;
            newMin.x = min1.x < min2.x ? min1.x : min2.x;
            newMin.y = min1.y < min2.y ? min1.y : min2.y;
            newMin.z = min1.z < min2.z ? min1.z : min2.z;

            Vector3 newMax = Vector3.zero;
            newMax.x = max1.x > max2.x ? max1.x : max2.x;
            newMax.y = max1.y > max2.y ? max1.y : max2.y;
            newMax.z = max1.z > max2.z ? max1.z : max2.z;

            Vector3 newSize = Vector3.zero;
            newSize.x = newMax.x - newMin.x;
            newSize.y = newMax.y - newMin.y;
            newSize.z = newMax.z - newMin.z;

            mergeSize = newSize;
        }
        public static bool IsContainSphere(float[] sphere1, float[] sphere2)
        {

            Vector3 center = new Vector3(sphere1[0], sphere1[1], sphere1[2]);
            float r = sphere1[3];

            Vector3 bcenter = new Vector3(sphere2[0], sphere2[1], sphere2[2]);
            float bR = sphere2[3];
            //dis + min(r1, r2) <= max(r1, r2);

            float dis = (bcenter - center).magnitude;
            return (dis + bR <= r);
        }
        public static void CalMergeSphereBound(float[] sphere1, float[] sphere2, out Vector3 center, out float radius)
        {
            Bounds a = new Bounds();
            a.center = new Vector3(sphere1[0], sphere1[1], sphere1[2]);
            a.size = new Vector3(sphere1[3] * 2.0f, sphere1[3] * 2.0f, sphere1[3] * 2.0f);

            Bounds b = new Bounds();
            b.center = new Vector3(sphere2[0], sphere2[1], sphere2[2]);
            b.size = new Vector3(sphere2[3] * 2.0f, sphere2[3] * 2.0f, sphere2[3] * 2.0f);

            a.Encapsulate(b);

            center = a.center;
            radius = a.size.x > a.size.y ? a.size.x : a.size.y;
            radius = radius > a.size.z ? radius : a.size.z;
            radius = radius * 0.5f;
        }

        static StringBuilder strBuilder = new StringBuilder(8);
        public static long GetMeshHashCode(int ver,ref string sceneTag, ref string prototypeName, ref string grassPrefabName, ref string meshRenderName, int lodIndex, int lightmapidx, ref string retMdsCode)
        {
            //大世界植被，增加场景标识名称.保持场景之间的数据独立
            if (ver >= Common.Ver_5_AreaWorldGrass)
            {
                strBuilder.Append(sceneTag);
                strBuilder.Append("_");
            }
            strBuilder.Append(prototypeName);
            strBuilder.Append(grassPrefabName);
            strBuilder.Append(meshRenderName);
            if (ver >= Common.Ver_1_SameMeshInLODS && lodIndex >= 0)
            {
                strBuilder.Append("LOD_");
                strBuilder.Append(lodIndex);
            }
            strBuilder.Append(lightmapidx);
            // string str = strBuilder.ToString();
            long meshCode = strBuilder.ToString().GetHashCode(); //prototypeName + grassPrefabName + meshRenderName.GetHashCode() + lightmapidx;

            //返回MD5值
            retMdsCode = GetMD5(strBuilder.ToString());

            strBuilder.Remove(0, strBuilder.Length);
            return meshCode;
        }
        public static long GetMeshHashCode_NoLightmapIdx(int ver, ref string sceneTag, ref string prototypeName, ref string grassPrefabName, ref string meshRenderName, int lodIndex, ref string retMdsCode)
        {
            //大世界植被，增加场景标识名称
            if (ver >= Common.Ver_5_AreaWorldGrass)
            {
                strBuilder.Append(sceneTag);
                strBuilder.Append("_");
            }
            strBuilder.Append(prototypeName);
            strBuilder.Append(grassPrefabName);
            strBuilder.Append(meshRenderName);
            if (ver >= Common.Ver_1_SameMeshInLODS && lodIndex >= 0)
            {
                strBuilder.Append("LOD_");
                strBuilder.Append(lodIndex);
            }
            long meshCode = strBuilder.ToString().GetHashCode(); //prototypeName + grassPrefabName + meshRenderName.GetHashCode() + lightmapidx;

            //返回MD5值
            retMdsCode = GetMD5(strBuilder.ToString());

            strBuilder.Remove(0, strBuilder.Length);
            return meshCode;
        }

        public static string GetCustomPrefabNameByScan(string prototypeName)
        {
            bool isTrue = prototypeName.StartsWith(CustomPrefix_Prefab);
            if (isTrue)
            {
                return prototypeName.Substring(CustomPrefix_Prefab.Length);
            }

            isTrue = prototypeName.StartsWith(CustomPrefix_LOD);
            if (isTrue)
            {
                return prototypeName.Substring(CustomPrefix_LOD.Length);
            }

            isTrue = prototypeName.StartsWith(CustomPrefix_Mesh);
            if (isTrue)
            {
                return prototypeName.Substring(CustomPrefix_Mesh.Length);
            }

            return "";
        }

        public static int GetCustomPrototypeModeByScan(string prototypeName)
        {
            bool isTrue = prototypeName.StartsWith(CustomPrefix_Prefab);
            if (isTrue)
            {
                return 0;
            }

            isTrue = prototypeName.StartsWith(CustomPrefix_LOD);
            if (isTrue)
            {
                return 1;
            }

            isTrue = prototypeName.StartsWith(CustomPrefix_Mesh);
            if (isTrue)
            {
                return 2;
            }

            return -1;
        }

        public static int GetMatIdentify(Material[] mat)
        {
            int hash = 0;
            if (mat != null)
            {
                for (int i = 0; i != mat.Length; ++i)
                {
                    if (mat[i] != null)
                    {
                        hash += mat[i].name.GetHashCode();
                    }
                }
            }
            return hash;
        }

        public static bool GetLightmapInfo(Renderer meshRender, long protoLightMapIndex, int protoLightMapNum, out int lightmapIndex, out Vector4 lightmapScaleOffset, out long outProtoLightMapIndex, out int outProtoLightMapNum)
        {
            lightmapIndex = meshRender.lightmapIndex;
            lightmapScaleOffset = Vector4.zero;
            long mapIndex = protoLightMapIndex;
            int mapIndexNum = protoLightMapNum;
            if (lightmapIndex >= 0)
            {
                //int tag = (1 << (lightmapIndex));
                long val = 1;
                long tag = (val << (lightmapIndex));
                if ((mapIndex & tag) != tag)
                {
                    mapIndex += tag;
                    ++mapIndexNum;
                }
                lightmapScaleOffset = meshRender.lightmapScaleOffset;
                outProtoLightMapIndex = mapIndex;
                outProtoLightMapNum = mapIndexNum;
                return true;
            }
            else
            {
                lightmapIndex = INDEXTAG_NOTUSELIGHTMAP;//-1;
                outProtoLightMapIndex = protoLightMapIndex;
                outProtoLightMapNum = protoLightMapNum;
                return false;
            }
        }

        public static bool GetLightmapInfo2(Renderer meshRender, long protoLightMapIndex, long protoLightMapIndex2,
            int protoLightMapNum, out int lightmapIndex, out Vector4 lightmapScaleOffset,
            out long outProtoLightMapIndex, out long outProtoLightMapIndex2, out int outProtoLightMapNum)
        {           
            lightmapIndex = meshRender.lightmapIndex;
            lightmapScaleOffset = Vector4.zero;
            long mapIndex = protoLightMapIndex;
            long mapIndex2 = protoLightMapIndex2;
            int mapIndexNum = protoLightMapNum;
            if (lightmapIndex >= 0)
            {
                if (lightmapIndex <= 63)
                {
                    long val = 1;
                    long tag = (val << (lightmapIndex));
                    if ((mapIndex & tag) != tag)
                    {
                        mapIndex += tag;
                        ++mapIndexNum;
                    }
                }
                else
                {
                    int moveBit = lightmapIndex - 64;
                    long val = 1;
                    long tag = (val << (moveBit));
                    if ((mapIndex2 & tag) != tag)
                    {
                        mapIndex2 += tag;
                        ++mapIndexNum;
                    }
                }

                lightmapScaleOffset = meshRender.lightmapScaleOffset;
                outProtoLightMapIndex = mapIndex;
                outProtoLightMapIndex2 = mapIndex2;
                outProtoLightMapNum = mapIndexNum;
                return true;
            }
            else
            {
                lightmapIndex = INDEXTAG_NOTUSELIGHTMAP;// - 1;
                outProtoLightMapIndex = protoLightMapIndex;
                outProtoLightMapIndex2 = protoLightMapIndex2;
                outProtoLightMapNum = protoLightMapNum;
                return false;
            }
        }

        public static int GetMeshTriangleNum(MeshFilter[] meshFilters)
        {
            int num = 0;
            for (int i = 0; i < meshFilters.Length; ++i)
            {
                if (meshFilters[i] != null && meshFilters[i].sharedMesh != null)
                {
                    num += meshFilters[i].sharedMesh.triangles.Length / 3;
                }
            }
            return num;

        }

        public static void InitDensityLods(ref float[] denLods, ref float[] deCutdown, float[] DensityLodsDistance, float[] DensityLodsCutdown)
        {
            //削减距离
            if (denLods == null || DensityLodsDistance.Length != denLods.Length)
            {
                denLods = new float[DensityLodsDistance.Length];
            }
            Array.Copy(DensityLodsDistance, 0, denLods, 0, denLods.Length);

            //削减比率
            if (deCutdown == null || DensityLodsCutdown.Length != deCutdown.Length)
            {
                deCutdown = new float[DensityLodsCutdown.Length];
            }
            Array.Copy(DensityLodsCutdown, 0, deCutdown, 0, deCutdown.Length);
        }

        //public static float CorrectPrototypeDensity(float density, int dataVerNum)
        //{
        //    //削减旧版本数据的密度.
        //    if (dataVerNum < Common.Ver_2_Dentiy)
        //    {
        //        return Math.Min(density, Common.DefCorrectDensity);//0.4f
        //    }
        //    return density;
        //}

        public static void Debug(string log, bool isError = false)
        {
            //unity运行时，输出LOG。
            if (Application.installMode == ApplicationInstallMode.Editor)
            {
                if(isError)
                    UnityEngine.Debug.LogError(log);
                else
                    UnityEngine.Debug.Log(log);
            }
        }
        public static string GetMD5(string input)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytValue, bytHash;
            bytValue = System.Text.Encoding.UTF8.GetBytes(input);
            bytHash = md5.ComputeHash(bytValue);
            md5.Clear();
            string sTemp = string.Empty;
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }
            return sTemp.ToLower();
        }
        public static int GetLightmapNum(long lmIndex1, long lmIndex2)
        {
            long tag = 1;
            int num = 0;
            //0-63
            long lmIndex = lmIndex1;
            for (int i = 0; i <= 63; ++i)
            {
                long val = tag << i;
                if ((lmIndex & val) == val)
                {
                    ++num;
                }
            }
            lmIndex = lmIndex2;
            //64--127                            
            for (int i = 0; i <= 63; ++i)
            {
                long val = tag << i;
                if ((lmIndex & val) == val)
                {
                    ++num;
                }
            }


            //
            return num;
        }
        public static string TryGetPrefabName(ProtoTypeData protoTypeData)
        {
            if (protoTypeData.GrassPrebab)
            {
                //扫描的带预制体的数据，沿用保存时的Prefab名称
                bool isTrue = protoTypeData.ProtoKey.StartsWith(Common.CustomPrefix_Prefab);
                if (isTrue)
                {
                    return protoTypeData.ProtoKey.Substring(Common.CustomPrefix_Prefab.Length);
                }

                //默认种植模式
                return protoTypeData.GrassPrebab.name;
            }

            //其它，扫描的数据模式
            if (protoTypeData.Custom != null)
            {
                return Common.GetCustomPrefabNameByScan(protoTypeData.ProtoKey);
            }

            return "";
        }
        public static void InitDistanceMeshLods(ref float[] LodDists, float[] globalMeshLods)
        {
            //距离
            if (LodDists == null || LodDists.Length == 0)
            {
                if (globalMeshLods != null && globalMeshLods.Length > 0)
                {
                    LodDists = new float[globalMeshLods.Length];
                    Array.Copy(globalMeshLods, 0, LodDists, 0, LodDists.Length);
                    return;
                }

                Array.Copy(Common.MeshLods, 0, LodDists, 0, LodDists.Length);
            }
        }
        public static bool GetLow64BitBool(long low64, int value)
        {
            long tmp = 1;
            long tag = (tmp << value);
            if ((low64 & tag) == tag)
            {
                return true;
            }

            return false;
        }
        public static bool GetHigh64BitBool(long high64, int value)
        {
            long tmp = 1;
            int moveBit = value - 64;
            long tag = (tmp << (moveBit));
            if ((high64 & tag) == tag)
            {
                return true;
            }
            return false;
        }
        public static bool GetBitBool(long low64, long high64, int value)
        {
            if (value <= 63)
            {
                long tmp = 1;
                long tag = (tmp << value);
                if ((low64 & tag) == tag)
                {
                    return true;
                }
            }
            else
            {
                long tmp = 1;
                int moveBit = value - 64;
                long tag = (tmp << (moveBit));
                if ((high64 & tag) == tag)
                {
                    return true;
                }
            }
            return false;
        }
        public static void SetBitBool(ref long low64, ref long high64, int value)
        {
            if (value <= 63)
            {
                long tmp = 1;
                long tag = (tmp << value);
                if ((low64 & tag) != tag)
                {
                    low64 |= tag;
                }
            }
            else
            {
                long tmp = 1;
                int moveBit = value - 64;
                long tag = (tmp << (moveBit));
                if ((high64 & tag) == tag)
                {
                    high64 |= tag;
                }
            }
        }
        public static bool GetLightOcclusionProbes(List<Vector3> posList, List<float> retOcclusionProbes)
        {
            if (posList.Count == 0)
            {
                return false;
            }

            var lightprobes = new UnityEngine.Rendering.SphericalHarmonicsL2[posList.Count];
            var occlusionprobes = new Vector4[posList.Count];
            var grassPoss = posList.ToArray();
            LightProbes.CalculateInterpolatedLightAndOcclusionProbes(grassPoss, lightprobes, occlusionprobes);
            retOcclusionProbes.Clear();
            for (int index = 0; index < occlusionprobes.Length; ++index)
            {
                retOcclusionProbes.Add(occlusionprobes[index].x);
            }
            return true;
        }

        public static int ResetQualityLevelData()
        {
            //检测画质是否变化
            if (QualityLevel == -1 || QualityLevel != QualitySettings.GetQualityLevel() || Common.Grass_RenderRatio != GameSupport.GameGlobalVars.Grass_RenderRatio)
            {
                float biasVal = Mathf.Max(0.000001f, GameSupport.GameGlobalVars.Grass_RenderRatio * QualitySettings.lodBias);
                
                Common.LodBias = 1.0f / biasVal;
                Common.LodBias_Quality = biasVal;
                Common.MaximumLODLevel = QualitySettings.maximumLODLevel;
                QualityLevel = QualitySettings.GetQualityLevel();

                Common.Grass_RenderRatio = GameSupport.GameGlobalVars.Grass_RenderRatio;
            }
            return QualityLevel;
        }

        public static bool ValidGrassRender()
        {
            return (GameSupport.GameGlobalVars.Grass_RenderRatio > 0.0f);
        }

        public static void ResetPositionData(Camera mainCamera,bool enableInteract)
        {
            Common.CameraPosition = mainCamera.transform.position;
            Common.CameraForward = mainCamera.transform.forward;
            Common.HeroValid = false;
            //if (enableInteract)
            //{
            //    Common.HeroValid = SimpleGrassGlobal.Global.GetHeroPos(out Common.HeroPosition);
            //}
        }
        #endregion

        #region 其它
        private static bool APP_INSTALLMODE_ISEDITOR = false;
        public static StringBuilder DebugLogBuilder = new StringBuilder();
        public static bool ENABLE_DEBUGOUTLOG = false;
        public static bool EnableDebugOutLog()
        {
            return ENABLE_DEBUGOUTLOG && Common.APP_INSTALLMODE_ISEDITOR;            
        }
        public static void ClearOutputDebugLog()
        {
            APP_INSTALLMODE_ISEDITOR = (Application.installMode == ApplicationInstallMode.Editor);
            DebugLogBuilder.Clear();
        }
        public static void OutputDebugLog(string log, bool isError = false)
        {
            if (EnableDebugOutLog())
            {
                //获取当前时间
                int hour = DateTime.Now.Hour;
                int minute = DateTime.Now.Minute;
                int second = DateTime.Now.Second;
                // int year = DateTime.Now.Year;
                // int month = DateTime.Now.Month;
                //int day = DateTime.Now.Day;

                //格式化显示当前时间
                //string timestr = string.Format("{0:D2}:{1:D2}:{2:D2} " + "{3:D4}/{4:D2}/{5:D2}", hour, minute, second, year, month, day);
                string str = string.Format("{0:D2}:{1:D2}:{2:D2}:{3} ", hour, minute, second, log);
                DebugLogBuilder.Append(str);
                DebugLogBuilder.Append(Environment.NewLine);
                if (DebugLogBuilder.Length > 500)
                {
                    DebugLogBuilder.Remove(0, 1);
                }

                if(!isError)
                {
                    UnityEngine.Debug.Log(str);
                }else
                {
                    UnityEngine.Debug.LogError(str);
                }
                
            }
        }

        public static void OutputDebugLog(string log, string log2)
        {
            if (EnableDebugOutLog())
            {
                //获取当前时间
                int hour = DateTime.Now.Hour;
                int minute = DateTime.Now.Minute;
                int second = DateTime.Now.Second;
                // int year = DateTime.Now.Year;
                // int month = DateTime.Now.Month;
                //int day = DateTime.Now.Day;

                //格式化显示当前时间
                //string timestr = string.Format("{0:D2}:{1:D2}:{2:D2} " + "{3:D4}/{4:D2}/{5:D2}", hour, minute, second, year, month, day);
                string str = string.Format("{0:D2}:{1:D2}:{2:D2}:{3}{4} ", hour, minute, second, log, log2);
                DebugLogBuilder.Append(str);
                DebugLogBuilder.Append(Environment.NewLine);
                if (DebugLogBuilder.Length > 500)
                {
                    DebugLogBuilder.Remove(0, 1);
                }
              
                UnityEngine.Debug.Log(str);               
            }
        }

        #endregion
    }

}
