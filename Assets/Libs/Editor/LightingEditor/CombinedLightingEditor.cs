using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CombinedLightingMgr))]
public class CombinedLightingEditor : Editor {

    CombinedLightingMgr m_LightingMgr;

    private void OnEnable()
    {
        m_LightingMgr = target as CombinedLightingMgr;
        m_LightingMgr.Init();
        //m_LightingMgr.SetRoot(m_LightingMgr.gameObject);

        EditorApplication.update += Update;
    }

    private void OnDisable()
    {
        EditorApplication.update -= m_LightingMgr.ApplyAllProperties;
        EditorApplication.update -= Update;
    }

    private void Update()
    {
        if(m_LightingMgr.animationClips != null && m_LightingMgr.animationClips.Length != 0)
        {
            foreach(AnimationEditorWidget clipWidget in m_LightingMgr.animationClips)
            {
                if(clipWidget.animation != null)
                {
                    clipWidget.animation.SampleAnimation(m_LightingMgr.gameObject, clipWidget.process * clipWidget.animation.length);
                }
            }
        }
        m_LightingMgr.Update();
    }

    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("清除全部"))
        {
            m_LightingMgr.ClearAll();
        }
        if (GUILayout.Button("扫描材质"))
        {
            m_LightingMgr.ScanMaterials();
        }
        GUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();

        serializedObject.ApplyModifiedProperties();
    }
}
