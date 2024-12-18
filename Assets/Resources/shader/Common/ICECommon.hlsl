#ifndef ICE_COMMON
#define ICE_COMMON


inline float _DecodeFloatRGBA(float4 enc)
{
	float4 kDecodeDot = float4(1.0, 1 / 255.0, 1 / 65025.0, 1 / 160581375.0);
	return dot(enc, kDecodeDot);
}

float3 mod3D289(float3 x) { return x - floor(x / 289.0) * 289.0; }

float4 mod3D289(float4 x) { return x - floor(x / 289.0) * 289.0; }

float4 permute(float4 x) { return mod3D289((x * 34.0 + 1.0) * x); }

float4 taylorInvSqrt(float4 r) { return 1.79284291400159 - r * 0.85373472095314; }

float snoise(float3 v)
{
	const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);
	float3 i = floor(v + dot(v, C.yyy));
	float3 x0 = v - i + dot(i, C.xxx);
	float3 g = step(x0.yzx, x0.xyz);
	float3 l = 1.0 - g;
	float3 i1 = min(g.xyz, l.zxy);
	float3 i2 = max(g.xyz, l.zxy);
	float3 x1 = x0 - i1 + C.xxx;
	float3 x2 = x0 - i2 + C.yyy;
	float3 x3 = x0 - 0.5;
	i = mod3D289(i);
	float4 p = permute(permute(permute(i.z + float4(0.0, i1.z, i2.z, 1.0)) + i.y + float4(0.0, i1.y, i2.y, 1.0)) + i.x + float4(0.0, i1.x, i2.x, 1.0));
	float4 j = p - 49.0 * floor(p / 49.0);  // mod(p,7*7)
	float4 x_ = floor(j / 7.0);
	float4 y_ = floor(j - 7.0 * x_);  // mod(j,N)
	float4 x = (x_ * 2.0 + 0.5) / 7.0 - 1.0;
	float4 y = (y_ * 2.0 + 0.5) / 7.0 - 1.0;
	float4 h = 1.0 - abs(x) - abs(y);
	float4 b0 = float4(x.xy, y.xy);
	float4 b1 = float4(x.zw, y.zw);
	float4 s0 = floor(b0) * 2.0 + 1.0;
	float4 s1 = floor(b1) * 2.0 + 1.0;
	float4 sh = -step(h, 0.0);
	float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
	float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
	float3 g0 = float3(a0.xy, h.x);
	float3 g1 = float3(a0.zw, h.y);
	float3 g2 = float3(a1.xy, h.z);
	float3 g3 = float3(a1.zw, h.w);
	float4 norm = taylorInvSqrt(float4(dot(g0, g0), dot(g1, g1), dot(g2, g2), dot(g3, g3)));
	g0 *= norm.x;
	g1 *= norm.y;
	g2 *= norm.z;
	g3 *= norm.w;
	float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
	m = m * m;
	m = m * m;
	float4 px = float4(dot(x0, g0), dot(x1, g1), dot(x2, g2), dot(x3, g3));
	return 42.0 * dot(m, px);	
}

inline half3 _NormalScale(sampler2D NormalTex, float2 UV, half normalScale)
{
	half3 frozenBump = UnpackNormal(tex2D(NormalTex, UV)).rgb;
	frozenBump.xy *= normalScale;
	frozenBump.z = max(1.0e-16, sqrt(1.0 - saturate(dot(frozenBump.xy, frozenBump.xy))));
	return frozenBump;
}

float3 _TransformWorldToTangent(float3 dirWS, float3x3 tangentToWorld)
{
    // Note matrix is in row major convention with left multiplication as it is build on the fly
    float3 row0 = tangentToWorld[0];
    float3 row1 = tangentToWorld[1];
    float3 row2 = tangentToWorld[2];

    // these are the columns of the inverse matrix but scaled by the determinant
    float3 col0 = cross(row1, row2);
    float3 col1 = cross(row2, row0);
    float3 col2 = cross(row0, row1);

    float determinant = dot(row0, col0);
    float sgn = determinant<0.0 ? (-1.0) : 1.0;

    // inverse transposed but scaled by determinant
    // Will remove transpose part by using matrix as the first arg in the mul() below
    // this makes it the exact inverse of what TransformTangentToWorld() does.
    float3x3 matTBN_I_T = float3x3(col0, col1, col2);

    return normalize( sgn * mul(matTBN_I_T, dirWS) );
}


