
using System.Collections.Generic;
using UnityEngine;

public class MyTest : MonoBehaviour
{
    public ProjectorShadow pw;
    public List<SceneObjectShadow> lstObj;
    // Start is called before the first frame update
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        pw.UpdateRoleList<SceneObjectShadow>(lstObj);
    }
}
