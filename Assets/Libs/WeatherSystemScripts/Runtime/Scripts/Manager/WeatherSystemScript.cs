//
// Weather Maker for Unity
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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

// #define COPY_FULL_DEPTH_TEXTURE

namespace WeatherSystem
{
    /// <summary>
    /// Weather Maker master script
    /// </summary>
    [ExecuteInEditMode]
    [DefaultExecutionOrder(50)]
    public class WeatherSystemScript : MonoBehaviour, IWeatherSystemProvider
    {
        /// <summary>Whether the prefab should exist forever. Set to false to have the prefab destroyed with the scene it is in.</summary>
        //[Header("Setup")]

        
               
        /// <summary>Executes when the weather profile changes for the local player. Parameter is WeatherSystemProfileScript.</summary>
        [Header("Events")]
        [Tooltip("Executes when the weather profile changes for the local player. Parameter is WeatherSystemProfileScript.")]
        public WeatherSystemEvent WeatherProfileChanged;
               
        //Not included in the build
        public bool ShowReferencesHeaderGroup = true;
        public bool ShowReferencesHeaderGroup_TerrainData = true;
        public bool ShowProfilesHeaderGroup = false;
        public bool ShowEventsHeaderGroup = false;
        public bool ShowOptionsHeaderGroup = false;
        public bool ShowOutputsHeaderGroup = false;

        // References
        public Light DirectionalLight;
        [Tooltip("A single weather camera.")]
        public Camera MainCamera = null;

        // Sky settings
        public float TimeOfDay = 8;

        public bool Global = true;

        // Profiles
        [NonSerialized]
        public WeatherSystemProfileScript DefaultProfile = null;
        [NonSerialized]
        public WeatherSystemProfileScript CurrentProfile = null;
        [NonSerialized]
        public WeatherSystemProfileScript TargetProfile = null;
        // Lists
        public List<WeatherSystemProfileScript> DefaultProfileList = new List<WeatherSystemProfileScript>();
        public List<WeatherSystemGlobalProfileConfigScript> GlobalWeatherList = new List<WeatherSystemGlobalProfileConfigScript>();

        // Global weather transition
        public float GlobalWeatherTransitionProgress = 0.0f;
        public float GlobalWeatherTransitionTime = 0.0f;
        public float GlobalWeatherStartTransitionTime = 0.0f;

        public bool IsGlobalWeatherChanging = false;

        private float m_weatherZoneClosestDistanceSqr;
        private float m_weatherZoneDistance;
        private float m_weatherZoneBlendDistanceSqr;
        private float m_weatherZoneInterpolationFactor;
        private Collider m_weatherZoneCollider;

        public static List<WeatherSystemScript> WeatherSystemScriptRunningInstances = new List<WeatherSystemScript>(10);

        public static void UpdateWeahterSystemRunningInstances(Camera camera)
        {
            foreach (var item in WeatherSystemScriptRunningInstances)
            {
                if (item.MainCamera == camera)
                {
                    item.UpdateProfiles();
                    //single camera for weather
                    break;
                }
            }
        }

        public static void UpdateWeahterSystemRunningInstances()
        {
            foreach (var item in WeatherSystemScriptRunningInstances)
            {
                item.UpdateProfiles();
            }
            
        }

       


        private void UpdateProfiles()
        {
            if (!IsGlobalWeatherChanging)
            {
                // Gets the current sky setting when there is no global weather transition or local weather zone influence
                ApplyWeatherDefaultSettings(CurrentProfile);
            }
            else
            {
                // Runs the global weather transition
                GlobalWeatherTransitionProgress = Mathf.Clamp01((Time.time - GlobalWeatherStartTransitionTime) / GlobalWeatherTransitionTime);

                // Performs the global weather blend
                ApplyGlobalWeatherTransition(CurrentProfile, TargetProfile, GlobalWeatherTransitionProgress);

                // Ends the global weather transition
                if (Math.Abs(GlobalWeatherTransitionProgress - 1.0f) <= 0.0f)
                {
                    IsGlobalWeatherChanging = false;
                    GlobalWeatherTransitionProgress = 0.0f;
                    GlobalWeatherStartTransitionTime = 0.0f;
                    CurrentProfile = TargetProfile;
                }
            }
        }

        /// <summary>
        /// Blends the profiles when there is a global weather transition.
        /// </summary>
        private void ApplyGlobalWeatherTransition(WeatherSystemProfileScript from, WeatherSystemProfileScript to, float t)
        {
            if (from == null || to == null)
            {
                return;
            }

            WeatherSystemProfileScript.ApplyGlobalWeatherTransition(this, from, to, TimeOfDay, t);
        }

        private void ApplyWeatherDefaultSettings(WeatherSystemProfileScript profile)
        {
            if (profile == null)
            {
                return;
            }
            WeatherSystemProfileScript.ApplyWeatherDefaultSettings(this, profile, TimeOfDay);
        }

