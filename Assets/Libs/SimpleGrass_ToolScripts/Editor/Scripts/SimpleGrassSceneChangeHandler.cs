using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace SimpleGrass
{
    [InitializeOnLoad]
    public class SceneChangeHandler
    {
        static SceneChangeHandler()
        {
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
        }

        private static void OnSceneChanged(Scene previousScene, Scene newScene)
        {
            InitGrassColor();
        }

        static void InitGrassColor()
        {
            // 在场景加载完成后执行的操作
            var grassChunks = Object.FindObjectsOfType<SimpleGrassChunk>();
            for (int i = 0; i < grassChunks.Length; i++)
            {
                var grassChunk = grassChunks[i];
                var childs = grassChunk.GetComponentsInChildren<MeshRenderer>();
                if (childs.Length > 0)
                {
                    grassChunk.childMaterialBlocks = new MaterialPropertyBlock[childs.Length];
                    for (int j = 0; j < grassChunk.childColors.Length; j++)
                    {
                        var materialBlock = new MaterialPropertyBlock();
                        var meshRenderer = childs[j];
                        grassChunk.childMaterialBlocks[j] = materialBlock;
                        meshRenderer.GetPropertyBlock(materialBlock);
                        Color tempColor = new Color(grassChunk.childColors[j].x, grassChunk.childColors[j].y, grassChunk.childColors[j].z, 1);
                        materialBlock.SetColor("_Color1", tempColor.gamma);
                        meshRenderer.SetPropertyBlock(materialBlock);
                    }
                }
            }

        }

    }
}