using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MoveClip)), CanEditMultipleObjects]
class MoveClipEditor : Editor
{
    static List<string> txtStartTypeList = new List<string>()
    {
        "移动到位置","移动到物体","向前移动","向左转","向右转"
    };

    static List<string> txtMoveStartPosType = new List<string>()
    {
        "当前位置","指定的位置"
    };




    SerializedProperty startType;
    SerializedProperty moveType;
    SerializedProperty startPos;
    SerializedProperty destPos;
    SerializedProperty targetObject;
    SerializedProperty destObjectDisOffset;
    SerializedProperty destObjectHeightOffset;
    SerializedProperty destAngleOffset;
    SerializedProperty forwardToDestination;
    SerializedProperty ignoreHeightDiff;
    SerializedProperty allowMotionBlur;
    SerializedProperty moveEaseType1;
    SerializedProperty moveEaseType2;
    SerializedProperty _startimeType;
    SerializedProperty _flySpeed;
    SerializedProperty _posByGame;
    SerializedProperty forwardIgnoreHeightDiff;
    SerializedProperty s_MovePathTargets;


    GUIContent m_moveType;
    GUIContent m_startType;
    GUIContent m_startPos;
    GUIContent m_destPos;
    GUIContent m_destObjectDisOffset;
    GUIContent m_destObjectHeightOffset;
    GUIContent m_destAngleOffset;
    GUIContent m_forwardToDestination;
    GUIContent m_ignoreHeightDiff;
    GUIContent m_allowMotionBlur;
    GUIContent m_moveEaseType1;
    GUIContent m_moveEaseType2;
    GUIContent m__startimeType;
    GUIContent m__flySpeed;
    GUIContent m__posByGame;
    GUIContent m_targetObject;
    GUIContent m_targetObjectText;
    GUIContent m_forwardIgnoreHeightDiff;
    GUIContent g_MovePathTargets;

