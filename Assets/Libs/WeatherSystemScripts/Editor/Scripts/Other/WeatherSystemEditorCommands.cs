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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.Rendering;


namespace WeatherSystem
{
  
    public static class WeatherSystemEditorCommands
    {
        /// <summary>
        /// Instantiate a prefab from an asset into the active scene
        /// </summary>
        /// <param name="assetName">Asset name</param>
        /// <param name="undoName">Undo operation name</param>
        /// <param name="identityPosition">Whether to set position and rotation to 0</param>
        /// <param name="setup">Callback for any additional setup that will be performed on the instantiated prefab</param>
        public static void InstantiatePrefab(string assetName, string undoName, bool identityPosition, System.Action<GameObject> setup = null)
        {
            string[] results = AssetDatabase.FindAssets(assetName);
            if (results.Length == 0)
            {
                Debug.LogError("Unable to find " + assetName + " in project");
                return;
            }
            GameObject prefab = null;
            foreach (string result in results)
            {
                string path = AssetDatabase.GUIDToAssetPath(result);
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    break;
                }
            }

            if (prefab == null)
            {
                Debug.LogError("Unable to deserialize prefab");
                return;
            }

#if UNITY_2018_3_OR_NEWER

            PrefabAssetType type = PrefabUtility.GetPrefabAssetType(prefab);
            if (type == PrefabAssetType.MissingAsset || type == PrefabAssetType.NotAPrefab)

#else

            PrefabType type = PrefabUtility.GetPrefabType(prefab);
            if (type == PrefabType.None)

#endif

            {
                Debug.LogError("Unable to load prefab, please reimport all assets");
                return;
            }
            var obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            obj.name = obj.name.Replace("(Clone)", string.Empty).Trim();
            obj.transform.parent = null;
            if (identityPosition)
            {
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = Quaternion.identity;
            }
            if (setup != null)
            {
                setup.Invoke(obj);
            }
            Undo.RegisterCreatedObjectUndo(obj, undoName);
        }

        [MenuItem("MY_Support/Weather System/Add Global Weather System to Scene", false, priority = 30)]
        public static void AddWeatherSystemPrefab()
        {
            var weatherSystemObjs = GameObject.FindObjectsOfType<WeatherSystemScript>();
            {
                foreach (var item in weatherSystemObjs)
                {
                    if (item.Global)
                    {
                        Debug.LogErrorFormat("Multiple global instances of {0} detected, this is not supported", item.name);
                        return;
                    }
                }
            }
            string name = "WeatherSystemPrefab";
            InstantiatePrefab(name, "Add Weather System", true, (obj) =>
            {                
                SyncLightDirectionLightWeatherSystemPrefab();
            });
        }

        [MenuItem("MY_Support/Weather System/Add Local Weather System to Scene", false, priority = 30)]
        public static void AddLocalWeatherSystemPrefab()
        {
            string name = "WeatherSystemPrefabLocal";
            InstantiatePrefab(name, "Add Weather System", true, (obj) =>
            {                
                SyncLightDirectionLightWeatherSystemPrefab();
            });
        }

        [MenuItem("MY_Support/Weather System/Reset SunDirection From DirectionLight", false, priority = 32)]
        public static void SyncLightDirectionLightDir()
        {
            //Camera
            var mainCamera = Camera.main;
            if (!mainCamera)
            {
                mainCamera = GameObject.FindObjectOfType<Camera>();
            }
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.clear;

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            //Direction
           var lightgo = GameObject.Find("Directional Light");
           var weatherSystemObjs = GameObject.FindObjectsOfType<WeatherSystemScript>();

            if (lightgo != null)
            {
                foreach (var item in weatherSystemObjs)
                {
                    Undo.RecordObject(item, "Changing the component on WeatherSystemScript");

                    item.DirectionalLight = lightgo.GetComponent<Light>();

                    PrefabUtility.RecordPrefabInstancePropertyModifications(item);

                    Undo.RecordObject(item, "Changing the component on WeatherSystemSkyScript");

                    var timeOfDay = (lightgo.transform.eulerAngles.x + 90.0f) * 24 / 360.0f;
                    Debug.Log($"Time of Day: {timeOfDay} Night: {24 - timeOfDay}");


                    float t = item.TimeOfDay * 360.0f / 24.0f - 90.0f;

                    if (item.CurrentProfile)
                    {                        
                        item.CurrentProfile.SkyProfile.SunDirection.slider = (lightgo.transform.eulerAngles.y + 180) % 360 - 180;
                        item.CurrentProfile.SkyProfile.SunXDirection.slider = (lightgo.transform.eulerAngles.x - t + 180) % 360 - 180;
                        EditorUtility.SetDirty(item.CurrentProfile);
                    }
                    item.SkyManager.skyScript.Sun.RotateYDegrees = 0;
                    item.SkyManager.skyScript.Sun.RotateXDegrees = 0;

                    PrefabUtility.RecordPrefabInstancePropertyModifications(item);
                }
                
            }
            else
            {
                Debug.LogErrorFormat("Object of type {0} is required, please ensure it is find in the Scene", typeof(Light));
            }
        }

        [MenuItem("MY_Support/Weather System/Reset Weather System", false, priority = 31)]
        public static void ResetWeatherSystem()
        {
            SyncLightDirectionLightWeatherSystemPrefab();
        }

        public static void SyncLightDirectionLightWeatherSystemPrefab()
        {
            //Camera
            var mainCamera = Camera.main;
            if (!mainCamera)
            {
                mainCamera = GameObject.FindObjectOfType<Camera>();
            }
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.clear;

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            //Direction
            var lightgo = GameObject.Find("Directional Light");
            if (lightgo == null)
            {
                lightgo = GameObject.Find("DL");
            }
            var weatherSystemObjs = GameObject.FindObjectsOfType<WeatherSystemScript>();
            if (lightgo != null)
            {
                foreach (var item in weatherSystemObjs)
                {
                    Undo.RecordObject(item, "Changing the component on WeatherSystemScript");

                    item.DirectionalLight = lightgo.GetComponent<Light>();
                    if (item.DirectionalLight == null)
                    {
                        for (int i = 0; i < lightgo.transform.childCount; i++)
                        {
                            GameObject c_dl = lightgo.transform.GetChild(i).gameObject;
                            if (c_dl.activeSelf)
                            {
                                item.DirectionalLight = c_dl.GetComponent<Light>();
                                break;
                            }
                        }
                    }
                    item.MainCamera = mainCamera;

                    PrefabUtility.RecordPrefabInstancePropertyModifications(item);

                    Undo.RecordObject(item, "Changing the component on WeatherSystemSkyScript");

                    var timeOfDay = (lightgo.transform.eulerAngles.x + 90.0f) * 24 / 360.0f;
                    Debug.Log($"Time of Day: {timeOfDay} Night: {24 - timeOfDay}");
                    
                    PrefabUtility.RecordPrefabInstancePropertyModifications(item);
                }
            }
            else
            {
                Debug.LogErrorFormat("Object of type {0} is required, please ensure it is find in the Scene", typeof(Light));
            }
        }

    }
}
