using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSupport
{
    [ExecuteInEditMode]

    public class WindSetting : MonoBehaviour
    {
        
        public Vector4 windDirection = Vector3.zero;

        public float windSpeed;

        [Range (0,20)]
        public float windStrength;

        private int PID_WindWorldDir = Shader.PropertyToID("_WindWorldDir");
        private int PID_WindSpeed = Shader.PropertyToID("_WindSpeed");
        private int PID_WindStrength = Shader.PropertyToID("_WindStrength");
        
        void Start()
        {            
        }

        void Update()
        {
            Shader.SetGlobalVector(PID_WindWorldDir, windDirection);
            Shader.SetGlobalFloat(PID_WindSpeed, windSpeed);
            Shader.SetGlobalFloat(PID_WindStrength, windStrength);
            
        }

    }

}