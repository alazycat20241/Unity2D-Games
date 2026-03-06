using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn : MonoBehaviour
{
    public Vector2 turnDirection;  // (1,0)右, (0,1)上, (-1,0)左, (0,-1)下
    private bool hasTriggered = false;  // 防止重复触发

    private void OnTriggerStay2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // 检查玩家是否到达格子中点
                // 当前格子的中点 = 当前轨道的位置
                float distanceToCenter = Vector2.Distance(player.transform.position, transform.position);

                // 到达中点附近才转向
                if (distanceToCenter < 0.1f)
                {
                    // 获取轨道的旋转角度
                    float angle = transform.parent.eulerAngles.z * Mathf.Deg2Rad;

                    // 旋转方向向量
                    float cos = Mathf.Cos(angle);
                    float sin = Mathf.Sin(angle);
                    Vector2 rotatedDirection = new Vector2(
                        turnDirection.x * cos - turnDirection.y * sin,
                        turnDirection.x * sin + turnDirection.y * cos
                    );

                    player.SetDirection(rotatedDirection.normalized);
                    hasTriggered = true;  // 只触发一次

                    Debug.Log($"中点转向：{turnDirection} → {rotatedDirection}");
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            hasTriggered = false;  // 离开后重置，下次进来重新判定
        }
    }
}
