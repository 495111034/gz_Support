using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//only for edit debug data
public class TestBehaviour : MonoBehaviour
{
    public int nValue;
    public string fValue;
    public string strValue;

#if UNITY_EDITOR
    private void Start()
    {
        
    }
    
    private void OnEnable()
    {
        //var debug  = 1;
    }
    
    private void OnDisable()
    {
        //var debug  = 1;
    }
#endif
    
}
