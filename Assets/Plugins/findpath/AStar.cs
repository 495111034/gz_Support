//#define ASTAR_DEBUG
//#define TEST_AIR_WALLS
using System;
using System.Collections.Generic;
using System.Text;
using Mask = System.Byte;
using Random = System.Random;
using Debug = UnityEngine.Debug;
using UnityEngine;
using CellList = System.Collections.Generic.List<findpath.AStarCell>;
using System.Collections;

namespace findpath
{
    public class DirMask
    {
        public Transform go;
        public float dir;
        public bool is_single_through;//true 单向通行， false 不可通行
        public HashSet<int> idxs;
        //public float xmax, xmin, ymax, ymin;
        public Vector3 pos3;
        public bool activeInHierarchy;
#if TEST_AIR_WALLS
        public GameObject _cells_root;
#endif
    }

    // 单元格
    public class AStarCell
    {
        public uint time;
        public short x, y;
        public int idx;
        public int g;				// 已获得的代价
        public int h;				// 估计的剩余的代价

        public AStarCell parent;		    // 父节点
        public CellList list;
    }

    // 寻径结果
    public enum Result
    {
        Ok,
        TimeOut,
        Failed,
    }

    /// <summary>
    /// A* 寻径
    /// </summary>
    public class AStar
    {
        const int MAX_OPEN_LIST = (1 << 4) + 8;

        public int width => _width;
        public int height => _height;

        int _width;					// 地图尺寸
        int _height;

        MapMask mapmask;

        byte[] masks;
        public Dictionary<Transform, DirMask> air_walls;//带方向的阻挡
        public HashSet<int> air_walls_cells = null;

        public byte[] area_ids;     //高度相差2的同一片区域
        public byte[] area_ids2;    //高度相同的同一片区域


        byte[] xmasks;//0 周围是否都是通行

        /// <summary>
        /// 建立地图
        /// </summary>
        void Create(int width, int height)
        {
            _width = width;
            _height = height;
        }


        /// <summary>
        /// 读入地图
        /// </summary>
        public void LoadMask(MapMask mapmask)
        {
            // 创建
            if (mapmask.width != _width || mapmask.height != _height)
            {
                Create(mapmask.width, mapmask.height);
            }
            this.mapmask = mapmask;
            this.masks = mapmask.masks;            
            GenAreas();
        }

        public int getidx(int x, int y)
        {
            return y * _width + x;
        }

        byte m_area_inc_id = 0;
        static short[][] directions = new short[][]
        {
            new short[] { -1, 1, 315, 7 },    new short[] { 0, 1 , 0, 5},       new short[] { 1, 1, 45,  7  },
            new short[] { -1, 0, 270, 5 },                                      new short[] { 1, 0, 90,  5 },
            new short[] { -1, -1,225, 7  },   new short[] { 0, -1, 180 , 5},    new short[] { 1, -1,135, 7 },
        };
        static Stack<int> _child_poses = new Stack<int>();
        //生成区域信息
        void _gen_area(int mx, int my, byte[] area_ids, int maxdt)
        {
            //var area_ids = this.area_ids;
            var child_poses = _child_poses;
            var masks = this.masks;

            child_poses.Clear();
            child_poses.Push(mx);
            child_poses.Push(my);

            while (child_poses.Count > 0)
            {
                int y = child_poses.Pop();
                int x = child_poses.Pop();

                int idx = getidx(x, y);
                int me = masks[idx];
                if (me == 0 || me == 255)
                {
                    //所有 阻挡区域id相同 默认0
                    continue;
                }
                if (area_ids[idx] == 0)
                {
                    //新的区域
                    area_ids[idx] = ++m_area_inc_id;
                }

                //查看4个方向是否相通
                for (int i = 0; i < 8; ++i)
                {
                    var pos = directions[i];
                    int xx = x + pos[0], yy = y + pos[1];
                    if (xx < 0 || xx >= _width)
                    {
                        continue;
                    }
                    if (yy < 0 || yy >= _height)
                    {
                        continue;
                    }

                    int child_idx = getidx(xx, yy);
                    int child = masks[child_idx];
                    if (child == 0 || child == 255)
                    {
                        //所有 阻挡区域 和 非阻挡不连通
                        continue;
                    }
                    int dt = child - me;
                    if (maxdt > 0)
                    {
                        if (dt == 1 || dt == -1)
                        {
                            //黄2 -> 绿3 //不能直达
                            if (Mathf.Max(child, me) == 3)
                            {
                                continue;
                            }
                        }
                    }                    
                    if (dt < -maxdt || dt > maxdt)
                    {
                        //高度差太大
                        continue;
                    }
                    if (area_ids[child_idx] == 0)
                    {
                        area_ids[child_idx] = area_ids[idx];//相通，子节点和父节点是同一个区域
                        child_poses.Push(xx);
                        child_poses.Push(yy);
                    }
                    else
                    {
                        Debug.Assert(area_ids[child_idx] == area_ids[idx]);
                    }
                }
            }
        }


