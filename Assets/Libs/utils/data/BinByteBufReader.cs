using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public static class DataUtils
{
    public static bool AssertTrue(bool asset)
    {
        if (!asset)Log.LogError("AssertTrue Data Error");
        return asset;
    }
   
}

namespace MyProtobuf
{
    public class BinByteBufReader : IDisposable
    {
        private byte[] buffer;
        private Int32 iBufSize;
        private Int32 iCurrentPos;
        private Int32 iStartPos;

        public BinByteBufReader()
        {
            buffer = null;
            iBufSize = 0;
            iCurrentPos = 0;
            iStartPos = 0;
        }

        public void Init(byte[] bufferNew, Int32 iBufSizeNew, Int32 iReadPosFromRaw)
        {
            buffer = bufferNew;
            iBufSize = iBufSizeNew;
            iCurrentPos = iReadPosFromRaw;
            iStartPos = iReadPosFromRaw;
        }

        public byte[] GetBuffer()
        {
            return buffer;
        }

        public bool ReadBytes(byte[] bytes, Int32 iLen)
        {
            int iLenLeft = iBufSize - iCurrentPos;
            if (!DataUtils.AssertTrue(iLen >= 0)) return false;
            if (!DataUtils.AssertTrue(iLen <= bytes.Length)) return false;

            if (!DataUtils.AssertTrue(iLenLeft >= iLen)) return false;

            for (Int32 k = 0; k < iLen; k++)
            {
                bytes[k] = buffer[iCurrentPos++];
            }
            return true;
        }

        public bool ReadBool(ref bool val)
        {
            val = false;
            byte by = 0;
            if (!ReadByte(ref by))
                return false;
            val = (by != 0);
            return true;
        }
        public bool ReadByte(ref byte val)
        {
            int iLenLeft = iBufSize - iCurrentPos;
            if (!DataUtils.AssertTrue(iLenLeft >= 1)) return false;

            val = buffer[iCurrentPos++];

            return true;
        }

        public byte ReadByte()
        {
            byte value = 0;
            if (ReadByte(ref value))
                return value;
            return 0;
        }

        public Int32 GetCurrentPos()
        {
            return iCurrentPos;
        }

        public bool ReadString(ref string val)
        {
            val = "";
            int len = 0;
            if (!ReadInt32(ref len))
                return false;

            if (!DataUtils.AssertTrue(iCurrentPos + len <= GetBuffer().Length)) return false;

            UTF8Encoding decoding = new UTF8Encoding();
            val = decoding.GetString(GetBuffer(), iCurrentPos, len);

            iCurrentPos += len;
            return true;
        }

        public string ReadString()
        {
            string value = "";
            if (ReadString(ref value))
                return value;
            return "";
        }

        public Int32 GetDataLeft()
        {
            if (null == buffer)
                return 0;
            if (!DataUtils.AssertTrue(iCurrentPos <= iBufSize)) return 0;
            return iBufSize - iCurrentPos;
        }

        public bool ReadInt16(ref Int16 val)
        {
            Int32 iBufLeft = GetDataLeft();
            if (!DataUtils.AssertTrue(iBufLeft > 0)) return false;

            Int32 i = 0;

            if (!DataUtils.AssertTrue(iBufLeft >= sizeof(Int16))) return false;
            byte[] pbDest = new byte[sizeof(Int16)];
            for (i = 0; i < sizeof(Int16); i++)
            {
                pbDest[i] = buffer[iCurrentPos++];
            }
            val = BitConverter.ToInt16(pbDest, 0);

            return true;

        }

        public Int16 ReadInt16()
        {
            Int16 value = 0;
            if (ReadInt16(ref value))
                return value;
            return 0;
        }

        public bool ReadUInt16(ref UInt16 val)
        {
            Int32 iBufLeft = GetDataLeft();
            if (!DataUtils.AssertTrue(iBufLeft > 0)) return false;

            Int32 i = 0;

            if (!DataUtils.AssertTrue(iBufLeft >= sizeof(UInt16))) return false;

            byte[] pbDest = new byte[sizeof(UInt16)];
            for (i = 0; i < sizeof(UInt16); i++)
            {
                pbDest[i] = buffer[iCurrentPos++];
            }

            val = BitConverter.ToUInt16(pbDest, 0);

            return true;

        }

