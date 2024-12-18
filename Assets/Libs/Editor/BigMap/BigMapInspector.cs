using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BigMapBehaviour))]
class BigMapInspector : Editor
{
    BigMapBehaviour _bm;
    int _map_width, _map_height, _block_width, _block_height;
    string _gui_state;

    //
    void OnEnable()
    {
        _bm = target as BigMapBehaviour;
        _map_width = _bm.MapWidth;
        _map_height = _bm.MapHeight;
        _block_width = _bm.BlockWidth;
        _block_height = _bm.BlockHeight;
        _gui_state = "default";
    }

    void OnDisable()
    {
    }

    MapBlockBehaviour[] GetBlocks()
    {
        return _bm.GetComponentsInChildren<MapBlockBehaviour>();
    }

    //
    public override void OnInspectorGUI()
    {
        switch (_gui_state)
        {
            case "default":

                // 新建地图
                if (GUILayout.Button("Create Map ..."))
                {
                    _gui_state = "create_map";
                }

                // 更新块 
                GUILayout.BeginHorizontal();
                GUILayout.Label("MaxObjSize");
                _bm.MaxObjSize = EditorGUILayout.IntField(_bm.MaxObjSize);
                if (GUILayout.Button("Update Blocks"))
                {
                    BigMapEditor.AlignBlocks(_bm);
                    BigMapEditor.PutGo(_bm);
                }
                GUILayout.EndHorizontal();

                // 选项
                GUILayout.Space(8);
                DrawOptions();

                // 光照贴图
                GUILayout.Space(8);
                DrawLightmap();

                // 地形
                GUILayout.Space(8);
                DrawTerrain();
                break;

            case "create_map":
                DrawCreateMap();
                break;
        }
    }

    void DrawTerrain()
    {
        //
        _bm.terrain = EditorGUILayout.ObjectField(_bm.terrain, typeof(Terrain)) as Terrain;

        // bake, clear
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

        // show, hide
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

    //
    void DrawOptions()
    {
        // 显示
        GUILayout.BeginHorizontal();
        {
            MapBlockBehaviour.ShowBounds1 = GUILayout.Toggle(MapBlockBehaviour.ShowBounds1, "Show Bounds 1");
            MapBlockBehaviour.ShowBounds2 = GUILayout.Toggle(MapBlockBehaviour.ShowBounds2, "Show Bounds 2");
            if (GUI.changed)
            {
                SceneView.RepaintAll();
            }
        }
        GUILayout.EndHorizontal();

        // 尺寸
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Lightmap Size");
            _bm.LightmapWidth = EditorGUILayout.IntField(_bm.LightmapWidth);
            _bm.LightmapHeight = EditorGUILayout.IntField(_bm.LightmapHeight);
        }
        GUILayout.EndHorizontal();
    }

    // 绘制 创建地图
    void DrawCreateMap()
    {
        GUILayout.Label("Create Map");

        // 尺寸
        _map_width = EditorGUILayout.IntField("Map Width", _map_width);
        _map_height = EditorGUILayout.IntField("Map Height", _map_height);
        _block_width = EditorGUILayout.IntField("Block Width", _block_width);
        _block_height = EditorGUILayout.IntField("Block Height", _block_height);

        // Ok & Cancel
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Ok"))
            {
                if (BigMapEditor.CreateMap(_bm, _map_width, _map_height, _block_width, _block_height))
                {
                    _gui_state = "default";
                }
            }
            if (GUILayout.Button("Cancel"))
            {
                _gui_state = "default";
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
                BigMapEditor.BakeLightmap(blocks, true, true);
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
