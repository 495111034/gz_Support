
/// <summary>
/// 数值统计
/// 
///     . 输入多个数值, 给出统计值(min/max/avg)
///     
/// </summary>
public class DataStatistics
{
    float[] _data_arr;
    int _next_idx;

    float _last_value, _min_value, _max_value, _total_value;


    //
    public DataStatistics(int num_samples)
    {
        _data_arr = new float[num_samples];
        _next_idx = 0;
    }

    // 添加一个数据
    public void AddValue(float value)
    {
        // 添加
        _data_arr[_next_idx] = value;
        _next_idx = (_next_idx + 1) % _data_arr.Length;

        // 统计
        _last_value = value;
        _max_value = float.MinValue;
        _min_value = float.MaxValue;
        _total_value = 0;
        foreach (var t in _data_arr)
        {
            _total_value += t;
            if (t > _max_value) _max_value = t;
            if (t < _min_value) _min_value = t;
        }
    }
        
    // 获取统计值
    public float MinValue { get { return _min_value; } }
    public float MaxValue { get { return _max_value; } }
    public float TotalValue { get { return _total_value; } }
    public float AvgValue { get { return _total_value / _data_arr.Length; } }
    public float LastValue { get { return _last_value; } }
    public int NumSamples { get { return _data_arr.Length; } }
}
