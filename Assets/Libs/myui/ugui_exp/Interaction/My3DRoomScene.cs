using System;
using System.Collections;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(EmptyImage))]
    [AddComponentMenu("UI/My3DRoomScene", 5)]
    public class My3DRoomScene : MonoBehaviour, IDragHandler
    {
        public static GameObject root_obj { set; get; }

        [Header("3D场景环境资源")]
        public string scene_model_res = "";

        [Header("模型站位节点名字")]
        public string model_node = "";

        [Header("预放置模型资源名")]
        public string resName = "";

        [Header("复制到主摄像机使用")]
        public bool use_main_camera = false;
        
        public BundlType uiroomType = BundlType.singleObj;
        public UIRoomCharacterType characterType = UIRoomCharacterType.Role;
        public DropEventType dropType = DropEventType.None;

        private GameObject scene_model;
        private Transform target_node;
        private Transform model;
        private Camera scene_camera;
        private Camera main_camera;
        //private Transform main_parent;
        private float modelScale = 1;
        private Vector3 modelRotation;
        private Vector3 modelOffset;
        [NonSerialized] public Vector3 extraModelOffset = Vector3.zero;
        private Vector3 safeOffset;

        private Vector3 cameraOffset = Vector3.positiveInfinity;
        private Vector3 cameraRotation = Vector3.positiveInfinity;
        private float cameraFOV;
        //private float fieldOfView;
        //private float near;
        //private float far;
        //private float camera_depth;
        //private int cull_mask = 0;

        private bool is_camera_rect_model; //使用设置相机显示矩形为当前RectTransform矩形
        private BundleRequestInfo param_scene = null;
        private BundleRequestInfo param_model = null;

        private Tuple<string, Action<object[]>, object[]> CacheWaitPlayAnim;
        private Coroutine AnimCoroute;

        [NonSerialized]
        public bool HideSceneAfterLoad = false;
        public Camera MainCamera => main_camera;
        public Transform Model => model;
        public Transform TargetNode => target_node;
        public GameObject SceneModel => scene_model;

        [NonSerialized] private RenderTexture _renderTexture;

        public int Career { set; get; }
        public bool IsReplaceModel { set; get; }

        //private SceneFogManager sceneFogManager;

        private Light directional = null;
        private static Rendering.PostProcessing.PostProcessProfile UICameraProfile;

        private void Start()
        {
            if (directional == null)
            {
                directional = gameObject.GetComponent<Light>();
                if (directional == null)
                {
                    directional = gameObject.AddComponent<Light>();
                    directional.type = LightType.Directional;
                    directional.color = Color.black;
                    directional.renderMode = LightRenderMode.Auto;
                }
            }

            LoadSceneModelRes(scene_model_res);
            if (!string.IsNullOrEmpty(resName))
            {
                LoadModelRes(resName);
            }
        }

        private void OnEnable()
        {
            if (scene_model != null)
            {
                scene_model.SetActive(true);
            }
            if (model != null)
            {
                model.gameObject.SetActive(true);
            }
            SetMainCameraPamas();
            Canvas canvas = transform.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.worldCamera != null)
            {
                SetCameraDepth((int)canvas.worldCamera.depth - 1);
            }
        }

        private void OnDisable()
        {
            RecoveryFog();
            RecoveryMainCamera();
            
            if (scene_model != null)
            {
                scene_model.SetActive(false);
            }
            if (model != null)
            {
                model.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            RecoveryFog();
            RecoveryMainCamera();
            if (_renderTexture != null)
            {
                _renderTexture.Release();
            }
            if (scene_model != null)
            {
                GameObject.Destroy(scene_model);
            }
            if (model != null)
            {
                GameObject.Destroy(model.gameObject);
            }
            scene_model = null;
            model = null;
            param_scene = null;
            param_model = null;
        }

        private void RecoveryFog()
        {
            //if(sceneFogManager != null)
            //{
            //    if (GetCurrentRoomSceneCount() <= 1)
            //    {
            //        sceneFogManager.enabled = true;
            //    }
            //}
        }

        private int GetCurrentRoomSceneCount()
        {
            int count = 0;
            if (root_obj != null)
            {
                count = root_obj.transform.childCount;
            }
            if (My3DRoomCamera.root_obj != null)
            {
                count += My3DRoomCamera.root_obj.transform.childCount;
            }
            return count;
        }

        private void RecoveryMainCamera()
        {
            if (use_main_camera && main_camera != null)
            {
                //main_camera.transform.SetParent(main_parent);
                //main_camera.orthographic = false;
                //main_camera.fieldOfView = fieldOfView;
                //main_camera.nearClipPlane = near;
                //main_camera.farClipPlane = far;
                //main_camera.depth = camera_depth;
                //main_camera.cullingMask = cull_mask;
                //main_camera.transform.localPosition = Vector3.zero;
                //main_camera.transform.localEulerAngles = Vector3.zero;
                //iTween.Stop(main_camera.gameObject);

                GameObject.Destroy(main_camera.gameObject);

                main_camera = null;
                //main_parent = null;
            }
        }

        public void SetRenderTexture(RenderTexture renderTexture)
        {
            this._renderTexture = renderTexture;
            if (use_main_camera && main_camera != null)
            {
                if (_renderTexture != null && _renderTexture.IsCreated())
                {
                    main_camera.targetTexture = _renderTexture;
                }
            }
        }

        public void SetMainCameraPamas()
        {
            GameObject cameraObj = null;
            if (Camera.main == null)
            {
                cameraObj = GameObject.Find("Main Camera");
            }
            else
            {
                cameraObj = Camera.main.gameObject;
            }
            if (use_main_camera && scene_camera != null)
            {
                if (main_camera == null)
                {
                    if (cameraObj == null)
                    {
                        main_camera = new GameObject("main_camera").AddMissingComponent<Camera>();
                    }
                    else
                    {
                        main_camera = GameObject.Instantiate(cameraObj).GetComponent<Camera>();
                    }
                    AudioListener listener = main_camera.gameObject.GetComponent<AudioListener>();
                    if (listener != null)
                    {
                        Object.Destroy(listener);
                    }
                    var processLayer = main_camera.gameObject.GetComponent<Rendering.PostProcessing.PostProcessLayer>();
                    if (processLayer != null)
                    {
                        processLayer.enabled = !QualityUtils.IsBadGPU;
                        //Object.Destroy(processLayer);
                    }
                    var processVolume = main_camera.gameObject.GetComponent<Rendering.PostProcessing.PostProcessVolume>();
                    if (processVolume != null)
                    {
                        processVolume.enabled = !QualityUtils.IsBadGPU;
                        if (processVolume.enabled)
                        {
                            if (UICameraProfile == null)
                            {
                                UICameraProfile = Resources.Load<Object>("UICameraProfile") as Rendering.PostProcessing.PostProcessProfile;
                            }
                            if (UICameraProfile != null)
                            {
                                processVolume.profile = UICameraProfile;
                            }
                        }
                        //Object.Destroy(processVolume);
                    }
                    main_camera.targetTexture = null;
                    if (_renderTexture != null && _renderTexture.IsCreated())
                    {
                        main_camera.targetTexture = _renderTexture;
                    }
                    else
                    {
                        main_camera.targetTexture = null;
                    }
                    //fieldOfView = main_camera.fieldOfView;
                    //near = main_camera.nearClipPlane;
                    //far = main_camera.farClipPlane;
                    //camera_depth = main_camera.depth;
                    //cull_mask = main_camera.cullingMask;
                    //main_parent = main_camera.transform.parent;
                }
                main_camera.enabled = true;
                main_camera.orthographic = scene_camera.orthographic;
                if (scene_camera.orthographic)
                {
                    main_camera.orthographicSize = scene_camera.orthographicSize;
                }
                else
                {
                    main_camera.fieldOfView = scene_camera.fieldOfView;
                }
                main_camera.nearClipPlane = scene_camera.nearClipPlane;
                main_camera.farClipPlane = scene_camera.farClipPlane;
                // 忽略 BackGround
                main_camera.cullingMask = scene_camera.cullingMask & (int)~(ObjLayerMask.BackGround);

                main_camera.transform.SetParent(scene_camera.transform);
                main_camera.transform.localPosition = Vector3.zero;
                main_camera.transform.localEulerAngles = Vector3.zero;
            }
        }

        public void LoadSceneModelRes(string res_name)
        {
            if (scene_model_res == res_name && scene_model != null)
            {
                return;
            }
            if (scene_model != null)
            {
                Destroy(scene_model);
                scene_model = null;
            }
            if (root_obj == null)
            {
                root_obj = GameObject.Find("My3DRoomScene");
                if (root_obj == null)
                {
                    root_obj = new GameObject("My3DRoomScene");
                    Object.DontDestroyOnLoad(root_obj);
                }
            }
            scene_model_res = res_name;
            if (!string.IsNullOrEmpty(scene_model_res))
            {
                if (param_scene == null) param_scene = new BundleRequestInfo();
                param_scene.career = Career;
                param_scene.res_name = scene_model_res;
                param_scene.res_type = BundlType.prefabObj;
                param_scene.character_type = UIRoomCharacterType.Dragon;
                param_scene.requester = this;
                param_scene.callBack = (go) =>
                {
                    if (!this || !gameObject || !gameObject.activeInHierarchy) return;
                    if (go.res_name == scene_model_res && go.responseObject)
                    {
                        SetSceneModelObj(go.responseObject);
                    }
                };

                SendMessageUpwards("RequestBundle", param_scene);
            }
        }

        public void SetSceneModelObj(GameObject go)
        {
            scene_model = go;
            if (scene_model != null)
            {
                // 将 Background 层改为 Default
                scene_model.SetLayerRecursively((int)ObjLayer.Default, 1 << (int)ObjLayer.BackGround);
            }
            Object.DontDestroyOnLoad(scene_model);
            scene_model.transform.parent = root_obj.transform;

            float minPosY = 0;
            for (int i = 0; i < root_obj.transform.childCount; i++)
            {
                float posY = root_obj.transform.GetChild(i).position.y;
                if (posY < minPosY) minPosY = posY;
            }
            scene_model.transform.position = new Vector3(-1999, minPosY - 1000, -1999);

            if (!string.IsNullOrEmpty(model_node))
            {
                target_node = scene_model.FindChild(model_node).transform;
            }

            //if (sceneFogManager == null)
            //{
            //    sceneFogManager = FindObjectOfType<SceneFogManager>();

            //}
            //if (sceneFogManager != null)
            //{
            //    sceneFogManager.enabled = false;
            //}

            var l = scene_model.transform.Find("L");
            var h = scene_model.transform.Find("H");
            l?.gameObject.SetActiveX(QualityUtils.IsBadGPU);
            h?.gameObject.SetActiveX(!QualityUtils.IsBadGPU);

            scene_camera = scene_model.GetComponentInChildren<Camera>();
            scene_camera.enabled = false;
            SetMainCameraPamas();
            if (is_camera_rect_model)
            {
                SetCameraToRectTransRect();
            }
            else if (target_node != null)
            {
                Canvas canvas = transform.GetComponentInParent<Canvas>();
                if (canvas != null && canvas.worldCamera != null)
                {
                    safeOffset.x = -canvas.worldCamera.rect.x * target_node.position.z;
                    SetCameraDepth((int)canvas.worldCamera.depth - 1);
                }
            }
            SetPamas();
            if (HideSceneAfterLoad) scene_model.SetActiveX(false);
        }

        public void LoadModelRes(string res_name)
        {
            if (resName == res_name && model != null)
            {
                return;
            }
            resName = res_name;
            if (model != null && !IsReplaceModel)
            {
                GameObject.Destroy(model.gameObject);
                model = null;
            }
            if (!string.IsNullOrEmpty(resName))
            {
                if (param_model == null) param_model = new BundleRequestInfo();
                param_model.career = Career;
                param_model.res_name = resName;
                param_model.res_type = uiroomType;
                param_model.character_type = characterType;
                param_model.requester = gameObject.GetComponent<EmptyImage>();
                param_model.callBack = (go) =>
                {
                    if (!this || !gameObject || !gameObject.activeInHierarchy) return;
                    if (go.res_name == resName && go.responseObject)
                    {
                        model = go.responseObject.transform;
                        if (model) model.position = Vector3.down * 2000; //预放置模型刚出来时会被看到,所以放在下面
                        Object.DontDestroyOnLoad(model);
                        model.gameObject.SetActive(true);
                        SetPamas();
                        OnLoadModelResFinished();
                    }
                };

                // 重新获取模型前，清掉旧的待播动画
                CacheWaitPlayAnim = null;
                StopAnimCoroutine();

                SendMessageUpwards("RequestBundle", param_model);
            }
            else
            {
                if (model != null) model.gameObject.SetActive(false);
            }
        }

        private void OnLoadModelResFinished()
        {
            if (CacheWaitPlayAnim != null)
            {
                PlayModelAnimate(CacheWaitPlayAnim.Item1, CacheWaitPlayAnim.Item2, CacheWaitPlayAnim.Item3);
            }
        }

        public void SetCameraDepth(float depth)
        {
            if (scene_camera != null)
            {
                scene_camera.depth = depth;
            }
            if (use_main_camera && main_camera != null)
            {
                main_camera.depth = depth;
            }
        }

        /// <summary>
        /// 格式：x|y|z|旋转|缩放 或 x,y,z,旋转,缩放
        /// </summary>
        /// <param name="infos"></param>
        public void SetCameraParams(float[] infos)
        {
            Vector3 offset = Vector3.zero;
            int len = infos.Length;
            if (len > 0) offset.x = infos[0];
            if (len > 1) offset.y = infos[1];
            if (len > 2) offset.z = infos[2];
            if (len > 3) this.modelRotation = Vector3.up * infos[3];
            if (len > 4) this.modelScale = infos[4];// 1 + (1 - infos[4]);
            this.modelOffset = offset;

            SetPamas();
        }

        public void SetCameraParams2(Vector3 offset, Vector3 rotation, float fov)
        {
            if (offset != null) cameraOffset = offset;
            if (rotation != null) cameraRotation = rotation;
            if (fov > 0) cameraFOV = fov;
            SetPamas();
        }

        public void SetPamas()
        {
            if (model != null && target_node != null)
            {
                model.localEulerAngles = modelRotation;
                model.position = target_node.position + modelOffset + safeOffset + extraModelOffset;
                if (modelScale == 0) modelScale = 1;
                model.localScale = Vector3.one * modelScale;
            }

            if (main_camera != null)
            {
                if (cameraOffset != null && !Vector3.positiveInfinity.Equals(cameraOffset))
                {
                    main_camera.transform.localPosition = cameraOffset;
                }
                if (cameraRotation != null && !Vector3.positiveInfinity.Equals(cameraRotation))
                {
                    main_camera.transform.localRotation = Quaternion.Euler(cameraRotation);
                }
                if (cameraFOV > 0) main_camera.fieldOfView = cameraFOV;
                if (!is_camera_rect_model)
                {
                    if (param_scene != null)
                    {
                        main_camera.rect = param_scene.camera_rect;
                    }
                    else if (param_model != null)
                    {
                        main_camera.rect = param_model.camera_rect;
                    }
                }
            }
        }

        public void SetCameraToRectTransRect()
        {
            is_camera_rect_model = true;
            if (main_camera != null)
            {
                main_camera.enabled = true;
                Canvas canvas = transform.GetComponentInParent<Canvas>();
                if (canvas != null && canvas.worldCamera != null)
                {
                    SetCameraDepth((int)canvas.worldCamera.depth + 1);
                }
                BundleRequestInfo info = null;
                if (param_scene != null)
                {
                    info = param_scene;
                }
                else if (param_model != null)
                {
                    info = param_model;
                }
                main_camera.rect = My3DRoomCamera.Convert(transform as RectTransform, canvas.worldCamera, info);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dropType == DropEventType.None)
                return;

            var target = model;
            if (target)
            {
                var rat = target.transform.localRotation.eulerAngles;

                var rats_y = rat.y;
                var rats_z = rat.z;
                if (dropType == DropEventType.HorizontalRotation || dropType == DropEventType.FreeRotation)
                    rats_y -= ((eventData.delta).x / Screen.width) * 360f;
                if (dropType == DropEventType.VerticalRotation || dropType == DropEventType.FreeRotation)
                    rats_z += ((eventData.delta).y / Screen.height) * 360f;

                target.transform.localRotation = Quaternion.Euler(new Vector3(rat.x, rats_y, rats_z));
            }
        }

        object waiting_ani;
        public static Action<Animator, string, Action<AnimationClip, object>> LoadAssetBundle;
        #region -- 模型动画相关
        /// <summary>
        /// 播放模型动画
        /// 补充：也可以考虑 GetModel，在外部去执行动画操作
        /// </summary>
        public void PlayModelAnimate(string animName, Action<object[]> callback = null, object[] callbackParam = null)
        {
            if (model == null)
            {
                CacheWaitPlayAnim = Tuple.Create(animName, callback, callbackParam);
                return;
            }

            CacheWaitPlayAnim = null;
            StopAnimCoroutine();
            var animators = model.GetComponentsInChildren<Animator>();
            if (animators == null || animators.Length == 0)
            {
                callback?.Invoke(callbackParam);
                return;
            }
            float animLength = GetAnimClipLength(animators[0], animName);
            if (animLength > 0 || LoadAssetBundle != null)
            {
                if (animLength > 0 )
                {
                    _PlayModelAnimate(animators, animName, animLength, callback, callbackParam);
                }
                else 
                {
                    LoadAssetBundle(animators[0], animName, (AnimationClip clip, object dept) => 
                    {
                        if (clip)
                        {
                            waiting_ani = dept;
                            _PlayModelAnimate(animators, animName, clip.length, callback, callbackParam);
                        }
                        else 
                        {
                            // 没找到动画片段，直接回调
                            callback?.Invoke(callbackParam);
                        }
                    });
                }
            }
            else 
            {
                // 没找到动画片段，直接回调
                callback?.Invoke(callbackParam);
            }
        }

        void _PlayModelAnimate(Animator[] animators, string animName, float animLength, Action<object[]> callback, object[] callbackParam)
        {
            for (int i = 0; i < animators.Length; i++)
            {
                animators[i].Play(animName, 0, 0);
            }

            if (callback != null)
            {
                AnimCoroute = StartCoroutine(DelayCallback(animLength, callback, callbackParam));
            }
        }

        /// <summary>
        /// 检索动画长度
        /// </summary>
        private float GetAnimClipLength(Animator animator, string animName)
        {
            if (animator == null) return 0;
            if (string.IsNullOrEmpty(animName)) return 0;
            var clips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                if (string.Equals(clip.name, animName))
                {
                    return clip.length;
                }
            }
            return 0;
        }

        IEnumerator DelayCallback(float length, Action<object[]> callback, object[] param)
        {
            yield return new WaitForSeconds(length);
            callback?.Invoke(param);
        }

        private void StopAnimCoroutine()
        {
            if (AnimCoroute != null)
            {
                StopCoroutine(AnimCoroute);
                AnimCoroute = null;
            }
        }
        #endregion
    }
}