    void OnEnable()
    {
        startType = serializedObject.FindProperty("templete.startType");
        moveType = serializedObject.FindProperty("templete.moveType");
        startPos = serializedObject.FindProperty("templete.startPos");
        destPos = serializedObject.FindProperty("templete.destPos");
        destObjectDisOffset = serializedObject.FindProperty("templete.destObjectDisOffset");
        destObjectHeightOffset = serializedObject.FindProperty("templete.destObjectHeightOffset");
        destAngleOffset = serializedObject.FindProperty("templete.destAngleOffset");
        forwardToDestination = serializedObject.FindProperty("templete.forwardToDestination");
        ignoreHeightDiff = serializedObject.FindProperty("templete.ignoreHeightDiff");
        allowMotionBlur = serializedObject.FindProperty("templete.allowMotionBlur");
        moveEaseType1 = serializedObject.FindProperty("templete.moveEaseType1");
        moveEaseType2 = serializedObject.FindProperty("templete.moveEaseType2");
        _startimeType = serializedObject.FindProperty("templete._startimeType");
        _flySpeed = serializedObject.FindProperty("templete._flySpeed");
        _posByGame = serializedObject.FindProperty("templete._posByGame");
        targetObject = serializedObject.FindProperty("templete.targetObject");
        forwardIgnoreHeightDiff = serializedObject.FindProperty("templete.forwardIgnoreHeightDiff");
        s_MovePathTargets = serializedObject.FindProperty("templete.MovePathTargets");


        m_moveType = new GUIContent("移动方式");
        m_startType = new GUIContent("开始位置类型");
        m_startPos = new GUIContent("开始位置");
        m_destPos = new GUIContent("目标位置");
        m_destObjectDisOffset = new GUIContent("距离偏移");
        m_destObjectHeightOffset = new GUIContent("高度偏移");
        m_destAngleOffset = new GUIContent("角度");
        m_forwardToDestination = new GUIContent("朝向目标");
        m_ignoreHeightDiff = new GUIContent("忽略高度差");
        m_allowMotionBlur = new GUIContent("开启相机运动模式");
        m_moveEaseType1 = new GUIContent("缓动计算方式");
        m_moveEaseType2 = new GUIContent("缓动计算方式2");
        m__startimeType = new GUIContent("开始时间点计算方式");
        m__flySpeed = new GUIContent("开始前准备速度");
        m__posByGame = new GUIContent("游戏中定义位置");
        m_targetObject = new GUIContent("目标物体");
        m_targetObjectText = new GUIContent("此对象会在游戏中设置，此处仅供预览使用");
        m_forwardIgnoreHeightDiff = new GUIContent("朝向忽略高度差");
        g_MovePathTargets = new GUIContent("推动路径目标");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(m_startType);
        var startPosTypeIdx = Mathf.Clamp(startType.enumValueIndex, 0, txtMoveStartPosType.Count - 1);
        startPosTypeIdx = EditorGUILayout.Popup(startPosTypeIdx, txtMoveStartPosType.ToArray(), new GUILayoutOption[0]);
        startType.enumValueIndex = startPosTypeIdx;
        EditorGUILayout.EndHorizontal();
        if (startPosTypeIdx == (int)MoveClipPlayableData.MoveStartType.SettingPos)
        {
            ++EditorGUI.indentLevel;
            EditorGUILayout.PropertyField(startPos, m_startPos, true, new GUILayoutOption[0]);
            --EditorGUI.indentLevel;
        }
       
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(m_moveType);
        var moveTypeIndx = Mathf.Clamp(moveType.enumValueIndex, 0, txtStartTypeList.Count - 1);
        moveTypeIndx = EditorGUILayout.Popup(moveTypeIndx, txtStartTypeList.ToArray(), new GUILayoutOption[0]);
        moveType.enumValueIndex = moveTypeIndx;
        EditorGUILayout.EndHorizontal();
        if ((target as MoveClip).OwningClip != null)
        {
            (target as MoveClip).OwningClip.displayName = txtStartTypeList[moveTypeIndx];
        }
        ++EditorGUI.indentLevel;
        if (moveTypeIndx == (int)MoveClipPlayableData.MoveDestinationType.MoveToTargetObjectAndOffset)
        {
            (serializedObject.targetObject as MoveClip).templete.targetObject = EditorGUILayout.ObjectField(m_targetObject, (serializedObject.targetObject as MoveClip).templete.targetObject,typeof(GameObject),true,  new GUILayoutOption[0]) as GameObject;
            EditorGUILayout.HelpBox(m_targetObjectText);
        }
        else if(moveTypeIndx == (int)MoveClipPlayableData.MoveDestinationType.MoveToTargetPosition)
        {
            EditorGUILayout.PropertyField(destPos, m_destPos, new GUILayoutOption[0]);
        }
        else if(moveTypeIndx == (int)MoveClipPlayableData.MoveDestinationType.RotationLeft || moveTypeIndx == (int)MoveClipPlayableData.MoveDestinationType.RotationRight)
        {
            EditorGUILayout.PropertyField(destAngleOffset, m_destAngleOffset, new GUILayoutOption[0]);
        }
        --EditorGUI.indentLevel;

        if (moveTypeIndx == (int)MoveClipPlayableData.MoveDestinationType.MoveToTargetObjectAndOffset
            || moveTypeIndx == (int)MoveClipPlayableData.MoveDestinationType.MoveToTargetPosition 
            || moveTypeIndx == (int)MoveClipPlayableData.MoveDestinationType.MoveForward)
        {
            EditorGUILayout.LabelField(m_moveEaseType1);
            ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(moveEaseType1,new GUIContent("EasyType:"));
                EditorGUILayout.PropertyField(moveEaseType2, new GUIContent("OutType:"));
            --EditorGUI.indentLevel;

            EditorGUILayout.PropertyField(destObjectDisOffset, m_destObjectDisOffset);
            EditorGUILayout.PropertyField(ignoreHeightDiff, m_ignoreHeightDiff, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(destObjectHeightOffset, m_destObjectHeightOffset, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(forwardToDestination, m_forwardToDestination);
           
        }

        ++EditorGUI.indentLevel;
        if (forwardToDestination.boolValue)
        {
            EditorGUILayout.PropertyField(forwardIgnoreHeightDiff, m_forwardIgnoreHeightDiff);
        }
        --EditorGUI.indentLevel;

        EditorGUILayout.PropertyField(s_MovePathTargets, g_MovePathTargets);

        serializedObject.ApplyModifiedProperties();
    }

}
