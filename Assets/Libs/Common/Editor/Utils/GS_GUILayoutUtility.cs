
using UnityEngine;

namespace GameSupportEditor
{
    public class GS_GUILayoutUtils
    {
        public static Rect GetNextRect(Rect lastRect, float height , float offsetY)
        {
            lastRect.y = lastRect.yMax + offsetY;
            lastRect.height = height;
            return lastRect;
        }

        public static Rect GetNextRect(Rect lastRect, float width, float height, float offsetY)
        {
            lastRect.y = lastRect.yMax + offsetY;
            lastRect.height = height;
            lastRect.width = width;
            return lastRect;
        }
    }
}
