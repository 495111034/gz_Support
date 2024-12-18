using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CanEditMultipleObjects]
[CustomEditor(typeof(EffectBehaviour))]
public class EffectBehaviourEditor : Editor
{
    SerializedProperty effective_seconds;
    SerializedProperty callback_seconds;
    SerializedProperty isLowOverhead;
  

    static GUIContent txt_effective_seconds = new GUIContent("显示时间限制(0为不限制)：");
    static GUIContent txt_callback_seconds = new GUIContent("回调时间(如需要)：");
    static GUIContent txtLowoverhead = new GUIContent("低开销特效：");   

    void OnEnable()
    {
        effective_seconds = serializedObject.FindProperty("effective_seconds");
        callback_seconds = serializedObject.FindProperty("callback_seconds");
        isLowOverhead = serializedObject.FindProperty("isLowOverhead");
    }

    private float time = 0;
    private float lasttime = 0;
    private void Awake()
    {
        time = 0;
        lasttime = Time.realtimeSinceStartup;

        if (!Application.isPlaying)
        {
            var s = target as EffectBehaviour;
           // s.played_time = 0f;
            if (s)
                s.Play();
        }
        EditorApplication.update += UpdateHandler;
    }

    private void OnDestroy()
    {
        //var s = target as EffectBehaviour;
       // s.played_time = 0;
        EditorApplication.update -= UpdateHandler;
    }

    void UpdateHandler()
    {       
        if (!Application.isPlaying)
        {
            var s = target as EffectBehaviour;
            if (s != null)
            {
                float deltaTime = Time.realtimeSinceStartup - lasttime;
                lasttime = Time.realtimeSinceStartup;

                time += deltaTime;

                s.OnEditor_Update(deltaTime, time);

                var minDelta = 1.0f / 240;
                var smallDelta = Mathf.Max(0.1f, Time.fixedDeltaTime * 2);
                var largeDelta = 0.2f;

                if (time < s.time ||
                           time > s.time + largeDelta)
                {
                    // Backward seek or big leap
                    // Reset the simulation with the current playhead position.
                    ResetSimulation(time);
                   
                }
                else if (time > s.time + smallDelta)
                {
                    // Fast-forward seek
                    // Simulate without restarting but with fixed steps.
                    s.Simulate(time - s.time, true, false, true);
                    
                }
                else if (time > s.time + minDelta)
                {
                    // Edit mode playback
                    // Simulate without restarting nor fixed step.
                    s.Simulate(time - s.time, true, false, false);
                  
                }
                else
                {
                    // Delta time is too small; Do nothing.
                }
            }

            foreach (var sceneView in SceneView.sceneViews)
            {
                if (sceneView is SceneView)
                {
                    (sceneView as SceneView).Repaint();
                }
            }
        }
    }

    void ResetSimulation(float time)
    {
        var particleSystem = target as EffectBehaviour;
        if (particleSystem == null || particleSystem.PsCount == 0) return;
        const float maxSimTime = 2.0f / 3;

        if (time < maxSimTime)
        {
            // The target time is small enough: Use the default simulation
            // function (restart and simulate for the given time).
            particleSystem.Simulate(time);
        }
        else
        {
            // The target time is larger than the threshold: The default
            // simulation can be heavy in this case, so use fast-forward
            // (simulation with just a single step) then simulate for a small
            // period of time.
            particleSystem.Simulate(time - maxSimTime, true, true, false);
            particleSystem.Simulate(maxSimTime, true, false, true);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(effective_seconds, txt_effective_seconds);
        EditorGUILayout.PropertyField(callback_seconds, txt_callback_seconds);
        EditorGUILayout.PropertyField(isLowOverhead, txtLowoverhead);

        if (!Application.isPlaying)
        {
            if (GUILayout.Button(new GUIContent("重新预览")))
            {
                time = 0;
                var s = target as EffectBehaviour;
                if (s)
                {
                    s.Resert();
                    //s.Play();
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}

