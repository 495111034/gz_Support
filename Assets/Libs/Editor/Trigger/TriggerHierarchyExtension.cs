

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class TriggerHierarchyExtension:Editor
{

    [MenuItem("GameObject/Trigger/添加触发器脚本", false, 1)]
    public static void AddTriggerScript()
    {
        GameObject g = Selection.gameObjects[0];
        g.AddMissingComponent<ScenarioBehaviour>();
    }

    [MenuItem("GameObject/Trigger/添加相机追踪触发器", false, 1)]
    public static void CameraTrace()
    {

        string name = CreateName("CameraTrace");
        if (name != null)
            AddTraceTrigger(name).transform.SetParent(Selection.gameObjects[0].transform, false);

    }

    [MenuItem("GameObject/Trigger/添加普通触发器", false, 1)]
    public static void AnimationTrigger()
    {

        string name = CreateName("NomalTrigger");
        if (name != null)
        {
            GameObject g = new GameObject();
            g.transform.localPosition = Vector3.zero;
            g.name = name;
            //-----------------------------------------------------//
            //添加碰撞体
            BoxCollider box = g.AddMissingComponent<BoxCollider>();
            box.isTrigger = true;
            //-----------------------------------------------------//
            g.layer = LayerMask.NameToLayer("Trigger");
            ScenarioBehaviour scb = g.AddMissingComponent<ScenarioBehaviour>();
            scb.TriggerParams = new Hashtable();
            g.transform.SetParent(Selection.gameObjects[0].transform, false);
        }
    }

    //[MenuItem("GameObject/Trigger/PlayAnimationBySample", false, 1)]
    //public static void PlayAnimationBySampleTrigger()
    //{

    //    string name = CreateName("PlayAnimationBySample");
    //    if (name != null)
    //    {
    //        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //        g.transform.localPosition = Vector3.zero;
    //        g.name = name;
    //        //-----------------------------------------------------//
    //        //添加碰撞体
    //        SphereCollider sphere = g.AddMissingComponent<SphereCollider>();
    //        sphere.isTrigger = true;
    //        //-----------------------------------------------------//
    //        g.layer = LayerMask.NameToLayer("Trigger");
    //        ScenarioBehaviour scb = g.AddMissingComponent<ScenarioBehaviour>();
    //        scb.triggerActions = new TriggerActionParams[1];
    //        scb.triggerActions[0] = new TriggerActionParams();
    //        //-----------------------------------------------------//
    //        //设置参数
    //        scb.triggerActions[0].action = TriggerActionParams.ActionType.PlayAnimationBySample;
    //        //-----------------------------------------------------//

    //        g.transform.SetParent(Selection.gameObjects[0].transform, false);
    //    }
    //}

    //[MenuItem("GameObject/Trigger/SampleAnimation", false, 1)]
    //public static void SampleAnimationTrigger()
    //{

    //    string name = CreateName("SampleAnimation");
    //    if (name != null)
    //    {
    //        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //        g.transform.localPosition = Vector3.zero;
    //        g.name = name;
    //        //-----------------------------------------------------//
    //        //添加碰撞体
    //        SphereCollider sphere = g.AddMissingComponent<SphereCollider>();
    //        sphere.isTrigger = true;
    //        //-----------------------------------------------------//
    //        g.layer = LayerMask.NameToLayer("Trigger");
    //        ScenarioBehaviour scb = g.AddMissingComponent<ScenarioBehaviour>();
    //        scb.triggerActions = new TriggerActionParams[1];
    //        scb.triggerActions[0] = new TriggerActionParams();
    //        //-----------------------------------------------------//
    //        //设置参数
    //        scb.triggerActions[0].action = TriggerActionParams.ActionType.SampleAnimation;
    //        //-----------------------------------------------------//

    //        g.transform.SetParent(Selection.gameObjects[0].transform, false);
    //    }
    //}

    //[MenuItem("GameObject/Trigger/ChangeSceneTrigger", false, 1)]
    //public static void SceneTrigger()
    //{

    //    string name = CreateName("ChangeSceneTrigger");
    //    if (name != null)
    //    {
    //        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //        g.transform.localPosition = Vector3.zero;
    //        g.name = name;
    //        //-----------------------------------------------------//
    //        //添加碰撞体
    //        SphereCollider sphere = g.AddMissingComponent<SphereCollider>();
    //        sphere.isTrigger = true;
    //        //-----------------------------------------------------//
    //        g.layer = LayerMask.NameToLayer("Trigger");
    //        ScenarioBehaviour scb = g.AddMissingComponent<ScenarioBehaviour>();
    //        scb.triggerActions = new TriggerActionParams[1];
    //        scb.triggerActions[0] = new TriggerActionParams();
    //        //-----------------------------------------------------//
    //        //设置参数
    //        scb.triggerActions[0].action = TriggerActionParams.ActionType.ChangeScene;
    //        //-----------------------------------------------------//

    //        g.transform.SetParent(Selection.gameObjects[0].transform, false);
    //    }
    //}

    //[MenuItem("GameObject/Trigger/MirrorTrigger", false, 1)]
    //public static void MirrorTrigger()
    //{

    //    string name = CreateName("MirrorTrigger");
    //    if (name != null)
    //    {
    //        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //        g.transform.localPosition = Vector3.zero;
    //        g.name = name;
    //        //-----------------------------------------------------//
    //        //添加碰撞体
    //        BoxCollider sphere = g.AddMissingComponent<BoxCollider>();
    //        sphere.isTrigger = true;
    //        sphere.size = new Vector3(1000, 1, 1000);
    //        //-----------------------------------------------------//
    //        g.layer = LayerMask.NameToLayer("Trigger");
    //        ScenarioBehaviour scb = g.AddMissingComponent<ScenarioBehaviour>();
    //        scb.triggerActions = new TriggerActionParams[1];
    //        scb.triggerActions[0] = new TriggerActionParams();
    //        //-----------------------------------------------------//
    //        //设置参数
    //        scb.triggerActions[0].action = TriggerActionParams.ActionType.MirrorTrigger;
    //        scb.triggerActions[0].mirrorPrefabName = "pro10102";
    //        //-----------------------------------------------------//

    //        g.transform.SetParent(Selection.gameObjects[0].transform, false);
    //    }
    //}

    //[MenuItem("GameObject/Trigger/SpecialCameraFollowTrigger", false, 1)]
    //public static void SpecialCameraFollowTrigger()
    //{

    //    string name = CreateName("SpecialCameraFollowTrigger");
    //    if (name != null)
    //    {
    //        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //        g.transform.localPosition = Vector3.zero;
    //        g.name = name;
    //        //-----------------------------------------------------//
    //        //添加碰撞体
    //        BoxCollider sphere = g.AddMissingComponent<BoxCollider>();
    //        sphere.isTrigger = true;
    //        sphere.size = new Vector3(1000, 1, 1000);
    //        //-----------------------------------------------------//
    //        g.layer = LayerMask.NameToLayer("Trigger");
    //        ScenarioBehaviour scb = g.AddMissingComponent<ScenarioBehaviour>();
    //        scb.triggerActions = new TriggerActionParams[1];
    //        scb.triggerActions[0] = new TriggerActionParams();
    //        //-----------------------------------------------------//
    //        //设置参数
    //        scb.triggerActions[0].action = TriggerActionParams.ActionType.MirrorTrigger;
    //        scb.triggerActions[0].SpecialCameraFollowIsLookAtPlayer = true;
    //        //-----------------------------------------------------//

    //        g.transform.SetParent(Selection.gameObjects[0].transform, false);
    //    }
    //}

    //[MenuItem("GameObject/Trigger/PlayMusic", false, 1)]
    //public static void PlayMusic()
    //{

    //    string name = CreateName("PlayMusic");
    //    if (name != null)
    //    {
    //        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //        g.transform.localPosition = Vector3.zero;
    //        g.name = name;
    //        //-----------------------------------------------------//
    //        //添加碰撞体

    //        //-----------------------------------------------------//
    //        g.layer = LayerMask.NameToLayer("Trigger");
    //        ScenarioBehaviour scb = g.AddMissingComponent<ScenarioBehaviour>();
    //        scb.triggerActions = new TriggerActionParams[1];
    //        scb.triggerActions[0] = new TriggerActionParams();
    //        //-----------------------------------------------------//
    //        //设置参数
    //        scb.triggerActions[0].action = TriggerActionParams.ActionType.PlayMusic;
    //        scb.triggerActions[0].SpecialCameraFollowIsLookAtPlayer = true;
    //        //-----------------------------------------------------//

    //        g.transform.SetParent(Selection.gameObjects[0].transform, false);
    //    }
    //}

    //[MenuItem("GameObject/Trigger/StopMusic", false, 1)]
    //public static void StopMusic()
    //{

    //    string name = CreateName("StopMusic");
    //    if (name != null)
    //    {
    //        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //        g.transform.localPosition = Vector3.zero;
    //        g.name = name;
    //        //-----------------------------------------------------//
    //        //添加碰撞体

    //        //-----------------------------------------------------//
    //        g.layer = LayerMask.NameToLayer("Trigger");
    //        ScenarioBehaviour scb = g.AddMissingComponent<ScenarioBehaviour>();
    //        scb.triggerActions = new TriggerActionParams[1];
    //        scb.triggerActions[0] = new TriggerActionParams();
    //        //-----------------------------------------------------//
    //        //设置参数
    //        scb.triggerActions[0].action = TriggerActionParams.ActionType.StopMusic;
    //        scb.triggerActions[0].SpecialCameraFollowIsLookAtPlayer = true;
    //        //-----------------------------------------------------//

    //        g.transform.SetParent(Selection.gameObjects[0].transform, false);
    //    }
    //}

    //[MenuItem("GameObject/Trigger/Play2DSound", false, 1)]
    //public static void Play2DSound()
    //{

    //    string name = CreateName("Play2DSound");
    //    if (name != null)
    //    {
    //        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //        g.transform.localPosition = Vector3.zero;
    //        g.name = name;
    //        //-----------------------------------------------------//
    //        //添加碰撞体

    //        //-----------------------------------------------------//
    //        g.layer = LayerMask.NameToLayer("Trigger");
    //        ScenarioBehaviour scb = g.AddMissingComponent<ScenarioBehaviour>();
    //        scb.triggerActions = new TriggerActionParams[1];
    //        scb.triggerActions[0] = new TriggerActionParams();
    //        //-----------------------------------------------------//
    //        //设置参数
    //        scb.triggerActions[0].action = TriggerActionParams.ActionType.Play2DSound;
    //        scb.triggerActions[0].SpecialCameraFollowIsLookAtPlayer = true;
    //        //-----------------------------------------------------//

    //        g.transform.SetParent(Selection.gameObjects[0].transform, false);
    //    }
    //}

    //[MenuItem("GameObject/Trigger/Stop2DSound", false, 1)]
    //public static void Stop2DSound()
    //{

    //    string name = CreateName("Stop2DSound");
    //    if (name != null)
    //    {
    //        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //        g.transform.localPosition = Vector3.zero;
    //        g.name = name;
    //        //-----------------------------------------------------//
    //        //添加碰撞体

    //        //-----------------------------------------------------//
    //        g.layer = LayerMask.NameToLayer("Trigger");
    //        ScenarioBehaviour scb = g.AddMissingComponent<ScenarioBehaviour>();
    //        scb.triggerActions = new TriggerActionParams[1];
    //        scb.triggerActions[0] = new TriggerActionParams();
    //        //-----------------------------------------------------//
    //        //设置参数
    //        scb.triggerActions[0].action = TriggerActionParams.ActionType.Stop2DSound;
    //        scb.triggerActions[0].SpecialCameraFollowIsLookAtPlayer = true;
    //        //-----------------------------------------------------//

    //        g.transform.SetParent(Selection.gameObjects[0].transform, false);
    //    }
    //}

    //[MenuItem("GameObject/Trigger/Play3DSound", false, 1)]
    //public static void Play3DSound()
    //{

    //    string name = CreateName("Play3DSound");
    //    if (name != null)
    //    {
    //        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //        g.transform.localPosition = Vector3.zero;
    //        g.name = name;
    //        //-----------------------------------------------------//
    //        //添加碰撞体

    //        //-----------------------------------------------------//
    //        g.layer = LayerMask.NameToLayer("Trigger");
    //        ScenarioBehaviour scb = g.AddMissingComponent<ScenarioBehaviour>();
    //        scb.triggerActions = new TriggerActionParams[1];
    //        scb.triggerActions[0] = new TriggerActionParams();
    //        //-----------------------------------------------------//
    //        //设置参数
    //        scb.triggerActions[0].action = TriggerActionParams.ActionType.Play3DSound;
    //        scb.triggerActions[0].SpecialCameraFollowIsLookAtPlayer = true;
    //        //-----------------------------------------------------//

    //        g.transform.SetParent(Selection.gameObjects[0].transform, false);
    //    }
    //}

    //[MenuItem("GameObject/Trigger/Stop3DSound", false, 1)]
    //public static void Stop3DSound()
    //{

    //    string name = CreateName("Stop3DSound");
    //    if (name != null)
    //    {
    //        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //        g.transform.localPosition = Vector3.zero;
    //        g.name = name;
    //        //-----------------------------------------------------//
    //        //添加碰撞体

    //        //-----------------------------------------------------//
    //        g.layer = LayerMask.NameToLayer("Trigger");
    //        ScenarioBehaviour scb = g.AddMissingComponent<ScenarioBehaviour>();
    //        scb.triggerActions = new TriggerActionParams[1];
    //        scb.triggerActions[0] = new TriggerActionParams();
    //        //-----------------------------------------------------//
    //        //设置参数
    //        scb.triggerActions[0].action = TriggerActionParams.ActionType.Stop3DSound;
    //        scb.triggerActions[0].SpecialCameraFollowIsLookAtPlayer = true;
    //        //-----------------------------------------------------//

    //        g.transform.SetParent(Selection.gameObjects[0].transform, false);
    //    }
    //}

    //[MenuItem("GameObject/Trigger/StopAll3DSound", false, 1)]
    //public static void StopAll3DSound()
    //{

    //    string name = CreateName("StopAll3DSound");
    //    if (name != null)
    //    {
    //        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //        g.transform.localPosition = Vector3.zero;
    //        g.name = name;
    //        //-----------------------------------------------------//
    //        //添加碰撞体

    //        //-----------------------------------------------------//
    //        g.layer = LayerMask.NameToLayer("Trigger");
    //        ScenarioBehaviour scb = g.AddMissingComponent<ScenarioBehaviour>();
    //        scb.triggerActions = new TriggerActionParams[1];
    //        scb.triggerActions[0] = new TriggerActionParams();
    //        //-----------------------------------------------------//
    //        //设置参数
    //        scb.triggerActions[0].action = TriggerActionParams.ActionType.StopAll3DSound;
    //        scb.triggerActions[0].SpecialCameraFollowIsLookAtPlayer = true;
    //        //-----------------------------------------------------//

    //        g.transform.SetParent(Selection.gameObjects[0].transform, false);
    //    }
    //}

    //[MenuItem("GameObject/Trigger/StopAllSound", false, 1)]
    //public static void StopAllSound()
    //{

    //    string name = CreateName("StopAllSound");
    //    if (name != null)
    //    {
    //        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //        g.transform.localPosition = Vector3.zero;
    //        g.name = name;
    //        //-----------------------------------------------------//
    //        //添加碰撞体

    //        //-----------------------------------------------------//
    //        g.layer = LayerMask.NameToLayer("Trigger");
    //        ScenarioBehaviour scb = g.AddMissingComponent<ScenarioBehaviour>();
    //        scb.triggerActions = new TriggerActionParams[1];
    //        scb.triggerActions[0] = new TriggerActionParams();
    //        //-----------------------------------------------------//
    //        //设置参数
    //        scb.triggerActions[0].action = TriggerActionParams.ActionType.StopAllSound;
    //        scb.triggerActions[0].SpecialCameraFollowIsLookAtPlayer = true;
    //        //-----------------------------------------------------//

    //        g.transform.SetParent(Selection.gameObjects[0].transform, false);
    //    }
    //}


    [MenuItem("GameObject/Trigger/生成Json", false, 1)]
    public static void CreateJson()
    {
        if (Selection.gameObjects.Length == 0)
            return;

        GameObject gameObject = Selection.gameObjects[0];
        Hashtable hashtable = new Hashtable();
        hashtable["position"] = $"{gameObject.transform.position.x}|{gameObject.transform.position.y}|{gameObject.transform.position.z}";
        if (gameObject.GetComponent<BoxCollider>() != null)
        {
            BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
            hashtable["collider"] = "Box Collider";
            hashtable["collider_size"] = $"{boxCollider.size.x}|{boxCollider.size.y}|{boxCollider.size.z}";
        }

        if (gameObject.GetComponent<SphereCollider>() != null)
        {
            SphereCollider sphereCollider = gameObject.GetComponent<SphereCollider>();
            hashtable["collider"] = "Sphere Collider";
            hashtable["collider_size"] = sphereCollider.radius.ToString();
        }

        ScenarioBehaviour scenarioBehaviour = gameObject.GetComponent<ScenarioBehaviour>();
        if (scenarioBehaviour != null && scenarioBehaviour.TriggerParams != null)
        {
            hashtable["actions"] = scenarioBehaviour.TriggerParams;
        }

        Hashtable triggers = new Hashtable();
        triggers[gameObject.name] = hashtable;
        string json = MiniJSON.JsonEncode(triggers);
        if (EditorUtility.DisplayDialog("结果", json, "复制", "No"))
        {
            TextEditor text2Editor = new TextEditor();
            text2Editor.text = json;
            text2Editor.OnFocus();
            text2Editor.Copy();
        }

    }

    private static string CreateName(string p_name)
    {
        if (Selection.gameObjects.Length > 0)
        {

            Debug.Log(Selection.gameObjects[0].transform.name);
            GameObject _selectGameObject = Selection.gameObjects[0];
            HashSet<string> _names = new HashSet<string>();

            foreach (Transform t in _selectGameObject.transform)
            {
                _names.Add(t.name);
                Debug.Log(t.name);
            }

            int i;
            for (i = 0; ; i++)
            {
                if (i == 0)
                {
                    if (!_names.Contains(p_name))
                    {
                        break;
                    }
                }
                else if (!_names.Contains(p_name + "_" + i))
                {
                    break;
                }
            }

            string _reName = p_name;
            if (i != 0)
            {
                _reName += "_" + i;
            }
            return _reName;
        }
        return null;
    }

    private static GameObject AddTraceTrigger(string p_name)
    {
        GameObject _gameObject = new GameObject();
        _gameObject.transform.localPosition = Vector3.zero;
        _gameObject.name = p_name;
        _gameObject.layer = LayerMask.NameToLayer("Trigger");

        GameObject _startTrigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _startTrigger.name = "StartTrigger";
        ScenarioBehaviour _startTriggerParems = _startTrigger.AddMissingComponent<ScenarioBehaviour>();

        _startTrigger.transform.SetParent(_gameObject.transform, false);
        BoxCollider box = _startTrigger.AddMissingComponent<BoxCollider>();
        box.isTrigger = true;

        GameObject _endTrigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _endTrigger.name = "EndTrigger";
        ScenarioBehaviour _endTriggerParems = _endTrigger.AddMissingComponent<ScenarioBehaviour>();

        _endTrigger.transform.SetParent(_gameObject.transform, false);
        box = _endTrigger.AddMissingComponent<BoxCollider>();
        box.isTrigger = true;

        return _gameObject;
    }
}
