using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(PanelConfig), true)]
    [CanEditMultipleObjects]
    public class PanelConfigEditor : Editor
    {
        SerializedProperty spHideScene;
        SerializedProperty spShowScenebackGround;
        SerializedProperty spBackgroundColor;
        SerializedProperty spHideWhenSceneChange;
        SerializedProperty spReshowWhenEnterScene;
        SerializedProperty spHideWhenClickOther;
        SerializedProperty spShowCommonToolsbar;
        SerializedProperty spShowCommonToolsbar2;
        SerializedProperty spShowCommonScrollview;
        SerializedProperty spAutoPlayPlay;
        SerializedProperty spDependPrefabAssets;
        SerializedProperty spDependPictureAssets;
        SerializedProperty spPanelLevel;
        SerializedProperty spRejectOtherLevel;
        SerializedProperty spPanelType;
        SerializedProperty spActivePanelType;
        SerializedProperty spScrollConfigId;
        SerializedProperty spTopLangId;
        SerializedProperty spSecSprite;


        GUIContent txtHideScene;
        GUIContent txtShowSceneBackground;
        GUIContent txtBackgroudColor;
        GUIContent txtHideWhenSceneChange;
        GUIContent txtReshowWhenEnterScene;
        GUIContent txtHideWhenClickOther;
        GUIContent txtShowCommonToolsbar;
        GUIContent txtShowCommonToolsbar2;
        GUIContent txtShowCommonScrollview;
        GUIContent texAutoPlayPlay;
        GUIContent txtDependPrefabAssets;
        GUIContent txtDependPictureAssets;
        GUIContent txtPanelLevel;
        GUIContent txtRejectOtherLevel;
        GUIContent txtNumber;
        GUIContent txtScrollConfigId;
        GUIContent txtTopLangId;
        GUIContent txtPanelType;
        GUIContent txtActivePanelType;
        GUIContent txtSecSprite;


        List<string> panelTypeList = new List<string>()
    {
        "主界面","一般界面","动态界面","提示界面","对话界面","剧情界面"
    };

        List<string> activePanelTypeList = new List<string>()
    {
        "2D界面","3D界面"
    };

        protected virtual void OnEnable()
        {
            txtHideScene = new GUIContent("隐藏场景");
            spHideScene = serializedObject.FindProperty("__hide_scene");

            spShowScenebackGround = serializedObject.FindProperty("__show_scene_background");
            txtShowSceneBackground = new GUIContent("截屏作为背景");

            spBackgroundColor = serializedObject.FindProperty("__background_color");
            txtBackgroudColor = new GUIContent("截屏背景的混合色");

            txtHideWhenSceneChange = new GUIContent("切换场景时隐藏");
            spHideWhenSceneChange = serializedObject.FindProperty("__hide_when_leave_scene");

            txtReshowWhenEnterScene = new GUIContent("进入场景后重新显示");
            spReshowWhenEnterScene = serializedObject.FindProperty("__reshow_when_entry_scene");

            txtHideWhenClickOther = new GUIContent("点击其它时关掉");
            spHideWhenClickOther = serializedObject.FindProperty("__hide_when_click_other");

            txtShowCommonToolsbar = new GUIContent("显示通用头顶工具栏");
            spShowCommonToolsbar = serializedObject.FindProperty("__show_comm_topbar");

            txtShowCommonToolsbar2 = new GUIContent("显示通用头顶工具栏2");
            spShowCommonToolsbar2 = serializedObject.FindProperty("__show_comm_topbar2");

            txtShowCommonScrollview = new GUIContent("显示通用侧边拖动条");
            spShowCommonScrollview = serializedObject.FindProperty("__show_comm_leftscroll");

            texAutoPlayPlay = new GUIContent("自动播放界面动效");
            spAutoPlayPlay = serializedObject.FindProperty("__auto_play_effect");

            txtDependPrefabAssets = new GUIContent("预加载prefab");
            spDependPrefabAssets = serializedObject.FindProperty("__depend_prefab_res");

            txtDependPictureAssets = new GUIContent("预加载图片");
            spDependPictureAssets = serializedObject.FindProperty("__depend_picture_res");

            txtPanelLevel = new GUIContent("互斥等级");
            spPanelLevel = serializedObject.FindProperty("__panel_level");

            txtRejectOtherLevel = new GUIContent("排斥的等级");
            spRejectOtherLevel = serializedObject.FindProperty("__Reject_other_panel_level");

            txtPanelType = new GUIContent("界面类型");
            spPanelType = serializedObject.FindProperty("__panelType");

            txtActivePanelType = new GUIContent("一般界面类型");
            spActivePanelType = serializedObject.FindProperty("__activePanelType");


            txtNumber = new GUIContent("数量");

            txtScrollConfigId = new GUIContent("拖动条配置id");
            spScrollConfigId = serializedObject.FindProperty("__scroll_view_config_id");

            txtTopLangId = new GUIContent("头顶框标题");
            spTopLangId = serializedObject.FindProperty("__top_lang_id");

            txtSecSprite = new GUIContent("使用第二套通用资源");
            spSecSprite = serializedObject.FindProperty("__use_secsprite");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(spHideScene, txtHideScene, new GUILayoutOption[0]);

            if (spHideScene.boolValue)
            {
                EditorGUILayout.PropertyField(spShowScenebackGround, txtShowSceneBackground, new GUILayoutOption[0]);
                if (spShowScenebackGround.boolValue)
                {
                    EditorGUILayout.PropertyField(spBackgroundColor, txtBackgroudColor, new GUILayoutOption[0]);
                }
            }

            EditorGUILayout.PropertyField(spHideWhenSceneChange, txtHideWhenSceneChange, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spReshowWhenEnterScene, txtReshowWhenEnterScene, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spHideWhenClickOther, txtHideWhenClickOther, new GUILayoutOption[0]);
          
            EditorGUILayout.PropertyField(spShowCommonToolsbar, txtShowCommonToolsbar, new GUILayoutOption[0]);
            if (spShowCommonToolsbar.boolValue)
            {
                //EditorGUILayout.TextField(txtTopLangId);
                spTopLangId.stringValue = EditorGUILayout.TextField(txtTopLangId, spTopLangId.stringValue, new GUILayoutOption[0]);
            }

            EditorGUILayout.PropertyField(spShowCommonToolsbar2, txtShowCommonToolsbar2, new GUILayoutOption[0]);

            EditorGUILayout.PropertyField(spShowCommonScrollview, txtShowCommonScrollview, new GUILayoutOption[0]);
            if (spShowCommonScrollview.boolValue)
            {
                EditorGUILayout.PropertyField(spScrollConfigId, txtScrollConfigId, new GUILayoutOption[0]);
            }

            if(spShowCommonToolsbar.boolValue || spShowCommonScrollview.boolValue)
            {
                EditorGUILayout.PropertyField(spSecSprite, txtSecSprite, new GUILayoutOption[0]);
            }

            EditorGUILayout.PropertyField(spAutoPlayPlay, texAutoPlayPlay, new GUILayoutOption[0]);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(txtPanelType);
            int selectIdx = Mathf.Clamp(spPanelType.enumValueIndex, 0, panelTypeList.Count - 1);
            selectIdx = EditorGUILayout.Popup(selectIdx, panelTypeList.ToArray());
            spPanelType.enumValueIndex = selectIdx;
            (serializedObject.targetObject as PanelConfig).panelType = (PanelType)selectIdx;
            EditorGUILayout.EndHorizontal();


            if (selectIdx == 1)
            {
                ++EditorGUI.indentLevel;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(txtActivePanelType);
                int selectIdx2 = Mathf.Clamp(spActivePanelType.enumValueIndex, 0, activePanelTypeList.Count - 1);
                selectIdx2 = EditorGUILayout.Popup(selectIdx2, activePanelTypeList.ToArray());
                spActivePanelType.enumValueIndex = selectIdx2;
                (serializedObject.targetObject as PanelConfig).activePanelType = (ActivePanelType)selectIdx2;
                EditorGUILayout.EndHorizontal();
                --EditorGUI.indentLevel;
            }


            EditorGUILayout.LabelField(txtDependPrefabAssets);
            ++EditorGUI.indentLevel;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(txtNumber);
            spDependPrefabAssets.arraySize = EditorGUILayout.DelayedIntField(spDependPrefabAssets.arraySize, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();
            for (int i = 0; i < spDependPrefabAssets.arraySize; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i + 1}：");
                spDependPrefabAssets.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(spDependPrefabAssets.GetArrayElementAtIndex(i).stringValue, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();
            }
            --EditorGUI.indentLevel;

            EditorGUILayout.LabelField(txtDependPictureAssets);
            ++EditorGUI.indentLevel;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(txtNumber);
            spDependPictureAssets.arraySize = EditorGUILayout.DelayedIntField(spDependPictureAssets.arraySize, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();
            for (int i = 0; i < spDependPictureAssets.arraySize; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i + 1}：");
                spDependPictureAssets.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(spDependPictureAssets.GetArrayElementAtIndex(i).stringValue, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();
            }
            --EditorGUI.indentLevel;
           
            EditorGUILayout.PropertyField(spPanelLevel, txtPanelLevel, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spRejectOtherLevel, txtRejectOtherLevel, new GUILayoutOption[0]);
            
            serializedObject.ApplyModifiedProperties();//end
        }
    }
}
