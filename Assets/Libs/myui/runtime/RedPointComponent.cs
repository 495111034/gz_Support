
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class RedPointInfo
{
    public GameObject go;
    public List<int> bind_packs = new List<int>();
    public List<int> bind_types = new List<int>();

    private bool isFind = false;
    private MyText myText = null;
#if UNITY_EDITOR
    private Dictionary<int, int> log_dic = new Dictionary<int, int>();

    public Dictionary<int, int> GetLog_Dic { get { return log_dic; } }

    public void SetLodDic(int num, int type_id)
    {
        log_dic[type_id] = num;
    }

#endif

    public void SetNum(int num)
    {
        if (go == null)
        {
            return;
        }
        if (!isFind)
        {
            isFind = true;
            if (myText == null)
            {
                Transform textTarget = go.transform.Find("text_point");
                if (textTarget != null)
                {
                    myText = textTarget.gameObject.GetComponent<MyText>();
                }
            }
        }
        if (myText != null)
        {
            myText.text = num.ToString();
        }
        if (go != null)
        {
            go.SetActiveX(num > 0);
        }
    }
}

public class RedPointComponent : MonoBehaviour
{
    public List<RedPointInfo> redPointInfos = new List<RedPointInfo>();
}
