using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

#if UNITY_EDITOR
public class ShaderProperty
{
    public string name;
    public int idx;
    public int pid;
    public ShaderUtil.ShaderPropertyType type;
    public TextureDimension texDim;
    public float fvalue;
    public Color colorValue;
    public Vector4 vectorValue;

    public override string ToString()
    {
        return $"name={name},pid={pid},type={type},fvalue={fvalue},colorValue={colorValue},vectorValue={vectorValue},texDim={texDim}";
    }
}

#endif
[CanEditMultipleObjects]
[CustomEditor(typeof(MaterialAnimationBehaviour), true)]
public class MaterialAnimationBehaviourEditor: Editor
{
    SerializedProperty _materialAnimtionList;
    SerializedProperty _binboardType;
    SerializedProperty _disableInstance;

    GUIContent materaalListContent;
    GUIContent _binboardTypeName;
    GUIContent _disableInstanceName;
    GUIContent  txtFlyFrameNumber;
    GUIContent txtFlyFrameTime;
    GUIContent txtKeyFrameName;
    Renderer targetRender;
   

    static List<string> typeNames = new List<string>()
    {
        "未选择类型",
        "Tiling和Offset",
        "颜色",
        "消融进度",
        "凹凸倍数",
        "高度倍数",
        "平移",
        "旋转",
        "缩放",
    };

    static List<string> billbardTypeNames = new List<string>()
    {
        "无",
        "广告牌",
        "水平广告牌",
        "垂直广告牌",
    };


    List<ShaderProperty> PropertyList = new List<ShaderProperty>();

    protected virtual void OnEnable()
    {
        _materialAnimtionList = serializedObject.FindProperty("_materialAnimtionList");
        _binboardType = serializedObject.FindProperty("_binboardType");
        _disableInstance = serializedObject.FindProperty("_disableInstance");
        materaalListContent = new GUIContent("动画列表：");
        _binboardTypeName = new GUIContent("广告牌");
        _disableInstanceName = new GUIContent("禁用硬件实例化");
        txtFlyFrameNumber = new GUIContent("中间关键帧数量");
        txtFlyFrameTime = new GUIContent("时间：");
        txtKeyFrameName = new GUIContent("值：");

        targetRender = (target as MaterialAnimationBehaviour).gameObject.GetComponent<Renderer>();
    }

    bool _firstGo = true;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if(!targetRender)
        {
            EditorGUILayout.HelpBox(new GUIContent("未找到Render组件"));
            goto ERRORHAND;
        }

        if(!targetRender.sharedMaterial)
        {
            EditorGUILayout.HelpBox(new GUIContent("未找到material"));
            goto ERRORHAND;
        }

        if(PropertyList.Count == 0)
        {
            for (int i = 0; i < ShaderUtil.GetPropertyCount(targetRender.sharedMaterial.shader); i++)
            {
                var propertyType = ShaderUtil.GetPropertyType(targetRender.sharedMaterial.shader, i);
                var propertyName = ShaderUtil.GetPropertyName(targetRender.sharedMaterial.shader, i);
                var propertyTexDim = ShaderUtil.GetTexDim(targetRender.sharedMaterial.shader, i);

                PropertyList.Add(new ShaderProperty()
                {
                    name = propertyName,
                    type = propertyType,
                    idx = i,
                    pid = Shader.PropertyToID(propertyName),
                    texDim = propertyTexDim,
                    fvalue = propertyType == ShaderUtil.ShaderPropertyType.Float || propertyType == ShaderUtil.ShaderPropertyType.Range ? targetRender.sharedMaterial.GetFloat(propertyName) : 0,
                    colorValue = propertyType == ShaderUtil.ShaderPropertyType.Color ? targetRender.sharedMaterial.GetColor(propertyName) : Color.white,
                    vectorValue = propertyType == ShaderUtil.ShaderPropertyType.Vector ? targetRender.sharedMaterial.GetVector(propertyName) : Vector4.zero,
                });

                if(propertyType == ShaderUtil.ShaderPropertyType.TexEnv && propertyTexDim == TextureDimension.Tex2D)
                {
                    PropertyList.Add(new ShaderProperty()
                    {
                        name = propertyName + "_ST",
                        type =  ShaderUtil.ShaderPropertyType.Vector,
                        idx = i,
                        pid = Shader.PropertyToID(propertyName + "_ST"),
                        texDim = propertyTexDim,
                        vectorValue = targetRender.sharedMaterial.GetVector(propertyName + "_ST"),
                    });
                }
            }
        }

