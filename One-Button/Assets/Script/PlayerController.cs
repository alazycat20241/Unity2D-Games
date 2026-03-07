using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 3f;
    public Transform[] pathPoints;  // 路线上的点
    private int currentPathIndex = 0;

    [Header("玩家图片")]
    public SpriteRenderer playerSprite;
    public Sprite redSprite;      // 红色图片
    public Sprite orangeSprite;   // 橙色图片
    public Sprite yellowSprite;   // 黄色图片
    public Sprite blueSprite;     // 蓝色图片

    public enum PlayerColor { Red, Orange, Yellow, Blue }
    public PlayerColor currentColor = PlayerColor.Red;

    private Track currentTrack;  // 当前所在的格子

    void Start()
    {
        UpdateSprite();
    }

    void Update()
    {
        if (!GameManager.Instance.isGameActive) return;

        // 颜色切换
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentColor = (PlayerColor)(((int)currentColor + 1) % 4);
            UpdateSprite();
            MusicManager.Instance.PlayClickSound();
        }

        // 向当前目标点移动
        if (currentPathIndex < pathPoints.Length)
        {
            Vector3 targetPos = pathPoints[currentPathIndex].position;
            Vector3 moveDir = (targetPos - transform.position).normalized; // 每帧重新计算方向
            transform.Translate(moveDir * moveSpeed * Time.deltaTime);

            // 到达目标点
            if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            {
                //transform.position = targetPos;
                currentPathIndex++;
                Debug.Log(currentPathIndex);
            }
        }
    }

    void UpdateSprite()
    {
        switch (currentColor)
        {
            case PlayerColor.Red: playerSprite.sprite = redSprite; break;
            case PlayerColor.Orange: playerSprite.sprite = orangeSprite; break;
            case PlayerColor.Yellow: playerSprite.sprite = yellowSprite; break;
            case PlayerColor.Blue: playerSprite.sprite = blueSprite; break;
        }
    }

    public void Die()
    {
        GameManager.Instance.GameOver();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Track"))
        {
            currentTrack = other.GetComponent<Track>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Track") && currentTrack == other.GetComponent<Track>())
        {
            currentTrack = null;
        }
    }
}
