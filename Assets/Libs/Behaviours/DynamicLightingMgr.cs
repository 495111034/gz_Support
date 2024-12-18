using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animation))]
public class DynamicLightingMgr : MonoBehaviour
{
    public HashSet<Material> m_GatheredMaterials;

    private CombinedLightingMgr root;

    #region 参数初始化
    public void Start()
    {
        root = GameObject.Find("LightMgr").GetComponent<CombinedLightingMgr>();
        if(root == null)
        {
            Log.LogError("LightMgr no find");
            return;
        }

        if (m_GatheredMaterials == null)
            m_GatheredMaterials = new HashSet<Material>();

        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

        for (int j = 0; j < renderers.Length; ++j)
        {
            Renderer r = renderers[j];

            if (r == null) continue;

            for (int k = 0; k < r.sharedMaterials.Length; ++k)
            {
                if (r.sharedMaterials[k] != null)
                    m_GatheredMaterials.Add(r.sharedMaterials[k]);
            }
        }
    }

    #endregion

    #region 帧更新
    public void Update() {
        if(root == null)
        {
            return;
        }
        foreach (Material m in m_GatheredMaterials)
        {
            m.SetColor(root.m_LightColorParam, root.m_AnimProperties.m_LightColor);
            m.SetFloat(root.m_LightHeightParam, root.m_AnimProperties.m_LightHeight);
            m.SetColor(root.m_LightColor2Param, root.m_AnimProperties.m_LightColor2);
            m.SetFloat(root.m_LightHeight2Param, root.m_AnimProperties.m_LightHeight2);
            m.SetColor(root.m_GIColorParam, root.m_AnimProperties.m_GIColor);
            m.SetColor(root.m_SunColorParam_Indoor, root.m_AnimProperties.m_SunColor_Indoor);
            m.SetColor(root.m_SkyLightColorParam, root.m_AnimProperties.m_SkyLightColor);
            m.SetFloat(root.m_AlphaParam, root.m_AnimProperties.m_Alpha);
        }
    }
    #endregion


    #region 参数更新
    public void SetMaterialProperty(Material m, int paramHash, Color value) { m.SetColor(paramHash, value); }
    public void SetMaterialProperty(Material m, int paramHash, float value) { m.SetFloat(paramHash, value); }
    #endregion
}