        public UInt16 ReadUInt16()
        {
            UInt16 value = 0;
            if (ReadUInt16(ref value))
                return value;
            return 0;
        }

        public bool ReadInt32(ref Int32 val)
        {
            Int32 iBufLeft = GetDataLeft();
            if (!DataUtils.AssertTrue(iBufLeft > 0)) return false;

            Int32 i = 0;

            if (!DataUtils.AssertTrue(iBufLeft >= sizeof(Int32))) return false;
            byte[] pbDest = new byte[sizeof(Int32)];
            for (i = 0; i < sizeof(Int32); i++)
            {
                pbDest[i] = buffer[iCurrentPos++];
            }
            val = BitConverter.ToInt32(pbDest, 0);

            return true;
        }

        public Int32 ReadInt32()
        {
            Int32 value = 0;
            if (ReadInt32(ref value))
                return value;
            return 0;
        }

        public bool ReadInt64(ref Int64 val)
        {
            Int32 iBufLeft = GetDataLeft();
            if (!DataUtils.AssertTrue(iBufLeft > 0)) return false;

            Int32 i = 0;

            if (!DataUtils.AssertTrue(iBufLeft >= sizeof(Int64))) return false;

            byte[] pbDest = new byte[sizeof(Int64)];
            for (i = 0; i < sizeof(Int64); i++)
            {
                pbDest[i] = buffer[iCurrentPos++];
            }
            val = BitConverter.ToInt64(pbDest, 0);

            return true;
        }

        public Int64 ReadInt64()
        {
            Int64 value = 0;
            if (ReadInt64(ref value))
                return value;
            return 0;
        }

        public bool ReadUInt64(ref UInt64 val)
        {
            Int32 iBufLeft = GetDataLeft();
            if (!DataUtils.AssertTrue(iBufLeft > 0)) return false;

            Int32 i = 0;

            if (!DataUtils.AssertTrue(iBufLeft >= sizeof(UInt64))) return false;
            byte[] pbDest = new byte[sizeof(UInt64)];
            for (i = 0; i < sizeof(UInt64); i++)
            {
                pbDest[i] = buffer[iCurrentPos++];
            }
            val = BitConverter.ToUInt64(pbDest, 0);

            return true;
        }

        public UInt64 ReadUInt64()
        {
            UInt64 value = 0;
            if (ReadUInt64(ref value))
                return value;
            return 0;
        }

        public bool ReadUInt32(ref UInt32 val)
        {
            Int32 iBufLeft = GetDataLeft();
            if (!DataUtils.AssertTrue(iBufLeft > 0)) return false;

            Int32 i = 0;

            if (!DataUtils.AssertTrue(iBufLeft >= sizeof(UInt32))) return false;
            byte[] pbDest = new byte[sizeof(UInt32)];
            for (i = 0; i < sizeof(Int32); i++)
            {
                pbDest[i] = buffer[iCurrentPos++];
            }
            val = BitConverter.ToUInt32(pbDest, 0);

            return true;
        }

        public UInt32 ReadUInt32()
        {
            UInt32 value = 0;
            if (ReadUInt32(ref value))
                return value;
            return 0;
        }

        public bool ReadFloat(ref float val)
        {
            Int32 iBufLeft = GetDataLeft();
            if (!DataUtils.AssertTrue(iBufLeft > 0)) return false;

            Int32 i = 0;

            if (!DataUtils.AssertTrue(iBufLeft >= sizeof(float))) return false;
            byte[] pbDest = new byte[sizeof(float)];
            for (i = 0; i < sizeof(float); i++)
            {
                pbDest[i] = buffer[iCurrentPos++];
            }
            val = BitConverter.ToSingle(pbDest, 0);

            return true;
        }

        public float ReadFloat()
        {
            float value = 0;
            if (ReadFloat(ref value))
                return value;
            return 0f;
        }

        public Int32 GetReadPos()
        {
            return iCurrentPos;
        }


        public void Dispose()
        {
            buffer = null;
            iBufSize = 0;
            iCurrentPos = 0;
            iStartPos = 0;
        }
    }
}