        if (PropertyList.Count == 0)
        {
            EditorGUILayout.HelpBox(new GUIContent("当前shader未找到属性"));
            goto ERRORHAND;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(_binboardTypeName);
        var binboardTypeId = Mathf.Clamp(_binboardType.enumValueIndex, 0, billbardTypeNames.Count);
        binboardTypeId = EditorGUILayout.Popup(binboardTypeId, billbardTypeNames.ToArray());
        _binboardType.enumValueIndex = binboardTypeId;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(_disableInstance, _disableInstanceName, new GUILayoutOption[0]);
        EditorGUILayout.HelpBox(new GUIContent("有颜色变化时建议选中此项"));


        if (!_materialAnimtionList.isArray) return;

        EditorGUILayout.LabelField(materaalListContent);

        var listCount = _materialAnimtionList.arraySize;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("数量 ：");
        _materialAnimtionList.arraySize = EditorGUILayout.IntField(_materialAnimtionList.arraySize, new GUILayoutOption[0]);
        EditorGUILayout.EndHorizontal();

       

        for (int i = 0; i < _materialAnimtionList.arraySize; ++i)
        {
            EditorGUILayout.LabelField($"{i + 1} ：");
            ++EditorGUI.indentLevel;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("类型 ：");
            //Log.LogError(_materialAnimtionList.GetArrayElementAtIndex(i).FindPropertyRelative("matPropertyType").intValue);
            var itemProperty = _materialAnimtionList.GetArrayElementAtIndex(i);
            int selectType = Mathf.Clamp(itemProperty .FindPropertyRelative("matPropertyType").enumValueIndex, 0, typeNames.Count - 1);
            selectType = EditorGUILayout.Popup(selectType, typeNames.ToArray());
            EditorGUILayout.EndHorizontal();

           

            List<ShaderProperty> propertyList = null;
            int curSelectPid = -1;
            if (selectType > (int)MaterialAnimationType.None && selectType < (int)MaterialAnimationType.Translate)
            {
                propertyList = GetPropertyNamesByType((MaterialAnimationType)selectType);
                if (propertyList.Count == 0)
                {
                    Log.LogError($"{targetRender.sharedMaterial.shader.name}没有{(MaterialAnimationType)selectType}类型的属性");
                    UnityEditor.EditorUtility.DisplayDialog("错误", $"{targetRender.sharedMaterial.shader.name}没有{(MaterialAnimationType)selectType}类型的属性", "重选");                    

                    selectType = 0;

                    continue;
                }

                

                if (propertyList != null && propertyList.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("属性 ：");
                    var curName = itemProperty.FindPropertyRelative("property_name").stringValue;

                    //if (_firstGo)
                    //{
                    //    for(int mm = 0; mm < propertyList.Count; ++mm)
                    //    {
                    //        Log.LogError(propertyList[mm].ToString());
                    //    }
                    //    Log.LogError($"{i}:{(MaterialAnimationType)selectType},property_id:{itemProperty.FindPropertyRelative("property_id").intValue},name:{itemProperty.FindPropertyRelative("property_name").stringValue},propertyList={propertyList.Count()},{Mathf.Clamp(IndexOfPropertyList(propertyList, curName), 0, propertyList.Count - 1)}");
                    //}

                    curSelectPid = Mathf.Clamp(IndexOfPropertyList(propertyList, curName), 0, propertyList.Count - 1);
                    curSelectPid = EditorGUILayout.Popup(curSelectPid, (propertyList.Select(m => m.name)).ToArray());

                    itemProperty.FindPropertyRelative("property_id").intValue = propertyList[curSelectPid].pid;
                    itemProperty.FindPropertyRelative("property_name").stringValue = propertyList[curSelectPid].name;

                    EditorGUILayout.EndHorizontal();
                }
            }
            itemProperty.FindPropertyRelative("matPropertyType").enumValueIndex = selectType;
           
                        
            if(selectType > 0)
            {
                EditorGUILayout.BeginHorizontal();
                ++EditorGUI.indentLevel;
                EditorGUILayout.LabelField("开始值 ：");
                switch ((MaterialAnimationType)selectType)
                {
                    case MaterialAnimationType.BurnAmount:
                    case MaterialAnimationType.BumpScale:
                        itemProperty.FindPropertyRelative("fromValue").floatValue = EditorGUILayout.FloatField(itemProperty.FindPropertyRelative("fromValue").floatValue, new GUILayoutOption[0]);
                        break;
                    case MaterialAnimationType.Color:
                        itemProperty.FindPropertyRelative("fromColor").colorValue = EditorGUILayout.ColorField(new GUIContent(""), itemProperty.FindPropertyRelative("fromColor").colorValue, true, true, true, new GUILayoutOption[0]);
                        break;
                    case MaterialAnimationType.Tilling:
                        itemProperty.FindPropertyRelative("fromTextureSt").vector4Value = EditorGUILayout.Vector4Field("", itemProperty.FindPropertyRelative("fromTextureSt").vector4Value, new GUILayoutOption[0]);
                        break;
                    case MaterialAnimationType.Translate:
                    case MaterialAnimationType.Rotation:
                    case MaterialAnimationType.Scale:
                        itemProperty.FindPropertyRelative("fromV3").vector3Value = EditorGUILayout.Vector3Field("", itemProperty.FindPropertyRelative("fromV3").vector3Value, new GUILayoutOption[0]);
                        break; 
                }
                EditorGUILayout.EndHorizontal();
                ++EditorGUI.indentLevel; ++EditorGUI.indentLevel;
                if (GUILayout.Button("填入当前值"))
                {
                    switch ((MaterialAnimationType)selectType)
                    {
                        case MaterialAnimationType.BurnAmount:
                        case MaterialAnimationType.BumpScale:
                            itemProperty.FindPropertyRelative("fromValue").floatValue = propertyList[curSelectPid].fvalue;
                            break;
                        case MaterialAnimationType.Color:
                            itemProperty.FindPropertyRelative("fromColor").colorValue = propertyList[curSelectPid].colorValue;
                            break;
                        case MaterialAnimationType.Tilling:
                            itemProperty.FindPropertyRelative("fromTextureSt").vector4Value = propertyList[curSelectPid].vectorValue;
                            break;
                        case MaterialAnimationType.Translate:
                            itemProperty.FindPropertyRelative("fromV3").vector3Value = targetRender.gameObject.transform.localPosition;
                            break;
                        case MaterialAnimationType.Rotation:
                            itemProperty.FindPropertyRelative("fromV3").vector3Value = targetRender.gameObject.transform.localRotation.eulerAngles;
                            break;
                        case MaterialAnimationType.Scale:
                            itemProperty.FindPropertyRelative("fromV3").vector3Value = targetRender.gameObject.transform.localScale;
                            break;
                    }
                }
                --EditorGUI.indentLevel; --EditorGUI.indentLevel;

                EditorGUILayout.BeginHorizontal();

                SerializedProperty arrayProperty = null;
                SerializedProperty frameTimes = itemProperty.FindPropertyRelative("frameTimes");

                EditorGUILayout.LabelField(txtFlyFrameNumber);
                switch ((MaterialAnimationType)selectType)
                {
                    case MaterialAnimationType.BurnAmount:
                    case MaterialAnimationType.BumpScale:
                        arrayProperty = itemProperty.FindPropertyRelative("values");                        
                        break;
                    case MaterialAnimationType.Color:
                        arrayProperty = itemProperty.FindPropertyRelative("Colors");                        
                        break;
                    case MaterialAnimationType.Tilling:
                        arrayProperty = itemProperty.FindPropertyRelative("TextureSts");                        
                        break;
                    case MaterialAnimationType.Translate:
                    case MaterialAnimationType.Rotation:
                    case MaterialAnimationType.Scale:
                        arrayProperty = itemProperty.FindPropertyRelative("Vector3s");
                        break;
                }

                if (arrayProperty != null)
                {
                    arrayProperty.arraySize = EditorGUILayout.IntField(arrayProperty.arraySize, new GUILayoutOption[0]);
                    frameTimes.arraySize = arrayProperty.arraySize;
                }

                EditorGUILayout.EndHorizontal();

                if(arrayProperty != null)
                {
                    float last_time = 0;
                    for (int n = 0; n < arrayProperty.arraySize; ++n)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField((n + 1).ToString());
                        EditorGUILayout.EndHorizontal();

                        ++EditorGUI.indentLevel;
                        {
                            var keytime = frameTimes.GetArrayElementAtIndex(n).floatValue;
                            EditorGUILayout.BeginHorizontal();
                            //EditorGUILayout.LabelField(txtFlyFrameTime);
                            EditorGUILayout.LabelField($"时间比例{keytime}，（{keytime * itemProperty.FindPropertyRelative("total_seconds").floatValue}秒）");

                            keytime = EditorGUILayout.Slider(keytime, 0, 1, new GUILayoutOption[0]);                           
                            if (keytime < last_time) keytime = last_time;
                            last_time = keytime;
                            

                            EditorGUILayout.EndHorizontal();


                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(txtKeyFrameName);
                            switch ((MaterialAnimationType)selectType)
                            {
                                case MaterialAnimationType.BurnAmount:
                                case MaterialAnimationType.BumpScale:
                                    arrayProperty.GetArrayElementAtIndex(n).floatValue = EditorGUILayout.FloatField(arrayProperty.GetArrayElementAtIndex(n).floatValue, new GUILayoutOption[0]);
                                    break;
                                case MaterialAnimationType.Color:
                                    arrayProperty.GetArrayElementAtIndex(n).colorValue = EditorGUILayout.ColorField(new GUIContent(""), arrayProperty.GetArrayElementAtIndex(n).colorValue, true, true, true, new GUILayoutOption[0]);
                                    break;
                                case MaterialAnimationType.Tilling:
                                    arrayProperty.GetArrayElementAtIndex(n).vector4Value = EditorGUILayout.Vector4Field("", arrayProperty.GetArrayElementAtIndex(n).vector4Value, new GUILayoutOption[0]);
                                    break;
                                case MaterialAnimationType.Translate:
                                case MaterialAnimationType.Rotation:
                                case MaterialAnimationType.Scale:
                                    arrayProperty.GetArrayElementAtIndex(n).vector3Value = EditorGUILayout.Vector3Field("", arrayProperty.GetArrayElementAtIndex(n).vector3Value, new GUILayoutOption[0]);
                                    break;
                            }
                            EditorGUILayout.EndHorizontal();
                            frameTimes.GetArrayElementAtIndex(n).floatValue = keytime;
                        }
                        --EditorGUI.indentLevel;
                    }
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("目标值 ：");
                switch ((MaterialAnimationType)selectType)
                {
                    case MaterialAnimationType.BurnAmount:
                    case MaterialAnimationType.BumpScale:
                    case MaterialAnimationType.BumpHeight:
                        itemProperty.FindPropertyRelative("dstValue").floatValue = EditorGUILayout.FloatField(itemProperty.FindPropertyRelative("dstValue").floatValue, new GUILayoutOption[0]);
                        break;
                    case MaterialAnimationType.Color:
                        itemProperty.FindPropertyRelative("dstColor").colorValue = EditorGUILayout.ColorField(new GUIContent(""), itemProperty.FindPropertyRelative("dstColor").colorValue, true, true, true, new GUILayoutOption[0]);
                        break;
                    case MaterialAnimationType.Tilling:
                        itemProperty.FindPropertyRelative("dstTextureSt").vector4Value = EditorGUILayout.Vector4Field("", itemProperty.FindPropertyRelative("dstTextureSt").vector4Value, new GUILayoutOption[0]);
                        break;
                    case MaterialAnimationType.Translate:
                    case MaterialAnimationType.Rotation:
                    case MaterialAnimationType.Scale:
                        itemProperty.FindPropertyRelative("dstV3").vector3Value = EditorGUILayout.Vector3Field("", itemProperty.FindPropertyRelative("dstV3").vector3Value, new GUILayoutOption[0]);
                        break;
                }
                EditorGUILayout.EndHorizontal();

                ++EditorGUI.indentLevel; ++EditorGUI.indentLevel;
                if (GUILayout.Button("填入当前值"))
                {
                    switch ((MaterialAnimationType)selectType)
                    {
                        case MaterialAnimationType.BurnAmount:
                        case MaterialAnimationType.BumpScale:
                        case MaterialAnimationType.BumpHeight:
                            itemProperty.FindPropertyRelative("dstValue").floatValue = propertyList[curSelectPid].fvalue;
                            break;
                        case MaterialAnimationType.Color:
                            itemProperty.FindPropertyRelative("dstColor").colorValue = propertyList[curSelectPid].colorValue;
                            break;
                        case MaterialAnimationType.Tilling:
                            itemProperty.FindPropertyRelative("dstTextureSt").vector4Value = propertyList[curSelectPid].vectorValue;
                            break;
                        case MaterialAnimationType.Translate:
                            itemProperty.FindPropertyRelative("dstV3").vector3Value = targetRender.gameObject.transform.localPosition;
                            break;
                        case MaterialAnimationType.Rotation:
                            itemProperty.FindPropertyRelative("dstV3").vector3Value = targetRender.gameObject.transform.localRotation.eulerAngles;
                            break;
                        case MaterialAnimationType.Scale:
                            itemProperty.FindPropertyRelative("dstV3").vector3Value = targetRender.gameObject.transform.localScale;
                            break;
                    }
                }
                --EditorGUI.indentLevel; --EditorGUI.indentLevel;

                --EditorGUI.indentLevel;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("延时 (秒)：");
                itemProperty.FindPropertyRelative("delay_seconds").floatValue = EditorGUILayout.Slider(itemProperty.FindPropertyRelative("delay_seconds").floatValue, 0f, 10f, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("时长 (秒)：");
                itemProperty.FindPropertyRelative("total_seconds").floatValue = EditorGUILayout.Slider(itemProperty.FindPropertyRelative("total_seconds").floatValue,0f,10f, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("无限循环：");
                var loopCount = itemProperty.FindPropertyRelative("effect_times").intValue;
                bool loop = EditorGUILayout.Toggle(itemProperty.FindPropertyRelative("effect_times").intValue <= 0, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();
                if (loop)
                {
                    if (loopCount > 0)
                    {
                        itemProperty.FindPropertyRelative("effect_times").intValue = 0;
                        loopCount = 0;
                    }
                }
                else
                {
                    if(loopCount <= 0)
                    {
                        itemProperty.FindPropertyRelative("effect_times").intValue = 1;
                        loopCount = 1;
                    }
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("循环次数：");
                    itemProperty.FindPropertyRelative("effect_times").intValue = EditorGUILayout.IntField(itemProperty.FindPropertyRelative("effect_times").intValue, new GUILayoutOption[0]);
                    EditorGUILayout.EndHorizontal();                    
                } 

            }
            else
            {
                EditorGUILayout.LabelField("请选择动画类型");
            }
         

           --EditorGUI.indentLevel;
        }

        _firstGo = false;
        ERRORHAND:

        serializedObject.ApplyModifiedProperties();//end
    }

    int IndexOfPropertyList(List<ShaderProperty> list, string pname)
    {
        for (int i = 0; i < list.Count; ++i)
        {
            if (list[i].name == pname)
                return i;
        }
        return -1;
    }

   List<ShaderProperty> GetPropertyNamesByType( MaterialAnimationType t)
    {
        List<ShaderProperty> result = new List<ShaderProperty>();

        if ( t > MaterialAnimationType.None && t < MaterialAnimationType.Translate)
        {
            for (int i = 0; i < PropertyList.Count; i++)
            {
                var PropertyItem = PropertyList[i];

                switch (t)
                {
                    case MaterialAnimationType.BumpScale:
                        if (PropertyItem.name == "_BumpScale")
                        {
                            result.Add(PropertyItem);
                            goto COMPLETE;
                        }
                        break;
                    case MaterialAnimationType.BumpHeight:
                        if (PropertyItem.name == "_HeightScale")
                        {
                            result.Add(PropertyItem);
                            goto COMPLETE;
                        }
                        break;
                    case MaterialAnimationType.BurnAmount:
                        if (PropertyItem.name == "_BurnAmount")
                        {
                            result.Add(PropertyItem);
                            goto COMPLETE;
                        }
                        break;
                    case MaterialAnimationType.Color:
                        if (PropertyItem.type == ShaderUtil.ShaderPropertyType.Color)
                        {
                            result.Add(PropertyItem);
                        }
                        break;
                    case MaterialAnimationType.Tilling:
                        if (PropertyItem.type == ShaderUtil.ShaderPropertyType.Vector && PropertyItem.texDim == TextureDimension.Tex2D)
                        {
                            result.Add(PropertyItem);
                        }
                        break;
                }

            }
        }

COMPLETE:

        return result;
    }

    private float time = 0;
    private void Awake()
    {
        time = Time.realtimeSinceStartup;
        EditorApplication.update += UpdateHandler;
    }

    private void OnDestroy()
    {
        EditorApplication.update -= UpdateHandler;
    }

    void UpdateHandler()
    {
        if (!Application.isPlaying)
        {
            var s = target as MaterialAnimationBehaviour;

            if (s != null)
            {
                float deltaTime = Time.realtimeSinceStartup - time;
                time = Time.realtimeSinceStartup;

                s.OnEditor_Update(deltaTime);
            }

            foreach (var sceneView in SceneView.sceneViews)
            {
                if (sceneView is SceneView)
                {
                    (sceneView as SceneView).Repaint();
                }
            }
        }
    }

}

