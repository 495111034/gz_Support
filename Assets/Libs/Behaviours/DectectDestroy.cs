using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DectectDestroy : MonoBehaviour
{
    private void OnDestroy()
    {
        Log.Log2File($"DectectDestroy {name}, path={gameObject.GetLocation()}\n{new System.Diagnostics.StackTrace(true)}");
    }
}
