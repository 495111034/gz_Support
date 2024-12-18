using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

/// <summary>
/// 地图掩码
/// </summary>
public class MapMask
{
    const uint TOKEN = 0x0070616d;
    public int width;
    public int height;
    public byte[] masks;//格子的高度，由高到低可通行，或者 相差不大 可通行
    public string path;
    //
    public bool Load(string path, byte[] data)
    {
        this.path = path;
        var ms = new MemoryStream(data, false);
        var br = new BinaryReader(ms);

        if (br.ReadUInt32() != TOKEN) return false;
        br.ReadUInt32();                        // update
        br.ReadUInt32();                        // version
        br.ReadUInt32();                        // id
        width = (int)br.ReadUInt32();           // width
        height = (int)br.ReadUInt32();          // height
        if (br.ReadUInt32() != 0) return false; // compress-flag
        var pos = br.ReadUInt32();              // data-start
        var len = br.ReadUInt32();              // data-length
        var count = len / 4;
        masks = new byte[count];                // data
        ms.Position = pos;
        var isPlaying = Application.isPlaying;
        //var line = new byte[width];
        for (int i = 0; i < count; i++)
        {
            var mask = (byte)br.ReadUInt32();
            //var x = i % width;
            //line[x] = mask;
            //if (x == width - 1) 
            //{
            //    Log.LogInfo($"{i/width} : {string.Join("",line)}");
            //}
            if (isPlaying && mask == 0)
            {
                mask = 255;
            }
            masks[i] = mask;
        }
        return true;
    }    

    // 
    public byte[] Save()
    {
        var ms = new MemoryStream();
        var bw = new BinaryWriter(ms);

        bw.Write((uint)TOKEN);      // token
        bw.Write((uint)0);          // update
        bw.Write((uint)1);          // version
        bw.Write((uint)0);          // id
        bw.Write((uint)width);      // width
        bw.Write((uint)height);     // height
        bw.Write((uint)0);          // compress-flag, 0=无, 1=zip
        bw.Write((uint)ms.Position + 8);        // data-start
        bw.Write((uint)(width * height * 4));   // data-length
        foreach (var b in masks)                // data
        {
            bw.Write((uint)b);
        }

        //
        var buff = new byte[ms.Length];
        Array.Copy(ms.GetBuffer(), buff, ms.Length);
        return buff;
    }
}
