using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace GameSupport
{
    [ExecuteInEditMode]
    public class ShaderGlobalVars : MonoBehaviour
    {

        //由于场景的后期亮度不同，因为有些场景的后期会把人脸变爆
        public float scenePostEffectAdjustFactor = 1.0f;


        //防止人物死黑或者爆亮
        [Range(-1,1)]
        public float diffuseDarkLimitGlobal = 0f;
        [Range(-1,1)]
        public float occProbeLimitGlobal = 0f;
        [Range(-2f, 2f)]
        public float lightProbeLimitGlobal = 0f;

        [ColorUsage(true, true)]
        public Color weatherChaColor = new Color(1, 1, 1, 1);

        private void Awake()
        {
            diffuseDarkLimitGlobal = Shader.GetGlobalFloat("_DiffuseDarkLimitGlobal");
            occProbeLimitGlobal = Shader.GetGlobalFloat("_OccProbeLimitGlobal");
            lightProbeLimitGlobal = Shader.GetGlobalFloat("_LightProbeLimitGlobal");
        }

        void OnEnable()
        {
            UpdateMaterialProperties();
        }

        void OnDestroy()
        {
            Shader.SetGlobalFloat("_ScenePostEffectAdjustFactor", 1.0f);
        }


        void UpdateMaterialProperties()
        {
            Shader.SetGlobalFloat("_ScenePostEffectAdjustFactor", scenePostEffectAdjustFactor);

            Shader.SetGlobalFloat("_DiffuseDarkLimitGlobal", diffuseDarkLimitGlobal);
            Shader.SetGlobalFloat("_OccProbeLimitGlobal", occProbeLimitGlobal);
            Shader.SetGlobalFloat("_LightProbeLimitGlobal", lightProbeLimitGlobal);
            Shader.SetGlobalColor("_WeatherChaColor", weatherChaColor);
        }

        private void OnValidate()
        {
            UpdateMaterialProperties();
        }
    }
}