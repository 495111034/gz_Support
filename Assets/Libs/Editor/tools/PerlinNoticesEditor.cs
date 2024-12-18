using System.IO;
using UnityEditor;
using UnityEngine;

namespace Perlin
{

    /// <summary>
    /// 噪音生成工具，用于生成随机噪音干扰、随机高度等数据
    /// </summary>
    public class PerlinNoticesEditor : ScriptableWizard
    {
        public enum TestTarget
        {
            Noise1D, Noise2D, Noise3D
        }

        public TestTarget target = TestTarget.Noise3D;

        [Range(1, 5)]
        public int fractalLevel = 3;
        public int Size = 128;
        public string textureName = "notices";
        void OnWizardUpdate()
        {
            helpString = "选择噪音维度与分形布朗等级";
            isValid = fractalLevel >= 1 && fractalLevel <= 5 && (int)target >= 0 && (int)target <= 2 && Size > 1 && !string.IsNullOrEmpty(textureName);
        }

        void OnWizardCreate()
        {


            Texture2D texture = new Texture2D(Size, Size);
            texture.wrapMode = TextureWrapMode.Clamp;

            if (target == TestTarget.Noise1D)
                UpdateTexture((x, y, t) => PerlinNoise.Fbm(x + t, fractalLevel), texture);
            else if (target == TestTarget.Noise2D)
                UpdateTexture((x, y, t) => PerlinNoise.Fbm(x + t, y, fractalLevel), texture);
            else
                UpdateTexture((x, y, t) => PerlinNoise.Fbm(x, y, t, fractalLevel), texture);
        }

        void UpdateTexture(System.Func<float, float, float, float> generator, Texture2D texture)
        {
            var scale = 1.0f / Size;
            var time = Time.time;

            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var n = generator.Invoke(x * scale, y * scale, time);
                    texture.SetPixel(x, y, Color.white * (n / 1.4f + 0.5f));
                }
            }

            texture.Apply();

            string assetPath = System.IO.Path.Combine(PathDefs.ASSETS_PATH_BUILD_TEMP, $"Notices/{textureName}");
            string dirname = System.IO.Path.GetDirectoryName(assetPath);

            if (!System.IO.Directory.Exists(PathDefs.ASSETS_PATH_BUILD_TEMP))
            {
                System.IO.Directory.CreateDirectory(PathDefs.ASSETS_PATH_BUILD_TEMP);
            }
            if (!System.IO.Directory.Exists(dirname))
            {
                System.IO.Directory.CreateDirectory(dirname);
            }
            string fileName = assetPath;
            int idx = 0;
            while (System.IO.File.Exists(fileName + ".png"))
            {
                fileName = assetPath + $"_{idx}";
                idx++;
            }
            using (FileStream fileStream = new FileStream(fileName + ".png", FileMode.Create))
            {
                byte[] bytes = texture.EncodeToPNG();
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Flush();
                fileStream.Close();
                fileStream.Dispose();
            }
            Log.LogInfo($"生成噪音图：{fileName}");
            AssetDatabase.Refresh();
        }

        [MenuItem("Export/生成噪音图")]
        public static void CreateNoticeTexture()
        {
            ScriptableWizard.DisplayWizard<PerlinNoticesEditor>(
                "创建噪音纹理", "创建");

        }
    }

}
