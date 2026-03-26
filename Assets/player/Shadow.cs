using UnityEngine;

public class PlayerShadowController : MonoBehaviour
{
    public Transform shadow;
    public Rigidbody2D rb;

    public Vector3 baseOffset = new Vector3(0f, -0.45f, 0f);
    public float moveOffsetAmount = 0.05f;
    public float smoothSpeed = 10f;

    private Vector3 currentLocalPos;

    void Start()
    {
        currentLocalPos = baseOffset;
        shadow.localPosition = baseOffset;
    }

    void LateUpdate()
    {
        Vector2 velocity = rb.linearVelocity;
        Vector3 targetOffset = baseOffset;

        if (velocity.sqrMagnitude > 0.01f)
        {
            Vector2 dir = velocity.normalized;
            targetOffset += new Vector3(-dir.x, -dir.y, 0f) * moveOffsetAmount;
        }

        currentLocalPos = Vector3.Lerp(currentLocalPos, targetOffset, Time.deltaTime * smoothSpeed);
        shadow.localPosition = currentLocalPos;
    }
}