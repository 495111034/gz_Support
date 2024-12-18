using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace GameSupport
{
    [ExecuteInEditMode]
    public class SceneGlobalVars : MonoBehaviour
    {
         
        public bool LIGHTMAP_MERGEMASK = false;
        
        
        private void Awake()
        {
            

        }

        void OnEnable()
        {
            Shader.SetGlobalFloat("_Lightmap_MergeMaskGlobal", LIGHTMAP_MERGEMASK ? 1.0f : 0.0f);
        }
        private void OnDisable()
        {
            Shader.SetGlobalFloat("_Lightmap_MergeMaskGlobal", 0.0f);
        }

        private void OnValidate()
        {
            Shader.SetGlobalFloat("_Lightmap_MergeMaskGlobal", LIGHTMAP_MERGEMASK ? 1.0f : 0.0f);
        }

        void OnDestroy()
        {
            Shader.SetGlobalFloat("_Lightmap_MergeMaskGlobal", 0.0f);
        }

    }
}