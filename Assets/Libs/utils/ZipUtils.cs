//#define USE_GZIP_STREAM           // 使用 .net 自带的 GZipStream, 但在 U3D 中不支持

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Checksums;
#if !USE_GZIP_STREAM
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;
#endif

class ObbFile
{
    //static int _log_cnt = 0;
    Dictionary<string, ZipEntry> _dict = new Dictionary<string, ZipEntry>();
    ZipFile _zip;
    byte[] _buffer;

    public int RetrieveAssetPack(string aabpath)
    {
        //
        Log.Log2File("ObbFile decode start");
        //
        var t0 = UnityEngine.Time.realtimeSinceStartup;
        _zip = new ZipFile(aabpath);
        var t1 = UnityEngine.Time.realtimeSinceStartup;
        //
        var unsafe_entries = _zip.unsafe_entries;
        var dict = new Dictionary<string, ZipEntry>();
        foreach (var e in unsafe_entries)
        {
            if (e.Name.StartsWith("assets/assetpack/"))
            {
                dict[e.Name] = e;
            }
        }

        int notmath = 0;
        var filelist = "filelist" + (BuilderConfig.lang_id == "cn" ? "" : ("--l" + BuilderConfig.lang_id));

        var files_num = new int[2];
        var dirs = new string[] { "datas", "android" };
        for (int i = 0; i < dirs.Length; i++)
        {            
            string dir = dirs[i];
            dict.TryGetValue($"assets/assetpack/{dir}/version.txt", out var entry);
            if (entry == null)
            {
                continue;
            }
            var file_num = files_num[i];
            var fd = _zip.GetInputStream(entry.ZipFileIndex);
            var bytes = new byte[entry.Size];
            fd.Read(bytes, 0, bytes.Length);
            var text = System.Text.UTF8Encoding.UTF8.GetString(bytes);
            var lines = text.Split('\n');////filename=time
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                var kv = line.Split('=');//filename=time
                if (kv.Length < 2)
                {
                    continue;
                }
                //version 中的key 需要加dir
                var path = $"{dir}/{kv[0]}";
                var k = $"assets/assetpack/{path}";
                dict.TryGetValue(k, out entry);
                if (entry == null)
                {
                    Log.Log2File($"ObbFile ZipEntry {k} not found");
                    continue;
                }
                _dict[path] = entry;
                ++file_num;
                entry.modifytime_ex = int.Parse(kv[1]);
                //
                if (kv[0].StartsWith(filelist))
                {
                    fd = _zip.GetInputStream(entry.ZipFileIndex);
                    bytes = new byte[entry.Size];
                    fd.Read(bytes, 0, bytes.Length);
                    text = System.Text.UTF8Encoding.UTF8.GetString(bytes);
                    var lines2 = text.Split('\n');//filename,time,size
                    foreach (var line2 in lines2)
                    {
                        if (string.IsNullOrEmpty(line2))
                        {
                            continue;
                        }
                        var arr = line2.Split(',');//filename,time,size
                        k = $"assets/assetpack/{dir}/{arr[0]}";
                        dict.TryGetValue(k, out entry);
                        if (entry == null)
                        {
                            Log.Log2File($"ObbFile ZipEntry {k} not found");
                            continue;
                        }
                        //filelist 中的 key 不需要dir
                        if (entry.Size == long.Parse(arr[2]))
                        {
                            entry.modifytime_ex = int.Parse(arr[1]);
                            _dict[arr[0]] = entry;
                            ++file_num;
                        }
                        else
                        {
                            ++notmath;
                            Log.Log2File($"ObbFile size not math, line2={line2}, entry.Szie={entry.Size}");
                        }
                    }
                }
            }
            files_num[i] = file_num;
        }
        var t2 = UnityEngine.Time.realtimeSinceStartup;
        Log.Log2File($"ObbFile decode done, assetpack={dict.Count}, filenum={_dict.Count}={files_num[0]}+{files_num[1]}, not math={notmath}, cost={(int)((t2 - t0) * 1000)}=(open){(int)((t1 - t0) * 1000)} + (read){(int)((t2 - t1) * 1000)}ms");
        return _dict.Count;
    }

    public ZipEntry GetZipEntryFromOBB(string entryname, int mtime, long size) 
    {
        if (!_dict.TryGetValue(entryname, out var entry) || entry.modifytime_ex == -1)
        {
            return null;
        }
        if (entry.modifytime_ex != mtime)
        {
#if DEBUG
            Log.Log2File($"ObbFile {entryname} time={entry.modifytime_ex} not match remote.time={mtime}");
#endif
            entry.modifytime_ex = -1;
            return null;
        }
        if (size >= 0)
        {
            if (size != entry.Size)
            {
                Log.Log2File($"ObbFile {entryname} zip.size={entry.Size} not match remote.size={size}");
                entry.modifytime_ex = -1;
                return null;
            }
        }
        return entry;
    }

    public byte[] GetFileFromOBB(string entryname, int mtime)
    {
        var entry = GetZipEntryFromOBB(entryname, mtime, -1);
        if (entry == null)
        {
            return null;
        }
        var size = (int)entry.Size;
        var data = new byte[size];
        lock (_zip)
        {
            var _fd = _zip.GetInputStream(entry.ZipFileIndex);
            var offset = 0;
            while (offset < size)
            {
                offset += _fd.Read(data, offset, size - offset);
            }
        }
        //Log.Log2File($"ObbFile GetFileFromOBB entryname={entryname}");
        return data;
    }
    public bool CopyFileFromOBB(string entryname, int size, int mtime, string savepath)
    {
        var entry = GetZipEntryFromOBB(entryname, mtime, size);
        if (entry == null)
        {
            return false;
        }
        var zipsize = (int)entry.Size;
        var savefd = new FileStream(savepath, FileMode.Create, FileAccess.Write, FileShare.Write, 1);
        lock (_zip)
        {
            if (_buffer == null)
            {
                _buffer = new byte[1024 * 1024];
            }
            var _fd = _zip.GetInputStream(entry.ZipFileIndex);
            {                
                var saved = 0;
                while (saved < zipsize)
                {
                    var read = _fd.Read(_buffer, 0, Math.Min(zipsize - saved, _buffer.Length));
                    saved += read;
                    savefd.Write(_buffer, 0, read);
                }
            }            
        }
        savefd.Dispose();
        //Log.Log2File($"ObbFile CopyFileFromOBB entryname={entryname} => {savepath}");
        return true;
    }
    public void Dispose()
    {
        _zip.Close();
        _dict.Clear();
        _buffer = null;
    }
}

