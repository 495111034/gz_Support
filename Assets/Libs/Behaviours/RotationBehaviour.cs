using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Effect/Rotation Behaviour (旋转控制)")]
public class RotationBehaviour : MonoBehaviour
{
    [HideInInspector] 
    [SerializeField]
    float speed_x = 0;
    [HideInInspector]
    [SerializeField]
    float speed_y = 0;
    [HideInInspector]
    [SerializeField]
    float speed_z = 0;
    void Start()
    {
        if (Application.isPlaying)
        {
            Init();
        }
    }

    public void Init()
    {

    }



    void Update()
    {
        if(Application.isPlaying)
        {
            __OnUpdate(Time.deltaTime);
        }
    }

#if UNITY_EDITOR
    public void EditorUpdate(float deltaTime)
    {
        if(!Application.isPlaying)
        {
            __OnUpdate(deltaTime);
        }
    }
#endif
   
    void __OnUpdate(float deltaTime)
    { 
        gameObject.transform.Rotate(speed_x * deltaTime * 10, speed_y * deltaTime * 10, speed_z * deltaTime * 10);
    }
}
