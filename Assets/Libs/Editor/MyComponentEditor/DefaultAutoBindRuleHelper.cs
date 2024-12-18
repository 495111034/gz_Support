using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 自动绑定规则辅助器接口
/// </summary>
public interface IAutoBindRuleHelper
{
    /// <summary>
    /// 是否为有效绑定
    /// </summary>
    bool IsValidBind(Transform target, List<string> filedNames, List<string> componentTypeNames);
}

/// <summary>
/// 默认自动绑定规则辅助器
/// </summary>
public class DefaultAutoBindRuleHelper : IAutoBindRuleHelper
{

    /// <summary>
    /// 命名前缀与类型的映射
    /// </summary>
    private Dictionary<string, string> m_PrefixesDict = new Dictionary<string, string>()
    {
        {"trans","Transform" },
        {"rect","RectTransform"},
        {"vgroup","VerticalLayoutGroup"},
        {"hgroup","HorizontalLayoutGroup"},
        {"ggroup","GridLayoutGroup"},
        {"tgroup","ToggleGroup"},

        {"btn","MyButton"},
        {"img", "MySpriteImage"},
        {"rimg","RawImage"},
        {"txt","MyText"},
        {"text","MyText"},
        {"input","InputField"},
        {"slider","Slider"},
        {"mask","Mask"},
        {"mask2d","RectMask2D"},
        {"toggle","Toggle"},
        {"sbar","Scrollbar"},
        {"srect","ScrollRect"},
        {"nsrectv","NLoopVerticalScrollRect"},
        {"nsrecth","NLoopHorizontalScrollRect"},
        {"drop","Dropdown"},
    };
    public bool IsValidBind(Transform target, List<string> filedNames, List<string> componentTypeNames)
    {
        // string[] strArray = target.name.Split('_');
        string[] strArray = target.name.Split(new char[] { '_' }, 2);

        if (strArray.Length == 1)
        {
            return false;
        }

        string filedName = strArray[strArray.Length - 1];

        for (int i = 0; i < strArray.Length - 1; i++)
        {
            string str = strArray[i];
            string comName;
            if (m_PrefixesDict.TryGetValue(str, out comName))
            {
                filedNames.Add($"{str}_{filedName}");
                componentTypeNames.Add(comName);
            }
            else
            {
                Debug.Log($"{target.name}的命名中{str}不存在对应的组件类型，跳过绑定");
                return false;
            }
        }

        return true;
    }
}
