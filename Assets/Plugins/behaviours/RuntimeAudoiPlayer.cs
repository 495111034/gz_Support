using System;
using UnityEngine;

public static class RuntimeAudoiPlayer 
{
    public static Func<string, Transform, float, int> Play3DAudio1;
    public static Func<string, Transform, long, float, int> Play3DSkillAudio1;
    public static Func<string, Vector3, int> Play3DAudio2;
    public static Func<int, bool> StopAudio;
}