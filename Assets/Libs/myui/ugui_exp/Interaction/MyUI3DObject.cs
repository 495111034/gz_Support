using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.UI
{
    public enum UIEffectLayer
    {
        UI = 0,
        UIEffect = 1,
    }

    [AddComponentMenu("UI/MyUI3DObject", 6)]
    public class MyUI3DObject: MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        protected string prefabname;

        //[HideInInspector]
        //[SerializeField]
        //protected int renderQueue;

       // [HideInInspector]
       // [SerializeField]
       // protected UIEffectLayer layerType;

        bool _isInit = false;

        public string PrefabName { get { return prefabname; } set { prefabname = value; _isInit = false;  } }

       // public int RenderQueue { get { return renderQueue; } set { renderQueue = value; _isInit = false; } }
      //  public UIEffectLayer LayerType { get { return layerType; }set { layerType = value;_isInit = false; } }

        void Start()
        {
          //  if (!_isInit) CreateObj();
        }

        void Update()
        {
            if (!_isInit)
            {
                CreateObj();
            }
        }

        void CreateObj()
        {            
            SendMessageUpwards("__3dObjOnInit", this);
            _isInit = true;
        }

        public void OnLoadComplete()
        {
            SendMessageUpwards("__3dObjLoadComplete", this);
        }
    }
}
