using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 大地图编辑器
/// </summary>
public class BigMapBehaviour : MonoBehaviour
{
    public int MapWidth = 1000;
    public int MapHeight = 1000;
    public int BlockWidth = 64;
    public int BlockHeight = 64;
    public int MaxObjSize = 128;

    //
    public int LightmapWidth = 512;
    public int LightmapHeight = 512;

    
    //
    public Terrain terrain;

    //
    void Awake()
    {
        if (terrain == null)
        {
            terrain = Terrain.activeTerrain;
        }

    }
}
