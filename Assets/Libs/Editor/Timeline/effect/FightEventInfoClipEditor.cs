using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using static FightEventInfoPlayableData;

[CustomEditor(typeof(FightEventInfoClip)), CanEditMultipleObjects]
public class FightEventInfoClipEditor : Editor
{
    List<string> txtEffectTypeTitle = new List<string>()
    {
        "可打断",
        "震屏一次",
        "持续震屏",
        "FieldView",
        "运动模糊",
        "景深模糊",
        "暂停相机跟随",
        "普攻连击点",
        "攻击点",//攻击 比 受击点 提前 约100毫秒，发战斗包给服务器
        "平移点",
        "宠物切换点",
    };

    List<string> txtTargetTypeTitle = new List<string>()
    {
        "仅攻击者有效",
        "仅受击者有效",
        "都有效"
    };

    SerializedProperty spEffectType;
    GUIContent txtEffectType;

    SerializedProperty spTargetType;
    GUIContent txtTargetType;

    SerializedProperty _targetFieldView;
    GUIContent _targetFieldViewText;

    SerializedProperty _startChangeTime;
    GUIContent _startChangeTimeText;

    SerializedProperty _endChangeTime;
    GUIContent _endChangeTimeText;

    SerializedProperty _motionStrength;
    GUIContent _motionStrengthText;

    SerializedProperty _depthPointOffset;
    SerializedProperty _DepthFNumber;
    SerializedProperty _DepthFLength;
    SerializedProperty _DepthKSize;
    SerializedProperty _JuGuaiOnCollectTargets, _JuGuaiDist, _JuGuaiRadius;

    SerializedProperty _add_buffer, _add_trap;
    SerializedProperty _break_bossai;

