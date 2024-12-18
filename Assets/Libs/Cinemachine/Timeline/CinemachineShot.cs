using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Cinemachine.Timeline
{
    internal sealed class CinemachineShotPlayable : PlayableBehaviour
    {
        public CinemachineVirtualCameraBase VirtualCamera;


        //[System.NonSerialized]
        //Vector3 campos;
        //[System.NonSerialized]
        //Transform cam;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);

            var graph = playable.GetGraph();
            var director = graph.GetResolver() as PlayableDirector;
            if (director != null && Application.isPlaying)
            {
                //director.playableGraph.getou
                var cama = VirtualCamera as CinemachineVirtualCamera;
                if (cama && cama.m_CamObjectTrackId > 0)
                {
                    int i = 0;
                    foreach (var output in director.playableAsset.outputs)
                    {
                        if (++i == cama.m_CamObjectTrackId)
                        {
                            var obj = director.GetGenericBinding(output.sourceObject);
                            var actor = obj as GameObject;
                            if (!actor)
                            {
                                var com = obj as Component;
                                if (com)
                                {
                                    actor = com.gameObject;
                                }
                            }
                            //Log.Log2File($"{obj}");
                            if (actor)
                            {
                                var child = actor.FindChild(cama.m_CamObjectNodeName);
                                if (child)
                                {
                                    cama.transform.SetParent(child.transform);
                                    cama.transform.localPosition = Vector3.zero;
                                    var localRotation = new Quaternion();
                                    localRotation.eulerAngles = new Vector3(0,180,0);
                                    cama.transform.localRotation = localRotation;
                                    //cam = child.transform;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            //Debug.LogError($"{transform.position} {gameObject.GetLocation()}");
        }


        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            base.OnBehaviourPause(playable, info);
            if (Application.isPlaying)
            {
                var cama = VirtualCamera as CinemachineVirtualCamera;
                if (cama && cama.m_CamObjectTrackId > 0)
                {
                    cama.transform.parent = null;
                    cama.transform.position = Vector3.zero;
                    cama.transform.rotation = Quaternion.identity;
                }
            }
        }
    }

    public sealed class CinemachineShot : PlayableAsset, IPropertyPreview
    {
        public ExposedReference<CinemachineVirtualCameraBase> VirtualCamera;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<CinemachineShotPlayable>.Create(graph);
            playable.GetBehaviour().VirtualCamera = VirtualCamera.Resolve(graph.GetResolver());
            return playable;
        }

        // IPropertyPreview implementation
        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            driver.AddFromName<Transform>("m_LocalPosition.x");
            driver.AddFromName<Transform>("m_LocalPosition.y");
            driver.AddFromName<Transform>("m_LocalPosition.z");
            driver.AddFromName<Transform>("m_LocalRotation.x");
            driver.AddFromName<Transform>("m_LocalRotation.y");
            driver.AddFromName<Transform>("m_LocalRotation.z");

            driver.AddFromName<Camera>("field of view");
            driver.AddFromName<Camera>("near clip plane");
            driver.AddFromName<Camera>("far clip plane");
        }
    }
}
