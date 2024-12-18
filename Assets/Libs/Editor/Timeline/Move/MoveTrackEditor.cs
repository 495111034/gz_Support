using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;


[CustomEditor(typeof(MoveTrack)), CanEditMultipleObjects]
class MoveTrackEditor : Editor
{
    static List<string> MoverTypeNamesSkill =new List<string>()
    {
         "攻击者",
         "攻击者的随从",
         "攻击目标",
    };

    static List<string> MoverTypeNamesScenario = new List<string>()
    {
         "当前主角",
         "其它角色",
         "NPC",
         "创建临时角色（完成删除）",
         "创建临时NPC（完成删除）",
        "创建新怪物（不删除）",
        "创建特效或物体（完成删除）",
        "创建新掉落物（不删除）"
    };   

    SerializedProperty moverType;
    GUIContent m_moverType;

    SerializedProperty timelineType;

    void OnEnable()
    {       
        timelineType = serializedObject.FindProperty("timelineType");
        moverType = serializedObject.FindProperty("template.moverType");

        m_moverType = new GUIContent("移动者类型");


    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

       
        if (timelineType.intValue == (int)TimelineType.FightSkill)
        {
            EditorGUILayout.LabelField("剧情类型：技能效果");

            var idx = moverType.enumValueIndex;
            var selectId = 0;
            switch ((MoveTrackMixer.MoverType)idx)
            {
                case MoveTrackMixer.MoverType.Attacker:
                    selectId = 0;
                    break;
                case MoveTrackMixer.MoverType.Pet:
                    selectId = 1;
                    break;
                case MoveTrackMixer.MoverType.FightTarget:
                    selectId = 2;
                    break;               
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_moverType);
            selectId = EditorGUILayout.Popup(selectId, MoverTypeNamesSkill.ToArray());
            EditorGUILayout.EndHorizontal();
            switch (selectId)
            {
                case 0:
                    idx = (int)MoveTrackMixer.MoverType.Attacker;
                    break;
                case 1:
                    idx = (int)MoveTrackMixer.MoverType.Pet;
                    break;
                case 2:
                    idx = (int)MoveTrackMixer.MoverType.FightTarget;
                    break;

            }          
            moverType.enumValueIndex = idx;

        }
        else
        {
            EditorGUILayout.LabelField("剧情类型：场景剧情");

            var idx = moverType.enumValueIndex;
            if (idx < (int)MoveTrackMixer.MoverType.CurrentMainRole) idx = (int)MoveTrackMixer.MoverType.CurrentMainRole;
            var selectId = 0;
            switch ((MoveTrackMixer.MoverType)idx)
            {
                case MoveTrackMixer.MoverType.CurrentMainRole:
                    selectId = 0;
                    break;
                case MoveTrackMixer.MoverType.OtherRole:
                    selectId = 1;
                    break;
                case MoveTrackMixer.MoverType.Npc:
                    selectId = 2;
                    break;
                case MoveTrackMixer.MoverType.CreateTmpRole:
                    selectId = 3;
                    break;
                case MoveTrackMixer.MoverType.CreateTmpNpc:
                    selectId = 4;
                    break;
                case MoveTrackMixer.MoverType.CreateNewMonster:
                    selectId = 5;
                    break;
                case MoveTrackMixer.MoverType.CreateTmpObject:
                    selectId = 6;
                    break;
                case MoveTrackMixer.MoverType.CreateItem:
                    selectId = 7;
                    break;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_moverType);
            selectId = EditorGUILayout.Popup(selectId, MoverTypeNamesScenario.ToArray());
            EditorGUILayout.EndHorizontal();
            switch (selectId)
            {
                case 0:
                    idx = (int)MoveTrackMixer.MoverType.CurrentMainRole;
                    break;
                case 1:
                    idx = (int)MoveTrackMixer.MoverType.OtherRole;
                    break;
                case 2:
                    idx = (int)MoveTrackMixer.MoverType.Npc;
                    break;
                case 3:
                    idx = (int)MoveTrackMixer.MoverType.CreateTmpRole;
                    break;
                case 4:
                    idx = (int)MoveTrackMixer.MoverType.CreateTmpNpc;
                    break;
                case 5:
                    idx = (int)MoveTrackMixer.MoverType.CreateNewMonster;
                    break;
                case 6:
                    idx = (int)MoveTrackMixer.MoverType.CreateTmpObject;
                    break;
                case 7:
                    idx = (int)MoveTrackMixer.MoverType.CreateItem;
                    break;                

            }
            moverType.enumValueIndex = idx;
        }

        

        serializedObject.ApplyModifiedProperties();
    }
}

