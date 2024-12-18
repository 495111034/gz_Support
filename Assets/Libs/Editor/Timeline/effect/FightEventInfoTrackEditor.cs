using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;



[CustomEditor(typeof(FightEventInfoTrack)), CanEditMultipleObjects]
public class FightEventInfoTrackEditor : Editor
{
    GUIContent txtAttacker;
    GUIContent txtContent;

    SerializedProperty eventTargetType;

    List<string> txtTargetTypeTitle = new List<string>()
    {
        "攻击者","攻击对象","攻击者的随从","其它对象"
    };

    void OnEnable()
    {
        txtAttacker = new GUIContent("通知对象类型");
        txtContent = new GUIContent("战斗其它效果，如结束时间点、震屏等");
        eventTargetType = serializedObject.FindProperty("_eventTargetType");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField(txtContent);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(txtAttacker);
        var selectIdx = Mathf.Clamp(eventTargetType.enumValueIndex, 0, txtTargetTypeTitle.Count - 1);
        selectIdx = EditorGUILayout.Popup(selectIdx, txtTargetTypeTitle.ToArray());
        eventTargetType.enumValueIndex = selectIdx;
        EditorGUILayout.EndHorizontal();

        //(serializedObject.targetObject as FightEventInfoTrack).trackTargetObject = EditorGUILayout.ObjectField(txtAttacker, ((serializedObject.targetObject as FightEventInfoTrack).trackTargetObject as ObjectBehaviourBase), typeof(ObjectBehaviourBase), true, new GUILayoutOption[0]) as ObjectBehaviourBase;
        serializedObject.ApplyModifiedProperties();
    }


}

