using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;

// BOM 类型定义
public enum Bomb
{
    nobomb,
    utf_8,                          // EF BB BF
    utf_16_big_endian,              // FE FF
    utf_16_little_endian,           // FF FE
    utf_32_big_endian,              // 00 00 FE FF
    utf_32_little_endian,           // FF FE 00 00
}

// 加密算法
public delegate byte[] EncryptHandler(byte[] data);

/// <summary>
/// byte 工具
/// </summary>
public static class ByteUtils
{
    /// <summary>
    /// 格式化 byte 数组, 按照二进制编辑器中的样式显示(行号,分隔符)
    /// </summary>
    public static string FormatBytes(byte[] bytes, int start, int length)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("FormatBytes, length: {0}", length);
        sb.AppendLine();
        for (int i = 0; i < length; i++)
        {
            if (i % 16 == 0)
                sb.AppendFormat("{0:X8}  ", i);

            var b = bytes[i + start];
            sb.AppendFormat("{0:X2}", b);

            if ((i + 1) % 16 == 0)
                sb.AppendLine();
            else if ((i + 1) % 8 == 0)
                sb.AppendFormat(" - ");
            else
                sb.AppendFormat(" ");
        }
        string str = sb.ToString();
        return str;
    }

    /// <summary>
    /// 返回 bomb 类型
    /// </summary>
    public static Bomb GetBomb(byte[] data)
    {
        if (ByteUtils.IsEqual(data, 0xef, 0xbb, 0xbf)) return Bomb.utf_8;
        if (ByteUtils.IsEqual(data, 0x00, 0x00, 0xfe, 0xff)) return Bomb.utf_16_big_endian;
        if (ByteUtils.IsEqual(data, 0xff, 0xfe, 0x00, 0x00)) return Bomb.utf_16_little_endian;
        if (ByteUtils.IsEqual(data, 0xfe, 0xff)) return Bomb.utf_32_big_endian;
        if (ByteUtils.IsEqual(data, 0xff, 0xfe)) return Bomb.utf_32_little_endian;
        return Bomb.nobomb;
    }

    /// <summary>
    /// 判断值是否相等
    /// </summary>
    public static bool IsEqual(byte[] data, params byte[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (data[i] != values[i]) return false;
        }
        return true;
    }

    //
    public static int Read7BitEncodedInt(byte[] buff, ref int start)
    {
        int i = 0;
        int j = 0;
        int k = 0;
        for (k = 0; k < 5; k++)
        {
            byte b = buff[start++];
            i |= (int)((int)(b & 127) << (j & 31));
            j += 7;
            if ((b & 128) != 0)
            {
            }
            else
            {
                break;
            }
        }
        if (k < 5)
        {
            return i;
        }
        throw new FormatException("Too many bytes in what should have been a 7 bit encoded Int32.");
    }

    public static void Write7BitEncodedInt(byte[] bytes, ref int index, int value)
    {
        uint num = (uint)value;
        while (num >= 128U)
        {
            bytes[index++] = ((byte)(num | 128U));
            num >>= 7;
        }
        bytes[index++] = ((byte)num);
    }

    // 计算 CRC32
    public static long GetCRC32(byte[] data)
    {
        var crc = new Crc32();
        crc.Update(data);
        return crc.Value;
    }

    // 加密数据
    public static byte[] Encrypt(byte[] data)
    {
        var num = data.Length;
        var key = num + 127;
        for (int i = 0; i < num; i++)
        {
            data[i] = (byte)(data[i] ^ key);
            key++;
        }
        return data;
    }

    // 写入加密后的字符串
    public static void WriteEncryptString(this BinaryWriter br, string str, Encoding encoder, EncryptHandler entrypt)
    {
        var bytes = encoder.GetBytes(str);
        bytes = entrypt(bytes);

        var len = bytes.Length;

        br.Write7BitEncodedInt(len);
        br.Write(bytes);
    }

    //
    public static void Write7BitEncodedInt(this BinaryWriter bw, int value)
    {
        uint num = (uint)value;
        while (num >= 128U)
        {
            bw.Write((byte)(num | 128U));
            num >>= 7;
        }
        bw.Write((byte)num);
    }
}
