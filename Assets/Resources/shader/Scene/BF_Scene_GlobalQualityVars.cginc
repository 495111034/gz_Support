#ifndef BF_SCENE_GLOBALQUALITYVARS_INCLUDED
#define BF_SCENE_GLOBALQUALITYVARS_INCLUDED

///测试用
#define DEBUG 0

#define _G_IN_RUNTIME 1

#if DEBUG
    #define _G_GAME_QUALITY_HIGH 1

    //通用
    #define _G_USE_NORMAL 1
    #define _G_USE_SPECULAR 1
    #define _G_USE_CUSTOMREALTIMESHADOW 1

    //雾效
    #define _G_USE_FOG 1
    #define _G_HEIGHTFOG_FALLOFF 1
    #define _G_HEIGHTFOG_DIRECTLIGHT 1
    #define _G_HEIGHTFOG_NOISE3D 1

    //地形
    #define _G_TERRAIN_CAMERALIGHT 1
    #define _G_TERRAIN_RAIN 1
    #define _G_TERRAIN_HEIGHTBLEND 1
    #define _G_TERRAIN_NORMAL 1
    #define _G_TERRAIN_METALLICTEX 1
    #define _G_TERRAIN_REFLECT 1

    //水
    #define _G_WATER_REFRECT 1
    #define _G_WATER_FOAM 1
    #define _G_WATER_NOISE 1
    #define _G_WATER_CAMERALIGHT 1
    #define _G_WATER_USEDEPTHTEX 1

    //uv动画
    #define _G_UVANI_LODFADE 1
    #define _G_UVANI_WAVE 1
    #define _G_UVANI_SECONDTEX 1
    #define _G_UVANI_NOISE 1
    #define _G_UVANI_INTERACT 1
    #define _G_UVANI_EMISSION 1
    #define _G_UVANI_TRANSLUCENCY 1
    #define _G_UVANI_CARTOONLAYER 1
    #define _G_UVANI_FILLLIGHT 1
    
    //ND
    #define _G_ND_RAIN 1
    #define _G_ND_EXTENDMAP 1
    #define _G_ND_HOLLOW 1
    #define _G_ND_EMISSION 1
    #define _G_ND_REFLECT 1
    #define _G_ND_MIRRORREF 1
    #define _G_ND_COVERNORMAL 1
    #define _G_ND_CAMERALIGHT 1
    #define _G_ND_FADE 1
    #define _G_ND_ICE 1
    #define _G_ND_LODFADE 1
    #define _G_ND_DUST 1

    //Actor
    #define _G_ACTOR_HOLLOW 1
    #define _G_ACTOR_DISSOLVE 1
    #define _G_ACTOR_REFLECT 1
    #define _G_ACTOR_CAMERALIGHT 1
    #define _G_ACTOR_EMISSION 1
    #define _G_ACTOR_RIM 1
    #define _G_ACTOR_FLOWLIGHT 1
    #define _G_ACTOR_RIM2 1
    #define _G_ACTOR_FRESNELRIM 1
    #define _G_ACTOR_ROLLTEX 1

    //Hair
    #define _G_HAIR_CAMERALIGHT 1
    #define _G_HAIR_REFLECT 1
    #define _G_HAIR_DISSOLVE 1
    #define _G_HAIR_RIM 1
    #define _G_HAIR_FRESNELRIM 1
    #define _G_HAIR_ANISOTROPICRENDER 1
#endif
///

#if !defined(_G_GAME_QUALITY_LOW) && !defined(_G_GAME_QUALITY_MIDDLE) && !defined(_G_GAME_QUALITY_HIGH) && !defined(_G_GAME_QUALITY_VERYLOW)
    #define _G_GAME_QUALITY_LOW 1
#endif

//global quality setting #pragma multi_compile _G_GAME_QUALITY_LOW _G_GAME_QUALITY_MIDDLE _G_GAME_QUALITY_HIGH
#if defined(_G_GAME_QUALITY_VERYLOW)
    #define _G_SHADER_QUALITY 4
    #define _G_RENDERMODEL_LOW 1
#elif defined(_G_GAME_QUALITY_LOW)
    #define _G_SHADER_QUALITY 3
    #define _G_RENDERMODEL_LOW 1
#elif defined(_G_GAME_QUALITY_MIDDLE)
    #define _G_SHADER_QUALITY 2
    #define _G_RENDERMODEL_MIDDLE 1
#elif defined(_G_GAME_QUALITY_HIGH)
    #define _G_SHADER_QUALITY 1
    #define _G_RENDERMODEL_HIGH 1
#endif


