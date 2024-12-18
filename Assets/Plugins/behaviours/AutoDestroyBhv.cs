using UnityEngine;
public class AutoDestroyBhv : MonoBehaviour 
{
    public Object obj;
    private void Start()
    {
        this.enabled = false;
    }
    private void OnDestroy()
    {
        //Log.LogError($"Destroy {obj}");
        Object.Destroy(obj);
    }
}

