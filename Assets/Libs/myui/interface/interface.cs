using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using System.Text;


namespace UnityEngine
{
    /// <summary>
    /// 还原时用于加载资源
    /// </summary>
    public interface IResourceLoader
    {
        void SetSpritePacker(IMySpritePacker comp, string packer_name, bool rsync = false);
        void SetSprite(IMySprite comp, string packer_name, string sprite_name, bool rsync = false);
        void SetSprite(IMySprite comp, string sprite_name, bool rsync = false);
        void SetTexture(IMyTexture comp, string tex_name, bool rsync = false);
        void SetSpriteOrTexture(Component comp, string tex_name, bool rsync = false);
        void SetFont(MyText comp, string font_name, bool rsync = false);
        void LoadLang(string txt);
        string LangFromId(string id, object paramList);
        bool HasLangFromId(string id);

        void LoadServerLang(string txt);
        string ServerLangFromId(int id, object paramList);
        string ServerLangFromId(int id);
        int ServerLangTypeFromId(int id);

        void LoadUILang(string txt);
        string UILangFromId(string id);
        //string LangFromId(string id, params string[] paramList);
        //public virtual void ReloadResource(GameObject go) { throw new ArgumentException("cannt use IResourceLoader"); }
        //public virtual MySpritePacker SpritePackerFromID(Component comp, string id) { throw new ArgumentException("cannt use IResourceLoader"); }
        void LateUpdate();
    }    
}

namespace UnityEngine.UI
{
    public interface IUITips
    {
        object TipParams { set; get; }
    }
}

namespace UnityEngine.EventSystems
{
    public interface IPointerLongpressHandler : IEventSystemHandler
    {
        PointerEventData pressingEventData { get; }
        void OnLongPressRepeat(PointerEventData eventData);
    }

    public interface IBlackRangeClickHandler : IEventSystemHandler
    {
        void OnPointerClick(PointerEventData eventData);
    }

    public static class ExecuteEvents2
    {
        private static readonly ExecuteEvents.EventFunction<IPointerLongpressHandler> s_PointerLongpressHandler = Execute;
        private static readonly ExecuteEvents.EventFunction<IBlackRangeClickHandler> s_BlankRangeClickHandler = Execute;

        public static T ValidateEventData<T>(BaseEventData data) where T : class
        {
            if ((data as T) == null)
                throw new ArgumentException(String.Format("Invalid type: {0} passed to event expecting {1}", data.GetType(), typeof(T)));
            return data as T;
        }

        private static void Execute(IPointerLongpressHandler handler, BaseEventData eventData)
        {
            handler.OnLongPressRepeat(ValidateEventData<PointerEventData>(eventData));
        }

        private static void Execute(IBlackRangeClickHandler handler, BaseEventData eventData)
        {
            handler.OnPointerClick(ValidateEventData<PointerEventData>(eventData));
        }

        public static ExecuteEvents.EventFunction<IPointerLongpressHandler> pointerLongpressHandler
        {
            get { return s_PointerLongpressHandler; }
        }

        public static ExecuteEvents.EventFunction<IBlackRangeClickHandler> pointerBlackRangeClickHandler
        {
            get { return s_BlankRangeClickHandler; }
        }
    }


}
