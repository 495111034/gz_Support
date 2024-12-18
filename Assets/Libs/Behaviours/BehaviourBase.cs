using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class BehaviourBase : MonoBehaviour
{
    
    public Action OnAwake;
    public Action OnUpdate;
    public Action OnStart;
    public Action OnLateUpdate;
    public Action On_Enable;
    public Action On_Destory;
    //public Action On_GUI;
    public Action On_Disable;

    public string BehavourName = "";
    public string expInfo = "";

    public bool IsNotisEnable = false;

    void Awake()
    {
        OnAwake?.Invoke();
    }

    void Update()
    {
        UnityEngine.Profiling.Profiler.BeginSample("BehaviourBase Update");
        OnUpdate?.Invoke();
        UnityEngine.Profiling.Profiler.EndSample();
    }

    void OnEnable()
    {
        if (!IsNotisEnable)
        {
            On_Enable?.Invoke();
        }
        IsNotisEnable = true;
    }

    void Start()
    {
        OnStart?.Invoke();
    }

    void LateUpdate()
    {
        OnLateUpdate?.Invoke();
    }

    void OnDestory()
    {
        IsNotisEnable = false;
        On_Destory?.Invoke();
        OnAwake = null;
        OnUpdate = null;
        OnStart = null;
        OnLateUpdate = null;
        On_Enable = null;
        On_Destory = null;
        On_Disable = null;
    }


    void OnDisable()
    {
        IsNotisEnable = false;
        On_Disable?.Invoke();
    }
}

