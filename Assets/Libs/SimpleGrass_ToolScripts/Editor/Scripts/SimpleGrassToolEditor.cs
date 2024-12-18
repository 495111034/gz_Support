using UnityEngine;
using UnityEditor;

using System;
using System.Reflection;
using System.IO;
using UnityEditorInternal;
using System.Collections.Generic;

namespace SimpleGrassTest
{

    public class SimpleGrassToolEditor : EditorWindow
    {

        [MenuItem("GameObject/GrassSys/修复窗口", false, 60)]
        public static void OpenReplace()
        {
            OpenWindow();
        }

        GameObject editPrototypePrefab;
        GameObject editChunkPrefab;

        float repareStartZScale = 0.0f;
        float repareEndZScale = 0.0f;
        float repareMul = 1.0f;

        private float offsetVal = 0.3f;
        static List<Transform> willDeleteList = new List<Transform>();
        string bakScenePath = "";
        string sourceScenePath = "";
        string bakSaveDataPath = "";
        string sourceSaveDataPath = "";
        public static SimpleGrassToolEditor window;
      
        public static void OpenWindow()
        {
            window = (SimpleGrassToolEditor)EditorWindow.GetWindow(typeof(SimpleGrassToolEditor));
  
            window.titleContent.text = " QuickSetup";
            window.Show();
        }

        class GrassGroup
        {
            public Vector3 center = Vector3.zero;
            public List<Transform> grassList = new List<Transform>();
            public List<Vector3> grassPosList = new List<Vector3>();
            public bool selfok = false;
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
            DrawReparePrefab();
            EditorGUILayout.Separator();
            DrawRepareChunkPrefab();
            EditorGUILayout.Separator();
            DrawRepareScale();
            EditorGUILayout.Separator();
            DrawRepareOverlapGrasses();
            // EditorGUI.BeginChangeCheck();           
            //GUILayout.Label("Quick Setup", EditorStyles.boldLabel);
            //GUILayout.BeginVertical();
            //int uiStart = 45;
            //int uiSpace = 0;


            //editPrototypePrefab = (GameObject)EditorGUI.ObjectField(new Rect(5, uiStart + uiSpace, position.width - 10, 16), "更新GrassPrefab", editPrototypePrefab, typeof(GameObject), false);
            //uiSpace += 25;

            //if (GUI.Button(new Rect(5, uiStart + uiSpace, position.width - 10, 32), "更新植被预制体 (选中SimpleGrass节点)"))
            //{
            //    if (UnityEditor.Selection.activeObject != null)
            //    {
            //        int count = UnityEditor.Selection.activeTransform.childCount;
            //        for(int index = 0; index < count; ++index)
            //        {
            //            Transform child = UnityEditor.Selection.activeTransform.GetChild(index);
            //            int subcount = child.childCount;
            //            for (int sub = 0; sub < subcount; ++sub)
            //            {
            //                Transform subChild = child.GetChild(sub);
            //                SimpleGrass.SimpleGrassChunk preb = subChild.gameObject.GetComponent<SimpleGrass.SimpleGrassChunk>();
            //                if (preb != null)
            //                {
            //                    preb.grassPrefab = editPrototypePrefab;
            //                }
            //            }
            //        }
            //    }
            //}

            // EditorGUILayout.Separator();
            // EditorGUILayout.Separator();
            //this.repareStartZScale = EditorGUI.FloatField(new Rect(5,120, position.width - 10, 20), "Start Z Scale:", this.repareStartZScale);
            //this.repareEndZScale = EditorGUI.FloatField(new Rect(5, 150, position.width - 10, 20), "End Z Scale:", this.repareEndZScale);
            //this.repareMul = EditorGUI.FloatField(new Rect(5, 180, position.width - 10, 20), "Mul Radio:", this.repareMul);
            //if (GUI.Button(new Rect(5, 210, position.width - 10, 32), "修正比例 (选中SimpleGrass子节点)"))
            //{
            //    int count = UnityEditor.Selection.activeTransform.childCount;
            //    SimpleGrass.SimpleGrassProtoInfo protoinfo = UnityEditor.Selection.activeTransform.GetComponent<SimpleGrass.SimpleGrassProtoInfo>();
            //    if (protoinfo != null)
            //    {
            //        for (int index = 0; index < count; ++index)
            //        {
            //            Transform child = UnityEditor.Selection.activeTransform.GetChild(index);
            //            int grassCount = child.childCount;
            //            for (int grass = 0; grass < grassCount; ++grass)
            //            {
            //                Transform grassChild = child.GetChild(grass);
            //                if (grassChild.localScale.z > repareStartZScale && grassChild.localScale.z < repareEndZScale)
            //                {
            //                    Vector3 localScale = grassChild.localScale * this.repareMul;
            //                    grassChild.localScale = localScale;
            //                }
            //            }
            //        }

