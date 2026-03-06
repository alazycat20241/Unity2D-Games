using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 3f;
    private Vector2 moveDirection = Vector2.right;

    [Header("颜色设置")]
    public SpriteRenderer playerSprite;
    public Color redColor = Color.red;
    public Color orangeColor = new Color(1f, 0.5f, 0f);
    public Color yellowColor = Color.yellow;
    public Color blueColor = Color.blue;

    [Header("变色动画")]
    public float colorTransitionSpeed = 5f;  // 颜色变化速度
    private Color targetColor;                 // 目标颜色
    private Color currentVisualColor;          // 当前显示的颜色

    public enum PlayerColor { Red, Orange, Yellow, Blue }
    public PlayerColor currentColor = PlayerColor.Red;

    void Start()
    {
        currentVisualColor = redColor;
        targetColor = redColor;
        playerSprite.color = currentVisualColor;
    }

    void Update()
    {
        if (!GameManager.Instance.isGameActive) return;

        // 按一次变一次色
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentColor = (PlayerColor)(((int)currentColor + 1) % 4);

            // 设置新的目标颜色
            switch (currentColor)
            {
                case PlayerColor.Red: targetColor = redColor; break;
                case PlayerColor.Orange: targetColor = orangeColor; break;
                case PlayerColor.Yellow: targetColor = yellowColor; break;
                case PlayerColor.Blue: targetColor = blueColor; break;
            }

            MusicManager.Instance.PlayClickSound();
        }

        // 平滑过渡到目标颜色
        currentVisualColor = Color.Lerp(currentVisualColor, targetColor, colorTransitionSpeed * Time.deltaTime);
        playerSprite.color = currentVisualColor;

        // 向当前方向自动移动
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    public void SetDirection(Vector2 newDirection)
    {
        moveDirection = newDirection;
    }

    public void Die()
    {
        GameManager.Instance.GameOver();
    }
}
