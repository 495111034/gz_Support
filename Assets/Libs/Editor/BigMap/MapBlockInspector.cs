using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapBlockBehaviour)), CanEditMultipleObjects]
class MapBlockInspector : Editor
{
    MapBlockBehaviour _block;

    void OnEnable()
    {
        _block = target as MapBlockBehaviour;
    }

    void OnDisable()
    {
    }

    MapBlockBehaviour[] GetBlocks()
    {
        return Array.ConvertAll(targets, (obj) => obj as MapBlockBehaviour);
    }

    //
    public override void OnInspectorGUI()
    {
        // 细节
        _show_detail = EditorGUILayout.Toggle("Show Detail", _show_detail);
        if (_show_detail)
        {
            DrawDefaultInspector();
        }

        // 选择
        if (GUILayout.Button("Select All GameObjects"))
        {
            List<GameObject> list = new List<GameObject>();
            var blocks = GetBlocks();
            foreach (var block in blocks)
            {
                list.AddRange(block.GetAllObjs());
            }
            Selection.objects = list.ToArray();
        }

        // 
        GUILayout.Space(8);
        DrawLightmap();

        // 
        GUILayout.Space(8);
        DrawTerrainMesh();
    }
    static bool _show_detail;

    // 显示 TerrainMesh
    void DrawTerrainMesh()
    {
        // 烘培/清空
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Back Terrain"))
            {
                var blocks = GetBlocks();
                BigMapEditor.BakeTerrain(blocks);
            }
            if (GUILayout.Button("Clear Terrain"))
            {
                var blocks = GetBlocks();
                BigMapEditor.ClearTerrain(blocks);
            }
        }
        GUILayout.EndHorizontal();

        // 显示/隐藏
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Show Terrain"))
            {
                var blocks = GetBlocks();
                BigMapEditor.ShowTerrain(blocks);
            }
            if (GUILayout.Button("Hide Terrain"))
            {
                var blocks = GetBlocks();
                BigMapEditor.HideTerrain(blocks);
            }
        }
        GUILayout.EndHorizontal();
    }

    // 绘制 光照贴图
    void DrawLightmap()
    {
        // 烘培/清空
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Bake Lightmap"))
            {
                var blocks = GetBlocks();
                BigMapEditor.BakeLightmap(blocks, false, true);
            }
            if (GUILayout.Button("Clear Lightmap"))
            {
                var blocks = GetBlocks();
                BigMapEditor.ClearLightmap(blocks);
            }
        }
        GUILayout.EndHorizontal();

        // 显示/隐藏
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Show Lightmap"))
            {
                var blocks = GetBlocks();
                BigMapEditor.ShowLightmap(blocks);
            }
            if (GUILayout.Button("Hide Lightmap"))
            {
                var blocks = GetBlocks();
                BigMapEditor.HideLightmap(blocks);
            }
        }
        GUILayout.EndHorizontal();
    }
}
