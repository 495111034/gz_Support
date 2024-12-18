#ifndef WEATHER_COMMON
#define WEATHER_COMMON

#define IS_EQUAL(x, a) abs(x - a) < 0.00001 
#define IS_EQUAL_HALF(x, a) abs(x - a) < 0.00001h

#define FOG_INPUT
#define FOG_INPUT_V2F(idx1) 
#define FOG_VERTEX(o, worldPos, weatherIsOpening)
#define FOG_PIXEL(i, color, weatherIsOpening)
#define SKYBOX_FOG(i, color, weatherIsOpening)
#define FOG_INPUT_PARAM_W(i) 1
#define SKY_VOLUMECLOUD_FOG_PIXEL(color, weatherIsOpening)

float3 _SkyboxLightDir;

#endif //WEATHER_COMMON