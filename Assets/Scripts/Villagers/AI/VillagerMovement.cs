using UnityEngine;

public class VillagerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 15f; // Speed of movement
    private Vector3 targetPosition; // Current target position

    public void SetTargetPosition(Vector3 target) => targetPosition = target;

    public void MoveToTarget()
    {
        if (targetPosition == null) return;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    public bool HasReachedTarget() => Vector3.Distance(transform.position, targetPosition) < 0.1f;

}