


using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FightFxTrigger : MonoBehaviour
{
    [SerializeField]
    public int PhaseId;

    [SerializeField]
    public bool IsHitFly;

    [SerializeField]
    public float StayCD;

    [SerializeField]
    public float HitBackDist;

    [NonSerialized]
    public int FightId;

    [NonSerialized]
    HashSet<Collider> Entered;

    [NonSerialized]
    float _last_fight_time;

    public static Action<ObjectBehaviourBase, FightFxTrigger> Event_OnTriggerEnter;

    public void SetFightId(int FightId) 
    {
        this.FightId = FightId;
        Log.LogInfo($"FightFxTrigger SetFightId={FightId}, {gameObject}");
        if (FightId == 0)
        {
            var rigidbody = gameObject.GetComponent<Rigidbody>();
            if (rigidbody) 
            {
                GameObject.DestroyImmediate(rigidbody,true);
            }
            var box = gameObject.GetComponent<BoxCollider>();
            if (box) 
            {
                box.enabled = false;
            }
        }
        else 
        {
            var rigidbody = gameObject.AddMissingComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;

            var box = gameObject.AddMissingComponent<BoxCollider>();
            box.isTrigger = true;
            box.enabled = true;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!gameObject.GetComponent<BoxCollider>())
        {
            var box = gameObject.AddComponent<BoxCollider>();
            box.size = Vector3.one;
            box.isTrigger = true;
        }
    }
#endif

    void OnEnable()
    {
        if (Entered == null)
        {
            Entered = new HashSet<Collider>();
        }
    }

    private void OnDisable()
    {
        Entered?.Clear();
    }

    private void OnTriggerStay(Collider other)
    {
        //Log.LogInfo($"OnTriggerStay other={other}");
        if (StayCD > 0) 
        {
            if (Time.time - _last_fight_time > StayCD)
            {
                _last_fight_time = Time.time;
                Log.LogInfo($"FightFxTrigger OnTriggerStay {gameObject} hit other={other}");
                Event_OnTriggerEnter?.Invoke(other.gameObject.GetComponent<ObjectBehaviourBase>(), this);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        _last_fight_time = Time.time;
        Log.LogInfo($"FightFxTrigger OnTriggerEnter {gameObject} hit other={other}");
        if (Entered.Add(other))
        {
            Event_OnTriggerEnter?.Invoke(other.gameObject.GetComponent<ObjectBehaviourBase>(), this);
        }
    }
}

