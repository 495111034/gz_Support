using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/My Ware Image(摆动的图片)")]
    [ExecuteInEditMode]
    public class MyWareImage : MySpriteImageBase
    {

        #region Editor属性

        [HideInInspector]
        [SerializeField]
        float _wareStrong =4f;
        
        [HideInInspector]
        [SerializeField]
        int _vertexDensity = 400;

        #endregion


        public float wareStrong { get { return _wareStrong; }set { _wareStrong = value; } }
        public int vertexDensity { get { return _vertexDensity; }set { _vertexDensity = value; OnInit(); } }


        private static int shaderid_offsetX = -1;
       // private static int shaderid_WaveStrong = -1;

        private Material _material = null;

        List<Selectable> children = new List<Selectable>();

        public override Material material
        {
            get
            {
                if(!_material)
                {
                    _material = Instantiate(UIGrapAssets.m_ware_ui_mat);
                }
                return _material;
            }
        }

#if UNITY_EDITOR
        public void Update_Editor(float nowTime)
        {
            if (gameObject)
            {
                OnUpdate(nowTime);
            }
        }
#endif


        #region 顶点计算       

       protected override void AddQuad(VertexHelper vertexHelper, MySpritePacker packer, Vector2 posMin, Vector2 posMax, Vector2 uvMin, Vector2 uvMax)
        { 

            //只有一个网络，表现出来的是摆动
            //int currentVertCount = vertexHelper.currentVertCount;
            //vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0f), Color.white, new Vector2(uvMin.x, uvMin.y));
            //vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0f), Color.white, new Vector2(uvMin.x, uvMax.y));
            //vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0f), Color.white, new Vector2(uvMax.x, uvMax.y));
            //vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0f), Color.white, new Vector2(uvMax.x, uvMin.y));
            //vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
            //vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);


            //拆分网格，如果网格足够密集，则会出现波浪形
            {

                float cur_length = 0f;
                float totalLength = posMax.y - posMin.y;
                float step = totalLength > 0 ? _vertexDensity : -_vertexDensity;
                float uvLength = uvMax.y - uvMin.y;

                while (Mathf.Abs(cur_length) < UnityEngine.Mathf.Abs(totalLength))
                {
                    float leftLength = totalLength - cur_length;

                    Vector2 curMinPos = new Vector2(posMin.x, posMin.y + cur_length);
                    Vector2 curMinUV = new Vector2(uvMin.x, uvMin.y + (cur_length / totalLength) * uvLength);
                    cur_length += (Mathf.Abs(leftLength) >= Mathf.Abs(step)) ? step : leftLength;
                    Vector2 curMaxPos = new Vector2(posMax.x, posMin.y + cur_length);
                    Vector2 curMaxUv = new Vector2(uvMax.x, uvMin.y + (cur_length / totalLength) * uvLength);


                    int currentVertCount = vertexHelper.currentVertCount;
                    //四个顶点
                    vertexHelper.AddVert(new Vector3(curMinPos.x, curMinPos.y, 0f), Color.white, new Vector2(curMinUV.x, curMinUV.y));
                    vertexHelper.AddVert(new Vector3(curMinPos.x, curMaxPos.y, 0f), Color.white, new Vector2(curMinUV.x, curMaxUv.y));
                    vertexHelper.AddVert(new Vector3(curMaxPos.x, curMaxPos.y, 0f), Color.white, new Vector2(curMaxUv.x, curMaxUv.y));
                    vertexHelper.AddVert(new Vector3(curMaxPos.x, curMinPos.y, 0f), Color.white, new Vector2(curMaxUv.x, curMinUV.y));

                    //两个三角形
                    vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
                    vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
                }
            }  

        }        

        #endregion

        #region Monobehaviour与运行时

       protected override void OnInit()
        {
            if (!gameObject.activeInHierarchy) return;

            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.anchorMin = new Vector2(0.5f, 1);

            UnityEngine.Profiling.Profiler.BeginSample("MyWareImage.SetAllDirty");
            SetAllDirty();
            UnityEngine.Profiling.Profiler.EndSample();

            InitChildrend();

            if(Application.isPlaying)
                OnUpdate(0);
        }

        public void InitChildrend()
        {
            children.Clear();

            gameObject.FindsInChild<Selectable>(children, "", true);

            //for(int i = 0; i < rectTransform.childCount; ++i)
            //{
            //    var child = rectTransform.GetChild(i);
            //    if(child is RectTransform)
            //    {
            //        children.Add(child as RectTransform);
            //    }
            //}
        }



        void Update()
        {
            OnUpdate(Time.time);
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            OnInit();
#if UNITY_EDITOR
            Update_Editor(0);
#endif
        }

        void OnUpdate(float nowTime)
        {
            var offset_x = Mathf.Sin(Mathf.PI * nowTime * Mathf.Clamp(1 - 0.1f, 0, 1)) * _wareStrong;
            //将参数传入shader，由shader做出当前模型的顶点变形
            if (shaderid_offsetX <= 0)
            {
                shaderid_offsetX = resource.ShaderNameHash.ShaderNameId("_offsetX");
                //shaderid_WaveStrong = Shader.PropertyToID("_WaveStrong");
            }
            
            if (shaderid_offsetX > 0  && _material)
            {
                _material.SetFloat(shaderid_offsetX, offset_x);
               // _material.SetFloat(shaderid_WaveStrong, _wareStrong);
            }

            if(children.Count == 0)
            {
                InitChildrend();
                return;
            }
            //计算挂接点的位置，计算方式必须与shader中保持一至
            for(int i = 0; i < children.Count; ++i )
            {
                if(!children[i])
                {
                    InitChildrend();
                    break;
                }
                var rt = children[i].gameObject.GetRectTransform();
                var localY = rt.localPosition.y;
                var localPosition = rt.position - rectTransform.position;
                var height =  (localPosition.y * -1) / rectTransform.GetSize().y;   //转换为与shader中的单位一至
                localPosition = new Vector3(offset_x * height, localPosition.y, localPosition.z);
                rt.position = rectTransform.position + localPosition;
                rt.localPosition = new Vector3(rt.localPosition.x, localY, rt.localPosition.z);
            }
        }

        protected override void OnDestroy()
        {

            if (_material)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(_material);
                else
#endif
                    Destroy(_material);

                _material = null;
            }
        }

        #endregion
    }
}
