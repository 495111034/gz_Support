using UnityEngine;

namespace UnityEngine.UI
{
    public class ScrollCellDelayAnima : MonoBehaviour
    {
        public void Init(float wait)
        {
            transform.localScale = Vector3.zero;
            Animator anima = gameObject.GetComponent<Animator>();
            if (anima != null)
            {
                anima.enabled = false;
                var behaviours = anima.GetBehaviours<AnimaEventBehaviour>();
                if (behaviours != null)
                {
                    foreach (var item in behaviours)
                    {
                        item.trigger_event += trigger_event;
                    }
                }
            }
            if (IsInvoking())
            {
                CancelInvoke();
            }
            Invoke("Delay", wait);
        }

        private void Delay()
        {
            transform.localScale = Vector3.one;
            Animator anima = gameObject.GetComponent<Animator>();
            if (anima != null) anima.enabled = true;
        }

        private void trigger_event(int id)
        {
            if (id == 998)
            {
                Animator anima = gameObject.GetComponent<Animator>();
                if (anima != null) anima.enabled = false;
            }
        }
    }
}
