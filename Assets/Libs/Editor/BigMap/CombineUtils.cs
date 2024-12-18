using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;

public   class CombineUtils
{
    public static void CombineMesh(GameObject gameObject)
    {
        Component[] filters = gameObject.GetComponentsInChildren(typeof(MeshFilter));
        Matrix4x4 myTransform = gameObject.transform.worldToLocalMatrix;
        Hashtable materialToMesh = new Hashtable();

        string combined_name = SceneManager.GetActiveScene().name + "_" + gameObject.name + "_CombinedMesh";
        

        for (int i = 0; i < filters.Length; i++)
        {
            MeshFilter filter = (MeshFilter)filters[i];
            Renderer curRenderer = filters[i].GetComponent<Renderer>();
            MeshCombineUtility.MeshInstance instance = new MeshCombineUtility.MeshInstance();
            instance.mesh = filter.sharedMesh;

            if (curRenderer != null && curRenderer.enabled && instance.mesh != null)
            {
                instance.transform = myTransform * filter.transform.localToWorldMatrix;

                Material[] materials = curRenderer.sharedMaterials;
                for (int m = 0; m < materials.Length; m++)
                {
                    instance.subMeshIndex = System.Math.Min(m, instance.mesh.subMeshCount - 1);

                    ArrayList objects = (ArrayList)materialToMesh[materials[m]];
                    if (objects != null)
                    {
                        objects.Add(instance);
                    }
                    else
                    {
                        objects = new ArrayList();
                        objects.Add(instance);
                        materialToMesh.Add(materials[m], objects);
                    }
                }
                //curRenderer.enabled = false;
            }
        }
        EnableChildren(gameObject, false);
        int idx = 1;
        foreach (DictionaryEntry de in materialToMesh)
        {
            ArrayList elements = (ArrayList)de.Value;
            MeshCombineUtility.MeshInstance[] instances = (MeshCombineUtility.MeshInstance[])elements.ToArray(typeof(MeshCombineUtility.MeshInstance));

            //// We have a maximum of one material, so just attach the mesh to our own game object
            //if (materialToMesh.Count == 1)
            //{
            //    // Make sure we have a mesh filter & renderer
            //    if (gameObject.GetComponent(typeof(MeshFilter)) == null)
            //        gameObject.AddComponent(typeof(MeshFilter));
            //    if (!gameObject.GetComponent("MeshRenderer"))
            //        gameObject.AddComponent("MeshRenderer");

            //    MeshFilter filter = (MeshFilter)gameObject.GetComponent(typeof(MeshFilter));
            //    if (Application.isPlaying) filter.mesh = MeshCombineUtility.Combine(instances, true);
            //    else filter.sharedMesh = MeshCombineUtility.Combine(instances, true);
            //    gameObject.renderer.material = (Material)de.Key;
            //    gameObject.renderer.enabled = true;
            //}
            //// We have multiple materials to take care of, build one mesh / gameobject for each material
            //// and parent it to this object
            //else
            {
                var comMesh = MeshCombineUtility.Combine(instances, true);
                string comPath = "Assets/Scene Assets/Static/Mesh/combined/";
                if(!System.IO.Directory.Exists(comPath))
                {
                    System.IO.Directory.CreateDirectory(comPath);
                }
                string assetName = comPath + combined_name + "_" + idx + ".asset";
                AssetDatabase.CreateAsset(comMesh, assetName);

                var mesh = AssetDatabase.LoadAssetAtPath(assetName, typeof(Mesh)) as Mesh;

                GameObject go = new GameObject(combined_name + "_" + idx);
                go.layer = gameObject.layer;
                go.transform.parent = gameObject.transform;
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localPosition = Vector3.zero;
                go.AddComponent(typeof(MeshFilter));
                go.AddComponent<MeshRenderer>();
                go.GetComponent<Renderer>().material = (Material)de.Key;
                MeshFilter filter = (MeshFilter)go.GetComponent(typeof(MeshFilter));
                if (Application.isPlaying) filter.mesh = mesh;
                else filter.sharedMesh = mesh;

                string comPrefabPath = "Assets/Scene Assets/Static/Prefab/combined/";
                if (!System.IO.Directory.Exists(comPrefabPath))
                {
                    System.IO.Directory.CreateDirectory(comPrefabPath);
                }
                var prefabObj = GetPrefab(go, comPrefabPath + combined_name + "_" + idx + ".prefab");

                var objClone = PrefabUtility.InstantiatePrefab(prefabObj) as GameObject;

               // var objClone = GameObject.Instantiate(prefabObj) as GameObject;
                objClone.layer = go.layer;
                objClone.transform.parent = gameObject.transform;
                objClone.transform.localScale = Vector3.one;
                objClone.transform.localRotation = Quaternion.identity;
                objClone.transform.localPosition = Vector3.zero;
                objClone.name = go.name;                

                Object.DestroyImmediate(go);
            }

            idx++;
        }      

    }

    static void EnableChildren(GameObject go, bool enable)
    {
        foreach (Transform ct in go.transform)
        {
            ct.gameObject.SetActiveRecursively(enable);
        }
    }

    public static Object GetPrefab(GameObject go, string name)
    {
        Object tempPrefab = PrefabUtility.CreatePrefab(name, go);
      //  tempPrefab = PrefabUtility.ReplacePrefab(go, tempPrefab);
        //Object.DestroyImmediate(go);
        return tempPrefab;
    }
}

