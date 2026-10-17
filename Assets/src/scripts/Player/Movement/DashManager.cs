using UnityEngine;
using System.Collections;
public class DashManager : MonoBehaviour
{
    public bool dashAvailable;
    public bool isDashing;
    [SerializeField] private float dashCooldown;
    [SerializeField] private float dashDuration;
    
    private BaseMovements playerMovements;
    
    
    private void Start()
    {
        playerMovements = GetComponent<BaseMovements>();
    }
    
    public void DashAction()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashAvailable)
        {
            StartCoroutine(DashStateManager(dashCooldown));
            Vector3 dashDirection = playerMovements.CurrentMovementDirection() == Vector3.zero ? transform.forward : playerMovements.CurrentMovementDirection();
            playerMovements.rb.AddForce(dashDirection * playerMovements.dashForce, ForceMode.Impulse);
        }
    }
    
    IEnumerator DashStateManager(float cooldown)
    {
        dashAvailable = false;
        isDashing = true;
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        yield return new WaitForSeconds(cooldown);
        dashAvailable = true;
    }
}
