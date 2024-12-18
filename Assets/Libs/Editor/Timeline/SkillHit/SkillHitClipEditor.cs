using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillHitTimelineClip)), CanEditMultipleObjects]
class SkillHitClipEditor: Editor
{
    SerializedProperty spEventType;
    GUIContent txtEventType;

    SerializedProperty spWeights;
    GUIContent txtWgights;

    SerializedProperty spEventContent;
    GUIContent txtEventContent;

    SerializedProperty spStartimeType;
    GUIContent txtStarttimeType;

    SerializedProperty spFlySpeed;
    GUIContent txtFlySpeed;

    SerializedProperty spAddBuffer;
    GUIContent txtAddBuffer;

    //SerializedProperty spConjure;
    //GUIContent txtConjure;

    SerializedProperty spSockCamera;
    GUIContent  txtSockCamera;

    SerializedProperty spShockCameraType;
    GUIContent txtShockCameraType;

    SerializedProperty spRepealDistance;
    GUIContent txtRepealDistance;

    SerializedProperty flyType;
    GUIContent txtFlyType;

    SerializedProperty flyFrames;
    GUIContent txtFlyFrames;
    GUIContent txtFlyFrameNumber;
    GUIContent txtFlyFrameTime;
    GUIContent txtFlyFrameHeight;

    SerializedProperty spUpV0;
    GUIContent txtUpV0;

    SerializedProperty spGa;
    GUIContent txtGa;

    SerializedProperty spByHitAnimType;
    GUIContent txtByHitAnimType;

    SerializedProperty spHitColor;
    GUIContent txtHitColor;

    SerializedProperty spHitColorScale;
    GUIContent txtHitColorScale;

    SerializedProperty spHitColorBias;
    GUIContent txtHitColorBias;

    SerializedProperty _byHitSound;
    GUIContent _txtByHitSound;

    SerializedProperty _playSoundType, _playSoundName;
    GUIContent _txtPlaySoundType, _txtPlaySoundName;

    SerializedProperty spDistJuGuai;
    GUIContent txtDistJuGuai;

    SerializedProperty spRadiusJuGuai;
    GUIContent txtRadiusJuGuai;

    SerializedProperty _attack_before;
    GUIContent _attack_beforeGC;

    static List<string> txtEventTypeList = new List<string>()
    {
        "受击","击退","击倒","击飞","招唤","锁定","挣脱","聚怪"
    };

    static public List<string> _startTimeTypeStrList = new List<string>()
    {
        "根据时间轴","根据距离和速度计算"
    };

    static List<string> txtShockCameraTypeTitleList = new List<string>()
    {
        "必然",
        "有伤害时",
        "暴击时"
    };

    static List<string> txtHitAnimType = new List<string>()
    {
        "受击",
        "击倒",
        "击飞",
        "无动作",
    };

    static List<string> txtFlyTypeList = new List<string>()
    {
        "物理",
        "关键帧曲线",
    };