            //        Undo.RegisterFullObjectHierarchyUndo(UnityEditor.Selection.activeTransform.gameObject, "undo repair");
            //    }
            //}
            //   GUILayout.EndVertical();



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

        private void DrawReparePrefab()
        {

            EditorGUI.DrawRect(EditorGUILayout.BeginHorizontal(new GUIStyle(GUI.skin.box)), new Color(0.15f, 0.15f, 0.15f));
            Rect foldoutRect = GUILayoutUtility.GetRect(500f, 16f);
            string strName = "ReparePrefab Foldout";
            EditorPrefs.SetBool(strName, EditorGUI.Foldout(foldoutRect, EditorPrefs.GetBool(strName), "修复预制体", true));
            EditorGUILayout.EndHorizontal();
            if (EditorPrefs.GetBool(strName))
            {
                EditorGUILayout.BeginVertical("Box");

                GUILayout.BeginVertical();                
                editPrototypePrefab = (GameObject)EditorGUILayout.ObjectField(editPrototypePrefab, typeof(GameObject), false, GUILayout.MaxWidth(250));
                EditorGUILayout.Space();
                if (GUILayout.Button(new GUIContent("修复植被预制体 (选中SimpleGrass下子节点)", "修复植被预制体 (选中SimpleGrass下子节点)."), GUILayout.Width(300)))
                {
                    if (UnityEditor.Selection.activeObject != null)
                    {
                      //  Undo.IncrementCurrentGroup();

                        Transform curActive = UnityEditor.Selection.activeTransform;
                        int count = curActive.childCount;
                        List<Transform> willDelList = new List<Transform>();
                        for (int index = 0; index < count; ++index)
                        {
                            Transform subChild = curActive.GetChild(index);
                            {
                                SimpleGrass.SimpleGrassChunk preb = subChild.gameObject.GetComponent<SimpleGrass.SimpleGrassChunk>();
                                if (preb != null)
                                {
                                    preb.grassPrefab = editPrototypePrefab;                                    
                                    int grassCount = subChild.childCount;
                                    willDelList.Clear();
                                    for (int grassIdx = 0; grassIdx < grassCount; ++grassIdx)
                                    {
                                        Transform grass = subChild.GetChild(grassIdx);
                                        willDelList.Add(grass);

                                        //新增，新的预制体的草
                                        GameObject newobj = PrefabUtility.InstantiatePrefab(preb.grassPrefab, subChild) as GameObject;
                                        newobj.transform.localPosition = grass.localPosition;
                                        newobj.transform.localScale = grass.localScale;
                                        newobj.transform.localRotation = grass.localRotation;

                                        Undo.RegisterCreatedObjectUndo(newobj, newobj.GetHashCode().ToString());
                                    }
                                    //删除旧的草
                                    for (int grassIdx = 0; grassIdx < grassCount; ++grassIdx)
                                    {
                                        Undo.DestroyObjectImmediate(willDelList[grassIdx].gameObject);
                                    }
                                    
                                }
                            }
                        }
                        
                       Undo.RegisterFullObjectHierarchyUndo(curActive.gameObject, "undo repair preab");

                        EditorUtility.DisplayDialog("提示：", "完成修复", "OK");
                    }else
                    {
                        EditorUtility.DisplayDialog("提示：", "请先选择SimpleGrass下子节点", "OK");
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawRepareChunkPrefab()
        {

            EditorGUI.DrawRect(EditorGUILayout.BeginHorizontal(new GUIStyle(GUI.skin.box)), new Color(0.15f, 0.15f, 0.15f));
            Rect foldoutRect = GUILayoutUtility.GetRect(500f, 16f);
            string strName = "RepareChunkPrefab Foldout";
            EditorPrefs.SetBool(strName, EditorGUI.Foldout(foldoutRect, EditorPrefs.GetBool(strName), "修复Chunk预制体", true));
            EditorGUILayout.EndHorizontal();
            if (EditorPrefs.GetBool(strName))
            {
                EditorGUILayout.BeginVertical("Box");

                GUILayout.BeginVertical();
                editChunkPrefab = (GameObject)EditorGUILayout.ObjectField(editChunkPrefab, typeof(GameObject), false, GUILayout.MaxWidth(250));
                EditorGUILayout.Space();
                if (GUILayout.Button(new GUIContent("修复Chunk预制体 (选中SimpleGrass下子节点)", "修复植被预制体 (选中SimpleGrass下子节点)."), GUILayout.Width(300)))
                {
                    if (UnityEditor.Selection.activeObject != null)
                    {
                        int count = UnityEditor.Selection.activeTransform.childCount;
                        for (int index = 0; index < count; ++index)
                        {
                            Transform subChild = UnityEditor.Selection.activeTransform.GetChild(index);
                            {
                                SimpleGrass.SimpleGrassChunk preb = subChild.gameObject.GetComponent<SimpleGrass.SimpleGrassChunk>();
                                if (preb != null)
                                {
                                    preb.selfPrefab = editChunkPrefab;
                                }
                            }
                        }
                        Undo.RegisterFullObjectHierarchyUndo(UnityEditor.Selection.activeTransform.gameObject, "undo repair chunk preab");

                        EditorUtility.DisplayDialog("提示：", "完成修复", "OK");
                    }else
                    {
                        EditorUtility.DisplayDialog("提示：", "请先选择SimpleGrass下子节点", "OK");
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }
        private void DrawRepareScale()
        {
            EditorGUI.DrawRect(EditorGUILayout.BeginHorizontal(new GUIStyle(GUI.skin.box)), new Color(0.15f, 0.15f, 0.15f));
            Rect foldoutRect = GUILayoutUtility.GetRect(500f, 16f);
            string strName = "RepareScale Foldout";
            EditorPrefs.SetBool(strName, EditorGUI.Foldout(foldoutRect, EditorPrefs.GetBool(strName), "修复比例", true));
            EditorGUILayout.EndHorizontal();
            if (EditorPrefs.GetBool(strName))
            {
                EditorGUILayout.BeginVertical("Box");
                
                this.repareStartZScale = EditorGUILayout.FloatField("Start Z Scale:", this.repareStartZScale);
                this.repareEndZScale = EditorGUILayout.FloatField( "End Z Scale:", this.repareEndZScale);
                this.repareMul = EditorGUILayout.FloatField("Mul Radio:", this.repareMul);

                EditorGUILayout.Space();
                if (GUILayout.Button(new GUIContent("修正比例 (选中SimpleGrass下子节点)", "修正比例 (选中SimpleGrass下子节点)."), GUILayout.Width(300)))
                {
                    int count = UnityEditor.Selection.activeTransform.childCount;
                    SimpleGrass.SimpleGrassProtoInfo protoinfo = UnityEditor.Selection.activeTransform.GetComponent<SimpleGrass.SimpleGrassProtoInfo>();
                    if (protoinfo != null)
                    {
                        for (int index = 0; index < count; ++index)
                        {
                            Transform child = UnityEditor.Selection.activeTransform.GetChild(index);
                            int grassCount = child.childCount;
                            for (int grass = 0; grass < grassCount; ++grass)
                            {
                                Transform grassChild = child.GetChild(grass);
                                if (grassChild.localScale.z > repareStartZScale && grassChild.localScale.z < repareEndZScale)
                                {
                                    Vector3 localScale = grassChild.localScale * this.repareMul;
                                    grassChild.localScale = localScale;
                                }
                            }
                        }

                        Undo.RegisterFullObjectHierarchyUndo(UnityEditor.Selection.activeTransform.gameObject, "undo repair scale");

                        EditorUtility.DisplayDialog("提示：", "完成修复", "OK");
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void backFiles(string scenePath)
        {
            System.DateTime dt = System.DateTime.Now;
            //备份场景文件
            string dtstr = dt.ToString("yyyy-MM-dd-HH-mm");
            string path1 = "LightMapBAK\\SceneBAK\\" + Path.GetDirectoryName(scenePath) + "\\" + dtstr + "\\";
            string filename1 = Path.GetFileName(scenePath);
            if (!Directory.Exists(path1))
            {
                Directory.CreateDirectory(path1);
            }
            string pullfile = path1 + filename1;
            System.IO.File.Copy(scenePath, pullfile, true);
            sourceScenePath = scenePath;
            bakScenePath = pullfile;
            //备份植被保存文件
            sourceSaveDataPath = "";
            bakSaveDataPath = "";
            SimpleGrass.SimpleGrassSys grassSys = GameObject.FindObjectOfType<SimpleGrass.SimpleGrassSys>();
            if (grassSys != null)
            {
                SimpleGrass.SimpleSaveData saveData = grassSys.SaveDataProfile;
                if (saveData != null)
                {
                    sourceSaveDataPath = AssetDatabase.GetAssetPath(saveData);
                    string assetfilename = Path.GetFileName(sourceSaveDataPath);
                    bakSaveDataPath = path1 + assetfilename;
                    System.IO.File.Copy(sourceSaveDataPath, bakSaveDataPath, true);
                }
            }
        }
        private void DrawRepareOverlapGrasses()
        {           
            EditorGUI.DrawRect(EditorGUILayout.BeginHorizontal(new GUIStyle(GUI.skin.box)), new Color(0.15f, 0.15f, 0.15f));
            Rect foldoutRect = GUILayoutUtility.GetRect(500f, 16f);
            string strName = "RepareOverlap Foldout";
            EditorPrefs.SetBool(strName, EditorGUI.Foldout(foldoutRect, EditorPrefs.GetBool(strName), "修复重叠", true));
            EditorGUILayout.EndHorizontal();
            if (EditorPrefs.GetBool(strName))
            {
                EditorGUILayout.BeginVertical("Box");
                offsetVal = EditorGUILayout.FloatField("距离误差",this.offsetVal);
                offsetVal = Mathf.Max(0f, offsetVal);
                EditorGUILayout.Space();
                string scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
                if (GUILayout.Button(new GUIContent("清除重叠 (选中SimpleGrass下子节点)", "清除重叠. (选中SimpleGrass下子节点)"), GUILayout.Width(300)))
                {
                    //System.DateTime dt = System.DateTime.Now;
                    ////备份场景文件
                    //string dtstr = dt.ToString("yyyy-MM-dd-HH-mm");
                    //string path1 = "LightMapBAK\\SceneBAK\\" + Path.GetDirectoryName(scenePath) + "\\"+ dtstr + "\\";
                    //string filename1 = Path.GetFileName(scenePath);
                    //if (!Directory.Exists(path1))
                    //{
                    //    Directory.CreateDirectory(path1);
                    //}
                    //string pullfile = path1 + filename1;
                    //System.IO.File.Copy(scenePath, pullfile,true);
                    //sourceScenePath = scenePath;
                    //bakScenePath = pullfile;
                    ////备份植被保存文件
                    //sourceSaveDataPath = "";
                    //bakSaveDataPath = "";
                    //SimpleGrass.SimpleGrassSys grassSys = GameObject.FindObjectOfType<SimpleGrass.SimpleGrassSys>();
                    //if (grassSys != null)
                    //{
                    //    SimpleGrass.SimpleSaveData saveData = grassSys.SaveDataProfile;
                    //    if(saveData != null)
                    //    {
                    //        sourceSaveDataPath = AssetDatabase.GetAssetPath(saveData);
                    //        string assetfilename = Path.GetFileName(sourceSaveDataPath);
                    //        bakSaveDataPath = path1 + assetfilename;
                    //        System.IO.File.Copy(sourceSaveDataPath, bakSaveDataPath, true);
                    //    }
                    //}                    

                    if (UnityEditor.EditorUtility.DisplayDialog("修复提示", "请确定已做好场景保存和备份 ？", "确认", "取消"))
                    {
                        if (!_RepareOverlapGrasses(offsetVal,scenePath))
                        {
                            sourceScenePath = "";
                            bakScenePath = "";
                            sourceSaveDataPath = "";
                            bakSaveDataPath = "";
                        }
                        PlayerPrefs.SetString("sourceScenePath", sourceScenePath);
                        PlayerPrefs.SetString("bakScenePath", bakScenePath);
                        PlayerPrefs.SetString("sourceSaveDataPath", sourceSaveDataPath);
                        PlayerPrefs.SetString("bakSaveDataPath", bakSaveDataPath);
                    }
                }

                sourceScenePath = PlayerPrefs.GetString("sourceScenePath");
                bakScenePath = PlayerPrefs.GetString("bakScenePath");
                sourceSaveDataPath = PlayerPrefs.GetString("sourceSaveDataPath");
                bakSaveDataPath = PlayerPrefs.GetString("bakSaveDataPath");
                if (sourceScenePath == scenePath)
                {
                    if (GUILayout.Button(new GUIContent("恢复场景", "恢复场景)"), GUILayout.Width(80)))
                    {
                        //恢复数据文件
                        if (System.IO.File.Exists(bakSaveDataPath))
                        {
                            if (System.IO.File.Exists(sourceSaveDataPath))
                            {
                                System.IO.File.Delete(sourceSaveDataPath);
                            }
                            System.IO.File.Copy(bakSaveDataPath, sourceSaveDataPath);
                            AssetDatabase.ImportAsset(sourceSaveDataPath, ImportAssetOptions.ForceUpdate);
                            SimpleGrass.SimpleGrassSys grassSys = GameObject.FindObjectOfType<SimpleGrass.SimpleGrassSys>();
                            if (grassSys != null)
                            {
                                SimpleGrass.SimpleSaveData saveData = grassSys.SaveDataProfile;
                                if (saveData != null)
                                {
                                    EditorUtility.SetDirty(saveData);
                                }
                            }
                        }
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        //恢复场景文件
                        if (System.IO.File.Exists(bakScenePath))
                        {
                            if (System.IO.File.Exists(sourceScenePath))
                            {
                                System.IO.File.Delete(sourceScenePath);
                            }
                            System.IO.File.Copy(bakScenePath, sourceScenePath);
                            AssetDatabase.ImportAsset(sourceScenePath, ImportAssetOptions.ForceUpdate);
                        }

                        willDeleteList.Clear();
                        sourceScenePath = "";
                        bakScenePath = "";
                        sourceSaveDataPath = "";
                        bakSaveDataPath = "";
                        PlayerPrefs.SetString("sourceScenePath", "");
                        PlayerPrefs.SetString("bakScenePath", "");
                        PlayerPrefs.SetString("sourceSaveDataPath", "");
                        PlayerPrefs.SetString("bakSaveDataPath", "");

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void _RepareOneGroup(GrassGroup oneGroup1, GrassGroup oneGroup2,float sqrOffset)
        {
            //组内整理
            if (!oneGroup1.selfok)
            {
                for (int index = 0; index < oneGroup1.grassList.Count; ++index)
                {
                    if (oneGroup1.grassList[index] == null)
                    {
                        continue;
                    }

                    for (int index2 = 0; index2 < oneGroup1.grassList.Count; ++index2)
                    {
                        if (index == index2)
                        {
                            continue;
                        }
                        if (oneGroup1.grassList[index2] == null)
                        {
                            continue;
                        }
                        float dx = oneGroup1.grassPosList[index2].x - oneGroup1.grassPosList[index].x;
                        if (Mathf.Abs(dx) > 5f)
                        {
                            continue;
                        }
                        float dz = oneGroup1.grassPosList[index2].z - oneGroup1.grassPosList[index].z;
                        if (Mathf.Abs(dz) > 5f)
                        {
                            continue;
                        }
                        float dy = oneGroup1.grassPosList[index2].y - oneGroup1.grassPosList[index].y;
                        if (Mathf.Abs(dy) > 5f)
                        {
                            continue;
                        }

                        if ((dx * dx + dy * dy + dz * dz) <= sqrOffset)
                        {
                            willDeleteList.Add(oneGroup1.grassList[index2]);
                            oneGroup1.grassList[index2] = null;
                        }
                    }
                    oneGroup1.selfok = true;
                }
            }

            for (int index = 0; index < oneGroup1.grassList.Count; ++index)
            {
                if (oneGroup1.grassList[index] == null)
                {
                    continue;
                }
                Vector3 pos1 = oneGroup1.grassPosList[index];
                
                for (int index2 = 0; index2 < oneGroup2.grassList.Count; ++index2)
                {
      
                    if (oneGroup2.grassList[index2] == null)
                    {
                        continue;
                    }
                    Vector3 pos2 = oneGroup2.grassPosList[index2];
                    float dx = pos1.x - pos2.x;
                    if (Mathf.Abs(dx) > 5f)
                    {
                        continue;
                    }
                    float dz = pos1.z - pos2.z;
                    if (Mathf.Abs(dz) > 5f)
                    {
                        continue;
                    }
                    float dy = pos1.y - pos2.y;
                    if (Mathf.Abs(dy) > 5f)
                    {
                        continue;
                    }

                    if ((dx * dx + dy * dy + dz * dz) <= sqrOffset)
                    {
                        willDeleteList.Add(oneGroup2.grassList[index2]);
                        oneGroup2.grassList[index2] = null;
                    }
                }
            }
        }
        private bool _RepareOverlapGrasses(float offset, string scenePath)
        {
            float sqrOffset = offset * offset;
            List<GrassGroup> grassGroupList = new List<GrassGroup>();
            willDeleteList.Clear();
            for (int selIndex = 0; selIndex < UnityEditor.Selection.transforms.Length; ++selIndex)
            {
                Transform root = UnityEditor.Selection.transforms[selIndex];
                int count = root.childCount;
                SimpleGrass.SimpleGrassProtoInfo protoinfo = root.GetComponent<SimpleGrass.SimpleGrassProtoInfo>();
                if (protoinfo != null)
                {
                    for (int index = 0; index < count; ++index)
                    {
                        Transform child = root.GetChild(index);
                        int grassCount = child.childCount;
                        if (grassCount > 0)
                        {
                            Vector3 center = Vector3.zero;
                            GrassGroup oneGroup = new GrassGroup();
                            for (int grass = 0; grass < grassCount; ++grass)
                            {
                                Transform grassChild = child.GetChild(grass);
                                {
                                    oneGroup.grassList.Add(grassChild);
                                    oneGroup.grassPosList.Add(grassChild.position);
                                    center += grassChild.position;
                                }
                            }
                            center /= grassCount;
                            oneGroup.center = center;
                            grassGroupList.Add(oneGroup);
                        }
                    }
                    for (int group1 = 0; group1 < grassGroupList.Count; ++group1)
                    {
                        GrassGroup oneGroup1 = grassGroupList[group1];
                        for (int group2 = 0; group2 < grassGroupList.Count; ++group2)
                        {
                            if (group1 == group2)
                            {
                                continue;
                            }
                            GrassGroup oneGroup2 = grassGroupList[group2];
                            if ((oneGroup1.center - oneGroup2.center).sqrMagnitude > 400)
                            {
                                continue;
                            }
                            _RepareOneGroup(oneGroup1, oneGroup2, sqrOffset);

                        }
                    }
                }

            }

            if (UnityEditor.EditorUtility.DisplayDialog("修复提示", "准备删除：" + willDeleteList.Count.ToString() + "个对象", "确认", "取消"))
            {
                if (willDeleteList.Count > 0)
                {
                    //先备份文件
                    backFiles(scenePath);

                    Undo.IncrementCurrentGroup();
                    for (int index = 0; index < willDeleteList.Count; ++index)
                    {
                        //  Undo.DestroyObjectImmediate(willDeleteList[index].gameObject);
                        DestroyImmediate(willDeleteList[index].gameObject, false);
                    }

                    SimpleGrass.SimpleGrassSysEditor.SaveGrassData(true);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    //Undo.RegisterFullObjectHierarchyUndo(root, "undo repair overlap");
                    return true;
                }
                else
                {
                    UnityEditor.EditorUtility.DisplayDialog("修复提示", "请选择[SimpleGrass]节点下的子节点， 进行操作。", "确认");
                }
            }

            return false;
        }
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

