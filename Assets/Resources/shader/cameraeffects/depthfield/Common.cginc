

#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;
float4 _MainTex_TexelSize;

struct v2f
{
    float4 pos : SV_POSITION;
    half2 uv : TEXCOORD0;
    half2 uvAlt : TEXCOORD1;
};

// 标准vert pass，注意UNITY_SINGLE_PASS_STEREO需要u3d 5.5以上版本
v2f vert(appdata_img v)
{
    half2 uvAlt = v.texcoord;
#if UNITY_UV_STARTS_AT_TOP
    if (_MainTex_TexelSize.y < 0.0) uvAlt.y = 1 - uvAlt.y;
#endif

    v2f o;
#if defined(UNITY_SINGLE_PASS_STEREO)
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
    o.uvAlt = UnityStereoScreenSpaceUVAdjust(uvAlt, _MainTex_ST);
#else
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = v.texcoord;
    o.uvAlt = uvAlt;
#endif

    return o;
}
