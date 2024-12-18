using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class BaseAnimaEventTrigger : MonoBehaviour
{
    private void Start()
    {
        RegAnimaEvent();
    }

    public void RegAnimaEvent()
    {
        if (gameObject != null)
        {
            Animator anima = gameObject.GetComponent<Animator>();
            if (anima != null && anima.runtimeAnimatorController != null)
            {
                var eventBehaviours = anima.GetBehaviours<AnimaEventBehaviour>();
                if (eventBehaviours != null && eventBehaviours.Length > 0)
                {
                    for (int i = 0; i < eventBehaviours.Length; i++)
                    {
                        eventBehaviours[i].trigger_event += OnStateTrigger;
                    }
                    return;
                }
            }
        }
        Invoke("RegAnimaEvent", 0.01f);
    }

    public virtual void OnStateTrigger(int _id)
    {
        
    }
}

public class AnimaEventBehaviour : StateMachineBehaviour
{
    public enum StateType
    {
        Enter,
        Exit
    }
    public StateType stateType;
    public int id = 0;
    public event System.Action<int> trigger_event;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
    {
        if (stateType == StateType.Enter)
        {
            trigger_event?.Invoke(id);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
    {
        if (stateType == StateType.Exit)
        {
            trigger_event?.Invoke(id);
        }
    }
}
