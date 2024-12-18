using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.UI
{
    public class ProjectorShadowUI : MonoBehaviour
    {
        public float mProjectorSize = 3f;
        public int mRenderTexSize = 512;       

        public GameObject rootGameobject;

        private bool mInit = false;
        private Projector mProjector;
        private Camera mShadowCam;
        private RenderTexture mShadowRT;
        private CommandBuffer mCommandBuf;
        private Material mReplaceMat;


        #region MonoBehaviour
        void Start()
        {

        }


        void Update()
        {
            if (!rootGameobject) return;
            if (!mInit) return;

            FillCommandBuffer();
        }

        private void OnDisable()
        {
            clearData();
        }

        private void OnDestroy()
        {
            clearData();
        }

        #endregion


        #region 方法

        public void Init()
        {
            if (mInit) return;

            gameObject.SetLayerRecursively((int)ObjLayer.Shadow);

            if (mShadowRT)
            {
                RenderTexture.ReleaseTemporary(mShadowRT);
                mShadowRT = null;
            }
            if (!mShadowRT)
            {
                mShadowRT = RenderTexture.GetTemporary(mRenderTexSize, mRenderTexSize, 0, RenderTextureFormat.R8);
                mShadowRT.name = "ShadowRT_UI";
                mShadowRT.antiAliasing = 1;
                mShadowRT.filterMode = FilterMode.Bilinear;
                mShadowRT.wrapMode = TextureWrapMode.Clamp;
            }

            if (!mProjector)
            {
                mProjector = gameObject.AddComponent<Projector>();
            }
            mProjector.orthographic = true;
            mProjector.orthographicSize = mProjectorSize;
            mProjector.ignoreLayers = ~((int)ObjLayerMask.ReceiveShadow);
            mProjector.material = Resources.Load<Material>("small/ProjectorShadow");
            mProjector.material.SetTexture("_ShadowTex", mShadowRT);
            //mProjector.material.SetTexture("_FalloffTex", ProjectorShadow.ShadowTexture);
            mProjector.nearClipPlane = -10;
            mProjector.farClipPlane = 10;

            if (!mShadowCam)
            {
                mShadowCam = gameObject.AddComponent<Camera>();
                mShadowCam.clearFlags = CameraClearFlags.Color;
                mShadowCam.backgroundColor = Color.black;
                mShadowCam.orthographic = true;
                mShadowCam.orthographicSize = mProjectorSize;
                mShadowCam.depth = -101.0f;
                mShadowCam.nearClipPlane = mProjector.nearClipPlane;
                mShadowCam.farClipPlane = mProjector.farClipPlane;
                mShadowCam.allowHDR = false;
                mShadowCam.allowMSAA = false;
                mShadowCam.allowDynamicResolution = false;
                mShadowCam.useOcclusionCulling = false;
                mShadowCam.cullingMask = 0;
                mShadowCam.RemoveAllCommandBuffers();
            }
            mShadowCam.targetTexture = mShadowRT;
            mShadowCam.enabled = true;
            

            if (mCommandBuf == null)
            {
                mCommandBuf = CommandBufferPool.Get("ProjectorShaderUI_CommandBuffer");
                mShadowCam.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, mCommandBuf);
            }

           
            

            if (!mReplaceMat)
            {
                Shader replaceshader = resource.ShaderManager.Find("ProjectorShadow/ShadowCaster");
                mReplaceMat = new Material(replaceshader);
                mReplaceMat.hideFlags = HideFlags.HideAndDontSave;
                mReplaceMat.enableInstancing = true;
            }

            mInit = true;
        }

        private void FillCommandBuffer()
        {
            mCommandBuf.Clear();

            var renderList = MyListPool<Renderer>.Get();
            rootGameobject.GetComponentsEx(renderList);
            for (int i = 0; i < renderList.Count; ++i)
            {
                if (renderList[i].gameObject.layer == (int)ObjLayer.RoleEffect) continue;
                {
                    var go = renderList[i].gameObject;
                    if (((int)ObjLayerMask.CasterShadow & (1 << go.layer)) != 0)
                    {
                        mCommandBuf.DrawRenderer(renderList[i], mReplaceMat);
                    }
                }
            }
            MyListPool<Renderer>.Release(renderList);
        }
        public void clearData()
        {
            if (mShadowCam)
            {
                mShadowCam.RemoveAllCommandBuffers();
                mShadowCam.targetTexture = null;
                mShadowCam.enabled = false;
            }

            //if (mProjector && mProjector.material)
            //    mProjector.material.SetTexture("_ShadowTex", null);

            if (mShadowRT)
            {
                if (mProjector && mProjector.material && mProjector.material.GetTexture("_ShadowTex") == mShadowRT)
                    mProjector.material.SetTexture("_ShadowTex", null);

                RenderTexture.ReleaseTemporary(mShadowRT);
            }
            mShadowRT = null;

            if (mCommandBuf != null)
            {
                CommandBufferPool.Release(mCommandBuf);
            }
            mCommandBuf = null;

            if(mReplaceMat)
            {
                GameObject.Destroy(mReplaceMat);
            }
            mReplaceMat = null;

            mInit = false;
        }
        #endregion
    }
}