inline half3 ApplyIceBlendColor(float3 viewDirWS, float3 positionWS, sampler2D IceTex, half3 IceColorTop,
	half IceNoise, half IceNoisePower,half2 UV,half3 waterColor)//half3 IceColorBackground,half IceDepth,  half BackGroundIceBlend,
{
	half2 iceUV = UV;
	
	///////////////////////
	float iceDepthVal = 0;// (IceDepth * 0.5 * 0.01);
	half2 temp_cast_1 = 0;// (iceDepthVal * 2.0).xx;
	float cosOffset = 0.825;//34.37//cos(0.6);//
	float sinOffset = 0.565;//sin(0.6);//
	half2x2 offsetMat = half2x2(cosOffset, -sinOffset, sinOffset, cosOffset);
	half2 rotatorUV1 = mul(temp_cast_1 - half2(0.5, 0.5), offsetMat) + half2(0.5, 0.5);
	//half3 blendOpSrc = IceTex.Sample(sampleIceTex, (rotatorUV1 + half2(0.7, 0.3))).rgb;
	half3 blendOpSrc = tex2D(IceTex, (rotatorUV1 + half2(0.7, 0.3))).rgb;
	half3 color1 = blendOpSrc;

	//half3 blendOpSrc2 = (IceTex.Sample(sampleIceTex, iceUV).rgb * IceColorTop).rgb;
	half3 blendOpSrc2 = (tex2D(IceTex, iceUV).rgb * IceColorTop).rgb;
	half3 color2 = blendOpSrc2;
	
	//if (BackGroundIceBlend > 0)
	//{
	//	half2 Offset238 = (-1 * viewDirWS.xy * iceDepthVal) + iceUV;
	//	half2 rotator244 = mul((Offset238 * half2(2, 2)) - half2(0.5, 0.5), offsetMat) + half2(0.5, 0.5);
	//	half3 blendOpDest = lerp(blendOpSrc, (IceTex.Sample(sampleIceTex, rotator244).rgb * IceColorBackground), BackGroundIceBlend);
	//	color1 = saturate(max(blendOpSrc, blendOpDest));

	//	half2 Offset201 = (-1 * viewDirWS.xy * IceDepth * 0.01) + iceUV;
	//	half3 blendOpDest2 = lerp(blendOpSrc2, (IceTex.Sample(sampleIceTex, Offset201).rgb * IceColorBackground), BackGroundIceBlend);
	//	color2 = saturate(max(blendOpSrc2, blendOpDest2));
	//}
	
	half simplePerlin = IceNoise;////snoise((positionWS / IceNoiseTiling));
	half3 simplePerlinBlendColor = lerp(color1, color2, simplePerlin);
	//half3 blendOpSrc_3 = lerp(half3(0, 0, 0), simplePerlinBlendColor, IceNoisePower);
	half3 blendOpSrc_3 = lerp(waterColor, simplePerlinBlendColor, IceNoisePower);


	half3 clampResult = clamp((saturate(max(blendOpSrc_3, color2))), half3(0, 0, 0), half3(1, 1, 1));
	//half4 blendIceColor = (half4(clampResult, _IceSmoothness));
	return clampResult.rgb;
}



inline half3 IceLightingRIM(half3 ambientColor, half lightAtten, float3 worldNormal, float3 worldViewDir, half4 RimColor,
	half4 SSSColor, half SSS,half SSSTransShadow, half RimPower, half RimIntensity,half RimBase,half mask,half useRimThicknessMask=1.0)
{
	half3 ssslightColor = _LightColor0.rgb * lerp(1, lightAtten, SSSTransShadow);
	//half3 lightColor = _LightColor0.rgb *lightAtten;
	half rim = 1.0 - max(0, dot(worldNormal, worldViewDir));
	half rimValue = lerp(rim, 0, SSS * 0.5);
	half3 newRimColor = ambientColor  * RimColor.rgb;
	//return lerp(SSSColor.a * SSSColor.rgb * ssslightColor, newRimColor, rimValue) * pow(abs(rimValue), RimPower) * RimIntensity * mask; //-0.59
	//half3 blendColor = SSSColor.a * SSSColor.rgb * ssslightColor * useRimThicknessMask;//RIM不使用遮罩时，混合颜色为0
	return  newRimColor* saturate(pow(abs(rimValue), RimPower) * (1 - RimBase) + RimBase)*RimIntensity * mask;

	//return lerp(blendColor, newRimColor, saturate(abs(rimValue))) * saturate(pow(abs(rimValue), RimPower) * (1 - RimBase) + RimBase)*RimIntensity * mask;
}

