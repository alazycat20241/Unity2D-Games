using System.Collections;
using UnityEngine;

/// <summary>
/// 弹簧板：玩家碰到时沿“当前重力的反方向”给一个瞬时冲刺力（弹射）。
/// 弹射方向由 PlayerController.ApplySpringBoost 内部根据当前重力方向处理
/// </summary>
public sealed class SpringPad : MonoBehaviour
{
    /// <summary>弹射速度大小</summary>
    public float BoostSpeed = 13f;

    /// <summary>弹簧板可视子物体（用于播放上下弹动的表现）</summary>
    [SerializeField, HideInInspector]
    private Transform plate;

    /// <summary>弹簧板的初始本地坐标，弹动结束后用于复位</summary>
    [SerializeField, HideInInspector]
    private Vector3 plateStart;

    /// <summary>上一次触发弹射的时间，用于 0.2 秒内防止重复触发</summary>
    private float lastBoostTime = -99f;

    /// <summary>
    /// 由外部（如关卡搭建脚本）注入弹簧板可视子物体，
    /// 并记录其初始本地位置，供 BouncePlate 弹动动画使用。
    /// </summary>
    public void SetPlate(Transform target)
    {
        plate = target;
        plateStart = plate.localPosition;
    }

    /// <summary>
    /// 玩家进入触发器时调用：
    /// 1) 过滤非玩家对象；
    /// 2) 0.2 秒冷却，防止一次停留触发多次；
    /// 3) 给玩家施加弹射力；
    /// 4) 播放粒子、音效与弹簧板弹动动画。
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只处理玩家；不是玩家直接忽略。
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null || Time.time - lastBoostTime < 0.2f)
        {
            return;
        }

        lastBoostTime = Time.time;

        // 弹射方向与重力相关的逻辑封装在玩家控制器里
        player.ApplySpringBoost(BoostSpeed);

        // 视觉反馈：橙色粒子爆发
        RuntimeArt.SpawnBurst(transform.position, new Color(1f, 0.78f, 0.25f), 8, 5f);

        // 音效反馈
        GameBootstrap.Instance.PlaySpringFeedback();

        // 先停掉旧的弹动协程，避免多次触发时动画叠加错乱，再重新播放
        StopAllCoroutines();
        StartCoroutine(BouncePlate());
    }

    /// <summary>
    /// 弹簧板弹动表现：0.18 秒内沿本地 Y 轴按正弦曲线向上隆起再回落，结束后复位
    /// 纯表现，不影响物理与触发逻辑
    /// </summary>
    private IEnumerator BouncePlate()
    {
        // 没有可视子物体就跳过表现，不影响弹射功能。
        if (plate == null)
        {
            yield break;
        }

        float duration = 0.18f;

        // elapsed 从 0 递增到 duration；sin(0..PI) 生成 0→1→0 的隆起曲线。
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            plate.localPosition = plateStart + Vector3.up * Mathf.Sin(elapsed / duration * Mathf.PI) * 0.2f;
            yield return null;
        }

        // 动画结束，精确复位，避免浮点误差累积导致位置漂移。
        plate.localPosition = plateStart;
    }
}