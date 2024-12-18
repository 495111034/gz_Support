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


[CustomEditor(typeof(SkillEditorData), true)]
[CanEditMultipleObjects]
public class SkillDataListEditor : UnityEditor.Editor
{
    SerializedProperty skillListProperty;

    private void OnEnable()
    {
        skillListProperty = serializedObject.FindProperty("skillList");

    }

    private void OnDisable()
    {

    }

    private void OnDestroy()
    {

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}

