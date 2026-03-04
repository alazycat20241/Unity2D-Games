using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour
{
    public enum TrackColor { Red, Blue }
    public TrackColor trackColor;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = (trackColor == TrackColor.Red) ? Color.red : Color.blue;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!GameManager.Instance.isGameActive) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // ºÏ≤È—’…´ «∑Ò∆•≈‰
                bool isMatch = (player.currentColor == PlayerController.PlayerColor.Red && trackColor == TrackColor.Red) ||
                               (player.currentColor == PlayerController.PlayerColor.Blue && trackColor == TrackColor.Blue);

                if (!isMatch)
                {
                    player.Die();  // —’…´≤ª∆•≈‰£¨À¿Õˆ
                }
                else
                {

                }
            }
        }
    }
}
