
using UnityEngine.UI;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace UnityEditor.UI
{
    /// <summary>
    /// 显示图集中的图片,点击图片可回调图片名
    /// </summary>
    public class SpriteSelector : ScriptableWizard
    {
        static public SpriteSelector instance;

        void OnEnable() { instance = this; }
        void OnDisable() { instance = null; }

        public delegate void Callback(string sprite,object param);

        SerializedObject mObject;
        SerializedProperty mProperty;

        MySpriteImage mSprite;
        bool need_scroll = true;
        Vector2 mPos = Vector2.zero;
        Callback mCallback;
        object mCallbackParam = null;
        float mClickTime = 0f;

        /// <summary>
        /// Draw the custom wizard.
        /// </summary>

        void OnGUI()
        {
            EditorGUIUtility.labelWidth = 80f;

            if (MySpritePackerTools.myapcker == null)
            {
                GUILayout.Label("当前没有选中图集", "LODLevelNotifyText");
            }
            else
            {
                MySpritePacker atlas = MySpritePackerTools.myapcker;
                bool close = false;
                GUILayout.Label("图集[" + atlas.name + "]的图片", "LODLevelNotifyText");
                MySpritePackerTools.DrawSeparator();

                // 顶部搜索栏
                GUILayout.BeginHorizontal();
                GUILayout.Space(12f);
                GUIStyle style = "SearchTextField";
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.red;
                MySpritePackerTools.partialSprite = EditorGUILayout.TextField(" 搜索 ", MySpritePackerTools.partialSprite, style);
                //
                if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
                {
                    MySpritePackerTools.partialSprite = "";
                    GUIUtility.keyboardControl = 0;
                }
                GUILayout.Space(60f);
                GUILayout.EndHorizontal();

                Texture2D tex = atlas.PackerImage as Texture2D;

                if (tex == null)
                {
                    GUILayout.Label("The atlas doesn't have a texture to work with");
                    return;
                }
               

                float size = 80f;
                float padded = size + 10f;
                int columns = Mathf.FloorToInt(Screen.width / padded) - 4;
                if (columns < 1) columns = 1;

                int offset = 0;
                Rect rect = new Rect(10f, 0, size, size);

                GUILayout.Space(10f);
                mPos = GUILayout.BeginScrollView(mPos);
                //Log.LogInfo($"mPos={mPos}");

                int rows = 1;

                var _list = atlas.uvList;
                
                while (offset < _list.Length)
                {
                    GUILayout.BeginHorizontal();
                    {
                        int col = 0;
                        rect.x = 10f;

                        for (; offset < _list.Length; ++offset)
                        {
                            var spInfo = _list[offset];
                            if (spInfo == null) continue;

                            if (!string.IsNullOrEmpty(MySpritePackerTools.partialSprite) && !spInfo.name.Contains(MySpritePackerTools.partialSprite)) 
                            {
                                continue;
                            }

                            // Button comes first
                            if (GUI.Button(rect, ""))
                            {
                                if (Event.current.button == 0)
                                {
                                    float delta = Time.realtimeSinceStartup - mClickTime;
                                    mClickTime = Time.realtimeSinceStartup;                                   

                                    if (MySpritePackerTools.selectedSprite != spInfo.name)
                                    {
                                        if (mSprite != null)
                                        {
                                            MySpritePackerTools.RegisterUndo("Atlas Selection", mSprite);
                                            //mSprite.MakePixelPerfect();
                                            EditorUtility.SetDirty(mSprite.gameObject);
                                        }

                                        MySpritePackerTools.selectedSprite = spInfo.name;
                                        RepaintSprites();
                                        mCallback?.Invoke(spInfo.name,mCallbackParam);

                                        //检查图集是否是最新
                                        var texPath = AssetDatabase.GetAssetPath(tex);
                                        if (!string.IsNullOrEmpty(texPath))
                                        {
                                            var datetime = File.GetLastWriteTimeUtc(texPath);
                                            var texPath2 = PathDefs.ASSETS_PATH_GUI_SPRITES + tex.name + "/" + spInfo.name + ".png";
                                            if (File.GetLastWriteTimeUtc(texPath2).CompareTo(datetime) >= 0 || File.GetLastWriteTimeUtc(texPath2 + ".meta").CompareTo(datetime) >= 0)
                                            {
                                                Log.LogError($"图集[{tex.name}] 不是最新的！请右击[{texPath2}] 更新图集！");
                                            }
                                            var tex2 = AssetDatabase.LoadAssetAtPath<Object>(texPath2);
                                            EditorGUIUtility.PingObject(tex2);
                                        }
                                    }
                                    else if (delta < 0.5f) close = true;
                                }                                
                            }

                            if (Event.current.type == EventType.Repaint)
                            {
                                // On top of the button we have a checkboard grid
                                DrawTiledTexture(rect, backdropTexture);
                                Rect uv = spInfo.rect;
                                //uv = NGUIMath.ConvertToTexCoords(uv, tex.width, tex.height);

                                // Calculate the texture's scale that's needed to display the sprite in the clipped area
                                float scaleX = rect.width / uv.width;
                                float scaleY = rect.height / uv.height;

                                // Stretch the sprite so that it will appear proper
                                float aspect = (scaleY / scaleX) / ((float)tex.height / tex.width);
                                Rect clipRect = rect;

                                if (aspect != 1f)
                                {
                                    if (aspect < 1f)
                                    {
                                        // The sprite is taller than it is wider
                                        float padding = size * (1f - aspect) * 0.5f;
                                        clipRect.xMin += padding;
                                        clipRect.xMax -= padding;
                                    }
                                    else
                                    {
                                        // The sprite is wider than it is taller
                                        float padding = size * (1f - 1f / aspect) * 0.5f;
                                        clipRect.yMin += padding;
                                        clipRect.yMax -= padding;
                                    }
                                }

                                GUI.DrawTextureWithTexCoords(clipRect, tex, uv);

                                // Draw the selection
                                if (MySpritePackerTools.selectedSprite == spInfo.name)
                                {
                                    DrawOutline(rect, new Color(0.4f, 1f, 0f, 1f));
                                    if (need_scroll)
                                    {
                                        need_scroll = false;
                                        mPos = new Vector2(0, rect.position.y - Screen.height / 2);
                                    }
                                }
                            }

                            GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
                            GUI.contentColor = new Color(1f, 1f, 1f, 0.7f);
                            GUI.Label(new Rect(rect.x, rect.y + rect.height, rect.width, 32f), spInfo.name, "ProgressBarBack");
                            GUI.contentColor = Color.white;
                            GUI.backgroundColor = Color.white;

                            if (++col >= columns)
                            {
                                ++offset;
                                break;
                            }
                            rect.x += padded;
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(padded);
                    rect.y += padded + 26;
                    ++rows;
                }
                GUILayout.Space(rows * 26);
                GUILayout.EndScrollView();

                if (close) Close();
            }
        }


        /// <summary>
        /// Property-based selection result.
        /// </summary>

        void OnSpriteSelection(string sp,object param)
        {
            if (mObject != null && mProperty != null)
            {
                mObject.Update();
                mProperty.stringValue = sp;
                mObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Show the sprite selection wizard.
        /// </summary>

        static public void ShowSelected()
        {
            if (MySpritePackerTools.myapcker != null)
            {
                Show(delegate (string sel,object param)
                {
                    MySpritePackerTools.SelectSprite(sel);
                },null
                );
            }
        }

        /// <summary>
        /// Show the sprite selection wizard.
        /// </summary>

        static public void Show(SerializedObject ob, SerializedProperty pro, MySpritePacker atlas)
        {
            if (instance != null)
            {
                instance.Close();
                instance = null;
            }

            if (ob != null && pro != null && atlas != null)
            {
                SpriteSelector comp = ScriptableWizard.DisplayWizard<SpriteSelector>("Select a Sprite");
                MySpritePackerTools.myapcker = atlas;
                MySpritePackerTools.selectedSprite = pro.hasMultipleDifferentValues ? null : pro.stringValue;
                comp.mSprite = null;
                comp.mObject = ob;
                comp.mProperty = pro;
                comp.mCallback = comp.OnSpriteSelection;
            }
        }

        /// <summary>
        /// Show the selection wizard.
        /// </summary>

        static public void Show(Callback callback,object param)
        {
            if (instance != null)
            {
                instance.Close();
                instance = null;
            }

            SpriteSelector comp = ScriptableWizard.DisplayWizard<SpriteSelector>("选择图集中的图片");
            comp.mSprite = null;
            comp.mCallback = callback;
            comp.mCallbackParam = param;
        }

        static public void RepaintSprites()
        {           
            if (SpriteSelector.instance != null)
                SpriteSelector.instance.Repaint();
        }

        static Texture2D mBackdropTex;
        static public Texture2D backdropTexture
        {
            get
            {
                if (mBackdropTex == null) mBackdropTex = CreateCheckerTex(
                    new Color(0.1f, 0.1f, 0.1f, 0.5f),
                    new Color(0.2f, 0.2f, 0.2f, 0.5f));
                return mBackdropTex;
            }
        }

        static public Texture2D blankTexture
        {
            get
            {
                return EditorGUIUtility.whiteTexture;
            }
        }

        static Texture2D CreateCheckerTex(Color c0, Color c1)
        {
            Texture2D tex = new Texture2D(16, 16);
            tex.name = "[Generated] Checker Texture";
            tex.hideFlags = HideFlags.DontSave;

            for (int y = 0; y < 8; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c1);
            for (int y = 8; y < 16; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c0);
            for (int y = 0; y < 8; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c0);
            for (int y = 8; y < 16; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c1);

            tex.Apply();
            tex.filterMode = FilterMode.Point;
            return tex;
        }
        static public void DrawTiledTexture(Rect rect, Texture tex)
        {
            GUI.BeginGroup(rect);
            {
                int width = Mathf.RoundToInt(rect.width);
                int height = Mathf.RoundToInt(rect.height);

                for (int y = 0; y < height; y += tex.height)
                {
                    for (int x = 0; x < width; x += tex.width)
                    {
                        GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
                    }
                }
            }
            GUI.EndGroup();
        }

        static public void DrawOutline(Rect rect, Color color)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Texture2D tex = blankTexture;
                GUI.color = color;
                GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), tex);
                GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1f, rect.height), tex);
                GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
                GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
                GUI.color = Color.white;
            }
        }
    }

}