using UnityEngine;
using UnityEditor;

using System;
using System.Reflection;
using System.IO;
using UnityEditorInternal;

namespace SimpleGrass
{

    public class SimpleGrassPrototypeQuickSetup : EditorWindow
    {
        struct TmpPrototypeData
        {
            public GameObject GrassPrefab;

            public Vector3 ColliderScale;

            public float CullingMaxDistance;

            public float MergeChunkDistance;

            public bool CastShadows;

            public bool ReceiveShadows;

            public int LayerID;
        }

        public enum OpenMode
        {
            OM_NEW,
            OM_EDIT,
            OM_DELETE
        }

        GameObject editPrototypePrefab;
        public static SimpleGrassPrototypeQuickSetup window;
        public OpenMode openMode;
        private int guiSpace = 25;        
        private TmpPrototypeData tmpPrototypeData;
        private string hintText;
        private Action<string> callBack;

        public static void DeletePrototypePrefab(string prefabPath)
        {
            if (prefabPath != "")
            {
                AssetDatabase.DeleteAsset(prefabPath);
                AssetDatabase.Refresh();
            }
        }

        public static void NewPrototypeWindow(Action<string> callBack)
        {            
            window = (SimpleGrassPrototypeQuickSetup)EditorWindow.GetWindow(typeof(SimpleGrassPrototypeQuickSetup));
            window.callBack = callBack;
            window.hintText = "";
            window.openMode = OpenMode.OM_NEW;
            window.tmpPrototypeData = new TmpPrototypeData();
            window.tmpPrototypeData.ColliderScale = new Vector3(1.5f, 1.5f, 1.5f);
            window.tmpPrototypeData.CullingMaxDistance = 200;
            window.tmpPrototypeData.MergeChunkDistance = 10;


            window.titleContent.text = "ProtoType";
            window.Show();
        }

        public static void EditPrototypeWindow(string prefabsPath, Action<string> callBack)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath(prefabsPath, typeof(System.Object)) as GameObject;
            if (go == null)
            {
                Debug.LogError("路径错误：" + prefabsPath);
                return;
            }            
            window = (SimpleGrassPrototypeQuickSetup)EditorWindow.GetWindow(typeof(SimpleGrassPrototypeQuickSetup));
            window.callBack = callBack;
            window.editPrototypePrefab = go;
            window.hintText = "";
            window.openMode = OpenMode.OM_EDIT;

            SimpleGrassChunk chunk = go.GetComponent<SimpleGrassChunk>();
            window.tmpPrototypeData = new TmpPrototypeData();
            window.tmpPrototypeData.GrassPrefab = chunk.GrassPrefab;
            window.tmpPrototypeData.ColliderScale = chunk.ColliderScale;
            window.tmpPrototypeData.CullingMaxDistance = chunk.CullingMaxDistance;
            window.tmpPrototypeData.MergeChunkDistance = chunk.MergeChunkDistance;
            window.tmpPrototypeData.CastShadows = chunk.CastShadows;
            window.tmpPrototypeData.ReceiveShadows = chunk.ReceiveShadows;
            window.tmpPrototypeData.LayerID = chunk.LayerID;

            window.titleContent.text = "ProtoType QuickSetup";
            window.Show();
        }

        public static void OpenWindow()
        {
            window = (SimpleGrassPrototypeQuickSetup)EditorWindow.GetWindow(typeof(SimpleGrassPrototypeQuickSetup));
            //if (UnityEditor.Selection.activeObject != null && UnityEditor.Selection.activeObject.GetType() == typeof(GameObject))
            //{

            //}

            window.titleContent.text = "ProtoType QuickSetup";
            window.Show();
        }

        private void Awake()
        {           
        }

        private void OnDestroy()
        {           
                   
        }

