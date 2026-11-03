using UnityEngine;

/// <summary>
/// 关卡物体状态
/// 记录物体一开始的位置和旋转，重新启用时还原回去
/// 主要给箱子、机关这类东西用，避免重开一局时位置乱掉
/// 位置和旋转都按本地坐标存，物体挂在父物体下面也能用
/// </summary>
public sealed class LevelObjectState : MonoBehaviour
{
    // 初始本地位置
    [SerializeField, HideInInspector]
    private Vector3 initialLocalPosition;

    // 初始本地旋转，默认单位旋转
    [SerializeField, HideInInspector]
    private Quaternion initialLocalRotation = Quaternion.identity;

    // 有没有记录过初始状态
    [SerializeField, HideInInspector]
    private bool captured;

    /// <summary>
    /// 记录当前位置和旋转作为初始状态
    /// 一般由 Awake 自动调用，也可以外部手动调
    /// </summary>
    public void Capture()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
        captured = true;
    }

    /// <summary>
    /// 启动时如果还没记录，就自动记录一次
    /// </summary>
    private void Awake()
    {
        if (!captured)
        {
            Capture();
        }
    }

    /// <summary>
    /// 每次启用时还原到初始位置
    /// 只在运行时生效，编辑器里不还原，免得搭关卡的时候被拉回去
    /// 有刚体的话顺便把速度和角速度清零
    /// </summary>
    private void OnEnable()
    {
        if (!Application.isPlaying || !captured)
        {
            return;
        }

        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;

        // 有刚体就同步一下，不然刚体位置还是旧的，物体会瞬移
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.position = transform.position;
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
    }
}