/// <summary>
/// 压缩工具
/// </summary>
public static class ZipUtils
{
    public static byte[]  Compress(Dictionary<string, byte[]> datas)
    {
        var ms = new MemoryStream();
        ZipOutputStream zip = new ZipOutputStream(ms);
        Crc32 crc = new Crc32();

        foreach (var kv in datas)
        {
            ZipEntry entry = new ZipEntry(kv.Key);
            entry.DateTime = System.DateTime.Now;
            entry.Size = kv.Value.Length;
            
            crc.Reset();
            crc.Update(kv.Value);

            entry.Crc = crc.Value;

            zip.PutNextEntry(entry);

            zip.Write(kv.Value, 0, kv.Value.Length);
        }
        zip.Finish();

        var ret = new byte[ms.Length];
        Array.Copy(ms.GetBuffer(), ret, ms.Length);
        return ret;
    }

    static List<byte[]> _buffers = new List<byte[]>();//大约 1个线程1MB
    public static void DecompressClearBuffers()
    {
        var Count = 0;
        lock (_buffers)
        {
            Count = _buffers.Count;
            _buffers.Clear();
        }
        Log.Log2File($"DecompressClearBuffers, Count={Count}");
    }
    public static void DecompressRelease(byte[] bytes)
    {
        lock (_buffers)
        {
            _buffers.Add(bytes);
        }
    }

