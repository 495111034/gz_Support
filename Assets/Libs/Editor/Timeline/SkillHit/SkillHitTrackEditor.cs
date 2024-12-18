using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;


[CustomEditor(typeof(SkillHitTrack)), CanEditMultipleObjects]
public class SkillHitTrackEditor : Editor
{
    // SerializedProperty spTrackTarget;
    GUIContent txtAttacker;
    GUIContent txtTrackTarget;
    GUIContent txtTargetDesc;

    void OnEnable()
    {
        // spTrackTarget = serializedObject.FindProperty("trackTargetObject");
        txtAttacker = new GUIContent("施法者");
        txtTrackTarget = new GUIContent("受击者");
        txtTargetDesc = new GUIContent("施法者和受击者为编辑时预览目标，动行时的受击者和数量以游戏中战斗数据为准");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        (serializedObject.targetObject as SkillHitTrack).template.attacker = EditorGUILayout.ObjectField(txtAttacker, ((serializedObject.targetObject as SkillHitTrack).template.attacker as ObjectBehaviourBase), typeof(ObjectBehaviourBase), true, new GUILayoutOption[0]) as ObjectBehaviourBase;
        (serializedObject.targetObject as SkillHitTrack).template.trackTargetObject = EditorGUILayout.ObjectField(txtTrackTarget, ((serializedObject.targetObject as SkillHitTrack).template.trackTargetObject as ObjectBehaviourBase), typeof(ObjectBehaviourBase), true, new GUILayoutOption[0]) as  ObjectBehaviourBase;
        //EditorGUILayout.PropertyField(spTrackTarget, txtTrackTarget,true, new GUILayoutOption[0]);
        EditorGUILayout.HelpBox(txtTargetDesc);

        serializedObject.ApplyModifiedProperties();
    }
}

