using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using Object = UnityEngine.Object;


namespace resource
{

    public static class EditPrefabList
    {
#if UNITY_ANDROID
        public const string os_name = "android";
#elif UNITY_IPHONE
        public const string os_name = "ios";
#else
        public const string os_name = "windows";
#endif
        public static string FindFileByAssetID(string fileid,string type = "")
        {
#if UNITY_EDITOR
            if (type == "scene")
            {
                var arr = Directory.GetDirectories("Assets/prefabs/", $"{fileid}.unity", SearchOption.AllDirectories);
                if (arr.Length > 0) return arr[0];
            }
            else if(type == "tex")
            {
                var arr = Directory.GetDirectories("Assets/prefabs/", $"{fileid}.png|{fileid}.jpg", SearchOption.AllDirectories);
                if (arr.Length > 0) return arr[0];
            }
            else if (type == "asset")
            {
                var arr = Directory.GetDirectories("Assets/prefabs/", $"{fileid}.asset", SearchOption.AllDirectories);
                if (arr.Length > 0) return arr[0];
            }
            else if(type == "data")
            {
                var arr = Directory.GetDirectories($"Assets/Res/datas/{os_name}/", $"{fileid}*.byte", SearchOption.AllDirectories);
                if (arr.Length > 0) return arr[0];
            }
            else
            {
                var arr = Directory.GetDirectories("Assets/prefabs/", $"{fileid}.prefab", SearchOption.AllDirectories);
                if (arr.Length > 0) return arr[0];
            }
#endif
            return "";
        }


    }


}

