using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(EmptyImage))]
    [AddComponentMenu("UI/My3DRoomCamera", 5)]
    public class My3DRoomCamera : MonoBehaviour, IDragHandler
    {
        [Header("相机是否使用正交")]
        public bool orthographic = false;
        [Header("相机广角")]
        public float fieldOfView = 10;
        [Header("预放置模型资源名")]
        public string resName = "";
        [Header("默认偏移")]
        public Vector3 frist_offset;
        [Header("默认旋转")]
        public Vector3 frist_ratation;

        public BundlType uiroomType = BundlType.singleObj;
        public UIRoomCharacterType characterType = UIRoomCharacterType.Role;
        public DropEventType dropType = DropEventType.None;

        private float cameraFielodOfView = 1;
        private Vector3 modelRotation;
        private Vector3 cameraOffset;
        private float curr_depth = 0;

        public static GameObject root_obj { set; get; }
        private GameObject my_root = null;
        private Camera model_camera;
        private Transform model;
        private BundleRequestInfo param_model = null;
        //private SceneFogManager sceneFogManager;
        private Coroutine coroutine = null;
        public int Career { set; get; }

        private Canvas _canvas;
        private Canvas curr_canvas
        {
            get
            {
                if (_canvas == null)
                {
                    _canvas = gameObject.GetComponentInParent<Canvas>();
                }
                return _canvas;
            }
        }

        private int def_layer = 5; //默认使用UI的层级

        private bool _is_full_camera; //是否不计算UI区域，直接全尺寸

        private Light directional = null;

        public void SetFullCameraModel(bool b)
        {
            _is_full_camera = b;
            if (model_camera != null)
            {
                if (param_model != null)
                {
                    model_camera.rect = param_model.camera_rect;
                }
                else
                {
                    model_camera.rect = new Rect(0, 0, 1, 1);
                }
            }
        }

        private void Awake()
        {
            cameraFielodOfView = fieldOfView;
            modelRotation = frist_ratation;
            cameraOffset = frist_offset;
        }

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
                    directional.cullingMask = LayerMask.GetMask("UI");
                }
            }

            if (!string.IsNullOrEmpty(resName) && root_obj == null)
            {
                InitCamera();
                LoadModelRes(resName);
            }
        }

        private void Init()
        {
            if (root_obj == null)
            {
                root_obj = GameObject.Find("My3DRoomCamera");
                if (root_obj == null)
                {
                    root_obj = new GameObject("My3DRoomCamera");
                    Object.DontDestroyOnLoad(root_obj);
                }
            }

            my_root = new GameObject("my_root");
            my_root.transform.SetParent(root_obj.transform);
            my_root.transform.localScale = Vector3.one;

            for (int i = 0; i < root_obj.transform.childCount; i++)
            {
                root_obj.transform.GetChild(i).position = new Vector3(-5000, i * -10, -5000);
            }
        }

        private void OnEnable()
        {
            if (my_root != null)
            {
                my_root.gameObject.SetActive(true);
            }
            RecoveryFog();
        }

        private void OnDisable()
        {
            if (my_root != null)
            {
                my_root.gameObject.SetActive(false);
            }
            RecoveryFog();
        }

        private void OnDestroy()
        {
            if (my_root != null)
            {
                GameObject.Destroy(my_root.gameObject);
            }
            my_root = null;
            model = null;
            param_model = null;
            model_camera = null;
            RecoveryFog();
        }

        private void RecoveryFog()
        {
            //if (sceneFogManager != null)
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
            if (My3DRoomScene.root_obj != null)
            {
                count += My3DRoomScene.root_obj.transform.childCount;
            }
            return count;
        }

        public void InitCamera()
        {
            if (my_root == null)
            {
                Init();
            }
            if (model != null) model.gameObject.SetActiveX(false);
            if (model_camera == null)
            {
                GameObject go = new GameObject("model_camera");
                go.transform.SetParent(my_root.transform);
                go.transform.localPosition = Vector3.forward;
                model_camera = go.AddComponent<Camera>();
                model_camera.allowMSAA = false;
                model_camera.allowHDR = false;
                model_camera.clearFlags = CameraClearFlags.Depth;
                model_camera.farClipPlane = 50;
                model_camera.cullingMask = 1 << def_layer;
            }
            model_camera.orthographic = orthographic;
            model_camera.fieldOfView = fieldOfView;
            if (!_is_full_camera)
            {
                model_camera.rect = Convert(transform as RectTransform, curr_canvas.worldCamera, param_model);
                if (param_model == null)
                {
                    if (coroutine != null)
                    {
                        StopCoroutine(coroutine);
                    }
                    coroutine = StartCoroutine(SetCameraRect());
                }
            }
            else
            {
                if (param_model != null)
                {
                    model_camera.rect = param_model.camera_rect;
                }
            }
            if (curr_depth == 0) curr_depth = curr_canvas.worldCamera.depth + 1;
            SetCameraDepth(curr_depth);
        }

        public static Rect Convert(RectTransform rectTransform, Camera camera, BundleRequestInfo info)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            float height = corners[2].y - corners[3].y;
            corners[0].y += Mathf.Abs(height) * (info == null ? 0 : info.camera_rect.y);
            var bottomLeft = camera.WorldToViewportPoint(corners[0]);
            var topLeft = camera.WorldToViewportPoint(corners[1]);
            var topRight = camera.WorldToViewportPoint(corners[2]);
            var bottomRight = camera.WorldToViewportPoint(corners[3]);

            var rect = new Rect();
            rect.x = Mathf.Clamp01(bottomLeft.x);
            rect.y = Mathf.Clamp01(bottomLeft.y);
            rect.width = Mathf.Clamp01(topRight.x - topLeft.x);
            rect.height = Mathf.Clamp01((topRight.y - bottomRight.y) * (info == null ? 1 : info.camera_rect.height));

            return rect;
        }

        public void LoadModelRes(string res_name)
        {
            if (resName == res_name && model != null)
            {
                model.gameObject.SetActiveX(true);
                return;
            }
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            if (my_root == null)
            {
                InitCamera();
            }
            if (model != null) model.gameObject.SetActive(false);
            resName = res_name;
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
                        //if (sceneFogManager == null)
                        //{
                        //    sceneFogManager = FindObjectOfType<SceneFogManager>();

                        //}
                        //if (sceneFogManager != null)
                        //{
                        //    sceneFogManager.enabled = false;
                        //}

                        model = go.responseObject.transform;
                        model.gameObject.SetActive(true);
                        model.transform.SetParent(my_root.transform);
                        model.transform.localPosition = Vector3.forward * 15;
                        model.gameObject.SetLayerRecursively(def_layer);
                        SetPamas();
                    }
                };

                StartCoroutine(PostMessageUpwards("RequestBundle", param_model));
            }
        }

        IEnumerator PostMessageUpwards(string metname, object param)
        {
            yield return null;
            SendMessageUpwards(metname, param);
        }

        public void SetCameraDepth(float depth)
        {
            if (model_camera != null)
            {
                model_camera.depth = depth;
            }
            curr_depth = depth;
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
            if (len > 4) this.cameraFielodOfView = infos[4];// 1 + (1 - infos[4]);
            this.cameraOffset = offset;

            SetPamas();
        }

        public void SetModelRotation(Vector3 rotation)
        {
            this.modelRotation = rotation;
            SetPamas();
        }

        public void SetFielodOfView(float fieldOfView)
        {
            this.cameraFielodOfView = fieldOfView;
            SetPamas();
        }

        private void SetPamas()
        {
            if (model != null)
            {
                model.localEulerAngles = modelRotation;
            }

            if (model_camera != null)
            {
                model_camera.transform.localPosition = cameraOffset;
                if (orthographic)
                {
                    model_camera.orthographicSize = cameraFielodOfView;
                }
                else
                {
                    model_camera.fieldOfView = cameraFielodOfView;
                }

                if (model != null)
                {
                    model_camera.cullingMask = 1 << model.gameObject.layer;
                }
                if (!_is_full_camera)
                {
                    if (coroutine != null)
                    {
                        StopCoroutine(coroutine);
                    }
                    coroutine = StartCoroutine(SetCameraRect());
                }
                else
                {
                    if (param_model != null)
                    {
                        model_camera.rect = param_model.camera_rect;
                    }
                }
            }
        }

        private IEnumerator SetCameraRect()
        {
            yield return new WaitForEndOfFrame();
            model_camera.enabled = false;
            while (model == null)
            {
                yield return new WaitForEndOfFrame();
            }
            if (model_camera != null && transform != null)
            {
                model_camera.rect = Convert(transform as RectTransform, curr_canvas.worldCamera, param_model);
                model_camera.enabled = true;
            }
            coroutine = null;
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
    }
}
