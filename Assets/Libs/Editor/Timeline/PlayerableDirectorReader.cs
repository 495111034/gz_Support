using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using Cinemachine.Timeline;
using Cinemachine;
using Object = UnityEngine.Object;


static internal class PlayerableDirectorReader
{

    [MenuItem("GameObject/TimeLine/导出Timeline数据", false, 1)]
    static public void ExportSelectionTimeLine(MenuCommand menuCommand) 
    {
        GameObject parent = menuCommand.context as GameObject;
        _ExportSelectionTimeLine(parent);
    }

    static void LogError(string fileName, int i, PlayableBinding bind) 
    {
        var msg = $"{fileName} 第{i}条轨道{bind.streamName} 未绑定目标GameObject";
        var ret = UnityEditor.EditorUtility.DisplayDialog("错误", msg, "继续", "退出");
        if (!ret) 
        {
            throw new Exception(msg);
        }
        Log.LogError(msg);
    }

    static void _ExportSelectionTimeLine(GameObject parent)
    {        
        TimelineType timelineType;
        // fightSkillClip = null;
        if (parent.GetComponent<PlayableDirector>() != null)
        {
            var playableDirector = parent.GetComponent<PlayableDirector>();
            var timelineAsset = playableDirector.playableAsset;

            var assetFile = AssetDatabase.GetAssetPath(timelineAsset).ToLower();
            var fileName = System.IO.Path.GetFileName(assetFile);
            string dataFile = "";

            if (assetFile.StartsWith(PathDefs.ASSETS_PATH_CHARACTER))
            {
                //战斗timeline
                timelineType = TimelineType.FightSkill;
                dataFile = System.IO.Path.Combine(PathDefs.ASSETS_PATH_ASSETDATA + "skill/", System.IO.Path.GetFileNameWithoutExtension(fileName) + ".asset");
            }
            else if (assetFile.StartsWith(PathDefs.ASSETS_PATH_SCENE_ASSETS))
            {
                //剧情timeline
                timelineType = TimelineType.SceneScenario;
                dataFile = System.IO.Path.Combine(PathDefs.ASSETS_PATH_ASSETDATA + "scenario/", System.IO.Path.GetFileNameWithoutExtension(fileName) + ".asset");
            }
            else
            {
                UnityEditor.EditorUtility.DisplayDialog("错误", $"无法确定此{assetFile},timeline类型，角色目录下的为战斗技能类型，场景目录下的为场景剧情类型", "知道了");
                return;
            }
            AssetDatabase.DeleteAsset(dataFile);

            // if (System.IO.File.Exists(dataFile))
            //     AssetDatabase.DeleteAsset(dataFile);

            var data = ScriptableObject.CreateInstance<TimelineData>();
            var assetobject = AssetDatabase.LoadAssetAtPath<TimelineAsset>(assetFile);
            data.timeLineDataAsset = assetobject;
            data.timeLineDataAssetName = assetobject.name.ToLower();
            data.DataType = timelineType;


            int i = 0;
            List<ActorData> actorDataList = new List<ActorData>();
            List<FModData> fmodeDataList = new List<FModData>();
            foreach (PlayableBinding bind in timelineAsset.outputs)
            {
                i++;

                var bindObj = playableDirector.GetGenericBinding(bind.sourceObject);

                Debug.Log($"{i}:streamName={bind.streamName},sourceBindingType={bind.outputTargetType},sourceObject.gettype={(bind.sourceObject? bind.sourceObject.GetType().ToString():"NULL")},sourceObjectname={(bind.sourceObject?bind.sourceObject.name:"NULL")},bindObjName={(bindObj?bindObj.name:"") },bindobjType={(bindObj? bindObj.GetType().ToString():"NULL")}");

                if (bind.sourceObject is CinemachineTrack)
                {
                    if (!bindObj)
                    {
                        LogError(fileName, i, bind);
                        return;
                    }

                    var animationTrack = (CinemachineTrack)bind.sourceObject;
                    List<VCamData> vcamList = new List<VCamData>();

                    List<TimelineClip> clips = animationTrack.GetClips().ToList();
                    clips = clips.OrderBy(x => x.start).ToList();
                    foreach (var clip in clips)
                    {
                        VCamData vcamData = new VCamData();
                        CinemachineShot shot = (CinemachineShot)clip.asset;

                        bool isOK;
                        var vcam = playableDirector.GetReferenceValue(shot.VirtualCamera.exposedName, out isOK) as CinemachineVirtualCamera;
                        if (!isOK)
                        {
                            // Log.LogError($"{clip.displayName},{shot.VirtualCamera.exposedName},{clip.start} 为空");
                            //return;
                        }
                        else
                        {
                            //Log.LogError($"{clip.displayName},{vcam.name},{clip.start} 正常");
                            var vcamGameObject = vcam.gameObject;

                            //string vcamPrefabname = "";
                            if (PrefabUtility.GetPrefabType(vcamGameObject) != PrefabType.PrefabInstance)
                            {
                                string errMsg = $"{fileName} 第{i}条轨道:{bind.streamName},{bindObj.name} 绑定的VCam必须为prefab";
                                Debug.LogError(errMsg);
                                UnityEditor.EditorUtility.DisplayDialog("错误", errMsg, "知道了");
                                return;
                            }

                            try
                            {
                                UnityEngine.GameObject parentObject = PrefabUtility.GetCorrespondingObjectFromSource(vcamGameObject);
                                //vcamData.vCamAssetObject = parentObject;
                                var vcamPrefabname = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(parentObject)).ToLower();
                                //
                                vcamData.vCamAssetName = vcamPrefabname;
                                //vcamData.vCamPrefab = parentObject;
                                //vcamData.vCamPosition = $"{vcamGameObject.transform.position.x}|{vcamGameObject.transform.position.y}|{vcamGameObject.transform.position.z};{vcamGameObject.transform.rotation.eulerAngles.x}|{vcamGameObject.transform.rotation.eulerAngles.y}|{vcamGameObject.transform.rotation.eulerAngles.z}";
                            }
                            catch (System.Exception e)
                            {
                                Log.LogError($"{vcam.gameObject.name} error:{e.Message}");
                            }

                            vcamList.Add(vcamData);
                        }
                    }
                    data.vCamDatas = vcamList.ToArray();
                }
                else if (bind.sourceObject is FMODEventTrack)
                {
                    FMODEventTrack fmodTrack = bind.sourceObject as FMODEventTrack;

                    if (fmodTrack)
                    {
                        FModData fmodData = new FModData();
                        List<FModeClipData> fmodClips = new List<FModeClipData>();
                        fmodData.gameobjectName = bindObj ? bindObj.name : "";
                        if (bindObj is GameObject go && go.TryGetComponent<ObjectBehaviourBase>(out var bhv)) 
                        {
                            fmodData.gameobjectName += "|" + (int)(bhv.objectType) + "|" + bhv.SceneAoiId;
                        }
                        fmodData.targetType = fmodTrack.SoundTargetType;
                        foreach (var clip in fmodTrack.GetClips())
                        {
                            FModeClipData fmodClip = new FModeClipData();
                            var fmodAsset = clip.asset as FMODEventPlayable;
                            if (fmodAsset)
                            {
                                //FMODUnity.EventManager.Events
                                fmodClip.eventName = fmodAsset.eventName;
                                //fmodClip.stopMode = fmodAsset.stopType;
                            }
                            fmodClip.start = clip.start;
                            fmodClip.duration = clip.duration;
                            fmodClip.end = clip.end;
                            fmodClip.stopBgMusic = fmodAsset.StopGameBGMusic;
                            //fmodClip.parameters = fmodAsset.parameters;
                            fmodClips.Add(fmodClip);
                        }
                        fmodData.clips = fmodClips.ToArray();
                        fmodeDataList.Add(fmodData);
                    }
                }
                else if (bind.sourceObject is GPUAnimationTrack)
                {
                    if (!bindObj)
                    {
                        LogError(fileName, i, bind);
                        return;
                    }


                    if (bindObj is ObjectBehaviourBase)
                    {
                        ActorData actorData = new ActorData();
                        actorData.trackType = TimelineTrackType.MyAnim;
                        var objBase = (bindObj as ObjectBehaviourBase);
                        if (objBase)
                        {
                            //角色
                            if (timelineType == TimelineType.FightSkill)
                            {
                                if (objBase)
                                {
                                    if (objBase.objectType == ObjectType.ObjTypePet)
                                        actorData.actorType = TimelineActorType.ActorPet;
                                    else if (objBase.objectType == ObjectType.ObjTypePlayer || objBase.objectType == ObjectType.ObjTypeMonster)
                                        actorData.actorType = TimelineActorType.ActorRoot;
                                }
                            }
                            else
                            {
                                //剧情timeline
                                if (objBase.IsMainRole)
                                {
                                    actorData.actorType = TimelineActorType.CurrentMainRole;
                                    actorData.aoi_type = (int)ObjectType.ObjTypePlayer;
                                    actorData.ScenarioID = "";
                                }
                                else if (objBase.isBossShow)
                                {
                                    actorData.actorType = TimelineActorType.ExternalTarget;
                                    actorData.aoi_type = (int)ObjectType.ObjTypeMonster;
                                    actorData.isBossShow = true;
                                    actorData.ScenarioID = "";
                                }
                                else if (objBase.objectType == ObjectType.ObjTypePlayer)
                                {
                                    actorData.actorType = TimelineActorType.ExternalTarget;
                                    actorData.aoi_type = (int)ObjectType.ObjTypePlayer;
                                    actorData.ScenarioID = objBase.SceneAoiId;
                                }
                                else
                                {
                                    actorData.actorType = TimelineActorType.ActorRoot;
                                    actorData.aoi_type = (int)objBase.objectType;
                                    actorData.ScenarioID = objBase.SceneAoiId;
                                    actorData.profGenderKey = objBase.profGenderKey;
                                    if (!string.IsNullOrEmpty(actorData.ScenarioID))
                                    {
                                        //actorData.ScenarioID = objBase.GetAssetListByChild();
                                        actorData.ScenarioID = objBase.SceneAoiId;
                                        var trans = objBase.gameObject.transform;
                                        actorData.actorPos = $"{trans.position.x}|{trans.position.y}|{trans.position.z};{trans.rotation.eulerAngles.x}|{trans.rotation.eulerAngles.y}|{trans.rotation.eulerAngles.z}";
                                    }
                                }
                            }
                            actorData.clips = null;
                            //Log.LogError($"add gpu actor:{objBase.name}");
                            actorDataList.Add(actorData);
                            continue;
                        }
                        else
                        {
                            UnityEditor.EditorUtility.DisplayDialog("错误", $"{fileName} 第{i}条轨道,{bind.streamName} 剧情角色必须用ObjectBehaviourBase配置数据", "知道了");
                            Debug.LogError($"{bind.streamName} 剧情角色必须用ObjectBehaviourBase配置数据");
                            return;
                        }
                    }
                    else
                    {
                        UnityEditor.EditorUtility.DisplayDialog("错误", $"{fileName} 第{i}条轨道,{bind.streamName},{bindObj.name} bind非ObjectBehaviourBase:{bindObj.GetType()}", "知道了");
                        Debug.LogError($"{bind.streamName},{bindObj.name} bind非ObjectBehaviourBase:{bindObj.GetType()}");
                        return;
                    }
                }
                else if (bind.sourceObject is AnimationTrack)
                {
                    if (!bindObj)
                    {
                        LogError(fileName, i, bind);
                        return;
                    }

                    //此类型有两种控制对象，第一是施法者，用于控制施法者的位移和动作
                    //第二是施法者技能所产生的特效，特效需要动态加载，可能需要控制特效的位移和动作
                    if (bindObj is Animator)
                    {
                        var targetObj = (bindObj as Animator).gameObject;
                        ActorData actorData = new ActorData();
                        actorData.trackType = TimelineTrackType.UnityAnim;
                        string assetsPath = AssetDatabase.GetAssetPath(targetObj).ToLower();
                        if (string.IsNullOrEmpty(assetsPath))
                        {
                            UnityEngine.Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(targetObj);
                            if (parentObject)
                                assetsPath = AssetDatabase.GetAssetPath(parentObject).ToLower();
                        }

                        var objBase = targetObj.GetComponent<ObjectBehaviourBase>();
                        if (string.IsNullOrEmpty(assetsPath) && !objBase)
                        {
                            UnityEditor.EditorUtility.DisplayDialog("错误", $"{fileName} 无法确定{bind.streamName} 文件的来源或角色类型", "知道了");
                            return;
                        }

                        if (objBase || assetsPath.StartsWith(PathDefs.ASSETS_PATH_CHARACTER) || assetsPath.StartsWith(PathDefs.PREFAB_PATH_CHARACTER))
                        {
                            //角色
                            if (timelineType == TimelineType.FightSkill)
                            {
                                if (objBase)
                                {
                                    if (objBase.objectType == ObjectType.ObjTypePet)
                                        actorData.actorType = TimelineActorType.ActorPet;
                                    else if (objBase.objectType == ObjectType.ObjTypePlayer || objBase.objectType == ObjectType.ObjTypeMonster)
                                        actorData.actorType = TimelineActorType.ActorRoot;
                                }
                                else
                                {
                                    //if (assetsPath.Contains("entourage"))
                                    //{
                                    //    actorData.actorType = TimelineActorType.ActorPet;
                                    //}
                                    //else
                                    //{
                                        //技能timeline，角色只有当前施法者
                                        actorData.actorType = TimelineActorType.ActorRoot;
                                    //}
                                }
                            }
                            else
                            {
                                //剧情timeline
                                if (!objBase)
                                {
                                    UnityEditor.EditorUtility.DisplayDialog("错误", $"{fileName} {bind.streamName} 剧情角色必须用ObjectBehaviourBase配置数据", "知道了");
                                    Debug.LogError($"{bind.streamName} 剧情角色必须用ObjectBehaviourBase配置数据");
                                    return;
                                }

                                //剧情timeline
                                if (objBase.IsMainRole)
                                {
                                    actorData.actorType = TimelineActorType.CurrentMainRole;
                                    actorData.aoi_type = (int)ObjectType.ObjTypePlayer;
                                    actorData.ScenarioID = "";
                                }
                                else if (objBase.isBossShow)
                                {
                                    actorData.actorType = TimelineActorType.ExternalTarget;
                                    actorData.aoi_type = (int)ObjectType.ObjTypeMonster;
                                    actorData.isBossShow = true;
                                    actorData.ScenarioID = "";
                                }
                                else if (objBase.objectType == ObjectType.ObjTypePlayer)
                                {
                                    actorData.actorType = TimelineActorType.ExternalTarget;
                                    actorData.aoi_type = (int)ObjectType.ObjTypePlayer;
                                    actorData.ScenarioID = objBase.SceneAoiId;
                                    if (string.IsNullOrEmpty(actorData.ScenarioID))
                                    {
                                        Log.LogError($"{objBase.GetAssetListByChild()}");
                                        actorData.ScenarioID = objBase.GetAssetListByChild();
                                    }
                                    var trans = targetObj.transform;
                                    actorData.actorPos = $"{trans.position.x}|{trans.position.y}|{trans.position.z};{trans.rotation.eulerAngles.x}|{trans.rotation.eulerAngles.y}|{trans.rotation.eulerAngles.z}";
                                }
                                else
                                {
                                    actorData.actorType = TimelineActorType.ActorRoot;
                                    actorData.aoi_type = (int)objBase.objectType;
                                    actorData.ScenarioID = objBase.SceneAoiId;
                                    actorData.profGenderKey = objBase.profGenderKey;
                                    if (string.IsNullOrEmpty(actorData.ScenarioID))
                                    {
                                        actorData.ScenarioID = objBase.GetAssetListByChild();
                                    }
                                    var trans = targetObj.transform;
                                    actorData.actorPos = $"{trans.position.x}|{trans.position.y}|{trans.position.z};{trans.rotation.eulerAngles.x}|{trans.rotation.eulerAngles.y}|{trans.rotation.eulerAngles.z}";
                                }
                            }

                            if (!objBase)
                            {
                                List<ActorAnimationClip> clips = new List<ActorAnimationClip>();
                                var animationTrack = (AnimationTrack)bind.sourceObject;
                                foreach (var clip in animationTrack.GetClips())
                                {
                                    if (!clip.animationClip)
                                    {
                                        UnityEditor.EditorUtility.DisplayDialog("错误", $"{fileName} {bind.streamName} 绑定的动画为空",
                                            "知道了");
                                        Debug.LogError($"{bind.streamName} 绑定的动画为空");
                                        return;
                                    }

                                    ActorAnimationClip c = new ActorAnimationClip();
                                    c.start = clip.start;
                                    c.duration = clip.duration;
                                    c.end = clip.end;
                                    c.animationClipName = clip.animationClip.name;
                                    c.clipIn = clip.clipIn;

                                    clips.Add(c);

                                    Log.LogError($"name:{clip.animationClip.name},start:{clip.start},clipname:{c.animationClipName}");
                                }

                                actorData.clips = clips.ToArray();
                            }
                            else
                                actorData.clips = null;

                            actorDataList.Add(actorData);
                            continue;
                        }
                        else if (assetsPath.StartsWith(PathDefs.PREFAB_PATH_COMPLEX_OBJECT))
                        {
                            //特效
                            actorData.actorType = TimelineActorType.EffectRoot;
                            var trans = (bindObj as GameObject).transform;
                            actorData.actorPos = $"{trans.position.x}|{trans.position.y}|{trans.position.z};{trans.rotation.eulerAngles.x}|{trans.rotation.eulerAngles.y}|{trans.rotation.eulerAngles.z}";

                            string effectPrefab = "";
                            if (PrefabUtility.GetPrefabType(bindObj) == PrefabType.PrefabInstance)
                            {
                                UnityEngine.Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(bindObj);
                                effectPrefab = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(parentObject)).ToLower();
                            }
                            else
                            {
                                string errMsg = $"{fileName} {bind.streamName},{bindObj.name} 绑定的特效必须为prefab";
                                Debug.LogError(errMsg);
                                UnityEditor.EditorUtility.DisplayDialog("错误", errMsg, "知道了");
                                return;
                            }
                            actorData.actorName = effectPrefab;
                            actorDataList.Add(actorData);
                            continue;
                        }
                        else if (assetsPath.StartsWith(PathDefs.PREFAB_PATH_GUI_PANEL))
                        {
                            //ui面版
                            actorData.actorType = TimelineActorType.UIPanelRoot;

                            string effectPrefab = "";
                            if (PrefabUtility.GetPrefabType(bindObj) == PrefabType.PrefabInstance)
                            {
                                UnityEngine.Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(bindObj);
                                effectPrefab = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(parentObject)).ToLower();
                            }
                            else
                            {
                                string errMsg = $"{fileName} {bind.streamName},{bindObj.name} 绑定的UI面版必须为prefab";
                                Debug.LogError(errMsg);
                                UnityEditor.EditorUtility.DisplayDialog("错误", errMsg, "知道了");
                                return;
                            }
                            actorData.actorName = effectPrefab;
                            actorDataList.Add(actorData);
                            continue;
                        }


                        {
                            string errMsg = $"{fileName} {bind.streamName} 绑定了未知类型的物体{(bindObj as GameObject).name},{assetsPath}";
                            Debug.LogError(errMsg);
                            UnityEditor.EditorUtility.DisplayDialog("错误", errMsg, "知道了");
                            return;
                        }
                    }

                }
                else if (bind.sourceObject is ActivationTrack)
                {
                    //用于特效的显示或隐藏                    
                    if (!bindObj)
                    {
                        LogError(fileName, i, bind);
                        return;
                    }


                    string assetsPath = AssetDatabase.GetAssetPath(bindObj).ToLower();
                    if (string.IsNullOrEmpty(assetsPath))
                    {
                        UnityEngine.Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(bindObj);
                        if (parentObject)
                            assetsPath = AssetDatabase.GetAssetPath(parentObject).ToLower();
                    }

                    //var objBase = (bindObj as GameObject).GetComponent<EffectBehaviour>();
                    if (string.IsNullOrEmpty(assetsPath) || !assetsPath.StartsWith(PathDefs.PREFAB_PATH_COMPLEX_OBJECT))
                    {
                        UnityEditor.EditorUtility.DisplayDialog("错误", $"{fileName} 第{i}条轨道,{bind.streamName}的目标物体必须是来自{PathDefs.PREFAB_PATH_COMPLEX_OBJECT}的预制体,{bindObj.name}不符合要求", "知道了");
                        return;
                    }
                    ActorData actorData = new ActorData();
                    actorData.trackType = TimelineTrackType.Activation;

                    actorData.actorType = TimelineActorType.EffectRoot;
                    var effectPrefab = System.IO.Path.GetFileNameWithoutExtension(assetsPath).ToLower();
                    actorData.actorName = effectPrefab;

                    actorDataList.Add(actorData);

                }
                else if (bind.sourceObject is ParticleSystemControlTrack)
                {
                    //粒子系统控制器                   

                    if (!bindObj)
                    {
                        LogError(fileName, i, bind);
                        return;
                    }
                    string assetsPath = AssetDatabase.GetAssetPath((bindObj as EffectBehaviour).gameObject).ToLower();
                    if (string.IsNullOrEmpty(assetsPath))
                    {
                        UnityEngine.Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource((bindObj as EffectBehaviour).gameObject);
                        if (parentObject)
                            assetsPath = AssetDatabase.GetAssetPath(parentObject).ToLower();
                    }

                    if (string.IsNullOrEmpty(assetsPath) || !assetsPath.StartsWith(PathDefs.PREFAB_PATH_COMPLEX_OBJECT))
                    {
                        UnityEditor.EditorUtility.DisplayDialog("错误", $"{fileName} 第{i}条轨道,{bind.streamName}的目标物体必须来自{PathDefs.PREFAB_PATH_COMPLEX_OBJECT}的预制体,{(bindObj as EffectBehaviour).gameObject.name}不符合要求", "知道了");
                        return;
                    }

                    ActorData actorData = new ActorData();
                    actorData.trackType = TimelineTrackType.Particle;
                    actorData.actorType = TimelineActorType.EffectRoot;
                    actorData.isIndependent = (bind.sourceObject as ParticleSystemControlTrack).Independent;
                    actorData.isLowOverhead = (bindObj as EffectBehaviour).IsLowOverhead;
                    var effectPrefab = System.IO.Path.GetFileNameWithoutExtension(assetsPath).ToLower();
                    actorData.actorName = effectPrefab;


                    actorDataList.Add(actorData);
                }
                
            }

            data.fModDatas = fmodeDataList.ToArray();
            data.actorDatas = actorDataList.ToArray();

            Debug.Log($"save {data.timeLineDataAssetName} -> {dataFile}");
            AssetDatabase.CreateAsset(data, dataFile);

        }

    }


    static bool CheckTimelineData(TimelineData data)
    {
        if(data.DataType == TimelineType.FightSkill)
        {
            if(data.actorDatas.Length == 0)
            {
                UnityEditor.EditorUtility.DisplayDialog("错误", "技能timeline数据至少需要一个施法者", "知道了");
                return false;
            }
            int actorCount = 0, petCount = 0;
            for(int i = 0; i < data.actorDatas.Length; ++i)
            {
                var a = data.actorDatas[i];
                if (a.actorType == TimelineActorType.ActorRoot)
                    actorCount += 1;
                else if (a.actorType == TimelineActorType.ActorPet)
                    petCount += 1;
                else
                {
                    UnityEditor.EditorUtility.DisplayDialog("错误", "技能timeline施法者只能是ActorRoot或ActorPet", "知道了");
                    return false;
                }
            }
            if (actorCount != 1)
            {
                UnityEditor.EditorUtility.DisplayDialog("错误", "技能timeline数据至少需要一个(只能有一个)施法者", "知道了");
                return false;
            }
            if(petCount > 1)
            {
                UnityEditor.EditorUtility.DisplayDialog("错误", "技能timeline数据只能有一个宠物", "知道了");
                return false;
            }

            return true;

        }

        return true;
    }


    [MenuItem("Export/导出所有Timeline数据")]
    static void ExportTimeLines() 
    {
        var scenes = System.IO.Directory.GetFiles("Assets\\SKILL","*.unity", System.IO.SearchOption.AllDirectories);
        foreach (var scene in scenes) 
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene);
            var dir = GameObject.FindObjectOfType<UnityEngine.Playables.PlayableDirector>();
            if (dir)
            {
                _ExportSelectionTimeLine(dir.gameObject);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

