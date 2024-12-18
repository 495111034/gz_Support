using UnityEngine;
using System;
using System.Collections;

public class AnimClipEvent : MonoBehaviour
{
    public Action OnFireEvent;

    //
    void EventFire()
    {
        OnFireEvent?.Invoke();
    }

    void EventEnd()
    {
    }

    void OnBecameVisible()
    {
        Debug.LogError(gameObject.name + " OnBecameVisible");
        if (gameObject.GetComponent<Animation>())
        {
            gameObject.GetComponent<Animation>().enabled = true;
        }
    }

    void OnBecameInvisible()
    {
        Debug.LogError(gameObject.name + " OnBecameInvisible");
        if (gameObject.GetComponent<Animation>())
        {
            gameObject.GetComponent<Animation>().enabled = false;
        }
    }

    private void OnDestroy()
    {
        OnFireEvent = null;
    }
}
