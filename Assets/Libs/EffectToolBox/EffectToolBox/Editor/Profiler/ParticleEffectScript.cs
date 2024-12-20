﻿#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace GameSupport.EffectToolBox
{
    /// <summary>
    /// 特效性能分析工具的管理类
    /// 将此类添加到特效上即可
    /// </summary>
    public class ParticleEffectScript : MonoBehaviour
    {

        public AnimationCurve ParticalCount = new AnimationCurve();
        public AnimationCurve DrawCall = new AnimationCurve();
        public AnimationCurve Overdraw = new AnimationCurve();
        //特效是否循环播放
        public bool loop = false;
        [Range(1, 10)]
        public int runTime = 3;

        EffectEvla m_EffectEvla;
        ParticleSystem[] m_ParticleSystems;
        MethodInfo m_CalculateEffectUIDataMethod;
        int m_ParticleCount = 0;
        int m_MaxParticleCount = 0;

        ParticleEffectCurve m_CurveParticleCount;
        ParticleEffectCurve m_CurveDrawCallCount;
        ParticleEffectCurve m_CurveOverdraw;

        void Awake()
        {
            Debug.Log("开始测试单个粒子系统");
            Application.targetFrameRate = ParticleEffectCurve.FPS;

            m_CurveParticleCount = new ParticleEffectCurve();
            m_CurveDrawCallCount = new ParticleEffectCurve();
            m_CurveOverdraw = new ParticleEffectCurve();
            m_EffectEvla = new EffectEvla(Camera.main);
        }

        void Start()
        {
            m_ParticleSystems = GetComponentsInChildren<ParticleSystem>();
#if UNITY_2018_1_OR_NEWER
        m_CalculateEffectUIDataMethod = typeof(ParticleSystem).GetMethod("CalculateEffectUIData", BindingFlags.Instance | BindingFlags.NonPublic);
#else
            m_CalculateEffectUIDataMethod = typeof(ParticleSystem).GetMethod("CountSubEmitterParticles", BindingFlags.Instance | BindingFlags.NonPublic);
#endif
        }

        private void LateUpdate()
        {
            RecordParticleCoun();
            m_EffectEvla.Update();

            UpdateParticleCountCurve();
            UpdateDrawCallCurve();
            UpdateOverdrawCurve();
        }

        public EffectEvlaData[] GetEffectEvlaData()
        {
            return m_EffectEvla.GetEffectEvlaData();
        }

        public void RecordParticleCoun()
        {
            m_ParticleCount = 0;
            foreach (var ps in m_ParticleSystems)
            {
                int count = 0;
#if UNITY_2018_1_OR_NEWER
                object[] invokeArgs = { count, 0.0f, Mathf.Infinity };
            m_CalculateEffectUIDataMethod.Invoke(ps, invokeArgs);
            count = (int)invokeArgs[0];
#else
                object[] invokeArgs = { count };
                m_CalculateEffectUIDataMethod.Invoke(ps, invokeArgs);
                count = (int)invokeArgs[0];
                count += ps.particleCount;
#endif
                m_ParticleCount += count;
            }
            if (m_MaxParticleCount < m_ParticleCount)
            {
                m_MaxParticleCount = m_ParticleCount;
            }
        }

        public int GetParticleCount()
        {
            return m_ParticleCount;
        }
        public int GetMaxParticleCount()
        {
            return m_MaxParticleCount;
        }

        void UpdateParticleCountCurve()
        {
            ParticalCount = m_CurveParticleCount.UpdateAnimationCurve(m_ParticleCount, loop, runTime);
        }
        void UpdateDrawCallCurve()
        {
            DrawCall = m_CurveDrawCallCount.UpdateAnimationCurve(GetParticleEffectData.GetOnlyParticleEffecDrawCall(), loop, runTime);
        }

        void UpdateOverdrawCurve()
        {
            EffectEvlaData[] effectEvlaData = this.GetEffectEvlaData();
            Overdraw = m_CurveOverdraw.UpdateAnimationCurve((float)effectEvlaData[0].GetPixRate(), loop, runTime);
        }

        //监听apply事件
        [InitializeOnLoadMethod]
        static void StartInitializeOnLoadMethod()
        {
            PrefabUtility.prefabInstanceUpdated += delegate (GameObject instance)
            {
                var particleEffectScript = instance.GetComponentsInChildren<ParticleEffectScript>(true);

                if (particleEffectScript.Length > 0)
                {
                    Debug.LogError("保存前请先删除ParticleEffectScript脚本！");
                }
            };
        }
    }
}
#endif