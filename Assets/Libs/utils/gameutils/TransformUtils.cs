using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// Transform 工具
public static class TransformUtils
{
    //// 计算从 t1->t2 的偏移量
    //public static Vector2 CalculateOffset(RectTransform t1, RectTransform t2, Camera camera)
    //{
    //    Vector2 tolocal1;
    //    Vector2 tolocal2;
    //    RectTransformUtility.ScreenPointToLocalPointInRectangle(t1, Vector2.zero, camera, out tolocal1);
    //    RectTransformUtility.ScreenPointToLocalPointInRectangle(t2, Vector2.zero, camera, out tolocal2);
    //    return tolocal2 - tolocal1;
    //}

    // 计算 pos1 在 t1 中的位置, 相对于 t2 的位置
    public static Vector3 CalculatePos2(Transform t1, Vector3 pos1, Transform t2)
    {
        var wpos = t1.localToWorldMatrix.MultiplyPoint(pos1);
        var pos2 = t2.worldToLocalMatrix.MultiplyPoint(wpos);
        return pos2;
    }

    // 视图位置, 左下00, 右上11
    public static Vector2 GetViewPos(RectTransform rt, Vector2 pos)
    {
        var rc = rt.rect;
        var x = (pos.x - rc.xMin) / rc.width;
        var y = (pos.y - rc.yMin) / rc.height;
        return new Vector2(x, y);
    }

    /// <summary>
    /// 从Matrix4x4矩阵到旋转
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static Quaternion ExtractRotation(this Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    /// <summary>
    /// 从Matrix4x4矩阵到平移位置
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static Vector3 ExtractPosition(this Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03;
        position.y = matrix.m13;
        position.z = matrix.m23;
        return position;
    }

    /// <summary>
    /// 从Matrix4x4矩阵到缩放数据
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static Vector3 ExtractScale(this Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
}
