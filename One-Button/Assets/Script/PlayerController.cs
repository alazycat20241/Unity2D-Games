using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 3f;
    public Transform pathParent;  // 拖入路径母物体，自动读取其子物体作为路径点
    private Transform[] pathPoints;  // 内部使用的路径点数组
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
        LoadPathPoints(); // 初始化时加载路径点
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
                currentPathIndex++;

                // 检查是否走完了所有路径点
                if (currentPathIndex >= pathPoints.Length)
                {
                    GameManager.Instance.GameWin();  // 通知游戏胜利
                }
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

    // 加载路径点：从 pathParent 读取所有子物体，按Hierarchy顺序（索引顺序）
    private void LoadPathPoints()
    {
        if (pathParent == null)
        {
            return;
        }

        int childCount = pathParent.childCount;
        pathPoints = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            pathPoints[i] = pathParent.GetChild(i); // 按索引顺序，即Hierarchy中的顺序
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
