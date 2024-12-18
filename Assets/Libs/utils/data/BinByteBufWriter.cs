using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MyProtobuf
{
    public class BinByteBufWriter : IDisposable
    {
        private byte[] bufferAlloc;
        private Int32 iWritePos;
        private Int32 iBufSize;
        private Int32 iStartPos;

        public const int InitBufferSize = 64;


        public BinByteBufWriter()
        {
            bufferAlloc = new byte[InitBufferSize];
            iWritePos = 0;
            iStartPos = 0;
            iBufSize = InitBufferSize;
            int k = 0;
            for (k = 0; k < bufferAlloc.Length; ++k)
            {
                bufferAlloc[k] = 0;
            }
        }

        public Int32 GetLength()
        {
            return iWritePos - iStartPos;
        }

        /// <summary>
        /// 检查或增加大小
        /// </summary>
        /// <param name="iNewSize"></param>
        /// <returns></returns>
        public bool ReAllocBuf(Int32 iNewSize)
        {
            if (bufferAlloc == null)
            {
                bufferAlloc = new byte[iNewSize];
            }
            //UnityEngine.Debug.Log("iWritePos= " + iWritePos + ",bufferAlloc.Length=" + bufferAlloc.Length + ",iNewSize=" + iNewSize);
            if (GetBuffer().Length >= iNewSize)
            {
                return true;
            }
            else
            {
                iBufSize = iNewSize;
                byte[] bufferOld = bufferAlloc;
                byte[] bufferNew = new byte[iNewSize];
                int k = 0;
                for (k = 0; k < bufferOld.Length; k++)
                {
                    bufferNew[k] = bufferOld[k];
                }
                for (; k < bufferNew.Length; k++)
                {
                    bufferNew[k] = 0;
                }
                bufferAlloc = bufferNew;
                bufferOld = null;
            }

            return true;
        }

        public bool WriteUInt32(UInt32 val)
        {
            int iWritePosNew = iWritePos + sizeof(UInt32);
            if (!ReAllocBuf(iWritePosNew)) return false;

            int i = 0;

            byte[] bufReal = GetBuffer();
            byte[] a = System.BitConverter.GetBytes(val);
            // Array.Reverse(a);
            for (i = 0; i < sizeof(UInt32); i++)
            {
                bufReal[iWritePos++] = a[i];
            }

            return true;

        }

        public bool WriteInt32(Int32 val)
        {
            int iWritePosNew = iWritePos + sizeof(Int32);
            if (!ReAllocBuf(iWritePosNew)) return false;

            int i = 0;

            byte[] bufReal = GetBuffer();
            byte[] a = System.BitConverter.GetBytes(val);
            // Array.Reverse(a);
            for (i = 0; i < sizeof(Int32); i++)
            {
                bufReal[iWritePos++] = a[i];
            }

            return true;

        }

        public bool WriteInt64(Int64 val)
        {

            int iWritePosNew = iWritePos + sizeof(Int64);
            if (!ReAllocBuf(iWritePosNew)) return false;

            Int32 i = 0;
            byte[] bufReal = GetBuffer();
            byte[] a = System.BitConverter.GetBytes(val);
            //  Array.Reverse(a);
            for (i = 0; i < sizeof(Int64); i++)
            {
                bufReal[iWritePos++] = a[i];
            }
            return true;
        }

        public bool WriteUInt64(UInt64 val)
        {

            int iWritePosNew = iWritePos + sizeof(UInt64);
            if (!ReAllocBuf(iWritePosNew)) return false;

            int i = 0;
            byte[] bufReal = GetBuffer();
            byte[] pbSrc = System.BitConverter.GetBytes(val);
            // Array.Reverse(pbSrc);
            for (i = 0; i < sizeof(UInt64); i++)
            {
                bufReal[iWritePos++] = pbSrc[i];
            }
            return true;
        }

        public bool WriteUInt16(UInt16 val)
        {
            int iWritePosNew = iWritePos + sizeof(UInt16);
            if (!ReAllocBuf(iWritePosNew)) return false;

            int i = 0;
            byte[] bufReal = GetBuffer();
            byte[] pbSrc = System.BitConverter.GetBytes(val);
            // Array.Reverse(pbSrc);
            for (i = 0; i < sizeof(UInt16); i++)
            {
                bufReal[iWritePos++] = pbSrc[i];
            }
            return true;
        }

        public bool WriteInt16(Int16 val)
        {
            int iWritePosNew = iWritePos + sizeof(Int16);
            if (!ReAllocBuf(iWritePosNew)) return false;

            int i = 0;

            byte[] bufReal = GetBuffer();
            byte[] pbSrc = System.BitConverter.GetBytes(val);
            //  Array.Reverse(pbSrc);
            for (i = 0; i < sizeof(Int16); i++)
            {
                bufReal[iWritePos++] = pbSrc[i];
            }

            return true;
        }

        public bool WriteBytes(byte[] bytes, int length = 0)
        {
            int len = bytes.Length;
            if (length > 0 && length <= bytes.Length)
                len = length;
            Int32 iWritePosNew = GetWritePos() + len + sizeof(UInt32);
            if (!ReAllocBuf(iWritePosNew)) return false;

            byte[] bufReal = GetBuffer();

            int i = 0;

            byte[] pbSrc = System.BitConverter.GetBytes(len);
            // Array.Reverse(pbSrc);
            for (i = 0; i < sizeof(UInt32); i++)
            {
                bufReal[iWritePos++] = pbSrc[i];
            }

            for (Int32 k = 0; k < len; k++)
            {
                bufReal[iWritePos++] = bytes[k];
            }
            return true;
        }

        public bool WriteBool(bool val)
        {
            byte by = 0;
            if (val) by = 1;
            return WriteByte(by);
        }

        public bool WriteByte(byte val)
        {
            Int32 iWritePosNew = GetWritePos() + 1;
            if (!ReAllocBuf(iWritePosNew)) return false;

            byte[] bufReal = GetBuffer();
            bufReal[iWritePos++] = val;
            return true;
        }

        public Int32 GetWritePos()
        {
            return iWritePos;
        }

        public bool WriteString(string val)
        {

            byte[] bytes = Encoding.UTF8.GetBytes(val);
            if (!ReAllocBuf(GetWritePos() + sizeof(UInt32) + bytes.Length)) return false;

            Int32 iTotalLen = sizeof(UInt32) + bytes.Length;
            int i = 0;
            if (WriteUInt32((UInt32)(bytes.Length)))
            {
                byte[] bufReal = GetBuffer();

                for (i = 0; i < bytes.Length; ++i)
                {
                    bufReal[iWritePos++] = bytes[i];
                }
            }

            return true;
        }

        public byte[] GetBuffer()
        {
            return bufferAlloc;
        }

        public bool SetUInt32(int pos, UInt32 val)
        {
            Int32 iWritePosNew = pos + sizeof(UInt32);
            if (!DataUtils.AssertTrue(iWritePosNew <= iBufSize))
            {
                return false;
            }
            int i = 0;
            byte[] bufReal = GetBuffer();
            byte[] pbSrc = System.BitConverter.GetBytes(val);
            //  Array.Reverse(pbSrc);        

            for (i = 0; i < sizeof(UInt32); i++)
            {
                bufReal[pos + i] = pbSrc[i];
            }

            return true;
        }

        public bool SetInt32(int pos, Int32 val)
        {
            Int32 iWritePosNew = pos + sizeof(Int32);
            if (!DataUtils.AssertTrue(iWritePosNew <= iBufSize))
            {
                return false;
            }
            int i = 0;
            byte[] bufReal = GetBuffer();
            byte[] pbSrc = System.BitConverter.GetBytes(val);
            //  Array.Reverse(pbSrc);

            for (i = 0; i < sizeof(Int32); i++)
            {
                bufReal[pos + i] = pbSrc[i];
            }

            return true;
        }

        public bool SetUInt16(int pos, UInt16 val)
        {
            Int32 iWritePosNew = pos + sizeof(UInt16);
            if (!DataUtils.AssertTrue(iWritePosNew <= iBufSize))
            {
                return false;
            }
            int i = 0;
            byte[] bufReal = GetBuffer();

            byte[] pbSrc = System.BitConverter.GetBytes(val);
            //   Array.Reverse(pbSrc);
            for (i = 0; i < sizeof(UInt16); i++)
            {
                bufReal[pos + i] = pbSrc[i];
            }

            return true;
        }

        public bool SetInt16(int pos, Int16 val)
        {
            Int32 iWritePosNew = pos + sizeof(Int16);
            if (!DataUtils.AssertTrue(iWritePosNew <= iBufSize))
            {
                return false;
            }
            int i = 0;
            byte[] bufReal = GetBuffer();

            byte[] pbSrc = System.BitConverter.GetBytes(val);
            // Array.Reverse(pbSrc);
            for (i = 0; i < sizeof(Int16); i++)
            {
                bufReal[pos + i] = pbSrc[i];
            }

            return true;
        }

        public bool SetByte(int pos, byte val)
        {
            if (!DataUtils.AssertTrue(pos >= 0)) return false;
            Int32 iWritePosNew = pos + sizeof(byte);
            if (!DataUtils.AssertTrue(iWritePosNew <= iBufSize)) return false;

            byte[] bufReal = GetBuffer();
            bufReal[pos] = val;
            return true;
        }

        public bool GetUInt16(int pos, ref UInt16 val)
        {
            if (!DataUtils.AssertTrue(pos >= 0)) return false;
            Int32 iWritePosNew = pos + sizeof(UInt16);
            if (!DataUtils.AssertTrue(iWritePosNew <= iBufSize)) return false;
            int i = 0;
            byte[] bufReal = GetBuffer();

            byte[] pbSrc = new byte[sizeof(UInt16)];
            for (i = 0; i < sizeof(UInt16); i++)
            {
                pbSrc[i] = bufReal[pos + i];
            }

            val = BitConverter.ToUInt16(pbSrc, 0);
            return true;
        }

        public bool GetInt16(int pos, ref Int16 val)
        {
            if (!DataUtils.AssertTrue(pos >= 0)) return false;
            Int32 iWritePosNew = pos + sizeof(Int16);
            if (!DataUtils.AssertTrue(iWritePosNew <= iBufSize)) return false;
            int i = 0;
            byte[] bufReal = GetBuffer();


            byte[] pbSrc = new byte[sizeof(Int16)];
            for (i = 0; i < sizeof(Int16); i++)
            {
                pbSrc[i] = bufReal[pos + i];
            }
            val = BitConverter.ToInt16(pbSrc, 0);
            return true;
        }

        public bool GetUInt32(int pos, ref UInt32 val)
        {
            if (!DataUtils.AssertTrue(pos >= 0)) return false;
            Int32 iWritePosNew = pos + sizeof(UInt32);
            if (!DataUtils.AssertTrue(iWritePosNew <= iBufSize)) return false;
            int i = 0;
            byte[] bufReal = GetBuffer();

            byte[] pbSrc = new byte[sizeof(UInt32)];
            for (i = 0; i < sizeof(UInt32); i++)
            {
                pbSrc[i] = bufReal[pos + i];
            }
            val = BitConverter.ToUInt32(pbSrc, 0);
            return true;
        }

        public bool GetInt32(int pos, ref Int32 val)
        {
            if (!DataUtils.AssertTrue(pos >= 0)) return false;
            Int32 iWritePosNew = pos + sizeof(Int32);
            if (!DataUtils.AssertTrue(iWritePosNew <= iBufSize)) return false;
            int i = 0;
            byte[] bufReal = GetBuffer();


            byte[] pbSrc = new byte[sizeof(Int32)];
            for (i = 0; i < sizeof(Int32); i++)
            {
                pbSrc[i] = bufReal[pos + i];
            }

            val = BitConverter.ToInt32(pbSrc, 0);
            return true;
        }

        public void Dispose()
        {
            bufferAlloc = null;
            iWritePos = 0;
            iStartPos = 0;
            iBufSize = 0;
        }
    }


}