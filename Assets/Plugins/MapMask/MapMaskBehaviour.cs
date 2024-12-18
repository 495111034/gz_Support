using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Mask = System.Byte;



/// <summary>
/// 地形掩码脚本
/// </summary>
[ExecuteInEditMode]
public class MapMaskBehaviour : MonoBehaviour
{
#if UNITY_EDITOR

    [NonSerialized]
    public MapMask mm;

    [NonSerialized]
    public Mesh[,] meshs;

    [NonSerialized]
    Material _mat;

    [NonSerialized]
    Material _mat_z;

    [NonSerialized]
    public bool use_z;

    //
    public void Update()
    {
        if (meshs != null)
        {
            if (_mat == null)
            {
                _mat = new Material(resource.ShaderManager.Find("mapmask"));
                _mat.hideFlags = HideFlags.HideAndDontSave;

                _mat_z = new Material(resource.ShaderManager.Find("mapmask z"));
                _mat_z.hideFlags = HideFlags.HideAndDontSave;
            }
            var mat = use_z ? _mat_z : _mat;
            foreach (var mesh in meshs)
            {
                Graphics.DrawMesh(mesh, Matrix4x4.identity, mat, 0);
            }
        }
    }

#endif

}
