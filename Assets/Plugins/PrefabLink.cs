using UnityEngine;
using System;

[ExecuteInEditMode]
public class PrefabLink : MonoBehaviour
{
    public string guid;             // GUID
    public int change_count;        // 修改次数


#if UNITY_EDITOR

    public static bool scene_dirty;     // 场景内是否变脏

    void Awake()
    {
        scene_dirty = true;
    }

#endif
}
