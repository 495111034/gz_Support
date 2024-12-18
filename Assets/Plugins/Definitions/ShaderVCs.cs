

using UnityEngine;


[System.Serializable]
public class ShaderVCs : ScriptableObject
{
    [SerializeField]
    public ShaderVariantCollection[] ShaderVariantCollections;

    [SerializeField]
    public Shader Shader;
}