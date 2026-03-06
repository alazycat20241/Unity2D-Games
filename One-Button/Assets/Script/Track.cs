using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour
{
    public enum TrackColor { Red, Orange, Yellow, Blue }
    public TrackColor trackColor;

    private SpriteRenderer spriteRenderer;
    private static List<Track> wrongTracks = new List<Track>();  // 记录连续的错误轨道



    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateColor();
    }

    void UpdateColor()
    {
        switch (trackColor)
        {
            case TrackColor.Red: spriteRenderer.color = Color.red; break;
            case TrackColor.Orange: spriteRenderer.color = new Color(1f, 0.5f, 0f); break;
            case TrackColor.Yellow: spriteRenderer.color = Color.yellow; break;
            case TrackColor.Blue: spriteRenderer.color = Color.blue; break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!GameManager.Instance.isGameActive) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // 检查颜色是否匹配
                bool isMatch = IsColorMatch(player.currentColor);

                if (!isMatch)
                {
                    // 颜色不匹配，加入错误列表
                    wrongTracks.Add(this);

                    // 如果连续错误达到2个，才死亡
                    if (wrongTracks.Count >= 2)
                    {
                        player.Die();
                        wrongTracks.Clear();
                    }
                }
                else
                {
                    // 颜色匹配，清空错误列表
                    wrongTracks.Clear();
                }
            }
        }
    }

    bool IsColorMatch(PlayerController.PlayerColor playerColor)
    {
        return (playerColor == PlayerController.PlayerColor.Red && trackColor == TrackColor.Red) ||
               (playerColor == PlayerController.PlayerColor.Orange && trackColor == TrackColor.Orange) ||
               (playerColor == PlayerController.PlayerColor.Yellow && trackColor == TrackColor.Yellow) ||
               (playerColor == PlayerController.PlayerColor.Blue && trackColor == TrackColor.Blue);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 离开轨道时，如果这个轨道在错误列表里，移除它
            if (wrongTracks.Contains(this))
            {
                wrongTracks.Remove(this);
            }
        }
    }
}
