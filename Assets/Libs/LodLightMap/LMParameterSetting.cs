using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
public class LMParameterSetting : MonoBehaviour
{
    public LightmapParameters m_Parameters;
    public Transform m_OverrideTransform;
    public bool paraToggle = true;
    public bool transToggle = false;
    public bool materialToggle = false;
    public bool queueToggle = false;
    public Dictionary<Material, int> oldMaterials = new Dictionary<Material, int>();
    public List<Material> newMaterials = new List<Material>();
    public List<int> materialQueue = new List<int>();
    public void Start()
    {
        
    }
    public void SetChildLMParameters()
    {
        SerializedObject m_SerializedObject = new SerializedObject(GetComponentsInChildren<MeshRenderer>(true));
        SerializedProperty m_LightmapParameters = m_SerializedObject.FindProperty("m_LightmapParameters");
        m_LightmapParameters.objectReferenceValue = m_Parameters;
        m_SerializedObject.ApplyModifiedProperties();
    }
    public void SetChildOverrideTransform()
    {
        foreach (var render in GetComponentsInChildren<MeshRenderer>(true))
        {
            render.probeAnchor = m_OverrideTransform;
        }
    }
    public void SetChildRenderMaterial()
    {
        foreach (var render in GetComponentsInChildren<MeshRenderer>(true))
        {
            if (oldMaterials.ContainsKey(render.sharedMaterial))
            {
                int index = oldMaterials[render.sharedMaterial];
                if (newMaterials[index] != render.sharedMaterial)
                {
                    render.sharedMaterial = newMaterials[index];
                }
                //else if (render.sharedMaterial.renderQueue != materialQueue[index])
                //{
                //    render.sharedMaterial.renderQueue = materialQueue[index];
                //    if (render.sharedMaterial.HasProperty("_QueueOffset"))
                //    {
                //        int queueOffset = materialQueue[index] >= 2450 ? materialQueue[index] - 2450 : materialQueue[index] - 2000;
                //        render.sharedMaterial.SetFloat("_QueueOffset", queueOffset);
                //    }
                //}
            }
        }
    }



}
[CustomEditor(typeof(LMParameterSetting))]
public class LMParameterSettingEditor : Editor
{
    SerializedProperty m_Parameters;
    SerializedProperty m_OverrideTransform;
    GUIContent content = EditorGUIUtility.TrTextContent("Lightmap Parameters");
    GUIContent defaultParameters = EditorGUIUtility.TrTextContent("Scene Default Parameters");
    public void OnEnable()
    {
        var setting = target as LMParameterSetting;
        m_Parameters = serializedObject.FindProperty("m_Parameters");
        m_OverrideTransform = serializedObject.FindProperty("m_OverrideTransform");
        if (setting.materialToggle)
        {
            RefreashMaterials(setting);
        }
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var setting = target as LMParameterSetting;
        #region 下拉
        //EditorGUILayout.PropertyField(m_Parameters);
        //Rect controlRect = EditorGUILayout.GetControlRect();
        //EditorGUI.BeginProperty(controlRect, content, m_Parameters);
        /*  下拉菜单
        controlRect = EditorGUI.PrefixLabel(controlRect, content);
        GUIContent content2 = (m_Parameters.objectReferenceValue != null) ? new GUIContent(m_Parameters.objectReferenceValue.name) : defaultParameters;
        //EditorGUILayout.BeginVertical();
        if (EditorGUI.DropdownButton(controlRect, content2, FocusType.Passive, EditorStyles.popup))
        {
            ShowAssetsPopupMenu(controlRect, m_Parameters);
        }
        EditorGUILayout.Space(10);
        */
        #endregion
        //LightmapParameters
        EditorGUILayout.BeginHorizontal();
        setting.paraToggle = EditorGUILayout.Toggle(setting.paraToggle, GUILayout.Width(20));
        EditorGUILayout.PropertyField(m_Parameters);

        EditorGUILayout.EndHorizontal();

        //Probe Anchor Override
        EditorGUILayout.BeginHorizontal();
        setting.transToggle = EditorGUILayout.Toggle(setting.transToggle, GUILayout.Width(20));
        EditorGUILayout.PropertyField(m_OverrideTransform, new GUIContent("Ref Anchor Override"));
        EditorGUILayout.EndHorizontal();

        //材质和渲染队列
        EditorGUILayout.BeginHorizontal();
        setting.materialToggle = EditorGUILayout.Toggle(setting.materialToggle, GUILayout.Width(20));
        //GUILayout.Label("材质修改");
        if (GUILayout.Button("刷新", GUILayout.Width(50)) && setting.materialToggle)
        {
            RefreashMaterials(setting);
        }
        if (GUILayout.Button("清除", GUILayout.Width(50)) && setting.materialToggle)
        {
            setting.oldMaterials.Clear();
            setting.newMaterials.Clear();
            setting.materialQueue.Clear();
        }
        //GUILayout.Space(5);
        //if (GUILayout.Button("一键开启GPU Instance", GUILayout.Width(180)))
        //{
        //    foreach (var item in setting.GetComponentsInChildren<MeshRenderer>())
        //    {
        //        if (!item.sharedMaterial.enableInstancing)
        //        {
        //            item.sharedMaterial.enableInstancing = true;
        //        }
        //    }
        //}
        if (GUILayout.Button("Scale负值查找", GUILayout.Width(100)) && setting.materialToggle)
        {
            foreach (var item in setting.GetComponentsInChildren<Transform>())
            {
                if (HasNegativeValue(item.localScale))
                {
                    Selection.activeGameObject = item.gameObject;
                    break;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel++;
        ShowChildMaterialGUI(setting);

        EditorGUI.indentLevel--;




        EditorGUILayout.Space(5);
        if (GUILayout.Button("应用", GUILayout.Height(25)))
        {
            if (setting.paraToggle)
            {
                (target as LMParameterSetting).SetChildLMParameters();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            if (setting.transToggle)
            {
                (target as LMParameterSetting).SetChildOverrideTransform();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            if (setting.materialToggle)
            {
                (target as LMParameterSetting).SetChildRenderMaterial();
                RefreashMaterials(setting);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        //EditorGUILayout.EndVertical();
        //EditorGUI.EndProperty();
        serializedObject.ApplyModifiedProperties();
    }
    public bool HasNegativeValue(Vector3 v)
    {
        return v.x < 0 || v.y < 0 || v.z < 0;
    }
    public static void RefreashMaterials(LMParameterSetting setting)
    {
        setting.oldMaterials.Clear();
        setting.newMaterials.Clear();
        setting.materialQueue.Clear();

        foreach (var render in setting.gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            Material mat = render.sharedMaterial;
            if (!setting.oldMaterials.ContainsKey(mat))
            {
                setting.oldMaterials.Add(mat, setting.oldMaterials.Count);
                setting.newMaterials.Add(mat);
                setting.materialQueue.Add(mat.renderQueue);
            }
        }
    }
    public static void ShowChildMaterialGUI(LMParameterSetting setting)
    {
        for (int i = 0; i < setting.newMaterials.Count; i++)
        {
            if (setting.newMaterials[i] == null)
            {
                continue;
            }
            GUILayout.BeginHorizontal();
            string AlphaClip;
            if (setting.newMaterials[i].HasProperty("_AlphaClip"))
            {
                AlphaClip = setting.newMaterials[i].GetFloat("_AlphaClip") == 1 ? "开启" : "关闭";
            }
            else if (setting.newMaterials[i].shader.name.Contains("AlphaTest"))
            {
                AlphaClip = "开启";
            }
            else
            {
                AlphaClip = "无";
            }
            
            GUILayout.Label("AlphaClipping" + AlphaClip, GUILayout.Width(120));
            setting.newMaterials[i] = (Material)EditorGUILayout.ObjectField(setting.newMaterials[i], typeof(Material), false);
            //setting.materialQueue[i] = EditorGUILayout.IntField(setting.materialQueue[i]);
            GUILayout.EndHorizontal();
        }
    }
    public void ShowAssetsPopupMenu(Rect buttonRect, SerializedProperty serializedProperty)
    {
        GenericMenu genericMenu = new GenericMenu();
        string objPath = (serializedProperty.objectReferenceValue != null) ? AssetDatabase.GetAssetPath(serializedProperty.objectReferenceValue) : "";
        genericMenu.AddItem(defaultParameters, "" == objPath, AssetPopupMenuCallback, new object[2]
            {
            "",
            serializedProperty
            });
        string searchFilter = "t:LightmapParameters";
        string[] guids = AssetDatabase.FindAssets(searchFilter);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            int index1 = path.LastIndexOf('/') + 1;
            int index2 = path.LastIndexOf('.');

            genericMenu.AddItem(new GUIContent(path.Substring(index1, index2 - index1)), path == objPath, AssetPopupMenuCallback, new object[2]
            {
            path,
            serializedProperty
            });
        }
        genericMenu.DropDown(buttonRect);


    }
    public void AssetPopupMenuCallback(object userData)
    {
        object[] array = userData as object[];
        string assetPath = (string)array[0];
        SerializedProperty serializedProperty = (SerializedProperty)array[1];
        serializedProperty.objectReferenceValue = AssetDatabase.LoadAssetAtPath<LightmapParameters>(assetPath);
        serializedObject.ApplyModifiedProperties();
    }
}
#else
public class LMParameterSetting : MonoBehaviour
{
}
#endif