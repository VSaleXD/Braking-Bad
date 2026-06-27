#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class GlowSpriteGenerator : EditorWindow
{
    private int textureSize = 256;
    private Color glowColor = new Color(0.3f, 0.6f, 1f, 1f);

    [MenuItem("Tools/Generate Glow Sprite")]
    public static void ShowWindow()
    {
        GetWindow<GlowSpriteGenerator>("Glow Sprite Generator");
    }

    private void OnGUI()
    {
        textureSize = EditorGUILayout.IntField("Texture Size", textureSize);
        glowColor = EditorGUILayout.ColorField("Glow Color", glowColor);

        if (GUILayout.Button("Generate Glow Sprite"))
        {
            GenerateAndSave();
        }
    }

    private void GenerateAndSave()
    {
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
        float maxRadius = textureSize / 2f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float normalizedDist = Mathf.Clamp01(distance / maxRadius);

                // Radial falloff: terang di tengah, transparan di pinggir
                float alpha = Mathf.Pow(1f - normalizedDist, 2f);
                Color pixelColor = glowColor;
                pixelColor.a *= alpha;

                texture.SetPixel(x, y, pixelColor);
            }
        }

        texture.Apply();

        byte[] pngData = texture.EncodeToPNG();
        string path = "Assets/Sprite/GlowCircle.png";

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, pngData);

        AssetDatabase.Refresh();

        // Auto-set import settings jadi Sprite
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        Debug.Log($"Glow sprite generated at {path}");
    }
}
#endif