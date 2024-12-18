

using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    public class ScrollRectDragUpSendMessage : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public bool is_hor = true;
        public float range = 10;

        private GameObject _prefab;
        private GameObject prefab
        {
            get
            {
                if (_prefab == null)
                {
                    if (transform.parent != null)
                    {
                        _prefab = transform.parent.gameObject;
                    }
                }
                return _prefab;
            }
        }

        private MonoBehaviour _sroll_comp;
        public MonoBehaviour sroll_comp
        {
            get
            {
                if (_sroll_comp == null)
                {
                    var comps = gameObject.GetComponents<Component>();
                    if (comps != null)
                    {
                        for (int i = 0; i < comps.Length; i++)
                        {
                            if ((comps[i] as IDragHandler) != null && comps[i] != this)
                            {
                                _sroll_comp = comps[i] as MonoBehaviour;
                                break;
                            }
                        }
                    }
                }
                return _sroll_comp;
            }
        }

        private bool is_keep_up = false;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsCheckCanUpSend(eventData))
            {
                return;
            }
            prefab?.SendMessageUpwards("OnBeginDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!is_keep_up)
            {
                return;
            }
            prefab?.SendMessageUpwards("OnDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!is_keep_up)
            {
                return;
            }
            is_keep_up = false;
            sroll_comp.enabled = true;
            prefab?.SendMessageUpwards("OnEndDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }

        private bool IsCheckCanUpSend(PointerEventData eventData)
        {
            is_keep_up = false;
            sroll_comp.enabled = true;
            if (is_hor)
            {
                if (eventData.delta.y < -range || eventData.delta.y > range)
                {
                    is_keep_up = true;
                    sroll_comp.enabled = false;
                    return true;
                }
            }
            else
            {
                if (eventData.delta.x < -range || eventData.delta.x > range)
                {
                    is_keep_up = true;
                    sroll_comp.enabled = false;
                    return true;
                }
            }
            return false;
        }
    }
}
