
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Playables;

[CustomEditor(typeof(GPUAnimationTrack)), CanEditMultipleObjects]
class GPUAnimTrackEditor : Editor
{


    SerializedProperty _attack_keyframe;
    SerializedProperty _cast_keyframe;
    SerializedProperty timelineType;
    SerializedProperty skillTarget;

    void OnEnable()
    {
        _attack_keyframe = serializedObject.FindProperty("template._attack_keyframe");
        _cast_keyframe = serializedObject.FindProperty("template._cast_keyframe");
        timelineType = serializedObject.FindProperty("timelineType");
        skillTarget = serializedObject.FindProperty("skillTarget");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if(timelineType.intValue == (int)TimelineType.FightSkill)
        {
            EditorGUILayout.LabelField("剧情类型：技能效果");
            EditorGUILayout.HelpBox($"timeline数据文件来自{PathDefs.ASSETS_PATH_CHARACTER}的为技能表现",MessageType.Info);

            ++EditorGUI.indentLevel;

            EditorGUILayout.LabelField("攻击时间点：");

            ++EditorGUI.indentLevel;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("数量：");
            _attack_keyframe.arraySize = EditorGUILayout.DelayedIntField(_attack_keyframe.arraySize, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();

            //Log.LogError($"{(serializedObject.targetObject as GPUAnimationTrack).playable.GetDuration().ToString()}");
            for (int i = 0; i < _attack_keyframe.arraySize; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i + 1}：");
                _attack_keyframe.GetArrayElementAtIndex(i).floatValue = EditorGUILayout.Slider(_attack_keyframe.GetArrayElementAtIndex(i).floatValue, i == 0 ? 0 : _attack_keyframe.GetArrayElementAtIndex(i - 1).floatValue, (float)(serializedObject.targetObject as GPUAnimationTrack).director.duration, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();
            }

            --EditorGUI.indentLevel;

            EditorGUILayout.LabelField("受击时间点：");

            ++EditorGUI.indentLevel;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("数量：");
            _cast_keyframe.arraySize = EditorGUILayout.DelayedIntField(_cast_keyframe.arraySize, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();
            for (int i = 0; i < _cast_keyframe.arraySize; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i + 1}：");
                _cast_keyframe.GetArrayElementAtIndex(i).floatValue = EditorGUILayout.Slider(_cast_keyframe.GetArrayElementAtIndex(i).floatValue, i == 0 ? 0 : _cast_keyframe.GetArrayElementAtIndex(i - 1).floatValue, (float)(serializedObject.targetObject as GPUAnimationTrack).director.duration, new GUILayoutOption[0]);

                EditorGUILayout.EndHorizontal();
            }
            --EditorGUI.indentLevel;

            (serializedObject.targetObject as GPUAnimationTrack).SkillTarget = EditorGUILayout.ObjectField(new GUIContent("技能目标:"), ((serializedObject.targetObject as GPUAnimationTrack).SkillTarget) as Object, typeof(ObjectBehaviourBase), true, new GUILayoutOption[0]) as ObjectBehaviourBase;
            EditorGUILayout.HelpBox($"技能目标仅编辑时预览使用，游戏中将以实际的战斗情况来自动设置目标，并且数量并不确定", MessageType.Info);
            --EditorGUI.indentLevel;
        }
        else
        {
            EditorGUILayout.LabelField("剧情类型：场景剧情");
            EditorGUILayout.HelpBox($"timeline数据文件来自{PathDefs.ASSETS_PATH_CHARACTER}的为技能表现", MessageType.Info);
        }






        serializedObject.ApplyModifiedProperties();

        //EditorGUILayout.PropertyField(_attack_keyframe,new GUIContent("攻击时间点:"), true, new GUILayoutOption[0]);
    }



}