    // 解压
    public static string Decompress(byte[] data, Encoding enc)
    {
        //flag
        if (data[0] != 0x1f || data[1] != 0x8b || data[2] != 0x08)
        {
            throw new Exception($"zip={data.Length}, not zip data:{data[0]},{data[1]},{data[2]}");
        }
        //解压后大小
        var crc = BitConverter.ToInt32(data, data.Length - 8);
        var len = BitConverter.ToInt32(data, data.Length - 4);//一次性读取整个文件，减少alloc碎片
        if (len == 0)
        {
            Log.LogWarning($"zip={data.Length}, decode len={len}, crc={crc}");
            return "";
        }

        if (len > 10 * 1024 * 1024 && len > 10 * data.Length)
        {
            throw new Exception($"zip={data.Length}, too large, len={len}, crc={crc}");
        }

        using (var ms = new MemoryStream(data, 0, data.Length, false))
        {
#if USE_GZIP_STREAM || HybridCLR || UNITY_EDITOR
            var gzip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress, false);
#else
            var gzip = new GZipInputStream(ms);
#endif
            using (gzip)
            {
                byte[] bytes = null;
                lock (_buffers)
                {
                    for (var i = 0; i < _buffers.Count; ++i)
                    {
                        if (len < _buffers[i].Length)
                        {
                            bytes = _buffers[i];
                            _buffers.RemoveAt(i);
                            break;
                        }
                    }
                }
                if (bytes == null)
                {
                    bytes = new byte[(len / 1024 / 1024 + 1) * 1024 * 1024];
                }
                //var md5 = StringUtils.md5(data);
                Log.Log2File($"data={data.Length}, Decompress data={len}, crc={crc}, (bytes={bytes.Length},hash={bytes.GetHashCode()}), gzip={gzip.GetType().FullName}");
                var offset = 0;
                while (offset < len)
                {
                    var num_read = gzip.Read(bytes, offset, len - offset + 1);
                    if (num_read != len - offset)
                    {
                        Log.LogWarning($"num_read={num_read} != {len} - {offset}");
                    }
                    if (num_read == 0) break;
                    offset += num_read;
                }
                if (ms.Position != data.Length)
                {
                    Log.LogError($"data={data.Length}, ms.Position={ms.Position}, gzip.Position={gzip.Position}, bytes={len}, offset={offset}");
                }
                //Log.Log2File($"Decompress Done, {data.Length} -> {len}, md5={StringUtils.md5_len(bytes, 0, len)}");

                var str = enc.GetString(bytes, 0, len);
                lock (_buffers)
                {
                    _buffers.Add(bytes);
                }
                return str;
            }
        }
    }

    // 解压
    public static byte[] DecompressOut(byte[] data, bool usecache, out int len)
    {
        //flag
        if (data[0] != 0x1f || data[1] != 0x8b || data[2] != 0x08)
        {
            throw new Exception($"zip={data.Length}, not zip data:{data[0]},{data[1]},{data[2]}");
        }
        //解压后大小
        len = BitConverter.ToInt32(data, data.Length - 4);//一次性读取整个文件，减少alloc碎片
        if (len > 10 * 1024 * 1024 && len > 10 * data.Length)
        {
            throw new Exception($"zip={data.Length}, too large:{len}");
        }

        using (var ms = new MemoryStream(data, 0, data.Length, false))
        {
#if USE_GZIP_STREAM || HybridCLR || UNITY_EDITOR
            var gzip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress, false);
#else
            var gzip = new GZipInputStream(ms);
#endif
            using (gzip)
            {
                //
                byte[] bytes = null;
                if (usecache)
                {
                    lock (_buffers)
                    {
                        for (var i = 0; i < _buffers.Count; ++i)
                        {
                            if (len < _buffers[i].Length)
                            {
                                bytes = _buffers[i];
                                _buffers.swap_tail_and_fast_remove(i);
                                break;
                            }
                        }
                    }
                    if (bytes == null)
                    {
                        bytes = new byte[(len / 1024 / 1024 + 1) * 1024 * 1024];
                    }
                }
                else 
                {
                    bytes = new byte[len];
                }
                //
                var offset = 0;
                while (offset < len)
                {
                    var num_read = gzip.Read(bytes, offset, len - offset + (usecache ? 1 : 0));
                    if (num_read != len - offset)
                    {
                        Log.LogWarning($"num_read={num_read} != {len} - {offset}");
                    }
                    if (num_read == 0) break;
                    offset += num_read;
                }
                if (ms.Position != data.Length)
                {
                    Log.LogWarning($"data={data.Length}, ms.Position={ms.Position}, gzip.Position={gzip.Position}, bytes={len}, offset={offset}");
                }
                return bytes;
            }
        }
    }
}
