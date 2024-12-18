using System.Collections.Generic;

namespace UnityEngine.UI
{
    /// <summary>
    /// 用于关联子级所有Animator动画，并可按延迟顺序播放
    /// </summary>
    public class UITimeSortChildPlayAnima : MonoBehaviour
    {
        public float invterval = 0;

        private int index = 0;
        private float time = 0;
        private List<GameObject> childs = new List<GameObject>();
        private Transform trans;
        private int childCount = 0;
        private void Start()
        {
            trans = transform;
            time = invterval;
            trans.localScale = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (trans.childCount != childCount)
            {
                childCount = trans.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    var child = trans.GetChild(i);
                    if (child.gameObject.activeSelf)
                    {
                        child.gameObject.SetActiveX(false);
                        if (!childs.Contains(child.gameObject)) childs.Add(child.gameObject);
                    }
                }
                trans.localScale = Vector3.one;
            }
            else
            {
                time -= Time.deltaTime;
                if (time < 0)
                {
                    time = invterval;
                    if (index < childs.Count)
                    {
                        childs[index++].gameObject.SetActiveX(true);
                    }
                    else
                    {
                        this.enabled = false;
                    }
                }
            }
        }
    }
}
