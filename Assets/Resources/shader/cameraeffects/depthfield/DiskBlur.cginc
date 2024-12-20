
#include "Common.cginc"
#include "DiskKernel.cginc"

// Camera parameters
float _RcpAspect;
float _MaxCoC;
float _RcpMaxCoC;

// Fragment shader: 圆形滤镜
half4 frag_Blur(v2f i) : SV_Target
{
    half4 samp0 = tex2D(_MainTex, i.uv);

    half4 bgAcc = 0; 
    half4 fgAcc = 0; 

    UNITY_LOOP for (int si = 0; si < kSampleCount; si++)
    {
        float2 disp = kDiskKernel[si] * _MaxCoC;
        float dist = length(disp);

        float2 duv = float2(disp.x * _RcpAspect, disp.y);
        half4 samp = tex2D(_MainTex, i.uv + duv);

        // BG: Compare CoC of the current sample and the center sample
        // and select smaller one.
        half bgCoC = max(min(samp0.a, samp.a), 0);

        // Compare the CoC to the sample distance.
        // Add a small margin to smooth out.
        const half margin = _MainTex_TexelSize.y * 2;
        half bgWeight = saturate((bgCoC   - dist + margin) / margin);
        half fgWeight = saturate((-samp.a - dist + margin) / margin);

        // Cut influence from focused areas because they're darkened by CoC
        // premultiplying. This is only needed for near field.
        fgWeight *= step(_MainTex_TexelSize.y, -samp.a);

        // Accumulation
        bgAcc += half4(samp.rgb, 1) * bgWeight;
        fgAcc += half4(samp.rgb, 1) * fgWeight;
    }

    // Get the weighted average.
    bgAcc.rgb /= bgAcc.a + (bgAcc.a == 0); // zero-div guard
    fgAcc.rgb /= fgAcc.a + (fgAcc.a == 0);

    // BG: Calculate the alpha value only based on the center CoC.
    // This is a rather aggressive approximation but provides stable results.
    bgAcc.a = smoothstep(_MainTex_TexelSize.y, _MainTex_TexelSize.y * 2, samp0.a);

    // FG: Normalize the total of the weights.
    fgAcc.a *= UNITY_PI / kSampleCount;

    // Alpha premultiplying
    half3 rgb = 0;
    rgb = lerp(rgb, bgAcc.rgb, saturate(bgAcc.a));
    rgb = lerp(rgb, fgAcc.rgb, saturate(fgAcc.a));

    // Combined alpha value
    half alpha = (1 - saturate(bgAcc.a)) * (1 - saturate(fgAcc.a));

    return half4(rgb, alpha);
}
