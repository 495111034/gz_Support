
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;



[CustomEditor(typeof(ParticleSystemControlClip)), CanEditMultipleObjects]
class ParticleSystemControlClipEditor : Editor
{
    static public List<string> _snapTypeList = new List<string>()
    {
        "初始化位置","跟随","移动向","朝向"
    };

    static public List<string> _snapType2List = new List<string>()
    {
        "初始化位置","跟随"
    };

    static public List<string> _flyTypeList = new List<string>()
    {
        "飞向目标","从目标飞回","无目标向前飞"
    };

    static public List<string> _flySpeedStrList = new List<string>()
    {
        "固定时间,动态计算速度","固定速度,动态计算时间"
    };

    SerializedProperty _rateOverTime;
    SerializedProperty _rateOverDistance;
    SerializedProperty _snapType;
    SerializedProperty _flyType;
    SerializedProperty _upSpeed;
    SerializedProperty _gravity;
    SerializedProperty _offset;
    SerializedProperty _offset_target;
    SerializedProperty _flySpeedType;
    SerializedProperty _flySpeed;
    SerializedProperty spStartimeType;
    SerializedProperty spFlySpeedBefore;
    SerializedProperty spLocalOffsetAngle;
    SerializedProperty spLocalOffsetDistance;
    SerializedProperty spRandAngleAndDistance;
    SerializedProperty spRandOffsetDistanceMax;
    SerializedProperty sprandOffsetDistanceMin;
    SerializedProperty spIgnoreHeightDiff;
    SerializedProperty spRotationRotation;
    SerializedProperty spScaleOffset;
    SerializedProperty spAnimSpeed;
    SerializedProperty spDontFollowRotate;
    SerializedProperty spIsRecordOrigin;
    SerializedProperty spHideParticleNoEnemy;
    SerializedProperty _hudu_offset;
    SerializedProperty spIgnoreTime;
    

    static class Styles
    {
        // public static readonly GUIContent time = new GUIContent("Over Time");
        // public static readonly GUIContent distance = new GUIContent("Over Distance");
        public static readonly GUIContent flySpeedTitle = new GUIContent("飞行速度(m/s)：");
        public static readonly GUIContent flySpeedTitle2 = new GUIContent("前置飞行速度(m/s)：");
        public static readonly GUIContent _hudu_offsetTitle = new GUIContent("圆弧偏移宽度");
    }

