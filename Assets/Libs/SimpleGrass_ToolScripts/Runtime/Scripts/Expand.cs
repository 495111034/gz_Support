using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Expand 
{
    public static bool CheckBoundIsInCamera(this Bounds bound, Matrix4x4 cameraMat)
    {
        System.Func<Vector4, int> ComputeOutCode = (projectionPos) =>
        {
            int _code = 0;
            if (projectionPos.x < -projectionPos.w) _code |= 1;
            if (projectionPos.x > projectionPos.w) _code |= 2;
            if (projectionPos.y < -projectionPos.w) _code |= 4;
            if (projectionPos.y > projectionPos.w) _code |= 8;
            if (projectionPos.z < -projectionPos.w) _code |= 16;
            if (projectionPos.z > projectionPos.w) _code |= 32;
            return _code;
        };        
       Vector4 worldPos = Vector4.one;
        Vector3 center = bound.center;
        Vector3 extents = bound.extents;
        int code = 63;
        for (int i = -1; i <= 1; i += 2)
        {
            for (int j = -1; j <= 1; j += 2)
            {
                for (int k = -1; k <= 1; k += 2)
                {
                    worldPos.x = center.x + i * extents.x;
                    worldPos.y = center.y + j * extents.y;
                    worldPos.z = center.z + k * extents.z;

                    //code &= ComputeOutCode(camera.projectionMatrix * camera.worldToCameraMatrix * worldPos);
                    code &= ComputeOutCode(cameraMat * worldPos);
                }
            }
        }
        return code == 0 ? true : false;
    }
}
