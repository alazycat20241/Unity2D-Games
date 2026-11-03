using UnityEngine;

public sealed class DoorController : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private Vector3 closedLocalPosition;

    [SerializeField, HideInInspector]
    private float openDistance = 2.8f;

    [SerializeField, HideInInspector]
    private bool initialized;

    private Collider2D doorCollider;
    private SpriteRenderer[] renderers;
    private bool shouldOpen;

    public void Initialize(float distance)
    {
        closedLocalPosition = transform.localPosition;
        openDistance = distance;
        initialized = true;
        doorCollider = GetComponent<Collider2D>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (!initialized)
        {
            return;
        }

        transform.localPosition = closedLocalPosition;
        shouldOpen = false;
        doorCollider = doorCollider != null ? doorCollider : GetComponent<Collider2D>();
        renderers = renderers ?? GetComponentsInChildren<SpriteRenderer>();

        if (doorCollider != null)
        {
            doorCollider.enabled = true;
        }

        if (renderers == null)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            Color color = renderers[i].color;
            color.a = 1f;
            renderers[i].color = color;
        }
    }

    public void SetOpen(bool open)
    {
        shouldOpen = open;
        if (doorCollider != null)
        {
            doorCollider.enabled = !open;
        }
    }

    private void Update()
    {
        Vector3 target = closedLocalPosition + new Vector3(0f, shouldOpen ? openDistance : 0f, 0f);
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            target,
            1f - Mathf.Exp(-12f * Time.deltaTime));

        if (renderers == null)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            Color color = renderers[i].color;
            color.a = Mathf.Lerp(
                color.a,
                shouldOpen ? 0.25f : 1f,
                1f - Mathf.Exp(-10f * Time.deltaTime));
            renderers[i].color = color;
        }
    }
}
