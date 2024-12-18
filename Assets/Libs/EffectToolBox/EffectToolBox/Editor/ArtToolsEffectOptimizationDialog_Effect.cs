using System.Collections.Generic;

using UnityEngine;
using UnityEditor;


namespace GameSupport.EffectToolBox
{
    public class ArtToolsEffectOptimizationCheker_Effect : EditorWindow
    {

        public static ArtToolsEffectOptimizationChecker_Effect mChecker_Effect = new ArtToolsEffectOptimizationChecker_Effect();


        public bool IsDirty = true;
        public string HintStr = "";

        private GUIStyle s_SectionHeaderStyle;
        private GUIStyle s_LabelStyle;
        private Vector2 _scrollPos;
        private Vector2 _scrollPos2;

        private TableView _resourceRefTable = null;
        private List<List<object>> _resourceList = new List<List<object>>();

        public static void ShowDialog(string hint = null)
        {
            ArtToolsEffectOptimizationCheker_Effect frm = EditorWindow.GetWindow<ArtToolsEffectOptimizationCheker_Effect>();            
            frm.Show();
        }

        public static void Refresh(string hint = null)
        {
            ArtToolsEffectOptimizationCheker_Effect frm = EditorWindow.GetWindow<ArtToolsEffectOptimizationCheker_Effect>();
            if (hint != null)
                frm.HintStr = hint;
            //if (mChecker_Texture.mData.Count > 0)
            {
                frm.IsDirty = true;
            }
        }


        public static void ClearAll()
        {
            mChecker_Effect.mData.Clear();
            mChecker_Effect.mGameObjectList.Clear();
        }

        void Awake()
        {
            _resourceRefTable = new TableView(this, typeof(MyData));
            _resourceRefTable.AddColumn("file", "对象", 0.6f, TextAnchor.MiddleLeft);
            _resourceRefTable.AddColumn("reason", "输出原因", 0.3f);
            _resourceRefTable.OnSelected += TableView_ResourceSelected;

            //  RefreshTables();
        }


        void OnGUI()
        {
            //#if UNITY_EDITOR
            // if (m_isShow)
            {
                SceneStatsGUI();
            }
            //#endif
        }

        private void OnDisable()
        {
            ClearAll();
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

        public void SceneStatsGUI()
        {
            if (IsDirty)
            {
                RefreshTables();
            }

            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
            GUI.color = new Color(1f, 1f, 1f, 1f);

            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            // GUILayout.Label("检测使用GPU INS_材质", GUILayout.Width(80));

            //mIsUseGPUIns = EditorGUILayout.Toggle("检测使用GPU INS_材质", mIsUseGPUIns);

            //if (GUILayout.Button("扫描当前场景"))
            //{
            //    ClearAll();
            //    //附加场景文件所在目录

            //    mChecker_MotionVectors.mIsUseGPUIns = mIsUseGPUIns;
            //    mChecker_MotionVectors.Execute();

            //    RefreshTables();

            //    HintStr = "扫描整个场景 ";
            //}

            if (GUILayout.Button("扫描当前目录"))
            {
                ClearAll();
                var paths = CustomResChecker.GetAllPathsBySelect();
                if (paths == null || paths.Length == 0)
                {
                    EditorUtility.DisplayDialog("提示：", "请重新定位扫描目录！！", "Yes");
                    return;
                }

                mChecker_Effect.SetScanDirs(paths);
                mChecker_Effect.Execute();

                HintStr = "【扫描目录】: ";
                for (int i = 0; i < paths.Length; ++i)
                    HintStr += paths[i];

                RefreshTables();
            }

            if (GUILayout.Button("扫描场景目录"))
            {
                ClearAll();
                string[] dirs = new string[] { "assets/gameres/map", "assets/gameres/map_obj" };
                mChecker_Effect.SetScanDirs(dirs);
                mChecker_Effect.Execute();
                RefreshTables();

                HintStr = "扫描整个场景 ";
            }

            if (GUILayout.Button("修复"))
            {
                mChecker_Effect.Process();
                ClearAll();
                RefreshTables();
            }

            if (GUILayout.Button("刷新"))
            {
                RefreshTables();
            }



            if (GUILayout.Button("清空"))
            {
                HintStr = "";
                ClearAll();
                RefreshTables();
            }


            GUILayout.EndHorizontal();


            //////////////////
            GUILayout.Space(4);
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUILayout.Label(HintStr, GUILayout.Width(800));
            GUILayout.EndHorizontal();
            /////////////////

            GUILayout.Space(10);


            GUILayout.BeginVertical();
            float toolbar = 30.0f;
            float padding = 30.0f;


            var height = toolbar + padding;

            {
                DrawTable(_resourceRefTable, new Rect(padding, height, position.width - padding * 2f,
                 position.height - height - padding));
            }

            DrawResult();
            GUILayout.EndVertical();

            //   

        }

        void OnDestroy()
        {
            if (_resourceRefTable != null)
                _resourceRefTable.Dispose();

            _resourceRefTable = null;
        }



        void DrawTable(TableView table, Rect rect)
        {
            if (table != null)
            {
                GUILayout.BeginArea(rect);
                table.Draw(new Rect(0, 0, rect.width, rect.height));
                GUILayout.EndArea();
            }
        }

        void DrawResult()
        {

        }


        private void RefreshTables()
        {
            if (_resourceRefTable == null)
            {
                return;
            }

            _resourceList.Clear();

            RefreshOneTable(mChecker_Effect.mData);

            //
            if (_resourceList.Count > 0)
            {
                _resourceRefTable.SetSortParams(2, true);
                _resourceRefTable.RefreshData(_resourceList[0]);
            }
            else
                _resourceRefTable.RefreshData(null);

            //大小排序        

            IsDirty = false;
        }


        private void RefreshOneTable(Dictionary<string, MyData> data)
        {
            if (data.Count == 0)
                return;
            List<object> lst = new List<object>();
            foreach (KeyValuePair<string, MyData> item in data)
            {

                /*CsvLoader csv = */
                MyData obj = new MyData(item.Value.file, item.Value.reason, item.Value.memory, item.Value.isreadwrite);
                lst.Add(obj);
            }
            _resourceList.Add(lst);
        }

        private void TableView_ResourceSelected(object selected, int col)
        {
            MyData foo = selected as MyData;
            if (foo == null)
            {
                Debug.LogErrorFormat("the selected object is not a valid one. ({0} expected, {1} got)",
                    typeof(MyData).ToString(), selected.GetType().ToString());
                return;
            }
            if (col == 0 || col == 1)
            {
                string strKey = foo.file;
                Selection.activeObject = mChecker_Effect.mGameObjectList[strKey];
            }
        }


    }
}
//#endif