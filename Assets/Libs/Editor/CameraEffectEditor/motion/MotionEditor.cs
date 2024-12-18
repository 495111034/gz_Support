
#pragma warning disable 414

// Show fancy graphs
#define SHOW_GRAPHS

using UnityEngine;
using UnityEditor;

using Motion = MyEffect.Motion;

namespace MyEffect
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Motion))]
    public class MotionEditor : Editor
    {
        MotionGraphDrawer _graph;

        SerializedProperty _shutterAngle;
        SerializedProperty _sampleCount;
        SerializedProperty _frameBlending;

        [SerializeField] Texture2D _blendingIcon;

        static GUIContent _textStrength = new GUIContent("混合强度：");
        static GUIContent _textShutterAngle = new GUIContent("快门角度（越大暴光越长）：");
        static GUIContent _textSampleAmount  = new GUIContent("采样点数（性能与效果相关）：");
        void OnEnable()
        {
            _shutterAngle = serializedObject.FindProperty("_shutterAngle");
            _sampleCount = serializedObject.FindProperty("_sampleCount");
            _frameBlending = serializedObject.FindProperty("_frameBlending");
        }

        public override void OnInspectorGUI()
        {
            if (_graph == null) _graph = new MotionGraphDrawer(_blendingIcon);

            serializedObject.Update();

            EditorGUILayout.LabelField("相机快门模拟：", EditorStyles.boldLabel);

            #if SHOW_GRAPHS
            _graph.DrawShutterGraph(_shutterAngle.floatValue);
            #endif

            EditorGUILayout.PropertyField(_shutterAngle,_textShutterAngle);
            EditorGUILayout.PropertyField(_sampleCount, _textSampleAmount);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("多帧混合设置：", EditorStyles.boldLabel);

            #if SHOW_GRAPHS
            _graph.DrawBlendingGraph(_frameBlending.floatValue);
            #endif

            EditorGUILayout.PropertyField(_frameBlending, _textStrength);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
