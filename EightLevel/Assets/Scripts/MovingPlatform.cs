using UnityEngine;

/// <summary>
/// 移动平台
/// 在两个点之间来回滑动，用 PingPong 做往返
/// 用的是 Kinematic 刚体 + MovePosition，这样玩家站在上面能跟着走
/// 位置是在父物体的本地坐标下算的，所以平台可以挂在别的物体下面
/// </summary>
public sealed class MovingPlatform : MonoBehaviour
{
    private Rigidbody2D body;

    // 往返的两个端点，本地坐标
    [SerializeField, HideInInspector]
    private Vector2 localPointA;

    [SerializeField, HideInInspector]
    private Vector2 localPointB;

    // 单程耗时，从 A 到 B 走多久
    [SerializeField, HideInInspector]
    private float duration = 5f;

    // 有没有初始化过，没初始化就不动
    [SerializeField, HideInInspector]
    private bool initialized;

    // 累计时间，用来算 PingPong 进度
    private float elapsed;

    /// <summary>
    /// 外部初始化
    /// 设置刚体类型、两个端点、单程耗时，并把平台先摆到起点
    /// </summary>
    public void Initialize(Vector2 from, Vector2 to, float cycleDuration)
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        localPointA = from;
        localPointB = to;
        duration = Mathf.Max(0.1f, cycleDuration); // 防止填 0 导致除零
        transform.localPosition = from;
        initialized = true;
    }

    /// <summary>
    /// 每次启用时重置
    /// 时间归零，平台回到起点，刚体位置同步一下
    /// 没初始化就直接返回，等 Initialize 被调用
    /// </summary>
    private void OnEnable()
    {
        elapsed = 0f;
        if (!initialized)
        {
            return;
        }

        transform.localPosition = localPointA;
        body = body != null ? body : GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.position = transform.position;
        }
    }

    /// <summary>
    /// 物理帧里移动平台
    /// PingPong 让进度在 0 到 1 之间来回，SmoothStep 让两头慢中间快
    /// 最后用 MovePosition 移动，Kinematic 刚体这样动玩家能站稳
    /// </summary>
    private void FixedUpdate()
    {
        if (!initialized)
        {
            return;
        }

        body = body != null ? body : GetComponent<Rigidbody2D>();
        elapsed += Time.fixedDeltaTime;
        float progress = Mathf.PingPong(elapsed / duration, 1f);
        float eased = Mathf.SmoothStep(0f, 1f, progress);
        Vector2 localTarget = Vector2.Lerp(localPointA, localPointB, eased);
        // 有父物体就把本地坐标转成世界坐标，没有就直接用
        Vector2 worldTarget = transform.parent != null
            ? transform.parent.TransformPoint(localTarget)
            : localTarget;
        body.MovePosition(worldTarget);
    }
}