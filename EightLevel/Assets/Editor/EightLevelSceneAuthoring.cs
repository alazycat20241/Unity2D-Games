using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class EightLevelSceneAuthoring
{
    private const float LevelSpacing = 60f;

    private static readonly string[] LevelNames =
    {
        "掉下去",
        "节拍",
        "别碰天花板",
        "会动的路",
        "冷却",
        "推一把",
        "弹上去",
        "全部"
    };

    [MenuItem("Tools/EightLevel/Build Authored Levels")]
    private static void BuildAuthoredLevels()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "EightLevel",
                "Exit Play Mode before building authored levels.",
                "OK");
            return;
        }

        if (GameObject.Find("Authored Levels") != null
            && !EditorUtility.DisplayDialog(
                "Rebuild authored levels?",
                "This replaces the existing Authored Levels hierarchy and any manual edits inside it.",
                "Rebuild",
                "Cancel"))
        {
            return;
        }

        EnsureSpriteAssets();

        GameBootstrap bootstrap = Object.FindObjectOfType<GameBootstrap>();
        if (bootstrap == null)
        {
            GameObject bootstrapObject = new GameObject("Game Bootstrap");
            bootstrap = bootstrapObject.AddComponent<GameBootstrap>();
        }

        GameObject previous = GameObject.Find("Authored Levels");
        if (previous != null)
        {
            Object.DestroyImmediate(previous);
        }

        GameObject container = new GameObject("Authored Levels");
        container.transform.position = Vector3.zero;

        for (int levelIndex = 0; levelIndex < LevelNames.Length; levelIndex++)
        {
            GameObject levelRoot = new GameObject($"Level {levelIndex + 1:00} - {LevelNames[levelIndex]}");
            levelRoot.transform.SetParent(container.transform, false);
            levelRoot.transform.localPosition = new Vector3(levelIndex * LevelSpacing, 0f, 0f);

            LevelAuthoringSnapshot snapshot = bootstrap.BuildLevelContentsForAuthoring(
                levelIndex,
                levelRoot.transform);

            LevelLayout layout = levelRoot.AddComponent<LevelLayout>();
            layout.MinX = snapshot.MinX;
            layout.MaxX = snapshot.MaxX;

            GameObject spawn = new GameObject("Spawn");
            spawn.transform.SetParent(levelRoot.transform, false);
            spawn.transform.localPosition = snapshot.Spawn;
            spawn.AddComponent<SpawnPointMarker>();
        }

        Selection.activeGameObject = container;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("EightLevel authored levels created and saved. Edit them under 'Authored Levels'.");
    }

    private static void EnsureSpriteAssets()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/EightLevel");
        EnsureFolder("Assets/Resources/EightLevel/Sprites");

        EnsureSprite("square", 8, 8, (x, y, width, height) => Color.white);
        EnsureSprite("circle", 96, 96, CirclePixel);
        EnsureSprite("rounded_square", 96, 96, RoundedSquarePixel);
        EnsureSprite("triangle", 64, 64, TrianglePixel);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureSprite(
        string assetName,
        int width,
        int height,
        System.Func<int, int, int, int, Color> pixelFactory)
    {
        string path = $"Assets/Resources/EightLevel/Sprites/{assetName}.png";
        if (!File.Exists(path))
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[y * width + x] = pixelFactory(x, y, width, height);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        bool changed = importer.textureType != TextureImporterType.Sprite
            || importer.spriteImportMode != SpriteImportMode.Single
            || importer.spritePixelsPerUnit != width
            || !importer.alphaIsTransparency
            || importer.mipmapEnabled
            || importer.filterMode != FilterMode.Bilinear
            || importer.wrapMode != TextureWrapMode.Clamp;

        if (!changed)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = width;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static Color CirclePixel(int x, int y, int width, int height)
    {
        Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);
        float radius = Mathf.Min(width, height) * 0.47f;
        float distance = Vector2.Distance(new Vector2(x, y), center);
        return new Color(1f, 1f, 1f, Mathf.Clamp01(radius - distance + 0.5f));
    }

    private static Color RoundedSquarePixel(int x, int y, int width, int height)
    {
        float radius = width * 0.208f;
        float dx = Mathf.Max(0f, Mathf.Max(radius - x, x - (width - 1 - radius)));
        float dy = Mathf.Max(0f, Mathf.Max(radius - y, y - (height - 1 - radius)));
        float alpha = Mathf.Clamp01(radius + 0.5f - new Vector2(dx, dy).magnitude);
        return new Color(1f, 1f, 1f, alpha);
    }

    private static Color TrianglePixel(int x, int y, int width, int height)
    {
        float halfWidth = (1f - y / (float)(height - 1)) * (width * 0.5f - 1f);
        float center = (width - 1) * 0.5f;
        return new Color(1f, 1f, 1f, Mathf.Clamp01(halfWidth - Mathf.Abs(x - center) + 0.6f));
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folderName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent))
        {
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
