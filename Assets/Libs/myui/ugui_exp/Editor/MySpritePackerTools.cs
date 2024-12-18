using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    public static class MySpritePackerTools
    {
        public static MySpritePacker myapcker;
        static public string selectedSprite;

        static string mEditedName = null;
        static string mLastSprite = null;

        public static string partialSprite;

        static Texture2D mBackdropTex;
        static Texture2D mContrastTex;
        static Texture2D mGradientTex;
        static GameObject mPrevious;

        static float clicktime;

        static public void DrawAdvancedSpriteField(MySpritePacker atlas, string spriteName, SpriteSelector.Callback callback, object callbackParam, bool editable, params GUILayoutOption[] options)
        {
            if (atlas == null) { EditorGUILayout.HelpBox("请先选择图集", MessageType.Warning); return; }

            if (atlas.uvList.Length == 0)
            {
                EditorGUILayout.HelpBox("此图集没有图片", MessageType.Warning);
                return;
            }


            // Sprite selection drop-down list
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(18f);
                if (GUILayout.Button("图片名称", "DropDown", GUILayout.Width(76f)))
                {
                    MySpritePackerTools.myapcker = atlas;
                    MySpritePackerTools.selectedSprite = spriteName;
                    SpriteSelector.Show(callback, callbackParam);
                }

                if (editable)
                {
                    Log.LogError("不支持编辑");
                }
                //else
                {
                    GUILayout.BeginHorizontal();
                    //GUILayout.Space(18f);
                    //GUILayout.Label(spriteName, "HelpBox", GUILayout.Height(18f));
                    GUIStyle box = "HelpBox";
                    if (GUILayout.Button(spriteName, box, GUILayout.Width(Mathf.Min(10 + spriteName.Length * 10, 160))))
                    {
                        if (!Application.isPlaying && spriteName.Length > 0)
                        {
                            var image = PathDefs.ASSETS_PATH_GUI_SPRITES + atlas.name + "/" + spriteName + ".png";
                            var tex = AssetDatabase.LoadAssetAtPath<Object>(image);
                            if (!tex)
                            {
                                if (atlas.GetUV(spriteName) != null)
                                {
                                    Log.LogError($"找不到图集资源：{image}");
                                }
                            }
                            else
                            {
                                var cd = Time.time - clicktime;
                                //Log.LogInfo($"cd={cd}");
                                if (cd < 0.2f)
                                {
                                    callback.Invoke("", callbackParam);
                                }
                                else 
                                {
                                    EditorGUIUtility.PingObject(tex);
                                }
                                clicktime = Time.time;
                            }
                        }
                    }

                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndHorizontal();
        }


        static public void RegisterUndo(string name, params Object[] objects)
        {
            if (objects != null && objects.Length > 0)
            {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
			UnityEditor.Undo.RegisterUndo(objects, name);
#else
                UnityEditor.Undo.RecordObjects(objects, name);
#endif
                foreach (Object obj in objects)
                {
                    if (obj == null) continue;
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        static public List<T> FindAll<T>() where T : Component
        {
            T[] comps = Resources.FindObjectsOfTypeAll(typeof(T)) as T[];

            List<T> list = new List<T>();

            foreach (T comp in comps)
            {
                if (comp.gameObject.hideFlags == 0)
                {
                    string path = AssetDatabase.GetAssetPath(comp.gameObject);
                    if (string.IsNullOrEmpty(path)) list.Add(comp);
                }
            }
            return list;
        }

        static public void Select(GameObject go)
        {
            mPrevious = Selection.activeGameObject;
            Selection.activeGameObject = go;
        }

        static public void SelectSprite(string spriteName)
        {
            if (myapcker != null)
            {
                selectedSprite = spriteName;
                Select(myapcker.gameObject);
                SpriteSelector. RepaintSprites();
            }
        }
        static public Texture2D blankTexture
        {
            get
            {
                return EditorGUIUtility.whiteTexture;
            }
        }

        static public void DrawSeparator()
        {
            GUILayout.Space(12f);

            if (Event.current.type == EventType.Repaint)
            {
                Texture2D tex = blankTexture;
                Rect rect = GUILayoutUtility.GetLastRect();
                GUI.color = new Color(0f, 0f, 0f, 0.25f);
                GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
                GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
                GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
                GUI.color = Color.white;
            }
        }
    }


}
