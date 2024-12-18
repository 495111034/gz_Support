using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{    

    public interface IMySpritePacker 
    {
        string PackerName { get; }
        MySpritePacker iPacker { get; }
#if UNITY_EDITOR
        void Editor_FixPackerName();
#endif

        void SetSpritePacker(MySpritePacker sp_packer, object dept);
    }

    public interface IMyTexture
    {
        string TextureName { get; }
        Texture iTexture { get; }
#if UNITY_EDITOR
        void Editor_FixTextureName();
#endif
        void SetTexture(Texture tex, object dept);
    }

    public interface IMySprite
    {
        string PackerName { get; }
        string PackerSpriteName { get; }
        string SpriteName { get; }
        Sprite iSprite { get; }

#if UNITY_EDITOR
        void Editor_FixPackerNameSpriteName();
#endif

        void SetSprite(Sprite sprite, object dept);
        void SetSprite(MySpritePacker sp_packer, string sp_name, object dept);
    }

}
