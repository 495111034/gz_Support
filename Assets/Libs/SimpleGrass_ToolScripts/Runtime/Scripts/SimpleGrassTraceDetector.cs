using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleGrass
{
  // Detect collisions 
  [RequireComponent(typeof(BoxCollider))]
  [ExecuteInEditMode]
  public class SimpleGrassTraceDetector : MonoBehaviour
  {
        static public List<GameObject> S_TriggeredEnclosures = new List<GameObject>();
        public Action<bool> enclosureChangedCallback;

        private bool _inCollider = false;
        private BoxCollider _collider = null;
        Bounds _correctBounds = new Bounds();
        Vector3 test;
        private void Awake()
        {
            _collider = this.GetComponent<BoxCollider>();
            _collider.isTrigger = true;
        }
        private void Start()
        {            
        }

        private void OnEnable()
        {
            _correctBounds = new Bounds(_collider.center, _collider.size);
            this.enclosureChangedCallback -= SimpleGrassTrace.OnTraceEnclosureDidChange;
            this.enclosureChangedCallback += SimpleGrassTrace.OnTraceEnclosureDidChange;
            ApplyEnclosure();
        }

        private void OnDisable()
        {
            _RemoveCollider();
            ApplyEnclosure();
            this.enclosureChangedCallback -= SimpleGrassTrace.OnTraceEnclosureDidChange;            
        }

        private void Update()
        {
            bool curInCollider = _Contains(SimpleGrassTrace.S_HeroFootPos);
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


        private void _OnTriggerEnter()
        {
            Common.OutputDebugLog("## SimpleGrassTraceDetector - Enter ");
            if (!S_TriggeredEnclosures.Contains(this.gameObject))
            {
                S_TriggeredEnclosures.Add(this.gameObject);
            }

            ApplyEnclosure();
        }

        private void _OnTriggerExit()
        {
            Common.OutputDebugLog("## SimpleGrassTraceDetector - Exit ");
            _RemoveCollider();

            ApplyEnclosure();
        }

        public void ApplyEnclosure()
        {
            //GameObject enclosure = null;
            //if (S_TriggeredEnclosures.Count > 0)
            //{
            //    enclosure = S_TriggeredEnclosures[S_TriggeredEnclosures.Count - 1];
            //}            

            if (enclosureChangedCallback != null)
            {
                enclosureChangedCallback(S_TriggeredEnclosures.Count > 0);
            }
        }

        private void _RemoveCollider()
        {
            if (!S_TriggeredEnclosures.Contains(this.gameObject))
            {
                return;
            }

            S_TriggeredEnclosures.Remove(this.gameObject);
        }

        private bool _Contains(Vector3 positionWS)
        {
            Vector3 pntBoxSpace = _collider.transform.InverseTransformPoint(positionWS);
            
            return _correctBounds.Contains(pntBoxSpace);
        }


        //private void OnDrawGizmos()
        //{
        //    Gizmos.DrawSphere(test, 1);
        //}
    }
}

