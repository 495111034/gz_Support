using UnityEngine.UI;
using System;
using easing;
using System.Collections.Generic;
using System.Collections;
using uTools;


namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Panel Effect Config", 0)]
    public class EffectPanelConfig : MonoBehaviour
    {
        public enum FadeInOutDir
        {
            center = 0,
            top = 1,
            bottom = 2,
            left = 3,
            right = 4
        };


        [HideInInspector]
        [SerializeField]
        protected bool _appearEffect = false;

        [HideInInspector]
        [SerializeField]
        protected FadeInOutDir _appearSrc;

        [HideInInspector]
        [SerializeField]
        [Range(0, 1)]
        protected float _appearSrcRatio;


        [HideInInspector]
        [SerializeField]
        protected bool _hideEffect = false;

        [HideInInspector]
        [SerializeField]
        protected FadeInOutDir _hideDst;

        [HideInInspector]
        [SerializeField]
        [Range(0, 1)]
        protected float _hideDstRatio;

        [HideInInspector]
        [SerializeField]
        [Range(0, 1)]
        protected float _appearScaleRatio;

        [HideInInspector]
        [SerializeField]
        [Range(0, 1)]
        protected float _hideScaleRatio;

        [HideInInspector]
        [SerializeField]
        [Range(0, 3)]
        protected float _appearTimespace;

        [HideInInspector]
        [SerializeField]
        [Range(0, 3)]
        protected float _hideTimespace;

        [HideInInspector]
        [SerializeField]
        //[Range(0, 1)]
        protected float _appearDelayTime;

        [HideInInspector]
        [SerializeField]
        //[Range(0, 1)]
        protected float _hideDelayTime;

        [HideInInspector]
        [SerializeField]
        private string _appear_sound_id;

        [HideInInspector]
        [SerializeField]
        private string _hide_sound_id;

        [HideInInspector]
        [SerializeField]
        [Range(0, 1)]
        protected float _appear_sound_delay;

        [HideInInspector]
        [SerializeField]
        [Range(0, 1)]
        protected float _hide_sound_delay;

        [HideInInspector]
        [SerializeField]
        private string _appear_compete_sound_id;

        [HideInInspector]
        [SerializeField]
        private string _hide_complete_sound_id;

        [HideInInspector]
        [SerializeField]
        public iTween.EaseType _appearEaseType = iTween.EaseType.easeInOutExpo;

        [HideInInspector]
        [SerializeField]
        iTween.EaseType _hideEaseType = iTween.EaseType.easeInOutExpo;

        [HideInInspector]
        [SerializeField]
        private bool _appear_local_move;

        [HideInInspector]
        [SerializeField]
        private bool _hide_local_move;

        [HideInInspector]
        [SerializeField]
        private Vector2 initPos = Vector2.zero;

        [HideInInspector]
        [SerializeField]
        private Vector3 initScale = Vector3.one;

        [HideInInspector]
        [SerializeField]
        private Vector2 initSize = Vector2.one;


        [HideInInspector]
        [SerializeField]
        private bool _testing;

        private Vector2 _appearPos = Vector2.zero;
        private bool _useUserAppearPos = false;

        private void OnEnable()
        {
            if(_testing)
            {
                if (_appearEffect)
                {
                    MyTask.Run(_playAppearEffectAsync());
                }

                if (_use_fade_tween)
                {
                    MyTask.Run(_playFadeEffectAsync());
                }

                if (_use_path_move)
                {
                    MyTask.Run(_playTweenPath());
                }
            }            
        }

        #region 列表展开效果

        //新增动画
        [HideInInspector]
        [SerializeField]
        protected bool _panelUse = false;

        [HideInInspector]
        [SerializeField]
        protected bool _gameObjUse = false;

        [HideInInspector]
        [SerializeField]
        public bool _listTween_1 = false;

        [HideInInspector]
        [SerializeField]
        public bool _listTween_2 = false;

        [HideInInspector]
        [SerializeField]
        public float _list_tween_dur;


        [HideInInspector]
        [SerializeField]
        public float _list_tween_delay;
        #endregion

        #region 透明度效果

        [HideInInspector]
        [SerializeField]
        public bool _use_fade_tween = false;

        [HideInInspector]
        [SerializeField]
        public float _fate_time;

        [HideInInspector]
        [SerializeField]
        public float _fate_delay;

        #endregion

        #region TweenPath
        [HideInInspector]
        [SerializeField]
        public bool _use_path_move = false;

        [HideInInspector]
        [SerializeField]
        public RectTransform _path_transforms;

        [HideInInspector]
        [SerializeField]
        public float _path_move_time;

        #endregion

        public void SaveInitData()
        {
            initPos = gameObject.GetRectTransform().anchoredPosition;
            initScale = gameObject.GetRectTransform().localScale;
            initSize = gameObject.GetRectTransform().GetSize();
        }

        public static void ResetPosition(List<EffectPanelConfig> configs)
        {
            for (int i = 0; i < configs.Count; ++i)
            {
                configs[i].__resetPosition();
            }
        }

        void __resetPosition()
        {
            var rectTranfomr = gameObject.GetRectTransform();
            if (rectTranfomr)
            {
                rectTranfomr.anchoredPosition = initPos;
                rectTranfomr.localScale = initScale;
            }
        }

        public void SetUserAppearPos(Vector2 pos)
        {
            _useUserAppearPos = true;
            _appearPos = pos;
        }



        public static IEnumerator PlayShowEffectAsync(List<EffectPanelConfig> configs, Action OnShowCallback)

        {
            for (int i = 0; i < configs.Count; ++i)
            {

                if (configs[i]._appearEffect && configs[i].gameObject.activeSelf)
                {
                    MyTask.Run(configs[i]._playAppearEffectAsync());
                }

                if (configs[i]._use_fade_tween && configs[i].gameObject.activeSelf)
                {
                    MyTask.Run(configs[i]._playFadeEffectAsync());
                }

                if(configs[i]._use_path_move && configs[i].gameObject.activeSelf)
                {
                    MyTask.Run(configs[i]._playTweenPath());                    
                }
            }

            while (true)
            {
                bool IsAllComplete = true;
                for (int i = 0; i < configs.Count; ++i)
                {
                    if (configs[i]._isPlayShowEffect)
                    {
                        IsAllComplete = false;
                        break;
                    }
                }
                if (IsAllComplete) break;


                yield return 50;

            }

            OnShowCallback?.Invoke();
            OnShowCallback = null;

        }

        public static IEnumerator PlayHideEffectAsync(List<EffectPanelConfig> configs, Action OnHideCallback)

        {
            for (int i = 0; i < configs.Count; ++i)
            {
                if(configs[i]._hideEffect && configs[i].gameObject.activeSelf)
                    MyTask.Run(configs[i]._playHideEffectAsync());
            }

            while (true)
            {
                bool IsAllComplete = true;
                for (int i = 0; i < configs.Count; ++i)
                {
                    if (configs[i]._isPlayHideEffect)
                    {
                        IsAllComplete = false;
                        break;
                    }
                }
                if (IsAllComplete) break;


                yield return 50;

            }

            OnHideCallback?.Invoke();
            OnHideCallback = null;

        }


        bool _isPlayShowEffect = false;
        bool _isPlayHideEffect = false;


        private IEnumerator _playAppearStartSoundAsync()

        {
            if (string.IsNullOrEmpty(_appear_sound_id))
            {
                yield break;    
            }


            yield return (int)(_appear_sound_delay * 1000);

            SendMessageUpwards("__OnPlaySound", _appear_sound_id);
        }



        private IEnumerator _playHideStartSoundAsync()

        {
            if (string.IsNullOrEmpty(_hide_sound_id))
            {
                yield break;
            }


            yield return (int)(_hide_sound_delay * 1000);

            SendMessageUpwards("__OnPlaySound", _appear_sound_id);
        }

        private void __afterShow()
        {
            _isPlayShowEffect = false;

            if (!this) return;
            if (gameObject)
            {
                if (!string.IsNullOrEmpty(_appear_compete_sound_id)) SendMessageUpwards("__OnPlaySound", _appear_compete_sound_id);
                if (gameObject && gameObject.GetComponent<iTween>())
                {
                    //var path = GameObjectUtils.GetLocation(gameObject);
                    GameObject.Destroy(gameObject.GetComponent<iTween>());
                }
                var rectTranfomr = gameObject.GetRectTransform();
                if (rectTranfomr)
                {
                    rectTranfomr.anchoredPosition = initPos;
                    rectTranfomr.localScale = initScale;
                }
            }
        }

        public void __afterFade()
        {
            _isPlayShowEffect = false;
            if (!this) return;
            if (!gameObject) return;

            var graphic = gameObject.GetComponent<Graphic>();
            if (graphic)
            {
                graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 1);
            }
            else
            {
                var group = gameObject.GetComponent<CanvasGroup>();

                if (group)
                    group.alpha = 1;
            }
        }

        private void TweenPathAfterShow()
        {
            _isPlayShowEffect = false;

            if (!this) return;

            if (gameObject)
            {
                if (!string.IsNullOrEmpty(_appear_compete_sound_id)) SendMessageUpwards("__OnPlaySound", _appear_compete_sound_id);

                if (gameObject && gameObject.GetComponent<iTween>())
                {
                    GameObject.Destroy(gameObject.GetComponent<iTween>());
                }
                var rectTranfomr = gameObject.GetRectTransform();

                if (rectTranfomr)
                {
                    rectTranfomr.localScale = initScale;
                }

                var group = gameObject.GetComponent<CanvasGroup>();

                if (group)
                    group.alpha = 1;
            }
        }


        private IEnumerator _playAppearEffectAsync()

        {

            _isPlayShowEffect = true;


            MyTask.Run(_playAppearStartSoundAsync());


            Canvas rootCanvas = gameObject.GetComponent<Canvas>();
            if (!rootCanvas)
            {
                var list = MyListPool<Canvas>.Get();
                gameObject.GetComponentsInParent(false, list);
                if (list.Count > 0)
                {
                    rootCanvas = list[0];
                    MyListPool<Canvas>.Release(list);
                }
            }

            var go = gameObject;
            GameObject parentObj = null;
            var oldPos = go.GetRectTransform().localPosition;
            var oldScale = initScale;
            var oldsize = initSize;
            if (go.GetComponent<iTween>())
            {
                GameObject.Destroy(go.GetComponent<iTween>());
            }

            int width, height;

            width = Screen.width;
            height = Screen.height;

            if (_appear_local_move)
            {
                if (go.transform.parent)
                {
                    parentObj = go.transform.parent.gameObject;
                    var rect = parentObj.GetRectTransform().GetSize();
                    width = Convert.ToInt32(rect.x);
                    height = Convert.ToInt32(rect.y);
                }
            }

            if (!_appearEffect)
            {
                __afterShow();

                yield break;

            }
            go.SetActive(true);

            MyTask.SetLeave(__afterShow);


            var rt = go.GetRectTransform();

            if (!_useUserAppearPos && !_appear_local_move)
            {
                switch (_appearSrc)
                {
                    case FadeInOutDir.right:
                        rt.localPosition = new Vector3(oldsize.x * _appearScaleRatio / 2f + width / 2, height / 2 - (height * _appearSrcRatio), 0);
                        break;
                    case FadeInOutDir.bottom:
                        rt.localPosition = new Vector3(width * _appearSrcRatio - width / 2, -((oldsize.y * _appearScaleRatio / 2f) + height / 2), 0);
                        break;
                    case FadeInOutDir.left:
                        rt.localPosition = new Vector3(-((oldsize.x * _appearScaleRatio / 2f) + width / 2), height / 2 - (height * _appearSrcRatio), 0);
                        break;
                    case FadeInOutDir.top:
                        rt.localPosition = new Vector3(width * _appearSrcRatio - width / 2, (oldsize.y * _appearScaleRatio / 2f) + height / 2, 0);
                        break;
                }

                rt.localScale = new Vector3(_appearScaleRatio, _appearScaleRatio, 1f);
            }
            else if (!_useUserAppearPos && _appear_local_move)
            {
                switch (_appearSrc)
                {
                    case FadeInOutDir.right:
                        rt.localPosition = new Vector3(width, -1 * height + height * _appearSrcRatio, 0);
                        break;
                    case FadeInOutDir.bottom:
                        rt.localPosition = new Vector3(width * _appearSrcRatio, -(height), 0);
                        break;
                    case FadeInOutDir.left:
                        rt.localPosition = new Vector3(-(width), -1 * height + height * _appearSrcRatio, 0);
                        break;
                    case FadeInOutDir.top:
                        rt.localPosition = new Vector3(width * _appearSrcRatio, height, 0);
                        break;
                }

                rt.localScale = new Vector3(_appearScaleRatio, _appearScaleRatio, 1f);
            }
            else
            {

                Vector2 localPos;
                if (rootCanvas && RectTransformUtility.ScreenPointToLocalPointInRectangle(rootCanvas.transform as RectTransform, _appearPos, rootCanvas.worldCamera, out localPos))
                {
                    rt.anchoredPosition = localPos;
                }
                else
                {
                    rt.anchoredPosition = new Vector2(_appearPos.x - Screen.width / 2, -(Screen.height / 2 - _appearPos.y));
                }

                rt.localScale = new Vector3(0.1f, 0.1f, 1f);
            }

            Hashtable p = new Hashtable();
            p["x"] = oldPos.x;
            p["y"] = oldPos.y;
            p["z"] = oldPos.z;
            p["islocal"] = true;
            if (_appearEaseType != iTween.EaseType.none)
                p["easetype"] = _appearEaseType;
            p["looptype"] = "none";
            p["time"] = _appearTimespace;
            if (_appearDelayTime > 0f) p["delay"] = _appearDelayTime;
            iTween.MoveTo(go, p);

            Hashtable p2 = new Hashtable();
            p2["x"] = oldScale.x;
            p2["y"] = oldScale.y;
            p2["z"] = oldScale.z;
            p2["islocal"] = true;
            if (_appearEaseType != iTween.EaseType.none)
                p2["easetype"] = _appearEaseType;
            p2["looptype"] = "none";
            p2["time"] = _appearTimespace;
            if (_appearDelayTime > 0f) p2["delay"] = _appearDelayTime;
            iTween.ScaleTo(go, p2);


            yield return (int)(_appearTimespace * 1000 + _appearDelayTime * 1000);


            if (go)
            {
                var it = go.GetComponent<iTween>();

                while (go.IsActive() && it && it.isActiveAndEnabled && it.isRunning)
                {

                    yield return 50;

                }
            }
        }

        private void __afterClose()
        {
            _isPlayHideEffect = false;
            if (!this) return;
            if (gameObject)
            {
                if (!string.IsNullOrEmpty(_hide_complete_sound_id)) SendMessageUpwards("__OnPlaySound", _hide_complete_sound_id);

                if (gameObject.GetComponent<iTween>())
                {
                    GameObject.Destroy(gameObject.GetComponent<iTween>());
                }
                //gameObject.GetRectTransform().anchoredPosition = initPos;
                //gameObject.GetRectTransform().localScale = initScale;
                //gameObject.SetActive(false);
            }

        }

        private IEnumerator _playHideEffectAsync()
        {
            _isPlayHideEffect = true;


            MyTask.Run(_playHideStartSoundAsync());

            if (!gameObject) yield break;
            Canvas rootCanvas = gameObject.GetComponent<Canvas>();
            if (!rootCanvas)
            {
                var list = MyListPool<Canvas>.Get();
                gameObject.GetComponentsInParent(false, list);
                if (list.Count > 0)
                {
                    rootCanvas = list[0];
                    MyListPool<Canvas>.Release(list);
                }
            }


            var go = gameObject;
            GameObject parentObj = null;
            var oldPos = go.GetRectTransform().localPosition;
            var oldScale = initScale;
            var oldsize = initSize;

            if (go.GetComponent<iTween>())
            {
                GameObject.Destroy(go.GetComponent<iTween>());
            }

            int width, height;

            width = Screen.width;
            height = Screen.height;

            if (!_hideEffect)
            {
                __afterClose();

                yield break;

            }

            if (_hide_local_move)
            {
                if (go.transform.parent)
                {
                    parentObj = go.transform.parent.gameObject;
                    var rect = parentObj.GetRectTransform().GetSize();
                    width = Convert.ToInt32(rect.x);
                    height = Convert.ToInt32(rect.y);
                }
            }


            MyTask.SetLeave(__afterClose);

            go.SetActive(true);
            var rt = go.GetRectTransform();

            Vector3 dstPos = Vector3.zero;
            if (!_useUserAppearPos && !_appear_local_move)
            {
                switch (_hideDst)
                {
                    case FadeInOutDir.right:
                        dstPos = new Vector3(oldsize.x * _hideScaleRatio / 2f + width / 2, height / 2 - (height * _hideDstRatio), 0);
                        break;
                    case FadeInOutDir.bottom:
                        dstPos = new Vector3(width * _hideDstRatio - width / 2, -((oldsize.y * _hideScaleRatio / 2f) + height / 2), 0);
                        break;
                    case FadeInOutDir.left:
                        dstPos = new Vector3(-((oldsize.x * _hideScaleRatio / 2f) + width / 2), height / 2 - (height * _hideDstRatio), 0);
                        break;
                    case FadeInOutDir.top:
                        dstPos = new Vector3(width * _hideDstRatio - width / 2, (oldsize.y * _hideScaleRatio / 2f) + height / 2, 0);
                        break;
                }
            }
            else if (!_useUserAppearPos && _appear_local_move)
            {
                switch (_appearSrc)
                {
                    case FadeInOutDir.right:
                        rt.localPosition = new Vector3(width, -1 * height + height * _appearSrcRatio, 0);
                        break;
                    case FadeInOutDir.bottom:
                        rt.localPosition = new Vector3(width * _appearSrcRatio, -(height), 0);
                        break;
                    case FadeInOutDir.left:
                        rt.localPosition = new Vector3(-(width), -1 * height + height * _appearSrcRatio, 0);
                        break;
                    case FadeInOutDir.top:
                        rt.localPosition = new Vector3(width * _appearSrcRatio, height, 0);
                        break;
                }

                rt.localScale = new Vector3(_appearScaleRatio, _appearScaleRatio, 1f);
            }
            else
            {
                Vector2 localPos;
                if (rootCanvas && RectTransformUtility.ScreenPointToLocalPointInRectangle(rootCanvas.transform as RectTransform, _appearPos, rootCanvas.worldCamera, out localPos))
                {
                    dstPos = new Vector3(localPos.x, localPos.y, 0);
                }
                else
                {
                    dstPos = new Vector3(_appearPos.x - Screen.width / 2, -(Screen.height / 2 - _appearPos.y), 0);
                }

            }

            Hashtable p = new Hashtable();
            p["x"] = dstPos.x;
            p["y"] = dstPos.y;
            p["z"] = dstPos.z;
            p["islocal"] = true;
            if (_hideEaseType != iTween.EaseType.none)
                p["easetype"] = _hideEaseType;
            p["looptype"] = "none";
            p["time"] = _hideTimespace;
            if (_hideDelayTime > 0f) p["delay"] = _hideDelayTime;
            iTween.MoveTo(go, p);

            Hashtable p2 = new Hashtable();
            p2["x"] = _useUserAppearPos ? 0 : _hideScaleRatio;
            p2["y"] = _useUserAppearPos ? 0 : _hideScaleRatio;
            p2["z"] = 1d;
            p2["islocal"] = true;
            if (_hideEaseType != iTween.EaseType.none)
                p2["easetype"] = _hideEaseType;
            p2["looptype"] = "none";
            p2["time"] = _hideTimespace;
            if (_hideDelayTime > 0f) p2["delay"] = _hideDelayTime;
            iTween.ScaleTo(go, p2);


            yield return (int)(_hideTimespace * 1000);

            if (go)
            {

                var it = go.GetComponent<iTween>();
                while (go.IsActive() && it && it.isActiveAndEnabled && it.isRunning)
                {

                    yield return 50;

                }
            }
        }

        private IEnumerator _playFadeEffectAsync()
        {
            if (!_use_fade_tween)
                yield break;

            _isPlayShowEffect = true;


            var graphci = gameObject.GetComponent<Graphic>();

            if(!graphci)
            {
                var group = gameObject.AddMissingComponent<CanvasGroup>();

                group.alpha = 0;
            }
            else
            {
                graphci.color = new Color(graphci.color.r, graphci.color.g, graphci.color.b, 0);
            }

            MyTask.SetLeave(__afterFade);

            var go = gameObject;
            //Hashtable p = new Hashtable();
            //p["easetype"] = _appearEaseType;
            //p["looptype"] = "none";
            //p["delay"] = _fate_delay;
            //p["time"] = _fate_time;
            //p["alpha"] = 1;

            TweenAlpha.Begin(go,0,1,_fate_time,_fate_delay);

            //iTween.FadeTo(go, p);

            yield return (int)(_fate_time * 1000 + _fate_delay * 1000);
        }

        private IEnumerator _playTweenPath()
        {
            if (!_use_path_move)
                yield break;

            var go = gameObject;

            var path = new List<Transform>();

            for (int i = 0; i < _path_transforms.childCount; i++)
            {
                var item = _path_transforms.GetChild(i);
                path.Add(item);
            }

            MyTask.SetLeave(()=> {
                TweenPathAfterShow();
            });

            go.transform.position = path[0].position;

            Hashtable p = new Hashtable();
            p["path"] = path.ToArray();
            p["islocal"] = false;
            if (_appearEaseType != iTween.EaseType.none)
                p["easetype"] = _appearEaseType;
            p["looptype"] = "none";
            p["time"] = _path_move_time;
            //if (_hideDelayTime > 0f) p["delay"] = _hideDelayTime;
            iTween.MoveTo(go, p);

            yield return 50;

            yield return (int)(_path_move_time * 1000);

        }
    }
}