    GUIContent _depthPointOffsetText;
    GUIContent _DepthFNumberText;
    GUIContent _DepthFLengthText;
    GUIContent _DepthKSizeText;
    GUIContent _JuGuaiOnCollectTargetsText, _JuGuaiDistText, _JuGuaiRadiusText;
    GUIContent _add_buffer_text, _add_trap_text;
    GUIContent _break_bossai_text;
    void OnEnable()
    {
        spEffectType = serializedObject.FindProperty("templete._effectType");
        txtEffectType = new GUIContent("效果类型：");

        spTargetType = serializedObject.FindProperty("templete._targetType");
        txtTargetType = new GUIContent("作用对象：");

        _targetFieldView = serializedObject.FindProperty("templete._targetFieldView");
        _targetFieldViewText = new GUIContent("主相机FieldView：");

        _startChangeTime = serializedObject.FindProperty("templete._startChangeTime");
        _startChangeTimeText = new GUIContent("开始渐变时间：");

        _endChangeTime = serializedObject.FindProperty("templete._endChangeTime");
        _endChangeTimeText = new GUIContent("结束渐变时间：");

        _motionStrength = serializedObject.FindProperty("templete._motionStrength");
        _motionStrengthText = new GUIContent("运动模糊强度：");

        _depthPointOffset = serializedObject.FindProperty("templete._depthPointOffset");
        _depthPointOffsetText = new GUIContent("景深焦点偏移量：");

        _DepthFNumber = serializedObject.FindProperty("templete._DepthFNumber");
        _DepthFNumberText = new GUIContent("光圈大小：");

        _DepthFLength = serializedObject.FindProperty("templete._DepthFLength");
        _DepthFLengthText = new GUIContent("焦距：");

        _DepthKSize = serializedObject.FindProperty("templete._DepthKSize");
        _DepthKSizeText = new GUIContent("画质：");

        _JuGuaiOnCollectTargets = serializedObject.FindProperty("templete._JuGuaiOnCollectTargets");
        _JuGuaiOnCollectTargetsText = new GUIContent("是否聚怪：");

        _JuGuaiDist = serializedObject.FindProperty("templete._JuGuaiDist");
        _JuGuaiDistText = new GUIContent("前方距离：");

        _JuGuaiRadius = serializedObject.FindProperty("templete._JuGuaiRadius");
        _JuGuaiRadiusText = new GUIContent("聚怪半径：");

        _add_buffer = serializedObject.FindProperty("templete._add_buffer");
        _add_buffer_text = new GUIContent("是否加buff：");

        _add_trap = serializedObject.FindProperty("templete._add_trap");
        _add_trap_text = new GUIContent("是否加陷阱：");

        _break_bossai = serializedObject.FindProperty("templete._break_bossai");
        _break_bossai_text = new GUIContent("是否打断BossAI：");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        #region 作用对象
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(txtTargetType);
        var selectTargetIdx = Mathf.Clamp(spTargetType.enumValueIndex, 0, txtTargetTypeTitle.Count - 1);
        selectTargetIdx = EditorGUILayout.Popup(selectTargetIdx, txtTargetTypeTitle.ToArray());
        spTargetType.enumValueIndex = selectTargetIdx;
        (serializedObject.targetObject as FightEventInfoClip).Templete.targetType = (TargetType)selectTargetIdx;
        EditorGUILayout.EndHorizontal();
        #endregion

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(txtEffectType);
        var selectIdx = Mathf.Clamp(spEffectType.enumValueIndex, 0, txtEffectTypeTitle.Count - 1);
        selectIdx = EditorGUILayout.Popup(selectIdx, txtEffectTypeTitle.ToArray());
        spEffectType.enumValueIndex = selectIdx;
        (serializedObject.targetObject as FightEventInfoClip).Templete.effectType = (EffectType)selectIdx;
        switch((EffectType)selectIdx)
        {
            case EffectType.CanBreak:                
                break;
            case EffectType.ShockCameraLoop:
                var anim1 = Resources.Load<AnimationClip>("anim/camera_shock_loop");
                (target as FightEventInfoClip).OwningClip.duration = (target as FightEventInfoClip).OwningClip.duration > anim1.length ? (target as FightEventInfoClip).OwningClip.duration : anim1.length;
                break;
            case EffectType.ShockCameraOnce:
                var anim2 = Resources.Load<AnimationClip>("anim/camera_shock");
                (target as FightEventInfoClip).OwningClip.duration = anim2.length;
                break;
        }
        EditorGUILayout.EndHorizontal();

        if ((EffectType)selectIdx == EffectType.FieldViewChange)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_targetFieldViewText);
            _targetFieldView.floatValue = EditorGUILayout.Slider(_targetFieldView.floatValue, 10, 80);            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_startChangeTimeText);
            _startChangeTime.floatValue = EditorGUILayout.Slider(_startChangeTime.floatValue, 0, 1);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_endChangeTimeText);
            _endChangeTime.floatValue = EditorGUILayout.Slider(_endChangeTime.floatValue, 0, 1- _startChangeTime.floatValue);
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }
        else if((EffectType)selectIdx == EffectType.MotionBlur)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_motionStrengthText);
            _motionStrength.floatValue = EditorGUILayout.Slider(_motionStrength.floatValue, 0.1f, 1);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }
        else if ((EffectType)selectIdx == EffectType.DepthBlur)
        {
            EditorGUI.indentLevel++;
            _depthPointOffset.vector3Value = EditorGUILayout.Vector3Field(_depthPointOffsetText, _depthPointOffset.vector3Value);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_DepthKSizeText);
            var ksizeIdx = Mathf.Clamp(_DepthKSize.enumValueIndex, 0, MyEffect.BokehEditor._kSizeText.Count - 1);
            ksizeIdx = EditorGUILayout.Popup(ksizeIdx, MyEffect.BokehEditor._kSizeText.ToArray());
            _DepthKSize.enumValueIndex = ksizeIdx;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(_DepthFNumber, _DepthFNumberText);

            EditorGUILayout.BeginHorizontal();
            var f = _DepthFLength.floatValue * 1000;
            f = EditorGUILayout.Slider(_DepthFLengthText, f, 10.0f, 300.0f);
           // if (EditorGUI.EndChangeCheck())
                _DepthFLength.floatValue = f / 1000;

            EditorGUILayout.EndHorizontal();
            // EditorGUILayout.PropertyField(_DepthFLength, _DepthFLengthText);

            EditorGUI.indentLevel--;
        }
        else if((EffectType)selectIdx == EffectType.CollectTargets) 
        {
            EditorGUILayout.PropertyField(_JuGuaiOnCollectTargets, _JuGuaiOnCollectTargetsText);
            if (_JuGuaiOnCollectTargets.boolValue)
            {
                EditorGUILayout.PropertyField(_JuGuaiDist, _JuGuaiDistText);
                EditorGUILayout.PropertyField(_JuGuaiRadius, _JuGuaiRadiusText);
            }
            EditorGUILayout.PropertyField(_add_buffer, _add_buffer_text);
            EditorGUILayout.PropertyField(_add_trap, _add_trap_text);
            EditorGUILayout.PropertyField(_break_bossai, _break_bossai_text);
        }

        (target as FightEventInfoClip).OwningClip.displayName = txtEffectTypeTitle[selectIdx];       

        serializedObject.ApplyModifiedProperties();
    }
}

