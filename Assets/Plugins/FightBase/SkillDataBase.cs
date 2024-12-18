using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Entity
{
    [System.Serializable]
    public class SkillEffectBase
    {
        public float effect_progress;       //时间点（0-1，总长度为timeline长度）
        public string effect_res_id;
        public SkillTargetType effect_parent_type;
        public Vector3 effect_offset;        //特效离挂接位置的偏移量
        public Vector3 effect_rotation;     //特效的旋转
        public float effect_scale;          //缩放值
        public string effect_parent_name;   //特效在角色身上的挂接点

        public float effective_time = 0f;   //有效时长

#if UNITY_EDITOR
        public GameObject EffectGameObject; //编辑时要指定特效实列，序列化时读取prefab赋值到effect_res_id
        //public GameObject effectPosObject;  //编辑时可直接指定场景中的位置，序列化时分析并赋值到effect_parent_type与effect_parent_name
        public void SaveData()
        {
            if (EffectGameObject)
            {
                var prefabType = PrefabUtility.GetPrefabType(EffectGameObject);
                if (prefabType == PrefabType.PrefabInstance)
                {                 
                    UnityEngine.Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(EffectGameObject);
                    effect_res_id = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(parentObject)).ToLower();
                }
                else if(prefabType == PrefabType.Prefab)
                {
                    effect_res_id = EffectGameObject.name.ToLower();
                }
                else
                {
                    string errMsg = $"{EffectGameObject.name} 的特效必须为prefab";
                    Debug.LogError(errMsg);
                    UnityEditor.EditorUtility.DisplayDialog("错误", errMsg, "知道了");
                    return;
                }
            }
        }
#endif
    }

    [System.Serializable]
    public class SkillUIEffectBase
    {
        public float effect_progress;           //时间点（0-1，总长度为timeline长度）
        public Vector2 effect_offset;           //特效离挂接位置的偏移量
        public Vector3 effect_rotation;
        public SkillTargetType effectTarget;    //特效位置

        public float effective_time = 0f;       //有效时长

        public string effect_res_id;
        public string uiItemPath;

#if UNITY_EDITOR
        public GameObject EffectGameObject;     //编辑时要指定特效实列，序列化时读取prefab赋值到effect_res_id
        public void SaveData()
        {
            if(EffectGameObject)
            {
                var prefabType = PrefabUtility.GetPrefabType(EffectGameObject);
                if (prefabType == PrefabType.PrefabInstance)
                {
                    UnityEngine.Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(EffectGameObject);
                    effect_res_id = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(parentObject)).ToLower();
                }
                else if (prefabType == PrefabType.Prefab)
                {
                    effect_res_id = EffectGameObject.name.ToLower();
                }
                else
                {
                    string errMsg = $"{EffectGameObject.name} 的特效必须为prefab";
                    Debug.LogError(errMsg);
                    UnityEditor.EditorUtility.DisplayDialog("错误", errMsg, "知道了");
                    return;
                }
            }
        }
#endif
    }

    [System.Serializable]
    public class TrajectoryEffectBase
    {
        public float effect_progress;                   //时间点（0-1，总长度为timeline长度）        
        public SkillTargetType effectTarget;            //目标类型
        public bool is_reverse;                         //是否反向运动
        public string effect_res_id;
        public Vector3 start_effect_offset;             //特效离发射位置的偏移量       
        public Vector3 end_effect_offset;               //特效离目标位置的偏移量
        public float total_time;                        //总运动时间
        public bool is_parabola;                         //抛物线
        public float start_up_speed;                    //上抛速度
        public float delay_destory;                     //延时消失时间

#if UNITY_EDITOR
        public GameObject EffectGameObject;             //编辑时要指定特效实列，序列化时读取prefab赋值到effect_res_id
        public void SaveData()
        {
            if (EffectGameObject)
            {
                var prefabType = PrefabUtility.GetPrefabType(EffectGameObject);
                if (prefabType == PrefabType.PrefabInstance)
                {
                    UnityEngine.Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(EffectGameObject);
                    effect_res_id = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(parentObject)).ToLower();
                }
                else if (prefabType == PrefabType.Prefab)
                {
                    effect_res_id = EffectGameObject.name.ToLower();
                }
                else
                {
                    string errMsg = $"{EffectGameObject.name} 的特效必须为prefab";
                    Debug.LogError(errMsg);
                    UnityEditor.EditorUtility.DisplayDialog("错误", errMsg, "知道了");
                    return;
                }
            }
        }
