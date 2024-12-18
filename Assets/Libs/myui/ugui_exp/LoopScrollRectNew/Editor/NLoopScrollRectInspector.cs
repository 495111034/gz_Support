using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(NLoopScrollRectBase), true)]
public class NLoopScrollRectInspector : Editor
{
    int index = 0;
    float speed = 1000, time = 1;
    public override void OnInspectorGUI ()
    {
        NLoopScrollRectBase scroll = (NLoopScrollRectBase)target;
        if (scroll.prefab != null)
        {
            if (scroll.prefab.GetComponent<LayoutElement>() == null)
            {
                EditorGUILayout.HelpBox("prefab未添加LayoutElement组件", MessageType.Error);
            }
        }

        if (target is NLoopHorizontalScrollRect || target is NLoopHorizontalScrollRectMulti)
        {
            scroll.horizontal = true;
            scroll.vertical = false;
        }
        else
        {
            scroll.horizontal = false;
            scroll.vertical = true;
        }

        base.OnInspectorGUI();
        EditorGUILayout.Space();

        GUI.enabled = Application.isPlaying;

        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Clear"))
        {
            scroll.ClearCells();
        }
        if (GUILayout.Button("Refresh"))
        {
            scroll.RefreshCells();
        }
        if(GUILayout.Button("Refill"))
        {
            scroll.RefillCells();
        }
        if(GUILayout.Button("RefillFromEnd"))
        {
            scroll.RefillCellsFromEnd();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = 45;
        float w = (EditorGUIUtility.currentViewWidth - 100) / 2;
        index = EditorGUILayout.IntField("Index", index, GUILayout.Width(w));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        speed = EditorGUILayout.FloatField("Speed", speed, GUILayout.Width(w));
        if(GUILayout.Button("Scroll With Speed", GUILayout.Width(130)))
        {
            scroll.SrollToCell(index, speed);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        time = EditorGUILayout.FloatField("Time", time, GUILayout.Width(w));
        if(GUILayout.Button("Scroll Within Time", GUILayout.Width(130)))
        {
            scroll.SrollToCellWithinTime(index, time);
        }
        EditorGUILayout.EndHorizontal();
    }
}