        /// <summary>
        /// Computes local weather zones influence.
        /// </summary>
        private void ApplyWeatherZonesInfluence( WeatherSystemProfileScript climateZoneProfile, float t)
        {
            WeatherSystemProfileScript.ApplyWeatherZonesInfluence(this, climateZoneProfile, TimeOfDay, t);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="transitionTime"></param>
        public void SetNewWeatherProfile(WeatherSystemProfileScript profile, float transitionTime = 5)
        {
            if (!profile)
            {
                Debug.LogError("SetNewWeatherProfile profile is Null ");
            }

            if (!Application.isPlaying || transitionTime < 0.000001f || CurrentProfile == null)
            {
                CurrentProfile = profile;

                GlobalWeatherTransitionProgress = 0.0f;
                GlobalWeatherStartTransitionTime = Time.time;
                GlobalWeatherTransitionTime = 0.0f;
                IsGlobalWeatherChanging = false;
                UpdateProfiles();
                return;
            }


            {
                TargetProfile = profile;

                // Starts the global weather transition progress
                GlobalWeatherTransitionProgress = 0.0f;
                GlobalWeatherStartTransitionTime = Time.time;
                GlobalWeatherTransitionTime = transitionTime;
                IsGlobalWeatherChanging = true;
            }           
        }

        /// <summary>
        /// Changes the global weather with a smooth transition.
        /// </summary>
        /// Set the index to -1 if you want to reset the global weather back to the default day profile.
        /// <param name="index">The target profile number in the "global weather profiles" list.</param>
        public void SetNewWeatherProfile(int index)
        {

        }

        private void Start()
        {
            if (Global)
            {
                WeatherSystemScript.EnsureInstance(this, ref instance);
            }

            if (DefaultProfileList.Count == 0)
                return;

            DefaultProfile = DefaultProfileList[0];
            CurrentProfile = DefaultProfile;
            TargetProfile = DefaultProfile;


            // First update of the shader uniforms
            UpdateProfiles();           
        }
        
          private void LateUpdate()
          {  
              
              //if (Application.isPlaying)
              {
                  CheckForMainCamera();
                  UpdateProfiles();
              }
          }
        

        public void UpdateInEditor()
        {
            if (!Application.isPlaying)
            {
                CheckForMainCamera();
                UpdateProfiles();
            }
        }


        private void CheckForMainCamera()
        {
            if ((mainCameraCheck += Time.deltaTime) < 0.5f)
            {
                return;
            }

            mainCameraCheck = 0.0f;
            if (MainCamera == null)
            {
                var mainCamera = Camera.main ?? GameObject.FindObjectOfType<Camera>();
                if (mainCamera != null)
                {
                    MainCamera = mainCamera;
                    
                    return;
                }
            }

            if (MainCamera == null)
            {
                Debug.LogError("Weather Maker allow cameras list is empty, please ensure your camera(s) are added to the wCameras list of WeatherSystemScript");
            }
        }


        /// <summary>
        /// Cloud manager
        /// </summary>
        public ICloudManager CloudManager { get; set; }

        /// <summary>
        /// Sky manager
        /// </summary>
        public ISkyManager SkyManager { get; set; }



        /// <summary>
        /// Whether we have had a weather transition, if not first transition is instant
        /// </summary>
        public bool HasHadWeatherTransition { get; set; }

        private float mainCameraCheck = 1.0f;
            
        
        private void OnEnable()
        {
            WeatherSystemScriptRunningInstances.Add(this);
            

            CloudManager = FindIfNull<ICloudManager, WeatherSystemCloudManagerScript>(CloudManager);
            CloudManager.weatherSystemScript = this;
            

            SkyManager = FindIfNull<ISkyManager, WeatherSystemSkyManagerScript>(SkyManager);
            SkyManager.weatherSystemScript = this;
        }

        private void OnDisable()
        {
            WeatherSystemScriptRunningInstances.Remove(this);
        }

        private void OnDestroy()
        {
            WeatherSystemScriptRunningInstances.Remove(this);
            if (Global)
            {
                WeatherSystemScript.ReleaseInstance(ref instance);
            }
        }

        private TInterface FindIfNull<TInterface, T>(TInterface value) where TInterface : class where T : UnityEngine.Component, TInterface
        {
            if (value == null)
            {
                value = gameObject.GetComponentInChildren<T>(true);
            }
            return value as TInterface;
        }

         /// <summary>
         /// Ensures that an object is the correct singleton for itself
         /// </summary>
         /// <typeparam name="T">Type of object</typeparam>
         /// <param name="obj">Object</param>
         /// <param name="instance">Singleton reference</param>
         public static void EnsureInstance<T>(MonoBehaviour obj, ref T instance) where T : MonoBehaviour
         {
             if (instance == null)
             {
                 instance = obj as T;

                 if (instance == null)
                 {
                     Debug.LogError("Incorrect object type passed to EnsureInstance, must be of type T");
                 }
             }
            else if (instance != obj)
            {
                Debug.LogErrorFormat("Multiple global instances of {0} detected, this is not supported", typeof(T).FullName);
            }
        }

         /// <summary>
         /// Release an instance created with FindOrCreateInstance
         /// </summary>
         /// <typeparam name="T">Type of object</typeparam>
         /// <param name="instance">Instance to release</param>
         public static void ReleaseInstance<T>(ref T instance) where T : MonoBehaviour
         {
             instance = null;
         }

        
         private static WeatherSystemScript instance;
         /// <summary>
         /// Shared instance of weather maker manager script
         /// </summary>
         public static WeatherSystemScript Instance
         {
             get { return instance; }
         }
         
    }

    /// <summary>
    /// Interface for all weather maker managers
    /// </summary>
    public class IWeatherSystemManager : MonoBehaviour
    {
        public WeatherSystemScript weatherSystemScript = null;

        public virtual void ApplyWeatherDefaultSettings(WeatherSystemProfileScript profile, float timeOfDay) { }

        public virtual void ApplyGlobalWeatherTransition( WeatherSystemProfileScript from, WeatherSystemProfileScript to, float timeOfDay, float t) { }

        public virtual void ApplyWeatherZonesInfluence( WeatherSystemProfileScript climateZoneProfile, float timeOfDay, float t) { }
    }

    /// <summary>
    /// WeatherSystem event
    /// </summary>
    [System.Serializable]
    public class WeatherSystemEvent : UnityEngine.Events.UnityEvent<object> { }


}
