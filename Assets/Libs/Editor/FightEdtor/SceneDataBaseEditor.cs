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

[CustomEditor(typeof(SceneDataBase), true)]
[CanEditMultipleObjects]
public class SceneDataBaseEditor : UnityEditor.Editor
{
    SerializedProperty m_sceneID;

    private void OnEnable()
    {
        if (File.Exists(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.txt")))
        {
            var text = File.ReadAllText(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.txt"));
            var lang_id = BuilderConfig.lang_id;
            if (text != null) BuilderConfig.ParseStartupParams(text);
            BuilderConfig.lang_id = lang_id;
        }
        else
        {
            Log.LogError("config.txt文件不存在");
            return;
        }

        m_sceneID = serializedObject.FindProperty("id");
        if (m_sceneID.intValue > 0)
        {
            
        }

    }

    private void OnDisable()
    {

    }

    private void OnDestroy()
    {

    }


}

