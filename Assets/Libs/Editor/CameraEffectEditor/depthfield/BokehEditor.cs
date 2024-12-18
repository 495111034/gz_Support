
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MyEffect
{
    [CanEditMultipleObjects, CustomEditor(typeof(DepthField))]
    public class BokehEditor : Editor
    {
        SerializedProperty _pointOfFocus;
        SerializedProperty _focusDistance;
        SerializedProperty _fNumber;
        SerializedProperty _useCameraFov;
        SerializedProperty _focalLength;
        SerializedProperty _kernelSize;
        SerializedProperty _visualize;
        SerializedProperty _pointOffset;

        static GUIContent _labelPointOfFocus = new GUIContent(
            "焦点",
            "焦点对象的transform"
        );

        static GUIContent _labelFocusDistance = new GUIContent(
            "距离",
            "到焦点的距离  (仅在没有指定焦点时)."
        );

        static GUIContent _labelFNumber = new GUIContent(
            "光圈 (f-stop)此值越小，景深越大",
            "孔径 (类似 f-stop 或 f-number). 此值越小，深度越浅."
        );

        static GUIContent _labelUseCameraFov = new GUIContent(
            "使用相机FOV",
            "通过相机视野计算焦距"
        );

        static GUIContent _labelFocalLength = new GUIContent(
            "焦距(mm)",
            "镜片到胶片的距离. 数值越大，深度越浅."
        );

        static GUIContent _labelKernelSize = new GUIContent(
            "画质",
            "计算景深效果的内核大小, 确定最大半径.同时也是影响性能的重要因素 (数值越大，需要的GPU时间越长)."
        );

        static GUIContent _labelVisualize = new GUIContent(
            "模拟显示",
            "将深度值虚化为红色(焦点)，, 绿色 (远处) 和蓝色 (近处)."
        );
        static GUIContent _labelPointOffset = new GUIContent(
            "焦点对象偏移量",
            "适用于有焦点对象的情况"
        );

        public static List<string> _kSizeText = new List<string>()
        {
            "低","中等","好","最好"
        };

        void OnEnable()
        {
            _pointOfFocus = serializedObject.FindProperty("____pointOfFocus");
            _focusDistance = serializedObject.FindProperty("_focusDistance");
            _fNumber = serializedObject.FindProperty("_fNumber");
            _useCameraFov = serializedObject.FindProperty("_useCameraFov");
            _focalLength = serializedObject.FindProperty("_focalLength");
            _kernelSize = serializedObject.FindProperty("_kernelSize");
            _visualize = serializedObject.FindProperty("_visualize");
            _pointOffset = serializedObject.FindProperty("_pointOffset");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Point of focus
            EditorGUILayout.PropertyField(_pointOfFocus, _labelPointOfFocus);
            if (_pointOfFocus.hasMultipleDifferentValues || _pointOfFocus.objectReferenceValue == null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_focusDistance, _labelFocusDistance);
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_pointOffset, _labelPointOffset);
                EditorGUI.indentLevel--;
            }

            // Aperture
            EditorGUILayout.PropertyField(_fNumber, _labelFNumber);

            // Focal Length
            EditorGUILayout.PropertyField(_useCameraFov, _labelUseCameraFov);

            if (_useCameraFov.hasMultipleDifferentValues || !_useCameraFov.boolValue)
            {
                if (_focalLength.hasMultipleDifferentValues)
                {
                    EditorGUILayout.PropertyField(_focalLength);
                }
                else
                {
                    EditorGUI.BeginChangeCheck();

                    var f = _focalLength.floatValue * 1000;
                    f = EditorGUILayout.Slider(_labelFocalLength, f, 10.0f, 300.0f);

                    if (EditorGUI.EndChangeCheck())
                        _focalLength.floatValue = f / 1000;
                }
            }

            // Kernel Size
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_labelKernelSize);
            var selectIdx = Mathf.Clamp(_kernelSize.enumValueIndex, 0, _kSizeText.Count - 1);
            selectIdx = EditorGUILayout.Popup(selectIdx, _kSizeText.ToArray());
            _kernelSize.enumValueIndex = selectIdx;
            EditorGUILayout.EndHorizontal();

            // Visualize
            EditorGUILayout.PropertyField(_visualize, _labelVisualize);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
