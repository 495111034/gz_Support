using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Entity;
using UnityEngine.Timeline;
using UnityEditor;
using Object = UnityEngine.Object;
using System.IO;



[CustomEditor(typeof(FightEditorDataHelper), true)]
[CanEditMultipleObjects]
public class FightEditorDataHelperEditor : UnityEditor.Editor
{
    FightEditorData targetObject;
    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {

    }

    private void OnDestroy()
    {

    }

    public override void OnInspectorGUI()
    {
        targetObject = (serializedObject.targetObject as FightEditorDataHelper).gameObject.GetComponent<FightEditorData>();
        if (!targetObject)
        {
            Log.LogError("FightEditorData is null");
        }

        if (targetObject != null)
        {
            if (GUILayout.Button("保存数据"))
            {
                targetObject.SaveData();
            }
        }
    }
}

