#ifndef BF_SCENE_UVANIEFFECTWAVEFUN_INCLUDED
#define BF_SCENE_UVANIEFFECTWAVEFUN_INCLUDED

half _WindBlend;
half4 _WavingColor;
float4 _WindParams; 

///uv动画
half4 _WaveParams;
half _WaveStartV;

///交互
float _CUSTOM_INTERACT;
half _PushStrength;
half _PushRadiu;
float4 _HeroPos;

///WAVE
half _EnableWave;//xx add
half _WaveLength;
half _WaveAmplitude;

void FastSinCos (float4 val, out float4 s, out float4 c) {
    val = val * 6.408849 - 3.1415927;
    // powers for taylor series
    float4 r5 = val * val;                  // wavevec ^ 2
    float4 r6 = r5 * r5;                        // wavevec ^ 4;
    float4 r7 = r6 * r5;                        // wavevec ^ 6;
    float4 r8 = r6 * r5;                        // wavevec ^ 8;

    float4 r1 = r5 * val;                   // wavevec ^ 3
    float4 r2 = r1 * r5;                        // wavevec ^ 5;
    float4 r3 = r2 * r5;                        // wavevec ^ 7;

    //Vectors for taylor's series expansion of sin and cos
    float4 sin7 = {1, -0.16161616, 0.0083333, -0.00019841};
    float4 cos8  = {-0.5, 0.041666666, -0.0013888889, 0.000024801587};

    // sin
    s =  val + r1 * sin7.y + r2 * sin7.z + r3 * sin7.w;

    // cos
    c = 1 + r5 * cos8.x + r6 * cos8.y + r7 * cos8.z + r8 * cos8.w;
}

void GrassWind(inout float4 vertex, inout float4 vertexWS, inout half4 color, float2 uv)
{
    vertexWS = mul(unity_ObjectToWorld, vertex);
    half4 _waveXSize = half4(0.012, 0.02, 0.06, 0.024) * _WindParams.y;
    half4 _waveZSize = half4 (0.006, .02, 0.02, 0.05) * _WindParams.y;
    half4 waveSpeed = half4 (1.2, 2, 1.6, 4.8);

    half4 _waveXmove = half4(0.024, 0.04, -0.12, 0.096);
    half4 _waveZmove = half4 (0.006, .02, -0.02, 0.1);

    float4 waves;
    waves = vertexWS.x * _waveXSize;
    waves += vertexWS.z * _waveZSize;

    // Add in time to model them over time
    waves += _WindParams.x * waveSpeed * _Time.y;

    float4 s, c;
    waves = frac (waves);
    FastSinCos (waves, s, c);

    s = s * s;

    s = s * s;

    half waveAmount =  saturate(uv.y * 2.22 - 0.33);// saturate((uv.y - 0.15) / (0.6 - 0.15));
    s = s * waveAmount * waveAmount;

    float3 waveMove = float3 (0,0,0);
    waveMove.x = dot (s, _waveXmove);
    waveMove.z = dot (s, _waveZmove);

    vertexWS.xz -= waveMove.xz * _WindParams.z;
#if _G_UVANI_INTERACT
    UNITY_BRANCH
    if(_CUSTOM_INTERACT > 0.5)
    {
        float dis = distance(_HeroPos, vertexWS);
        float3 pushDown = saturate((1 - saturate(dis / _PushRadiu)) * uv.y * _PushStrength) * 0.4;
        float3 direction = normalize(vertexWS.xyz - _HeroPos.xyz);
        direction.y *= 0.3;
        vertexWS.xz += (direction * pushDown).xz;
    }
#endif

    float lighting = dot(s, normalize(float4 (1, 1, .4, .2))) * .7;
    half3 waveColor = lerp(0.5, _WavingColor, lighting);
    float3 offset = vertexWS.xyz - _WorldSpaceCameraPos.xyz;

    vertex.xyz = mul(unity_WorldToObject, vertexWS).xyz;
    color.rgb = lerp(half3(1, 1, 1), 2 * waveColor, (2 * (1 - dot(offset, offset) / _WindParams.w)));
}

void CalulateVertByWave(float2 uv0,inout float4 vertexOS, inout float4 vertexWS)
{
    vertexWS = mul(unity_ObjectToWorld, vertexOS);

#if (defined(_WAVE_WAVE) && _G_UVANI_WAVE) || _G_UVANI_INTERACT
#if defined(_WAVE_WAVE) && _G_UVANI_WAVE
    half pushFactor = 0.4;
#else
    half pushFactor = 0.6;
#endif

    UNITY_BRANCH
    if(uv0.y > _WaveStartV)
    {
    //交互
    #if _G_UVANI_INTERACT
        UNITY_BRANCH
        if(_CUSTOM_INTERACT > 0.5)
        {
            float dis = distance(_HeroPos, vertexWS);
            float3 pushDown = saturate((1 - saturate(dis / _PushRadiu)) * uv0.y * _PushStrength) * pushFactor;
            float3 direction = normalize(vertexWS.xyz - _HeroPos.xyz);
            direction.y *= 0.3;
            vertexWS.xz += (direction * pushDown).xz;
        }
    #endif

    #if defined(_WAVE_WAVE) && _G_UVANI_WAVE
        half wind = _WaveParams.y * sin(_WaveParams.z * 2.0 * 3.14 * _Time.y + vertexWS.x);
        float param = smoothstep(0, _WaveParams.x, uv0.y);
        vertexWS.x += wind * param * smoothstep(0, _WaveParams.x, uv0.x);
        vertexWS.x -= wind * param * smoothstep(0, uv0.x, _WaveParams.x);
    #endif
    vertexOS.xyz = mul(unity_WorldToObject, vertexWS).xyz;
    }
#endif
}

#endif