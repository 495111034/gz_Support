using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Cinemachine.Editor;
using UnityEngine;
using UnityEngine.UI;
using Entity;
using UnityEngine.Timeline;
using UnityEditor;
using UnityEditor.Animations;
using Object = UnityEngine.Object;

[CustomEditor(typeof(GPUAnimTimelineClip), true)]
[CanEditMultipleObjects]
public class GPUAnimatorTimelineClipEditor:  UnityEditor.Editor
{
    static List<string> horseAnimTypeTextList = new List<string>()
    {
        "同时播放","仅播放人物动作","仅播放座骑动作","仅播放圣物动作"
    };


    private SerializedProperty spClipName;
    private SerializedProperty spStartOffset;
    private SerializedProperty spEndOffset;
    private SerializedProperty spAnimSpeed;
    private SerializedProperty spWrapMode;
    private SerializedProperty spHorseAnimType;
    private SerializedProperty spUnscaledTime;

    private float animclipLength = 0f;
    
    private void OnEnable()
    {
        spClipName =  serializedObject.FindProperty("clipName");
        spStartOffset = serializedObject.FindProperty("startOffset");
        spEndOffset = serializedObject.FindProperty("endOffset");
        spAnimSpeed = serializedObject.FindProperty("animSpeed");
        spWrapMode = serializedObject.FindProperty("animWrapMode");
        spHorseAnimType = serializedObject.FindProperty("horseAnimType");
        spUnscaledTime = serializedObject.FindProperty("unscaledTime");

    }

    private void OnDisable()
    {
    }

    private void OnDestroy()
    {
    }

    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        GPUAnimTimelineClip clip = target as GPUAnimTimelineClip;
       // Log.LogError(clip.TrackTargetObject.name);
        if (!clip.Templete.parentTrack.trackTargetObject)
        {
            EditorGUILayout.HelpBox("请先设置ObjectBehaviourBase类型的对象", MessageType.Warning);
            return;
        }

        var anim = GetAnimator(clip.Templete.parentTrack.trackTargetObject);

        if (!anim)
        {
            EditorGUILayout.HelpBox("未找到animator对象", MessageType.Warning);
            return;
        }

        animclipLength = 0f;
        if(anim)
            DrawAnimatorClips(anim);
        
        var totalLength = animclipLength - spStartOffset.floatValue - spEndOffset.floatValue;
        clip.OwningClip.duration = totalLength > 0?(totalLength / spAnimSpeed.floatValue):1f;
        
        
        serializedObject.ApplyModifiedProperties();
    }

    Animator GetAnimator(ObjectBehaviourBase root)
    {
        var anims = root.gameObject.GetComponentsEx<Animator>();
        for (int i = 0; i < anims.Count; ++i)
        {
            if (anims[i].runtimeAnimatorController)
                return anims[i];
        }

        return null;
    }

    void DrawAnimatorClips(Animator anim)
    {
        var control = anim.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
        if (control == null)
        {
            animclipLength = 0f;
            Log.LogError($"{anim.name} is not AnimatorController");
            return;
        }

        string selectName = spClipName.stringValue;
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("动画片断");
        List<string> allClips = new List<string>();

        AnimationClip clip;
        if (Application.isPlaying)
        {
            AnimationClip[] animationClips = anim.runtimeAnimatorController.animationClips;
            for (int i = 0; i < animationClips.Length; i++)
            {
                //Log.LogError($"animationClips[i].name:{animationClips[i].name}");
                if (!allClips.Contains(animationClips[i].name))
                    allClips.Add(animationClips[i].name);
            }

            int selectidx = allClips.IndexOf(selectName);
            selectidx = selectidx < 0 ? 0 : selectidx;
            selectidx = EditorGUILayout.Popup(selectidx, allClips.ToArray());
            spClipName.stringValue = allClips[selectidx];

            clip = animationClips.First(x => x.name == spClipName.stringValue);
        }
        else
        {
            Dictionary<string, ChildAnimatorState> animaotrDic = new Dictionary<string, ChildAnimatorState>();
            int layerCount = control.layers.Length;
            for (int i = 0; i < layerCount; i++)
            {
                foreach (var s in control.layers[i].stateMachine.states)
                {
                    if (!allClips.Contains(s.state.name))
                        allClips.Add(s.state.name);

                    if (!animaotrDic.ContainsKey(s.state.name))
                        animaotrDic.Add(s.state.name, s);
                }
            }

            int selectidx = allClips.IndexOf(selectName);
            selectidx = selectidx < 0 ? 0 : selectidx;
            selectidx = EditorGUILayout.Popup(selectidx, allClips.ToArray());
            spClipName.stringValue = allClips[selectidx];

            var selectState = animaotrDic[spClipName.stringValue];
            clip = selectState.state.motion as AnimationClip;
        }
        EditorGUILayout.EndHorizontal();

        (target as GPUAnimTimelineClip).name = spClipName.stringValue;
        (target as GPUAnimTimelineClip).OwningClip.displayName = spClipName.stringValue;

        if (!clip)
        {
            animclipLength = 0f;
            EditorGUILayout.HelpBox("当前动画的clip为空", MessageType.Error);
            return;
        }

        animclipLength = clip.length;
        
        EditorGUILayout.LabelField($"动画长度：{animclipLength}秒");
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("启始偏移值");
        var r = animclipLength - spEndOffset.floatValue - (animclipLength * 0.1f);
        spStartOffset.floatValue = EditorGUILayout.Slider(spStartOffset.floatValue, 0, r> 0?r:0f,new GUILayoutOption[0]);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("结束偏移值");
        r = animclipLength - spStartOffset.floatValue - (animclipLength * 0.1f);
        spEndOffset.floatValue =
            EditorGUILayout.Slider(spEndOffset.floatValue, 0, r > 0 ? r : 0f, new GUILayoutOption[0]);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("动画速度");
        spAnimSpeed.floatValue = EditorGUILayout.Slider(spAnimSpeed.floatValue, 0.1f, 5f, new GUILayoutOption[0]);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("是否忽略timeScale");
        spUnscaledTime.boolValue = EditorGUILayout.Toggle(spUnscaledTime.boolValue);
        EditorGUILayout.EndHorizontal();
        
    }
    
}
