using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//网上找的，挂在状态机的某个状态上。进入或退出该状态时清除某些parameter，保证下次进入时为初始状态不会发生异常
public class FSMCleaSignals : StateMachineBehaviour
{
    public string[] ckearAtEnter;
    public string[] ckearAtExit;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach (var signal in ckearAtEnter)
        {
            animator.ResetTrigger(signal);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach (var signal in ckearAtExit)
        {
            animator.ResetTrigger(signal);
        }
    }
}