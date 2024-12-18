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

    public class SimpleGrassHintInfoDialog : EditorWindow
    {
        private Vector2 scrollPosition = Vector2.zero;
        private GUIStyle myTextAreaStyle;
        public static void ControlDialog(SimpleGrassSys grassSys, bool restPosition)
        {
            if (grassSys )
            {
                if (grassSys.HintInfo)
                {
                    SimpleGrassHintInfoDialog.ShowDialog(restPosition);                    
                }
                else
                {
                    if(!grassSys.HintInfo && IsShowed)
                      SimpleGrassHintInfoDialog.CloseDialog();
                }
            }
            else
            {
                if (IsShowed)
                    SimpleGrassHintInfoDialog.CloseDialog();
            }
        }
     
        public static void ShowDialog(bool restPosition)
        {
            float fWidth = 220;
            float fHeight = 120;
            if (!SimpleGrassHintInfoDialog.IsShowed)
            {
                Vector2 pos = Vector2.zero;
                string title = "GrassSys";
                
                if (Application.isPlaying)
                {
                    pos = SimpleGrassHintInfoDialog.GetGameViewPosition();
                    IsPriorInPlaying = true;
                }
                else
                {
                    title = "SimpleGrass";
                    if (UnityEditor.SceneView.sceneViews.Count > 0)
                    {
                        pos.x = (UnityEditor.SceneView.sceneViews[0] as SceneView).position.x + 10;
                        pos.y = (UnityEditor.SceneView.sceneViews[0] as SceneView).position.y + 70;
                    }
                    IsPriorInPlaying = false;
                    fHeight = 50;
                }               

                SimpleGrassHintInfoDialog frm = EditorWindow.GetWindow<SimpleGrassHintInfoDialog>(true, title);                
                frm.autoRepaintOnSceneChange = true;                
                //frm.minSize = new Vector2(220, 80);
                frm.position = new Rect(pos.x, pos.y, fWidth, fHeight);
                frm.Show();
                IsShowed = true;
            }else
            {
              //  if(restPosition)
                //{
                //    Vector2 pos = Vector2.zero;
                //    if (Application.isPlaying)
                //    {
                //        pos = SimpleGrassHintInfoDialog.GetGameViewPosition();
                //    }
                //    else
                //    {
                //        if (UnityEditor.SceneView.sceneViews.Count > 0)
                //        {
                //            pos.x = (UnityEditor.SceneView.sceneViews[0] as SceneView).position.x + 10;
                //            pos.y = (UnityEditor.SceneView.sceneViews[0] as SceneView).position.y + 70;
                //        }
                //        fHeight = 50;
                //    }

                //    SimpleGrassHintInfoDialog frm = EditorWindow.GetWindow<SimpleGrassHintInfoDialog>(true, "GrassSys");
                //    if(frm)
                //      frm.position = new Rect(pos.x, pos.y, fWidth, fHeight);
                //}
            }
        }

        public static Vector2 GetGameViewPosition()
        {
            Vector2 ret = new Vector2(10, 70);
            System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            if (T != null)
            {
                System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (GetMainGameView != null)
                {
                    System.Object Res = GetMainGameView.Invoke(null, null);
                    if (Res != null)
                    {
                        var gameView = (UnityEditor.EditorWindow)Res;
                        if (gameView)
                        {
                            ret.x = gameView.position.xMin + 10;
                            ret.y = gameView.position.yMin + 70;
                        }
                    }
                }
            }
            return ret;
        }

        public static void CloseDialog()
        {
            if(IsShowed)
            {
                SimpleGrassHintInfoDialog frm = EditorWindow.GetWindow<SimpleGrassHintInfoDialog>();
                frm.Close();
            }
        }


        private GUIStyle s_SectionHeaderStyle;
        private GUIStyle s_LabelStyle;


        private GameObject GrassRoot = null;
        private GameObject SimpleGrassRoot = null;
        private Color normalColor = new Color(1f, 1f, 1f, 0.65f);
        private Color warningColor = new Color(1f, 0f, 0f, 0.65f);
        private Color warningColor2 = new Color(1f, 0.4f, 0f, 0.65f);
        public static bool IsShowed = false;
        public static bool IsPriorInPlaying = false;
        private int AllGrassNum = 0;

        void Awake()
        {
            GrassRoot = GameObject.Find("GrassSys");
            SimpleGrassRoot = GameObject.Find("SimpleGrass");
        }

        void OnDestroy()
        {
            IsShowed = false;
        }

        void OnGUI()
        {
            if (Application.isPlaying)
            {
                StatInPlaying();
            }else
            {
                RestSimpleGrassNum();
                StatInEditor();
            }
            
        }



        private void StatInPlaying()
        {
            if (GrassRoot == null)
            {
                return;
            }
            int oldfontsize = GUI.skin.label.fontSize;// GUI.skin.textField.fontSize;
            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
            GUI.color = normalColor;   
            int defsize = GUI.skin.textField.fontSize;
            GUI.skin.label.fontSize = 20;            
            // GUI.TextField(new Rect(20, 155, 150, 30), "显示数量：" + Common.TestVisibleInstanceNum.ToString() + " / " + Common.TestInstanceNum.ToString());
            // GUI.TextField(new Rect(20, 20, 200, 30), "块数(" + Common.TestGroupNum.ToString() + ")：" + Common.TestVisibleGroupNum.ToString());
            // GUI.TextField(new Rect(20, 55, 200, 30), "数量(" + Common.TestInstanceNum.ToString() + ")：" + Common.TestVisibleInstanceNum.ToString());            
            GUILayout.Label("块数(" + Common.TestGroupNum.ToString() + ")：" + Common.TestVisibleGroupNum.ToString(), labelStyle, new GUILayoutOption[0]);
            if(Common.TestVisibleInstanceNum > SimpleEditorCommon.MAX_GRASS_SHOWLIMIT)
            {
                GUI.color = warningColor;
            }
            else
            {
                GUI.color = normalColor;
            }            
            GUILayout.Label("数量(" + Common.TestInstanceNum.ToString() + ")：" + Common.TestVisibleInstanceNum.ToString(), labelStyle, new GUILayoutOption[0]);

            GUI.color = normalColor;
            //GUILayout.Label("Mesh：(" + Common.TestVisibleMeshNum.ToString() + ")" , labelStyle, new GUILayoutOption[0]);
            if (Common.TestRealVisibleInstNum > SimpleEditorCommon.MAX_GRASS_REALSHOWLIMIT)
            {
                GUI.color = warningColor2;
            }
            else
            {
                GUI.color = normalColor;
            }
            GUILayout.Label("*(" + Common.TestRealVisibleInstNum.ToString() + ")", labelStyle, new GUILayoutOption[0]);
            GUI.color = normalColor;
            if (GUILayout.Button("详细信息"))
            {
                if (Application.isPlaying)
                {
                    SimpleGrassCheckDataDialog.ShowDialog();
                }
            }
            //           
          //  GUI.skin.textField.fontSize = oldfontsize;
            GUI.skin.label.fontSize = oldfontsize;

            DrawDebugLogs();
        }

        private void StatInEditor()
        {
            if (SimpleGrassRoot == null)
            {
                return;
            }
            int oldfontsize = GUI.skin.label.fontSize;
            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
           // GUI.color = new Color(1f, 1f, 1f, 0.65f);
            int defsize = GUI.skin.textField.fontSize;
            GUI.skin.label.fontSize = 20;
            if (AllGrassNum > SimpleEditorCommon.MAX_GRASS_NUMLIMIT)
            {
                GUI.color = warningColor;
            }
            else
            {
                GUI.color = normalColor;
            }

            GUILayout.Label("数量： " + AllGrassNum.ToString(), labelStyle, new GUILayoutOption[0]);

            GUI.color = normalColor;

            if (GUILayout.Button("详细信息"))
            {
                //SimpleGrassCheckDataDialog.ShowDialog();
                GrassResInfoDialog.ShowDialog();
            }
            GUI.skin.label.fontSize = oldfontsize;

            DrawDebugLogs();
        }

        private void DrawDebugLogs()
        {
            if(!Common.ENABLE_DEBUGOUTLOG)
            {
                return;
            }
            EditorGUI.DrawRect(EditorGUILayout.BeginHorizontal(new GUIStyle(GUI.skin.box)), new Color(0.15f, 0.15f, 0.15f));
            Rect foldoutRect = GUILayoutUtility.GetRect(40f, 16f);
            string strName = "debuglog" + " Foldout";
            EditorPrefs.SetBool(strName, EditorGUI.Foldout(foldoutRect, EditorPrefs.GetBool(strName), "debuglog", true));
            EditorGUILayout.EndHorizontal();
            if (EditorPrefs.GetBool(strName))
            {
                EditorStyles.textArea.wordWrap = true;
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(800), GUILayout.Height(500));
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.TextArea(Common.DebugLogBuilder.ToString(), GUILayout.MaxWidth(800f));                
                ////EditorGUILayout.TextArea("afdasfdsf\n ggggg\n  uuuuuuu\n", GUILayout.MaxWidth(400f));                
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
            }
        }
        private void RestSimpleGrassNum()
        {
            AllGrassNum = 0;
            if(!SimpleGrassRoot)
            {
                return;
            }
            for (int i = 0; i != SimpleGrassRoot.transform.childCount; ++i)
            {
                Transform trf = SimpleGrassRoot.transform.GetChild(i);
                int grassNum = 0;
                for (int j = 0; j != trf.childCount; ++j)
                {
                    grassNum += trf.GetChild(j).childCount;
                }
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
    }
}
//#endif