        public void GenAreas()
        {
            var _width = this._width;
            var _height = this._height;
            {
                var len = _width * _height;
                if (area_ids == null || area_ids.Length != len)
                {
                    area_ids = new byte[len];
                }
                else
                {
                    Array.Clear(area_ids, 0, len);
                }
                if (area_ids2 == null || area_ids2.Length != len)
                {
                    area_ids2 = new byte[len];
                }
                else
                {
                    Array.Clear(area_ids2, 0, len);
                }
                if (this.xmasks == null || this.xmasks.Length != len)
                {
                    this.xmasks = new byte[len];
                }
                else
                {
                    Array.Clear(this.xmasks, 0, len);
                }
            }

            var masks = this.masks;
            {
                var xmasks = this.xmasks;
                for (int y = 0; y < _height; ++y)
                {
                    for (int x = 0; x < _width; ++x)
                    {
                        var idx = getidx(x, y);
                        var mask = masks[idx];
                        if (mask == 0 || mask == 255)
                        {
                            xmasks[idx] = 1;
                            foreach (var d in directions)
                            {
                                var dx = x + d[0];
                                var dy = y + d[1];
                                if (dx >= 0 && dx < _width && dy >= 0 && dy < _height) 
                                {
                                    var idx2 = getidx(dx, dy);
                                    xmasks[idx2] = 1;
                                }
                                if (d[0] == 0 || d[1] == 0) 
                                {
                                    dx = x + d[0] * 2;
                                    dy = y + d[1] * 2;
                                    if (dx >= 0 && dx < _width && dy >= 0 && dy < _height)
                                    {
                                        var idx2 = getidx(dx, dy);
                                        xmasks[idx2] = 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //
            {
                m_area_inc_id = 0;
                var area_ids = this.area_ids2;
                //设置孤岛信息
                for (int y = 0; y < _height; ++y)
                {
                    for (int x = 0; x < _width; ++x)
                    {
                        var idx = getidx(x, y);
                        if (area_ids[idx] == 0)
                        {
                            var mask = masks[idx];
                            if (mask != 0 && mask != 255)
                            {
                                _gen_area(x, y, area_ids, 0);
                            }
                        }
                    }
                }
            }
            {
                m_area_inc_id = 0;
                var area_ids = this.area_ids;
                //设置孤岛信息
                for (int y = 0; y < _height; ++y)
                {
                    for (int x = 0; x < _width; ++x)
                    {
                        var idx = getidx(x, y);
                        if (area_ids[idx] == 0)
                        {
                            var mask = masks[idx];
                            if (mask != 0 && mask != 255)
                            {
                                _gen_area(x, y, area_ids, 2);
                            }
                        }
                    }
                }
            }
        }


        // 坐标是否有效
        public bool IsValid(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }


        // 是否可通过
        static public bool CanPass(Mask m1, Mask m2, float extra_height)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (m1 == 0)
                {
                    m1 = 255;
                }
                if (m2 == 0)
                {
                    m2 = 255;
                }
            }
#endif
            if (m1 == 255) return true;             // 必定可以从阻挡走出来
            if (extra_height == 0)
            {
                var height = m2 - m1;
                return height <= 1 && height >= -2;     // 太高, 太低 都无法通过
            }
            else
            {
                var height = m2 - (m1 + extra_height);
                return height <= 1; //太低无法通过
            }
        }


        static uint _time;
        static CellList[] s_open_list_buff;
        static AStarCell[] _cells_rect = null;
        static CellList _cells_pool = null;

        static CellList[] GetOpenListBuff(int count)
        {
            var cells_rect = AStar._cells_rect;
            if (cells_rect == null || cells_rect.Length < count)
            {
                var old = cells_rect;
                AStar._cells_rect = cells_rect = new AStarCell[count];
                if (old != null)
                {
                    Array.Copy(old, cells_rect, old.Length);
                }
            }

            var buff = s_open_list_buff;
            if (buff == null)
            {
                buff = new CellList[MAX_OPEN_LIST];
                for (int i = 0; i < MAX_OPEN_LIST; i++)
                {
                    buff[i] = new CellList();
                }
                s_open_list_buff = buff;
            }
            else
            {
                for (int i = 0; i < MAX_OPEN_LIST; i++)
                {
                    buff[i].Clear();
                }
            }
            return buff;
        }
        uint UpdateTime(uint add)
        {
            uint time = _time + add;
            if (time > 0xfffffff0)
            {
                foreach (var s in _cells_pool)
                {
                    if (s != null)
                    {
                        s.time = 0;
                    }
                }
                time = add;
            }
            return _time = time;
        }

        public int loop;



        /// <summary>
        /// A* 寻径
        /// </summary>
#if ASTAR_DEBUG
        void DrawCell(AStarCell c, Color color, Texture2D tex)
        {
            for (var y = c.y * 3; y < c.y * 3 + 3; ++y)
            {
                for (var x = c.x * 3; x < c.x * 3 + 3; ++x)
                {
                    tex.SetPixel(x, y, color);
                }
            }
        }

        void DrawPath(AStarCell c, Color color, Texture2D tex)
        {
            while (c != null)
            {
                DrawCell(c, color, tex);
                c = c.parent;
            }
        }
        public Result Search(float fx0, float fy0, float fx1, float fy1, float error, List<float> pos_list, float extra_height)
        {
            return Result.Failed;
        }

        public IEnumerator Search(float fx0, float fy0, float fx1, float fy1, float error, List<float> pos_list,float extra_height, Texture2D tex)
#else
        public Result Search(float fx0, float fy0, float fx1, float fy1, float error, List<float> pos_list, float extra_height, ref bool find_nearest, bool log)
#endif
        {
#if ASTAR_DEBUG
            int delay_ms = 500;
#endif

            var bfind_nearest = find_nearest;
            find_nearest = false;

            loop = 0;

            short x0 = (short)fx0;
            short y0 = (short)fy0;
            short x1 = (short)fx1;
            short y1 = (short)fy1;

            pos_list.Clear();

            // 超界
            if (!IsValid(x1, y1))
            {
#if ASTAR_DEBUG
                yield
#endif
                return Result.Failed;
#if ASTAR_DEBUG
                yield break;
#endif
            }

            // 快速判断
            if ((x0 == x1 && y0 == y1) || !IsValid(x0, y0))
            {
                pos_list.Add(fx0);
                pos_list.Add(fy0);
                pos_list.Add(fx1);
                pos_list.Add(fy1);
#if ASTAR_DEBUG
                yield
#endif
                return Result.Ok;
#if ASTAR_DEBUG
                yield break;
#endif
            }

            //确保不要离目的地距离 > error
            error -= 0.01f;
            if (error < 0.1f)
            {
                error = 0;
            }
            var sqr_error = error * error;

            var id_start = getidx(x0, y0);
            var id_end = getidx(x1, y1);
            var area_start = area_ids[id_start];
            var area_end = area_ids[id_end];
            //var botherror = area_start == 0 && area_end == 0;
            //
            if (area_start == 0)
            {
                bfind_nearest = true;
            }
            //
            if (extra_height == 0 && !bfind_nearest)
            {                
                if (area_start != area_end)
                {
                    if (error == 0)
                    {
#if ASTAR_DEBUG
                        yield
#endif
                        return Result.Failed;
#if ASTAR_DEBUG
                        yield break;
#endif
                    }
                    else
                    {
                        var ok = false;
                        var xsqr_error = sqr_error * 0.9f;
                        for (var y = (int)-error; !ok && y < error; ++y)
                        {
                            for (var x = (int)-error; x < error; ++x)
                            {
                                if (x != 0 || y != 0)
                                {
                                    if (IsValid(x1 + x, y1 + y))
                                    {
                                        var id = getidx(x1 + x, y1 + y);
                                        if (area_start == area_ids[id])
                                        {
                                            var mx = x1 + x + 0.5f - fx1;
                                            var my = y1 + y + 0.5f - fy1;
                                            if (mx * mx + my * my < xsqr_error)
                                            {
                                                ok = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!ok)
                        {
#if ASTAR_DEBUG
                            yield
#endif
                            return Result.Failed;
#if ASTAR_DEBUG
                            yield break;
#endif
                        }
                    }
                }
            }

            //是否可以直达？
            //if(area_start > 0)
            {
                float refx1 = fx1, refy1 = fy1;
                if (error > 0)
                {
                    var dst = new Vector2(fx1, fy1);
                    var dir = (new Vector2(fx0, fy0) - dst).normalized;
                    dst = dst + dir * error;
                    refx1 = dst.x;
                    refy1 = dst.y;
                }
                if (GetFarest(fx0, fy0, ref refx1, ref refy1, extra_height))
                {
                    pos_list.Add(fx0);
                    pos_list.Add(fy0);
                    pos_list.Add(refx1);
                    pos_list.Add(refy1);
#if ASTAR_DEBUG
                    yield
#endif
                    return Result.Ok;
#if ASTAR_DEBUG
                    yield break;
#endif
                }
            }

            //var sqr_error = error * error;

            //Log.LogInfo($"search from({x0},{y0}) => ({x1},{y1}), id_end={id_end}");
#if !ASTAR_DEBUG
            UnityEngine.Profiling.Profiler.BeginSample("AStar.Search");
#endif
            // 更新计数器, 使得 time 比 cell.time 至少大 2 
            var time = UpdateTime(2);
            uint time_new = time - 2;			// <= time_new, 是未访问过的新节点
            uint time_open = time - 1;			// == time_open, 在 open_list 中
            uint time_close = time - 0;			// == time_close, 在 close_list 中

            // 构造 open_list
            var _width = this._width;
            var _height = this._height;
            var open_list_buff = GetOpenListBuff(_width * _height);
            int open_list_idx = 0;
            var open_list = open_list_buff[open_list_idx];

            var masks = this.masks;
            var xmasks = this.xmasks;
            var directions = AStar.directions;
            var has_air = air_walls?.Count > 0;

            var cells_pool = _cells_pool;
            if (cells_pool == null)
            {
                cells_pool = _cells_pool = new CellList(1 + height * width / 4);
            }
            var cells_pool_size = cells_pool.Count;
            var cells_pool_idx = 0;

            var cells_rect = _cells_rect;
            // 把开始节点 c0 放入 open_list
            AStarCell c0 = cells_rect[id_start] = cells_pool_idx < cells_pool_size ? cells_pool[cells_pool_idx++] : cells_pool.AddT(new AStarCell());
            {
                c0.x = x0;
                c0.y = y0;
                c0.idx = id_start;
                c0.g = 0;
                c0.h = GetDistance(x0, y0, x1, y1);
                c0.parent = null;
                c0.list = open_list;
                open_list.Add(c0);
            }
#if UNITY_EDITOR
            var isPlaying = Application.isPlaying;
#endif

            // 开始寻径
            AStarCell c_found = null, c_nearest = null;

            //var t0 = Time.realtimeSinceStartup;
            int loops = 0;
            while (true)
            {
                // 寻找第一个非空的 open_list
                if (open_list.Count == 0)
                {
                    for (int off = 1; off < MAX_OPEN_LIST; off++)
                    {
                        int i = (open_list_idx + off) % MAX_OPEN_LIST;
                        open_list = open_list_buff[i];
                        if (open_list.Count > 0)
                        {
                            open_list_idx = i;
                            break;
                        }
                    }
                    if (open_list.Count == 0) break;
                    //
                    var timeout = _width * _height / 4;                    
                    if (loops > timeout && Application.isPlaying)
                    {
                        if (!bfind_nearest)
                        {
                            Log.LogError($"Search Timeout, loops={loops}, pos:({fx0},{fy0}) => ({fx1},{fy1}), error={error}, extra_height={extra_height}, _width={_width}, _height={_height}, shield={mapmask.path}");
                        }
                        else 
                        {
                            Log.LogInfo($"Search Timeout, loops={loops}, pos:({fx0},{fy0}) => ({fx1},{fy1}), error={error}, extra_height={extra_height}, _width={_width}, _height={_height}, shield={mapmask.path}");
                        }
                        break;
                    }
                }

                // 获取 open_list 中最小代价的节点 c1, 并设置 close 标记
                var c1 = open_list[open_list.Count - 1];
                open_list.RemoveAt(open_list.Count - 1);
                if (c1.list != open_list)
                {
                    continue;
                }

                if (bfind_nearest) 
                {
                    if (c_nearest == null || c_nearest.h > c1.h) 
                    {
                        c_nearest = c1;
                    }
                }

                ++loops;
                c1.list = null;
                c1.time = time_close;
#if ASTAR_DEBUG
                Log.LogInfo($"pop({c1.x},{c1.y}), gh={c1.g + c1.h}={c1.g}+{c1.h}, open_list_idx={open_list_idx}, cnt={open_list.Count} ------gh={c1.g + c1.h}------");
                DrawPath(c1, Color.cyan, tex);
                yield return delay_ms;                
#endif
                // 如果 c1 是终点, 则结束
                if (c1.idx == id_end)
                {
                    c_found = c1;
                    break;
                }
                var x = c1.x;
                var y = c1.y;
                if (sqr_error > 0)
                {
                    var dx = x + 0.5f - fx1;
                    var dy = y + 0.5f - fy1;
                    if (dx * dx + dy * dy <= sqr_error)
                    {
                        c_found = c1;
                        break;
                    }
                }
                // 搜索和 c1 相邻的其它节点
                var f1 = c1.g + c1.h;					// 当前代价
                var mask = masks[c1.idx];
#if UNITY_EDITOR
                if (!isPlaying && mask == 0)
                {
                    mask = 255;
                }
#endif
                int c2_id;
                AStarCell c2;

                for (int j = 0; j < 8; j++)
                {
                    var dir = directions[j];
                    short x2 = (short)(x + dir[0]), y2 = (short)(y + dir[1]);
                    //if (!IsValid(x2, y2))
                    if (!(x2 >= 0 && x2 < _width && y2 >= 0 && y2 < _height))
                    {
                        continue;
                    }
                    //c2_id = getidx(x2, y2);
                    c2_id = y2 * _width + x2;
                    //if (mask != 255)
                    {
                        var mask2 = masks[c2_id];
#if UNITY_EDITOR
                        if (!isPlaying && mask2 == 0)
                        {
                            mask2 = 255;
                        }
#endif
                        //if (!CanPass(mask, masks[c2_id]))
                        //if (mask2 == 255)
                        //{
                        //    continue;
                        //}

                        if (extra_height == 0)
                        {
                            var dt = mask2 - mask;
                            if (dt < -2 || dt > 1)     // 太高, 太低 都无法通过
                            {
                                continue;
                            }
                        }
                        else
                        {
                            var dt = mask2 - (mask + extra_height);
                            if (dt > 1) // 太低无法通过
                            {
                                continue;
                            }
                        }
                    }

                    if (has_air)
                    {
                        var deg = dir[2];
                        if (isBlockByAriWall(deg, x2, y2, extra_height))
                        {
                            continue;
                        }
                    }

                    var cost12 = dir[3];
                    int c2_modify = 0;

                    c2 = cells_rect[c2_id];
                    // 如果 c2 是新节点, 则加入 open_list 中
                    if (c2 == null || c2.idx != c2_id || c2.time <= time_new)
                    {
                        c2 = cells_rect[c2_id] = cells_pool_idx < cells_pool_size ? cells_pool[cells_pool_idx++] : cells_pool.AddT(new AStarCell());
                        c2.time = time_open;
                        c2.x = x2;
                        c2.y = y2;
                        c2.idx = c2_id;
                        c2.g = c1.g + cost12;
                        c2.h = GetDistance(x2, y2, x1, y1);
                        c2.parent = c1;
                        c2_modify = 1;

                        //旁边有阻挡，拉高代价
                        if ((xmasks[c2_id] & 1) != 0) 
                        {
                            c2.g += cost12;
                            //if (c2.g + c2.h >= f1 + MAX_OPEN_LIST) 
                            //{
                            //    c2.g = f1 + MAX_OPEN_LIST - c2.h - 1;
                            //}
                        }
#if ASTAR_DEBUG
                        DrawCell(c2, Color.white, tex);
                        yield return delay_ms;
#endif

                    }
                    // 如果 c2 在 open 表中, 则修改代价
                    else if (c2.time == time_open)
                    {
                        // 如果从 c1 到达 c2, 能使得 c2 具有更小的代价
                        if (c1.g + cost12 < c2.g)
                        {
                            //Log.LogInfo($"({c2.x},{c2.y}) change parent from ({c2.parent.x},{c2.parent.y}), g={c2.g} -> {c1.g + cost12}");
                            //c2.list.Remove(c2);
                            c2.parent = c1;
                            c2.g = c1.g + cost12;
                            c2_modify = 2;
                        }
                    }

                    // 把 c2 放入所属的 open_list 中
                    if (c2_modify != 0)
                    {
                        // 获取 list
                        var dist = ((c2.g + c2.h) - f1);
                        if (dist < 0 || dist >= MAX_OPEN_LIST)
                        {
                            throw new Exception(string.Format("dist error, pos0:{0},{1}, pos1:{2},{3}, c1:{4},{5}, c2:{6},{7}, f1:{8}, g2:{9}, h2:{10}, dist:{11}", x0, y0, x1, y1, c1.x, c1.y, c2.x, c2.y, f1, c2.g, c2.h, dist));
                        }
                        //var idx = (open_list_idx + dist) & (MAX_OPEN_LIST - 1);
                        var idx = (open_list_idx + dist) % (MAX_OPEN_LIST);
                        var list = open_list_buff[idx];
                        if (c2_modify == 2 && c2.list == list)
                        {
                            //低概率
                            list.Remove(c2);
                        }
                        list.Add(c2);
                        c2.list = list;
                        if (log)
                        {
                            Log.LogInfo($"{c1.x},{c1.y} -> {c2.x},{c2.y} | {c2_modify}");
                        }
#if ASTAR_DEBUG
                        Log.LogInfo($"pus({c2.x},{c2.y}), gh={c2.g + c2.h}={c2.g}+{c2.h}, open_list_idx={idx}, cnt={list.Count}, modify={c2_modify}");
#endif
                    }
                }
#if ASTAR_DEBUG
                DrawPath(c1, Color.gray, tex);
#endif
            }
            loop = loops;

            if (c_found == null && c_nearest != null) 
            {
                c_found = c_nearest;
                find_nearest = true;
                error = 0;
                fx1 = c_nearest.x + 0.5f;
                fy1 = c_nearest.y + 0.5f;
            }

            // 结束
            var ret = Result.Failed;
            if (c_found != null)
            {                
                BuildPath(c0, c_found, pos_list, extra_height);
                pos_list[0] = fx0;
                pos_list[1] = fy0;
                if (error == 0)
                {
                    pos_list[pos_list.Count - 2] = fx1;
                    pos_list[pos_list.Count - 1] = fy1;
                }
                else 
                {
                    var dst = new Vector2(fx1, fy1);
                    var dir = (new Vector2(pos_list[pos_list.Count - 2], pos_list[pos_list.Count - 1]) - dst).normalized;
                    dst = dst + dir * error;
                    pos_list[pos_list.Count - 2] = dst.x;
                    pos_list[pos_list.Count - 1] = dst.y;
                }
                //Log.LogInfo($"pos_list={string.Join(",", pos_list)}");
                ret = Result.Ok;
            }
            else if (open_list.Count > 0)
            {
                ret = Result.TimeOut;
            }

#if !ASTAR_DEBUG
            UnityEngine.Profiling.Profiler.EndSample();
#endif

#if ASTAR_DEBUG
            yield
#endif
            return ret;

        }

        //static HashSet<int> s_throuth_idxs = new HashSet<int>();
        static List<AStarCell> s_tmp_cell_list = new List<AStarCell>();
        // 构建路径
        void BuildPath(AStarCell c0, AStarCell c_found, List<float> pos_list, float extra_height)
        {
            //Log.LogInfo($"c0={c0.x},{c0.y}, c_found={c_found.x},{c_found.y}");
            // 收集 cell 列表
            var cell_list = s_tmp_cell_list;
            cell_list.Clear();
            {
                AStarCell last = null;
                int dtx = 0, dty = 0;
                for (var cell = c_found; cell != null; cell = cell.parent)
                {
                    var dtx2 = dtx; var dty2 = dty;
                    if (last != null) 
                    {
                        dtx = cell.x - last.x;
                        dty = cell.y - last.y;
                    }
                    if (cell_list.Count < 2 || dtx != dtx2  || dty != dty2)
                    {
                        //Log.LogInfo($"add {cell.x},{cell.y}, dt={dtx},{dty}");
                        cell_list.Add(cell);
                    }
                    else
                    {
                        //Log.LogInfo($"rep {cell.x},{cell.y}, dt={dtx},{dty}");
                        cell_list[cell_list.Count - 1] = cell;
                    }
                    last = cell;
                }
                //容错
                if (cell_list[cell_list.Count - 1] != c0)
                {
                    //Log.LogInfo($"add {c0.x},{c0.y}");
                    cell_list.Add(c0);
                }
            }
            //
            pos_list.Clear();
            pos_list.Add(c0.x + 0.5f);
            pos_list.Add(c0.y + 0.5f);
            //
            var now = c0;
            for (var check_idx = cell_list.Count - 3; check_idx >= 0; --check_idx)
            {
                var check = cell_list[check_idx];
                //Log.LogInfo($"{check_idx} check {now.x},{now.y} -> {check.x},{check.y}");
                float fcx = check.x + 0.5f, fcy = check.y + 0.5f;
                if (!GetFarest(now.x + 0.5f, now.y + 0.5f, ref fcx, ref fcy, extra_height))
                {
                    now = cell_list[check_idx + 1];                    
                    pos_list.Add(now.x + 0.5f);
                    pos_list.Add(now.y + 0.5f);
                    //Log.LogInfo($"add pos[{check_idx + 1}] {now.x},{now.y} at {pos_list.Count}");
                }
                else 
                {
                    //Log.LogInfo($"passed");
                }
            }
            //
            pos_list.Add(c_found.x + 0.5f);
            pos_list.Add(c_found.y + 0.5f);
            cell_list.Clear();
        }

        // 返回2点的距离
        static int GetDistance(int x0, int y0, int x1, int y1)
        {
            int dx = x0 - x1; if (dx < 0) dx = -dx;
            int dy = y0 - y1; if (dy < 0) dy = -dy;
            int dt = dx - dy;
            if (dt > 0)
            {
                return (dt << 2) + dt + (dy << 3) - dy;
                //return dt * 5 + dy * 7; 
            }
            dt = -dt;
            return (dt << 2) + dt + (dx << 3) - dx;
            //return dt * 5 + dx * 7;
        }

        // 加入额外可通行掩码
        public void ApplyExtraPassable(int x0, int y0, int x1, int y1, byte extra_mask)
        {
            var cells = get_cross_cells(null, x0, y0, x1, y1, false);
            var cnt = cells.Count;
            if (cnt > 0)
            {
                if (masks == mapmask.masks)
                {
                    masks = masks.Clone() as byte[];
                }
                for (int i = 0; i < cnt; ++i)
                {
                    masks[cells[i]] = extra_mask;
                }
                GenAreas();
            }
        }


        //static int _cross_stop_i = 0;
        static List<int> _temp_cross_cells = new List<int>();
        static List<float> _temp_cross_poses = new List<float>();
        //public static List<float> LastCrossPos => _pos;
        List<int>  get_cross_cells(List<int> cells, float x1, float y1, float x2, float y2, bool ret_xy)
        {
            if (cells == null)
            {
                cells = _temp_cross_cells;
            }
            cells.Clear();

            var fdx = x2 - x1;
            var fdy = y2 - y1;

            var bdx = fdx >= 0;
            var bdy = fdy >= 0;

            var x_step1 = bdx ? 1 : -1;
            var y_step2 = bdy ? 1 : -1;

            var pos = _temp_cross_poses;
            pos.Clear();
            pos.Add(x1);
            pos.Add(y1);

            if (fdx == 0 && fdy == 0)
            {
                if (ret_xy)
                {
                    cells.Add((int)x1);
                    cells.Add((int)y1);
                }
                else
                {
                    cells.Add(getidx((int)x1, (int)y1));
                }
                pos.Add(x2);
                pos.Add(y2);
            }
            else
            {
                if ((int)x1 == (int)x2)
                {
                    var dt = Mathf.Abs(fdx / fdy);
                    var x_step2 = bdx ? dt : -dt;
                    float nexty = (int)y1 + (bdy ? 1 : 0);
                    float nextx = x1 + Mathf.Abs(nexty - y1) * x_step2;

                    var ix = (int)x1;
                    var iy = (int)y2;
                    for (var y = (int)y1; bdy ? y < y2 : y >= iy; y += y_step2)
                    {
                        if (ret_xy)
                        {
                            cells.Add(ix);
                            cells.Add(y);
                        }
                        else
                        {
                            cells.Add(getidx(ix, y));
                        }
                        pos.Add(nextx); nextx += x_step2;
                        pos.Add(nexty); nexty += y_step2;
                    }
                    pos.Add(x2);
                    pos.Add(y2);
                }
                else if ((int)y1 == (int)y2)
                {
                    var dt = Mathf.Abs(fdy / fdx);
                    var y_step1 = bdy ? dt : -dt;
                    float nextx = (int)x1 + (bdx ? 1 : 0);
                    float nexty = y1 + Mathf.Abs(nextx - x1) * y_step1;

                    var ix = (int)x2;
                    var iy = (int)y1;
                    for (var x = (int)x1; bdx ? x < x2 : x >= ix; x += x_step1)
                    {
                        if (ret_xy)
                        {
                            cells.Add(x);
                            cells.Add(iy);
                        }
                        else
                        {
                            cells.Add(getidx(x, iy));
                        }
                        pos.Add(nextx); nextx += x_step1;
                        pos.Add(nexty); nexty += y_step1;
                    }
                    pos.Add(x2);
                    pos.Add(y2);
                }
                else
                {

                    var dt = Mathf.Abs(fdy / fdx);
                    var y_step1 = bdy ? dt : -dt;
                    var x_step2 = bdx ? 1 / dt : -1 / dt;

                    float x = (int)x1 + (bdx ? 1 : 0);
                    float nexty = y1 + Mathf.Abs(x - x1) * y_step1;

                    float y = (int)y1 + (bdy ? 1 : 0);
                    float nextx = x1 + Mathf.Abs(y - y1) * x_step2;

                    float lastx = pos[0], lasty = pos[1];
                    while ((bdx ? x < x2 : x > x2) || (bdy ? y < y2 : y > y2))
                    {
                        if (bdx == (x <= nextx))
                        {
                            if (Mathf.Abs(x - lastx) > 0.01f || Mathf.Abs(nexty - lasty) > 0.01f)
                            {
                                lastx = x; lasty = nexty;
                                pos.Add(x);
                                pos.Add(nexty);
                            }
                            x += x_step1;
                            nexty += y_step1;
                        }
                        else
                        {
                            if (Mathf.Abs(nextx - lastx) > 0.01f || Mathf.Abs(y - lasty) > 0.01f)
                            {
                                lastx = nextx; lasty = y;
                                pos.Add(nextx);
                                pos.Add(y);
                            }
                            y += y_step2;
                            nextx += x_step2;
                        }
                    }
                    if (Mathf.Abs(x2 - lastx) > 0.01f || Mathf.Abs(y2 - lasty) > 0.01f)
                    {
                        pos.Add(x2);
                        pos.Add(y2);
                    }                  
                    for (int n = 2, cnt = pos.Count - 1; n < cnt; n += 2)
                    {
                        var xn = (int)((pos[n] + pos[n - 2]) / 2);
                        var yn = (int)((pos[n + 1] + pos[n - 1]) / 2);
                        if (ret_xy)
                        {
                            cells.Add(xn);
                            cells.Add(yn);
                        }
                        else
                        {
                            cells.Add(getidx(xn, yn));
                        }
                    }
                }
            }

            if (ret_xy && pos.Count < cells.Count) 
            {
                Log.LogError($"{x1},{y1} => {x2},{y2}, pos.Count={pos.Count} < cells.Count={cells.Count}");
            }
            return cells;
        }

        void _calc_air_wall_cells(DirMask mask, float dir)
        {
            var go = mask.go;
            var len = go.localScale.x + 1;
            var pos3 = go.position;
            mask.dir = dir;
            mask.pos3 = pos3;
            var y1 = (dir + 90) * Mathf.Deg2Rad;
            var dir1 = new Vector2(Mathf.Sin(y1), Mathf.Cos(y1));
            var y2 = (dir - 90) * Mathf.Deg2Rad;
            var dir2 = new Vector2(Mathf.Sin(y2), Mathf.Cos(y2));

            {
                mask.idxs.Clear();
                var dir0 = new Vector2(Mathf.Sin(dir * Mathf.Deg2Rad), Mathf.Cos(dir * Mathf.Deg2Rad));
                //
                var pos2 = new Vector2(pos3.x, pos3.z);
                var head = pos2 + dir1 * len / 2;
                var tail = pos2 + dir2 * len / 2;
                //
                head -= dir0 / 2f; tail -= dir0 / 2f;
                var cells = get_cross_cells(null, (head.x), (head.y), (tail.x), (tail.y), false);                
                foreach (var id in cells)
                {
                    mask.idxs.Add(id);
                }
                //
                head += dir0; tail += dir0;
                cells = get_cross_cells(null, (head.x), (head.y), (tail.x), (tail.y), false);
                foreach (var id in cells)
                {
                    mask.idxs.Add(id);
                }
            }
            //
#if TEST_AIR_WALLS
            if (mask._cells_root)
            {
                GameObject.Destroy(mask._cells_root);
            }
            var _air_root = mask._cells_root = new GameObject(mask.go.name);
            foreach(var idx in mask.idxs)
            {
                var x = idx % _width;
                var y = (idx - x) / _width;
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.parent = _air_root.transform;
                cube.transform.localScale = new Vector3(0.9f, 2, 0.2f);
                cube.transform.position = new Vector3(x + 0.5f, pos3.y, y + 0.5f);
            }
#endif
        }

        public void addAirWall(Transform t, bool is_single_through)
        {
            air_walls_cells?.Clear();
            t.position = new Vector3( (int)(t.position.x) + 0.5f, t.position.y, (int)(t.position.z) + 0.5f );
            if (air_walls == null)
            {
                air_walls = new Dictionary<Transform, DirMask>();
            }
            air_walls[t] = new DirMask()
            {
                go = t,
                is_single_through = is_single_through,
                idxs = new HashSet<int>(),
                dir = t.eulerAngles.y + 1,//下一帧 触发 _calc_air_wall_cells
                pos3 = Vector3.zero,
            };
        }

        public void removeAirWall(Transform t)
        {
            air_walls_cells?.Clear();
            air_walls.Remove(t);
        }

        public void removeAllAirWall()
        {
            air_walls_cells?.Clear();
            air_walls?.Clear();
        }


        int _isBlockByAriWall_frame = -1;
        List<Transform> _dels = new List<Transform>();
        public bool isBlockByAriWall(float dir, int x, int y, float extra_height)
        {
            if (_isBlockByAriWall_frame != Time.frameCount)
            {
                _isBlockByAriWall_frame = Time.frameCount;
                UnityEngine.Profiling.Profiler.BeginSample("init air_walls_cells");
                {
                    var dirty = false;
                    foreach (var kv in air_walls)
                    {
                        var dirmask = kv.Value;
                        var t = dirmask.go;
                        if (!t)
                        {
                            dirty = true;
                            _dels.Add(t);
                            continue;
                        }

                        var eulerAngles = t.eulerAngles.y;
                        while (eulerAngles < 0) 
                        {
                            eulerAngles += 360;
                        }
                        while (eulerAngles > 360)
                        {
                            eulerAngles -= 360;
                        }
                        var activeInHierarchy = t.gameObject.activeInHierarchy;
                        if (dirmask.dir != eulerAngles || dirmask.pos3 != t.position || dirmask.activeInHierarchy != activeInHierarchy)
                        {
                            dirty = true;
                            if (activeInHierarchy)
                            {
                                _calc_air_wall_cells(dirmask, eulerAngles);
                            }
                            dirmask.dir = eulerAngles;
                            dirmask.pos3 = t.position;
                            dirmask.activeInHierarchy = activeInHierarchy;
                        }
                    }
                    if (_dels.Count > 0)
                    {
                        foreach (var k in _dels)
                        {
                            air_walls.Remove(k);
                        }
                        _dels.Clear();
                    }

                    if (dirty || air_walls_cells == null || air_walls_cells.Count == 0)
                    {
                        if (air_walls_cells == null)
                        {
                            air_walls_cells = new HashSet<int>();
                        }
                        foreach (var kv in air_walls)
                        {
                            if (kv.Value.go.gameObject.activeInHierarchy)
                            {
                                foreach (var id in kv.Value.idxs)
                                {
                                    air_walls_cells.Add(id);
                                }
                            }
                        }
                    }
                }
                UnityEngine.Profiling.Profiler.EndSample();
            }

            var idx = getidx(x, y);
            if (!air_walls_cells.Contains(idx))
            {
                //UnityEngine.Profiling.Profiler.EndSample();
                //Log.LogInfo($"{x},{y} not in air wall");
                return false;
            }

            UnityEngine.Profiling.Profiler.BeginSample("isBlockByAriWall");

            while (dir < 0)
            {
                dir += 360;
            }
            while (dir > 360)
            {
                dir -= 360;
            }

            DirMask debug = null;
            foreach (var kv in air_walls)
            {
                var dirmask  = debug = kv.Value;
                if (dirmask.idxs.Contains(idx))
                {
                    var t = dirmask.go;
                    if (t is null || !dirmask.activeInHierarchy)
                    {
                        continue;
                    }
                    if (extra_height > 0 && extra_height > t.localScale.y)
                    {
                        continue;
                    }
                    if (!dirmask.is_single_through || Mathf.Abs(dir - dirmask.dir) > 89)
                    {
                        UnityEngine.Profiling.Profiler.EndSample();
                        //Log.LogInfo($"{x},{y} block in air wall {t.name}, dir={dir}");
                        return true;
                    }
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();
            //Log.LogInfo($"{x},{y} through air wall, dir={dir}, debug.dir={debug.dir}");
            return false;
        }

        public bool isBlock(int x, int y) 
        {
            if (!IsValid(x, y))
            {
                return true;
            }
            var mask = masks[getidx(x, y)];
            return mask == 0 || mask == 255;
        }

        public bool isPassable(float fromx, float fromy, float tox, float toy, float extra_height, bool check_area, float dir = -1000)
        {
            if (tox < 0 || tox >= _width)
            {
                return false;
            }

            if (toy < 0 || toy >= _height)
            {
                return false;
            }

            var tx = (int)tox;
            var ty = (int)toy;
            if (!IsValid(tx, ty))
            {
                return false;
            }
            var fx = (int)fromx;
            var fy = (int)fromy;
            if (!IsValid(fx, fy))
            {
                return true;
            }

            var m1 = masks[getidx(fx, fy)]; var m2 = masks[getidx(tx, ty)];
            if (!CanPass(m1, m2, extra_height))
            {
                return false;
            }
            //
            if (check_area)
            {
                if (m1 != 0 && m1 != 255)
                {
                    if (area_ids[getidx(fx, fy)] != area_ids[getidx(tx, ty)])
                    {
                        return false;
                    }
                }
            }
            //
            if (air_walls != null && air_walls.Count > 0)
            {
                if (dir == -1000)
                {
                    var dx = tox - fromx;
                    var dy = toy - fromy;
                    dir = Mathf.Atan2(dx, dy) * Mathf.Rad2Deg;
                }
                if (isBlockByAriWall(dir, tx, ty, extra_height))
                {
                    return false;
                }
            }
            return true;
        }

        public byte GetMask(int x, int y)
        {
            var b = masks[getidx(x, y)];
            if (b == 255)
            {
                b = 0;
            }
            return b;
        }

        static List<int> _temp_cross_cells2 = new List<int>();
        public bool GetFarest(float fx, float fy, ref float tx, ref float ty, float extra_height)
        {
            if ((int)fx == (int)tx && (int)fy == (int)ty) 
            {
                //同一个格子，不用判断
                return true;
            }

            var dx = tx - fx;
            var dy = ty - fy;
            var deg = Mathf.Atan2(dx, dy);
            var dir = deg * Mathf.Rad2Deg;
            var cells = get_cross_cells(_temp_cross_cells2, fx, fy, tx, ty, true);
            if (cells.Count < 2) 
            {
                Log.LogError($"scene_id={Log.report_scene_id},w={_width},h={_height}, fx={fx}, fy={fy}, tx={tx}, ty={ty}, cells={string.Join(",", cells)}");
            }
            float x0 = fx, y0 = fy;
            for (int i = 2, cnt0 = cells.Count; i < cnt0; i += 2)
            {
                float x1 = cells[i] + 0.5f, y1 = cells[i + 1] + 0.5f;
                if (!isPassable(x0, y0, x1, y1, extra_height, true, dir))
                {
                    var ok = false;
                    var dist = MathUtils.Distance(fx, fy, x0, y0) + 0.75f;
                    while (dist > 0)
                    {
                        tx = fx + dist * Mathf.Sin(deg);
                        ty = fy + dist * Mathf.Cos(deg);
                        if (isPassable(x0, y0, tx, ty, extra_height, true, dir))
                        {
                            ok = true;
                            break;
                        }
                        dist -= 0.25f;
                    }
                    if (!ok) 
                    {
                        tx = fx;
                        ty = fy;
                    }
                    return false;
                }
                x0 = x1;
                y0 = y1;
            }
            return true;
        }
    }
}
