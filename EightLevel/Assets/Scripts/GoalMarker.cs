using UnityEngine;

public sealed class GoalMarker : MonoBehaviour
{
    private Vector3 baseScale;
    private bool reached;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        reached = false;
        if (baseScale.sqrMagnitude > 0f)
        {
            transform.localScale = baseScale;
        }
    }

    private void Update()
    {
        float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.025f;
        transform.localScale = baseScale * pulse;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (reached || other.GetComponent<PlayerController>() == null || GameBootstrap.Instance == null)
        {
            return;
        }

        reached = true;
        GameBootstrap.Instance.ReachGoal(transform.position);
    }
}
