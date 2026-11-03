using UnityEngine;

public sealed class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null && GameBootstrap.Instance != null)
        {
            GameBootstrap.Instance.HandlePlayerDeath();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null && GameBootstrap.Instance != null)
        {
            GameBootstrap.Instance.HandlePlayerDeath();
        }
    }
}
