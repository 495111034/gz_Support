using UnityEngine;
using System.Collections;

public class MonoBehaviourParam : MonoBehaviour {

    public object Param;
	void Start () {
	
	}
	
	// Update is called once per frame
//	void Update () {
//	
//	}

    private void OnDestroy()
    {
        Param = null;
    }
}
