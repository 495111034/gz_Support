
using UnityEngine;

namespace MyComponentExUtil
{
    [Config("UI")]
    public class UIConfigEditor
    {
        //目前只实现了以下类型，新增的话需要同步修改 MyComponentEditor
        //[Config("测试Bool")]

        //[Config("测试Color")]

        //[Config("测试浮点型")]

        //[Config("测试string")]

        //[Config("测试int型")]

        [Config("附加/添加通用模糊背景", null, new string[] { "默认", "强制重建" })]
        public int f1;

        [Config("附加/添加通用黑底背景", " ")]
        public int f2;

        [Config("附加/显示通用货币信息栏", " ")]
        public int f3;

        [Config("功能/打开时关闭主界面", " ")]
        public int f4;

        [Config("功能/点击其它关闭界面", "需要UIPointRectOut")]
        public int f5;

        [Config("动画/开启时动画")]
        public string f6 = "";

        [Config("动画/关闭时动画")]
        public string f7 = "";

        [Config("音效/开启时音效延迟")]
        public float f8 = 0;

        [Config("音效/开启时音效")]
        public string f9 = "";

        [Config("音效/关闭时音效延迟")]
        public float f10 = 0;

        [Config("音效/关闭时音效")]
        public string f11 = "";

        [Config("界面类型", null, new string[] { "主界面", "独显界面", "一般界面", "动态界面", "提示界面", "Top界面", "背景界面", "缩放界面", "过场界面"})]
        public int f12 = 0;

        [Config("功能/自动关闭(秒)")]
        public int f13 = 0;

        [Config("缓存(秒)释放,-1切场景,-2永不清理")]
        public int f14 = 0;

        [Config("切换场景不关闭", " ")]
        public int f15;

        [Config("加入回滚列表", " ")]
        public int f16;

        [Config("适配异形屏缩放", null, new string[] { "加入size计算", "只缩放" })]
        public int f17;

        [Config("附加/自定义货币信息栏,equipId,逗号隔开")]
        public string f18 = "";

        [Config("功能/不受关闭主界面影响", " ")]
        public int f19 = 0;

        [Config("使用同步加载", " ")]
        public int f20 = 0;

        [Config("功能/使用渐变淡入", " ")]
        public int f21 = 0;

        [Config("功能/隐藏主摄像机", "优化性能,适用全屏界面")]
        public int f22 = 0;

        [Config("功能/关闭同类型的其它界面", " ")]
        public int f23 = 0;

        [Config("附加/道具品质特效显示", null, new string[] { "全部不显示", "显示紫色以上", "显示橙色以上", "显示红色以上" })]
        public int f24 = 3;

        [Config("功能/打开x秒内不能关闭")]
        public float f25 = 0;
    }

    [Config("cfg")]
    public class FieldsConfigEditor
    {
        [Config("int型参数", "Int32")]
        public int f01 = 0;

        [Config("float型参数", "Single")]
        public int f02 = 0;

        [Config("bool型参数", "Boolean")]
        public int f03 = 0;

        [Config("string型参数", "String")]
        public int f04 = 0;

        [Config("color型参数", "Color")]
        public int f05 = 0;

        [Config("vector2参数", "Vector2")]
        public int f06 = 0;

        [Config("vector3参数", "Vector3")]
        public int f07 = 0;
    }
}
