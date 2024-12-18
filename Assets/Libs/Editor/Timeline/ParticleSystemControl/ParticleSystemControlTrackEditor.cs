
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;



[CustomEditor(typeof(ParticleSystemControlTrack)), CanEditMultipleObjects]
class ParticleSystemControlTrackEditor : Editor
{
    SerializedProperty _attacker;
    SerializedProperty _snapTarget;
    SerializedProperty timelineType;
    SerializedProperty _randomSeed;
    SerializedProperty _snapTargetType;
    SerializedProperty _otherSnapTargetName;
    SerializedProperty _snapTargetId;
    SerializedProperty spMutiInstance;
    SerializedProperty _snapMount;
    SerializedProperty _mountOtherName;
    SerializedProperty _posByGame;
    SerializedProperty _rotationByGame;
    SerializedProperty independent;
    SerializedProperty _targetZhenYing;
    GUIContent txtMutiInstance;
    GUIContent txt_independent ;



    static public List<string> targetTypeList = new List<string>()
    {
        "施法者自己","施法目标","宠物","坐骑","场景中物体","游戏中传入位置","圣物","友方目标","敌方目标","施法者守护灵"
    };

    static public List<string> targetType2List = new List<string>()
    {
        "当前主角","NPC","怪物","座骑","场景物体","指定位置","其它","Boss登场"
    };

    static public List<string> _mountTargetList = new List<string>()
    {
        "无","左手","右手","双手","头发","翅膀","坐骑","其它"
    };

    static public List<string> _zhenyingList = new List<string>()
    {
        "不限制","我方","敌方"
    };



    void OnEnable()
    {
        independent = serializedObject.FindProperty("_independent");
        _attacker = serializedObject.FindProperty("template.attacker");
        _snapTarget = serializedObject.FindProperty("template.snapTarget");
        _randomSeed = serializedObject.FindProperty("template.randomSeed");
        _snapTargetType = serializedObject.FindProperty("template._snapTartetType");
        _snapTargetId = serializedObject.FindProperty("template._snapTargetId");
        _otherSnapTargetName = serializedObject.FindProperty("template._otherSnapTargetName");
        spMutiInstance = serializedObject.FindProperty("template._mutiInstance");
        _snapMount = serializedObject.FindProperty("template._snapMount");
        _mountOtherName = serializedObject.FindProperty("template._mountOtherName");
        timelineType = serializedObject.FindProperty("timelineType");
        _posByGame = serializedObject.FindProperty("template._posByGame");
        _rotationByGame = serializedObject.FindProperty("template._rotationByGame");
        _targetZhenYing = serializedObject.FindProperty("template._targetZhenYing");

        txtMutiInstance = new GUIContent("允许多实例(每个目标一个实例)：");
        txt_independent = new GUIContent("独立timeline对象：");

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (timelineType.intValue == (int)TimelineType.FightSkill)
        {
            EditorGUILayout.LabelField("剧情类型：技能效果");
            EditorGUILayout.PropertyField(_attacker, new GUIContent("施法者:"), new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(_snapTarget, new GUIContent("跟随目标:"), new GUILayoutOption[0]);

            EditorGUILayout.HelpBox(new GUIContent("施法者和施法目标仅编辑时预览使用，游戏中将以实际战斗数据来自动设置目标"));

            EditorGUILayout.PropertyField(spMutiInstance, txtMutiInstance, new GUILayoutOption[0]);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("目标类型：");
            var selectIdx = Mathf.Clamp(_snapTargetType.enumValueIndex, 0, targetTypeList.Count - 1);
            selectIdx = EditorGUILayout.Popup(selectIdx, targetTypeList.ToArray());
            _snapTargetType.enumValueIndex = selectIdx;
            (serializedObject.targetObject as ParticleSystemControlTrack).template.snapTartetType = (ParticleSystemControlMixer.SnapTargetType)selectIdx;
            EditorGUILayout.EndHorizontal();

            if (selectIdx == (int)ParticleSystemControlMixer.SnapTargetType.otherObj)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(_otherSnapTargetName, new GUIContent("物体名称:"), new GUILayoutOption[0]);
                --EditorGUI.indentLevel;
            }
            else if (selectIdx == (int)ParticleSystemControlMixer.SnapTargetType.target)
            {
                if (spMutiInstance.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("目标阵营：");
                    selectIdx = Mathf.Clamp(_targetZhenYing.intValue, 0, _zhenyingList.Count - 1);
                    selectIdx = EditorGUILayout.Popup(selectIdx, _zhenyingList.ToArray());
                    _targetZhenYing.intValue = selectIdx;
                    (serializedObject.targetObject as ParticleSystemControlTrack).template.targetZhenYing = selectIdx;
                    EditorGUILayout.EndHorizontal();
                }
            }            
        }
        else
        {
            EditorGUILayout.LabelField("剧情类型：场景剧情");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("跟随目标类型：");
            var selectIdx = Mathf.Clamp(_snapTargetType.enumValueIndex, 0, targetType2List.Count - 1);
            selectIdx = EditorGUILayout.Popup(selectIdx, targetType2List.ToArray());
            _snapTargetType.enumValueIndex = selectIdx;
            (serializedObject.targetObject as ParticleSystemControlTrack).template.snapTartetType = (ParticleSystemControlMixer.SnapTargetType)selectIdx;
            EditorGUILayout.EndHorizontal();

            switch(selectIdx)
            {
                case (int)ParticleSystemControlMixer.SnapTargetType.self://npc
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(_snapTarget, new GUIContent("主角:"), new GUILayoutOption[0]);
                    --EditorGUI.indentLevel;
                    break;
                case (int)ParticleSystemControlMixer.SnapTargetType.target://npc
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(_snapTargetId, new GUIContent("Npc ID:"), new GUILayoutOption[0]);
                    --EditorGUI.indentLevel;
                    break;
                case (int)ParticleSystemControlMixer.SnapTargetType.pet://怪物
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(_snapTargetId, new GUIContent("怪物id:"), new GUILayoutOption[0]);
                    --EditorGUI.indentLevel;
                    break;
                case (int)ParticleSystemControlMixer.SnapTargetType.otherObj://场景物体
                    ++EditorGUI.indentLevel;
                    var objName = GetGameobjectName();
                    if(!string.IsNullOrEmpty(objName))
                    {
                        _otherSnapTargetName.stringValue = objName;
                    }
                    EditorGUILayout.PropertyField(_otherSnapTargetName, new GUIContent("物体名称:"), new GUILayoutOption[0]);
                    --EditorGUI.indentLevel;
                    break;
                case (int)ParticleSystemControlMixer.SnapTargetType.posByGame://指定坐标
                    ++EditorGUI.indentLevel;
                    Vector3 pos;
                    Quaternion rot;
                    AddVector3ItemWithGameObjetc(out pos, out rot);
                    if (pos != Vector3.zero)
                        _posByGame.vector3Value = pos;
                    if (rot != Quaternion.identity)
                        _rotationByGame.quaternionValue = rot;
                    _posByGame.vector3Value =  EditorGUILayout.Vector3Field("位置：", _posByGame.vector3Value, new GUILayoutOption[0]);
                    _rotationByGame.quaternionValue = Quaternion.Euler( EditorGUILayout.Vector3Field("旋转：", _rotationByGame.quaternionValue.eulerAngles, new GUILayoutOption[0]));
                    --EditorGUI.indentLevel;
                    break;
                case (int)ParticleSystemControlMixer.SnapTargetType.boss_show://boss登场
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(_snapTarget, new GUIContent("Boss:"), new GUILayoutOption[0]);
                    --EditorGUI.indentLevel;
                    break;
            }
        }

        EditMount();
        EditorGUILayout.PropertyField(independent, txt_independent);
        EditorGUILayout.PropertyField(_randomSeed,new GUIContent("随机种子:"), new GUILayoutOption[0]);



        serializedObject.ApplyModifiedProperties();

    }

