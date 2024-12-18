using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

[CustomEditor(typeof(RulerTarget))]
class RulerEditor : Editor
{
    GUIStyle _lab_style;

    void OnSceneGUI()
    {
        var source = Object.FindObjectOfType<RulerSource>();
        if (source == null) return;

        var t1 = source.transform;
        var step = source.step;
        var color = source.color;
        if (step <= 0) return;

        var target = base.target as RulerTarget;
        var t2 = target.transform;

        //
        var pos1 = t1.position;
        var pos2 = t2.position;

        var diff = (pos2 - pos1);
        var forward = diff.normalized;
        var dist = diff.magnitude;
        float count = 0;

        Handles.color = color;
        Handles.DrawLine(pos1, pos2);

        if (_lab_style == null)
        {
            _lab_style = new GUIStyle(EditorStyles.largeLabel);
            _lab_style.normal.textColor = color;
        }

        Handles.Label(pos2, dist.ToString("f3"), _lab_style);

        var pos = pos1;
        while (dist > step)
        {
            pos += forward * step;
            dist -= step;
            count += step;
            Handles.Label(pos, count.ToString("f3"), _lab_style);
        }
    }
}
