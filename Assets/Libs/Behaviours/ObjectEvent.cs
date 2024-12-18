using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectEvent : MonoBehaviour {

    public Action OnUpdate;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        OnUpdate?.Invoke();

    }   

    void OnTrigTerrain()
    {        
        SendMessageUpwards("__OnTrigTerrain");
    }

    void OnDie()
    {
        SendMessageUpwards("__OnDie");
    }

    void OnBecameVisible()
    {       
        if (gameObject.GetComponent<Animation>()) gameObject.GetComponent<Animation>().enabled = true;
        if (gameObject.GetComponent<Animator>()) gameObject.GetComponent<Animator>().enabled = true;
    }

    void OnBecameInvisible()
    {       
        if (gameObject.GetComponent<Animation>()) gameObject.GetComponent<Animation>().enabled = false;       
        if (gameObject.GetComponent<Animator>()) gameObject.GetComponent<Animator>().enabled = false;
    }

}
