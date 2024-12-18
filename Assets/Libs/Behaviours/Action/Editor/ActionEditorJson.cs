using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace ActionEditor
{
    [Serializable]
    public class ActionConfig
    {
        #region 编辑器
        public float x;
        public float y;
        public string desc;
        public int sort = 0;
        #endregion

        public int id = 0;
        public string class_name = "";
        public List<ActionField> fields = new List<ActionField>();
        public List<ActionConfig> childs = new List<ActionConfig>();
    }

    [Serializable]
    public class ActionField
    {
        public string desc = "";
        public string f = "";
        public string v = "";
    }
}
