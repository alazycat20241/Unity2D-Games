using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 运行时美术工具类
/// 没有美术资源时，用代码直接生成方块、圆、圆角方块、三角这些基础图形
/// 还能生成粒子、线条材质，保证项目零资源也能跑起来
/// 所有资源都带缓存，第一次访问时生成，之后直接复用
/// 如果 Resources 下存在同名 Sprite，就优先加载外部资源，否则代码生成
/// </summary>
public static class RuntimeArt
{
    // 缓存的资源，懒加载：第一次用到时才创建，之后复用
    private static Sprite square;          // 方块
    private static Sprite circle;          // 圆
    private static Sprite roundedSquare;   // 圆角方块
    private static Sprite triangle;        // 三角
    private static Material lineMaterial;  // 线条和精灵通用材质

    /// <summary>
    /// 方块 Sprite
    /// 优先从 Resources/EightLevel/Sprites/square 加载，没有就用 8x8 纯白贴图生成一个
    /// </summary>
    public static Sprite Square
    {
        get
        {
            if (square == null)
            {
                square = Resources.Load<Sprite>("EightLevel/Sprites/square");
                if (square == null)
                {
                    // 外部资源不存在，代码生成一个 8x8 纯白方块贴图
                    Texture2D texture = MakeTexture(8, 8, Color.white);
                    square = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),   // 轴心放中心
                        texture.width,             // Pixels Per Unit 等于贴图宽度，让 1 格 = 1 单位
                        0,
                        SpriteMeshType.FullRect);
                    square.name = "Runtime Square";
                }
            }

