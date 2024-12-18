
using System.Collections.Generic;
using UnityEngine;

public class SceneRootUniqueChild : MonoBehaviour 
{
    [System.NonSerialized]
    public Dictionary<string, Transform> UniqueChild = new Dictionary<string, Transform>();
}