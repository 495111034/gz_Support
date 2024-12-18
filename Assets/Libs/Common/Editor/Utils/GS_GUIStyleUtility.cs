

using UnityEditor;
using UnityEngine;

namespace GameSupportEditor
{
    public class GS_GUIStyleUtils
    {
        public static GUIStyle GetHelpBox()
        {
            return EditorStyles.helpBox;
        }

        public static GUIStyle GetRadioButton()
        {
            return EditorStyles.radioButton;
        }

        #region lable style
        public static GUIStyle GetBoldLabel(int size)
        {
            return new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = size };
        }

        #endregion
    }
}
