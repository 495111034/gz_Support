using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScenarioBehaviour : MonoBehaviour
{
    /// <summary>
    /// 碰撞时向上抛事件中的参数
    /// </summary>
    public class ScenarioTriggerParams
    {
        public ScenarioBehaviour trigger;
        public GameObject Collider;
        public bool isDianTi;
    }


    [HideInInspector]
    public Hashtable TriggerParams;

    public string TriggerParamsStr;


    bool isSendedEnterScene = false;

    [HideInInspector]
    public bool IsWaittingFromServer { get; set; }

    [HideInInspector]
    public uint Scenario_id { get; set; }

    [HideInInspector]
    public float tmp_dist_sq;

    [HideInInspector]
    public string nodeEffect;

    [HideInInspector]
    public int funcOpenID;

    //public bool isDianTi = false;

    private void OnEnable()
    {
        MyTask.Last_opcode = "ScenarioBehaviour.OnEnable";
        gameObject.SetLayerRecursively((int)ObjLayer.Trigger);
        MyTask.Last_opcode = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(TriggerParamsStr))
            TriggerParams = MiniJSON.JsonDecode(TriggerParamsStr) as Hashtable;
    }
#endif

    public void Awake()
    {
        MyTask.Last_opcode = "ScenarioBehaviour.Awake";

        if (!string.IsNullOrEmpty(TriggerParamsStr))
            TriggerParams = MiniJSON.JsonDecode(TriggerParamsStr) as Hashtable;

        if(TriggerParams == null)
        {
            if(Application.isEditor)  Log.LogWarning("TriggerParamsStr decode error!");
        }
        MyTask.Last_opcode = null;
    }

    public void ResetState()
    {
        IsWaittingFromServer = false;
       // var ids = gameObject.name.Split('_');
       // Scenario_id = ConvertUtils.ToUInt(ids[ids.Length - 1]);
    }

    /// <summary>
    /// 上层调用，当场景加载完成时
    /// </summary>
    public void OnEnterScene()
    {

        MyTask.Last_opcode = "ScenarioBehaviour.OnEnterScene";

        if (!isSendedEnterScene)
        {
            SendMessageUpwards("_onEnterScene", this);
        }

        isSendedEnterScene = true;

        MyTask.Last_opcode = null;
    }

    /// <summary>
    /// 上层调用，波次结束
    /// </summary>
    public void OnPhaseComplete()
    {

        MyTask.Last_opcode = "ScenarioBehaviour.OnPhaseComplete";

        SendMessageUpwards("_onPhaseComplete", this);

        MyTask.Last_opcode = null;
    }

    /// <summary>
    /// 上层调用，触发事件
    /// </summary>
    public void OnScenarioEvent()
    {
        MyTask.Last_opcode = "ScenarioBehaviour.OnScenarioEvent";

        SendMessageUpwards("_onSceneEvent", this);

        MyTask.Last_opcode = null;
    }

    /// <summary>
    /// unity调用，当进入碰撞体时
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerEnter(Collider col)
    {
        MyTask.Last_opcode = "ScenarioBehaviour.OnTriggerEnter";
        //Log.LogError($"_onTriggerEnter:{col.gameObject.name}");
        if (col && col.gameObject)
        {
            ScenarioTriggerParams p = new ScenarioTriggerParams();
            p.Collider = col.gameObject;
            p.trigger = this;
            //p.isDianTi = isDianTi;
            SendMessageUpwards("_onTriggerEnter", p);
        }

        MyTask.Last_opcode = null;
    }

    /// <summary>
    /// unity调用，当离开碰撞体时
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerExit(Collider col)
    {
        MyTask.Last_opcode = "ScenarioBehaviour.OnTriggerExit";
        if (col && col.gameObject)
        {
            ScenarioTriggerParams p = new ScenarioTriggerParams();
            p.Collider = col.gameObject;
            p.trigger = this;
            //p.isDianTi = isDianTi;
            SendMessageUpwards("_onTriggerExit", p);
        }

        MyTask.Last_opcode = null;
    }

    /// <summary>
    /// unity调用，在碰撞体停留时每帧调用
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerStay(Collider col)
    {

        MyTask.Last_opcode = "ScenarioBehaviour.OnTriggerStay";
        if (col && col.gameObject)
        {
            ScenarioTriggerParams p = new ScenarioTriggerParams();
            p.Collider = col.gameObject;
            p.trigger = this;
            //p.isDianTi = isDianTi;
            SendMessageUpwards("_onTriggerStay", p);
        }

        MyTask.Last_opcode = null;
    }
}
