using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class TransfromExtensions
{
    /// <summary>
    /// 把Prefab中的Transform替换为RectTransform
    /// 有个不足的地方是prefab的继承关系会丢失（Variant关系丢失）
    /// 如果一个prefab中引用另一个prefab，这种关系也会丢失
    /// </summary>
    /// <typeparam name="T">RectTransform</typeparam>
    /// <param name="obj"></param>
    public static void AddComponentFromPrefab<T>(GameObject obj) where T : UnityEngine.Component
    {
        PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(obj);
        //Debug.Log($"AddComponentFromPrefab,name:{obj.name},PrefabAssetType:{prefabType}");
        switch (prefabType)
        {

            case PrefabAssetType.Regular:
            case PrefabAssetType.Variant:
                //获取prefab实例的根节点
                GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
                //获取prefab资源，在project中的
                GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                //Object prefabAsset = PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj);
                //获取资源的路径名字
                string assetPath = AssetDatabase.GetAssetPath(prefabAsset);
                //Debug.Log(assetPath);

                //断开Model的联系，如果是Model是禁止修改RectTransform的
                PrefabUtility.UnpackPrefabInstance(prefabRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction); //有个不足的地方是prefab的继承关系会丢失,这里是完全断开连接，非完全的方式没有测试
                PrefabUtility.SaveAsPrefabAssetAndConnect(prefabRoot, assetPath, InteractionMode.AutomatedAction);

                //修改prefab的RectTransform，如果修改实例的RectTransfom是失败的（Model虽然断开连接，但是还是禁止直接修改prefab实例的RectTransfom）
                GameObject pobj = PrefabUtility.LoadPrefabContents(assetPath);
                var ts = pobj.GetComponentsInChildren<Transform>();
                foreach (var t in ts)
                    if (!(t is RectTransform))
                        t.gameObject.AddComponent<T>();
                PrefabUtility.SaveAsPrefabAsset(pobj, assetPath);
                PrefabUtility.UnloadPrefabContents(pobj);
                break;
            case PrefabAssetType.Model:
                PrefabUtility.UnpackPrefabInstanceAndReturnNewOutermostRoots(obj, PrefabUnpackMode.Completely);
                if (!(obj.transform is T))
                    obj.AddComponent<T>();
                break;
            default:
                break;
        }


    }
}
