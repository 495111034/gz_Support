using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.PostProcessing
{
    [Serializable]
    [PostProcess(typeof(CustomBloomRenderer), "Unity/CustomBloom")]
    public sealed class CustomBloom : PostProcessEffectSettings
    {
        [Min(0f) , Tooltip("Filters out pixels under this level of brightness. Value is in gamma-space.")]
        public FloatParameter threshold = new FloatParameter { value = 0.9f };

        [Min(0f) , Tooltip("Strength of the bloom filter.")]
        public FloatParameter intensity = new FloatParameter { value = 0.9f };

        [Min(0f) , Tooltip("Clamps pixels to control the bloom amount.")]
        public FloatParameter clamp = new FloatParameter { value = 65472f };

        [Tooltip("Global tint of the bloom filter.")]
        public ColorParameter tint = new ColorParameter { value = Color.white };

        [Tooltip("Clamps pixels to control the bloom amount.")]
        public Vector4Parameter blurComposeWeights = new Vector4Parameter { value = new Vector4(0.3f, 0.3f, 0.26f, 0.15f) };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value
                && intensity.value > 0f;
        }
    }

    [UnityEngine.Scripting.Preserve]
    internal sealed class CustomBloomRenderer : PostProcessEffectRenderer<CustomBloom>
    {
        //Custom Bloom
        const float BloomTextureScale = 0.25f;
        const int BlurTextureTotalHeight = 158;
        const int BlurBaseHeight1 = 85;
        const int BlurBaseHeight2 = 50;
        const int BlurBaseHeight3 = 20;

        static Mesh s_FullscreenMesh = null;
        static Mesh s_customBloomMesh = null;
        static Mesh s_downSampleBlurMesh = null;

        public static Mesh fullscreenMesh
        {
            get
            {
                if (s_FullscreenMesh != null)
                    return s_FullscreenMesh;

                float topV = 1.0f;
                float bottomV = 0.0f;

                s_FullscreenMesh = new Mesh { name = "Fullscreen Quad" };
                s_FullscreenMesh.SetVertices(new List<Vector3>
                {
                    new Vector3(-1.0f, -1.0f, 0.0f),
                    new Vector3(-1.0f,  1.0f, 0.0f),
                    new Vector3(1.0f, -1.0f, 0.0f),
                    new Vector3(1.0f,  1.0f, 0.0f)
                });

                s_FullscreenMesh.SetUVs(0, new List<Vector2>
                {
                    new Vector2(0.0f, bottomV),
                    new Vector2(0.0f, topV),
                    new Vector2(1.0f, bottomV),
                    new Vector2(1.0f, topV)
                });

                s_FullscreenMesh.SetIndices(new[] { 0, 1, 2, 2, 1, 3 }, MeshTopology.Triangles, 0, false);
                s_FullscreenMesh.UploadMeshData(true);
                return s_FullscreenMesh;
            }
        }


        public static Mesh customBloomMesh
        {

            get
            {
                if (s_customBloomMesh != null)
                    return s_customBloomMesh;

                s_customBloomMesh = new Mesh { name = "Custom Bloom Mesh" };
                s_customBloomMesh.SetVertices(new List<Vector3>
                {
                    new Vector3(-1.0f, -1.0f, 0.0f),
                    new Vector3(-1.0f,  3.0f, 0.0f),
                    new Vector3(3.0f, -1.0f, 0.0f)
                });

                s_customBloomMesh.SetIndices(new[] { 0, 1, 2 }, MeshTopology.Triangles, 0, false);
                s_customBloomMesh.UploadMeshData(true);
                return s_customBloomMesh;
            }
        }

        public static Mesh downSampleBlurMesh
        {
            get
            {
                if (s_downSampleBlurMesh != null)
                    return s_downSampleBlurMesh;

                s_downSampleBlurMesh = new Mesh { name = "Custom Bloom Down Sample Mesh" };
                s_downSampleBlurMesh.SetVertices(new List<Vector3>
                {
                    new Vector3(-1.0f, -1.0f, 0.0f),
                    new Vector3(-1.0f,  1.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 0.0f),
                    new Vector3(1.0f,  1.0f, 0.0f),
                    new Vector3(1.0f, -1.0f, 0.0f),
                    new Vector3(-1.0f,  -1.0f, 0.0f)
                });

                s_downSampleBlurMesh.SetIndices(new[] { 0, 1, 2, 3, 4, 5 }, MeshTopology.Triangles, 0, false);
                s_downSampleBlurMesh.UploadMeshData(true);
                return s_downSampleBlurMesh;
            }
        }

        public static float Luminance(in Color color) => color.r * 0.2126729f + color.g * 0.7151522f + color.b * 0.072175f;

        public override void Init()
        {
              
        }

        public override void Render(PostProcessRenderContext context)
        {
            var cmd = context.command;
            cmd.BeginSample("CustomBloomPyramid");

            var sheet = context.propertySheets.Get(context.resources.shaders.customBloom);

            float clamp = settings.clamp.value;
            float threshold = Mathf.GammaToLinearSpace(settings.threshold.value);

            sheet.properties.SetVector(ShaderIDs._Params, new Vector4(settings.intensity.value, clamp, threshold, threshold / 2.0f));

            int tw = context.screenWidth;
            int th = context.screenHeight;
            float aspectRatio = (float)tw / th;

            int gaussWidth = (int)(tw * BloomTextureScale);
            int gaussHeight = (int)(th * BloomTextureScale);

            tw = gaussWidth;
            th = gaussHeight;

            context.GetScreenSpaceTemporaryRT(cmd, ShaderIDs._BloomBlurGaussH, 0, context.sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, tw , th);
            context.GetScreenSpaceTemporaryRT(cmd, ShaderIDs._BloomBlurGaussV, 0, context.sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, tw, th);

            //Whole Pass Params
            sheet.properties.SetVector("_UVTransformSource", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
            sheet.properties.SetVector("_UVTransformTarget", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));

            var source = context.source;

            //Filter
            cmd.SetRenderTarget(ShaderIDs._BloomBlurGaussH, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.SetGlobalTexture("_SourceTex", source);
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(customBloomMesh, Matrix4x4.identity, sheet.material, 0, 0 , sheet.properties);

            //Gauss H and V
            sheet.properties.SetVector("_Scaler", new Vector4(1.0f / tw, 0.0f, 0.0f, 0.0f));
            cmd.SetGlobalTexture("_SourceTex", ShaderIDs._BloomBlurGaussH);
            cmd.SetRenderTarget(ShaderIDs._BloomBlurGaussV, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(customBloomMesh, Matrix4x4.identity, sheet.material, 0, 1 , sheet.properties);


            cmd.SetGlobalTexture("_SourceTex", ShaderIDs._BloomBlurGaussV);
            sheet.properties.SetVector("_Scaler", new Vector4(0.0f, 1.0f / th, 0.0f, 0.0f));
            cmd.SetRenderTarget(ShaderIDs._BloomBlurGaussH, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(customBloomMesh, Matrix4x4.identity, sheet.material, 0, 2, sheet.properties);

            //DownSample
            float currentHeight = 0;
            int blurBaseWidth1 = Mathf.CeilToInt(aspectRatio * BlurBaseHeight1);
            tw = blurBaseWidth1;
            th = BlurTextureTotalHeight;

            context.GetScreenSpaceTemporaryRT(cmd, ShaderIDs._BloomMultiBlur1, 0, context.sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, tw, th);

            cmd.SetRenderTarget(ShaderIDs._BloomMultiBlur1, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.ClearRenderTarget(false, true, Color.clear);
            cmd.SetGlobalTexture("_SourceTex", ShaderIDs._BloomBlurGaussH);

            Vector2 scale = new Vector2(1.0f, (float)BlurBaseHeight1 / BlurTextureTotalHeight);
            Vector2 offset = new Vector2(0.0f, -(1.0f - scale.y));
            sheet.properties.SetVector("_UVTransformTarget", new Vector4(scale.x, scale.y, offset.x, offset.y));
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(downSampleBlurMesh, Matrix4x4.identity, sheet.material, 0, 3, sheet.properties);

            currentHeight += BlurBaseHeight1 + 1;
            scale = new Vector2(Mathf.Ceil(aspectRatio * BlurBaseHeight2) / blurBaseWidth1, (float)BlurBaseHeight2 / BlurTextureTotalHeight);
            offset = new Vector2(-(1.0f - scale.x), -(1.0f - scale.y) + 2.0f * currentHeight / BlurTextureTotalHeight);
            sheet.properties.SetVector("_UVTransformTarget", new Vector4(scale.x, scale.y, offset.x, offset.y));
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(downSampleBlurMesh, Matrix4x4.identity, sheet.material, 0, 3, sheet.properties);

            currentHeight += BlurBaseHeight2 + 1;
            scale = new Vector2(Mathf.Ceil(aspectRatio * BlurBaseHeight3) / blurBaseWidth1, (float)BlurBaseHeight3 / BlurTextureTotalHeight);
            offset = new Vector2(-(1.0f - scale.x), -(1.0f - scale.y) + 2.0f * currentHeight / BlurTextureTotalHeight);
            sheet.properties.SetVector("_UVTransformTarget", new Vector4(scale.x, scale.y, offset.x, offset.y));
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(downSampleBlurMesh, Matrix4x4.identity, sheet.material, 0, 3, sheet.properties);

            //Three Horizontal Blur
            context.GetScreenSpaceTemporaryRT(cmd, ShaderIDs._BloomMultiBlur2, 0, context.sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, tw, th);

            cmd.SetRenderTarget(ShaderIDs._BloomMultiBlur2, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.ClearRenderTarget(false, true, Color.clear);
            cmd.SetGlobalTexture("_SourceTex", ShaderIDs._BloomMultiBlur1);
            sheet.properties.SetVector("_Scaler", new Vector4(1.0f / blurBaseWidth1, 0.0f, 0.0f, 0.0f));
            currentHeight = 0;
            scale = new Vector2(1.0f, (float)BlurBaseHeight1 / BlurTextureTotalHeight);
            offset = new Vector2(0.0f, -(1.0f - scale.y));
            sheet.properties.SetVector("_UVTransformSource", new Vector4(scale.x, scale.y, 0.0f, 0.0f));
            sheet.properties.SetVector("_UVTransformTarget", new Vector4(scale.x, scale.y, offset.x, offset.y));
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(downSampleBlurMesh, Matrix4x4.identity, sheet.material, 0, 4, sheet.properties);

            currentHeight += BlurBaseHeight1 + 1;
            scale = new Vector2(Mathf.Ceil(aspectRatio * BlurBaseHeight2) / blurBaseWidth1, (float)BlurBaseHeight2 / BlurTextureTotalHeight);
            offset = new Vector2(-(1.0f - scale.x), -(1.0f - scale.y) + 2.0f * currentHeight / BlurTextureTotalHeight);
            sheet.properties.SetVector("_UVTransformSource", new Vector4(scale.x, scale.y, 0.0f, currentHeight / BlurTextureTotalHeight));
            sheet.properties.SetVector("_UVTransformTarget", new Vector4(scale.x, scale.y, offset.x, offset.y));
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(downSampleBlurMesh, Matrix4x4.identity, sheet.material, 0, 5, sheet.properties);

            currentHeight += BlurBaseHeight2 + 1;
            scale = new Vector2(Mathf.Ceil(aspectRatio * BlurBaseHeight3) / blurBaseWidth1, (float)BlurBaseHeight3 / BlurTextureTotalHeight);
            offset = new Vector2(-(1.0f - scale.x), -(1.0f - scale.y) + 2.0f * currentHeight / BlurTextureTotalHeight);
            sheet.properties.SetVector("_UVTransformSource", new Vector4(scale.x, scale.y, 0.0f, currentHeight / BlurTextureTotalHeight));
            sheet.properties.SetVector("_UVTransformTarget", new Vector4(scale.x, scale.y, offset.x, offset.y));
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(downSampleBlurMesh, Matrix4x4.identity, sheet.material, 0, 6, sheet.properties);

            //Three Vertical Blur
            cmd.SetRenderTarget(ShaderIDs._BloomMultiBlur1, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.SetGlobalTexture("_SourceTex", ShaderIDs._BloomMultiBlur2);
            sheet.properties.SetVector("_Scaler", new Vector4(0.0f, 1.0f / BlurTextureTotalHeight, 0.0f, 0.0f));
            currentHeight = 0;
            scale = new Vector2(1.0f, (float)BlurBaseHeight1 / BlurTextureTotalHeight);
            offset = new Vector2(0.0f, -(1.0f - scale.y));
            cmd.ClearRenderTarget(false, true, Color.clear);
            sheet.properties.SetVector("_UVTransformSource", new Vector4(scale.x, scale.y, 0.0f, 0.0f));
            sheet.properties.SetVector("_UVTransformTarget", new Vector4(scale.x, scale.y, offset.x, offset.y));
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(downSampleBlurMesh, Matrix4x4.identity, sheet.material, 0, 4, sheet.properties);

            currentHeight += BlurBaseHeight1 + 1;
            scale = new Vector2(Mathf.Ceil(aspectRatio * BlurBaseHeight2) / blurBaseWidth1, (float)BlurBaseHeight2 / BlurTextureTotalHeight);
            offset = new Vector2(-(1.0f - scale.x), -(1.0f - scale.y) + 2.0f * currentHeight / BlurTextureTotalHeight);
            sheet.properties.SetVector("_UVTransformSource", new Vector4(scale.x, scale.y, 0.0f, currentHeight / BlurTextureTotalHeight));
            sheet.properties.SetVector("_UVTransformTarget", new Vector4(scale.x, scale.y, offset.x, offset.y));
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(downSampleBlurMesh, Matrix4x4.identity, sheet.material, 0, 5, sheet.properties);

            currentHeight += BlurBaseHeight2 + 1;
            scale = new Vector2(Mathf.Ceil(aspectRatio * BlurBaseHeight3) / blurBaseWidth1, (float)BlurBaseHeight3 / BlurTextureTotalHeight);
            offset = new Vector2(-(1.0f - scale.x), -(1.0f - scale.y) + 2.0f * currentHeight / BlurTextureTotalHeight);
            sheet.properties.SetVector("_UVTransformSource", new Vector4(scale.x, scale.y, 0.0f, currentHeight / BlurTextureTotalHeight));
            sheet.properties.SetVector("_UVTransformTarget", new Vector4(scale.x, scale.y, offset.x, offset.y));
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(downSampleBlurMesh, Matrix4x4.identity, sheet.material, 0, 6, sheet.properties);
            //Merge
            tw = gaussWidth;
            th = gaussHeight;
            context.GetScreenSpaceTemporaryRT(cmd, ShaderIDs._Bloom_Texture, 0, context.sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, tw, th);

            cmd.SetRenderTarget(ShaderIDs._Bloom_Texture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.SetGlobalTexture("_MultiBlurTex", ShaderIDs._BloomMultiBlur1);
            cmd.SetGlobalTexture("_SourceTex", ShaderIDs._BloomBlurGaussH);

            currentHeight = 0;
            scale = new Vector2(1.0f, (float)BlurBaseHeight1 / BlurTextureTotalHeight);
            sheet.properties.SetVector("_UVTransformBlur1", new Vector4(scale.x, scale.y, 0.0f, 0.0f));

            currentHeight += BlurBaseHeight1 + 1;
            scale = new Vector2(Mathf.Ceil(aspectRatio * BlurBaseHeight2) / blurBaseWidth1, (float)BlurBaseHeight2 / BlurTextureTotalHeight);
            sheet.properties.SetVector("_UVTransformBlur2", new Vector4(scale.x, scale.y, 0.0f, currentHeight / BlurTextureTotalHeight));

            currentHeight += BlurBaseHeight2 + 1;
            scale = new Vector2(Mathf.Ceil(aspectRatio * BlurBaseHeight3) / blurBaseWidth1, (float)BlurBaseHeight3 / BlurTextureTotalHeight);
            sheet.properties.SetVector("_UVTransformBlur3", new Vector4(scale.x, scale.y, 0.0f, currentHeight / BlurTextureTotalHeight));
            sheet.properties.SetVector("_BlurComposeWeights", settings.blurComposeWeights.value);
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(customBloomMesh, Matrix4x4.identity, sheet.material, 0, 7, sheet.properties);

            cmd.ReleaseTemporaryRT(ShaderIDs._BloomBlurGaussH);
            cmd.ReleaseTemporaryRT(ShaderIDs._BloomBlurGaussV);
            cmd.ReleaseTemporaryRT(ShaderIDs._BloomMultiBlur1);
            cmd.ReleaseTemporaryRT(ShaderIDs._BloomMultiBlur2);

            // Setup bloom on uber
            var uberSheet = context.uberSheet;

            var dirtTexture = RuntimeUtilities.blackTexture;
            var dirtRatio = (float)dirtTexture.width / (float)dirtTexture.height;
            var screenRatio = (float)context.screenWidth / (float)context.screenHeight;
            var dirtTileOffset = new Vector4(1f, 1f, 0f, 0f);
            if (dirtRatio > screenRatio)
            {
                dirtTileOffset.x = screenRatio / dirtRatio;
                dirtTileOffset.z = (1f - dirtTileOffset.x) * 0.5f;
            }
            else if (screenRatio > dirtRatio)
            {
                dirtTileOffset.y = dirtRatio / screenRatio;
                dirtTileOffset.w = (1f - dirtTileOffset.y) * 0.5f;
            }

            var tint = settings.tint.value.linear;
            var luma = Luminance(tint);
            tint = luma > 0f ? tint * (1f / luma) : Color.white;

            var bloomParams = new Vector4(1, settings.intensity.value, 1, 1);
            uberSheet.properties.SetVector(ShaderIDs.Bloom_Settings, bloomParams);
            uberSheet.properties.SetVector(ShaderIDs.Bloom_DirtTileOffset, dirtTileOffset);
            uberSheet.properties.SetColor(ShaderIDs.Bloom_Color, tint);
            uberSheet.properties.SetTexture(ShaderIDs.Bloom_DirtTex, dirtTexture);

            cmd.SetGlobalTexture(ShaderIDs.BloomTex, ShaderIDs._Bloom_Texture);

            uberSheet.EnableKeyword("BLOOM_LOW");

            cmd.EndSample("CustomBloomPyramid");
            context.bloomBufferNameID = ShaderIDs._Bloom_Texture;
        }
    }
}