    void OnEnable()
    {
        // _rateOverTime = serializedObject.FindProperty("template.rateOverTime");
        //_rateOverDistance = serializedObject.FindProperty("template.rateOverDistance");
        _snapType = serializedObject.FindProperty("template._snapType");
        _flyType = serializedObject.FindProperty("template._flyType");
        //_upSpeed = serializedObject.FindProperty("template._upSpeed");
        _gravity = serializedObject.FindProperty("template._gravity");
        _offset = serializedObject.FindProperty("template._offset");
        _offset_target = serializedObject.FindProperty("template._offset_target");
        _flySpeedType = serializedObject.FindProperty("template._flySpeedType");
        _flySpeed = serializedObject.FindProperty("template._flySpeed");
        spStartimeType = serializedObject.FindProperty("template._startimeType");
        spFlySpeedBefore = serializedObject.FindProperty("template._flySpeed_before");
        spLocalOffsetAngle = serializedObject.FindProperty("template._localAngle");
        spLocalOffsetDistance = serializedObject.FindProperty("template._localoffsetDistance");
        spRandAngleAndDistance = serializedObject.FindProperty("template._randAngleAndDistance");
        spRandOffsetDistanceMax = serializedObject.FindProperty("template._randOffsetDistanceMax");
        sprandOffsetDistanceMin = serializedObject.FindProperty("template._randOffsetDistanceMin");
        spIgnoreHeightDiff = serializedObject.FindProperty("template.ignoreHeightDiff");
        spRotationRotation = serializedObject.FindProperty("template._rotationOffset");
        spScaleOffset = serializedObject.FindProperty("template._scaleOffset");
        spAnimSpeed = serializedObject.FindProperty("template._anim_speed");
        spDontFollowRotate = serializedObject.FindProperty("template._dontFollowRotate");
        spIsRecordOrigin = serializedObject.FindProperty("template._is_record_origin");
        spHideParticleNoEnemy = serializedObject.FindProperty("template._hide_particle_no_enemy");
        _hudu_offset = serializedObject.FindProperty("template._hudu_offset");
        spIgnoreTime = serializedObject.FindProperty("template._ignoretime");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // EditorGUILayout.LabelField("Particle Emission Rates");
        // EditorGUI.indentLevel++;
        // EditorGUILayout.PropertyField(_rateOverTime, Styles.time);
        //EditorGUILayout.PropertyField(_rateOverDistance, Styles.distance);
        // EditorGUI.indentLevel--;

        if ((target as ParticleSystemControlClip).parentTrack.m_TimelineType == TimelineType.FightSkill)
        {

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("跟随方式：");
            var selectIdx = Mathf.Clamp(_snapType.enumValueIndex, 0, _snapTypeList.Count - 1);
            selectIdx = EditorGUILayout.Popup(selectIdx, _snapTypeList.ToArray());
            _snapType.enumValueIndex = selectIdx;
            (target as ParticleSystemControlClip).template.snapType = (ParticleSystemControlMixer.SnapType)selectIdx;
            EditorGUILayout.EndHorizontal();
            
            //if (selectIdx == 0)
            EditorGUILayout.PropertyField(spIsRecordOrigin, new GUIContent("记录初始目标位置"));

            if (selectIdx == 1)
            {
                EditorGUILayout.PropertyField(spDontFollowRotate, new GUIContent("不跟随朝向"));
                EditorGUILayout.PropertyField(spHideParticleNoEnemy, new GUIContent("无目标时隐藏该特效"));
            }

            if (selectIdx == (int)ParticleSystemControlMixer.SnapType.FlyTarget || selectIdx == (int)ParticleSystemControlMixer.SnapType.LookatTarget)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("忽略高度差：");
                EditorGUILayout.PropertyField(spIgnoreHeightDiff, new GUIContent(), new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();
            }

            bool isForward = false;
            if (selectIdx == (int)ParticleSystemControlMixer.SnapType.FlyTarget)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("飞行方式：");
                selectIdx = Mathf.Clamp(_flyType.enumValueIndex, 0, _flyTypeList.Count - 1);
                selectIdx = EditorGUILayout.Popup(selectIdx, _flyTypeList.ToArray());
                _flyType.enumValueIndex = selectIdx;
                (serializedObject.targetObject as ParticleSystemControlClip).template.flyType = (ParticleSystemControlMixer.FlyType)selectIdx;
                EditorGUILayout.EndHorizontal();

                isForward = (ParticleSystemControlMixer.FlyType)selectIdx == ParticleSystemControlMixer.FlyType.Forward;

                if (!isForward)
                    EditorGUILayout.PropertyField(_hudu_offset, Styles._hudu_offsetTitle, new GUILayoutOption[0]);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("飞行速度计算方式：");
                var _speedTpIdx = Mathf.Clamp(_flySpeedType.enumValueIndex, 0, _flySpeedStrList.Count - 1);
                _speedTpIdx = EditorGUILayout.Popup(_speedTpIdx, _flySpeedStrList.ToArray());
                if (isForward) _speedTpIdx = 1;
                _flySpeedType.enumValueIndex = _speedTpIdx;
                EditorGUILayout.EndHorizontal();

                if (_speedTpIdx == (int)ParticleSystemControlMixer.FlySpeedType.ByDistance)
                {
                    EditorGUILayout.HelpBox(isForward ? "输入向前飞行速度" : "实际Clip长度与设计图形可能不一至", MessageType.Warning);

                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(_flySpeed, Styles.flySpeedTitle, new GUILayoutOption[0]);
                    if (_flySpeed.floatValue <= 0)
                    {
                        EditorGUILayout.HelpBox("请输入正确的速度", MessageType.Error);
                    }
                    --EditorGUI.indentLevel;
                }
                else
                {

                }

                //EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.LabelField("初始上抛速度：");
                //_upSpeed.floatValue =  EditorGUILayout.Slider(_upSpeed.floatValue, 0, 50, new GUILayoutOption[0]);
                //EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("重力加速度：");
                _gravity.floatValue = EditorGUILayout.Slider(_gravity.floatValue, 0, 100, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();

                --EditorGUI.indentLevel;
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("开始时间计算方式：");
                var _speedTpIdx = Mathf.Clamp(spStartimeType.enumValueIndex, 0, SkillHitClipEditor._startTimeTypeStrList.Count - 1);
                _speedTpIdx = EditorGUILayout.Popup(_speedTpIdx, SkillHitClipEditor._startTimeTypeStrList.ToArray());
                spStartimeType.enumValueIndex = _speedTpIdx;
                EditorGUILayout.EndHorizontal();
                if (_speedTpIdx == (int)ParticleSystemControlMixer.FlySpeedType.ByDistance)
                {
                    EditorGUILayout.HelpBox("实际开始时间与设计图形可能不一至", MessageType.Warning);

                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(spFlySpeedBefore, Styles.flySpeedTitle2, new GUILayoutOption[0]);
                    if (spFlySpeedBefore.floatValue <= 0)
                    {
                        EditorGUILayout.HelpBox("请输入正确的速度", MessageType.Error);
                    }
                    --EditorGUI.indentLevel;
                }
            }

            if (selectIdx == (int)ParticleSystemControlMixer.SnapType.InitPos || selectIdx == (int)ParticleSystemControlMixer.SnapType.FlyTarget || selectIdx == (int)ParticleSystemControlMixer.SnapType.FollowPos)
            {
                //旋转偏移值
                EditorGUILayout.BeginHorizontal();
                spRotationRotation.vector3Value = EditorGUILayout.Vector3Field(new GUIContent("旋转偏移量:"), spRotationRotation.vector3Value, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            spScaleOffset.vector3Value = EditorGUILayout.Vector3Field(new GUIContent("缩放:"), spScaleOffset.vector3Value, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();
            if ((target as ParticleSystemControlClip).parentTrack.template.snapTartetType == ParticleSystemControlMixer.SnapTargetType.posByGame)
            {
                EditorGUILayout.LabelField("正在使用游戏中传入的位置");
            }
            else
            {

                if (!isForward) EditorGUILayout.PropertyField(spRandAngleAndDistance, new GUIContent("位置随机偏移："), new GUILayoutOption[0]);
                else
                {
                    if (spRandAngleAndDistance.boolValue) spRandAngleAndDistance.boolValue = false;
                }

                if (!spRandAngleAndDistance.boolValue)
                {
                    if ((target as ParticleSystemControlClip).template.snapType == ParticleSystemControlMixer.SnapType.FlyTarget)
                    {
                        _offset.vector3Value = EditorGUILayout.Vector3Field(new GUIContent("开始位置偏移量:"), _offset.vector3Value, new GUILayoutOption[0]);
                        if (!isForward) _offset_target.vector3Value = EditorGUILayout.Vector3Field(new GUIContent("目标位置偏移量:"), _offset_target.vector3Value, new GUILayoutOption[0]);
                    }
                    else
                    {
                        _offset.vector3Value = EditorGUILayout.Vector3Field(new GUIContent("位置偏移量:"), _offset.vector3Value, new GUILayoutOption[0]);
                    }
                   

                    //EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("相对偏移：");
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("角度：");
                    spLocalOffsetAngle.floatValue = EditorGUILayout.Slider(spLocalOffsetAngle.floatValue, 0, 360, new GUILayoutOption[0]);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("距离：");
                    spLocalOffsetDistance.floatValue = EditorGUILayout.Slider(spLocalOffsetDistance.floatValue, -20, 20, new GUILayoutOption[0]);
                    EditorGUILayout.EndHorizontal();
                    --EditorGUI.indentLevel;

                    //EditorGUILayout.EndHorizontal();


                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("随机最小值：");
                    sprandOffsetDistanceMin.floatValue = EditorGUILayout.Slider(sprandOffsetDistanceMin.floatValue, 0, 10, new GUILayoutOption[0]);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("随机最大值：");
                    spRandOffsetDistanceMax.floatValue = EditorGUILayout.Slider(spRandOffsetDistanceMax.floatValue, sprandOffsetDistanceMin.floatValue, sprandOffsetDistanceMin.floatValue + 10, new GUILayoutOption[0]);
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.BeginHorizontal();
            spAnimSpeed.floatValue = EditorGUILayout.FloatField(new GUIContent("Animation特效速度系数:"), spAnimSpeed.floatValue, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();

            (target as ParticleSystemControlClip).OwningClip.displayName = _snapTypeList[(int)((target as ParticleSystemControlClip).template.snapType)];
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("跟随方式：");
            var selectIdx = Mathf.Clamp(_snapType.enumValueIndex, 0, _snapType2List.Count - 1);
            selectIdx = EditorGUILayout.Popup(selectIdx, _snapType2List.ToArray());
            _snapType.enumValueIndex = selectIdx;
            (target as ParticleSystemControlClip).template.snapType = (ParticleSystemControlMixer.SnapType)selectIdx;
            EditorGUILayout.EndHorizontal();

            if (selectIdx == (int)ParticleSystemControlMixer.SnapType.InitPos ||selectIdx == (int)ParticleSystemControlMixer.SnapType.FollowPos)
            {
                //旋转偏移值
                EditorGUILayout.BeginHorizontal();
                spRotationRotation.vector3Value = EditorGUILayout.Vector3Field(new GUIContent("旋转偏移量:"), spRotationRotation.vector3Value, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            spScaleOffset.vector3Value = EditorGUILayout.Vector3Field(new GUIContent("缩放:"), spScaleOffset.vector3Value, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();
            if ((target as ParticleSystemControlClip).parentTrack.template.snapTartetType == ParticleSystemControlMixer.SnapTargetType.posByGame)
            {
                EditorGUILayout.LabelField("正在使用游戏中传入的位置");
            }
            else
            {

                EditorGUILayout.PropertyField(spRandAngleAndDistance, new GUIContent("位置随机偏移："), new GUILayoutOption[0]);

                if (!spRandAngleAndDistance.boolValue)
                {

                    _offset.vector3Value = EditorGUILayout.Vector3Field(new GUIContent("位置偏移量:"), _offset.vector3Value, new GUILayoutOption[0]);

                    //EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("相对偏移：");
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("角度：");
                    spLocalOffsetAngle.floatValue = EditorGUILayout.Slider(spLocalOffsetAngle.floatValue, 0, 360, new GUILayoutOption[0]);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("距离：");
                    spLocalOffsetDistance.floatValue = EditorGUILayout.Slider(spLocalOffsetDistance.floatValue, -20, 20, new GUILayoutOption[0]);
                    EditorGUILayout.EndHorizontal();
                    --EditorGUI.indentLevel;
                    //EditorGUILayout.EndHorizontal();


                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("随机最小值：");
                    sprandOffsetDistanceMin.floatValue = EditorGUILayout.Slider(sprandOffsetDistanceMin.floatValue, 0, 10, new GUILayoutOption[0]);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("随机最大值：");
                    spRandOffsetDistanceMax.floatValue = EditorGUILayout.Slider(spRandOffsetDistanceMax.floatValue, sprandOffsetDistanceMin.floatValue, sprandOffsetDistanceMin.floatValue + 10, new GUILayoutOption[0]);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.BeginHorizontal();
            spAnimSpeed.floatValue = EditorGUILayout.FloatField(new GUIContent("Animation特效速度系数:"), spAnimSpeed.floatValue, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();
            (target as ParticleSystemControlClip).OwningClip.displayName = _snapType2List[(int)((target as ParticleSystemControlClip).template.snapType)];
        }
        EditorGUILayout.PropertyField(spIgnoreTime, new GUIContent("显隐不受时间轴控制"));

        serializedObject.ApplyModifiedProperties();
    }
}