#endif
    }

    [System.Serializable]
    public class SkillSoundBase
    {
        public float effect_progress;           //时间点（0-1，总长度为timeline长度）
        public string sound_fmod_event;         //编辑时要指定声音文件，序列化时读取prefab赋值到sound_res_id
        public float effective_time = 0f;       //有效时长
    }

    [System.Serializable]
    public class SkillTargetEffectBase
    {
        public List<SkillSoundBase> target_sound_list;  //声音
        public List<SkillEffectBase> target_effect_list;    //特效

        public float target_anim_progress;    //动作时间点（0-1，总长度为timeline长度）
        public string target_anim;       //受击动作

        [Tooltip("多段连击，x为时间点，y边受击动作速率")]
        public List<Vector2> target_anim_progress_list; //受击时间点列表=>受击动作速率

        public string target_anim_name;

#if UNITY_EDITOR
        public void SaveData()
        {
            if (target_effect_list != null)
                foreach (var i in target_effect_list)
                    if (i != null) i.SaveData();
        }
#endif
    }

    [System.Serializable]
    public enum SkillOtherSpecialType
    {
        None = 0,
        SpecialTypeHide = 1,        //隐形
        HitFly = 2,                 //击飞
        ShockCameraOnce = 3,
        ShockCameraLoop = 4,
        HideUI = 5,
        HideAllRole = 6,
        SlowMotion = 7,                 //慢镜头
    }

    [System.Serializable]
    public class SkillOtherSpecialBase
    {
        public float start_time;    //开始时间（秒，从技能开始算起）
        public float effective_time;    //持续时长（秒）
        public SkillOtherSpecialType special_type = SkillOtherSpecialType.None;
        public string special_params;
    }

    [System.Serializable]
    public enum SkillTargetType
    {
        None = 0,
        Enemy = 1,          //敌方
        Our = 2,            //我方
        Self = 3,           //自己
        SceneCenter = 4,    //场景中心点
        EnemyCenter = 5,    //敌方中心点
        OurCenter = 6,      //我方中心点

        FighterPos = 7,     //施法者位置
        TargetPos = 8,      //受击者位置
    }

    [System.Serializable]
    public class SkillDataBase
    {
        public int SkillID;
        public string SkillTitle;

        public SkillTargetType targetType;                  //目标类型
        [Tooltip("是否需要面向受击者")]
        public bool needLookTarget;                         //是否需要面向受击者
        [Tooltip("是否需要受击者面向我")]
        public bool needTargetLookMe;                       //是否需要受击者面向我
        [Tooltip("是否群攻")]
        public bool IsGroupAttack = false;                  //是否群攻
        [Tooltip("是否需要跑/跳向受击者")]
        public bool IsRunToAttack = false;                  //是否需要跑向受击者
        [Tooltip("跑的动作，不填为run，填了则为跳，可以填3段，分别为jump_start,jump_loop,jump_end")]
        public string RunToAnimName = "";                   //跑的动作
        [Tooltip("跑/跳向受击者的时间")]
        public float TimeLineStartTime = 0f;                //跑的时间
        [Tooltip("跑/跳向受击者偏移量，x为角度，y为距离")]
        public Vector2 RunToAttackOffset = Vector2.zero;    //距离目标的偏移量
        [Tooltip("跑回时是否回头")]
        public bool IsRunBackTurnBack = true;               //跑回时是否回头
        [Tooltip("跑回的动作，不填为run，填了则为跳，可以填3段，分别为jump_start,jump_loop,jump_end")]
        public string RunBackAnimName = "";                 //跑回的动作
        [Tooltip("跑回的时间")]
        public float RunBackTime = 0f;                      //跑回的时间

#if UNITY_EDITOR
        [NonSerialized]
        public TimelineData before_timeLine_asset;    //前置阶段剧情
        [NonSerialized]
        public TimelineData fight_timeLine_asset;     //战斗阶段剧情（可重复）
        [NonSerialized]
        public TimelineData after_timeLine_asset;     //回退阶段剧情
#endif

        public List<SkillEffectBase> before_effect_list;    //前置阶段特效
        public List<SkillEffectBase> fight_effect_list;     //战斗阶段特效
        public List<SkillEffectBase> after_effect_list;     //回退阶段特效

        public List<SkillSoundBase> before_sound_list;      //前置阶段声音
        public List<SkillSoundBase> fight_sound_list;      //战斗阶段声音
        public List<SkillSoundBase> after_sound_list;       //回退阶段声音

        public SkillTargetEffectBase before_target_effect;  //前置阶段受击方效果
        public SkillTargetEffectBase fight_target_effect;   //战斗阶段受击方效果
        public SkillTargetEffectBase after_target_effect;   //后置阶段受击方效果

        public List<SkillUIEffectBase> before_ui_effect;    //前置阶段UI特效
        public List<SkillUIEffectBase> fight_ui_effect;    //战斗阶段UI特效
        public List<SkillUIEffectBase> after_ui_effect;    //后置阶段UI特效

        public List<SkillOtherSpecialBase> before_special_list;  //前置特殊效果
        public List<SkillOtherSpecialBase> fight_special_list;  //战斗特殊效果
        public List<SkillOtherSpecialBase> after_special_list;  //后置特殊效果

        public TrajectoryEffectBase before_trajectory_effect;   //前置阶段弹道特效
        public TrajectoryEffectBase fight_trajectory_effect;   //战斗阶段弹道特效
        public TrajectoryEffectBase after_trajectory_effect;   //战斗阶段弹道特效



        public string before_timeLine_str;    //前置阶段剧情
        public string fight_timeLine_str;     //战斗阶段剧情（可重复）
        public string after_timeLine_str;     //回退阶段剧情
#if UNITY_EDITOR
        //public GameObject targetObject;         //编辑时拖放目标，类型包括：敌方、己方、自己、战场中心点、敌方中心点、己方中心点，序列化时分析目标并赋值到targetType
        public void SaveData()
        {
            if (Application.isPlaying)
                return;
            if (before_effect_list != null)
                foreach (var i in before_effect_list)
                    if (i != null)
                        i.SaveData();
            if (fight_effect_list != null)
                foreach (var i in fight_effect_list)
                    if (i != null)
                        i.SaveData();
            if (after_effect_list != null)
                foreach (var i in after_effect_list)
                    if (i != null)
                        i.SaveData();
            if (before_target_effect != null) before_target_effect.SaveData();
            if (fight_target_effect != null) fight_target_effect.SaveData();
            if (after_target_effect != null) after_target_effect.SaveData();
            if (before_ui_effect != null) foreach (var i in before_ui_effect) if (i != null) i.SaveData();
            if (fight_ui_effect != null) foreach (var i in fight_ui_effect) if (i != null) i.SaveData();
            if (after_ui_effect != null) foreach (var i in after_ui_effect) if (i != null) i.SaveData();
            if (before_trajectory_effect != null) before_trajectory_effect.SaveData();
            if (fight_trajectory_effect != null) fight_trajectory_effect.SaveData();
            if (after_trajectory_effect != null) after_trajectory_effect.SaveData();
            if (before_timeLine_asset) before_timeLine_str = before_timeLine_asset.name.ToLower();
            else before_timeLine_str = "";
            if (fight_timeLine_asset) fight_timeLine_str = fight_timeLine_asset.name.ToLower();
            else fight_timeLine_str = "";
            if (after_timeLine_asset) after_timeLine_str = after_timeLine_asset.name.ToLower();
            else after_timeLine_str = "";
        }
#endif

    }
}