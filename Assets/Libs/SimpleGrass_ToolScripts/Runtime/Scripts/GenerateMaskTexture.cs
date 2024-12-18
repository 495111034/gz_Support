
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace GameSupport
{
    public enum GenerateMaskType
    {
        MT_None = 0,
        MT_Circle = 1,
    };
    public enum GenerateMaskLayer
    {
        MT_LayerR = 0,
        MT_LayerG = 1,
        MT_LayerB = 2,
        MT_LayerA = 3,
    };

    public class GenerateMaskTask
    {
        public GenerateMaskType maskType = GenerateMaskType.MT_None;        
        public GenerateMaskLayer layerIndex;
        public float value;

        public GenerateMaskTask()
        {
            maskType = GenerateMaskType.MT_None;        
        }        
    }

    public class GenerateMaskTask_Circle :GenerateMaskTask
    {
        public Vector3 center;
        public float radius;
        public float powval;
        // public float minAngle;
        //public float maxAngle;
        public float arcAngle;
        public Vector3 arcDir = Vector3.zero;
        public GenerateMaskTask_Circle(GenerateMaskLayer layer, Vector3 centerPnt, float radiusValue,float powValue=1.0f)
        {
            layerIndex = layer;
            maskType = GenerateMaskType.MT_Circle;
            center = centerPnt;
            radius = radiusValue;
            powval = powValue;
            // minAngle = 0.0f;
            // maxAngle = Mathf.Deg2Rad * 360f;
            arcAngle = Mathf.Deg2Rad * 360f;
            arcDir = Vector3.right;//1,0,0
        }
        public GenerateMaskTask_Circle(GenerateMaskLayer layer, Vector3 centerPnt, float radiusValue,Vector3 dir , float arcAngleValue, float powValue = 1.0f)
        {
            layerIndex = layer;
            maskType = GenerateMaskType.MT_Circle;
            center = centerPnt;
            radius = radiusValue;
            powval = powValue;
            // float centerAngle = Mathf.Rad2Deg * (Mathf.Atan2(dir.z, dir.x));
            //centerAngle = centerAngle < 0 ? 360 + centerAngle : centerAngle;
            // minAngle = Mathf.Deg2Rad * (centerAngle - arcAngle * 0.5f);
            // maxAngle = Mathf.Deg2Rad * (centerAngle + arcAngle * 0.5f);
            arcDir = dir;
            arcAngle = Mathf.Deg2Rad * arcAngleValue;
        }

    }

    public class GenerateMaskScheduler
{
        // 任务队列，使用链表比 List 和 Array 更方便执行插队出队操作（队列中不会出现空位）
        private readonly LinkedList<GenerateMaskTask> _tasks = new LinkedList<GenerateMaskTask>();

        // 最大处理数
        private readonly int _maxNum;
        
        
        public GenerateMaskScheduler(int maxNum)
        {
            _maxNum = maxNum;            
        }

        public void QueueTask(GenerateMaskTask mask)
        {
             _tasks.AddLast(mask);
        }

        public void AppendCircle(GenerateMaskLayer layer, Vector3 center, float radius, float powValue = 1.0f)
        {
            GenerateMaskTask_Circle task = new GenerateMaskTask_Circle(layer,center,radius, powValue);
            _tasks.AddLast(task);
        }

        public void AppendArc(GenerateMaskLayer layer, Vector3 center, float radius, Vector3 dir, float arcAngle, float powValue = 1.0f)
        {
            GenerateMaskTask_Circle task = new GenerateMaskTask_Circle(layer, center, radius,dir,arcAngle, powValue);
            _tasks.AddLast(task);
        }

        // 尝试将已调度的 Task 移出调度队列
        public bool TryDequeue(ref List<GenerateMaskTask> outDequeue)
        {
            outDequeue.Clear();
            for (int i = 0; i < _maxNum; i++)
            {
                if(_tasks.Count == 0)
                {
                    break;
                }
                GenerateMaskTask ritem = _tasks.First.Value;
                outDequeue.Add(ritem);
                _tasks.RemoveFirst();
            }
            return outDequeue.Count > 0;
        }
    }




    public class GenerateMaskTexture : MonoBehaviour
    {
        public static GenerateMaskTexture instance;


        [Header("Settings")]
       // public bool ShouldRender = false;
        public Vector4 center = Vector4.zero;
        public int resolution = 512;
        public int textureSize = 512;
        public Material maskMaterial;
        public RenderTexture _maskRT;
        public RenderTexture _tempRT;

        private bool _qualitySwitch = true;
        private bool _isHardwareSupported = true;
        private Vector3 _lastPlayerPos;
        private bool _isFirst = false;
        private int PID_CenterPos = Shader.PropertyToID("_CenterPos");
       // private int PID_DeltaPos = Shader.PropertyToID("_DeltaPos");
        private int PID_Resolution = Shader.PropertyToID("_Resolution");
        private int PID_LayerSpeed = Shader.PropertyToID("_LayerSpeed");
        private int PID_LayerFadeMin = Shader.PropertyToID("_LayerFadeMin");
        private int PID_LayerOverlying = Shader.PropertyToID("_LayerOverlying");


        private int PID_GenerateMaskTexture = Shader.PropertyToID("_GenerateMaskTexture");
        private int PID_GenerateMask_CenterPos = Shader.PropertyToID("_GenerateMask_CenterPos");
        private int PID_GenerateMask_Resolution = Shader.PropertyToID("_GenerateMask_Resolution");

        private int PID_GenerateMask_Properties0 = Shader.PropertyToID("_GenerateMask_Properties0");
        private int PID_GenerateMask_Properties1 = Shader.PropertyToID("_GenerateMask_Properties1");
        private int PID_GenerateMask_Properties2 = Shader.PropertyToID("_GenerateMask_Properties2");


        public GenerateMaskScheduler generateMaskScheduler = new GenerateMaskScheduler(2);
        public Vector4 layerDuration = new Vector4(-1, -1, -1, -1);
        public Vector4 layerFadeMinValue = Vector4.zero;
        public Vector4 layerOverlying = Vector4.zero;
        private Vector4[] _maskProperties0 = new Vector4[2];
        private Vector4[] _maskProperties1 = new Vector4[2];
        private Vector4[] _maskProperties2 = new Vector4[2];

        private void Awake()
        {
            if (GenerateMaskTexture.instance != this && GenerateMaskTexture.instance != null)
            {
                Debug.LogError("GenerateMaskTexture脚本， 不允许重复存在！！");
            }
            else
            {
                GenerateMaskTexture.instance = this;
            }
        }

        void OnEnable()
        {
            _isHardwareSupported = _IsHardwareSupported();
            _qualitySwitch = _QualitySwitch();
            if (!_qualitySwitch|| !_isHardwareSupported)
            {
                return;
            }

            RenderTextureFormat rtf = RenderTextureFormat.ARGBHalf;
            //RenderTextureFormat rtf = RenderTextureFormat.ARGB32;
            _maskRT = RenderTexture.GetTemporary(textureSize, textureSize, 0, GraphicsFormatUtility.GetGraphicsFormat(rtf, false));
            _maskRT.wrapMode = TextureWrapMode.Clamp;
            _maskRT.filterMode = FilterMode.Bilinear;
            _maskRT.autoGenerateMips = false;
            _maskRT.isPowerOfTwo = true;

            _tempRT = RenderTexture.GetTemporary(_maskRT.descriptor);
            _lastPlayerPos = _GetPlayerPosition();
           
            _isFirst = true;

            //generateMaskScheduler.AppendCircle(GenerateMaskLayer.MT_Layer0, _lastPlayerPos, 5);
            //Vector3 tmp = new Vector3(_lastPlayerPos.x + 10, _lastPlayerPos.y, _lastPlayerPos.z);
            //generateMaskScheduler.AppendCircle(GenerateMaskLayer.MT_Layer1, tmp, 5);
        }

        void OnDisable()
        {
            if (_maskRT != null)
            {
                RenderTexture.ReleaseTemporary(_maskRT);
                _maskRT.DiscardContents();
                _maskRT = null;               
            }

            Shader.SetGlobalTexture(PID_GenerateMaskTexture, Texture2D.blackTexture);
            Shader.SetGlobalVector(PID_GenerateMask_CenterPos, Vector4.zero);
            Shader.SetGlobalFloat(PID_GenerateMask_Resolution, 0.0f);
        }

        private void OnDestroy()
        {
            if(GenerateMaskTexture.instance = this)
            {
                GenerateMaskTexture.instance = null;
            }
        }

        private void Update()
        {
            //画质切换处理
            bool cur_QualitySwitch = _QualitySwitch();
            if (cur_QualitySwitch != _qualitySwitch)
            {
                _qualitySwitch = cur_QualitySwitch;
                if (!cur_QualitySwitch)
                {
                    OnDisable();
                }
                else
                {
                    OnEnable();
                }
            }
            if (!_qualitySwitch)
            {
                return;
            }

            Vector3 curPlayerPos = _GetPlayerPosition();
            if (_isFirst)
            {
                _isFirst = false;

                _lastPlayerPos = curPlayerPos;

                Graphics.Blit(null, _maskRT, maskMaterial, 0);
            }

            for (int i = 0; i < _maskProperties0.Length; i++)
            {
                _maskProperties0[i] = Vector4.zero;
                _maskProperties1[i] = Vector4.zero;
                _maskProperties2[i] = Vector4.zero;
            }
            List<GenerateMaskTask> outDequeue = new List<GenerateMaskTask>();
            bool isGenerateNewMask = false;
            bool isHeroMoved = false;
            if (generateMaskScheduler.TryDequeue(ref outDequeue))
            {
                Vector4 tmp = Vector4.zero;
                for (int i = 0; i < outDequeue.Count; i++)
                {
                    float paramz = 0.0f;
                    float paramw = 0.0f;

                    //Circle
                    if (outDequeue[i].maskType == GenerateMaskType.MT_Circle)
                    {
                        GenerateMaskTask_Circle circle = (outDequeue[i] as GenerateMaskTask_Circle);
                        //_maskProperties1
                        tmp = new Vector4(circle.center.x, circle.center.y, circle.center.z, circle.radius);
                        _maskProperties1[i] = tmp;

                        paramz = circle.powval;

                        //maskProperties2
                        tmp = new Vector4(circle.arcDir.x, circle.arcDir.y, circle.arcDir.z, circle.arcAngle);
                        _maskProperties2[i] = tmp;
                    }
                    //_maskProperties0
                    tmp = new Vector4((float)outDequeue[i].layerIndex, (float)outDequeue[i].maskType, paramz, paramw);
                    _maskProperties0[i] = tmp;
                   
                }
                isGenerateNewMask = true;
            }

            Shader.SetGlobalVectorArray(PID_GenerateMask_Properties0, _maskProperties0);
            Shader.SetGlobalVectorArray(PID_GenerateMask_Properties1, _maskProperties1);
            Shader.SetGlobalVectorArray(PID_GenerateMask_Properties2, _maskProperties2);
            maskMaterial.SetVector(PID_LayerSpeed, new Vector4(1.0f / Mathf.Max(0.00001f, layerDuration.x),
                1.0f / Mathf.Max(0.00001f, layerDuration.y), 1.0f / Mathf.Max(0.00001f, layerDuration.z), 1.0f / Mathf.Max(0.00001f, layerDuration.w)));
            maskMaterial.SetVector(PID_LayerFadeMin, layerFadeMinValue);
            maskMaterial.SetVector(PID_LayerOverlying, layerOverlying);
            maskMaterial.SetFloat(PID_Resolution, 1.0f / resolution);
            maskMaterial.SetVector(PID_CenterPos, center);
            // maskMaterial.SetVector(PID_CenterPos, curPlayerPos);
            //maskMaterial.SetVector(PID_DeltaPos, Vector4.zero);
            //Vector2 offset = Vector2.zero;
            //if (Vector3.Distance(curPlayerPos, _lastPlayerPos) > 0.01f)
            //{
            //    offset = new Vector2((curPlayerPos.x - _lastPlayerPos.x) / resolution, (curPlayerPos.z - _lastPlayerPos.z) / resolution);
            //    maskMaterial.SetVector(PID_DeltaPos, new Vector4( curPlayerPos.x - _lastPlayerPos.x, curPlayerPos.y - _lastPlayerPos.y, curPlayerPos.z - _lastPlayerPos.z, 0.0f));
            //    _lastPlayerPos = curPlayerPos;
            //    isHeroMoved = true;
            //}
            
            if (isGenerateNewMask)
            {
                 Graphics.Blit(_maskRT, _tempRT);
                 Graphics.Blit(_tempRT, _maskRT, maskMaterial, 1);
            }else
            {
                bool isFadeOut = (layerDuration.x >= 0) || (layerDuration.y >= 0) || (layerDuration.z >= 0) || (layerDuration.w >= 0);
                if (isFadeOut)
                {
                    Graphics.Blit(_maskRT, _tempRT);
                    Graphics.Blit(_tempRT, _maskRT, maskMaterial, 2);
                }
            }

            Shader.SetGlobalTexture(PID_GenerateMaskTexture, _maskRT);            
            Shader.SetGlobalVector(PID_GenerateMask_CenterPos, center);//Shader.SetGlobalVector(PID_GenerateMask_CenterPos, new Vector4(_lastPlayerPos.x, _lastPlayerPos.y, _lastPlayerPos.z, 0));
            Shader.SetGlobalFloat(PID_GenerateMask_Resolution, 1.0f / resolution);
        }

        Vector3 _GetPlayerPosition()
        {
            Vector3 heroPos = Vector3.zero;
            SimpleGrass.SimpleGrassGlobal.Global.GetHeroPos(out heroPos);
            return heroPos;
        }

        bool _QualitySwitch()
        {
            int QualityLevel = QualitySettings.GetQualityLevel();
            return QualityLevel >= GameSupport.GameGlobalVars.TractIntract_QualityLevel; //return QualityLevel >= 2;//0低，1中，2高。。。
        }
       
        bool _IsHardwareSupported()
        {
            if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
            {
                return false;
            }
            return true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(new Vector3(center.x, center.y, center.z), new Vector3(resolution / 2.0f, 20f, resolution / 2.0f));
            //if (_maskRT != null)
            //    Gizmos.DrawGUITexture(new Rect(50, 50, 128, 128), _maskRT);
        }
#if UNITY_EDITOR
        private void OnGUI()
        {
            if (_maskRT != null)
                GUI.DrawTexture(new Rect(20, 20, 100, 100), _maskRT,ScaleMode.StretchToFill,false);
        }
#endif
    }


}