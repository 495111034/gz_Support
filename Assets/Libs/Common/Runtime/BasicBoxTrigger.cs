using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameSupport
{
  public class BasicBoxTrigger : MonoBehaviour
  {
        public enum TRIGGER_STATUS
        {
            TS_BEGIN = 0,
            TS_END = 1,
            TS_ENTER = 2,
            TS_EXIT = 3,
        };

        public class TriggerData
        {
            public List<GameObject> Enclosures = new List<GameObject>();
            public UnityAction<Transform, TRIGGER_STATUS> OnTrigger = null;
        };

        static public Dictionary<int, TriggerData> S_TriggeredEnclosures = new Dictionary<int, TriggerData>();
        public static bool IsRegistered(int kindCode)
        {
            return S_TriggeredEnclosures.ContainsKey(kindCode);
        }
        static public void RegisterKind(int kindCode, UnityAction<Transform, TRIGGER_STATUS> action)
        {
            TriggerData data;
            if (!S_TriggeredEnclosures.TryGetValue(kindCode, out data))
            {
                data = new TriggerData();
                data.OnTrigger = action;
                S_TriggeredEnclosures.Add(kindCode, data);
            }else
            {
                data.OnTrigger = action;
            }
        }
        static public void UnRegisterKind(int kindCode)
        {
            if (S_TriggeredEnclosures.ContainsKey(kindCode))
            {
                S_TriggeredEnclosures.Remove(kindCode);
            }
        }       

        private bool _inCollider = false;
        private BoxCollider _collider = null;
        Bounds _correctBounds = new Bounds();
        private bool _triggerEnabled = false;
        int _kindCode = 0;
        private void Awake()
        {
            _collider = this.GetComponent<BoxCollider>();
            _collider.isTrigger = true;
            _collider.enabled = false;
            _kindCode = RegisterCode();
           
        }

        virtual protected  void OnEnable()
        {
            _triggerEnabled = CheckValid();
            _inCollider = false;
            _correctBounds = new Bounds(_collider.center, _collider.size);
        }

        virtual protected void OnDisable()
        {
            _triggerEnabled = false;           
            _OnTriggerExit();
        }

        virtual protected void Update()
        {
            bool curChecked = CheckValid();
            if (_triggerEnabled && !curChecked)
            {
                _OnTriggerExit();                
                _inCollider = false;
            }
            _triggerEnabled = curChecked;
            if(!_triggerEnabled)
            {
                return;
            }


            bool curInCollider = IsInCollider();
            if (_inCollider && !curInCollider)
            {
                _OnTriggerExit();
            }
            
            if (!_inCollider && curInCollider)
            {
                _OnTriggerEnter();
            }

            _inCollider = curInCollider;
        }

        protected virtual bool CheckValid()
        {
            return true;
        }

        protected virtual int RegisterCode()
        {
            return 0;
        }

        protected virtual bool IsInCollider()
        {
            return false;
        }

        protected virtual void _OnTriggerEnter()
        {                         
            TriggerData data;
            if (! S_TriggeredEnclosures.TryGetValue(_kindCode,out data))
            {
                return;
            }
            if(data.Enclosures == null)
            {
                return;
            }
            if (!data.Enclosures.Contains(this.gameObject))
            {
                data.Enclosures.Add(this.gameObject);
            }

            if (data.Enclosures.Count == 1)
            {
                data.OnTrigger?.Invoke(this.transform, TRIGGER_STATUS.TS_BEGIN);
                //Debug.Log("## BasicBox Trigger - BEGIN ");
            }
            data.OnTrigger?.Invoke(this.transform, TRIGGER_STATUS.TS_ENTER);
            //Debug.Log("## BasicBox Trigger - Enter ");
        }

        protected virtual void _OnTriggerExit()
        {
            if (_RemoveCollider())
            {
                //Debug.Log("## BasicBox Trigger - Exit ");               

                TriggerData data;
                if (!S_TriggeredEnclosures.TryGetValue(_kindCode, out data))
                {
                    return ;
                }
                data.OnTrigger?.Invoke(this.transform, TRIGGER_STATUS.TS_EXIT);

                if (data.Enclosures != null && data.Enclosures.Count == 0)
                {
                    data.OnTrigger?.Invoke(this.transform, TRIGGER_STATUS.TS_END);
                   // Debug.Log("## BasicBox Trigger - END ");
                }
            }
        }

        private bool _RemoveCollider()
        {
            TriggerData data = null;
            if (!S_TriggeredEnclosures.TryGetValue(_kindCode, out data))
            {
                return false;
            }
            if (data.Enclosures != null)
            {
                if (!data.Enclosures.Contains(this.gameObject))
                { 
                    return false;
                }
                data.Enclosures.Remove(this.gameObject);
            }            
            return true;
        }

        protected bool _Contains(Vector3 positionWS)
        {
            Vector3 pntBoxSpace = _collider.transform.InverseTransformPoint(positionWS);

            return _correctBounds.Contains(pntBoxSpace);
        }
    }
}

