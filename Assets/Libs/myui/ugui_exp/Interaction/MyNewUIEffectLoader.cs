

using Coffee.UIExtensions;
using System.Collections;

namespace UnityEngine.UI
{
    public class MyNewUIEffectLoader : MonoBehaviour
    {
        [Header("特效资源名")]
        public string resName = "";
        [Header("缩放")]
        public Vector2 scale = Vector2.zero;

        private BundleRequestInfo param_model = null;
        //private PrefabBundle prefabBundle;
        private GameObject model = null;
        private UIParticle uIParticle;
        private float particleInitScale;

        private Vector3 _pos = Vector3.zero;
        public Vector3 localPosition { set { _pos = value; if (model != null) { model.transform.localPosition = _pos; } } get { return _pos; } }
        public int quality { set; get; }

        public float add_scale_z { set; get; } = 1;

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
                    uIParticle = null;
                }
                model = go.responseObject;
                if(uIParticle == null)
                {
                    uIParticle = model.GetComponent<UIParticle>();
                    if (uIParticle == null)
                    {
                        return;
                    }
                    particleInitScale = uIParticle.scale;
                }
                model.gameObject.SetActive(true);
                model.transform.SetParent(transform);
                model.transform.localPosition = localPosition;
                var rect = model.transform as RectTransform;
                if (rect != null)
                {
                    rect.anchoredPosition = localPosition;
                }
                model.transform.localEulerAngles = Vector3.zero;


                if (scale.x != 0 && scale.y != 0)
                {
                    uIParticle.scale = scale.x * particleInitScale;
                    if (add_scale_z != 0)
                    {
                        uIParticle.scale3D = new Vector3(uIParticle.scale, uIParticle.scale, uIParticle.scale + add_scale_z);
                    }
                }
                else if (source_size > 0)
                {
                    if (!IsInvoking("_checkSize"))
                    {
                        _checkSize();
                    }
                }

                ShowHideChild(true);
            }
        }

        private void ShowHideChild(bool is_active)
        {
            if (model != null)
            {
                model.SetActive(is_active);
                for (int i = 0; i < model.transform.childCount; i++)
                {
                    var t = model.transform.GetChild(i);
                    t.gameObject.SetActive(is_active);
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
                uIParticle.scale = scale.x * particleInitScale;
                if (add_scale_z != 0)
                {
                    uIParticle.scale3D = new Vector3(uIParticle.scale, uIParticle.scale, uIParticle.scale + add_scale_z);
                }
            }
            if (scale.x < 0 || scale.y < 0)
            {
                Invoke("_checkSize", 0.1f);
            }
        }

        private void OnDestroy()
        {
            if(model != null)
            {
                Destroy(model);
                model = null;
            }
            uIParticle = null;
        }
    }
}
