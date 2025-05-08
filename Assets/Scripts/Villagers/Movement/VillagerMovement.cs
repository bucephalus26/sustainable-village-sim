using UnityEngine;

public class VillagerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 15f; // Speed of movement
    [SerializeField] private float stoppingDistance = 0.1f;

    private Vector3 targetPosition;
    private bool hasTarget = false;

    // Visual feedback
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void SetTargetPosition(Vector3 target)
    {
        targetPosition = target;
        hasTarget = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    public void MoveToTarget()
    {
        if (!hasTarget) return;

        if (HasReachedTarget())
        {
            // Visual feedback when destination reached
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
            return;
        }

        // Move towards target
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime);

        //  Face the movement direction
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction.x != 0)
        {
            // Flip sprite based on movement direction
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }
    }

    public bool HasReachedTarget()
    {
        return !hasTarget || Vector3.Distance(transform.position, targetPosition) < stoppingDistance;
    }

    public void ClearTarget()
    {
        hasTarget = false;
    }

}