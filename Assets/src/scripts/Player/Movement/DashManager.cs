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
            playerMovements.rb.AddForce(playerMovements.CurrentMovementDirection() * (playerMovements.dashForce), ForceMode.Impulse);
        }
    }
    
    IEnumerator DashStateManager(float cooldown)
    {
        dashAvailable = false;
        isDashing = true;
        playerMovements.CurrentState = BaseMovements.MovementStates.Dash;
        yield return new WaitForSecondsRealtime(dashDuration);
        isDashing = false;
        yield return new WaitForSecondsRealtime(dashDuration + (dashDuration * .1f));
        playerMovements.CurrentState = BaseMovements.MovementStates.Walk;
        yield return new WaitForSecondsRealtime(cooldown);
        dashAvailable = true;
    }
}
