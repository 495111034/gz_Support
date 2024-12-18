

using Coffee.UIExtensions;
using System.Collections;

namespace UnityEngine.UI
{
    public class MyUIEffectLoader : MonoBehaviour
    {
        [Header("特效资源名")]
        public string resName = "";
        [Header("缩放")]
        public Vector2 scale = Vector2.zero;

        private Vector3 _pos = Vector3.zero;
        public Vector3 localPosition { set { _pos = value; if (model != null) { model.transform.localPosition = _pos; } } get { return _pos; } }
        public float add_scale_z { set; get; } = 1;

        private BundleRequestInfo param_model = null;
        private GameObject model = null;

        private static Sprite sprite = null;

        private RectTransform scroll_trans = null;

        private SpriteMaskInteraction maskInteraction = SpriteMaskInteraction.None;

        private UIParticle uIParticle;
        private float particleInitScale;

        public int quality { set; get; }

        public RectTransform GetParentMaskTrans()
        {
            return scroll_trans;
        }

        public void SetMaskInteraction(SpriteMaskInteraction interaction)
        {
            maskInteraction = interaction;
            find_mask_tag = false;
        }

        private bool find_mask_tag = true;
        private bool already_start = false;

        private void Start()
        {
            already_start = true;
            LoadModelRes(resName);
        }

        private void OnEnable()
        {
            if (already_start)
            {
                LoadModelRes(resName);
            }
        }

        private void OnDisable()
        {
            if (already_start)
            {
                ShowHideChild(false);
            }
        }

        private void FindMaskTag()
        {
            if (find_mask_tag)
            {
                find_mask_tag = false;
                maskInteraction = SpriteMaskInteraction.None;
                RectMask2D rectMask2D = gameObject.GetComponentInParent<RectMask2D>();
                if (rectMask2D != null)
                {
                    scroll_trans = rectMask2D.transform as RectTransform;
                }
                else
                {
                    Mask rectMask = gameObject.GetComponentInParent<Mask>();
                    if (rectMask != null) scroll_trans = rectMask.transform as RectTransform;
                }
                if (scroll_trans != null)
                {
                    maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                    if (scroll_trans.Find("UIEffectRectOutMask_out") != null)
                    {
                        maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                    }
                }
            }
        }

        public void LoadModelRes(string res_name)
        {
            if (resName == res_name && model != null)
            {
                ShowHideChild(!string.IsNullOrEmpty(res_name));
                return;
            }
            resName = res_name;
            if (!string.IsNullOrEmpty(resName))
            {
                if (param_model == null) param_model = new BundleRequestInfo();
                param_model.res_name = resName;
                param_model.res_type = BundlType.prefabObj;
                param_model.character_type = UIRoomCharacterType.ItemRes;
                param_model.requester = this;
                param_model.callBack = FinishLoad;

                CancelInvoke("PostMessageUpwards");
                Invoke("PostMessageUpwards", Time.deltaTime);
            }
            else
            {
                ShowHideChild(false);
            }
        }

        private void PostMessageUpwards()
        {
            SendMessageUpwards("RequestBundle", param_model);
        }

        private void FinishLoad(BundleRequestInfo go)
        {
            if (!this || !gameObject) return;
            if (go.res_name == resName && go.responseObject)
            {
                if (model != null && model != go.responseObject)
                {
                    Destroy(model);
                }
                model = go.responseObject;
                model.gameObject.SetActive(true);
                model.transform.SetParent(transform);
                model.transform.localPosition = localPosition;
                var rect = model.transform as RectTransform;
                if (rect != null)
                {
                    rect.anchoredPosition = localPosition;
                }
                model.transform.localEulerAngles = Vector3.zero;

                uIParticle = model.GetComponent<UIParticle>();
                if (uIParticle != null)
                {
                    particleInitScale = uIParticle.scale;
                }
                else
                {
                    if (find_mask_tag)
                    {
                        FindMaskTag();
                    }
                    if (maskInteraction != SpriteMaskInteraction.None)
                    {
                        ParticleSystem[] particleSystems = model.GetComponentsInChildren<ParticleSystem>();
                        if (particleSystems != null)
                        {
                            for (int i = 0; i < particleSystems.Length; i++)
                            {
                                var particle = particleSystems[i].GetComponent<ParticleSystemRenderer>();
                                particle.maskInteraction = maskInteraction;
                            }
                        }
                    }
                }

                if (scale.x != 0 && scale.y != 0)
                {
                    if (uIParticle != null)
                    {
                        uIParticle.scale = scale.x * particleInitScale;
                        if (add_scale_z != 0)
                        {
                            uIParticle.scale3D = new Vector3(uIParticle.scale, uIParticle.scale, uIParticle.scale + add_scale_z);
                        }
                    }
                    else
                    {
                        model.transform.localScale = new Vector3(scale.x, scale.y, Mathf.Max(scale.x, scale.y));
                    }
                }
                else if (source_size > 0)
                {
                    if (!IsInvoking("_checkSize"))
                    {
                        _checkSize();
                    }
                }

                if (uIParticle != null)
                {
                    ShowHideChild(true);
                }
            }
        }

        private void ShowHideChild(bool is_active)
        {
            if (model != null)
            {
                model.SetActive(is_active);
                if (uIParticle != null)
                {
                    for (int i = 0; i < model.transform.childCount; i++)
                    {
                        var t = model.transform.GetChild(i);
                        t.gameObject.SetActive(is_active);
                    }
                }
            }
        }

        private float source_size = 0;
        public void CheckSizeChange(float source_size)
        {
            this.source_size = source_size;
            _checkSize();
        }

        private void _checkSize()
        {
            scale = Vector2.one * ((transform as RectTransform).rect.width / source_size);
            if (model != null)
            {
                if (uIParticle != null)
                {
                    uIParticle.scale = scale.x * particleInitScale;
                    if (add_scale_z != 0)
                    {
                        uIParticle.scale3D = new Vector3(uIParticle.scale, uIParticle.scale, uIParticle.scale + add_scale_z);
                    }
                }
                else
                {
                    model.transform.localScale = new Vector3(scale.x, scale.y, Mathf.Max(scale.x, scale.y));
                }
            }
            if (scale.x < 0 || scale.y < 0)
            {
                Invoke("_checkSize", 0.1f);
            }
        }

        private void OnDestroy()
        {
            if (model != null)
            {
                Destroy(model);
                model = null;
            }
            uIParticle = null;
        }
    }
}
