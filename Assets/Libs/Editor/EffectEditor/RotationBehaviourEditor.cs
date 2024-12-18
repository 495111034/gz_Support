using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(RotationBehaviour))]
class RotationBehaviourEditor : Editor
{
    SerializedProperty speed_x, speed_y, speed_z;

    static GUIContent txt_x = new GUIContent("X轴速度");
    static GUIContent txt_y = new GUIContent("Y轴速度");
    static GUIContent txt_z = new GUIContent("Z轴速度");

    private float time = 0;

    void OnEnable()
    {
        speed_x = serializedObject.FindProperty("speed_x");
        speed_y = serializedObject.FindProperty("speed_y");
        speed_z = serializedObject.FindProperty("speed_z");
    }



    private void Awake()
    {
        time = Time.realtimeSinceStartup;
        EditorApplication.update += UpdateHandler;

        RotationBehaviour player = target as RotationBehaviour;
        if (player != null && !Application.isPlaying)
        {
            player.Init();
        }
    }


    private void OnDestroy()
    {
        EditorApplication.update -= UpdateHandler;
    }

    private void UpdateHandler()
    {
        if (AssetDatabase.Contains(target))
        {
            return;
        }

        float deltaTime = Time.realtimeSinceStartup - time;
        time = Time.realtimeSinceStartup;

        RotationBehaviour player = target as RotationBehaviour;
        if (player != null)
        {
            player.EditorUpdate(deltaTime);
        }

        foreach (var sceneView in SceneView.sceneViews)
        {
            if (sceneView is SceneView)
            {
                (sceneView as SceneView).Repaint();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(txt_x);
        speed_x.floatValue =  EditorGUILayout.Slider(speed_x.floatValue, -100, 100, new GUILayoutOption[0]);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(txt_y);
        speed_y.floatValue = EditorGUILayout.Slider(speed_y.floatValue, -100, 100, new GUILayoutOption[0]);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(txt_z);
        speed_z.floatValue = EditorGUILayout.Slider(speed_z.floatValue, -100, 100, new GUILayoutOption[0]);
        EditorGUILayout.EndHorizontal();
      

        serializedObject.ApplyModifiedProperties();
    }
}
