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

    public class SimpleGrassCheckDataDialog : EditorWindow
    {

        [MenuItem("MY_Support/草系统/统计_保存的数据", false, 5002)]

        public static void ShowDialog()
        {
            SimpleGrassCheckDataDialog frm = EditorWindow.GetWindow<SimpleGrassCheckDataDialog>(true, "统计-保存的数据");
            //frm.autoRepaintOnSceneChange = true;
            frm.Show();
        }


        public class MyData
        {
            public string prototype = "";
            public int chuck = 0;
            public int lightmap = 0;
            public int grassNum = 0;
            public float cullMaxDist = 0;
            public string prefabName = "";

            public int maxMeshVecNum = 0;
            public int maxMeshTrisNum = 0;
            public int maxLodNum = 0;

            public GameObject prefab = null;
            public ProtoTypeData refProtoType;

            // public UnityEngine.Object obj;

            public MyData(string sprototype, int ichuck, int ilightmap, int igrassNum, float fcullMaxDist,
                int iMaxMeshVecNum, int iMaxMeshTrisNum, int iMaxLodNum, GameObject objPrefab, ProtoTypeData protoType)
            {
                prototype = sprototype;
                chuck = ichuck;
                lightmap = ilightmap;
                grassNum = igrassNum;
                cullMaxDist = fcullMaxDist;
                maxMeshVecNum = iMaxMeshVecNum;
                maxMeshTrisNum = iMaxMeshTrisNum;
                maxLodNum = iMaxLodNum;
                prefab = objPrefab;
                refProtoType = protoType;
                if (prefab)
                {
                    prefabName = prefab.name;
                }
                else
                {
                    if (protoType.Custom != null)
                    {
                        if (refProtoType.Custom.LODs.Count > 0 && refProtoType.Custom.LODs[0].Meshs.Count > 0)
                        {
                            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(protoType.Custom.LODs[0].Meshs[0]);
                            prefabName = Path.GetFileName(assetPath) + " ...";
                        }                            
                    }
                }
            }
        }


        public class GrassResData
        {
            public string name;
            public int chucknum;
            public int grassnum;
        }

        private List<GrassResData> GrassResDatas = new List<GrassResData>();

        private GUIStyle s_SectionHeaderStyle;
        private GUIStyle s_LabelStyle;
        //private Vector2 _scrollPos;
        //private Vector2 _scrollPos2;


        private bool IsShowDetial = false;
        private GameObject GrassRoot = null;
        //private int fontsize = 20;
        private bool IsDirty = true;
        private int totalGrassNum = 0;
        private int totalProtoNum = 0;


        private TableView _resourceRefTable = null;
        private List<List<object>> _resourceList = new List<List<object>>();
        void Awake()
        {
            //GrassRoot = GameObject.Find("SimpleGrass");
            GrassRoot = GameObject.Find("GrassSys");

            ResetTable();
        }

        void ResetTable()
        {
            if (_resourceRefTable != null)
                _resourceRefTable.Dispose();
            _resourceRefTable = null;

            _resourceRefTable = new TableView(this, typeof(MyData));

            if(! IsShowDetial)
            {
                _resourceRefTable.AddColumn("prototype", "种类", 0.3f, TextAnchor.MiddleLeft);
                _resourceRefTable.AddColumn("chuck", "块数", 0.1f);
                _resourceRefTable.AddColumn("lightmap", "LightMap数", 0.1f);
                _resourceRefTable.AddColumn("cullMaxDist", "裁剪距离", 0.15f);
                _resourceRefTable.AddColumn("grassNum", "数量", 0.15f);
                _resourceRefTable.AddColumn("prefabName", "预制体", 0.2f, TextAnchor.MiddleLeft);

            }else
            {
                _resourceRefTable.AddColumn("prototype", "种类", 0.3f, TextAnchor.MiddleLeft);
                _resourceRefTable.AddColumn("maxMeshVecNum", "最大顶点数", 0.05f);
                _resourceRefTable.AddColumn("maxMeshTrisNum", "最大面数", 0.05f);
                _resourceRefTable.AddColumn("maxLodNum", "Lod数", 0.05f);

                _resourceRefTable.AddColumn("chuck", "块数", 0.05f);
                _resourceRefTable.AddColumn("lightmap", "LightMap数", 0.1f);
                _resourceRefTable.AddColumn("cullMaxDist", "裁剪距离", 0.05f);

                _resourceRefTable.AddColumn("grassNum", "数量", 0.15f);
                _resourceRefTable.AddColumn("prefabName", "预制体", 0.2f, TextAnchor.MiddleLeft);
            }
            _resourceRefTable.OnSelected += TableView_ResourceSelected;
        }
        void OnDestroy()
        {
            if (_resourceRefTable != null)
                _resourceRefTable.Dispose();

            _resourceRefTable = null;
        }

        void OnGUI()
        {
            RefreshData();
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
            if ((IsShowDetial && (col == 0 || col == 8)) || (!IsShowDetial && (col == 0 || col == 5))) //if (col == 0 || col == 5)
            {
                //UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(foo.file);
                
                if (foo.prefab != null)
                {
                    EditorGUIUtility.PingObject(foo.prefab);
                    Selection.activeObject = foo.prefab;
                }
                else
                {
                    if (foo.refProtoType.Custom != null)
                    {
                        if (foo.refProtoType.Custom.LODs.Count > 0 && foo.refProtoType.Custom.LODs[0].Meshs.Count > 0)
                        {
                            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(foo.refProtoType.Custom.LODs[0].Meshs[0]);
                            // Debug.Log(assetPath);        
                            EditorGUIUtility.PingObject(foo.refProtoType.Custom.LODs[0].Meshs[0]);
                            Selection.activeObject = foo.refProtoType.Custom.LODs[0].Meshs[0];
                        }
                    }
                }                               
            }
            //else if (col == 1)
            //    EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(foo.depPath));        
        }

        private void RefreshOneTable()
        {
            if (!GrassRoot)
                return;
            SimpleGrassSys sys = GrassRoot.GetComponent<SimpleGrassSys>();
            if (!sys)
                return;
            if (!sys.SaveDataProfile)
                return;

            List<object> lst = new List<object>();

            totalGrassNum = 0;
            for (int i=0; i != sys.SaveDataProfile.ProtoTypes.Count; ++i)
            {
                string protoType = sys.SaveDataProfile.ProtoTypes[i].ProtoKey;
                int chuckNum = sys.SaveDataProfile.ProtoTypes[i].ChuckNum;
                float cullMaxDist = sys.SaveDataProfile.ProtoTypes[i].CullingMaxDist;
                Dictionary<int, int> Indexes = new Dictionary<int, int>();
                int lightMapNum = GetUseLightMapNum(sys.SaveDataProfile.ProtoTypes[i],ref Indexes);
                int grassNum = GetGrassNum(sys.SaveDataProfile, i);
                totalGrassNum += grassNum;
                GameObject prefab = sys.SaveDataProfile.ProtoTypes[i].GrassPrebab;
                int maxLodNum = 0;
                int maxMeshVecNum = 0;
                int maxMeshTrisNum = 0;
                if(prefab != null)
                {
                    GetDetailMeshInfo(prefab, out maxLodNum, out maxMeshVecNum, out maxMeshTrisNum);
                    maxMeshVecNum = maxMeshVecNum * grassNum;
                    maxMeshTrisNum = maxMeshTrisNum * grassNum;
                }                

                MyData obj = new MyData(protoType, chuckNum, lightMapNum, grassNum, cullMaxDist, maxMeshVecNum,maxMeshTrisNum, maxLodNum, prefab, sys.SaveDataProfile.ProtoTypes[i]);
                lst.Add(obj);
            }
            totalProtoNum = sys.SaveDataProfile.ProtoTypes.Count;
            
            _resourceList.Add(lst);
        }

        void GetDetailMeshInfo(GameObject prefab, out int maxLodNum, out int maxMeshVecNum,out int maxMeshTrisNum)
        {
            int iMaxLodNum = 0;
            int iMaxMeshVecNum = 0;
            int iMaxMeshTrisNum = 0;

            MeshFilter[] allMeshFilterAry = null;
            List<MeshFilter> listMeshFilter = new List<MeshFilter>();
            LODGroup lod = prefab.GetComponent<LODGroup>();
            if (lod)
            {
                iMaxLodNum = lod.GetLODs().Length;
                for(int i=0; i != iMaxLodNum; ++i)
                //if(iMaxLodNum > 0)
                {   //取LOD0                  
                     foreach (var render in lod.GetLODs()[i].renderers)
                    {
                        if (render is MeshRenderer)
                        {
                            //listMeshMaterial.Add(render.sharedMaterials);
                            MeshFilter meshFilter = render.gameObject.GetComponentInChildren<MeshFilter>();
                            if(meshFilter)
                               listMeshFilter.Add(meshFilter);
                        }
                    }
                }                
            }else
            {
                allMeshFilterAry = prefab.GetComponentsInChildren<MeshFilter>();
                if(allMeshFilterAry != null)
                {
                    listMeshFilter = allMeshFilterAry.ToList();
                }
            }
            ///////////////////
            iMaxMeshVecNum = 0;
            iMaxMeshTrisNum = 0;
            for(int i = 0; i != listMeshFilter.Count; ++i)
            {
                if (listMeshFilter[i].sharedMesh)
                {
                    iMaxMeshTrisNum += listMeshFilter[i].sharedMesh.triangles.Length / 3;
                    iMaxMeshVecNum += listMeshFilter[i].sharedMesh.vertexCount;
                }
            }

            maxLodNum = iMaxLodNum;
            maxMeshVecNum = iMaxMeshVecNum;
            maxMeshTrisNum = iMaxMeshTrisNum;
        }
        int GetGrassNum(SimpleSaveData saveData, int protoTypeIndex)
        {
            int num = 0;
            bool bStart = false;
            Common.TestInstNum_ByBuff.TryGetValue(protoTypeIndex, out num);

            for (int i=0; i != saveData.ChunkData.Count; ++i)
            {
                if (saveData.ChunkData[i].PrototypeIndex == protoTypeIndex)
                {
                    num += saveData.ChunkData[i].InstanceList.Count;
                    bStart = true;
                }else
                {
                    if(bStart)
                    {
                        return num;
                    }
                }
            }
            return num;
        }

    int GetUseLightMapNum(ProtoTypeData proto,  ref Dictionary<int,int> Indexes)
    {
        long tag = 1;
        for (int idx = 0; idx < proto.LightMapIndex.Count; ++idx)
        {
            //0-63
            long lmIndex = proto.LightMapIndex[idx];
            for (int i = 0; i <= 63; ++i)
            {
                long val = tag << i;
                if ((lmIndex & val) == val)
                {
                    if(!Indexes.ContainsKey(i))
                    {
                        Indexes[i] = 1;
                    }                    
                }
            }
        }


        if (proto.LightMapIndex2 != null)
        {
            for (int idx = 0; idx < proto.LightMapIndex2.Count; ++idx)
            {
                long lmIndex = proto.LightMapIndex2[idx];
                for (int i = 0; i <= 63; ++i)
                {
                    long val = tag << i;
                    if ((lmIndex & val) == val)
                    {
                        if (!Indexes.ContainsKey(64 + i))
                        {
                            Indexes[i] = 64 + i;
                        }
                    }
                }
            }
        }
        
        return Indexes.Count;
    }

        void RefreshTables()
        {
            if (_resourceRefTable == null)
            {
                return;
            }

            _resourceList.Clear();

            RefreshOneTable();

            //
            if (_resourceList.Count > 0)
            {
                _resourceRefTable.SetSortParams(0, false);
                _resourceRefTable.RefreshData(_resourceList[0]);
            }
            else
                _resourceRefTable.RefreshData(null);

            //大小排序        
        }

        public void RefreshData()
        {
            if (IsDirty)
            {
                RefreshTables();
                IsDirty = false;
            }

            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);            
            GUI.color = new Color(1f, 1f, 1f, 1f);

            GUILayout.BeginHorizontal(new GUILayoutOption[0]);

            if (GUILayout.Button("刷新"))
            {
                RefreshTables();
            }

            bool oldval = IsShowDetial;
            IsShowDetial = GUILayout.Toggle(IsShowDetial, "详细信息");
            if(oldval != IsShowDetial)           
            {
                ResetTable();
                RefreshTables();
            }
            int oldfontsize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = 15;
            GUILayout.Label("种类数量： " , GUILayout.Width(80));
            GUI.color = new Color(1f, 0f, 0f, 1f);
            GUILayout.Label(totalProtoNum.ToString(), GUILayout.Width(100));

            GUI.color = new Color(1f, 1f, 1f, 1f);
            GUILayout.Label("   总数： ", GUILayout.Width(80));
            GUI.color = new Color(1f, 0f, 0f, 1f);
            GUILayout.Label(totalGrassNum.ToString(), GUILayout.Width(100));
   

            GUILayout.EndHorizontal();
            GUI.skin.label.fontSize = oldfontsize;


            GUI.color = new Color(1f, 1f, 1f, 1f);
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            float toolbar = 30.0f;
            float padding = 30.0f;

            var height = toolbar + padding;
            {
                DrawTable(_resourceRefTable, new Rect(padding, height, position.width - padding * 2f,
                 position.height - height - padding));
            }

            GUILayout.EndVertical();

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

        //private void Stat()
        //{
        //    if (GrassRoot == null)
        //    {
        //        return;
        //    }

        //    int oldfontsize = GUI.skin.textField.fontSize;
        //    GrassResDatas.Clear();
        //    GUI.color = Color.white;
        //    GUI.skin.textField.fontSize = fontsize;

        //    int AllGrassNum = 0;
        //    for (int i = 0; i != GrassRoot.transform.childCount; ++i)
        //    {
        //        Transform trf = GrassRoot.transform.GetChild(i);

        //        GrassResData data = new GrassResData();
        //        GrassResDatas.Add(data);
        //        data.name = trf.name;
        //        data.chucknum = trf.childCount;
        //        int grassNum = 0;
        //        for (int j = 0; j != trf.childCount; ++j)
        //        {
        //            grassNum += trf.GetChild(j).childCount;
        //        }
        //        data.grassnum = grassNum;

        //        AllGrassNum += grassNum;

        //    }
        //    GUI.color = Color.white;
        //    //GUI.color = Color.red;
        //    GUI.contentColor = Color.white;
        //    GUI.TextField(new Rect(20, 20, 150, 30), "种类数量： " + GrassRoot.transform.childCount.ToString());
        //    GUI.contentColor = Color.red;
        //    GUI.TextField(new Rect(155, 20, 300, 30), "   总数： " + AllGrassNum.ToString());
        //    int startY = 20;

        //    GUI.contentColor = Color.white;
        //    for (int i = 0; i != GrassResDatas.Count; ++i)
        //    {
        //        startY = startY + 40;
        //        GUI.TextField(new Rect(35, startY, 350, 30), i.ToString() + ": 种类名称：" + GrassResDatas[i].name);

        //        startY = startY + 30;
        //        GUI.TextField(new Rect(35, startY, 350, 30), "    块数：" + GrassResDatas[i].chucknum.ToString());

        //        startY = startY + 30;

        //        GUI.TextField(new Rect(35, startY, 350, 30), "  草数量：" + GrassResDatas[i].grassnum.ToString());

        //    }

        //    GUI.skin.textField.fontSize = oldfontsize;
        //}


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