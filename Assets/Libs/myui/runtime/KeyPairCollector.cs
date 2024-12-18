using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class KeyPairCollectorData
{
    public string remark;
    public string key;
    public int content;
}

public class KeyPairCollector : MonoBehaviour, ISerializationCallbackReceiver
{
    public List<KeyPairCollectorData> IntParams = new List<KeyPairCollectorData>();

    public readonly Dictionary<string, int> Dict = new Dictionary<string, int>();

    private Action action_replay;

    public int GetOrDefault(string key, int defaultValue)
    {
        int dictGo;
        if (!Dict.TryGetValue(key, out dictGo))
        {
            return defaultValue;
        }
        return dictGo;
    }

    public int Get(string key)
    {
        int dictGo;
        if (!Dict.TryGetValue(key, out dictGo))
        {
            return 0;
        }
        return dictGo;
    }

    public void OnBeforeSerialize()
    {
    }

    public void OnAfterDeserialize()
    {
        Dict.Clear();
        foreach (var pair in IntParams)
        {
            if (!Dict.ContainsKey(pair.key))
            {
                Dict.Add(pair.key, pair.content);
            }
        }
    }

    public void SetReplayAction(Action action)
    {
#if UNITY_EDITOR
        action_replay = action;
#endif
    }

    [ContextMenu("Replay")]
    private void DoReplay()
    {
#if UNITY_EDITOR
        action_replay?.Invoke();
#endif
    }
}