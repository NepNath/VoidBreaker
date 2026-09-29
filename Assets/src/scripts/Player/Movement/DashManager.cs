using UnityEngine;
using System.Collections;
public class DashManager : MonoBehaviour
{
    public bool dashAvailable;
    public bool isDashing;
    [SerializeField] private float dashCooldown;
    
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
            playerMovements.rb.AddForce(playerMovements.CurrentMovementDirection() * (playerMovements.DashForce * 10f), ForceMode.Impulse);
        }
    }
    
    IEnumerator DashStateManager(float dashCooldown)
    {
        dashAvailable = false;
        isDashing = true;
        yield return new WaitForSecondsRealtime(dashCooldown);
        dashAvailable = true;
        isDashing = false;
    }
}
