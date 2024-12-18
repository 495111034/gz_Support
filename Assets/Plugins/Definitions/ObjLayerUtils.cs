using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.CoreAudioApi;
using UnityEngine;

public class ObjTag
{
    public const string CheckShader = "CheckShader";
    public const string ExportPrefab = "ExportPrefab";
}
public enum ObjLayer
{
    // 系统保留
    Default = 0,
    TransParentFX = 1,
    IgnoreRaycast = 2,      //场景不重要物体
    Water = 4,              //水、玻璃等物体
    UI = 5,

    //以下是自定义
    Player = 8,         //角色
    Scenario = 9,       //剧情对象
    Trigger = 10,       //触发器
    SceneBaseObj = 11,  //场景基本组件
    Monster = 12,       //怪物
    UIEffect = 13,      //UI特效
    Terrain = 14,       //地面（确定高度）
    RoleEffect = 15,       //角色特效
    SceneEffect = 16,       //场景特效
    NPC = 17,               //NPC
    Weapon = 18,            //角色挂件
    CameraCollider = 19,    //相机碰撞体
    Item = 20,              //动态小物体、掉落物、采集物 
    SceneInstance = 21,     //场景中需要做GPUInstance的小物
    BackGround = 22,        //背景、天空等
    FightEffect = 23,       //战斗特效
    MainRole=24,            //主角
    Camera = 25,            //相机
    Shadow = 26,            //阴影
    Pet = 27,               //宠物
    MainPanel = 28,         //主界面   
    Hidden = 31,            //隐藏（所有相机不可见）
}

public enum ObjLayerMask
{
    Default = 1 << ObjLayer.Default,
    TransParentFX = 1 << ObjLayer.TransParentFX,
    IgnoreRaycast = 1 << ObjLayer.IgnoreRaycast,
    Water = 1 << ObjLayer.Water,
    UI = 1 << ObjLayer.UI,

    Player = 1 << ObjLayer.Player,
    Scenario = 1 << ObjLayer.Scenario,
    Trigger = 1 << ObjLayer.Trigger,
    SceneBaseObj = 1 << ObjLayer.SceneBaseObj,
    Monster = 1 << ObjLayer.Monster,
    UIEffect = 1 << ObjLayer.UIEffect,
    Terrain = 1 << ObjLayer.Terrain,
    RoleEffect = 1 << ObjLayer.RoleEffect,
    SceneEffect = 1 << ObjLayer.SceneEffect,
    NPC = 1 << ObjLayer.NPC,
    Weapon = 1 << ObjLayer.Weapon,
    CameraCollider = 1 << ObjLayer.CameraCollider,
    Item = 1 << ObjLayer.Item,
    SceneInstance = 1 << ObjLayer.SceneInstance,
    BackGround = 1 << ObjLayer.BackGround,
    FightEffect = 1 << ObjLayer.FightEffect,
    MainRole = 1 << ObjLayer.MainRole,
    Shadow = 1 << ObjLayer.Shadow,
    Pet = 1 << ObjLayer.Pet,
    Camera = 1 << ObjLayer.Camera,
    Hidden = 1 << ObjLayer.Hidden,
    MainPanel = 1 << ObjLayer.MainPanel,

    AllObject = Player | Weapon | NPC | Monster | Item | MainRole | Shadow | Pet,
    AllRole = AllObject | RoleEffect,                                                      //角色相关
    ViewAll = ~(Trigger | UI | UIEffect | CameraCollider | Hidden | Default | Camera),     //所有物体
    ViewAllSample = ~(Trigger | UI | UIEffect | CameraCollider | Hidden | Default | Camera | IgnoreRaycast | Water | SceneEffect),     //低配视野
    MirroRef = ~(Trigger | UI | UIEffect | CameraCollider | Hidden | Default | Camera | Water | Shadow),            //镜面，不包括水
    MirroRef2 = ~(Trigger | UI | UIEffect | CameraCollider | Hidden | Default | Camera | Water | Shadow | RoleEffect | SceneEffect | FightEffect), //镜面2，不包括特效
    NotRole = ~(AllRole | Trigger |  UI | UIEffect | CameraCollider | Hidden | Camera | Shadow | Pet),              //所有非角色物体    
    //HeightTest = SceneBaseObj | Terrain ,                                           //确定高度
    HeightTest = Terrain,                                           //确定高度
    ObjHitTest = Player | NPC | Monster | Item| Trigger | Pet,                            //点击目标   
    UILayer = UI| UIEffect,
    MainPanelLayer = UI| MainPanel,//特殊处理主界面的特效，因为其它3D界面是镂空的，特效会穿透
    CasterShadow = AllObject,                                                      //投射阴影
    ReceiveShadow = Terrain | Water | SceneBaseObj,                                 //接收阴影
    Scene = SceneBaseObj | Terrain | SceneInstance | BackGround | IgnoreRaycast | Water | TransParentFX,
    UI3DModel = (UI | Player | RoleEffect | Item | BackGround),


}


public class ObjLayerUtils
{
}

public static class UIDefs
{
    // UI相机参数 ,改为static类型,允许的热更代码中修改此值
    public static int CAMERA_DEPTH_MASK = 10;           // 遮罩相机深度
    public static int CAMERA_DEPTH_MAIN = 50;           // 主界面相机深度
    public static int CAMERA_DEPTH_UI = 53;             // 一般相机深度
    public static int CAMERA_DEPTH_2D = 53;             // 一般相机深度
    public static int CAMERA_DEPTH_3D = 52;             // 一般相机深度
    public static int CAMERA_DEPTH_TOP = 57;            // 顶层相机深度
    public static int CAMERA_DEPTH_HEADINFO = 49;       // 名字层相机深度
    public static int CAMERA_DEPTH_TALK = 56;           // 对话界面相机深度
    public static int CAMERA_DEPTH_TALK_MASK = 54;      // 对话遮罩界面相机深度
    public static int CAMERA_DEPTH_JUQING = 55;         // 剧情界面相机深度

    public static float CAMERA_NEAR = 10f;
    public static float CAMERA_FAR = 3000f;
    public static float CAMERA_SIZE = 1f;
    public static float PANEL_DISTANCE = 100f;      // panel 之间的距离, 总共可显示的 panel 个数为 CAMERA_FAR/PANEL_DISTANCE

}

