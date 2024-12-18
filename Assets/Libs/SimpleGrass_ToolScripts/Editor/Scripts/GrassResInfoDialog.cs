using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEditor.Experimental;
using UnityEditorInternal;
namespace SimpleGrass
{

    public class GrassResInfoDialog : EditorWindow
    {


        public class GrassResData
        {
            public string name;
            public int chucknum;
            public int grassnum;
        }

        private List<GrassResData> GrassResDatas = new List<GrassResData>();

        private GUIStyle s_SectionHeaderStyle;
        private GUIStyle s_LabelStyle;
        private Vector2 _scrollPos = new Vector2(0,0);
        //private Vector2 _scrollPos2;


        private GameObject GrassRoot = null;
        private int fontsize = 15;
        private bool isDirty = true;
        private int AllGrassNum = 0;        

        [MenuItem("MY_Support/草系统/统计_刷草", false, 5002)]
        
        public static void ShowDialog()
        {
            GrassResInfoDialog frm = EditorWindow.GetWindow<GrassResInfoDialog>(true, "统计-SimpleGrass节点");
            // frm.autoRepaintOnSceneChange = true;
            frm.Show();            
        }



        void Awake()
        {
           
        }

        void OnGUI()
        {
            GrassRoot = GameObject.Find("SimpleGrass");
            if (GrassRoot == null)
            {
                return;
            }

            int oldfontsize = GUI.skin.textField.fontSize;
            
            GUI.color = Color.white;
            GUI.skin.textField.fontSize = fontsize;

            if(isDirty)
            {
                RefreshData();

                isDirty = false;
            }

        //    EditorGUILayout.BeginHorizontal();
            //GUI.color = Color.red;            
            if (GUI.Button(new Rect(2, 10, 100, 30), "刷新 "))
            {
                RefreshData();
            }
            
            GUI.contentColor = Color.white;
            GUI.TextField(new Rect(105, 10, 150, 30), "种类数量： " + GrassRoot.transform.childCount.ToString());
            GUI.contentColor = Color.red;
            GUI.TextField(new Rect(255, 10, 160, 30), "   总数： " + AllGrassNum.ToString());
            int startY = 0;
           // EditorGUILayout.EndHorizontal();

           // GUI.BeginHorizontal();
            _scrollPos =  GUI.BeginScrollView(new Rect(0,35,position.width,position.height),_scrollPos,new Rect(0,0,1000,1000));
            GUI.contentColor = Color.white;
            for (int i = 0; i != GrassResDatas.Count; ++i)
            {
                startY = startY + 30;
                GUI.TextField(new Rect(35, startY, 350, 20), i.ToString() + ": 种类名称：" + GrassResDatas[i].name);

                startY = startY + 20;
               GUI.TextField(new Rect(35, startY, 350, 20), "    块数：" + GrassResDatas[i].chucknum.ToString());

                startY = startY + 20;

                GUI.TextField(new Rect(35, startY, 350, 20), "  草数量：" + GrassResDatas[i].grassnum.ToString());

            }
            GUI.EndScrollView();
          //  EditorGUILayout.EndHorizontal();

            GUI.skin.textField.fontSize = oldfontsize;
        }

        private void RefreshData()
        {
            GrassResDatas.Clear();
            AllGrassNum = 0;
            for (int i = 0; i != GrassRoot.transform.childCount; ++i)
            {
                Transform trf = GrassRoot.transform.GetChild(i);

                GrassResData data = new GrassResData();
                GrassResDatas.Add(data);
                data.name = trf.name;
                data.chucknum = trf.childCount;
                int grassNum = 0;
                for (int j = 0; j != trf.childCount; ++j)
                {
                    grassNum += trf.GetChild(j).childCount;
                }
                data.grassnum = grassNum;

                AllGrassNum += grassNum;

            }
        }
        private GUIStyle sectionHeaderStyle
        {
            get
            {
                if (s_SectionHeaderStyle == null)
                {
                    s_SectionHeaderStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).GetStyle("BoldLabel");
                }
                return s_SectionHeaderStyle;
            }
        }
        private GUIStyle labelStyle
        {
            get
            {
                if (s_LabelStyle == null)
                {
                    s_LabelStyle = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).label);
                    s_LabelStyle.richText = true;
                }
                return s_LabelStyle;
            }
        }


        void OnFocus()
        {
            isDirty = true;
        }

        void OnDestroy()
        {

        }



    }
}
//#endif