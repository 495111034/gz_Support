

using UnityEngine;

public class GameObjectRotationBhv : MonoBehaviour 
{
    public bool x, y = true, z;
    public float speedx = 50, speedy = 50, speedz = 50;


    //float[] start_time = new float[3];
    //Vector3 start;
    private void OnEnable()
    {
        //start = transform.eulerAngles;
        //start_time[0] = start_time[1] = start_time[2] = Time.time;
    }

    private void Update()
    {
        var an = Vector3.zero;
        if (x)
        {
            an.x += speedx * Time.deltaTime;
        }

        if (y)
        {
            an.y += speedy * Time.deltaTime;
        }

        if (z)
        {
            an.z += speedz * Time.deltaTime;
        }

        transform.Rotate(an);
    }
}