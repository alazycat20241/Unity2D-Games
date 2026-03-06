using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 3f;
    private Vector2 moveDirection = Vector2.right;

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

        // 自动前进
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
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

    public void SetDirection(Vector2 newDirection)
    {
        moveDirection = newDirection;
    }

    public Vector2 GetDirection()
    {
        return moveDirection;
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
