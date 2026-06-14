using UnityEngine;

public class BlueNoiseGenerator : MonoBehaviour
{
    public static Texture2D Generate(int size = 128)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var colors = new Color[size * size];

        // 多层噪声叠加，更接近Blue Noise效果
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float n = 0f;
                // 第一层：大尺度
                n += Mathf.PerlinNoise(x * 0.3f + 42.0f, y * 0.3f + 17.0f) * 0.5f;
                // 第二层：中尺度
                n += Mathf.PerlinNoise(x * 0.7f + 100f, y * 0.7f + 50f) * 0.3f;
                // 第三层：小尺度
                n += Mathf.PerlinNoise(x * 1.5f + 200f, y * 1.5f + 100f) * 0.2f;
                // 第四层：极细微细节
                n += Mathf.PerlinNoise(x * 3.0f + 300f, y * 3.0f + 200f) * 0.1f;

                n = Mathf.Clamp01(n);
                colors[y * size + x] = new Color(n, n, n, 1f);
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Point;
        return tex;
    }
}
