//
// Weather System for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// *** A NOTE ABOUT PIRACY ***
// 
// If you got this asset from a pirate site, please consider buying it from the Unity asset store at https://assetstore.unity.com/packages/slug/60955?aid=1011lGnL. This asset is only legally available from the Unity Asset Store.
// 
// I'm a single indie dev supporting my family by spending hundreds and thousands of hours on this and other assets. It's very offensive, rude and just plain evil to steal when I (and many others) put so much hard work into the software.
// 
// Thank you.
//
// *** END NOTE ABOUT PIRACY ***
//

using UnityEngine;
using System.Collections;

namespace WeatherSystem
{
    /// <summary>
    /// Base script with type of profile
    /// </summary>
    /// <typeparam name="T">Type of cloud profile</typeparam>
    public abstract class WeatherSystemBaseScript<T> : MonoBehaviour where T: WeatherSystemBaseSettingScript , new()
    {
        /// <summary>Profile</summary>
        [Header("Profile and material")]
        [Tooltip("Profile")]
        public T Profile = new T();



        [Tooltip("Renderer")]
        public MeshRenderer MeshRenderer;


        private bool m_HasMeshRenderer;


        /// <summary>
        /// Awake
        /// </summary>
        protected virtual void Awake()
        {
            
        }

        /// <summary>
        /// Start
        /// </summary>
        protected virtual void Start()
        {
            m_HasMeshRenderer = MeshRenderer != null;
        }
        
        /// <summary>
        /// OnEnable
        /// </summary>
        protected virtual void OnEnable()
        {
            
        }
        
        /// <summary>
        /// OnDisable
        /// </summary>
        protected virtual void OnDisable()
        {
            
        }
        
        public virtual void UpdateMaterialFromProfile(WeatherSystemScript weatherSystemScript)
        {
            if (m_HasMeshRenderer && this.MeshRenderer.enabled != Profile.IsEnabled)
            {
                this.MeshRenderer.enabled = Profile.IsEnabled;
            }

            if (!Profile.IsEnabled)
            {
                return;
            }

            Profile.UpdateMaterialProperties(weatherSystemScript, this.MeshRenderer);
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        protected virtual void OnDestroy()
        {

        }
    }
}
