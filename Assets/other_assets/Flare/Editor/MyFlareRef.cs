using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyFlareRef : MonoBehaviour
{
    public Material flareRefMaterial;
    // Start is called before the first frame update
    void Start()
    {
        flareRefMaterial = new Material(Shader.Find("Hidden/Internal-Flare"));
        // 将材质保存到Assets文件夹中
        string path = "Assets/other_assets/Flare/MyFlareRef.mat";
        UnityEditor.AssetDatabase.CreateAsset(flareRefMaterial, path);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
    }

    private void OnDestroy()
    {
        Destroy(flareRefMaterial);
        flareRefMaterial = null;
    }
}
