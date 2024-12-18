using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundBhv : MonoBehaviour
{
    public string sound;
    // Start is called before the first frame update
    private void OnEnable()
    {
        if (!string.IsNullOrEmpty(sound) && RuntimeAudoiPlayer.Play3DAudio1 != null)
        {
            RuntimeAudoiPlayer.Play3DAudio1( sound, null, 0 );
        }
    }
}


