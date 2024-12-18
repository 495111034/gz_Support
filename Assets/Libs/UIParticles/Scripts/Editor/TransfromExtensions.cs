using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class TransfromExtensions
{
    /// <summary>
    /// ��Prefab�е�Transform�滻ΪRectTransform
    /// �и�����ĵط���prefab�ļ̳й�ϵ�ᶪʧ��Variant��ϵ��ʧ��
    /// ���һ��prefab��������һ��prefab�����ֹ�ϵҲ�ᶪʧ
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
                //��ȡprefabʵ���ĸ��ڵ�
                GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
                //��ȡprefab��Դ����project�е�
                GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                //Object prefabAsset = PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj);
                //��ȡ��Դ��·������
                string assetPath = AssetDatabase.GetAssetPath(prefabAsset);
                //Debug.Log(assetPath);

                //�Ͽ�Model����ϵ�������Model�ǽ�ֹ�޸�RectTransform��
                PrefabUtility.UnpackPrefabInstance(prefabRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction); //�и�����ĵط���prefab�ļ̳й�ϵ�ᶪʧ,��������ȫ�Ͽ����ӣ�����ȫ�ķ�ʽû�в���
                PrefabUtility.SaveAsPrefabAssetAndConnect(prefabRoot, assetPath, InteractionMode.AutomatedAction);

                //�޸�prefab��RectTransform������޸�ʵ����RectTransfom��ʧ�ܵģ�Model��Ȼ�Ͽ����ӣ����ǻ��ǽ�ֱֹ���޸�prefabʵ����RectTransfom��
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
