using UnityEngine;

public sealed class EffectParticle : MonoBehaviour
{
    private Vector2 velocity;
    private SpriteRenderer spriteRenderer;
    private float life;
    private float age;

    public void Initialize(Vector2 initialVelocity, Color color)
    {
        velocity = initialVelocity;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
        life = Random.Range(0.16f, 0.28f);
    }

    private void Update()
    {
        age += Time.deltaTime;
        velocity *= 0.93f;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.Rotate(0f, 0f, 260f * Time.deltaTime);
        float alpha = 1f - Mathf.Clamp01(age / life);
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;

        if (age >= life)
        {
            Destroy(gameObject);
        }
    }
}
