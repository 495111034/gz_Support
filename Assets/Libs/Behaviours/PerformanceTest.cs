using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

public class PerformanceTest : MonoBehaviour
{
    public Mesh mesh;
    public Material mat;
    public float TestScore = 0;// 性能测试评分
    public int state = 0;
    public System.Action<PerformanceTest> state_update;
    public int excute_count = 1;

    private int _TotalDrawTimes = 0;
    private long _TotalDrawMilliseconds = 0;
    private int frameCount = 0;

    private float _total_score = 0;
    private int _excute_total_count = 0;

    void OnPostRender()
    {
        switch (state)
        {
            case 0:
                {
                    state = 1;
                    _total_score = 0;
                    _excute_total_count = excute_count;
                }
                break;
            case 1:
                {
                    mat.SetPass(0);

                    int draw_times = 0;
                    long milliseconds = 0;

                    Stopwatch sw = new Stopwatch();

                    for (int n = 0; n < 1000; ++n)
                    {
                        sw.Start();

                        for (int i = 0; i < 100; ++i)
                        {
                            Graphics.DrawMeshNow(mesh, new Vector3(0, 0, -10000), Quaternion.identity);
                            draw_times++;
                        }

                        sw.Stop();

                        milliseconds += sw.ElapsedMilliseconds;
                        if (milliseconds > 16)
                        {
                            break;
                        }
                    }

                    _TotalDrawTimes += draw_times;
                    _TotalDrawMilliseconds += milliseconds;

                    this.TestScore = 1.0f * _TotalDrawTimes / _TotalDrawMilliseconds;

                    frameCount++;
                    if (frameCount > 10)// 采样N帧数据
                    {
                        UnityEngine.Debug.Log("采样N帧数据: " + TestScore);
                        _total_score += TestScore;
                        excute_count--;
                        if (excute_count <= 0)
                        {
                            TestScore = _total_score / _excute_total_count;
                            UnityEngine.Debug.LogFormat("Performance test draw mesh time(MS): {0}, draw times: {1}, score: {2}", _TotalDrawMilliseconds / 10, _TotalDrawTimes / 10, this.TestScore);
                            state = 2;
                        }
                        else
                        {
                            _TotalDrawTimes = 0;
                            _TotalDrawMilliseconds = 0;
                            frameCount = 0;
                        }
                    }
                }
                break;
            default:
                {

                }
                break;
        }
        state_update?.Invoke(this);
    }
}
