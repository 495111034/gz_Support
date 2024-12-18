//
// 粒子发射器shader公共函数
//

sampler2D _PositionBuffer;          //C#传入，传入粒子位置信息(xyz)和时间信息(w)，上一次GPU计算结果（RenderTextuer）
sampler2D _RotationBuffer;          //C#传入,传入粒子旋转信息（四元数），上一次GPU计算结果（RenderTextuer）

half _ColorMode;
half4 _Color;
half4 _Color2;
float _ScaleMin;                    //C#传入, 传入缩放随机值限制
float _ScaleMax;                    //C#传入,传入缩放随机值限制
float _RandomSeed;


float2 _BufferOffset;               //C#传入，数据起点偏移量（uv值）
fixed _VerticalBillboarding;        //C#传入，广告牌垂直约束

// PRNG算法（梅森旋转算法）
float nrand(float2 uv, float salt)
{
    uv += float2(salt, _RandomSeed);
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

// 四元数乘法
// http://mathworld.wolfram.com/Quaternion.html
float4 qmul(float4 q1, float4 q2)
{
    return float4(
        q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
        q1.w * q2.w - dot(q1.xyz, q2.xyz)
    );
}

// 用四元数旋转矢量
// http://mathworld.wolfram.com/Quaternion.html
float3 rotate_vector(float3 v, float4 r)
{   
    float4 r_c = r * float4(-1, -1, -1, 1) ;    
    return qmul(r, qmul(float4(v, 0), r_c )).xyz ;
}

float3 Billboarding_rotate_vector(float3 v)
{   
    float4 r_c = float4(0, 0, 0, -1) * float4(-1, -1, -1, 1) ;    
    return qmul(r_c, qmul(float4(v, 0), r_c )).xyz ;
}

float4 vector_to_clip(float3 v,float3 pos)
{
    float3 modelPos = pos + v;
    return UnityObjectToClipPos(float4(modelPos,1));
}

float4 Billboarding_vector_to_clip(float3 v,float3 pos)
{  
    float3 center = pos;
    float3 modelPos = pos + v;
    float3 viewer = mul(unity_WorldToObject,float4(_WorldSpaceCameraPos, 1));    
    float3 centerOffs = v;
    float3 normalDir = viewer - center;
    normalDir.y = normalDir.y * _VerticalBillboarding;
	normalDir = normalize(normalDir);

    fixed upLeft = step(abs(normalDir.y),0.99);
    float3 upDir =(1-upLeft) *  float3(0, 0, 1)  + upLeft *  float3(0, 1, 0);
    float3 rightDir = normalize(cross(upDir, normalDir));
    upDir = normalize(cross(normalDir, rightDir));

    float3 localPos = center + rightDir * centerOffs.x + upDir * centerOffs.y + normalDir* centerOffs.z  ;

    return UnityObjectToClipPos(float4(localPos,1));
}

// 生成缩放比例
float calc_scale(float2 uv, float time01)
{
    float s = lerp(_ScaleMin, _ScaleMax, nrand(uv, 14));
    // Linear scaling animation with life.
    // (0, 0) -> (0.1, 1) -> (0.9, 1) -> (1, 0)
    return s * min(1.0, 5.0 - abs(5.0 - time01 * 10));
}

// 颜色
float4 calc_color(float2 uv, float time01)
{
#if _COLORMODE_RANDOM
    return lerp(_Color, _Color2, nrand(uv, 15));
#else
    return lerp(_Color, _Color2, (1.0 - time01) * _ColorMode);
#endif
}
