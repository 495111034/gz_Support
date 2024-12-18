using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// Trigger是高自由度的消息传送组件，在这里细分.目前用于UI的json传送
/// </summary>
public enum TriggerType
{
    UI = 0,                 //各种UI界面
    ZhenPing = 1,           //震屏
    BlackScreen = 2,        //黑屏
    PingBi = 3,             //屏蔽功能
    Skip = 4,               //显示“跳过”按钮（已弃用，合在屏蔽功能里了）
    SkyboxColor = 5,        //改变环境颜色
    Burn = 6,               //消融
    SetLockView = 7,        //设置lockview
    CameraBackToRole = 8,   //摄像机立即复位
    QiCheng = 9,            //骑乘
    BossShow = 10,          //BossShow界面
    PlaySceneAnim = 11,     //播放场景物件的动画
    GoToScene = 12,         //跳转场景
    TaskUI = 13,            //任务对话界面
    BurnReverse = 14,       //反消融
    TextOnebyOne = 15,      //逐字句显示文本
    Pause = 16,             //暂停-调整timeScale为0.1
}

public enum JuQingUIID
{
    JuQing = 0,
    MaoPao = 1,
    JieMian = 2,
    JieMianCall = 3,
}

public enum SpeakerType
{
    MainRole = 0,
    NPC = 1,
    Monster = 2
}

[System.Serializable]
public class JuQingUIInfo
{
    public JuQingUIID juQingUIID = JuQingUIID.JuQing;
    public int aoi_id = 0;//要冒泡的演员的aoi_id，0表示主角
    public SpeakerType speakerType = SpeakerType.MainRole;//屏幕下方说话的人的类型
    public int NPCorMonsterID = 0;//屏幕下方说话的人的ID，0表示主角
    public bool isRoleSpeak = false;
    public string dialogue = "";
}


[System.Serializable]
public class OneByOneStruct
{
    public string lang_id;
    //[Tooltip("文字显示间隔时间")]
    public float speed;
}


[System.Serializable]
public class TriggerTimelineClip: PlayableAsset
{
    [HideInInspector]
    [SerializeField]
    private TriggerPlayableData templete = new TriggerPlayableData();
    public TriggerPlayableData Templete { get { return templete; } set { templete = value; } }

    public GameObject TrackTargetObject { get; set; }
    public TimelineClip OwningClip { get; set; }

    [SerializeField] private string triggerContent;
    
    public string TriggerContent { get { return triggerContent; } set { triggerContent = value; } }

    [SerializeField]
    [HideInInspector]
    public TriggerType triggerType = TriggerType.UI;

    [SerializeField]
    [HideInInspector]
    public JuQingUIInfo uIData;

    //[SerializeField]
    //[HideInInspector]
    //public int creatByAoi;
    [SerializeField]
    [HideInInspector]
    public float startAlpha = 0;

    [SerializeField]
    [HideInInspector]
    public float endAlpha = 0;

    [SerializeField]
    [HideInInspector]
    public bool isCloseMainUI = true;//关闭主界面

    [SerializeField]
    [HideInInspector]
    public bool isShowSkipBtn = true;//跳过按钮

    [SerializeField]
    [HideInInspector]
    public bool actorHide = false;

    [SerializeField]
    [HideInInspector]
    public bool mainRolehide = false;

    [SerializeField]
    [HideInInspector]
    public bool effectHide = false;

    [SerializeField]
    [HideInInspector]
    public Color32 fromColor = new Color32(255,255,255,255);

    [SerializeField]
    [HideInInspector]
    public Color32 toColor = new Color32(255, 255, 255, 255);

    [SerializeField]
    [HideInInspector]
    public Color32 dlFromColor = new Color32(255, 255, 255, 255);

    [SerializeField]
    [HideInInspector]
    public Color32 dlToColor = new Color32(255, 255, 255, 255);

    [SerializeField]
    [HideInInspector]
    public Color32 fogFromColor = new Color32(255, 255, 255, 255);

    [SerializeField]
    [HideInInspector]
    public Color32 fogToColor = new Color32(255, 255, 255, 255);

    [SerializeField]
    [HideInInspector]
    public Color32 DLFromColor = new Color32(255, 255, 255, 255);

    [SerializeField]
    [HideInInspector]
    public Color32 DLToColor = new Color32(255, 255, 255, 255);

    [SerializeField]
    [HideInInspector]
    public bool isSetPos = false;

    [SerializeField]
    [HideInInspector]
    public Vector3 roleEndPos = Vector3.zero; //x,y,z

    [SerializeField]
    [HideInInspector]
    public string burnID;

    [SerializeField]
    [HideInInspector]
    public string burnParts;

    [SerializeField]
    [HideInInspector]
    public Vector3 lockViewSet = Vector3.zero;//废弃，由以下angle_y，angle_xz，distance代替

    [SerializeField]
    [HideInInspector]
    public float angle_y = 0;
    [SerializeField]
    [HideInInspector]
    public float angle_xz = 0;
    [SerializeField]
    [HideInInspector]
    public float distance = 0;

    [SerializeField]
    [HideInInspector]
    public int horseID = 0;

    [SerializeField]
    [HideInInspector]
    public string sceneObjectName;

    [SerializeField]
    [HideInInspector]
    public string sceneObjectAnim;

    /// <summary>
    /// 跳转场景
    /// </summary>
    [SerializeField]
    [HideInInspector]
    public int sceneID = 0;

    [SerializeField]
    [HideInInspector]
    public bool isSetDefaultPos;

    /// <summary>
    /// 跳转场景后的初始位置
    /// </summary>
    [SerializeField]
    [HideInInspector]
    public Vector2 toPos = Vector2.zero;

    [SerializeField]
    [HideInInspector]
    public bool showSkin;

    [SerializeField]
    [HideInInspector]
    public List<string> dios = new List<string>();

    [SerializeField]
    [HideInInspector]
    public List<string> btns = new List<string>();

    [SerializeField]
    [HideInInspector]
    public List<string> oneByOne = new List<string>();

    [SerializeField]
    [HideInInspector]
    public Vector2 pauseBtnPos = Vector2.zero;

    [SerializeField]
    [HideInInspector]
    public bool set_slience = false;

    [SerializeField]
    [HideInInspector]
    public string pauseEffect;//暂停按钮特效

    [SerializeField]
    [HideInInspector]
    public string pauseEndEffect;//暂停按钮消失特效

    [SerializeField]
    [HideInInspector]
    public string pauseBtnText;//暂停按钮文本

    [SerializeField]
    [HideInInspector]
    public float oneByOneJianGe = 0;//oneByOne 每行间隔

    [SerializeField]
    [HideInInspector]
    public string bossName;     //出场Boss的名字

    /// <summary>
    /// 跳转场景是否要全场景加载
    /// </summary>
    [SerializeField]
    [HideInInspector]
    public bool checkSceneAllDone = false;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        var playable = ScriptPlayable<TriggerPlayableData>.Create(graph, templete);
        TriggerPlayableData behavior = playable.GetBehaviour();
        
        behavior.targetObject = TrackTargetObject;
        behavior.OwningClip = OwningClip;
        behavior.tirggerTimelineClip = this;
        
        return playable;
    }

}
