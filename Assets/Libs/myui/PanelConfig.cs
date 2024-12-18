using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{

    public enum PanelType
    {
        MainPanel = 0,      //主界面
        ActivePanel = 1,    //一般界面
        DynamicPanel = 2,   //动态界面
        TipsPanel = 3,      //提示界面
        TalkPanel = 4,      //对话界面
        JuQingPanel = 5,    //剧情界面
    }

    public enum ActivePanelType
    {
        Type2D = 0,         //2D界面
        Type3D = 1         //3D界面
    }

    [AddComponentMenu("UI/Panel Config")]
    public class PanelConfig : MonoBehaviour
    {

        /// <summary>
        /// 是否隐藏主相机
        /// </summary>
        public bool hide_scene { get { return __hide_scene; } set { __hide_scene = value; } }
        /// <summary>
        /// 隐藏主相机后是否截屏作为背景
        /// </summary>
        public bool ShowSceneBackground { get { return __show_scene_background; }set { __show_scene_background = value; } }
        /// <summary>
        /// 隐藏主相机后截屏背景的颜色
        /// </summary>
        public Color BackGroundColor { get { return __background_color; }set { __background_color = value; } }
        /// <summary>
        /// 切换场景时是否关闭界面
        /// </summary>
        public bool hide_when_leave_scene { get { return __hide_when_leave_scene; } set { __hide_when_leave_scene = value; } }

        public bool reshow_when_entry_scene { get { return __reshow_when_entry_scene; } set { __reshow_when_entry_scene = value; } }
        /// <summary>
        /// 点击其它界面时是否关闭界面
        /// </summary>
        public bool hide_when_click_other { get { return __hide_when_click_other; } set { __hide_when_click_other = value; } }

        /// <summary>
        /// 显示上方工具条
        /// </summary>
        public bool show_comm_topbar { get { return __show_comm_topbar; } set { __show_comm_topbar = value; } }


        /// <summary>
        /// 显示上方工具条
        /// </summary>
        public bool show_comm_topbar2 { get { return __show_comm_topbar2; } set { __show_comm_topbar2 = value; } }

        /// <summary>
        /// 显示左边拖动条
        /// </summary>
        public bool show_comm_leftscroll { get { return __show_comm_leftscroll; } set { __show_comm_leftscroll = value; } }

        /// <summary>
        /// 使用第二套通用资源
        /// </summary>
        public bool use_secsprite { get { return __use_secsprite; } set { __use_secsprite = value; } }

        public bool auto_play_effect { get { return __auto_play_effect; } set { __auto_play_effect = value; } }

        public string[] depend_prefab_res { get { return __depend_prefab_res; } set { __depend_prefab_res = value; } }
        public string[] depend_picture_res { get { return __depend_picture_res; } set { __depend_picture_res = value; } }

        public int panel_level { get { return __panel_level; } set { __panel_level = value; } }
        public int Reject_other_panel_level { get { return __Reject_other_panel_level; } set { __Reject_other_panel_level = value; } }
        public int scroll_view_config_id { get { return __scroll_view_config_id; } set { __scroll_view_config_id = value; } }
        public string top_lang_id { get { return __top_lang_id; } set { __top_lang_id = value; } }

        public PanelType panelType { get { return __panelType; } set { __panelType = value; } }

        public ActivePanelType activePanelType { get { return __activePanelType; } set { __activePanelType = value; } }


        [HideInInspector]
        [SerializeField]
        protected bool __hide_scene = false;

        [HideInInspector]
        [SerializeField]
        protected bool __show_scene_background = true;

        [HideInInspector]
        [SerializeField]
        protected Color __background_color = Color.white;

        [HideInInspector]
        [SerializeField]
        protected bool __hide_when_leave_scene = false;

        [HideInInspector]
        [SerializeField]
        protected bool __reshow_when_entry_scene = false;

        [HideInInspector]
        [SerializeField]
        protected bool __hide_when_click_other = false;

        [HideInInspector]
        [SerializeField]
        protected bool __show_comm_topbar = false;

        [HideInInspector]
        [SerializeField]
        protected bool __show_comm_topbar2 = false;
        

        [HideInInspector]
        [SerializeField]
        protected bool __show_comm_leftscroll = false;

        [HideInInspector]
        [SerializeField]
        protected bool __auto_play_effect = true;

        [HideInInspector]
        [SerializeField]
        protected string[] __depend_prefab_res;

        [HideInInspector]
        [SerializeField]
        protected string[] __depend_picture_res;

        [HideInInspector]
        [SerializeField]
        protected int __panel_level = 0;

        [HideInInspector]
        [SerializeField]
        protected int __Reject_other_panel_level = 0;

        [HideInInspector]
        [SerializeField]
        protected int __scroll_view_config_id = 0;

        [HideInInspector]
        [SerializeField]
        protected string __top_lang_id = "";

        [HideInInspector]
        [SerializeField]
        protected PanelType __panelType = PanelType.ActivePanel;

        [HideInInspector]
        [SerializeField]
        protected bool __use_secsprite = false;

        [HideInInspector]
        [SerializeField]
        protected ActivePanelType __activePanelType = ActivePanelType.Type2D;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
//        void Update()
//        {
//
//        }
    }
}