            return square;
        }
    }

    /// <summary>
    /// 圆 Sprite
    /// 优先从 Resources 加载，没有就用 96x96 贴图逐像素生成一个抗锯齿的圆
    /// </summary>
    public static Sprite Circle
    {
        get
        {
            if (circle == null)
            {
                circle = Resources.Load<Sprite>("EightLevel/Sprites/circle");
                if (circle == null)
                {
                    const int size = 96;
                    Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                    texture.name = "Runtime Circle";
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.filterMode = FilterMode.Bilinear;
                    Color[] pixels = new Color[size * size];

                    // 圆心和半径，半径取 0.47 留一点边，避免贴边被裁掉
                    Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
                    float radius = size * 0.47f;

                    // 逐像素算到圆心的距离，alpha 用 clamp 做边缘过渡，看起来更平滑
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float distance = Vector2.Distance(new Vector2(x, y), center);
                            float alpha = Mathf.Clamp01(radius - distance + 0.5f);
                            pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                        }
                    }

                    texture.SetPixels(pixels);
                    texture.Apply(false, true);
                    circle = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, size, size),
                        new Vector2(0.5f, 0.5f),
                        size,
                        0,
                        SpriteMeshType.FullRect);
                    circle.name = "Runtime Circle Sprite";
                }
            }

            return circle;
        }
    }

    /// <summary>
    /// 圆角方块 Sprite
    /// 优先从 Resources 加载，没有就用 96x96 贴图生成一个四角带圆角的方块
    /// </summary>
    public static Sprite RoundedSquare
    {
        get
        {
            if (roundedSquare == null)
            {
                roundedSquare = Resources.Load<Sprite>("EightLevel/Sprites/rounded_square");
                if (roundedSquare == null)
                {
                    const int size = 96;
                    const float radius = 20f;
                    Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                    texture.name = "Runtime Rounded Square";
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.filterMode = FilterMode.Bilinear;
                    Color[] pixels = new Color[size * size];

                    // 逐像素判断是否在"圆角矩形"范围内
                    // dx/dy 是当前像素超出直边区域的偏移量，只在角上才不为 0
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dx = Mathf.Max(0f, Mathf.Max(radius - x, x - (size - 1 - radius)));
                            float dy = Mathf.Max(0f, Mathf.Max(radius - y, y - (size - 1 - radius)));
                            float alpha = Mathf.Clamp01(radius + 0.5f - new Vector2(dx, dy).magnitude);
                            pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                        }
                    }

                    texture.SetPixels(pixels);
                    texture.Apply(false, true);
                    roundedSquare = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, size, size),
                        new Vector2(0.5f, 0.5f),
                        size,
                        0,
                        SpriteMeshType.FullRect);
                    roundedSquare.name = "Runtime Rounded Square Sprite";
                }
            }

            return roundedSquare;
        }
    }

    /// <summary>
    /// 三角 Sprite
    /// 优先从 Resources 加载，没有就用 64x64 贴图生成一个尖朝上的三角
    /// </summary>
    public static Sprite Triangle
    {
        get
        {
            if (triangle == null)
            {
                triangle = Resources.Load<Sprite>("EightLevel/Sprites/triangle");
                if (triangle == null)
                {
                    const int size = 64;
                    Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                    texture.name = "Runtime Triangle";
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.filterMode = FilterMode.Bilinear;
                    Color[] pixels = new Color[size * size];

                    // 越往上的行越窄，形成三角形
                    // halfWidth 是当前行的一半宽度，center 是水平中心
                    for (int y = 0; y < size; y++)
                    {
                        float halfWidth = (1f - y / (float)(size - 1)) * (size * 0.5f - 1f);
                        float center = (size - 1) * 0.5f;
                        for (int x = 0; x < size; x++)
                        {
                            float alpha = Mathf.Clamp01(halfWidth - Mathf.Abs(x - center) + 0.6f);
                            pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                        }
                    }

                    texture.SetPixels(pixels);
                    texture.Apply(false, true);
                    triangle = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, size, size),
                        new Vector2(0.5f, 0.5f),
                        size,
                        0,
                        SpriteMeshType.FullRect);
                    triangle.name = "Runtime Triangle Sprite";
                }
            }

            return triangle;
        }
    }

    /// <summary>
    /// 线条和精灵通用材质
    /// 用 Sprites/Default 着色器，第一次访问时创建
    /// </summary>
    public static Material LineMaterial
    {
        get
        {
            if (lineMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                lineMaterial = new Material(shader)
                {
                    name = "Runtime Line Material"
                };
            }

            return lineMaterial;
        }
    }

    /// <summary>
    /// 给目标物体加一个 SpriteRenderer 并设置好精灵、颜色、排序
    /// 返回这个 renderer 方便后面继续用
    /// </summary>
    public static SpriteRenderer AddSprite(
        GameObject target,
        Sprite sprite,
        Color color,
        int sortingOrder = 0)
    {
        SpriteRenderer renderer = target.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    /// <summary>
    /// 快速创建一个带精灵的 GameObject
    /// 指定名字、父物体、位置、大小、精灵、颜色、排序
    /// </summary>
    public static GameObject CreateSpriteObject(
        string name,
        Transform parent,
        Vector2 position,
        Vector2 size,
        Sprite sprite,
        Color color,
        int sortingOrder = 0)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);
        gameObject.transform.localPosition = position;
        gameObject.transform.localScale = new Vector3(size.x, size.y, 1f);
        AddSprite(gameObject, sprite, color, sortingOrder);
        return gameObject;
    }

    /// <summary>
    /// 在指定位置生成一圈粒子爆发效果
    /// 每个粒子随机方向、随机速度飞出去，用于翻转、弹簧等反馈
    /// </summary>
    public static void SpawnBurst(Vector2 position, Color color, int count, float speed)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject particle = CreateSpriteObject(
                "Flip Particle",
                null,
                position,
                Vector2.one * Random.Range(0.08f, 0.16f),
                Square,
                color,
                20);

            // 随机角度决定粒子飞的方向，速度在 55%~100% 之间随机，避免太整齐
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            EffectParticle effect = particle.AddComponent<EffectParticle>();
            effect.Initialize(direction * Random.Range(speed * 0.55f, speed), color);
        }
    }

    /// <summary>
    /// 生成一张纯色贴图，宽高和颜色由参数决定
    /// 给 Square 这类简单图形用
    /// </summary>
    private static Texture2D MakeTexture(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }

    /// <summary>
    /// 清空所有缓存
    /// 一般只有重载域或者需要重新生成资源时才调用
    /// </summary>
    public static void ClearCachedAssets()
    {
        square = null;
        circle = null;
        roundedSquare = null;
        triangle = null;
        lineMaterial = null;
    }
}