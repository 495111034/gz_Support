using UnityEngine;
using System.Collections;
using resource;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    /// <summary>
    /// 高斯糊糊,将图片模糊，只在初始化时计算一次，只可用于UI
    /// </summary>
    public class GaussianBlur : MonoBehaviour
    {

        public Shader gaussianBlurShader;
        private Material gaussianBlurMaterial = null;

        [Range(0, 4)]
        public int iterations = 3;

        [Range(0.2f, 3.0f)]
        public float blurSpread = 0.6f;

        [Range(1, 8)]
        public int downSample = 2;

        [Range(0,1)]
        public float _light = 1;


        Texture imgTexture;
        bool hasDoBlur = false;

        RenderTexture buffer0 = null;

        public void DoReset()
        {
            if (buffer0)
                RenderTexture.ReleaseTemporary(buffer0);
            buffer0 = null;
            if (gaussianBlurMaterial)
                Destroy(gaussianBlurMaterial);
            gaussianBlurMaterial = null;
            hasDoBlur = false;
            if (GetComponent<MySpriteImage>() && imgTexture)
                GetComponent<MySpriteImage>().SetTexture(imgTexture,null);

            //if (imgTexture)
            //    RenderTexture.ReleaseTemporary(imgTexture);
            //imgTexture = null;
            //Log.LogInfo("GaussianBlur:DoReset");
        }

        private void OnDisable()
        {
            DoReset();
        }

        void OnDestroy()
        {
            DoReset();
        }
        void Update()
        {
            if (hasDoBlur || !GetComponent<MySpriteImage>())
            {
                // Log.LogInfo("not to blur");
                return;
            }

            if (!imgTexture)
                imgTexture = GetComponent<MySpriteImage>().mainTexture;

            if (imgTexture)
            {
                try
                {
                    if (gaussianBlurMaterial == null)
                    {
                        if (gaussianBlurShader == null)
                            gaussianBlurShader = ShaderManager.Find("MyShaders/others/Gaussian Blur");

                        if (gaussianBlurShader != null)
                        {
                            gaussianBlurMaterial = new Material(gaussianBlurShader);
                            gaussianBlurMaterial.hideFlags = HideFlags.DontSave;
                        }
                    }

                    if (gaussianBlurMaterial == null) return;

                    doBlur();
                }
                catch (System.Exception e)
                {
                    Log.LogError($"GaussianBlur error:{e.Message}");
                    hasDoBlur = true;
                }

            }
            else
            {
               // Log.LogError("cannt doBlur:imgTexture is null");
            }
        }

        void doBlur()
        {
            //  RenderTexture dest = new RenderTexture(imgTexture);
            if (gaussianBlurMaterial != null)
            {
                int rtW = imgTexture.width / downSample;
                int rtH = imgTexture.height / downSample;

                if (buffer0)
                    RenderTexture.ReleaseTemporary(buffer0);

                buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
                buffer0.filterMode = FilterMode.Bilinear;
                buffer0.name = "blur texture";

                Graphics.Blit(imgTexture, buffer0);

                for (int i = 0; i < iterations; i++)
                {
                    gaussianBlurMaterial.SetFloat("_BlurSize", 1.0f + i * blurSpread);
                    gaussianBlurMaterial.SetFloat("_light", _light);

                    RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

                    // 调用pass0
                    Graphics.Blit(buffer0, buffer1, gaussianBlurMaterial, 0);

                    RenderTexture.ReleaseTemporary(buffer0);
                    buffer0 = buffer1;
                    buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

                    // 调用pass1
                    Graphics.Blit(buffer0, buffer1, gaussianBlurMaterial, 1);

                    RenderTexture.ReleaseTemporary(buffer0);
                    buffer0 = buffer1;
                }

                GetComponent<MySpriteImage>().SetTexture(buffer0,null);
                //Graphics.Blit(buffer0, imgTexture);
                // RenderTexture.ReleaseTemporary(buffer0);

               // Log.LogInfo("doBlur OK......");

                //if (imgTexture && imgTexture is RenderTexture)
                //    RenderTexture.ReleaseTemporary(imgTexture as RenderTexture);

                hasDoBlur = true;
            }
            else
            {
                Log.LogError("cannt doBlur:gaussianBlurMaterial is null");
                //Graphics.Blit(imgTexture, imgTexture);
            }





        }

        void Start()
        {

            imgTexture = GetComponent<MySpriteImage>().mainTexture;


        }
    }
}
