using System.Collections.Generic;
using UnityEngine;
using static iTween;
namespace GameBase.utils
{
    enum iTweenLiteInfoType
    {
        None,
        Move,
        Scale,
        Alpha,
    }

    public class iTweenLiteResult
    {
        public float process;
    }

    class iTweenLiteInfo
    {
        public iTweenLiteInfoType type;
        public Transform go;
        public float start_time;
        public float time;
        public float end_time;
        public Vector3 from;
        public Vector3 end;
        public iTweenLiteResult ret;
        public EasingFunction ease;
        public float easeprocess;

        public void Reset()
        {
            easeprocess = 0;
            ease = null;
            type = iTweenLiteInfoType.None;
            go = null;
            if (ret != null)
            {
                ret.process = 1;
                ret = null;
            }
        }
    }

    public static class iTweenLite
    {

        static LinkedList<iTweenLiteInfo> _Working = new LinkedList<iTweenLiteInfo>();
        static Stack<LinkedListNode<iTweenLiteInfo>> _pool = new Stack<LinkedListNode<iTweenLiteInfo>>();

        public static void ScaleTo(Transform go, Vector3 to, float time, iTween.EaseType easetype, iTweenLiteResult ret)
        {
            _Add( iTweenLiteInfoType.Scale, go, to, time, easetype, ret);
        }

        public static void MoveTo(Transform go, Vector3 to, float time, iTween.EaseType easetype, iTweenLiteResult ret) 
        {
            _Add(iTweenLiteInfoType.Move, go, to, time, easetype, ret);
        }

        public static void AlphaTo(Transform go, float from, float to, float time, iTween.EaseType easetype, iTweenLiteResult ret)
        {
            _Add(iTweenLiteInfoType.Alpha, go, new Vector3(from, to, 0), time, easetype, ret);
        }

        public static void AlphaSet(Transform go, float to)
        {
            var alpha = go.gameObject.GetComponent<CanvasGroup>();
            if (alpha)
            {
                alpha.alpha = to;
            }
        }

        static void _Add(iTweenLiteInfoType type, Transform go, Vector3 to, float time, iTween.EaseType easetype, iTweenLiteResult ret) 
        {
            if (time <= 0)
            {
                time = 0.001f;
            }

            if (ret != null) 
            {
                ret.process = 0;
            }

            LinkedListNode<iTweenLiteInfo> node = null;
            if (_pool.Count > 0)
            {
                node = _pool.Pop();
            }
            else
            {
                node = new LinkedListNode<iTweenLiteInfo>(new iTweenLiteInfo());
            }
            var value = node.Value;
            value.type = type;
            value.start_time = TimeUtils.time;
            value.time = time;
            value.end_time = TimeUtils.time + time;
            value.go = go;
            value.ret = ret;
            value.ease = iTween.GetEasingFunction(easetype);
            value.end = to;

            switch (value.type)
            {
                case iTweenLiteInfoType.Move:
                    value.from = go.localPosition;                    
                    break;

                case iTweenLiteInfoType.Scale:
                    value.from = go.localScale;
                    break;

                case iTweenLiteInfoType.Alpha:
                    var alpha = go.gameObject.AddMissingComponent<CanvasGroup>();
                    if (!alpha.enabled)
                    {
                        alpha.enabled = true;
                    }
                    alpha.alpha = to.x;
                    break;
            }
            //Debug.Log($"{value.type}, ease={value.ease.Method.Name}, {value.from}->{value.end}, {value.GetHashCode()}");
            _Working.AddLast(node);
        }

        static bool _Check(float nowtime, iTweenLiteInfo info)
        {
            if (!info.go)
            {
                return true;
            }
            var passed = nowtime - info.start_time;
            var process = passed / info.time;
            var t = info.ease(0, 1, Mathf.Min(1, process));
            //Debug.Log($"{info.type}, process={process}={passed}/{info.time}, {info.ease.Method.Name}={t}, {info.from}->{info.end}, {info.GetHashCode()}");
            if (t > 1)
            {
                t = 1;
            }
            switch (info.type)
            {
                case iTweenLiteInfoType.Move:
                    info.go.localPosition = Vector3.Lerp(info.from, info.end, t);
                    //Debug.Log($"dist={(info.go.localPosition-info.from).magnitude}, dt={t-info.easeprocess}*100={t*100}-{info.easeprocess*100}, passed={passed}, process={process}");
                    break;

                case iTweenLiteInfoType.Scale:
                    info.go.localScale = Vector3.Lerp(info.from, info.end, t);
                    break;

                case iTweenLiteInfoType.Alpha:
                    var alpha = info.go.GetComponent<CanvasGroup>();
                    alpha.alpha = Mathf.Lerp(info.end.x, info.end.y, t);
                    break;
            }
            info.easeprocess = t;
            if (info.ret != null)
            {
                info.ret.process = process;
            }
            return process >= 1;
        }

        public static void LateUpdate()
        {
            var nowtime = TimeUtils.time;
            var node = _Working.First;
            while (node != null)
            {
                var now = node;
                node = node.Next;
                if (_Check(nowtime, now.Value))
                {
                    now.Value.Reset();
                    _Working.Remove(now);
                    if (_pool.Count < 100)
                    {
                        _pool.Push(now);
                    }
                }
            }
        }
    }
}
