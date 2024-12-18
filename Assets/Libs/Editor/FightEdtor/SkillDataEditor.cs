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


[CustomEditor(typeof(SkillEditorDataHelper), true)]
[CanEditMultipleObjects]
public class SkillDataEditor : UnityEditor.Editor
{
    SkillEditorData targetObject;
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
        targetObject = (serializedObject.targetObject as SkillEditorDataHelper).gameObject.GetComponent<SkillEditorData>();
        if (!targetObject)
        {
            Log.LogError("SkillEditorData is null");
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

