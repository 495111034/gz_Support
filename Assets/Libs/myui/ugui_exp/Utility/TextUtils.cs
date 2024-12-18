using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    /// <summary>
    /// Text 辅助
    /// </summary>
    public class TextUtils
    {
        // 每行统计信息
        class LineInfo
        {
            public int i_start;     // 顶点 开始/结束 索引
            public int i_end;
            public float y0;        // 第 0/2 号顶点的 y 坐标
            public float y2;

            //
            public float height;
        }

        // 构造行信息
        static List<LineInfo> GenLineList(UIVertex[] vbo)
        {
            var v_count = vbo.Length;
            var line_list = s_line_info;
            line_list.Clear();

            // 遍历每个字符, 每个字符4个顶点, 顺序为左上角开始, 顺时针
            LineInfo line = null;
            var last_x = float.MaxValue;
            var last_y = float.MaxValue;
            for (int i = 0; i < v_count; i += 4)
            {
                // 获取 0/2 位置
                var p0 = vbo[i].position;           // 左上角
                var p2 = vbo[i + 2].position;       // 右下角

                // 判断 width
                var width = p2.x - p0.x;
                if (width <= 0) continue;           // 不可见

                // 确保 height >= width
                var height = p0.y - p2.y;           // 注意 Y 向上, 因此 p0.y 值更大
                if (height < width)
                {
                    height = width;
                    p0.y = p2.y + height;           // 调整 p0
                }

                // 注意, 对于某些音标字符(如泰语), 后一个字符显示在前一个字符的上面, 因此要同时判断 x/y 2个方向
                // 新行开始
                if (p0.x < last_x && p0.y < last_y)
                {
                    if (line != null) line.i_end = i;

                    line = new LineInfo();
                    line_list.Add(line);

                    line.i_start = i;
                    line.y0 = p0.y;
                    line.y2 = p2.y;
                }
                else
                {
                    if (p0.y > line.y0) line.y0 = p0.y;
                    if (p2.y < line.y2) line.y2 = p2.y;
                }

                //
                last_x = p0.x;
                last_y = p0.y;
            }
            if (line != null) line.i_end = v_count;

            //
            return line_list;
        }
        static List<LineInfo> s_line_info = new List<LineInfo>();

        // 移动行
        static void OffsetLine(UIVertex[] vbo, LineInfo line, float offset)
        {
            line.y0 += offset;
            line.y2 += offset;

            for (int i = line.i_start; i < line.i_end; i += 4)
            {
                vbo[i + 0].position.y += offset;
                vbo[i + 1].position.y += offset;
                vbo[i + 2].position.y += offset;
                vbo[i + 3].position.y += offset;
            }
        }
       
       
    }
}
