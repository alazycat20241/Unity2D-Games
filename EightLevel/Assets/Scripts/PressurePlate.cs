using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 压力板
/// 玩家或箱子站上去就开门，全部离开就关门
/// 用 HashSet 记录当前站在板上的碰撞体，支持多个物体同时压住
/// 板上还有一个可视子物体，被压时往下沉一点，给个反馈
/// </summary>
public sealed class PressurePlate : MonoBehaviour
{
    // 当前站在板上的碰撞体，用 HashSet 保证同一个物体只算一次
    private readonly HashSet<Collider2D> occupants = new HashSet<Collider2D>();

    // 这块板控制的门
    [SerializeField, HideInInspector]
    private DoorController door;

    // 板的可视子物体，用来做下沉动画
    [SerializeField, HideInInspector]
    private Transform plateVisual;

    // 可视子物体的初始本地坐标，松手后回到这个位置
    [SerializeField, HideInInspector]
    private Vector3 plateStart;

    /// <summary>
    /// 外部初始化，把门和可视子物体传进来
    /// 顺便记录可视子物体的初始位置，供下沉动画用
    /// </summary>
    public void Initialize(DoorController targetDoor, Transform visual)
    {
        door = targetDoor;
        plateVisual = visual;
        plateStart = plateVisual.localPosition;
    }

    /// <summary>
    /// 每次启用时重置状态
    /// 清空站在板上的人，可视子物体归位，门先关好
    /// 避免重开关卡时残留上一局的状态
    /// </summary>
    private void OnEnable()
    {
        occupants.Clear();
        if (plateVisual != null)
        {
            plateVisual.localPosition = plateStart;
        }

        if (door != null)
        {
            door.SetOpen(false);
        }
    }

    /// <summary>
    /// 每帧检查板上有没有人
    /// 有人就开门，没人就关门
    /// 同时让可视子物体平滑地下沉或回弹
    /// </summary>
    private void Update()
    {
        bool pressed = occupants.Count > 0;
        if (door != null)
        {
            door.SetOpen(pressed);
        }

        if (plateVisual != null)
        {
            // 按下时往下沉 0.1，松开回到原位
            Vector3 target = plateStart + Vector3.down * (pressed ? 0.1f : 0f);
            // 用指数插值做平滑，1 - e^(-14*dt) 是常见写法，跟帧率无关
            plateVisual.localPosition = Vector3.Lerp(
                plateVisual.localPosition,
                target,
                1f - Mathf.Exp(-14f * Time.deltaTime));
        }
    }

    /// <summary>
    /// 有物体进入触发器
    /// 只认玩家和箱子，别的东西不计数
    /// 加进去之后马上开门
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidOccupant(other))
        {
            return;
        }

        occupants.Add(other);
        if (door != null)
        {
            door.SetOpen(true);
        }
    }

    /// <summary>
    /// 有物体离开触发器
    /// 从集合里移除，如果移除后没人了就把门关上
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (occupants.Remove(other) && occupants.Count == 0 && door != null)
        {
            door.SetOpen(false);
        }
    }

    /// <summary>
    /// 判断这个碰撞体算不算有效占用者
    /// 玩家或者推箱子都行，其他东西不算
    /// 用 GetComponentInParent 是因为碰撞体可能在子物体上
    /// </summary>
    private static bool IsValidOccupant(Collider2D other)
    {
        return other.GetComponentInParent<PushBox>() != null
            || other.GetComponentInParent<PlayerController>() != null;
    }
}