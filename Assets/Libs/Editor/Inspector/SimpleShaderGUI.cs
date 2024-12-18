using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Rendering;

public class SimpleShaderGUI : ShaderGUI
{
    enum BlendMode
    {
        Blend,
        Add,
        Deepen,
    }

    BlendMode blendMode = BlendMode.Blend;
    private bool isOpenZwrite = false;
    private CullMode mCullMode = CullMode.Off;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {

        Material targetMat = materialEditor.target as Material;
      
        if (targetMat.HasProperty("_SrcBlend"))
        {
            float src = targetMat.GetFloat("_SrcBlend");
            float dst = targetMat.GetFloat("_DstBlend");

            if ((int)src == (int)UnityEngine.Rendering.BlendMode.DstColor 
                && (int)dst == (int)UnityEngine.Rendering.BlendMode.Zero)
            {
                blendMode = BlendMode.Deepen;
            }
            else if ((int)src == (int)UnityEngine.Rendering.BlendMode.SrcAlpha 
                     && (int)dst == (int)UnityEngine.Rendering.BlendMode.One)
            {
                blendMode = BlendMode.Add;
            }
            else
            {
                blendMode = BlendMode.Blend;
            }


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Blend Mode");

            blendMode = (BlendMode)EditorGUILayout.EnumPopup(blendMode);

            EditorGUILayout.EndHorizontal();

            switch (blendMode)
            {
                case BlendMode.Blend:
                    targetMat.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    targetMat.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    break;
                case BlendMode.Add:
                    targetMat.SetFloat("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    targetMat.SetFloat("_DstBlend",(int)UnityEngine.Rendering.BlendMode.One);
                    break;
                case BlendMode.Deepen:
                    targetMat.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    targetMat.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    break;
                default:
                    targetMat.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    targetMat.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    break;
            }
        }

        if (targetMat.HasProperty("_Zwrite"))
        {
            float z = targetMat.GetFloat("_Zwrite");

            if (z == 0)
            {
                isOpenZwrite = false;
            }
            else
            {
                isOpenZwrite = true;
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ZWrite");

            isOpenZwrite = EditorGUILayout.Toggle(isOpenZwrite);

            EditorGUILayout.EndHorizontal();
            
            if (isOpenZwrite)
            {
                targetMat.SetFloat("_Zwrite", 1);
            }
            else
            {
                targetMat.SetFloat("_Zwrite", 0);
            }
        }

        if (targetMat.HasProperty("_CullMode"))
        {
            mCullMode = (CullMode)targetMat.GetInt("_CullMode");
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cull Mode");

            CullMode mode = (CullMode)EditorGUILayout.EnumPopup(mCullMode);

            if (mode != mCullMode)
            {
                mCullMode = mode;
                targetMat.SetInt("_CullMode", (int)mCullMode);
            }

            EditorGUILayout.EndHorizontal();
        }

        // render the default gui
        base.OnGUI(materialEditor, properties);

        MaterialProperty mainUV = FindProperty("_UVMove", properties);
        MaterialProperty uSpeed = FindProperty("_USpeed", properties);
        MaterialProperty vSpeed = FindProperty("_VSpeed", properties);
        if (mainUV != null && uSpeed != null && vSpeed != null)
        {
            mainUV.floatValue = EditorGUILayout.Toggle("uv 流动", mainUV.floatValue == 1) ? 1 : 0;
            if(mainUV.floatValue == 1)
            {
                targetMat.EnableKeyword("_UVMOVE_ON");
                materialEditor.ShaderProperty(uSpeed, "流动速度X");
                materialEditor.ShaderProperty(vSpeed, "流动速度Y");
            }
            else
            {
                targetMat.DisableKeyword("_UVMOVE_ON");
            }
        }

        MaterialProperty useMask = FindProperty("_UseMask", properties);
        MaterialProperty maskTex = FindProperty("_MaskTex", properties);
        if (useMask != null && maskTex != null)
        {
            useMask.floatValue = EditorGUILayout.Toggle("启用遮罩", useMask.floatValue == 1) ? 1 : 0;
            if(useMask.floatValue == 1)
            {
                targetMat.EnableKeyword("_MASK_ON");
                materialEditor.ShaderProperty(maskTex, "遮罩图(alpha)");

                MaterialProperty maskUV = FindProperty("_MUVMove", properties);
                MaterialProperty muSpeed = FindProperty("_MUSpeed", properties);
                MaterialProperty mvSpeed = FindProperty("_MVSpeed", properties);

                if(maskUV != null && muSpeed != null && mvSpeed != null)
                {
                    maskUV.floatValue = EditorGUILayout.Toggle("uv 流动", maskUV.floatValue == 1) ? 1 : 0;
                    if(maskUV.floatValue == 1)
                    {
                        targetMat.EnableKeyword("_MASK_UVMOVE_ON");
                        materialEditor.ShaderProperty(muSpeed, "流动速度X");
                        materialEditor.ShaderProperty(mvSpeed, "流动速度Y");
                    }
                    else
                    {
                        targetMat.DisableKeyword("_MASK_UVMOVE_ON");
                    }
                }
            }
            else
            {
                targetMat.DisableKeyword("_MASK_ON");
            }
        }

        MaterialProperty showStencil = FindProperty("_ShowStencil", properties);
        MaterialProperty comStencil = FindProperty("_StencilComp", properties);
        MaterialProperty refStencil = FindProperty("_Stencil", properties);
        MaterialProperty opStencil = FindProperty("_StencilOp", properties);
        MaterialProperty writeMaskStencil = FindProperty("_StencilWriteMask", properties);
        MaterialProperty readMaskStencil = FindProperty("_StencilReadMask", properties);

        if(showStencil != null)
        {
            showStencil.floatValue = EditorGUILayout.Toggle("启用模板检测", showStencil.floatValue == 1) ? 1 : 0;
            if(showStencil.floatValue == 1)
            {
                if(comStencil != null)
                {
                    materialEditor.ShaderProperty(comStencil, "通过条件");
                }
                if (refStencil != null)
                {
                    materialEditor.ShaderProperty(refStencil, "Stencil ID");
                }
                if (opStencil != null)
                {
                    materialEditor.ShaderProperty(opStencil, "Stencil Operation");
                }
                if (writeMaskStencil != null)
                {
                    materialEditor.ShaderProperty(writeMaskStencil, "Stencil Write Mask");
                }
                if (readMaskStencil != null)
                {
                    materialEditor.ShaderProperty(readMaskStencil, "Stencil Read Mask");
                }
            }
        }
        targetMat.enableInstancing = true;

    }
}