        void OnGUI()
        {

            if (window == null)
            {
                OpenWindow();
            }
            EditorGUI.BeginChangeCheck();
            //GUILayout.Label("Quick Setup", EditorStyles.boldLabel);
            GUILayout.BeginVertical();
            int uiStart = 45;
            int uiSpace = 0;
            tmpPrototypeData.LayerID = EditorGUILayout.LayerField("LayerID", tmpPrototypeData.LayerID);

            tmpPrototypeData.GrassPrefab = (GameObject)EditorGUI.ObjectField(new Rect(5, uiStart + uiSpace, position.width - 10, 16), "GrassPrefab", tmpPrototypeData.GrassPrefab, typeof(GameObject), false);

            uiSpace += guiSpace ;
            tmpPrototypeData.ColliderScale = EditorGUI.Vector3Field(new Rect(5, uiStart + uiSpace, position.width - 10, 16), "Collider Scale", tmpPrototypeData.ColliderScale);

            uiSpace += guiSpace + 20;
            tmpPrototypeData.CullingMaxDistance = EditorGUI.FloatField(new Rect(5, uiStart + uiSpace, position.width - 10, 16), "Culling Max Distance", tmpPrototypeData.CullingMaxDistance);
            tmpPrototypeData.CullingMaxDistance = Mathf.Max(0.0f, tmpPrototypeData.CullingMaxDistance);

            uiSpace += guiSpace + 15;
            tmpPrototypeData.MergeChunkDistance = EditorGUI.FloatField(new Rect(5, uiStart + uiSpace, position.width - 10, 16), "Merge Chunk Distance", tmpPrototypeData.MergeChunkDistance);
            tmpPrototypeData.MergeChunkDistance = Mathf.Max(0.0f, tmpPrototypeData.MergeChunkDistance);

            uiSpace += guiSpace + 15;
            tmpPrototypeData.CastShadows = GUI.Toggle(new Rect(5, uiStart + uiSpace, position.width - 10, 16), tmpPrototypeData.CastShadows, "Cast Shadows");
           
            uiSpace += guiSpace + 15;
            tmpPrototypeData.ReceiveShadows = GUI.Toggle(new Rect(5, uiStart + uiSpace, position.width - 10, 16), tmpPrototypeData.ReceiveShadows, "Receive Shadows");

           

            uiSpace += guiSpace +25;
            if (this.openMode == OpenMode.OM_NEW)
            {
                if (GUI.Button(new Rect(5, uiStart + uiSpace, position.width - 10, 32), "增加"))
                {
                    string prefabFullPath = "";
                    if (NewPrototypePrefab(out prefabFullPath))
                    {
                        if(callBack!= null)
                        {
                            callBack.Invoke(prefabFullPath);
                        }
                        Close();
                    }
                }
            }
            
            if (this.openMode == OpenMode.OM_EDIT)
            {
                if (GUI.Button(new Rect(5, uiStart + uiSpace, position.width - 10, 32), "确定"))
                {
                    if (EditPrototypePrefab())
                    {
                        if (callBack != null)
                        {
                            callBack.Invoke("");
                        }
                        Close();
                    }
                }
            }

            uiSpace += guiSpace + 40;
            
            Color orgColor = GUI.color;
            GUI.color = Color.red;
            GUI.TextArea(new Rect(5, uiStart + uiSpace, position.width - 10, 60), hintText);
            GUI.color = orgColor;
            GUILayout.EndVertical();
            //if (this.openMode == OpenMode.OM_DELETE)
            //{
            //    if (GUI.Button(new Rect(5, uiStart + uiSpace, position.width - 10, 32), "确定"))
            //    {
            //        if (EditPrototypePrefab())
            //        {
            //            Close();
            //        }
            //    }
            //}


            if (EditorGUI.EndChangeCheck())
            {

            }

        }

        bool isFirstUpdate = true;
        void Update()
        {
            if (isFirstUpdate)
            {
              
                isFirstUpdate = false;
            }


            Repaint();


        }

        public bool NewPrototypePrefab(out string prefabFullPath)
        {
            if (!Directory.Exists(SimpleEditorCommon.PrefabPath))
            {
                Directory.CreateDirectory(SimpleEditorCommon.PrefabPath);
                AssetDatabase.Refresh();
            }
            if (tmpPrototypeData.GrassPrefab == null)
            {
                this.hintText = "错误： 植被的预制体不能为空（GrassPrefab==NULL）!!";
                prefabFullPath = "";
                return false;
            }

            string prototypePrefabPath = SimpleEditorCommon.PrefabPath + tmpPrototypeData.GrassPrefab.name + ".prefab";
            if (File.Exists(prototypePrefabPath))
            {
                System.DateTime dt = System.DateTime.Now;
                string strdt = dt.ToFileTime().ToString();
                prototypePrefabPath = SimpleEditorCommon.PrefabPath + tmpPrototypeData.GrassPrefab.name + "_" + strdt + ".prefab";
            }
            var go = new GameObject { name = "newPrototypePrefab" };
            SimpleGrassChunk cmp = go.AddComponent<SimpleGrassChunk>();
            cmp.grassPrefab = tmpPrototypeData.GrassPrefab;
            cmp.ColliderScale = tmpPrototypeData.ColliderScale;
            cmp.CullingMaxDistance = tmpPrototypeData.CullingMaxDistance;
            cmp.MergeChunkDistance = tmpPrototypeData.MergeChunkDistance;
            cmp.CastShadows = tmpPrototypeData.CastShadows;
            cmp.ReceiveShadows = tmpPrototypeData.ReceiveShadows;
            cmp.LayerID = tmpPrototypeData.LayerID;
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, prototypePrefabPath);//"Assets/SimpleGrass/Prefabs/"                            
            DestroyImmediate(go);
            AssetDatabase.Refresh();

