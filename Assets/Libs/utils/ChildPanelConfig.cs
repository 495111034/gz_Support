using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;


[AddComponentMenu("UI/ChildPanelConfig",1)]
public class ChildPanelConfig : MonoBehaviour
{
    public string ChildResName { get { return _child_res_name; } set { _child_res_name = value; } }

    public string ExpInfo { get { return _child_exp_info; } set { _child_exp_info = value; } }

    public bool IsInitOK = false;

    [SerializeField]
    protected string _child_res_name;

    [SerializeField]
    protected string _child_exp_info;
}

