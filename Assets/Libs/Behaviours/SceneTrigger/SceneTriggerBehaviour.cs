using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("场景通用触发器")]
/// <summary>
/// 场景通用触发器，独立于ScenarioBehaviour系列
/// </summary>
public class SceneTriggerBehaviour : MonoBehaviour
{
    public string[] parmas_jsons;

    public event System.Action onUpdate;
    public event System.Action onTriggerEnter;
    public event System.Action onTriggerStay;
    public event System.Action onTriggerExit;
    public event System.Action<int> onDataUpdate;

    public GameObject collider_go;

    public List<int> trigger_counts { set; get; }

    public void ExcuteDataUpdate(int data_index)
    {
        onDataUpdate?.Invoke(data_index);
    }

    //private Collider __collider = null;
    //private float _delay_time = 0;

    //private void Start()
    //{
        //__collider = gameObject.GetComponent<Collider>();
    //}

    private void LateUpdate()
    {
        onUpdate?.Invoke();

        //if (__collider != null)
        //{
        //    _delay_time += Time.deltaTime;
        //    __collider.isTrigger = !__collider.isTrigger;
        //    if (_delay_time > 5)
        //    {
        //        __collider.isTrigger = true;
        //        __collider = null;
        //    }
        //}
    }

    /// <summary>
    /// unity调用，当进入碰撞体时
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerEnter(Collider col)
    {
        //__collider = null;
        if (col && col.gameObject)
        {
            collider_go = col.gameObject;
            onTriggerEnter?.Invoke();
        }
    }

    /// <summary>
    /// unity调用，当离开碰撞体时
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerExit(Collider col)
    {
        //__collider = null;
        if (col && col.gameObject)
        {
            collider_go = col.gameObject;
            onTriggerExit?.Invoke();
        }
    }

    /// <summary>
    /// unity调用，在碰撞体停留时每帧调用
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerStay(Collider col)
    {
        //__collider = null;
        if (col && col.gameObject)
        {
            collider_go = col.gameObject;
            onTriggerStay?.Invoke();
        }
    }
}
