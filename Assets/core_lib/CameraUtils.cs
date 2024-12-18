using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;



/// <summary>
/// 相机工具类
/// </summary>
public static class CameraUtils
{
    public const string CAMERA_WHOLE_BODY = "Camera Whole Body";
    public const string CAMERA_UPPER_BODY = "Camera Upper Body";

    public static float CAMERA_FOV;

    static Camera _mainCamera;
    public static Camera MainCamera {
        get
        {
            if (_mainCamera) return _mainCamera;
            return Camera.main;

        }
        set
        {
            _mainCamera = value;
        }
    }

    public static void SetMainCamera(Camera mainCamera)
    {
        MainCamera = mainCamera;
        MainCamera.cullingMask = (int)ObjLayerMask.ViewAll;
        DisableAudioListener();
    }


    // 截屏
    public static bool CaptureScreenshot(RenderTexture rt, Camera camera)
    {
        var main = MainCamera;
        //////if (!main)
        //////    main = App.MainCameraObject.GetComponent<Camera>();

        if (main == null)
        {
            Log.LogError("CaptureScreenshot, no main camera");
            return false;
        }
        if (camera == null)
        {
            camera = main;
        }
        else
        {
            camera.CopyFrom(main);
        }

        //
        var a = camera.cullingMask;
        var b = camera.targetTexture;

        //Render之前手动调一次Instancing物体的drawcall，否则drawcall在此之后
        //////var mydrawCalls = GameObject.FindObjectsOfType<SceneObjectInstance>();       
        //////for(int i = 0; i < mydrawCalls.Length; ++i)
        //////{
        //////if (mydrawCalls[i].CurrentNumber > 0)
        //////{
        //////mydrawCalls[i].DrawCall();
        //////}
        //////}

        camera.enabled = true;
        camera.cullingMask = (int)ObjLayerMask.NotRole;
        camera.targetTexture = rt;
        camera.Render();
        camera.enabled = false;
        camera.cullingMask = a;
        camera.targetTexture = b;       
        return true;
    }


    // 禁用 Camera
    public static void DisableCamera(GameObject go)
    {
        foreach (var c in go.GetComponentsInChildren<Camera>())
        {
            //c.enabled = false;
            //c.gameObject.SetActive(false);
            GameObject.Destroy(c);
        }
    }

    // 禁用主相机上的 AudioListener
    static void DisableAudioListener()
    {
        var camera = MainCamera;
        if (camera)
        {
            var listener = camera.GetComponent<AudioListener>();
            if (listener)
            {
                listener.enabled = false;
                if(Application.isEditor)  Log.LogInfo("DisableAudioListener");
            }
        }
    }

    // 获取 Look 相机, deepFind=是否深度查找
    public static Transform GetLookCamera(GameObject go, string name, bool deepFind)
    {
        if (deepFind)
        {
            var sub = MyUITools.FindChild(go, name,false,false);
            return sub != null ? sub.transform : null;
        }
        else
        {
            return go.transform.Find(name);
        }
    }

