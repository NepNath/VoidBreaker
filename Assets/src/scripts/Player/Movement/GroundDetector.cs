using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [Range(0f, 2f)]
    [SerializeField] private float detectionDistance;
    [SerializeField] private Vector3 detectionOffset;
    public LayerMask groundLayerMask;

    /// <summary>
    /// Determines whether the player is currently grounded by casting a ray downward on the "Ground" layer mask.
    /// </summary>
    /// <returns>
    /// A boolean value indicating whether the player is grounded.
    /// </returns>
    public bool IsGrounded()
    {
        Vector3 rayOrigin = transform.position + detectionOffset;
        return Physics.Raycast(rayOrigin, Vector3.down, detectionDistance, groundLayerMask);
    }

    private void OnDrawGizmos()
    {
        Vector3 rayOrigin = transform.position + detectionOffset;
        Vector3 rayEnd = rayOrigin + Vector3.down * detectionDistance;

        if (IsGrounded())
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawLine(rayOrigin, rayEnd);
    }
}