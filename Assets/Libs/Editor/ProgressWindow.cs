using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

interface IProgressWorker
{
    void OnGUI();
    void OnDestroy();
    void Update();
}

/// <summary>
/// 进度条界面
/// </summary>
class ProgressWindow : EditorWindow
{
    IProgressWorker _worker;

    // 显示窗口
    public void Show(IProgressWorker worker)
    {
        _worker = worker;
        base.Show();
    }

    void Update()
    {
        _worker.Update();
        Repaint();
    }

    void OnGUI()
    {
        _worker.OnGUI();
    }

    void OnDestroy()
    {
        _worker.OnDestroy();
        _worker = null;
    }
}
