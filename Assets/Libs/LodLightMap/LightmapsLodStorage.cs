using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class LightmapsLodStorage : MonoBehaviour
{
    public List<LODGroup> bakedLodRenderers = new List<LODGroup>();

    void Awake()
    {
        RefreshScene();
    }

    public void RefreshScene()
    {
        RefreshScene(this);
    }

    public static void RefreshScene(LightmapsLodStorage lodStorage = null, bool updateNonBaked = false)
    {
        if (lodStorage == null)
        {
            return;
        }

        for (int i = 0; i < lodStorage.bakedLodRenderers.Count; i++)
        {
            var lod_group = lodStorage.bakedLodRenderers[i];
            if (lod_group == null) continue;
            var lods = lod_group.GetLODs();
            if (lods.Length == 0) continue;
            int base_length = lods[0].renderers.Length;
            if (base_length == 0) continue;
            var source_render = lods[0].renderers[0];
            if (source_render == null) continue;
            for (int x = 1; x < lods.Length; x++)
            {
                var lod = lods[x];
                if (lod.renderers != null && lod.renderers.Length > 0)
                {
                    for (int i1 = 0; i1 < lod.renderers.Length; i1++)
                    {
                        var render = lod.renderers[i1];
                        if (render != null)
                        {
                            if (base_length > 1)
                            {
                                string find_name = render.gameObject.name;
                                int match_count = 0;
                                Renderer _base_render = null;
                                for (int i2 = 0; i2 < base_length; i2++)
                                {
                                    int _count = 0;
                                    var base_render = lods[0].renderers[i2];
                                    if (base_render != null)
                                    {
                                        if (base_render == render)
                                        {
                                            _base_render = base_render;
                                            break;
                                        }
                                        else
                                        {
                                            string base_name = base_render.gameObject.name;
                                            for (int i3 = 0; i3 < base_name.Length && i3 < find_name.Length; i3++)
                                            {
                                                if (base_name[i3] == find_name[i3])
                                                {
                                                    _count++;
                                                }
                                            }
                                        }
                                    }
                                    if (_count > match_count)
                                    {
                                        match_count = _count;
                                        _base_render = base_render;
                                    }
                                }
                                if (_base_render != null)
                                {
                                    render.lightmapIndex = _base_render.lightmapIndex;
                                    render.lightmapScaleOffset = _base_render.lightmapScaleOffset;
                                }
                                else 
                                {
                                    render.lightmapIndex = source_render.lightmapIndex;
                                    render.lightmapScaleOffset = source_render.lightmapScaleOffset;
                                }
                            }
                            else
                            {
                                render.lightmapIndex = source_render.lightmapIndex;
                                render.lightmapScaleOffset = source_render.lightmapScaleOffset;
                            }
                        }
                    }
                }
            }
        }
    }

    public void ClearScene()
    {
        bakedLodRenderers.Clear();
    }

#if UNITY_EDITOR

    [ContextMenu("去掉非LOD0的静态标记")]
    private void SetRemoveStatic()
    {
        var lod_groups = gameObject.GetComponentsInChildren<LODGroup>(true);
        for (int i = 0; i < lod_groups.Length; i++)
        {
            var lod_group = lod_groups[i];
            var lods = lod_group.GetLODs();
            if (lods.Length > 0)
            {
                for (int i1 = 1; i1 < lods.Length; i1++)
                {
                    var lod = lods[i1];
                    if (lod.renderers != null && lod.renderers.Length > 0)
                    {
                        for (int i2 = 0; i2 < lod.renderers.Length; i2++)
                        {
                            if (lod.renderers[i2] != null)
                            {
                                lod.renderers[i2].gameObject.isStatic = false;
                            }
                        }
                    }
                }
            }
        }
    }

    [ContextMenu("查找所有lightmap关联")]
    private void SetLODRenderList()
    {
        bakedLodRenderers.Clear();
        var lod_groups = gameObject.GetComponentsInChildren<LODGroup>(true);
        for (int i = 0; i < lod_groups.Length; i++)
        {
            var lod_group = lod_groups[i];
            var lods = lod_group.GetLODs();
            if (lods.Length > 0)
            {
                if (lods[0].renderers != null && lods[0].renderers.Length > 0 && lods[0].renderers[0] != null)
                {
                    bool is_instance = false;
                    //__gpu_instance
                    Transform check = lods[0].renderers[0].transform;
                    while (check != null)
                    {
                        if (check.parent != null && check.parent.name.Contains("__gpu_instance"))
                        {
                            is_instance = true;
                            break;
                        }
                        check = check.parent;
                    }
                    for (int x = 0; x < lods.Length; x++)
                    {
                        if (lods[x].renderers != null && lods[x].renderers.Length > 0 && lods[x].renderers[0] != null)
                        {
                            var go = lods[x].renderers[0].gameObject;
                            go.isStatic = false;
                            if (true || is_instance)
                            {
                                UnityEditor.GameObjectUtility.SetStaticEditorFlags(go, UnityEditor.StaticEditorFlags.ContributeGI | UnityEditor.StaticEditorFlags.ReflectionProbeStatic);
                            }
                            else
                            {
                                UnityEditor.GameObjectUtility.SetStaticEditorFlags(go, UnityEditor.StaticEditorFlags.ContributeGI | UnityEditor.StaticEditorFlags.BatchingStatic | UnityEditor.StaticEditorFlags.ReflectionProbeStatic);
                            }
                        }
                    }
                    if (lods.Length > 1)
                    {
                        if (lods[1].renderers != null && lods[1].renderers.Length > 0 && lods[1].renderers[0] != null)
                        {
                            bakedLodRenderers.Add(lod_group);
                        }
                    }
                }
            }
        }

        RefreshScene();

        UnityEditor.EditorUtility.SetDirty(gameObject);
    }

    [ContextMenu("切换到超低画质")]
    private void ChangeSuperLowQuality()
    {
        QualitySettings.masterTextureLimit = 1;
        QualitySettings.lodBias = 0.6f;
        SetShaderQualityLevelKeyword(0);
    }


    [ContextMenu("切换到低画质")]
    private void ChangeLowQuality()
    {
        QualitySettings.masterTextureLimit = 1;
        QualitySettings.lodBias = 0.6f;
        SetShaderQualityLevelKeyword(1);
    }

    [ContextMenu("切换到高画质")]
    private void ChangeHightQuality()
    {

        QualitySettings.masterTextureLimit = 0;
        QualitySettings.lodBias = 1f;
        SetShaderQualityLevelKeyword(2);
    }

    public static void SetShaderQualityLevelKeyword(int level)
    {
        switch (level)
        {
            case 0:
                {
                    Shader.EnableKeyword("_G_GAME_QUALITY_VERYLOW");
                    Shader.DisableKeyword("_G_GAME_QUALITY_LOW");
                    Shader.DisableKeyword("_G_GAME_QUALITY_HIGH");
                }
                break;
            case 1:
                {
                    Shader.DisableKeyword("_G_GAME_QUALITY_VERYLOW");
                    Shader.EnableKeyword("_G_GAME_QUALITY_LOW");
                    Shader.DisableKeyword("_G_GAME_QUALITY_HIGH");
                }
                break;
            case 2:
                {
                    Shader.DisableKeyword("_G_GAME_QUALITY_VERYLOW");
                    Shader.DisableKeyword("_G_GAME_QUALITY_LOW");
                    Shader.EnableKeyword("_G_GAME_QUALITY_HIGH");
                }
                break;
        }


        //}
        //else
        //{
        //    Shader.EnableKeyword("_G_GAME_QUALITY_VERYLOW");
        //    Shader.DisableKeyword("_G_GAME_QUALITY_LOW");
        //    Shader.DisableKeyword("_G_GAME_QUALITY_HIGH");   

        //    //Shader.EnableKeyword("_G_GAME_QUALITY_LOW");
        //    //Shader.DisableKeyword("_G_GAME_QUALITY_HIGH");
        //}
    }

#endif
}
