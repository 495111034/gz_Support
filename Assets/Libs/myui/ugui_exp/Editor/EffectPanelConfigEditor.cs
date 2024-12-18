using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{

    [CustomEditor(typeof(EffectPanelConfig), true)]
    [CanEditMultipleObjects]
    public class EffectPanelConfigEditor : Editor
    {
        SerializedProperty _appearEffectProperty;
        SerializedProperty _appearSrcProperty;
        SerializedProperty _appearSrcRatioProperty;
        SerializedProperty _hideEffectProperty;
        SerializedProperty _hideDstProperty;
        SerializedProperty _hideDstRatioProperty;
        SerializedProperty _appearScaleRatioProperty;
        SerializedProperty _hideScaleRatioProperty;
        SerializedProperty _appearTimespaceProperty;
        SerializedProperty _hideTimespaceProperty;
        SerializedProperty _appearEaseTypeProperty;
        SerializedProperty _hideEaseTypeProperty;
        SerializedProperty _appear_local_moveProperty;
        SerializedProperty _hide_local_moveProperty;
        SerializedProperty _appearDelayTimeProperty, _hideDelayTimeProperty, _appear_sound_idProperty, _hide_sound_idProperty, _appear_sound_delayProperty, _hide_sound_delayProperty, _appear_compete_sound_idProperty, _hide_complete_sound_idProperty;

        SerializedProperty _panelUseProperty;
        SerializedProperty _gameObjUseProperty;
        SerializedProperty _listTween_1Property;
        SerializedProperty _listTween_2Property;
        SerializedProperty _list_tween_durProperty;
        SerializedProperty _list_tween_delayProperty;
        

        SerializedProperty _use_fade_tweenProperty;
        SerializedProperty _fade_timeProperty;
        SerializedProperty _fade_delayProperty;

        SerializedProperty _use_path_moveProperty;
        SerializedProperty _path_transformsProperty;
        SerializedProperty _path_move_timeProperty;

        SerializedProperty _testingProperty;

        GUIContent appearEffectContnet, appearSrcContent, appearSrcRatioContent, appearTimespaceContent, hideTimespaceContent,
            hideEffectContent, hideDstContent, hideDstRatioContent, appearScaleRatioContent, hideScaleRatioContent,
            appearEaseTypeContent, hideEaseTypeContent;

        SerializedProperty _initPosition;

        GUIContent _appearDelayTimeContent, _hideDelayTimeContent, _appear_sound_idContent, _hide_sound_idContent, _appear_sound_delayContent, _hide_sound_delayContent, _appear_compete_sound_idContent, _hide_complete_sound_idContent;


        GUIContent _panelUseContent, _gameObjUseContent, _listTween_2Content, _listTween_1Content, _list_tween_durContent, _list_tween_delayContent;
        GUIContent _appear_local_moveContent, _hide_local_moveContent;
        GUIContent _use_fade_tweenContent, _fade_timeContent;
        GUIContent _use_path_moveContent, _path_transformsContent, _path_move_timeContent;
        GUIContent _testingContent;


        protected virtual void OnEnable()
        {
            _appearEffectProperty = serializedObject.FindProperty("_appearEffect");
            _appearSrcProperty = serializedObject.FindProperty("_appearSrc");
            _appearSrcRatioProperty = serializedObject.FindProperty("_appearSrcRatio");
            _hideEffectProperty = serializedObject.FindProperty("_hideEffect");
            _hideDstProperty = serializedObject.FindProperty("_hideDst");
            _hideDstRatioProperty = serializedObject.FindProperty("_hideDstRatio");
            _appearScaleRatioProperty = serializedObject.FindProperty("_appearScaleRatio");
            _hideScaleRatioProperty = serializedObject.FindProperty("_hideScaleRatio");
            _appearTimespaceProperty = serializedObject.FindProperty("_appearTimespace");
            _hideTimespaceProperty = serializedObject.FindProperty("_hideTimespace");
            _appearEaseTypeProperty = serializedObject.FindProperty("_appearEaseType");
            _hideEaseTypeProperty = serializedObject.FindProperty("_hideEaseType");

            _appearDelayTimeProperty = serializedObject.FindProperty("_appearDelayTime");
            _hideDelayTimeProperty = serializedObject.FindProperty("_hideDelayTime");
            _appear_sound_idProperty = serializedObject.FindProperty("_appear_sound_id");
            _hide_sound_idProperty = serializedObject.FindProperty("_hide_sound_id");
            _appear_sound_delayProperty = serializedObject.FindProperty("_appear_sound_delay");
            _hide_sound_delayProperty = serializedObject.FindProperty("_hide_sound_delay");
            _appear_compete_sound_idProperty = serializedObject.FindProperty("_appear_compete_sound_id");
            _hide_complete_sound_idProperty = serializedObject.FindProperty("_hide_complete_sound_id");
            _appear_local_moveProperty = serializedObject.FindProperty("_appear_local_move");
            _hide_local_moveProperty = serializedObject.FindProperty("_hide_local_move");

            _initPosition = serializedObject.FindProperty("initPos");

            _panelUseProperty = serializedObject.FindProperty("_panelUse");
            _gameObjUseProperty = serializedObject.FindProperty("_gameObjUse");
            _listTween_1Property = serializedObject.FindProperty("_listTween_1");
            _listTween_2Property = serializedObject.FindProperty("_listTween_2");
            _list_tween_durProperty = serializedObject.FindProperty("_list_tween_dur");
            _list_tween_delayProperty = serializedObject.FindProperty("_list_tween_delay");


            _use_fade_tweenProperty = serializedObject.FindProperty("_use_fade_tween");
            _fade_timeProperty = serializedObject.FindProperty("_fate_time");
            _fade_delayProperty = serializedObject.FindProperty("_fate_delay");

            _use_path_moveProperty = serializedObject.FindProperty("_use_path_move");
            _path_transformsProperty = serializedObject.FindProperty("_path_transforms");
            _path_move_timeProperty = serializedObject.FindProperty("_path_move_time");

            _testingProperty = serializedObject.FindProperty("_testing");

            appearEffectContnet = new GUIContent("出现效果");
            appearSrcContent = new GUIContent("出现的来源方向");
            appearSrcRatioContent = new GUIContent("来源方向的比例");
            appearScaleRatioContent = new GUIContent("来源的缩放比例");
            hideEffectContent = new GUIContent("隐藏效果");
            hideDstContent = new GUIContent("隐藏方向");
            hideDstRatioContent = new GUIContent("隐藏方向比例");
            appearScaleRatioContent = new GUIContent("隐藏缩放比例");
            appearTimespaceContent = new GUIContent("出现效果时间");
            hideTimespaceContent = new GUIContent("隐藏效果时间");
            appearEaseTypeContent = new GUIContent("出现加速度算法");
            hideEaseTypeContent = new GUIContent("隐藏加速度算法");

            _appearDelayTimeContent = new GUIContent("延时时间");
            _hideDelayTimeContent = new GUIContent("延时时间");
            _appear_sound_idContent = new GUIContent("声音");
            _hide_sound_idContent = new GUIContent("声音");
            _appear_sound_delayContent = new GUIContent("声音延时");
            _hide_sound_delayContent = new GUIContent("声音延时");
            _appear_compete_sound_idContent = new GUIContent("完成时声音");
            _hide_complete_sound_idContent = new GUIContent("完成时声音");
            _appear_local_moveContent = new GUIContent("子界面移动");
            _hide_local_moveContent = new GUIContent("子界面移动");

            _gameObjUseContent = new GUIContent("游戏对象调用");
            _panelUseContent = new GUIContent("界面底层调用");
            _listTween_1Content = new GUIContent("列表平铺展开");
            _listTween_2Content = new GUIContent("列表缩放展开");
            _list_tween_durContent = new GUIContent("子项效果时间");
            _list_tween_delayContent = new GUIContent("延迟时间");

            _use_path_moveContent = new GUIContent("Tween path 移动");
            _path_transformsContent = new GUIContent("Path节点");
            _path_move_timeContent = new GUIContent("移动时间");

            _use_fade_tweenContent = new GUIContent("透明度变化");

            _testingContent = new GUIContent("UI预览");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();  //start

            EditorGUILayout.PropertyField(_appearEffectProperty, appearEffectContnet);
            if (_appearEffectProperty.boolValue)
            {
                ++EditorGUI.indentLevel;
                {
                    EditorGUILayout.PropertyField(_appear_local_moveProperty, _appear_local_moveContent);
                    if (_appear_local_moveProperty.boolValue)
                    {
                        EditorGUILayout.HelpBox("子界面移动，因为位置信息会受父节点影响，所以子节点位置必须同步父节点" + '\n' + "RectTransform.Anchors"
                            + '\n'  + "Min:(0,1)"
                            +'\n' + "Max:(0,1)"
                            +'\n' + "Pivot:(0,1)",
                            MessageType.Info);
                    }

                    EditorGUILayout.PropertyField(_appearSrcProperty, appearSrcContent);
                    EditorGUILayout.PropertyField(_appearSrcRatioProperty, appearSrcRatioContent);
                    EditorGUILayout.PropertyField(_appearScaleRatioProperty, appearScaleRatioContent);
                    EditorGUILayout.PropertyField(_appearTimespaceProperty, appearTimespaceContent);
                    if (_appearTimespaceProperty.floatValue == 0f) EditorGUILayout.HelpBox("时间不能为0", MessageType.Error);
                    if (_appearTimespaceProperty.floatValue > 1f) EditorGUILayout.HelpBox("时间太长影响体验", MessageType.Warning);
                    EditorGUILayout.PropertyField(_appearEaseTypeProperty, appearEaseTypeContent);
                    if (_appearEaseTypeProperty.intValue != (int)iTween.EaseType.none) EditorGUILayout.HelpBox("详情百度搜索: tweenlite 缓动函数测试图", MessageType.None);

                    EditorGUILayout.PropertyField(_appearDelayTimeProperty, _appearDelayTimeContent);
                    EditorGUILayout.PropertyField(_appear_sound_idProperty, _appear_sound_idContent);
                    if (!string.IsNullOrEmpty(_appear_sound_idProperty.stringValue))
                    {
                        ++EditorGUI.indentLevel;
                        EditorGUILayout.PropertyField(_appear_sound_delayProperty, _appear_sound_delayContent);
                        --EditorGUI.indentLevel;
                    }
                    EditorGUILayout.PropertyField(_appear_compete_sound_idProperty, _appear_compete_sound_idContent);

                    //EditorGUILayout.PropertyField(_panelUseProperty, _panelUseContent);
                    //EditorGUILayout.PropertyField(_gameObjUseProperty, _gameObjUseContent);
                    //if (!_panelUseProperty.boolValue && !_gameObjUseProperty.boolValue)
                    //{
                    //    EditorGUILayout.HelpBox("(游戏对象调用/界面底层调用)都不选，则由上层调用或不需要改功能", MessageType.Info);
                    //}

                }
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.PropertyField(_hideEffectProperty, hideEffectContent);
            if (_hideEffectProperty.boolValue)
            {
                ++EditorGUI.indentLevel;
                {
                    EditorGUILayout.PropertyField(_hide_local_moveProperty, _hide_local_moveContent);
                    if (_hide_local_moveProperty.boolValue)
                    {
                        EditorGUILayout.HelpBox("游戏对象必须居中", MessageType.Info);
                    }

                    EditorGUILayout.PropertyField(_hideDstProperty, hideDstContent);
                    EditorGUILayout.PropertyField(_hideDstRatioProperty, hideDstRatioContent);
                    EditorGUILayout.PropertyField(_hideScaleRatioProperty, appearScaleRatioContent);
                    EditorGUILayout.PropertyField(_hideTimespaceProperty, hideTimespaceContent);
                    if (_hideTimespaceProperty.floatValue == 0f) EditorGUILayout.HelpBox("时间不能为0", MessageType.Error);
                    if (_hideTimespaceProperty.floatValue > 1f) EditorGUILayout.HelpBox("时间太长影响体验", MessageType.Warning);
                    EditorGUILayout.PropertyField(_hideEaseTypeProperty, hideEaseTypeContent);
                    if (_appearEaseTypeProperty.intValue != (int)iTween.EaseType.none) EditorGUILayout.HelpBox("详情百度搜索: tweenlite 缓动函数测试图", MessageType.None);

                    EditorGUILayout.PropertyField(_hideDelayTimeProperty, _hideDelayTimeContent);
                    EditorGUILayout.PropertyField(_hide_sound_idProperty, _hide_sound_idContent);
                    if (!string.IsNullOrEmpty(_hide_sound_idProperty.stringValue))
                    {
                        ++EditorGUI.indentLevel;
                        EditorGUILayout.PropertyField(_hide_sound_delayProperty, _hide_sound_delayContent);
                        --EditorGUI.indentLevel;
                    }
                    EditorGUILayout.PropertyField(_hide_complete_sound_idProperty, _hide_complete_sound_idContent);

                    //EditorGUILayout.PropertyField(_panelUseProperty, _panelUseContent);
                    //EditorGUILayout.PropertyField(_gameObjUseProperty, _gameObjUseContent);
                    //if (!_panelUseProperty.boolValue && !_gameObjUseProperty.boolValue )
                    //{
                    //    EditorGUILayout.HelpBox("(游戏对象调用/界面底层调用)都不选，则由上层调用或不需要改功能", MessageType.Info);
                    //}
                }
                --EditorGUI.indentLevel;
            }

            if(Application.isPlaying)
            {
                var pos = _initPosition.vector2Value ;
                EditorGUILayout.LabelField($"初始位置： x:{pos.x},y:{pos.y}");
            }

            EditorGUILayout.PropertyField(_listTween_1Property, _listTween_1Content);

            if (_listTween_1Property.boolValue)
            {
                ++EditorGUI.indentLevel;
                {
                    EditorGUILayout.PropertyField(_list_tween_durProperty, _list_tween_durContent);
                    EditorGUILayout.PropertyField(_list_tween_delayProperty, _list_tween_delayContent);

                    if (_list_tween_durProperty.floatValue == 0f) EditorGUILayout.HelpBox("时间不能为0", MessageType.Error);

                    EditorGUILayout.PropertyField(_appearEaseTypeProperty, appearEaseTypeContent);
                    if (_appearEaseTypeProperty.intValue != (int)iTween.EaseType.none) EditorGUILayout.HelpBox("详情百度搜索: tweenlite 缓动函数测试图", MessageType.None);

                    //EditorGUILayout.PropertyField(_panelUseProperty, _panelUseContent);
                    //EditorGUILayout.PropertyField(_gameObjUseProperty, _gameObjUseContent);
                    //if (!_panelUseProperty.boolValue && !_gameObjUseProperty.boolValue)
                    //{
                    //    EditorGUILayout.HelpBox("(游戏对象调用/界面底层调用)都不选，则由上层调用或不需要改功能", MessageType.Info);
                    //}
                }
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.PropertyField(_listTween_2Property, _listTween_2Content);

            if (_listTween_2Property.boolValue)
            {
                ++EditorGUI.indentLevel;
                {
                    EditorGUILayout.PropertyField(_list_tween_durProperty, _list_tween_durContent);
                    EditorGUILayout.PropertyField(_list_tween_delayProperty, _list_tween_delayContent);
                    if (_list_tween_durProperty.floatValue == 0f) EditorGUILayout.HelpBox("时间不能为0", MessageType.Error);

                    EditorGUILayout.PropertyField(_appearEaseTypeProperty, appearEaseTypeContent);
                    if (_appearEaseTypeProperty.intValue != (int)iTween.EaseType.none) EditorGUILayout.HelpBox("详情百度搜索: tweenlite 缓动函数测试图", MessageType.None);

                    //EditorGUILayout.PropertyField(_panelUseProperty, _panelUseContent);
                    //EditorGUILayout.PropertyField(_gameObjUseProperty, _gameObjUseContent);
                    //if (!_panelUseProperty.boolValue && !_gameObjUseProperty.boolValue)
                    //{
                    //    EditorGUILayout.HelpBox("(游戏对象调用/界面底层调用)都不选，则由上层调用或不需要改功能", MessageType.Info);
                    //}
                }
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.PropertyField(_use_fade_tweenProperty, _use_fade_tweenContent);
            if(_use_fade_tweenProperty.boolValue)
            {
                ++EditorGUI.indentLevel;

                EditorGUILayout.PropertyField(_fade_timeProperty, _fade_timeContent);
                if (_fade_timeProperty.floatValue == 0f) EditorGUILayout.HelpBox("时间不能为0", MessageType.Error);

                EditorGUILayout.PropertyField(_fade_delayProperty, _appearDelayTimeContent);

                --EditorGUI.indentLevel;
            }

            EditorGUILayout.PropertyField(_use_path_moveProperty, _use_path_moveContent);
            if (_use_path_moveProperty.boolValue)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(_path_move_timeProperty, _path_move_timeContent);
                if (_path_move_timeProperty.floatValue == 0f) EditorGUILayout.HelpBox("时间不能为0", MessageType.Error);

                EditorGUILayout.PropertyField(_appearEaseTypeProperty, appearEaseTypeContent);
                if (_appearEaseTypeProperty.intValue != (int)iTween.EaseType.none) EditorGUILayout.HelpBox("详情百度搜索: tweenlite 缓动函数测试图", MessageType.None);

                EditorGUILayout.PropertyField(_path_transformsProperty, _path_transformsContent);
                if (_path_transformsProperty.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("没有指定路径父节点", MessageType.Error);
                }
                else
                {
                    var rect = _path_transformsProperty.objectReferenceValue as RectTransform;
                    if (rect.childCount <= 0)
                    {
                        EditorGUILayout.HelpBox("该父节点没有路径节点", MessageType.Error);
                    }
                }

                --EditorGUI.indentLevel;

            }

            EditorGUILayout.PropertyField(_testingProperty, _testingContent);

            serializedObject.ApplyModifiedProperties();//end

        }
    }
}