    private void EditMount()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("目标挂接点：");
        var selectIdx = Mathf.Clamp(_snapMount.enumValueIndex, 0, _mountTargetList.Count - 1);
        selectIdx = EditorGUILayout.Popup(selectIdx, _mountTargetList.ToArray());
        _snapMount.enumValueIndex = selectIdx;
        (serializedObject.targetObject as ParticleSystemControlTrack).template.snapMount = (ParticleSystemControlMixer.SnapTargetMount)selectIdx;
        EditorGUILayout.EndHorizontal();

        string bundName = "";
        switch (selectIdx)
        {
            case (int)ParticleSystemControlMixer.SnapTargetMount.none:
                bundName = "";
                _mountOtherName.stringValue = bundName;
                break;
            case (int)ParticleSystemControlMixer.SnapTargetMount.leftHand:
                bundName = "LeftHand";
                _mountOtherName.stringValue = bundName;
                break;
            case (int)ParticleSystemControlMixer.SnapTargetMount.rightHand:
                bundName = "RightHand";
                _mountOtherName.stringValue = bundName;
                break;
            case (int)ParticleSystemControlMixer.SnapTargetMount.bothHand:
                bundName = "LeftHand,RightHand";
                _mountOtherName.stringValue = bundName;
                break;
            case (int)ParticleSystemControlMixer.SnapTargetMount.hair:
                bundName  = "Bip001 Head";
                _mountOtherName.stringValue = bundName;
                break;
            case (int)ParticleSystemControlMixer.SnapTargetMount.win:
                bundName = "Bone_wing";
                _mountOtherName.stringValue = bundName;
                break;
            case (int)ParticleSystemControlMixer.SnapTargetMount.sadde:
                bundName = "ride";
                _mountOtherName.stringValue = bundName;
                break;
            case (int)ParticleSystemControlMixer.SnapTargetMount.other:
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(_mountOtherName, new GUIContent("挂接点名称（多实例用,隔开）:"), new GUILayoutOption[0]);
                --EditorGUI.indentLevel;
                break;
        }
        
        (target as ParticleSystemControlTrack).template.mountOtherName = _mountOtherName.stringValue;      

    }

    private static void AddVector3ItemWithGameObjetc(out Vector3 pos, out Quaternion rot)
    {
        pos = Vector3.zero;
        rot = Quaternion.identity;

        GameObject gameObject = null;
        gameObject = EditorGUILayout.ObjectField("定位目标：", gameObject, typeof(GameObject)) as GameObject;
        if (gameObject != null)
        {
            pos = (gameObject.transform.position);
            rot = gameObject.transform.rotation;
        }
       
    }

    private static string  GetGameobjectName()
    {
        GameObject gameObject = null;
        gameObject = EditorGUILayout.ObjectField("定位目标：", gameObject, typeof(GameObject)) as GameObject;
        if (gameObject != null)
        {
            return gameObject.name;
        }
        return "";
    }
}


