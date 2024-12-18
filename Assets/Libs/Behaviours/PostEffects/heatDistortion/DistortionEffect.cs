using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 需要产生扭曲的Render
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class DistortionEffect : MonoBehaviour
{
    public Renderer Renderer { get; private set; }
    public Material Material { get; private set; }  

    private void OnEnable()
    {
        gameObject.SetLayerRecursively((int)ObjLayer.Hidden);
        Renderer = GetComponent<Renderer>();
        if (Renderer)
        {
            Renderer.enabled = false;
            Material = Renderer.sharedMaterial;
            if (Material)
            {
                if (Material.shader && Material.shader.name.CompareTo("hc/partical/Heat Distortion Source") == 0)
                {
                    Material.enableInstancing = true;
                    MyEffect.HeatDistortion.RegisterRender(this);
                }
                else
                {
                    Log.LogError($"{gameObject.name}为扭曲效果，必须使用shader:hc/partical/Heat Distortion Source");
                }
            }
        }
    }

    private void OnDisable()
    {
        MyEffect.HeatDistortion.DeregisterRender(this);
    }
}
