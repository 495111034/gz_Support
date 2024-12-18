using UnityEngine;
public class ObjectVisiable : MonoBehaviour
{
    private void OnBecameVisible()
    {
        var go = this.gameObject.GetComponentInParent<ObjectBehaviourBase>();
        if (go)
        {
            go.isCameraVisible = true;
        }
    }
    private void OnBecameInvisible()
    {
        var go = this.gameObject.GetComponentInParent<ObjectBehaviourBase>();
        if (go)
        {
            go.isCameraVisible = false;
        }
    }
}



