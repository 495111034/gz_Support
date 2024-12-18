using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
///  屏幕截图
/// </summary>
public class Screenshot
{
    public enum State { None, Used, Released };

    State _state = State.None;

    public State CurrentState
    {
        get
        {
            return _state;
        }
    }

    //
    public void Release()
    {
        if (_state == State.Used)
        {
            _state = State.Released;
            DecRef();
        }
    }

    public RenderTexture Texture
    {
        get
        {
            if (_state == State.None)
            {
                _state = State.Used;
                AddRef();
            }

            return _rt;
        }
    }



    #region 静态方法

    static RenderTexture _rt;
    static Camera _camera;
    static int _ref_count;


    // 获取截图
    static void AddRef()
    {
        if (_ref_count == 0)
        {
            if (_camera == null)
            {
                var go = new GameObject("Screenshot Camera");
                UnityEngine.Object.DontDestroyOnLoad(go);

                _camera = go.AddComponent<Camera>();  
                _camera.enabled = false;
            }

            var useRGBM = !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.DefaultHDR) || Application.isMobilePlatform || Graphics.activeTier < UnityEngine.Rendering.GraphicsTier.Tier3;

            var rtFormat = useRGBM ?
               RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR;

            _rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, rtFormat);
            _rt.name = "Screenshot Camera";         
            CameraUtils.CaptureScreenshot(_rt, _camera);
        }

        ++_ref_count;
    }

    static void DecRef()
    {
        --_ref_count;       

        if (_ref_count == 0)
        {
            if (_rt)
            {
                RenderTexture.ReleaseTemporary(_rt);
            }
           //if (_rt.IsCreated()) _rt.Release();
           //RenderTexture.Destroy(_rt);
           _rt = null;
        }
    }

#endregion

}
