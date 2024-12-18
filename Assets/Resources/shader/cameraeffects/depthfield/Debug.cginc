
#include "Common.cginc"

sampler2D_float _CameraDepthTexture;

// Camera parameters
float _Distance;
float _LensCoeff;  // f^2 / (N * (S1 - f) * film_width * 2)

// Fragment shader: CoC visualization
half4 frag_CoC(v2f i) : SV_Target
{
    half4 src = tex2D(_MainTex, i.uv);
    float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uvAlt));

    // Calculate the radiuses of CoC.
    float coc = (depth - _Distance) * _LensCoeff / depth;
    coc *= 80;

    // Visualize CoC (white -> red -> gray)
    half3 rgb = lerp(half3(1, 0, 0), half3(1, 1, 1), saturate(-coc));
    rgb = lerp(rgb, half3(0.4, 0.4, 0.4), saturate(coc));

    // Black and white image overlay
    rgb *= dot(src.rgb, 0.5 / 3) + 0.5;

    // Gamma correction
    rgb = lerp(rgb, GammaToLinearSpace(rgb), unity_ColorSpaceLuminance.w);

    return half4(rgb, src.a);
}
