using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace GameSupport
{
    public  class GameGlobalVars
    {
        //光晕开关
        public static bool SunFlarLenEnabled = true;
        //草
        public static float Grass_RenderRatio = 1.0f;//0.0---1.0f. 值越小，显示数量越小. 0=不显示
        //雨帘
        public static bool RainDownfallEnabled = true;
        //雨花
        public static bool RainSplashEnabled = true;
        //雨雾
        public static bool RainSmokeEnabled = true;
        //闪电
        public static bool LightningEnabled = true;

        //是否开启了TAA效果
        public static bool IsOpenedTAA = false;

        //开启水面实时反射，起始画质（>=高画质) //QualityLevel： 0低，1中，2高。。。
        public static int WaterRealTimeReflect_QualityLevel = 2;
        //开启水面交互，起始画质（>=高画质)
        public static int WaterIntract_QualityLevel = 2;
        //开启足迹交互，起始画质（>=高画质)
        public static int TractIntract_QualityLevel = 2;
    }
}