using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public sealed class GameBootstrap : MonoBehaviour
{
    public const float GroundSurface = -3.8f;
    public const float CeilingSurface = 3.8f;
    private const float WorldGravity = 9.81f;

    private static GameBootstrap instance;

    public static GameBootstrap Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameBootstrap>();
            }

            return instance;
        }
        private set => instance = value;
    }

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

    private Camera mainCamera;
    private CameraRig cameraRig;
    private GameHud hud;
    private PlayerController player;
    private Transform levelRoot;
    private AudioSource audioSource;
    private AudioClip flipClip;
    private AudioClip deathClip;
    private AudioClip goalClip;
    private AudioClip springClip;
    private Vector2 levelSpawn;
    private float levelMinX;
    private float levelMaxX;
    private Transform authoredLevelsContainer;
    private readonly Transform[] authoredLevelRoots = new Transform[8];
    private bool usesAuthoredLevels;
    private int currentLevelIndex;
    private int gravitySign = -1;
    private bool playerDead;
    private bool levelClearing;
    private bool gameComplete;

    public int CurrentLevelIndex => currentLevelIndex;
    public float GravitySign => gravitySign;
    public bool InputEnabled =>
        player != null
        && !playerDead
        && !levelClearing
        && !gameComplete;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Application.runInBackground = true;
        Physics2D.gravity = new Vector2(0f, gravitySign * WorldGravity);
        SetupCamera();
        SetupAudio();

        GameObject hudObject = new GameObject("HUD");
        hudObject.transform.SetParent(transform, false);
        hud = hudObject.AddComponent<GameHud>();
        hud.Build(this);
    }

    private void Start()
    {
        DiscoverAuthoredLevels();
        CreatePlayer();
        cameraRig.Initialize(mainCamera, player.transform);
        LoadLevel(0);
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R)) && !levelClearing)
        {
            RestartCurrentLevel();
            return;
        }

        if (!playerDead
            && !levelClearing
            && (player.transform.position.y < -6.5f
                || player.transform.position.y > 6.5f
                || player.transform.position.x < levelMinX - 2f
                || player.transform.position.x > levelMaxX + 3f))
        {
            HandlePlayerDeath();
        }
    }

    public void FlipGravity()
    {
        gravitySign = -gravitySign;
        Physics2D.gravity = new Vector2(0f, gravitySign * WorldGravity);
        cameraRig.Shake(0.08f, 0.05f);
    }

    public void PlayFlipFeedback()
    {
        PlayClip(flipClip, 0.34f);
    }

    public void PlaySpringFeedback()
    {
        PlayClip(springClip, 0.48f);
        cameraRig.Shake(0.11f, 0.045f);
    }

    public void HandlePlayerDeath()
    {
        if (playerDead || levelClearing || gameComplete)
        {
            return;
        }

        playerDead = true;
        player.SetControlsEnabled(false);
        PlayClip(deathClip, 0.42f);
        cameraRig.Shake(0.12f, 0.06f);
        hud.FlashDamage();
        RuntimeArt.SpawnBurst(player.transform.position, new Color(1f, 0.25f, 0.22f), 12, 5.5f);
        StartCoroutine(RespawnAfterDelay());
    }

    public void ReachGoal(Vector2 goalPosition)
    {
        if (levelClearing || gameComplete)
        {
            return;
        }

        levelClearing = true;
        player.FreezeForTransition();
        PlayClip(goalClip, 0.55f);
        cameraRig.Shake(0.13f, 0.04f);
        RuntimeArt.SpawnBurst(goalPosition, new Color(1f, 0.84f, 0.28f), 16, 6.5f);
        StartCoroutine(CompleteLevelRoutine());
    }

    private void RestartCurrentLevel()
    {
        bool restartWholeGame = gameComplete;
        StopAllCoroutines();
        playerDead = false;
        levelClearing = false;
        gameComplete = false;
        LoadLevel(restartWholeGame ? 0 : currentLevelIndex);
    }

    private void CreatePlayer()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.transform.SetParent(transform, false);
        RuntimeArt.AddSprite(playerObject, RuntimeArt.RoundedSquare, PlayerController.NormalColor, 10);
        player = playerObject.AddComponent<PlayerController>();

        AddEye(playerObject.transform, new Vector2(-0.18f, 0.13f));
        AddEye(playerObject.transform, new Vector2(0.18f, 0.13f));
    }

    private static void AddEye(Transform parent, Vector2 position)
    {
        GameObject eye = RuntimeArt.CreateSpriteObject(
            "Eye",
            parent,
            position,
            new Vector2(0.1f, 0.15f),
            RuntimeArt.RoundedSquare,
            new Color(0.025f, 0.06f, 0.12f, 0.95f),
            12);
        eye.transform.localPosition = new Vector3(position.x, position.y, 0f);
    }

    private void LoadLevel(int index)
    {
        currentLevelIndex = Mathf.Clamp(index, 0, LevelNames.Length - 1);
        playerDead = false;
        levelClearing = false;
        gameComplete = false;
        gravitySign = -1;
        Physics2D.gravity = new Vector2(0f, gravitySign * WorldGravity);

        DiscoverAuthoredLevels();
        if (usesAuthoredLevels)
        {
            LoadAuthoredLevel(currentLevelIndex);
        }
        else
        {
            if (levelRoot != null)
            {
                Destroy(levelRoot.gameObject);
            }

            GameObject rootObject = new GameObject($"Level {currentLevelIndex + 1:00} - {LevelNames[currentLevelIndex]}");
            rootObject.transform.SetParent(transform, false);
            levelRoot = rootObject.transform;
            BuildLevel(currentLevelIndex);
        }

        player.Respawn(levelSpawn, true);
        cameraRig.SetLevelBounds(levelMinX, levelMaxX, true);
        hud.SetLevel(currentLevelIndex, LevelNames[currentLevelIndex]);
    }

    private void DiscoverAuthoredLevels()
    {
        GameObject container = GameObject.Find("Authored Levels");
        if (container == null)
        {
            usesAuthoredLevels = false;
            return;
        }

        authoredLevelsContainer = container.transform;
        Transform[] allChildren = authoredLevelsContainer.GetComponentsInChildren<Transform>(true);
        for (int levelIndex = 0; levelIndex < LevelNames.Length; levelIndex++)
        {
            string prefix = $"Level {levelIndex + 1:00}";
            authoredLevelRoots[levelIndex] = null;
            for (int childIndex = 0; childIndex < allChildren.Length; childIndex++)
            {
                Transform candidate = allChildren[childIndex];
                if (candidate.parent == authoredLevelsContainer
                    && candidate.name.StartsWith(prefix))
                {
                    authoredLevelRoots[levelIndex] = candidate;
                    break;
                }
            }

            if (authoredLevelRoots[levelIndex] == null)
            {
                usesAuthoredLevels = false;
                return;
            }
        }

        usesAuthoredLevels = true;
        for (int i = 0; i < authoredLevelRoots.Length; i++)
        {
            authoredLevelRoots[i].gameObject.SetActive(false);
        }
    }

    private void LoadAuthoredLevel(int index)
    {
        for (int i = 0; i < authoredLevelRoots.Length; i++)
        {
            if (authoredLevelRoots[i] != null)
            {
                authoredLevelRoots[i].gameObject.SetActive(false);
            }
        }

        levelRoot = authoredLevelRoots[index];
        levelRoot.gameObject.SetActive(true);

        LevelLayout layout = levelRoot.GetComponent<LevelLayout>();
        float localMinX = layout != null ? layout.MinX : -1f;
        float localMaxX = layout != null ? layout.MaxX : 20f;
        levelMinX = levelRoot.TransformPoint(new Vector3(localMinX, 0f, 0f)).x;
        levelMaxX = levelRoot.TransformPoint(new Vector3(localMaxX, 0f, 0f)).x;

        Transform spawn = FindDescendant(levelRoot, "Spawn");
        levelSpawn = spawn != null
            ? (Vector2)spawn.position
            : (Vector2)levelRoot.TransformPoint(
                new Vector3(localMinX + 2f, GroundSurface + 0.55f, 0f));
    }

    private static Transform FindDescendant(Transform parent, string objectName)
    {
        Transform[] transforms = parent.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i] != parent && transforms[i].name == objectName)
            {
                return transforms[i];
            }
        }

        return null;
    }

    private void BuildLevel(int index)
    {
        switch (index)
        {
            case 0:
                BuildLevelOne();
                break;
            case 1:
                BuildLevelTwo();
                break;
            case 2:
                BuildLevelThree();
                break;
            case 3:
                BuildLevelFour();
                break;
            case 4:
                BuildLevelFive();
                break;
            case 5:
                BuildLevelSix();
                break;
            case 6:
                BuildLevelSeven();
                break;
            default:
                BuildLevelEight();
                break;
        }
    }

    private void BuildLevelOne()
    {
        levelMinX = -1f;
        levelMaxX = 18f;
        AddBackdrop();
        AddFloor(-1f, 6f);
        AddFloor(10f, 18f);
        AddCeiling(-1f, 11f);
        AddGoal(16f, false);
        levelSpawn = new Vector2(1f, GroundSurface + 0.55f);
    }

    private void BuildLevelTwo()
    {
        levelMinX = -1f;
        levelMaxX = 21f;
        AddBackdrop();
        AddFloor(-1f, 5f);
        AddCeiling(3f, 9f);
        AddFloor(7f, 13f);
        AddCeiling(11f, 17f);
        AddFloor(15f, 21f);
        AddGoal(19f, false);
        levelSpawn = new Vector2(1f, GroundSurface + 0.55f);
    }

    private void BuildLevelThree()
    {
        levelMinX = -1f;
        levelMaxX = 25f;
        AddBackdrop();
        AddFloor(-1f, 10f);
        AddCeiling(4f, 23f);
        AddSpikeStrip(5f, 8.4f, true);
        AddFloor(15f, 25f);
        AddGoal(21f, false);
        levelSpawn = new Vector2(1f, GroundSurface + 0.55f);
    }

    private void BuildLevelFour()
    {
        levelMinX = -1f;
        levelMaxX = 27f;
        AddBackdrop();
        AddFloor(-1f, 5f);
        AddFloor(19f, 27f);
        AddMovingPlatform(new Vector2(7.1f, -4.02f), new Vector2(15.9f, -4.02f), 5.8f, 6.5f);
        AddGoal(24f, false);
        levelSpawn = new Vector2(1f, GroundSurface + 0.55f);
    }

    private void BuildLevelFive()
    {
        levelMinX = -1f;
        levelMaxX = 22f;
        AddBackdrop();
        AddFloor(-1f, 4.5f);
        AddCeiling(3.5f, 9f);
        AddFloor(8f, 13.5f);
        AddCeiling(12.5f, 18f);
        AddFloor(17f, 22f);
        AddGoal(20.5f, false);
        levelSpawn = new Vector2(1f, GroundSurface + 0.55f);
    }

    private void BuildLevelSix()
    {
        levelMinX = -1f;
        levelMaxX = 18f;
        AddBackdrop();
        AddFloor(-1f, 18f);
        AddCeiling(1f, 10f);
        DoorController door = AddDoor(11.5f);
        AddPressurePlate(8.2f, door);
        AddPushBox(5f, GroundSurface + 0.46f);
        AddGoal(15.5f, false);
        levelSpawn = new Vector2(1f, GroundSurface + 0.55f);
    }

    private void BuildLevelSeven()
    {
        levelMinX = -1f;
        levelMaxX = 20f;
        AddBackdrop();
        AddFloor(-1f, 8f);
        AddCeiling(2f, 20f);
        AddSpikeStrip(2.4f, 6.5f, true);
        AddSpring(7.3f, false);
        AddGoal(16.5f, true);
        levelSpawn = new Vector2(1f, GroundSurface + 0.55f);
    }

    private void BuildLevelEight()
    {
        levelMinX = -1f;
        levelMaxX = 41f;
        AddBackdrop();

        AddFloor(-1f, 10f);
        AddCeiling(4f, 17f);
        AddSpikeStrip(5f, 8f, true);
        AddFloor(14f, 18f);
        AddMovingPlatform(new Vector2(19.5f, -4.02f), new Vector2(23.5f, -4.02f), 5.5f, 4.5f);
        AddFloor(24f, 41f);

        DoorController door = AddDoor(34.5f);
        AddPressurePlate(31.5f, door);
        AddPushBox(27f, GroundSurface + 0.46f);
        AddSpring(38.2f, false);
        AddGoal(40f, false);
        levelSpawn = new Vector2(1f, GroundSurface + 0.55f);
    }

    private void AddBackdrop()
    {
        float centerX = (levelMinX + levelMaxX) * 0.5f;
        float width = levelMaxX - levelMinX + 20f;
        GameObject background = RuntimeArt.CreateSpriteObject(
            "Backdrop",
            levelRoot,
            new Vector2(centerX, 0f),
            new Vector2(width, 12f),
            RuntimeArt.Square,
            new Color(0.035f, 0.06f, 0.105f, 1f),
            -50);
        background.transform.SetParent(levelRoot, true);

        for (float x = Mathf.Floor(levelMinX); x <= levelMaxX + 10f; x += 2f)
        {
            RuntimeArt.CreateSpriteObject(
                "Grid Vertical",
                levelRoot,
                new Vector2(x, 0f),
                new Vector2(0.012f, 8f),
                RuntimeArt.Square,
                new Color(0.15f, 0.28f, 0.38f, 0.12f),
                -40);
        }

        for (float y = -3f; y <= 3f; y += 1.5f)
        {
            RuntimeArt.CreateSpriteObject(
                "Grid Horizontal",
                levelRoot,
                new Vector2(centerX, y),
                new Vector2(width, 0.012f),
                RuntimeArt.Square,
                new Color(0.15f, 0.28f, 0.38f, 0.1f),
                -40);
        }

        RuntimeArt.CreateSpriteObject(
            "Upper Glow",
            levelRoot,
            new Vector2(centerX, 0f),
            new Vector2(width, 0.025f),
            RuntimeArt.Square,
            new Color(0.2f, 0.8f, 1f, 0.08f),
            -39);
    }

    private void AddFloor(float xMin, float xMax)
    {
        AddPlatform(xMin, xMax, GroundSurface, false);
    }

    private void AddCeiling(float xMin, float xMax)
    {
        AddPlatform(xMin, xMax, CeilingSurface, true);
    }

    private void AddPlatform(float xMin, float xMax, float surfaceY, bool ceiling)
    {
        const float thickness = 0.8f;
        GameObject block = CreateWorldBlock(
            ceiling ? "Ceiling" : "Ground",
            new Vector2((xMin + xMax) * 0.5f, surfaceY + (ceiling ? thickness * 0.5f : -thickness * 0.5f)),
            new Vector2(xMax - xMin, thickness),
            ceiling
                ? new Color(0.25f, 0.31f, 0.4f, 1f)
                : new Color(0.15f, 0.2f, 0.29f, 1f),
            -2);
        block.AddComponent<BoxCollider2D>();

        float edgeY = surfaceY + (ceiling ? -0.045f : 0.045f);
        RuntimeArt.CreateSpriteObject(
            "Surface Edge",
            levelRoot,
            new Vector2((xMin + xMax) * 0.5f, edgeY),
            new Vector2(xMax - xMin, 0.09f),
            RuntimeArt.Square,
            ceiling
                ? new Color(0.56f, 0.75f, 0.84f, 0.95f)
                : new Color(0.25f, 0.63f, 0.78f, 0.95f),
            1);
    }

    private void AddSpikeStrip(float xMin, float xMax, bool ceiling)
    {
        float surfaceY = ceiling ? CeilingSurface : GroundSurface;
        const float spacing = 0.62f;
        for (float x = xMin + spacing * 0.5f; x < xMax; x += spacing)
        {
            GameObject spike = RuntimeArt.CreateSpriteObject(
                "Spike",
                levelRoot,
                new Vector2(x, surfaceY + (ceiling ? -0.35f : 0.35f)),
                new Vector2(0.58f, 0.7f),
                RuntimeArt.Triangle,
                new Color(0.95f, 0.23f, 0.2f, 1f),
                3);
            spike.transform.rotation = Quaternion.Euler(0f, 0f, ceiling ? 180f : 0f);
            BoxCollider2D spikeCollider = spike.AddComponent<BoxCollider2D>();
            spikeCollider.isTrigger = true;
            spikeCollider.size = new Vector2(0.86f, 0.74f);
            spike.AddComponent<Hazard>();
        }

        RuntimeArt.CreateSpriteObject(
            "Spike Base",
            levelRoot,
            new Vector2((xMin + xMax) * 0.5f, surfaceY + (ceiling ? -0.035f : 0.035f)),
            new Vector2(xMax - xMin, 0.07f),
            RuntimeArt.Square,
            new Color(0.5f, 0.11f, 0.1f, 0.95f),
            2);
    }

    private void AddGoal(float x, bool ceiling)
    {
        float surfaceY = ceiling ? CeilingSurface : GroundSurface;
        GameObject root = new GameObject("Goal");
        root.transform.SetParent(levelRoot, false);
        root.transform.localPosition = new Vector2(x, surfaceY);
        root.transform.rotation = Quaternion.Euler(0f, 0f, ceiling ? 180f : 0f);

        RuntimeArt.CreateSpriteObject(
            "Pole",
            root.transform,
            new Vector2(0f, 0.9f),
            new Vector2(0.11f, 1.8f),
            RuntimeArt.Square,
            new Color(0.91f, 0.96f, 1f, 1f),
            5);
        RuntimeArt.CreateSpriteObject(
            "Flag",
            root.transform,
            new Vector2(0.5f, 1.56f),
            new Vector2(0.88f, 0.48f),
            RuntimeArt.Triangle,
            new Color(1f, 0.76f, 0.2f, 1f),
            6).transform.rotation = Quaternion.Euler(0f, 0f, -90f);
        RuntimeArt.CreateSpriteObject(
            "Base",
            root.transform,
            new Vector2(0f, 0.06f),
            new Vector2(0.8f, 0.12f),
            RuntimeArt.Square,
            new Color(1f, 0.76f, 0.2f, 0.9f),
            5);
        BoxCollider2D trigger = root.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.offset = new Vector2(0f, 0.95f);
        trigger.size = new Vector2(1.8f, 2f);
        root.AddComponent<GoalMarker>();
    }

    private DoorController AddDoor(float x)
    {
        GameObject doorObject = RuntimeArt.CreateSpriteObject(
            "Locked Door",
            levelRoot,
            new Vector2(x, -2.55f),
            new Vector2(0.34f, 2.5f),
            RuntimeArt.Square,
            new Color(0.24f, 0.87f, 0.7f, 0.9f),
            4);
        doorObject.AddComponent<BoxCollider2D>();
        RuntimeArt.CreateSpriteObject(
            "Door Glow",
            doorObject.transform,
            new Vector2(-0.08f, 0f),
            new Vector2(0.06f, 0.94f),
            RuntimeArt.Square,
            new Color(0.75f, 1f, 0.9f, 0.85f),
            5);
        DoorController door = doorObject.AddComponent<DoorController>();
        door.Initialize(2.8f);
        return door;
    }

    private void AddPressurePlate(float x, DoorController door)
    {
        GameObject root = new GameObject("Pressure Plate");
        root.transform.SetParent(levelRoot, false);
        root.transform.position = new Vector2(x, GroundSurface + 0.16f);
        BoxCollider2D trigger = root.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(1.8f, 0.36f);

        GameObject visual = RuntimeArt.CreateSpriteObject(
            "Plate",
            root.transform,
            Vector2.zero,
            new Vector2(1.65f, 0.18f),
            RuntimeArt.Square,
            new Color(0.26f, 0.82f, 0.7f, 1f),
            3);
        PressurePlate plate = root.AddComponent<PressurePlate>();
        plate.Initialize(door, visual.transform);
    }

    private void AddPushBox(float x, float y)
    {
        GameObject box = RuntimeArt.CreateSpriteObject(
            "Push Box",
            levelRoot,
            new Vector2(x, y),
            new Vector2(0.9f, 0.9f),
            RuntimeArt.RoundedSquare,
            new Color(0.82f, 0.55f, 0.28f, 1f),
            3);
        RuntimeArt.CreateSpriteObject(
            "Box Inset",
            box.transform,
            Vector2.zero,
            new Vector2(0.52f, 0.52f),
            RuntimeArt.Square,
            new Color(0.98f, 0.76f, 0.45f, 0.55f),
            4);

        BoxCollider2D collider = box.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        Rigidbody2D body = box.AddComponent<Rigidbody2D>();
        body.mass = 1.2f;
        body.drag = 3.2f;
        body.angularDrag = 6f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        box.AddComponent<PushBox>();
        LevelObjectState state = box.AddComponent<LevelObjectState>();
        state.Capture();
    }

    private void AddSpring(float x, bool ceiling)
    {
        float surfaceY = ceiling ? CeilingSurface : GroundSurface;
        GameObject root = new GameObject("Spring");
        root.transform.SetParent(levelRoot, false);
        root.transform.position = new Vector2(x, surfaceY);
        root.transform.rotation = Quaternion.Euler(0f, 0f, ceiling ? 180f : 0f);

        RuntimeArt.CreateSpriteObject(
            "Spring Base",
            root.transform,
            new Vector2(0f, 0.08f),
            new Vector2(1.18f, 0.15f),
            RuntimeArt.Square,
            new Color(0.75f, 0.32f, 0.14f, 1f),
            3);

        for (int i = 0; i < 3; i++)
        {
            RuntimeArt.CreateSpriteObject(
                "Spring Coil",
                root.transform,
                new Vector2(0f, 0.2f + i * 0.13f),
                new Vector2(0.95f - i * 0.06f, 0.055f),
                RuntimeArt.Square,
                new Color(1f, 0.56f, 0.2f, 1f),
                4);
        }

        GameObject plate = RuntimeArt.CreateSpriteObject(
            "Spring Plate",
            root.transform,
            new Vector2(0f, 0.6f),
            new Vector2(1.15f, 0.18f),
            RuntimeArt.RoundedSquare,
            new Color(1f, 0.78f, 0.3f, 1f),
            5);

        BoxCollider2D trigger = root.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.offset = new Vector2(0f, 0.42f);
        trigger.size = new Vector2(1.3f, 0.95f);

        SpringPad spring = root.AddComponent<SpringPad>();
        spring.SetPlate(plate.transform);
    }

    private void AddMovingPlatform(Vector2 from, Vector2 to, float width, float duration)
    {
        GameObject platform = RuntimeArt.CreateSpriteObject(
            "Moving Platform",
            levelRoot,
            from,
            new Vector2(width, 0.45f),
            RuntimeArt.RoundedSquare,
            new Color(0.34f, 0.48f, 0.66f, 1f),
            3);
        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        Rigidbody2D body = platform.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        RuntimeArt.CreateSpriteObject(
            "Platform Edge",
            platform.transform,
            new Vector2(0f, 0.45f),
            new Vector2(0.94f, 0.08f),
            RuntimeArt.Square,
            new Color(0.55f, 0.85f, 0.92f, 0.95f),
            4);

        MovingPlatform moving = platform.AddComponent<MovingPlatform>();
        moving.Initialize(from, to, duration);
    }

    private GameObject CreateWorldBlock(string name, Vector2 position, Vector2 size, Color color, int sortingOrder)
    {
        return RuntimeArt.CreateSpriteObject(
            name,
            levelRoot,
            position,
            size,
            RuntimeArt.Square,
            color,
            sortingOrder);
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(0.28f);
        player.Respawn(levelSpawn, true);
        gravitySign = -1;
        Physics2D.gravity = new Vector2(0f, gravitySign * WorldGravity);
        playerDead = false;
    }

    private IEnumerator CompleteLevelRoutine()
    {
        yield return new WaitForSeconds(0.65f);
        if (currentLevelIndex >= LevelNames.Length - 1)
        {
            gameComplete = true;
            hud.ShowCompletion();
            cameraRig.Shake(0.2f, 0.035f);
            yield break;
        }

        LoadLevel(currentLevelIndex + 1);
    }

    private void SetupCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 5.15f;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0.035f, 0.06f, 0.105f, 1f);
        mainCamera.transform.position = new Vector3(3f, 0f, -10f);
        mainCamera.transparencySortMode = TransparencySortMode.Default;

        cameraRig = mainCamera.GetComponent<CameraRig>();
        if (cameraRig == null)
        {
            cameraRig = mainCamera.gameObject.AddComponent<CameraRig>();
        }

        cameraRig.Initialize(mainCamera, null);
    }

    private void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        flipClip = CreateTone("Flip", 0.09f, 750f, 430f, 0.55f);
        deathClip = CreateTone("Respawn", 0.12f, 190f, 80f, 0.7f);
        goalClip = CreateTone("Goal", 0.28f, 480f, 980f, 0.45f);
        springClip = CreateTone("Spring", 0.16f, 260f, 720f, 0.55f);
    }

    private void PlayClip(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    private static AudioClip CreateTone(
        string name,
        float duration,
        float startFrequency,
        float endFrequency,
        float noiseAmount)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(duration * sampleRate);
        float[] samples = new float[sampleCount];
        float phase = 0f;
        uint noiseState = 2463534242u;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)Mathf.Max(1, sampleCount - 1);
            float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
            phase += frequency / sampleRate * Mathf.PI * 2f;
            noiseState ^= noiseState << 13;
            noiseState ^= noiseState >> 17;
            noiseState ^= noiseState << 5;
            float noise = (noiseState / (float)uint.MaxValue) * 2f - 1f;
            float envelope = Mathf.Pow(1f - t, 2.2f);
            samples[i] = (Mathf.Sin(phase) * (1f - noiseAmount) + noise * noiseAmount) * envelope * 0.38f;
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnDestroy()
    {
        if (ReferenceEquals(instance, this))
        {
            Physics2D.gravity = new Vector2(0f, -WorldGravity);
        }
    }

#if UNITY_EDITOR
    public LevelAuthoringSnapshot BuildLevelContentsForAuthoring(int index, Transform targetRoot)
    {
        levelRoot = targetRoot;
        BuildLevel(index);
        return new LevelAuthoringSnapshot
        {
            Spawn = levelSpawn,
            MinX = levelMinX,
            MaxX = levelMaxX
        };
    }
#endif
}

[System.Serializable]
public struct LevelAuthoringSnapshot
{
    public Vector2 Spawn;
    public float MinX;
    public float MaxX;
}
