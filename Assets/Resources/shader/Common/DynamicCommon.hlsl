#ifndef DYNAMIC_INCLUDED
#define DYNAMIC_INCLUDED
	

half pow4(half x)
{
	half xx = x * x;
	return xx * xx;
}
half pow5(half x)
{
	half xx = x * x;
	return xx * xx * x;
}
half MetallicFromSpec(half3 specular)
{
	// return sqrt(max (max (specular.r, specular.g), specular.b));
	return max(max(specular.r, specular.g), specular.b);
}
half3 FresnelLerp1(half3 F0, half3 F90, half cosA)
{

	half t = pow5(1 - cosA);   // ala Schlick interpoliation
	return lerp(F0, F90, t);
	// return F0 + (1-F0) * t;
}
half3 DirectBDRFURP(half NoH, half LoH, half roughness)
{
	half r2 = roughness * roughness;
	half d = NoH * NoH * (r2 - 1.0) + 1.0;

	half LoH2 = LoH * LoH;
	half normalizationTerm = roughness * 4.0 + 2.0;
	half specularTerm = r2 / ((d * d) * max(0.1h, LoH2) * normalizationTerm);

	half3 color = min(15.0, specularTerm);
	return color;
}
half3 ReflectIBL(half3 ReflectDir, half RoughArt, half Roughness, half3 FTermIBL)
{
	half _RoughIBL = (1.7 - 0.7 * RoughArt) * RoughArt;
	half4 _Cubemap_var = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, ReflectDir, _RoughIBL * 6);
	_Cubemap_var.rgb = DecodeHDREnvironment(_Cubemap_var, unity_SpecCube0_HDR);
	half3 ReflectFinal = _Cubemap_var.rgb * FTermIBL;
	return ReflectFinal;
}
half3 ReflectIBL(half3 ReflectDir, half RoughArt, half Roughness, half3 FTermIBL, half3 ColorCube,half CubePow, half CubeScale, TEXTURECUBE(_CubeTex), SAMPLER(sampler_CubeTex))
{
	half _RoughIBL = (1.7 - 0.7 * RoughArt) * RoughArt;
	half4 _Cubemap_var = SAMPLE_TEXTURECUBE_LOD(_CubeTex, sampler_CubeTex, ReflectDir, _RoughIBL * 6);
	_Cubemap_var.rgb = pow(abs(_Cubemap_var.rgb), CubePow) * ColorCube * CubeScale;
	half3 ReflectFinal = _Cubemap_var.rgb * FTermIBL;
	return ReflectFinal;
}
half D_GGXaniso(half RoughnessX, half RoughnessY, half NoH, half3 H, half3 X, half3 Y)
{
	half2 axay = half2(RoughnessX, RoughnessY);
		//   axay = axay * axay;
	// half ax = RoughnessX * RoughnessX;
	// half ay = RoughnessY * RoughnessY;
	half XoH = dot( X, H );
	half YoH = dot( Y, H );
	half4 f = half4(axay, XoH, YoH);
		f = f * f;
	// float d = XoH*XoH / (ax*ax + 1e-5f) + YoH*YoH / (ay*ay + 1e-5f) + NoH*NoH;
	// half d = XoH*XoH / (ax*ax) + YoH*YoH / (ay*ay) + NoH*NoH;
	half d = f.z/ f.x + f.w / f.y + NoH * NoH;
	// return 1 / ( PI * ax*ay * d*d  + 1e-4h);
	half c = (1 + RoughnessX * 2) / (axay.x*axay.y * d*d + 0.001);
		 c = min(15.0, c);
	return c;
}

half3 SimpleSpec(half NoH, half4 _SpecGlossMap_var, half NoLattenuation, half probeOCC, half3 lightColor, half3 normalWS)
{
	half3 NoLLow = NoLattenuation * probeOCC * lightColor;

	half3 s1 = pow(NoH, (_SpecGlossMap_var.a + 1) * 10)  * NoLLow;//简易高光
	half3 s2 = 0.15 * (1 - saturate(normalWS.y));//金属朝下的方向稍微提亮

	//无补充，有些金属可能会比较黑
	// half3 spec = s1 * _SpecGlossMap_var.rgb;

	//有补充，金属暗部获得轻微提亮
	half3 spec = (s1 + s2) * _SpecGlossMap_var.rgb;// *(1.0 - _SpecGlossMap_var.a);
	return spec;
}

half3 SimpleEnvReflection(half NoH, half4 _SpecGlossMap_var, half NoLattenuation, half probeOCC, half3 lightColor, half3 normalWS)
{
	half3 NoLLow = NoLattenuation * probeOCC * lightColor;

	half3 s1 = pow(NoH, (_SpecGlossMap_var.a + 1) * 10) * 2 * NoLLow;//简易高光
	half3 s2 = 0.15 * (1 - saturate(normalWS.y));//金属朝下的方向稍微提亮

	//无补充，有些金属可能会比较黑
	// half3 spec = s1 * _SpecGlossMap_var.rgb;

	//有补充，金属暗部获得轻微提亮
	half3 spec = (s1 + s2) * _SpecGlossMap_var.rgb;
	return spec;
}

half CalcSpecAASmoothness(half3 normalWS, half specularAAVariance, half specularAAThreshold, half smoothness)
{
	half3 deltaU = ddx(normalWS);
	half3 deltaV = ddy(normalWS);
	half variance = specularAAVariance * (dot(deltaU, deltaU) + dot(deltaV, deltaV));

	half roughness = (1.0 - smoothness) * (1.0 - smoothness);
	half squaredRoughness = saturate(roughness * roughness + min(2.0 * variance, specularAAThreshold * specularAAThreshold));
	squaredRoughness = sqrt(squaredRoughness);
	smoothness = 1.0 - sqrt(squaredRoughness);
	return smoothness;
}
// half D_GGXanisoX(half RoughnessX, half RoughnessY, half NoH, half3 H, half3 X, half3 Y)
// {
// 	half2 axay = half2(RoughnessX, NoH);
// 	half XoH = dot( X, H );
// 	half YoH = dot( Y, H );
// 	half4 f = half4(axay, XoH, YoH);
// 		f = f * f;
// 	half d = f.z/ f.x + f.w + f.y;
// 	half c = (1 + RoughnessX * 2) / (axay.x * d*d + 0.001);
// 		 c = min(15.0, c);
// 	return c;
// }

//half3 HairSpec(half lightColor, half3 spec1, half3 spec2, half dotTH, half glossiness1, half glossiness2,
//	half specRange, half3 halfDir, half3 viewDir, half3 normalWS, half3 noLfinal, half3 normalEnv,
//	half3 spec_EnvOcc, half specEnvOcc2, half3 flowmapVar, half3 bumMapVar)
//{
//
//}

#endif