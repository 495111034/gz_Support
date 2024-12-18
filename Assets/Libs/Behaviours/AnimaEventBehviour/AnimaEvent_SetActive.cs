using UnityEngine;
using UnityEngine.Animations;

public class AnimaEvent_SetActive : BaseAnimaEventTrigger
{
    [System.Serializable]
    public class SetActiveObj
    {
        public GameObject go = null;
        public bool is_active = false;
    }
    public int id = 0;
    public SetActiveObj[] gos = null;
    public override void OnStateTrigger(int _id)
    {
        if (id == _id)
        {
            if (gos != null)
            {
                for (int i = 0; i < gos.Length; i++)
                {
                    gos[i].go?.SetActiveX(gos[i].is_active);
                }
            }
        }
    }
}
