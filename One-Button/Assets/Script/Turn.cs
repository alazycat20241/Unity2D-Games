using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn : MonoBehaviour
{
    public Vector2 turnDirection;  // 在Inspector里设置方向

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetDirection(turnDirection);
                Debug.Log($"转向：{turnDirection}");
            }
        }
    }
}
