using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SceneVar : MonoBehaviour
{
    public bool isUseWater;

    [ColorUsageAttribute(true, true)]
    public Color ShadowColor = Color.white;
    public float ShadowDistance = 100;

    public int farClipPlane;

    int shadowColorID = -1;
    int shadowDistanceID = -1;


    public SceneVar() 
    {
        //Debug.Log($"SceneVar .ctor");
    }

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log($"SceneVar Start");
        shadowColorID = Shader.PropertyToID("_ShadowColor");
        shadowDistanceID = Shader.PropertyToID("_ShadowDistance");

        var camera = Camera.main;
        if (!camera)
        {
            //camera = CameraManager.MainCamera;
            var go = GameObject.Find("Main Camera");
            if (go) 
            {
                camera = go.GetComponent<Camera>();
            }
            if (!camera) 
            {
                camera = GameObject.FindObjectOfType<Camera>(true);
            }
        }
        
        if(camera)
        {
#if UNITY_EDITOR //美术工程预览用
            if (!Application.isPlaying)
            {
                int _UseDepthTex = Shader.PropertyToID("_UseDepthTex");
                Shader.SetGlobalFloat(_UseDepthTex, isUseWater ? 1 : 0);
            }
#endif
            camera.depthTextureMode = isUseWater ? DepthTextureMode.Depth : DepthTextureMode.None;
            if (farClipPlane > 0) 
            {
                camera.farClipPlane = farClipPlane;
            }
        }
        else
        {
            Log.LogError("找不到可用摄像机");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalColor(shadowColorID, ShadowColor);
        Shader.SetGlobalFloat(shadowDistanceID, ShadowDistance);
    }

    private void OnEnable()
    {
        //Debug.Log($"SceneVar OnEnable");
    }

    private void OnDisable()
    {
        //Debug.Log($"SceneVar OnDisable");
    }

}
