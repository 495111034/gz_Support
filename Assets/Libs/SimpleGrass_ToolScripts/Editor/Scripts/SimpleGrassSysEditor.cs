using UnityEngine;
using UnityEditor;
using UnityEditor.Macros;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditorInternal;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

namespace SimpleGrass
{

    [System.Serializable]
    public class PrototypeItem_Editor
    { 

        [SerializeField]
        public Texture icon;
        [SerializeField]
        public GameObject prefab;
        [SerializeField]
        public string name;
        [SerializeField]
        public int index;

    }

    [CustomEditor(typeof(SimpleGrassSys))]
    public partial class SimpleGrassSysEditor : Editor
    {
        public class MyStyles
        {
            public static Color headerColor = new Color(0.15f, 0.15f, 0.15f);
            public static GUIStyle moduleHeader;
            public static GUIStyle title;
            public static GUIStyle textRight;
            public static GUIStyle foundText;
            public static Color textHightLightColor = new Color(0.78f, 1.0f, 1.0f);

            static MyStyles()
            {
                moduleHeader = new GUIStyle(GUI.skin.box);
                moduleHeader.padding = new RectOffset(32, 10, 3, 3);

                title = new GUIStyle(GUI.skin.label);
                title.alignment = TextAnchor.MiddleCenter;

                textRight = new GUIStyle(GUI.skin.label);
                textRight.alignment = TextAnchor.UpperRight;

                foundText = new GUIStyle(GUI.skin.label);
                foundText.fontStyle = FontStyle.Italic;
                foundText.fontSize = 14;
            }
        }

        private bool priorIsActiveAndEnabled = true;
        private SimpleGrassSys grassSys = null;
        private Color brushColor = Color.blue; // 线框颜色  
        private float minGrassPatchDist = 5f;//草块UI上最小距离
        private bool profileChanges = false;
        private Vector3 lastMousePos;
        private GameObject RootNode;
        private GameObject ColliderRootNode;

        private GUIContent[] chunkPrefab_;
        private int[] chunkPrefab_Index;
        private List<GameObject> chunkPrefab_Values = new List<GameObject>();
        private List<string> chunkPrefab_Paths = new List<string>();
        private int chunkPrefabIndex = -1;

        private ReorderableList prototypeList;
        private List<PrototypeItem_Editor> prototypePrefabList = new List<PrototypeItem_Editor>();
        private Vector2 scrollPosition = Vector2.zero;

        private GUIContent[] profile_;
        private int[] profile_Index;
        private List<string> profilePaths = new List<string>();
        private int profileIndex = -1;

        //private bool showHint = false;
        private string lastProtoType = "";
        private int lastProtoIndex = 0;
        private Action<string> PrototypeCallBack;
        private string helpStr = "";
        private string helpDrawingStr = "";

        private string searchText = "";
        private Dictionary<int, int> searchIndexDic = new Dictionary<int, int>();

        #region Mesh Preview
        private PreviewRenderUtility m_PreviewRenderUtility;
        private MeshFilter m_TargetMeshFilter;
        private MeshRenderer m_TargetMeshRenderer;
        private Vector2 m_Drag;
        #endregion


        SerializedProperty _hintInfo, _EditorMode, _Editor_ViewDist, _PaintingLayerMask, _GrassChunkPrefab, _KindName, _RandomRot,
            _RandomRotMin, _RandomRotMax, _MinMaxScale, _BrushRadius, _Density, _StartRadi, _StartLength, _MinRadi,
            _MinAvoidDist, _OnNormal, _RayCastDist, _Profile, _WorldBounds, _WorldCellResolution, _SaveDataProfile, _AutoSave;//_Interactive,_MoveWithObject, _BrushErase, 

        #region System Function

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            if (Application.isPlaying)
            {
                return;
            }
            //是否自动保存（保存场景和烘焙光照结束，自动保存)
            // UnityEditor.SceneManagement.EditorSceneManager.sceneSaving -= OnEditorSceneManagerBeforeSceneSaved;
            //  UnityEditor.SceneManagement.EditorSceneManager.sceneSaving += OnEditorSceneManagerBeforeSceneSaved;

            Lightmapping.bakeCompleted -= BakeCompleted;
            Lightmapping.bakeCompleted += BakeCompleted;
            // Debug.LogFormat("SimpleGrassSysEditor: Initialize {0}", "");
        }

