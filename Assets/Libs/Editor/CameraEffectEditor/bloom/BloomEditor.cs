
using UnityEngine;
using UnityEditor;

namespace MyEffect
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MyEffect.Bloom))]
    public class BloomEditor : Editor
    {
        BloomGraphDrawer _graph;

        SerializedProperty _threshold;
        SerializedProperty _softKnee;
        SerializedProperty _radius;
        SerializedProperty _intensity;
        SerializedProperty _highQuality;
        SerializedProperty _antiFlicker;

        static GUIContent _textThreshold = new GUIContent("亮度阀值 (gamma空间)");
        static GUIContent _textSoftKnee = new GUIContent("阀值上下的缓动计算");
        static GUIContent _textQuality = new GUIContent("高品质（影响性能）");
        static GUIContent _textIntensity = new GUIContent("计算结果的混合因子");
        static GUIContent _textFlicker = new GUIContent("抗抖动");
        static GUIContent _textRadius = new GUIContent("溢出范围");

        void OnEnable()
        {
            _graph = new BloomGraphDrawer();
            _threshold = serializedObject.FindProperty("_threshold");
            _softKnee = serializedObject.FindProperty("_softKnee");
            _radius = serializedObject.FindProperty("_radius");
            _intensity = serializedObject.FindProperty("_intensity");
            _highQuality = serializedObject.FindProperty("_highQuality");
            _antiFlicker = serializedObject.FindProperty("_antiFlicker");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (!serializedObject.isEditingMultipleObjects) {
                EditorGUILayout.Space();
                _graph.Prepare((Bloom)target);
                _graph.DrawGraph();
                EditorGUILayout.Space();
            }

            EditorGUILayout.PropertyField(_threshold, _textThreshold);
            EditorGUILayout.PropertyField(_softKnee,_textSoftKnee);
            EditorGUILayout.PropertyField(_intensity, _textIntensity);
            EditorGUILayout.PropertyField(_radius, _textRadius);
            EditorGUILayout.PropertyField(_highQuality, _textQuality);
            EditorGUILayout.PropertyField(_antiFlicker, _textFlicker);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
