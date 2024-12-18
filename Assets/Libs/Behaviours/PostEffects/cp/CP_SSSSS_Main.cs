//次表面模拟
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace MyEffect
{
    /// <summary>
    /// 屏幕空间的次表面散射
    /// </summary>
    [RequireComponent(typeof(Camera))]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [ImageEffectOpaque]
    [ExecuteInEditMode]
    public class CP_SSSSS_Main : MonoBehaviour
    {
        public Shader shader;      
        public Shader copyDepthShader;       

        CommandBuffer buffer;
        CameraEvent camEvent = CameraEvent.BeforeImageEffectsOpaque;

        private Material m_Material;
        Material material
        {
            get
            {
                if(!shader ) shader = resource.ShaderManager.Find("Hidden/CPSSSSSShader");
                if (m_Material == null && shader )
                {
                    m_Material = new Material(shader);
                    m_Material.hideFlags = HideFlags.HideAndDontSave;
                }
                return m_Material;
            }
        }
        [Tooltip("缩小规模")]
        [Range(1, 3)]
        public int downscale = 1;
        [Tooltip("模糊迭代，数量与质量成正比，与性能成反比")]
        [Range(1, 3)]
        public int blurIterations = 1;
        [Tooltip("散射距离")]
        [Range(0.01f, 1.6f)]
        public float scatterDistance = 0.4f;
        [Tooltip("散射强度")]
        [Range(0f, 2f)]
        public float scatterIntensity = 1f;
        [Tooltip("深度差缓冲区")]
        [Range(0.001f, 0.3f)]
        public float softDepthBias = 0.05f;
        [Tooltip("直接叠加")]
        [Range(0f, 1f)]
        public float affectDirect = 0.5f;


        public int sss_obj_count = 0;
        void OnDisable()
        {
            ClearData();
        }

        public void ClearData()
        {
            if (m_Material)
            {
                DestroyImmediate(m_Material);
            }
            if (m_CopyDepthMaterial) DestroyImmediate(m_CopyDepthMaterial);           

            m_Material = null;
            m_CopyDepthMaterial = null;          

            CleanupBuffer();
        }

        void OnEnable()
        {
            if (!SystemInfo.supportsImageEffects)
            {
                enabled = false;
                return;
            }

            CleanupBuffer();

            if (material &&  !material.shader.isSupported)
            {
                enabled = false;
                return;
            }           

        }

        private void OnPreRender()
        {
            if (Graphics.activeTier != GraphicsTier.Tier3) return;
            if (meshRenderList.Count == 0) return;

            UnityEngine.Profiling.Profiler.BeginSample("sss pre render");
            if (buffer == null) ApplyBuffer();
            if (buffer != null) UpdateBuffer();
            UnityEngine.Profiling.Profiler.EndSample();
        }

        void OnPostRender()
        {
           
        }

        //List<GPUSkinningPlayerMono> playerList = new List<GPUSkinningPlayerMono>();
        List<Renderer> meshRenderList = new List<Renderer>();
        List<Material> materialList = new List<Material>();
        //ArrayDictionaryT<Renderer, Material> renderMatDic = new ArrayDictionaryT<Renderer, Material>();

#if UNITY_EDITOR
        public void AddRenders(List<Renderer> rl)
        {
            for (int m = 0; m < rl.Count; ++m)
            {
                var r = rl[m];
                if (!r || !r.gameObject || !r.gameObject.IsActive()) continue;
                if(true)
                {
                    if (r is SkinnedMeshRenderer || r is MeshRenderer)
                    {
                        if (r.sharedMaterial.HasMaterialSSSParameter())
                        {
                            if (!meshRenderList.Contains(r))
                                meshRenderList.Add(r);                            
                        }
                    }
                }
            }
        }
#endif


        public void UpdateSSSRenderList(List<ObjectBehaviourBase> renderList)
        {
            var meshRenderList = this.meshRenderList;
            //

            //playerList.Clear();           
            meshRenderList.Clear();

            //
            if (!this.enabled) return;

            UnityEngine.Profiling.Profiler.BeginSample("UpdateSSSRenderList");
            for (int n = 0, Count = renderList.Count; n < Count; ++n)
            {
                var render = renderList[n];
                if (!render) 
                {
                    continue;
                }

                var gameObject = render.gameObject;
                if (!gameObject || !gameObject.IsActive())
                {
                    continue;
                }

                var rl = render.CacheRendererList;
                if (rl != null)
                {
                    for (int m = 0, rl_Count = rl.Count; m < rl_Count; ++m)
                    {
                        var r = rl[m];
                        if (!r || !r.gameObject || !r.gameObject.IsActive()) continue;
                        if (true)
                        {
                            if (r is SkinnedMeshRenderer || r is MeshRenderer)
                            {
                                if (r.sharedMaterial.HasMaterialSSSParameter())
                                {
                                    if (!meshRenderList.Contains(r))
                                    {
                                        meshRenderList.Add(r);
                                    }
                                    //var material = new Material(resource.ShaderManager.Find("hc/charactor/CPSSSSSMask"));
                                    //material.name = $"{r.sharedMaterial.name}_sss";
                                    //material.mainTexture = r.sharedMaterial.GetTexture(GameObjectUtils.scatteringMapID);
                                    //material.mainTextureScale = r.sharedMaterial.GetTextureScale(GameObjectUtils.scatteringMapID);
                                    //material.mainTextureOffset = r.sharedMaterial.GetTextureOffset(GameObjectUtils.scatteringMapID);
                                    //material.SetColor(GameObjectUtils.scatteringColorID, r.sharedMaterial.GetColor(GameObjectUtils.scatteringColorID));
                                    //material.hideFlags = HideFlags.DontSave;
                                    //material.enableInstancing = true;

                                    //renderMatDic[r] = material;
                                }
                            }
                        }
                    }
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

        public void UpdateSSSRenderList(List<Renderer> rl)
        {
            //playerList.Clear();           
            meshRenderList.Clear();

            if (!this.enabled) return;
            for (int m = 0; m < rl.Count; ++m)
            {
                var r = rl[m];
                if (!r || !r.gameObject || !r.gameObject.IsActive()) continue;

                if (true )
                {
                    if (r is SkinnedMeshRenderer || r is MeshRenderer)
                    {
                        if (r.sharedMaterial.HasMaterialSSSParameter())
                        {
                            if (!meshRenderList.Contains(r))
                                meshRenderList.Add(r);
                            //var material = new Material(resource.ShaderManager.Find("hc/charactor/CPSSSSSMask"));
                            //material.name = $"{r.sharedMaterial.name}_sss";
                            //material.mainTexture = r.sharedMaterial.GetTexture(GameObjectUtils.scatteringMapID);
                            //material.mainTextureScale = r.sharedMaterial.GetTextureScale(GameObjectUtils.scatteringMapID);
                            //material.mainTextureOffset = r.sharedMaterial.GetTextureOffset(GameObjectUtils.scatteringMapID);
                            //material.SetColor(GameObjectUtils.scatteringColorID, r.sharedMaterial.GetColor(GameObjectUtils.scatteringColorID));
                            //material.hideFlags = HideFlags.DontSave;
                            //material.enableInstancing = true;

                            //renderMatDic[r] = material;
                        }
                    }
                }
            }
        }



        Material m_CopyDepthMaterial;
        Material CopyDepthMaterial
        {
            get
            {
                if(!copyDepthShader) copyDepthShader = resource.ShaderManager.Find("Hidden/CPSSSSSBlitDepthTextureToDepth");
                if (m_CopyDepthMaterial == null && copyDepthShader)
                {
                    m_CopyDepthMaterial = new Material(copyDepthShader)
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                }
                return m_CopyDepthMaterial;
            }
        }

        void Reset()
        {             
           
        }

        void ApplyBuffer()
        {
            if (buffer == null)
            {
                buffer = CommandBufferPool.Get("屏幕空间次表面散射");                
                GetComponent<Camera>().AddCommandBuffer(camEvent, buffer);
                GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
            }
        }

        void UpdateBuffer()
        {           
            buffer.Clear();
            sss_obj_count = 0;

            sss_obj_count = meshRenderList.Count;

            AddMakeSSSMaskCommands(buffer);

            int blurRT1 = resource.ShaderNameHash.ShaderNameId("_CPSSSSSBlur1");
            int blurRT2 = resource.ShaderNameHash.ShaderNameId("_CPSSSSSBlur2");
            int src = resource.ShaderNameHash.ShaderNameId("_CPSSSSSSource");
            int w = -1;
            int h = -1;
            Camera cam = Camera.current;
            if (cam != null)
            {
                w = cam.pixelWidth / downscale;
                h = cam.pixelHeight / downscale;
            }
           
            buffer.GetTemporaryRT(blurRT1, w, h, 16, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);           
            buffer.GetTemporaryRT(blurRT2, w, h, 16, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
            buffer.GetTemporaryRT(src, -1, -1, 24, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
            buffer.SetGlobalFloat(resource.ShaderNameHash.ShaderNameId("_SoftDepthBias"), softDepthBias * 0.05f * 0.2f);

            buffer.Blit(BuiltinRenderTextureType.CameraTarget, blurRT2);           
            buffer.Blit(BuiltinRenderTextureType.CameraTarget, src);

            //多次调用模糊算法
            for (int k = 1; k <= blurIterations; k++)
            {
                buffer.SetGlobalFloat(resource.ShaderNameHash.ShaderNameId("_BlurStr"), Mathf.Clamp01(scatterDistance * 0.08f - k * 0.022f * scatterDistance));
                buffer.SetGlobalVector(resource.ShaderNameHash.ShaderNameId("_BlurVec"), new Vector4(1, 0, 0, 0));
                buffer.Blit(blurRT2, blurRT1, material, 0);
                buffer.SetGlobalVector(resource.ShaderNameHash.ShaderNameId("_BlurVec"), new Vector4(0, 1, 0, 0));
                buffer.Blit(blurRT1, blurRT2, material, 0);

                buffer.SetGlobalVector(resource.ShaderNameHash.ShaderNameId("_BlurVec"), new Vector4(1, 1, 0, 0).normalized);
                buffer.Blit(blurRT2, blurRT1, material, 0);
                buffer.SetGlobalVector(resource.ShaderNameHash.ShaderNameId("_BlurVec"), new Vector4(-1, 1, 0, 0).normalized);
                buffer.Blit(blurRT1, blurRT2, material, 0);
            }

            //buffer.Blit(blurRT2, blurBuf);

            buffer.SetGlobalTexture(resource.ShaderNameHash.ShaderNameId("_BlurTex"), blurRT2);
            buffer.SetGlobalFloat(resource.ShaderNameHash.ShaderNameId("_EffectStr"), scatterIntensity);
            buffer.SetGlobalFloat(resource.ShaderNameHash.ShaderNameId("_PreserveOriginal"), 1 - affectDirect);
            buffer.Blit(src, BuiltinRenderTextureType.CameraTarget, material, 1);

            buffer.ReleaseTemporaryRT(blurRT1);
            buffer.ReleaseTemporaryRT(blurRT2);
            buffer.ReleaseTemporaryRT(src);
        }

        void CleanupBuffer()
        {
            //playerList.Clear();
            meshRenderList.Clear();
            foreach (var mat in materialList)
            {
                if (mat)
                    GameObject.Destroy(mat);
            }
            materialList.Clear();

            if (buffer != null)
            {
                buffer.Clear();
                GetComponent<Camera>().RemoveCommandBuffer(camEvent, buffer);
                CommandBufferPool.Release(buffer);
            }
        }
        int maskRT = -1;
        public void AddMakeSSSMaskCommands(CommandBuffer buffer)
        {
            if (meshRenderList.Count == 0) return;            

            if(maskRT <0)
                maskRT = resource.ShaderNameHash.ShaderNameId("_MaskTex");

            buffer.GetTemporaryRT(maskRT, -1, -1, 24, FilterMode.Bilinear, RenderTextureFormat.ARGB32);           
            buffer.Blit(BuiltinRenderTextureType.None, maskRT, CopyDepthMaterial);            
            buffer.SetRenderTarget(maskRT);

            for(int i = 0; i < meshRenderList.Count; ++i)
            {
                var r = meshRenderList[i];

                Material m = null;
                if(materialList.Count > i)
                {
                    m = materialList[i];
                }
                else
                {
                    m = new Material(resource.ShaderManager.Find("hc/charactor/CPSSSSSMask"));
                    materialList.Add(m);                    
                }

                m.mainTexture = r.sharedMaterial.GetTexture(GameObjectUtils.scatteringMapID);
                m.mainTextureScale = r.sharedMaterial.GetTextureScale(GameObjectUtils.scatteringMapID);
                m.mainTextureOffset = r.sharedMaterial.GetTextureOffset(GameObjectUtils.scatteringMapID);
                m.SetColor(GameObjectUtils.scatteringColorID, r.sharedMaterial.GetColor(GameObjectUtils.scatteringColorID));
                m.hideFlags = HideFlags.DontSave;
                m.enableInstancing = true;

                if (r && m)
                    buffer.DrawRenderer(r, m, 0);
            }

            buffer.SetGlobalTexture(resource.ShaderNameHash.ShaderNameId("_MaskTex"), maskRT);

           
        }
    }
}