        static void OnEditorSceneManagerSceneOpened(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
        {
            Debug.LogFormat("SceneOpened: {0}", scene.name);
        }

        static void OnEditorSceneManagerBeforeSceneSaved(UnityEngine.SceneManagement.Scene scene, string path)
        {
            if (Application.isPlaying)
            {
                return;
            }           
            if (SaveGrassData())
            {
                Debug.LogFormat("SceneSaved: GrassData Saved {0}", scene.name);
            }
        }

        static void BakeCompleted()
        {
            if (Application.isPlaying)
            {
                return;
            }            
            if (SaveGrassData())
            {
                string curSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                Debug.LogFormat("BakeCompleted: GrassData Saved {0}", curSceneName);
            }
        }

        public static bool SaveGrassData(bool forceSave = false)
        {
            if (UnityEngine.SceneManagement.SceneManager.sceneCount > 1)
            {
                Debug.LogFormat("SceneSaved: GrassData保存失败，打开了多个场景 ");
                return false;
            }

            SimpleGrassSys grassSys = GameObject.FindObjectOfType<SimpleGrassSys>();
            if (grassSys == null )
            {
                return false;
            }
            if(!grassSys.AutoSave && forceSave == false)
            {
                return false;
            }

            GameObject root = GameObject.Find("SimpleGrass");
            if(root == null)
            {
                return false;
            }
            GameObject ColliderRootNode = null;
            //保存植被数据
            SimpleSaveData saveData = grassSys.SaveDataProfile;
            if (saveData != null)
            {
                SimpleGrassSysDataHandler.SaveData(ref root, ref ColliderRootNode, ref saveData, grassSys.SaveToBuffer,grassSys.OptimizeChunkParams,grassSys.OptimizeSaveChunk);

                if (grassSys.RefreshLightmapDatas())
                {
                    SimpleInstancingMgr instMgr = grassSys.transform.GetComponent<SimpleInstancingMgr>();
                    if (instMgr != null)
                    {
                        EditorUtility.SetDirty(instMgr);
                    }
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                return true;
            }
            return false;
        }

        #endregion


        #region Unity Function

        private void Awake()
        {
            //if(Application.isPlaying)
            //{
            //    SimpleGrassHintInfoDialog.ControlDialog(grassSys, false);
            //}
            PrototypeCallBack = new Action<string>(PrototypeOperaterCallBack);
            PrototypeCallBack += PrototypeOperaterCallBack;
        }        
        

        void OnEnable()
        {
            helpStr = " ";
           helpDrawingStr = " # [开启Gizmos??] [Not OnSceneGUI?]";
        //titleColor = EditorGUIUtility.isProSkin ? new Color(0.52f, 0.66f, 0.9f) : new Color(0.12f, 0.16f, 0.4f);

           grassSys = target as SimpleGrassSys;
            if (grassSys.isActiveAndEnabled)
            {
                SceneView.duringSceneGui -= _OnSceneGUI;
                SceneView.duringSceneGui += _OnSceneGUI;
            }
            priorIsActiveAndEnabled = grassSys.isActiveAndEnabled;

            GameObject root = GameObject.Find("SimpleGrass");
            if (root != null)
            {
                this.RootNode = root;
            }

            this.ColliderRootNode = GameObject.Find(SimpleEditorCommon.SaveColliderNodeName);

            //迁移Profile配置文件目录
            //ConvertProfileDir();

            RefreshProperty();

            //创建相关的资源路径
            CreateDefaultAssetPath();

            RefreshPrototypeData();

            ////保存的最近一次操作的植被种类
            //ResetLastProtoType();

            ResetChunkPrefabIndex();

            GetProfilesData();
            ResetProfileIndex();


            SimpleGrassHintInfoDialog.ControlDialog(grassSys, false);
            // m_PreviousTime = EditorApplication.timeSinceStartup;     

            DefinePrototypeList();
        }

        void OnDisable()
        {
            priorIsActiveAndEnabled = false;
            SceneView.duringSceneGui -= _OnSceneGUI;

            if (!Application.isPlaying)
            {
                SimpleGrassHintInfoDialog.CloseDialog();
            }

            if (m_PreviewRenderUtility != null)
            {
                m_PreviewRenderUtility.Cleanup();
            }
        }

        public override void OnInspectorGUI()
        {
            grassSys = target as SimpleGrassSys;
            if(priorIsActiveAndEnabled != grassSys.isActiveAndEnabled)
            {
                if (!grassSys.isActiveAndEnabled)
                {
                    SceneView.duringSceneGui -= _OnSceneGUI;                
                }
                else
                {
                    SceneView.duringSceneGui -= _OnSceneGUI;
                    SceneView.duringSceneGui += _OnSceneGUI;
                }
                priorIsActiveAndEnabled = grassSys.isActiveAndEnabled;
            }
           
            if (!_EditorMode.boolValue)
            {
                helpStr = "帮助: [没在编辑模式] ";
            }
            else
            {
                helpStr = "帮助: [编辑中] ";
            }

       //     base.OnInspectorGUI();
            serializedObject.UpdateIfRequiredOrScript();


            EditorGUILayout.Separator();
            Color defColor = GUI.color;
            GUI.color = MyStyles.textHightLightColor;
            GUILayout.BeginHorizontal();
            GUILayout.Label(helpStr + helpDrawingStr, GUILayout.MaxWidth(500));
            GUILayout.EndHorizontal();
            GUI.color = defColor;

            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("修复", "修复 ."), GUILayout.Width(60)))
            {
                _RepareGrasses();
            }
            if (GUILayout.Button(new GUIContent("...", "弹出工具窗口..."), GUILayout.Width(60)))
            {
                SimpleGrassTest.SimpleGrassToolEditor.OpenWindow();
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(_hintInfo, new GUIContent("HintInfo"));

            if (grassSys.HintInfo)
            {
                if (GUILayout.Button(new GUIContent("显示提示", ""), GUILayout.Width(80)))
                {
                    SimpleGrassHintInfoDialog.ControlDialog(grassSys, false);
                }
            }
            else
            {
                if (GUILayout.Button(new GUIContent("关闭提示", ""), GUILayout.Width(80)))
                {
                    SimpleGrassHintInfoDialog.ControlDialog(grassSys, false);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_EditorMode, new GUIContent("EditorMode"));
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            #region  Profile 植被种类定义及编辑
            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();
            int oldLastProtoIndex = this.lastProtoIndex;
            this.lastProtoIndex = EditorGUILayout.IntPopup(new GUIContent("植被"), this.lastProtoIndex, chunkPrefab_, chunkPrefab_Index);
            ResetLastProtoIndex();            
            if (oldLastProtoIndex != this.lastProtoIndex)
            {
                SaveLastProtoType(this.lastProtoIndex,false);
            }
            GUILayout.EndHorizontal();            
            //新增、编辑、删除
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(130));
            if (GUILayout.Button(new GUIContent("新建", "Creates a new ProtoType."), GUILayout.Width(60)))
            {
                SimpleGrassPrototypeQuickSetup.NewPrototypeWindow(PrototypeCallBack);
            }
            if (GUILayout.Button(new GUIContent("编辑", "Edit current selected ProtoType configuration."), GUILayout.Width(60)))
            {
                if (this.lastProtoIndex >= 0)
                {
                    string prefabPath = this.chunkPrefab_Paths[this.lastProtoIndex];
                    SimpleGrassPrototypeQuickSetup.EditPrototypeWindow(prefabPath, PrototypeCallBack);
                }
            }
            if (GUILayout.Button(new GUIContent("定位", "Locate current selected ProtoType."), GUILayout.Width(60)))
            {
                if (this.lastProtoIndex >= 0)
                {
                    string prefabPath = this.chunkPrefab_Paths[this.lastProtoIndex];
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                }
            }
            if (GUILayout.Button(new GUIContent("删除", "Delete current selected ProtoType configuration."), GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("删除", "确定要删除该植被配置？", "确定", "取消"))
                {
                    if (this.lastProtoIndex >= 0)
                    {
                        string prefabPath = this.chunkPrefab_Paths[this.lastProtoIndex];
                        AssetDatabase.DeleteAsset(prefabPath);
                        AssetDatabase.Refresh();
                        RefreshPrototypeData();
                        this.lastProtoIndex = 0;
                        ResetLastProtoIndex();
                        SaveLastProtoType(this.lastProtoIndex,false);
                    }
                }
            }
            GUILayout.EndHorizontal();

            DrawPrototypeListGUI("植被图例");

            #endregion

            #region  Profile 相关编辑
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();
            int newprofileIndex = EditorGUILayout.IntPopup(new GUIContent("画刷"), profileIndex, profile_, profile_Index);
            if (newprofileIndex != profileIndex)
            {
                profileIndex = newprofileIndex;
                ResetProfilesData();
                ResetChunkPrefabIndex();
                GUIUtility.ExitGUI();
                return;
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();

            //SimpleGrassProfile newProfile = (SimpleGrassProfile)EditorGUILayout.ObjectField(new GUIContent("配置文件", "Create or load stored presets."), grassSys.Profile, typeof(SimpleGrassProfile), false);
            EditorGUILayout.PropertyField(_Profile, new GUIContent("画刷配置"));
            if (_Profile.objectReferenceValue != grassSys.Profile)
            {
                grassSys.Profile = (SimpleGrassProfile)(_Profile.objectReferenceValue);
                ResetProfileIndex();
                ResetChunkPrefabIndex();
                GUIUtility.ExitGUI();
                return;
            }

            if (grassSys.Profile != null)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(130));
                if (GUILayout.Button(new GUIContent("新建", "Creates a new Profile which is a copy of the current settings."), GUILayout.Width(60)))
                {
                    CreateProfile();

                    GetProfilesData();
                    ResetProfileIndex();
                    ResetChunkPrefabIndex();


                    profileChanges = false;
                    GUIUtility.ExitGUI();
                    return;
                }
                if (GUILayout.Button(new GUIContent("恢复", "恢复."), GUILayout.Width(60)))
                {
                    profileChanges = false;
                    grassSys.Profile.Load(grassSys);

                    GetProfilesData();
                    ResetProfileIndex();
                    ResetChunkPrefabIndex();
                }
                if (!profileChanges)
                    GUI.enabled = false;
                if (GUILayout.Button(new GUIContent("保存", "Updates Profile configuration with changes in this inspector."), GUILayout.Width(60)))
                {
                    profileChanges = false;
                    grassSys.Profile.Save(grassSys);

                    EditorUtility.SetDirty(grassSys.Profile);
                    AssetDatabase.SaveAssets();
                }
                GUI.enabled = true;
            }
            else
            {
                if (GUILayout.Button(new GUIContent("新建", "Creates a new Profile which is a copy of the current settings."), GUILayout.Width(60)))
                {
                    CreateProfile();

                    GetProfilesData();
                    ResetProfileIndex();
                    ResetChunkPrefabIndex();

                    GUIUtility.ExitGUI();
                    return;
                }
            }


            EditorGUILayout.EndHorizontal();
            #endregion

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
       
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_PaintingLayerMask, new GUIContent("PaintLayer","刷草的图层掩码"));//图层掩码

