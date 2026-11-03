using UnityEngine;

/// <summary>
/// 玩家控制器
/// 只有两个操作：左右移动 + 鼠标左键翻转重力
/// 没有跳跃，纵向位移全靠翻转，第 5 关起翻转带冷却
/// 带离地宽容期，刚离开平台的一小段时间内还能翻
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public sealed class PlayerController : MonoBehaviour
{
    public float MoveSpeed = 6f;        // 左右移动速度
    public float FlipImpulse = 4f;      // 翻转时给一个朝新重力方向的初速度，让翻转更干脆
    public float FlipCooldown = 0.4f;   // 翻转冷却，第 5 关起生效
    public float CoyoteAirTime = 0.15f; // 离地宽容期，刚走出平台还能翻的宽限时间

    // 地面检测用的缓冲区，一次最多记 8 个碰撞结果
    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[8];
    private Rigidbody2D body;
    private CapsuleCollider2D capsule;
    private SpriteRenderer spriteRenderer;
    private LineRenderer cooldownRing;  // 头顶的冷却圆环
    private ContactFilter2D groundFilter;
    private float lastGroundedTime = -99f; // 上次站地上的时间
    private float lastFlipTime = -99f;     // 上次翻转的时间
    private bool isGrounded;
    private bool controlsEnabled = true;   // 转场或冻结时关掉操作
    private Vector2 spawnPosition;

    // 两种状态的颜色，蓝=正常重力，橙=反重力
    public static readonly Color NormalColor = new Color(0.25f, 0.7f, 1f, 1f);
    public static readonly Color InvertedColor = new Color(1f, 0.55f, 0.18f, 1f);

    public bool IsGrounded => isGrounded;
    public bool ControlsEnabled => controlsEnabled;
    public Vector2 SpawnPosition => spawnPosition;

    /// <summary>
    /// 初始化刚体、碰撞体、地面检测和冷却圆环
    /// </summary>
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 3f;
        body.freezeRotation = true;          // 不让刚体自己转，旋转由翻转逻辑控制
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 高速移动也不穿墙
        body.interpolation = RigidbodyInterpolation2D.Interpolate;         // 插值让移动更平滑

        capsule.direction = CapsuleDirection2D.Vertical;
        capsule.size = new Vector2(0.78f, 1f);

        groundFilter = new ContactFilter2D
        {
            useTriggers = false, // 触发器不算地面
            useLayerMask = false
        };

        CreateCooldownRing();
    }

    /// <summary>
    /// 记录出生点，重生时用
    /// </summary>
    public void SetSpawn(Vector2 position)
    {
        spawnPosition = position;
    }

    /// <summary>
    /// 重生到指定位置
    /// 清空速度、恢复操作、按当前重力方向设置颜色
    /// </summary>
    public void Respawn(Vector2 position, bool gravityDown)
    {
        spawnPosition = position;
        transform.position = position;
        transform.rotation = Quaternion.identity;
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;
        controlsEnabled = true;
        capsule.enabled = true;
        lastGroundedTime = Time.time;
        lastFlipTime = -99f;
        SetVisualGravity(gravityDown);
        UpdateCooldownRing();
    }

    /// <summary>
    /// 转场时冻结操作，角色停下
    /// </summary>
    public void FreezeForTransition()
    {
        controlsEnabled = false;
        body.velocity = Vector2.zero;
    }

    /// <summary>
    /// 开关操作
    /// 关掉时顺便把速度清零，避免还在滑
    /// </summary>
    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
        if (!enabled)
        {
            body.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 弹簧弹射
    /// 方向跟着当前重力走，人在地面就往上弹，人在天花板就往下弹
    /// </summary>
    public void ApplySpringBoost(float upwardSpeed)
    {
        if (!controlsEnabled)
        {
            return;
        }

        float direction = GameBootstrap.Instance.GravitySign;
        body.velocity = new Vector2(body.velocity.x, -direction * upwardSpeed);
        lastGroundedTime = Time.time;
    }

    /// <summary>
    /// 每帧处理输入
    /// 先检测有没有站地上，再判断能不能翻
    /// 能翻就翻，顺便更新冷却圆环
    /// </summary>
    private void Update()
    {
        if (!controlsEnabled || GameBootstrap.Instance == null || !GameBootstrap.Instance.InputEnabled)
        {
            UpdateCooldownRing();
            return;
        }

        ProbeForGround();
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }

        bool withinCoyoteTime = Time.time - lastGroundedTime <= CoyoteAirTime;
        // 前 4 关没有冷却，第 5 关起才检查冷却
        bool cooldownReady = GameBootstrap.Instance.CurrentLevelIndex < 4
            || Time.time - lastFlipTime >= FlipCooldown;

        if (Input.GetMouseButtonDown(0) && (isGrounded || withinCoyoteTime) && cooldownReady)
        {
            Flip();
        }

        UpdateCooldownRing();
    }

    /// <summary>
    /// 物理帧里处理左右移动
    /// 竖直速度不动，只改水平速度
    /// </summary>
    private void FixedUpdate()
    {
        if (!controlsEnabled || GameBootstrap.Instance == null || !GameBootstrap.Instance.InputEnabled)
        {
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        body.velocity = new Vector2(horizontal * MoveSpeed, body.velocity.y);
    }

    /// <summary>
    /// 检测脚下有没有地面
    /// 朝着重力方向发射胶囊体，碰到非触发器就算站地上
    /// </summary>
    private void ProbeForGround()
    {
        Vector2 gravityDirection = Vector2.up * GameBootstrap.Instance.GravitySign;
        int hitCount = capsule.Cast(gravityDirection, groundFilter, groundHits, 0.09f, true);
        isGrounded = false;
        for (int i = 0; i < hitCount; i++)
        {
            if (groundHits[i].collider != null && !groundHits[i].collider.isTrigger)
            {
                isGrounded = true;
                break;
            }
        }
    }

    /// <summary>
    /// 执行翻转
    /// 改重力、给初速度、转 180 度、换颜色、放粒子、播音效
    /// </summary>
    private void Flip()
    {
        lastFlipTime = Time.time;
        GameBootstrap.Instance.FlipGravity();
        float direction = GameBootstrap.Instance.GravitySign;
        body.velocity = new Vector2(body.velocity.x, direction * FlipImpulse);
        transform.rotation = Quaternion.Euler(0f, 0f, direction > 0f ? 180f : 0f);
        SetVisualGravity(direction < 0f);
        RuntimeArt.SpawnBurst(transform.position, spriteRenderer.color, 9, 4.5f);
        GameBootstrap.Instance.PlayFlipFeedback();
    }

    /// <summary>
    /// 根据当前重力方向设置角色颜色
    /// 蓝=正常重力，橙=反重力
    /// </summary>
    private void SetVisualGravity(bool gravityDown)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = gravityDown ? NormalColor : InvertedColor;
        }
    }

    /// <summary>
    /// 创建头顶的冷却圆环
    /// 用 LineRenderer 画一段弧，翻转后转一圈表示冷却进度
    /// </summary>
    private void CreateCooldownRing()
    {
        GameObject ringObject = new GameObject("Flip Cooldown Ring");
        ringObject.transform.SetParent(transform, false);
        cooldownRing = ringObject.AddComponent<LineRenderer>();
        cooldownRing.useWorldSpace = false;
        cooldownRing.loop = false;
        cooldownRing.widthMultiplier = 0.055f;
        cooldownRing.numCapVertices = 4;
        cooldownRing.numCornerVertices = 4;
        cooldownRing.material = RuntimeArt.LineMaterial;
        cooldownRing.startColor = new Color(1f, 0.82f, 0.35f, 0.95f);
        cooldownRing.endColor = new Color(1f, 0.82f, 0.35f, 0.25f);
        cooldownRing.sortingOrder = 25;
        cooldownRing.positionCount = 0;
    }

    /// <summary>
    /// 更新冷却圆环
    /// 只有第 5 关起、且在冷却中才显示
    /// 进度从 0 到 1，弧线跟着慢慢画满
    /// </summary>
    private void UpdateCooldownRing()
    {
        bool showCooldown = GameBootstrap.Instance != null
            && GameBootstrap.Instance.CurrentLevelIndex >= 4
            && Time.time - lastFlipTime < FlipCooldown;

        if (!showCooldown)
        {
            cooldownRing.positionCount = 0;
            return;
        }

        float progress = Mathf.Clamp01((Time.time - lastFlipTime) / FlipCooldown);
        int segments = Mathf.Max(2, Mathf.CeilToInt(30f * progress));
        cooldownRing.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float angle = -90f + 360f * progress * (i / (float)segments);
            float radians = angle * Mathf.Deg2Rad;
            cooldownRing.SetPosition(
                i,
                new Vector3(Mathf.Cos(radians) * 0.72f, Mathf.Sin(radians) * 0.72f, 0f));
        }
    }
}