            prefabFullPath = prototypePrefabPath;
            return true;          
        }

        private bool EditPrototypePrefab()
        {
            if (!Directory.Exists(SimpleEditorCommon.PrefabPath))
            {
                Directory.CreateDirectory(SimpleEditorCommon.PrefabPath);
                AssetDatabase.Refresh();
            }
            if(tmpPrototypeData.GrassPrefab == null)
            {
                this.hintText = "错误： 植被的预制体为空（GrassPrefab）!!";
                return false;
            }

            if (window.editPrototypePrefab != null)
            {                              
                SimpleGrassChunk cmp = window.editPrototypePrefab.GetComponent<SimpleGrassChunk>();
                cmp.grassPrefab = tmpPrototypeData.GrassPrefab;
                cmp.ColliderScale = tmpPrototypeData.ColliderScale;
                cmp.CullingMaxDistance = tmpPrototypeData.CullingMaxDistance;
                cmp.MergeChunkDistance = tmpPrototypeData.MergeChunkDistance;
                cmp.CastShadows = tmpPrototypeData.CastShadows;
                cmp.ReceiveShadows = tmpPrototypeData.ReceiveShadows;
                cmp.LayerID = tmpPrototypeData.LayerID;
                bool saveOK = false;
                UnityEditor.PrefabUtility.SavePrefabAsset(window.editPrototypePrefab, out saveOK);
                
                AssetDatabase.Refresh();
                return true;
            }
            return false;                        
        }

        void SavePlayerPrefs()
        {
           
            //PlayerPrefs.SetInt("MyWater_textureSize", textureSize);
            //PlayerPrefs.SetFloat("MyWater_ShoreLineDepth", shoreLineDepth);

        }

        //string GetCurSceneResMatPath()
        //{
        //    bool bMustRefresh = false;
        //    string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        //    string scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        //    string dataPath = Path.GetDirectoryName(scenePath) + "/" + MaterialsPath ;
        //    if (!Directory.Exists(dataPath))
        //    {
        //        Directory.CreateDirectory(dataPath);
        //        bMustRefresh = true;
        //    }

        //    if (bMustRefresh)
        //        AssetDatabase.Refresh();

        //    return dataPath;
        //}

      

        Type GetType(string TypeName)
        {

            // Try Type.GetType() first. This will work with types defined
            // by the Mono runtime, in the same assembly as the caller, etc.
            var type = Type.GetType(TypeName);

            // If it worked, then we're done here
            if (type != null)
                return type;

            // If the TypeName is a full name, then we can try loading the defining assembly directly
            if (TypeName.Contains("."))
            {
                // Get the name of the assembly (Assumption is that we are using 
                // fully-qualified type names)
                var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));

                // Attempt to load the indicated Assembly
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    if (assembly == null)
                        return null;

                    // Ask that assembly to return the proper Type
                    type = assembly.GetType(TypeName);
                    if (type != null)
                        return type;
                }
                catch (Exception)
                {
                    //Debug.Log("Unable to load assemmbly : " + ex.Message);
                }
            }

            // If we still haven't found the proper type, we can enumerate all of the 
            // loaded assemblies and see if any of them define the type
            var currentAssembly = Assembly.GetCallingAssembly();
            {
                // Load the referenced assembly
                if (currentAssembly != null)
                {
                    // See if that assembly defines the named type
                    type = currentAssembly.GetType(TypeName);
                    if (type != null)
                        return type;
                }

            }

            //All loaded assemblies
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int asyIdx = 0; asyIdx < assemblies.GetLength(0); asyIdx++)
            {
                type = assemblies[asyIdx].GetType(TypeName);
                if (type != null)
                {
                    return type;
                }
            }

            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            foreach (var assemblyName in referencedAssemblies)
            {
                // Load the referenced assembly
                var assembly = Assembly.Load(assemblyName);
                if (assembly != null)
                {
                    // See if that assembly defines the named type
                    type = assembly.GetType(TypeName);
                    if (type != null)
                        return type;
                }
            }

            // The type just couldn't be found...
            return null;
        }

    }   
}

