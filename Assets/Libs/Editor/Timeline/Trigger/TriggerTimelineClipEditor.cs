using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TriggerTimelineClip), true)]
[CanEditMultipleObjects]
class TriggerTimelineClipEditor : Editor
{
    static List<string> triggerTypeTextList = new List<string>()
    {
        "剧情UI",
        "震屏",
        "黑屏",
        "屏蔽",
        "跳过(弃用,合屏蔽里了)",
        "天空盒变色",
        "消融",
        "剧情结束后设置lockView",
        "摄像机归位",
        "骑乘",
        "Boss登场界面",
        "播放场景物件的动画",
        "跳转场景",
        "任务对话",
        "反消融",
        "逐字句显示文本",
        "暂停-调整timeScale为0.1"
    };
    static List<string> juQingUIIDTextList = new List<string>()
    {
        "剧情","冒泡","界面","界面调用"
    };
    static List<string> speakerTypeTextList = new List<string>()
    {
        "主角","NPC","怪物"
    };

    private SerializedProperty triggerType;
    private SerializedProperty uIData;
    private SerializedProperty juQingUIID;
    private SerializedProperty aoi_id;
    private SerializedProperty speakerType;
    private SerializedProperty NPCorMonsterID;
    private SerializedProperty dialogue;
    private SerializedProperty isRoleSpeak;

    //private SerializedProperty creatByAoi;

    private SerializedProperty startAlpha;
    private SerializedProperty endAlpha;
    private SerializedProperty isCloseMainUI;
    private SerializedProperty actorHide;
    private SerializedProperty mainRolehide;
    private SerializedProperty effectHide;
    private SerializedProperty isShowSkipBtn;
    private SerializedProperty fromColor;
    private SerializedProperty toColor;
    private SerializedProperty dlFromColor;
    private SerializedProperty dlToColor;
    private SerializedProperty fogFromColor;
    private SerializedProperty fogToColor;
    private SerializedProperty DLFromColor;
    private SerializedProperty DLToColor;
    private SerializedProperty isSetPos;
    private SerializedProperty roleEndPos;
    private SerializedProperty burnID, burnParts;
    private SerializedProperty lockViewSet;
    private SerializedProperty angle_y;
    private SerializedProperty angle_xz;
    private SerializedProperty distance;
    private SerializedProperty horseID;
    private SerializedProperty sceneObjectName;
    private SerializedProperty sceneObjectAnim;
    private SerializedProperty sceneID;
    private SerializedProperty isSetDefaultPos;
    private SerializedProperty toPos;
    private SerializedProperty showSkin;
    private SerializedProperty dios;
    private SerializedProperty btns;
    private SerializedProperty oneByOne;
    private SerializedProperty pauseBtnPos;
    private SerializedProperty set_slience;
    private SerializedProperty pauseEffect;
    private SerializedProperty pauseEndEffect;
    private SerializedProperty pauseBtnText;
    private SerializedProperty oneByOneJianGe;
    private SerializedProperty bossName;
    private SerializedProperty checkSceneAllDone;

