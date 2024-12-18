using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SimpleGrass{


    [Serializable]
    //每个实例内，同类meshcode中的所有实例信息
    public class DetailData
    {
        //mesh: 矩阵信息  //一组：WorldPosition,WorldRotation,lossyScale
        public List<Vector3> TRS = new List<Vector3>();
        //mesh: lightmapScaleOffset
        public List<Vector4> LSO = new List<Vector4>();
        //地表颜色
        public List<Vector4> Color = new List<Vector4>();
    }

    [Serializable]
    //每个实例数据
    public class InstData
    {            
        //LODGroup及裁剪相关信息
        public Vector3 pos = Vector3.zero;     //每个实例的LODGroup或Mesh父节点世界坐标
        public float maxScale = 0;             //每个实例的LODGROUP或Mesh父节点lossyScale最大值
        public Vector3[] Collider = new Vector3[] { Vector3.zero, Vector3.zero };//MESH的包围盒 Vector3.zero;ColliderSize
        
        public List<long> meshCodes = new List<long>();//MeshCode = prototypeName + prefabNameCode + meshRenderAry[meshIdx].name + lightmapIndex
        public List<DetailData> datas = new List<DetailData>();//索引与meshCodes对应

        public float lightProbOcc;
        #region 公共方法
        public void AppendDetailTRS(int index, List<Vector3> trs)
        {
            if(index >= datas.Count)
            {
                for(int i = datas.Count; i <= index; ++i)
                {
                    datas.Add(new DetailData());
                }
            }

            datas[index].TRS = trs;
        }

        public void AppendDetailLSO(int index, List<Vector4> lso)
        {
            if (index >= datas.Count)
            {
                for (int i = datas.Count; i <= index; ++i)
                {
                    datas.Add(new DetailData());
                }
            }
            
            datas[index].LSO = lso;
        }

        public void AppendDetailColor(int index, List<Vector4> colors)
        {
            if (index >= datas.Count)
            {
                for (int i = datas.Count; i <= index; ++i)
                {
                    datas.Add(new DetailData());
                }
            }

            datas[index].Color = colors;
        }

        public DetailData GetDetailData(int index)
        {
            if (index >= datas.Count)
            {
                return null;
            }
            return datas[index];
        }

        #endregion
    }

    [Serializable]
    //每个块数据
    public class InstanceChunkData
    {
        public int PrototypeIndex = 0;
        public Vector3 worldPos = Vector3.zero;        
        public float[] CullingCollider = new float[4];//centerX,centerY,centerZ、半径   裁剪用               
        public float maxScale = 0;//块中实例的最大比例值 //public float[] MaxScale = new float[3];
        //块下所有实例数据
        public List<InstData> InstanceList = new List<InstData>();
    }

    [Serializable]
    public class MatList
    {
        public Material[] Mats;
    }

    [Serializable]
    public class CustomLod
    {
        public float RelaHeight;
        public List<Mesh> Meshs = new List<Mesh>();
        public List<MatList> Mats = new List<MatList>();
    }

    [Serializable]
    public class ProtoTypeCustom
    {
        public int LodNum;
        public float LODSize;
        public List<CustomLod> LODs = new List<CustomLod>();
    }

    [Serializable]
    //每个类型的数据
    public class ProtoTypeData
    {
        public string ProtoKey;
        //类型名称，chunck预制体资源路径   
       // public string ChunkKey;
        public string ChunkPrebab;
        //类型名称，chunck下物件预制体资源路径   
        //public string GrassKey;
        public GameObject GrassPrebab;
        //扫描资源，扩展的自定义
        public ProtoTypeCustom Custom;
        //块的数量
        public int ChuckNum;
        //使用到lightmap数量
        //public int LightMapNum;
        //1,2,4,8...(0,1,2,3...)每个分类使用到的所有lightmap index        
       // public long LightMapIndex;

        public List<long> MeshCode;//GetMeshHashCode_NoLightmapIdx
        public List<long> LightMapIndex;//0-63
        public List<long> LightMapIndex2;//64-127
        public List<int> LightMapNum;       

        //最远裁剪距离
        public float CullingMaxDist = Common.DefMaxDistance;//cullingMaxDistance

        public bool CastShadows = false;
        public bool ReceiveShadows = false;
        public int Layer = 0;
        public float Density = 1.0f;

        public bool UseLightProb = false;
        public int Tag = 0;

        static public void GetAllLightmapIndexes(List<ProtoTypeData> ProtoTypes,ref SortedSet<int> dict)
        {
            long tag = 1;
            for (int protoIndex = 0; protoIndex < ProtoTypes.Count; ++protoIndex)
            {
                if (ProtoTypes[protoIndex].MeshCode != null)
                {
                    for (int idx = 0; idx < ProtoTypes[protoIndex].MeshCode.Count; ++idx)
                    {
                        int num = ProtoTypes[protoIndex].LightMapNum[idx];
                        if (num <= 0)
                        {
                            continue;
                        }
                        //0-63
                        long lmIndex = ProtoTypes[protoIndex].LightMapIndex[idx];
                        for (int i = 0; i <= 63; ++i)
                        {
                            long val = tag << i;
                            if ((lmIndex & val) == val)
                            {
                                dict.Add(i);
                                --num;
                            }
                            if (num <= 0)
                                break;
                        }
                        //64--127                            
                        if ((num > 0) && ProtoTypes[protoIndex].LightMapIndex2 != null)
                        {
                            lmIndex = ProtoTypes[protoIndex].LightMapIndex2[idx];
                            for (int i = 0; i <= 63; ++i)
                            {
                                long val = tag << i;
                                if ((lmIndex & val) == val)
                                {
                                    dict.Add(64 + i);
                                    --num;
                                }
                                if (num <= 0)
                                    break;
                            }
                        }

                    }
                }
            }
        }
    }


    [Serializable]
    public class SimpleSaveData : ScriptableObject
    {
        public int Ver = 0;
        public List<ProtoTypeData> ProtoTypes = new List<ProtoTypeData>();
        //块数量
        public List<InstanceChunkData> ChunkData = new List<InstanceChunkData>();
        public Byte[] ChunkDataBuff = null;
        //编码和MD5对照表
        public List<long> Codes = new List<long>();
        public List<String> CodeMD5 = new List<string>();

        public List<int> ProtoTypeChunkNum = new List<int>();

        public bool IsSavedBuffer()
        {
            return ((this.ChunkData == null || this.ChunkData.Count == 0) &&
                  (this.ChunkDataBuff != null && this.ChunkDataBuff.Length > 0));
        }
        public int GetProtoTypeChunkNum(int prototypeIndex)
        {
            if (prototypeIndex <= ProtoTypeChunkNum.Count - 1)
            {
                return ProtoTypeChunkNum[prototypeIndex];
            }
            return 0;
        }

        public void SetProtoTypeChunkNum(int prototypeIndex, int num)
        {
            if (prototypeIndex > ProtoTypeChunkNum.Count - 1)
            {
                for (int i = ProtoTypeChunkNum.Count; i < prototypeIndex + 1; ++i)
                {
                    ProtoTypeChunkNum.Add(0);
                }
            }
            ProtoTypeChunkNum[prototypeIndex] = num;
        }

        public long GetCodeByMD5(string codeMD5, long defVal,ref bool existedMD5)
        {
            existedMD5 = false;
            if (CodeMD5 != null && Codes != null)
            {
                for (int index = 0; index < CodeMD5.Count; ++index)
                {
                    if (CodeMD5[index].Equals(codeMD5))
                    {
                        existedMD5 = true;
                        if (index < Codes.Count)
                        {
                            return Codes[index];
                        }
                    }
                }
            }
            return defVal;
        }

        #region 公共方法

        //public void GetAllLightmapIndexes(ref SortedSet<int> dict)
        //{           
        //    long tag = 1;
        //    for (int protoIndex = 0; protoIndex < this.ProtoTypes.Count; ++protoIndex)
        //    {
        //        if (this.ProtoTypes[protoIndex].MeshCode != null)
        //        {
        //            for (int idx = 0; idx < this.ProtoTypes[protoIndex].MeshCode.Count; ++idx)
        //            {
        //                int num = this.ProtoTypes[protoIndex].LightMapNum[idx];
        //                if (num <= 0)
        //                {
        //                    continue;
        //                }
        //                //0-63
        //                long lmIndex = this.ProtoTypes[protoIndex].LightMapIndex[idx];
        //                for (int i = 0; i <= 63; ++i)
        //                {
        //                    long val = tag << i;
        //                    if ((lmIndex & val) == val)
        //                    {
        //                        dict.Add(i);
        //                        --num;
        //                    }
        //                    if (num <= 0)
        //                        break;
        //                }
        //                //64--127                            
        //                if ((num > 0) && this.ProtoTypes[protoIndex].LightMapIndex2 != null)
        //                {
        //                    lmIndex = this.ProtoTypes[protoIndex].LightMapIndex2[idx];
        //                    for (int i = 0; i <= 63; ++i)
        //                    {
        //                        long val = tag << i;
        //                        if ((lmIndex & val) == val)
        //                        {
        //                            dict.Add(64 + i);
        //                            --num;
        //                        }
        //                        if (num <= 0)
        //                            break;
        //                    }
        //                }

        //            }
        //        }
        //    }
        //}
        public bool GetLightMapIndexes(int protoIndex, long code, out List<int> outLightmapIndex)
        {
            long tag = 1;
            outLightmapIndex = new List<int>();
            if (protoIndex >= 0 && protoIndex < ProtoTypes.Count)
            {
                if (ProtoTypes[protoIndex].MeshCode != null)
                {
                    for (int idx = 0; idx < ProtoTypes[protoIndex].MeshCode.Count; ++idx)
                    {
                        if (ProtoTypes[protoIndex].MeshCode[idx] == code)
                        {
                            int num = ProtoTypes[protoIndex].LightMapNum[idx];
                            if (num <= 0)
                            {
                                outLightmapIndex = null;
                                return false;
                            }
                            //0-63
                            long lmIndex = ProtoTypes[protoIndex].LightMapIndex[idx];
                            for (int i = 0; i <= 63; ++i)
                            {
                                long val = tag << i;
                                if ((lmIndex & val) == val)
                                {
                                    outLightmapIndex.Add(i);
                                    --num;
                                }
                                if (num <= 0)
                                    break;
                            }
                            //64--127                            
                            if((num > 0) && ProtoTypes[protoIndex].LightMapIndex2 != null)
                            {
                                lmIndex = ProtoTypes[protoIndex].LightMapIndex2[idx];
                                for (int i = 0; i <= 63; ++i)
                                {
                                    long val = tag << i;
                                    if ((lmIndex & val) == val)
                                    {
                                        outLightmapIndex.Add(64 + i);
                                        --num;
                                    }
                                    if (num <= 0)
                                        break;
                                }
                            }
                            
                            //
                            return outLightmapIndex.Count > 0;
                        }
                    }
                }
            }
                               
            outLightmapIndex = null;
            return false;
        }

        public bool ExistLightMapData(int protoIndex,long code)
        {
            if (protoIndex >= 0 && protoIndex < ProtoTypes.Count)
            {
                if (ProtoTypes[protoIndex].MeshCode != null)
                {
                    for (int idx = 0; idx < ProtoTypes[protoIndex].MeshCode.Count; ++idx)
                    {
                        if (ProtoTypes[protoIndex].MeshCode[idx] == code)
                        {
                            int num = ProtoTypes[protoIndex].LightMapNum[idx];
                            return num > 0;
                        }
                    }
                }                         
            }
            return false;
        }

        public int GetLightMapNum(int protoIndex, long meshCode)
        {
            if (protoIndex >= 0 && protoIndex < ProtoTypes.Count)
            {
                if (ProtoTypes[protoIndex].MeshCode != null)
                {
                    for (int idx = 0; idx < ProtoTypes[protoIndex].MeshCode.Count; ++idx)
                    {
                        if (ProtoTypes[protoIndex].MeshCode[idx] == meshCode)
                        {
                            int num = ProtoTypes[protoIndex].LightMapNum[idx];
                            return num;
                        }
                    }
                }
            }
            return 0;
        }

       
        public int GetAllChuckNum()
        {
            int prototypeCount = ProtoTypes.Count;
            
            int num = 0;
            for (int i = 0; i < prototypeCount; ++i)
            {
                num += ProtoTypes[i].ChuckNum;
            }
            ////     
            if(prototypeCount == 0 && this.ProtoTypeChunkNum != null)
            {
                prototypeCount = this.ProtoTypeChunkNum.Count;
                for (int i = 0; i < prototypeCount; ++i)
                {
                    num += this.ProtoTypeChunkNum[i];
                }
            }
            return num;
        }
             
        public int ChunkDict_FindKey(string key)
        {
            for(int i = 0; i < ProtoTypes.Count; ++i)
            {
                if(ProtoTypes[i].ProtoKey.Equals(key))               
                {
                    return i;
                }
            }
            return -1;
        }

        
        public int GrassDict_FindKey(string key)
        {
            for (int i = 0; i < ProtoTypes.Count; ++i)
            {
                if (ProtoTypes[i].ProtoKey.Equals(key))
                {
                    return i;
                }
            }
            return -1;
        }


        public int ChunkDict_FindAndNew(string key)
        {
            int protoTypeIdx = ChunkDict_FindKey(key);
            if (protoTypeIdx < 0)
            {
                ProtoTypeData newProto = new ProtoTypeData();
                newProto.ProtoKey = key;
                newProto.ChunkPrebab = "";
                ProtoTypes.Add(newProto);               
                protoTypeIdx = ProtoTypes.Count - 1;
               
            }
            return protoTypeIdx;
        }

        public int GrassDict_FindAndNew(string key)
        {
            int protoTypeIdx = GrassDict_FindKey(key);
            if (protoTypeIdx < 0)
            {
                ProtoTypeData newProto = new ProtoTypeData();
                newProto.ProtoKey = key;
                newProto.GrassPrebab = null;
                ProtoTypes.Add(newProto);
                protoTypeIdx = ProtoTypes.Count - 1;
                
            }
            return protoTypeIdx;
        }
        #endregion

        public void Load(GameObject root)
        {

        }

        public void Clear()
        {
            if(ChunkData != null)
                ChunkData.Clear();
            if(ProtoTypes != null)
                ProtoTypes.Clear();
            if(Codes != null)
                Codes.Clear();
            if(CodeMD5 != null)
                CodeMD5.Clear();
            if(ProtoTypeChunkNum != null)
                ProtoTypeChunkNum.Clear();
    }

}
}
