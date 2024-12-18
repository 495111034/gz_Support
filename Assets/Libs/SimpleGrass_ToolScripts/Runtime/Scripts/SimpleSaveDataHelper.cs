using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace SimpleGrass{


    public class SimpleSaveDataHelper
    {

        #region 生成实现数据BUFF

        static public bool BuildDataBuffer(SimpleSaveData saveData)
        {
            if(saveData.Ver < Common.Ver_6_ChunkDataBuff)
            {
                return false;
            }
            List<byte> prototypeModeList = _GetPrototypeModeArray(saveData.ProtoTypes);
            MMO_MemoryStream stream = new MMO_MemoryStream();
            //ushort: chunkNum
            _WriteChunkNum(stream, (ushort)saveData.ChunkData.Count);
            for (int index = 0; index < saveData.ChunkData.Count; ++index)
            {
                int prototypeIndex = saveData.ChunkData[index].PrototypeIndex;
                byte _protoMode = (byte)prototypeModeList[prototypeIndex];
                bool useLightProb = saveData.ProtoTypes[prototypeIndex].UseLightProb;

                _WriteChunkData(stream, _protoMode, useLightProb, saveData.ChunkData[index]);
            }
            if (saveData.ChunkData != null)
            {
                saveData.ChunkData.Clear();
            }
            saveData.ChunkDataBuff = stream.ToArray();

            stream.Dispose();
            return true;
        }

        static List<byte> _GetPrototypeModeArray(List<ProtoTypeData> prototypes)
        {
            List<byte> ret = new List<byte>();
            for (int i = 0; i < prototypes.Count; i++)
            {
                byte _mode = 0;//BatchMode

                //扫描的预制体原型时                
                bool isCustomTrue = prototypes[i].ProtoKey.StartsWith(Common.CustomPrefix_Prefab);
                 if (prototypes[i].GrassPrebab)
                {
                    LODGroup lod = prototypes[i].GrassPrebab.GetComponent<LODGroup>();
                    if (lod != null)
                    {
                        _mode = 1;//SingleMode
                         //批量使用LOD的模型
                        if (!isCustomTrue && prototypes[i].Tag == (int)Common.ProtoMode.BatchMode_LOD)
                        {
                            _mode = 2;// BatchMode_LOD
                        }
                    }
                }else
                {
                    //扫描的数据：获取模式
                    if (prototypes[i].Custom != null)
                    {
                        int imode = Common.GetCustomPrototypeModeByScan(prototypes[i].ProtoKey);
                        _mode = (Byte)Math.Max(0, imode);                        
                    }                                        
                }

                ret.Add(_mode);
            }
            return ret;
        }

        static void _WriteChunkNum(MMO_MemoryStream stream, ushort chunkNum)
        {
            stream.WriteUShort(chunkNum);//0 ~ 65,535
        }
        static int _ReadChunkNum(MMO_MemoryStream stream)
        {
            ushort chunkNum = stream.ReadUShort();
            return chunkNum;
        }

        static void _WriteChunkBaseData(MMO_MemoryStream stream, byte protoMode, InstanceChunkData chunk)
        {
            stream.WriteByte((byte)chunk.PrototypeIndex);//0 ~ 255
            stream.WriteByte((byte)protoMode);

            stream.WriteFloat(chunk.CullingCollider[0]);
            stream.WriteFloat(chunk.CullingCollider[1]);
            stream.WriteFloat(chunk.CullingCollider[2]);
            stream.WriteFloat(chunk.CullingCollider[3]);

            stream.WriteFloat(chunk.maxScale);
        }
        static int _ReadChunkBaseData(MMO_MemoryStream stream,InstanceChunkData chunk)
        {
            chunk.PrototypeIndex = (int)stream.ReadByte();//0 ~ 255
            int protoMode = (int)stream.ReadByte();

            chunk.CullingCollider[0] = stream.ReadFloat();
            chunk.CullingCollider[1] = stream.ReadFloat();
            chunk.CullingCollider[2] = stream.ReadFloat();
            chunk.CullingCollider[3] = stream.ReadFloat();

            chunk.maxScale = stream.ReadFloat();

            return protoMode;
        }

        static void _WriteChunkInstData(MMO_MemoryStream stream, byte protoMode, bool lightProbOcc, InstanceChunkData chunk)
        {
            ushort num = (ushort)chunk.InstanceList.Count;//0~65,535
            stream.WriteUShort(num);
            for (int index = 0; index < num; ++index)
            {
                InstData inst = chunk.InstanceList[index];
                //SingleMode
                if (protoMode == 1)
                {
                    stream.WriteVecter3(inst.pos);
                    stream.WriteFloat(inst.maxScale);

                    stream.WriteVecter3(inst.Collider[0]);
                    stream.WriteVecter3(inst.Collider[1]);
                }
                //lightProbOcc
                if (lightProbOcc)
                {
                    stream.WriteFloat(inst.lightProbOcc);
                }

                ushort dataNum = (ushort)inst.meshCodes.Count;
                stream.WriteUShort(dataNum);
                for (int i = 0; i < inst.meshCodes.Count; i++)
                {
                    stream.WriteInt((int)inst.meshCodes[i]);

                    List<Vector3> trs = inst.datas[i].TRS;
                    List<Vector4> LSO = inst.datas[i].LSO;

                    //trs
                    int _num = (trs.Count);
                    stream.WriteByte((byte)_num);
                    for (int itrs = 0; itrs < _num; ++itrs)
                    {
                        stream.WriteVecter3(trs[itrs]);
                    }

                    //lso
                    _num = LSO.Count;
                    stream.WriteByte((byte)_num);
                    for (int ilso = 0; ilso < _num; ++ilso)
                    {
                        stream.WriteVecter4(LSO[ilso]);
                    }
                }
            }
        }
        
        static void _WriteChunkData(MMO_MemoryStream stream, byte protoMode, bool lightProbOcc, InstanceChunkData chunk)
        {
            _WriteChunkBaseData(stream, protoMode, chunk);
            _WriteChunkInstData(stream, protoMode, lightProbOcc, chunk);
        }

        #endregion

        #region 加载数据BUFF
        /// <summary>
        /// 整理每个数据块中实例的数据(块信息、实例矩阵、实例光照信息）
        /// </summary>        
        static public bool LoadInstGroupDataByBuffer_Thread(SimpleInstancingMgr instancingMg, SimpleSaveData savedProfile,
            ref SimpleInstancingGroup[] instancingGroupAry, ref List<SimpleInstancingMgr.GroupSortData> instancingGroupSort,
            ref int TestGroupNum, ref int TestInstanceNum, ref int TestMeshNum,ref Dictionary<int, int> TestInstNum_ByBuff, int beginProtypeIndex)
        {                      
            try
            {
                MMO_MemoryStream stream = new MMO_MemoryStream(savedProfile.ChunkDataBuff);
                //所有块的数量
                int chunkNum = SimpleSaveDataHelper._ReadChunkNum(stream);
                instancingGroupAry = new SimpleInstancingGroup[chunkNum];
                instancingGroupSort = new List<SimpleInstancingMgr.GroupSortData>();
                ///初始化，裁剪
                int prototypeIndex = -1;
                int tmpIndex = -1;
                //
                TestInstNum_ByBuff.Clear();
                TestGroupNum = chunkNum;
                TestInstanceNum = 0;
                TestMeshNum = 0;
                bool isSingleMode = false;
                bool useLightProbe = false;
                Common.ProtoMode protoMode = Common.ProtoMode.BatchMode;
                InstanceChunkData chunk = new InstanceChunkData();
                for (int i = 0; i < chunkNum; ++i)
                {
                    //read group data
                    int protoMode_int = _ReadChunkBaseData(stream, chunk);

                    prototypeIndex = beginProtypeIndex + chunk.PrototypeIndex;
                    if (tmpIndex != prototypeIndex)
                    {
                        tmpIndex = prototypeIndex;

                        SimpleInstancingMgr.PrototypeInfo protoInfo = instancingMg.prototypeList[prototypeIndex];
                        isSingleMode = protoInfo.isSingleMode;
                        protoMode = protoInfo.mode;
                        useLightProbe = protoInfo.UseLightProb;
                    }
                    SimpleInstancingGroup group = _BuildGroupData(instancingMg, chunk, i, protoMode, ref instancingGroupAry, ref instancingGroupSort);

                    int oldNum = Common.TestInstanceNum;
                    //LODGroup模式, 一个实例对应一个SimpleInstance，解析矩阵及光照信息
                    if (isSingleMode)
                    {
                        group.IsSingleMode = true;
                        group.protoMode = protoMode;
                        #region LODGroup模式
                        //_BuildOneGroupData_SingleMode(ref group, stream, ref TestInstanceNum, ref TestMeshNum, useLightProbe, protoMode_int);
                        #endregion LODGroup模式
                    }
                    //////////////////批量模式
                    else
                    {
                        group.protoMode = protoMode;
                        #region 批量模式
                        _BuildOnGroupData_BatchMode(ref group, stream, ref TestInstanceNum, ref TestMeshNum, useLightProbe, protoMode_int);
                        #endregion 批量模式
                    }

                    int newNum = TestInstanceNum - oldNum;
                    int inum = 0;
                    if (! TestInstNum_ByBuff.TryGetValue(prototypeIndex,out inum))
                    {
                        TestInstNum_ByBuff.Add(prototypeIndex, 0);
                    }
                    TestInstNum_ByBuff[prototypeIndex] += newNum;

                }
                
                stream.Dispose();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            return false;
        }

        static private SimpleInstancingGroup _BuildGroupData(SimpleInstancingMgr instancingMg, InstanceChunkData chunk,int index, Common.ProtoMode protoMode,
            ref SimpleInstancingGroup[] instancingGroupAry, ref List<SimpleInstancingMgr.GroupSortData> instancingGroupSort)
        {
            int prototypeIndex = chunk.PrototypeIndex;

            SimpleInstancingGroup group = new SimpleInstancingGroup(instancingMg);
            group.PrototypeIndex = prototypeIndex;
            Vector3 center = new Vector3(chunk.CullingCollider[0], chunk.CullingCollider[1], chunk.CullingCollider[2]);

            group.worldPosition = center;
            group.MaxScale = chunk.maxScale;

            //创建每块的裁剪使用的球
            UnityEngine.BoundingSphere bounding = new UnityEngine.BoundingSphere();
            bounding.position = center;
            bounding.radius = chunk.CullingCollider[3];
            group.BoundingSpere = bounding;

            instancingGroupAry[index] = group;
            if (Common.UseSort)
            {
                SimpleInstancingMgr.GroupSortData sortData = new SimpleInstancingMgr.GroupSortData();
                sortData.index = index;
                sortData.dist = 0;
                sortData.isVisible = true;
                instancingGroupSort.Add(sortData);
            }

            //增加group裁剪球体
            //test AddCullingBoundingSphere(group);

            //加载块内数据   
            //新增BatchModeLOD
            if (protoMode == Common.ProtoMode.BatchMode_LOD)
            {
                group.IsSingleMode = false;
                group.protoMode = protoMode;
            }

            return group;
        }

        //static public bool _BuildOneGroupData_SingleMode(ref SimpleInstancingGroup group, MMO_MemoryStream stream, 
        //    ref int TestInstanceNum, ref int TestMeshNum, bool useLightProbe,int protoMode)
        //{
        //    Debug.Log("+++++++_BuildOneGroupData_SingleMode+");
        //    {
        //        List<Matrix4x4> tmpMat = new List<Matrix4x4>();
        //        List<Vector4> tmpLSOList = new List<Vector4>();
        //        List<float> tmpLightProbeOccList = new List<float>();

        //        int instNum = (int)stream.ReadUShort();
        //       // Common.TestInstanceNum += instNum;
        //        TestInstanceNum += instNum;
        //        for (int instIdx = 0; instIdx < instNum; ++instIdx)
        //        {
        //            SimpleInstance inst = new SimpleInstance();
        //            //SingleMode
        //            if (protoMode == 1)
        //            {
        //                inst.pos = stream.ReadVecter3();

        //                //LODGROUP相关数据
        //                inst.maxScale = stream.ReadFloat();
        //                inst.meshBounds.center = stream.ReadVecter3();
        //                inst.meshBounds.size = stream.ReadVecter3();
        //            }

        //            float lightProbeOcc = 0.0f;
        //            if (useLightProbe)
        //            {
        //                lightProbeOcc = stream.ReadFloat();
        //            }

        //            int dataNum = (int)stream.ReadUShort();                    
        //            for (int codeIdx = 0; codeIdx < dataNum; ++codeIdx)
        //            {
        //                long code = (long)stream.ReadInt();
        //                //trs                        
        //                int vCount =  (int)stream.ReadByte();
                        
        //                //实例的世界矩阵信息
        //                tmpMat.Clear();
        //                tmpLSOList.Clear();
        //                tmpLightProbeOccList.Clear();
        //                vCount = vCount / 3;
        //                //Common.TestMeshNum += vCount;
        //                TestMeshNum += vCount;
        //                for (int trsIdx = 0; trsIdx < vCount; ++trsIdx)
        //                {
        //                    Vector3 t = stream.ReadVecter3();
        //                    Vector3 r = stream.ReadVecter3();
        //                    Vector3 s = stream.ReadVecter3();

        //                    //考虑直接保存在profile中
        //                    Matrix4x4 mat = new Matrix4x4();
        //                    Quaternion q = Quaternion.Euler(r);
        //                    mat.SetTRS(t, q, s);
        //                    tmpMat.Add(mat);
        //                }
        //                //
        //                // if (useLightmap)
        //                if (!useLightProbe)
        //                {
        //                    //light lso                                                                           
        //                    int count = (int)stream.ReadByte();                            
        //                    for (int idx = 0; idx < count; ++idx)
        //                    {
        //                        Vector4 lso = stream.ReadVecter4();
        //                        tmpLSOList.Add(lso);
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

        static public bool _BuildOnGroupData_BatchMode(ref SimpleInstancingGroup group, MMO_MemoryStream stream,
            ref int TestInstanceNum, ref int TestMeshNum, bool useLightProbe, int protoMode)
        {
            if (protoMode == 1) //SingleMode
            {
                return false;
            }
             //if (instList != null)
            {
                List<Matrix4x4> tmpMat = new List<Matrix4x4>();
                List<Vector4> tmpLSOList = new List<Vector4>();
                Dictionary<long, List<Matrix4x4>> objTmpTRS = new Dictionary<long, List<Matrix4x4>>();
                Dictionary<long, List<Vector4>> objTmpLSO = new Dictionary<long, List<Vector4>>();
                Dictionary<long, List<float>> objTmpLightProbeOcc = new Dictionary<long, List<float>>();

                //Common.TestInstanceNum += instList.Count;
                int instNum = (int)stream.ReadUShort();
                TestInstanceNum += instNum;
                group.VisibleInstNum = instNum;
                group.OrgInstNum = instNum;
               
                for (int instIdx = 0; instIdx < instNum; ++instIdx)
                {
                    float lightProbeOcc = 0.0f;
                    if (useLightProbe)
                    {
                        lightProbeOcc = stream.ReadFloat();
                    }
                    int dataNum = (int)stream.ReadUShort();
                    for (int codeIdx = 0; codeIdx < dataNum; ++codeIdx)
                    {
                        long code = (long)stream.ReadInt();
                        //trs                        
                        int vCount = (int)stream.ReadByte();

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
                            Vector3 t = stream.ReadVecter3();
                            Vector3 r = stream.ReadVecter3();
                            Vector3 s = stream.ReadVecter3();
                            //考虑直接保存在profile中
                            Matrix4x4 mat = new Matrix4x4();
                            Quaternion q = Quaternion.Euler(r);
                            mat.SetTRS(t, q, s);
                            outMat.Add(mat);
                        }
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
                            int count = (int)stream.ReadByte();
                            for (int idx = 0; idx < count; ++idx)
                            {
                                Vector4 lso = stream.ReadVecter4();
                                outLSOList.Add(lso);
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
    }

    public class MMO_MemoryStream : MemoryStream
    {
        public MMO_MemoryStream()
        { 
        }

        public MMO_MemoryStream(byte[] buffer) : base(buffer)
        {

        }

        #region Short

        public short ReadShort()
        {
            byte[] arr = new byte[2];

            base.Read(arr, 0, arr.Length);

            return BitConverter.ToInt16(arr, 0);
        }

        public void WriteShort(short value)
        {
            byte[] arr = BitConverter.GetBytes(value);

            base.Write(arr, 0, arr.Length);
        }

        #endregion

        #region UShort

        public ushort ReadUShort()
        {
            byte[] arr = new byte[2];

            base.Read(arr, 0, arr.Length);

            return BitConverter.ToUInt16(arr, 0);
        }

        public void WriteUShort(ushort value)
        {
            byte[] arr = BitConverter.GetBytes(value);

            base.Write(arr, 0, arr.Length);
        }

        #endregion

        #region Int

        public int ReadInt()
        {
            byte[] arr = new byte[4];

            base.Read(arr, 0, arr.Length);

            return BitConverter.ToInt32(arr, 0);
        }

        public void WriteInt(int value)
        {
            byte[] arr = BitConverter.GetBytes(value);

            base.Write(arr, 0, arr.Length);
        }

        #endregion

        #region UInt

        public uint ReadUInt()
        {
            byte[] arr = new byte[4];

            base.Read(arr, 0, arr.Length);

            return BitConverter.ToUInt32(arr, 0);
        }

        public void WriteUInt(uint value)
        {
            byte[] arr = BitConverter.GetBytes(value);

            base.Write(arr, 0, arr.Length);
        }

        #endregion

        #region Float

        public float ReadFloat()
        {
            byte[] arr = new byte[4];

            base.Read(arr, 0, arr.Length);

            return BitConverter.ToSingle(arr, 0);
        }

        public void WriteFloat(float value)
        {
            byte[] arr = BitConverter.GetBytes(value);

            base.Write(arr, 0, arr.Length);
        }

        #endregion

        #region Double

        public double ReadDouble()
        {
            byte[] arr = new byte[8];

            base.Read(arr, 0, arr.Length);

            return BitConverter.ToDouble(arr, 0);
        }

        public void WriteDouble(double value)
        {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }

        #endregion

        #region Bool

        public bool ReadBool()
        {
            return base.ReadByte() == 1;
        }

        public void WriteBool(bool value)
        {
            base.WriteByte((byte)(value == true ? 1 : 0));
        }

        #endregion

        #region String

        public string ReadString()
        {
            ushort len = this.ReadUShort();

            byte[] arr = new byte[len];

            base.Read(arr, 0, len);

            return Encoding.UTF8.GetString(arr, 0, len);
        }

        public void WriteString(string value)
        {
            byte[] arr = Encoding.UTF8.GetBytes(value);

            if (arr.Length > 65535)
            {
                throw new Exception("长度超出范围");
            }

            this.WriteUShort((ushort)value.Length);

            base.Write(arr, 0, arr.Length);

        }
        #endregion

        #region Vecter3

        public Vector3 ReadVecter3()
        {
            Vector3 ret = Vector3.zero;
            byte[] arr = new byte[4];

            base.Read(arr, 0, arr.Length);
            ret.x = BitConverter.ToSingle(arr, 0);

            base.Read(arr, 0, arr.Length);
            ret.y = BitConverter.ToSingle(arr, 0);

            base.Read(arr, 0, arr.Length);
            ret.z = BitConverter.ToSingle(arr, 0);

            return ret;
        }

        public void WriteVecter3(Vector3 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
        }

        public Vector4 ReadVecter4()
        {
            Vector4 ret = Vector4.zero;
            byte[] arr = new byte[4];

            base.Read(arr, 0, arr.Length);
            ret.x = BitConverter.ToSingle(arr, 0);

            base.Read(arr, 0, arr.Length);
            ret.y = BitConverter.ToSingle(arr, 0);

            base.Read(arr, 0, arr.Length);
            ret.z = BitConverter.ToSingle(arr, 0);

            base.Read(arr, 0, arr.Length);
            ret.w = BitConverter.ToSingle(arr, 0);

            return ret;
        }

        public void WriteVecter4(Vector4 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
            WriteFloat(value.w);
        }
        #endregion


}

}
