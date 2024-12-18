using System.Collections.Generic;
using UnityEngine;

public class SceneObjectShadow : MonoBehaviour , ISceneObject
{
    public List<Renderer> CacheRendererList { get; set; }
    public bool iHightQualityShow => HightQuality;
    public bool HightQuality;

    [System.NonSerialized]
    public bool IsLoaded = false;

    [System.NonSerialized]
    public float size = 0;
}