    private void OnEnable()
    {
        triggerType = serializedObject.FindProperty("triggerType");
        uIData = serializedObject.FindProperty("uIData");
        juQingUIID = serializedObject.FindProperty("uIData.juQingUIID");
        aoi_id = serializedObject.FindProperty("uIData.aoi_id");
        speakerType = serializedObject.FindProperty("uIData.speakerType");
        NPCorMonsterID = serializedObject.FindProperty("uIData.NPCorMonsterID");
        dialogue = serializedObject.FindProperty("uIData.dialogue");
        isRoleSpeak = serializedObject.FindProperty("uIData.isRoleSpeak");

        startAlpha = serializedObject.FindProperty("startAlpha");
        endAlpha = serializedObject.FindProperty("endAlpha");
        isCloseMainUI = serializedObject.FindProperty("isCloseMainUI");
        actorHide = serializedObject.FindProperty("actorHide");
        mainRolehide = serializedObject.FindProperty("mainRolehide");
        effectHide = serializedObject.FindProperty("effectHide");
        isShowSkipBtn = serializedObject.FindProperty("isShowSkipBtn");
        fromColor = serializedObject.FindProperty("fromColor");
        toColor = serializedObject.FindProperty("toColor");

        dlFromColor = serializedObject.FindProperty("dlFromColor");
        dlToColor = serializedObject.FindProperty("dlToColor");

        fogFromColor = serializedObject.FindProperty("fogFromColor");
        fogToColor = serializedObject.FindProperty("fogToColor");

        DLFromColor = serializedObject.FindProperty("DLFromColor");
        DLToColor = serializedObject.FindProperty("DLToColor");

        isSetPos = serializedObject.FindProperty("isSetPos");
        roleEndPos = serializedObject.FindProperty("roleEndPos");
        burnID = serializedObject.FindProperty("burnID");
        burnParts = serializedObject.FindProperty("burnParts");
        lockViewSet = serializedObject.FindProperty("lockViewSet");
        angle_y = serializedObject.FindProperty("angle_y");
        angle_xz = serializedObject.FindProperty("angle_xz");
        distance = serializedObject.FindProperty("distance");
        horseID = serializedObject.FindProperty("horseID");
        sceneObjectName = serializedObject.FindProperty("sceneObjectName");
        sceneObjectAnim = serializedObject.FindProperty("sceneObjectAnim");
        sceneID = serializedObject.FindProperty("sceneID");
        isSetDefaultPos = serializedObject.FindProperty("isSetDefaultPos");
        toPos = serializedObject.FindProperty("toPos");
        showSkin = serializedObject.FindProperty("showSkin");
        dios = serializedObject.FindProperty("dios");
        btns = serializedObject.FindProperty("btns");
        oneByOne = serializedObject.FindProperty("oneByOne");
        pauseBtnPos = serializedObject.FindProperty("pauseBtnPos");
        set_slience = serializedObject.FindProperty("set_slience");
        pauseEffect = serializedObject.FindProperty("pauseEffect");
        pauseEndEffect = serializedObject.FindProperty("pauseEndEffect");
        pauseBtnText = serializedObject.FindProperty("pauseBtnText");
        oneByOneJianGe = serializedObject.FindProperty("oneByOneJianGe");
        bossName = serializedObject.FindProperty("bossName");
        checkSceneAllDone = serializedObject.FindProperty("checkSceneAllDone");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        TriggerTimelineClip clip = target as TriggerTimelineClip;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("类型");
        var typeIdx = Mathf.Clamp(triggerType.enumValueIndex, 0, triggerTypeTextList.Count - 1);
        typeIdx = EditorGUILayout.Popup(typeIdx, triggerTypeTextList.ToArray(), new GUILayoutOption[0]);
        triggerType.enumValueIndex = typeIdx;
        EditorGUILayout.EndHorizontal();
        if (triggerType.enumValueIndex == 0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("设置要显示的剧情UI");
            var juQingIdx = Mathf.Clamp(juQingUIID.enumValueIndex, 0, juQingUIIDTextList.Count - 1);
            juQingIdx = EditorGUILayout.Popup(juQingIdx, juQingUIIDTextList.ToArray(), new GUILayoutOption[0]);
            juQingUIID.enumValueIndex = juQingIdx;
            EditorGUILayout.EndHorizontal();

            if (juQingUIID.enumValueIndex == 0)
            {
                EditorGUILayout.BeginHorizontal();
                isRoleSpeak.boolValue = EditorGUILayout.Toggle("是主角说话", isRoleSpeak.boolValue);
                EditorGUILayout.EndHorizontal();
            }
            else if (juQingUIID.enumValueIndex == 1)
            {
                //EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.LabelField("要冒泡的演员的aoi_id(0表示主角)");
                //aoi_id.intValue = int.Parse(EditorGUILayout.TextField(aoi_id.intValue.ToString(), new GUILayoutOption[0]));
                //EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("标明讲话人的类型(以便读表)");
                var speakerIdx = Mathf.Clamp(speakerType.enumValueIndex, 0, speakerTypeTextList.Count - 1);
                speakerIdx = EditorGUILayout.Popup(speakerIdx, speakerTypeTextList.ToArray(), new GUILayoutOption[0]);
                speakerType.enumValueIndex = speakerIdx;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("讲话人ID：");
                NPCorMonsterID.intValue = int.Parse(EditorGUILayout.TextField(NPCorMonsterID.intValue.ToString(), new GUILayoutOption[0]));
                EditorGUILayout.EndHorizontal();
            }
            else if (juQingUIID.enumValueIndex == 2)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("标明讲话人的类型(以便读表)");
                var speakerIdx = Mathf.Clamp(speakerType.enumValueIndex, 0, speakerTypeTextList.Count - 1);
                speakerIdx = EditorGUILayout.Popup(speakerIdx, speakerTypeTextList.ToArray(), new GUILayoutOption[0]);
                speakerType.enumValueIndex = speakerIdx;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("讲话人ID：");
                NPCorMonsterID.intValue = int.Parse(EditorGUILayout.TextField(NPCorMonsterID.intValue.ToString(), new GUILayoutOption[0]));
                EditorGUILayout.EndHorizontal();
            }
            else if (juQingUIID.enumValueIndex == 3)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("界面ID：");
                aoi_id.intValue = int.Parse(EditorGUILayout.TextField(aoi_id.intValue.ToString(), new GUILayoutOption[0]));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("CallID：");
                NPCorMonsterID.intValue = int.Parse(EditorGUILayout.TextField(NPCorMonsterID.intValue.ToString(), new GUILayoutOption[0]));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("要显示的话(填langID)");
            dialogue.stringValue = EditorGUILayout.TextField(dialogue.stringValue, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();

            clip.TriggerContent = "UI" + "{\"juQingUIID\":" + juQingIdx + ","
                + "\"aoi_id\":" + aoi_id.intValue + ","
                + "\"speakerType\":" + speakerType.intValue + ","
                + "\"NPCorMonsterID\":" + NPCorMonsterID.intValue + ","
                //+ "\"speak_name\":"+"\"" + speak_name.stringValue+"\","
                //+ "\"icon\":"+"\"" + icon.stringValue+"\","
                + "\"isRoleSpeak\":" + isRoleSpeak.intValue + ","
                + "\"dialogue\":" + "\"" + dialogue.stringValue + "\"}";
        }
        else if (triggerType.enumValueIndex == 1)
        {
            //EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField("生成对象(aoi_id)");
            //creatByAoi.intValue = ConvertUtils.ToInt(EditorGUILayout.TextField(creatByAoi.intValue.ToString(), new GUILayoutOption[0]));
            //EditorGUILayout.EndHorizontal();
            clip.TriggerContent = "ZhenPing";
        }
        else if (triggerType.enumValueIndex == 2)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("初始透明度");
            startAlpha.floatValue = EditorGUILayout.Slider(startAlpha.floatValue, 0, 1, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("最终透明度");
            endAlpha.floatValue = EditorGUILayout.Slider(endAlpha.floatValue, 0, 1, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();
            clip.TriggerContent = "BlackScreen" + startAlpha.floatValue + "," + endAlpha.floatValue;
        }
        else if (triggerType.enumValueIndex == 3)
        {
            EditorGUILayout.BeginHorizontal();
            isCloseMainUI.boolValue = EditorGUILayout.Toggle("关闭主界面", isCloseMainUI.boolValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            isShowSkipBtn.boolValue = EditorGUILayout.Toggle("是否显示跳过按钮", isShowSkipBtn.boolValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            actorHide.boolValue = EditorGUILayout.Toggle("隐藏主角之外的角色", actorHide.boolValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            mainRolehide.boolValue = EditorGUILayout.Toggle("隐藏主角", mainRolehide.boolValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            effectHide.boolValue = EditorGUILayout.Toggle("隐藏触发器特效", effectHide.boolValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            set_slience.boolValue = EditorGUILayout.Toggle("标记为静默[播放时可自动战斗]", set_slience.boolValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            isSetPos.boolValue = EditorGUILayout.Toggle("结束时是否给主角设置位置", isSetPos.boolValue);
            EditorGUILayout.EndHorizontal();
            if (isSetPos.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                roleEndPos.vector3Value = EditorGUILayout.Vector3Field("主角的最终位置", roleEndPos.vector3Value);
                EditorGUILayout.EndHorizontal();
            }
            clip.TriggerContent = $"PingBi{isCloseMainUI.boolValue},{isShowSkipBtn.boolValue}," +
                $"{actorHide.boolValue},{mainRolehide.boolValue}," +
                $"{effectHide.boolValue},{isSetPos.boolValue}," +
                $"{roleEndPos.vector3Value}";
        }
        else if (triggerType.enumValueIndex == 4)
        {

            clip.TriggerContent = $"Skip{isShowSkipBtn.boolValue}";
        }
        else if (triggerType.enumValueIndex == 5)
        {
            EditorGUILayout.BeginHorizontal();
            fromColor.colorValue = EditorGUILayout.ColorField("skyfrom", fromColor.colorValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            toColor.colorValue = EditorGUILayout.ColorField("skyto", toColor.colorValue);
            EditorGUILayout.EndHorizontal();
            Color32 fromColor32 = fromColor.colorValue;
            Color32 toColor32 = toColor.colorValue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            dlFromColor.colorValue = EditorGUILayout.ColorField("dlplayfrom", dlFromColor.colorValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            dlToColor.colorValue = EditorGUILayout.ColorField("dlplayto", dlToColor.colorValue);
            EditorGUILayout.EndHorizontal();
            Color32 dlFromColor32 = dlFromColor.colorValue;
            Color32 dlToColor32 = dlToColor.colorValue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            fogFromColor.colorValue = EditorGUILayout.ColorField("fogfrom", fogFromColor.colorValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            fogToColor.colorValue = EditorGUILayout.ColorField("fogto", fogToColor.colorValue);
            EditorGUILayout.EndHorizontal();
            Color32 fogFromColor32 = fogFromColor.colorValue;
            Color32 fogToColor32 = fogToColor.colorValue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            DLFromColor.colorValue = EditorGUILayout.ColorField("DLfrom", DLFromColor.colorValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DLToColor.colorValue = EditorGUILayout.ColorField("DLto", DLToColor.colorValue);
            EditorGUILayout.EndHorizontal();
            Color32 DLFromColor32 = DLFromColor.colorValue;
            Color32 DLToColor32 = DLToColor.colorValue;

            clip.TriggerContent = $"SkyboxColor{fromColor32.r},{fromColor32.g},{fromColor32.b},{fromColor32.a}|{toColor32.r},{toColor32.g},{toColor32.b},{toColor32.a}" +
                $"&{dlFromColor32.r},{dlFromColor32.g},{dlFromColor32.b},{dlFromColor32.a}|{dlToColor32.r},{dlToColor32.g},{dlToColor32.b},{dlToColor32.a}" +
                $"&{fogFromColor32.r},{fogFromColor32.g},{fogFromColor32.b},{fogFromColor32.a}|{fogToColor32.r},{fogToColor32.g},{fogToColor32.b},{fogToColor32.a}" +
                $"&{DLFromColor32.r},{DLFromColor32.g},{DLFromColor32.b},{DLFromColor32.a}|{DLToColor32.r},{DLToColor32.g},{DLToColor32.b},{DLToColor32.a}";
        }
        else if (triggerType.enumValueIndex == 6 || triggerType.enumValueIndex == 14)
        {
            EditorGUILayout.BeginHorizontal();
            var x = triggerType.enumValueIndex == 14 ? "反" : "";
            EditorGUILayout.LabelField($"{x}消融的BOSSID,多个用逗号隔开,主角填0");
            burnID.stringValue = EditorGUILayout.TextField(burnID.stringValue, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"反消融部位列表，用逗号隔开");
            burnParts.stringValue = EditorGUILayout.TextField(burnParts.stringValue, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();

            clip.TriggerContent = $"Burn{burnID.stringValue}";
        }
        else if (triggerType.enumValueIndex == 7)
        {
            //EditorGUILayout.BeginHorizontal();
            //lockViewSet.vector3Value = EditorGUILayout.Vector3Field("lockView参数", lockViewSet.vector3Value);
            //EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            angle_y.floatValue = EditorGUILayout.FloatField("angle_y", angle_y.floatValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            angle_xz.floatValue = EditorGUILayout.FloatField("angle_xz", angle_xz.floatValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            distance.floatValue = EditorGUILayout.FloatField("distance", distance.floatValue);
            EditorGUILayout.EndHorizontal();

            clip.TriggerContent = $"SetLockView{angle_y.floatValue},{angle_xz.floatValue},{distance.floatValue}";
        }
        else if (triggerType.enumValueIndex == 8)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("摄像机立即复位");
            EditorGUILayout.EndHorizontal();
            clip.TriggerContent = $"CameraBackToRole";
        }
        else if (triggerType.enumValueIndex == 9)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("坐骑ID(int)");
            horseID.intValue = int.Parse(EditorGUILayout.TextField(horseID.intValue.ToString(), new GUILayoutOption[0]));
            EditorGUILayout.EndHorizontal();
            clip.TriggerContent = $"QiCheng{horseID.intValue}";
        }
        else if (triggerType.enumValueIndex == 10)
        {
            EditorGUILayout.BeginHorizontal();
            bossName.stringValue = EditorGUILayout.TextField("设置Boss名字[lang_id]", bossName.stringValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("[不填则读取场景中Boss的名字]");
            EditorGUILayout.EndHorizontal();
            clip.TriggerContent = $"BossShow{bossName.stringValue}";
        }
        else if (triggerType.enumValueIndex == 11)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("场景物件名称");
            sceneObjectName.stringValue = EditorGUILayout.TextField(sceneObjectName.stringValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("场景物件动画名称");
            sceneObjectAnim.stringValue = EditorGUILayout.TextField(sceneObjectAnim.stringValue);
            EditorGUILayout.EndHorizontal();

            clip.TriggerContent = $"PlaySceneAnim{sceneObjectName.stringValue},{sceneObjectAnim.stringValue}";
        }
        else if (triggerType.enumValueIndex == 12)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("SceneID[填入SceneBase.id]");
            sceneID.intValue = EditorGUILayout.IntField(sceneID.intValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            isSetDefaultPos.boolValue = EditorGUILayout.Toggle("指定跳转后的初始位置", isSetDefaultPos.boolValue);
            EditorGUILayout.EndHorizontal();
            if (isSetDefaultPos.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                toPos.vector2Value = EditorGUILayout.Vector2Field("\t", toPos.vector2Value);
                EditorGUILayout.EndHorizontal();
            }
            checkSceneAllDone.boolValue = EditorGUILayout.Toggle("是否全场景加载", checkSceneAllDone.boolValue);

            clip.TriggerContent = $"SceneID{sceneID.intValue},isSetDefaultPos:{isSetDefaultPos.boolValue},toPos:{toPos.vector2Value},checkSceneAllDone:{checkSceneAllDone}";
        }
        else if (triggerType.enumValueIndex == 13)
        {
            EditorGUILayout.BeginHorizontal();
            showSkin.boolValue = EditorGUILayout.Toggle("是否显示跳过按钮", showSkin.boolValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(dios, new GUIContent("可选对话[根据前个支线选择显示对话]"), true, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(btns, new GUIContent("支线按钮[不填则不显示按钮]"), true, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();

            clip.TriggerContent = $"taskdial{showSkin.boolValue}";
        }
        else if (triggerType.enumValueIndex == 15)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(oneByOne, new GUIContent("逐字句显示文本langid+本行总时间 例:lang_001,2.5"), new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            oneByOneJianGe.floatValue = EditorGUILayout.FloatField("每行出现之前延迟(0不延迟)", oneByOneJianGe.floatValue);
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("估算时间", new GUILayoutOption[0]))
            {
                if (oneByOne.arraySize > 0)
                {
                    float time = 0;
                    for (int i = 0, size = oneByOne.arraySize; i < size; i++)
                    {
                        var element = oneByOne.GetArrayElementAtIndex(i);
                        var splitArr = element.stringValue.Split(',');
                        if (splitArr.Length == 2)
                        {
                            bool isFound = false;
                            var strText = UnityEditor.UI.Language.GetString(splitArr[0], out isFound);
                            if (isFound)
                            {
                                time += (float.Parse(splitArr[1]) * strText.Length);
                            }
                        }
                    }

                    if (EditorUtility.DisplayDialog("预估时间", $"{time}s", "设置为clip长度,最好再拉长一些", "no"))
                    {
                        if (time > 0) clip.OwningClip.duration = time;
                    }
                }
            }

            clip.TriggerContent = $"textonebyone{oneByOne.arraySize}";
        }
        else if (triggerType.enumValueIndex == 16)
        {
            EditorGUILayout.BeginHorizontal();
            pauseBtnPos.vector2Value = EditorGUILayout.Vector2Field("暂停按钮坐标", pauseBtnPos.vector2Value);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            pauseEffect.stringValue = EditorGUILayout.TextField("暂停按钮特效(特效全名)", pauseEffect.stringValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            pauseEndEffect.stringValue = EditorGUILayout.TextField("暂停按钮消失特效(特效全名)", pauseEndEffect.stringValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            pauseBtnText.stringValue = EditorGUILayout.TextField("暂停按钮文本(lang_id)", pauseBtnText.stringValue);
            EditorGUILayout.EndHorizontal();
            clip.TriggerContent = $"Pause{pauseBtnPos.vector2Value},{pauseEffect.stringValue},{pauseEndEffect.stringValue},{pauseBtnText.stringValue}";
        }
        if (triggerType.enumValueIndex == 0)
        {
            (target as TriggerTimelineClip).OwningClip.displayName =
                triggerTypeTextList[(int)((target as TriggerTimelineClip).triggerType)] + "_"
                + juQingUIIDTextList[(int)((target as TriggerTimelineClip).uIData.juQingUIID)];
        }
        else
        {
            (target as TriggerTimelineClip).OwningClip.displayName = triggerTypeTextList[(int)((target as TriggerTimelineClip).triggerType)];
        }

        serializedObject.ApplyModifiedProperties();
    }
}