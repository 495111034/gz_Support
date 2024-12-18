using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SmallMapBehaviour : MonoBehaviour
{
    public int tex_width = 256;
    public int tex_height = 256;
    public Texture2D tex;

    public float realWidth;
    public float realHeight;

    public float realWidth2;
    public float realHeight2;

    private Vector3 localLeftBottom;
    private Vector3 localRightTop;

    public Vector3 worldLeftBottom;
    public Vector3 worldRightTop;
    public Vector3 worldRightBottom;

    Camera bindCamera;

    private void Awake()
    {
        bindCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        UpdateCoord();
    }

    public void UpdateCoord()
    {
        worldLeftBottom = bindCamera.ScreenToWorldPoint(Vector3.zero);
        worldRightTop = bindCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        worldRightBottom = bindCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));

        localLeftBottom = this.transform.InverseTransformPoint(worldLeftBottom);
        localRightTop = this.transform.InverseTransformPoint(worldRightTop);

        realWidth = localRightTop.x - localLeftBottom.x;
        realHeight = localRightTop.y - localLeftBottom.y;

        realWidth2 = (worldRightBottom - worldLeftBottom).magnitude;
        realHeight2 = (worldRightTop - worldRightBottom).magnitude;
    }
}
