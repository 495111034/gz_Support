
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSupport.EffectToolBox
{
    /// <summary>
    /// 主要用于计算overdraw像素
    /// </summary>
    public class EffectEvla
    {
        public Camera _camera;
        SingleEffectEvla singleEffectEvla;
        public float time = 0;
        //采集特效数据的区域大小
        int rtSizeWidth = 512;
        int rtSizeHeight = 512;
        bool _overDrawView = false;
       
        public EffectEvla(Camera camera)
        {
            SetCamera(camera);
            //rtSizeWidth = camera.pixelWidth;
            //rtSizeHeight = camera.pixelHeight;           
            singleEffectEvla = new SingleEffectEvla(1);
        }

        public void SetCamera(Camera camera)
        {            
            _camera = camera;          
        }

        public void SetOverDrawView(bool overDrawView)
        {
            _overDrawView = overDrawView;
        }

        public void Destroy()
        {
            if(_camera)
                _camera.ResetReplacementShader();
        }

        public void Reset()
        {
            singleEffectEvla.Reset();
        }

        public void Update()
        {
            time += Time.deltaTime;            
            RecordOverDrawData(singleEffectEvla);
        }

        public EffectEvlaData[] GetEffectEvlaData()
        {
            return singleEffectEvla.GetEffectEvlaDatas();
        }

        #region overdraw
        public void RecordOverDrawData(SingleEffectEvla singleEffectEvla)
        {
            long pixTotal = 0;
            long pixActualDraw = 0;

            GetCameraOverDrawData(out pixTotal, out pixActualDraw);

            // 往数据+1
            singleEffectEvla.UpdateOneData(pixTotal, pixActualDraw);
        }

        public void GetCameraOverDrawData(out long pixTotal, out long pixActualDraw)
        {

            //记录当前激活的渲染纹理
            RenderTexture activeTextrue = RenderTexture.active;
            //渲染指定范围的rt，并记录范围内所有rgb像素值    
            _camera.SetReplacementShader(Shader.Find("EffectToolBox/OverDraw"), "");      
            
            RenderTexture rt = RenderTexture.GetTemporary(rtSizeWidth, rtSizeHeight, 32, RenderTextureFormat.ARGB32);
            rt.filterMode = FilterMode.Point;
            _camera.targetTexture = rt;
            _camera.Render();
            RenderTexture.active = rt;
            Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
     
            GetOverDrawData(texture, out pixTotal, out pixActualDraw);

            //恢复之前激活的渲染纹理
            RenderTexture.active = activeTextrue;
            Texture2D.DestroyImmediate(texture);
            RenderTexture.ReleaseTemporary(rt);
            if(!_overDrawView)
                _camera.ResetReplacementShader();

            _camera.targetTexture = null;
        }

        public void GetOverDrawData(Texture2D texture, out long pixTotal, out long pixActualDraw)
        {
            var texw = texture.width;
            var texh = texture.height;

            var pixels = texture.GetPixels();
            pixTotal = pixels.Length;

            int index = 0;

            //pixTotal = 0;
            pixActualDraw = 0;

            for (var y = 0; y < texh; y++)
            {
                for (var x = 0; x < texw; x++)
                {
                    float r = Mathf.GammaToLinearSpace(pixels[index].r);
                    float g = Mathf.GammaToLinearSpace(pixels[index].g);
                    float b = Mathf.GammaToLinearSpace(pixels[index].b);
                                      
                    //bool isEmptyPix = IsEmptyPix(r, g, b);
                    //if (isEmptyPix)
                    //{
                    //    continue;
                    //}

                    int drawThisPixTimes = DrawPixTimes(r, g, b);
                    pixActualDraw += drawThisPixTimes;

                    index++;
                }
            }
        }


        //计算单像素的绘制次数
        public int DrawPixTimes(float r, float g, float b)
        {
            //在OverDraw.Shader中像素每渲染一次，g值就会叠加0.04
            return Convert.ToInt32(g / 0.04);
        }

        public bool IsEmptyPix(float r, float g, float b)
        {
            return r == 0 && g == 0 && b == 0;
        }
        #endregion
    }
}