inline half3 IceLightingScattering(half lightAtten,half3 worldLightDir, half3 worldViewDir, float3 worldNormal,
	half3 albedo,  half4 transParams, half4 SSSColor,half frontTransNormalDistortion,half frontSSSIntensity, inout float SSS)
{	
	//_TransScattering, _BackTransNormalDistortion, _Translucency, _TransShadow
	half TransScattering = transParams.x;	
	half BackTransNormalDistortion = transParams.y;
	half Translucency = transParams.z;
	half TransShadow = transParams.w;

	float3 frontLitDir = worldNormal * frontTransNormalDistortion - worldLightDir;
	float3 backLitDir = worldNormal * BackTransNormalDistortion + worldLightDir;
	float frontSSS = saturate(dot(worldViewDir, -frontLitDir));
	float backSSS = saturate(dot(worldViewDir, -backLitDir));
	float SSSDot = saturate(frontSSS * frontSSSIntensity + backSSS);
	SSS = SSSDot;

	half3 translucencyRGB = albedo;
	lightAtten = lerp(1, lightAtten, TransShadow);
	half3 lightColor = _LightColor0.rgb * lightAtten;
	half transVdotL = pow(SSSDot, TransScattering);
	half3 translucency = lightColor * transVdotL * SSSColor.a * SSSColor.rgb;//  *translucencyRGB;
	return  half3(albedo * saturate(translucency) * Translucency);
}

inline half3 ICEDustColor(sampler2D DustTex, half3 DustColor, float3 positionWS, half3 worldViewDir, float3x3 tangentToWorld,
	float2 uv, half useWSUV,half noiseUVScale,half dustNoiseIntensity,half dustDepthShift,half4 UVScale)
{
//#if _G_ICE_DUST_ON
	float2 wsUV = float2(positionWS.x, -positionWS.z);
	float2 sampleUV = (wsUV * useWSUV + uv * (1 - useWSUV));
	float2 sampleUV1 = sampleUV * noiseUVScale;
	half dustNoise = dustDepthShift;
//#if _G_ICE_DUST_NOISE
		//half dustTex_B = DustTex.Sample(sampleDustTex, sampleUV1).g * dustNoiseIntensity;//.b
		half dustTex_B = tex2D(DustTex, sampleUV1).g * dustNoiseIntensity;//.b
		dustNoise = (dustTex_B + dustDepthShift);
//#endif	

	float2 tangentViewDir = _TransformWorldToTangent(worldViewDir * -0.5, tangentToWorld).xy;
	float2 neweSampleUV = tangentViewDir * dustNoise + (sampleUV * UVScale.xy + UVScale.zw * _Time.x);
	//return  DustTex.Sample(sampleDustTex, neweSampleUV).r * DustColor;
	return  tex2D(DustTex, neweSampleUV).r * DustColor;
//#else
//	return (half3)0;
//#endif
}



inline half3 IceLightingScattering_Lake(half lightAtten,half3 worldLightDir, half3 worldViewDir, float3 worldNormal, half3 indirectDiffuse,
	half3 albedo, half3 translucencyRGB,half4 transParams, half TransAmbient)
{
	half TransScattering = transParams.x;
	half TransNormalDistortion = transParams.y;
	half Translucency = transParams.z;
	half TransShadow = transParams.w;

	
	lightAtten = lerp(1, lightAtten, TransShadow);
	half3 lightColor = _LightColor0.rgb * lightAtten;
	
	worldLightDir = normalize(worldLightDir + worldNormal * TransNormalDistortion);
	half transVdotL = pow(saturate(dot(worldViewDir, -worldLightDir)), TransScattering);
	//half3 translucency = lightColor * (transVdotL * TransDirect + indirectDiffuse * TransAmbient) * translucencyRGB;
	half3 translucency = lightColor * (transVdotL + indirectDiffuse * TransAmbient) * translucencyRGB;
	return  half3(albedo * translucency * Translucency);	
}

//inline void NormalFromTexture_float(Texture2D Texture, SamplerState Sampler, float2 UV, float Offset, float Strength, out float3 Out)
//{
//	Offset = pow(Offset, 3) * 0.1;
//	float2 offsetU = float2(UV.x + Offset, UV.y);
//	float2 offsetV = float2(UV.x, UV.y + Offset);
//	float normalSample = SAMPLE_TEXTURE2D(Texture, Sampler, UV);
//	float uSample = SAMPLE_TEXTURE2D(Texture, Sampler, offsetU);
//	float vSample = SAMPLE_TEXTURE2D(Texture, Sampler, offsetV);
//	float3 va = float3(1, 0, (uSample - normalSample) * Strength);
//	float3 vb = float3(0, 1, (vSample - normalSample) * Strength);
//	Out = normalize(cross(va, vb));
//
//
//}

inline void FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
{
	Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);

	//float FresnelEffect = 0;
//half FresnelStrength = _IceDepth;
//FresnelEffect_float(input.normalWS, input.viewDirWS,1 / FresnelStrength, FresnelEffect);
//surfaceData.albedo.rgb = _IceColorTop;// +FresnelEffect.xxx * 0.5;
}




#endif

