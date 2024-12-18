using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(MyText), true)]
    [CanEditMultipleObjects]
    public class MyTextEditor : GraphicEditor
    {

        public static SerializedProperty IndexOf(SerializedProperty array, int key, out int index)
        {
            for (int i = 0, len = array.arraySize; i < len; ++i)
            {
                var e = array.GetArrayElementAtIndex(i);
                var k = e.FindPropertyRelative("Key");
                if (k.intValue == key)
                {
                    index = i;
                    return e.FindPropertyRelative("Value");
                }
            }
            index = -1;
            return null;
        }

        public static SerializedProperty Append(SerializedProperty array, int key) 
        {
            array.arraySize += 1;
            var hit = array.GetArrayElementAtIndex(array.arraySize - 1);
            var ite = hit.GetEnumerator();
            ite.MoveNext();
            hit.intValue = key;
            ite.MoveNext();
            hit.stringValue = "";
            return hit;
        }

        SerializedProperty m_maxWidth;
        SerializedProperty m_autoSize;
        SerializedProperty m_SaveToAB;
        SerializedProperty m_textSpace;
        SerializedProperty m_Text_id;
        SerializedProperty m_use_sys_languageFile;
        SerializedProperty m_Text;
        SerializedProperty m_FontData;
        SerializedProperty m_Extras;
        SerializedProperty m_FontData_id;
        SerializedProperty language_params;
        GUIContent m_SaveToABContent, m_textContent, m_fontContent, m_maxWidthContent, m_autosizeContent, cTextSpace;
        GUIContent m_useLanuage, mc_languageID, m_FontData_id_Content;

        List<string> m_LanguageParamList;

        static string[] s_langs = new string[] { "sgp" };

        protected override void OnEnable()
        {
            base.OnEnable();

            m_useLanuage = new GUIContent("使用游戏语言包");
            m_LanguageParamList = new List<string>();

            m_SaveToABContent = new GUIContent("保存到UI语言包");
            m_maxWidthContent = new GUIContent("最大宽度");
            m_autosizeContent = new GUIContent("自动尺寸");
            cTextSpace = new GUIContent("字间距");
            mc_languageID = new GUIContent("语言包ID");
            m_FontData_id_Content = new GUIContent("字体路径");
            m_SaveToAB = serializedObject.FindProperty("m_saveToAB");
            m_maxWidth = serializedObject.FindProperty("m_maxWidth");
            m_autoSize = serializedObject.FindProperty("m_autoSize");
            m_textSpace = serializedObject.FindProperty("_textSpacing");
            m_use_sys_languageFile = serializedObject.FindProperty("__use_language_file");
            m_Text_id = serializedObject.FindProperty("m_Text_id");
            m_Text = serializedObject.FindProperty("m_Text");
            m_FontData = serializedObject.FindProperty("m_FontData");
            m_FontData_id = serializedObject.FindProperty("m_FontData_id");
            language_params = serializedObject.FindProperty("__language_params");
            m_Extras = serializedObject.FindProperty("m_Extras");


        }

        int selectLangIndex = 0;
        public override void OnInspectorGUI()
        {

            base.serializedObject.Update();

            EditorGUILayout.PropertyField(m_autoSize, m_autosizeContent);

            if (m_autoSize.boolValue)
            {
                EditorGUILayout.PropertyField(m_maxWidth, m_maxWidthContent);
            }

            m_textSpace.floatValue = EditorGUILayout.Slider(cTextSpace,m_textSpace.floatValue, 0f, 100f);
            m_use_sys_languageFile.boolValue = EditorGUILayout.Toggle(m_useLanuage,m_use_sys_languageFile.boolValue);

            if (!m_use_sys_languageFile.boolValue)
            {
                EditorGUILayout.PropertyField(m_Text);
                EditorGUILayout.PropertyField(m_SaveToAB, m_SaveToABContent);
            }
            else
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(mc_languageID);
                m_Text_id.stringValue = EditorGUILayout.TextField(m_Text_id.stringValue);
                GUILayout.EndHorizontal();
                if (!string.IsNullOrEmpty(m_Text_id.stringValue))
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("语言包参数数量");
                    language_params.arraySize = EditorGUILayout.DelayedIntField(language_params.arraySize);
                    GUILayout.EndHorizontal();
                    m_LanguageParamList.Clear();
                    ++EditorGUI.indentLevel;
                    for (int i = 0; i < language_params.arraySize; ++i)
                    {
                        language_params.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(language_params.GetArrayElementAtIndex(i).stringValue);
                        m_LanguageParamList.Add(language_params.GetArrayElementAtIndex(i).stringValue);
                    }
                    --EditorGUI.indentLevel;
                    bool isFound = false;
                    var strText = Language.GetString(m_Text_id.stringValue, out isFound, m_LanguageParamList.Count > 0 ? m_LanguageParamList.ToArray() : null);

                    if (!isFound)
                    {
                        m_Text.stringValue = "";
                        EditorGUILayout.HelpBox($"语言包id:{m_Text_id.stringValue}不存在", MessageType.Error);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(strText) && m_Text != null)
                            m_Text.stringValue = strText;
                    }
                }
                else
                {
                    m_Text.stringValue = "";
                    EditorGUILayout.HelpBox("请输入语言包id", MessageType.Error);
                }

                if (GUILayout.Button("刷新语言包", new GUILayoutOption[0]))
                {
                    Language.RefreshLanguage();
                }
            }
            EditorGUILayout.PropertyField(m_FontData);
            var mytext = target as MyText;
            //多语言字号
            if(mytext.resizeTextForBestFit)
            {
                int key = (int)MyKeyValuePairType.FontSizes;
                var fontsize = IndexOf(m_Extras, key, out var index);
                Dictionary<string, int[]> ht = null;
                if (fontsize != null) 
                {
                    ht = Unity.Plastic.Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int[]>>(fontsize.stringValue);
                }

                var click = false;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("设置多语言字号", GUILayout.Width(EditorGUIUtility.labelWidth));
                string[] lables = null;
                {                    
                    if (ht == null)
                    {
                        lables = s_langs;
                    }
                    else 
                    {
                        List<string> list = null;
                        foreach (var l in s_langs) 
                        {
                            if (!ht.ContainsKey(l)) 
                            {
                                list ??= new List<string>();
                                list.Add(l);
                            }
                        }
                        if (list != null) 
                        {
                            lables = list.ToArray();
                        }
                    }
                    if (lables != null)
                    {
                        var select = EditorGUILayout.Popup(this.selectLangIndex, lables, GUILayout.Width(EditorGUIUtility.fieldWidth));
                        click = select != this.selectLangIndex || GUILayout.Button("+", GUILayout.Width(EditorGUIUtility.fieldWidth / 2));
                        this.selectLangIndex = select;
                    }
                }

                GUILayout.EndHorizontal();
                if (fontsize == null && click)
                {
                    fontsize = Append(m_Extras, key);
                }
                if (fontsize != null)
                {
                    ++EditorGUI.indentLevel;
                    ht ??= new Dictionary<string, int[]>();
                    if (click && lables != null)
                    {
                        var lang = lables[this.selectLangIndex];
                        this.selectLangIndex = 0;
                        var mt = target as MyText;
                        ht[lang] = new int[3] { mt.fontSize, mt.resizeTextMinSize, mt.resizeTextMaxSize };
                    }

                    foreach (var kv in ht) 
                    {
                        if (System.Array.IndexOf(s_langs, kv.Key) < 0) 
                        {
                            ht.Remove(kv.Key);
                            break;
                        }
                    }

                    foreach (var lang in s_langs)
                    {
                        if (ht.TryGetValue(lang, out var v3))
                        {
                            GUILayout.BeginHorizontal();
                            
                            if (EditorGUILayout.ToggleLeft(lang, true, GUILayout.Width(EditorGUIUtility.labelWidth * 2 / 3)))
                            //if(!GUILayout.Button("- " + lang, GUILayout.Width(EditorGUIUtility.fieldWidth)))
                            {
                                var vlables = new GUIContent[] { new GUIContent(" Font"), new GUIContent(" Min"), new GUIContent(" Max") };
                                EditorGUI.MultiIntField(EditorGUILayout.GetControlRect(true, 18f), vlables, v3);
                                v3[0] = Mathf.Max(v3[0], 1);
                                v3[1] = Mathf.Max(v3[1], 1);
                                v3[1] = Mathf.Min(v3[1], v3[0]);
                                v3[2] = Mathf.Max(v3[2], v3[0]);
                            }
                            else
                            {
                                ht.Remove(lang);
                                GUILayout.EndHorizontal();
                                break;
                            }
                            GUILayout.EndHorizontal();
                        }
                    }

                    if (ht.Count > 0)
                    {
                        var newstring = Unity.Plastic.Newtonsoft.Json.JsonConvert.SerializeObject(ht);
                        if (newstring != fontsize.stringValue)
                        {
                            fontsize.stringValue = newstring;
                        }
                    }
                    else
                    {
                        //if (string.IsNullOrEmpty(fontsize.stringValue) || UnityEditor.EditorUtility.DisplayDialog("警告", "数据删除后不能恢复，下次打开需要重新设定！", "确定"))
                        {
                            m_Extras.DeleteArrayElementAtIndex(index);
                        }
                    }
                    --EditorGUI.indentLevel;
                }
            }

            //多语言颜色
            if(false)
            {
                int key = (int)MyKeyValuePairType.TextColor;
                var textcolor = IndexOf(m_Extras, key, out var index);
                var b = EditorGUILayout.Toggle("设置多语言颜色", textcolor != null);
                if (textcolor == null)
                {
                    if (b)
                    {
                        Append(m_Extras, key);
                    }
                }
                else
                {
                    if (b)
                    {
                        Color32 color = Color.white;
                        var oldv = textcolor.stringValue;
                        if (!string.IsNullOrEmpty(oldv)) 
                        {
                            try
                            {
                                color = JsonUtility.FromJson<Color32>(oldv);
                            }
                            catch { }
                        }
                        var ncolor = EditorGUILayout.ColorField("", color);
                        if (ncolor != color)
                        {
                            textcolor.stringValue = JsonUtility.ToJson(ncolor);
                        }
                    }
                    else
                    {
                        m_Extras.DeleteArrayElementAtIndex(index);
                    }
                }
            }

            //EditorGUILayout.PropertyField(m_Extras);

            
            //EditorGUILayout.PropertyField(m_FontData_id, m_FontData_id_Content);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();

        }
    }
}
