using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    public enum BundlType
    {
        character = 1,
        singleObj = 2,
        prefabObj = 3,
        sceneObj = 4,
    }
    public class BundleRequestInfo
    {
        public string res_name;
        public int career;
        public BundlType res_type;
        public UIRoomCharacterType character_type;
        public Action<BundleRequestInfo> callBack;
        public GameObject responseObject;
        public MonoBehaviour requester;
        public Rect camera_rect = new Rect(0, 0, 1, 1);
    }    

    public enum UIRoomCharacterType
    {
        Role = 1,
        Npc = 2,
        Pet = 3,       
        Monster = 5,
        Dragon = 6,
        /// <summary>
        /// 仅用在界面上加载资源显示
        /// </summary>
        ItemRes = 7,
        Horse = 8,
    }

    public enum ClickEventType
    {
        None = 0,
        ClickByRoom = 1,
        ClickByChildObject = 2,
    }

    public enum DropEventType
    {
        None = 0,
        HorizontalRotation = 1,
        VerticalRotation = 2,
        FreeRotation = 3,
    }

    /// <summary>
    /// 图片精度等级
    /// </summary>
    public enum ImageAccuracyLevel
    {
        Low = 0,
        Middle = 1,
        Good =2,
    }

    public class My3DRoomImageCamera
    {
        public Camera renderTextureCamera;
        public RenderTexture renderTexture;

        public void ReleaseTemporary()
        {
            RenderTexture.ReleaseTemporary(renderTexture);
        }
    }


    [AddComponentMenu("UI/My3DRoomImage", 5)]
    public class My3DRoomImage:MonoBehaviour, IDragHandler, IPointerClickHandler,IPointerDownHandler
    {
        [HideInInspector]
        [SerializeField]
        protected string resName;
        [HideInInspector]
        [SerializeField]
        protected bool isOrthoGraphic = true;
        [HideInInspector]
        [SerializeField]
        protected BundlType uiroomType = BundlType.singleObj;
        [HideInInspector]
        [SerializeField]
        protected bool foceHeightQuality = false;
        [HideInInspector]
        [SerializeField]
        protected Vector3 cameraOffset = Vector3.zero;
        [HideInInspector]
        [SerializeField]
        protected Vector3 cameraRotation = Quaternion.identity.eulerAngles;
        [HideInInspector]
        [SerializeField]
        protected Vector3 objectRotation = Quaternion.identity.eulerAngles;
        [HideInInspector]
        [SerializeField]
        protected Vector3 objectScale = Vector3.one;
        [HideInInspector]
        [SerializeField]
        protected string attachmentName = "";
        [HideInInspector]
        [SerializeField]
        protected UIRoomCharacterType characterType = UIRoomCharacterType.Npc;
        [HideInInspector]
        [SerializeField]
        protected bool useCameraConfig2 = false;
        [HideInInspector]
        [SerializeField]
        protected float cameraFieldView = 0;
        [HideInInspector]
        [SerializeField]
        protected float cameraNearClip = 0;
        [HideInInspector]
        [SerializeField]
        protected float cameraFarClip = 0;
        [HideInInspector]
        [SerializeField]
        protected ClickEventType clickType = ClickEventType.None;
        [HideInInspector]
        [SerializeField]
        protected DropEventType dropType = DropEventType.None;
        [HideInInspector]
        [SerializeField]
        protected ImageAccuracyLevel accuracyLevel = ImageAccuracyLevel.Good;
        [HideInInspector]
        [SerializeField]
        protected bool useHDR = false;
        [HideInInspector]
        [SerializeField]
        protected bool usePointLight = true;
        [HideInInspector]
        [SerializeField]
        [ColorUsage(false,true)]
        protected Color pointLightColor = Color.white;
        [HideInInspector]
        [SerializeField]
        protected Vector3 pointOffset = Vector3.one;
        [HideInInspector]
        [SerializeField]
        protected float lightIntensity = 1;
        [HideInInspector]
        [SerializeField]
        protected Vector3 lightDirection = Vector3.down;
        [HideInInspector]
        [SerializeField]
        protected LightType lightType = LightType.Directional;
        [HideInInspector]
        [SerializeField]
        protected bool useBloom = true;
        [HideInInspector]
        [SerializeField]
        protected float thresholdGamma = 0.8f;
        [HideInInspector]
        [SerializeField]
        protected float bloomRadius = 2.5f;
        [HideInInspector]
        [SerializeField]
        protected float bloomIntensity = 0.8f;
        [HideInInspector]
        [SerializeField]
        protected float bloomSoftKnee = 0.88f;
        [HideInInspector]
        [SerializeField]
        protected bool useSSS = true;
        [HideInInspector]
        [SerializeField]
        protected bool useRenderTexture = true;
        [HideInInspector]
        [SerializeField]
        protected bool showShadows = true;
        [HideInInspector]
        [SerializeField]
        protected Vector3 shadowProjectorPosition = Vector3.zero;
        [HideInInspector]
        [SerializeField]
        protected Vector3 shadowProjectorRotation = new Vector3(56, 29, 0);
        [HideInInspector]
        [SerializeField]
        [Range(1,30)]
        protected float shadowSize = 3f;
        [HideInInspector]
        [SerializeField]
        protected ImageAccuracyLevel shadowLevel = ImageAccuracyLevel.Middle;
        [HideInInspector]
        [SerializeField]
        protected bool useColorSuite = false;

        #region 颜色校正参数

        // White balance.
        [SerializeField] float _colorTemp = 0.0f;
        [SerializeField] float _colorTint = 0.0f;

        /// <summary>
        /// 色温
        /// </summary>
        public float colorTemp
        {
            get { return _colorTemp; }
            set { _colorTemp = value; }
        }
        /// <summary>
        /// 颜色(绿-紫)
        /// </summary>
        public float colorTint
        {
            get { return _colorTint; }
            set { _colorTint = value; }
        }

        // 色调映射.
        [SerializeField] bool _toneMapping = false;
        [SerializeField] float _exposure = 1.0f;

        /// <summary>
        /// 开启色调映射
        /// </summary>
        public bool toneMapping
        {
            get { return _toneMapping; }
            set { _toneMapping = value; }
        }
        /// <summary>
        /// 曝光度
        /// </summary>
        public float exposure
        {
            get { return _exposure; }
            set { _exposure = value; }
        }

        // 饱和度
        [SerializeField] float _saturation = 1.0f;

        /// <summary>
        /// 饱和度
        /// </summary>
        public float saturation
        {
            get { return _saturation; }
            set { _saturation = value; }
        }

        // Curves.
        [SerializeField] AnimationCurve _rCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _gCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _bCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _cCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public AnimationCurve redCurve
        {
            get { return _rCurve; }
            set { _rCurve = value;  }
        }
        public AnimationCurve greenCurve
        {
            get { return _gCurve; }
            set { _gCurve = value;  }
        }
        public AnimationCurve blueCurve
        {
            get { return _bCurve; }
            set { _bCurve = value; }
        }
        public AnimationCurve rgbCurve
        {
            get { return _cCurve; }
            set { _cCurve = value;  }
        }

        #endregion

        protected ObjLayerMask layerMask = ObjLayerMask.ViewAll;

        [HideInInspector]
        [SerializeField]
        protected bool use_other_offset = false;

        bool _isInit = false;

        public string ResName { get { return resName; } set { if (resName == value) { return; } resName = value; _isInit = false; } }
        public string AttachmentName { get { return attachmentName; } set { attachmentName = value; } }
        public bool IsOrthographic { get { return isOrthoGraphic; } set { isOrthoGraphic = value; _isInit = false; } }
        public BundlType ResouseType { get { return uiroomType; } set { uiroomType = value; _isInit = false; } }
        public bool FoceHeightQuality { get { return foceHeightQuality; }set { foceHeightQuality = value;_isInit = false; } }
        public Vector3 CameraOffset { get { return cameraOffset; } set { cameraOffset = value; SetCamera(); } }
        public Vector3 CameraRotation { get { return cameraRotation; } set { cameraRotation = value; SetCamera(); } }
        public Vector3 ObjectRotation { get { return objectRotation; } set { objectRotation = value; SetRotation(); } }
        public Vector3 ObjectScale { get { return objectScale; } set { objectScale = value; SetScale(); } }
        public UIRoomCharacterType CharacterType { get { return characterType; } set { characterType = value; _isInit = false; } }
        public float CameraFieldView { get { return cameraFieldView; } set { cameraFieldView = value; SetCamera(); } }
        public float CameraNearClip { get { return cameraNearClip; } set { cameraNearClip = value; SetCamera(); } }
        public float CameraFarClip { get { return cameraFarClip; } set { cameraFarClip = value; SetCamera(); } }
        public bool UseCameraConfig2 { get { return useCameraConfig2; } set { useCameraConfig2 = value; } }
        public ClickEventType ClickType { get { return clickType; }set { clickType = value; } }
        public DropEventType DropType { get { return dropType; } set { dropType = value; } }
        public ImageAccuracyLevel AccuracyLevel { get { return accuracyLevel; } set { accuracyLevel = value; } }
        public Color PointLightColor { get { return pointLightColor; }set { pointLightColor = value; } }
        public Vector3 LightDirection { get { return lightDirection; }set { lightDirection = value; } }
        public float LightIntensity { get { return lightIntensity; }set { lightIntensity = value; } }
        public bool UsePointLight { get { return usePointLight; }set { usePointLight = value; } }
        public bool UseHDR { get { return useHDR; } set { useHDR = value; } }
        public Vector3 PointOffset { get { return pointOffset; }set { pointOffset = value; } }
        public bool UseBloom { get { return useBloom; }set { useBloom = value; } }
        public float ThresholdGamma { get { return thresholdGamma; }set { thresholdGamma = value; } }
        public float BloomRadius { get { return bloomRadius; }set { bloomRadius = value; } }
        public float BloomIntensity { get { return bloomIntensity; }set { bloomIntensity = value; } }
        public float BloomSoftKnee { get { return bloomSoftKnee; }set { bloomSoftKnee = value; } }
        public bool UseSSS { get { return useSSS; }set { useSSS = value; } }
        public bool UseRenderTexture { get { return useRenderTexture; }set { useRenderTexture = value; } }
        public bool ShowShadows { get { return showShadows; }set { showShadows = value; } }
        public Vector3 ShadowProjectorDir { get { return shadowProjectorRotation; }set { shadowProjectorRotation = value; } }
        public Vector3 ShadowProjectorPosition { get { return shadowProjectorPosition; }set { shadowProjectorPosition = value; } }
        public float ShadowSize { get { return shadowSize; }set { shadowSize = value; } }
        public ImageAccuracyLevel ShadowLevel { get { return shadowLevel; }set { shadowLevel = value; } }
        public bool UseColorSuite { get { return useColorSuite; }set { useColorSuite = value; } }
        /// <summary>
        /// true会优先使用camera_offset_in_ui3
        /// </summary>
        public bool UseOtherOffset { get { return use_other_offset; } set { use_other_offset = value; } }
        public ObjLayerMask LayerMask { get { return layerMask; }set { layerMask = value; } }
        /// <summary>
        /// 初始化是否结束
        /// </summary>
        public bool CameraInitEnd = false;

        /// <summary>
        /// 是否资源内置相机
        /// </summary>
        bool _isNativeCamera = true;

        public Camera roomCamera;
        public int RoomCameraCullingMask { get; set; }

        GameObject childGameObject;
        [NonSerialized] public GameObject ResponseObject;
        [NonSerialized] public int Career;

        public GameObject rootObj;
        public MySpriteImage _texObj = null;
        public GameObject dragTarget = null;
        public List<GameObject> dragTargetList = null;
        BundleRequestInfo request_param = null;
        public void SetCameraVisible(bool isVisible)
        {
            if (!roomCamera) return;
            if (roomCamera.enabled != isVisible)
            {
                roomCamera.enabled = isVisible;
                if (childGameObject)
                {
                    var anims = childGameObject.GetComponentsInChildren<Animation>();
                    for (int i = 0; i < anims.Length; ++i)
                        anims[i].enabled = isVisible;
                }
            }

        }

        RenderTexture _renderTexture;
        My3DRoomImageCamera _renderCamera2;

        /// <summary>
        /// 是否资源内置相机
        /// </summary>
        /// <returns></returns>
        public bool IsNativeCamera()
        {
            return _isNativeCamera;
        }


        bool _setCamera = true;

        void Awake()
        {
            InitContent();
        }

        void Start()
        {
            if (_isInit) return;
            InitRoom();
        }

        void OnDestroy()
        {
            ReleaseRoom();
            CameraInitEnd = false;
            //if(childGameObject)
            //{
            //    Debug.LogError("OnDestory:" + gameObject.name);
            //    GameObject.Destroy(childGameObject);
            //}
            //childGameObject = null;
            //_isInit = false;
        }

        void OnDisable()
        {
            if (childGameObject)
            {
                childGameObject.SetActive(false);
            }
            if (_texObj != null)
            {
                _texObj.enabled = false;
            }
            //if (childGameObject)
            //{
            //    Debug.LogError("OnDisable:" + gameObject.name);
            //    GameObject.Destroy(childGameObject);
            //}
            //childGameObject = null;
            // _isInit = false;
        }

        void OnEnable()
        {
            if (childGameObject)
            {
                childGameObject.SetActive(true);
            }
            if (_texObj != null && _isInit)
            {
                _texObj.enabled = true;
            }

        }


        float lastUpdateTime = 0;
        void Update()
        {
            if (!_isInit)
            {
                __initRoom();
            }

            if (_setCamera)
            {
                DoSetCamera();
            }

            if (Time.realtimeSinceStartup - lastUpdateTime < 1f) return;
            lastUpdateTime = Time.realtimeSinceStartup;
            if (Graphics.activeTier == UnityEngine.Rendering.GraphicsTier.Tier3 && useSSS && roomCamera && roomCamera.gameObject.GetComponent<MyEffect.CP_SSSSS_Main>() && roomCamera.gameObject.GetComponent<MyEffect.CP_SSSSS_Main>().enabled)
            {
                roomCamera.gameObject.GetComponent<MyEffect.CP_SSSSS_Main>().UpdateSSSRenderList(rootObj.GetComponentsEx<Renderer>());
            }
        }

        public void InitRoom()
        {
            //_isInit = false;

            if (rootObj != null)
            {
                GameObject.Destroy(rootObj);
                rootObj = null;
            }
        }

        void __initRoom()
        {
            InitContent();
            LoadRoomRes();
            _isInit = true;
        }

        /// <summary>
        /// 其它控件与当前对象共用RenderTexture时，清除引用，以免被重复ReleaseTemporary，触发了unity的一个bug
        /// </summary>
        /// <param name="tex"></param>
        public void ClearRenderTexture(RenderTexture tex)
        {
            if(_renderTexture == tex)
            {
                _renderTexture = null;
            }
        }

        public void ReleaseRoom()
        {
            if (_renderTexture != null)
            {
                //清除其它控件对_renderTexture的引用，以免被重复ReleaseTemporary
                if (_texObj.mainTexture == _renderTexture)
                    _texObj.SetTexture(null, null);
                RenderTexture.ReleaseTemporary(_renderTexture);
                _renderTexture = null;

                if (_renderCamera2 != null)
                {
                    _renderCamera2.ReleaseTemporary();
                }
            }

            CleanShadowProjector();

            if (_texObj) _texObj.SetTexture(null, null);

            if (_pointLight)
                GameObject.Destroy(_pointLight.gameObject);

            if (childGameObject)
            {
                GameObject.Destroy(childGameObject);
            }
            if (roomCamera)
            {
                GameObject.Destroy(roomCamera.gameObject);
            }
            _renderCamera2 = null;

            childGameObject = null;
            _isInit = false;
            CameraInitEnd = false;
            request_param = null;
            resName = "";
        }
        void InitContent()
        {
            var rawImage = GetComponent<RawImage>();
            if (rawImage != null)
            {
                DestroyImmediate(rawImage);
            }

            _texObj = GetComponent<MySpriteImage>();

            if (_texObj == null)
            {
                _texObj = GameObjectUtils.AddMissingComponent<MySpriteImage>(gameObject);
            }

            if (_texObj != null)
            {
                _texObj.enabled = false;
            }
        }

        public void LoadRoomRes()
        {
            CameraInitEnd = false;
            if (!childGameObject)
            {
                childGameObject = new GameObject();
                if(!MyUITools.roomRoot)
                {
                    MyUITools.roomRoot = new GameObject();
                    MyUITools.roomRoot.name = "RoomRoot";
                    Object.DontDestroyOnLoad(MyUITools.roomRoot);
                }
                childGameObject.transform.parent = MyUITools.roomRoot.transform;
                childGameObject.name = gameObject.name + "'s childGameObject";

            }
            childGameObject.SetActive(true);
            for (int i = 0; i < MyUITools.roomRoot.transform.childCount; i++)
            {
                MyUITools.roomRoot.transform.GetChild(i).position = new Vector3(-1999, i * -50, -1999);
            }
            var __event = GameObjectUtils.AddMissingComponent<UIRoomChildEvent>(childGameObject);
            __event.parent_object = gameObject;

            if (!rootObj)
            {
                rootObj = GameObjectUtils.AddChild(childGameObject);
                rootObj.name = "rootObj";
            }

            rootObj.transform.localPosition = new Vector3(0, 0, -5);
            rootObj.transform.localRotation = Quaternion.identity;
            rootObj.SetActive(true);


            if(_texObj!=null)
                _texObj.SetTexture(null, null);


            SetRotation();           

            if (!string.IsNullOrEmpty(resName) && resName != "0")
            {
                if(request_param == null) request_param = new BundleRequestInfo();
                request_param.res_name = resName;
                request_param.career = Career;
                request_param.res_type = uiroomType;
                request_param.character_type = characterType;
                request_param.requester = this;
                request_param.callBack = (go) =>
                {
                    if (!this || !gameObject || !gameObject.activeInHierarchy) return;
                    if (go.res_name == resName && go.responseObject)
                    {
                        ResponseObject = go.responseObject;
                        if (uiroomType == BundlType.character)
                        {
                            //if (characterType == UIRoomCharacterType.Role)
                            //{
                            //    go.responseObject.SetLayerRecursively((int)ObjLayer.Player);
                            //} else if (characterType == UIRoomCharacterType.Pet)
                            //{
                            //    go.responseObject.SetLayerRecursively((int)ObjLayer.Pet);
                            //}
                        }
                        go.responseObject.transform.parent = rootObj.transform;
                        go.responseObject.transform.localPosition = Vector3.zero;
                        go.responseObject.transform.localScale = objectScale;
                        go.responseObject.transform.localRotation = Quaternion.identity;
                    }
                    CreatePointLight();
                    StartCoroutine(SetCameraTexture());
                };
                StartCoroutine(PostMessageUpwards("RequestBundle", request_param));
            }
        }

        public void CreateLightAndCamera()
        {
            CreatePointLight();
            StartCoroutine(SetCameraTexture());
        }

        IEnumerator PostMessageUpwards(string metname, object param)
        {
            yield return null;
            SendMessageUpwards(metname, param);
        }

        Vector2 _camera_size = new Vector2(512,512);
        IEnumerator SetCameraTexture()
        {
         
            if (_texObj)
            {
                _texObj.SetTexture(null, null);
                _texObj.enabled = false;
            }

            if (!this || !gameObject || !gameObject.activeInHierarchy) yield break;
            if (!roomCamera)
            {
                var camObj = GameObjectUtils.FindInChild<Camera>(childGameObject);
                if (!camObj)
                {
                    roomCamera = GameObjectUtils.AddChild<Camera>(childGameObject);
                    roomCamera.transform.localPosition = cameraOffset;
                    roomCamera.transform.localRotation = Quaternion.Euler(cameraRotation);

                    if (IsOrthographic)
                    {
                        roomCamera.orthographic = true;
                        roomCamera.orthographicSize = 1f;
                        roomCamera.farClipPlane = 10;
                        roomCamera.nearClipPlane = -10;
                    }
                    else
                    {
                        roomCamera.orthographic = false;
                        roomCamera.fieldOfView = cameraFieldView;
                        roomCamera.farClipPlane = cameraFarClip;
                        roomCamera.nearClipPlane = cameraNearClip;
                    }

                                    
                    _isNativeCamera = false;
                    roomCamera.renderingPath = RenderingPath.Forward;                   
                    roomCamera.useOcclusionCulling = false;

                }
                else
                {
                    _isNativeCamera = true;
                    roomCamera = camObj;
                    if (roomCamera.gameObject.GetComponent<AudioListener>())
                    {
                        GameObject.Destroy(roomCamera.gameObject.GetComponent<AudioListener>());
                    }
                    var postLayer = roomCamera.gameObject.GetComponent<Rendering.PostProcessing.PostProcessLayer>();
                    if (postLayer != null) postLayer.enabled = false;
                    var postDebug = roomCamera.gameObject.GetComponent<Rendering.PostProcessing.PostProcessDebug>();
                    if (postDebug != null) postDebug.enabled = false;
                    var postVolumn = roomCamera.gameObject.GetComponent<Rendering.PostProcessing.PostProcessVolume>();
                    if (postVolumn != null) postVolumn.enabled = false;

                    if (!useRenderTexture)
                    {
                        roomCamera.tag = "MainCamera";
                    }

                    //if (cameraOffset != Vector3.zero)
                    //{
                    //    roomCamera.transform.localPosition = cameraOffset;
                    //}

                    //if (cameraFieldView > 1)
                    //{
                    //    roomCamera.fieldOfView = cameraFieldView;
                    //}                    

                }
            }

            roomCamera.clearFlags = CameraClearFlags.SolidColor;
            roomCamera.backgroundColor = new Color(0f, 0f, 0f, 0);
            roomCamera.allowMSAA = false;
            roomCamera.targetTexture = null;
            roomCamera.useOcclusionCulling = true;
            roomCamera.depth = -10;
            if (RoomCameraCullingMask != 0)
            {
                roomCamera.cullingMask = RoomCameraCullingMask;
            } else
            {
                roomCamera.cullingMask = (int)(~ObjLayerMask.BackGround & ~ObjLayerMask.Default);
            }

            if (Graphics.activeTier >= Rendering.GraphicsTier.Tier3 && useHDR)
            {
                //roomCamera.allowHDR = useHDR; 
                roomCamera.allowHDR = false;
            }
            else
            {               
                roomCamera.allowHDR = false;
            }

            if(useBloom && Graphics.activeTier >= Rendering.GraphicsTier.Tier3)
            {
                var bloom = roomCamera.gameObject.AddMissingComponent<MyEffect.Bloom>();

                if (!roomCamera.allowHDR && useHDR)
                    bloom.thresholdGamma = thresholdGamma * 0.8f;
                else
                    bloom.thresholdGamma = thresholdGamma;

                bloom.radius = bloomRadius;
                bloom.intensity = bloomIntensity;
                bloom.softKnee = bloomSoftKnee;
                bloom.highQuality =!QualityUtils.IsLowMem;
                bloom.antiFlicker = true;

                if (useRenderTexture)
                    bloom.IsInUI = true;
                else
                    bloom.IsInUI = false;
            }
            else
            {
                if (roomCamera.gameObject.GetComponent<MyEffect.Bloom>())
                {
                    Destroy(roomCamera.gameObject.GetComponent<MyEffect.Bloom>());
                }
            }

            if(useSSS && Graphics.activeTier >= Rendering.GraphicsTier.Tier3)
            {
                var sss = roomCamera.gameObject.AddMissingComponent<MyEffect.CP_SSSSS_Main>();
                sss.scatterIntensity = 2f;
                sss.affectDirect = 1f;
            }
            else
            {
                if(roomCamera.gameObject.GetComponent<MyEffect.CP_SSSSS_Main>())
                {
                    Destroy(roomCamera.gameObject.GetComponent<MyEffect.CP_SSSSS_Main>());
                }
            }

            if(useColorSuite)
            {
                var _colorSuite = roomCamera.gameObject.AddMissingComponent<MyEffect.ColorSuite>();
                _colorSuite.saturation = _saturation;
                _colorSuite.colorTemp = _colorTemp;
                _colorSuite.colorTint = _colorTint;
                _colorSuite.toneMapping = _toneMapping;
                _colorSuite.exposure = _exposure;
                _colorSuite.SetRGBCurvas(_rCurve, _gCurve, _bCurve, _cCurve);
            }

            if (useBloom || useColorSuite)
            {
                roomCamera.gameObject.AddMissingComponent<MyEffect.MyPostEffectsBase>();
            }

            roomCamera.enabled = false;
            yield return null;
            roomCamera.enabled = true;


            while (!roomCamera.enabled)
                yield return 50;

            yield return null;
            
            if (useRenderTexture)
            {
                float accuraryValue = 1f;
                bool useRGBM = false;
                RenderTextureFormat rtFormat = RenderTextureFormat.Default;
                if (!_renderTexture)
                {
                    _camera_size = _texObj.rectTransform.GetSize();
                    switch (accuracyLevel)
                    {
                        case ImageAccuracyLevel.Low:
                            accuraryValue = 0.7f;
                            break;
                        case ImageAccuracyLevel.Middle:
                            accuraryValue = 1f;
                            break;
                        case ImageAccuracyLevel.Good:
                            accuraryValue = 1.5f;
                            break;
                    }

                    if (QualityUtils.IsLowMem) accuraryValue = accuraryValue * 0.5f;

                    useRGBM =  !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.DefaultHDR) || Application.isMobilePlatform || !useHDR || Graphics.activeTier < UnityEngine.Rendering.GraphicsTier.Tier3;
                    
                    //var useRGBM = !useHDR || Graphics.activeTier < Rendering.GraphicsTier.Tier3;

                    rtFormat = useRGBM ?
                         RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR;

                    _renderTexture = RenderTexture.GetTemporary((int)(_camera_size.x * accuraryValue), (int)(_camera_size.y * accuraryValue), 24, GetTextureFormat());
                    _renderTexture.useMipMap = false;
                }

                if (_renderTexture)
                    roomCamera.targetTexture = _renderTexture;

                roomCamera.depth = -99;
                roomCamera.gameObject.tag = "Untagged";
                _texObj.material = UIGrapAssets.m_default_ui_mat;
                if (roomCamera.allowHDR)
                {
                    if (roomCamera.transform.childCount == 0 || _renderCamera2 == null)
                    {
                        Camera new_camera = GameObjectUtils.AddChild<Camera>(roomCamera.gameObject);
                        new_camera.transform.localPosition = Vector3.zero;
                        new_camera.transform.localEulerAngles = Vector3.zero;

                        if (_renderCamera2 == null)
                        {
                            _renderCamera2 = new My3DRoomImageCamera();
                        }

                        if (IsOrthographic)
                        {
                            new_camera.orthographic = true;
                            new_camera.orthographicSize = roomCamera.orthographicSize;
                            new_camera.farClipPlane = 10;
                            new_camera.nearClipPlane = -10;
                        }
                        else
                        {
                            new_camera.orthographic = false;
                            new_camera.fieldOfView = cameraFieldView;
                            new_camera.farClipPlane = cameraFarClip;
                            new_camera.nearClipPlane = cameraNearClip;
                        }
                        new_camera.renderingPath = RenderingPath.Forward;
                        new_camera.allowHDR = false;
                        new_camera.allowMSAA = false;
                        new_camera.clearFlags = CameraClearFlags.SolidColor;
                        new_camera.backgroundColor = Color.clear;
                        new_camera.useOcclusionCulling = false;
                        new_camera.depth = -100;
                        new_camera.gameObject.tag = "Untagged";
                        new_camera.cullingMask = (int)(~ObjLayerMask.BackGround & ~ObjLayerMask.Default);

                        _renderCamera2.renderTextureCamera = new_camera;
                    }

                    _renderCamera2.renderTexture = RenderTexture.GetTemporary((int)(_camera_size.x * accuraryValue), (int)(_camera_size.y * accuraryValue), 24, GetTextureFormat());
                    _renderCamera2.renderTexture.useMipMap = false;
                    _renderCamera2.renderTextureCamera.targetTexture = _renderCamera2.renderTexture;

                    _texObj.material.SetTexture("_MainTex", _renderCamera2.renderTexture);
                }
            }
            else
            {
                roomCamera.depth = 1;
                roomCamera.gameObject.tag = "MainCamera";
            }
            yield return null;
            
            if (roomCamera && _texObj && this && gameObject && gameObject.activeInHierarchy)
            {
                if (_renderTexture)
                    _texObj.SetTexture(roomCamera.targetTexture, null);

                SendMessageUpwards("UIRoomLoadComplete", this);
            }

            CameraInitEnd = true;
            yield return null;
            if (_texObj)
                _texObj.enabled = true;
        }

        private RenderTextureFormat GetTextureFormat()
        {
            if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat) &&
                SystemInfo.SupportsBlendingOnRenderTextureFormat(RenderTextureFormat.ARGBFloat))
            {
                return RenderTextureFormat.ARGBFloat;
            }

            if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32) &&
                SystemInfo.SupportsBlendingOnRenderTextureFormat(RenderTextureFormat.ARGB32))
            {
                return RenderTextureFormat.ARGB32;
            }
            return RenderTextureFormat.Default;
        }

        void SetCamera()
        {
            _setCamera = true;

        }

        void DoSetCamera()
        {
            if (!roomCamera) return;
            if (uiroomType == BundlType.sceneObj || _isNativeCamera) return;

            roomCamera.transform.localPosition = cameraOffset;
            roomCamera.transform.localRotation = Quaternion.Euler(cameraRotation);

            if (!roomCamera.orthographic)
            {
                roomCamera.fieldOfView = CameraFieldView;
                roomCamera.farClipPlane = CameraFarClip;
                roomCamera.nearClipPlane = CameraNearClip;
            }

            //Log.LogInfo($"DoSetCamera,{cameraOffset},是否正交相机：{roomCamera.orthographic}，cameraFieldView={cameraFieldView}");
            if (roomCamera.orthographic && cameraFieldView > 0)
            {
                roomCamera.orthographicSize = cameraFieldView;
                if (_renderCamera2 != null)
                {
                    _renderCamera2.renderTextureCamera.orthographicSize = cameraFieldView;
                }
            }

            _setCamera = false;
        }

        protected Light _pointLight;
        void CreatePointLight()
        {
            if (_pointLight)
                GameObject.Destroy(_pointLight.gameObject);
            //#if UNITY_ADNROID && !UNITY_EDITOR
            //            var lights = GameObjectUtils.FindsInChild<Light>(childGameObject);
            //            for(int i = 0; i < lights.Count; ++i)
            //            {
            //                lights[i].intensity *= 0.5f;
            //            }
            //#endif

            if (usePointLight && (uiroomType != BundlType.prefabObj && uiroomType != BundlType.sceneObj))
            {
                var lightGameobject = new GameObject();
                lightGameobject.name = "pointLight";
                lightGameobject.transform.parent = childGameObject.transform;
                lightGameobject.transform.localPosition = pointOffset;
                _pointLight = lightGameobject.AddMissingComponent<Light>();
                _pointLight.renderMode = LightRenderMode.ForcePixel;
                _pointLight.type = lightType;
                _pointLight.color = pointLightColor;
                _pointLight.intensity = lightIntensity;
                if (lightType != LightType.Point)
                {
                    _pointLight.transform.rotation = Quaternion.Euler(lightDirection);
                }

            }
            else
            {
                if(uiroomType == BundlType.prefabObj || uiroomType == BundlType.sceneObj)
                {
                    _pointLight = childGameObject.GetComponentEx<Light>();
                }
            }

            ///高配UI场景添加阴影投影器（2020-9-9去除 by yangqibo）
            //if((uiroomType == BundlType.prefabObj || uiroomType == BundlType.sceneObj ) && _pointLight && Graphics.activeTier >= Rendering.GraphicsTier.Tier2)
            //{
            //    CreateShadowProjector();
            //}
           
        }

        void CreateShadowProjector()
        {
            if(rootObj && showShadows)
            {
                var projectorObj = new GameObject();
                projectorObj.transform.parent = rootObj.gameObject.transform;
                projectorObj.transform.localPosition = shadowProjectorPosition;
                projectorObj.transform.localRotation =  Quaternion.Euler(shadowProjectorRotation);
                projectorObj.transform.localScale = Vector3.one;
                projectorObj.layer = (int)ObjLayer.Player;
                projectorObj.name = "UIShadowProjector";

                var shadowProjector = projectorObj.AddComponent<ProjectorShadowUI>();
                shadowProjector.rootGameobject = rootObj;
                shadowProjector.Init();
            }
            
        }

        void CleanShadowProjector()
        {
            if(childGameObject && childGameObject.gameObject.GetComponentInChildren<ProjectorShadowUI>())
            {
                var proector = childGameObject.gameObject.GetComponentInChildren<ProjectorShadowUI>();
                proector.clearData();
                GameObject.Destroy(proector.gameObject);               
            }
        }

        void SetRotation()
        {
            if (!rootObj)
                return;
            if (uiroomType == BundlType.sceneObj) return;
            rootObj.transform.localRotation = Quaternion.Euler(objectRotation);
        }

        void SetScale()
        {
            if (!ResponseObject)
                return;
            if (uiroomType == BundlType.sceneObj) return;
            ResponseObject.transform.localScale = objectScale;
        }

        public Vector2 GetUIPosByGameObject(GameObject obj, Vector3? offset = null)
        {
            var pos3 = roomCamera.WorldToViewportPoint(obj.transform.position + (offset.HasValue ? offset.Value : Vector3.zero));
            var posInCamera = new Vector2(pos3.x * _texObj.rectTransform.rect.width, pos3.y * _texObj.rectTransform.rect.height);

            return posInCamera;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dropType == DropEventType.None)
                return;
           
            var target = dragTarget ? dragTarget : rootObj;
            if (target)
            {
                var rat = target.transform.localRotation.eulerAngles;

                var rats_y = rat.y;
                var rats_z = rat.z;
                if(dropType == DropEventType.HorizontalRotation || dropType == DropEventType.FreeRotation)
                    rats_y -= ((eventData.delta).x / Screen.width) * 360f;
                if (dropType == DropEventType.VerticalRotation || dropType == DropEventType.FreeRotation)
                    rats_z += ((eventData.delta).y / Screen.height) * 360f;
              
                target.transform.localRotation = Quaternion.Euler(new Vector3(rat.x, rats_y, rats_z));
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (clickType == ClickEventType.None)
                return;
            if (!roomCamera ) return;

            else if (clickType == ClickEventType.ClickByRoom)
            {
                SendMessageUpwards("OnClickEvent", this);
                return;
            }
            else
            {
                var posInCamera = eventData.pressPosition;
                if (useRenderTexture && roomCamera.targetTexture)
                {

                    var texPos = MyUITools.GetUIPosition(gameObject);

                    //当前点击在控件内的位置(起点左下)
                    var clickPosInTex = new Vector2(eventData.pressPosition.x - (texPos.x - _texObj.rectTransform.rect.width / 2f), eventData.pressPosition.y - (texPos.y - _texObj.rectTransform.rect.height / 2));

                    //把相对控件的位置当作全屏位置
                    posInCamera = new Vector2(clickPosInTex.x / ((float)_texObj.rectTransform.rect.width / (float)roomCamera.targetTexture.width), clickPosInTex.y / ((float)_texObj.rectTransform.rect.height / (float)roomCamera.targetTexture.height));

                    // Log.LogError($"texPos = {texPos}, _texObj.rectTransform.rect=({ _texObj.rectTransform.rect.width},{ _texObj.rectTransform.rect.height}),eventData.pressPosition={eventData.pressPosition}\n clickPosInTex={clickPosInTex},posInCamera={posInCamera}");

                }
                else
                {
                    posInCamera = eventData.pressPosition;
                }

                var r = roomCamera.ScreenPointToRay(posInCamera);

                RaycastHit hit;
                if (Physics.Raycast(r, out hit, 100f, (int)LayerMask))                {
                    SendMessageUpwards("__OnClickByUIRoomObject", hit.collider.gameObject);
                }
            }

        }

        public virtual void  OnPointerDown(PointerEventData eventData)
        {
            if (dropType == DropEventType.None)
                return;

            if (!roomCamera) return;

            if (dragTargetList != null && dragTargetList.Count > 0)
            {
                var posInCamera = eventData.pressPosition;
                if (useRenderTexture && roomCamera.targetTexture)
                {
                    var texPos = MyUITools.GetUIPosition(gameObject);

                    //当前点击在控件内的位置(起点左下)
                    var clickPosInTex = new Vector2(eventData.pressPosition.x - (texPos.x - _texObj.rectTransform.rect.width / 2f), eventData.pressPosition.y - (texPos.y - _texObj.rectTransform.rect.height / 2));

                    //把相对控件的位置当作全屏位置
                    posInCamera = new Vector2(clickPosInTex.x / ((float)_texObj.rectTransform.rect.width / (float)roomCamera.targetTexture.width), clickPosInTex.y / ((float)_texObj.rectTransform.rect.height / (float)roomCamera.targetTexture.height));

                    // Log.LogError($"texPos = {texPos}, _texObj.rectTransform.rect=({ _texObj.rectTransform.rect.width},{ _texObj.rectTransform.rect.height}),eventData.pressPosition={eventData.pressPosition}\n clickPosInTex={clickPosInTex},posInCamera={posInCamera}");

                }
                else
                {
                    posInCamera = eventData.pressPosition;
                }

                var r = roomCamera.ScreenPointToRay(posInCamera);

                RaycastHit hit;
                if (Physics.Raycast(r, out hit, 200f, (int)LayerMask) && dragTargetList.Contains(hit.collider.gameObject))
                {
                    dragTarget = hit.collider.gameObject;
                }
            }
        }

        public Light GetPointLight()
        {
            return _pointLight;
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
            if (len > 3) this.CameraRotation = Vector3.up * infos[3];
            if (len > 4) this.CameraFieldView = infos[4];// 1 + (1 - infos[4]);
            this.CameraOffset = offset;
        }

        private Tuple<string, Action<object[]>, object[]> CacheWaitPlayAnim;
        private Coroutine AnimCoroute;

        // <summary>
        /// 播放模型动画
        /// 补充：也可以考虑 GetModel，在外部去执行动画操作
        /// </summary>
        public void PlayModelAnimate(string animName, Action<object[]> callback = null, object[] callbackParam = null)
        {
            if (childGameObject == null)
            {
                CacheWaitPlayAnim = Tuple.Create(animName, callback, callbackParam);
                return;
            }

            CacheWaitPlayAnim = null;
            StopAnimCoroutine();
            var animator = childGameObject.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                callback?.Invoke(callbackParam);
                return;
            }

            animator.Play(animName, 0, 0);

            if (callback != null)
            {
                float animLength = GetAnimClipLength(animator, animName);
                if (animLength > 0)
                {

                    AnimCoroute = StartCoroutine(DelayCallback(animLength, callback, callbackParam));
                }
                else
                {
                    // 没找到动画片段，直接回调
                    callback?.Invoke(callbackParam);
                }
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
    }


    public class UIRoomChildEvent : MonoBehaviour
    {
        public GameObject parent_object = null;

        void __OnEffectCallBack(MonoBehaviour behaviour)
        {
            if (behaviour is EffectBehaviour)
            {
                if (parent_object)
                    parent_object.SendMessageUpwards("__OnEffectCallBack", behaviour);
            }
        }
    }
}
