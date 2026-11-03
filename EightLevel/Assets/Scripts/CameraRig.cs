using UnityEngine;

public sealed class CameraRig : MonoBehaviour
{
    private Camera targetCamera;
    private Transform target;
    private float minCameraX;
    private float maxCameraX;
    private float shakeTime;
    private float shakeDuration;
    private float shakeMagnitude;
    private Vector3 smoothVelocity;

    public void Initialize(Camera cameraToDrive, Transform targetTransform)
    {
        targetCamera = cameraToDrive;
        target = targetTransform;
    }

    public void SetLevelBounds(float minX, float maxX, bool snap)
    {
        float halfWidth = targetCamera.orthographicSize * targetCamera.aspect;
        minCameraX = minX + Mathf.Min(3.5f, halfWidth);
        maxCameraX = Mathf.Max(minCameraX, maxX - Mathf.Min(4.5f, halfWidth));

        if (snap && target != null)
        {
            transform.position = new Vector3(
                GetTargetX(),
                0f,
                -10f);
            smoothVelocity = Vector3.zero;
        }
    }

    public void Shake(float duration, float magnitude)
    {
        shakeDuration = Mathf.Max(shakeDuration, duration);
        shakeTime = Mathf.Max(shakeTime, duration);
        shakeMagnitude = Mathf.Max(shakeMagnitude, magnitude);
    }

    private void LateUpdate()
    {
        if (target == null || targetCamera == null)
        {
            return;
        }

        Vector3 targetPosition = new Vector3(GetTargetX(), 0f, -10f);
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref smoothVelocity,
            0.14f,
            Mathf.Infinity,
            Time.unscaledDeltaTime);

        if (shakeTime > 0f)
        {
            shakeTime -= Time.unscaledDeltaTime;
            float fade = shakeDuration > 0f ? Mathf.Clamp01(shakeTime / shakeDuration) : 0f;
            Vector2 offset = Random.insideUnitCircle * shakeMagnitude * fade;
            transform.position += new Vector3(offset.x, offset.y, 0f);
            if (shakeTime <= 0f)
            {
                shakeMagnitude = 0f;
            }
        }
    }

    private float GetTargetX()
    {
        float followAhead = 2.75f;
        return Mathf.Clamp(target.position.x + followAhead, minCameraX, maxCameraX);
    }
}
