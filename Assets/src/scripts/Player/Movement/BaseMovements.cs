using System;
using UnityEngine;

public class BaseMovements : MonoBehaviour
{
    //----------- Variables --------------//
    [Header("Movement variables")]
    public float walkSpeed;
    public float dashForce;
    public float airBorneSpeed;
    public float jumpForce;
    
    [Header("Damping variables")]
    public float groundDamping;
    public float airBorneDamping;
    public float dashDamping;
    
    [HideInInspector] public float PlayerDamping;
    
    [Header("References")]
    public Rigidbody rb;
    public MovementStates CurrentState;
    private GroundDetector GroundDetector;
    private DashManager DashManager;
    
    //----------- Unity built in --------------//
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        GroundDetector = GetComponent<GroundDetector>();
        DashManager = GetComponent<DashManager>();
    }
    private void FixedUpdate() 
    {
        Movement();
    }
    private void Update() 
    {
        Jump();
        DashManager.DashAction();
        UpdateMovementState();
        CurrentPlayerDamping();
        
    }
    
    //----------- Movement logic --------------//
    /// <summary>
    /// Function to execute base movement on a component.
    /// doesn't do it on IA controlled entity, only players
    /// don't manage complexe movements like slides, wall jumps, and other advanced mechanics.
    /// </summary>
    private void Movement()
    {
        if (CurrentState == MovementStates.Dash)
        {
            return;
        }
        rb.AddForce(new Vector3(
            CurrentMovementDirection().x * CurrentMovementSpeed(),
            0,
            CurrentMovementDirection().z * CurrentMovementSpeed()
            ), ForceMode.VelocityChange);
    }
    /// <summary>
    /// Executes the logic for the jump action of the player character.
    /// </summary>
    private void Jump()
    {
        if (GroundDetector.IsGrounded() && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }
    }
    //----------- Movement state management --------------//
    /// <summary>
    /// Movement states for the player character.
    /// To use to manage different movement variables like speed, damping etc...
    /// </summary>
    public enum MovementStates
    {
        Walk,
        Airborne,
        Dash,
    }
    /// <summary>Updates the movement state of the player character depending on its current situation</summary>
    /// <returns>The updated movement state of the player character.</returns>
    private MovementStates UpdateMovementState()
    {
        if (DashManager.isDashing)
        {
            CurrentState = MovementStates.Dash;
            PlayerDamping = dashDamping;
        }
        else if(!GroundDetector.IsGrounded())
        {
            CurrentState = MovementStates.Airborne;
            PlayerDamping = airBorneDamping;
        }
        else
        {
            CurrentState = MovementStates.Walk;
            PlayerDamping = groundDamping;
        }
        
        return CurrentState;
    }
    /// <summary>assign the speed value of the player depending on its current movement state</summary>
    /// <returns> current speed float value of the player</returns>
    public float CurrentMovementSpeed()
    {
        switch(CurrentState)
        {
            case MovementStates.Walk:
                return walkSpeed;
            case MovementStates.Airborne:
                return airBorneSpeed;
            default:
                return 0f;
        }
    }
    
    /// <summary>apply the damping value of the player depending on its current movement state</summary>
    public void CurrentPlayerDamping()
    {
        rb.linearDamping = PlayerDamping;
    }


    /// <summary>Gets the current movement direction of the player </summary>
    /// <returns> A vector3 representing the current movement direction of the player</returns>
    public Vector3 CurrentMovementDirection()
    {
        float hrz = Input.GetAxisRaw("Horizontal");
        float vrt = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = transform.TransformDirection(new Vector3(hrz, 0f, vrt));

        return inputDirection.normalized;
    }
}