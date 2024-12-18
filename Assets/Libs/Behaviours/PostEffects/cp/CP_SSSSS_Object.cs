using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MyEffect
{
    [ExecuteInEditMode]
    //[RequireComponent(typeof(Renderer))]
    public class CP_SSSSS_Object : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {


        }

        void OnWillRenderObject()
        {
#if UNITY_EDITOR
           // if (Application.isPlaying) return;


            var camera = Camera.current;
            var sssssMain = camera.GetComponent<CP_SSSSS_Main>();
            if (sssssMain && sssssMain.isActiveAndEnabled)
            {
                sssssMain.AddRenders(gameObject.GetComponentsEx<Renderer>());
            }
#endif
        }




    }

}