            if (grassSys.Profile != null)
            {
                int layerVal = grassSys.Profile.PaintingLayerMask.value;
                if (layerVal != _PaintingLayerMask.intValue)
                {
                    profileChanges = true;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_Editor_ViewDist, new GUIContent("EditorViewDist", "编辑视野距离"));//视野最大距离
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_RayCastDist, new GUIContent("RayDist", "射线距离"));//射线最大距离
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_BrushRadius, new GUIContent("BrushRadius", "画刷半径"));//画刷半径
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_Density, new GUIContent("Density", "密度范围"));//密度
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_StartRadi, new GUIContent("StartRadius(X-Z)", "开始比例(X-Z)， 每次0.7系数衰减到EndRadius, 每次刷[Density]棵"));//开始比例(X-Z)
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_MinRadi, new GUIContent("EndRadius(X-Z)", "结束比例(X-Z)"));//最小比例(X-Z)
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_StartLength, new GUIContent("StartLength", "开始高度比例, 按0.8系数进行衰减"));//开始高度
            GUILayout.EndHorizontal();

            

            //GUILayout.BeginHorizontal();
            //EditorGUILayout.PropertyField(_MinAvoidDist, new GUIContent("MinAvoidDist"));//回避距离
            //GUILayout.EndHorizontal();


            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_RandomRot, new GUIContent("RandomRot", "随机旋转"));//随机旋转
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _RandomRotMin.floatValue = Mathf.Clamp(_RandomRotMin.floatValue, 0.0f, 360);
            EditorGUILayout.PropertyField(_RandomRotMin, new GUIContent("RandomRotMin", "随机旋转(最小值)"));//随机旋转(最小值)
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _RandomRotMax.floatValue = Mathf.Clamp(_RandomRotMax.floatValue, 0.0f, 360);
            EditorGUILayout.PropertyField(_RandomRotMax, new GUIContent("RandomRotMax", "随机旋转(最大值)"));//随机旋转(最大值)
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_MinMaxScale, new GUIContent("EndScale(X-Z)"));//最终随机比例(X-Z)
            GUILayout.EndHorizontal();


            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_OnNormal, new GUIContent("OnNormal"));//朝向法线
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            #region 刷新，保存数据
            //WorldGrassAreaMgr worldGrassMgr = this.grassSys.GetComponent<WorldGrassAreaMgr>();
            //bool useWorld = (worldGrassMgr != null && worldGrassMgr.enabled);
            bool useWorld = false;
            int openedSceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
            
            if (useWorld)
            {
                //世界相关GUI
                //_OnInspectorGUI_WorldContent(openedSceneCount);
            }
            else
            { 
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_SaveDataProfile, new GUIContent("保存数据"));                
                if (_SaveDataProfile.objectReferenceValue != grassSys.saveDataProfile)
                {
                    grassSys.saveDataProfile = (SimpleSaveData)(_SaveDataProfile.objectReferenceValue);
                }

                //  grassSys.saveDataProfile = (SimpleSaveData)EditorGUILayout.ObjectField(new GUIContent("保存数据", "Create or load stored presets."), grassSys.saveDataProfile, typeof(SimpleSaveData), false);
                if (openedSceneCount == 1)
                {
                    if (grassSys.SaveDataProfile != null)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("", GUILayout.Width(130));
                        if (GUILayout.Button(new GUIContent("创建", "Creates a new Profile "), GUILayout.Width(60)))
                        {
                            CreateSaveDataProfile();
                            RefreshLightmapDatas();
                            GUIUtility.ExitGUI();
                            return;
                        }
                        if (GUILayout.Button(new GUIContent("保存/刷新", "Updates data to file."), GUILayout.Width(60)))
                        {
                            GameObject root = this.RootNode;
                            SimpleSaveData saveData = grassSys.SaveDataProfile;
                            SimpleGrassSysDataHandler.SaveData(ref root, ref this.ColliderRootNode, ref saveData, grassSys.SaveToBuffer, grassSys.OptimizeChunkParams, grassSys.OptimizeSaveChunk);
                            RefreshLightmapDatas();
                        }
                        GUI.enabled = true;
                    }
                    else
                    {
                        if (GUILayout.Button(new GUIContent("创建", "Creates a new Profile ."), GUILayout.Width(60)))
                        {
                            CreateSaveDataProfile();
                            RefreshLightmapDatas();
                            GUIUtility.ExitGUI();
                            return;
                        }
                    }
                }
                if (EditorGUI.EndChangeCheck())
                {
                    RefreshLightmapDatas();
                }

                //}
                #endregion
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_AutoSave, new GUIContent("AutoSave"));//是否自动保存（保存场景和烘焙光照结束，自动保存)
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();


            
            
            if (serializedObject.ApplyModifiedProperties() || (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "UndoRedoPerformed"))
            {
                if (grassSys.Profile != null)
                    profileChanges = true;
            }
        }

        public void _OnSceneGUI(SceneView sceneView)
        {
            helpDrawingStr = " # ";
            if (Application.isPlaying || grassSys == null)
            {
                //Debug.Log("#GrassEditor: return1 grassSys == null");
                return;
            }


            Event cur = Event.current;

            if (cur.type == EventType.KeyUp && cur.keyCode == (KeyCode.Escape))
            {
                grassSys.EditorMode = !grassSys.EditorMode;
            }

            if (!grassSys.EditorMode)
            {
                //Debug.Log("#GrassEditor: return2 !grassSys.EditorMode");
                return;
            }

            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            //画圈
            bool bReadyAdd = false;
            Ray ray = HandleUtility.GUIPointToWorldRay(cur.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                if (grassSys.Erasing)
                {
                    if (grassSys.BrushErase)
                    {
                        Handles.color = Color.red;
                        float fRadius = grassSys.BrushRadius;
                        Handles.DrawWireDisc(hit.point, hit.normal, fRadius * 1.2f);
                    }
                    else
                    {
                        Handles.color = Color.red;
                        Handles.DrawWireDisc(hit.point, hit.normal, 12f);
                    }
                }
                else
                {
                    int layerMask = 1 << hit.collider.gameObject.layer;
                    if ((layerMask & grassSys.PaintingLayerMask) != layerMask)
                    {
                        // Debug.Log("#GrassEditor: return3 layer:" + hit.collider.gameObject.layer);
                        helpDrawingStr += " 选中: [非画刷图层]" + hit.collider.gameObject.name;
                        return;
                    }

                    Handles.color = brushColor;
                    float fRadius = grassSys.BrushRadius;
                    Handles.DrawWireDisc(hit.point, hit.normal, fRadius * 1.2f);

                    Handles.color = Color.green;
                    Handles.DrawLine(hit.point, hit.point + hit.normal * fRadius * 1.6f);
                    bReadyAdd = true;

                    helpDrawingStr += " 选中: " + hit.collider.gameObject.name;
                }
                SceneView.RepaintAll();
            }
            else
            {
                helpDrawingStr += " 选中: 无 ";
            }
            ///////
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
            }
            else if (cur.type == EventType.KeyDown & (cur.keyCode == (KeyCode.LeftControl) || cur.keyCode == (KeyCode.LeftCommand) || cur.keyCode == (KeyCode.LeftAlt))
              || (cur.type == EventType.MouseDrag || cur.type == EventType.MouseDown) && (cur.alt && (cur.control || cur.command))
              )
            {
                //rotate camera
                //Debug.Log("Lock camera");
            }
            else
            {
                //"Camera rot
                grassSys.Looking = false;
                if (cur.modifiers > 0)
                {
                    //if ((cur.modifiers) == EventModifiers.Control) {
                    if ((cur.modifiers) == EventModifiers.Alt || (cur.isKey && cur.type == EventType.KeyDown) || (cur.modifiers) == EventModifiers.Command
                         || (cur.modifiers) == EventModifiers.Numeric || (cur.modifiers) == EventModifiers.FunctionKey)//|| (cur.modifiers) == EventModifiers.Control
                    {
                        //Debug.Log("look");
                        grassSys.Looking = true;
                    }
                }

                //按下Shift,没按下Ctrl键，进行擦除
                grassSys.Erasing = ((cur.modifiers & EventModifiers.Shift) != 0) && ((cur.modifiers & EventModifiers.Control) == 0);

                bool leftBtnPressed = (cur.button == 0);
                // bool rightBtnPressed = (cur.button == 1);//(!script.leftMousePaint && cur.button == 1) ||
                if (!grassSys.Looking && cur.keyCode != (KeyCode.LeftControl) && cur.keyCode != (KeyCode.LeftAlt) && cur.type != EventType.KeyDown &&
                      //((cur.type == EventType.MouseDown && (leftBtnPressed || rightBtnPressed))
                      //  || ((cur.type == EventType.MouseDrag && rightBtnPressed && Vector3.Distance(lastMousePos, cur.mousePosition) > minGrassPatchDist)))
                      ((cur.type == EventType.MouseDown && (leftBtnPressed))
                       || ((cur.type == EventType.MouseDrag && leftBtnPressed && Vector3.Distance(lastMousePos, cur.mousePosition) > minGrassPatchDist)))
                    )
                {
                    lastMousePos = cur.mousePosition;

                    bool bInEditViewDist = (Vector3.Distance(hit.point, Camera.current.transform.position) < grassSys.Editor_ViewDist);

                    if (bInEditViewDist)
                    {
                        //放置、种植
                        if (bReadyAdd && !grassSys.Erasing)
                        {
                            if (cur.type == EventType.MouseDrag)
                            {
                                if ((cur.modifiers & EventModifiers.Control) != 0)
                                    PrepareAddGrass(hit);
                            }
                            else
                                PrepareAddGrass(hit);
                        }
                        //擦除
                        if (grassSys.Erasing)
                        {
                            PrepareEraseGrass(ray, hit);
                        }
                    }
                    else
                    {
                        Debug.Log("放置位置，超出操作距离！");
                    }
                }
            }
            //////


            HandleUtility.AddDefaultControl(controlId);

        }

        public override bool HasPreviewGUI()
        {
            ValidateData();

            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            m_Drag = Drag2D(m_Drag, r);

            if (Event.current.type == EventType.Repaint)
            {
                if (m_TargetMeshRenderer == null)
                {
                    EditorGUI.DropShadowLabel(r, "Mesh Renderer Required");
                }
                else
                {
                    m_PreviewRenderUtility.BeginPreview(r, background);

                    m_PreviewRenderUtility.DrawMesh(m_TargetMeshFilter.sharedMesh, Matrix4x4.identity, m_TargetMeshRenderer.sharedMaterial, 0);

                    m_PreviewRenderUtility.camera.transform.position = Vector2.zero;
                    m_PreviewRenderUtility.camera.transform.rotation = Quaternion.Euler(new Vector3(-m_Drag.y, -m_Drag.x, 0));
                    m_PreviewRenderUtility.camera.transform.position = m_PreviewRenderUtility.camera.transform.forward * -8f;
                    m_PreviewRenderUtility.camera.Render();

                    var result_render = m_PreviewRenderUtility.EndPreview();
                    GUI.DrawTexture(r, result_render, ScaleMode.StretchToFill, false);
                }
            }
        }

        public override void OnPreviewSettings()
        {
            if (GUILayout.Button("Reset Camera", EditorStyles.whiteMiniLabel))
            {
                m_Drag = Vector2.zero;
            }
        }
        #endregion

        #region Private Function
        protected void DrawPrototypeListGUI(string name)
        {
            EditorGUI.DrawRect(EditorGUILayout.BeginHorizontal(MyStyles.moduleHeader), MyStyles.headerColor);
            Rect foldoutRect = GUILayoutUtility.GetRect(40f, 16f);
            EditorPrefs.SetBool(name + " Foldout", EditorGUI.Foldout(foldoutRect, EditorPrefs.GetBool(name + " Foldout"), name, true));
            EditorGUILayout.EndHorizontal();            
            if (EditorPrefs.GetBool(name + " Foldout"))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("搜索:");
                EditorGUI.BeginChangeCheck();                
                string oldStr = this.searchText;
                Rect pos = new Rect(50, 205, 250, 16);
                this.searchText = EditorGUI.TextField(pos,this.searchText);
                if (EditorGUI.EndChangeCheck() && oldStr.CompareTo(this.searchText) != 0)
                {
                    bool isFistMatchSuccess = true;
                    this.searchIndexDic.Clear();
                    if (this.searchText.CompareTo("") != 0)
                    {
                        for (int index = 0; index < prototypePrefabList.Count; ++index)
                        {
                            PrototypeItem_Editor item = prototypePrefabList[index];
                            var isMatchSuccess = Regex.IsMatch(item.name, this.searchText, RegexOptions.IgnoreCase);
                            if (isMatchSuccess)
                            {
                                if (isFistMatchSuccess)
                                {
                                    scrollPosition.y = index * 60;
                                    isFistMatchSuccess = false;
                                }
                                this.searchIndexDic.Add(index, index);
                            }
                        }
                    }

                    if (this.searchIndexDic.Count == 0 && this.lastProtoIndex >= 0)
                    {
                        scrollPosition.y = this.lastProtoIndex * 60;
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(300), GUILayout.Height(400));
                EditorGUILayout.BeginVertical("Box");
                prototypeList.DoLayoutList();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
            }
        }

        void DefinePrototypeList()
        {
            prototypeList = new ReorderableList(prototypePrefabList, typeof(PrototypeItem_Editor));
           // prototypeList = new ReorderableList(serializedObject, this.serializedObject.FindProperty("prototypePrefabList"));
            prototypeList.drawHeaderCallback = (Rect rect) =>
            {
                GUI.Label(rect, "列表");
            };

            prototypeList.elementHeight = 60;

               
             prototypeList.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
            {
        
                PrototypeItem_Editor item = prototypePrefabList[index];
                rect.height -= 4;
                rect.y += 2;                
                Rect position = rect;
                EditorGUIUtility.labelWidth = 60;
                position.height = EditorGUIUtility.singleLineHeight;
                var iconRect = new Rect(position)
                {
                    width = 64,
                    height = 64
                };

                var nameRect = new Rect(iconRect)
                {
                    x = iconRect.x + 64 + 5,
                    width = rect.width - 64 - 5
                };

                EditorGUI.ObjectField(iconRect, item.icon, typeof(Texture), false);
                Color defColor = GUI.color;                
                if (this.searchIndexDic.ContainsKey(index))
                {
                    GUI.color = MyStyles.textHightLightColor;
                    EditorGUI.LabelField(nameRect, item.name, MyStyles.foundText);
                    GUI.color = defColor;
                }
                else
                {
                    EditorGUI.LabelField(nameRect, item.name);
                }
                
            };

            prototypeList.onSelectCallback = (ReorderableList list) =>
            {
                int oldLastProtoIndex = this.lastProtoIndex;
                this.lastProtoIndex = list.index;
                ResetLastProtoIndex();
                if (oldLastProtoIndex != this.lastProtoIndex)
                {
                    SaveLastProtoType(this.lastProtoIndex,true);

                    if (this.lastProtoIndex >= 0)
                    {
                        string prefabPath = this.chunkPrefab_Paths[this.lastProtoIndex];
                        
                        GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                        EditorGUIUtility.PingObject(obj);
                    }
                }               
            };

            prototypeList.displayAdd = false;
            prototypeList.displayRemove = false;
            prototypeList.draggable = false;
        }
        void RefreshLightmapDatas()
        {
            if(grassSys.RefreshLightmapDatas())
            {
                SimpleInstancingMgr instMgr = grassSys.transform.GetComponent<SimpleInstancingMgr>();
                if (instMgr != null)
                {
                    EditorUtility.SetDirty(instMgr);
                }
            }
            //if (grassSys.saveDataProfile != null)
            //{
            //    SimpleInstancingMgr instMgr = grassSys.transform.GetComponent<SimpleInstancingMgr>();
            //    if (instMgr != null)
            //    {
            //        instMgr.lightmapDic.Refresh(grassSys.saveDataProfile);
            //        EditorUtility.SetDirty(instMgr);
            //    }
            //}
        }
        void PrototypeOperaterCallBack(string newPrefabFullPath)
        {
            RefreshPrototypeData();

            int newIndex = -1;
            if (newPrefabFullPath != "")
            {
                for (int i = 0; i < chunkPrefab_Paths.Count; ++i)
                {
                    if (chunkPrefab_Paths[i].ToLower() == newPrefabFullPath.ToLower())
                    {
                        newIndex = i;
                        break;
                    }
                }
            }

            if (newIndex != -1)
            {
                this.lastProtoIndex = newIndex;
                ResetLastProtoIndex();
                SaveLastProtoType(this.lastProtoIndex,false);
            }
        }
        void RefreshPrototypeData()
        {
            List<GUIContent> chunkPrefab = new List<GUIContent>();
            SimpleEditorCommon.GetChunkPrefabs(out chunkPrefab, out chunkPrefab_Values, out chunkPrefab_Paths);
            chunkPrefab_Index = new int[chunkPrefab_Values.Count];

            //图列列表
            prototypePrefabList.Clear();
            for (int i = 0; i < chunkPrefab_Values.Count; ++i)
            {
                chunkPrefab_Index[i] = i;

                //////
                PrototypeItem_Editor item = new PrototypeItem_Editor();
                item.index = i;
                item.name = chunkPrefab_Values[i].name;
                item.prefab = chunkPrefab_Values[i];
                item.icon = null;
                if (item.prefab != null)
                {
                    SimpleGrassChunk chunkcmp = item.prefab.GetComponent<SimpleGrassChunk>();
                    if (chunkcmp != null && chunkcmp.grassPrefab != null)
                    {
                        //item.icon = AssetPreview.GetAssetPreview(chunkcmp.grassPrefab) as Texture;
                        MeshFilter meshFilter = chunkcmp.grassPrefab.GetComponentInChildren<MeshFilter>();
                        if (meshFilter != null)
                        {
                            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                            if(meshRenderer != null)
                            {
                                item.icon = meshRenderer.sharedMaterial.GetTexture("_MainTex");
                            }
                        }                        
                    }
                }
                prototypePrefabList.Add(item);
                ////
            }
            chunkPrefab_ = chunkPrefab.ToArray();            

            //保存的最近一次操作的植被种类
            ResetLastProtoType();
        }
        private void inspectorUpdate()
        {
           
        }

        private void ConvertProfileDir()
        {
            if (Directory.Exists(SimpleEditorCommon.OldProfilePath))
            {
                //新的目录
                if (!Directory.Exists(SimpleEditorCommon.ProfilePath))
                {
                    Directory.CreateDirectory(SimpleEditorCommon.ProfilePath);

                    FileUtil.ReplaceDirectory(SimpleEditorCommon.OldProfilePath, SimpleEditorCommon.ProfilePath);

                    FileUtil.DeleteFileOrDirectory(SimpleEditorCommon.OldProfilePath);
                }

                // FileUtil.CopyFileOrDirectory(SimpleEditorCommon.OldProfilePath, SimpleEditorCommon.ProfilePath);                          
            }            
        }

        private void CreateDefaultAssetPath()
        {
            //创建相关的资源路径
            bool bRefresh = false;
            if (!Directory.Exists(SimpleEditorCommon.ProfilePath))
            {
                Directory.CreateDirectory(SimpleEditorCommon.ProfilePath);
                bRefresh = true;
            }

            if (!Directory.Exists(SimpleEditorCommon.PrefabPath))
            {
                Directory.CreateDirectory(SimpleEditorCommon.PrefabPath);
                bRefresh = true;
            }

            //if (!Directory.Exists(SimpleEditorCommon.SaveDataPath))
            //{
            //    Directory.CreateDirectory(SimpleEditorCommon.SaveDataPath);
            //    bRefresh = true;
            //}

            if (bRefresh)
            {
                AssetDatabase.Refresh();
            }
        }

        private void RefreshProperty()
        {
            _hintInfo = serializedObject.FindProperty("hintInfo");

            _EditorMode = serializedObject.FindProperty("editorMode");
            _Editor_ViewDist = serializedObject.FindProperty("Editor_ViewDist");
            _PaintingLayerMask = serializedObject.FindProperty("PaintingLayerMask");
            _GrassChunkPrefab = serializedObject.FindProperty("GrassChunkPrefab");
            _KindName = serializedObject.FindProperty("kindName");

            _BrushRadius = serializedObject.FindProperty("BrushRadius");
            _Density = serializedObject.FindProperty("Density");

            _StartRadi = serializedObject.FindProperty("StartRadi");
            _StartLength = serializedObject.FindProperty("StartLength");
            _MinRadi = serializedObject.FindProperty("MinRadi");
            _MinAvoidDist = serializedObject.FindProperty("MinAvoidDist");

            _RandomRot = serializedObject.FindProperty("RandomRot");
            _RandomRotMin = serializedObject.FindProperty("RandomRotMin");
            _RandomRotMax = serializedObject.FindProperty("RandomRotMax");
            _MinMaxScale = serializedObject.FindProperty("EndMinMaxScale");

            _OnNormal = serializedObject.FindProperty("OnNormal");
            _RayCastDist = serializedObject.FindProperty("RayCastDist");     

            _Profile = serializedObject.FindProperty("profile");
            _SaveDataProfile = serializedObject.FindProperty("saveDataProfile");

            //if (SimpleGrassSys.UseWorld)
            {
                _WorldBounds = serializedObject.FindProperty("WorldBounds");
                _WorldCellResolution = serializedObject.FindProperty("WorldCellResolution");
            }

            _AutoSave = serializedObject.FindProperty("AutoSave");
            //showHint = _hintInfo.boolValue;
        }

        private void GetProfilesData()
        {
            List<GUIContent> profiles = new List<GUIContent>();
            SimpleEditorCommon.GetProfiles(out profiles, out profilePaths);
            profile_Index = new int[profilePaths.Count];
            for (int i = 0; i < profilePaths.Count; ++i)
            {
                profile_Index[i] = i;
            }
            profile_ = profiles.ToArray();
        }

        private void ResetProfilesData()
        {
            if (profileIndex < 0)
            {
                grassSys.Profile = null;
            }
            else
            {
                SimpleGrassProfile profileAsset = AssetDatabase.LoadAssetAtPath<SimpleGrassProfile>(profilePaths[profileIndex]);
                grassSys.Profile = profileAsset;

                _Profile.objectReferenceValue = grassSys.Profile;
            }
        }
        private void ResetChunkPrefabIndex()
        {
            chunkPrefabIndex = -1;
            for (int i = 0; i < chunkPrefab_Values.Count; ++i)
            {
                if (grassSys.GrassChunkPrefab != null)
                {
                    string str = grassSys.GrassChunkPrefab.name;
                    if (chunkPrefab_Values[i].name.CompareTo(str) == 0)
                    {
                        chunkPrefabIndex = i;
                        break;
                    }
                }
            }
        }

        private void ResetProfileIndex()
        {
            profileIndex = -1;
            if (grassSys.Profile != null)
            {
                string str = grassSys.Profile.name;
                for (int i = 0; i < profile_.Length; ++i)
                {
                    if (profile_[i].text.CompareTo(str) == 0)
                    {
                        profileIndex = i;
                        break;
                    }
                }
            }
        }
            

        private void ResetLastProtoIndex()
        {
            if (this.lastProtoIndex >= 0 && this.lastProtoIndex < chunkPrefab_Values.Count)
            {
                _GrassChunkPrefab.objectReferenceValue = chunkPrefab_Values[this.lastProtoIndex];
                _KindName.stringValue = "NULL";
                if (_GrassChunkPrefab.objectReferenceValue != null)
                {
                    _KindName.stringValue = _GrassChunkPrefab.objectReferenceValue.name;
                }
            }    
        }

        private void PrepareAddGrass(RaycastHit rayHit)
        {
            AddGrassChunk(rayHit);

            Selection.activeGameObject = grassSys.gameObject;
            Selection.activeObject = grassSys.gameObject;
            Selection.activeTransform = grassSys.transform;
            // Debug.Log("drag -add-grass ");
        }

        private void AddGrassChunk(RaycastHit hit)
        {
            if (grassSys.GrassChunkPrefab == null)
            {
                Debug.LogError("草预制件不能为空！！！！！");
                return;
            }
            Vector3 hitPosition = hit.point;
            Vector3 hitNorm = hit.normal;
            GameObject hitGameObj = hit.transform.gameObject;

            //根
            GameObject root = GameObject.Find("SimpleGrass");
            if (root == null)
            {
                root = new GameObject("SimpleGrass");
                root.tag = "EditorOnly";
                root.transform.localPosition = Vector3.zero;
                root.transform.localEulerAngles = Vector3.zero;
                root.transform.localScale = Vector3.one;

                this.RootNode = root;
            }
            ///种类根点点
            string kindStr = grassSys.KindName;
            if (kindStr == "")
            {
                Debug.Log("GrassSys.KindName,不能为空,不能重复");
                return;
            }

            Transform subRoot = root.transform.Find(kindStr);
            bool bResetProtoInfo = (subRoot == null);
            if (subRoot == null)
            {
                GameObject subRootObj = new GameObject(kindStr);
                subRoot = subRootObj.transform;
                subRoot.parent = root.transform;

                subRoot.localPosition = Vector3.zero;
                subRoot.localEulerAngles = Vector3.zero;
                subRoot.localScale = Vector3.one;
            }


            //植被块
            GameObject grassChunkObj = GameObject.Instantiate(grassSys.GrassChunkPrefab);
            grassChunkObj.transform.parent = subRoot.transform;
            grassChunkObj.transform.position = hitPosition;
            grassChunkObj.transform.up = hit.normal;
            if (!grassSys.OnNormal)
            {
                grassChunkObj.transform.up = Vector3.up;
            }

            SimpleGrassChunk go = grassChunkObj.GetComponent<SimpleGrassChunk>();

            //节点增加，原型自定义信息
            CreateAndResetProtoInfo(subRoot.gameObject, go);
            ////

            go.SelfPrefab = grassSys.GrassChunkPrefab;
            go.PaintOnObj = hitGameObj;
            go.EndScale = UnityEngine.Random.Range(grassSys.EndMinMaxScale.x, grassSys.EndMinMaxScale.y);//初始随机比例
            go.RayCastDist = grassSys.RayCastDist;

            go.HitPos = hitPosition;
            go.HitNorm = hitNorm;
            //go.Prop.MinMaxScale = grassSys.MinMaxScale;
            go.Prop.RandomRot = grassSys.RandomRot;
            go.Prop.RandomRotMin = grassSys.RandomRotMin;
            go.Prop.RandomRotMax = grassSys.RandomRotMax;
            go.Prop.BrushRadius = grassSys.BrushRadius;

            go.Prop.Density = grassSys.Density;
            go.Prop.StartRadi = grassSys.StartRadi;
            go.Prop.StartLength = grassSys.StartLength;
            go.Prop.MinRadi = grassSys.MinRadi;
            go.Prop.MinAvoidDist = grassSys.MinAvoidDist;
            go.Prop.MoveWithObject = grassSys.MoveWithObject;
            go.Prop.Interactive = grassSys.Interactive;
            go.Prop.OnNormal = grassSys.OnNormal;

            go.Generate(true, go.Prop.StartRadi, go.Prop.StartLength, FunCreateGrassPrefab());
            go.BuildCollider();
            BakeChildObjColor(go);

            Undo.RegisterCreatedObjectUndo(grassChunkObj, "undo grasschunk");
        }

        RenderTexture rt;
        Texture2D tex;
        private void BakeChildObjColor(SimpleGrassChunk cmp)
        {
            var childs = cmp.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < childs.Length; i++)
            {
                childs[i].gameObject.SetActive(false);
            }

            var initPos = Camera.main.transform.position;
            Camera currentCamera = SceneView.lastActiveSceneView.camera;
            Camera.main.CopyFrom(currentCamera);

            //// 使用RenderTexture捕捉屏幕上的像素信息
            if(rt == null)
            {
                rt = new RenderTexture(Screen.width, Screen.height, -1);
            }
            Camera.main.targetTexture = rt;
            Camera.main.Render();
            RenderTexture.active = rt;

            // 创建一个新的Texture2D并从RenderTexture中读取像素颜色
            if (tex == null)
            {
                tex = new Texture2D(Screen.width, Screen.height);
            }
            tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            tex.Apply();

            //存储到本地方便 debug
            //string screenPngPath = Application.streamingAssetsPath + "/test";
            //if (!Directory.Exists(screenPngPath))
            //{
            //    Directory.CreateDirectory(screenPngPath);
            //}

            //var downBuffer = tex.EncodeToPNG();
            //string fullPath = screenPngPath + "/" + "screenTex.png";
            //FileStream file = File.Create(fullPath);
            //file.Write(downBuffer, 0, downBuffer.Length);
            //file.Flush();
            //file.Close();
            //file.Dispose();


            // 恢复相机设置
            Camera.main.targetTexture = null;
            RenderTexture.active = null;
            rt.Release();

            cmp.childColors = new Vector3[childs.Length];
            cmp.childMaterialBlocks = new MaterialPropertyBlock[childs.Length];
            for (int i = 0; i < childs.Length; i++)
            {
                var screenUv = Camera.main.WorldToViewportPoint(childs[i].transform.position);
                var color = tex.GetPixelBilinear(screenUv.x, screenUv.y).linear;
                Vector3 tempColorToV3 = new Vector3(color.r, color.g, color.b);
                cmp.childColors[i] = tempColorToV3;
                cmp.childMaterialBlocks[i] = new MaterialPropertyBlock();
                childs[i].GetPropertyBlock(cmp.childMaterialBlocks[i]);
                cmp.childMaterialBlocks[i].SetColor("_Color1", color.gamma);
                childs[i].SetPropertyBlock(cmp.childMaterialBlocks[i]);
                //Debug.Log($"i:{i}, uv:{screenUv}");
            }
            for (int i = 0; i < childs.Length; i++)
            {
                childs[i].gameObject.SetActive(true);
            }

            //Camera.main.transform.position = initPos;
        }

        private System.Func<SimpleGrassChunk, GameObject> FunCreateGrassPrefab()
        {
            Func<SimpleGrassChunk, GameObject> obj = (Prefab) => PrefabUtility.InstantiatePrefab(Prefab.GrassPrefab) as GameObject;
            return obj;

        }

        private void PrepareEraseGrass(Ray ray, RaycastHit rayHit)
        {
            //单个块删除
            if (grassSys.Erasing & !grassSys.BrushErase)
            {
                if (rayHit.collider != null && rayHit.collider.gameObject.GetComponent<InteractCollider>() != null)
                {
                    SimpleGrassChunk chunk = rayHit.collider.gameObject.GetComponent<InteractCollider>().ChunkHandler;
                    //UNDO

                    Undo.DestroyObjectImmediate(chunk.gameObject);

                    //DestroyImmediate(obj);

                    //删除相关的数据                    
                }
            }

            //批量MASS ERASE
            if (grassSys.Erasing & grassSys.BrushErase)
            {
                RaycastHit[] hits = Physics.SphereCastAll(ray, grassSys.BrushRadius, Mathf.Infinity);
                if (hits != null & hits.Length > 0)
                {
                    for (int j = 0; j < hits.Length; j++)
                    {
                        RaycastHit hit1 = hits[j];
                        if (hit1.collider != null && hit1.collider.gameObject != null)
                        {
                            SimpleGrassChunk chunk = hit1.collider.gameObject.GetComponent<SimpleGrassChunk>();
                            if (chunk != null)
                            {
                                if (Vector3.Distance(hit1.point, Camera.current.transform.position) <= grassSys.Editor_ViewDist)
                                {
                                    Undo.DestroyObjectImmediate(chunk.gameObject);
                                }
                            }
                        }
                    }
                }
            }
            //if (grassSys.Erasing & grassSys.BrushErase)
            //{
            //    RaycastHit[] hits = Physics.SphereCastAll(ray, grassSys.BrushRadius, Mathf.Infinity);
            //    if (hits != null & hits.Length > 0)
            //    {

            //        bool one_is_outside_view = false;
            //        for (int j = 0; j < hits.Length; j++)
            //        {
            //            if (Vector3.Distance(hits[j].point, Camera.current.transform.position) > grassSys.Editor_ViewDist)
            //            {
            //                one_is_outside_view = true;
            //            }
            //        }

            //        if (!one_is_outside_view)
            //        {
            //            for (int j = 0; j < hits.Length; j++)
            //            {
            //                RaycastHit hit1 = hits[j];

            //                if (hit1.collider != null && hit1.collider.gameObject.GetComponent<InteractCollider>() != null)
            //                {
            //                    SimpleGrassChunk chunk = hit1.collider.gameObject.GetComponent<InteractCollider>().ChunkHandler;

            //                    Undo.DestroyObjectImmediate(chunk.gameObject);
            //                }
            //            }
            //        }
            //    }
            //}

            Selection.activeGameObject = grassSys.gameObject;
            Selection.activeObject = grassSys.gameObject;
            Selection.activeTransform = grassSys.transform;
        }

        private void CreateProfile()
        {
            SimpleGrassProfile newProfile = ScriptableObject.CreateInstance<SimpleGrassProfile>();
            newProfile.Save(grassSys);
            newProfile.GrassChunkPrefab = null;
            newProfile.KindName = "";

            System.DateTime dt = System.DateTime.Now;
            string strdt = dt.ToFileTime().ToString();//127756416859912816

            //创建保存目录

            string strFileName = SimpleEditorCommon.ProfilePath + "Profile_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!Directory.Exists(SimpleEditorCommon.ProfilePath))
            {
                Directory.CreateDirectory(SimpleEditorCommon.ProfilePath);
                AssetDatabase.Refresh();
            }
            if (File.Exists(strFileName + ".asset"))
            {
                strFileName = strFileName + "_" + strdt;
            }
            AssetDatabase.CreateAsset(newProfile, strFileName + ".asset");//Assets/SimpleGrassProfile_

            //AssetDatabase.CreateAsset(newProfile, ProfilePath + "SimpleGrassProfile_" + strdt + ".asset");//Assets/SimpleGrassProfile_
            AssetDatabase.SaveAssets();


            //EditorUtility.FocusProjectWindow();
            // Selection.activeObject = newProfile;

            grassSys.Profile = newProfile;


        }

        private void CreateSaveDataProfile()
        {
            SimpleSaveData newProfile = ScriptableObject.CreateInstance<SimpleSaveData>();
            newProfile.Ver = Common.Ver;//版本号
            System.DateTime dt = System.DateTime.Now;
            //dt.ToString();//2005-11-5 13:21:25 
            string strdt = dt.ToFileTime().ToString();//127756416859912816

            //创建保存目录
            string scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            string sceneDataPath = Path.GetDirectoryName(scenePath) + "/data";
            if (!Directory.Exists(sceneDataPath))
            {
                Directory.CreateDirectory(sceneDataPath);
                AssetDatabase.Refresh();
            }

            // string strFileName = SimpleEditorCommon.SaveDataPath + "SaveData_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string strFileName = sceneDataPath + "/" + "SaveData_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            //if (!Directory.Exists(SimpleEditorCommon.SaveDataPath))
            //{
            //    Directory.CreateDirectory(SimpleEditorCommon.SaveDataPath);
            //    AssetDatabase.Refresh();
            //}
            if (File.Exists(strFileName + ".asset"))
            {
                strFileName = strFileName + "_" + strdt;
            }
            AssetDatabase.CreateAsset(newProfile, strFileName + ".asset");//"Assets/SimpleSaveData_"
            //AssetDatabase.CreateAsset(newProfile, SaveDataPath + "SimpleSaveData_" + strdt + ".asset");//"Assets/SimpleSaveData_"
            AssetDatabase.SaveAssets();

            //  EditorUtility.FocusProjectWindow();
            // Selection.activeObject = newProfile;

            grassSys.SaveDataProfile = newProfile;
        }

        private void ResetLastProtoType()
        {
            //保存的最近一次操作的植被种类
            this.lastProtoType = "";
            if (PlayerPrefs.HasKey("GrassSys_LastProtoType"))
            {
                this.lastProtoType = PlayerPrefs.GetString("GrassSys_LastProtoType");
            }
            
            for (int i = 0; i < chunkPrefab_Values.Count; ++i)
            {
                if (chunkPrefab_Values[i].name == lastProtoType)
                {
                    this.lastProtoIndex = i;
                    break;
                }
            }
        }

        private bool SaveLastProtoType(int protoIndex, bool fromReorderableList)
        {
            if(chunkPrefab_Values != null && protoIndex >= 0 && protoIndex < chunkPrefab_Values.Count)
            {
                lastProtoIndex = protoIndex;
                lastProtoType = chunkPrefab_Values[protoIndex].name;
                PlayerPrefs.SetString("GrassSys_LastProtoType", lastProtoType);
                PlayerPrefs.Save();

                if(!fromReorderableList && prototypeList != null)
                {
                    prototypeList.index = lastProtoIndex;
                    
                    scrollPosition.y = this.lastProtoIndex * 60;                    
                }
                
            }
            //更新KineName
            if (_GrassChunkPrefab.objectReferenceValue != null)
            {
                _KindName.stringValue = _GrassChunkPrefab.objectReferenceValue.name;
            }
            return true;             
        }


        private void _RepareGrasses()
        {
            GameObject root = this.RootNode;
            int childCount = root.transform.childCount;
            GameObject obj = null;
            GameObject objChunck = null;

            Undo.RegisterFullObjectHierarchyUndo(root, "undo repair");
            //遍历ProtoType 原型
            for (int i = 0; i < childCount; ++i)
            {
                obj = root.transform.GetChild(i).gameObject;
                int chunckCount = obj.transform.childCount;
                for (int chunckIdx = 0; chunckIdx < chunckCount; ++chunckIdx)
                {
                    objChunck = obj.transform.GetChild(chunckIdx).gameObject;

                    Vector3 pos = objChunck.transform.position;
                    pos.y += 200;
                    Ray ray1 = new Ray(pos, -Vector3.up);
                    RaycastHit hit1 = new RaycastHit();
                    if (Physics.Raycast(ray1, out hit1, 200 * 2, grassSys.PaintingLayerMask.value))
                    {
                        //OnNormal
                        Vector3 hitPosition = hit1.point;
                        Vector3 hitNorm = hit1.normal;
                        objChunck.transform.position = hitPosition;
                        //objChunck.transform.up = hitNorm;
                    }

                    int grassCount = objChunck.transform.childCount;
                    GameObject objGrass = null;
                    //块下所有实例数据
                    for (int grassIdx = 0; grassIdx < grassCount; ++grassIdx)
                    {
                        objGrass = objChunck.transform.GetChild(grassIdx).gameObject;
                        Vector3 startPos = objGrass.transform.position;
                        startPos.y += 200;
                        Ray ray = new Ray(startPos, -Vector3.up);
                        RaycastHit hit = new RaycastHit();
                        if (Physics.Raycast(ray, out hit, 200 * 2, grassSys.PaintingLayerMask.value))
                        {
                            //OnNormal
                            Vector3 hitPosition = hit.point;
                            Vector3 hitNorm = hit.normal;
                            //Quaternion r = Quaternion.FromToRotation(objGrass.transform.up, hitNorm);
                            objGrass.transform.position =  hitPosition;
                            //objGrass.transform.Rotate(r.eulerAngles);
                            
                            if (grassSys.OnNormal)
                            {
                                objGrass.transform.up = hitNorm;
                            }
                            
                            // Undo.RegisterFullObjectHierarchyUndo(objGrass, "undo repair2");
                        }
                    }
                }
            }

            
        }

        #endregion

        #region MeshPreview Fun
        private void ValidateData()
        {
            if (m_PreviewRenderUtility == null)
            {
                m_PreviewRenderUtility = new PreviewRenderUtility();

                m_PreviewRenderUtility.camera.transform.position = new Vector3(0, 0, -6);
                m_PreviewRenderUtility.camera.transform.rotation = Quaternion.identity;
            }

            m_TargetMeshFilter = null;
            m_TargetMeshRenderer = null;
            if (this.lastProtoIndex >= 0 && this.lastProtoIndex < this.chunkPrefab_Values.Count)
            {
                GameObject prefab = this.chunkPrefab_Values[this.lastProtoIndex];
                if(prefab != null)
                {
                    SimpleGrassChunk chunkcmp = prefab.GetComponent<SimpleGrassChunk>();
                    if(chunkcmp != null && chunkcmp.grassPrefab != null)
                    {
                        m_TargetMeshFilter = chunkcmp.grassPrefab.GetComponentInChildren<MeshFilter>();
                        if (m_TargetMeshFilter != null)
                        {
                            m_TargetMeshRenderer = m_TargetMeshFilter.GetComponent<MeshRenderer>();
                        }
                    }
                }   
            }
        }

        public static Vector2 Drag2D(Vector2 scroll_pos, Rect position)
        {
            var control_ID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
            var current = Event.current;
            switch (current.GetTypeForControl(control_ID))
            {
                case EventType.MouseDown:
                    if (position.Contains(current.mousePosition) && position.width > 50f)
                    {
                        GUIUtility.hotControl = control_ID;
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == control_ID)
                    {
                        GUIUtility.hotControl = 0;
                    }
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == control_ID)
                    {
                        scroll_pos -= current.delta * (float)((!current.shift) ? 1 : 3) / Mathf.Min(position.width, position.height) * 140f;
                        scroll_pos.y = Mathf.Clamp(scroll_pos.y, -90f, 90f);
                        current.Use();
                        GUI.changed = true;
                    }
                    break;
            }
            return scroll_pos;
        }
        #endregion

        #region Static Function

        public static SimpleGrassProtoInfo CreateAndResetProtoInfo(GameObject objNode, SimpleGrassChunk grassChunk)
        {
            if (!objNode || !grassChunk)
            {
                return null;
            }

            //原型自定义信息
            SimpleGrassProtoInfo cmpProtoInfo = objNode.GetComponent<SimpleGrassProtoInfo>();
            if (!cmpProtoInfo)
            {
                cmpProtoInfo = objNode.gameObject.AddComponent<SimpleGrassProtoInfo>();
                cmpProtoInfo.LayerID = grassChunk.LayerID;
                cmpProtoInfo.CullingMaxDistance = grassChunk.CullingMaxDistance;
                cmpProtoInfo.CastShadows = grassChunk.CastShadows;
                cmpProtoInfo.ReceiveShadows = grassChunk.ReceiveShadows;
                cmpProtoInfo.MergeChunkDistance = grassChunk.MergeChunkDistance;
                //cmpProtoInfo.Density = 1.0f;
            }
            return cmpProtoInfo;
        }

        #endregion

        #region Menu Item
        [MenuItem("MY_Support/草系统/创建GrassSys", false, 5000)]
        static void CreatesimpleGrass()
        {
            GameObject sceneRoot = GameObject.Find("[Scene]");
            if (sceneRoot == null)
            {
                sceneRoot = new GameObject("[Scene]");
            }
            Transform sysTrans = sceneRoot.transform.Find("GrassSys");
            if (sysTrans != null)
            {
                return;
            }

            GameObject go = new GameObject("GrassSys");
            go.transform.SetParent(sceneRoot.transform);
            SimpleGrassSys syscmp = go.AddComponent<SimpleGrassSys>();
            go.AddComponent<SimpleInstancingMgr>();

            SimpleInstancingMgr mgr = go.GetComponent<SimpleInstancingMgr>();
            mgr.SimpleSys = syscmp;

            go.transform.position = Vector3.zero;
        }

        [MenuItem("MY_Support/草系统/创建扫描目录", false, 5001)]
        static void CreateSimpleGrassScann()
        {
            GameObject go = GameObject.Find("SimpleGrass_Scan");
            if (go != null)
            {
                return;
            }

            go = new GameObject("SimpleGrass_Scan");
            go.tag = "EditorOnly";
        }

        //[MenuItem("MY_Support/草系统/创建轨迹触发", false, 5001)]
        //static void CreateSimpleGrassTraceCollider()
        //{
        //    GameObject curNode = Selection.activeGameObject;
        //    if (curNode == null)
        //    {
        //        return;
        //    }
        //    GameObject go = new GameObject("GrassTraceDetector");
        //    go.layer = curNode.layer;
        //    go.AddComponent<SimpleGrassTraceDetector>();
        //    go.tag = curNode.tag;
        //    go.transform.SetParent(curNode.transform);           
        //    EditorGUIUtility.PingObject(go);
        //    Selection.activeObject = go;
        //}

        //[MenuItem("GameObject/GrassSys", true, 49)]
        //static bool ValidateSelect_()
        //{
        //    return Selection.activeGameObject != null;
        //}

        [MenuItem("GameObject/GrassSys", true, 49)]
        static bool ValidateSelect()
        {
            return Selection.activeGameObject != null;
        }


        [MenuItem("GameObject/GrassSys/创建扫描【节点】", false, 49)]
        static void CreateSimpleGrassScann_()
        {
            GameObject curNode = Selection.activeGameObject;           
            if (curNode == null)
            {
                return;
            }

            GameObject go = new GameObject("ScanNode");
            go.layer = curNode.layer;
            go.AddComponent<SimpleGrassProtoInfo>();
            go.tag = "EditorOnly";
            go.transform.SetParent(curNode.transform);

            EditorGUIUtility.PingObject(go);
            Selection.activeObject = go;
        }


        [MenuItem("GameObject/GrassSys/增加扫描【标识】", false, 49)]
        static void CreateSimpleGrassScannProtoInfo()
        {
            GameObject curNode = Selection.activeGameObject;
            if (curNode == null)
            {
                return;
            }

            if (curNode.GetComponent<SimpleGrassProtoInfo>() == null)
            {
                curNode.AddComponent<SimpleGrassProtoInfo>();
            }                        
        }


        [MenuItem("GameObject/GrassSys/删除扫描【标识】", false, 49)]
        static void RemoveSimpleGrassScannProtoInfo()
        {
            GameObject curNode = Selection.activeGameObject;
            if (curNode == null)
            {
                return;
            }

            SimpleGrassProtoInfo cmp = curNode.GetComponent<SimpleGrassProtoInfo>();
            if (cmp != null)
            {
                string backupTag = cmp.BackupTag;
                DestroyImmediate(cmp);
                curNode.tag = backupTag;
            }
        }
        

        #endregion
    }


}

