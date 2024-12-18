namespace UnityEngine.UI
{
    public class LayoutRebuilderEx
    {
        public static void ForceRebuildLayoutImmediate(RectTransform rect)
        {
            var txts = MyListPool<MyText>.Get();
            rect.gameObject.GetComponentsInChildren(txts);
            foreach (var txt in txts)
            {
                txt.Check_LateUpdate();
            }
            MyListPool<MyText>.Release(txts);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
        public static void MarkLayoutForRebuild(RectTransform rect)
        {
            LayoutRebuilder.MarkLayoutForRebuild(rect);
        }
    }
}