using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace GameSupport.EffectToolBox
{
    public class ArtToolsEffectProfiler
    {
        private const string RequestTestKey = "TestParticleEffectRquestTest";
        private static bool _hasPlayed;
        private static GameObject m_CurrEffect;
        static bool isRestart = false;

        static string[] m_Label = new string[20];

        public static void Profiler()
        {
            var go = Selection.activeGameObject;
            //m_CurrEffect = go;
            /*
             var particleSystemRenderer = go.GetComponentsInChildren<ParticleSystemRenderer>(true);

            if (particleSystemRenderer.Length == 0)
            {
                Debug.LogError("不是特效无法测试！");
                return;
            }
            */
            EditorPrefs.SetBool(RequestTestKey, true);

            //已经在播放状态，使其重新开始
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                isRestart = true;
            }
            else
                EditorApplication.isPlaying = true;
        }

        public static void OnGUI()
        {
            var go = Selection.activeGameObject;

            if (!go)
                return;

            ParticleEffectScript particleEffectScript = go.GetComponent<ParticleEffectScript>() ;

            if(!particleEffectScript)
                return;

            int index = 0;
            m_Label[index] = GetParticleEffectData.GetGetRuntimeMemorySizeStr(particleEffectScript.gameObject);
            m_Label[++index] = GetParticleEffectData.GetParticleSystemCount(particleEffectScript.gameObject);

           
            m_Label[++index] = GetParticleEffectData.GetOnlyParticleEffecDrawCallStr();
            m_Label[++index] = GetParticleEffectData.GetParticleCountStr(particleEffectScript);
            m_Label[++index] = GetParticleEffectData.GetPixDrawAverageStr(particleEffectScript);
            m_Label[++index] = GetParticleEffectData.GetPixActualDrawAverageStr(particleEffectScript);
            m_Label[++index] = GetParticleEffectData.GetPixRateStr(particleEffectScript);


            for (int i = 0; i < m_Label.Length; i++)
            {
                if (!string.IsNullOrEmpty(m_Label[i]))
                {
                    EditorGUILayout.LabelField(m_Label[i]);
                }
            }

        }

        static ArtToolsEffectProfiler()
        {
            EditorApplication.update += Update;
            EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
        }

        private static void Update()
        {
            if (EditorPrefs.HasKey(RequestTestKey) && !_hasPlayed &&
                EditorApplication.isPlaying &&
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorPrefs.DeleteKey(RequestTestKey);
                _hasPlayed = true;

                var go = Selection.activeGameObject;

                var particleEffectScript = go.GetComponentsInChildren<ParticleEffectScript>(true);

                if (particleEffectScript.Length == 0)
                {
                    go.AddComponent<ParticleEffectScript>();
                }
            }
        }

        private static void PlaymodeStateChanged()
        {
            if (!EditorApplication.isPlaying)
            {
                _hasPlayed = false;
            }

            if (isRestart)
            {
                EditorApplication.isPlaying = true;
                isRestart = false;
            }
        }
    }
}