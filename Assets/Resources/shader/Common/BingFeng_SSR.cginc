#ifndef BINGFENG_SSR_INCLUED
#define BINGFENG_SSR_INCLUED

float UVJitter(float2 uv)
{
    return frac((52.9829189 * frac(dot(uv, float2(0.06711056, 0.00583715)))));
}

void SSRRayConvert(float3 positionWS, out float4 positionCS, out float3 screenPos)
{
    positionCS = UnityWorldToClipPos(positionWS);
    float k = 1.0 / positionCS.w;
    screenPos.xy = ComputeScreenPos(positionCS).xy * k ;
    screenPos.z = k;
}

float3 SSRRayMarch(float4 positionCS, float3 positionWS, float3 reflectDir, float rayMarchScale, float maxRayMarchStep)
{
    float4 startClipPos = 0;
    float3 startScreenPos;
    SSRRayConvert(positionWS, startClipPos, startScreenPos);

    float4 endClipPos;
    float3 endScreenPos;

    SSRRayConvert(positionWS + reflectDir, endClipPos, endScreenPos);

    float3 screenDir = endScreenPos - startScreenPos;
    float2 dScreenDir = abs(screenDir.xy);
    screenDir = screenDir * 1 / max(dScreenDir.x * _ScreenParams.x, dScreenDir.y * _ScreenParams.y) * rayMarchScale;

    half rayMarchedTimes = UVJitter(positionCS.xy) * 0.1;

    half lastRayDepth = startClipPos.w;
    float3 lastScreenMarchUVZ = startScreenPos;
    float lastDeltaDepth = 0;

#if defined (SHADER_API_OPENGL) || defined (SHADER_API_D3D11) || defined (SHADER_API_D3D12)
    [unroll(4)]//SSRMaxSampleCount= Range(0, 3)
#else
    UNITY_LOOP
#endif
    for(int i = 0; i < maxRayMarchStep; i++)
    {
        rayMarchedTimes += 1;
        float3 currentMarchUVZ = startScreenPos + screenDir * rayMarchedTimes;

        if((currentMarchUVZ.x <= 0) || (currentMarchUVZ.x >= 1) || (currentMarchUVZ.y <= 0) || (currentMarchUVZ.y >= 1))
        {
            break;
        }

        float currentScreenDepth = LinearEyeDepth(tex2Dlod(_CameraDepthTexture, float4(currentMarchUVZ.xy, 0, 0)));
        float rayDepth = 1.0 / currentMarchUVZ.z;

        half deltaDepth = rayDepth - currentScreenDepth;
        if((deltaDepth > 0) && (deltaDepth < (abs(rayDepth - lastRayDepth) * 2)))
        {
            float samplePercent = saturate(lastDeltaDepth / (lastDeltaDepth - deltaDepth));
            float hitRayDepth = lerp(lastRayDepth, rayDepth, samplePercent);
            hitRayDepth = 1 / hitRayDepth;
            samplePercent = (hitRayDepth - lastScreenMarchUVZ.z) / (currentMarchUVZ.z - lastScreenMarchUVZ.z);
            samplePercent = lerp(samplePercent, 1, rayDepth >= _ProjectionParams.z);
            float3 hitScreenUVZ = lerp(lastScreenMarchUVZ, currentMarchUVZ, samplePercent);
            return float3(hitScreenUVZ.xy, 1);
        }

        lastRayDepth = rayDepth;
        lastScreenMarchUVZ = currentMarchUVZ;
        lastDeltaDepth = deltaDepth;
    }

    float4 farClipPos;
    float3 farScreenPos;

    SSRRayConvert(positionWS + reflectDir * 100000, farClipPos, farScreenPos);

    UNITY_BRANCH
    if((farScreenPos.x > 0) && (farScreenPos.x < 1) && (farScreenPos.y > 0) && (farScreenPos.y < 1))
    {
            float farDepth = LinearEyeDepth(tex2D(_CameraDepthTexture, farScreenPos.xy));
			float dept = farDepth - startClipPos.w;
			if(farDepth > startClipPos.w && dept < 200)
			{
				return float3(farScreenPos.xy, 1);
			}
    }

    return float3(0, 0, 0);
}

float3 GetSSRUVZ(float4 positionCS, float3 positionWS, float3 reflectDir, half NoV,float2 screenUV, float rayMarchScale, float maxRayMarchStep)
{
    float screenUV_val = screenUV.x * 2 - 1;
    screenUV_val *= screenUV_val;

    half ssrWeight = saturate(1 - screenUV_val * screenUV_val);

    half NdotV = NoV * 2.5;
    ssrWeight *= (1 - NdotV * NdotV);

    float3 uvz = 0;
    UNITY_BRANCH
    if(ssrWeight > 0.005)
    {
        uvz = SSRRayMarch(positionCS, positionWS, reflectDir, rayMarchScale, maxRayMarchStep);
        uvz.z *= ssrWeight;
    }
    return uvz;
}
#endif