    void OnEnable()
    {
        spEventType = serializedObject.FindProperty("templete.hitType");
        spEventContent = serializedObject.FindProperty("templete.eventContent");
        spStartimeType = serializedObject.FindProperty("templete._startimeType");
        spAddBuffer = serializedObject.FindProperty("templete._addBuffer");
        spSockCamera = serializedObject.FindProperty("templete._shockCamera");
        spShockCameraType = serializedObject.FindProperty("templete._shockCameraType");
        // spConjure = serializedObject.FindProperty("_conjure");
        spFlySpeed = serializedObject.FindProperty("templete._flySpeed");
        spRepealDistance = serializedObject.FindProperty("templete._repealDistance");
        spUpV0 = serializedObject.FindProperty("templete._upV0");
        spGa = serializedObject.FindProperty("templete._g");
        spByHitAnimType = serializedObject.FindProperty("templete._byHitAnimType");
        spHitColor = serializedObject.FindProperty("templete._hitColor");
        spHitColorScale = serializedObject.FindProperty("templete._hitColorScale");
        spHitColorBias = serializedObject.FindProperty("templete._hitColorBias");
        flyType = serializedObject.FindProperty("templete.flyType");
        flyFrames = serializedObject.FindProperty("templete.flyFrames");
        _byHitSound = serializedObject.FindProperty("templete._byHitSound");
        _playSoundType = serializedObject.FindProperty("templete._playSoundType");
        _playSoundName = serializedObject.FindProperty("templete._playSoundName");
        spWeights = serializedObject.FindProperty("templete._weights");
        spDistJuGuai = serializedObject.FindProperty("templete._distJuGuai");
        spRadiusJuGuai = serializedObject.FindProperty("templete._radiusJuGuai");
        _attack_before = serializedObject.FindProperty("templete._attack_before");

        txtEventType = new GUIContent("受击类型：");
        txtEventContent = new GUIContent("附加参数：");
        txtStarttimeType = new GUIContent("开始时间计算方式：");
        txtFlySpeed = new GUIContent("速度(m/s)：");
        txtAddBuffer = new GUIContent("增加buffer(取决于技能)：");
        txtSockCamera = new GUIContent("受击震屏：");
        txtShockCameraType = new GUIContent("震屏条件：");
        // txtConjure = new GUIContent("招唤(取决于技能)：");
        txtRepealDistance = new GUIContent("击退距离：");
        txtUpV0 = new GUIContent("初始上抛速度：");
        txtGa = new GUIContent("重力加速度：");
        txtByHitAnimType = new GUIContent("受击动作类型：");
        txtHitColor = new GUIContent("受击闪光颜色：");
        txtHitColorScale = new GUIContent("受击闪光倍数：");
        txtHitColorBias = new GUIContent("受击闪光范围：");
        txtFlyType = new GUIContent("击飞轨迹计算方式：");
        txtFlyFrames = new GUIContent("轨迹关键帧：");
        txtFlyFrameNumber = new GUIContent("关键帧数量：");
        txtFlyFrameTime = new GUIContent("时间：");
        txtFlyFrameHeight = new GUIContent("高度：");
        _txtByHitSound = new GUIContent("受击声音：");
        _txtPlaySoundType = new GUIContent("声音条件：");
        _txtPlaySoundName = new GUIContent("声音资源：");
        txtWgights = new GUIContent("扣血权重（所有受击之和必须为1）：");

        txtDistJuGuai = new GUIContent("聚怪圆心在前方距离：");
        txtRadiusJuGuai = new GUIContent("聚怪半径：");
        _attack_beforeGC = new GUIContent("受击前发动攻击:");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("受击方式：");
        var selectIdx = Mathf.Clamp(spEventType.enumValueIndex, 0, txtEventTypeList.Count - 1);
        selectIdx = EditorGUILayout.Popup(selectIdx, txtEventTypeList.ToArray());
        spEventType.enumValueIndex = selectIdx;
        (serializedObject.targetObject as SkillHitTimelineClip).Templete.HitType = (SkillHitTimelineClip.ByHitType)selectIdx;
        EditorGUILayout.EndHorizontal();

        if(selectIdx == (int)SkillHitTimelineClip.ByHitType.Repel)
        {
            ++EditorGUI.indentLevel;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(txtRepealDistance);
            spRepealDistance.floatValue = EditorGUILayout.Slider(spRepealDistance.floatValue, -10, 10, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();
            --EditorGUI.indentLevel;
        }
        else if(selectIdx == (int)SkillHitTimelineClip.ByHitType.Normal)
        {
            ++EditorGUI.indentLevel;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(txtWgights);
            spWeights.floatValue = EditorGUILayout.Slider(spWeights.floatValue, 0, 1, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();
            --EditorGUI.indentLevel;
        }
        else if(selectIdx == (int)SkillHitTimelineClip.ByHitType.KnockFly)
        {
            ++EditorGUI.indentLevel;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(txtFlyType);
            var flyTypeIdx = Mathf.Clamp(flyType.enumValueIndex, 0, txtFlyTypeList.Count - 1);
            flyTypeIdx = EditorGUILayout.Popup(flyTypeIdx, txtFlyTypeList.ToArray(), new GUILayoutOption[0]);
            flyType.enumValueIndex = flyTypeIdx;
            EditorGUILayout.EndHorizontal();

            ++EditorGUI.indentLevel;
            if (flyTypeIdx == (int)SkillHitTimelineClip.KnockFlyType.ByPhysical)
            {                

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(txtUpV0);
                spUpV0.floatValue = EditorGUILayout.Slider(spUpV0.floatValue, 0, 100, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(txtGa);
                spGa.floatValue = EditorGUILayout.Slider(spGa.floatValue, 0, 100, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();

               
            }
            else if (flyTypeIdx == (int)SkillHitTimelineClip.KnockFlyType.ByKeyFrame)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(txtFlyFrameNumber);
                flyFrames.arraySize = EditorGUILayout.IntField(flyFrames.arraySize, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();

                float last_time = 0;
                for(int i = 0; i < flyFrames.arraySize; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField((i +1).ToString());
                    EditorGUILayout.EndHorizontal();
                    
                    ++EditorGUI.indentLevel;
                    {
                        var item = flyFrames.GetArrayElementAtIndex(i).vector2Value;                       
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(txtFlyFrameTime);                        
                        item.x = EditorGUILayout.Slider(item.x, 0, 1, new GUILayoutOption[0]);
                        if (item.x < last_time) item.x = last_time;
                        last_time = item.x;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(txtFlyFrameHeight);
                        item.y = EditorGUILayout.Slider(item.y, 0, 10, new GUILayoutOption[0]);
                        EditorGUILayout.EndHorizontal();

                        flyFrames.GetArrayElementAtIndex(i).vector2Value = item;


                    }
                    --EditorGUI.indentLevel;                    
                }

            }

            --EditorGUI.indentLevel;

            --EditorGUI.indentLevel;
        }
        (target as SkillHitTimelineClip).OwningClip.displayName = txtEventTypeList[selectIdx];


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(txtByHitAnimType);
        selectIdx = Mathf.Clamp(spByHitAnimType .enumValueIndex, 0, txtHitAnimType.Count - 1);
        selectIdx = EditorGUILayout.Popup(selectIdx, txtHitAnimType.ToArray());
        spByHitAnimType.enumValueIndex = selectIdx;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(txtStarttimeType);
        var _speedTpIdx = Mathf.Clamp(spStartimeType.enumValueIndex, 0, _startTimeTypeStrList.Count - 1);
        _speedTpIdx = EditorGUILayout.Popup(_speedTpIdx, _startTimeTypeStrList.ToArray());
        spStartimeType.enumValueIndex = _speedTpIdx;
        EditorGUILayout.EndHorizontal();

        if (_speedTpIdx == (int)SkillHitTimelineClip.StartTimeType.ByDistance)
        {
            EditorGUILayout.HelpBox("实际开始时间可能与时间轴不一至", MessageType.Warning);

            ++EditorGUI.indentLevel;
            EditorGUILayout.PropertyField(spFlySpeed, txtFlySpeed, new GUILayoutOption[0]);
            if (spFlySpeed.floatValue <= 0)
            {
                EditorGUILayout.HelpBox("请输入正确的速度", MessageType.Error);
            }
            --EditorGUI.indentLevel;
        }
        else
        {
            
        }

        EditorGUILayout.PropertyField(spAddBuffer, txtAddBuffer, new GUILayoutOption[0]);
        //EditorGUILayout.PropertyField(spConjure, txtConjure, new GUILayoutOption[0]);

        EditorGUILayout.PropertyField(spEventContent, txtEventContent, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(spSockCamera, txtSockCamera, new GUILayoutOption[0]);
        if(spSockCamera.boolValue)
        {
            ++EditorGUI.indentLevel;
            EditorGUILayout.BeginHorizontal();

            var _shockTypeIdx = Mathf.Clamp(spShockCameraType.enumValueIndex, 0, txtShockCameraTypeTitleList.Count - 1);
            EditorGUILayout.LabelField(txtShockCameraType);
            _shockTypeIdx = EditorGUILayout.Popup(_shockTypeIdx, txtShockCameraTypeTitleList.ToArray());
            spShockCameraType.enumValueIndex = _shockTypeIdx;
            EditorGUILayout.EndHorizontal();
            --EditorGUI.indentLevel;
        }


        EditorGUILayout.PropertyField(_byHitSound, _txtByHitSound, new GUILayoutOption[0]);
        if (_byHitSound.boolValue)
        {
            ++EditorGUI.indentLevel;
            EditorGUILayout.BeginHorizontal();

            var idx = Mathf.Clamp(_playSoundType.enumValueIndex, 0, txtShockCameraTypeTitleList.Count - 1);
            EditorGUILayout.LabelField(_txtPlaySoundType);
            idx = EditorGUILayout.Popup(idx, txtShockCameraTypeTitleList.ToArray());
            _playSoundType.enumValueIndex = idx;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(_playSoundName, _txtPlaySoundName, new GUILayoutOption[0]);
            --EditorGUI.indentLevel;
        }

        EditorGUILayout.PropertyField(spHitColor, txtHitColor, new GUILayoutOption[0]);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(txtHitColorBias);
        spHitColorBias.floatValue = EditorGUILayout.Slider(spHitColorBias.floatValue, 0, 2, new GUILayoutOption[0]);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(txtHitColorScale);
        spHitColorScale.floatValue = EditorGUILayout.Slider(spHitColorScale.floatValue, 0, 2, new GUILayoutOption[0]);
        EditorGUILayout.EndHorizontal();

        //if (false)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_attack_beforeGC);
            _attack_before.floatValue = EditorGUILayout.Slider(_attack_before.floatValue, 0, 0.3f, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();

            if (_attack_before.floatValue > 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(txtDistJuGuai);
                spDistJuGuai.floatValue = EditorGUILayout.Slider(spDistJuGuai.floatValue, 0, 5, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(txtRadiusJuGuai);
                spRadiusJuGuai.floatValue = EditorGUILayout.Slider(spRadiusJuGuai.floatValue, 0, 5, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();
            }
        }
        //(target as GPUAnimTimelineClip).name = txtEventTypeList[selectIdx];


        serializedObject.ApplyModifiedProperties();
    }
}

