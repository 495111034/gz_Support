using UnityEngine;
using UnityEngine.UI;

public class TriggerEnterExitBehaviour : MonoBehaviour
{
    public int trigger_id = 0;
    public System.Action<TriggerEnterExitBehaviour, GameObject> onTriggerEnter;
    public System.Action<TriggerEnterExitBehaviour, GameObject> onTriggerExit;
    void OnTriggerEnter(Collider col)
    {
        if (col && col.gameObject)
        {
            onTriggerEnter?.Invoke(this, col.gameObject);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col && col.gameObject)
        {
            onTriggerExit?.Invoke(this, col.gameObject);
        }
    }
}

