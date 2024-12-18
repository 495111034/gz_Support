
using UnityEditor;
using UnityEngine;

public class CustomKeys:Editor
{
    /// <summary>
    /// 显示/隐藏选中对象
    /// </summary>
    [MenuItem("Tools/CustomKey/Active GameObject &`")]
    static void EditorCustomKey1()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null) return;
        go.SetActive(!go.activeSelf);
    }

    [MenuItem("Tools/CustomKey/Ping Focused &1")]
    static void EditorCustomKey2()
    {
        var go = UnityEngine.EventSystems.MyStandaloneInputModule.CurrentFocusedGameObject;
        if (go != null)
        {
            EditorGUIUtility.PingObject(go);
            Selection.activeGameObject = go;
        } else
        {
            Debug.Log("当前鼠标位置未获取到组件");
        }
    }

    [MenuItem("Tools/CustomKey/Ping Focused Open &2")]
    static void EditorCustomKey3()
    {
        var go = UnityEngine.EventSystems.MyStandaloneInputModule.CurrentFocusedGameObject;
        if (go == null)
        {
            go = Selection.activeGameObject;
            if (go == null && (Selection.objects != null && Selection.objects.Length > 0))
            {
                go = Selection.objects[0] as GameObject;
            }
        }
        if (go != null)
        {
            MyComponent curr_comp = go.GetComponent<MyComponent>();
            if (curr_comp != null)
            {
                if (IsViewFile(go.name))
                {
                    curr_comp.TryOpenProgramFile();
                    return;
                }
            }
            var comps = go.transform.GetComponentsInParent<MyComponent>();
            foreach (var comp in comps)
            {
                if (IsViewFile(comp.gameObject.name))
                {
                    comp.TryOpenProgramFile();
                    return;
                }
            }

            var canvasScaler = go.GetComponentInParent<UnityEngine.UI.CanvasScaler>();
            if (canvasScaler != null)
            {
                if (canvasScaler.TryGetComponent<MyComponent>(out var myComponent))
                {
                    myComponent.TryOpenProgramFile();
                    return;
                }
            }
        }
        else
        {
            Debug.Log("当前鼠标位置未获取到组件");
        }
    }

    static bool IsViewFile(string objName)
    {
        if (string.IsNullOrEmpty(objName)) return false;
        if (!objName.Contains("_panel")) return false;
        string[] splits = objName.Split("(");
        string finalClassName = splits[0];
        finalClassName = finalClassName.Split(" ")[0];
        string finalClassPath = System.IO.Path.GetFullPath($"../GameLogic/script/view/auto/{finalClassName}.cs");
        return System.IO.File.Exists(finalClassPath);
    }
}
