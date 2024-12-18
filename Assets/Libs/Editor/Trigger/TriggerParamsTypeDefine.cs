

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TriggerParamsTypeDefine
{
    public static string[] fire_types = new string[] { "enter", "exit", "stay", "event", "click", "enter_scene", "phase_complete" };
    public static string[] fire_types_chinese = new string[] { "进入碰撞体", "退出碰撞体", "在碰撞体内", "其他对象发出的事件", "点击", "场景初始化完成", "波次结束" };
    public static Dictionary<string, Dictionary<string, string[]>> TriggerParamsType = new Dictionary<string, Dictionary<string, string[]>>();

    static TriggerParamsTypeDefine()
    {

        TriggerParamsType["物件类"] = new Dictionary<string, string[]>();
        //TriggerParamsType["物件类"].Add("show_item",new string[] { "显示物件","fire_times","int","触发次数","item_id", "int","物件ID" ,"position","vector3","显示位置"});
        //TriggerParamsType["物件类"].Add("hide_item",new string[] {  "移除物件", "fire_times", "int", "触发次数", "item_id", "int","物件ID" });
        TriggerParamsType["物件类"].Add("show_object", new string[] { "显示对象", "fire_times", "int", "触发次数", "game_object_name", "game_object", "物件路径", "position", "vector3", "物件位置", "phase_id", "int", "波次ID" });
        TriggerParamsType["物件类"].Add("hide_object", new string[] { "隐藏对象", "fire_times", "int", "触发次数", "game_object_name", "game_object", "物件路径", "phase_id", "int", "波次ID" });


        TriggerParamsType["场景AOI类"] = new Dictionary<string, string[]>();
        TriggerParamsType["场景AOI类"].Add("show_aoi", new string[] { "显示AOI", "fire_times", "int", "触发次数", "aoi_group_id", "string", "AOI组ID(逗号隔开多个)", "delay", "int", "延时执行(秒)", "findpath_index", "int", "编号（用于场景内就近寻路）", "phase_id", "int", "波次ID" });
        TriggerParamsType["场景AOI类"].Add("remove_aoi", new string[] { "移除AOI", "fire_times", "int", "触发次数", "aoi_group_id", "string", "AOI组ID(逗号隔开多个)", "delay", "int", "延时执行(秒)", "phase_id", "int", "波次ID" });
        TriggerParamsType["场景AOI类"].Add("request_phase", new string[] { "下一波次", "fire_times", "int", "触发次数", "delay", "int", "延时执行(秒)", "phase_id", "int", "波次ID" });


        TriggerParamsType["界面类"] = new Dictionary<string, string[]>();
        TriggerParamsType["界面类"].Add("show_move_bar", new string[] { "显示界面", "fire_times", "int", "触发次数", "phase_id", "int", "波次ID" });
        TriggerParamsType["界面类"].Add("hide_move_bar", new string[] { "隐藏界面", "fire_times", "int", "触发次数", "phase_id", "int", "波次ID" });

        TriggerParamsType["相机寻迹类"] = new Dictionary<string, string[]>();
        TriggerParamsType["相机寻迹类"].Add("camera_trace", new string[] { "相机寻迹", "fire_times", "int", "触发次数", "transition_speed", "float", "相机切换速度", "look_type", "bool", "注视模式", "node_type", "bool", "是否起点", "phase_id", "int", "波次ID" });
        TriggerParamsType["相机寻迹类"].Add("stop_camera_trace", new string[] { "停止相机寻迹", "fire_times", "int", "触发次数", "phase_id", "int", "波次ID" });

        TriggerParamsType["场景类"] = new Dictionary<string, string[]>();
        TriggerParamsType["场景类"].Add("change_scene", new string[] { "切换场景", "fire_times", "int", "触发次数", "scene_id", "int", "目标场景ID(0表示退出到主城)", "phase_id", "int", "波次ID" });
        TriggerParamsType["场景类"].Add("move_to", new string[] { "移动到目的地", "fire_times", "int", "触发次数", "dst_pos", "vector2", "目的地", "phase_id", "int", "波次ID" });
        TriggerParamsType["场景类"].Add("tollgate_scene", new string[] { "切换到关卡场景", "fire_times", "int", "触发次数", "is_jingying", "bool", "是否是精英章节", "phase_id", "int", "波次ID" });
        TriggerParamsType["场景类"].Add("jiguan", new string[] { "记录对象进出", "fire_times", "int", "触发次数", "dianti_name", "string", "电梯名字", "phase_id", "int", "波次ID" });
        TriggerParamsType["场景类"].Add("xunlu", new string[] { "用于寻路", "fire_times", "int", "触发次数", "phase_id", "int", "波次ID" });
        TriggerParamsType["场景类"].Add("juqing", new string[] { "开始剧情", "fire_times", "int", "触发次数", "juqing_name", "string", "剧情名字", "juqing_length", "int", "剧情长度(ms)", "inside_trigger", "string", "被包裹的trigger名字", "phase_id", "int", "波次ID" });

        //TriggerParamsType[""].Add(new string[] { "show_mirror","显示镜面", "fire_times", "int", "触发次数", "week_distane", "float","行走距离", "mirror_name", "object_name","镜面对象", "camera_distance", "float","相机距离", "player_end_position","vector3","角色最后位置", "next_trigger", "object_name","下一个触发器" });

        TriggerParamsType["声音类"] = new Dictionary<string, string[]>();
        TriggerParamsType["声音类"].Add("play_2d_sound", new string[] { "播放2d声音", "fire_times", "int", "触发次数", "music_event", "string", "声音事件", "phase_id", "int", "波次ID" });
        TriggerParamsType["声音类"].Add("stop_2d_sound", new string[] { "停止2d声音", "fire_times", "int", "触发次数", "music_event", "string", "声音事件", "phase_id", "int", "波次ID" });
        TriggerParamsType["声音类"].Add("play_3d_sound", new string[] { "播放3d声音", "fire_times", "int", "触发次数", "music_event", "string", "声音事件", "game_object", "object_name", "声音挂接对象", "phase_id", "int", "波次ID" });
        TriggerParamsType["声音类"].Add("stop_3d_sound", new string[] { "停止3d声音", "fire_times", "int", "触发次数", "game_object", "object_name", "声音挂接对象", "phase_id", "int", "波次ID" });
        TriggerParamsType["声音类"].Add("stop_all_3d_sound", new string[] { "停止所有3d声音", "fire_times", "int", "触发次数", "phase_id", "int", "波次ID" });
        TriggerParamsType["声音类"].Add("stop_all_sound", new string[] { "停止所有声音", "fire_times", "int", "触发次数", "phase_id", "int", "波次ID" });
        TriggerParamsType["声音类"].Add("play_music", new string[] { "播放背景音乐", "fire_times", "int", "触发次数", "music_event", "string", "声音事件", "phase_id", "int", "波次ID" });
        TriggerParamsType["声音类"].Add("stop_music", new string[] { "停止背景音乐", "fire_times", "int", "触发次数", "music_event", "string", "声音事件", "phase_id", "int", "波次ID" });

        TriggerParamsType["动画类"] = new Dictionary<string, string[]>();
        TriggerParamsType["动画类"].Add("play_aniamtion", new string[] { "播放动画", "fire_times", "int", "触发次数", "object_name", "game_object", "挂接对象", "animationclip_name", "object_name", "动画文件", "phase_id", "int", "波次ID" });
        TriggerParamsType["动画类"].Add("play_aniamtion_sample", new string[] { "播放动画(可重叠)", "fire_times", "int", "触发次数", "object_name", "game_object", "挂接对象", "animationclip_name", "object_name", "动画文件", "next_trigger", "trigger_object", "播放完毕触发的触发器", "phase_id", "int", "波次ID" });
        //TriggerParamsType["动画类"].Add(new string[] { "sample_animation","动画放样", "fire_times", "int", "触发次数", "object_name", "object_name", "挂接对象", "animationclip_name", "object_name", "动画文件", "time","float","时间点"});

        TriggerParamsType["天气系统类"] = new Dictionary<string, string[]>();
        TriggerParamsType["天气系统类"].Add("play_loop_weather", new string[] {
            "播放场景循环动画",
            "fire_times", "int", "触发次数" ,
            "nomal_animation", "object_name", "环境动画文件",
            "lamp_on_animation", "object_name", "开灯状态动画文件",
            "lamp_off_animation", "object_name", "关灯状态动画文件",
            "phase_id", "int", "波次ID"});
        TriggerParamsType["天气系统类"].Add("play_plot_weather", new string[] {
            "播放剧情动画",
            "fire_times", "int", "触发次数" ,
            "nomal_animation", "object_name", "环境动画文件",
            "lamp_on_animation", "object_name", "开灯状态动画文件",
            "lamp_off_animation", "object_name", "关灯状态动画文件",
            "is_return_loop_animation", "bool", "是否恢复场景循环动画",
            "phase_id", "int", "波次ID"});
        TriggerParamsType["天气系统类"].Add("lamp_on", new string[] { "开灯", "fire_times", "int", "触发次数", "phase_id", "int", "波次ID" });
        TriggerParamsType["天气系统类"].Add("lamp_off", new string[] { "关灯", "fire_times", "int", "触发次数", "phase_id", "int", "波次ID" });
        TriggerParamsType["天气系统类"].Add("twinkle_lamp", new string[] { "闪灯", "fire_times", "int", "触发次数", "twinkle_animation", "object_name", "动画文件", "phase_id", "int", "波次ID" });

        TriggerParamsType["主角控制类"] = new Dictionary<string, string[]>();
        TriggerParamsType["主角控制类"].Add("player_run_enable", new string[] { "角色奔跑", "enable", "bool", "是否奔跑", "fire_times", "int", "触发次数", "phase_id", "int", "波次ID" });
    }
}