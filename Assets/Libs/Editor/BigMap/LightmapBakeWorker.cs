using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;


/// <summary>
/// 烘培器
/// </summary>
class LightmapBakeWorker : IProgressWorker
{
    MapBlockBehaviour[] _blocks;
    bool _bakeNew;
    bool _bShow;
    int _next;
    MapBlockBehaviour _cur;

    //
    ProgressWindow _window;

    //
    public void Start(MapBlockBehaviour[] blocks, bool bakeNew, bool bShow)
    {
        _blocks = blocks;
        _bakeNew = bakeNew;
        _bShow = bShow;
        _next = 0;
        _cur = null;

        // 隐藏光照贴图
        BigMapEditor.HideLightmap(blocks);

        //
        _window = EditorWindow.GetWindow<ProgressWindow>();
        _window.Show(this);
    }

    // 获取下一个可烘培的块
    MapBlockBehaviour GetNext()
    {
        while (_next < _blocks.Length)
        {
            var b = _blocks[_next++];
            if (b == null) continue;
            if (_bakeNew && b.LightmapCount > 0) continue;
            return b;
        }
        return null;
    }

    //
    public void OnGUI()
    {
        GUILayout.Label(string.Format("Bake Lightmap {0}/{1}, {2}", _next, _blocks.Length, (_cur != null ? _cur.name : null)));
        if (GUILayout.Button("Cancel"))
        {
            _window.Close();
        }
    }

    public void OnDestroy()
    {
        // 取消烘培
        if (Lightmapping.isRunning)
        {
            Lightmapping.Cancel();
        }

        // 显示
        if (_bShow)
        {
            BigMapEditor.ShowLightmap(_blocks);
        }
    }

    public void Update()
    {
        if (Lightmapping.isRunning) return;

        // 保存信息
        if (_cur != null)
        {
            BigMapEditor.AssignLightmap(_cur);
            _cur = null;
        }

        // 获取当前烘培的块
        var block = GetNext();

        // 结束
        if (block == null)
        {
            _window.Close();
            return;
        }

        // 选择对象
        var objs = Selection.objects = block.GetAllObjs();
        if (objs.Length == 0) return;
        
        // 计算尺寸
        var bm = BigMapEditor.GetBigMap(block);
        var width = block.LightmapWidth > 0 ? block.LightmapWidth : bm.LightmapWidth;
        var height = block.LightmapHeight > 0 ? block.LightmapHeight : bm.LightmapHeight;

        // 如果之前烘培过, 且尺寸太小, 则增加尺寸
        if (block.LightmapCount >= 4 && block.LightmapWidth == 0 && block.LightmapHeight == 0)
        {
            width *= 2;
            height *= 2;
            block.LightmapWidth = width;
            block.LightmapHeight = height;
        }
        
        // 设置属性
        LightmapEditorSettings.maxAtlasSize = width;
        LightmapEditorSettings.maxAtlasHeight = height;

        // 删除之前的光照贴图
        BigMapEditor.ClearLightmap(new MapBlockBehaviour[] { block });

        // 烘培
        Lightmapping.Clear();
        if (Lightmapping.BakeAsync())
        {
            // 设置当前
            _cur = block;
        }
    }
}