//超低画质
#if _G_SHADER_QUALITY <=4
    #define _G_SETTING_VERYLOW 1
    // #define _G_SETTING_UNLIT 1
    
    //地形
    #define _G_TERRAIN_HEIGHTBLEND 1

    //uv动画
    #define _G_UVANI_EMISSION 1

    //Actor
    #define _G_ACTOR_RIM2 1
    #define _G_ACTOR_FRESNELRIM 1
    #define _G_ACTOR_EMISSION 1

    //Hair
    #define _G_HAIR_RIM 1

    //ND
    #define _G_ND_FADE 1
    #define _G_ND_METALLICCLAMP 1                        ///低画质和超低画质没有Specular部分，只有Diffuse部分，需要对金属度进行截断防止变黑
#endif
//低画质
#if _G_SHADER_QUALITY <=3

    #undef _G_SETTING_VERYLOW
    // #undef _G_SETTING_UNLIT 

    #define _G_USE_FOG 1

    //地形
    #define _G_TERRAIN_HEIGHTBLEND 1


    //uv动画
    #define _G_UVANI_CARTOONLAYER 1
    #define _G_UVANI_FILLLIGHT 1

    //水
    //#define _G_WATER_REFRECT 1

    //Actor
    #define _G_ACTOR_EMISSION 1

    //Hair

    //ND
    #define _G_ND_MIRRORREF 1
#endif

//中画质
#if _G_SHADER_QUALITY <=2

    //地形
    #define _G_TERRAIN_NORMAL 1

    //水
    #define _G_WATER_FOAM 1
    #define _G_WATER_NOISE 1

    //uv动画
    #define _G_UVANI_WAVE 1

    //ND
    #define _G_ND_EXTENDMAP 1
    #define _G_ND_HOLLOW 1
    #define _G_ND_ICE 1
    #define _G_ND_EMISSION 1
#endif

//高画质
#if _G_SHADER_QUALITY <=1

    #define _G_USE_CUSTOMREALTIMESHADOW 1
    #define _G_USE_SPECULAR 1
    #define _G_USE_NORMAL 1

    //地形
    #define _G_TERRAIN_CAMERALIGHT 1
    #define _G_TERRAIN_METALLICTEX 1
    #define _G_TERRAIN_REFLECT 1

    //水
    #define _G_WATER_CAMERALIGHT 1
    #define _G_WATER_USEDEPTHTEX 1
    #define _G_WATER_REFRECT 1

    //uv动画
    #define _G_UVANI_LODFADE 1
    #define _G_UVANI_SECONDTEX 1
    #define _G_UVANI_NOISE 1
    #define _G_UVANI_INTERACT 1
    #define _G_UVANI_TRANSLUCENCY 1
    #define _G_UVANI_EMISSIONMAP 1

    //ND
    #undef _G_ND_METALLICCLAMP
    #define _G_ND_REFLECT 1
    #define _G_ND_COVERNORMAL 1
    #define _G_ND_CAMERALIGHT 1
    #define _G_ND_LODFADE 1
    #define _G_ND_EMISSIONMAP 1
    #define _G_ND_DUST 1

    //Actor
    #define _G_ACTOR_HOLLOW 1
    #define _G_ACTOR_DISSOLVE 1
    #define _G_ACTOR_REFLECT 1
    #define _G_ACTOR_CAMERALIGHT 1
    #define _G_ACTOR_FLOWLIGHT 1
    #define _G_ACTOR_ROLLTEX 1
    #define _G_ACTOR_RIM 1
    #define _G_ACTOR_EMISSION 1

    //Hair
    #define _G_HAIR_CAMERALIGHT 1
    #define _G_HAIR_REFLECT 1
    #define _G_HAIR_DISSOLVE 1
    #define _G_HAIR_FRESNELRIM 1
    #define _G_HAIR_ANISOTROPICRENDER 1

    //Fog
    #define _G_HEIGHTFOG_FALLOFF 1
    #define _G_HEIGHTFOG_DIRECTLIGHT 1
    #define _G_HEIGHTFOG_NOISE3D 1
#endif


///SpecialSetting
#if _G_IS_ND
    #if _G_ND_REFLECT
        #define _G_BRDF_REFLECT 1
    #endif
#endif

#if _G_IS_TERRAIN
    #if _G_TERRAIN_REFLECT
        #define _G_BRDF_REFLECT 1
    #endif
#endif


#if _G_USE_SPECULAR
    #define _G_BRDF_SPECULAR 1
#endif

// #if defined(_G_GAME_QUALITY_LOW)
//     #define _G_ND_LOWESTRENDERING 1
// #endif

#endif