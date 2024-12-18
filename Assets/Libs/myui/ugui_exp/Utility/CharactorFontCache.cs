using System;
using System.Collections.Generic;
using System.Text;


namespace UnityEngine.UI
{
    
    public static class CharactorFontCache
    {
        public static string AllCharactors = "";
        public static bool _content_update = false;

        public static StringBuilder sb = new StringBuilder("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_+=~`[]{}|\\:;\"'<>,.?/ ");
        public static void UpdateTextString(string txt)
        {           
            sb.Append(txt);
            AllCharactors = sb.ToString();
            _content_update = true;
        }

        public static void OnUpdate()
        {
            if (_content_update)
            {
                MyUITools.DefaultFont.RequestCharactersInTexture(AllCharactors);
            }
            _content_update = false;
        }
    }

   
}
