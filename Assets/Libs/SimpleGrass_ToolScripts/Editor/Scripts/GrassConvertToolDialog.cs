using log4net.Filter;
using SimpleGrass;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GrassConvertToolDialog : EditorWindow
{
    [MenuItem("MY_Support/草系统/GrassConvertTool")]
    public static void ShowWindow()
    {
        GrassConvertToolDialog window = EditorWindow.GetWindow<GrassConvertToolDialog>("GrassConvertTool");
        window.Show();
    }

    bool bindGrassSystem = false;
    bool expandInstanceList = false;
    bool expandPrefabList = false;
    bool expandCopyGrassList = false;
    bool expandCopyPrefabList = false;
    bool orginGrassState = true;
    bool copyGrassState = true;
    Vector2 instanceScrollPos = Vector2.zero;
    Vector2 prefabScrollPos = Vector2.zero;
    Vector2 copyListScrollPos = Vector2.zero;
    Vector2 copyPrefabScrollPos = Vector2.zero;
    Box box = new Box();

    string grassMatch = "";

    Camera sampleTerrainCamera = null;
    LayerMask layerMask = -1;
    
    SimpleGrassSys simpleGrassSys = null;
    GameObject grassRoot = null;

    HashSet<GameObject> grassPrefabHashSet = new HashSet<GameObject>();
    List<GrassInstance> grassInstanceList = new List<GrassInstance>();

    List<GrassChunk> copyGrassList = new List<GrassChunk>();
    List<GameObject> grassPrefabRepairedList = new List<GameObject>();

    class GrassInstance
    {
        public GameObject grassItem;
        public GameObject grassPrefab;
        public GrassInstance(GameObject grassItem, GameObject grassPrefab)
        {
            this.grassItem = grassItem;
            this.grassPrefab = grassPrefab;
        }
    }

    class GrassChunk
    {
        public bool showChild = false;
        public SimpleGrassChunk simpleGrassChunk;
        public List<GameObject> grassList = new List<GameObject>();
    }

    struct Box
    {
        public float xMin;
        public float xMax;
        public float yMin;
        public float yMax;
        public float zMin;
        public float zMax;

        public Vector3 subBoxSize;

        public Vector3 position
        {
            get
            {
                return new Vector3(xMin, yMin, zMin);
            }
        }

        public void RefreshBoxSize(Transform transform)
        {
            xMin = Mathf.Min(transform.position.x, xMin);
            xMax = Mathf.Max(transform.position.x, xMax);
            yMin = Mathf.Min(transform.position.y, yMin);
            yMax = Mathf.Max(transform.position.y, yMax);
            zMin = Mathf.Min(transform.position.z, zMin);
            zMax = Mathf.Max(transform.position.z, zMax);
        }

        public Vector3 Size
        {
            get
            {
                Vector3 size = new Vector3(xMax - xMin, yMax - yMin, zMax - zMin);
                size.x = Mathf.Abs(size.x);
                size.y = Mathf.Abs(size.y);
                size.z = Mathf.Abs(size.z);
                return size;
            }
        }

        public Vector3 GetSubBoxIndex(Transform transform)
        {
            return GetSubBoxIndex(transform.position, this.subBoxSize);
        }

        public Vector3 GetSubBoxIndex(Vector3 positionWS, Vector3 subBoxSize)
        {
            Vector3 offset = positionWS - position;
            //Vector3Int result = Vector3Int.zero;
            offset.x = (int)Mathf.Round(offset.x / Mathf.Max(0.1f, subBoxSize.x));
            offset.y = (int)Mathf.Round(offset.y / Mathf.Max(0.1f, subBoxSize.y));
            offset.z = (int)Mathf.Round(offset.z / Mathf.Max(0.1f, subBoxSize.z));
            return offset;
        }

        public Vector3 GetSubBoxIndexCenterWorldSpace(Vector3 boxIndex)
        {
            return GetSubBoxIndexCenterWorldSpace(boxIndex, this.subBoxSize);
        }

        public Vector3 GetSubBoxIndexCenterWorldSpace(Vector3 boxIndex, Vector3 subBoxSize)
        {
            Vector3 result = new Vector3(boxIndex.x * Mathf.Max(0.1f, subBoxSize.x),
                            boxIndex.y * Mathf.Max(0.1f, subBoxSize.y),
                            boxIndex.z * Mathf.Max(0.1f, subBoxSize.z));
            result += position;
            return result;
        }

        public Vector3 GetPositionWS(Vector3 positionBS)
        {
            return positionBS + position;
        }
    }

    #region UnityFun
    private void Awake()
    {
        position = new Rect(400, 200, 400, 600);
        box.subBoxSize = new Vector3(2, 2, 2);
        CheckGrassSystem();
        CheckSampleTerrainCamera();
    }

    private void OnDestroy()
    {
        if(sampleTerrainCamera != null)
        {
            GameObject.DestroyImmediate(sampleTerrainCamera.gameObject);
        }
        sampleTerrainCamera = null;
    }

    private void OnGUI()
    {
        GUILayout.Space(20);
        EditorGUILayout.LabelField("GrassSystem");
        if (GUILayout.Button("RefreshGrassSystem"))
        {
            CheckGrassSystem();
        }
        if(simpleGrassSys != null)
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Grass System", simpleGrassSys, typeof(SimpleGrassSys), true);
            GUI.enabled = true;
        }
        else
        {
            EditorGUILayout.HelpBox("请先创建草系统", MessageType.Info);
            return;
        }

        grassMatch = EditorGUILayout.TextField("草名字匹配，多个用逗号隔开", grassMatch);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("原节点");
        grassRoot = EditorGUILayout.ObjectField("Orgin Grass Root", grassRoot, typeof(GameObject), true) as GameObject;
        if(!grassRoot)
        {
            EditorGUILayout.HelpBox("选择原先草根路径", MessageType.Info);
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("RefreshGrassList"))
        {
            ExtractGrassList();
            RefreshGrassInstanceList();
        }
        if(grassInstanceList.Count > 0)
        {
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Change Orgin Grasses Active"))
            {
                for (int i = 0; i < grassInstanceList.Count; i++)
                {
                    grassInstanceList[i].grassItem.SetActive(!orginGrassState);
                }
                orginGrassState = !orginGrassState;
            }
            expandInstanceList = EditorGUILayout.Foldout(expandInstanceList, 
                "Grass Instances: Total " + grassInstanceList.Count.ToString(), true);
            if (expandInstanceList)
            {
                EditorGUI.indentLevel++;
                instanceScrollPos = GUILayout.BeginScrollView(instanceScrollPos, GUILayout.MaxHeight(300));
                GUI.enabled = false;
                for(int i = 0; i < grassInstanceList.Count; i++)
                {
                    var grassInstance = grassInstanceList[i];
                    EditorGUILayout.ObjectField(grassInstance.grassItem, typeof(GameObject), true);
                }
                GUI.enabled = true;
                GUILayout.EndScrollView();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);
            expandPrefabList = EditorGUILayout.Foldout(expandPrefabList, 
                "Grass Prefabs: Total " + grassPrefabHashSet.Count.ToString(), true);
            if (expandPrefabList)
            {
                EditorGUI.indentLevel++;
                prefabScrollPos = GUILayout.BeginScrollView(prefabScrollPos, GUILayout.MaxHeight(100));
                GUI.enabled = false;
                foreach(var prefab in grassPrefabHashSet)
                {
                    EditorGUILayout.ObjectField(prefab, typeof(GameObject), false);
                }
                GUI.enabled = true;
                GUILayout.EndScrollView();
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField("ConvertSettings");
            CheckSampleTerrainCamera();
            GUI.enabled = false;
            EditorGUILayout.Vector3Field("BoxSize", box.Size);
            EditorGUILayout.ObjectField("SampleCamera", sampleTerrainCamera, typeof(Camera), true);
            
            GUI.enabled = true;
            layerMask = EditorGUILayout.MaskField("Cull Mask", 
                InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layerMask), InternalEditorUtility.layers);
            layerMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(layerMask);
            box.subBoxSize = EditorGUILayout.Vector3Field("Split Box Size", box.subBoxSize);
            GUILayout.Space(20);
            if (GUILayout.Button("Convert"))
            {
                Convert();
            }
        }

        if(copyGrassList.Count > 0)
        {
            GUILayout.Space(10);
            if (GUILayout.Button("Change Copy Grasses Active"))
            {
                for (int i = 0; i < copyGrassList.Count; i++)
                {
                    copyGrassList[i].simpleGrassChunk.gameObject.SetActive(!copyGrassState);
                }
                copyGrassState = !copyGrassState;
            }
            expandCopyGrassList = EditorGUILayout.Foldout(expandCopyGrassList,"ConverResult Total Chunk:" + copyGrassList.Count, true);
            if(expandCopyGrassList)
            {
                EditorGUI.indentLevel++;
                copyListScrollPos = GUILayout.BeginScrollView(copyListScrollPos, GUILayout.MaxHeight(300));
                for(int i = 0; i < copyGrassList.Count; i++)
                {
                    var grassChunk = copyGrassList[i];
                    EditorGUILayout.BeginHorizontal();
                    grassChunk.showChild = EditorGUILayout.Foldout(grassChunk.showChild, "grass chunk", true);
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField(grassChunk.simpleGrassChunk, typeof(SimpleGrassChunk), true);
                    EditorGUILayout.EndHorizontal();
                    if (grassChunk.showChild)
                    {
                        EditorGUI.indentLevel++;
                        for(int j = 0; j < grassChunk.grassList.Count; ++j)
                        {
                            EditorGUILayout.ObjectField(grassChunk.grassList[j], typeof(GameObject), true);
                        }
                        EditorGUI.indentLevel--;
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndScrollView();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space(10);
            expandCopyPrefabList = EditorGUILayout.Foldout(expandCopyPrefabList, "copyGrassPrefab: Total " + grassPrefabRepairedList.Count, true);
            if (expandCopyPrefabList)
            {
                EditorGUI.indentLevel++;
                copyPrefabScrollPos = GUILayout.BeginScrollView(copyPrefabScrollPos, GUILayout.MaxHeight(100));
                GUI.enabled = false;
                for (int i = 0; i < grassPrefabRepairedList.Count; i++)
                {
                    EditorGUILayout.ObjectField(grassPrefabRepairedList[i], typeof(GameObject), false);
                }
                GUI.enabled = true;
                GUILayout.EndScrollView();
                EditorGUI.indentLevel--;
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Delete Orgin Grasses"))
            {
                if(EditorUtility.DisplayDialog("Delete Orgin Grass", "删除后将无法还原，是否确认删除？", "确认", "取消"))
                {
                    for(int i = 0; i < grassInstanceList.Count; i++)
                    {
                        var instance = grassInstanceList[i];
                        GameObject.DestroyImmediate(instance.grassItem);
                        instance.grassItem = null;
                    }
                    grassInstanceList.Clear();
                    grassPrefabHashSet.Clear();
                }
            }
            if(GUILayout.Button("Delete Copy Grasses"))
            {
                if (EditorUtility.DisplayDialog("Delete Copy Grass", "删除后将无法还原，是否确认删除？", "确认", "取消"))
                {
                    int count = copyGrassList.Count;
                    for (int i = 0; i < count; i++)
                    {
                        GameObject.DestroyImmediate(copyGrassList[i].simpleGrassChunk.gameObject);
                    }
                    copyGrassList.Clear();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
    #endregion

    #region privateFun
    private bool CheckGrassSystem()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        GameObject[] rootGameObjects = currentScene.GetRootGameObjects();
        SimpleGrassSys simpleGrassSys = GameObject.FindObjectOfType<SimpleGrassSys>();
        if(simpleGrassSys != null)
        {
            this.simpleGrassSys = simpleGrassSys;
            bindGrassSystem = true;
            return true;
        }
        this.simpleGrassSys = null;
        bindGrassSystem = false;
        return false;
    }

    private void CheckSampleTerrainCamera()
    {
        if(sampleTerrainCamera != null)
        {
            return;
        }
        var cameraGO = GameObject.Find("SampleTerrainCamera");
        if (cameraGO == null)
        {
            GameObject go = new GameObject("SampleTerrainCamera");
            sampleTerrainCamera = go.AddComponent<Camera>();
        }
        else
        {
            sampleTerrainCamera = cameraGO.GetComponent<Camera>();
        }
        sampleTerrainCamera.enabled = false;
    }


    private void BakeTerrainColor(Dictionary<Vector3, List<GrassChunk>> bakeGroupList, Vector3 bakeBoxSize, LayerMask cullingMask)
    {
        if(sampleTerrainCamera == null)
        {
            return;
        }
        sampleTerrainCamera.enabled = true;
        sampleTerrainCamera.backgroundColor = Color.black;
        sampleTerrainCamera.cullingMask = cullingMask;
        sampleTerrainCamera.transform.forward = -Vector3.up;
        sampleTerrainCamera.transform.up = Vector3.forward;
        int texSize = 2048;
        Texture2D tex = new Texture2D(texSize, texSize, TextureFormat.ARGB32, false, true);
        RenderTexture rt = RenderTexture.GetTemporary(texSize, texSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        sampleTerrainCamera.targetTexture = rt;

        foreach(var bakeGroup in bakeGroupList)
        {
            var bakeBoxIndex = bakeGroup.Key;
            var bakeChunkList = bakeGroup.Value;
            Vector3 cameraPos = box.GetSubBoxIndexCenterWorldSpace(bakeBoxIndex, bakeBoxSize);
            cameraPos.y = cameraPos.y + bakeBoxSize.y * 0.5f +  
                (bakeBoxSize.x + box.subBoxSize.x) * 0.65f / Mathf.Tan(sampleTerrainCamera.fieldOfView * Mathf.PI  / 360);
            sampleTerrainCamera.transform.position = cameraPos;
            sampleTerrainCamera.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, texSize, texSize), 0, 0, false);
            tex.Apply(false);

            for (int i = 0; i < bakeChunkList.Count; ++i)
            {
                var chunk = bakeChunkList[i];
                chunk.simpleGrassChunk.childColors = new Vector3[chunk.grassList.Count];
                chunk.simpleGrassChunk.childMaterialBlocks = new MaterialPropertyBlock[chunk.grassList.Count];
                for(int j = 0; j < chunk.grassList.Count; j++)
                {
                    var grassInstance = chunk.grassList[j];
                    Vector3 samplePos = sampleTerrainCamera.WorldToViewportPoint(grassInstance.transform.position);
                    Color terrainColor = tex.GetPixelBilinear(samplePos.x, samplePos.y);
                    chunk.simpleGrassChunk.childColors[j] = new Vector3(terrainColor.r, terrainColor.g, terrainColor.b);
                    MaterialPropertyBlock tempBlock = new MaterialPropertyBlock();
                    MeshRenderer meshRenderer = grassInstance.GetComponent<MeshRenderer>();
                    meshRenderer.GetPropertyBlock(tempBlock);
                    tempBlock.SetColor("_Color1", terrainColor.gamma);
                    meshRenderer.SetPropertyBlock(tempBlock);
                    chunk.simpleGrassChunk.childMaterialBlocks[j] = tempBlock;
                }
            }

            //if (!Directory.Exists("E://Temp"))
            //{
            //    Directory.CreateDirectory("E://Temp");
            //}
            //var data = tex.EncodeToPNG();
            //File.WriteAllBytes("E://Temp//" + "sample " + cameraPos + ".png", data);
        }

        sampleTerrainCamera.targetTexture = null;
        sampleTerrainCamera.enabled = false;
        DestroyImmediate(tex);
        rt.Release();

    }

    private void ExtractGrassList()
    {
        if (grassMatch == null || string.IsNullOrEmpty(grassMatch))
        {
            return;
        }

        if (grassRoot == null)
        {
            return;
        }

        string[] matches = grassMatch.Split(",");
        var meshRenderers = FindObjectsOfType<MeshRenderer>(true);

        for (int i = 0; i < meshRenderers.Length; ++i)
        {
            var gameObject = meshRenderers[i].gameObject;
            for (int j = 0; j < matches.Length; ++j)
            {
                if (gameObject.name.Contains(matches[j]))
                {
                    if (PrefabUtility.IsAnyPrefabInstanceRoot(gameObject) && gameObject.GetComponentInParent<SimpleGrassChunk>(true) == null)
                    {
                        gameObject.transform.SetParent(grassRoot.transform);
                    }
                    break;
                }
            }
        }
    }

    private void RefreshGrassInstanceList()
    {
        if(grassMatch == null || string.IsNullOrEmpty(grassMatch))
        {
            return;
        }
        string[] matches = grassMatch.Split(",");
        grassPrefabHashSet.Clear();
        grassInstanceList.Clear();
        if(grassRoot == null)
        {
            return;
        }

        //var tsList = SearchEndNodes(grassRoot.transform);
        List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        grassRoot.GetComponentsInChildren<MeshRenderer>(true, meshRenderers);

        for(int i = 0; i < meshRenderers.Count; ++i)
        {
            var gameObject = meshRenderers[i].gameObject;
            for(int j = 0; j<matches.Length; ++j)
            {
                if (gameObject.name.Contains(matches[j]))
                {
                    if (PrefabUtility.IsAnyPrefabInstanceRoot(gameObject))
                    {
                        GameObject grassPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
                        box.RefreshBoxSize(gameObject.transform);
                        GrassInstance grassInstance = new GrassInstance(gameObject, grassPrefab);
                        grassInstanceList.Add(grassInstance);
                        grassPrefabHashSet.Add(grassPrefab);
                    }
                    break;
                }
            }
        }

    }

    private List<Transform> SearchEndNodes(Transform root)
    {
        List<Transform> result = new List<Transform>();
        int childCount = root.childCount;
        if(childCount > 0 )
        {
            for(int i = 0; i < childCount; ++i)
            {
                var searchResult = SearchEndNodes(root.GetChild(i));
                result.AddRange(searchResult);
            }
        }
        else
        {
            result.Add(root);
        }
        return result;
    }

    private void Convert()
    {
        if (!simpleGrassSys)
        {
            return;
        }
        if(grassInstanceList == null || grassInstanceList.Count <= 0)
        {
            return;
        }

        GameObject grassRoot = GameObject.Find("SimpleGrass");
        if (grassRoot == null)
        {
            grassRoot = new GameObject("SimpleGrass");
            grassRoot.tag = "EditorOnly";
            grassRoot.transform.localPosition = Vector3.zero;
            grassRoot.transform.localEulerAngles = Vector3.zero;
            grassRoot.transform.localScale = Vector3.one;
        }

        ///<orginPrefab, repairedPrefab>
        Dictionary<GameObject, GameObject> repairedPrefabDic = new Dictionary<GameObject, GameObject>();
        ///<grassPrefab, <protoTypePrefab, subRoot>> 创建原型和放置节点
        Dictionary<GameObject, Tuple<GameObject, Transform>> prefabMapProtoDic = new Dictionary<GameObject, Tuple<GameObject, Transform>>();

        grassPrefabRepairedList.Clear();
        foreach(var prefab in grassPrefabHashSet)
        {
            ///去除lod
            var repairedPrefab = RepairePrefabLod(prefab);
            repairedPrefabDic.Add(prefab, repairedPrefab);
            grassPrefabRepairedList.Add(repairedPrefab);

            var protoType = CreateGrassProtoType(repairedPrefab);
            var subRoot = grassRoot.transform.Find(protoType.name);
            if(subRoot != null)
            {
                RemoveAllChild(subRoot);
            }
            else
            {
                subRoot = new GameObject(protoType.name).transform;
                subRoot.parent = grassRoot.transform;
                subRoot.localPosition = Vector3.zero;
                subRoot.localEulerAngles = Vector3.zero;
                subRoot.localScale = Vector3.one;
            }
            SimpleGrassChunk grassChunk = protoType.GetComponent<SimpleGrassChunk>();
            SimpleGrassSysEditor.CreateAndResetProtoInfo(subRoot.gameObject, grassChunk);

            prefabMapProtoDic.Add(prefab, new Tuple<GameObject, Transform>(protoType, subRoot));
        }

        ///区域划分
        ///subBoxGroup
        Dictionary<Vector3, List<GrassInstance>> grassGroups = new Dictionary<Vector3, List<GrassInstance>>();
        for(int i = 0; i < grassInstanceList.Count; ++i)
        {
            var instance = grassInstanceList[i];
            Vector3 subBoxCenter = box.GetSubBoxIndex(instance.grassItem.transform);
            List<GrassInstance> grassList;
            if(!grassGroups.TryGetValue(subBoxCenter, out grassList))
            {
                grassList = new List<GrassInstance>();
                grassGroups.Add(subBoxCenter, grassList);
            }
            grassList.Add(instance);
        }

        ///归类
        Dictionary<Transform, GrassChunk> groupTypes = new Dictionary<Transform, GrassChunk>();
        ///sampleTerrainGroup
        Dictionary<Vector3, List<GrassChunk>> sampleTerrainGroup = new Dictionary<Vector3, List<GrassChunk>>();
        Vector3 sampleTerrainBox = new Vector3(100, 20, 100);
        copyGrassList.Clear();
        foreach (var item in grassGroups)
        {
            groupTypes.Clear();
            var groupPos = item.Key;
            var grassGroup = item.Value;
            
            ///<subRoot, chunk>
            for(int i = 0; i < grassGroup.Count; ++i)
            {
                var instance = grassGroup[i];
                var protoTypeAndSubroot = prefabMapProtoDic[instance.grassPrefab];
                GrassChunk chunk = null;
                if(!groupTypes.TryGetValue(protoTypeAndSubroot.Item2, out chunk))
                {
                    GameObject protoChunkGO = Instantiate(protoTypeAndSubroot.Item1);
                    Vector3 chunkPos = box.GetSubBoxIndexCenterWorldSpace(groupPos);
                    chunkPos.x += UnityEngine.Random.Range(-box.subBoxSize.x * 0.2f, box.subBoxSize.x * 0.2f);
                    chunkPos.z += UnityEngine.Random.Range(-box.subBoxSize.z * 0.2f, box.subBoxSize.z * 0.2f);
                    chunkPos.y += box.subBoxSize.y * 0.5f;
                    protoChunkGO.transform.position = chunkPos;
                    protoChunkGO.transform.parent = protoTypeAndSubroot.Item2;
                    chunk = new GrassChunk();
                    chunk.simpleGrassChunk = protoChunkGO.GetComponent<SimpleGrassChunk>();
                    chunk.simpleGrassChunk.selfPrefab = repairedPrefabDic[instance.grassPrefab];
                    chunk.simpleGrassChunk.BuildCollider();
                    groupTypes.Add(protoTypeAndSubroot.Item2, chunk);
                    copyGrassList.Add(chunk);
                }
                GameObject copyGrass = PrefabUtility.InstantiatePrefab(repairedPrefabDic[instance.grassPrefab],
                    chunk.simpleGrassChunk.transform) as GameObject;
                DeepCopyGameObject(instance.grassItem, copyGrass);
                copyGrass.SetActive(true);
                chunk.grassList.Add(copyGrass);
            }

            foreach(var groupType in groupTypes)
            {
                var grassChunk = groupType.Value;
                Vector3 subBoxIndex = box.GetSubBoxIndex(grassChunk.simpleGrassChunk.transform.position, sampleTerrainBox);
                List<GrassChunk> chunkList;
                if(!sampleTerrainGroup.TryGetValue(subBoxIndex, out chunkList))
                {
                    chunkList = new List<GrassChunk>();
                    sampleTerrainGroup.Add(subBoxIndex, chunkList);
                }
                chunkList.Add(grassChunk);
            }
        }

        BakeTerrainColor(sampleTerrainGroup, sampleTerrainBox, layerMask);
    }

    private GameObject RepairePrefabLod(GameObject orginPrefab)
    {
        if (!orginPrefab)
        {
            return null;
        }
        LODGroup lodGroup;
        if(orginPrefab.TryGetComponent<LODGroup>(out lodGroup))
        {
            string repairedPrefabName = orginPrefab.name + "_WithoutLod";
            string repairedPrefabPath = "Assets/SimpleGrass/RepairedGrassPrefabs/" + repairedPrefabName + ".prefab";
            if (!File.Exists(repairedPrefabPath))
            {
                if (!Directory.Exists("Assets/SimpleGrass/RepairedGrassPrefabs/"))
                {
                    Directory.CreateDirectory("Assets/SimpleGrass/RepairedGrassPrefabs/");
                    AssetDatabase.Refresh();
                }
                GameObject copyGO = Instantiate(orginPrefab);
                RemoveLODGroup(copyGO);
                PrefabUtility.SaveAsPrefabAsset(copyGO, repairedPrefabPath);
                DestroyImmediate(copyGO);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            return AssetDatabase.LoadAssetAtPath<GameObject>(repairedPrefabPath);
        }
        else
        {
            return orginPrefab;
        }
    }
    private GameObject CreateGrassProtoType(GameObject grassPrefab)
    {
        if(grassPrefab == null)
        {
            return null;
        }
        if (!Directory.Exists(SimpleEditorCommon.PrefabPath))
        {
            Directory.CreateDirectory(SimpleEditorCommon.PrefabPath);
            AssetDatabase.Refresh();
        }
        SimpleGrassChunk simpleGrassChunk;
        GameObject protoTypePrefab;
        Scene currentScene = SceneManager.GetActiveScene();
        string protoTypePrefabName = grassPrefab.name + "_" + currentScene.name;
        string prototypePrefabPath = SimpleEditorCommon.PrefabPath + protoTypePrefabName + ".prefab";
        if(!File.Exists(prototypePrefabPath))
        {
            protoTypePrefab = new GameObject(protoTypePrefabName);
            PrefabUtility.SaveAsPrefabAsset(protoTypePrefab, prototypePrefabPath);
        }
        protoTypePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prototypePrefabPath);
        if (!protoTypePrefab.TryGetComponent<SimpleGrassChunk>(out simpleGrassChunk))
        {
            simpleGrassChunk = protoTypePrefab.AddComponent<SimpleGrassChunk>();
            simpleGrassChunk.grassPrefab = grassPrefab;
            simpleGrassChunk.ColliderScale = new Vector3(1.5f, 1.5f, 1.5f);
            simpleGrassChunk.CullingMaxDistance = Common.DefMaxDistance;
            simpleGrassChunk.MergeChunkDistance = 10;
            simpleGrassChunk.CastShadows = false;
            simpleGrassChunk.ReceiveShadows = true;
            simpleGrassChunk.LayerID = 0;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return AssetDatabase.LoadAssetAtPath<GameObject>(prototypePrefabPath);
    }

    private void RemoveAllChild(Transform parent)
    {
        List<Transform> children = new List<Transform>();
        for(int i = 0; i<parent.childCount; i++)
        {
            children.Add(parent.GetChild(i));
        }
        for(int i = 0; i < children.Count; i++)
        {
            GameObject.DestroyImmediate(children[i].gameObject);
        }
    }

    private void RemoveLODGroup(GameObject gameObject)
    {
        LODGroup lodGroup = null;
        if(gameObject.TryGetComponent<LODGroup>(out lodGroup))
        {
            LOD[] lods = lodGroup.GetLODs();
            if(lods.Length > 1)
            {
                for(int index = 1; index < lods.Length; index++)
                {
                    var renderers = lods[index].renderers;
                    for(int j =  0; j < renderers.Length; j++)
                    {
                        GameObject.DestroyImmediate(renderers[j].gameObject);
                    }
                }
            }
            GameObject.DestroyImmediate(lodGroup);
        }
    }

    private void DeepCopyGameObject(GameObject orginGO, GameObject targetGO)
    {
        Component[] targetComponents = targetGO.GetComponents<Component>();
        foreach(var targetComponent in targetComponents)
        {
            Component orginComponent;
            if(targetGO.TryGetComponent(targetComponent.GetType(), out orginComponent))
            {
                ComponentUtility.CopyComponent(orginComponent);
                ComponentUtility.PasteComponentValues(targetComponent);
            }
        }
        targetGO.transform.position = orginGO.transform.position;
        targetGO.transform.rotation = orginGO.transform.rotation;
        targetGO.transform.localScale = orginGO.transform.localScale;
        MeshRenderer targetMR;
        if(targetGO.TryGetComponent<MeshRenderer>(out targetMR))
        {
            MeshRenderer orginMR;
            if(orginGO.TryGetComponent<MeshRenderer>(out orginMR))
            {
                targetMR.lightmapIndex = orginMR.lightmapIndex;
                targetMR.lightmapScaleOffset = orginMR.lightmapScaleOffset;
            }
        }
        for(int i = 0; i < targetGO.transform.childCount; i++)
        {
            var targetChild = targetGO.transform.GetChild(i);
            var child = orginGO.transform.Find(targetChild.name);
            if(child != null)
            {
                DeepCopyGameObject(child.gameObject, targetChild.gameObject);
            }
        }
    }

    #endregion
}
