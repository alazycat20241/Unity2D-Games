using UnityEngine;

/// <summary>
/// 出生点标记：一个只在编辑器 Scene 视图中绘制可视化图标的空物体。
/// 运行时没有任何逻辑，不影响游戏；仅供关卡搭建时标记玩家出生位置。
/// 挂载方式：在关卡中新建空物体，挂上本脚本，摆到期望的出生点即可。
/// </summary>
public sealed class SpawnPointMarker : MonoBehaviour
{
    /// <summary>
    /// 只在编辑器下被调用（Scene 视图重绘时触发），用于绘制 Gizmos 辅助图标。
    /// 运行时不会被调用，也不会出现在游戏画面里。
    /// </summary>
    private void OnDrawGizmos()
    {
        // 青色、略透明，便于与场景内其他 Gizmos 区分。
        Gizmos.color = new Color(0.3f, 0.85f, 1f, 0.9f);

        // 画一个半径 0.32 的线框圆，标记出生点范围。
        Gizmos.DrawWireSphere(transform.position, 0.32f);

        // 画水平线（左右各延伸 0.55），形成十字准星。
        Gizmos.DrawLine(
            transform.position + Vector3.left * 0.55f,
            transform.position + Vector3.right * 0.55f);

        // 画垂直线（上下各延伸 0.55），与水平线组成十字。
        Gizmos.DrawLine(
            transform.position + Vector3.down * 0.55f,
            transform.position + Vector3.up * 0.55f);
    }
}