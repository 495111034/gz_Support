using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EditAnimator))]
public class EditAnimatorInspector : Editor
{

    /// <summary>
    /// slider time controller
    /// </summary>
    private float m_SliderTimeController;

    /// <summary>
    /// 是否已经烘培过
    /// </summary>
    private bool m_HasBake;

    private bool m_Playing;
    private float m_RunningTime;
    private double m_PreviousTime;
    private float m_RecorderStopTime;
    private const float kDuration = 30f;
    private Animator m_Animator;
    private Animator m_Animator2;
    //IndoorLightingMgr indoorMgr;

    //private EditAnimator editAnimator { get { return target as EditAnimator; } }

    private Animator animator
    {
        get { return m_Animator; }
    }

    void OnEnable()
    {
        m_PreviousTime = EditorApplication.timeSinceStartup;
        EditorApplication.update += inspectorUpdate;
        m_Animator = GameObject.Find("scene/_EnvLightingMgr/Skybox").GetComponent<Animator>();
        m_Animator2 = GameObject.Find("scene/_EnvLightingMgr/Indoor").GetComponent<Animator>();
        //indoorMgr = GameObject.Find("scene/_EnvLightingMgr/Indoor").GetComponent<IndoorLightingMgr>();
    }

    void OnDisable()
    {
        EditorApplication.update -= inspectorUpdate;
    }


    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Bake"))
        {
            m_HasBake = false;
            bake();
        }
        //if (GUILayout.Button("Play"))
        //{
        //    play();
        //}
        //if (GUILayout.Button("Stop"))
        //{
        //    stop();
        //}
        EditorGUILayout.EndHorizontal();
        m_SliderTimeController = EditorGUILayout.Slider("Time:", m_SliderTimeController, 0f, kDuration);
        manualUpdate();
    }

    /// <summary>
    /// 烘培记录动画数据
    /// </summary>
    private void bake()
    {
        if (m_HasBake)
        {
            return;
        }

        if (Application.isPlaying || animator == null || !m_Animator2)
        {
            return;
        }

        const float frameRate = 30f;
        const int frameCount = (int)((kDuration * frameRate) + 2);

        animator.Rebind();
        animator.StopPlayback();
        animator.recorderStartTime = 0;

        m_Animator2.Rebind();
        m_Animator2.StopPlayback();
        m_Animator2.recorderStartTime = 0;

        // 开始记录指定的帧数
        animator.StartRecording(frameCount);
        m_Animator2.StartRecording(frameCount);

        for (var i = 0; i < frameCount - 1; i++)
        {
            //// 这里可以在指定的时间触发新的动画状态
            //if (i == 200)
            //{
            //    animator.SetTrigger("Dance");
            //}

            // 记录每一帧
            animator.Update(1.0f / frameRate);
            m_Animator2.Update(1.0f / frameRate);
        }
        // 完成记录
        animator.StopRecording();
        m_Animator2.StopRecording();

        // 开启回放模式
        animator.StartPlayback();
        m_Animator2.StartPlayback();
        m_HasBake = true;
        m_RecorderStopTime = animator.recorderStopTime;
    }

    /// <summary>
    /// 进行预览播放
    /// </summary>
    //private void play()
    //{
    //    if (Application.isPlaying || animator == null)
    //    {
    //        return;
    //    }

    //    bake();
    //    m_RunningTime = 0f;
    //    m_Playing = true;
    //}

    /// <summary>
    /// 停止预览播放
    /// </summary>
    //private void stop()
    //{
    //    if (Application.isPlaying || animator == null)
    //    {
    //        return;
    //    }

    //    m_Playing = false;
    //    m_SliderTimeController = 0f;
    //}


    /// <summary>
    /// 预览播放状态下的更新
    /// </summary>
    private void update()
    {
        if (Application.isPlaying || animator == null)
        {
            return;
        }

        if (m_RunningTime > m_RecorderStopTime)
        {
            m_Playing = false;
            return;
        }

        // 设置回放的时间位置
        animator.playbackTime = m_RunningTime;
        animator.Update(0);
        m_SliderTimeController = m_RunningTime;
    }

    /// <summary>
    /// 非预览播放状态下，通过滑杆来播放当前动画帧
    /// </summary>
    private void manualUpdate()
    {
        Debug.Log(animator == null);
        if (m_Animator2 && animator && !m_Playing && m_HasBake && m_SliderTimeController < m_RecorderStopTime)
        {
            animator.playbackTime = m_SliderTimeController;
            animator.Update(0);
            m_Animator2.playbackTime = m_SliderTimeController;
            m_Animator2.Update(0);
            //indoorMgr.ApplyProperties();
        }
    }


    private void inspectorUpdate()
    {
        var delta = EditorApplication.timeSinceStartup - m_PreviousTime;
        m_PreviousTime = EditorApplication.timeSinceStartup;

        if (!Application.isPlaying && m_Playing)
        {
            m_RunningTime = Mathf.Clamp(m_RunningTime + (float)delta, 0f, kDuration);
            update();
        }
    }

}