    // 把 src 对齐到 go 中一个叫 name 的相机位置, 返回是否OK
    public static bool Align2LookCamera(Transform src, GameObject go, string name, bool deepFind)
    {
        var ct = GetLookCamera(go, name, deepFind);
        if (ct != null)
        {
            src.position = ct.position;
            src.rotation = ct.rotation;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 在主相机上挂接子相机, 并保持它们的参数一致
    /// </summary>
    public static bool CheckSubCameraWithMain(ref Camera sub_camera, string name, CameraClearFlags clearFlags, int layer, int depth, RenderingPath path)
    {
        var main_camera = MainCamera;
        if (main_camera != null)
        {
            if (sub_camera == null)
            {
                var go = new GameObject(name);
                go.transform.SetParent(main_camera.transform, false);

                sub_camera = go.AddComponent<Camera>();
                sub_camera.clearFlags = clearFlags;
                sub_camera.cullingMask = 1 << layer;
                sub_camera.depth = depth;
                sub_camera.renderingPath = path;
            }
            sub_camera.fieldOfView = main_camera.fieldOfView;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 检测主相机的子相机
    /// </summary>
    public static bool CheckSubCameraWithMain(ref Camera sub_camera, string name, Action<Camera> initCamera)
    {
        var main_camera = MainCamera;
        if (main_camera != null)
        {
            if (sub_camera == null)
            {
                var go = new GameObject(name);
                go.transform.SetParent(main_camera.transform, false);

                sub_camera = go.AddComponent<Camera>();
                initCamera(sub_camera);
            }
            sub_camera.fieldOfView = main_camera.fieldOfView;
            return true;
        }
        return false;
    }

    // 查看对象
    public static void ViewObject(Camera camera, GameObject go)
    {
       // camera.fieldOfView = CAMERA_FOV;
        if (Align2LookCamera(camera.transform, go, CAMERA_WHOLE_BODY, true)) return;

        var b = GameObjectUtils.GetFirstColliderBounds(go);
        CameraLookAt(camera, b);
    }

    // 查看对象上半身
    public static void ViewUpHalfObject(Camera camera, GameObject go)
    {
       // camera.fieldOfView = CAMERA_FOV;
        if (Align2LookCamera(camera.transform, go, CAMERA_UPPER_BODY, true)) return;

        var b = GameObjectUtils.GetFirstColliderBounds(go);
        var size = b.size * 0.5f;
        var center = b.center;
        center.y += size.y / 2;             // 1/4 高度
        b = new Bounds(center, size);       // 高度增加 1/4
        CameraLookAt(camera, b);
    }

    // 相机对准目标, 让相机能够看见整个包围盒
    public static void CameraLookAt(Camera camera, Bounds b)
    {
        // 调整相机角度
       // camera.fieldOfView = CAMERA_FOV;
        camera.transform.rotation = Quaternion.Euler(CAMERA_FOV / 2, 180, 0);

        // 计算距离
        var height = b.size.y;
        var width = Mathf.Max(b.size.x, b.size.z);

        var dist1 = height / Mathf.Tan(CAMERA_FOV * Mathf.Deg2Rad); // 纵向距离
        var dist2 = width / 2 / Mathf.Tan(CAMERA_FOV / 2 * Mathf.Deg2Rad); // 横向距离
        var dist = Mathf.Max(dist1, dist2);
        dist += width / 2;

        // 设置相机位置
        camera.transform.position = b.center + camera.transform.forward * -dist;
    }

    // 获取剪裁距离
    public static float[] GetLayerDistances(float[] userSetting)
    {       
        if (_layer_distances == null)
        {
            _layer_distances = new float[32];

            // 默认值
            for (int i = 0; i < _layer_distances.Length; i++) _layer_distances[i] = 1000;

            

            // 特殊值               
            _layer_distances[(int)ObjLayer.TransParentFX] = 30;
            _layer_distances[(int)ObjLayer.IgnoreRaycast] = 30;
            _layer_distances[(int)ObjLayer.Water] = 30;
            _layer_distances[(int)ObjLayer.Player] = 30;
            _layer_distances[(int)ObjLayer.Scenario] = 40;
            _layer_distances[(int)ObjLayer.SceneBaseObj] = 100;
            _layer_distances[(int)ObjLayer.Monster] = 30;
            _layer_distances[(int)ObjLayer.Terrain] = 100;
            _layer_distances[(int)ObjLayer.RoleEffect] = 30;
            _layer_distances[(int)ObjLayer.SceneEffect] = 30;
            _layer_distances[(int)ObjLayer.NPC] = 40;
            _layer_distances[(int)ObjLayer.Pet] = 40;
            _layer_distances[(int)ObjLayer.Weapon] = 20;
            _layer_distances[(int)ObjLayer.Item] = 20;
            _layer_distances[(int)ObjLayer.SceneInstance] = 50;
            _layer_distances[(int)ObjLayer.FightEffect] = 50;
            _layer_distances[(int)ObjLayer.MainRole] = 100;            
        }
        float rate = Graphics.activeTier >= UnityEngine.Rendering.GraphicsTier.Tier3 ? 1 : 0;
        if (userSetting != null)
        {
            for(int i = 0; i < userSetting.Length; ++i)
            {
                if(userSetting[i] >= 0)
                    _layer_distances[i] = userSetting[i] * rate;
            }
        }       
        _layer_distances[(int)ObjLayer.BackGround] = 1000;


        return _layer_distances;
    }
    static float[] _layer_distances = null;

    // 设置相机距离剪裁
    public static void SetLayerCullDistances(Camera camera, float[] userConfig )
    {        
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 1000;        
        camera.layerCullSpherical = true;
        camera.layerCullDistances = GetLayerDistances(userConfig);
    }

    //
    public static Vector2 World2ScreenPos(Vector3 wpos)
    {
        var camera = Camera.main;
        if (camera == null) return Vector2.zero;

        var spos = camera.WorldToScreenPoint(wpos);
        spos.y = Screen.height - spos.y;

        return new Vector2(spos.x, spos.y);
    }

    // 世界坐标 -> rc 中的坐标, 要求 rt 对齐到父窗口的中心点
    public static Vector2 World2RectPos(Vector3 wpos, Rect rc)
    {
        var camera = MainCamera;
        if (camera == null) return Vector2.zero;

        // vpos, 屏幕中心为 (0,0)
        var vpos = camera.WorldToViewportPoint(wpos);

        var x = rc.xMin + rc.width * vpos.x;
        var y = rc.yMin + rc.height * vpos.y;

        return new Vector2(x, y);
    }
}

