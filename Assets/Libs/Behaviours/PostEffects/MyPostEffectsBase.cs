using UnityEngine;

namespace MyEffect
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Camera Effects/My Post Effect Base")]
    public class MyPostEffectsBase : MonoBehaviour
    {

        #region 后处理对象
        HeatDistortion _heat;   //扭曲
        DepthField _depth;      //景深
        Bloom _bloom;           //溢出
       // ColorSuite _color;      //校色
        Motion _motion;         //运动

        public void OnComponentChange()
        {
            if (!_heat) _heat = GetComponent<HeatDistortion>();
            if (!_depth) _depth = GetComponent<DepthField>();
            if (!_bloom) _bloom = GetComponent<Bloom>();
           // if (!_color) _color = GetComponent<ColorSuite>();
            if (!_motion) _motion = GetComponent<Motion>();
        }
   

        #endregion




        #region unity functions



        void OnEnable()
        {
            OnComponentChange();
        }

        void Start()
        {
        }

//        void Update()
//        {
//
//        }


        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            RenderTexture _lastResult = source;

            if (_bloom && _bloom.enabled)
            {
                var _result = _bloom._renderImage(_lastResult);
                if (_result != null)
                    _lastResult = _result;
            }

            if(_depth && _depth.enabled)
            {
                var _result = _depth._renderImage(_lastResult);
                if (_result != null)
                    _lastResult = _result;
            }

            if(_motion && _motion.enabled)
            {
                var _result = _motion._renderImage(_lastResult);
                if (_result != null)
                    _lastResult = _result;
            }

            //if(_color && _color.enabled)
            //{
            //    var _result = _color._renderImage(_lastResult);
            //    if (_result != null)
            //        _lastResult = _result;
            //}

            if(_heat && _heat.enabled)
            {
                var _result = _heat._renderImage(_lastResult);
                if (_result != null)
                    _lastResult = _result;
            }

            if(_lastResult != null)
            {
                Graphics.Blit(_lastResult, destination);
            }
            else
            {
                Graphics.Blit(source, destination);
            }


            if (_bloom && _bloom.enabled)
            {
                _bloom._afterRender();
            }

            if (_depth && _depth.enabled)
            {
                _depth._afterRender();
            }

            if (_motion && _motion.enabled)
            {
                _motion._afterRender();
            }

            //if (_color && _color.enabled)
            //{
            //    _color._afterRender();
            //}

            if (_heat && _heat.enabled)
            {
                _heat._afterRender();
            }
        }


        #endregion


    }
}