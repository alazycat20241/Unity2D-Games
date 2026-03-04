using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 3f;
    private Vector2 moveDirection = Vector2.right;  // 当前移动方向

    [Header("颜色设置")]
    public SpriteRenderer playerSprite;
    public Color redColor = Color.red;
    public Color blueColor = Color.blue;

    public enum PlayerColor { Red, Blue }
    public PlayerColor currentColor = PlayerColor.Red;

    void Start()
    {
        UpdateColor();
    }

    void Update()
    {
        if (!GameManager.Instance.isGameActive) return;

        // 空格切换颜色
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentColor = (currentColor == PlayerColor.Red) ?
                PlayerColor.Blue : PlayerColor.Red;
            UpdateColor();
        }

        // 向当前方向自动移动
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    void UpdateColor()
    {
        playerSprite.color = (currentColor == PlayerColor.Red) ? redColor : blueColor;
    }

    // 转向方法（被转向触发器调用）
    public void SetDirection(Vector2 newDirection)
    {
        moveDirection = newDirection;
    }

    // 死亡方法（被轨道调用）
    public void Die()
    {
        GameManager.Instance.GameOver();
    }
}
