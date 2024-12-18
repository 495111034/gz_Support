using System.Collections.Generic;

namespace findpath
{
    /// <summary>
    /// 佛洛伊德平滑算法
    /// </summary>
    public class Floyd
    {
        public delegate bool IsBlockFunc(int x0, int y0, int x1, int y1);

        /// <summary>
        /// 平滑算法
        /// </summary>
        static void SmoothOnce(List<int> path, IsBlockFunc isBlock, List<int> ret)
        {            
            int a = 0, b = 2, c = 4;        // a=开始, b=下一个, c=测试

            // 放入 a
            ret.Add(path[a]); ret.Add(path[a + 1]);

            // 测试每个 c
            while (c < path.Count)
            {
                // 对每个 c, 如果 a->c 有障碍, 则保留 b
                if (isBlock(path[a], path[a + 1], path[c], path[c + 1]))
                {
                    ret.Add(path[b]); ret.Add(path[b + 1]);
                    a = b;
                }
                b = c;
                c += 2;
            }

            // 放入 b
            if (b < path.Count)
            {
                ret.Add(path[b]); ret.Add(path[b + 1]);
            }         
        }
        public static void Smooth(List<int> path, IsBlockFunc isBlock, List<int> ret)
        {
            if (path != null)
            {
                while (path.Count > 4)
                {
                    var count = path.Count;
                     SmoothOnce(path, isBlock, ret);
                    if (path.Count == count) break;
                }
            }            
        }

        // 转换为浮点坐标
        public static int MakeFloat(List<int> path, float size, float x0, float y0, float x1, float y1, MyBetterList<float> result)
        {
            // 无路径
            if (path == null)
            {
                return 0;
            }

            // 路径太短
            var count = path.Count;
            if (count < 4)
            {
                return 0;
            }

            // 确保 x1/y1 落在 path 的最后一个格子内
            var last = count - 2;
            var last_x = path[last];
            var last_y = path[last + 1];
            if ((int)(x1 / size) != last_x || (int)(y1 / size) != last_y)
            {
                x1 = (last_x + 0.5f) * size;
                y1 = (last_y + 0.5f) * size;
            }

            // 拟合路径
            //float[] arr = new float[count];

            result.Add(x0);
            result.Add(y0);
          

            for (int i = 2; i < last; i += 2)
            {
                result.Add((path[i] + 0.5f) * size);
                result.Add((path[i + 1] + 0.5f) * size);
            }

            result.Add( x1);
            result.Add( y1);

            return count;
        }
    }
}
