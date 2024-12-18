

//游戏中传入的数据
half4x4 _GameInfos;
//选中目标的光圈图
sampler2D _TargetSelectedTexture;

sampler2D _MainroleCircleTex;

//地图阻挡数据
sampler2D _SceneMapMask;
//地图尺寸
half4 _SceneSize;
//箭头图片
sampler2D _SceneDirTex;

//half __ambient_scale;


//uniform float4 _TimeEditor;

    //主角光圈
    //以当前场景实时光的50%加强主角周围漫反射，用于体现光照纹理无法表现的法线凹凸
    //tangentNormal为法线采样数据
    fixed3 MainRoleLightColor(half3 rolePos,half3 albedo,half3 lightDir, half3 tangentNormal)
    {        
        fixed disToCharacter = length(rolePos);


        //float val = clamp(1.0-disToCharacter*0.3,0.,1.);
        //val = pow(val,0.3);
        //fixed3 mainRoleColor = val * fixed3(1,0,0);
        //mainRoleColor = mainRoleColor *  max(0, dot(tangentNormal, lightDir)) * val;
        //return mainRoleColor;

        //fixed _FollowDamping = 2.5; 
        fixed _FollowEnd = 10;
        
        fixed swif = step(disToCharacter, _FollowEnd );                               //a<=b为1,否则为0  , radus为光圈大小
        //以目标位置的xz为中心(0.5,0.5)、以光圈直径为max(1,1)来生成UV，用于光圈纹理采样
        half2 targetUV =  (half2(disToCharacter,disToCharacter) + half2(_FollowEnd,_FollowEnd))  / half2(_FollowEnd * 2,_FollowEnd * 2);
        fixed followDisRate =  tex2D(_MainroleCircleTex, targetUV).r * swif;   

        //fixed inCircle = step(disToCharacter, _FollowEnd);
        //fixed followDisRate = inCircle*((_FollowDamping-1)*disToCharacter/_FollowEnd+1) + (1-inCircle)*(_FollowEnd*_FollowEnd/(disToCharacter*disToCharacter))*_FollowDamping;

        ////half followDisRate = clamp(cos(  disToCharacterdist  /  UNITY_PI /( _FollowEnd / 2 ) ),0,1);     //线性插值，cos(dist / PI / f / 2) ，f为光圈大小

        //fixed3 mainRoleColor = followDisRate * fixed3(1,1,1) ;
        fixed3 mainRoleColor = fixed3(clamp(followDisRate * albedo.r * 1.5 ,0,1),clamp(followDisRate * albedo.g * 1.5,0,1),clamp(followDisRate * albedo.b * 1.5,0,1)); 
        fixed3 mainRoleColor2 = fixed3(clamp(followDisRate * albedo.r ,0,1),clamp(followDisRate * albedo.g ,0,1),clamp(followDisRate * albedo.b ,0,1)); 
		mainRoleColor = (mainRoleColor * max(0, dot(tangentNormal, lightDir)) + mainRoleColor2) * 0.4;
        
        return 0;
       

    }

    //选中目标的光圈
    fixed3 SelectedTargetColor(half3 targetPos)
    {
        fixed3 color = fixed3(1,1,1);
        color *= 0;//(EnemyColor * IsEnemy + OursideColor * IsOurside + MiddleColor * IsMiddle + BossColor * IsBoss);
        return color;
    }


//获取当前的环境光
inline fixed4 GetAmbient()
{
    return UNITY_LIGHTMODEL_AMBIENT;// * __ambient_scale;
}

//绘制阻挡数据
fixed4 SceneColor(fixed4 oldColor,half3 worldPos)
{ 
    #if __MAPDEBUG
        float2 worldUV = float2(((worldPos.x ) / _SceneSize.x )   ,  ((worldPos.z ) / _SceneSize.y)   );    
        float4 mapColor = tex2D(_SceneMapMask, worldUV); 
    
        float2 jiantouUV = frac(float2(worldPos.x,worldPos.z)) ;
        float angle = (mapColor.b  * 180) / 3.14159;

        float2 pivot = float2(0.5, 0.5);         //中心点

        float sin1 = sin(angle);
		float cos1 = cos(angle);
		float2x2 rot1 = float2x2(cos1, -sin1, sin1, cos1);
        jiantouUV = mul(rot1,jiantouUV - pivot) + pivot; 

        fixed jiantouColor = tex2D(_SceneDirTex,jiantouUV).a * mapColor.g;
        return fixed4( oldColor.rgb + fixed3(1,0,0) * mapColor.r + fixed3(0,jiantouColor,0) , oldColor.a);
    #else 
        return oldColor;  
    #endif
    
}

//以下代码为解码法线
//与unity标准法线解码不同的是：增加了alpha通道，通过此通道的数据调整Z方向的数值

fixed3 MyUnpackNormalmapRGorAG(fixed4 packednormal,fixed xyScale,fixed zScale)
{  
    fixed3 normal;
    normal.xy = (packednormal.xy * 2 - 1) * xyScale;
    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy))) +  packednormal.w * zScale;
    return normal;
}

fixed3 MyUnpackNormalmapRG(fixed4 packednormal,fixed xyScale,fixed zScale)
{  
    fixed3 normal = UnpackNormal(packednormal);
    normal.xy *= xyScale;
    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy))) +  packednormal.w * zScale;
    return normal;
}

inline fixed3 MyUnpackNormal(fixed4 packednormal, fixed xyScale, fixed zScale)
{
    return MyUnpackNormalmapRGorAG(packednormal,xyScale,zScale);
}
//以上代码为解码法线
