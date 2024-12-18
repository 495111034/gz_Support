using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(Transform))] 
class TransformInspector : Editor
{
    private const float FIELD_WIDTH = 212.0f;
    private const bool WIDE_MODE = true;

    private SerializedProperty positionProperty;
    private SerializedProperty rotationProperty;
    private SerializedProperty scaleProperty;

    private const float POSITION_MAX = 100000.0f;


    private static GUIContent positionGUIContent = new GUIContent(LocalString("Position"), LocalString("相对于父对象的定位"));
    private static GUIContent rotationGUIContent = new GUIContent(LocalString("Rotation"), LocalString("相对于父对象的旋转"));
    private static GUIContent scaleGUIContent = new GUIContent(LocalString("Scale"), LocalString("相对于父对象的缩放"));

    private static string positionWarningText = LocalString("由于浮点数的精度限制，建议使用较小的数值。");

    private void Awake()
    {
        ShowShaderHight();
    }
    private void OnDestroy()
    {

    }

    private static string LocalString(string text)
    {
        return text;
        //return LocalizationDatabase.GetLocalizedString(text);
    }

    public override void OnInspectorGUI()
    {

        DrawTransform();

        if (!Application.isPlaying)
        {
           

            //if (GUILayout.Button("合并模型"))
            //{
            //    CombineUtils.CombineMesh((target as Transform).gameObject);
            //}
            if (GUILayout.Button("高配显示"))
            {
                ShowShaderHight();
            }
            if (GUILayout.Button("中配显示"))
            {
                ShowShaderMidde();
            }
            if (GUILayout.Button("低配显示"))
            {
                ShowShaderLow();
            }

            Shader.DisableKeyword("ENAGLE_SELECTED_TARGET");
            Shader.EnableKeyword("SELECTED_TARGET_OFF");
        }
    }

    public void OnEnable()
    {
        this.positionProperty = this.serializedObject.FindProperty("m_LocalPosition");
        this.rotationProperty = this.serializedObject.FindProperty("m_LocalRotation");
        this.scaleProperty = this.serializedObject.FindProperty("m_LocalScale");
    }

    // 绘制 Transform, http://wiki.unity3d.com/index.php?title=TransformInspector
    void DrawTransform()
    {
        Transform t = (Transform)target;

        EditorGUIUtility.wideMode = TransformInspector.WIDE_MODE;
        EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - TransformInspector.FIELD_WIDTH; // align field to right of inspector

        this.serializedObject.Update();

        EditorGUILayout.PropertyField(this.positionProperty, positionGUIContent);
        this.RotationPropertyField(this.rotationProperty, rotationGUIContent);
        EditorGUILayout.PropertyField(this.scaleProperty, scaleGUIContent);

        if (!ValidatePosition(((Transform)this.target).position))
        {
            EditorGUILayout.HelpBox(positionWarningText, MessageType.Warning);
        }

        this.serializedObject.ApplyModifiedProperties();
    }

    private bool ValidatePosition(Vector3 position)
    {
        if (Mathf.Abs(position.x) > TransformInspector.POSITION_MAX) return false;
        if (Mathf.Abs(position.y) > TransformInspector.POSITION_MAX) return false;
        if (Mathf.Abs(position.z) > TransformInspector.POSITION_MAX) return false;
        return true;
    }

    private void RotationPropertyField(SerializedProperty rotationProperty, GUIContent content)
    {
        Transform transform = (Transform)this.targets[0];
        Quaternion localRotation = transform.localRotation;
        foreach (UnityEngine.Object t in (UnityEngine.Object[])this.targets)
        {
            if (!SameRotation(localRotation, ((Transform)t).localRotation))
            {
                EditorGUI.showMixedValue = true;
                break;
            }
        }

        EditorGUI.BeginChangeCheck();

        Vector3 eulerAngles = EditorGUILayout.Vector3Field(content, localRotation.eulerAngles);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObjects(this.targets, "Rotation Changed");
            foreach (UnityEngine.Object obj in this.targets)
            {
                Transform t = (Transform)obj;
                t.localEulerAngles = eulerAngles;
            }
            rotationProperty.serializedObject.SetIsDifferentCacheDirty();
        }

        EditorGUI.showMixedValue = false;
    }

    private bool SameRotation(Quaternion rot1, Quaternion rot2)
    {
        if (rot1.x != rot2.x) return false;
        if (rot1.y != rot2.y) return false;
        if (rot1.z != rot2.z) return false;
        if (rot1.w != rot2.w) return false;
        return true;
    }

    void ShowShaderLow()
    {
        if (Application.isPlaying) return;

        Shader.EnableKeyword("MAININFO_OFF");
        Shader.DisableKeyword("ENAGLE_MAINROLEINFO");


        //高光
        Shader.DisableKeyword("ENABLE_GLOSS");
        Shader.EnableKeyword("GLOSS_OFF");

        //镜面
        Shader.DisableKeyword("MIRROR_WAVE");
        Shader.EnableKeyword("MIRROR_SIMPLE");

        //水面
        Shader.DisableKeyword("WATER_WAVE");
        Shader.EnableKeyword("WATER_SIMPLE");

        //角色

        //自发光
        Shader.DisableKeyword("ENABLE_EMISSION");
        Shader.EnableKeyword("EMISSION_OFF");
        //溜光
        Shader.DisableKeyword("ENABLE_HALO");
        Shader.EnableKeyword("HALO_OFF");

    }




    void ShowShaderHight()
    {
      
        if (Application.isPlaying) return;

        Shader.EnableKeyword("MAININFO_OFF");
        Shader.DisableKeyword("ENAGLE_MAINROLEINFO");

        //普通物体高光
        Shader.EnableKeyword("ENABLE_GLOSS");
        Shader.DisableKeyword("GLOSS_OFF");

        //镜面
        Shader.EnableKeyword("MIRROR_WAVE");
        Shader.DisableKeyword("MIRROR_SIMPLE");

        //水面
        Shader.EnableKeyword("WATER_WAVE");
        Shader.DisableKeyword("WATER_SIMPLE");

        //自发光
        Shader.EnableKeyword("ENABLE_EMISSION");
        Shader.DisableKeyword("EMISSION_OFF");

        //溜光
        Shader.EnableKeyword("ENABLE_HALO");
        Shader.DisableKeyword("HALO_OFF");
    }
    void ShowShaderMidde()
    {
        if (Application.isPlaying) return;

        Shader.EnableKeyword("MAININFO_OFF");
        Shader.DisableKeyword("ENAGLE_MAINROLEINFO");

        //高光
        Shader.EnableKeyword("ENABLE_GLOSS");
        Shader.DisableKeyword("GLOSS_OFF");

        //镜面
        Shader.EnableKeyword("MIRROR_WAVE");
        Shader.DisableKeyword("MIRROR_SIMPLE");

        //水面
        Shader.DisableKeyword("WATER_WAVE");
        Shader.EnableKeyword("WATER_SIMPLE");

        //自发光
        Shader.EnableKeyword("ENABLE_EMISSION");
        Shader.DisableKeyword("EMISSION_OFF");

        //溜光
        Shader.DisableKeyword("ENABLE_HALO");
        Shader.EnableKeyword("HALO_OFF");
    }

}
