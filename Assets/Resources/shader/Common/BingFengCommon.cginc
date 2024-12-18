
#ifndef CALMWATER_HELPER_INCLUDED
#define CALMWATER_HELPER_INCLUDED

////Specular/////
half SpecularTerm (half3 lightDir,half3 viewDir,half3 normalDir)
{
	return dot(reflect(-lightDir, normalDir), viewDir);
}

half3 SpecularRate (half gloss, half3 lightDir,half3 viewDir,half3 normalDir)
{
	return pow(max(0.0, SpecularTerm (lightDir,viewDir,normalDir) ), gloss * 128.0);
}
////Specular/////

inline half3 SafeNormalize(half3 inVec)
{
	half dp3 = max(0.001f, dot(inVec, inVec));
	return inVec * rsqrt(dp3);
}

inline half3 WorldNormal(half3 t0,half3 t1, half3 t2, half3 bump)
{
	return normalize( half3( dot(t0, bump) , dot(t1, bump) , dot(t2, bump) ) );
}

//==========================================================================================================
// UnpackNormals blend and scale
//==========================================================================================================
half3 UnpackNormalBlend ( half4 n1, half4 n2, half scale)
{
#if defined(UNITY_NO_DXT5nm)
	half3 normal = normalize((n1.xyz * 2 - 1) + (n2.xyz * 2 - 1));
	#if (SHADER_TARGET >= 30)
	normal.xy *= scale;
	#endif
	return normal;
#else
	half3 normal;
	normal.xy = (n1.wy * 2 - 1) + (n2.wy * 2 - 1);
	#if (SHADER_TARGET >= 30)
		normal.xy *= scale;
	#endif
	normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
	return normalize(normal);
#endif
}

#endif
