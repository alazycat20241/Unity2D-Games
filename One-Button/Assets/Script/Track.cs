using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour
{
    public enum TrackColor { Red, Orange, Yellow, Blue }
    public TrackColor trackColor;

    [Header("判定设置")]
    public float centerRadius = 0.4f;      // 中心区域半径

    private bool hasReachedCenter = false;  // 是否到达中心

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!GameManager.Instance.isGameActive) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null) return;

            float distToCenter = Vector2.Distance(other.transform.position, transform.position);

            // 到达中心时进行判定
            if (!hasReachedCenter && distToCenter < centerRadius)
            {
                hasReachedCenter = true;
                Debug.Log("到达中心");
                // 颜色判定
                if ((int)player.currentColor != (int)trackColor)
                {
                    player.Die();
                    return;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 如果游戏还在进行中，但玩家离开了轨道（没有到达中心或没有正确转向）
        if (GameManager.Instance.isGameActive)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // 如果还没到达中心就离开轨道，或者已经到达中心但没触发转向（针对转弯格子）
                if (!hasReachedCenter)
                {
                    player.Die();
                    return;
                }
            }
        }

        // 离开格子，重置状态
        hasReachedCenter = false;
    }

}
