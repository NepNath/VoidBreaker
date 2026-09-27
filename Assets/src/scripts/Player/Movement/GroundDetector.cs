using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    
    [Range(0f, 1f)]
    [SerializeField] float detectionRadius;
    [SerializeField] private Vector3 detectionOffset;
    public LayerMask groundLayerMask;

    /// <summary>
    /// Determines whether the player is currently grounded by checking for collision with the "Ground" layer mask.
    /// </summary>
    /// <returns>
    /// A boolean value indicating whether the player is grounded.
    /// </returns>
    public bool IsGrounded()
    {
        return Physics.CheckSphere(transform.position + detectionOffset, detectionRadius, groundLayerMask);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position + detectionOffset, detectionRadius);
    }
    
}
